using Mafi;
using Mafi.Base;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using System;
using System.Linq;
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
                    .ParseLayoutOrThrow(
                        new EntityLayoutParams(
                            customTokens: [
                                new CustomLayoutToken("~0~", (EntityLayoutParams param, int height) =>
                                {
                                    return new LayoutTokenSpec(
                                        heightFrom: height,
                                        heightToExcl: 9,
                                        minTerrainHeight: -10,
                                        maxTerrainHeight: height - 1,
                                        constraint: LayoutTileConstraint.NoRubbleAfterCollapse
                                    );
                                })
                            ]
                        ),
                        "         ~8~~8~~8~~8~~8~         ",
                        "      ~8~~7~~7~~7~~7~~8~~8~      ",
                        "   ~8~~7~~6~~6~~6~~6~~6~~8~~8~   ",
                        "~8~~7~~6~~5~~5~~5~~5~~5~~6~~7~~8~",
                        "~8~~7~~6~~5~[8][8][8]~5~~6~~7~~8~",
                        "~8~~7~~6~~5~[8][8][8]~5~~6~~7~~8~",
                        "~8~~7~~6~~5~[8][8][8]~5~~6~~7~~8~",
                        "~8~~7~~6~~5~~5~~5~~5~~5~~6~~7~~8~",
                        "   ~8~~7~~6~~6~~6~~6~~6~~7~~8~   ",
                        "      ~8~~7~~7~~7~~7~~7~~8~      ",
                        "         ~8~~8~~8~~8~~8~         "
                    ),
                costs: ((EntityCostsTpl)new EntityCostsTpl.Builder().CP2(5).MaintenanceT1(2).Product(9, Ids.Products.ConcreteSlab)).MapToEntityCosts(registrator),
                graphics: new LayoutEntityProto.Gfx(
                    prefabPath: "Assets/WindPower/WindTurbine_T1.prefab",
                    customIconPath: "Assets/WindPower/WindTurbine_T1_Icon.png",
                    categories: registrator.GetCategoriesProtos(Ids.ToolbarCategories.Power_General)
                ),
                generatedPower: 1200.Kw(),
                brakingPower: 500.KwMech(),
                gondolaHeight: new HeightTilesF(16 / 2),
                bladeWidth: new HeightTilesF((3f / 2f).ToFix32()),
                constructionDurationPerProduct: Duration.FromSec(1)
            ));

            Log.Info("Layouts parsed");
        }
    }
}
