using System;
using System.IO;
using CustomAssets.Ui.Primitives;
using Mafi;

namespace CustomAssets.Data.Mod {

    /// Ensures the stock primitive .obj files (Wide box, Tall box, Flat plate,
    /// Cylinder horizontal / vertical, Container+heap, Custom box) exist on
    /// disk inside the deployed CustomAssets mod folder. Modders' packs
    /// reference these by mod-relative path ("Assets/Primitives/wide_box.obj");
    /// <see cref="CustomAssetRegistrator"/> falls back to this folder when a
    /// pack-relative mesh path can't be found locally.
    ///
    /// Idempotent: existing files are NOT overwritten so manual edits survive
    /// a process restart. Delete the file (or the whole folder) to force
    /// regeneration on next launch.
    ///
    /// Source of truth = <see cref="PrimitiveCatalog"/> + the procedural
    /// builders in <see cref="PrimitiveShapes"/>, so the in-game editor
    /// preview and the shipped on-disk files share a single generator.
    internal static class PrimitiveStockGenerator {

        public const string PrimitivesSubfolder = "Assets/Primitives";

        private static bool s_alreadyRan;
        private static string s_targetDir;

        /// <summary>Absolute path to the stock primitives folder under the
        /// running CustomAssets mod's deploy directory. Computed lazily so
        /// callers that need to scan the folder (e.g. asset pickers) don't
        /// have to repeat the assembly-location lookup.</summary>
        public static string TargetDir {
            get {
                if (s_targetDir != null) return s_targetDir;
                string modRoot = ResolveCoreModRoot();
                if (string.IsNullOrEmpty(modRoot)) return null;
                s_targetDir = Path.Combine(modRoot, PrimitivesSubfolder);
                return s_targetDir;
            }
        }

        /// <summary>Mod root for the running CustomAssets DLL — sourced
        /// from <see cref="PackRegistry.CoreModBasePath"/> which is set
        /// during <c>CustomAssetsMod.RegisterPrototypes</c> from the
        /// active mod's manifest. <see cref="Assembly.GetExecutingAssembly"/>
        /// .Location returns empty under Mafi's <c>non_locking_dll_load</c>
        /// (the DLL is loaded from memory after a copy), so that's not a
        /// reliable source. Returns null when the core mod hasn't started
        /// yet — callers should treat this as "skip for now, try later".
        /// </summary>
        public static string ResolveCoreModRoot() {
            return PackRegistry.CoreModBasePath;
        }

        /// <summary>Generate any missing stock primitive files. Safe to call
        /// repeatedly; only the first call does work, subsequent calls are
        /// no-ops. Failures are logged but never thrown — a primitive that
        /// fails to write must not block the rest of the registrator.</summary>
        public static void EnsureGenerated() {
            if (s_alreadyRan) return;
            s_alreadyRan = true;

            string dir = TargetDir;
            if (string.IsNullOrEmpty(dir)) {
                Log.Warning("PrimitiveStockGenerator: target directory unresolved; skipping.");
                return;
            }

            try {
                Directory.CreateDirectory(dir);
            } catch (Exception ex) {
                Log.Warning("PrimitiveStockGenerator: cannot create '" + dir
                    + "' — " + ex.Message);
                return;
            }

            int created = 0;
            int skipped = 0;
            foreach (PrimitivePreset preset in PrimitiveCatalog.All) {
                string baseName = preset.DefaultBaseName ?? preset.Id;
                string objPath = Path.Combine(dir, baseName + ".obj");
                string pngPath = Path.Combine(dir, baseName + "_uv.png");

                bool needObj = !File.Exists(objPath);
                bool needPng = !File.Exists(pngPath);
                if (!needObj && !needPng) {
                    skipped++;
                    continue;
                }

                try {
                    ShapeResult shape = preset.Build(preset.Width, preset.Height, preset.Depth);
                    shape.Mesh.Name = baseName;

                    if (needObj) {
                        File.WriteAllText(objPath, shape.Mesh.ToObj());
                    }
                    if (needPng) {
                        byte[] png = PrimitiveWireframe.RenderPng(shape, 1024);
                        if (png != null) File.WriteAllBytes(pngPath, png);
                    }
                    created++;
                } catch (Exception ex) {
                    Log.Warning("PrimitiveStockGenerator: failed to generate '" + baseName
                        + "': " + ex.Message);
                }
            }

            Log.Info("PrimitiveStockGenerator: " + created + " generated, "
                + skipped + " already present, in " + dir);
        }
    }
}
