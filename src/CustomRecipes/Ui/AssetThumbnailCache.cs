using System;
using System.Collections.Generic;
using System.IO;
using Mafi;
using UnityEngine;

namespace CustomAssets.Ui {

    /// Shared cache for on-disk image thumbnails (PNG/JPG). The recipe editor's
    /// AssetPathPicker hits this for every preview thumbnail in the popup —
    /// scanning a pack with dozens of textures would stutter the UI if every
    /// row decoded the file on layout.
    ///
    /// Keyed by absolute path; lookups for missing or unparseable files cache
    /// a "not found" sentinel so repeated misses don't re-stat the disk.
    /// Mirrors <see cref="PackThumbnailCache"/>'s lifecycle.
    public static class AssetThumbnailCache {

        private static readonly Dictionary<string, Texture2D> s_byPath =
            new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> s_checkedNotFound =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public static Texture2D LoadPng(string absolutePath) {
            if (string.IsNullOrEmpty(absolutePath)) return null;
            if (s_byPath.TryGetValue(absolutePath, out Texture2D cached)) return cached;
            if (s_checkedNotFound.Contains(absolutePath)) return null;
            if (!File.Exists(absolutePath)) {
                s_checkedNotFound.Add(absolutePath);
                return null;
            }
            try {
                byte[] data = File.ReadAllBytes(absolutePath);
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
                if (tex.LoadImage(data, markNonReadable: true)) {
                    tex.wrapMode = TextureWrapMode.Clamp;
                    tex.filterMode = FilterMode.Bilinear;
                    s_byPath[absolutePath] = tex;
                    return tex;
                }
                UnityEngine.Object.Destroy(tex);
            } catch (Exception ex) {
                Log.Warning("AssetThumbnailCache: failed to load " + absolutePath + " — " + ex.Message);
            }
            s_checkedNotFound.Add(absolutePath);
            return null;
        }

        public static void Clear() {
            foreach (Texture2D tex in s_byPath.Values) {
                if (tex != null) UnityEngine.Object.Destroy(tex);
            }
            s_byPath.Clear();
            s_checkedNotFound.Clear();
        }
    }
}
