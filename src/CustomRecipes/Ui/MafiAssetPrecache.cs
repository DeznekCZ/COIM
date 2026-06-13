using System;
using CustomAssets.Editor;
using Mafi.Unity.UiToolkit;

namespace CustomAssets.Ui {

    /// One-shot warmup for <c>AssetsDb</c>'s sprite cache covering every image
    /// constant on <c>Mafi.Base.Assets</c>. Without this, the first time
    /// <see cref="Components.AssetPathPicker"/> opens its popup the UI thread
    /// stalls for ~1s loading 800+ sprites; running the warmup at window-attach
    /// time spreads the cost across one frame at construction (no user input
    /// is waiting on the editor at that point) and every subsequent picker
    /// open finds the textures already memoized.
    public static class MafiAssetPrecache {

        private static bool s_done;

        /// <summary>Idempotent — safe to call from every Window constructor.
        /// The first invocation does the work; later calls are no-ops.</summary>
        public static void Warm(UiRoot root) {
            if (s_done || root == null || root.AssetsDb == null) return;
            s_done = true;
            try {
                foreach (AssetsCatalog.MafiAsset a in AssetsCatalog.ImagesOnly()) {
                    try { root.AssetsDb.GetSharedSprite(a.ValuePath); }
                    catch {
                        // Per-sprite failures are common (some assets only
                        // exist in scenes that haven't loaded yet). The next
                        // GetSharedSprite call will retry, so swallowing here
                        // keeps the warmup from short-circuiting on one bad
                        // entry.
                    }
                }
            } catch (Exception ex) {
                Mafi.Log.Warning("MafiAssetPrecache warm failed: " + ex.Message);
            }
        }
    }
}
