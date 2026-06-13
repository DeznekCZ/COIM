using System;
using System.Collections.Generic;
using System.Linq;
using Mafi;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Products;
using Mafi.Unity;
using Mafi.Unity.Terrain;
using UnityEngine;

namespace CustomAssets.Data.Mod
{
    /// Plan to materialize a unit-product prefab GameObject at injection time, after the
    /// referenced material has been resolved. ProductsRenderer expects a single GameObject
    /// with no children, exactly one MeshFilter (with sharedMesh) and one MeshRenderer (with
    /// sharedMaterial); this builder produces exactly that shape.
    internal sealed class DeferredPrefab
    {
        public string OutputPath;       // asset path the prefab gets registered under
        public Mesh Mesh;               // built at register time (box or .obj-loaded)
        public string MaterialPath;     // resolved through loadedAssets at injection time;
                                        // points at the DeferredMaterial built before us
    }

    /// Plan to materialize a Unity Material at injection time, when the live AssetsDb
    /// is available and any referenced material can be looked up by path.
    /// Albedo/Normals/SmoothMetal are stored as lists for forward compatibility with multi-
    /// slice variant rendering; today the patcher consumes only index 0 per product because
    /// LPMM allocates one Texture2DArray slice per LooseProductProto.
    internal sealed class DeferredMaterial
    {
        public string OutputPath;
        public Texture2D NewTexture;          // legacy: single-texture form used by add_texture_material
        public List<Texture2D> AlbedoArray;   // populated by add_loose_product_material; preferred over NewTexture
        public List<Texture2D> NormalsArray;  // null = inherit from reference (no override)
        public List<Texture2D> SmoothMetalArray; // null = inherit from reference (no override)
        public string ReferencePath; // resolved through assets.LoadedAssets at injection time
        public string ShaderName;    // alternative: build fresh from Shader.Find(...)
        public float Tiling = 1f;    // texture repetition factor when blitting into the array slice
                                     // (e.g. 4 = source repeated 4×4 times → rocks render 1/4 the size).
    }

    [GlobalDependency(RegistrationMode.AsSelf, false, false)]
    public class CustomAssetManager
    {
        public static Dict<string, UnityEngine.Object> Alternations { get; } = new Dict<string, UnityEngine.Object>();

        // Materials that need the live AssetsDb (e.g. cloning a vanilla material).
        // Filled by add_texture_material; processed during the ctor.
        internal static List<DeferredMaterial> PendingMaterials { get; } = new List<DeferredMaterial>();

        // Prefabs that depend on a deferred material to be built first.
        // Filled by add_unit_prefab; processed AFTER PendingMaterials in EnsureInjected so the
        // resolved Material is available when we wire up MeshRenderer.sharedMaterial.
        internal static List<DeferredPrefab> PendingPrefabs { get; } = new List<DeferredPrefab>();

        // Late-bound game textures. Key is the parent asset's path in
        // Alternations (the prefab/material that needs the texture on its
        // mainTexture slot); value is the game asset path to resolve
        // against AssetsDb at injection time. Filled by consumers
        // (add_prefab_box, add_unit_prefab, add_texture_material) when they
        // receive a Tex marked loadedAsset=true from add_texture(...).
        public static Dict<string, string> PendingGameTextures { get; } = new Dict<string, string>();

        // Mesh cache: a single Mesh instance is shared across every add_unit_prefab call that
        // references the same .obj path. Saves memory and mirrors Unity's typical asset-sharing
        // pattern (Mesh assets in a bundle are shared by all prefabs that reference them).
        public static Dict<string, Mesh> Meshes { get; } = new Dict<string, Mesh>();

        // The most recently constructed instance — set in the ctor. ProtoRegistrator-time code
        // (e.g. add_loose_product_material, add_unit_prefab callbacks) uses this to trigger
        // injection AT THE END of mod RegisterData, so deferred items land in LoadedAssets
        // before COI's ProductsRenderer queries for them on first scene render.
        public static CustomAssetManager Instance { get; private set; }

        public static void Clear()
        {
            Alternations.Clear();
            PendingMaterials.Clear();
            PendingPrefabs.Clear();
            PendingGameTextures.Clear();
            Meshes.Clear();
        }

