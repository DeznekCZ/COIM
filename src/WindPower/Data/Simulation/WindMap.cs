using Mafi;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Environment;
using Mafi.Core.Terrain;
using Mafi.Depedencies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WindPower.Entity;

namespace WindPower.Simulation
{
    [GlobalDependency(RegistrationMode.AsSelf)]
    public class WindMap : IDisposable
    {
        private CancellationTokenSource m_cancelationTokenSourceCalculation;
        private CancellationTokenSource m_cancellationTokenSourceWindDirection;
        private Dictionary<Tile3i, Percent> m_cache;
        private IWeatherManager m_weatherManager;
        private TerrainManager m_terrainManager;
        private IEntitiesManager m_entitiesManager;
        private Task m_windDirectionTask;

        //private float[] m_terrainHeightMap;
        //private int[] m_entityHeightMap;
        private Fix32 m_windDirection;
        private Fix32 m_oldWind;

        public WindMap(IWeatherManager weatherManager, TerrainManager terrainManager, IEntitiesManager entitiesManager)
        {
            Init(weatherManager, terrainManager, entitiesManager);
        }

        private void Init(IWeatherManager weatherManager, TerrainManager terrainManager, IEntitiesManager entitiesManager)
        {
            m_cache = new Dictionary<Tile3i, Percent>();
            m_cancelationTokenSourceCalculation = new CancellationTokenSource();
            m_cancellationTokenSourceWindDirection = new CancellationTokenSource();
            m_windDirection = UnityEngine.Random.Range(0, 360);

            m_weatherManager = weatherManager;
            m_terrainManager = terrainManager;
            m_entitiesManager = entitiesManager;

            m_windDirectionTask = WindDirection(m_cancellationTokenSourceWindDirection.Token);
        }

        private async Task WindDirection(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(UnityEngine.Random.Range(1000, 5001), token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                m_windDirection += UnityEngine.Random.Range(-10f, 10f).ToFix32();
                if (m_windDirection < 0)
                    m_windDirection += Fix32.FromInt(360);
                else if (m_windDirection > 360)
                    m_windDirection -= Fix32.FromInt(360);
            }
        }

        public Fix32 GetWindDirection()
        {
            return m_windDirection;
        }

        public Percent GetWindPower(Tile3i tile, HeightTilesF gondola, HeightTilesF bladeWidth)
        {
            // TODO: temporary quicker solution
            Fix32 newWind = m_weatherManager.CurrentWeather.Graphics.WindStrength.ToFix32();
            Fix32 terrain = 0.5.ToFix32() + (tile.Z.ToFix32() + gondola.Value) / 40.ToFix32() * 0.5.ToFix32();
            return m_cache[tile] = (newWind * terrain).ToPercent();
        }

        //public Percent GetWindPower(Tile3i tile, HeightTilesF gondola, HeightTilesF bladeWidth)
        //{
        //    if (m_terrainManager == null || m_weatherManager == null || m_entitiesManager == null)
        //    {
        //        m_terrainManager = GlobalDependencyResolver.Get<TerrainManager>();
        //        m_weatherManager = GlobalDependencyResolver.Get<IWeatherManager>();
        //        m_entitiesManager = GlobalDependencyResolver.Get<IEntitiesManager>();
        //    }

        //    Fix32 newWind = m_weatherManager.CurrentWeather.Graphics.WindStrength.ToFix32();
        //    if (m_oldWind != newWind)
        //    {
        //        m_cache.Clear();
        //        m_oldWind = newWind;
        //    }
        //    else if (m_cache.TryGetValue(tile, out Percent power))
        //        return power;

        //    //float buildingHeight = 0;
        //    //float count = 0;
        //    //for (int x = 1; x <= 10; x++)
        //    //{
        //    //    for (int y = 1; y <= 10; y++)
        //    //    {
        //    //        if ((x * y).Sqrt() < 10) continue;
        //    //        count += 4;
        //    //
        //    //        int index = m_terrainManager.GetTileIndex(tile.X + x, tile.Y + y).Value;
        //    //        buildingHeight += m_terrainHeightMap[index] + m_entityHeightMap[index];
        //    //
        //    //        index = m_terrainManager.GetTileIndex(tile.X - x, tile.Y + y).Value;
        //    //        buildingHeight += m_terrainHeightMap[index] + m_entityHeightMap[index];
        //    //
        //    //        index = m_terrainManager.GetTileIndex(tile.X - x, tile.Y - y).Value;
        //    //        buildingHeight += m_terrainHeightMap[index] + m_entityHeightMap[index];
        //    //
        //    //        index = m_terrainManager.GetTileIndex(tile.X + x, tile.Y - y).Value;
        //    //        buildingHeight += m_terrainHeightMap[index] + m_entityHeightMap[index];
        //    //
        //    //        //Log.Debug($"T:{m_terrainHeightMap[index]}, B:{m_entityHeightMap[index]}");
        //    //    }
        //    //}
        //    //buildingHeight /= count;

        //    //if (buildingHeight.ToFix32() > tile.Z + (gondola.Value * 2))
        //    //    return m_cache[tile] = Percent.Zero;

        //    // Get partial by terrain height
        //    Fix32 terrain = 0.5.ToFix32() + (tile.Z.ToFix32() + gondola.Value) / 40.ToFix32() * 0.5.ToFix32();
        //    //Fix32 building = (tile.Z.ToFix32() + gondola.Value - buildingHeight.ToFix32()) / bladeWidth.Value;

        //    //if (tile.Z.ToFix32() + gondola.Value - buildingHeight.ToFix32() > bladeWidth.Value)
        //    //    return m_cache[tile] = newWind.ToPercent() * terrain.ToPercent();


        //    return m_cache[tile] = (newWind * terrain/* * building*/).ToPercent();
        //}

        public void Dispose()
        {
            m_cancelationTokenSourceCalculation.Cancel();
            m_cancellationTokenSourceWindDirection.Cancel();
        }
    }
}
