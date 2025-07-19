class CaptainOffice:

    def __init__(self):
        self.Prototype = None
        self.CanBePaused = False
        self.CurrentState = None
        self.Upgrader = None
        from Mafi import Option
        self.ElectricityConsumer = Option()
        self.IsActive = False
        self.EmissionIntensity = None
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
        self.AlwaysUseCustomPfTargetTiles = False
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
class State:
    None = None
    Paused = None
    NotEnoughWorkers = None
    NotEnoughPower = None
    Working = None

    def __init__(self):
        pass

class CaptainOfficeManager:

    def __init__(self):
        from Mafi import Option
        self.CaptainOffice = Option()
        self.OfficeBuilt = False
        self.IsOfficeActive = False
        self.OnOfficeActiveChanged = None
class CaptainOfficeProto:

    def __init__(self):
        self.EntityType = None
        self.Upgrade = None
        self.UpgradeNonGeneric = None
        self.TierData = None
        self.ElectricityConsumed = None
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
class OfficeBuilding:

    def __init__(self):
        self.Prototype = None
        self.CanBePaused = False
        self.CurrentState = None
        self.FocusPointsLastTick = int(0)
        self.FocusPointsMaxAvailable = int(0)
        self.PointsMultiplier = None
        self.Upgrader = None
        self.Maintenance = None
        from Mafi import Option
        self.ElectricityConsumer = Option()
        self.ComputingBoostStep = int(0)
        from Mafi import Option
        self.ComputingConsumer = Option()
        self.EmissionIntensity = None
        self.Progress = None
        self.InputBuffer = None
        self.OutputBuffer = None
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
        self.AlwaysUseCustomPfTargetTiles = False
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
        self.ComputingRequired = None
        self.MaintenanceCosts = None
        self.IsIdleForMaintenance = False
class State:
    Paused = None
    Broken = None
    MissingSupplies = None
    MissingWorkers = None
    NotEnoughPower = None
    WorkingComputingLow = None
    Working = None

    def __init__(self):
        pass

class OfficeBuildingProto:

    def __init__(self):
        self.EntityType = None
        self.UpgradeNonGeneric = None
        self.Upgrade = None
        self.TierData = None
        self.ElectricityConsumed = None
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
class OfficeBuildingsManager:

    def __init__(self):
        self.AllFocuses = None
        self.FocusPointsAvailable = int(0)
        self.FocusPointsLastTick = int(0)
        self.FocusPointsMaxAvailable = int(0)
        self.FocusPointsRequired = int(0)
class SetOfficeFocusStepCmd:

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
class SetOfficeComputingBoostStepCmd:

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
class OfficeFocus:

    def __init__(self):
        self.CurrentActiveStep = int(0)
        self.TargetStep = int(0)
        self.PointsAssigned = int(0)
        self.PointsRequired = int(0)
class OfficeFocusProto:

    def __init__(self):
        self.IconPath = str(0)
        self.Id = None
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
class DescriptionFunc:

    def __init__(self):
        self.Method = None
        self.Target = None
class Gfx:
    Empty = None

    def __init__(self):
        self.IconPath = str(0)
class OfficeFocusWithProperties:

    def __init__(self):
        self.CurrentActiveStep = int(0)
        self.TargetStep = int(0)
        self.PointsAssigned = int(0)
        self.PointsRequired = int(0)
class OfficeFocusWithPropertiesProto:

    def __init__(self):
        self.IconPath = str(0)
        self.Id = None
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
