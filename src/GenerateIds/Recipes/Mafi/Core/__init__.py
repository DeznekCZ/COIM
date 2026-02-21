
class FileSystemHelper:
    DISALLOWED_PATH_NAMES = None
    ASSET_BUNDLES_DIR_NAME = ""
    DLCS_DIR_NAME = ""
    MAPS_DIR_NAME = ""
    MAP_NAME_SUFFIX = ""
    MAP_WIP_NAME_SUFFIX = ""
    MAP_AUTOSAVE_NAME_SUFFIX = ""
    def __init__(self):
        self.GameDataRootDirPath = ""
        self.GameDataRootDirPathLegacy = ""
        self.WorkDirPath = ""
        self.BuiltInMapsPath = ""

class IFileSystemHelper:
    def __init__(self):
        self.GameDataRootDirPath = ""
        self.GameDataRootDirPathLegacy = ""
        self.WorkDirPath = ""
        self.BuiltInMapsPath = ""

class FileType:
    Misc = None
    GameSave = None
    Replay = None
    Map = None
    Screenshot = None
    Log = None
    Debug = None
    Console = None
    Mod = None
    Blueprints = None
    AssetsOverrides = None
    TerrainDataCache = None
    CameraRecording = None
    def __init__(self):
        self.value__ = 0

class SaveFileInfo:
    AUTOSAVE_SUFFIX = ""
    AUTOSAVE_WHEN_PAUSED_SUFFIX = ""
    def __init__(self):
        self.IsAutoSave = False
        self.GameName = ""
        self.NameNoExtension = ""
        self.WriteTimestamp = None
        self.SizeBytes = 0

class SaveFileGroup:
    def __init__(self):
        self.GameName = ""
        self.Saves = None

class FileSystemHelperExtensions:
    def __init__(self):
        pass


class ProductQuantityAssertionExtensions:
    def __init__(self):
        pass


class ProtoAssertionExtensions:
    def __init__(self):
        pass


class BuildInfo:
    Data = None
    IS_DEBUG = False
    IS_DEV_ONLY = False
    IS_RELEASE_CHEATS = False
    IS_DEMO = False
    def __init__(self):
        pass


class CoreMod:
    def __init__(self):
        self.Manifest = None
        self.IsUiOnly = False
        from Mafi import Option
        self.ModConfig = Option()
        self.JsonConfig = None
        self.Config = None

class CoreModConfig:
    def __init__(self):
        from Mafi import Option
        self.LoadedWorldMapName = Option()
        self.DisableTerrainPhysics = False
        self.DisableTerrainSurfaceSimulation = False
        self.DisablePathFinding = False
        self.DisableMultiThreadTerrainGeneration = False
        self.DisableBoundaryCellAutoUnlock = False
        self.DisableResourcesGeneration = False
        self.LoadedIslandMapName = Option()
        self.DisableLockedCellsTerrainGeneration = False
        self.ShouldUnlockAllProtosOnInit = False
        self.LogCommandsAsCSharp = False
        self.IsInstaBuildEnabled = False
        self.DisableBuildPathHeuristics = False
        self.DisableSimulationBackgroundThread = False
        self.DeterminismValidationEnabled = False
        self.DeterminismValidationFrequencySteps = None
        self.DeterminismDisableCommandsForwarding = False
        self.DefenderExtraBattlePriority = 0
        self.MaxBattleRounds = 0
        self.StartingExtraFleetDistance = 0
        self.PossibleEscapeDistance = 0
        self.ShipEscapeHpThreshold = None
        self.BaseRoundsToEscape = 0
        self.ChanceForSameEntityRepeatedFire = None
        self.ChanceForDisabledEnemyFire = None
        self.ExtraMissChanceWhenEscaping = None
        self.MaxArmorReduction = None
        self.RecoverableHpMultiplier = None
        self.HullDamageMultWhenPartIsHit = None
        self.StartingPopulation = 0
        self.SaveTraceOnSimOvertime = False
        self.SaveTraceOnSimOvertimeMinDelay = None
        self.SaveTimingLogPeriod = None
        self.InitialVehiclesCap = 0
        self.AlwaysSunny = False

class GameTime:
    DEFAULT_SIM_STEP_DURATION_MS = 0
    def __init__(self):
        from Mafi import Fix64
        self.TimeSinceStartMs = Fix64()
        self.TimeSinceLoadMs = Fix64()
        self.WallTimeSinceLoadMs = Fix64()
        self.TotalElapsedSeconds = Fix64()
        self.SimStepsCount = 0
        self.SimStepsSinceLoad = 0
        self.TotalElapsedSimStepsSmooth = Fix64()
        self.DeltaSimStepsApprox = 0.0
        from Mafi import Fix32
        self.TimeSinceLastSimUpdateMs = Fix32()
        self.DeltaTimeMs = 0.0
        self.FrameTimeSec = 0.0
        self.AbsoluteT = 0.0
        self.RelativeT = 0.0
        self.DeltaT = 0.0
        self.IsGamePaused = False
        self.SimStepsPerUpdate = 0
        self.GameSpeedMult = 0.0
        self.CurrSimUpdateDurationMs = Fix32()

