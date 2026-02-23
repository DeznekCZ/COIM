
class ClearancePathabilityProvider:
    FLAT_STEEPNESS_DELTA = None
    MAX_STEEPNESS_DELTA = None
    TILE_FREE = None
    TILE_BLOCKED = None
    REQUIRE_TILE_FREE = None
    OCEAN_PRESENT = None
    ALLOW_OCEAN = None
    ALLOW_NO_OCEAN = None
    STEEPNESS_NO_SLOPE = None
    STEEPNESS_SLIGHT_SLOPE = None
    STEEPNESS_STEEP_SLOPE = None
    ALLOW_SLIGHT_SLOPE = None
    ALLOW_NO_SLOPE = None
    HEIGHT_CLEARANCE_FREE = None
    HEIGHT_CLEARANCE_7T = None
    HEIGHT_CLEARANCE_5T = None
    HEIGHT_CLEARANCE_4T = None
    HEIGHT_CLEARANCE_2T = None
    HEIGHT_CLEARANCE_1T = None
    HEIGHT_CLEARANCE_BLOCKED = None
    REQUIRE_CLEARANCE_INF = None
    REQUIRE_CLEARANCE_7T = None
    REQUIRE_CLEARANCE_5T = None
    REQUIRE_CLEARANCE_4T = None
    REQUIRE_CLEARANCE_2T = None
    REQUIRE_CLEARANCE_1T = None
    REQUIRE_NO_CLEARANCE = None
    MAX_QUERY_CLEARANCE = 0
    def __init__(self):
        self.RecomputedChunksCount = 0
        self.TerrainManager = None

    class DataChunk:
        def __init__(self):
            self.IsDirty = False
            self.IsDirtyPathability = False
            self.IsDirtySteepness = False
            self.IsDirtyHeightClearance = False
            self.AllNeighborsEnsured = False
            self.Parent = None
            self.OriginTileAndIndex = None
            self.ChunkIndex = None
            from Mafi import Option
            self.PlusXNeighbor = Option()
            self.PlusXyNeighbor = Option()
            self.PlusYNeighbor = Option()
            self.MinusXNeighbor = Option()
            self.MinusXyNeighbor = Option()
            self.MinusYNeighbor = Option()

    class CapabilityChunkData:
        def __init__(self):
            self.Nodes = None
            self.IsDirty = False
            self.CapabilityIndex = 0

class PathabilityBitmap:
    def __init__(self):
        self.Bitmap = None

class HeightClearancePathability:
    IgnoreClearance = None
    Require1TileClearance = None
    Require2TilesClearance = None
    Require4TilesClearance = None
    Require5TilesClearance = None
    Require7TilesClearance = None
    RequireInfiniteClearance = None
    def __init__(self):
        self.value__ = None

class HeightClearancePathabilityExtensions:
    def __init__(self):
        pass


class SteepnessPathability:
    IgnoreSlope = None
    SlightSlopeAllowed = None
    NoSlopeAllowed = None
    def __init__(self):
        self.value__ = None

class OceanPathability:
    AllowOcean = None
    NoOceanAllowed = None
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
        self.CurrentSimTick = None
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
        self.GoalDirection = None
        self.DistanceEstimationGoalTile = None
        self.IsToEdgeOfMap = False
        self.AllStartDirectionsAllowed = False

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
        self.GoalDirection = None
        self.DistanceEstimationGoalTile = None
        self.IsToEdgeOfMap = False
        self.AllStartDirectionsAllowed = False

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
        self.GoalDirection = None
        self.AllStartDirectionsAllowed = False
        self.DistanceEstimationGoalTile = None
        self.IsWaitingForProcessing = False
        self.IsBeingProcessed = False
        from Mafi import Option
        self.Goal = Option()
        self.EnqueuedAtTick = None
        self.StartedProcessingAtTick = None
        self.FinishedProcessingAtTick = None
        self.IsToEdgeOfMap = False
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
        self.value__ = 0

