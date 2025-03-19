class ClearancePathabilityProvider:

    def __init__(self):
        self.DataChunksCount = int(0)
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
        self.IsDirty = False
class PathabilityBitmap:

    def __init__(self):
        pass

class HeightClearancePathability:

    def __init__(self):
        pass

class SteepnessPathability:

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

    def __init__(self):
        pass

class ExploredPfNode:

    def __init__(self):
        pass

class IVehiclePathFindingManager:

    def __init__(self):
        self.QueueLength = int(0)
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
class VehiclePfResultStatus:

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

    def __init__(self):
        self.MaxStepsPerUpdate = int(0)
        self.QueueLength = int(0)
        self.PathabilityProvider = None
        self.HasMoreTasksToProcess = False
        self.CompletedPfTasks = int(0)
        self.CompletedUnreachableGoalTasks = int(0)
class PerfData:

    def __init__(self):
        self.TotalTimeMs = float(0)
class VehiclePathFindingParams:

    def __init__(self):
        pass

