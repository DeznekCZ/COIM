
class NuclearReactor:
    PROD_COUNTERS_LABELS = None
    POWER_LEVEL_HEAT_CAPACITY_DURATION = None
    MAX_POWER_INCREASE_PER_TICK = None
    MAX_POWER_DECREASE_PER_TICK = None
    MELTDOWN_OVERHEAT_DAMAGE_INTERVAL = None
    DECREASE_MAINTENANCE_MELTDOWN_DAMAGE = None
    POWER_LEVEL_HEAT_CAPACITY = 0
    SELF_COOLING_HEAT_MARGIN = 0
    HEAT_PER_POWER_LEVEL_PER_TICK = 0
    HEAT_REMOVED_IN_MELTDOWN_PER_TICK = 0
    SELF_COOLING_PER_TICK = 0
    HEAT_EXCHANGER_HEAT_CAPACITY_MULT = 0
    def __init__(self):
        self.MaxPowerLevel = 0
        self.MaxPowerLevelPercent = None
        self.MeltdownAtHeat = 0
        self.UpgradableProto = None
        self.Prototype = None
        self.CanBePaused = False
        self.LogisticsInputControl = None
        self.LogisticsOutputControl = None
        self.IsLogisticsInputDisabled = False
        self.IsLogisticsOutputDisabled = False
        self.IsCargoAffectedByGeneralPriority = False
        self.Maintenance = None
        self.CurrentState = None
        self.IsOverheated = False
        self.ActiveRecipes = None
        self.AllRecipes = None
        self.EmissionIntensity = None
        self.SoundParams = None
        self.IsSoundOn = False
        self.ComputingRequired = None
        from Mafi import Option
        self.ComputingConsumer = Option()
        self.DurationMultiplier = None
        self.HeatAmount = 0
        self.OptimalHeatForCurrentPower = 0
        self.StartEmergencyCoolingAtHeat = 0
        self.IsInMeltdown = False
        self.CurrentPowerLevel = None
        self.TargetPowerLevel = None
        self.CoolantInBuffer = None
        self.CoolantOutBuffer = None
        self.AllowedFuel = None
        self.IsAutomaticPowerRegulationSupported = False
        self.IsAutomaticPowerRegulationEnabled = False
        self.EnrichmentInputBuffer = Option()
        self.EnrichmentOutputBuffer = Option()
        self.EnrichmentStep = 0
        self.OngoingMonthlyData = None
        self.ProductivityCounterHistory = None
        self.ProductivityCounterLabels = None
        self.CustomTitle = Option()
        self.GeneralPriority = 0
        self.IsGeneralPriorityVisible = False
        self.Ports = None
        self.Transform = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.PfTargetTiles = None
        self.CenterTile = None
        self.Position2f = None
        self.Position3f = None
        self.AlwaysUseCustomPfTargetTiles = False
        self.ConstructionState = None
        self.IsConstructed = False
        self.IsNotConstructed = False
        self.IsBeingUpgraded = False
        self.ConstructionProgress = Option()
        self.DoNotAdjustTerrainDuringConstruction = False
        self.AreConstructionCubesDisabled = False
        self.Id = None
        self.DefaultTitle = None
        self.Context = None
        self.IsDestroyed = False
        self.IsEnabled = False
        self.IsNotEnabled = False
        self.IsPaused = False
        self.IsNotPaused = False
        self.RendererData = None
        self.WorkersNeeded = 0
        self.HasWorkersCached = False
        self.MaintenanceCosts = None
        self.IsIdleForMaintenance = False
        self.IsBoosted = False
        self.WorkedThisTick = False

    class Recipe:
        def __init__(self):
            from Mafi.Core.Prototypes import Proto
            self.Id = Proto.ID()

            self.AllUserVisibleInputs = None
            self.AllUserVisibleOutputs = None
            self.Duration = None
            self.FuelMultiplier = None
            self.FuelPerMinute = None

    class State:
        None = None
        Broken = None
        Paused = None
        Meltdown = None
        NotEnoughWorkers = None
        NotEnoughComputing = None
        NotEnoughMaintenance = None
        NotEnoughInput = None
        OutputFull = None
        NoRecipes = None
        Idle = None
        Working = None
        def __init__(self):
            self.value__ = 0

