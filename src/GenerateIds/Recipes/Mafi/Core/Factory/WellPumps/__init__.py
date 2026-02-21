
class IVirtualResourceMiningEntity:
    def __init__(self):
        self.Description = ""
        self.ProductToMine = None
        self.CapacityOfMine = None
        self.QuantityLeftToMine = None
        self.NotifyOnLowReserve = False

class WellInjectionPump:
    def __init__(self):
        self.Prototype = None
        self.UpgradableProto = None
        self.CanBePaused = False
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
        self.VirtualOutputMultiplier = None
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
        self.MaintenanceCosts = None
        self.IsIdleForMaintenance = False
        self.ComputingRequired = None
        self.IsSoundOn = False
        self.WorkersNeeded = 0
        self.HasWorkersCached = False

class WellInjectionPumpAdditionValidator:
    def __init__(self):
        self.Priority = None

class WellInjectionPumpProto:
    def __init__(self):
        self.EntityType = None
        self.ElectricityConsumed = None
        self.ComputingConsumed = None
        self.Recipes = None
        self.Upgrade = None
        self.TierData = None
        self.IsWasteDisposal = False
        self.UseAllRecipesAtStartOrAfterUnlock = False
        self.AnimationParams = None
        from Mafi.Core.Factory.Machines import MachineProto
        self.Id = MachineProto.ID()

        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.IconPath = ""
        self.CanMoveUpDownWhenInvalidPlacement = False
        self.IsUnique = False
        self.CloningDisabled = False
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
        self.TerrainProductRequired = None
        self.ConsumedPowerPerTick = None
        self.BuffersMultiplier = None
        self.EmissionWhenRunning = None
        self.DisableLogisticsByDefault = False
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

class WellInjectionPumpProtoBuilder:
    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None

    class State:
        def __init__(self):
            self.Builder = None

class WellPump:
    def __init__(self):
        self.Prototype = None
        self.Description = ""
        self.ProductToMine = None
        self.CapacityOfMine = None
        self.QuantityLeftToMine = None
        self.NotifyOnLowReserve = False
        self.UpgradableProto = None
        self.CanBePaused = False
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
        self.VirtualOutputMultiplier = None
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
        self.MaintenanceCosts = None
        self.IsIdleForMaintenance = False
        self.ComputingRequired = None
        self.IsSoundOn = False
        self.WorkersNeeded = 0
        self.HasWorkersCached = False

class WellPumpAdditionValidator:
    def __init__(self):
        self.Priority = None

class WellPumpProto:
    def __init__(self):
        self.EntityType = None
        self.ElectricityConsumed = None
        self.ComputingConsumed = None
        self.Recipes = None
        self.Upgrade = None
        self.TierData = None
        self.IsWasteDisposal = False
        self.UseAllRecipesAtStartOrAfterUnlock = False
        self.AnimationParams = None
        from Mafi.Core.Factory.Machines import MachineProto
        self.Id = MachineProto.ID()

        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.IconPath = ""
        self.CanMoveUpDownWhenInvalidPlacement = False
        self.IsUnique = False
        self.CloningDisabled = False
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
        self.ReserveDescription = ""
        self.MinedProduct = None
        self.NotifyWhenBelow = None
        self.ConsumedPowerPerTick = None
        self.BuffersMultiplier = None
        self.EmissionWhenRunning = None
        self.DisableLogisticsByDefault = False
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

class WellPumpProtoBuilder:
    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None

    class State:
        def __init__(self):
            self.Builder = None
