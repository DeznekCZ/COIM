using Mafi;
using Mafi.Base;
using Mafi.Core.Mods;
using Mafi.Core.Research;
using ResNodeID = Mafi.Core.Research.ResearchNodeProto.ID;

namespace ControlledTransport
{
	public partial class NewIds
	{
		public partial class Research
		{
			[ResearchCosts(difficulty: 1)]
			public static readonly ResNodeID ControlledTransport = Ids.Research.CreateId("ControlledTransport");
		}
	}

	internal class Research : AValidatedData, IResearchNodesData
	{

		protected override void RegisterDataInternal(ProtoRegistrator registrator)
		{
			ResearchNodeProto nodeProto = registrator.ResearchNodeProtoBuilder
				.Start("Controlled transport", NewIds.Research.ControlledTransport)
				.Description("Unlocks controlled input by condition")
				.SetCosts(ResearchCostsTpl.Build.SetDifficulty(4))
				.AddLayoutEntityToUnlock(NewIds.ControlledTransports.PressureValve)
				.AddLayoutEntityToUnlock(NewIds.ControlledTransports.WeightOpener)
				.AddLayoutEntityToUnlock(NewIds.ControlledTransports.Counter)
				.BuildAndAdd();
			
			nodeProto.GridPosition = new Vector2i(32, 1);
			nodeProto.AddParent(registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(Ids.Research.ConveyorRouting));
		}

	}
}