class CargoDepotAssignContractCmd:

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
class CargoDepotManager:

    def __init__(self):
        self.HasShipOrDepot = False
        self.AmountOfShipsDiscovered = int(0)
        self.AmountOfShipsInUse = int(0)
        self.RepairedUnusedShips = None
class AvailableCargoShipData:

    def __init__(self):
        pass

class CargoDepotModule:

    def __init__(self):
        self.LogisticsInputControl = None
        self.LogisticsOutputControl = None
        self.Prototype = None
        self.CanBePaused = False
        self.Maintenance = None
        self.IsIdleForMaintenance = False
        from Mafi import Option
        self.ElectricityConsumer = Option()
        self.CurrentState = None
        self.Upgrader = None
        from Mafi import Option
        self.Depot = Option()
        self.UsableCapacity = None
        self.OnProductStoredChanged = None
        self.ImportPriority = int(0)
        self.ExportPriority = int(0)
        self.IsAnimatingImport = False
        self.CargoAnimationProgress = None
        self.IsCargoTransferAnimating = False
        self.IsPipeMoving = False
        self.PipeMovementProgress = None
        from Mafi import Option
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
        self.PowerRequired = None
        self.MaintenanceCosts = None
class State:

    def __init__(self):
        pass

class CargoDepotBuffer:

    def __init__(self):
        self.CurrentQuantityPercent = int(0)
        self.ImportUntilPercent = None
        self.ExportFromPercent = None
        self.CleaningMode = False
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
class CargoDepotModuleConfigExtensions:

    def __init__(self):
        pass

class CargoDepotModuleClearProductCmd:

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
class CargoDepotModuleProto:

    def __init__(self):
        self.EntityType = None
        self.Upgrade = None
        self.UpgradeNonGeneric = None
        self.HasPipeCraneAnimation = False
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
class Gfx:

    def __init__(self):
        self.PrefabPath = str(0)
        self.PrefabOrigin = None
        self.IconPath = str(0)
        self.VisualizedLayers = None
        self.Categories = None
class CraneAnimationControllerOverride:

    def __init__(self):
        pass

class ID:

    def __init__(self):
        pass

class CargoDepotModuleProtoBuilder:

    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None
class State:

    def __init__(self):
        pass

class CargoDepotModuleSetProductCmd:

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
class CargoShipDepartNowCmd:

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
class CargoShipPayWithUnityIfOutOfFuelCmd:

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
class CargoShipReplaceFuelCmd:

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
class CargoShipSetFuelSaverCmd:

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
