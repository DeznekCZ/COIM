using Mafi;
using Mafi.Base;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
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
                gondolaHeight: new HeightTilesF(16 / 2),
                bladeWidth: new HeightTilesF((3f / 2f).ToFix32()),
                cannotBeReflected: true,
                constructionDurationPerProduct: Duration.FromSec(1)
            ));

            Log.Info("Layouts parsed");
        }
    }
}
