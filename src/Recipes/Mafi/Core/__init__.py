
class FileSystemHelper:
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

class IFileSystemHelper:
    def __init__(self):
        self.GameDataRootDirPath = ""
        self.GameDataRootDirPathLegacy = ""
        self.WorkDirPath = ""

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
    def __init__(self):
        self.value__ = 0

class SaveFileInfo:
    def __init__(self):
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
    IS_ALPHA_ONLY = False
    IS_RELEASE_CHEATS = False
    def __init__(self):
        pass


class CoreMod:
    def __init__(self):
        self.Name = ""
        self.Version = 0
        self.IsUiOnly = False
        from Mafi import Option
        self.ModConfig = Option()
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
        self.IsGodModeEnabled = False
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
        self.TotalElapsedSeconds = Fix64()
        self.SimStepsCount = 0
        self.SimStepsSinceLoad = 0
        self.TotalElapsedSimStepsSmooth = Fix64()
        self.DeltaSimStepsApprox = None
        from Mafi import Fix32
        self.TimeSinceLastSimUpdateMs = Fix32()
        self.DeltaTimeMs = 0.0
        self.FrameTimeSec = 0.0
        self.AbsoluteT = 0.0
        self.RelativeT = 0.0
        self.DeltaT = 0.0
        self.IsGamePaused = False
        self.GameSpeedMult = 0
        self.CurrSimUpdateDurationMs = Fix32()

class IGodModeConfig:
    def __init__(self):
        self.IsGodModeEnabled = False

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
        Diesel = ProductProto.ID('Product_Diesel')
        ConcreteSlab = ProductProto.ID('Product_ConcreteSlab')
        Wood = ProductProto.ID('Product_Wood')
        CleanWater = ProductProto.ID('Product_Water')
        Waste = ProductProto.ID('Product_Waste')
        Biomass = ProductProto.ID('Product_Biomass')
        Recyclables = ProductProto.ID('Product_Recyclables')
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
        DIESEL = ""
        CONCRETE_SLAB = ""
        WOOD = ""
        CLEAN_WATER = ""
        WASTE = ""
        BIOMASS = ""
        RECYCLABLES = ""
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
        def __init__(self):
            pass


    class PropertyIds:
        VehiclesFuelConsumptionMultiplier = None
        ShipsFuelConsumptionMultiplier = None
        TrucksCapacityMultiplier = None
        MaintenanceConsumptionMultiplier = None
        TrucksMaintenanceMultiplier = None
        FuelConsumptionDisabled = None
        UnityProductionMultiplier = None
        SettlementConsumptionMultiplier = None
        MiningMultiplier = None
        ResearchStepsMultiplier = None
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
        DiseaseMortalityMultiplier = None
        WaterPollutionMultiplier = None
        AirPollutionMultiplier = None
        LandfillPollutionMultiplier = None
        OreSortingEnabled = None
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
        HealthCareNeed = Proto.ID('HealthCareNeed')
        def __init__(self):
            pass


    class World:
        from Mafi.Core.Entities import EntityProto
        Fleet = EntityProto.ID('Fleet')
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
        def __init__(self):
            pass


    class Weather:
        from Mafi.Core.Prototypes import Proto
        Sunny = Proto.ID('SunnyWeather')
        def __init__(self):
            pass


    class ToolbarCategories:
        from Mafi.Core.Prototypes import Proto
        Surfaces = Proto.ID('surfaceCategory')
        def __init__(self):
            pass


