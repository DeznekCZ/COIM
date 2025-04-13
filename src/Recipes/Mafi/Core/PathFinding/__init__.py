
class ClearancePathabilityProvider:
    FLAT_STEEPNESS_DELTA = None
    MAX_STEEPNESS_DELTA = None
    BASIC_HEIGHT_CLEARANCE = None
    MAX_HEIGHT_CLEARANCE = None
    STEEPNESS_SLIGHT_SLOPE = None
    STEEPNESS_STEEP_SLOPE = None
    ALLOW_SLIGHT_SLOPE = None
    ALLOW_NO_SLOPE = None
    HEIGHT_CLEARANCE_LOW = None
    HEIGHT_CLEARANCE_BLOCKED = None
    ALLOW_LOW_CLEARANCE = None
    ALLOW_MAX_CLEARANCE = None
    TILE_FREE = None
    TILE_BLOCKED = None
    MAX_QUERY_CLEARANCE = 0
    def __init__(self):
        self.DataChunksCount = 0
        self.RecomputedChunksCount = 0
        self.TerrainManager = None

    class DataChunk:
        def __init__(self):
            self.IsDirty = False
            self.IsDirtyPathability = False
            self.IsDirtySteepness = False
            self.IsDirtyHeightClearance = False
            self.AllNeighborsEnsured = False
            self.OriginTile = None
            self.Parent = None
            self.Data = None
            from Mafi import Option
            self.PlusXNeighbor = Option()
            self.PlusXyNeighbor = Option()
            self.PlusYNeighbor = Option()
            self.MinusXNeighbor = Option()
            self.MinusXyNeighbor = Option()
            self.MinusYNeighbor = Option()

    class CapabilityChunkData:
        def __init__(self):
            self.IsDirty = False
            self.CapabilityIndex = 0
            self.Nodes = None

class PathabilityBitmap:
    def __init__(self):
        self.Bitmap = None

class HeightClearancePathability:
    IgnoreClearance = None
    CanPassUnder = None
    NoPassingUnder = None
    def __init__(self):
        self.value__ = None

class SteepnessPathability:
    IgnoreSlope = None
    SlightSlopeAllowed = None
    NoSlopeAllowed = None
    def __init__(self):
        self.value__ = None

class IPathabilityProvider:
    def __init__(self):
        pass


class IVehiclePathFinder:
    def __init__(self):
        self.CurrentPfId = 0
        self.TotalStepsCount = 0
        self.DistanceEstimationStartCoord = None
        self.DistanceEstimationGoalCoord = None
        self.PathabilityProvider = None

class VehiclePathFinderInitResult:
    Unknown = None
    GoalAlreadyReached = None
    PathFound = None
    ReadyForPf = None
    NoStarts = None
    AllStartsInvalid = None
    NoGoals = None
    AllGoalsInvalid = None
    def __init__(self):
        self.value__ = 0

class ExploredPfNode:
    def __init__(self):
        self.Node = None
        self.ParentNode = None
        from Mafi import Fix32
        self.Cost = Fix32()
        self.IsProcessed = False
        self.IsVisitedFromStart = False

class IVehiclePathFindingManager:
    def __init__(self):
        self.QueueLength = 0
        self.PathabilityProvider = None

class IVehiclePathFindingTask:
    def __init__(self):
        self.Vehicle = None
        self.PathFindingParams = None
        self.MaxRetries = 0
        self.ExtraTolerancePerRetry = None
        self.AllowSimplePathOnly = False
        self.NavigateClosebyIsSufficient = False
        self.MaxNavigateClosebyDistance = None
        self.MaxNavigateClosebyHeightDifference = None
        self.HasResult = False
        self.StartTiles = None
        self.DistanceEstimationStartTile = None
        self.GoalTiles = None
        self.DistanceEstimationGoalTile = None

class IManagedVehiclePathFindingTask:
    def __init__(self):
        self.IsWaitingForProcessing = False
        self.IsBeingProcessed = False
        self.Vehicle = None
        self.PathFindingParams = None
        self.MaxRetries = 0
        self.ExtraTolerancePerRetry = None
        self.AllowSimplePathOnly = False
        self.NavigateClosebyIsSufficient = False
        self.MaxNavigateClosebyDistance = None
        self.MaxNavigateClosebyHeightDifference = None
        self.HasResult = False
        self.StartTiles = None
        self.DistanceEstimationStartTile = None
        self.GoalTiles = None
        self.DistanceEstimationGoalTile = None

