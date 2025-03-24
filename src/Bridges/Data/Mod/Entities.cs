using Mafi;
using Mafi.Base;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Buildings.Ramps;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Gfx;
using Mafi.Core.Mods;
using Mafi.Core.Ports.Io;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.Terrain;
using System;

namespace ProgramableNetwork
{
    public partial class NewIds
    {
        public partial class Bridges
        {
            internal static StaticEntityProto.ID Floor_1x1_H(int h) => new StaticEntityProto.ID($"Bridges_Floor_1x1_H{h}");

            internal static StaticEntityProto.ID Floor_1x1_P(int h) => new StaticEntityProto.ID($"Bridges_Floor_1x1_P{h}");

            internal static StaticEntityProto.ID Floor_1x1_HO => new StaticEntityProto.ID($"Bridges_Floor_1x1_HO");

            internal static StaticEntityProto.ID Floor_1x1_PO => new StaticEntityProto.ID($"Bridges_Floor_1x1_PO");

            internal static StaticEntityProto.ID Ramp_1x3_R(int h)
            {
                return new StaticEntityProto.ID($"Bridges_Ramp_1x3_R{h}");
            }
        }
    }

    public partial class NewAssets
    {
        public partial class Bridges
        {
            public partial class Icons
            {
                public static string Floor_1x1_H(int h) => "Assets/Bridges/Icons/Floor_1x1.png";
                public static string Floor_1x1_P(int h) => "Assets/Bridges/Icons/Floor_1x1.png";
                public static string Floor_1x1_HO => "Assets/Bridges/Icons/Floor_1x1.png";
                public static string Floor_1x1_PO => "Assets/Bridges/Icons/Floor_1x1.png";
                public static string Ramp_1x3_R(int h) => "Assets/Bridges/Icons/Floor_1x1.png";
            }

            public static string Floor_1x1_H(int h) => $"Assets/Bridges/Prefabs/Floor_1x1_H{h}.prefab";
            public static string Floor_1x1_P(int h) => $"Assets/Bridges/Prefabs/Floor_1x1_H{h}.prefab";
            public static string Floor_1x1_HO => $"Assets/Bridges/Prefabs/Floor_1x1_H1.prefab";
            public static string Floor_1x1_PO => $"Assets/Bridges/Prefabs/Floor_1x1_H1.prefab";
            public static string Ramp_1x3_R(int h) => $"Assets/Bridges/Prefabs/Ramp_1x3_R{h}.prefab";
        }
    }

