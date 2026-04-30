using System.Linq;
using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Input;
using Mafi.Core.Prototypes;
using Mafi.Core.World.Entities;
using Mafi.Core.World;

namespace ProgramableNetwork
{
	/// <summary>
	/// Processor for antena / data-band mutation commands dispatched from the
	/// AntenaInspector and the FM/AM channel views.  Keeps the prior call shapes
	/// (CreateChannel, RemoveChannel, channel.Index/Antena/AmSource setters) so on-disk
	/// format and runtime behaviour are unchanged — only the entry point moves under
	/// the input scheduler.
	/// </summary>
	[GlobalDependency(RegistrationMode.AsAllInterfaces, false, false)]
	internal class AntenaCommandExecutor :
		ICommandProcessor<AntenaCreateRedirectedChannelCmd>,
		ICommandProcessor<AntenaRemoveRedirectedChannelCmd>,
		ICommandProcessor<AntenaChannelSetIndexCmd>,
		ICommandProcessor<AntenaChannelSetFmSourceCmd>,
		ICommandProcessor<AntenaChannelSetAmSourceCmd>,
		ICommandProcessor<AntenaSetDataBandCmd>
	{
		private readonly IEntitiesManager m_entitiesManager;
		private readonly ProtosDb m_protosDb;

		public AntenaCommandExecutor(IEntitiesManager entitiesManager, ProtosDb protosDb)
		{
			m_entitiesManager = entitiesManager;
			m_protosDb = protosDb;
		}

		public void Invoke(AntenaCreateRedirectedChannelCmd cmd)
		{
			if (!tryGetAntena(cmd.AntenaId, out Antena antena, out string error)) {
				cmd.SetResultError(error);
				return;
			}
			if (antena.DataBand == null) {
				cmd.SetResultError("Antena has no data band.");
				return;
			}
			antena.DataBand.CreateChannel();
			cmd.SetResultSuccess();
		}

		public void Invoke(AntenaRemoveRedirectedChannelCmd cmd)
		{
			if (!tryGetAntena(cmd.AntenaId, out Antena antena, out string error)) {
				cmd.SetResultError(error);
				return;
			}
			var channel = getRedirectedChannel(antena, cmd.RedirectedSlot);
			if (channel == null) {
				cmd.SetResultError($"Redirected slot {cmd.RedirectedSlot} not found.");
				return;
			}
			antena.DataBand.RemoveChannel(channel);
			cmd.SetResultSuccess();
		}

		public void Invoke(AntenaChannelSetIndexCmd cmd)
		{
			if (!tryGetAntena(cmd.AntenaId, out Antena antena, out string error)) {
				cmd.SetResultError(error);
				return;
			}
			var channel = getRedirectedChannel(antena, cmd.RedirectedSlot);
			if (channel == null) {
				cmd.SetResultError($"Redirected slot {cmd.RedirectedSlot} not found.");
				return;
			}
			// Channels expose the Index setter via their concrete type — both FM and AM.
			if (channel is FMDataBandChannel fm) {
				fm.Index = cmd.NewIndex;
			} else if (channel is AMDataBandChannel am) {
				am.Index = cmd.NewIndex;
			}
			cmd.SetResultSuccess();
		}

		public void Invoke(AntenaChannelSetFmSourceCmd cmd)
		{
			if (!tryGetAntena(cmd.AntenaId, out Antena antena, out string error)) {
				cmd.SetResultError(error);
				return;
			}
			var channel = getRedirectedChannel(antena, cmd.RedirectedSlot) as FMDataBandChannel;
			if (channel == null) {
				cmd.SetResultError($"Slot {cmd.RedirectedSlot} is not an FM channel.");
				return;
			}
			Antena source = null;
			if (cmd.SourceAntenaId.HasValue) {
				if (!m_entitiesManager.TryGetEntity(cmd.SourceAntenaId.Value, out source)) {
					cmd.SetResultError($"Source antena {cmd.SourceAntenaId.Value} not found.");
					return;
				}
			}
			channel.Antena = source;
			cmd.SetResultSuccess();
		}

		public void Invoke(AntenaChannelSetAmSourceCmd cmd)
		{
			if (!tryGetAntena(cmd.AntenaId, out Antena antena, out string error)) {
				cmd.SetResultError(error);
				return;
			}
			var channel = getRedirectedChannel(antena, cmd.RedirectedSlot) as AMDataBandChannel;
			if (channel == null) {
				cmd.SetResultError($"Slot {cmd.RedirectedSlot} is not an AM channel.");
				return;
			}
			if (!cmd.SourceEntityId.HasValue) {
				channel.WorldMapMine = null;
				cmd.SetResultSuccess();
				return;
			}
			// Single non-generic lookup, dispatch on runtime type — same pattern as
			// AMDataBandChannel.UpdateAntenaReference, so no proto id is needed in the cmd.
			var maybeSource = m_entitiesManager.GetEntity(cmd.SourceEntityId.Value);
			if (!maybeSource.HasValue) {
				cmd.SetResultError($"Source entity {cmd.SourceEntityId.Value} not found.");
				return;
			}
			if (maybeSource.Value is WorldMapMine mine) {
				channel.WorldMapMine = mine;
				cmd.SetResultSuccess();
				return;
			}
			if (maybeSource.Value is BattleShip ship) {
				channel.BattleShip = ship;
				cmd.SetResultSuccess();
				return;
			}
			cmd.SetResultError($"Entity {cmd.SourceEntityId.Value} is neither a mine nor a battleship.");
		}

		public void Invoke(AntenaSetDataBandCmd cmd)
		{
			if (!tryGetAntena(cmd.AntenaId, out Antena antena, out string error)) {
				cmd.SetResultError(error);
				return;
			}
			var maybeProto = m_protosDb.Get<DataBandProto>(cmd.DataBandProtoId);
			if (!maybeProto.HasValue) {
				cmd.SetResultError($"DataBand proto {cmd.DataBandProtoId.Value} not found.");
				return;
			}
			var proto = maybeProto.Value;
			antena.DataBand = proto.Constructor(antena, antena.Context, proto);
			cmd.SetResultSuccess();
		}

		private bool tryGetAntena(EntityId antenaId, out Antena antena, out string error)
		{
			if (!m_entitiesManager.TryGetEntity(antenaId, out antena)) {
				error = $"Antena {antenaId} not found.";
				return false;
			}
			error = "";
			return true;
		}

		private static IDataBandChannel getRedirectedChannel(Antena antena, int slot)
		{
			if (antena.DataBand == null) {
				return null;
			}
			var list = antena.DataBand.Channels.ToList();
			if (slot < 0 || slot >= list.Count) {
				return null;
			}
			return list[slot];
		}
	}
}
