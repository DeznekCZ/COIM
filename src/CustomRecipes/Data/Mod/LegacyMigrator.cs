using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mafi;
using PythonAPI;
using PythonAPI.Arguments;
using PythonAPI.Expressions;
using PythonAPI.Statements;

namespace CustomAssets.Data.Mod {

    /// <summary>
    /// One-shot migration from the legacy load-order convention to the
    /// `Definitions/__init__.py` convention.
    ///
    /// Legacy shape: each definition file declares its own load-order deps via
    /// a top-level <c>dependencies("a.py", "b.py")</c> call. The runtime walked
    /// every .py in the pack and resolved the resulting dependency graph.
    ///
    /// New shape: a single <c>Definitions/__init__.py</c> holds one
    /// <c>dependencies("a.py", "b.py", "c.py")</c> call naming every load
    /// order entry in order; individual files no longer need it.
    ///
    /// The migrator:
    ///   1. Collects every top-level <c>dependencies(...)</c> call across the
    ///      pack's files, recording the args (positional string literals) and
    ///      the source line range that holds the call.
    ///   2. Deletes those calls in place by splicing the original line range
    ///      out of each file's source.
    ///   3. Writes a fresh <c>Definitions/__init__.py</c> containing one
    ///      consolidated <c>dependencies(...)</c> with the union of names in
    ///      first-seen order (preserves authorial intent for the load order).
    ///
    /// The migrator is conservative: if <c>__init__.py</c> already exists OR
    /// the pack has no legacy <c>dependencies(...)</c> calls, it does nothing
    /// and returns a `NoChange` report. Migration is idempotent — running it
    /// twice on the same pack is a no-op the second time.
    /// </summary>
    public static class LegacyMigrator {

        public sealed class Report {
            public bool Migrated;
            public string DepsFilePath;
            public List<string> CollectedNames = new List<string>();
            public List<string> AffectedFiles  = new List<string>();
            public string Error;

            public static Report NoChange(string reason) {
                return new Report { Migrated = false, Error = reason };
            }
        }

