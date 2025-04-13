
class DebugGameRendererRoads:
    def __init__(self):
        pass


class IRoadGraphEntity:
    def __init__(self):
        self.RoadLanesCount = 0
        self.RoadProto = None
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.Value = None
        self.ConstructionCost = None
        self.ConstructionState = None
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.DefaultTitle = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False

class IRoadGraphTerrainConnector:
    def __init__(self):
        self.RoadTerrainConnectionsCount = 0
        self.RoadLanesCount = 0
        self.RoadProto = None
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.Value = None
        self.ConstructionCost = None
        self.ConstructionState = None
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.DefaultTitle = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False

class RoadEntity:
    DISCRETIZATION_STEP = None
    ROAD_LAYOUT_HEIGHT = 0
    def __init__(self):
        self.Prototype = None
        self.CanBePaused = False
        self.RoadLanesCount = 0
        self.RoadProto = None
        from Mafi import Option
        self.CustomTitle = Option()
        self.GeneralPriority = 0
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
        self.Position = None
        self.Direction = None
        self.Type = None

class RoadDirectionCanonical:
    def __init__(self):
        self.DirectionSigns = None
        self.RawData = None

class RoadConnectionType:
    Invalid = None
    OneLane = None
    TwoLane = None
    TerrainToRoad = None
    RoadToTerrain = None
    def __init__(self):
        self.value__ = None

class RoadGraphNodeKey:
    def __init__(self):
        self.Position = None
        self.Position2f = None
        self.Direction = None
        self.m_unused = None

class RoadGraphNodeDirection:
    def __init__(self):
        self.DirectionSigns = None
        self.RawData = None

class RoadConnectionPointProto:
    def __init__(self):
        self.PositionRelative = None
        self.DirectionSigns = None
        self.Type = None

class RoadLaneTrajectory:
    def __init__(self):
        self.LaneCenterSamples = None
        self.LaneDirectionSamples = None
        self.SegmentLengthsPrefixSums = None

class RoadLaneMetadata:
    def __init__(self):
        self.StartPosition = None
        self.EndPosition = None
        self.StartDirection = None
        self.EndDirection = None
        self.LaneLength = None

class RoadEntityProto:
    LANE_WIDTH_OUTER = None
    DOUBLE_LANE_CENTER_OFFSET = None
    LANE_WIDTH_INNER = None
    RAMP_HEIGHT_DELTA = None
    def __init__(self):
        self.EntityType = None
        self.Graphics = None
        self.Layout = None
        self.Ports = None
        self.CloningDisabled = False
        self.IsUnique = False
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.IconPath = ""
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.LanesSpecs = None
        self.LanesData = None
        self.LanesTrajectories = None
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.CannotBeBuiltByPlayer = False
        self.ConstructionDurationPerProduct = None
        self.VehicleGoalHeightAllowedRange = None
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False
        self.IsInitialized = False

class RoadLaneSpec:
    def __init__(self):
        self.TrajectoryCurve = None
        from Mafi import Option
        self.CustomZCurve = Option()
        self.TrajectoryOffset = None
        self.IsReversed = False
        self.IsHidden = False

class RoadEntityBase:
    def __init__(self):
        self.RoadLanesCount = 0
        self.RoadProto = None
        from Mafi import Option
        self.CustomTitle = Option()
        self.GeneralPriority = 0
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
        self.Graphics = None
        self.Layout = None
        self.Ports = None
        self.CloningDisabled = False
        self.IsUnique = False
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.IconPath = ""
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.LanesSpecs = None
        self.LanesData = None
        self.LanesTrajectories = None
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.CannotBeBuiltByPlayer = False
        self.ConstructionDurationPerProduct = None
        self.VehicleGoalHeightAllowedRange = None
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False
        self.IsInitialized = False

    class Gfx:
        def __init__(self):
            self.PrefabPath = ""
            self.PrefabOrigin = None
            self.IconPath = ""
            self.VisualizedLayers = None
            self.Categories = None
            self.MeshSegmentsCount = 0
            self.MaterialPath = ""
            self.IconIsCustom = False
            self.UseInstancedRendering = False
            self.UseSemiInstancedRendering = False
            self.SemiInstancedRenderingExcludedObjects = None
            self.MaxRenderedLod = 0
            self.DisableEmptyChildrenStripping = False
            self.InstancedRendererIndex = None
            self.AnimatedGameObjects = None
            self.AnimationLength = 0.0
            self.HideBlockedPortsIcon = False
            self.Color = None
            self.RendererIndex = 0

class RoadEntranceEntity:
    def __init__(self):
        self.CanBePaused = False
        self.RoadTerrainConnectionsCount = 0
        self.Prototype = None
        self.RoadLanesCount = 0
        self.RoadProto = None
        from Mafi import Option
        self.CustomTitle = Option()
        self.GeneralPriority = 0
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
        self.Graphics = None
        self.Layout = None
        self.Ports = None
        self.CloningDisabled = False
        self.IsUnique = False
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.IconPath = ""
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.TerrainConnections = None
        self.LanesSpecs = None
        self.LanesData = None
        self.LanesTrajectories = None
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.CannotBeBuiltByPlayer = False
        self.ConstructionDurationPerProduct = None
        self.VehicleGoalHeightAllowedRange = None
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False
        self.IsInitialized = False

class LaneTerrainConnectionSpec:
    def __init__(self):
        self.LayoutTile = None
        self.LaneIndex = 0
        self.IsAtLaneStart = False

class RoadTerrainConnection:
    def __init__(self):
        self.TerrainTile = None
        self.RoadGraphNode = None
        self.IsEntranceToRoadGraph = False

class IRoadsManager:
    def __init__(self):
        self.RoadGraphNodes = None
        self.TerrainGraphConnections = None
        self.GraphTerrainConnections = None

class RoadNetworkSearchStatus:
    InvalidStartNode = None
    StepsRanOut = None
    Success = None
    def __init__(self):
        self.value__ = 0

class DummyRoadsManager:
    def __init__(self):
        self.RoadGraphNodes = None
        self.TerrainGraphConnections = None
        self.GraphTerrainConnections = None

class RoadGraphPath:
    def __init__(self):
        self.Path = None
        self.StartTile = None
        self.GoalTile = None
        from Mafi import Fix32
        self.TotalDistance = Fix32()

class RoadPathSegment:
    def __init__(self):
        self.Entity = None
        self.LaneIndex = 0

class GraphTerrainConnection:
    def __init__(self):
        self.RoadNodeId = 0
        self.TerrainTile = None
        self.IsFromTerrainToRoad = False
