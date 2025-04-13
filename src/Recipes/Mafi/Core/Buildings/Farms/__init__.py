
class AnimalFarm:
    MAX_SLIDES_STEPS = 0
    def __init__(self):
        self.Prototype = None
        self.CanBePaused = False
        self.CurrentState = None
        self.Upgrader = None
        self.AnimationParams = None
        self.AnimationStatesProvider = None
        self.FoodInputBuffer = None
        self.WaterInputBuffer = None
        from Mafi import Option
        self.ProductProducedBuffer = Option()
        from Mafi import Fix32
        self.CarcassPerMonth = Fix32()
        self.CarcassBuffer = None
        self.AnimalsCount = 0
        self.CapacityLeft = 0
        self.AnimalsBornPerMonth = Fix32()
        self.IsSlaughteringEnabled = False
        self.IsGrowthPaused = False
        self.AreAnimalsStarving = False
        self.AreAnimalsMissingWater = False
        self.SlaughterStep = 0
        self.SlaughterLimit = None
        self.CanDisableLogisticsInput = False
        self.CanDisableLogisticsOutput = False
        self.LogisticsInputMode = None
        self.LogisticsOutputMode = None
        self.CustomTitle = Option()
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.Ports = None
        self.Value = None
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
        self.OutputBuffers = None

    class State:
        Paused = None
        Working = None
        MissingWorkers = None
        MissingFood = None
        MissingWater = None
        NoAnimals = None
        FullOutput = None
        def __init__(self):
            self.value__ = 0

class AnimalFarmConfigExtensions:
    def __init__(self):
        pass


class AnimalFarmProto:
    def __init__(self):
        self.EntityType = None
        self.Upgrade = None
        self.UpgradeNonGeneric = None
        self.AnimationParams = None
        self.Layout = None
        self.Ports = None
        self.CloningDisabled = False
        self.IsUnique = False
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.IconPath = ""
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.FoodPerAnimalPerMonth = None
        self.ProducedPerAnimalPerMonth = None
        from Mafi import Fix32
        self.CarcassMultiplier = Fix32()
        self.CarcassProto = None
        self.CarcassOutpotPortName = None
        self.AnimalsBornPer100AnimalsPerMonth = Fix32()
        self.AnimalsCapacity = 0
        self.WaterPerAnimalPerMonth = None
        self.Animal = None
        self.FoodBufferCapacity = None
        self.WaterBufferCapacity = None
        self.CarcassBufferCapacity = None
        self.ProducedBufferCapacity = None
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.CannotBeBuiltByPlayer = False
        self.ConstructionDurationPerProduct = None
        self.VehicleGoalHeightAllowedRange = None
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False
        self.IsInitialized = False

class CropProto:
    def __init__(self):
        self.IconPath = ""
        self.MonthsToGrow = 0
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.ProductProduced = None
        self.ConsumedWaterPerDay = None
        self.ConsumedFertilityPerDay = None
        self.MinFertilityToStartGrowth = None
        self.DaysToGrow = 0
        self.DaysToSurviveWithNoWater = None
        self.Graphics = None
        self.IsEmptyCrop = False
        self.RequiresGreenhouse = False
        self.PlantByDefault = False
        self.IsPhantom = False
        self.IsInitialized = False

    class Gfx:
        Empty = None
        def __init__(self):
            self.IconPath = ""
            self.PrefabPath = ""
            self.ScaleVariation = 0.0
            self.WindTimeScale = 0.0
            self.WindWaviness = 0.0
            self.WindAmplitude = 0.0

class Farm:
    FERTILITY_PENALTY_FOR_SAME_CROP = None
    FERTILITY_REPLENISH_MULT_WHEN_ABOVE_100 = None
    CROP_FERTILITY_DEMAND_MULT_WHEN_FERTILITY_ABOVE_100 = None
    STARTING_FERTILITY = None
    NO_YIELD_BEFORE_GROWTH_PERC = None
    FERTILITY_PER_SLIDER_STEP = None
    MAX_FERTILITY_SLIDER_VALUE = None
    CROP_SLOTS_COUNT = 0
    MAX_DAYS_DISABLED = 0
    INPUT_WATER_PORT_NAME = None
    INPUT_FERTILIZER_PORT_NAME = None
    FERTILITY_SLIDER_STEPS = 0
    def __init__(self):
        self.Prototype = None
        self.CanBePaused = False
        self.Maintenance = None
        self.IsIdleForMaintenance = False
        self.CurrentState = None
        self.Upgrader = None
        from Mafi import Option
        self.CurrentCrop = Option()
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
        self.FertilityTargetIndex = 0
        self.FertilityTargetValue = None
        self.FertilityNeededPerDay = None
        self.Schedule = None
        self.ActiveScheduleIndex = 0
        self.LifetimePlantedCropsCount = 0
        self.IsIrrigating = False
        self.ShouldAnimate = False
        self.IsSoundOn = False
        self.SoundParams = None
        self.TotalRotationDurationDays = 0
        self.AvgYieldPerYear = None
        self.CanDisableLogisticsInput = False
        self.CanDisableLogisticsOutput = False
        self.LogisticsInputMode = None
        self.LogisticsOutputMode = None
        self.CustomTitle = Option()
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.Ports = None
        self.Value = None
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
            self.value__ = 0