        private readonly AssetsDb m_assets;
        private readonly LooseProductMaterialManager m_lpmm;
        private readonly LooseProductsSlimIdManager m_slimIdMgr;
        private readonly HashSet<int> m_patchedPileSlices = new HashSet<int>();

        // Taking LPMM + LooseProductsSlimIdManager as ctor deps forces this class to construct
        // AFTER them, so PatchPileTextureArrays can reach into LPMM's already-built
        // Texture2DArrays. The unit-prefab side has its own injector class
        // (CustomUnitPrefabHook) that runs after ProductsRenderer for the same reason.
        public CustomAssetManager(AssetsDb assets, LooseProductMaterialManager lpmm, LooseProductsSlimIdManager slimIdMgr)
        {
            m_assets = assets;
            m_lpmm = lpmm;
            m_slimIdMgr = slimIdMgr;
            Instance = this;
            Log.Info($"[CAM] ctor — pending alternations={Alternations.Count}, deferred materials={PendingMaterials.Count}, deferred prefabs={PendingPrefabs.Count}");
            RunInjection();
        }

        // Public entry point. Idempotent — items already present in LoadedAssets are skipped,
        // pile slices already patched are skipped.
        public void RunInjection()
        {
            EnsureInjected();
            PatchPileTextureArrays();
        }

        private void EnsureInjected()
        {
            Dict<string, UnityEngine.Object> loadedAssets = (Dict<string, UnityEngine.Object>)m_assets.LoadedAssets;

            int alternsAdded = 0;
            foreach (var item in Alternations)
            {
                if (loadedAssets.ContainsKey(item.Key)) continue;
                loadedAssets[item.Key] = item.Value;
                alternsAdded++;
            }

            int matsBuilt = 0;
            foreach (var plan in PendingMaterials)
            {
                if (loadedAssets.ContainsKey(plan.OutputPath)) continue; // already built on a prior pass
                try
                {
                    Material built = BuildDeferred(plan, loadedAssets);
                    if (built != null) { loadedAssets[plan.OutputPath] = built; matsBuilt++; }
                }
                catch (Exception ex)
                {
                    Log.Warning($"CustomAssetManager: failed to build material '{plan.OutputPath}': {ex.Message}");
                }
            }

            // Prefabs are built last because they reference materials produced above.
            // ProductsRenderer in COI extracts MeshFilter.sharedMesh and MeshRenderer
            // .sharedMaterial from the prefab; nothing else on the GameObject is looked at,
            // so we build the smallest valid form (one mesh, one material).
            int prefabsBuilt = 0;
            foreach (var plan in PendingPrefabs)
            {
                if (loadedAssets.ContainsKey(plan.OutputPath)) continue;
                try
                {
                    GameObject built = BuildDeferredPrefab(plan, loadedAssets);
                    if (built != null) { loadedAssets[plan.OutputPath] = built; prefabsBuilt++; }
                }
                catch (Exception ex)
                {
                    Log.Warning($"CustomAssetManager: failed to build prefab '{plan.OutputPath}': {ex.Message}");
                }
            }

            // Late-bound game textures: resolved AFTER prefabs/materials are
            // in LoadedAssets so the parent objects we mutate are guaranteed
            // present. Idempotent across re-injection passes â€” we just
            // overwrite the texture each time, no harm done.
            int gameTexturesBound = 0;
            foreach (var kvp in PendingGameTextures) {
                string parentPath = kvp.Key;
                string gameAssetPath = kvp.Value;
                try {
                    if (!loadedAssets.TryGetValue(parentPath, out UnityEngine.Object parentObj)) {
                        Log.Warning($"[CAM] PendingGameTextures: parent '{parentPath}' missing from " +
                                    "LoadedAssets, skipping game-texture bind");
                        continue;
                    }
                    Texture2D gameTex = m_assets.GetSharedTexture(gameAssetPath);
                    if (gameTex == null || gameTex == m_assets.DefaultTexture) {
                        Log.Warning($"[CAM] PendingGameTextures: game asset '{gameAssetPath}' not found " +
                                    $"in AssetsDb (parent='{parentPath}')");
                        continue;
                    }
                    if (bindMainTexture(parentObj, gameTex)) gameTexturesBound++;
                } catch (Exception ex) {
                    Log.Warning($"[CAM] PendingGameTextures: bind failed for '{parentPath}' â† '{gameAssetPath}': "
                                + ex.Message);
                }
            }

            if (alternsAdded > 0 || matsBuilt > 0 || prefabsBuilt > 0 || gameTexturesBound > 0)
                Log.Info($"[CAM] injected +{alternsAdded} assets, +{matsBuilt} materials, +{prefabsBuilt} prefabs, "
                    + $"+{gameTexturesBound} game-texture binds (totals: {Alternations.Count}/{PendingMaterials.Count}/{PendingPrefabs.Count}/{PendingGameTextures.Count}).");
        }

