using Mafi;
using Mafi.Base;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindPower.Data.Unity;
using WindPower.Entity;

namespace WindPower {
	public partial class NewIds {
		public partial class WindPower {
			public static StaticEntityProto.ID WindTurbine_T1 => new StaticEntityProto.ID($"WindTurbine_T1");
			public static StaticEntityProto.ID WindTurbine_T2 => new StaticEntityProto.ID($"WindTurbine_T2");
		}
	}

	internal class Entities : AValidatedData {
		protected override void RegisterDataInternal(ProtoRegistrator registrator) {
			// Wind turbine protos
			WindTurbineProto t1 = registrator.PrototypesDb.Add(new WindTurbineProto(
				id: NewIds.WindPower.WindTurbine_T1,
				strings: Proto.CreateStr(NewIds.WindPower.WindTurbine_T1, "Wind turbine", "Basic wind turbine with manual braking"),
				layout: new EntityLayoutParser(registrator.PrototypesDb)
					.ParseLayoutOrThrow(
						new EntityLayoutParams(
							customTokens: [
								new CustomLayoutToken("~0]", (EntityLayoutParams param, int height) =>
								{
									return new LayoutTokenSpec(
										heightFrom: 0,
										heightToExcl: 14,
										minTerrainHeight: -1,
										maxTerrainHeight: 1,
										constraint: LayoutTileConstraint.Ground
									);
								}),
								new CustomLayoutToken("~0~", (EntityLayoutParams param, int height) =>
								{
									return new LayoutTokenSpec(
										heightFrom: 5,
										heightToExcl: 14,
										minTerrainHeight: -20,
										maxTerrainHeight: 20,
										constraint: LayoutTileConstraint.NoRubbleAfterCollapse
											| LayoutTileConstraint.NoConstructionCubes
									);
								})
							]
						),
						"         ~8~~8~~8~~8~~8~~8~         ",
						"      ~8~~7~~7~~7~~7~~7~~8~~8~      ",
						"   ~8~~7~~6~~6~~6~~6~~6~~6~~8~~8~   ",
						"~8~~7~~6~~5~~5~~5~~5~~5~~5~~6~~7~~8~",
						"~8~~7~~6~~5~~8]~8]~8]~8]~5~~6~~7~~8~",
						"~8~~7~~6~~5~~8]~8]~8]~8]~5~~6~~7~~8~",
						"~8~~7~~6~~5~~8]~8]~8]~8]~5~~6~~7~~8~",
						"~8~~7~~6~~5~~8]~8]~8]~8]~5~~6~~7~~8~",
						"~8~~7~~6~~5~~5~~5~~5~~5~~5~~6~~7~~8~",
						"   ~8~~7~~6~~6~~6~~6~~6~~6~~7~~8~   ",
						"      ~8~~7~~7~~7~~7~~7~~7~~8~      ",
						"         ~8~~8~~8~~8~~8~~8~         "
					),
				costs: ((EntityCostsTpl)new EntityCostsTpl.Builder()
					// TODO be constructed by special product type later
					.CP2(25)
					.MaintenanceT1(0.5)
					.Product(25, Ids.Products.ConcreteSlab)
				).MapToEntityCosts(registrator),
				graphics: new WindTurbineProto.Gfx(
					prefabPath: "Assets/WindPower/WindTurbine_T1.prefab",
					gondolaGo: "pivot_axis",
					rotorGo: "pivot_axis/turbine_axis",
					bladeGos: ImmutableArray.Empty, //ImmutableArray.Create<string>(
						//"pivot_axis/turbine_axis/turbine_blade1_axis",
						//"pivot_axis/turbine_axis/turbine_blade2_axis",
						//"pivot_axis/turbine_axis/turbine_blade3_axis"
					//),
					blurMeshPaths: ImmutableArray.Create(
						"pivot_axis/turbine_axis/turbine_blade1_axis",
						"pivot_axis/turbine_axis/turbine_blade2_axis",
						"pivot_axis/turbine_axis/turbine_blade3_axis",
						"pivot_axis/turbine_axis/turbine_hub_LOD0",
						"pivot_axis/turbine_axis/turbine_hub_LOD1",
						"pivot_axis/turbine_axis/turbine_hub_LOD2",
						"pivot_axis/turbine_axis/turbine_hub_LOD3",
						"pivot_axis/turbine_axis/turbine_blades_blur"
					),
					blurConfig: ImmutableArray.Create(
						new BlurConfig(0, 30, 0, 6),
						new BlurConfig(30, 50, 0, 7),
						new BlurConfig(50, int.MaxValue, 3, 7)
					),
					maximumRpm: 72,
					customIconPath: "Assets/WindPower/WindTurbine_T1_Icon.png",
					categories: registrator.GetCategoriesProtos(Ids.ToolbarCategories.Power_General)
				),
				generatedPower: 100.Kw(),
				brakingPower: 50.KwMech(),
				gondolaHeight: HeightTilesF.Zero + 18.0.MetersThick(),
				bladeWidth: new HeightTilesF((4.5f / 2f).ToFix32()),
				cannotBeReflected: true,
				constructionDurationPerProduct: Duration.FromSec(1)
			));

			// Wind turbine protos
			WindTurbineProto t2 = registrator.PrototypesDb.Add(new WindTurbineProto(
				id: NewIds.WindPower.WindTurbine_T2,
				strings: Proto.CreateStr(NewIds.WindPower.WindTurbine_T2, "Wind turbine", "Basic wind turbine with manual braking"),
				layout: new EntityLayoutParser(registrator.PrototypesDb)
					.ParseLayoutOrThrow(
						new EntityLayoutParams(
							customTokens: [
								new CustomLayoutToken("00]", (EntityLayoutParams param, int height) => new LayoutTokenSpec(
									heightFrom: 0,
									heightToExcl: 50,
									minTerrainHeight: -2,
									maxTerrainHeight: 2,
									constraint: LayoutTileConstraint.NoRubbleAfterCollapse,
									terrainSurfaceHeight: height == 9 ? 0 : null,
									surfaceId: Ids.TerrainTileSurfaces.DefaultConcrete
								)),
								new CustomLayoutToken("~0~", (EntityLayoutParams param, int height) => new LayoutTokenSpec(
									heightFrom: 20,
									heightToExcl: 50,
									minTerrainHeight: -2,
									maxTerrainHeight: 2,
									constraint: LayoutTileConstraint.NoRubbleAfterCollapse
										| LayoutTileConstraint.NoConstructionCubes
								)),
								new CustomLayoutToken("10~", (EntityLayoutParams param, int height) => new LayoutTokenSpec(
									heightFrom: 20,
									heightToExcl: 50,
									minTerrainHeight: -40,
									maxTerrainHeight: 20,
									constraint: LayoutTileConstraint.NoRubbleAfterCollapse
										| LayoutTileConstraint.NoConstructionCubes
								))
							]
						),
						generateCircleWithCore(
							radius: 25, radiusSymbol: "18~", core: 3, coreSymbol: "08]", centerTile: false,
							floor: 4, radiusFloorSymbol: "~8~", coreFloorSymbol: "08]", floorSymbol: "_1_")
							.ToArray()
					),
				// TODO be constructed by special product type later
				costs: ((EntityCostsTpl)new EntityCostsTpl.Builder()
					.CP4(100)
					.MaintenanceT2(1.5)
					.Product(75, Ids.Products.CompositePanel)
				).MapToEntityCosts(registrator),
				graphics: new WindTurbineProto.Gfx(
					prefabPath: "Assets/WindPower/WindTurbine_T2.prefab",
					gondolaGo: "pivot_axis",
					rotorGo: "pivot_axis/turbine_axis",
					bladeGos: ImmutableArray.Empty, //ImmutableArray.Create<string>(
						//"pivot_axis/turbine_axis/turbine_blade1_axis",
						//"pivot_axis/turbine_axis/turbine_blade2_axis",
						//"pivot_axis/turbine_axis/turbine_blade3_axis"
					//),
					blurMeshPaths: ImmutableArray.Empty,
					blurConfig: ImmutableArray.Empty,
					maximumRpm: -22,
					customIconPath: "Assets/WindPower/WindTurbine_T1_Icon.png",
					categories: registrator.GetCategoriesProtos(Ids.ToolbarCategories.Power_General)
				),
				generatedPower: 1800.Kw(),
				brakingPower: 600.KwMech(),
				gondolaHeight: new HeightTilesF(70 / 2),
				bladeWidth: new HeightTilesF((70 / 2).ToFix32()),
				cannotBeReflected: true,
				constructionDurationPerProduct: Duration.FromSec(1)
			));

			t1.SetNextTierIndirect(t2);

			Log.Info("Layouts parsed");
		}

