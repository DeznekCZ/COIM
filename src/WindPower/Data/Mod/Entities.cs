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
using ProgramableNetwork;
using System;
using WindPower.Entity;

namespace WindPower
{
    public partial class NewIds
    {
        public partial class WindPower
        {
            public static StaticEntityProto.ID WindTurbine_T1 => new StaticEntityProto.ID($"WindTurbine_T1");
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
            // Wind turbine protos
            registrator.PrototypesDb.Add(new WindTurbineProto(
                id: NewIds.WindPower.WindTurbine_T1,
                strings: Proto.CreateStr(NewIds.WindPower.WindTurbine_T1, "Wind turbine", "Basic wind turbine with manual braking"),
                layout: new EntityLayoutParser(registrator.PrototypesDb)
                    .ParseLayoutOrThrow("[8][8][8]", "[8][8][8]", "[8][8][8]"),
                costs: ((EntityCostsTpl)new EntityCostsTpl.Builder().CP2(5).MaintenanceT1(2).Product(9, Ids.Products.ConcreteSlab)).MapToEntityCosts(registrator),
                graphics: new LayoutEntityProto.Gfx(
                    prefabPath: "Assets/WindPower/WindTurbine_T1.prefab",
                    customIconPath: "Assets/WindPower/WindTurbine_T1_Icon.png",
                    categories: registrator.PrototypesDb.GetOrThrow<ToolbarCategoryProto>(Ids.ToolbarCategories.MachinesElectricity).SomeOption().ToImmutableArray()),
                generatedPower: 1200.Kw(),
                brakingPower: 500.KwMech(),
                cannotBeReflected: true,
                constructionDurationPerProduct: Duration.FromSec(1)
            ));

            Log.Info("Layouts parsed");
        }
    }
}