class IPathFindingResult:
    def __init__(self):
        self.Task = None
        self.ResultStatus = None
        self.GoalRawTile = None
        from Mafi import Option
        self.NextPathSegment = Option()
        self.ExploredTiles = None

class IPathFindingResultForVehicle:
    def __init__(self):
        self.HasNextPathSegment = False
        self.Task = None
        self.ResultStatus = None
        self.GoalRawTile = None
        from Mafi import Option
        self.NextPathSegment = Option()
        self.ExploredTiles = None

class VehiclePathFindingTask:
    def __init__(self):
        self.Vehicle = None
        self.PathFindingParams = None
        self.MaxRetries = 0
        self.ExtraTolerancePerRetry = None
        self.AllowSimplePathOnly = False
        self.NavigateClosebyIsSufficient = False
        self.MaxNavigateClosebyDistance = None
        self.MaxNavigateClosebyHeightDifference = None
        self.HasResult = False
        self.IsFinished = False
        self.Result = None
        self.StartTiles = None
        self.DistanceEstimationStartTile = None
        self.GoalTiles = None
        self.DistanceEstimationGoalTile = None
        self.IsWaitingForProcessing = False
        self.IsBeingProcessed = False
        from Mafi import Option
        self.Goal = Option()

class VehiclePfResultStatus:
    Unknown = None
    PathFound = None
    StartInvalid = None
    AllGoalsInvalid = None
    NoValidGoals = None
    PathDoesNotExist = None
    StepLimitExceeded = None
    Aborted = None
    def __init__(self):
        self.value__ = 0

class PfNode:
    def __init__(self):
        self.CurrentNeighbors = None
        self.IsDestroyed = False
        from Mafi import Fix32
        self.CurrentCost = Fix32()
        from Mafi import Option
        self.ParentNodeOnPath = Option()
        self.RoadConnectionToParent = Option()
        self.PathLength = 0
        self.IsVisitedFromStart = False
        self.IsVisited = False
        self.IsProcessed = False
        self.HasParent = False
        self.IsDirty = False
        self.Area = None
        self.ParentChunk = None

    class Edge:
        def __init__(self):
            self.OtherConnectionLine = None
            self.Node = None
            from Mafi import Fix32
            self.Distance = Fix32()
            self.ConnectionLine = None
            self.NeighborDirection = None

class IVehiclePathSegment:
    def __init__(self):
        from Mafi import Option
        self.NextSegment = Option()

class IVehiclePathSegmentExtensions:
    def __init__(self):
        pass


class VehicleTerrainPathSegment:
    def __init__(self):
        from Mafi import Option
        self.NextSegment = Option()
        self.PathRawReversed = None

class VehicleRoadPathSegment:
    def __init__(self):
        from Mafi import Option
        self.NextSegment = Option()
        self.Path = None

class VehiclePathFindingManager:
    DEFAULT_STEPS_PER_UPDATE = 0
    EXTRA_STEPS_PER_QUEUED_VEHICLE = 0
    def __init__(self):
        self.MaxStepsPerUpdate = 0
        self.QueueLength = 0
        self.PathabilityProvider = None
        self.HasMoreTasksToProcess = False
        self.CompletedPfTasks = 0
        self.CompletedUnreachableGoalTasks = 0

    class PerfData:
        def __init__(self):
            self.TotalTimeMs = 0.0
            self.PfId = 0
            self.InitTimeMs = 0.0
            self.SearchTimeMs = 0.0
            self.SearchTimePerTickMax = 0.0
            self.Result = None
            from Mafi import Fix32
            self.PathLength = Fix32()
            self.PathLengthTerrain = Fix32()
            self.PathLengthRoad = Fix32()
            self.PathTilesCount = 0
            self.PathNodesCount = 0
            self.ExploredNodesCount = 0
            self.PathLengthEuclidean = Fix32()
            self.PfSteps = 0
            self.SimSteps = 0
            self.Clearance = None

class VehiclePathFindingParams:
    DEFAULT = None
    def __init__(self):
        self.RequiredClearance = None
        self.SteepnessPathability = None
        self.HeightClearancePathability = None
        self.MaterialTraversalSensitivity = None
        self.PathabilityQueryMask = None
