class AnimalFarm:
    MAX_SLIDES_STEPS = None

    def __init__(self):
        self.UpgradableProto = None
        self.Prototype = None
        self.CanBePaused = False
        self.CurrentState = None
        self.AnimationParams = None
        self.AnimationStatesProvider = None
        self.FoodInputBuffer = None
        self.WaterInputBuffer = None
        from Mafi import Option
        self.ProductProducedBuffer = Option()
        from Mafi import Fix32
        self.CarcassPerMonth = Fix32()
        self.CarcassBuffer = None
        self.AnimalsCount = int(0)
        self.CapacityLeft = int(0)
        from Mafi import Fix32
        self.AnimalsBornPerMonth = Fix32()
        self.IsSlaughteringEnabled = False
        self.IsGrowthPaused = False
        self.AreAnimalsStarving = False
        self.AreAnimalsMissingWater = False
        self.SlaughterStep = int(0)
        self.SlaughterLimit = None
        self.CanDisableLogisticsInput = False
        self.CanDisableLogisticsOutput = False
        self.LogisticsInputMode = None
        self.LogisticsOutputMode = None
        from Mafi import Option
        self.CustomTitle = Option()
        self.GeneralPriority = int(0)
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.Ports = None
        self.ConstructionCost = None
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
        from Mafi import Option
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
        self.WorkersNeeded = int(0)
        self.HasWorkersCached = False
class State:
    Paused = None
    Working = None
    MissingWorkers = None
    MissingFood = None
    MissingWater = None
    NoAnimals = None
    FullOutput = None

    def __init__(self):
        pass

class AnimalFarmConfigExtensions:

    def __init__(self):
        pass

class AnimalFarmProto:

    def __init__(self):
        self.EntityType = None
        self.Upgrade = None
        self.TierData = None
        self.AnimationParams = None
        self.Layout = None
        self.Ports = None
        self.CloningDisabled = False
        self.IsUnique = False
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.IconPath = str(0)
        self.Id = None
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
class CropProto:

    def __init__(self):
        self.IconPath = str(0)
        self.MonthsToGrow = int(0)
        self.Id = None
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
class Gfx:
    Empty = None

    def __init__(self):
        pass

class Farm:
    FERTILITY_PENALTY_FOR_SAME_CROP = None
    FERTILITY_REPLENISH_MULT_WHEN_ABOVE_100 = None
    CROP_FERTILITY_DEMAND_MULT_WHEN_FERTILITY_ABOVE_100 = None
    STARTING_FERTILITY = None
    NO_YIELD_BEFORE_GROWTH_PERC = None
    FERTILITY_PER_SLIDER_STEP = None
    MAX_FERTILITY_SLIDER_VALUE = None
    CROP_SLOTS_COUNT = None
    MAX_DAYS_DISABLED = None
    INPUT_WATER_PORT_NAME = None
    INPUT_FERTILIZER_PORT_NAME = None
    FERTILITY_SLIDER_STEPS = None

    def __init__(self):
        self.UpgradableProto = None
        self.Prototype = None
        self.CanBePaused = False
        self.Maintenance = None
        self.IsIdleForMaintenance = False
        self.CurrentState = None
        from Mafi import Option
        self.CurrentCrop = Option()
        from Mafi import Option
        self.PreviousCrop = Option()
        self.NotifyOnFullBuffer = False
        self.SoilWaterBuffer = None
        self.AvgWaterNeedPerMonth = None
        self.ImportedWaterBuffer = None
        self.OutputBuffers = None
        self.Fertility = None
        self.NaturalFertilityEquilibrium = None
        self.NaturalReplenishPerDay = None
        self.StoredFertilizerCount = None
        self.StoredFertilizerCapacity = None
        self.MaxFertilityProvidedByFertilizer = None
        self.FertilityPerFertilizer = None
        self.FertilityTargetIndex = int(0)
        self.FertilityTargetValue = None
        self.FertilityNeededPerDay = None
        self.Schedule = None
        self.ActiveScheduleIndex = int(0)
        self.LifetimePlantedCropsCount = int(0)
        self.IsIrrigating = False
        self.ShouldAnimate = False
        self.IsSoundOn = False
        self.SoundParams = None
        self.TotalRotationDurationDays = int(0)
        self.AvgYieldPerYear = None
        self.CanDisableLogisticsInput = False
        self.CanDisableLogisticsOutput = False
        self.LogisticsInputMode = None
        self.LogisticsOutputMode = None
        from Mafi import Option
        self.CustomTitle = Option()
        self.GeneralPriority = int(0)
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.Ports = None
        self.ConstructionCost = None
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
        from Mafi import Option
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
        self.WorkersNeeded = int(0)
        self.HasWorkersCached = False
        self.MaintenanceCosts = None