class IdsCore:
    def __init__(self):
        pass


    class Buildings:
        from Mafi.Core.Entities.Static import StaticEntityProto
        MineTower = StaticEntityProto.ID('MineTower')
        def __init__(self):
            pass


    class TerrainDesignators:
        from Mafi.Core.Prototypes import Proto
        MiningDesignator = Proto.ID('MiningDesignator')
        ForestryDesignator = Proto.ID('ForestryDesignator')
        DumpingDesignator = Proto.ID('DumpingDesignator')
        LevelDesignator = Proto.ID('LevelDesignator')
        PlaceSurfaceDesignator = Proto.ID('ConcreteDesignator')
        ClearSurfaceDesignator = Proto.ID('ClearSurfaceDesignator')
        PlaceDecalDesignator = Proto.ID('PlaceDecalDesignator')
        ClearDecalDesignator = Proto.ID('ClearDecalDesignator')
        DESIGNATOR_SUFFIX = ""
        MINING_DESIGNATOR = ""
        FORESTRY_DESIGNATOR = ""
        DUMPING_DESIGNATOR = ""
        LEVEL_DESIGNATOR = ""
        PLACE_SURFACE_DESIGNATOR = ""
        CLEAR_SURFACE_DESIGNATOR = ""
        PLACE_DECAL_DESIGNATOR = ""
        CLEAR_DECAL_DESIGNATOR = ""
        def __init__(self):
            pass


    class Technology:
        from Mafi.Core.Prototypes import Proto
        CustomRoutes = Proto.ID('TechnologyCustomRoutes')
        LogisticsZones = Proto.ID('TechnologyLogisticsZones')
        MechPowerAutoBalance = Proto.ID('TechnologyMechPowerAutoBalance')
        CustomSurfaces = Proto.ID('TechnologyCustomSurfaces')
        Recycling = Proto.ID('TechnologyRecycling')
        CropRotation = Proto.ID('TechnologyCropRotation')
        Blueprints = Proto.ID('TechnologyBlueprints')
        CopyTool = Proto.ID('TechnologyCopyTool')
        CutTool = Proto.ID('TechnologyCutTool')
        CloneTool = Proto.ID('TechnologyCloneTool')
        UnityTool = Proto.ID('TechnologyUnityTool')
        PauseTool = Proto.ID('TechnologyPauseTool')
        UpgradeTool = Proto.ID('TechnologyUpgradeTool')
        PlanningTool = Proto.ID('TechnologyPlanningTool')
        TerrainLeveling = Proto.ID('TechnologyTerrainLeveling')
        Trains = Proto.ID('TechnologyTrains')
        def __init__(self):
            pass


    class Products:
        from Mafi.Core.Prototypes import Proto
        VirtualCrudeOil = Proto.ID('Product_VirtualResource_CrudeOil')
        Groundwater = Proto.ID('Product_Virtual_Groundwater')
        from Mafi.Core.Products import ProductProto
        PollutedWater = ProductProto.ID('Product_Virtual_PollutedWater')
        PollutedAir = ProductProto.ID('Product_Virtual_PollutedAir')
        MechanicalPower = ProductProto.ID('Product_Virtual_MechPower')
        Electricity = ProductProto.ID('Product_Virtual_Electricity')
        Computing = ProductProto.ID('Product_Virtual_Computing')
        Upoints = ProductProto.ID('Product_Virtual_Upoints')
        SpaceCrew = ProductProto.ID('Product_Virtual_SpaceCrew')
        SpaceResearchPoints = ProductProto.ID('Product_Virtual_SpaceResearchPoints')
        Diesel = ProductProto.ID('Product_Diesel')
        ConcreteSlab = ProductProto.ID('Product_ConcreteSlab')
        Wood = ProductProto.ID('Product_Wood')
        CleanWater = ProductProto.ID('Product_Water')
        Waste = ProductProto.ID('Product_Waste')
        Biomass = ProductProto.ID('Product_Biomass')
        Recyclables = ProductProto.ID('Product_Recyclables')
        SpaceProbeParts = ProductProto.ID('Product_SpaceProbeParts')
        AsteroidBoosterParts = ProductProto.ID('Product_AsteroidBoosterParts')
        PREFIX = ""
        VIRTUAL_PREFIX = ""
        VIRTUAL_RESOURCE_PREFIX = ""
        GROUND_WATER = ""
        POLLUTED_WATER = ""
        POLLUTED_AIR = ""
        MECHANICAL_POWER = ""
        ELECTRICITY = ""
        COMPUTING = ""
        UPOINTS = ""
        SPACE_CREW = ""
        SPACE_RESEARCH_POINTS = ""
        DIESEL = ""
        CONCRETE_SLAB = ""
        WOOD = ""
        CLEAN_WATER = ""
        WASTE = ""
        BIOMASS = ""
        RECYCLABLES = ""
        SPACE_PROBE_PARTS = ""
        BOOSTER_PARTS = ""
        def __init__(self):
            pass


    class TerrainMaterials:
        from Mafi.Core.Prototypes import Proto
        HardenedRock = Proto.ID('HardenedRock_Terrain')
        Grass = Proto.ID('Grass_Terrain')
        FarmGround = Proto.ID('FarmGround_Terrain')
        Landfill = Proto.ID('Landfill_Terrain')
        Bedrock = Proto.ID('Bedrock_Terrain')
        BEDROCK_SOLID = ""
        def __init__(self):
            pass


    class TerrainTileSurfaces:
        from Mafi.Core.Prototypes import Proto
        DefaultConcrete = Proto.ID('DefaultConcrete_TerrainSurface')
        def __init__(self):
            pass


    class Notifications:
        from Mafi.Core.Notifications import EntityNotificationProto
        UpgradeInProgress = EntityNotificationProto.ID('UpgradeInProgress')
        DowngradeInProgress = EntityNotificationProto.ID('DowngradeInProgress')
        ConstructionPrioritized = EntityNotificationProto.ID('ConstructionPrioritized')
        from Mafi.Core.Notifications import GeneralNotificationProto
        Homeless = GeneralNotificationProto.ID('Homeless')
        LowFoodSupply = GeneralNotificationProto.ID('LowFoodSupply')
        PopsStarving = GeneralNotificationProto.ID('PopsStarving')
        from Mafi.Core.Notifications import GeneralNotificationProto`1
        PopsStarvedToDeath = GeneralNotificationProto`1.ID('PopsStarvedToDeath')
        HomelessLeft = GeneralNotificationProto`1.ID('HomelessLeft')
        from Mafi.Core.Notifications import EntityNotificationProto`1
        CropWillDrySoon = EntityNotificationProto`1.ID('CropWillDrySoon')
        CropLacksMaintenance = EntityNotificationProto`1.ID('CropLacksMaintenance')
        CropDiedNoWater = EntityNotificationProto`1.ID('CropDiedNoWater')
        CropDiedNoFertility = EntityNotificationProto`1.ID('CropDiedNoFertility')
        CropDiedNoMaintenance = EntityNotificationProto`1.ID('CropDiedNoMaintenance')
        CropCouldNotBeStored = EntityNotificationProto`1.ID('CropCouldNotBeStored')
        LowFarmFertility = EntityNotificationProto.ID('LowFarmFertility')
        NoCropToGrow = EntityNotificationProto.ID('NoCropToGrow')
        NotEnoughWorkers = EntityNotificationProto.ID('NotEnoughWorkers')
        EntityIsBoosted = EntityNotificationProto.ID('EntityIsBoosted')
        VehicleIsBroken = EntityNotificationProto.ID('VehicleIsBroken')
        MachineIsBroken = EntityNotificationProto.ID('MachineIsBroken')
        TruckCannotDeliver = EntityNotificationProto`1.ID('TruckCannotDeliver')
        TruckCannotDeliverMixedCargo = EntityNotificationProto.ID('TruckCannotDeliverMixedCargo')
        SortingPlantNoProductSet = EntityNotificationProto.ID('SortingPlantNoProductSet')
        SortingPlantBlockedOutput = EntityNotificationProto.ID('SortingPlantBlockedOutput')
        VehicleGoalUnreachable = EntityNotificationProto.ID('VehicleGoalUnreachable')
        VehicleGoalUnreachableCannotGoUnder = EntityNotificationProto.ID('VehicleGoalUnreachableCannotGoUnder')
        VehicleGoalStruggling = EntityNotificationProto.ID('VehicleGoalStruggling')
        VehicleGoalStrugglingCannotGoUnder = EntityNotificationProto.ID('VehicleGoalStrugglingCannotGoUnder')
        VehicleNoFuel = EntityNotificationProto.ID('VehicleNoFuel')
        EntityCannotBeReached = EntityNotificationProto.ID('EntityCannotBeReached')
        TruckHasNoValidExcavator = EntityNotificationProto.ID('TruckHasNoValidExcavator')
        ExcavatorHasNoValidTruck = EntityNotificationProto.ID('ExcavatorHasNoValidTruck')
        NoTruckAssignedToTreeHarvester = EntityNotificationProto.ID('NoTruckAssignedToTreeHarvester')
        NoTreeSaplingsForPlanter = EntityNotificationProto.ID('NoTreeSaplingsForPlanter')
        NoTreesToHarvest = EntityNotificationProto.ID('NoTreesToHarvest')
        NotEnoughElectricity = GeneralNotificationProto.ID('NotEnoughPower')
        NotEnoughElectricityForEntity = EntityNotificationProto.ID('NotEnoughPowerForEntity')
        NotEnoughComputingForEntity = EntityNotificationProto.ID('NotEnoughComputingForEntity')
        NoResourceToExtract = EntityNotificationProto.ID('NoResourceToExtract')
        ResourceIsLow = EntityNotificationProto`1.ID('ResourceIsLow')
        LowGroundwater = GeneralNotificationProto.ID('LowGroundwater')
        NotEnoughFuelToRefuel = GeneralNotificationProto.ID('NotEnoughFuelToRefuel')
        FuelStationOutOfFuel = EntityNotificationProto.ID('FuelStationOutOfFuel')
        FuelStationNotConnected = EntityNotificationProto.ID('FuelStationNotConnected')
        NoMineDesignInTowerArea = EntityNotificationProto.ID('NoMineDesignInTowerArea')
        NoAvailableMineDesignInTowerArea = EntityNotificationProto.ID('NoAvailableMineDesignInTowerArea')
        NoForestryDesignInTowerArea = EntityNotificationProto.ID('NoForestryDesignInTowerArea')
        NoAvailableForestryDesignInTowerArea = EntityNotificationProto.ID('NoAvailableForestryDesignInTowerArea')
        CannotDeliverFromMineTower = EntityNotificationProto.ID('CannotDeliverFromMineTower')
        AreasWithoutTowers = GeneralNotificationProto.ID('AreasWithoutTowers')
        AreasWithoutForestryTowers = GeneralNotificationProto.ID('AreasWithoutForestryTowers')
        VehicleNoReachableDesignations = EntityNotificationProto.ID('VehicleNoReachableDesignations')
        NoRecipeSelected = EntityNotificationProto.ID('NoRecipeSelected')
        NeedsTransportConnected = EntityNotificationProto`1.ID('NeedsTransportConnected')
        TransportTooLong = EntityNotificationProto.ID('TransportTooLong')
        from Mafi.Core.Notifications import GeneralNotificationProto`1
        ShipCargoLoaded = GeneralNotificationProto`1.ID('ShipCargoLoaded')
        ShipCargoDelivered = GeneralNotificationProto`1.ID('ShipCargoDelivered')
        ShipRepaired = GeneralNotificationProto.ID('ShipRepaired')
        ShipModified = GeneralNotificationProto.ID('ShipModified')
        OceanAccessBlocked = EntityNotificationProto.ID('OceanAccessBlocked')
        OceanAccessPartlyBlocked = EntityNotificationProto.ID('OceanAccessPartlyBlocked')
        from Mafi.Core.Notifications import GeneralNotificationProto`1
        WorldEntityRepaired = GeneralNotificationProto`1.ID('WorldEntityRepaired')
        CargoShipMissingFuel = EntityNotificationProto.ID('CargoShipMissingFuel')
        CargoShipContractLacksUpoints = EntityNotificationProto.ID('CargoShipContractLacksUpoints')
        CargoDepotHasNoShip = EntityNotificationProto.ID('CargoDepotHasNoShip')
        CargoDepotHasNoModule = EntityNotificationProto.ID('CargoDepotHasNoModule')
        NotEnoughUpoints = GeneralNotificationProto.ID('NotEnoughUpoints')
        NotEnoughUpointsForEntity = EntityNotificationProto.ID('NotEnoughUpointsForEntity')
        LabCannotResearchHigherTech = EntityNotificationProto.ID('LabCannotResearchHigherTech')
        LabMissingInputProducts = EntityNotificationProto.ID('LabMissingInputProducts')
        SettlementHasNoFoodModule = EntityNotificationProto.ID('SettlementHasNoFoodModule')
        SettlementIsStarving = EntityNotificationProto.ID('SettlementIsStarving')
        SettlementFullOfLandfill = EntityNotificationProto.ID('SettlementFullOfLandfill')
        NoProductAssignedToEntity = EntityNotificationProto.ID('NoProductAssignedToEntity')
        from Mafi.Core.Notifications import GeneralNotificationProto`1
        NotEnoughMaintenance = GeneralNotificationProto`1.ID('NotEnoughMaintenance')
        CargoDepotModuleNoProductAssigned = EntityNotificationProto.ID('CargoDepotModuleNoProductAssigned')
        CargoDepotModuleContractNotMatching = EntityNotificationProto.ID('CargoDepotModuleContractNotMatching')
        from Mafi.Core.Notifications import GeneralNotificationProto`1
        NewErrorOccurred = GeneralNotificationProto`1.ID('NewErrorOccurred')
        NuclearReactorInMeltdown = EntityNotificationProto.ID('NuclearReactorInMeltdown')
        NuclearReactorLacksMaintenance = EntityNotificationProto.ID('NuclearReactorLacksMaintenance')
        StorageSupplyTooLow = EntityNotificationProto`1.ID('StorageSupplyTooLow')
        StorageSupplyTooHigh = EntityNotificationProto`1.ID('StorageSupplyTooHigh')
        EntityMayCollapseUnevenTerrain = EntityNotificationProto.ID('EntityMayCollapseUnevenTerrain')
        AnimalFarmMissingFood = EntityNotificationProto.ID('AnimalFarmMissingFood')
        AnimalFarmMissingWater = EntityNotificationProto.ID('AnimalFarmMissingWater')
        InvalidImportRoute = EntityNotificationProto.ID('InvalidImportRoute')
        InvalidExportRoute = EntityNotificationProto.ID('InvalidExportRoute')
        LoanPaymentDelayed = GeneralNotificationProto`1.ID('LoanPaymentDelayed')
        LoanPaymentFailed = GeneralNotificationProto`1.ID('LoanPaymentFailed')
        TrainHasInvalidScheduleItem = EntityNotificationProto.ID('TrainHasInvalidScheduleItem')
        TrainHasNoScrapDepot = EntityNotificationProto.ID('TrainHasNoScrapDepot')
        TrainIsIdleWarning = EntityNotificationProto.ID('TrainIsIdleWarning')
        TrainIsIdleError = EntityNotificationProto.ID('TrainIsIdleError')
        TrainCannotFindPath = EntityNotificationProto.ID('TrainCannotFindPath')
        TrainSelfIntersect = EntityNotificationProto.ID('TrainSelfIntersect')
        TrainAlignmentFork = EntityNotificationProto.ID('TrainAlignmentFork')
        TrainDestroyedNoLine = EntityNotificationProto.ID('TrainDestroyedNoLine')
        from Mafi.Core.Notifications import EntityNotificationProto`1
        TrainDestroyed = EntityNotificationProto`1.ID('TrainDestroyed')
        TrainCannotFindPathFromDepot = EntityNotificationProto.ID('TrainCannotFindPathFromDepot')
        TrainHasInvalidScheduleItemFromDepot = EntityNotificationProto.ID('TrainHasInvalidScheduleItemFromDepot')
        TrainNoPowerFromDepot = EntityNotificationProto.ID('TrainNoPowerFromDepot')
        StationHasNoAssignedUnloadModule = EntityNotificationProto.ID('StationHasNoAssignedUnloadModule')
        AsteroidDiscovered = GeneralNotificationProto.ID('AsteroidDiscovered')
        AsteroidArrivedInOrbit = GeneralNotificationProto.ID('AsteroidArrivedInOrbit')
        SpaceStationNoCrewSupplies = GeneralNotificationProto.ID('SpaceStationNoCrewSupplies')
        SpaceStationMaintenanceLow = GeneralNotificationProto.ID('SpaceStationMaintenanceLow')
        SpaceStationDegraded = GeneralNotificationProto.ID('SpaceStationDegraded')
        SpaceStationCrewRotationFailed = GeneralNotificationProto.ID('SpaceStationCrewRotationFailed')
        RoadHasInvalidConnection = EntityNotificationProto.ID('RoadHasInvalidConnection')
        StackerTowerFullNotification = EntityNotificationProto.ID('StackerTowerFullNotification')
        def __init__(self):
            pass


    class PropertyIds:
        VehiclesFuelConsumptionMultiplier = None
        TrucksCapacityMultiplier = None
        TrucksMaintenanceMultiplier = None
        FuelConsumptionDisabled = None
        VehicleLimitBonus = None
        ShipsFuelConsumptionMultiplier = None
        MaintenanceConsumptionMultiplier = None
        UnityProductionMultiplier = None
        SettlementConsumptionMultiplier = None
        HousingCapacityMultiplier = None
        UnityCapacityMultiplier = None
        MiningMultiplier = None
        ResearchStepsMultiplier = None
        ResearchEfficiencyMultiplier = None
        DeconstructionRefundMultiplier = None
        ConstructionCostsMultiplier = None
        QuickActionsUnityCostMultiplier = None
        FoodConsumptionMultiplier = None
        CanWithholdWorkersOnStarvation = None
        BaseHealthMultiplier = None
        BaseHealthDiffEdicts = None
        TradeVolumeMultiplier = None
        ForceRunAllMachinesEnabled = None
        FarmWaterConsumptionMultiplier = None
        FarmYieldMultiplier = None
        TreesGrowthSpeed = None
        RecyclingRatioDiff = None
        SolarPowerMultiplier = None
        DiseaseEffectsMultiplier = None
        LogisticsCanWorkOnLowPower = None
        LogisticsIgnorePower = None
        SlowDownIfBroken = None
        MaintenanceProductionMultiplier = None
        MachineSpeedOnLowPower = None
        MachineSpeedOnLowComputing = None
        RainYieldMultiplier = None
        GroundWaterPumpSpeedWhenDepleted = None
        GroundWaterReplenishWhenLow = None
        ShipsCanUseUnityIfOutOfFuel = None
        VehicleSlowDownOnLowFuel = None
        WorldMinesCanRunWithoutUnity = None
        UnlimitedWorldMines = None
        WorldMinesReserveMultiplier = None
        ContractsProfitMultiplier = None
        ContractsUnityCostMultiplier = None
        WorldMinesEfficiency = None
        DiseaseMortalityMultiplier = None
        WaterPollutionMultiplier = None
        AirPollutionMultiplier = None
        LandfillPollutionMultiplier = None
        VehiclesPollutionMultiplier = None
        TrainsPollutionMultiplier = None
        ShipsPollutionMultiplier = None
        OreSortingEnabled = None
        TrainPowerMultiplier = None
        TrainBrakingMultiplier = None
        TrainSlopeDifficultyMultiplier = None
        TrainsSlowDownOnLowFuel = None
        TrainsFuelConsumptionMultiplier = None
        TrainsCapacityMultiplier = None
        FocusPointsMultiplier = None
        RocketsCapacityMultiplier = None
        def __init__(self):
            pass


    class UpointsStatsCategories:
        from Mafi.Core.Prototypes import Proto
        IslandBuilding = Proto.ID('UpointsCat_IslandBuildings')
        OneTimeAction = Proto.ID('UpointsCat_OneTimeActions')
        Ignore = Proto.ID('UpointsCat_Ignore')
        def __init__(self):
            pass


    class UpointsCategories:
        from Mafi.Core.Prototypes import Proto
        Edict = Proto.ID('UpointsCat_Edict')
        Boost = Proto.ID('UpointsCat_Boost')
        Health = Proto.ID('UpointsCat_Health')
        Starvation = Proto.ID('UpointsCat_Starvation')
        Homeless = Proto.ID('UpointsCat_Homeless')
        SettlementQuality = Proto.ID('UpointsCat_Decorations')
        Rockets = Proto.ID('UpointsCat_Rockets')
        Contract = Proto.ID('UpointsCat_Contract')
        FreeUnity = Proto.ID('UpointsCat_FreeUnity')
        PopsAdoption = Proto.ID('UpointsCat_PopsAdoption')
        QuickTrade = Proto.ID('UpointsCat_QuickTrade')
        QuickBuild = Proto.ID('UpointsCat_QuickBuild')
        QuickRemove = Proto.ID('UpointsCat_QuickRemove')
        QuickRepair = Proto.ID('UpointsCat_QuickRepair')
        QuickRefuel = Proto.ID('UpointsCat_QuickRefuel')
        ContractEstablish = Proto.ID('UpointsCat_ContractEstablish')
        VehicleRecovery = Proto.ID('UpointsCat_VehicleRecovery')
        OtherDecorations = Proto.ID('UpointsCat_OtherDecorations')
        ShipFuel = Proto.ID('UpointsCat_ShipFuel')
        def __init__(self):
            pass


    class HealthPointsCategories:
        from Mafi.Core.Prototypes import Proto
        Base = Proto.ID('HealthPointsCat_Base')
        Edicts = Proto.ID('HealthPointsCat_Edicts')
        LandfillPollution = Proto.ID('HealthPointsCat_LandfillPollution')
        WaterPollution = Proto.ID('HealthPointsCat_WaterPollution')
        AirPollution = Proto.ID('HealthPointsCat_AirPollution')
        AirPollutionVehicles = Proto.ID('HealthPointsCat_AirPollutionVehicles')
        AirPollutionShips = Proto.ID('HealthPointsCat_AirPollutionShips')
        AirPollutionTrains = Proto.ID('HealthPointsCat_AirPollutionTrains')
        Food = Proto.ID('HealthPointsCat_Food')
        Healthcare = Proto.ID('HealthPointsCat_Healthcare')
        WasteInSettlement = Proto.ID('HealthPointsCat_WasteInSettlement')
        Disease = Proto.ID('HealthPointsCat_Disease')
        def __init__(self):
            pass


    class BirthRateCategories:
        from Mafi.Core.Prototypes import Proto
        Base = Proto.ID('BirthRateCategoryCat_Base')
        Starvation = Proto.ID('BirthRateCategoryCat_Starvation')
        Radiation = Proto.ID('BirthRateCategoryCat_Radiation')
        Disease = Proto.ID('BirthRateCategoryCat_Disease')
        Edicts = Proto.ID('BirthRateCategoryCat_Edicts')
        Health = Proto.ID('BirthRateCategoryCat_Health')
        def __init__(self):
            pass


    class PopNeeds:
        from Mafi.Core.Prototypes import Proto
        Food = Proto.ID('FoodNeed')
        PowerNeed = Proto.ID('PowerNeed')
        WaterNeed = Proto.ID('WaterNeed')
        HouseholdGoodsNeed = Proto.ID('HouseholdGoodsNeed')
        HouseholdAppliancesNeed = Proto.ID('HouseholdAppliancesNeed')
        ConsumerElectronicsNeed = Proto.ID('ConsumerElectronicsNeed')
        LuxuryGoodsNeed = Proto.ID('LuxuryGoodsNeed')
        HealthCareNeed = Proto.ID('HealthCareNeed')
        ComputingNeed = Proto.ID('ComputingNeed')
        def __init__(self):
            pass


    class World:
        from Mafi.Core.Entities.Dynamic import DynamicEntityProto
        Fleet = DynamicEntityProto.ID('Fleet')
        def __init__(self):
            pass


    class TrainTracks:
        from Mafi.Core.Entities.Static import StaticEntityProto
        Pillar = StaticEntityProto.ID('TrainTracksPillar')
        def __init__(self):
            pass


    class Transports:
        from Mafi.Core.Ports.Io import IoPortShapeProto
        ShaftPortShape = IoPortShapeProto.ID('IoPortShape_Shaft')
        from Mafi.Core.Entities.Static import StaticEntityProto
        Pillar = StaticEntityProto.ID('TransportsPillar')
        SHAFT_PORT_SHAPE = ""
        def __init__(self):
            pass


    class Messages:
        from Mafi.Core.Prototypes import Proto
        TutorialOnFarming = Proto.ID('TutorialOnFarming')
        TutorialOnFarmFertility = Proto.ID('TutorialOnFarmFertility')
        TutorialOnTreesPlanting = Proto.ID('TutorialOnTreesPlanting')
        TutorialOnTransports = Proto.ID('TutorialOnTransports')
        TutorialTreeHarvesting = Proto.ID('TutorialTreeHarvesting')
        TutorialOnMineTower = Proto.ID('TutorialOnMineTower')
        TutorialOnRetainingWalls = Proto.ID('TutorialOnRetainingWalls')
        TutorialOnDumping = Proto.ID('TutorialOnDumping')
        TutorialOnVehiclesAccessibility = Proto.ID('TutorialOnVehiclesAccessibility')
        TutorialOnCargoShip = Proto.ID('TutorialOnCargoShip')
        TutorialOnAdvancedLogistics = Proto.ID('TutorialOnAdvancedLogistics')
        TutorialOnLogisticsZones = Proto.ID('TutorialOnLogisticsZones')
        TutorialOnMaintenance = Proto.ID('TutorialOnMaintenance')
        TutorialOnPopsAndUnity = Proto.ID('TutorialOnPopsAndUnity')
        TutorialOnCoalPower = Proto.ID('TutorialOnCoalPower')
        TutorialOnWorldEntities = Proto.ID('TutorialOnWorldEntities')
        TutorialOnContracts = Proto.ID('TutorialOnContracts')
        PlanningModeTutorial = Proto.ID('PlanningModeTutorial')
        TutorialOnCopyTool = Proto.ID('TutorialOnCopyTool')
        TutorialOnCutTool = Proto.ID('TutorialOnCutTool')
        TutorialOnPauseTool = Proto.ID('TutorialOnPauseTool')
        TutorialOnCloneTool = Proto.ID('TutorialOnCloneTool')
        TutorialOnUnityTool = Proto.ID('TutorialOnUnityTool')
        TutorialOnHealth = Proto.ID('TutorialOnHealth')
        TutorialOnBidirectional = Proto.ID('TutorialOnBidirectional')
        def __init__(self):
            pass


    class Weather:
        from Mafi.Core.Prototypes import Proto
        Sunny = Proto.ID('SunnyWeather')
        def __init__(self):
            pass


    class SpaceProgram:
        from Mafi.Core.Prototypes import Proto
        SpaceStation = Proto.ID('Space_SpaceStation')
        def __init__(self):
            pass


    class ToolbarCategories:
        from Mafi.Core.Prototypes import Proto
        Surfaces = Proto.ID('surfaceCategory')
        Terraforming = Proto.ID('terraformingCategory')
        Forestry = Proto.ID('forestryCategory')
        def __init__(self):
            pass