class NuclearReactorSetPowerLevelCmd:
    def __init__(self):
        self.AffectsSaveState = False
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.IsVerificationCmd = False
        self.Result = False
        self.HasError = False
        self.ErrorMessage = ""
        self.ReactorId = None
        self.PowerLevel = 0

class NuclearReactorToggleAllowedFuelCmd:
    def __init__(self):
        self.AffectsSaveState = False
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.IsVerificationCmd = False
        self.Result = False
        self.HasError = False
        self.ErrorMessage = ""
        self.ReactorId = None
        from Mafi.Core.Products import ProductProto
        self.FuelProtoId = ProductProto.ID()


class NuclearReactorToggleAutomaticRegulationCmd:
    def __init__(self):
        self.AffectsSaveState = False
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.IsVerificationCmd = False
        self.Result = False
        self.HasError = False
        self.ErrorMessage = ""
        self.ReactorId = None

class NuclearReactorSetEnrichmentStepCmd:
    def __init__(self):
        self.AffectsSaveState = False
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.IsVerificationCmd = False
        self.Result = False
        self.HasError = False
        self.ErrorMessage = ""
        self.ReactorId = None
        self.EnrichmentStep = 0

class NuclearReactorProto:
    def __init__(self):
        self.EntityType = None
        self.ComputingConsumed = None
        self.Upgrade = None
        self.TierData = None
        self.Recipes = None
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.IconPath = ""
        self.CanMoveUpDownWhenInvalidPlacement = False
        self.IsUnique = False
        self.CloningDisabled = False
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
        self.MaxPowerLevel = 0
        self.WaterInPerPowerLevel = None
        self.SteamOutPerPowerLevel = None
        self.WaterInPorts = ""
        self.SteamOutPorts = ""
        self.ProcessDuration = None
        self.FuelCapacity = None
        self.MinFuelToOperate = None
        self.FuelPairs = None
        self.FuelInPort = None
        self.FuelOutPort = None
        self.CoolantIn = None
        self.CoolantOut = None
        self.CoolantInPort = None
        self.CoolantOutPort = None
        self.LeakRadiationOnMeltdown = False
        self.DestroyFuelOnMeltdown = False
        from Mafi import Option
        self.Enrichment = Option()
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.ConstructionDurationPerProduct = None
        self.CollapseRubbleScale = None
        self.CustomBuriedTolerance = None
        self.CustomSuspendedTolerance = None
        self.VehicleGoalHeightAllowedRange = None
        self.CannotBeBuiltByPlayer = False
        self.CannotBeDestroyedByFlood = False
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False

    class Gfx:
        Empty = None
        def __init__(self):
            self.PrefabPath = ""
            self.PrefabOrigin = None
            self.IconPath = ""
            self.YawForGeneratedIcon = None
            self.VisualizedLayers = None
            self.Categories = None
            self.AnimationDataAssetPathBase = ""
            from Mafi import Option
            self.SoundPrefabPath = Option()
            self.FuelIconPath = Option()
            self.MaxEmissionIntensity = 0.0
            self.IconIsCustom = False
            self.UseInstancedRendering = False
            self.UseSemiInstancedRendering = False
            self.SemiInstancedRenderingExcludedObjects = None
            self.MaxRenderedLod = 0
            self.DisableEmptyChildrenStripping = False
            self.InstancedRendererIndex = None
            self.AnimatedGameObjects = None
            self.AnimationLength = 0.0
            self.RemoveUndergroundVertices = False
            self.HideBlockedPortsIcon = False
            self.Color = None
            self.RendererIndex = 0

    class FuelData:
        def __init__(self):
            self.IsEmpty = False
            self.FuelInProto = None
            self.SpentFuelOutProto = None
            self.Duration = None

    class EnrichmentData:
        def __init__(self):
            self.InputProduct = None
            self.InPort = None
            self.OutputProduct = None
            self.OutPort = None
            self.ProcessedPerLevel = None
            self.BuffersCapacity = None
            self.DestroyContentOnMeltdown = False
            self.EnrichmentSteps = None
            self.DefaultEnrichmentStep = 0

    class EnrichmentStepData:
        def __init__(self):
            self.FuelMultiplier = None
            self.BreedingRatio = 0
            self.SteamReductionDiv = 0
