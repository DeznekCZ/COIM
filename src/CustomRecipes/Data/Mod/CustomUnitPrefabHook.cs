using System;
using System.Linq;
using System.Reflection;
using Mafi;
using Mafi.Core;
using Mafi.Core.Products;
using Mafi.Unity;
using Mafi.Unity.InstancedRendering;
using Mafi.Unity.Terrain;
using UnityEngine;

namespace CustomAssets.Data.Mod
{
    /// Late-injection hook for unit-product prefabs.
    ///
    /// Why this exists: ProductsRenderer.ctor takes LooseProductMaterialManager as a dep, so
    /// DI typically constructs it BEFORE CustomAssetManager. During its ctor it queries
    /// AssetsDb.LoadedAssets[<your-prefab-path>] for every CountableProductProto and caches
    /// the missing-prefab fallback (a 0.5×0.5×0.5 grey AaBox) when the path resolves to null.
    /// Any later injection of the prefab into LoadedAssets is invisible — the renderer has
    /// already memorised the fallback in m_staticDrawData / m_dynamicDrawData /
    /// m_dynamicQuaternionDrawData.
    ///
    /// We can't beat the renderer's construction order (it has no dep on us, and IInitializer
    /// fires after LPMM/PR ctors anyway). So we run AFTER it: take ProductsRenderer +
    /// CustomAssetManager as ctor deps, force CAM injection (idempotent), then rebuild the
    /// renderer's per-product CommonDataMutable for each of our DeferredPrefab plans by
    /// reflecting into the private initializeDrawData method and overwriting the three cache
    /// arrays.
    ///
    /// Reflection targets (decompiled COI 0.8.1b — names are not obfuscated):
    ///   private CommonDataMutable initializeDrawData(ProductProto, bool, bool, LooseProductMaterialManager)
    ///   private readonly ImmutableArray<int>     m_productToDataIndex
    ///   private readonly DrawDataMutableStatic[]   m_staticDrawData          (.Common is the cache slot)
    ///   private readonly DrawDataMutableDynamic[]  m_dynamicDrawData         (same)
    ///   private readonly DrawQuatDataMutableDynamic[] m_dynamicQuaternionDrawData (same)
    [GlobalDependency(RegistrationMode.AsSelf, false, false)]
    public class CustomUnitPrefabHook
    {
        public CustomUnitPrefabHook(
            CustomAssetManager cam,
            ProductsRenderer renderer,
            AssetsDb assets,
            ProductsSlimIdManager productsSlimIdManager,
            LooseProductMaterialManager looseProductMaterialManager)
        {
            // CAM injection is idempotent; this guarantees prefabs+materials are in LoadedAssets
            // by the time we patch the renderer's cache below.
            cam.RunInjection();

            int patched = 0;
            foreach (var plan in CustomAssetManager.PendingPrefabs)
            {
                try
                {
                    if (PatchRendererForPrefab(renderer, productsSlimIdManager, looseProductMaterialManager, plan))
                        patched++;
                }
                catch (Exception ex)
                {
                    Log.Warning($"[CustomUnitPrefabHook] failed to patch '{plan.OutputPath}': {ex.Message}");
                }
            }
            if (patched > 0)
                Log.Info($"[CustomUnitPrefabHook] FIXED: rebuilt ProductsRenderer cache for {patched} prefab(s). " +
                         "Earlier 'Asset ... not found in any bundle' from ProductsRenderer ctor was retroactively resolved.");
        }

