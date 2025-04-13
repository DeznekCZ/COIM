
class Hospital:
    def __init__(self):
        self.ProvidedNeed = None
        self.Prototype = None
        self.CanBePaused = False
        self.Upgrader = None
        self.PowerRequired = None
        self.EmissionIntensity = None
        self.AnimationParams = None
        self.AnimationStatesProvider = None
        self.MaintenanceCosts = None
        self.Maintenance = None
        self.IsIdleForMaintenance = False
        self.CurrentState = None
        from Mafi import Option
        self.Settlement = Option()
        self.BuffersPerSlot = None
        self.UnityProductionMultiplier = None
        self.BuffersCount = 0
        self.ElectricityConsumer = Option()
        self.SupportedProducts = None
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

    class State:
        Paused = None
        Broken = None
        Working = None
        MissingInput = None
        MissingWorkers = None
        NotEnoughPower = None
        def __init__(self):
            self.value__ = 0

class HospitalProto:
    def __init__(self):
        self.EntityType = None
        self.PopsNeed = None
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
        self.PowerRequired = None
        self.BuffersCount = 0
        self.CapacityPerBuffer = None
        self.EmissionIntensity = None
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.CannotBeBuiltByPlayer = False
        self.ConstructionDurationPerProduct = None
        self.VehicleGoalHeightAllowedRange = None
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False
        self.IsInitialized = False

class ISettlementModuleProto:
    def __init__(self):
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.IsUnique = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.Mod = None

class ISettlementModuleForNeedProto:
    def __init__(self):
        self.PopsNeed = None
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.IsUnique = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.Mod = None

class ISettlementServiceModule:
    def __init__(self):
        self.ProvidedNeed = None
        self.DefaultTitle = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False

class ISettlementSquareModule:
    def __init__(self):
        from Mafi import Option
        self.Settlement = Option()
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.Value = None
        self.ConstructionCost = None
        self.ConstructionState = None
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.DefaultTitle = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False

class ISettlementSquareModuleProto:
    def __init__(self):
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.IsUnique = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.Mod = None

class Settlement:
    HealthBonusPerFoodCategory = None
    def __init__(self):
        self.TotalHousingCapacity = 0
        self.UpointsCapacity = None
        self.Population = 0
        self.FreeHousingCapacity = 0
        self.AmountStarvedToDeathLastMonth = 0
        self.ArePeopleStarving = False
        self.HousingModules = None
        self.SquareModules = None
        self.MonthsOfFood = 0
        self.ConsumptionMultiplier = None
        self.AllFoodModules = None
        self.HasNoFoodModule = False
        self.FoodTypesMap = None
        self.NominalHealthLastDayFromFood = None
        self.FoodCategoriesWithHealthSatisfaction = None
        self.CategoriesWithHealthBenefitTotal = 0
        self.NumberOfWorkersWithheld = 0
        self.AllHospitals = None
        self.LandfillInSettlement = None
        self.LandfillLimitForCurrentPopulation = None
        self.NominalHealthPenaltyFromWasteLastDay = None
        self.BaseLandfillPerPopPerDay = None
        self.AllLandfillModules = None
        self.BioWasteModules = None
        self.RecyclablesModules = None
        self.AllNeeds = None
        self.DiseaseMortalityDeductionLastDay = None
        self.BioWasteReductionMultiplier = None
        from Mafi import Fix64
        self.WoodToBiomassMultiplier = Fix64()
        self.RecyclableToLandfillMultiplier = None

    class FoodCategoryData:
        def __init__(self):
            self.PopDaysSupplyTemp = 0
            self.PopsAssignedTemp = 0
            self.PopsDaysSupplyLeftTemp = 0
            self.Prototype = None
            self.FoodTypes = None

    class FoodData:
        def __init__(self):
            self.PopDaysSupplyTemp = 0
            self.PopsAssignedTemp = 0
            self.SupplyTemp = None
            self.SupplyLeft = None
            self.EstimatedMonthlyConsumption = None
            self.UpointsGivenLastDay = None
            self.PopsDaysSupplyLeftTemp = 0
            self.WasSatisfiedLastUpdate = False
            self.Prototype = None
            self.Category = None
            self.FoodLeftToConsume = None
            self.DailyRecords = None

