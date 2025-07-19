class ClearancePathabilityProvider:
    FLAT_STEEPNESS_DELTA = None
    MAX_STEEPNESS_DELTA = None
    TILE_FREE = None
    TILE_BLOCKED = None
    STEEPNESS_NO_SLOPE = None
    STEEPNESS_SLIGHT_SLOPE = None
    STEEPNESS_STEEP_SLOPE = None
    ALLOW_SLIGHT_SLOPE = None
    ALLOW_NO_SLOPE = None
    HEIGHT_CLEARANCE_FREE = None
    HEIGHT_CLEARANCE_7T = None
    HEIGHT_CLEARANCE_5T = None
    HEIGHT_CLEARANCE_4T = None
    HEIGHT_CLEARANCE_3T = None
    HEIGHT_CLEARANCE_2T = None
    HEIGHT_CLEARANCE_1T = None
    HEIGHT_CLEARANCE_BLOCKED = None
    REQUIRE_CLEARANCE_INF = None
    REQUIRE_CLEARANCE_7T = None
    REQUIRE_CLEARANCE_5T = None
    REQUIRE_CLEARANCE_4T = None
    REQUIRE_CLEARANCE_3T = None
    REQUIRE_CLEARANCE_2T = None
    REQUIRE_CLEARANCE_1T = None
    REQUIRE_NO_CLEARANCE = None
    MAX_QUERY_CLEARANCE = None

    def __init__(self):
        self.RecomputedChunksCount = int(0)
class DataChunk:

    def __init__(self):
        self.IsDirty = False
        self.IsDirtyPathability = False
        self.IsDirtySteepness = False
        self.IsDirtyHeightClearance = False
        self.AllNeighborsEnsured = False
class CapabilityChunkData:

    def __init__(self):
        self.Nodes = None
        self.IsDirty = False
class PathabilityBitmap:

    def __init__(self):
        pass

class HeightClearancePathability:
    IgnoreClearance = None
    Require1TileClearance = None
    Require2TilesClearance = None
    Require3TilesClearance = None
    Require4TilesClearance = None
    Require5TilesClearance = None
    Require7TilesClearance = None
    RequireInfiniteClearance = None

    def __init__(self):
        pass

class HeightClearancePathabilityExtensions:

    def __init__(self):
        pass

class SteepnessPathability:
    IgnoreSlope = None
    SlightSlopeAllowed = None
    NoSlopeAllowed = None

    def __init__(self):
        pass

class IPathabilityProvider:

    def __init__(self):
        pass

class IVehiclePathFinder:

    def __init__(self):
        self.CurrentPfId = int(0)
        self.TotalStepsCount = int(0)
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
        pass

class ExploredPfNode:

    def __init__(self):
        pass

class IVehiclePathFindingManager:

    def __init__(self):
        self.QueueLength = int(0)
        self.CurrentSimTick = None
        self.PathabilityProvider = None
class IVehiclePathFindingTask:

    def __init__(self):
        self.Vehicle = None
        self.PathFindingParams = None
        self.MaxRetries = int(0)
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
        self.MaxRetries = int(0)
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
        self.MaxRetries = int(0)
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
        self.EnqueuedAtTick = None
        self.StartedProcessingAtTick = None
        self.FinishedProcessingAtTick = None
        self.InQueueDuration = None
        self.PathFindingDuration = None
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
        pass

class PfNode:

    def __init__(self):
        self.CurrentNeighbors = None
        self.IsDestroyed = False
        from Mafi import Fix32
        self.CurrentCost = Fix32()
        from Mafi import Option
        self.ParentNodeOnPath = Option()
        from Mafi import Option
        self.RoadConnectionToParent = Option()
        self.PathLength = int(0)
        self.IsVisitedFromStart = False
        self.IsVisited = False
        self.IsProcessed = False
        self.HasParent = False
        self.IsDirty = False
class PfConnLine:

    def __init__(self):
        self.AsToLine2i = None
class Edge:

    def __init__(self):
        self.OtherConnectionLine = None
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
class VehicleRoadPathSegment:

    def __init__(self):
        from Mafi import Option
        self.NextSegment = Option()
class VehiclePathFindingManager:
    DEFAULT_STEPS_PER_UPDATE = None
    EXTRA_STEPS_PER_QUEUED_VEHICLE = None

    def __init__(self):
        self.MaxStepsPerUpdate = int(0)
        self.QueueLength = int(0)
        self.CurrentSimTick = None
        self.PathabilityProvider = None
        self.HasMoreTasksToProcess = False
        self.CompletedPfTasks = int(0)
        self.CompletedUnreachableGoalTasks = int(0)
class PerfData:

    def __init__(self):
        self.TotalTimeMs = float(0)
class VehiclePathFindingParams:
    DEFAULT = None

    def __init__(self):
        pass