        // Push a Texture2D onto the parent asset's mainTexture slot. Handles
        // the two parent shapes consumers can produce:
        //   â€¢ Material â€” mainTexture is set directly.
        //   â€¢ GameObject with MeshRenderer + Material â€” the renderer's
        //     sharedMaterial gets its mainTexture set (also the _AlbedoTex
        //     property when the shader exposes it, mirroring
        //     ApplyMainTexture's albedo-aliasing behaviour).
        // Returns true when a binding was applied so the caller can count
        // it accurately. Logs a warning for unknown parent shapes.
        private static bool bindMainTexture(UnityEngine.Object parent, Texture2D tex)
        {
            if (parent is Material mat) {
                mat.mainTexture = tex;
                if (mat.HasProperty("_AlbedoTex")) {
                    mat.SetTexture("_AlbedoTex", tex);
                }
                return true;
            }
            if (parent is GameObject go) {
                var renderer = go.GetComponent<MeshRenderer>();
                if (renderer == null || renderer.sharedMaterial == null) {
                    Log.Warning($"[CAM] PendingGameTextures: parent GameObject '{parent.name}' has no "
                                + "MeshRenderer/sharedMaterial to bind onto");
                    return false;
                }
                renderer.sharedMaterial.mainTexture = tex;
                if (renderer.sharedMaterial.HasProperty("_AlbedoTex")) {
                    renderer.sharedMaterial.SetTexture("_AlbedoTex", tex);
                }
                return true;
            }
            Log.Warning($"[CAM] PendingGameTextures: parent type '{parent.GetType().Name}' is not a "
                        + "Material or GameObject; don't know how to bind a texture onto it");
            return false;
        }

        // Construct a single-GameObject prefab carrying the given mesh and the resolved
        // material from loadedAssets[plan.MaterialPath]. SetActive(false) + HideAndDontSave
        // so the GameObject behaves as an asset (won't run scene logic, won't be destroyed
        // on scene unload, won't appear in the hierarchy).
        private static GameObject BuildDeferredPrefab(DeferredPrefab plan, Dict<string, UnityEngine.Object> loadedAssets)
        {
            Material mat = null;
            if (loadedAssets.TryGetValue(plan.MaterialPath, out UnityEngine.Object matObj) && matObj is Material m)
            {
                mat = m;
            }
            else
            {
                Log.Warning($"CustomAssetManager: material '{plan.MaterialPath}' for prefab '{plan.OutputPath}' " +
                            "not found in LoadedAssets; using Standard fallback.");
                mat = new Material(Shader.Find("Standard"));
            }

            GameObject go = new GameObject(plan.OutputPath);
            go.SetActive(false);
            go.hideFlags = HideFlags.HideAndDontSave;
            go.AddComponent<MeshFilter>().sharedMesh = plan.Mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }

