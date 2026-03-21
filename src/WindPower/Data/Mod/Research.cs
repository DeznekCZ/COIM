using Mafi;
using Mafi.Base;
using Mafi.Core.Mods;
using Mafi.Core.Research;
using System;
using System.Collections.Generic;
using Mafi.Core.Prototypes;
using ResNodeID = Mafi.Core.Research.ResearchNodeProto.ID;

namespace WindPower {
	public partial class NewIds {
		public partial class Research {
			public static ResNodeID WindPowerT1 = new ResNodeID("WindPower_T1");
			public static ResNodeID WindPowerT2 = new ResNodeID("WindPower_T2");
		}
	}

	internal class Research : AValidatedData, IResearchNodesData {

		protected override void RegisterDataInternal(ProtoRegistrator registrator) {
			ResearchNodeProto research = registrator.PrototypesDb.Get<ResearchNodeProto>(Ids.Research.Cp2Packing).Value;
			ResearchNodeProto t1 = registrator.ResearchNodeProtoBuilder.Start("Wind power", NewIds.Research.WindPowerT1, 20)
				.SetGridPosition(new Vector2i(24, 0))
				.AddParents(research)
				.AddLayoutEntityToUnlock(NewIds.WindPower.WindTurbine_T1)
				.BuildAndAdd();

			research = registrator.PrototypesDb.Get<ResearchNodeProto>(Ids.Research.AluminumSmelting).Value;
			registrator.ResearchNodeProtoBuilder.Start("Wind power II", NewIds.Research.WindPowerT2, 180)
				.SetGridPosition(new Vector2i(128, 11))
				.AddParents(research, t1)
				.AddLayoutEntityToUnlock(NewIds.WindPower.WindTurbine_T2)
				.AddRecipeToUnlock(Ids.Recipes.CompositePanelAssemblyT1)
				.AddRecipeToUnlock(Ids.Recipes.CompositePanelAssemblyT2)
				.AddRecipeToUnlock(Ids.Recipes.CompositePanelAssemblyT3, hideInUi: true)
				.BuildAndAdd();
		}
	}
}