		private IEnumerable<string> generateCircleWithCore(
			int radius, string radiusSymbol, int core, string coreSymbol, bool centerTile = true,
			int floor = -1, string coreFloorSymbol = null,  string radiusFloorSymbol = null, string floorSymbol = null
		) {
			int radiusS = radius * radius;
			int coreS = core * core;
			for (int x = -radius; x <= radius; x++) {
				if (x == 0 && centerTile == false) {
					continue;
				}
				StringBuilder sb = new StringBuilder(capacity: 2 * radius * 3);
				for (int y = -radius; y <= radius; y++) {
					if (y == 0 && centerTile == false) {
						continue;
					}
					int xS = x * x;
					int yS = y * y;
					int dS = xS + yS;
					if (floor >= 0 && x.Abs() <= floor && y.Abs() <= floor) {
						if (dS <= coreS) {
							sb.Append(coreFloorSymbol);
						} else if (dS <= radiusS) {
							sb.Append(radiusFloorSymbol);
						} else {
							sb.Append(floorSymbol);
						}
					} else if (dS <= coreS) {
						sb.Append(coreSymbol);
					} else if (dS <= radiusS) {
						sb.Append(radiusSymbol);
					} else {
						sb.Append("   ");
					}
				}
				yield return sb.ToString();
			}
		}
	}
}
