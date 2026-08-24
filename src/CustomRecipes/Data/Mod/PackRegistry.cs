using System.Collections.Generic;
using System.IO;
using CustomAssets.Python;
using Mafi;
using PythonAPI;
using PythonAPI.Statements;

namespace CustomAssets.Data.Mod {

    /// One file inside a pack: the absolute path on disk and the parsed Python AST. Held so
    /// the editor can re-walk the AST (extracting build_* calls into PackModel) without
    /// re-reading and re-tokenising files from disk.
    public sealed class LoadedFile {
        public readonly string AbsolutePath;
        public readonly Block Ast;

        public LoadedFile(string absolutePath, Block ast) {
            AbsolutePath = absolutePath;
            Ast = ast;
        }
    }

    /// One pack — a mod folder containing manifest.json + Definitions/*.py. Identified by
    /// the mod's manifest Id. Files are appended in load order (the same order
    /// CustomAssetRegistrator processed them: __init__.py first, then EnumerateFiles).
    public sealed class LoadedPack {
        public readonly string ModId;
        public readonly string RootPath;
        public readonly List<LoadedFile> Files = new List<LoadedFile>();

        public LoadedPack(string modId, string rootPath) {
            ModId = modId;
            RootPath = rootPath;
        }
    }

    /// Static singleton populated by CustomAssetRegistrator.register() as packs load.
    /// The in-game recipe editor reads from this — the editor does NOT scan mod folders
    /// itself, so the registry is the single source of truth for "which packs exist".
    ///
    /// Why a static singleton (matching CustomAssetManager.Instance):
    ///   - CustomAssetRegistrator is instantiated manually (new CustomAssetRegistrator()),
    ///     not via DI, so it can't receive a DI-resolved registry in its constructor.
    ///   - The editor controller IS DI-managed and needs to read this data later.
    ///   - A static singleton bridges the two without forcing a DI rewrite of the
    ///     registrator.
    public static class PackRegistry {
        private static readonly Dictionary<string, LoadedPack> s_packs =
            new Dictionary<string, LoadedPack>();

        /// Absolute path of the CustomAssets mod's own folder, captured by
        /// <see cref="CustomAssetsMod.RegisterPrototypes"/> at game-start.
        /// Used as the authoritative "where do mods live" anchor by the
        /// new-pack scaffolder so creating a pack works on a fresh save
        /// where no user packs have been loaded yet â€” the editor itself
        /// is shipped inside a mod folder, so its parent IS the COI mods
        /// directory.
        public static string CoreModBasePath;

        /// Reset all recorded packs. Called from CustomAssetsMod.RegisterPrototypes alongside
        /// CustomAssetManager.Clear() so a fresh game session starts with an empty registry.
        public static void Clear() {
            s_packs.Clear();
            CustomAssets.Ui.PackManifestCache.Clear();
            CustomAssets.Ui.PackThumbnailCache.Clear();
        }

        /// Create-or-fetch the pack entry for a given mod. Idempotent — calling twice for the
        /// same modId returns the existing entry (rootPath is set on first call and not
        /// re-validated; mods shouldn't change their root mid-load).
        public static LoadedPack GetOrAdd(string modId, string rootPath) {
            if (!s_packs.TryGetValue(modId, out LoadedPack pack)) {
                pack = new LoadedPack(modId, rootPath);
                s_packs[modId] = pack;
            }
            return pack;
        }

        /// Append a parsed file to a pack. No-op if the pack hasn't been registered yet
        /// (defensive — callers should always GetOrAdd first).
        public static void RecordFile(string modId, string absolutePath, Block ast) {
            if (s_packs.TryGetValue(modId, out LoadedPack pack)) {
                pack.Files.Add(new LoadedFile(absolutePath, ast));
            }
        }

        /// Snapshot of all currently registered packs. The IEnumerable can be safely iterated
        /// while load is in progress; underlying dictionary mutations during iteration would
        /// throw — but in practice the editor only reads after RegisterData is complete.
        public static IEnumerable<LoadedPack> Packs => s_packs.Values;

        public static int Count => s_packs.Count;

        public static bool TryGet(string modId, out LoadedPack pack) {
            return s_packs.TryGetValue(modId, out pack);
        }

        /// True iff the pack uses any of the legacy conventions:
        ///   1. No <c>Definitions/__init__.py</c> AND at least one .py file
        ///      declares load order via a top-level <c>dependencies(...)</c>
        ///      call. (Original legacy shape.)
        ///   2. The pack folder still contains a per-pack
        ///      <c>CustomAssets_&lt;modId&gt;.dll</c> from the old build
        ///      pattern, where each pack shipped its own generated DLL
        ///      instead of the shared <c>CustomAssetPack.dll</c> stub.
        ///   3. The pack's <c>manifest.json</c> still references the legacy
        ///      DLL name in <c>primary_dlls</c> (even after the DLL itself
        ///      was deleted) or is missing <c>primary_mod_class_name</c>.
        ///
        /// The editor's LegacyBanner is shown when this returns true (and the
        /// modder hasn't dismissed it this session). The migration step that
        /// the banner advertises is implemented by LegacyMigrator.
        public static bool IsLegacyStructure(LoadedPack pack) {
            if (pack == null || string.IsNullOrEmpty(pack.RootPath)) return false;

            // Per-pack DLL leftover from the old build pattern is by itself
            // enough — once the legacy DLL exists in the deploy folder, Mafi
            // will pick it up over the shared stub and the pack runs against
            // a stale copy of the framework even after the modder updates
            // the editor mod. Catch it whether or not the load-order shape
            // was migrated.
            if (hasLegacyPerPackDll(pack)) return true;

            // Manifest still pointing at the legacy DLL name is enough on its
            // own — without primary_dlls = CustomAssetPack.dll, the loader
            // won't find any IMod class even though the data files migrated.
            if (hasLegacyManifest(pack)) return true;

            // Check for __init__.py via the registered files list (cheaper
            // than touching disk; mirrors what the loader already saw).
            bool hasInit = false;
            foreach (LoadedFile f in pack.Files) {
                if (Path.GetFileName(f.AbsolutePath) == "__init__.py") {
                    hasInit = true;
                    break;
                }
            }
            if (hasInit) return false;

            // No __init__.py → check whether any file declares load order
            // inline via top-level `dependencies(...)`. We walk the cached AST
            // so the check is free of disk I/O.
            foreach (LoadedFile f in pack.Files) {
                if (hasTopLevelDependenciesCall(f.Ast)) return true;
            }
            return false;
        }