class Tr:
    Enabled = None
    Disabled = None
    Undefined = None
    Unity = None
    Unity__Tooltip = None
    Bonus = None
    MwSec__Unit = None
    NumberOfDays = None
    NumberOfMonths = None
    Months = None
    NumberOfYears = None
    OptionValMeters = None
    NumberOfTiles = None
    NumberOfKilometersPerHour = None
    NumberOfSeconds_Short = None
    ProductType__Countable = None
    ProductType__Loose = None
    ProductType__Fluid = None
    ProductType__Molten = None
    Km = None
    Kph = None
    Kn = None
    Kw = None
    Tons = None
    OptionValStandard = None
    OptionValIncreased = None
    OptionValReduced = None
    Menu__Discord = None
    COIHub = None
    MailingList = None
    Suggestions = None
    ReportIssue = None
    Credits = None
    SelectOption = None
    NoOptions = None
    Error__Title = None
    Error__View = None
    Error__Copy = None
    NewFolderTitlePlaceholder = None
    NewBlueprintTitlePlaceholder = None
    DescriptionPlaceholder = None
    OpenCoIHub = None
    NewGameWizard__MapSelection = None
    NewGameWizard__Difficulty = None
    NewGameWizard__Mechanics = None
    NewGameWizard__Customization = None
    NewGameWizard__GameName = None
    NewGameWizard__GameName__InUse = None
    NewGameWizard__GameName__FailedToWrite = None
    NewGameWizard__GameName__InvalidChars = None
    NewGameWizard__Launch = None
    NewGameWizard__LaunchSandbox = None
    MapInvalid = None
    NewGameWizard__Title = None
    NewSandboxWizard__Title = None
    Option_Unlimited = None
    Tutorials__Title = None
    Tutorials__Description = None
    ChangeHistory__Title = None
    ChangeHistory__EmptyLabel = None
    ChangeHistory__ConfirmTitle = None
    ChangeHistory__ConfirmPrompt = None
    DateYear__Label = None
    DateYearsRange__Label = None
    DifficultySettingsSaved = None
    StartingLocationDifficulty_Easy = None
    StartingLocationDifficulty_Medium = None
    StartingLocationDifficulty_Hard = None
    StartingLocationDifficulty_Insane = None
    StartingLocationDifficulty__EasyTooltip = None
    StartingLocationDifficulty__MediumTooltip = None
    StartingLocationDifficulty__HardTooltip = None
    StartingLocationDifficulty__InsaneTooltip = None
    DifficultyFood__Easy = None
    DifficultyFood__Normal = None
    DifficultyFood__Hard = None
    DifficultyConstruction__Easy = None
    DifficultyConstruction__Normal = None
    DifficultyConstruction__Hard = None
    DifficultyFuel__Easy = None
    DifficultyFuel__Normal = None
    DifficultyFuel__Hard = None
    DifficultyMaintenance__Easy = None
    DifficultyMaintenance__Normal = None
    DifficultyMaintenance__Hard = None
    DifficultyMining__Easy = None
    DifficultyMining__Normal = None
    DifficultyMining__Hard = None
    DifficultyResearch__Easy = None
    DifficultyResearch__Normal = None
    DifficultyResearch__Hard = None
    DifficultyGrowth__Easy = None
    DifficultyGrowth__Normal = None
    DifficultyGrowth__Hard = None
    DifficultyRainfall__Easy = None
    DifficultyRainfall__Normal = None
    DifficultyRainfall__Hard = None
    DifficultyContracts__Easy = None
    DifficultyContracts__Normal = None
    DifficultyContracts__Hard = None
    DifficultyDisease__Easy = None
    DifficultyDisease__Normal = None
    DifficultyDisease__Hard = None
    DifficultyPollution__Easy = None
    DifficultyPollution__Normal = None
    DifficultyPollution__Hard = None
    DifficultyUnity__Easy = None
    DifficultyUnity__Normal = None
    DifficultyUnity__Hard = None
    Menu__Continue = None
    Menu__DifficultySettings = None
    Menu__NewGame = None
    Menu__Save = None
    Menu__Load = None
    Menu__OpenSettings = None
    Menu__MapEditor = None
    QuitGame = None
    QuitGame__ConfirmationQuestion = None
    ExitToMainMenu = None
    Version = None
    Difficulty = None
    Sandbox = None
    Save_Title = None
    SaveNew = None
    Save_Action = None
    SaveInProgress = None
    LoadInProgress = None
    Load_Title = None
    Load_Action = None
    OverwriteSave__Action = None
    Load_ModCongifRequiredTooltip = None
    ManageMods__Action = None
    ManageMods__Tooltip = None
    ModsScreen_Title__Select = None
    ModsScreen_Title__ManageForSave = None
    ModsScreen_Title__ViewForSave = None
    ModsScreen_ModsTitle = None
    ModsScreen_DlcsTitle = None
    ModsScreen_NoModsFound = None
    ModsScreen_ThankYou = None
    ModsScreen_LoadingContent = None
    ModsScreen_InvalidMods = None
    ModsScreen_ApplyChangesLoadGame = None
    ModsScreen_CannotContinueDueToErrors = None
    ModsScreen_ConfirmLoadWithNewMods = None
    ModsScreen_RemovalWarning = None
    ModsScreen_SafetyWarning = None
    ModsScreen_SafetyWarning__Tooltip = None
    ModsScreenMsg_MissingContent__Title = None
    ModsScreenMsg_MissingContent__Text = None
    ModsScreenMsg_FailedMods__Title = None
    ModsScreenMsg_FailedMods__Text = None
    ModsScreenMsg_ConfirmationRequired__Title = None
    ModsScreenMsg_ConfirmationRequired__Text = None
    ModsScreenMsg_CircularDependency__Title = None
    ModsScreenMsg_CircularDependency__Text = None
    ModTile_ViewOnSteamTooltip = None
    ModTile_ViewModDetailsTooltip = None
    ModTile_SeeErrors = None
    ModDependency_VersionTooOld = None
    ModDependency_Missing = None
    ModDependency_CannotAdd = None
    ModStatus_DlcNotInstalled = None
    ModStatus_ModMissing = None
    ModStatus_NotInstalled = None
    ModStatus_FailedToLoad = None
    ModTooltip_CircularDependencyDetected = None
    ModTooltip_DependencyMissingOrFailed = None
    ModTooltip_CannotAddToExistingSave = None
    ModTooltip_CannotRemoveFromExistingSave = None
    ModTooltip_MissingItemTooltip = None
    ModTooltip_CannotDisableWhileRunning = None
    ModTooltip_UsedInSaveFile = None
    ModTooltip_GameTooOld = None
    ModTooltip_GameTooNew = None
    ModErrorWindow_Intro = None
    ModInfo_Id = None
    ModInfo_Authors = None
    ModInfo_DependenciesTitle = None
    ModInfo_OptionalDependencies = None
    ModInfo_MinGameVersion = None
    ModInfo_LastVerifiedVersion = None
    ModInfo_Links = None
    ModInfo_Description = None
    ModInfo_NoDescription = None
    ModInfo_Properties = None
    ModInfo_Changelog = None
    ModsSummary_Hint_View = None
    ModsSummary_Hint_Manage = None
    ModsSummary_ModsUsed = None
    ModsSummary_DlcsUsed = None
    ModsSummary_DlcsCanAdd = None
    ModInvalid_ManifestNotFound = None
    ModInvalid_ManifestNotFound__Tooltip = None
    ModsInvalid_ManifestLoadFailed = None
    ModInvalid_DuplicateId = None
    ModInvalid_DuplicateId__Tooltip = None
    DeleteSave__Confirm = None
    DeleteSave__SuccessMessage = None
    DeleteSave__FailMessage = None
    OverwriteSave__ConfirmPrompt = None
    Save__SuccessMessage = None
    Save__FailureMessage = None
    LoadDisabled__Corrupted = None
    LoadDisabled__Error = None
    LoadFailed = None
    LoadDisabled__NotDemo = None
    LoadDisabled__ModsMissing = None
    Game__Title = None
    Save__Title = None
    Save__GameVersion = None
    Save__GameVersionCurrent = None
    Saved__Detail = None
    GateTime__Detail = None
    Map = None
    Research__Detail = None
    Launches__Detail = None
    UnsortedSaves__Title = None
    SaveName__Label = None
    CannotQuit_SaveInProgress = None
    GameSaveLoad__VersionTooHigh = None
    GameSaveLoad__VersionTooLow = None
    GameInitFail = None
    GameInitFail_CorruptedSave = None
    GameInitFail__ModLikely = None
    GameInitFail__ModFailed = None
    GameInitFail__OutOrMemory = None
    GameSaveLoad__MissingFile = None
    GameSaveLoad__SaveNotFinishedButSaved = None
    GameSaveLoad__CannotSaveFile = None
    GameSaveLoad__ChecksumFail = None
    GameSaveLoad__CannotSaveFile_Crash = None
    GameSaveLoad__CannotLoadFile = None
    GameSaveLoad__CannotLoadChecksumFail = None
    GameSaveLoad__SwitchSteamVersion = None
    RelativeTime_Seconds = None
    RelativeTime_Minutes = None
    RelativeTime_Hours = None
    RelativeTime_Days = None
    GameOver__Title = None
    GameOver__Message = None
    Settings_Title = None
    VideoSettings_Title = None
    AudioSettings_Title = None
    ControlsSettings_Title = None
    MiscellaneousSettings_Title = None
    PatchNotes = None
    PatchNotes__New = None
    ScreenSetting_Title = None
    ActiveDisplay_Setting = None
    ActiveDisplay_ConfirmationDialog__Title = None
    ActiveDisplay_ConfirmationDialog__Question = None
    ActiveDisplay_ConfirmationDialog__AutoRevert = None
    ActiveDisplay_ConfirmationDialog__Confirm__Action = None
    Resolution = None
    BestEffortLocalized = None
    AccessibilitySetting__Title = None
    AccessibilitySetting__Flashes = None
    WindowMode__Title = None
    WindowMode__Option_Fullscreen = None
    WindowMode__Option_Borderless = None
    WindowMode__Option_Windowed = None
    UiSettings_Title = None
    UiSettings_DisableTransparency = None
    UiSettings_LargeText = None
    CameraSettings__Title = None
    CameraSettings__Fov = None
    CameraSettings_EdgeScrolling = None
    CameraSettings_EdgeScrolling__Tooltip = None
    CameraSettings_ConfineMouseCursor = None
    CameraSettings_ConfineMouseCursor__Tooltip = None
    Scale = None
    Language = None
    RenderingSetting_Title = None
    RenderingSettingPreset_Label = None
    RenderingQuality__ExtremelyHigh = None
    RenderingQuality__VeryHigh = None
    RenderingQuality__High = None
    RenderingQuality__Medium = None
    RenderingQuality__Low = None
    RenderingQuality__On = None
    RenderingQuality__Off = None
    RenderingQuality__Standard = None
    RenderingSetting_NotSupported = None
    Autosave__Interval = None
    Autosave__Interval_Minutes = None
    Autosave__MaxCount = None
    Autosave__MaxCountTooltip = None
    ErrorReporting__Title = None
    ErrorReporting__Tooltip = None
    TutorialReset__Title = None
    TutorialReset__Action = None
    TutorialReset__ResetDone = None
    TutorialReset__Tooltip = None
    Tutorial__WatchOnine = None
    AudioEffectsVolume__Master = None
    AudioEffectsVolume__Music = None
    AudioEffectsVolume__EffectsGroup = None
    AudioEffectsVolume__UI = None
    AudioEffectsVolume__Ambient = None
    AudioEffectsVolume__Entities = None
    AudioSettings_Notifications = None
    AudioSettings_MuteAlerts = None
    AudioSettings_MuteAlertsCritical = None
    AudioSettings_MuteNewMessages = None
    Off_Option = None
    RestartRequiredSuffix = None
    FileSize_Title = None
    Paused = None
    RenameTooltip = None
    MessageCenter__Title = None
    MessageCenter__MessagesTitle = None
    RecipesBook__Title = None
    RecipesBook__OpenHint = None
    Codex__BuildMachineHint = None
    OpenStats = None
    Codex__Entries = None
    Codex__ExactTimeToggle = None
    Codex__ExactTimeTooltip = None
    Codex__StructuresTitle = None
    Recipes = None
    Recipes__Tooltip = None
    Recipes__SetAction = None
    NoRecipesAssigned = None
    ShowPerDuration = None
    BoostMachine__Enable = None
    BoostMachine__Disable = None
    BoostMachine__Tooltip = None
    EntityWorkersRequiredTooltip = None
    EntityWorkersNeededTooltip = None
    EntityWorkersNeededTooltip_NotAssigned = None
    EntityWorkersNeededTooltip_Assigned = None
    EntityMonthlyUnitTooltip = None
    EntityMonthlyUnitTooltip__NotConsuming = None
    EntityMonthlyUnitTooltip__NotEnough = None
    EntityMonthlyUnitTooltip__Consuming = None
    EntityElectricityConsumptionTooltip = None
    EntityElectricityConsumptionTooltip__NotConsuming = None
    EntityElectricityConsumptionTooltip__NotEnough = None
    EntityElectricityConsumptionTooltip__Consuming = None
    EntityElectricityConsumptionPerUnitTooltip = None
    EntityElectricityProductionTooltip = None
    PowerGenerationPriorityTooltip = None
    EntityComputingConsumptionTooltip = None
    EntityComputingConsumptionTooltip__NotConsuming = None
    EntityComputingConsumptionTooltip__NotEnough = None
    EntityComputingConsumptionTooltip__Consuming = None
    EntityComputingProductionTooltip = None
    EntityToggleNavigationOverlay = None
    EntityToggleNavigationOverlay__Tooltip = None
    EntityToggleTrainsNavigationOverlay__Tooltip = None
    EntityRepair__Tooltip = None
    EntityRepair_QuickRepair = None
    EntityRepair__FastAccessTooltip = None
    EntityBreakdownChance = None
    EntityBreakdownChance_Threshold = None
    Options = None
    CargoTitle = None
    ConstructionCost = None
    AssignedTrucks__Title = None
    AssignedExcavators__Title = None
    AssignedTreePlanters__Title = None
    AssignedTreeHarvesters__Title = None
    AssignedTrucks__MineTower_Tooltip = None
    AssignedExcavators__MineTower_Title = None
    AssignedTreePlanters__ForestryTower_Title = None
    AssignedTreeHarvesters__ForestryTower_Title = None
    AssignedTrucks__FuelStation_Tooltip = None
    AssignedTrucks__TreeHarvester_Tooltip = None
    AssignedTrucks__Building_Tooltip = None
    AssignedTrucksEnforce__Title = None
    AssignedTrucksEnforce__Tooltip = None
    AssigningFromZone = None
    ReleasingToZone = None
    AssignOverride__part1 = None
    AssignOverride__part2Assign = None
    AssignOverride__part2Release = None
    NoVehiclesAssigned = None
    SupportedTrucks__Title = None
    SupportedTrucks__Tooltip = None
    AssignVehicleBtn__Tooltip = None
    AssignVehicleBtn__NotAvailable = None
    ScrapVehicle__Action = None
    ScrapVehicle__Tooltip = None
    ScrapVehicle__InProgress = None
    RecoverVehicle__Action = None
    RecoverVehicle__Tooltip = None
    Action__Delete = None
    Action__Duplicate = None
    Action__Build = None
    Action__BringToView = None
    Tooltip__OpenInspector = None
    Action__Confirm = None
    Action__RightClickToRemoveTooltop = None
    Action__ToRemoveTooltip = None
    FuelTank_Title = None
    FuelTank_ReserveTooltip = None
    ReplaceVehicle__MainTooltip = None
    ReplaceVehicle__OnItsWay = None
    ReplaceVehicle__WaitingForReplace = None
    ReplaceVehicle__NoVehicleSelected = None
    ReplaceVehicle__NoDepot = None
    SelectVehicle_Title = None
    VehiclesReplacer__Title = None
    VehiclesReplacer__DepotAny = None
    VehiclesReplacerFilter__Unassigned = None
    VehiclesReplacerFilter__All = None
    VehiclesReplacer__ActiveTasks = None
    VehiclesReplacer__CompletedTasks = None
    VehiclesReplacer__NewTaskTitle = None
    VehiclesReplacer__StartAction = None
    VehiclesReplacerTask__Limit = None
    VehiclesReplacerTask__ReplacedLabel = None
    VehiclesReplacerTaskState__Completed = None
    VehiclesReplacerTaskState__Cancelled = None
    VehiclesReplacerTaskState__Waiting = None
    VehiclesReplacerTaskState__InProgress = None
    VehiclesReplacerError__NoVehiclesSelected = None
    VehiclesReplacerError__VehiclesAreSame = None
    VehiclesReplacerError__DepotNoSupport = None
    VehicleMinClearanceTooltip = None
    ThisVehicleCannotDriveUnderTransports = None
    EntityCannotBeReachedDesc = None
    VehicleRole__TreeHarvesting = None
    NoVehicleDepotAvailable = None
    AllVehicles__Title = None
    PartialTrucksToggle = None
    PartialTrucksToggle__Tooltip = None
    ConstructionPrio__Label = None
    ConstructionPrio__Tooltip = None
    ConstructionHighPriority_Display__Shorthand = None
    DeconstructionPrio__Label = None
    DeconstructionPrio__Tooltip = None
    NumberOfIdleVehicles = None
    DeliveriesCompleted = None
    NoDataYet = None
    Vehicles = None
    VehiclesLimit__Tooltip = None
    Vehicles_InUse = None
    VehiclesAssignments__Title = None
    VehiclesZonesAssignments__Title = None
    TrucksStats__Title = None
    TrucksStats__OptionGeneral = None
    TrucksStats__OptionGeneralTooltip = None
    TrucksStats__OptionMining = None
    TrucksStats__OptionMiningTooltip = None
    TrucksStats__OptionRefueling = None
    TrucksStats__RefuelingSelf = None
    TrucksStats__OptionRefuelingTooltip = None
    TrucksStats__LifetimeStats = None
    TrucksStats__LifetimeDistance = None
    TrucksStats__LifetimeCargo = None
    TrucksStats__LifetimeMined = None
    TrucksStats__LifetimeTreesCut = None
    TrucksStats__LifetimeTreesPlanted = None
    VehicleJob__InvalidState = None
    VehicleJob__SearchingForDesignation = None
    VehicleJob__DrivingToGoal = None
    VehicleJob__Loading = None
    VehicleJob__Unloading = None
    VehicleJob__InQueue = None
    VehicleJob__ProcessingSurface = None
    VehicleGoal__TerrainPosition = None
    VehicleGoal__SurfaceModification = None
    VehicleJob__Navigating = None
    VehicleJob__NavigatingToVia = None
    RuinsRecycle__Action = None
    RuinsRecycleFormatted__Tooltip = None
    ManagedArea__Info = None
    ManagedArea__EditAction = None
    ManagedDesignation__EditAction = None
    Trees__CutAfter = None
    Trees__NoCut = None
    Trees__HarvestingOptions = None
    Trees__HarvestingOptionsTooltip = None
    PerTree = None
    SetArea__EditAction = None
    LogisticsZoneDelete_Confirmation = None
    LogisticsZoneName = None
    LogisticsZone__Default = None
    LogisticsZone__All = None
    LogisticsZoneSelected = None
    NoAreaSet = None
    ConstructionsZones__Title = None
    ConstructionsZones__Tooltip = None
    LogisticsZoneLimitReached = None
    LogisticsZoneConfig__Name = None
    LogisticsZoneConfig__Color = None
    LogisticsZones__Title = None
    LogisticsZones__TitleShort = None
    LogisticsZoneSelector__VehicleDepotTooltip = None
    LogisticsZoneSelector__VehicleTooltip = None
    LogisticsZonesFromAssignment__Tooltip = None
    Skip = None
    Collect = None
    Cancel = None
    Inspect = None
    Dismiss = None
    DismissAll = None
    Close = None
    Continue = None
    Upgrade = None
    ReplaceAction = None
    Pause = None
    GoBack = None
    GoNext = None
    Repair = None
    Empty = None
    Total = None
    Orders = None
    None = None
    All = None
    InputsTitle = None
    OutputsTitle = None
    Provides = None
    Accepts = None
    IoLabel__IN = None
    IoLabel__OUT = None
    Search = None
    NothingFoundFor = None
    NothingFound = None
    SearchResultFor = None
    QuantityPerMonth = None
    QuantityPerMonthShort = None
    OneMonth = None
    AmountOfWorkers = None
    Workers__Needed = None
    Workers__Available = None
    Workers = None
    AmountOfPops = None
    PopsCannotWorkTitle = None
    PopsCannotWork__Starving = None
    PopsCannotWork__Quarantine = None
    ProductSelectorTitle = None
    ReserveStatus = None
    QuickBuild__Action = None
    QuickBuild__NotAllowed = None
    QuickBuild__NotEnoughProduct = None
    QuickRemove__Action = None
    EntityPropertyModifiers = None
    EntityPropertyNoModifiers = None
    EntityPropertyBase = None
    EntityPropertyModifier_Research__Tooltip = None
    EntityPropertyModifier_Focuses__Tooltip = None
    EntityPropertyModifier_Edicts__Tooltip = None
    EntityPropertyModifier_PopulationSmall__Tooltip = None
    EntityPropertyModifier_SpaceStation__Tooltip = None
    EntityPropertyModifier_Others__Tooltip = None
    GlobalMaintenanceDemand__Title = None
    MaxGlobalMaintenanceRequired = None
    GlobalMaintenanceStatus__Tooltip = None
    GlobalDemand = None
    LastDelta = None
    MonthDurationLegend = None
    MaintenanceRequired = None
    VehiclesMaintenance = None
    QuickBuild__Tooltip = None
    NotEnoughProducts = None
    QuickRemove__Tooltip = None
    QuickRemoveAfterReplace__Tooltip = None
    QuickReplaceAction__Tooltip = None
    RefundAfterReplace__Label = None
    RefundAfterReplace__Tooltip = None
    ConstructionState__WaitingForRemoval = None
    ConstructionState__WaitingForDelivery = None
    ConstructionState__Paused = None
    ConstructionState__Ready = None
    ConstructionState__InProgress = None
    ConstructionState__Repairing = None
    ConstrType_Deconstructing = None
    ConstrType_DeconstructionPaused = None
    ConstrType_PreparingUpgrade = None
    ConstrType_Upgrading = None
    Construction__Cancel_Tooltip = None
    Construction__Resume_Tooltip = None
    Construction__Pause_Tooltip = None
    Deconstruction__Cancel_Tooltip = None
    Deconstruction__Resume_Tooltip = None
    Deconstruction__Pause_Tooltip = None
    Construction__StopCargo_Tooltip = None
    Construction__ResumeCargo_Tooltip = None
    EntityStatus = None
    EntityStatus__Broken = None
    EntityStatus__NeedsRepairs = None
    EntityStatus__ResourceDepleted = None
    EntityStatus__LowPower = None
    EntityStatus__NoComputing = None
    EntityStatus__NeedsFuel = None
    EntityStatus__Idle = None
    EntityStatus__ResearchTooAdvanced = None
    EntityStatus__NoSpaceResearchPoints = None
    EntityStatus__MissingInput = None
    EntityStatus__WaitingForProducts = None
    EntityStatus__WaitingForProductsTooltip = None
    EntityStatus__InvalidPlacement = None
    EntityStatus__MissingCoolant = None
    EntityStatus__FullOutput = None
    EntityStatus__NoRecipe = None
    EntityStatus__Working = None
    EntityStatus__Paused = None
    EntityStatus__NoWorkers = None
    EntityStatus__WorkingPartially = None
    EntityStatus__NotConnected = None
    EntityStatus__Clearing = None
    EntityStatus__FullStorage = None
    EntityStatus__NoUnity = None
    EntityStatus__NoJobs = None
    EntityStatus__NoShaft = None
    EntityStatus__PartiallyStuck = None
    EntityStatus__CannotWork = None
    LabStatus__MissingInput = None
    EntityStatus__Datacenter_NoServers = None
    EntityStatus__Farm_NoCrop = None
    EntityStatus__Farm_NoWater = None
    EntityStatus__Farm_Growing = None
    EntityStatus__AnimalFarm_NoAnimals = None
    EntityStatus__AnimalFarm_NoFood = None
    EntityStatus__Ship_Docked = None
    EntityStatus__Ship_Exploring = None
    EntityStatus__Ship_InBattle = None
    EntityStatus__Ship_Moving = None
    EntityStatus__Ship_NoOrders = None
    EntityStatus__Damaged = None
    EntityStatus__UnderRepair = None
    EntityStatus___NuclearReactor_Overheated = None
    EntityStatus___NuclearReactor_FuelLow = None
    EntityStatus___ProductRemoval = None
    AdditionError__NoDeposit = None
    AdditionError__ThinDeposit = None
    AdditionError__HasDeposit = None
    AdditionError__Unique = None
    AdditionError__CollisionWith = None
    AdditionError__NeedsOcean = None
    AdditionError__SomethingInWay = None
    AdditionError__OutsideOfMap = None
    AdditionError__OceanNotAllowed = None
    AdditionError__OceanTooClose = None
    AdditionError__OceanBlocked = None
    AdditionError__OceanBlockedBy = None
    AdditionError__OceanBlockedByTerrain = None
    AdditionError__NotInSlot = None
    AdditionError__BridgeTooLow = None
    AdditionError__NotFarmable = None
    AdditionError__NotStable = None
    AdditionError__NotFertile = None
    AdditionError__NotASurface = None
    AdditionError__TooCloseToOtherTree = None
    AdditionError__InvalidHeight = None
    AdditionError__DesignationOverlap = None
    AdditionError__TerrainTooLow = None
    AdditionError__TerrainTooHigh = None
    AdditionWarning__HighLift = None
    TrAdditionError__Blocked = None
    TrAdditionError__InvalidTransport = None
    TrAdditionError__InvalidConnection = None
    TrAdditionError__BeingDestroyed = None
    TrCutError__ConstructionAlreadyStarted = None
    TrAdditionError__NoMiniZipper = None
    TrAdditionError__NotFlat = None
    TrAdditionError__IncompatiblePortAtStart = None
    TrAdditionError__IncompatiblePortAtEnd = None
    TrAdditionError__TypesNoMatch = None
    TrAdditionError__Loop = None
    TrAdditionError__SelfColliding = None
    TrAdditionError__IncompatibleDirection = None
    TrAdditionError__CannotReverse = None
    TrAdditionError__NoPillars = None
    TrAdditionError__TerrainCollision = None
    TrAdditionError__TooCloseToOtherMiniZipper = None
    TrAdditionError__InvalidTransportCut = None
    TransportTooLong__HowToResolve = None
    TrkAdditionError__NoPath = None
    TrkAdditionError__NoPillars = None
    TrkAdditionError__AlreadyExists = None
    TrkDirectionChangeError__TrackIsBlocked = None
    TrkAdditionError__Reserved = None
    TrkElectrificationError__TrackIsBlocked = None
    TrainStationAdditionError__InvalidStationGroup = None
    RemovalError__HousingHasModuleAttached = None
    RemovalError__NotContiguous = None
    RemovalError__ScrapItFirst = None
    RemovalError__ShipHasCargo = None
    RemovalError__DepotMovingCargo = None
    RemovalError__HasProductsStored = None
    RemovalError__RemoveModulesFirst = None
    RemovalError__RemoveTrainsFirst = None
    RemovalError__StopTrainReplacement = None
    RemovalError__CannotRemove = None
    RemovalError__HasShipAssigned = None
    RemovalError__FarmHasAnimals = None
    RemovalError__TrainOnTrack = None
    RemovalError__ExtensionAttached = None
    RemovalError__RoadOccupied = None
    RemovalError__BridgeOccupied = None
    ShipCantVisit__OnWay = None
    ShipCantVisit__Damaged = None
    ShipCantVisit__BeingModified = None
    ShipCantVisit__BeingRepaired = None
    ShipCantVisit__NoAccess = None
    ShipCantVisit__Ok = None
    Option_AllowRefuelInEntity = None
    FuelAvailable = None
    FuelStation_NoPipe = None
    Beacon__Notice = None
    Beacon__Status = None
    Beacon__NoMoreRefugees = None
    StoredProduct__Title = None
    StoredProduct__Tooltip = None
    StoredProduct__WorldMapTooltip = None
    StoredProduct__NothingStored = None
    StoredProduct__ImportSliderLabel = None
    StoredProduct__ExportSliderLabel = None
    StoredProduct__OutputToBeltSliderLabel = None
    StoredProduct__InputFromBeltSliderLabel = None
    StoredProduct__Clear_Tooltip = None
    StoredProduct__ClearActive_Tooltip = None
    StoredProduct__KeepFull = None
    StoredProduct__KeepEmpty = None
    RetiredWaste__Tooltip = None
    RetiredWaste__NextDisposal = None
    RetiredWaste__NoProduct = None
    OutputThisProductOnly = None
    OutputThisProductOnly__Tooltip = None
    StoredHeat__Title = None
    StoredHeat__Tooltip = None
    StoredHeat__NoProductSelected = None
    ThermalStorage__ChargingRecipeTitle = None
    ThermalStorage__DischargingRecipeTitle = None
    Research = None
    ResearchProgress = None
    ResearchFinished = None
    CurrentResearch = None
    ResearchSpeed__Title = None
    StartNewResearch_Action = None
    OpenResearch_Action = None
    NoLabAvailable = None
    NoResearchSelected = None
    Unlocks = None
    Locked = None
    LockedByParent = None
    Requires = None
    Required = None
    Recipes__New = None
    StartResearch_Action = None
    ResearchQueue__Title = None
    ResearchQueue__Add = None
    ResearchQueue__Remove = None
    ResearchQueue__Status = None
    SpaceResearchPoints_LabTooltip = None
    ResearchLockedBySpaceStation = None
    ResearchCost_Tooltip = None
    Research_AccBonus = None
    Research_IncrementalTooltip = None
    ResearchPoints_InRecipe = None
    ResearchPoints_Needed = None
    WaterStored = None
    RainHarvester_Collecting = None
    RainHarvester_WaitingForRain = None
    RainHarvester_FullStorage = None
    Designation__Mining = None
    Designation__Dumping = None
    Designation__Leveling = None
    Designation__TreeHarvesting = None
    Designation__Forestry = None
    DesignationError__Invalid = None
    DesignationWarning__NoTower = None
    DesignationWarning__NoForestryTower = None
    DesignationWarning__CannotStartMining = None
    DesignationWarning__CannotStartDumping = None
    DesignationWarning__CannotStartLeveling = None
    DesignationWarning__CannotStartForestry = None
    DesignationWarning__CannotPlaceDecal = None
    Overlays__Title = None
    Overlays__Designations = None
    Overlays__Grid = None
    Overlays__Trees = None
    Overlays__Resources_Title = None
    Overlays__Resources_Tooltip = None
    WorldLocation__Home_Title = None
    WorldLocation__Home_Desc = None
    WorldLocation__Unknown_Title = None
    WorldLocation__Unknown_Desc = None
    WorldLocation__UnknownOnWay_Desc = None
    WorldLocation__Explored_Title = None
    WorldLocation__Explored_Desc = None
    WorldLocation__WithEnemy_Title = None
    WorldLocation__WithEnemyOnWay_Desc = None
    WorldLocation__WithEnemy_Desc = None
    WorldLocation__BeingExplored_Title = None
    WorldLocation_Orders = None
    WorldLocation_Orders__GoHome = None
    WorldLocation_Orders__Battle = None
    WorldLocation_Orders__Visit = None
    WorldLocation_Orders__LoadCargo = None
    WorldLocation_Orders__DeliverCargo = None
    WorldLocation_EnemyFound = None
    WorldLocation_StructureFound = None
    ExplorationResult__Title = None
    ExplorationResult__Nothing = None
    ExplorationResult__Loot = None
    ExplorationResult__Entity = None
    ConfirmGreatNews = None
    NewDiscovery = None
    StartRepairs__Tooltip = None
    NeedsRepairsDesc__Parametrized = None
    WorldMine_ReserveTooltip = None
    WorldMine_ReserveEstimate = None
    WorldMine_ReserveEstimate__Tooltip = None
    WorldMine_ReducedOutput = None
    WorldMine_ReducedOutputShort = None
    WorldMineInfo__NeedsRepair = None
    WorldMineInfo__ProvidesResources = None
    UpgradeInProgress = None
    WorldMap = None
    WorldSettlement_NeutralDesc = None
    TradeOffers = None
    TradeOffers__Tooltip = None
    Trade_PriceIncreased = None
    BuyPrefix = None
    SellPrefix = None
    ImportPrefix = None
    ExportPrefix = None
    Trade__Action = None
    Trade__SoldOut = None
    TradeStatus__CantAfford = None
    TradeStatus__NoUnity = None
    TradeStatus__SoldOut = None
    TradeStatus__NoTradeDock = None
    TradeStatus__TradeDockNotOperational = None
    TradeStatus__NoSpaceInFarm = None
    TradeStatus__Info = None
    TradeStatus__Info_Animal = None
    TradeStatus__Info_CargoShip = None
    TradeOfferDelivered = None
    TradeOfferDelivered__Animal = None
    TradeOfferDelivered__CargoShip = None
    Status_LowReputation = None
    ShipCantVisit__NoCrew = None
    ShipCantVisit__NoFuel = None
    ShipCantVisit__TooFar = None
    WorldLocation_Orders__Explore = None
    WorldMineProductionLvl__Title = None
    WorldMineProductionLvl__Tooltip = None
    AdoptPops__Title = None
    AdoptPops__Tooltip = None
    AdoptPopsAction = None
    Adopt__Action = None
    ReputationIncrease__Title = None
    ReputationIncrease__Tooltip = None
    ReputationIncrease__DonateAction = None
    ReputationIncrease__DonateAlreadyStarted = None
    DonationInProgress = None
    ViewLoans = None
    ViewContracts = None
    SettlementTitleWithReputation = None
    ReputationIncreaseTitle = None
    PopsToAdoptNotAvailable = None
    ToggleDirection = None
    RemoveProducts = None
    CannotRemoveProduct = None
    RemoveProducts__Stop = None
    RemoveProducts__Tooltip = None
    TransportedProducts = None
    ShipyardFullMessage = None
    ShipyardFullMessage__Tooltip = None
    ShipyardKeepEmpty__Tooltip = None
    ShipyardCargo__Tooltip = None
    ShipyardRecoverOceanAccess__Title = None
    ShipyardRecoverOceanAccess__Tooltip = None
    ShipyardRecoverOceanAccess__BtnTooltip = None
    ShipyardRecoverOceanAccess__Button = None
    ShipyardNeedsRepairs = None
    ShipyardMakePrimary = None
    ShipyardMakePrimary__Tooltip = None
    ShipyardMakePrimary__TooltipInProgress = None
    ShipyardKeepEmpty = None
    ShipLoading__Title = None
    ShipLoading__Desc = None
    ShipLoading__Action = None
    ShipLoading__NotStarted = None
    ShipLoading__Stop = None
    ShipLoading__Done = None
    ShipLoading__CancelProject = None
    FuelForShip__Title = None
    FuelForShip__Tooltip = None
    AddNewShipPart = None
    ReplaceShipPart = None
    ShipStats = None
    Armor = None
    HitPoints = None
    AvgDamage = None
    MaxWeaponRange = None
    RadarRange = None
    BattleScore = None
    DamagedSuffix = None
    MainShipTitle = None
    ShipCrew = None
    ShipCrew__Tooltip = None
    ShipCannotUnload = None
    ShipFuelUnload = None
    ShipFuelUnload__Tooltip = None
    ShipFuelSwitch__Tooltip = None
    ShipFuelSwitch__InProgress = None
    ShipFuelSwitch__InUse = None
    ShipFuelSwitch__ShipBusy = None
    ShipFuelSwitch__MissingMaterials = None
    ShipDesigner = None
    ShipDesigner_ShipBeingRepaired = None
    ShipDesigner_ShipNeedsRepairs = None
    ShipUpgrade_InProgress = None
    ShipUpgrade_Cost = None
    ShipUpgrade_Preparing = None
    ShipUpgrade_Preparing__Tooltip = None
    ShipUpgrade_ReadyWaiting = None
    ShipUpgrade_Performing = None
    ShipUpgrade_Available = None
    ShipUpgrade_CannotAsBeingModified = None
    ShipUpgrade_CannotDowngrade = None
    ShipCrew__Load = None
    ShipCrew__Unload = None
    ShipHealth__Title = None
    ShipHealth__Tooltip = None
    ShipHealth__ThresholdTooltip = None
    ShipAutoRepair__Toggle = None
    ShipAutoRepair__Tooltip = None
    ShipAutoReturn__Toggle = None
    ShipAutoReturn__Tooltip = None
    Input__Enable = None
    Input__Pause = None
    TradeDockCargo__Tooltip = None
    ShipStatus__DistanceLabel = None
    ShipStatus__EtaLabel = None
    ShipStatus__TravelingTo = None
    ShipStatus__Exploring = None
    ShipStatus__At = None
    SettlementTitle = None
    SettlementServices = None
    Health = None
    HousingBonus__Active = None
    Health__Tooltip = None
    SettlementServices__Tooltip = None
    NumberOfSettlements__Tooltip = None
    UnityCap__Title = None
    UnityCap__Tooltip = None
    HousePopulationTooltip = None
    ResearchEfficiencyBonus = None
    ResearchEfficiencyBonus__Tooltip = None
    Occupants__TooltipForIsland = None
    Occupants__TooltipForSettlement = None
    PopulationGrowth__Title = None
    PopulationGrowth__Tooltip = None
    PopulationGrowth__RecentlyGained = None
    UnityBonusToAdjacentHousing = None
    PopulationOverview__Title = None
    PopulationOverview__OpenAction = None
    PossibleMaxUnity__Tooltip = None
    LastMonthUnityChanges__Title = None
    LastMonthUnityChanges__Tooltip = None
    TradeTitle = None
    TombOfCaptains_NextStage__Action = None
    FuelPerJourneySuffix = None
    PerJourneySuffix = None
    RunOnLowFuel__Action = None
    RunOnLowFuel__Tooltip = None
    SelectFuel_Title = None
    CargoDepotProduct__ImportTitle = None
    CargoDepotProduct__ExportTitle = None
    CargoDepotWizard__Title = None
    CargoDepotWizard__Tooltip = None
    CargoDepotWizard__AssignContract = None
    CargoDepotWizard__ImportProducts = None
    ContractAssigned__Title = None
    ContractAssigned__Tooltip = None
    MoreContracts = None
    EstablishedContracts__NoneInfo = None
    EstablishedContracts__Title = None
    EstablishedContracts__Tooltip = None
    Contracts_GroupBy = None
    Contract__Establish = None
    Contract__EstablishTitle = None
    Contract__EstablishTooltip = None
    Contract__Assign = None
    Contract__Unassign = None
    ProductionCostEstimate = None
    Contracts__Title = None
    Contracts__None = None
    Contracts__Tooltip = None
    Contracts__ShipSize = None
    Contracts__ShipSizeModules = None
    Contracts__NoneEstablished = None
    Contract__MonthlyCost = None
    Contract__MonthlyCostTooltip = None
    Contract__ExchangeCost = None
    Contract__ExchangeCostTooltip = None
    ContractCancelStatus__IsAssigned = None
    ContractCancelStatus__ProductNotResearched = None
    ContractAssignCheck__ModuleNotSupported = None
    ContractAssignCheck__IncompatibleProduct = None
    CargoShip__NoModulesBuilt = None
    CargoShip__ShipIsBeingUnloaded = None
    CargoShip__TransferringCargo = None
    CargoShip__NothingToPickUp = None
    CargoShip__NotEnoughToPickUp = None
    CargoShip_TripDuration = None
    CargoShip_TripDuration__Tooltip = None
    CargoShip_JourneyOptions = None
    CargoShip_FuelSaver__Toggle = None
    CargoShip_FuelSaver__Tooltip = None
    CargoShipDepartNow__Action = None
    CargoShipDepartNow__Tooltip = None
    CargoShipCannotDepartNow__WasRequested = None
    CargoShipCannotDepartNow__General = None
    CargoShip_JourneyEstimate__NotReady = None
    CargoShip_JourneyEstimate__Explained = None
    Maintenance = None
    Maintenance__EntityTooltip = None
    ShaftOverview = None
    ShaftOverview__Tooltip = None
    Shaft__Status = None
    MechShaft__Title = None
    MechShaft__Tooltip = None
    MechShaft__Throughput = None
    MechShaft__ThroughputTooltip = None
    MechShaft__AccPower = None
    MechShaft__AccPowerTooltip = None
    MechShaft__AccPower_StopsBelow = None
    MechShaft__AccPower_StartsAbove = None
    MechPowerGenerator__EfficiencyTitle = None
    MechPowerGenerator__EfficiencyTooltip = None
    MechPowerGenerator__AutoBalance = None
    MechPowerGenerator__AutoBalanceTooltip = None
    MechPowerGenerator__AutoBalanceStatus = None
    ThroughputWithParam = None
    Throughput = None
    MaximumThroughput = None
    ElectricityConsumption = None
    ElectricityProduction = None
    StorageCapacity = None
    TransportationSpeed = None
    TilesPerSecond = None
    PowerGenerator__AutoScalingTooltip = None
    PowerGenerator__Utilization = None
    Assign = None
    Unassign = None
    Unassign__VehicleTooltip = None
    VehiclesAvailable = None
    AssignedToPrefix = None
    VehicleLimitReached = None
    RocketCrewCapacity = None
    CargoCapacity = None
    FarmFertilityTitle = None
    FarmFertility = None
    FarmFertility__Tooltip = None
    FarmFertility__TargetLabel = None
    FarmFertility__NeedShort = None
    FarmFertility__NeedTooltip = None
    FarmFertility__NaturalReplenish = None
    FarmFertility__NaturalReplenishTooltip = None
    FarmFertility__EquilibriumTitle = None
    FarmFertility__EquilibriumTooltip = None
    FarmFertilizer__Title = None
    FarmFertilizer__Tooltip = None
    FarmFertilizer__MaxFertilityTitle = None
    FarmFertilizer__MaxFertilityTooltip = None
    FarmFertilizer__FertilizerConversionTooltip = None
    FarmFertilizer__Replenish = None
    CropSchedule = None
    CropSchedule__Tooltip = None
    CropSchedule__AddCropTooltip = None
    CropScheduleSkip__Tooltip = None
    FarmCropSelector = None
    FarmAvgProduction__Tooltip = None
    Farm_PlantedCrop = None
    FarmWater__Title = None
    FarmWater__Tooltip = None
    FarmWater__AvgNeed = None
    FarmIrrigation__Title = None
    FarmIrrigation__Tooltip = None
    CropSchedule__NoCrop = None
    CropState__DeadNoMaintenance = None
    CropState__DeadNoWater = None
    CropState__DeadNoFertility = None
    CropState__RemovedForChange = None
    CropOverdue = None
    CropOverdue__Tooltip = None
    FarmFertilityPenaltyNoRotation = None
    CropWaiting__Fertility = None
    CropWaiting__Water = None
    CropWaiting__NoReason = None
    CropHarvestNow__Action = None
    CropHarvestNow__Tooltip = None
    CropHarvestStats__Open = None
    CropStats__MoreDueToFertility = None
    CropStats__LessDueToFertility = None
    CropStats__MoreDueToBonus = None
    CropStats__LessDueToWater = None
    CropStats__DelayedDueToWater = None
    CropStats__MonthsWithoutWater = None
    CropStats__LessDueEarlyHarvest = None
    CropRequiresGreenhouse = None
    NextHarvest__Title = None
    NextHarvest__Tooltip = None
    Bridges_LanePicker__Title = None
    Bridges_LanesLayout__Title = None
    BridgeAdditionError__NoPath = None
    BridgeAdditionWarning__IntersectsReservedOcean = None
    BridgeAdditionWarning__DifferentGridOffset = None
    AverageProduction = None
    DesignationRemovalTooltip = None
    TipOnLoad__Prefix = None
    TipOnLoad__Food = None
    TipOnLoad__Diesel = None
    TipOnLoad__OilRig = None
    TipOnLoad__BuildTransports = None
    TipOnLoad__Unity = None
    TipOnLoad__Unity2 = None
    TipOnLoad__TransportUX = None
    TipOnLoad__TransportStraight = None
    TipOnLoad__TransportSnap = None
    TipOnLoad__PartialTrucks = None
    TipOnLoad__Calculator = None
    TipOnLoad__Codex = None
    TipOnLoad__TiersCycle = None
    TipOnLoad__PlacementHistory = None
    Statistics = None
    Statistics__NoData = None
    Statistics__Now = None
    Products = None
    Statistics_NoProductSelected = None
    Pollution = None
    Population = None
    PopGrowth = None
    Fuel = None
    Overview = None
    StatisticsGroup__TopConsumers = None
    StatisticsGroup__TopProducers = None
    PinProduct_Tooltip = None
    UnpinProduct_Tooltip = None
    RatioToTopBar_Tooltip = None
    ElectricityStats = None
    ComputingStats = None
    StatsRange__Years = None
    StatsRange__Months = None
    StatsRange__Days = None
    StatsRange__Max = None
    Stats_NoDataYet = None
    StatsEntry__TotalProduction = None
    StatsEntry__Mining = None
    StatsEntry__Recycling = None
    StatsEntry__Import = None
    StatsEntry__Deconstruction = None
    StatsEntry__TotalConsumption = None
    StatsEntry__Dumping = None
    StatsEntry__Construction = None
    StatsEntry__Farming = None
    StatsEntry__Export = None
    StatsProduct_Quantity = None
    StatsEntry__TotalQuantity = None
    StatsRange__ThisYear = None
    StatsRange__LastYear = None
    StatsRange__Lifetime = None
    StatsRange__LifetimeProduced = None
    StatsRange__LifetimeConsumed = None
    StatsCat__Vehicles = None
    StatsCat__Trains = None
    StatsCat__CargoShips = None
    StatsCat__MainShip = None
    StatsCat__PowerProduction = None
    StatsPops__Born = None
    StatsPops__Lost = None
    StatsPops__Refugees = None
    StatsCat__Radiation = None
    StatsCat__Other = None
    ProductsList_Pinned = None
    ProductsList_All = None
    TotalPopulation = None
    HousingCap = None
    FreeHousing = None
    WorkersDemand = None
    NewRefugees = None
    NewRefugees__Beacon = None
    LootReceived = None
    ProductsToFilter = None
    ProductsToFilter__None = None
    OreSorter_ProductsInBuffer = None
    Enemy = None
    BattleResult__ShipTitle = None
    Battle_DamageDealt__Tooltip = None
    Battle_FleetHealth__Tooltip = None
    Battle_ShotsFired__Tooltip = None
    Battle_ViewSummary = None
    BattleWindow__Title = None
    BattleStatus_Title = None
    BattleStatus__EnteringCombat = None
    BattleStatus__InProgress = None
    BattleStatus__PlayerVictory = None
    BattleStatus__PlayerDefeat = None
    BattleLog__Title = None
    BattleLog__Start = None
    BattleLog__End = None
    BattleLog__ShipDestroyed = None
    BattleLog__ShipEscaped = None
    BattleLog__EscapingLowHp = None
    BattleLog__EscapingNoWeapons = None
    BattleShipState__Destroyed = None
    BattleShipState__Escaped = None
    BattleShipState__Escaping = None
    BattleShipState__NoWeapons = None
    CargoShipsLimitTooltip = None
    FleetStatus = None
    ComputingDisplayTooltip = None
    ElectricityDisplayTooltip = None
    VehiclesManagement = None
    OwnedVehicles = None
    AvailableToAssign = None
    VehiclesAssignedToMining = None
    VehiclesAssignedToTreeHarvesting = None
    VehiclesAssignedToBuildings = None
    VehiclesManagement__Drivers = None
    VehiclesManagement__IdleCount = None
    SupplyLeft = None
    ServiceCoverageLastMonth = None
    FoodSupplyTitle__TooltipForSettlement = None
    Food = None
    FoodInSettlement__Title = None
    FoodInSettlement__Tooltip = None
    IndividualFoodSupply__Tooltip = None
    MaxFoodDemand__Tooltip = None
    FoodHealth__Title = None
    FoodHealth__Tooltip = None
    FoodHealth__CategoryTooltip = None
    FoodFeedInfo = None
    FoodCategoriesSatisfied = None
    Stored = None
    NotAvailableYet = None
    TotalServiceUnityTooltip = None
    SettlementWaste__Title = None
    SettlementWaste__Tooltip = None
    SettlementState__FoodLow = None
    SettlementState__TooMuchWaste = None
    SettlementState__NoWasteModule = None
    SettlementState__FreshWaterLow = None
    HousingUnityBonus = None
    HousingUnityBonus__Tooltip = None
    HousingDemandIncrease = None
    HousingDemandIncrease__Tooltip = None
    BetterBonusAvailable = None
    General = None
    TransportHeightTooltip = None
    TrAdditionError__PathNotFound = None
    BalancerPrioritization__Title = None
    BalancerPrioritization__Tooltip = None
    BalancerRatios__Title = None
    BalancerRatios__Inputs = None
    BalancerRatios__InputsTooltip = None
    BalancerRatios__Outputs = None
    BalancerRatios__Tooltip = None
    Consumption = None
    CurrentConsumption = None
    MaxConsumption = None
    Production = None
    Demand = None
    MaxCapacity = None
    Max = None
    ConsumedLastMonth = None
    ProducedLastMonth = None
    Utilization = None
    TotalSettlementNeed = None
    TotalSettlementOutput = None
    ToolsTitle = None
    Layers = None
    PauseTool = None
    PlanningMode = None
    Demolish = None
    CloneTool = None
    UnityTool = None
    CopyTool = None
    CutTool = None
    ReplaceTool = None
    SelectReplacement_Title = None
    PropsRemovalTool = None
    PolygonAreaTool__Confirm__Tooltip = None
    PolygonAreaTool__AddPoint__Tooltip = None
    PolygonAreaTool__RemovePoint__Tooltip = None
    PolygonAreaTool__MovePoint__Tooltip = None
    PolygonAreaTool__Rect = None
    PolygonAreaTool__Rect__Tooltip = None
    PolygonAreaTool__Rect__HudLabel = None
    ApplySettingsFrom = None
    AssignedForLogistics__ExportTooltipMineTower = None
    AssignedForLogistics__ImportTooltipMineTower = None
    AssignedForLogistics__ImportTooltipForestryTower = None
    AssignedForLogistics__ExportTooltipForestryTower = None
    AssignedForLogistics__ExportTooltipGeneral = None
    AssignedForLogistics__ImportTooltipGeneral = None
    AssignedForLogistics__ExportTooltipFuelStation = None
    ImportRoutesEnforce__Title = None
    ImportRoutesEnforce__Tooltip = None
    InvalidImportRouteSuffix = None
    InvalidExportRouteSuffix = None
    AssignedForLogistics__Empty = None
    ImportRoutesTitle = None
    ExportRoutesTitle = None
    DumpingFilter__Title = None
    DumpingFilter__Empty = None
    DumpingFilter__Tooltip = None
    DumpingFilterGlobal__Title = None
    DumpingFilterGlobal__Tooltip = None
    MineTowerNotifyFilter__Title = None
    MineTowerNotifyFilter__Tooltip = None
    MineTowerNotifyFilter__Empty = None
    FocusManagedAreaTooltip = None
    SupportedProducts = None
    ShiftsCount = None
    MiningPriority__Title = None
    MiningPriority__Tooltip = None
    MiningPriority__NotSet = None
    NuclearReactor__PowerLevelTitle = None
    NuclearReactor__PowerLevelTooltip = None
    NuclearReactor__AutoThrottle = None
    NuclearReactor__AutoThrottle_Tooltip = None
    NuclearReactor__EnrichmentTitle = None
    NuclearReactor__EnrichmentTooltip = None
    NuclearReactor__HeatLevelTitle = None
    NuclearReactor__HeatLevelTooltip = None
    NuclearReactor__HeatLevelRadiationSuffix = None
    NuclearReactor__HeatLevelNoRadiationSuffix = None
    NuclearReactor__OptimalHeatMarkerTooltip = None
    NuclearReactor__CoolingMarkerTooltip = None
    NuclearReactor__EmergencyCoolingTitle = None
    NuclearReactor__EmergencyCoolingTooltip = None
    NuclearReactorRods__StatusTitle = None
    NuclearReactorRods__Tooltip = None
    NuclearReactorRods__MinRequired = None
    RadiationLevel__Tooltip = None
    NuclearReactor__MinFuelMarker = None
    NuclearReactor__DisableBeforeUpgrade = None
    NuclearReactor__Overheated = None
    NuclearReactor__NotEnoughMaintenance = None
    FastBreederStepInfo__ReactorsSustained = None
    FastBreederStepInfo__SteamProduced = None
    FastBreederStepInfo__FuelConsumed = None
    DumpOffset = None
    DropDepth__OrderingExplanation = None
    StackerProducts__Title = None
    StackerTower__DumpOffset = None
    StackerTower__DumpOffset__Tooltip = None
    StackerTower__DumpRadiusLeft = None
    StackerTower__DumpRadiusRight = None
    StackerTower__ExtendRail = None
    StackerTower__ExtendRail__Tooltip = None
    StackerTower__Segments = None
    StackerTower__SlopePreview = None
    StackerTower__TotalDumpedLastMonth__Tooltip = None
    StackerTower__AlertFull__Tooltip = None
    StackerTower__Status__Moving = None
    StackerTower__Status__Transferring = None
    StackerTower__Status__DumpingFullfilled = None
    IncreasedPriority__Action = None
    IncreasedPriority__Label = None
    IncreasedPriority__ConstructionTooltip = None
    Priority = None
    Priority__ConstructionTooltip = None
    Priority__DeconstructionTooltip = None
    PriorityGeneral__Tooltip = None
    PriorityGeneral__TooltipWithCargo = None
    MakeDefault = None
    MakeDefault__ConstructionTooltip = None
    MakeDefault__DeconstructionTooltip = None
    ExportPriority = None
    ImportPriority = None
    ImportPriority__StorageTooltip = None
    ExportPriority__StorageTooltip = None
    ImportPriority__ShipFuelTooltip = None
    ExportPriority__ShipFuelTooltip = None
    ExportPriority__ShipyardCargo = None
    Priority__OrderingExplanation = None
    ImportPriority__ShipRepairTooltip = None
    ImportPriority__ShipCargoTooltip = None
    SupporterMaps__Title = None
    CustomMaps__Title = None
    MapArea__Flat = None
    MapArea__Land = None
    MapArea__Total = None
    Area_Value = None
    MapSize_XY = None
    StartingLocation_Title = None
    MapResources_Title = None
    MapResources_EasyToReach_Tooltip = None
    MapResources_ShowPinsTooltip = None
    MapResources_ShowPins = None
    CustomizeDifficulty__Description = None
    LockedFor__Tooltip = None
    GameSeed = None
    GameSeed__Tooltip = None
    StorageAlert__BtnTitle = None
    StorageAlert__OpenTooltip = None
    StorageAlert__Prefix = None
    StorageAlert__Empty = None
    StorageAlert__Full = None
    DumpInMineTowerOnly = None
    DumpAnywhereTooltip = None
    ServerRacks__Title = None
    ServerRacks__Tooltip = None
    ServerRacks__Maintenance = None
    ServerRacks__MaintenanceTooltip = None
    WasteSortingOutputs__Tooltip = None
    WasteSortingThreshold__Tooltip = None
    AnimalSlaughtering__Title = None
    AnimalSlaughtering__Tooltip = None
    AnimalSlaughtering_SliderLabel = None
    AnimalFarm_PauseGrowth__Title = None
    AnimalFarm_PauseGrowth__Tooltip = None
    AnimalFarm_Title = None
    AnimalFarm_TitleTooltip = None
    AnimalFarm_RemoveAllAnimals = None
    AnimalFarm_AddAnimals_Tooltip = None
    AnimalFarm_RemoveAnimals_Tooltip = None
    AnimalFarm_AnimalsBornEst = None
    Hospital_MortalityReduction = None
    Hospital_MortalityReductionTooltip = None
    Hospital_InputsTooltip = None
    Hospital_Bonuses = None
    CurrentDisease__Title = None
    CurrentDisease__Tooltip = None
    CurrentDisease__NoDisease = None
    CurrentDisease__Info = None
    HealthPenalty = None
    MortalityTooltip = None
    MonthsLeft = None
    MessagesNotifHud_Title = None
    MessagesNotifHud_NoNew = None
    ResearchNotifHud_Title = None
    ResearchNotifHud_NoNew = None
    AlertsNotifHud_Title = None
    AlertsNotifHud_HiddenTitle = None
    AlertsNotifHud_NoNew = None
    AlertsNotifHud_HiddenToggleTooltip = None
    AlertsNotifHud_HiddenCount = None
    AlertsNotifHud_Unhide__Tooltip = None
    Notification__NewRefugees = None
    Notification__LocationExplored = None
    Notification__ShipInBattle = None
    LaunchPad_FuelTitle = None
    LaunchPad_Payload__Title = None
    LaunchPad_Payload__HintForDelivery = None
    LaunchPad_Payload__HintForNoDelivery = None
    LaunchPad_OrbitDeliveries__Title = None
    LaunchPad_OrbitDeliveries__Tooltip = None
    LaunchPad_OrbitDeliveries__NotSet = None
    LaunchPad_NoRocketHint = None
    LaunchPad_WaterBufferTitle = None
    LaunchPad_Launch__Title = None
    LaunchPad_Launch__Start = None
    LaunchPad_Launch__AutoStart = None
    LaunchPad_Launch__LiftOff = None
    LaunchPad_Status__WaterLow = None
    LaunchPad_Status__FuelLow = None
    LaunchPad_Status__Waiting = None
    LaunchPad_Status__WaitingForRocket = None
    LaunchPad_Status__LaunchingRocket = None
    LaunchPad_Status__WaitingForPayloadRequest = None
    LaunchPad_Status__AutoLaunchNotEnabled = None
    LaunchPad_MuteCountDown = None
    EstimatedWaterYieldTitle = None
    NotifyIfFarmBufferFull = None
    TradeWithVillage = None
    LogisticsStatus__Stable = None
    LogisticsStatus__Busy = None
    LogisticsStatus__VeryBusy = None
    LogisticsStatus__ExtremelyBusy = None
    LogisticsStatus__Tooltip = None
    WorkersAvailable__Tooltip = None
    TotalPopulation__Tooltip = None
    TotalPopulation__HomelessTooltip = None
    FoodLeftMainPanel__Tooltip = None
    PlanningModeActive__Title = None
    PlanningModeActive__Tooltip = None
    TransportSnappingOff__Title = None
    TransportSnappingOff__Tooltip = None
    ResearchToRepair__Tooltip = None
    ResearchUnlocked = None
    ResearchUnlocked__NewBuildings = None
    ResearchUnlocked__NewProducts = None
    Status = None
    Collected = None
    GameSpeed = None
    Camera = None
    Designations = None
    BuildMode = None
    TransportMode = None
    TrainTracksMode = None
    TrainTracksEdit = None
    BridgesMode = None
    WindowsShortcuts = None
    GeneralShortcuts = None
    UiNavigationShortcuts = None
    PhotoMode = None
    DLC = None
    WaitingForKeyPress = None
    KeybindingHowToEdit = None
    KeybindingHowToClear = None
    IncreasePriority = None
    DecreasePriority = None
    FollowVehicleTooltip = None
    FollowTrainTooltip = None
    Notifications__Mute = None
    Notifications__Unmute = None
    Notifications__NoNew = None
    ApplyChanges = None
    ApplyChangesConflictPrompt = None
    ConflictsWith = None
    RestoreDefaults = None
    DiscardChanges = None
    Cargo__DiscardTooltip = None
    RemoveProductsInBuffers__Title = None
    RemoveProductsInBuffers__Tooltip = None
    OptionalOutput__Title = None
    OptionalOutput__Desc = None
    MachineBuffer__NotEnoughInput = None
    MachineBuffer__NotEnoughOutputSpace = None
    Machine_SelectRecipes = None
    Machine_PowerMult__IncTooltip = None
    Machine_PowerMult__DecTooltip = None
    Location_EnemyScore = None
    Location_HasEntity = None
    Location_Distance = None
    Location_ShipOnWay = None
    ShowInExplorer = None
    GoTo__Tooltip = None
    DiscardAllProducts__Action = None
    DiscardAllProducts__Confirmation = None
    DiscardAllProducts__NotSupported = None
    NotifyOnLowReserve = None
    WorldCargo__Title = None
    NavigateTo__Previous = None
    NavigateTo__Next = None
    NavigateTo__KeyHint = None
    MatchesFound = None
    CopyTool__Tooltip = None
    CopyTool__NoCopyTooltip = None
    ToolFilter__EntitiesTooltip = None
    ToolFilter__SurfaceTooltip = None
    ToolFilter__DecalsTooltip = None
    PlaceMultipleTooltip = None
    HoldToPlace__part1 = None
    HoldToPlace__part2Collision = None
    HoldToPlace__part2Replace = None
    ReplacingStructure_Disclaimer = None
    MovingStructure_Disclaimer = None
    NumberOfItemsToDeconstruct = None
    CutTool__Tooltip = None
    PauseTool__Tooltip = None
    UpgradeTool__Tooltip = None
    UpgradeTool__CancelTooltip = None
    PauseTool__PauseOnlyTooltip = None
    DeleteTool__Tooltip = None
    DeleteTool__QuickRemoveTooltip = None
    DeleteTool__EntireTransport = None
    UnityTool__Tooltip = None
    PropsRemovalTool__Tooltip = None
    CopySettings__Tooltip = None
    FlipShortcut__Tooltip = None
    RotateShortcut__Tooltip = None
    TransportTool__TieBreakTooltip = None
    TransportTool__NoTurnTooltip = None
    TransportTool__PortSnapTooltip = None
    TransportTool__PortBlockTooltip = None
    Toolbox__HideCosts = None
    TrackTool__TrackSnapTooltip = None
    ToggleDirectionTool__ToolTip = None
    ToggleCriticalTool__ToolTip = None
    ToggleCriticalTool__SelectAllToolTip = None
    ToggleSuperTool__ToolTip = None
    ToggleSuperTool__SelectAllToolTip = None
    TrackPillarTool__ToolTip = None
    AddElectrification__ToolTip = None
    ElectrifyTool__QuickUpgradeTooltip = None
    Goals_Title = None
    Goals_NoGoalsForNow = None
    Goals_AllGoalsComplete = None
    GoalSkip__Action = None
    GoalSkip__Confirmation = None
    Goal_TakeTime = None
    GoalShowCompleted__Action = None
    GoalShowLocked__Action = None
    GoalNotifState__Completed = None
    GoalNotifState__NewGoal = None
    Goal_CollectReward_Action = None
    Goal_CollectReward_Title = None
    OpenTutorial = None
    ClickToLearnMore = None
    Blueprints = None
    Blueprints__NewAction = None
    Blueprints__PlaceAction = None
    Blueprints__OpenAction = None
    Blueprints__Detail = None
    BlueprintDelete__Action = None
    BlueprintDelete__Confirmation = None
    BlueprintDelete__Result = None
    Blueprints__NotSupportedInDemo = None
    Blueprint_NewFromStringTooltip = None
    Blueprint_NewFromSelectionTooltip = None
    Blueprints__GetMoreOnHub = None
    Blueprint__ItemNotAvailable = None
    Blueprint__ItemObsolete = None
    Placement__ItemsObsolete = None
    Blueprint__ItemWillBeReplacedWithLowerTier = None
    Blueprint__ItemWillBeSkipped = None
    Blueprint__PlacementErrorNothingResearched = None
    Blueprint__PlacementWarning = None
    Blueprint__PlacementWarningMissing = None
    Blueprint__PlacementWarningDowngrade = None
    Blueprint__DeconstructionModeTooltip = None
    NewFolder__Tooltip = None
    Blueprint_ExportToStringTooltip = None
    UpdateDescription__Tooltip = None
    UpdateDescription__Title = None
    UpdateDescription__Placeholder = None
    CopyString__Action = None
    CopyString__Tooltip = None
    CopyString__Success = None
    PasteString__Action = None
    PasteString__Tooltip = None
    CharactersCount = None
    ImportBlueprint__Title = None
    ImportBlueprint__Success = None
    ImportBlueprint__Fail = None
    ExportBlueprint__Title = None
    Blueprints_BuildingRequired = None
    BlueprintContentMissing__Info = None
    BlueprintContentMissing__ListTitle = None
    BlueprintProtosLocked__NotAvailable = None
    BlueprintProtosLocked__CanDowngrade = None
    BlueprintLibStatus__Synchronized = None
    BlueprintLibStatus__FailedToSave = None
    BlueprintLibStatus__FailedToBackup = None
    BlueprintLibStatus__FailedToSaveTooltip = None
    BlueprintLibStatus__FailedToBackupTooltip = None
    BlueprintLibStatus__FailedToLoad = None
    BlueprintLibStatus__FailedToLoadOnFormat = None
    BlueprintLibStatus__FailedToLoadOnPermission = None
    FileLocation = None
    Blueprint__NumberOfBackups = None
    BlueprintMadeInVersionTooltip = None
    StatsTab__Breakdown = None
    StatsTab__Chart = None
    ConsumeSurplusPower__Toggle = None
    ConsumeSurplusPower__Tooltip = None
    ProductionPriority = None
    ProvideSurplusPower__Toggle = None
    ProvideSurplusPower__Tooltip = None
    ImportantAnnouncementTitle = None
    SaveMigration__Intro = None
    SaveMigration__BlueprintsNote = None
    DoNotShowAgain = None
    Update1__LocationChange = None
    Update1__OldSaveLocation = None
    Update1__NewSaveLocation = None
    Update1__BlueprintsCopied = None
    Update1__NewBlueprintsLocation = None
    Update1__OldLocationStillExists = None
    Loans_Title = None
    Loans_Title__Tooltip = None
    Loans_Active = None
    Loan_NoneActive = None
    Loans_ProductsToLend = None
    Loan_CreditScore = None
    Loan_CreditScore__Tooltip = None
    Loan_MaxLoans = None
    Loan_MaxLoans__Tooltip = None
    Loan_Multiplier = None
    Loan_Multiplier__Tooltip = None
    Loan_Fee = None
    Loan_Fee__Tooltip = None
    Loan_InterestRate = None
    Loan_InterestRate__Tooltip = None
    Loan_MaxToBorrowTooltip = None
    Loan_DurationTooltip = None
    Loan_NewLoan = None
    Loan_SelectProduct = None
    Loan_BorrowFieldLabel = None
    Loan_Borrow__Action = None
    Loan_Borrow__Tooltip = None
    Loan_ProductsDeliveryTooltip = None
    Loan_Debt = None
    Loan_LifetimeInterest = None
    Loan_PayPerYear__part1 = None
    Loan_PayPerYear__part2 = None
    Loan_TimeLeft = None
    Loan_NextPayment = None
    Loan_NextPaymentIn = None
    Loan_NextPayment__Tooltip = None
    Loan_Repay__Action = None
    Loan_Repay__Tooltip = None
    Loan_Repay__LackOfProducts = None
    Loan_Repay__InvalidQuantity = None
    Balance_LatestTransactions = None
    Loan_StartDate = None
    Loan_InterestSoFar = None
    Loan_RemainingInterest = None
    Loan_StartingLoan = None
    Loan_Overdue = None
    Loan_PaymentBuffer__Closed = None
    Loan_PaymentBuffer__OpensIn = None
    Loan_PaymentBuffer__ClosedInfo = None
    Loan_PaymentBuffer__PriorityToggle = None
    Loan_PaymentBuffers__Title = None
    Loan_PaymentBuffers__Tooltip = None
    Loan_NotAvailable__LowProduction = None
    Loan_NotAvailable__MaxLoans = None
    Loan_NotAvailable__QuantityHigh = None
    Loan_NotAvailable__QuantityLow = None
    Resources = None
    Costs = None
    Economy = None
    Power = None
    Nature = None
    FailureOutages = None
    Mechanics = None
    ClearSurface__Title = None
    ClearSurface__Tooltip = None
    PlaceSurface__Tooltip = None
    Decals_Name = None
    Decals_Paint = None
    Decals_TextToolTooltip = None
    Decals_PlaceText = None
    Decals_Recolor = None
    Decals_PaintTooltip = None
    Weight = None
    Weight__WhenFull = None
    LongWagonSuffix = None
    Vehicle_TractiveEffort = None
    Vehicle_GradeForce = None
    Vehicle_AirDragForce = None
    Vehicle_RollDragForce = None
    Vehicle_Throttle = None
    Vehicle_Braking = None
    Vehicle_FuelUsage = None
    Viaduct_Bridges_Start = None
    Cable_Bridges_Start = None
    Viaduct_Bridges_Continue = None
    Cable_Bridges_Continue = None
    Train_SkipStationTooltip = None
    TrainTracks_Build = None
    TrainTracks_Build_Electric = None
    TrainTracks_Direction = None
    TrainTracks_Critical = None
    TrainTracks_CriticalToolTip = None
    TrainTracks_Super = None
    TrainTracks_SuperToolTip = None
    TrainTracks_AddRemovePillars = None
    TrainTracks_AddRemovePillarsToolTip = None
    TrainStations_Waypoint = None
    TrainStations_WaypointToolTip = None
    TrainTracks_Electrify = None
    TrainTracks_ElectrifyToolTip = None
    TrainTracks_Electrify_Obsolete_Error = None
    ScrapTrain__Tooltip = None
    ScrapTrain__InProgress = None
    ReplaceTrain__Tooltip = None
    ReplaceTrain__InProgress = None
    ReplaceTrain__Cancel = None
    RefuelTrain__Action = None
    RefuelTrain__Tooltip = None
    RefuelTrain__NotEnoughFuel = None
    TrainCargo_SetFilter__Tooltip = None
    TrainCargo_ClearFilter__Tooltip = None
    TrainLevelCrossing_IsClosed = None
    TrainLevelCrossing_BadConnection = None
    TrainLevelCrossing_TrainsPassed = None
    NearestCompatibleDepot = None
    RecoverTrainAndPause__Action = None
    DepotCannotAcceptTrain_TrainTooLong = None
    DepotCannotAcceptTrain_Full = None
    DepotCannotAcceptTrain_DepotPaused = None
    DepotCannotAcceptTrainGeneric = None
    TrainTitle = None
    Train_Line = None
    TrainStatus_NotSpawned = None
    TrainStatus_Stopped = None
    TrainStatus_ExplicitGoal = None
    TrainStatus_DepotToScrap = None
    TrainStatus_PlayerDriving = None
    TrainStatus_NoLine = None
    TrainStatus_EmptyLine = None
    TrainStatus_NoOtherStation = None
    TrainStatus_NoOtherStation__Tooltip = None
    TrainStatus_NavigatingToStation = None
    TrainStatus_ServicingStation = None
    TrainStatus_SearchingForNextGoal = None
    TrainStatus_Driving = None
    TrainStatus_DrivingToDepot = None
    TrainStatus_InvalidGoal = None
    TrainStatus_NoValidGoal = None
    TrainStatus_ArrivalConditionsNotMet = None
    TrainStatus_WaitingForFreeTrack = None
    TrainStatus_WaitingBlockedBy__Tooltip = None
    TrainStatus_Arriving = None
    TrainStatus_Departing = None
    TrainStatus_AtStation = None
    TrainStatus_AtDepot = None
    TrainStatus_NoPower = None
    TrainStatus_WaitingForDepotDoors = None
    TrainStatus_WaitingForSuperBlock = None
    TrainStatus_CannotFindPath = None
    TrainStatus_SelfIntersect = None
    TrainStatus_SelfIntersect__Tooltip = None
    TrainStatus__HintForBidirectional = None
    TrainStatus_Pathfinding = None
    TrainStatus_Unknown = None
    TrainIssue_NoValidGoals = None
    TrainIssue_CannotFindPath = None
    TrainIssue_SelfIntersect = None
    TrainIssue_NoPower = None
    TrainWarning_AlignmentForkedAtStation = None
    TrainWarning_AlignmentForkedAtStationV2 = None
    TrainWarning_AlignmentEndedAtStation = None
    TrainDriveMode__ToGoal = None
    TrainDriveMode__Manual = None
    TrainDriveMode__Scrapping = None
    TrainDriveMode__Replacing = None
    Train_NoTrainDepotAvailableError = None
    TrainDepartCondition_Add = None
    TrainDepartCondition_FullOfAllOption = None
    TrainDepartCondition_FullOfProductOption = None
    TrainDepartCondition_EmptyOfAllOption = None
    TrainDepartCondition_EmptyOfProductOption = None
    TrainDepartCondition_ContainsProductOption = None
    TrainDepartCondition_TimePassedOption = None
    TrainDepartCondition_AfterDays = None
    TrainDepartCondition_PercentOf = None
    TrainDepartCondition_FullOf = None
    TrainDepartCondition_EmptyOf = None
    TrainDepartCondition_Contains = None
    TrainDepartCondition_AllCargo = None
    TrainDepartCondition_SpecificProduct = None
    TrainDepartCondition_LogicalOps__Tooltip = None
    LogicalOperator__And = None
    LogicalOperator__Or = None
    TrainDepartCondition_AllProvidedProducts = None
    TrainDepartCondition_AnyProvidedProducts = None
    TrainDepartCondition_AllAcceptedProducts = None
    TrainDepartCondition_AnyAcceptedProducts = None
    TrainAcceptCondition_TrainLimit = None
    TrainAcceptCondition_TrainLimit__Tooltip = None
    TrainAcceptCondition_RequestOnlyIf = None
    TrainAcceptCondition_ApproachIf = None
    TrainAcceptCondition_ApproachIf__Short = None
    TrainAcceptCondition_Unrestricted = None
    TrainAcceptCondition_AllEmpty = None
    TrainAcceptCondition_AllFull = None
    TrainAcceptCondition_AnyEmpty = None
    TrainAcceptCondition_AnyFull = None
    Train_NewTrain = None
    TrainDepot_NoActiveConstruction = None
    TrainDepot_NoActiveReplacement = None
    TrainDepot_ServiceLanes = None
    TrainDepot_ServiceLanes__NoTrains = None
    TrainDepot_StoredTrainCars = None
    TrainDepot_ReplacementInfo = None
    Vechile_UnderConstruction = None
    Vechile_InQueue = None
    DuplicateTrain__Tooltip = None
    TrainDesigner_Name = None
    TrainDesigner_NewTrainSection = None
    TrainDesigner_ReplaceTrainSection = None
    TrainDesigner_ReplaceLineSection = None
    TrainDesigner_NoDepotSelected = None
    TrainDesigner_RemoveCar__Action = None
    TrainDesigner_DuplicateCar__Action = None
    TrainDesigner_FlipCar__Action = None
    TrainDesigner_FlipFilter__Action = None
    TrainDesigner_Locomotives = None
    TrainDesigner_Wagons = None
    TrainDesigner_TrainStats = None
    TrainDesigner_TrainBuildScheduledToast = None
    TrainDesigner_ErrorNoLocomotive = None
    TrainDesigner_ErrorNoDepot = None
    TrainDesigner_ReplaceTrainBaseGroup = None
    TrainDesigner_ErrorInvalidCarPosition = None
    TrainDesigner_ScheduleBuildTooltip = None
    TrainDesigner_CannotBuildTooltip = None
    TrainProperty_Capacity = None
    TrainProperty_RunningCost = None
    TrainProperty_Length = None
    TrainCarProperty_Power = None
    TrainCarProperty_StartingTraction = None
    TrainCarProperty_BrakeForce = None
    TrainDesigner_Speeds__Title = None
    TrainDesigner_SpeedsBenchmark__Explanation = None
    TrainDesigner_MaxSpeed__Title = None
    TrainDesigner_TimeToTravel__Title = None
    TrainDesigner_Speed_Flat = None
    TrainDesigner_Speed_Grade12 = None
    TrainDesigner_Speed_Grade25 = None
    TrainDesigner_Speed_Backwards = None
    TrainDesigner_TimeSpeedJuntion = None
    TrainDesigner_BuildCost = None
    TrainDesigner_SwitchAlternateDesign__Tooltip = None
    TrainLine_Manager = None
    TrainLine_NoLineSelected = None
    TrainLine_NoTrainsAssignedToLine = None
    TrainLine_ScheduleTab = None
    TrainLine_TrainsTab = None
    TrainLine_TrainLinesSection = None
    TrainLine_UnassignedTrains = None
    TrainLine_NewLine = None
    TrainLine_DeleteNoTrains = None
    TrainLine_DeleteWithTrains = None
    TrainLine_DeletedToast = None
    TrainLine_ApplyProductColor = None
    TrainLine_ApplyColorToTrain = None
    TrainLine_CreateTooltip = None
    TrainLine_EditTooltip = None
    TrainLine_StationWasDestroyed = None
    TrainLine_NoStationInSchedule = None
    TrainLine_NoTrainLineAssigned = None
    TrainLine_NoStop = None
    TrainLine_AddStop = None
    TrainLine_StopLowFuelOnly = None
    TrainLine_AddStationToGroup = None
    TrainLine_FilterLoad = None
    TrainLine_FilterUnload = None
    TrainLine_FilterAnything = None
    TrainLine_FilterNothing = None
    TrainLine_AddFiltersTooltip = None
    TrainLine_Warning_OnlyRefuelStops = None
    TrainLine_Warning_OnlyRefuelStops__Tooltip = None
    TrainLine_Warning_OnlyOneStop = None
    TrainLine_Warning_OnlyOneStop__Tooltip = None
    TrainLine_Warning_OnlyOneStopNonFuel = None
    TrainLine_Warning_OnlyOneStopNonFuel__Tooltip = None
    TrainLine_Warning_DestroyedStation = None
    TrainLine_Arrivals__Title = None
    TrainLine_Arrivals_Empty = None
    TrainLine_SelectStationPrompt = None
    TrainLine_SelectReplacementStationPrompt = None
    TrainLine_RetargetStation_Button__Tooltip = None
    TrainLine_ReplaceStationAllLines__Label = None
    TrainLine_AssignNoRootError = None
    TrainTrackTool_NoTurnTooltip = None
    TrainTrackTool_SnapTooltip = None
    TrainTrackTool_AlternateRouteTooltip = None
    TrainTrackTool_RotateCcwTooltip = None
    TrainTrackTool_RotateCwTooltip = None
    TrainTrackTool_CycleRadiusTooltip = None
    TrainTrackTool_CycleGradeTooltip = None
    BridgeTool_NoTurnTooltip = None
    BridgeTool_SnapTooltip = None
    BridgeTool_AlternateRouteTooltip = None
    BridgeTool_RotateCcwTooltip = None
    BridgeTool_RotateCwTooltip = None
    TrainStation_Settings = None
    TrainStation_Loading = None
    TrainStation_Unloading = None
    TrainStation_TrainDeparting = None
    TrainStation_MissingRoot = None
    TrainStation_QuickClear = None
    TrainStation_Load = None
    TrainStation_SwitchToLoad = None
    TrainStation_Unload = None
    TrainStation_SwitchToUnload = None
    GlobalEffect = None
    MaintenanceReductionTooltip = None
    BuildQueue = None
    BuildQueue_Empty = None
    KeepBuildingForever_Tooltip = None
    Focus_MaxedOut = None
    Focus_NotEnoughPoints = None
    Office_ComputingBoostTooltip = None
    Office_ComputingBoost2Tooltip = None
    Focuses__Title = None
    Focuses__Tooltip = None
    FocusPoints_Office__Tooltip = None
    FocusPoints_Global__Tooltip = None
    FocusPoints_Available = None
    OfficeStatus__MissingSupplies = None
    BoostWithComputing = None
    ComputingNotAvailable = None
    OrbitalSupplies = None
    OrbitalSupplies__Tooltip = None
    OrbitalSupplies_SpaceProbesInOrbit_Tooltip = None
    SpaceStation = None
    SpaceStation_EstablishNew = None
    SpaceStation_EstablishNewHint = None
    SpaceStation_MaintenanceTooltip = None
    SpaceStation_MaintenanceMarkerTooltip = None
    SpaceStation_Crew__Title = None
    SpaceStation_Crew__Tooltip = None
    SpaceStation_Shutdown__Title = None
    SpaceStation_Shutdown__Tooltip = None
    SpaceStation_NextCrewRotation = None
    SpaceStation_ResearchTooltip = None
    SpaceStation_Downgrade__Commit = None
    SpaceStation_Downgrade__Confirmation = None
    SpaceStation_BonusesProvided = None
    SpaceStation_CrewRequired = None
    SpaceStation_BonusReduced = None
    RocketInboundTooltip = None
    SpaceStationStatus_NoCrewSupplies = None
    SpaceStationStatus_NoMaintenanceParts = None
    SpaceStationStatus_MaintenanceCriticallyLow = None
    SpaceStationStatus_CrewLow = None
    SpaceStationStatus_NoResearchSupplies = None
    UnityProvided = None
    ResearchEfficiencyBonusProvided = None
    AsteroidPlacement__Title = None
    AsteroidPlacement__Desc = None
    AsteroidPlacement__NeedsLocation = None
    AsteroidPlacement__Confirm = None
    Asteroid_ArrivingFromOrbit = None
    AsteroidsButton__Title = None
    AsteroidWindowTitle = None
    AsteroidDiscovery__Title = None
    AsteroidDiscovery__Tooltip = None
    AsteroidDiscovery__NoAsteroidsHint = None
    AsteroidDiscovery__ScanForNew = None
    AsteroidDiscovery__ScanForNew_Tooltip = None
    Asteroid_BringToOrbit = None
    Asteroid_BringToOrbit_Tooltip = None
    AsteroidComposition__Label = None
    AsteroidRadius__Label = None
    AsteroidReady = None
    AsteroidsInTransit__Title = None
    AsteroidsInTransit__NoAsteroidsHint = None
    Asteroid_BringToIsland = None
    AsteroidStatus__MovingIntoOrbit = None
    AsteroidStatus__WaitingInOrbit = None
    AsteroidStatus__UnknownYet = None
    Asteroid_AbandonTooltip = None
    AsteroidLimitReached = None
    SpaceStationRequired = None
    NotEnough = None
    RequiresStationOfHigherTier = None
    LogisticsControl__InputTitle = None
    LogisticsControl__InputTooltip = None
    LogisticsControl__OutputTitle = None
    LogisticsControl__OutputTooltip = None
    LogisticsControl__OnInput_Title = None
    LogisticsControl__OnOutput_Title = None
    LogisticsControl__OffInput_Title = None
    LogisticsControl__OffOutput_Title = None
    LogisticsControl_CyclePrev = None
    LogisticsControl_CycleNext = None
    LogisticsControl__AutoInput_Title = None
    LogisticsControl__AutoOutput_Title = None
    LogisticsControl__Auto_InputTooltip = None
    LogisticsControl__Auto_OutputTooltip = None
    LogisticsControl__DisableForNew = None
    OreSorter_AllowedProducts__Title = None
    OreSorter_AllowedProducts__Tooltip = None
    OreSorter_AllowedProducts__AutoAssign = None
    OreSorter_AllowedProducts__AutoAssignTooltip = None
    OreSorter_InputTitle = None
    OreSorter_NoSingleLoad__Toggle = None
    OreSorter_NoSingleLoad__Tooltip = None
    OreSorter_BlockedAlert__Tooltip = None
    OreSorter_ProductBlocked__Tooltip = None
    OreSorter_AddProduct = None
    OreSorter_HintToAddProduct = None
    OreSorter_PortsMap__Title = None
    OreSorter_PortsMap__Tooltip = None
    OreSorter_ProductNotMapped = None
    OreSorter_UnassignProduct = None
    RecyclingEfficiency__Title = None
    RecyclingEfficiency__Tooltip = None
    UpointsCategory__Decorations = None
    UpointsCategory__DecorationsLong = None
    EdictReason__HealthLow = None
    EdictReason__HousingFull = None
    CaptainOfficeNotAvailable = None
    EdictRequiresAdvancedOffice = None
    Edict__UnityCostLabel = None
    Edict__UnityProvidedLabel = None
    StartRepairs = None
    SpeedReduced__Machine = None
    SpeedReduced__Vehicle = None
    StageStr = None
    NotResearchedYet = None
    NotAvailableInDemo = None
    DemoSaveCompatible = None
    GlobalSearch_ResultsFound = None
    GlobalSearch_ResultByProduct = None
    GlobalSearch_HelpTooltip = None
    GlobalSearch_HelpTooltip2 = None
    Productivity__Monthly = None
    Productivity__Yearly = None
    def __init__(self):
        pass


