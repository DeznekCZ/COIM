using System.Collections.Generic;
using System.IO;
using CustomAssets.Data.Mod;
using PythonAPI;
using PythonAPI.Expressions;
using PythonAPI.Statements;

namespace CustomAssets.Editor.Io {

    /// <summary>
    /// Works out the order in which a pack's files actually CREATE prototypes at
    /// runtime, so the editor can tell whether one definition is visible to
    /// another. The COI Python runtime has no forward references: a definition
    /// that names another by bare id only resolves if that other one was already
    /// created.
    ///
    /// This deliberately does NOT trust <see cref="LoadedPack.Files"/> order:
    ///   - <see cref="PackRegistry.RecordFile"/> records a file BEFORE executing
    ///     it, so a file pulled in by <c>dependencies(...)</c> is recorded AFTER
    ///     its includer even though its definitions run FIRST.
    ///   - <see cref="PackRegistry.RescanPack"/> (which the editor runs after
    ///     every save) rebuilds the list from a bare directory enumeration.
    /// Recomputing from the ASTs instead keeps the answer stable across saves.
    ///
    /// The order mirrors <c>CustomAssetRegistrator.RegisterData</c>:
    ///   1. <c>__init__.py</c> first when present, then the remaining files in
    ///      directory-enumeration order.
    ///   2. A <c>dependencies("x")</c> call loads <c>x.py</c> to completion at
    ///      that point, so a file's dependencies are emitted BEFORE the file
    ///      itself — a post-order walk. This assumes the documented convention
    ///      that <c>dependencies(...)</c> sits at the top of a file; a call
    ///      placed after some definitions would make those definitions land
    ///      earlier than modelled here, which can only under-report.
    /// </summary>
    public static class PackLoadOrder {

        /// Map of absolute file path → 0-based position in effective load order.
        /// Files not reachable from the walk still get a position (appended in
        /// enumeration order) so every def has an answer.
        public static Dictionary<string, int> Compute(LoadedPack pack) {
            var order = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
            if (pack == null) return order;

            // Index the ASTs by path so dependency resolution doesn't touch disk.
            var astByPath = new Dictionary<string, Block>(System.StringComparer.OrdinalIgnoreCase);
            foreach (LoadedFile f in pack.Files) {
                if (f?.AbsolutePath == null) continue;
                astByPath[f.AbsolutePath] = f.Ast;
            }

            var visited = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            var sequence = new List<string>();

            // __init__.py first — same as the registrator.
            foreach (LoadedFile f in pack.Files) {
                if (f?.AbsolutePath == null) continue;
                if (Path.GetFileName(f.AbsolutePath) == "__init__.py") {
                    visit(f.AbsolutePath, astByPath, visited, sequence);
                    break;
                }
            }
            foreach (LoadedFile f in pack.Files) {
                if (f?.AbsolutePath == null) continue;
                visit(f.AbsolutePath, astByPath, visited, sequence);
            }

            for (int i = 0; i < sequence.Count; i++) {
                order[sequence[i]] = i;
            }
            return order;
        }

        // Post-order: everything this file pulls in is emitted before the file
        // itself. `visited` is marked on ENTRY, so an accidental dependency cycle
        // terminates instead of recursing forever (the runtime would fail on such
        // a pack anyway — here we just need to not hang the editor).
        private static void visit(string path, Dictionary<string, Block> astByPath,
                HashSet<string> visited, List<string> sequence) {
            if (path == null || !visited.Add(path)) return;

            if (astByPath.TryGetValue(path, out Block ast)) {
                string dir = Path.GetDirectoryName(path);
                foreach (string dep in dependenciesOf(ast)) {
                    if (string.IsNullOrEmpty(dir)) continue;
                    visit(Path.Combine(dir, dep + ".py"), astByPath, visited, sequence);
                }
            }
            sequence.Add(path);
        }

        // Sibling modules this file pulls in, in source order. BOTH spellings
        // count, because both end up invoking the same `dependencies` helper:
        //
        //   import battery_products          → LocalImportStatement
        //   dependencies("battery_products") → EvaluateStatement/CallExpression
        //
        // Missing the `import` form would be worse than doing nothing: packs use
        // it precisely BECAUSE their files don't sort into dependency order by
        // name (Batteries has battery_charger.py referencing products defined in
        // battery_products.py — alphabetically backwards), so falling back to
        // enumeration order would report every one of those as a violation.
        //
        // `from X import a, b` (ImportStatement) is a different thing entirely —
        // it pulls symbols out of Mafi/CustomAssets and loads no pack file.
        private static List<string> dependenciesOf(Block ast) {
            var names = new List<string>();
            if (ast?.statements == null) return names;
            foreach (IStatement statement in ast.statements) {
                if (statement is LocalImportStatement local) {
                    if (!string.IsNullOrEmpty(local.moduleName)) names.Add(local.moduleName);
                    continue;
                }
                if (!(statement is EvaluateStatement ev)) continue;
                if (!(ev.Expression is CallExpression call)) continue;
                if (!(call.Calle is VariableExpression v) || v.Path != "dependencies") continue;
                if (call.Arguments == null) continue;
                foreach (var arg in call.Arguments) {
                    if (arg?.Expression is StringConstant s && !string.IsNullOrEmpty(s.Value)) {
                        names.Add(s.Value);
                    }
                }
            }
            return names;
        }
    }
}
