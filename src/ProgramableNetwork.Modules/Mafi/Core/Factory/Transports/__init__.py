class CanBuildTransportResult:

    def __init__(self):
        pass

class CanCutOutTransportTrajResult:

    def __init__(self):
        pass

class CanCutOutTransportResult:

    def __init__(self):
        pass

class CanCutOutTransportAtResult:

    def __init__(self):
        pass

class CanPlaceMiniZipperAtResult:

    def __init__(self):
        pass

class MiniZipperAtResult:

    def __init__(self):
        self.IsValid = False
class CanChangeDirectionResult:

    def __init__(self):
        pass

class ITransportPathFinder:

    def __init__(self):
        self.CurrentStart = None
        self.CurrentGoal = None
        self.OriginalGoal = None
        from Mafi import Option
        self.CurrentTransportProto = Option()
        self.Options = None
class TransportPathFinderOptions:

    def __init__(self):
        pass

class TransportPathFinderFlags:
    None = None
    StartMustBeFlat = None
    GoalMustBeFlat = None
    InvertTieBreaking = None
    BanTilesInFrontOfPorts = None
    AllowOnlyStraight = None
    BanStartRampsInX = None
    BanStartRampsInY = None

    def __init__(self):
        pass

class TransportPfExploredTile:

    def __init__(self):
        pass

class Stacker:

    def __init__(self):
        self.DumpHeightOffset = None
        from Mafi import Option
        self.ElectricityConsumer = Option()
        self.Prototype = None
        self.CanBePaused = False
        self.PowerRequired = None
        self.AreParticlesEnabled = False
        self.DumpPositionXy = None
        from Mafi import Option
        self.LastDumpedMaterial = Option()
        self.IsDumpingActive = False
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
class StackerConfigExtensions:

    def __init__(self):
        pass

class StackerProto:

    def __init__(self):
        self.EntityType = None
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
class Gfx:

    def __init__(self):
        self.PrefabPath = str(0)
        self.PrefabOrigin = None
        self.IconPath = str(0)
        self.YawForGeneratedIcon = None
        self.VisualizedLayers = None
        self.Categories = None
class Transport:
    MAX_TRANSPORT_WAYPOINTS = None

    def __init__(self):
        self.LastInsertedProduct = None
        self.Prototype = None
        self.CanBePaused = False
        self.Value = None
        self.ConstructionCost = None
        self.GeneralPriority = int(0)
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.PfTargetTiles = None
        self.Trajectory = None
        self.LastPivotIndex = int(0)
        self.StartPosition = None
        self.EndPosition = None
        self.StartDirection = None
        self.EndDirection = None
        self.Ports = None
        self.StartInputPort = None
        self.EndOutputPort = None
        self.TransportedProducts = None
        self.FirstProduct = None
        self.LastProduct = None
        self.CanReceiveProducts = False
        self.MovedStepsTotal = int(0)
        self.IsMoving = False
        self.IsFullyConnected = False
        self.TransportManager = None
        self.Upgrader = None
        self.Maintenance = None
        self.MaintenanceCosts = None
        self.DoNotAdjustTerrainDuringConstruction = False
        self.ProductsStateVersion = int(0)
        self.ProductsIndexBase = int(0)
        self.TransportColor = None
        self.TransportAccentColor = None
        self.PowerRequired = None
        from Mafi import Option
        self.ElectricityConsumer = Option()
        self.IsTooLongTransportNotificationOn = False
        self.IsTooLong = False
        self.IsProductsRemovalInProgress = False
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
        self.TransportedProductsMutable = None
        self.IsIdleForMaintenance = False
class Status:
    Idle = None
    NotConnected = None
    Moving = None
    Paused = None
    PowerLow = None
    ProductRemoval = None

    def __init__(self):
        pass

class TransportFlow:
    Empty = None

    def __init__(self):
        self.Color = None
        self.IsFlowing = False
        self.HasProducts = False
class TransportConfigExtensions:

    def __init__(self):
        pass

class BuildTransportCmd:

    def __init__(self):
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.AffectsSaveState = False
        self.IsVerificationCmd = False
        self.Result = None
        self.HasError = False
        self.ErrorMessage = str(0)
class ReverseTransportCmd:

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
class ClearTransportCmd:

    def __init__(self):
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.AffectsSaveState = False
        self.IsVerificationCmd = False
        self.Result = None
        self.HasError = False
        self.ErrorMessage = str(0)
class QuickClearTransportCmd:

    def __init__(self):
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.AffectsSaveState = False
        self.IsVerificationCmd = False
        self.Result = None
        self.HasError = False
        self.ErrorMessage = str(0)
class DeconstructTransportSegmentCmd:

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
class TransportCrossSection:
    Empty = None

    def __init__(self):
        pass

class TransportedProductMutable:

    def __init__(self):
        self.Quantity = None
class TransportHelper:

    def __init__(self):
        pass

class TransportSupportInfo:

    def __init__(self):
        pass

class TransportTileMetadata:

    def __init__(self):
        self.IsStraight = False
        self.IsStartFlat = False
        self.IsEndFlat = False
class TransportStartEndType:
    Flat = None
    RampUp = None
    RampDown = None
    Vertical = None

    def __init__(self):
        pass

