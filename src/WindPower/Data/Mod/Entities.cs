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
				costs: ((EntityCostsTpl)new EntityCostsTpl.Builder()
					// TODO be constructed by special product type later
					.CP2(25)
					.MaintenanceT1(0.5)
					.Product(25, Ids.Products.ConcreteSlab)
				).MapToEntityCosts(registrator),
				graphics: new WindTurbineProto.Gfx(
					prefabPath: "Assets/WindPower/WindTurbine_T1.prefab",
					gondolaGo: "Scaling/Gondola",
					rotorGo: "Scaling/Gondola/Rotor",
					bladeGos: [
						"Scaling/Gondola/Rotor/Blade",
						"Scaling/Gondola/Rotor/Blade_1",
						"Scaling/Gondola/Rotor/Blade_2"
					],
					speedMultiplier: 0.85.ToFix32(),
					customIconPath: "Assets/WindPower/WindTurbine_T1_Icon.png",
					categories: registrator.GetCategoriesProtos(Ids.ToolbarCategories.Power_General)
				),
				generatedPower: 60.Kw(),
				brakingPower: 15.KwMech(),
				gondolaHeight: new HeightTilesF(24 / 2),
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
								new CustomLayoutToken("00]", (EntityLayoutParams param, int height) =>
								{
									return new LayoutTokenSpec(
										heightFrom: 0,
										heightToExcl: 50,
										minTerrainHeight: -10,
										maxTerrainHeight: height - 1,
										constraint: LayoutTileConstraint.NoRubbleAfterCollapse,
										surfaceId: Ids.TerrainTileSurfaces.ConcreteReinforced
									);
								}),
								new CustomLayoutToken("~0~", (EntityLayoutParams param, int height) =>
								{
									return new LayoutTokenSpec(
										heightFrom: 20,
										heightToExcl: 50,
										minTerrainHeight: -10,
										maxTerrainHeight: height - 1,
										constraint: LayoutTileConstraint.NoRubbleAfterCollapse
									);
								})
							]
						),
						generateCircleWithCore(
							radius: 25, radiusSymbol: "~8~", core: 2, coreSymbol: "09]", centerTile: false)
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
					gondolaGo: "turbine_box",
					rotorGo: "turbine_box/turbine_axis/turbine_hub",
					bladeGos: [
						//"turbine_box/turbine_axis/turbine_hub/turbine_blade1",
						//"turbine_box/turbine_axis/turbine_hub/turbine_blade2",
						//"turbine_box/turbine_axis/turbine_hub/turbine_blade3"
					],
					speedMultiplier: 0.25.ToFix32(),
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
			int radius, string radiusSymbol, int core, string coreSymbol, bool centerTile = true) {
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
					if (dS <= coreS) {
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