class Crop:
    def __init__(self):
        self.ConsumedWaterPerDay = None
        self.IsStarted = False
        self.GrowthPercent = None
        self.DrynessPercent = None
        self.WillDrySoon = False
        self.DaysRemaining = 0
        self.IsMissingWater = False
        self.DaysMissingWater = 0
        self.TotalDaysWithoutWater = 0
        self.HarvestWillYieldProducts = False
        self.Yield = None
        self.HarvestReason = None
        self.YieldDeltaDueToFertility = None
        self.YieldDeltaDueToBonusMultiplier = None
        self.YieldLostDueToLackOfWater = None
        self.YieldLostDueToPrematureHarvest = None
        self.DaysWaitingForWaterBeforeGrowthStart = 0
        self.Prototype = None
        self.ConsumedFertilityPerDay = None
        self.ProductProduced = None
        self.HasFertilityPenalty = False

class CropHarvestReason:
    None = None
    HarvestedNormally = None
    PrematureLackOfMaintenance = None
    PrematureNoWater = None
    PrematureNoFertility = None
    PrematureHarvestedByPlayer = None
    PrematureClearedByPlayer = None
    def __init__(self):
        self.value__ = 0

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
        self.ErrorMessage = ""
        self.FarmId = None
        self.CropId = None
        self.ScheduleSlot = 0

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
        self.ErrorMessage = ""
        self.FarmId = None

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
        self.ErrorMessage = ""
        self.FarmId = None
        self.Target = None

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
        self.ErrorMessage = ""
        self.FarmId = None

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
        self.ErrorMessage = ""
        self.AnimalFarmId = None
        self.SlaughterSliderStep = None

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
        self.ErrorMessage = ""
        self.AnimalFarmId = None

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
        self.ErrorMessage = ""
        self.AnimalFarmId = None
        self.NumberToMove = 0

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
        self.ErrorMessage = ""
        self.FarmId = None

class FarmProto:
    def __init__(self):
        self.EntityType = None
        self.Upgrade = None
        self.UpgradeNonGeneric = None
        self.Recipes = None
        self.Layout = None
        self.Ports = None
        self.CloningDisabled = False
        self.IsUnique = False
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.IconPath = ""
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.WaterCollectedPerDay = None
        self.FertilityReplenishPerDay = None
        self.YieldMultiplier = None
        self.DemandsMultiplier = None
        self.HasIrrigationAndFertilizerSupport = False
        self.IsGreenhouse = False
        self.WaterEvaporationPerDay = None
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.CannotBeBuiltByPlayer = False
        self.ConstructionDurationPerProduct = None
        self.VehicleGoalHeightAllowedRange = None
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False
        self.IsInitialized = False

    class Gfx:
        Empty = None
        def __init__(self):
            self.PrefabPath = ""
            self.PrefabOrigin = None
            self.IconPath = ""
            self.VisualizedLayers = None
            self.Categories = None
            self.CropPositions = None
            from Mafi import Option
            self.SprinklerPrefabPath = Option()
            self.SprinklerSoundPath = Option()
            self.IconIsCustom = False
            self.UseInstancedRendering = False
            self.UseSemiInstancedRendering = False
            self.SemiInstancedRenderingExcludedObjects = None
            self.MaxRenderedLod = 0
            self.DisableEmptyChildrenStripping = False
            self.InstancedRendererIndex = None
            self.AnimatedGameObjects = None
            self.AnimationLength = 0.0
            self.HideBlockedPortsIcon = False
            self.Color = None
            self.RendererIndex = 0

class FarmFertileGroundValidator:
    MIN_FARMABLE_THICKNESS = None
    MAX_TILES_WITH_NON_FARMABLE_MATERIAL = None
    def __init__(self):
        self.Priority = None

class FertilizerProductParam:
    def __init__(self):
        self.AllowedProtoType = None
        self.FertilityPerQuantity = None
        self.MaxFertility = None

class LooseProductParam:
    def __init__(self):
        self.AllowedProtoType = None
        from Mafi.Core.Prototypes import Proto
        self.DumpAs = Proto.ID()