class TransportPillarAttachmentType:
    NoAttachment = None
    FlatToFlat_Straight = None
    FlatToFlat_Turn = None
    RampDownToRampUp_Turn = None
    FlatToRampUp_Straight = None
    FlatToRampUp_Turn = None
    FlatToRampDown_Straight = None
    FlatToRampDown_Turn = None
    FlatToVertical = None
    VerticalToVertical = None
    FlatToVertical_Down = None

    def __init__(self):
        pass

class TransportPathFinder:
    XY_SIZE = None
    Z_SIZE = None

    def __init__(self):
        self.CurrentStart = None
        self.CurrentGoal = None
        self.OriginalGoal = None
        from Mafi import Option
        self.CurrentTransportProto = Option()
        self.Options = None
        self.CurrentPfId = int(0)
        self.TotalStepsCount = int(0)
        self.QueueSize = int(0)
class TransportPillar:

    def __init__(self):
        self.CanBePaused = False
        self.VehicleSurfaceHeights = None
        self.PfTargetTiles = None
        self.Value = None
        self.ConstructionCost = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.Height = None
        self.TopTileHeight = None
        self.AreConstructionCubesDisabled = False
        self.Prototype = None
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
        self.Id = None
        self.DefaultTitle = None
        self.Context = None
        self.IsDestroyed = False
        self.IsEnabled = False
        self.IsNotEnabled = False
        self.IsPaused = False
        self.IsNotPaused = False
        self.RendererData = None
class TransportPillarAddRequest:
    Instance = None

    def __init__(self):
        self.ReasonToAdd = None
class TransportPillarEntityValidator:

    def __init__(self):
        self.Priority = None
class TransportPillarRendererData:

    def __init__(self):
        self.IsValid = False
class TransportPillarProto:
    MAX_PILLAR_HEIGHT = None

    def __init__(self):
        self.EntityType = None
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
class Gfx:
    Empty = None

    def __init__(self):
        pass

class TransportPillarsBuilder:

    def __init__(self):
        pass

class TransportProto:
    MAX_TERRAIN_PENETRATION = None
    LENGTH_PER_COST = None

    def __init__(self):
        self.EntityType = None
        self.BaseMaintenanceCost = None
        self.Upgrade = None
        self.UpgradeNonGeneric = None
        self.TierData = None
        self.IconPath = str(0)
        self.CanGoUpDown = False
        self.NeedsPillars = False
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
class Gfx:
    Empty = None

    def __init__(self):
        self.IconPath = str(0)
        self.Categories = None
class TransportInstancedRenderingData:

    def __init__(self):
        pass

class FlowIndicatorSpec:
    BIAS_TOWARD_ENDS = None

    def __init__(self):
        pass

class TransportsBuilder:

    def __init__(self):
        pass

class TransportsCommandsProcessor:

    def __init__(self):
        pass

class TransportsConstructionHelper:

    def __init__(self):
        pass

class PillarVisualsSpec:

    def __init__(self):
        pass

class PillarLayerSpec:
    BEAMS_MASK = None
    FILL_PLUS_X_MASK = None
    FILL_PLUS_Y_MASK = None
    FILL_MINUS_X_MASK = None
    FILL_MINUS_Y_MASK = None
    FLIP_Y_MASK = None
    ALL_FILLS_MASK = None
    BEAMS_AND_ALL_FILLS_MASK = None

    def __init__(self):
        self.HasBeams = False
        self.HasBeamsAndAllBraces = False
        self.HasAnyFill = False
        self.HasFillPlusX = False
        self.HasFillPlusY = False
        self.HasFillMinusX = False
        self.HasFillMinusY = False
        self.AttachmentFlipY = False
class ITransportsPredicates:

    def __init__(self):
        self.IgnoreTransportsElevatedAndMiniZippersPredicate = None
        self.IgnorePillarsPredicate = None
        self.IgnoreTransportsAndPillars = None
class IPillarsChecker:

    def __init__(self):
        pass

class TransportsManager:

    def __init__(self):
        self.Transports = None
        self.Pillars = None
        self.PillarProto = None
        self.ProductsManager = None
        self.IgnoreTransportsElevatedAndMiniZippersPredicate = None
        self.IgnorePillarsPredicate = None
        self.IgnoreTransportsAndPillars = None
class TransportTrajectory:

    def __init__(self):
        self.Curve = None
        self.PivotSegmentIndices = None
        self.OccupiedTiles = None
        self.OccupiedTilesMetadata = None
        self.FlowIndicatorsPoses = None
        self.Waypoints = None
        self.CurveSegmentWaypointIndices = None
        self.TrajectoryLength = None
        self.MaxProducts = int(0)
        self.Price = None
        self.TilesSupportInfo = None
class TransportWaypoint:

    def __init__(self):
        pass

class TransportWaypointRotation:

    def __init__(self):
        pass

class TransportFlowIndicatorPose:

    def __init__(self):
        pass

class SubTransport:

    def __init__(self):
        pass

class TransportUpgrader:

    def __init__(self):
        self.UpgradeExists = False
        from Mafi import Option
        self.NextTier = Option()
        self.PriceToUpgrade = None
        self.ConstructionCostToUpgrade = None
        self.UpgradeTitle = None
        self.Icon = str(0)
class ITransportUpgraderFactory:

    def __init__(self):
        self.EntityIdFactory = None
class TransportUpgraderFactory:

    def __init__(self):
        self.EntityIdFactory = None