        // Locate the ProductProto whose PileMaterialAssetPath==... no, PrefabsPath matches our
        // plan, find its slim-id position, look up the data index, build a fresh
        // CommonDataMutable via the private initializeDrawData, then overwrite the cached
        // entry in all three draw-data arrays.
        private static bool PatchRendererForPrefab(
            ProductsRenderer renderer,
            ProductsSlimIdManager productsSlimIdManager,
            LooseProductMaterialManager looseProductMaterialManager,
            DeferredPrefab plan)
        {
            int slimIdx = -1;
            ProductProto target = null;
            for (int i = 0; i < productsSlimIdManager.ManagedProtos.Length; i++)
            {
                var proto = productsSlimIdManager.ManagedProtos[i];
                if (proto?.Graphics?.PrefabsPath.IsNone ?? true) continue;
                if (proto.Graphics.PrefabsPath.Value != plan.OutputPath) continue;
                slimIdx = i;
                target = proto;
                break;
            }
            if (target == null)
            {
                // Prefab registered but no ProductProto uses it — nothing to patch.
                return false;
            }

            var rendererType = typeof(ProductsRenderer);
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            var idxField = rendererType.GetField("m_productToDataIndex", Flags);
            if (idxField == null) { Log.Warning("[CustomUnitPrefabHook] m_productToDataIndex field missing"); return false; }
            var idxArrayObj = idxField.GetValue(renderer);
            // m_productToDataIndex is an ImmutableArray<int>. Use its indexer via reflection.
            var idxIndexer = idxArrayObj.GetType().GetProperty("Item", new[] { typeof(int) });
            int dataIdx = (int)idxIndexer.GetValue(idxArrayObj, new object[] { slimIdx });
            if (dataIdx < 0)
            {
                // Product had no PrefabsPath at construction time — ProductsRenderer skipped it.
                return false;
            }

            // The shipping COI DLL has initializeDrawData(ProductProto, bool, bool) — 3 params,
            // LPMM is read from a private field inside the method. (Decompiled source from a
            // dev build showed a 4-arg variant; the release build uses the field.)
            MethodInfo initMethod = null;
            foreach (var m in rendererType.GetMethods(Flags))
            {
                if (m.Name != "initializeDrawData") continue;
                var ps = m.GetParameters();
                if (ps.Length != 3) continue;
                if (ps[0].ParameterType != typeof(ProductProto)) continue;
                if (ps[1].ParameterType != typeof(bool)) continue;
                if (ps[2].ParameterType != typeof(bool)) continue;
                initMethod = m;
                break;
            }
            if (initMethod == null)
            {
                var found = string.Join(", ", rendererType.GetMethods(Flags)
                    .Where(m => m.Name.IndexOf("init", StringComparison.OrdinalIgnoreCase) >= 0
                             || m.Name.IndexOf("Draw", StringComparison.OrdinalIgnoreCase) >= 0)
                    .Select(m => m.Name + "(" + string.Join(",", m.GetParameters().Select(p => p.ParameterType.Name)) + ")"));
                Log.Warning($"[CustomUnitPrefabHook] initializeDrawData(ProductProto, bool, bool) not found. " +
                            $"Candidates with 'init' or 'Draw' in name: {found}");
                return false;
            }

            // Build fresh CommonDataMutable for the three render variants (static / dynamic /
            // dynamic-quaternion) and overwrite each slot's .Common. The old Common values'
            // Material/DataBuffer are leaked rather than disposed — disposing would tear down
            // the fallback's mesh, which Unity then complains about elsewhere. Negligible
            // memory cost for a one-shot fix.
            object newCommonStatic = initMethod.Invoke(renderer, new object[] { target, false, false });
            object newCommonDynamic = initMethod.Invoke(renderer, new object[] { target, true, false });
            object newCommonDynamicQuat = initMethod.Invoke(renderer, new object[] { target, true, true });

            ReplaceCommonInStructArray(rendererType, renderer, "m_staticDrawData", dataIdx, newCommonStatic);
            ReplaceCommonInStructArray(rendererType, renderer, "m_dynamicDrawData", dataIdx, newCommonDynamic);
            ReplaceCommonInStructArray(rendererType, renderer, "m_dynamicQuaternionDrawData", dataIdx, newCommonDynamicQuat);

            // ProductsRenderer.StackOffsetsPacked was computed in its ctor using the fallback
            // mesh's (0.5×0.5×0.5) bounds, so for any size-sensitive packing — Auto, Triangle,
            // Row, etc. — the per-product layout entries are wrong (typically: single item per
            // tile instead of the triangle/row layout the actual mesh size would imply).
            // Recompute the entry for this product using the now-correct CommonDataMutable.
            RecomputeStackOffsets(rendererType, renderer, slimIdx, target, newCommonStatic);

            // Diagnostic: log the actual mesh bounds (in meters — matches Unity's unit
            // convention) so it's easy to verify what size the renderer will treat the item
            // as. Triangle packing wants sizeX < 0.5m AND sizeZ < 0.5m to be auto-selected.
            var b = plan.Mesh != null ? plan.Mesh.bounds : default;
            Log.Info($"[CustomUnitPrefabHook] product '{target.Id.Value}' slot {dataIdx} " +
                     $"rebuilt with prefab '{plan.OutputPath}' " +
                     $"(mesh bounds: size {b.size.x:F3}×{b.size.y:F3}×{b.size.z:F3} m, " +
                     $"center {b.center.x:F3},{b.center.y:F3},{b.center.z:F3} m).");
            return true;
        }

