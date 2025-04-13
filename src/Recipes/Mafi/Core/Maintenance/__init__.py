
class IEntityMaintenanceProvider:
    def __init__(self):
        self.Costs = None
        self.Status = None
        self.RepairCost = None

class NoMaintenanceProvider:
    Instance = None
    def __init__(self):
        self.Costs = None
        self.Status = None
        self.RepairCost = None

class EntityMaintenanceProvider:
    def __init__(self):
        self.ProtoToken = 0
        self.Costs = None
        self.Status = None
        self.IsDestroyed = False
        self.Priority = 0
        self.RepairCost = None
        self.Entity = None

class IEntityMaintenanceProvidersFactory:
    def __init__(self):
        pass


class EntityMaintenanceProvidersFactory:
    def __init__(self):
        pass


class IMaintainedEntity:
    def __init__(self):
        self.MaintenanceCosts = None
        self.Maintenance = None
        self.IsIdleForMaintenance = False
        self.GeneralPriority = 0
        self.IsGeneralPriorityVisible = False
        self.IsCargoAffectedByGeneralPriority = False
        self.DefaultTitle = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False

class IMaintenanceConfig:
    def __init__(self):
        self.BufferMaxCapacity = None
        self.ReliabilityIssuesStartAt = None
        self.MaxBreakdownChance = None
        self.MaxReplenishSpeed = None
        self.IdleMaintenanceMultiplier = None
        self.BaseReplenishPerMonth = None
        self.BrokenDurationMin = None
        self.BrokenDurationMax = None
        self.DailyBreakdownChanceWhenShouldBeBroken = None

class MaintenanceConfig:
    def __init__(self):
        self.BufferMaxCapacity = None
        self.ReliabilityIssuesStartAt = None
        self.MaxBreakdownChance = None
        self.MaxReplenishSpeed = None
        self.IdleMaintenanceMultiplier = None
        self.BaseReplenishPerMonth = None
        self.BrokenDurationMin = None
        self.BrokenDurationMax = None
        self.DailyBreakdownChanceWhenShouldBeBroken = None

class MaintenanceCosts:
    def __init__(self):
        self.Product = None
        self.MaintenancePerMonth = None
        self.MaxMaintenancePerMonth = None
        self.InitialMaintenanceBoost = None

class MaintenanceDepot:
    def __init__(self):
        self.Prototype = None
        self.CanBePaused = False
        self.Upgrader = None
        self.SoundParams = None
        self.EmissionIntensity = None
        self.MaxMonthlyUnityConsumed = None
        self.MonthlyUnityConsumed = None
        from Mafi.Core.Prototypes import Proto
        self.UpointsCategoryId = Proto.ID()

        self.IsCargoAffectedByGeneralPriority = False
        self.CurrentState = None
        self.CanDisableLogisticsInput = False
        self.CanDisableLogisticsOutput = False
        self.LogisticsInputMode = None
        self.LogisticsOutputMode = None
        self.PowerRequired = None
        from Mafi import Option
        self.ElectricityConsumer = Option()
        self.ComputingConsumer = Option()
        self.Maintenance = None
        self.AnimationParams = None
        self.AnimationStatesProvider = None
        self.IsBoostRequested = False
        self.IsBoosted = False
        self.BoostCost = None
        self.UnityConsumer = Option()
        self.LastRecipeInProgress = Option()
        self.WorkedThisTick = False
        self.ProgressPerc = None
        self.RecipeProductionTicks = None
        self.Utilization = None
        self.RecipesAssigned = None
        self.SpeedFactor = None
        self.DurationMultiplier = None
        self.CustomTitle = Option()
        self.GeneralPriority = 0
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
        self.MaintenanceCosts = None
        self.IsIdleForMaintenance = False
        self.ComputingRequired = None
        self.IsSoundOn = False
        self.WorkersNeeded = 0
        self.HasWorkersCached = False

class MaintenanceDepotProto:
    def __init__(self):
        self.EntityType = None
        self.ElectricityConsumed = None
        self.ComputingConsumed = None
        self.Recipes = None
        self.Upgrade = None
        self.UpgradeNonGeneric = None
        self.IsWasteDisposal = False
        self.UseAllRecipesAtStartOrAfterUnlock = False
        self.AnimationParams = None
        from Mafi.Core.Factory.Machines import MachineProto
        self.Id = MachineProto.ID()

        self.Layout = None
        self.Ports = None
        self.CloningDisabled = False
        self.IsUnique = False
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.IconPath = ""
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.MaintenanceBufferExtraCapacity = None
        self.ConsumedPowerPerTick = None
        self.BuffersMultiplier = None
        self.EmissionWhenRunning = None
        self.DisableLogisticsByDefault = False
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.CannotBeBuiltByPlayer = False
        self.ConstructionDurationPerProduct = None
        self.VehicleGoalHeightAllowedRange = None
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False
        self.IsInitialized = False

class MaintenanceManager:
    def __init__(self):
        self.NotEnoughMaintenanceThisMonth = None
        self.ProvidedProducts = None
        self.CanSlowDownIfBroken = False
        self.ConsumptionMultiplier = None
        self.MaintenanceBuffers = None
        self.Config = None

    class ConsumptionPerProto:
        def __init__(self):
            self.Proto = None
            self.LastTick = None
            self.EntitiesTotal = 0

    class ConsumptionLastTick:
        Empty = None
        def __init__(self):
            self.Demand = None
            self.MaxPossibleConsumption = None

class IMaintenanceBufferReadonly:
    def __init__(self):
        self.Product = None
        self.Quantity = None
        self.Capacity = None
        self.DeltaLastMonth = None
        self.MonthlyNeededMaintenance = None
        self.MonthlyNeededMaintenanceMax = None
        self.ShouldBeLastDeltaReported = False
        self.ShouldShowInUi = False
        self.ProducedTotalStats = None
        self.ConsumedTotalStats = None

class MaintenanceProtoParam:
    def __init__(self):
        self.AllowedProtoType = None
        self.QuickRepairCost = None

class MaintenanceStatus:
    def __init__(self):
        self.MissingPointsToFull = None
        self.IsBroken = False
        self.MaintenancePointsCurrent = None
        self.MaintenancePointsMax = None
        self.CurrentBreakdownChance = None
        from Mafi import Fix32
        self.BrokenDurationDays = Fix32()

class QuickRepairEntityCmd:
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
        self.EntityId = None