        /// True iff the pack folder contains the per-pack DLL produced by the
        /// pre-stub build pattern (<c>CustomAssets_&lt;modId&gt;.dll</c>).
        /// Public so the LegacyMigrator can also check + delete it.
        public static bool hasLegacyPerPackDll(LoadedPack pack) {
            if (pack == null
                    || string.IsNullOrEmpty(pack.RootPath)
                    || string.IsNullOrEmpty(pack.ModId)) {
                return false;
            }
            return File.Exists(Path.Combine(pack.RootPath, pack.ModId + ".dll"));
        }

        /// True iff manifest.json still references the legacy DLL shape: an
        /// entry in <c>primary_dlls</c> that isn't <c>CustomAssetPack.dll</c>,
        /// or a missing <c>primary_mod_class_name</c> on a pack whose data
        /// shape otherwise already migrated. The migrator's manifest step
        /// fixes both in one pass.
        public static bool hasLegacyManifest(LoadedPack pack) {
            if (pack == null || string.IsNullOrEmpty(pack.RootPath)) return false;
            string manifestPath = Path.Combine(pack.RootPath, "manifest.json");
            if (!File.Exists(manifestPath)) return false;
            try {
                object root = MiniJson.Parse(File.ReadAllText(manifestPath));
                if (!(root is Dictionary<string, object> dict)) return false;

                // primary_dlls must contain CustomAssetPack.dll (and only
                // that — extra entries imply a legacy-or-custom packaging
                // we should flag for re-migration).
                if (!dict.TryGetValue("primary_dlls", out object rawDlls)
                        || !(rawDlls is List<object> dlls)
                        || dlls.Count != 1
                        || !(dlls[0] is string dll)
                        || !string.Equals(dll, "CustomAssetPack.dll",
                                System.StringComparison.Ordinal)) {
                    return true;
                }

                // primary_mod_class_name must be present and = CustomAssetPack.
                if (!dict.TryGetValue("primary_mod_class_name", out object rawClass)
                        || !(rawClass is string cls)
                        || !string.Equals(cls, "CustomAssetPack",
                                System.StringComparison.Ordinal)) {
                    return true;
                }
                return false;
            } catch (System.Exception ex) {
                Log.Warning("PackRegistry.hasLegacyManifest: " + ex.Message);
                return false;
            }
        }

        // Detect a top-level `dependencies("a", "b", ...)` call. The AST shape
        // is EvaluateStatement → CallExpression with Calle = VariableExpression
        // of name "dependencies".
        private static bool hasTopLevelDependenciesCall(Block ast) {
            if (ast == null) return false;
            foreach (var statement in ast.statements) {
                if (statement is PythonAPI.EvaluateStatement ev
                    && ev.Expression is PythonAPI.Expressions.CallExpression call
                    && call.Calle is PythonAPI.Expressions.VariableExpression v
                    && v.Path == "dependencies") {
                    return true;
                }
            }
            return false;
        }

        /// Re-tokenise and re-parse every Definitions/*.py in the pack, replacing the
        /// cached ASTs in `pack.Files`. Called by the editor after PackEmitter.Save —
        /// the saved files have shifted line ranges, and the next PackLoader.Load(pack)
        /// would otherwise return stale RecipeDef.SourceStartLine/EndLine values.
        ///
        /// Same parse logic as CustomAssetRegistrator.register, but without executing
        /// the AST (we only need the parse tree for editing, not the side effects of
        /// build_recipe / build_product_* calls).
        public static void RescanPack(LoadedPack pack) {
            pack.Files.Clear();
            DirectoryInfo modules = new DirectoryInfo(Path.Combine(pack.RootPath, "Definitions"));
            if (!modules.Exists) return;
            // __init__.py FIRST, matching CustomAssetRegistrator.RegisterData and
            // this list's documented contract. A bare EnumerateFiles would leave
            // Files in directory order, so after the first save of a session the
            // list silently stopped meaning "load order" — which anything
            // reasoning about definition order (see PackLoadOrder) depends on.
            List<FileInfo> ordered = new List<FileInfo>();
            FileInfo initFile = new FileInfo(Path.Combine(modules.FullName, "__init__.py"));
            if (initFile.Exists) ordered.Add(initFile);
            foreach (FileInfo file in modules.EnumerateFiles("*.py")) {
                if (string.Equals(file.FullName, initFile.FullName,
                        System.StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }
                ordered.Add(file);
            }
            foreach (FileInfo file in ordered) {
                Token[] tokens = Tokenizer.ParseFile(file.FullName);
                Block block = Lexer.Parse(tokens);
                pack.Files.Add(new LoadedFile(file.FullName, block));
            }
        }
    }
}
