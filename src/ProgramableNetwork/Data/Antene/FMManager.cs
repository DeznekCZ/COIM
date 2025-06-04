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
        private readonly Dictionary<Tile3i, FMDataBand> m_dataBands;
        private static List<FMDataBand> m_flips;

        public FMManager(IEntitiesManager entitiesManager, IGameLoopEvents gameLoopEvents) {

            m_antenas = entitiesManager.GetAllEntitiesOfType<Antena>().ToDictionary(a => a.Position3f.Tile3i);
            m_dataBands = m_antenas.Where(p => p.Value.DataBand is FMDataBand).ToDictionary(a => a.Key, a => a.Value.DataBand as FMDataBand);

            entitiesManager.EntityAdded.AddNonSaveable(this, OnAdded);
            entitiesManager.EntityRemoved.AddNonSaveable(this, OnRemoved);
            gameLoopEvents.SyncUpdate.AddNonSaveable(this, OnSync);
        }

        /// <summary>
        /// UI question for signals arround
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Dictionary<int, (Fix32, FMDataBandChannel)> Signals(Tile3i position, int? channelIdx = null, bool logging = false)
        {
            Dictionary<int, Fix32> distances = new Dictionary<int, Fix32>();
            Dictionary<int, (Fix32, FMDataBandChannel)> channels = new Dictionary<int, (Fix32, FMDataBandChannel)>();

            if (logging)
                Log.Info($"[FMManager] Get stats");
            foreach (var pair in m_dataBands)
            {
                (Tile3i tile, FMDataBand databand) = (pair.Key, pair.Value);
                Antena antena = databand.Antena;
                Fix32 distance = (tile.ToCenterVector3() - position.ToCenterVector3()).magnitude.ToFix32();
                Fix32 targetDistance = (antena.Prototype.DistanceBoost * databand.Prototype.Distance);

                if (logging)
                    Log.Info($"[FMManager] IS in distance: {distance <= targetDistance}, spread: {targetDistance}, distance: {distance}");
                if (distance <= targetDistance)
                {
                    if (channelIdx != null)
                    {
                        FMDataBandChannel channel = databand.ActiveChannels[channelIdx ?? 0];
                        if (channel.ValidIterations > 0 && (!distances.TryGetValue(channel.Index, out Fix32 farther) || farther > distance))
                        {
                            distances[channelIdx ?? 0] = distance;
                            channels[channelIdx ?? 0] = (
                                channel.ValidIterations > 0 ?
                                    1 - (distance / targetDistance) :
                                    Fix32.Zero, channel);
                        }
                        if (logging)
                            Log.Info($"[FMManager] IN distance [{channelIdx ?? 0}]: {distances[channelIdx ?? 0]} with strength: {channels[channelIdx ?? 0].Item1} and datalen: {channels[channelIdx ?? 0].Item2.Value.Length}");
                        continue;
                    }

                    foreach (FMDataBandChannel channel in databand.ActiveChannels)
                    {
                        if (!distances.TryGetValue(channel.Index, out Fix32 farther) || farther > distance)
                        {
                            distances[channel.Index] = distance;
                            channels[channel.Index] = (
                                channel.ValidIterations > 0 ?
                                    1 - (distance / targetDistance) :
                                    Fix32.Zero, channel);
                        }
                    }
                }
            }
            return channels;
        }

        public (Fix32 strenght, Fix32[] values) Signal(Tile3i position, int channelIdx, bool logging = false)
        {
            if (Signals(position, channelIdx, logging).TryGetValue(channelIdx, out var channel))
            {
                return (channel.Item1, channel.Item2.Value);
            }
            return (Fix32.Zero, Array.Empty<Fix32>());
        }

        private void OnSync(GameTime time)
        {
            if (time.IsGamePaused) return;

            foreach (var item in m_antenas)
            {
                if (item.Value.DataBand is FMDataBand dataBand)
                {
                    m_dataBands[item.Key] = dataBand;
                }
                else
                {
                    m_dataBands.Remove(item.Key);
                }
            }
        }

        private void OnAdded(IEntity entity)
        {
            if (entity is Antena antena)
            {
                m_antenas[antena.Position3f.Tile3i] = antena;
                if (antena.DataBand is FMDataBand dataBand)
                    m_dataBands[antena.Position3f.Tile3i] = dataBand;
            }
        }

        private void OnRemoved(IEntity entity)
        {
            if (entity is Antena antena)
            {
                m_antenas.Remove(antena.Position3f.Tile3i);
                m_dataBands.Remove(antena.Position3f.Tile3i);
            }
        }
    }
}
