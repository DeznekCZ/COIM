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
        private float[] m_terrainHeightMap;
        private int[] m_entityHeightMap;
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

            m_terrainManager.HeightChanged.AddNonSaveable(this, heightChanged);
            m_entitiesManager.StaticEntityAdded.AddNonSaveable(this, entityAdded);
            m_entitiesManager.StaticEntityRemoved.AddNonSaveable(this, entityRemoved);

            m_terrainHeightMap = m_terrainManager.HeightsData.AsEnumerable()
                                .Select(h => h.Value.ToFloat()).ToArray();
            m_entityHeightMap = new int[m_terrainHeightMap.Length];

            _ = Recalculate(m_cancelationTokenSourceCalculation.Token);
            _ = WindDirection(m_cancellationTokenSourceWindDirection.Token);
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
            if (m_terrainManager == null || m_weatherManager == null || m_entitiesManager == null)
            {
                m_terrainManager = GlobalDependencyResolver.Get<TerrainManager>();
                m_weatherManager = GlobalDependencyResolver.Get<IWeatherManager>();
                m_entitiesManager = GlobalDependencyResolver.Get<IEntitiesManager>();
            }

            Fix32 newWind = m_weatherManager.CurrentWeather.Graphics.WindStrength.ToFix32();
            if (m_oldWind != newWind)
            {
                m_cache.Clear();
                m_oldWind = newWind;
            }
            else if (m_cache.TryGetValue(tile, out Percent power))
                return power;

            float buildingHeight = 0;
            float count = 0;
            for (int x = 1; x <= 10; x++)
            {
                for (int y = 1; y <= 10; y++)
                {
                    if ((x * y).Sqrt() < 10) continue;
                    count += 4;

                    int index = m_terrainManager.GetTileIndex(tile.X + x, tile.Y + y).Value;
                    buildingHeight += m_terrainHeightMap[index] + m_entityHeightMap[index];

                    index = m_terrainManager.GetTileIndex(tile.X - x, tile.Y + y).Value;
                    buildingHeight += m_terrainHeightMap[index] + m_entityHeightMap[index];

                    index = m_terrainManager.GetTileIndex(tile.X - x, tile.Y - y).Value;
                    buildingHeight += m_terrainHeightMap[index] + m_entityHeightMap[index];

                    index = m_terrainManager.GetTileIndex(tile.X + x, tile.Y - y).Value;
                    buildingHeight += m_terrainHeightMap[index] + m_entityHeightMap[index];

                    //Log.Debug($"T:{m_terrainHeightMap[index]}, B:{m_entityHeightMap[index]}");
                }
            }
            buildingHeight /= count;

            if (buildingHeight.ToFix32() > tile.Z + (gondola.Value * 2))
                return m_cache[tile] = Percent.Zero;

            // Get partial by terrain height
            Fix32 terrain = 0.5.ToFix32() + (tile.Z.ToFix32() + gondola.Value) / 40.ToFix32() * 0.5.ToFix32();
            Fix32 building = (tile.Z.ToFix32() + gondola.Value - buildingHeight.ToFix32()) / bladeWidth.Value;

            if (tile.Z.ToFix32() + gondola.Value - buildingHeight.ToFix32() > bladeWidth.Value)
                return m_cache[tile] = newWind.ToPercent() * terrain.ToPercent();


            return m_cache[tile] = (newWind * terrain * building).ToPercent();
        }

        public void Dispose()
        {
            m_cancelationTokenSourceCalculation.Cancel();
            m_cancellationTokenSourceWindDirection.Cancel();
        }

        private void entityAdded(IStaticEntity entity)
        {
            // ignore wind turbine in calculation of displacement
            if (entity is WindTurbine)
                return;

            m_cancelationTokenSourceCalculation.Cancel();
            m_cancelationTokenSourceCalculation = new CancellationTokenSource();
            _ = Recalculate(m_cancelationTokenSourceCalculation.Token);
        }

        private void entityRemoved(IStaticEntity entity)
        {
            // ignore wind turbine in calculation of displacement
            if (entity is WindTurbine)
                return;

            m_cancelationTokenSourceCalculation.Cancel();
            m_cancelationTokenSourceCalculation = new CancellationTokenSource();
            _ = Recalculate(m_cancelationTokenSourceCalculation.Token);
        }

        private async Task Recalculate(CancellationToken token)
        {
            Log.Debug("[WindPower] Creation of height map of entities - STARTED");
            int[] entityHeightMap = new int[m_entityHeightMap.Length];
            foreach (IStaticEntity entity
                in m_entitiesManager.Entities
                                    .Where(e => (e is IStaticEntity && !(e is WindTurbine)))
                                    .Cast<IStaticEntity>())
            {
                foreach (OccupiedTileRelative tileRef in entity.OccupiedTiles)
                {
                    Tile2i coord = entity.Position2f.Tile2i + tileRef.RelCoord;
                    int index = m_terrainManager.GetTileIndex(coord).Value;
                    int height = tileRef.FromHeightRel.Value;
                    entityHeightMap[index] = entityHeightMap[index].Max(height);
                }
                if (token.IsCancellationRequested)
                    break; // apply partial cache

                await Task.Yield();
            }
            m_entityHeightMap = entityHeightMap;
            m_cache.Clear();
            Log.Debug("[WindPower] Creation of height map of entities - " + (token.IsCancellationRequested ? "CANCELED" : "DONE"));
        }

        private void heightChanged(Tile2iAndIndex index)
        {
            m_terrainHeightMap[index.IndexRaw] = m_terrainManager.GetHeight(index.Index).Value.ToFloat();
        }
    }
}