class TrCore:
    Unity = None
    Unity__Tooltip = None
    MwSec__Unit = None
    NumberOfDays = None
    NumberOfMonths = None
    NumberOfYears = None
    AdditionError__NoDeposit = None
    AdditionError__ThinDeposit = None
    AdditionError__HasDeposit = None
    AdditionError__Unique = None
    AdditionError__CollisionWith = None
    AdditionError__NeedsOcean = None
    AdditionError__SomethingInWay = None
    AdditionError__OutsideOfMap = None
    AdditionError__OceanNotAllowed = None
    AdditionError__OceanBlocked = None
    AdditionError__OceanBlockedBy = None
    AdditionError__OceanBlockedByTerrain = None
    AdditionError__NotInSlot = None
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
    RemovalError__HousingHasModuleAttached = None
    RemovalError__NotContiguous = None
    RemovalError__ScrapItFirst = None
    RemovalError__ShipHasCargo = None
    RemovalError__DepotMovingCargo = None
    RemovalError__HasProductsStored = None
    RemovalError__RemoveModulesFirst = None
    RemovalError__CannotRemove = None
    RemovalError__HasShipAssigned = None
    RemovalError__FarmHasAnimals = None
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
    ManagedArea__EditAction = None
    ManagedDesignation__EditAction = None
    VehicleRole__TreeHarvesting = None
    WorldLocation_Orders__Explore = None
    DesignationError__Invalid = None
    DesignationWarning__NoTower = None
    DesignationWarning__NoForestryTower = None
    DesignationWarning__CannotStartMining = None
    DesignationWarning__CannotStartDumping = None
    DesignationWarning__CannotStartLeveling = None
    DesignationWarning__CannotStartForestry = None
    DesignationWarning__CannotPlaceDecal = None
    GameSaveLoad__VersionTooHigh = None
    GameSaveLoad__VersionTooLow = None
    GameInitFail = None
    GameInitFail__Mod = None
    GameInitFail__OutOrMemory = None
    GameSaveLoad__MissingMod = None
    GameSaveLoad__MissingFile = None
    GameSaveLoad__SaveNotFinishedButSaved = None
    GameSaveLoad__CannotSaveFile = None
    GameSaveLoad__ChecksumFail = None
    GameSaveLoad__CannotSaveFile_Crash = None
    GameSaveLoad__CannotLoadFile = None
    GameSaveLoad__CannotLoadChecksumFail = None
    GameSaveLoad__SwitchSteamVersion = None
    COIHub = None
    MailingList = None
    Suggestions = None
    ReportIssue = None
    Credits = None
    Enabled = None
    Disabled = None
    OptionValStandard = None
    OptionValIncreased = None
    OptionValReduced = None
    OptionValMeters = None
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
    MapInvalid = None
    NewGameWizard__Title = None
    Option_Unlimited = None
    Tutorials__Title = None
    Tutorials__Description = None
    ChangeHistory__Title = None
    ChangeHistory__EmptyLabel = None
    ChangeHistory__ConfirmTitle = None
    ChangeHistory__ConfirmPrompt = None
    DateYear__Label = None
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
    TradeStatus__CantAfford = None
    TradeStatus__NoUnity = None
    TradeStatus__SoldOut = None
    TradeStatus__NoTradeDock = None
    TradeStatus__TradeDockNotOperational = None
    TradeStatus__NoSpaceInFarm = None
    Status_LowReputation = None
    Contract__MonthlyCost = None
    Contract__ExchangeCost = None
    ContractCancelStatus__IsAssigned = None
    ContractCancelStatus__ProductNotResearched = None
    ContractAssignCheck__ModuleNotSupported = None
    ContractAssignCheck__IncompatibleProduct = None
    SettlementTitleWithReputation = None
    ReputationIncreaseTitle = None
    WorldMineTitleWithLevel = None
    PopsToAdoptNotAvailable = None
    TradeStatus__Info = None
    TradeStatus__Info_Animal = None
    TradeStatus__Info_CargoShip = None
    TradeOfferDelivered = None
    TradeOfferDelivered__Animal = None
    TradeOfferDelivered__CargoShip = None
    RecyclingEfficiency__Title = None
    RecyclingEfficiency__Tooltip = None
    OreSorter_AllowedProducts__Title = None
    OreSorter_AllowedProducts__Tooltip = None
    OreSorter_SelectProducts = None
    OreSorter_InputTitle = None
    OreSorter_LimitReached = None
    OreSorter_NoSingleLoad__Toggle = None
    OreSorter_NoSingleLoad__Tooltip = None
    OreSorter_BlockedAlert__Tooltip = None
    OreSorter_ProductBlocked__Tooltip = None
    OutputPort__Tooltip = None
    UpointsCategory__Decorations = None
    UpointsCategory__DecorationsLong = None
    GameOver__Title = None
    GameOver__Message = None
    StageStr = None
    EdictReason__HealthLow = None
    EdictReason__HousingFull = None
    PauseTool = None
    PlanningMode = None
    Demolish = None
    CloneTool = None
    UnityTool = None
    CopyTool = None
    CutTool = None
    UpgradeTool = None
    PropsRemovalTool = None
    ThisVehicleCannotDriveUnderTransports = None
    EntityCannotBeReachedDesc = None
    InvalidImportRouteSuffix = None
    InvalidExportRouteSuffix = None
    StoredProduct__KeepFull = None
    StoredProduct__KeepEmpty = None
    NoVehicleDepotAvailable = None
    CaptainOfficeNotAvailable = None
    EdictRequiresAdvancedOffice = None
    AssignedTrucksEnforce__Title = None
    AssignedTrucksEnforce__Tooltip = None
    VehicleJob__InvalidState = None
    VehicleJob__SearchingForDesignation = None
    VehicleJob__DrivingToGoal = None
    VehicleJob__Loading = None
    VehicleJob__Unloading = None
    VehicleJob__ProcessingSurface = None
    VehicleJob__Navigating = None
    VehicleJob__NavigatingToVia = None
    ShipCantVisit__NoCrew = None
    ShipCantVisit__NoFuel = None
    ShipCantVisit__TooFar = None
    CargoShipDepartNow__Action = None
    CargoShipDepartNow__Tooltip = None
    CargoShipCannotDepartNow__WasRequested = None
    CargoShipCannotDepartNow__General = None
    TrucksAnalytics__Title = None
    LogisticsControl__InputTitle = None
    LogisticsControl__InputTooltip = None
    LogisticsControl__OutputTitle = None
    LogisticsControl__OutputTooltip = None
    LogisticsControl__On = None
    LogisticsControl__On_InputTooltip = None
    LogisticsControl__On_OutputTooltip = None
    LogisticsControl__Off = None
    LogisticsControl__Off_InputTooltip = None
    LogisticsControl__Off_OutputTooltip = None
    LogisticsControl__Auto = None
    LogisticsControl__Auto_InputTooltip = None
    LogisticsControl__Auto_OutputTooltip = None
    RuinsRecycle__Action = None
    RuinsRecycleFormatted__Tooltip = None
    ImportRoutesTitle = None
    ExportRoutesTitle = None
    ShipyardKeepEmpty = None
    PartialTrucksToggle = None
    PartialTrucksToggle__Tooltip = None
    StartRepairs = None
    WorldMineProductionLvl__Title = None
    WorldMineProductionLvl__Tooltip = None
    WorldMap = None
    NewFolderTitlePlaceholder = None
    NewBlueprintTitlePlaceholder = None
    NuclearReactor__DisableBeforeUpgrade = None
    NuclearReactor__Overheated = None
    NuclearReactor__NotEnoughMaintenance = None
    EntityStatus__Broken = None
    EntityStatus__ResourceDepleted = None
    EntityStatus__LowPower = None
    EntityStatus__NoComputing = None
    EntityStatus__NeedsFuel = None
    SpeedReduced__Machine = None
    SpeedReduced__Vehicle = None
    ShipFuelSwitch__Tooltip = None
    ShipFuelSwitch__InProgress = None
    ShipFuelSwitch__InUse = None
    ShipFuelSwitch__ShipBusy = None
    ShipFuelSwitch__MissingMaterials = None
    Loan_NotAvailable__LowProduction = None
    Loan_NotAvailable__MaxLoans = None
    Loan_NotAvailable__QuantityHigh = None
    Loan_NotAvailable__QuantityLow = None
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
        self.ToNullable = None
        self.Value = None

class IoPortId:
    Invalid = None
    def __init__(self):
        self.IsValid = False
        self.Value = 0

    class Factory:
        def __init__(self):
            pass


class MessageNotificationId:
    Invalid = None
    def __init__(self):
        self.IsValid = False
        self.Value = 0

    class Factory:
        def __init__(self):
            pass


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

class TileTransform:
    Identity = None
    def __init__(self):
        self.Transform90RotFlip = None
        self.TransformMatrix = None
        self.Position = None
        self.Rotation = None
        self.IsReflected = False

class Transform90RotFlip:
    TOTAL_VALUES_COUNT = 0
    def __init__(self):
        self.Rotation90 = None
        self.IsFlipped = False
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

