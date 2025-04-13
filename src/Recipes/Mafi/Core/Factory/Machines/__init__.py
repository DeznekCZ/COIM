
class IEntityWithBoost:
    def __init__(self):
        self.IsBoostRequested = False
        self.BoostCost = None
        self.DefaultTitle = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False

class Machine:
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

    class State:
        None = None
        Broken = None
        Paused = None
        NotEnoughWorkers = None
        NotEnoughPower = None
        NotEnoughComputing = None
        NotEnoughInput = None
        InvalidPlacement = None
        OutputFull = None
        NoRecipes = None
        Working = None
        def __init__(self):
            self.value__ = 0

    class WasteInputPortPriorityProvider:
        Instance = None
        def __init__(self):
            pass


    class MachineInputBuffer:
        def __init__(self):
            self.IsNotUsedByCurrentRecipes = False
            self.IsFull = False
            self.IsNotFull = False
            self.IsEmpty = False
            self.IsNotEmpty = False
            self.Quantity = None
            self.ProductQuantity = None
            self.Capacity = None
            self.Product = None
            self.UsableCapacity = None
            self.IsDestroyed = False
            self.MinCapacity = None

    class MachineOutputBuffer:
        def __init__(self):
            self.IsAnyPortConnected = False
            self.IsNotUsedByCurrentRecipes = False
            self.IsRegisteredToLogistics = False
            self.IsFull = False
            self.IsNotFull = False
            self.IsEmpty = False
            self.IsNotEmpty = False
            self.Quantity = None
            self.ProductQuantity = None
            self.Capacity = None
            self.Product = None
            self.UsableCapacity = None
            self.IsDestroyed = False
            self.MinCapacity = None

class MachineCommandsProcessor:
    COST_TO_DISCARD_PRODUCTS = None
    def __init__(self):
        pass


class MachineSetRecipeActiveCmd:
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
        self.MachineId = None
        from Mafi.Core.Factory.Recipes import RecipeProto
        self.RecipeId = RecipeProto.ID()

        self.EnableRecipe = False

class ReorderRecipeCmd:
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
        self.MachineId = None
        from Mafi.Core.Prototypes import Proto
        self.RecipeId = Proto.ID()

        self.IndexDiff = 0

class MachineToggleRecipeActiveCmd:
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
        self.MachineId = None
        from Mafi.Core.Factory.Recipes import RecipeProto
        self.RecipeId = RecipeProto.ID()


class MachineBoostToggleCmd:
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
        self.MachineId = None

class WellPumpAlertSetEnabledCmd:
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
        self.WellPumpId = None
        self.IsEnabled = False

class ClearRecipeProductsCmd:
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
        self.MachineId = None
        from Mafi.Core.Prototypes import Proto
        self.RecipeId = Proto.ID()


class MachineProto:
    BOOST_COST = None
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

    class ID:
        def __init__(self):
            self.Value = ""

    class Gfx:
        Empty = None
        def __init__(self):
            self.PrefabPath = ""
            self.PrefabOrigin = None
            self.IconPath = ""
            self.VisualizedLayers = None
            self.Categories = None
            self.ParticlesParams = None
            self.EmissionsParams = None
            from Mafi import Option
            self.MachineSoundPrefabPath = Option()
            self.HasSign = False
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

class MachineProtoBuilder:
    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None

    class MachineProtoBuilderStateBase:
        def __init__(self):
            self.Builder = None