        // --- After injection, retroactively patch LPMM's Texture2DArrays for any LooseProductProto
        //     whose PileMaterialAssetPath now resolves to one of our materials. The materials we
        //     built (cloning vanilla pile mats, then overriding the albedo) have the correct
        //     _AlbedoTex / _NormalsTex / _SmoothMetalTex properties — we copy each into the right
        //     slice of LPMM.AlbedoTexArray / NormalsTexArray / SmoothMetalTexArray.
        //     LPMM bakes textures at its own ctor; we can't run before it without a Harmony patch,
        //     so this slice rewrite is the next-best fix and clears the visual regression even
        //     though the original "Asset ... not found in any bundle" error remains in the log.
        private void PatchPileTextureArrays()
        {
            var albedoArr = m_lpmm.AlbedoTexArray;
            var normalsArr = m_lpmm.NormalsTexArray;
            var smoothArr = m_lpmm.SmoothMetalTexArray;
            if (albedoArr == null)
            {
                Log.Warning("[CAM] LPMM.AlbedoTexArray is null; pile-texture patching skipped.");
                return;
            }
            Log.Info($"[CAM] pile arrays: {albedoArr.width}x{albedoArr.height} depth={albedoArr.depth} albedo={albedoArr.format} normals={normalsArr?.format} smooth={smoothArr?.format}");

            var loadedAssets = (Dict<string, UnityEngine.Object>)m_assets.LoadedAssets;
            int albedoId = Shader.PropertyToID("_AlbedoTex");
            int normalsId = Shader.PropertyToID("_NormalsTex");
            int smoothId = Shader.PropertyToID("_SmoothMetalTex");

            // Index ManagedProtos by PileMaterialAssetPath for O(1) lookup.
            var protoByPath = new Dictionary<string, LooseProductProto>();
            foreach (var proto in m_slimIdMgr.ManagedProtos)
            {
                if (proto == null || proto.IsPhantom) continue;
                var path = proto.Graphics?.PileMaterialAssetPath;
                if (!string.IsNullOrEmpty(path)) protoByPath[path] = proto;
            }

            int patched = 0;
            foreach (var plan in PendingMaterials)
            {
                if (!protoByPath.TryGetValue(plan.OutputPath, out var proto))
                {
                    // Material was registered but no LooseProductProto uses that path — could be
                    // an unrelated (non-loose) material; just skip the pile patch.
                    continue;
                }
                if (!loadedAssets.TryGetValue(plan.OutputPath, out var matObj) || !(matObj is Material mat))
                {
                    Log.Warning($"[CAM] material '{plan.OutputPath}' missing from LoadedAssets after injection.");
                    continue;
                }

                int slice = proto.LooseSlimId.Value - 1; // LPMM does ManagedProtos.RemoveAt(0)
                if (slice < 0 || slice >= albedoArr.depth)
                {
                    Log.Warning($"[CAM] product '{proto.Id.Value}' slice {slice} out of range (depth={albedoArr.depth}).");
                    continue;
                }
                if (m_patchedPileSlices.Contains(slice)) continue; // idempotent across re-injection passes

                Log.Info($"[CAM] retroactive pile fix → product '{proto.Id.Value}' slice {slice} (tiling={plan.Tiling}x)");
                CopySlot(mat, albedoId, "_AlbedoTex", albedoArr, slice, plan.Tiling);
                if (normalsArr != null) CopySlot(mat, normalsId, "_NormalsTex", normalsArr, slice, plan.Tiling);
                if (smoothArr != null) CopySlot(mat, smoothId, "_SmoothMetalTex", smoothArr, slice, plan.Tiling);
                m_patchedPileSlices.Add(slice);
                patched++;
            }

            if (patched > 0)
                Log.Info($"[CAM] FIXED: rewrote {patched} pile texture slice(s). Earlier 'Asset ... not found in any bundle' from LPMM ctor was retroactively resolved.");
        }

