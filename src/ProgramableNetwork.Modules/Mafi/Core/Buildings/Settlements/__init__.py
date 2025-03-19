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
        self.BuffersCount = int(0)
        from Mafi import Option
        self.ElectricityConsumer = Option()
        self.SupportedProducts = None
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

    def __init__(self):
        pass

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
        self.IconPath = str(0)
        self.Id = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class ISettlementModuleProto:

    def __init__(self):
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.IsUnique = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.Id = None
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
        self.Id = None
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
        from Mafi import Option
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
        self.Id = None
        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.Mod = None
class Settlement:

    def __init__(self):
        self.TotalHousingCapacity = int(0)
        self.UpointsCapacity = None
        self.Population = int(0)
        self.FreeHousingCapacity = int(0)
        self.AmountStarvedToDeathLastMonth = int(0)
        self.ArePeopleStarving = False
        self.HousingModules = None
        self.SquareModules = None
        self.MonthsOfFood = int(0)
        self.ConsumptionMultiplier = None
        self.AllFoodModules = None
        self.HasNoFoodModule = False
        self.FoodTypesMap = None
        self.NominalHealthLastDayFromFood = None
        self.FoodCategoriesWithHealthSatisfaction = None
        self.CategoriesWithHealthBenefitTotal = int(0)
        self.NumberOfWorkersWithheld = int(0)
        self.AllHospitals = None
        self.LandfillInSettlement = None
        self.LandfillLimitForCurrentPopulation = None
        self.NominalHealthPenaltyFromWasteLastDay = None
        self.BaseLandfillPerPopPerDay = None
        self.AllLandfillModules = None
        self.BioWasteModules = None
        self.RecyclablesModules = None
class FoodCategoryData:

    def __init__(self):
        self.PopDaysSupplyTemp = int(0)
        self.PopsAssignedTemp = int(0)
        self.PopsDaysSupplyLeftTemp = int(0)
class FoodData:

    def __init__(self):
        self.PopDaysSupplyTemp = int(0)
        self.PopsAssignedTemp = int(0)
        self.SupplyTemp = None
        self.SupplyLeft = None
        self.EstimatedMonthlyConsumption = None
        self.UpointsGivenLastDay = None
        self.PopsDaysSupplyLeftTemp = int(0)
        self.WasSatisfiedLastUpdate = False
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
class DailyUpointsRecord:

    def __init__(self):
        pass

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
        self.IconPath = str(0)
        self.Id = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
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
        self.GeneralPriority = int(0)
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

    def __init__(self):
        pass

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
        self.IconPath = str(0)
        self.Id = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
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
        self.AchievedUnityIncreaseIndexLastUpdate = int(0)
        self.Population = int(0)
        self.Capacity = int(0)
        self.UpointsCapacity = None
        self.CoreSize = None
        self.Position = None
        from Mafi import Option
        self.CustomTitle = Option()
        self.GeneralPriority = int(0)
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
        self.IconPath = str(0)
        self.Id = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class Gfx:

    def __init__(self):
        self.PrefabPath = str(0)
        self.PrefabOrigin = None
        self.IconPath = str(0)
        self.VisualizedLayers = None
        self.Categories = None
class SettlementHousingProtoBuilder:

    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None
class State:

    def __init__(self):
        pass

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
        self.IconPath = str(0)
        self.Id = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class SettlementModuleProtoBuilder:

    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None
class State:

    def __init__(self):
        pass

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
        from Mafi import Option
        self.Settlement = Option()
        self.ProvidedNeed = None
        self.InputProduct = None
        self.InputProductCapacity = None
        self.OutputProduct = None
        self.OutputProductCapacity = None
        from Mafi import Option
        self.CustomTitle = Option()
        self.GeneralPriority = int(0)
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
        self.PowerRequired = None
        self.MaintenanceCosts = None
class State:

    def __init__(self):
        pass

class SettlementsManager:

    def __init__(self):
        self.LastPopulationDiff = int(0)
        self.AmountStarvedToDeathLastMonth = int(0)
        self.ArePeopleStarving = False
        self.TotalHousingCapacity = int(0)
        self.FreeHousingCapacity = int(0)
        self.NumberOfHomeless = int(0)
        self.StarvingHomelessCountDays = int(0)
        self.NumberOfStarvingPopsWithheld = int(0)
        self.OnWorkersRemoved = None
        self.OnWorkersAdded = None
        self.SettlementsCount = int(0)
        self.MonthsOfFood = int(0)
        self.Settlements = None
class PopsAdditionReason:

    def __init__(self):
        pass

class IPopulationConfig:

    def __init__(self):
        self.StartingPopulation = int(0)
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
        from Mafi import Option
        self.CustomTitle = Option()
        self.GeneralPriority = int(0)
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
        self.IconPath = str(0)
        self.Id = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
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
        from Mafi import Option
        self.CustomTitle = Option()
        self.GeneralPriority = int(0)
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

    def __init__(self):
        pass

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
        self.IconPath = str(0)
        self.Id = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
