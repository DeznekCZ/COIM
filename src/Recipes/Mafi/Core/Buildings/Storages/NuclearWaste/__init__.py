
class NuclearWasteStorage:
    def __init__(self):
        self.Prototype = None
        self.EmissionIntensity = None
        from Mafi import Option
        self.OutputBuffer = Option()
        self.DoNotSendRetiredWasteToOutputPort = False
        self.CanBePaused = False
        self.Upgrader = None
        self.ImportUntilPercent = None
        self.ExportFromPercent = None
        self.CleaningInProgress = False
        self.UsableCapacity = None
        self.AssignedInputs = None
        self.AssignedOutputs = None
        self.AllowNonAssignedOutput = False
        self.LogisticsInputControl = None
        self.LogisticsOutputControl = None
        self.IsGeneralPriorityVisible = False
        self.ImportPriority = 0
        self.ExportPriority = 0
        self.AreOnlyAssignedVehiclesAllowed = False
        self.AllVehicles = None
        self.AreAlertsAvailable = False
        self.AlertWhenAboveEnabled = False
        self.AlertWhenAbove = None
        self.AlertWhenBelowEnabled = False
        self.AlertWhenBelow = None
        self.PowerRequired = None
        self.ElectricityConsumer = Option()
        self.StoredProduct = Option()
        self.Capacity = None
        self.CurrentQuantity = None
        self.PercentFull = None
        self.IsEmpty = False
        self.IsNotEmpty = False
        self.IsFull = False
        self.IsNotFull = False
        self.IsLogisticsInputDisabled = False
        self.IsLogisticsOutputDisabled = False
        self.CustomTitle = Option()
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
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

class NuclearWasteStorageToggleOutputPortCmd:
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
        self.StorageId = None

class RadioactiveWasteParam:
    def __init__(self):
        self.AllowedProtoType = None
        self.YearsUntilSafeToDispose = 0
        from Mafi.Core.Products import ProductProto
        self.TransferIntoProduct = ProductProto.ID()


class WasteAgeTracker:
    def __init__(self):
        self.QuantityTotal = None
        self.QuantityInQueue = None
        self.WasteToRetire = None