class MapsLoadingHelper:
    def __init__(self):
        pass


class NotificationId:
    Invalid = None
    def __init__(self):
        self.IsValid = False
        self.Value = None

class ThicknessIRange:
    Zero = None
    def __init__(self):
        self.Height = None
        self.From = None
        self.To = None

class LooseProductQuantity:
    None = None
    def __init__(self):
        self.ProductQuantity = None
        self.IsEmpty = False
        self.IsNotEmpty = False
        self.Product = None
        self.Quantity = None

class PartialProductQuantity:
    None = None
    def __init__(self):
        self.IsEmpty = False
        self.IsNotEmpty = False
        self.Product = None
        self.Quantity = None

class ProductQuantity:
    None = None
    def __init__(self):
        self.IsEmpty = False
        self.IsNotEmpty = False
        self.Product = None
        self.Quantity = None

class ProductQuantityLarge:
    None = None
    def __init__(self):
        self.IsEmpty = False
        self.IsNotEmpty = False
        self.Product = None
        self.Quantity = None

class EntityId:
    Invalid = None
    def __init__(self):
        self.IsValid = False
        self.IsNotValid = False
        self.Value = 0

    class Factory:
        def __init__(self):
            pass


class EntityIdOption:
    None = None
    def __init__(self):
        self.HasValue = False
        self.AsNullable = None
        self.Value = None

