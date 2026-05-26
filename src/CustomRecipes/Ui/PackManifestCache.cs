using System.Collections.Generic;
using System.IO;
using CustomAssets.Data.Mod;

namespace CustomAssets.Ui {

    /// <summary>
    /// Loads each pack's <c>manifest.json</c> once and caches the parsed dict
    /// so the editor's UI components (pack card, pack picker, deps dialog,
    /// …) can pull display name + short description without re-reading the
    /// file on every render. The cache keys by ModId; entries are populated
    /// lazily on first access and cleared by <c>PackRegistry.Clear()</c>
    /// (called from CustomAssetsMod.RegisterPrototypes at game start).
    ///
    /// Manifest format reference (see Mafi.Core.Mods.ModManifest):
    ///   {
    ///     "id":                "CustomAssets_Batteries",
    ///     "display_name":      "Batteries [CAL]",
    ///     "description_short": "Adds Lead-acid …",
    ///     …
    ///   }
    /// </summary>
    public static class PackManifestCache {

        private static readonly Dictionary<string, Dictionary<string, object>> s_byModId
            = new Dictionary<string, Dictionary<string, object>>();
        private static readonly HashSet<string> s_attempted = new HashSet<string>();

        /// <summary>Display name from <c>manifest.json["display_name"]</c>,
        /// falling back to ModId when the manifest is missing or has no
        /// display name.</summary>
        public static string DisplayName(LoadedPack pack) {
            if (pack == null) return "";
            Dictionary<string, object> m = getOrLoad(pack);
            if (m == null) return pack.ModId;
            if (m.TryGetValue("display_name", out object v) && v is string s
                && !string.IsNullOrEmpty(s)) return s;
            return pack.ModId;
        }

        /// <summary>Short description from <c>manifest.json["description_short"]</c>,
        /// or empty when missing.</summary>
        public static string DescriptionShort(LoadedPack pack) {
            if (pack == null) return "";
            Dictionary<string, object> m = getOrLoad(pack);
            if (m == null) return "";
            if (m.TryGetValue("description_short", out object v) && v is string s) return s;
            return "";
        }

        /// <summary>Drop all cached manifests. Call when packs are re-scanned
        /// so a manifest edited externally is picked up next access.</summary>
        public static void Clear() {
            s_byModId.Clear();
            s_attempted.Clear();
        }

        // Try-once lookup: returns the cached dict, attempts a fresh parse on
        // first call for a given ModId, and remembers parse failures so we
        // don't spam disk I/O on every UI repaint when a pack has no
        // manifest.json or a malformed one.
        private static Dictionary<string, object> getOrLoad(LoadedPack pack) {
            if (s_byModId.TryGetValue(pack.ModId, out Dictionary<string, object> cached))
                return cached;
            if (s_attempted.Contains(pack.ModId)) return null;
            s_attempted.Add(pack.ModId);

            string path = Path.Combine(pack.RootPath ?? "", "manifest.json");
            if (!File.Exists(path)) return null;

            try {
                object parsed = MiniJson.Parse(File.ReadAllText(path));
                if (parsed is Dictionary<string, object> root) {
                    s_byModId[pack.ModId] = root;
                    return root;
                }
            } catch {
                // Malformed manifest — leave uncached so a future edit can
                // recover by re-Clear()'ing the cache. Editor falls back to
                // ModId-only display.
            }
            return null;
        }
    }
}
