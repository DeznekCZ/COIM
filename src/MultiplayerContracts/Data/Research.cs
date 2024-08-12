using Mafi;
using Mafi.Base;
using Mafi.Core.Entities.Static;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using Mafi.Core.Research;
using ResNodeID = Mafi.Core.Research.ResearchNodeProto.ID;
using Mafi.Core.Entities;
using Mafi.Core.Products;

namespace MultiplayerContracts
{
	public partial class NewIds
	{
        public static readonly EntityProto.ID MultiplayerTradeVillage = new EntityProto.ID("MultiplayerTradeVillage");
		public static readonly StaticEntityProto.ID MultiplayerTradeDock = new StaticEntityProto.ID("MultiplayerTradeDock");

		public partial class MultiplayerTradeDocks
        {
        }

        public partial class Research
		{
			[ResearchCosts(difficulty: 1)]
			public static readonly ResNodeID MultiplayerTrade = Ids.Research.CreateId("MultiplayerTrade");
		}
	}

	internal class Research : AValidatedData, IResearchNodesData
	{

		protected override void RegisterDataInternal(ProtoRegistrator registrator)
		{
			ResearchNodeProto nodeProto = registrator.ResearchNodeProtoBuilder
				.Start("Captain trades", NewIds.Research.MultiplayerTrade)
				.Description("Unlocks trade with other friendly captains")
				.SetCosts(ResearchCostsTpl.Build.SetDifficulty(4))
				.AddLayoutEntityToUnlock(NewIds.MultiplayerTradeDock)
				.BuildAndAdd();
			
			nodeProto.GridPosition = new Vector2i(36, 14);
			nodeProto.AddParent(registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(Ids.Research.CargoDepot));

			MultiplayerTradeManager.Init(registrator.PrototypesDb);
		}

	}
}