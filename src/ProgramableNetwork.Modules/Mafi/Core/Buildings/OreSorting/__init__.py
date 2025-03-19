class OreSortingPlant:

    def __init__(self):
        self.Prototype = None
        self.AnimationParams = None
        self.AnimationStatesProvider = None
        self.AreParticlesEnabled = False
        from Mafi import Option
        self.ElectricityConsumer = Option()
        self.Maintenance = None
        self.IsIdleForMaintenance = False
        self.CanBePaused = False
        self.CurrentState = None
        self.AssignedOutputs = None
        self.AllowNonAssignedOutput = False
        self.DoNotAcceptSingleProduct = False
        self.OutputBuffers = None
        self.AllowedProducts = None
        self.ProductsData = None
        self.AllReservedJobs = None
        self.Capacity = None
        self.CapacityLeft = None
        self.SortedPerDuration = None
        self.MixedTotal = None
        self.PercentFull = None
        self.IsEmpty = False
        self.IsNotEmpty = False
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
        self.MaintenanceCosts = None
        self.PowerRequired = None
class State:

    def __init__(self):
        pass

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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
