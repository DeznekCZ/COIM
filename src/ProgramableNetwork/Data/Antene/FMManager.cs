using Mafi;
using System.Collections.Generic;
using System.Linq;
using Mafi.Core.Entities;
using Mafi.Core;
using Mafi.Core.GameLoop;
using System;
using Mafi.Unity;

namespace ProgramableNetwork.Data.Antene
{
	[GlobalDependency(RegistrationMode.AsEverything, false, false)]
	public class FMManager : IDataBandManager
	{
		private readonly Dictionary<Tile3i, Antena> m_antenas;
		private static List<FMDataBand> m_flips;

		public FMManager(IEntitiesManager entitiesManager, IGameLoopEvents gameLoopEvents) {

			m_antenas = entitiesManager.GetAllEntitiesOfType<Antena>().ToDictionary(a => a.Position3f.Tile3i);

			entitiesManager.EntityAdded.AddNonSaveable(this, OnAdded);
			entitiesManager.EntityRemoved.AddNonSaveable(this, OnRemoved);
		}

		/// <summary>
		/// UI question for signals arround
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public Dictionary<int, (Fix32 signalStrength, FMDataBandChannel channelInfo)> Signals(Tile3i position, int? channelIdx = null, bool logging = false)
		{
			Dictionary<int, Fix32> distances = new Dictionary<int, Fix32>();
			Dictionary<int, (Fix32, FMDataBandChannel)> channels = new Dictionary<int, (Fix32, FMDataBandChannel)>();

			if (logging) {
				Log.Info($"[FMManager] Get stats");
			}

			foreach ((Tile3i tile, Antena antenna) in m_antenas)
			{
				if (antenna.DataBand is not FMDataBand dataBand) {
					continue;
				}

				Fix32 distance = (tile.ToCenterVector3() - position.ToCenterVector3()).magnitude.ToFix32();
				Fix32 maxDistance = (antenna.Prototype.DistanceBoost * dataBand.Prototype.Distance);

				if (logging) {
					Log.Info($"[FMManager] IS in distance: {distance <= maxDistance}, spread: {maxDistance}, distance: {distance}");
				}

				// Skip unreachable antenna broadcasts
				if (distance > maxDistance) {
					continue;
				}

				// If a channel index is specified, only consider that channel for each data band
				if (channelIdx != null)
				{
					int channelIndex = channelIdx ?? 0;
					FMDataBandChannel channel = dataBand.ActiveChannels[channelIndex];
					if (channel.ValidIterations < 1
						|| (distances.TryGetValue(channel.Index, out Fix32 existing)
							&& existing <= distance)) {
						continue;
					}
					distances[channel.Index] = distance;
					channels[channel.Index] = (1 - (distance / maxDistance), channel);

					if (logging) {
						Log.Info($"[FMManager] IN distance [{channelIndex}]: {distances[channelIndex]} with strength: {channels[channelIndex].Item1} and datalen: {channels[channelIndex].Item2.Count}");
					}
					continue;
				}

				// If no channel index is specified, consider all channels for each data band
				foreach (FMDataBandChannel channel in dataBand.ActiveChannels) {
					if (channel.ValidIterations < 1
						|| (distances.TryGetValue(channel.Index, out Fix32 existing)
							&& existing <= distance)) {
						continue;
					}
					distances[channel.Index] = distance;
					channels[channel.Index] = (1 - (distance / maxDistance), channel);
				}
			}
			return channels;
		}

		public (Fix32 strenght, FMDataBandChannel values) Signal(Tile3i position, int channelIdx, bool logging = false)
		{
			if (Signals(position, channelIdx, logging).TryGetValue(channelIdx, out var channel))
			{
				return (channel.Item1, channel.Item2);
			}
			return (Fix32.Zero, null);
		}

		private void OnAdded(IEntity entity)
		{
			if (entity is Antena antena)
			{
				m_antenas[antena.Position3f.Tile3i] = antena;
			}
		}

		private void OnRemoved(IEntity entity)
		{
			if (entity is Antena antena)
			{
				m_antenas.Remove(antena.Position3f.Tile3i);
			}
		}
	}
}
