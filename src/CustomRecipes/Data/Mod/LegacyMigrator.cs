using System;
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
    /// New shape: a single <c>Definitions/__init__.py</c> containing plain
    /// <c>import &lt;module&gt;</c> statements — one per sibling .py file. Python
    /// runs each module's top-level code on import, so the order of `import`
    /// lines directly encodes the load order. This is the form the
    /// CustomAssets framework loads first when present, and it matches the
    /// shape modders see in every other Python package.
    ///
    /// The migrator:
    ///   1. Collects every top-level <c>dependencies(...)</c> call across the
    ///      pack's files, recording the args (positional string literals) and
    ///      the source line range that holds the call.
    ///   2. Deletes those calls in place by splicing the original line range
    ///      out of each file's source.
    ///   3. Enumerates every <c>Definitions/*.py</c> file so files that the
    ///      legacy <c>dependencies(...)</c> calls never mentioned still show
    ///      up — those would have been loaded by the framework's directory
    ///      walk before, and dropping them on migration would silently break
    ///      the pack.
    ///   4. Writes a fresh <c>Definitions/__init__.py</c> with one
    ///      <c>import &lt;name&gt;</c> per line. Ordering: names that appeared
    ///      in legacy <c>dependencies(...)</c> calls come first in their
    ///      original order (preserves authorial intent for the load order);
    ///      remaining files are appended in alphabetical order so the result
    ///      is deterministic across runs.
    ///
    /// The migrator is conservative: if <c>__init__.py</c> already exists, it
    /// does nothing and returns a `NoChange` report. Migration is idempotent
    /// — running it twice on the same pack is a no-op the second time.
    /// </summary>
    public static class LegacyMigrator {

        public sealed class Report {
            public bool Migrated;
            public string DepsFilePath;
            public List<string> CollectedNames = new List<string>();
            public List<string> AffectedFiles  = new List<string>();
            public string CopiedPackStub;
            public string DeletedLegacyDll;
            public bool ManifestUpdated;
            public string Error;

            public static Report NoChange(string reason) {
                return new Report { Migrated = false, Error = reason };
            }
        }

        /// <summary>Apply the migration to `pack`. Caller is responsible for
        /// re-running PackRegistry.RescanPack afterwards so the in-memory
        /// state reflects the rewritten files. Two independent migration
        /// steps run:
        ///   • load-order: converts per-file <c>dependencies(...)</c> calls
        ///     into a single <c>Definitions/__init__.py</c> of imports. Skipped
        ///     when an __init__.py already exists.
        ///   • pack-stub DLL: deletes the legacy
        ///     <c>CustomAssets_&lt;modId&gt;.dll</c> and drops the shared
        ///     <c>CustomAssetPack.dll</c> stub in its place. Runs whenever the
        ///     legacy DLL is present, regardless of load-order shape.
        /// Either step running on its own counts as a successful migration.
        /// </summary>
        public static Report Migrate(LoadedPack pack) {
            if (pack == null) return Report.NoChange("pack was null");
            if (string.IsNullOrEmpty(pack.RootPath)) return Report.NoChange("pack RootPath was empty");

            Report report = new Report { Migrated = false };

            string definitionsDir = Path.Combine(pack.RootPath, "Definitions");
            string initPath = Path.Combine(definitionsDir, "__init__.py");
            bool runLoadOrderMigration = Directory.Exists(definitionsDir)
                                         && !File.Exists(initPath);
            // DLL step also runs when manifest still references the legacy
            // DLL name — even if the .dll itself was already deleted, an
            // unfixed manifest leaves the pack unloadable.
            bool runDllMigration = PackRegistry.hasLegacyPerPackDll(pack)
                                   || PackRegistry.hasLegacyManifest(pack);

            if (!runLoadOrderMigration && !runDllMigration) {
                return Report.NoChange(File.Exists(initPath)
                    ? "pack already in canonical shape (init + manifest + stub)"
                    : "Definitions/ folder not found and manifest already canonical");
            }

            if (runDllMigration) {
                migrateLegacyDll(pack, report);
            }

            if (!runLoadOrderMigration) {
                return report;
            }

            // Walk every file's AST collecting top-level dependencies(...) calls
            // with their args and line ranges. We keep the call-site list as a
            // tuple-like (path, startLine, endLine) so the splice step can run
            // per-file with the correct ranges, without re-walking the AST.
            //
            // Names are normalized through stripPyExtension before being added
            // to seenNames / collected. The legacy dialect was casual about
            // the `.py` suffix — some mods wrote `dependencies("a")`, others
            // `dependencies("a.py")`, and many had both forms across files.
            // Treating those as distinct produced duplicate import lines
            // (`import a` followed by `import a` again) when migration ran
            // against a mixed-form pack. Stripping to a canonical "bare module
            // name" gives the dedupe set a single string to compare against
            // regardless of how the original was written.
            var collected = new List<string>();
            var seenNames = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
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
                            string normalized = stripPyExtension(s.Value);
                            if (seenNames.Add(normalized)) {
                                collected.Add(normalized);
                            }
                        }
                    }
                    if (sites == null) sites = new List<(int, int)>();
                    sites.Add((ev.StartLine, ev.EndLine));
                }
                if (sites != null) perFileCallSites[file.AbsolutePath] = sites;
            }

            // Step 1b: enumerate every .py in Definitions/ so files the
            // legacy calls never named still get an import line. The legacy
            // framework loaded by directory walk; with __init__.py taking
            // over, anything missing from the imports list is silently
            // skipped, which is exactly the bug this guard prevents.
            List<string> allDefFiles = new List<string>();
            try {
                foreach (string p in Directory.EnumerateFiles(definitionsDir, "*.py",
                        SearchOption.TopDirectoryOnly)) {
                    string fname = Path.GetFileName(p);
                    if (string.Equals(fname, "__init__.py", System.StringComparison.OrdinalIgnoreCase))
                        continue;
                    allDefFiles.Add(fname);
                }
            } catch (System.Exception ex) {
                Log.Warning("LegacyMigrator: failed to enumerate '"
                            + definitionsDir + "' — " + ex.Message);
            }
            allDefFiles.Sort(System.StringComparer.OrdinalIgnoreCase);

            // Merge: collected names from the legacy calls keep their existing
            // order (load-order matters for some packs), then any sibling .py
            // file we haven't yet listed comes in alphabetically. seenNames
            // dedupe is what stops duplicate imports here too.
            foreach (string fname in allDefFiles) {
                string normalized = stripPyExtension(fname);
                if (seenNames.Add(normalized)) {
                    collected.Add(normalized);
                }
            }

            if (collected.Count == 0) {
                // No load-order work to do, but if the DLL step ran above
                // we still report success — Migrated stays true via the
                // migrateLegacyDll path.
                if (!report.Migrated) {
                    return Report.NoChange("no .py files in Definitions/ to import");
                }
                return report;
            }

            report.Migrated = true;
            report.DepsFilePath = initPath;
            report.CollectedNames = collected;

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

            // Step 3: write the consolidated __init__.py. One `import <name>`
            // per line so Python runs each module's top-level code in order.
            // collected[] is already normalized (no .py suffix, deduped), so
            // we can emit it directly.
            try {
                StringBuilder sb = new StringBuilder();
                sb.Append("# Auto-generated by the CustomAssets editor's LegacyMigrator.\n");
                sb.Append("# Consolidates the per-file dependencies(...) calls that used to\n");
                sb.Append("# live in individual Definitions/*.py files into explicit imports\n");
                sb.Append("# here — load order follows the line order below.\n");
                foreach (string name in collected) {
                    sb.Append("import ").Append(name).Append('\n');
                }
                File.WriteAllText(initPath, sb.ToString(),
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            } catch (System.Exception ex) {
                report.Error = "init write failed: " + ex.Message;
            }

            // The pack-stub copy is part of migrateLegacyDll and already ran
            // upfront when applicable. Idempotent: re-running here would just
            // overwrite the same file with the same bytes.
            if (!string.IsNullOrEmpty(report.CopiedPackStub) == false) {
                try {
                    string copied = CopyPackStubInto(pack.RootPath);
                    if (!string.IsNullOrEmpty(copied)) {
                        report.CopiedPackStub = copied;
                    }
                } catch (System.Exception ex) {
                    Log.Warning("LegacyMigrator: failed to copy CustomAssetPack.dll — " + ex.Message);
                    if (report.Error == null) report.Error = "stub copy failed: " + ex.Message;
                }
            }

            return report;
        }

        // Delete the legacy per-pack DLL (CustomAssets_<modId>.dll + .pdb) and
        // drop the shared CustomAssetPack.dll stub in its place. Runs before
        // the load-order migration so the pack is structurally on the new
        // pattern even if the modder cancels mid-flow on a corrupted pack.
        private static void migrateLegacyDll(LoadedPack pack, Report report) {
            try {
                string legacyDll = Path.Combine(pack.RootPath, pack.ModId + ".dll");
                string legacyPdb = Path.Combine(pack.RootPath, pack.ModId + ".pdb");
                if (File.Exists(legacyDll)) {
                    File.Delete(legacyDll);
                    report.DeletedLegacyDll = legacyDll;
                }
                if (File.Exists(legacyPdb)) {
                    File.Delete(legacyPdb);
                }
                string copied = CopyPackStubInto(pack.RootPath);
                if (!string.IsNullOrEmpty(copied)) {
                    report.CopiedPackStub = copied;
                }
                report.Migrated = true;
            } catch (Exception ex) {
                Log.Warning("LegacyMigrator: pack-stub DLL migration failed — "
                            + ex.Message);
                if (report.Error == null) report.Error = "DLL migration failed: " + ex.Message;
            }

            // Manifest swap is its own try/catch so a DLL copy failure
            // doesn't skip it, and a manifest write failure doesn't undo
            // the DLL move. Mafi's mod loader keys off primary_dlls +
            // primary_mod_class_name to find the IMod class to instantiate;
            // leaving the legacy "CustomAssets_<modId>.dll" entry pointing
            // at a now-deleted file would crash the loader on next launch.
            try {
                updateManifestForStub(pack, report);
            } catch (Exception ex) {
                Log.Warning("LegacyMigrator: manifest update failed — " + ex.Message);
                if (report.Error == null) report.Error = "manifest update failed: " + ex.Message;
            }
        }

        // Rewrite manifest.json's primary_dlls + primary_mod_class_name to
        // point at the shared stub. Preserves every other key via MiniJson
        // round-trip so display_name, version, mod_dependencies, etc. stay
        // exactly as the modder wrote them.
        //
        // Legacy shape:
        //   "primary_dlls": [ "CustomAssets_<modId>.dll" ]
        //   (no primary_mod_class_name)
        //
        // New shape:
        //   "primary_dlls": [ "CustomAssetPack.dll" ]
        //   "primary_mod_class_name": "CustomAssetPack"
        private static void updateManifestForStub(LoadedPack pack, Report report) {
            if (pack == null || string.IsNullOrEmpty(pack.RootPath)) return;
            string manifestPath = Path.Combine(pack.RootPath, "manifest.json");
            if (!File.Exists(manifestPath)) {
                // No manifest to update — pack must be authored manually
                // before it can load. Nothing the migrator can do here.
                return;
            }

            object root = MiniJson.Parse(File.ReadAllText(manifestPath));
            if (!(root is Dictionary<string, object> dict)) {
                Log.Warning("LegacyMigrator: manifest.json root is not a JSON object — skipping");
                return;
            }

            bool changed = false;

            // primary_dlls: rebuild from scratch with just the stub. Any
            // extra DLL entries the legacy pack listed (rare — most have
            // only the per-pack DLL) are dropped. Pure mods that ship third-
            // party DLLs alongside their own would need a manual edit
            // afterwards, but that path was already broken on the legacy
            // pattern too.
            List<object> newPrimaryDlls = new List<object> { "CustomAssetPack.dll" };
            if (!isSameStringList(dict, "primary_dlls", newPrimaryDlls)) {
                dict["primary_dlls"] = newPrimaryDlls;
                changed = true;
            }

            // primary_mod_class_name: required by the new pattern so Mafi
            // knows which class inside the shared CustomAssetPack.dll to
            // instantiate as the pack's IMod. Idempotent — if already set
            // to "CustomAssetPack" we leave it alone.
            const string PackClassName = "CustomAssetPack";
            if (!dict.TryGetValue("primary_mod_class_name", out object existing)
                    || !(existing is string s)
                    || !string.Equals(s, PackClassName, StringComparison.Ordinal)) {
                dict["primary_mod_class_name"] = PackClassName;
                changed = true;
            }

            if (changed) {
                File.WriteAllText(manifestPath, MiniJsonWriter.Write(dict),
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                report.ManifestUpdated = true;
                report.Migrated = true;
            }
        }

        // Cheap structural compare so we skip a write when the manifest is
        // already in the canonical shape — keeps mtime stable on re-runs.
        private static bool isSameStringList(
                Dictionary<string, object> dict, string key, List<object> candidate) {
            if (!dict.TryGetValue(key, out object raw)) return false;
            if (!(raw is List<object> list)) return false;
            if (list.Count != candidate.Count) return false;
            for (int i = 0; i < list.Count; i++) {
                if (!(list[i] is string a) || !(candidate[i] is string b)
                        || !string.Equals(a, b, StringComparison.Ordinal)) {
                    return false;
                }
            }
            return true;
        }

        // Locate a CustomAssetPack.dll source file on disk and copy it into
        // <packRoot>/. Also copies the matching .pdb when present (debug
        // builds ship one). Returns the destination path on success or
        // null when no source could be located.
        //
        // Source lookup tries (in order):
        //   1. The directory of the executing assembly — works when Mafi
        //      loaded the editor mod from disk.
        //   2. The COI mods sibling folder <COI_MODS>/CustomAssets/ — works
        //      when the editor was loaded from bytes and Location is empty,
        //      but the deployed mod folder still exists on disk.
        //   3. Any other pack folder under <COI_MODS>/ that already has a
        //      CustomAssetPack.dll — covers fresh installs where the editor
        //      mod folder doesn't yet ship the stub but other migrated
        //      packs do.
        internal static string CopyPackStubInto(string packRoot) {
            if (string.IsNullOrEmpty(packRoot)) return null;

            string srcDll = locateStubSourceDll(packRoot);
            if (string.IsNullOrEmpty(srcDll) || !File.Exists(srcDll)) {
                Log.Warning("LegacyMigrator: CustomAssetPack.dll source not found — "
                          + "pack-stub copy skipped. Tried executing-assembly dir, "
                          + "<COI_MODS>/CustomAssets/, and sibling pack folders.");
                return null;
            }

            string dstDll = Path.Combine(packRoot, "CustomAssetPack.dll");
            // Skip self-copy if for any reason source and destination are
            // the same file — File.Copy errors out on identical paths.
            if (!string.Equals(Path.GetFullPath(srcDll), Path.GetFullPath(dstDll),
                    StringComparison.OrdinalIgnoreCase)) {
                File.Copy(srcDll, dstDll, overwrite: true);
            }

            string srcDir = Path.GetDirectoryName(srcDll);
            if (!string.IsNullOrEmpty(srcDir)) {
                string srcPdb = Path.Combine(srcDir, "CustomAssetPack.pdb");
                if (File.Exists(srcPdb)) {
                    string dstPdb = Path.Combine(packRoot, "CustomAssetPack.pdb");
                    if (!string.Equals(Path.GetFullPath(srcPdb), Path.GetFullPath(dstPdb),
                            StringComparison.OrdinalIgnoreCase)) {
                        File.Copy(srcPdb, dstPdb, overwrite: true);
                    }
                }
            }
            return dstDll;
        }

        // Find a CustomAssetPack.dll that exists on disk. Each step is
        // wrapped in its own try so a bad Location string (or any other
        // single-source failure) can't poison the whole lookup — the
        // original "Invalid path" report was a Path.GetDirectoryName("")
        // throwing ArgumentException because Mafi loads mods via
        // Assembly.Load(byte[]) and Location is the empty string.
        private static string locateStubSourceDll(string packRoot) {
            // Step 1: executing-assembly directory, guarded against empty
            // Location (the in-memory-load case).
            try {
                string loc = typeof(LegacyMigrator).Assembly.Location;
                if (!string.IsNullOrEmpty(loc)) {
                    string dir = Path.GetDirectoryName(loc);
                    if (!string.IsNullOrEmpty(dir)) {
                        string candidate = Path.Combine(dir, "CustomAssetPack.dll");
                        if (File.Exists(candidate)) return candidate;
                    }
                }
            } catch (Exception ex) {
                Log.Warning("LegacyMigrator: assembly-location stub lookup failed — "
                            + ex.Message);
            }

            // Step 2: <COI_MODS>/CustomAssets/CustomAssetPack.dll. packRoot
            // is <COI_MODS>/<packId>/, so its parent is the mods folder.
            try {
                string trimmed = packRoot.TrimEnd(Path.DirectorySeparatorChar, '/');
                string modsDir = Path.GetDirectoryName(trimmed);
                if (!string.IsNullOrEmpty(modsDir)) {
                    string candidate = Path.Combine(modsDir, "CustomAssets",
                                                     "CustomAssetPack.dll");
                    if (File.Exists(candidate)) return candidate;
                }
            } catch (Exception ex) {
                Log.Warning("LegacyMigrator: mods-folder stub lookup failed — "
                            + ex.Message);
            }

            // Step 3: any other pack folder under <COI_MODS>/ that already
            // shipped CustomAssetPack.dll (new-pattern packs deploy it).
            // First match wins — they're all the same stub, byte-for-byte.
            try {
                string trimmed = packRoot.TrimEnd(Path.DirectorySeparatorChar, '/');
                string modsDir = Path.GetDirectoryName(trimmed);
                if (!string.IsNullOrEmpty(modsDir) && Directory.Exists(modsDir)) {
                    foreach (string sibling in Directory.EnumerateDirectories(modsDir)) {
                        string candidate = Path.Combine(sibling, "CustomAssetPack.dll");
                        if (File.Exists(candidate)
                                && !string.Equals(Path.GetFullPath(sibling),
                                       Path.GetFullPath(trimmed),
                                       StringComparison.OrdinalIgnoreCase)) {
                            return candidate;
                        }
                    }
                }
            } catch (Exception ex) {
                Log.Warning("LegacyMigrator: sibling-pack stub lookup failed — "
                            + ex.Message);
                Log.Warning("LegacyMigrator: stub lookup step skipped —sibling-pack lookup failed — " + ex.Message);
            }

            return null;
        }

        // Filenames in the legacy dependencies(...) calls are stored with
        // their `.py` extension; Python's `import` statement uses the bare
        // module name. Strip the suffix at write time so the generated
        // __init__.py is valid Python.
        private static string stripPyExtension(string name) {
            if (string.IsNullOrEmpty(name)) return name;
            if (name.EndsWith(".py", System.StringComparison.OrdinalIgnoreCase)) {
                return name.Substring(0, name.Length - 3);
            }
            return name;
        }
    }
}
