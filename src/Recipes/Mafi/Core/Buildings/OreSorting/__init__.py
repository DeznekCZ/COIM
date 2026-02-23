
class OreSortingPlant:
    MAX_PRODUCTS = 0
    def __init__(self):
        self.OutputPortsCount = 0
        self.ZoneMask = None
        self.UpgradableProto = None
        self.Prototype = None
        self.AnimationParams = None
        self.AnimationStatesProvider = None
        self.AreParticlesEnabled = False
        from Mafi import Option
        self.ElectricityConsumer = Option()
        self.Maintenance = None
        self.IsIdleForMaintenance = False
        self.AlwaysUseCustomPfTargetTiles = False
        self.CanBePaused = False
        self.CurrentState = None
        self.AssignedOutputs = None
        self.AllowNonAssignedOutput = False
        self.AutoAssignBuffers = False
        self.DoNotAcceptSingleProduct = False
        self.OutputBuffers = None
        self.AllowedProducts = None
        self.ProductsData = None
        self.AllReservedJobs = None
        self.Capacity = None
        self.CapacityLeftForMixed = None
        self.SortedPerDuration = None
        self.MixedTotal = None
        self.PercentFull = None
        self.IsEmpty = False
        self.IsNotEmpty = False
        self.ReservedTotal = None
        self.OngoingMonthlyData = None
        self.ProductivityCounterHistory = None
        self.ProductivityCounterLabels = None
        self.CanDisableLogisticsInput = False
        self.CanDisableLogisticsOutput = False
        self.LogisticsInputMode = None
        self.LogisticsOutputMode = None
        self.CustomTitle = Option()
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
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
        self.PowerRequired = None
        self.AllSupportedProducts = None

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

    class OreSortingPlantProductData:
        def __init__(self):
            self.UnsortedQuantity = None
            self.BeingSorted = None
            self.ToWaste = None
            self.SortedLastMonth = None
            self.SortedThisMonth = None
            self.NotifyIfFullOutput = False
            self.OutputPort = None
            self.CanAcceptMoreTrucksForUi = False
            self.CanAcceptMoreTrucks = False
            self.Reserved = None
            self.Buffer = None
            self.OutputPorts = None
            self.CanBeWasted = False

class OreSorterConfigExtensions:
    def __init__(self):
        pass


class OreSortingPlantProto:
    def __init__(self):
        self.EntityType = None
        self.ConversionLoss = None
        self.Duration = None
        self.QuantityPerDuration = None
        self.ElectricityConsumed = None
        self.AnimationParams = None
        self.Upgrade = None
        self.TierData = None
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
        self.InputBufferCapacity = None
        self.OutputBuffersCapacity = None
        self.AcceptAllTrucksUntilFreeCapacity = None
        self.CustomPfTargetTiles = None
        self.CustomPfTargetTilesForRetry = None
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
        def __init__(self):
            self.PrefabPath = ""
            self.PrefabOrigin = None
            self.IconPath = ""
            self.YawForGeneratedIcon = None
            self.VisualizedLayers = None
            self.Categories = None
            self.AnimationDataAssetPathBase = ""
            self.SmoothPileObjectPath = ""
            self.PileTextureParams = None
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

class OreSortingPlantsManager:
    def __init__(self):
        self.IsSortingEnabled = False

class AddProductToSortCmd:
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
        self.SortingPlantId = None
        from Mafi.Core.Products import ProductProto
        self.ProductId = ProductProto.ID()


class ToggleSorterAutoAssignBuffers:
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
        self.SortingPlantId = None

class SetProductPortCmd:
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
        self.SortingPlantId = None
        from Mafi.Core.Products import ProductProto
        self.ProductId = ProductProto.ID()

        self.PortIndex = 0
        self.ClearPortInstead = False

class RemoveProductToSortCmd:
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
        self.SortingPlantId = None
        from Mafi.Core.Products import ProductProto
        self.ProductId = ProductProto.ID()


class SortingPlantNoSingleProductCmd:
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
        self.SortingPlantId = None
        self.DoNotAcceptSingleProduct = False

class SortingPlantSetBlockedProductAlertCmd:
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
        self.SortingPlantId = None
        from Mafi.Core.Products import ProductProto
        self.ProductId = ProductProto.ID()

        self.IsAlertEnabled = False