    internal class Entities : AValidatedData
    {
        protected override void RegisterDataInternal(ProtoRegistrator registrator)
        {

            EntityLayoutParams layoutParams = new EntityLayoutParams(null, new CustomLayoutToken[]
            {
                new CustomLayoutToken("_0_", (EntityLayoutParams p, int h) =>
                {
                    int heightFrom2_ = h - 1;
                    Fix32? vehicleHeight2_ = h - 1;
                    return new LayoutTokenSpec(heightFrom2_, h, LayoutTileConstraint.None, null, minTerrainHeight:0, maxTerrainHeight:0, vehicleHeight2_, null, null);
                }),
                new CustomLayoutToken("_0=", (EntityLayoutParams p, int h) =>
                {
                    return new LayoutTokenSpec(h-2, h-1, LayoutTileConstraint.None, null, null, maxTerrainHeight:h, null, null, null, isRamp: true);
                }),
            }, portsCanOnlyConnectToTransports: false, null, null, null, null, null);

            var category = registrator.PrototypesDb.Get<ToolbarCategoryProto>(Ids.ToolbarCategories.BuildingsForVehicles).ToImmutableArray();

            Log.Info("Floor loading");
            for (int h = 1; h <= 4; h++)
            {
                registrator.PrototypesDb.Add(new VehicleRampProto(
                    id: NewIds.Bridges.Floor_1x1_H(h),
                    strings: Proto.CreateStr(NewIds.Bridges.Floor_1x1_H(h), $"Floor (height: {h * 2}m)", "Base floor"),
                    layout: registrator.LayoutParser.ParseLayoutOrThrow(layoutParams, $"_{h + 1}_"),
                    costs: ((EntityCostsTpl)Costs.Build.Concrete(1).Steel(1)).MapToEntityCosts(registrator),
                    graphics: new LayoutEntityProto.Gfx(
                        prefabPath: NewAssets.Bridges.Floor_1x1_H(h),
                        customIconPath: NewAssets.Bridges.Icons.Floor_1x1_H(h),
                        categories: category
                    )
                ));

                registrator.PrototypesDb.Add(new VehicleRampProto(
                    id: NewIds.Bridges.Floor_1x1_P(h),
                    strings: Proto.CreateStr(NewIds.Bridges.Floor_1x1_P(h), $"Pillar (height: {h * 2}m)", "Supported floor"),
                    layout: registrator.LayoutParser.ParseLayoutOrThrow(layoutParams, $"_{h + 1}_"),
                    costs: ((EntityCostsTpl)Costs.Build.Concrete(h).Steel(h)).MapToEntityCosts(registrator),
                    graphics: new LayoutEntityProto.Gfx(
                        prefabPath: NewAssets.Bridges.Floor_1x1_P(h),
                        customIconPath: NewAssets.Bridges.Icons.Floor_1x1_P(h),
                        categories: category
                    )
                ));

                registrator.PrototypesDb.Add(new VehicleRampProto(
                    id: NewIds.Bridges.Ramp_1x3_R(h),
                    strings: Proto.CreateStr(NewIds.Bridges.Ramp_1x3_R(h), $"Ramp (height: {(h-1)*2}m->{h*2}m)", "Ramp floor"),
                    layout: registrator.LayoutParser.ParseLayoutOrThrow(layoutParams, $"_{h}__{h+1}=_{h+1}=_{h+1}=_{h+1}_"),
                    costs: ((EntityCostsTpl)Costs.Build.Concrete(1).Steel(1)).MapToEntityCosts(registrator),
                    graphics: new LayoutEntityProto.Gfx(
                        prefabPath: NewAssets.Bridges.Ramp_1x3_R(h),
                        customIconPath: NewAssets.Bridges.Icons.Ramp_1x3_R(h),
                        categories: category
                    )
                ));
            }

            //registrator.PrototypesDb.Add(new VehicleRampProto(
            //    id: NewIds.Bridges.Floor_1x1_HO,
            //    strings: Proto.CreateStr(NewIds.Bridges.Floor_1x1_HO, $"Floor (ocean)", "Base ocean floor"),
            //    layout: registrator.LayoutParser.ParseLayoutOrThrow(layoutParams, $"_1*"),
            //    costs: ((EntityCostsTpl)Costs.Build.Concrete(1).Steel(1)).MapToEntityCosts(registrator),
            //    graphics: new LayoutEntityProto.Gfx(
            //        prefabPath: NewAssets.Bridges.Floor_1x1_HO,
            //        customIconPath: NewAssets.Bridges.Icons.Floor_1x1_HO,
            //        categories: category
            //    )
            //));

            //registrator.PrototypesDb.Add(new VehicleRampProto(
            //    id: NewIds.Bridges.Floor_1x1_PO,
            //    strings: Proto.CreateStr(NewIds.Bridges.Floor_1x1_PO, $"Pillar (ocean)", "Supported ocean floor"),
            //    layout: registrator.LayoutParser.ParseLayoutOrThrow(layoutParams, $"_1*"),
            //    costs: ((EntityCostsTpl)Costs.Build.Concrete(5).Steel(5)).MapToEntityCosts(registrator),
            //    graphics: new LayoutEntityProto.Gfx(
            //        prefabPath: NewAssets.Bridges.Floor_1x1_PO,
            //        customIconPath: NewAssets.Bridges.Icons.Floor_1x1_PO,
            //        categories: category
            //    )
            //));

            Log.Info("Layouts parsed");
        }
    }
}
