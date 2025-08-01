class DebugGameRendererRoads:

    def __init__(self):
        pass

class IRoadGraphEntity:

    def __init__(self):
        self.RoadLanesCount = int(0)
        self.RoadProto = None
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.ConstructionCost = None
        self.ConstructionState = None
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AlwaysUseCustomPfTargetTiles = False
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
class ICloseableRoadGraphEntity:

    def __init__(self):
        self.IsRoadClosedSelf = False
        self.IsRoadGloballyClosed = False
        self.RoadLanesCount = int(0)
        self.RoadProto = None
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.ConstructionCost = None
        self.ConstructionState = None
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AlwaysUseCustomPfTargetTiles = False
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
class IRoadGraphEntityProto:

    def __init__(self):
        self.MaxVehicleSpeedPerTick = None
        self.LanesSpecs = None
        self.LanesData = None
        self.LanesTrajectories = None
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.IsUnique = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.Id = None
        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsInitialized = False
        self.Mod = None
class IRoadGraphTerrainConnector:

    def __init__(self):
        self.RoadTerrainConnectionsCount = int(0)
        self.RoadLanesCount = int(0)
        self.RoadProto = None
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.ConstructionCost = None
        self.ConstructionState = None
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AlwaysUseCustomPfTargetTiles = False
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
class RoadEntity:
    DISCRETIZATION_STEP = None
    ROAD_LAYOUT_HEIGHT = None

    def __init__(self):
        self.Prototype = None
        self.CanBePaused = False
        self.RoadLanesCount = int(0)
        self.RoadProto = None
        self.HasBadConnection = False
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
class RoadConnectionDirection:

    def __init__(self):
        pass

class RoadDirectionCanonical:

    def __init__(self):
        self.DirectionSigns = None
class RoadConnectionType:
    Invalid = None
    OneLane = None
    TwoLane = None
    TerrainToRoad = None
    RoadToTerrain = None

    def __init__(self):
        pass

class RoadGraphNodeKey:

    def __init__(self):
        self.Position = None
        self.Position2f = None
class RoadGraphNodeDirection:

    def __init__(self):
        self.DirectionSigns = None
class RoadConnectionPointProto:

    def __init__(self):
        pass

class RoadLaneTrajectory:

    def __init__(self):
        pass

class RoadLaneMetadata:

    def __init__(self):
        pass

class RoadEntityProto:
    LANE_WIDTH_OUTER = None
    DOUBLE_LANE_CENTER_OFFSET = None
    LANE_WIDTH_INNER = None
    RAMP_HEIGHT_DELTA = None

    def __init__(self):
        self.EntityType = None
        self.TierData = None
        self.MaxVehicleSpeedPerTick = None
        self.LanesSpecs = None
        self.LanesData = None
        self.LanesTrajectories = None
        self.Graphics = None
        self.Layout = None
        self.Ports = None
        self.CloningDisabled = False
        self.IsUnique = False
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
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
class RoadLaneSpec:

    def __init__(self):
        pass

class RoadEntityBase:

    def __init__(self):
        self.RoadLanesCount = int(0)
        self.RoadProto = None
        self.HasBadConnection = False
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
        self.CanBePaused = False
        self.IsEnabled = False
        self.IsNotEnabled = False
        self.IsPaused = False
        self.IsNotPaused = False
        self.RendererData = None
class RoadEntityProtoBase:

    def __init__(self):
        self.MaxVehicleSpeedPerTick = None
        self.LanesSpecs = None
        self.LanesData = None
        self.LanesTrajectories = None
        self.Graphics = None
        self.Layout = None
        self.Ports = None
        self.CloningDisabled = False
        self.IsUnique = False
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.IconPath = str(0)
        self.Id = None
        self.EntityType = None
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
        self.PrefabPath = str(0)
        self.PrefabOrigin = None
        self.IconPath = str(0)
        self.YawForGeneratedIcon = None
        self.VisualizedLayers = None
        self.Categories = None
class RoadLaneType:
    MaskTwoTileLane = None
    MaskFourTileLane = None
    MaskAllowAll = None
    MaskAllowNone = None
    TwoTilesLaneFlag = None
    FourTilesLaneFlag = None
    BasicLaneFlag = None
    ElevatedLaneFlag = None
    TerrainConnectionFlag = None

    def __init__(self):
        pass

class RoadEntranceEntity:

    def __init__(self):
        self.CanBePaused = False
        self.RoadTerrainConnectionsCount = int(0)
        self.IsRoadGloballyClosed = False
        self.IsRoadClosedSelf = False
        self.GateClosedPercentage = None
        self.Prototype = None
        self.RoadLanesCount = int(0)
        self.RoadProto = None
        self.HasBadConnection = False
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
class RoadEntranceEntityProto:

    def __init__(self):
        self.EntityType = None
        self.TierData = None
        self.MaxVehicleSpeedPerTick = None
        self.LanesSpecs = None
        self.LanesData = None
        self.LanesTrajectories = None
        self.Graphics = None
        self.Layout = None
        self.Ports = None
        self.CloningDisabled = False
        self.IsUnique = False
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
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
class LaneTerrainConnectionSpec:

    def __init__(self):
        pass

class RoadTerrainConnection:

    def __init__(self):
        pass

class RoadsConstants:
    ROAD_SURFACE_HEIGHT = None
    ROAD_TRAIN_CROSSING_EXTRA_HEIGHT = None

    def __init__(self):
        pass

class IRoadsManager:

    def __init__(self):
        self.RoadGraphNodes = None
        self.TerrainGraphConnections = None
        self.GraphTerrainConnections = None
        self.RoadConnectionAdded = None
        self.RoadConnectionRemoved = None
class RoadNetworkSearchStatus:
    InvalidStartNode = None
    StepsRanOut = None
    Success = None

    def __init__(self):
        pass

class RoadsManager:

    def __init__(self):
        self.RoadGraphNodesCount = int(0)
        self.RoadGraphEdgesCount = int(0)
        self.RoadGraphNodes = None
        self.TerrainGraphConnections = None
        self.GraphTerrainConnections = None
        self.RoadConnectionAdded = None
        self.RoadConnectionRemoved = None
class NodeData:

    def __init__(self):
        self.TotalEdgesCount = int(0)
class DummyRoadsManager:

    def __init__(self):
        self.RoadGraphNodes = None
        self.TerrainGraphConnections = None
        self.GraphTerrainConnections = None
        self.RoadConnectionAdded = None
        self.RoadConnectionRemoved = None
class RoadGraphPath:

    def __init__(self):
        pass

class RoadPathSegment:

    def __init__(self):
        pass

class GraphTerrainConnection:

    def __init__(self):
        pass