class ShipPfStep:
    def __init__(self):
        self.DirectionBoundA = None
        self.DirectionBoundB = None
        self.IsUnbound = False
        self.IsBackwards = False
        self.IsSideways = False
        self.Position = None
        self.Angle = None

class PfNodeInfo:
    def __init__(self):
        self.Direction = None
        self.IsBackwards = False
        self.IsSideways = False
        self.CornerTilePosition = None
        self.DirectionIndexRaw = 0
        self.Position = None

class ShipPfNode:
    def __init__(self):
        from Mafi import Fix32
        self.CurrentCost = Fix32()
        self.ParentIndexOnPath = 0
        self.PathFinderInstanceId = 0
        self.IsVisited = False
        self.IsProcessed = False
        self.IsVisitedFromStart = False
        self.PathLength = 0

class ShipsClearancePathabilityProvider:
    def __init__(self):
        self.RecomputedFineChunksCount = 0
        self.TerrainManager = None
        self.OnMultipleFineChunksDirtied = None

    class FineChunkInfoForVis:
        def __init__(self):
            self.Origin = None
            self.CapabilityIndex = 0
            self.Pathability = None
            self.Dirty = False

class ShipHeightClass:
    CargoShip = None
    Battleship = None
    def __init__(self):
        self.value__ = None

class ShipHeightClassExtensions:
    COUNT = 0
    def __init__(self):
        pass


    class <G>$E6017A4FBE41F5D1062F4C5CF25EEC3C:
        def __init__(self):
            pass


        class <M>$CEA1F5CAB28BBB18CB768AF193FAAF3D:
            def __init__(self):
                pass


class ShipPathSegment:
    def __init__(self):
        from Mafi import Option
        self.NextSegment = Option()
        self.PathRawReversed = None

class ShipsPathFinderMode:
    PathFinding = None
    Pathfinding_Close = None
    PathFollowing = None
    def __init__(self):
        self.value__ = 0

class ShipsPathFindingManager:
    DEFAULT_STEPS_PER_UPDATE = 0
    def __init__(self):
        self.MaxStepsPerUpdate = 0
        self.LastWorkDuration = None
        self.QueueLength = 0
        self.CurrentSimTick = None
        self.PathabilityProvider = None
        self.HasMoreTasksToProcess = False
        self.CompletedPfTasks = 0
        self.CompletedUnreachableGoalTasks = 0

class ShipsPathFindingManagerConfig:
    def __init__(self):
        self.PerformWorkInBackgroundThread = False

class ShipsPathFindingParams:
    DEFAULT = None
    def __init__(self):
        self.PathabilityQueryMask = None
        self.RequiredClearance = None
        self.RequiredClearanceBitmaps = None
        self.HeightClass = None
        self.BitmapExtents = None
        self.MinSizeClearance = None
        self.MinHeightClearance = None
        self.SteepnessPathability = None
        self.HeightClearancePathability = None
        self.OceanPathability = None
        self.MaterialTraversalSensitivity = None
        self.RoadLaneTypeMask = None
        self.MaxWheelSubmerge = None

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
        self.PathReversed = None

class VehiclePathFindingManager:
    DEFAULT_STEPS_PER_UPDATE = 0
    EXTRA_STEPS_PER_QUEUED_VEHICLE = 0
    def __init__(self):
        self.MaxStepsPerUpdate = 0
        self.QueueLength = 0
        self.CurrentSimTick = None
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
        self.PathabilityQueryMask = None
        self.MinSizeClearance = None
        self.MinHeightClearance = None
        self.SteepnessPathability = None
        self.HeightClearancePathability = None
        self.OceanPathability = None
        self.MaterialTraversalSensitivity = None
        self.RoadLaneTypeMask = None
        self.MaxWheelSubmerge = None

class VehiclePfNode:
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

    class PfConnLine:
        def __init__(self):
            self.AsToLine2i = None
            self.From = None
            self.To = None

    class Edge:
        def __init__(self):
            self.OtherConnectionLine = None
            self.Node = None
            self.ConnectionLine = None
            from Mafi import Fix32
            self.Distance = Fix32()
            self.NeighborDirection = None