class PopNeed:
    def __init__(self):
        self.IsFoodNeed = False
        self.IsHealthcareNeed = False
        self.ShouldBeShown = False
        self.Proto = None
        self.UnityAfterLastUpdate = None
        self.PossibleMaxAfterLastUpdate = None
        self.WasNotFullySatisfiedLastDay = False
        self.PercentSatisfiedLastMonth = None
        self.NeedIncreaseFromHousing = None
        self.ConsumptionMultiplier = None
        self.ModulesProvidingTheNeed = None
        self.LastUsedModuleIndex = 0
        self.DailyRecords = None
        self.PercentSatisfiedAfterLastUpdate = None
        self.CoverageStats = None

class DailyUpointsRecord:
    def __init__(self):
        self.PercentSatisfied = None
        self.Unity = None
        self.PossibleMax = None

class SettlementDecorationModule:
    def __init__(self):
        self.Prototype = None
        self.CanBePaused = False
        from Mafi import Option
        self.Settlement = Option()
        self.Upgrader = None
        self.AreParticlesEnabled = False
        self.CoreSize = None
        self.Position = None
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

class SettlementDecorationModuleProto:
    def __init__(self):
        self.EntityType = None
        self.Upgrade = None
        self.UpgradeNonGeneric = None
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
        self.UpointsBonusToNearbyHousing = None
        self.BonusRange = 0
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.CannotBeBuiltByPlayer = False
        self.ConstructionDurationPerProduct = None
        self.VehicleGoalHeightAllowedRange = None
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False
        self.IsInitialized = False

class SettlementFoodModule:
    def __init__(self):
        self.Prototype = None
        self.CanBePaused = False
        self.Settlement = None
        self.CurrentState = None
        self.IsOperational = False
        self.Upgrader = None
        self.BuffersPerSlot = None
        self.SupportedProducts = None
        self.CanDisableLogisticsInput = False
        self.CanDisableLogisticsOutput = False
        self.LogisticsInputMode = None
        self.LogisticsOutputMode = None
        from Mafi import Option
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

    class State:
        Paused = None
        Broken = None
        Working = None
        MissingInput = None
        MissingWorkers = None
        NoProductAssigned = None
        def __init__(self):
            self.value__ = 0

class SettlementFoodModuleProto:
    def __init__(self):
        self.EntityType = None
        self.StayConnectedToLogisticsByDefault = False
        self.Upgrade = None
        self.UpgradeNonGeneric = None
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
        self.BuffersCount = 0
        self.CapacityPerBuffer = None
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.CannotBeBuiltByPlayer = False
        self.ConstructionDurationPerProduct = None
        self.VehicleGoalHeightAllowedRange = None
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False
        self.IsInitialized = False

class SettlementHousingEntityFactory:
    def __init__(self):
        pass


class SettlementHousingModule:
    def __init__(self):
        self.Prototype = None
        self.CanBePaused = False
        from Mafi import Option
        self.Settlement = Option()
        self.Upgrader = None
        self.AchievedUnityIncreaseIndexLastUpdate = 0
        self.Population = 0
        self.Capacity = 0
        self.UpointsCapacity = None
        self.CoreSize = None
        self.Position = None
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

class SettlementHousingModuleProto:
    def __init__(self):
        self.EntityType = None
        self.Upgrade = None
        self.UpgradeNonGeneric = None
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
        self.UnityIncreases = None
        self.NeedsIncreases = None
        self.Capacity = 0
        self.UpointsCapacity = None
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
            self.MaterialPaths = None
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

class SettlementHousingProtoBuilder:
    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None

    class State:
        def __init__(self):
            self.m_materialPaths = None
            self.Builder = None

class SettlementModuleProto:
    def __init__(self):
        self.EntityType = None
        self.ElectricityConsumed = None
        self.AnimationParams = None
        self.Recipe = None
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
        self.PopsNeed = None
        self.InputProduct = None
        from Mafi import Option
        self.OutputProduct = Option()
        self.InputBufferCapacity = None
        self.OutputBufferCapacity = None
        self.AnimateOnlyWhenServicingPops = False
        self.StayConnectedToLogisticsByDefault = False
        self.EmissionIntensity = None
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.CannotBeBuiltByPlayer = False
        self.ConstructionDurationPerProduct = None
        self.VehicleGoalHeightAllowedRange = None
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False
        self.IsInitialized = False

class SettlementModuleProtoBuilder:
    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None

    class State:
        def __init__(self):
            self.Builder = None

class SettlementModulesSlotBasedValidator:
    def __init__(self):
        self.Priority = None