        // Copy a Material's texture property into a Texture2DArray slice. CRITICAL:
        //   - dst's format is likely compressed (DXT5/BC7/etc) because that's how COI ships
        //     its pile albedos, and TextureArrayUtils inherits the format from the first
        //     vanilla texture.
        //   - ReadPixels and Apply(updateMipmaps: true) only work on uncompressed formats.
        //     Earlier code did ReadPixels into a compressed temp Texture2D, leaving the mip
        //     chain as undefined memory — at distance this produced yellow-striped garbage as
        //     Unity sampled the corrupt blocks. That's the flicker we kept seeing.
        //   - Fix: ALWAYS build the temp in RGBA32, fill its mip chain, then convert to dst's
        //     format. For DXT-family destinations Texture2D.Compress() works at runtime; for
        //     BCn/BC7 there is no runtime encoder in Unity, so we copy mip 0 and downsampled
        //     RGBA mips into the slice via per-mip blits — at least every mip is consistent
        //     so the pile no longer flickers (it may look slightly soft at distance).
        private static void CopySlot(Material mat, int propId, string propName, Texture2DArray dst, int slice, float tilingMult = 1f)
        {
            if (!mat.HasProperty(propId))
            {
                Log.Warning($"[CAM]   '{propName}' missing on material '{mat.name}'.");
                return;
            }
            var src = mat.GetTexture(propId) as Texture2D;
            if (src == null)
            {
                Log.Warning($"[CAM]   '{propName}' on material '{mat.name}' is null or not a Texture2D.");
                return;
            }

            int dstMips = dst.mipmapCount;
            Log.Info($"[CAM]     {propName}: src={src.width}x{src.height} {src.format} mips={src.mipmapCount} → dst slice {slice} of {dst.width}x{dst.height} {dst.format} mips={dstMips}");

            // Fast path: identical size/format/mips → direct full-chain copy.
            if (src.width == dst.width && src.height == dst.height && src.format == dst.format && src.mipmapCount == dstMips)
            {
                Graphics.CopyTexture(src, 0, dst, slice);
                Log.Info($"[CAM]       direct copy");
                return;
            }

            // Always build the uncompressed mip chain via RGB24/RGBA32 — ReadPixels & Apply(true)
            // only work on uncompressed formats. The presence of an alpha channel decides the
            // outcome of Texture2D.Compress(highQuality:true): RGBA32 → DXT5, RGB24 → DXT1.
            // Match the destination so the compress lands the right format. Mismatch (e.g. RGBA32
            // → DXT5 going into a DXT1 slice) makes Graphics.CopyTexture fail at the per-mip step
            // because DXT1 and DXT5 have different block byte sizes (8 vs 16); the slice then
            // keeps its prior data and the pile renders a fallback look.
            TextureFormat tempFmt = dst.format == TextureFormat.DXT1
                ? TextureFormat.RGB24
                : TextureFormat.RGBA32;
            var rt = RenderTexture.GetTemporary(dst.width, dst.height, 0, RenderTextureFormat.ARGB32);
            Texture2D rgba = null;
            try
            {
                BlitTiled(src, rt, tilingMult);
                rgba = new Texture2D(dst.width, dst.height, tempFmt, mipChain: dstMips > 1);
                var prev = RenderTexture.active;
                RenderTexture.active = rt;
                rgba.ReadPixels(new Rect(0, 0, dst.width, dst.height), 0, 0);
                rgba.Apply(updateMipmaps: dstMips > 1, makeNoLongerReadable: false);
                RenderTexture.active = prev;
            }
            catch (Exception ex)
            {
                Log.Warning($"[CAM]       RGBA32 build failed: {ex.Message}");
                if (rgba != null) UnityEngine.Object.Destroy(rgba);
                RenderTexture.ReleaseTemporary(rt);
                return;
            }
            RenderTexture.ReleaseTemporary(rt);

            try
            {
                // Case 1: dst is uncompressed → direct mip-chain copy works.
                if (dst.format == TextureFormat.RGBA32 || dst.format == TextureFormat.ARGB32 || dst.format == TextureFormat.RGB24)
                {
                    if (rgba.format != dst.format)
                    {
                        // Format aliasing for RGBA32 vs ARGB32 differs in channel order; build
                        // a temp in dst.format and use Graphics.ConvertTexture which honours that.
                        var converted = new Texture2D(dst.width, dst.height, dst.format, mipChain: dstMips > 1);
                        Graphics.ConvertTexture(rgba, converted);
                        if (converted.mipmapCount == dstMips)
                            Graphics.CopyTexture(converted, 0, dst, slice);
                        else
                            CopyMipsBest(converted, dst, slice);
                        UnityEngine.Object.Destroy(converted);
                    }
                    else if (rgba.mipmapCount == dstMips)
                    {
                        Graphics.CopyTexture(rgba, 0, dst, slice);
                    }
                    else
                    {
                        CopyMipsBest(rgba, dst, slice);
                    }
                    Log.Info($"[CAM]       resampled to uncompressed {dst.format}");
                    return;
                }

                // Case 2: dst is DXT1/DXT5 → Texture2D.Compress() handles these at runtime.
                if (dst.format == TextureFormat.DXT1 || dst.format == TextureFormat.DXT5)
                {
                    rgba.Compress(highQuality: true);
                    if (rgba.format == dst.format && rgba.mipmapCount == dstMips)
                    {
                        Graphics.CopyTexture(rgba, 0, dst, slice);
                        Log.Info($"[CAM]       compressed to {rgba.format}, full-chain copy");
                    }
                    else
                    {
                        CopyMipsBest(rgba, dst, slice);
                        Log.Warning($"[CAM]       compress mismatch (got {rgba.format} mips={rgba.mipmapCount}); copied what fit");
                    }
                    return;
                }

                // Case 3: dst is BCn/BC6/BC7 etc — no runtime encoder available in Unity. Fall
                // back to per-mip blits into a per-mip RGBA32 Texture2D and copy bytes-for-bytes;
                // the format mismatch means the slice will end up uncompressed-interpreted-as-
                // BCn (still wrong) for some formats. Best we can do without a third-party
                // encoder is at least make every mip consistent so the pile doesn't flicker.
                Log.Warning($"[CAM]       dst format '{dst.format}' has no runtime encoder; per-mip ARGB32 blit fallback. Pile may look soft at distance but should not flicker.");
                CopyMipsViaPerMipBlit(src, dst, slice, tilingMult);
            }
            catch (Exception ex)
            {
                Log.Warning($"[CAM]       conversion FAILED: {ex.Message}");
            }
            finally
            {
                if (rgba != null) UnityEngine.Object.Destroy(rgba);
            }
        }

