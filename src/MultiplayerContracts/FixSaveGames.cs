

using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Mafi;
using Mafi.Base;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Collections.ReadonlyCollections;
using Mafi.Core;
using Mafi.Core.Buildings.FuelStations;
using Mafi.Core.Buildings.Mine;
using Mafi.Core.Buildings.Settlements;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Dynamic;
using Mafi.Core.Entities.Static;
using Mafi.Core.Population;
using Mafi.Core.Products;
using Mafi.Core.PropertiesDb;
using Mafi.Core.Prototypes;
using Mafi.Core.Stats;
using Mafi.Core.Terrain;
using Mafi.Core.Vehicles.Trucks;
using Mafi.Core.World;
using Mafi.Core.World.Contracts;
using Mafi.Core.World.Entities;
using MultiplayerContracts;

[GlobalDependency(RegistrationMode.AsEverything, false, false)]
public class FixSavedGames : IInitializer
{
	private readonly SettlementsManager m_settlementsManager;
    private readonly TerrainManager m_terrainManager;
    private readonly EntitiesBuilder m_entitiesBuilder;
    private readonly IEntitiesManager m_entitiesManager;

	private readonly IConstructionManager m_constructionManager;

	private readonly IProductsManager m_productsManager;

	private readonly PopsHealthManager m_healthManager;

	private readonly UpointsManager m_upointsManager;

	private readonly ProtosDb m_protosDb;

	private readonly UnlockedProtosDb m_unlockedProtosDb;

	private readonly IPropertiesDb m_propsDb;

	private readonly StatsManager m_statsManager;

	private readonly WorldMapManager m_mapManager;

    private readonly ContractsManager m_contractManager;

    public bool IsBeingLoaded { get; }

	public FixSavedGames(SettlementsManager settlementsManager, EntitiesBuilder entitiesBuilder, TerrainManager terrainManager, IEntitiesManager entitiesManager, IConstructionManager constructionManager, IProductsManager productsManager, PopsHealthManager healthManager, UpointsManager upointsManager, ProtosDb protosDb, UnlockedProtosDb unlockedProtosDb, IPropertiesDb propsDb, StatsManager statsManager, ContractsManager contractManager, WorldMapManager mapManager)
	{
		m_settlementsManager = settlementsManager;
		m_terrainManager = terrainManager;
		m_entitiesBuilder = entitiesBuilder;
		m_entitiesManager = entitiesManager;
		m_constructionManager = constructionManager;
		m_productsManager = productsManager;
		m_healthManager = healthManager;
		m_upointsManager = upointsManager;
		m_protosDb = protosDb;
		m_unlockedProtosDb = unlockedProtosDb;
		m_propsDb = propsDb;
		m_statsManager = statsManager;
		m_mapManager = mapManager;
		m_contractManager = contractManager;
		IsBeingLoaded = true;

		Log.Info($"[{ModDefinition.ModName}]: Save game fixer created.");
		((IInitializer)this).DoOnNewGameOrAfterLoad((Action)SavedGameFixer);
	}

	void IInitializer.DoOnNewGameOnly(Action action)
	{
		Log.Info($"[{ModDefinition.ModName}]: Save game fix initialized on new game, so do nothing.");
	}

	private void SavedGameFixer()
	{
		int countOfIsland = m_mapManager.Map.Locations
			.Where(loc => loc.Entity.ValueOrNull?.Prototype.Id == NewIds.MultiplayerTradeVillage)
			.Count();

		Log.Debug($"[{ModDefinition.ModName}]: Creating multiplayer island {countOfIsland}");
		if (countOfIsland == 0)
		{
			Log.Debug($"[{ModDefinition.ModName}]: Creating multiplayer island");

			WorldMapLocation mapLocation = new WorldMapLocation("testMPShop", m_mapManager.Map.HomeLocation.Position + new Vector2i(-100, 100));

			MethodInfo m = typeof(WorldMapLocation)
				.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
				.First(methos => methos.Name == "SetEntityProto");
			m.Invoke(mapLocation, new object[] { m_protosDb.Get<MultiplayerTradeVillageProto>(NewIds.MultiplayerTradeVillage).ValueOrThrow("Missing multiplayer proto") });

			m = typeof(WorldMapLocation)
				.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
				.First(methos => methos.Name == "SetState");
			m.Invoke(mapLocation, new object[] { WorldMapLocationState.NotExplored });

			m_mapManager.Map.AddLocation(mapLocation);
			m_mapManager.Map.AddConnection(m_mapManager.Map.HomeLocation, mapLocation);

			Log.Debug($"[{ModDefinition.ModName}]: Multiplayer island created");
		}
	}

    void IInitializer.DoOnNewGameOrAfterLoad(Action action)
	{
		Log.Info($"[{ModDefinition.ModName}]: Save game fix on loaded game.");
        try
		{
			SavedGameFixer();
		}
        catch (Exception e)
        {
			Log.Exception(e);
            throw;
        }
		Log.Info($"[{ModDefinition.ModName}]: Save game fix on loaded game: DONE");
	}
}