class AsteroidId:
    def __init__(self):
        self.Value = 0

class IoPortId:
    Invalid = None
    def __init__(self):
        self.IsValid = False
        self.Value = 0

    class Factory:
        def __init__(self):
            pass


class LogisticsZoneId:
    def __init__(self):
        self.Value = 0

class MessageNotificationId:
    Invalid = None
    def __init__(self):
        self.IsValid = False
        self.Value = 0

    class Factory:
        def __init__(self):
            pass


class TrainGraphEdgeId:
    def __init__(self):
        self.HasValue = False
        self.IsNone = False
        self.Index = 0

class TrainGraphNodeId:
    def __init__(self):
        self.HasValue = False
        self.IsNone = False
        self.Index = 0

class TrainId:
    def __init__(self):
        self.Value = None

class TrainIdOrNone:
    None = None
    def __init__(self):
        self.HasValue = False
        self.IsNone = False
        self.Value = None
        self.Raw = None

class TrainLineId:
    def __init__(self):
        self.Value = 0

class TrainLineIdOrNone:
    None = None
    def __init__(self):
        self.HasValue = False
        self.IsNone = False
        self.Value = None
        self.Raw = 0

class TrainStationGroupId:
    def __init__(self):
        self.Value = 0

class TrainStationGroupIdOrNone:
    None = None
    def __init__(self):
        self.HasValue = False
        self.IsNone = False
        self.Value = None
        self.Raw = 0