        // Copy as many mip levels as the source has into dst's slice. Used when mip counts
        // differ slightly (some compressed formats can't hold a full mip chain when generated
        // from RGBA32 source).
        private static void CopyMipsBest(Texture2D src, Texture2DArray dst, int slice)
        {
            int copyMips = Math.Min(src.mipmapCount, dst.mipmapCount);
            for (int mip = 0; mip < copyMips; mip++)
                Graphics.CopyTexture(src, 0, mip, dst, slice, mip);
        }

        // Per-mip path: blit the source down to each mip's resolution via RenderTexture, copy
        // mip 0 into a same-size dst-format Texture2D (which will only succeed for formats the
        // engine can encode at runtime), then CopyTexture into the array slice. Works as a
        // last-resort consistency fix even when the format isn't writable from RGBA — at least
        // every mip ends up with the SAME data interpretation, so flicker is gone.
        private static void CopyMipsViaPerMipBlit(Texture2D src, Texture2DArray dst, int slice, float tilingMult = 1f)
        {
            for (int mip = 0; mip < dst.mipmapCount; mip++)
            {
                int w = Math.Max(1, dst.width >> mip);
                int h = Math.Max(1, dst.height >> mip);
                var rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32);
                BlitTiled(src, rt, tilingMult);
                var tmp = new Texture2D(w, h, dst.format, mipChain: false);
                try
                {
                    var prev = RenderTexture.active;
                    RenderTexture.active = rt;
                    tmp.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                    tmp.Apply(updateMipmaps: false, makeNoLongerReadable: false);
                    RenderTexture.active = prev;
                    Graphics.CopyTexture(tmp, 0, 0, dst, slice, mip);
                }
                finally
                {
                    UnityEngine.Object.Destroy(tmp);
                    RenderTexture.ReleaseTemporary(rt);
                }
            }
        }

