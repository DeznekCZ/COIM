using Mafi;
using Mafi.Collections.ReadonlyCollections;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Environment;
using Mafi.Core.Terrain;
using Mafi.Core.Terrain.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private readonly Dictionary<Tile3i, Percent> m_cache;
        private readonly IWeatherManager m_weatherManager;
        private readonly TerrainManager m_terrainManager;
        private readonly IEntitiesManager m_entitiesManager;
        private HeightTilesF[] m_terrainHeightMap;
        private int[] m_entityHeightMap;
        private Fix32 m_windDirection;

        public WindMap(IWeatherManager weatherManager, TerrainManager terrainManager, IEntitiesManager entitiesManager)
        {
            m_cache = new Dictionary<Tile3i, Percent>();
            m_cancelationTokenSourceCalculation = new CancellationTokenSource();
            m_cancellationTokenSourceWindDirection = new CancellationTokenSource();
            m_windDirection = UnityEngine.Random.Range(0, 360);

            m_weatherManager = weatherManager;
            m_terrainManager = terrainManager;
            m_entitiesManager = entitiesManager;

            m_terrainManager.HeightChanged.Add(this, heightChanged);
            m_entitiesManager.StaticEntityAdded.Add(this, entityAdded);
            m_entitiesManager.StaticEntityRemoved.Add(this, entityRemoved);

            m_terrainHeightMap = m_terrainManager.HeightsData.AsEnumerable().ToArray();
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

        public Percent GetWindPower(Tile3i tile, HeightTilesI gondola)
        {
            if (m_cache.TryGetValue(tile, out Percent power))
                return power;

            float buildingHeight = 0;
            float terrainHeight = 0;
            float count = 0;
            const float coef = 121; 
            for (int x = 1; x <= 10; x++)
            {
                for (int y = 1; y <= 10; y++)
                {
                    if (x * y > x * x || x * y > y * y) continue;
                    count += 4 * (coef / x * y);

                    int index = m_terrainManager.GetTileIndex(tile.X + x, tile.Y + y).Value;
                    buildingHeight += (m_entityHeightMap[index] - tile.Y) * (coef / x * y);
                    terrainHeight += (m_terrainHeightMap[index].Value.ToFloat() - tile.Y) * (coef / x * y);

                    index = m_terrainManager.GetTileIndex(tile.X - x, tile.Y + y).Value;
                    buildingHeight += (m_entityHeightMap[index] - tile.Y) * (coef / x * y);
                    terrainHeight += (m_terrainHeightMap[index].Value.ToFloat() - tile.Y) * (coef / x * y);

                    index = m_terrainManager.GetTileIndex(tile.X - x, tile.Y - y).Value;
                    buildingHeight += (m_entityHeightMap[index] - tile.Y) * (coef / x * y);
                    terrainHeight += (m_terrainHeightMap[index].Value.ToFloat() - tile.Y) * (coef / x * y);

                    index = m_terrainManager.GetTileIndex(tile.X + x, tile.Y - y).Value;
                    buildingHeight += (m_entityHeightMap[index] - tile.Y) * (coef / x * y);
                    terrainHeight += (m_terrainHeightMap[index].Value.ToFloat() - tile.Y) * (coef / x * y);
                }
            }

            float buildingHeightF = ((buildingHeight / count) / gondola.Value).Max(0).Min(1);
            float terrainHeightF = ((terrainHeight / count) + tile.Y + gondola.Value).Max(0);

            m_cache[tile] = Percent.FromFloat(
                m_weatherManager.CurrentWeather.Graphics.WindStrength *
                ((1 - buildingHeightF).Max(0)) *
                (terrainHeightF) / 50);
            return m_cache[tile];
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
                int terrainHeight = entity.Position3f.Z.IntegerPart;
                foreach (OccupiedTileRelative tileRef in entity.OccupiedTiles)
                {
                    Tile2i coord = entity.Position2f.Tile2i + tileRef.RelCoord;
                    int index = m_terrainManager.GetTileIndex(coord).Value;
                    int height = tileRef.FromHeightRel.Value + terrainHeight;
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
            m_terrainHeightMap[index.IndexRaw] = m_terrainManager.GetHeight(index.Index);
        }
    }
}