class TrainTrackId:
    def __init__(self):
        self.Value = 0

class TrainTrackSuperBlockId:
    def __init__(self):
        self.Index = None

class TrainTrackSuperBlockIdOrNone:
    None = None
    def __init__(self):
        self.HasValue = False
        self.IsNone = False
        self.Value = None
        self.Index = None

class VehicleJobId:
    Invalid = None
    def __init__(self):
        self.IsValid = False
        self.Value = 0

    class Factory:
        def __init__(self):
            pass


class ProtoBuilderException:
    def __init__(self):
        self.Message = ""
        self.Data = None
        self.InnerException = None
        self.TargetSite = None
        self.StackTrace = ""
        self.HelpLink = ""
        self.Source = ""
        self.HResult = 0

class ProtoDepAttribute:
    def __init__(self):
        self.TypeId = None
        from Mafi.Core.Prototypes import Proto
        self.ProtoId = Proto.ID()


class RandomProvider:
    def __init__(self):
        self.MasterSeed = ""

class ICoreRandom:
    def __init__(self):
        pass


class RandomGeneratorType:
    Unrestricted = None
    SimOnly = None
    NonSim = None
    def __init__(self):
        self.value__ = 0

class RandomSeedConfig:
    DEFAULT_SEED = ""
    def __init__(self):
        self.MasterRandomSeed = ""