        /// <summary>Apply the migration to `pack`. Caller is responsible for
        /// re-running PackRegistry.RescanPack afterwards so the in-memory
        /// state reflects the rewritten files.</summary>
        public static Report Migrate(LoadedPack pack) {
            if (pack == null) return Report.NoChange("pack was null");
            if (string.IsNullOrEmpty(pack.RootPath)) return Report.NoChange("pack RootPath was empty");

            // Don't clobber an existing __init__.py — that's the steady-state
            // form already, so the modder has presumably already migrated or
            // hand-authored it. We'd rather skip than overwrite their work.
            string definitionsDir = Path.Combine(pack.RootPath, "Definitions");
            string initPath = Path.Combine(definitionsDir, "__init__.py");
            if (File.Exists(initPath)) {
                return Report.NoChange("Definitions/__init__.py already exists");
            }
            if (!Directory.Exists(definitionsDir)) {
                return Report.NoChange("Definitions/ folder not found");
            }

            // Walk every file's AST collecting top-level dependencies(...) calls
            // with their args and line ranges. We keep the call-site list as a
            // tuple-like (path, startLine, endLine) so the splice step can run
            // per-file with the correct ranges, without re-walking the AST.
            var collected = new List<string>();
            var seenNames = new HashSet<string>(System.StringComparer.Ordinal);
            var perFileCallSites = new Dictionary<string, List<(int start, int end)>>(System.StringComparer.Ordinal);

            foreach (LoadedFile file in pack.Files) {
                if (file.Ast == null) continue;
                List<(int, int)> sites = null;
                foreach (IStatement stmt in file.Ast.statements) {
                    if (!(stmt is EvaluateStatement ev)) continue;
                    if (!(ev.Expression is CallExpression call)) continue;
                    if (!(call.Calle is VariableExpression v) || v.Path != "dependencies") continue;

                    // Pull every positional string-literal argument out as a
                    // name. We ignore non-literal forms here (typed refs etc.)
                    // because the legacy API only accepts string literals — a
                    // mod using a non-literal here was already broken pre-migration.
                    foreach (IArgument arg in call.Arguments) {
                        if (arg is OrderedArgument && arg.Expression is StringConstant s) {
                            if (!seenNames.Contains(s.Value)) {
                                collected.Add(s.Value);
                                seenNames.Add(s.Value);
                            }
                        }
                    }
                    if (sites == null) sites = new List<(int, int)>();
                    sites.Add((ev.StartLine, ev.EndLine));
                }
                if (sites != null) perFileCallSites[file.AbsolutePath] = sites;
            }

            if (collected.Count == 0) {
                return Report.NoChange("no top-level dependencies(...) calls found");
            }

            Report report = new Report {
                Migrated = true,
                DepsFilePath = initPath,
                CollectedNames = collected
            };

            // Step 2: delete every dependencies(...) call site from its source.
            // We splice line ranges in descending order so earlier ranges aren't
            // invalidated by removals further down the file.
            foreach (var kvp in perFileCallSites) {
                try {
                    string[] lines = File.ReadAllLines(kvp.Key, Encoding.UTF8);
                    List<string> output = new List<string>(lines);
                    var sortedDesc = kvp.Value.OrderByDescending(s => s.start).ToList();
                    foreach (var site in sortedDesc) {
                        int startIdx = site.start - 1;
                        int endIdx   = site.end   - 1;
                        if (startIdx < 0 || endIdx < startIdx || endIdx >= output.Count) continue;
                        output.RemoveRange(startIdx, endIdx - startIdx + 1);
                    }
                    File.WriteAllLines(kvp.Key, output, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                    report.AffectedFiles.Add(kvp.Key);
                } catch (System.Exception ex) {
                    // Don't bail completely — record the first failure and keep
                    // going so the user sees what survived. Aborting halfway
                    // through would leave the pack in a worse state than
                    // either fully-legacy or fully-migrated.
                    Log.Warning("LegacyMigrator: failed to splice '"
                                + kvp.Key + "' — " + ex.Message);
                    if (report.Error == null) report.Error = ex.Message;
                }
            }

            // Step 3: write the consolidated __init__.py. One dependencies(...)
            // call with comma-separated string-literal args in collection order.
            // No trailing comma per the CustomAssets dialect's no-trailing-comma
            // rule (see PythonAPI dialect quirks memory).
            try {
                StringBuilder sb = new StringBuilder();
                sb.Append("# Auto-generated by the CustomAssets editor's LegacyMigrator.\n");
                sb.Append("# Consolidates the per-file dependencies(...) calls that used to\n");
                sb.Append("# live in individual Definitions/*.py files into a single load\n");
                sb.Append("# order declaration here.\n");
                sb.Append("dependencies(");
                for (int i = 0; i < collected.Count; i++) {
                    if (i > 0) sb.Append(", ");
                    sb.Append('"').Append(escapeStringLiteral(collected[i])).Append('"');
                }
                sb.Append(")\n");
                File.WriteAllText(initPath, sb.ToString(),
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            } catch (System.Exception ex) {
                report.Error = "init write failed: " + ex.Message;
            }

            return report;
        }

        // Escape just enough to round-trip filenames that could contain `\` or
        // `"`. Filenames in the legacy dependencies(...) calls almost never
        // need it (they're plain .py basenames), but escaping is essentially
        // free and avoids producing invalid Python on the off chance.
        private static string escapeStringLiteral(string value) {
            if (value == null) return "";
            StringBuilder sb = new StringBuilder(value.Length);
            foreach (char c in value) {
                switch (c) {
                    case '\\': sb.Append("\\\\"); break;
                    case '"':  sb.Append("\\\""); break;
                    case '\n': sb.Append("\\n");  break;
                    default:   sb.Append(c);       break;
                }
            }
            return sb.ToString();
        }
    }
}