        // Blit src into rt with UV tiling. Two contributions to the final tile factor:
        //   1. Auto-upscale: if rt is larger than src, repeat src enough times to fill rt at
        //      native resolution (rather than bilinearly stretching it, which destroys
        //      high-frequency detail like rocks).
        //   2. tilingMult: explicit user-requested repetition (the `tiling` arg on
        //      add_loose_product_material). 4 = 4×4 repetitions on top of auto-upscale.
        // Source's wrapMode is forced to Repeat for the blit and restored afterwards.
        private static void BlitTiled(Texture2D src, RenderTexture rt, float tilingMult = 1f)
        {
            float autoX = rt.width  > src.width  ? (float)rt.width  / src.width  : 1f;
            float autoY = rt.height > src.height ? (float)rt.height / src.height : 1f;
            float tileX = autoX * tilingMult;
            float tileY = autoY * tilingMult;

            if (tileX == 1f && tileY == 1f)
            {
                Graphics.Blit(src, rt);
                return;
            }

            var prevWrap = src.wrapMode;
            src.wrapMode = TextureWrapMode.Repeat;
            try
            {
                Graphics.Blit(src, rt, new Vector2(tileX, tileY), Vector2.zero);
            }
            finally
            {
                src.wrapMode = prevWrap;
            }
        }

        private static Material BuildDeferred(DeferredMaterial plan, Dict<string, UnityEngine.Object> loadedAssets)
        {
            Material baseMaterial = null;
            if (!string.IsNullOrEmpty(plan.ReferencePath))
            {
                if (loadedAssets.TryGetValue(plan.ReferencePath, out UnityEngine.Object refObj) && refObj is Material refMat)
                    baseMaterial = refMat;
                else
                    Log.Warning($"CustomAssetManager: reference material '{plan.ReferencePath}' for '{plan.OutputPath}' " +
                                $"not found in AssetsDb; falling back to shader-only construction.");
            }

            Material mat;
            if (baseMaterial != null)
                mat = new Material(baseMaterial);
            else if (!string.IsNullOrEmpty(plan.ShaderName))
                mat = new Material(Shader.Find(plan.ShaderName) ?? Shader.Find("Standard"));
            else
                mat = new Material(Shader.Find("Standard"));

            // Apply albedo: prefer AlbedoArray[0] (loose-product API), fall back to legacy NewTexture.
            Texture2D albedo = (plan.AlbedoArray != null && plan.AlbedoArray.Count > 0)
                ? plan.AlbedoArray[0]
                : plan.NewTexture;
            if (albedo != null)
                ApplyMainTexture(mat, albedo);

            // Override _NormalsTex / _SmoothMetalTex if user supplied them. Otherwise keep the
            // values cloned from the reference material — that's the "copied from reference"
            // path the user asked for.
            if (plan.NormalsArray != null && plan.NormalsArray.Count > 0 && plan.NormalsArray[0] != null)
            {
                int id = Shader.PropertyToID("_NormalsTex");
                if (mat.HasProperty(id))
                {
                    mat.SetTexture(id, plan.NormalsArray[0]);
                    Log.Info($"  + set shader property '_NormalsTex' (override)");
                }
            }
            if (plan.SmoothMetalArray != null && plan.SmoothMetalArray.Count > 0 && plan.SmoothMetalArray[0] != null)
            {
                int id = Shader.PropertyToID("_SmoothMetalTex");
                if (mat.HasProperty(id))
                {
                    mat.SetTexture(id, plan.SmoothMetalArray[0]);
                    Log.Info($"  + set shader property '_SmoothMetalTex' (override)");
                }
            }

            mat.name = plan.OutputPath;
            loadedAssets[plan.OutputPath] = mat;
            return mat;
        }

        private static void ApplyMainTexture(Material mat, Texture2D tex)
        {
            Log.Info($"CustomAssetManager: applying texture '{tex.name}' ({tex.width}x{tex.height}) to material '{mat.name}' " +
                     $"(shader '{mat.shader?.name}')");
            mat.mainTexture = tex;
            int hits = 0;
            foreach (var name in s_albedoPropertyCandidates)
            {
                int id = Shader.PropertyToID(name);
                if (mat.HasProperty(id))
                {
                    mat.SetTexture(id, tex);
                    hits++;
                    Log.Info($"  + set shader property '{name}'");
                }
            }
            if (hits == 0)
                Log.Warning($"  ! no albedo-candidate property matched. Texture only set via mainTexture. " +
                            $"Shader exposes texture properties: {string.Join(", ", mat.GetTexturePropertyNames())}");
        }

        private static readonly string[] s_albedoPropertyCandidates =
        {
            "_AlbedoTex",
            "_BaseMap",
            "_MainTex",
        };
    }
}
