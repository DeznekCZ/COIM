using System;
using System.Collections.Generic;
using System.IO;
using CustomAssets.Ui.Primitives;
using Mafi;
using UnityEngine;

namespace CustomAssets.Ui {

    /// On-disk thumbnail cache for Wavefront .obj meshes. Parallel to
    /// <see cref="AssetThumbnailCache"/>: keyed by absolute path, parses
    /// the file once, renders an axonometric flat-shaded preview via
    /// <see cref="PrimitiveWireframe.RenderMesh3DTexture"/>, and caches
    /// the resulting <see cref="Texture2D"/> so subsequent picker opens
    /// don't re-parse / re-render the file.
    ///
    /// Lookups for missing or unparseable files cache a "not found"
    /// sentinel so repeated misses don't re-stat the disk — same shape
    /// as the PNG cache. The thumbnail size is fixed at 128 px so the
    /// AssetPathPicker option rows and the trigger thumbnail share one
    /// scaled instance.
    public static class MeshThumbnailCache {

        private const int ThumbPixelSize = 128;

        private static readonly Dictionary<string, Texture2D> s_byPath =
            new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> s_checkedNotFound =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public static Texture2D LoadObj(string absolutePath) {
            if (string.IsNullOrEmpty(absolutePath)) return null;
            if (s_byPath.TryGetValue(absolutePath, out Texture2D cached)) return cached;
            if (s_checkedNotFound.Contains(absolutePath)) return null;
            if (!File.Exists(absolutePath)) {
                s_checkedNotFound.Add(absolutePath);
                return null;
            }
            try {
                string objText = File.ReadAllText(absolutePath);
                PrimitiveMesh mesh = ObjMeshLoader.Parse(objText);
                Texture2D tex = PrimitiveWireframe.RenderMesh3DTexture(mesh, ThumbPixelSize);
                if (tex != null) {
                    s_byPath[absolutePath] = tex;
                    return tex;
                }
            } catch (Exception ex) {
                Log.Warning("MeshThumbnailCache: failed to load " + absolutePath + " — " + ex.Message);
            }
            s_checkedNotFound.Add(absolutePath);
            return null;
        }

        /// Drop the cached entry for a single path. Use when the file
        /// has been overwritten by a fresh OBJ (e.g. the primitive shape
        /// dialog regenerating the same name) so the next picker open
        /// re-renders the updated geometry instead of serving stale art.
        public static void Invalidate(string absolutePath) {
            if (string.IsNullOrEmpty(absolutePath)) return;
            if (s_byPath.TryGetValue(absolutePath, out Texture2D tex) && tex != null) {
                UnityEngine.Object.Destroy(tex);
            }
            s_byPath.Remove(absolutePath);
            s_checkedNotFound.Remove(absolutePath);
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