class SourceSinkCheatManager:
    def __init__(self):
        self.AreSourcesAndSinksAllowed = False

class SandboxManager:
    def __init__(self):
        self.CanCheat = False
        self.AreCheatsHidden = False
        self.IgnoreMissingMaintenance = False
        self.EnableInstantConstruction = False
        self.IgnoreFuelConsumption = False
        self.IgnoreMissingWorkers = False
        self.IgnoreMissingPower = False
        self.IgnoreMissingComputing = False
        self.IgnoreMissingUnity = False
        self.IgnoreMissingFood = False
        from Mafi import Option
        self.CurrentWeather = Option()
        self.SourceSinkCheatManager = None

class TileTransform:
    Identity = None
    def __init__(self):
        self.Transform90RotFlip = None
        self.TransformMatrix = None
        self.Position = None
        self.Rotation = None
        self.IsReflected = False

class Transform90RotFlip:
    Identity = None
    TOTAL_VALUES_COUNT = 0
    def __init__(self):
        self.Rotation90 = None
        self.IsFlipped = False
        self.ToTileTransform = None
        self.RawValue = None

class IInitializer:
    def __init__(self):
        self.IsBeingLoaded = False

class ITracingConfig:
    def __init__(self):
        self.SaveTraceOnSimOvertime = False
        self.SaveTraceOnSimOvertimeMinDelay = None
        self.SaveTimingLogPeriod = None

class TruckCaps:
    SmallTruckCapacity = None
    LargeTruckCapacity = None
    def __init__(self):
        pass