class SettlementServiceModule:
    def __init__(self):
        self.CanBePaused = False
        self.Maintenance = None
        self.IsIdleForMaintenance = False
        self.EmissionIntensity = None
        self.CurrentState = None
        self.AnimationParams = None
        self.AnimationStatesProvider = None
        self.CanDisableLogisticsInput = False
        self.CanDisableLogisticsOutput = False
        self.LogisticsInputMode = None
        self.LogisticsOutputMode = None
        from Mafi import Option
        self.ElectricityConsumer = Option()
        self.IsCargoAffectedByGeneralPriority = False
        self.Settlement = Option()
        self.ProvidedNeed = None
        self.InputProduct = None
        self.InputProductCapacity = None
        self.OutputProduct = None
        self.OutputProductCapacity = None
        self.CustomTitle = Option()
        self.GeneralPriority = 0
        self.IsGeneralPriorityVisible = False
        self.Ports = None
        self.Value = None
        self.ConstructionCost = None
        self.Prototype = None
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
        self.PowerRequired = None
        self.MaintenanceCosts = None
        self.TotalInputLastMonth = None
        self.TotalInputThisMonth = None
        self.TotalOutputLastMonth = None
        self.TotalOutputThisMonth = None

    class State:
        Paused = None
        Broken = None
        Working = None
        MissingInput = None
        MissingWorkers = None
        NotEnoughPower = None
        FullOutput = None
        def __init__(self):
            self.value__ = 0

class SettlementsManager:
    def __init__(self):
        self.LastPopulationDiff = 0
        self.AmountStarvedToDeathLastMonth = 0
        self.ArePeopleStarving = False
        self.TotalHousingCapacity = 0
        self.FreeHousingCapacity = 0
        self.NumberOfHomeless = 0
        self.StarvingHomelessCountDays = 0
        self.NumberOfStarvingPopsWithheld = 0
        self.OnWorkersRemoved = None
        self.OnWorkersAdded = None
        self.SettlementsCount = 0
        self.MonthsOfFood = 0
        self.Settlements = None
        self.LastHomelessPenalty = None
        self.TotalHousingStats = None
        self.TotalPopulationStats = None
        self.NewPopsFromAdoptions = None

class PopsAdditionReason:
    RefugeesOrAdopted = None
    Other = None
    def __init__(self):
        self.value__ = 0

class IPopulationConfig:
    def __init__(self):
        self.StartingPopulation = 0

class SettlementTransformer:
    def __init__(self):
        self.CanBePaused = False
        self.Maintenance = None
        self.IsIdleForMaintenance = False
        self.ProvidedNeed = None
        self.Settlement = None
        self.PowerRequired = None
        from Mafi import Option
        self.ElectricityConsumer = Option()
        self.CustomTitle = Option()
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.Ports = None
        self.Value = None
        self.ConstructionCost = None
        self.Prototype = None
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

class SettlementTransformerProto:
    def __init__(self):
        self.EntityType = None
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
        self.PopsNeed = None
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.CannotBeBuiltByPlayer = False
        self.ConstructionDurationPerProduct = None
        self.VehicleGoalHeightAllowedRange = None
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False
        self.IsInitialized = False

class SettlementWasteModule:
    def __init__(self):
        self.CurrentState = None
        self.CanBePaused = False
        self.LogisticsOutputControl = None
        self.StoredQuantity = None
        self.Prototype = None
        from Mafi import Option
        self.StoredProduct = Option()
        self.Capacity = None
        self.CurrentQuantity = None
        self.PercentFull = None
        self.IsEmpty = False
        self.IsNotEmpty = False
        self.IsFull = False
        self.IsNotFull = False
        self.LogisticsInputControl = None
        self.IsLogisticsInputDisabled = False
        self.IsLogisticsOutputDisabled = False
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

    class State:
        Paused = None
        Working = None
        MissingWorkers = None
        FullOutput = None
        def __init__(self):
            self.value__ = 0

class SettlementWasteModuleProto:
    def __init__(self):
        self.EntityType = None
        self.Recipe = None
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
        self.ProductAccepted = None
        self.Capacity = None
        self.TransferLimit = None
        self.TransferLimitDuration = None
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.CannotBeBuiltByPlayer = False
        self.ConstructionDurationPerProduct = None
        self.VehicleGoalHeightAllowedRange = None
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False
        self.IsInitialized = False