        // Replace renderer.StackOffsetsPacked[slimIdx] with the result of the (private)
        // getPackedOffsets(commonData, productProto). The field is `public readonly
        // ImmutableArray<ImmutableArray<ushort>>`; we use ImmutableArray.SetItem to produce a
        // new outer array and reflect-write the field.
        private static void RecomputeStackOffsets(Type rendererType, ProductsRenderer renderer, int slimIdx, ProductProto product, object newCommon)
        {
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

            // Find static private getPackedOffsets(CommonDataMutable, ProductProto) → ImmutableArray<ushort>.
            MethodInfo getPackedOffsets = null;
            foreach (var m in rendererType.GetMethods(Flags))
            {
                if (m.Name != "getPackedOffsets") continue;
                var ps = m.GetParameters();
                if (ps.Length != 2) continue;
                if (ps[1].ParameterType != typeof(ProductProto)) continue;
                getPackedOffsets = m;
                break;
            }
            if (getPackedOffsets == null) { Log.Warning("[CustomUnitPrefabHook] getPackedOffsets not found"); return; }

            object newOffsets;
            try
            {
                newOffsets = getPackedOffsets.Invoke(null, new object[] { newCommon, product });
            }
            catch (Exception ex)
            {
                Log.Warning($"[CustomUnitPrefabHook] getPackedOffsets threw: {ex.InnerException?.Message ?? ex.Message}");
                return;
            }

            var soField = rendererType.GetField("StackOffsetsPacked", Flags);
            if (soField == null) { Log.Warning("[CustomUnitPrefabHook] StackOffsetsPacked field missing"); return; }

            object outerArray = soField.GetValue(renderer);                       // ImmutableArray<ImmutableArray<ushort>>
            var setItemMethod = outerArray.GetType().GetMethod("SetItem");        // returns a new ImmutableArray
            if (setItemMethod == null) { Log.Warning("[CustomUnitPrefabHook] ImmutableArray.SetItem missing"); return; }

            object updatedOuter = setItemMethod.Invoke(outerArray, new object[] { slimIdx, newOffsets });
            soField.SetValue(renderer, updatedOuter);
        }

        // Struct-array element fields can't be written directly via reflection on the field
        // because GetValue returns a copy of the struct. We must read the struct out, mutate
        // its `Common`, and write it back — but `.Common` is itself a struct field on a struct
        // value, so the same boxing rules apply. We do it through SetValueDirect on a TypedReference.
        private static void ReplaceCommonInStructArray(Type rendererType, ProductsRenderer renderer, string fieldName, int idx, object newCommon)
        {
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var arrField = rendererType.GetField(fieldName, Flags);
            if (arrField == null) { Log.Warning($"[CustomUnitPrefabHook] field '{fieldName}' missing"); return; }
            var array = (Array)arrField.GetValue(renderer);
            if (idx < 0 || idx >= array.Length) return;
            object slot = array.GetValue(idx);                      // box the struct
            var commonField = slot.GetType().GetField("Common", Flags);
            if (commonField == null) { Log.Warning($"[CustomUnitPrefabHook] '.Common' missing on {slot.GetType().Name}"); return; }
            commonField.SetValue(slot, newCommon);                  // mutate the boxed copy
            array.SetValue(slot, idx);                              // write back
        }
    }
}