class State:
    None = None
    Paused = None
    Broken = None
    NotEnoughWorkers = None
    NoCropSelected = None
    NotEnoughWater = None
    LowFertility = None
    Growing = None

    def __init__(self):
        pass

class Crop:

    def __init__(self):
        self.ConsumedWaterPerDay = None
        self.IsStarted = False
        self.GrowthPercent = None
        self.DrynessPercent = None
        self.WillDrySoon = False
        self.DaysRemaining = int(0)
        self.IsMissingWater = False
        self.DaysMissingWater = int(0)
        self.TotalDaysWithoutWater = int(0)
        self.HarvestWillYieldProducts = False
        self.Yield = None
        self.HarvestReason = None
        self.YieldDeltaDueToFertility = None
        self.YieldDeltaDueToBonusMultiplier = None
        self.YieldLostDueToLackOfWater = None
        self.YieldLostDueToPrematureHarvest = None
        self.DaysWaitingForWaterBeforeGrowthStart = int(0)
class CropHarvestReason:
    None = None
    HarvestedNormally = None
    PrematureLackOfMaintenance = None
    PrematureNoWater = None
    PrematureNoFertility = None
    PrematureHarvestedByPlayer = None
    PrematureClearedByPlayer = None

    def __init__(self):
        pass

class FarmConfigExtensions:

    def __init__(self):
        pass

class FarmAssignCropCmd:

    def __init__(self):
        self.AffectsSaveState = False
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.IsVerificationCmd = False
        self.Result = False
        self.HasError = False
        self.ErrorMessage = str(0)
class FarmForceNextSlotCmd:

    def __init__(self):
        self.AffectsSaveState = False
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.IsVerificationCmd = False
        self.Result = False
        self.HasError = False
        self.ErrorMessage = str(0)
class FarmSetFertilityTargetCmd:

    def __init__(self):
        self.AffectsSaveState = False
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.IsVerificationCmd = False
        self.Result = False
        self.HasError = False
        self.ErrorMessage = str(0)
class FarmHarvestNowCmd:

    def __init__(self):
        self.AffectsSaveState = False
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.IsVerificationCmd = False
        self.Result = False
        self.HasError = False
        self.ErrorMessage = str(0)
class AnimalFarmSetSlaughterLimitCmd:

    def __init__(self):
        self.AffectsSaveState = False
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.IsVerificationCmd = False
        self.Result = False
        self.HasError = False
        self.ErrorMessage = str(0)
class AnimalFarmToggleGrowthPauseCmd:

    def __init__(self):
        self.AffectsSaveState = False
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.IsVerificationCmd = False
        self.Result = False
        self.HasError = False
        self.ErrorMessage = str(0)
class MoveAnimalsCmd:

    def __init__(self):
        self.AffectsSaveState = False
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.IsVerificationCmd = False
        self.Result = False
        self.HasError = False
        self.ErrorMessage = str(0)
class ToggleFullBufferNotificationCmd:

    def __init__(self):
        self.AffectsSaveState = False
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.IsVerificationCmd = False
        self.Result = False
        self.HasError = False
        self.ErrorMessage = str(0)
class FarmProto:

    def __init__(self):
        self.EntityType = None
        self.Upgrade = None
        self.TierData = None
        self.Recipes = None
        self.Layout = None
        self.Ports = None
        self.CloningDisabled = False
        self.IsUnique = False
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.IconPath = str(0)
        self.Id = None
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
class Gfx:
    Empty = None

    def __init__(self):
        self.PrefabPath = str(0)
        self.PrefabOrigin = None
        self.IconPath = str(0)
        self.YawForGeneratedIcon = None
        self.VisualizedLayers = None
        self.Categories = None
class FarmFertileGroundValidator:
    MIN_FARMABLE_THICKNESS = None
    MAX_TILES_WITH_NON_FARMABLE_MATERIAL = None

    def __init__(self):
        self.Priority = None
class FertilizerProductParam:

    def __init__(self):
        self.AllowedProtoType = None
class LooseProductParam:

    def __init__(self):
        self.AllowedProtoType = None
