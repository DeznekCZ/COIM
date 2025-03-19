class IVirtualResourceMiningEntity:

    def __init__(self):
        self.Description = str(0)
        self.ProductToMine = None
        self.CapacityOfMine = None
        self.QuantityLeftToMine = None
        self.NotifyOnLowReserve = False
class WellInjectionPump:

    def __init__(self):
        self.Prototype = None
        self.CanBePaused = False
        self.Upgrader = None
        self.SoundParams = None
        self.EmissionIntensity = None
        self.MaxMonthlyUnityConsumed = None
        self.MonthlyUnityConsumed = None
        self.UpointsCategoryId = None
        self.IsCargoAffectedByGeneralPriority = False
        self.CurrentState = None
        self.CanDisableLogisticsInput = False
        self.CanDisableLogisticsOutput = False
        self.LogisticsInputMode = None
        self.LogisticsOutputMode = None
        self.PowerRequired = None
        from Mafi import Option
        self.ElectricityConsumer = Option()
        from Mafi import Option
        self.ComputingConsumer = Option()
        self.Maintenance = None
        self.AnimationParams = None
        self.AnimationStatesProvider = None
        self.IsBoostRequested = False
        self.IsBoosted = False
        self.BoostCost = None
        from Mafi import Option
        self.UnityConsumer = Option()
        from Mafi import Option
        self.LastRecipeInProgress = Option()
        self.WorkedThisTick = False
        self.ProgressPerc = None
        self.RecipeProductionTicks = None
        self.Utilization = None
        self.RecipesAssigned = None
        self.SpeedFactor = None
        self.DurationMultiplier = None
        from Mafi import Option
        self.CustomTitle = Option()
        self.GeneralPriority = int(0)
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
        self.MaintenanceCosts = None
        self.IsIdleForMaintenance = False
        self.ComputingRequired = None
        self.IsSoundOn = False
        self.WorkersNeeded = int(0)
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
        self.UpgradeNonGeneric = None
        self.IsWasteDisposal = False
        self.UseAllRecipesAtStartOrAfterUnlock = False
        self.AnimationParams = None
        self.Id = None
        self.Layout = None
        self.Ports = None
        self.CloningDisabled = False
        self.IsUnique = False
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.IconPath = str(0)
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class WellInjectionPumpProtoBuilder:

    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None
class State:

    def __init__(self):
        pass

class WellPump:

    def __init__(self):
        self.Prototype = None
        self.Description = str(0)
        self.ProductToMine = None
        self.CapacityOfMine = None
        self.QuantityLeftToMine = None
        self.NotifyOnLowReserve = False
        self.CanBePaused = False
        self.Upgrader = None
        self.SoundParams = None
        self.EmissionIntensity = None
        self.MaxMonthlyUnityConsumed = None
        self.MonthlyUnityConsumed = None
        self.UpointsCategoryId = None
        self.IsCargoAffectedByGeneralPriority = False
        self.CurrentState = None
        self.CanDisableLogisticsInput = False
        self.CanDisableLogisticsOutput = False
        self.LogisticsInputMode = None
        self.LogisticsOutputMode = None
        self.PowerRequired = None
        from Mafi import Option
        self.ElectricityConsumer = Option()
        from Mafi import Option
        self.ComputingConsumer = Option()
        self.Maintenance = None
        self.AnimationParams = None
        self.AnimationStatesProvider = None
        self.IsBoostRequested = False
        self.IsBoosted = False
        self.BoostCost = None
        from Mafi import Option
        self.UnityConsumer = Option()
        from Mafi import Option
        self.LastRecipeInProgress = Option()
        self.WorkedThisTick = False
        self.ProgressPerc = None
        self.RecipeProductionTicks = None
        self.Utilization = None
        self.RecipesAssigned = None
        self.SpeedFactor = None
        self.DurationMultiplier = None
        from Mafi import Option
        self.CustomTitle = Option()
        self.GeneralPriority = int(0)
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
        self.MaintenanceCosts = None
        self.IsIdleForMaintenance = False
        self.ComputingRequired = None
        self.IsSoundOn = False
        self.WorkersNeeded = int(0)
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
        self.UpgradeNonGeneric = None
        self.IsWasteDisposal = False
        self.UseAllRecipesAtStartOrAfterUnlock = False
        self.AnimationParams = None
        self.Id = None
        self.Layout = None
        self.Ports = None
        self.CloningDisabled = False
        self.IsUnique = False
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.IconPath = str(0)
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class WellPumpProtoBuilder:

    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None
class State:

    def __init__(self):
        pass

