class MineTower:

    def __init__(self):
        self.CanBePaused = False
        self.HasInputStorageOrTowerAssigned = False
        self.HasOutputStorageOrTowerAssigned = False
        self.AssignedInputs = None
        self.AssignedOutputs = None
        self.AssignedInputStorages = None
        self.AssignedInputOreSorters = None
        self.AssignedOutputStorages = None
        self.AllowNonAssignedOutput = False
        self.AssignedFuelStations = None
        self.AssignedInputTowers = None
        self.AssignedOutputTowers = None
        self.ManagedDesignations = None
        self.DumpableProducts = None
        self.ProductsToNotifyIfCannotGetRidOf = None
        self.Area = None
        self.AssignedExcavatorsTotal = int(0)
        self.AllAssignedExcavators = None
        self.AssignedTrucksTotal = int(0)
        self.AllVehicles = None
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
class MineTowerConfigExtensions:

    def __init__(self):
        pass

class MineTowerAreaChangeCmd:

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
class AddProductToDumpCmd:

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
class RemoveProductToDumpCmd:

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
class AddProductToNotifyIfCannotDumpCmd:

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
class RemoveProductToNotifyIfCannotDumpCmd:

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
class MineTowerProto:

    def __init__(self):
        self.EntityType = None
        from Mafi import Option
        self.DefaultProductOfAssignedTrucks = Option()
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
class MineArea:

    def __init__(self):
        pass

class MineTowerProtoBuilder:

    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None
class State:

    def __init__(self):
        pass

class MineTowersManager:

    def __init__(self):
        self.OnTowerAdded = None
        self.OnTowerRemoved = None
        self.OnAreaChange = None
        self.Towers = None
class NotifyIfCannotDumpFromTowerParam:

    def __init__(self):
        self.AllowedProtoType = None
