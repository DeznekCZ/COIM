using System;
using System.Collections.Generic;
using System.IO;
using CustomAssets.Data.Mod;
using Mafi;
using Mafi.Core.Mods;
using UnityEngine;

namespace CustomAssets.Ui {

    /// Loads `<pack-root>/Thumbnail.png` per pack and caches the Texture2D by ModId.
    /// Mirrors COI's own ModTile.tryGetModThumbnail convention so packs that already
    /// ship a thumbnail for the in-game mods panel work in the recipe editor too with
    /// no extra files.
    ///
    /// Cached for the lifetime of the game session — the dropdown's OptionFactory can
    /// fire repeatedly (once on layout, once per popup open), and reading + decoding
    /// a PNG on every call would stutter the UI for packs with large thumbnails.
    /// Cache is keyed by ModId, not RootPath, so a pack reload via PackRegistry.Clear()
    /// followed by a re-register reuses textures from prior loads. PackRegistry.Clear()
    /// also calls ClearCache() to drop stale textures if the pack's Thumbnail.png changed.
    public static class PackThumbnailCache {

        // Fallback asset path used when a pack has no Thumbnail.png. Same generic icon
        // COI uses in the mods panel — keeps visual consistency for the modder.
        public const string FallbackIconPath = "Assets/Unity/UserInterface/General/ModLarge.svg";

        // Sentinel value cached for packs that have no Thumbnail.png on disk, so we
        // don't re-stat the filesystem on every dropdown render. Keys with this value
        // mean "checked, no file" — caller should use FallbackIconPath.
        private static readonly Texture2D NotFoundSentinel = null;

        private static readonly Dictionary<string, Texture2D> s_byModId =
            new Dictionary<string, Texture2D>();
        private static readonly HashSet<string> s_checkedNotFound = new HashSet<string>();

        /// Returns the cached Texture2D for the pack's Thumbnail.png, or null if the
        /// pack has no thumbnail (caller should display FallbackIconPath instead).
        public static Texture2D TryGet(LoadedPack pack) {
            if (pack == null) return NotFoundSentinel;
            return tryGetByModRoot(pack.ModId, pack.RootPath);
        }

        /// Same lookup but for an arbitrary loaded mod (the LoadedModPicker
        /// shows EVERY loaded mod, not just CustomAssets packs, so it needs
        /// to reach mods that aren't in PackRegistry). Reuses the same
        /// ModId-keyed cache so a CustomAssets pack that's BOTH a pack and
        /// a mod (it always is) loads its thumbnail once.
        public static Texture2D TryGet(LoadedModData mod) {
            if (mod?.Manifest == null) return NotFoundSentinel;
            return tryGetByModRoot(mod.Manifest.Id, mod.Manifest.RootDirectoryPath);
        }

        private static Texture2D tryGetByModRoot(string modId, string rootPath) {
            if (string.IsNullOrEmpty(modId)) return NotFoundSentinel;
            if (s_byModId.TryGetValue(modId, out Texture2D cached)) return cached;
            if (s_checkedNotFound.Contains(modId)) return NotFoundSentinel;

            string path = Path.Combine(rootPath ?? "", "Thumbnail.png");
            if (!File.Exists(path)) {
                s_checkedNotFound.Add(modId);
                return NotFoundSentinel;
            }

            try {
                byte[] data = File.ReadAllBytes(path);
                // markNonReadable: true frees the CPU-side pixel buffer after upload —
                // matches what ModTile does for its mod-panel thumbnails.
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
                if (tex.LoadImage(data, markNonReadable: true)) {
                    tex.wrapMode = TextureWrapMode.Clamp;
                    tex.filterMode = FilterMode.Bilinear;
                    s_byModId[modId] = tex;
                    return tex;
                }
                UnityEngine.Object.Destroy(tex);
            } catch (Exception ex) {
                Log.Warning($"PackThumbnailCache: failed to load {path}: {ex.Message}");
            }
            s_checkedNotFound.Add(modId);
            return NotFoundSentinel;
        }

        /// Drop all cached textures. Call when PackRegistry is rebuilt so a pack whose
        /// Thumbnail.png changed on disk picks up the new image. Destroys the underlying
        /// Texture2D objects to free GPU memory.
        public static void Clear() {
            foreach (Texture2D tex in s_byModId.Values) {
                if (tex != null) UnityEngine.Object.Destroy(tex);
            }
            s_byModId.Clear();
            s_checkedNotFound.Clear();
        }
    }
}
