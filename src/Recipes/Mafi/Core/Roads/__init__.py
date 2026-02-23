
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
        self.RoadLanesCount = 0
        self.RoadProto = None
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
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
        self.UseTerrainHeightForVehicles = False
        self.LanesSpecs = None
        self.LanesData = None
        self.LanesTrajectories = None
        self.RoadTotalWidth = None
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.IsUnique = False
        self.AutoBuildMiniZippers = False
        self.CanMoveUpDownWhenInvalidPlacement = False
        self.Graphics = None
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

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
        self.IsObsolete = False
        self.Mod = None

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
    ROAD_LAYOUT_HEIGHT = 0
    def __init__(self):
        self.Prototype = None
        self.CanBePaused = False
        self.RoadLanesCount = 0
        self.RoadProto = None
        self.HasBadConnection = False
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
        self.X = None
        self.Y = None
        self.Z = None
        self.Direction = None
        self.LaneType = None

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
        self.StartType = None
        self.EndType = None
        self.LaneLength = None

class RoadEntityProto:
    LANE_WIDTH_OUTER = None
    DOUBLE_LANE_CENTER_OFFSET = None
    LANE_WIDTH_INNER = None
    RAMP_HEIGHT_DELTA = None
    def __init__(self):
        self.EntityType = None
        self.TierData = None
        self.MaxVehicleSpeedPerTick = None
        self.UseTerrainHeightForVehicles = False
        self.LanesSpecs = None
        self.LanesData = None
        self.LanesTrajectories = None
        self.RoadTotalWidth = None
        self.Graphics = None
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.IconPath = ""
        self.CanMoveUpDownWhenInvalidPlacement = False
        self.IsUnique = False
        self.CloningDisabled = False
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

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
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.ConstructionDurationPerProduct = None
        self.CollapseRubbleScale = None
        self.CustomBuriedTolerance = None
        self.CustomSuspendedTolerance = None
        self.VehicleGoalHeightAllowedRange = None
        self.CannotBeBuiltByPlayer = False
        self.CannotBeDestroyedByFlood = False
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False

class RoadLaneSpec:
    def __init__(self):
        self.TrajectoryCurve = None
        self.CurveRightOffset = None
        self.HeightCurve = None
        self.IsHidden = False
        self.StartType = None
        self.EndType = None

class RoadEntityBase:
    def __init__(self):
        self.RoadLanesCount = 0
        self.RoadProto = None
        self.HasBadConnection = False
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
        self.UseTerrainHeightForVehicles = False
        self.LanesSpecs = None
        self.LanesData = None
        self.LanesTrajectories = None
        self.RoadTotalWidth = None
        self.Graphics = None
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.IconPath = ""
        self.CanMoveUpDownWhenInvalidPlacement = False
        self.IsUnique = False
        self.CloningDisabled = False
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

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
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.ConstructionDurationPerProduct = None
        self.CollapseRubbleScale = None
        self.CustomBuriedTolerance = None
        self.CustomSuspendedTolerance = None
        self.VehicleGoalHeightAllowedRange = None
        self.CannotBeBuiltByPlayer = False
        self.CannotBeDestroyedByFlood = False
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False

    class Gfx:
        Empty = None
        def __init__(self):
            self.PrefabPath = ""
            self.PrefabOrigin = None
            self.IconPath = ""
            self.YawForGeneratedIcon = None
            self.VisualizedLayers = None
            self.Categories = None
            self.AnimationDataAssetPathBase = ""
            self.IconIsCustom = False
            self.UseInstancedRendering = False
            self.UseSemiInstancedRendering = False
            self.SemiInstancedRenderingExcludedObjects = None
            self.MaxRenderedLod = 0
            self.DisableEmptyChildrenStripping = False
            self.InstancedRendererIndex = None
            self.AnimatedGameObjects = None
            self.AnimationLength = 0.0
            self.RemoveUndergroundVertices = False
            self.HideBlockedPortsIcon = False
            self.Color = None
            self.RendererIndex = 0

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
        self.value__ = None

class RoadEntranceEntity:
    def __init__(self):
        self.CanBePaused = False
        self.RoadTerrainConnectionsCount = 0
        self.IsRoadGloballyClosed = False
        self.IsRoadClosedSelf = False
        self.GateClosedPercentage = None
        self.Prototype = None
        self.RoadLanesCount = 0
        self.RoadProto = None
        self.HasBadConnection = False
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
        self.UseTerrainHeightForVehicles = False
        self.LanesSpecs = None
        self.LanesData = None
        self.LanesTrajectories = None
        self.RoadTotalWidth = None
        self.Graphics = None
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.IconPath = ""
        self.CanMoveUpDownWhenInvalidPlacement = False
        self.IsUnique = False
        self.CloningDisabled = False
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

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
        self.TerrainConnections = None
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.ConstructionDurationPerProduct = None
        self.CollapseRubbleScale = None
        self.CustomBuriedTolerance = None
        self.CustomSuspendedTolerance = None
        self.VehicleGoalHeightAllowedRange = None
        self.CannotBeBuiltByPlayer = False
        self.CannotBeDestroyedByFlood = False
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False

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

class RoadsConstants:
    ROAD_SURFACE_HEIGHT = None
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
        self.value__ = 0

class RoadsManager:
    def __init__(self):
        self.RoadGraphNodesCount = 0
        self.RoadGraphEdgesCount = 0
        self.Priority = None
        self.RoadGraphNodes = None
        self.TerrainGraphConnections = None
        self.GraphTerrainConnections = None
        self.RoadConnectionAdded = None
        self.RoadConnectionRemoved = None

    class NodeData:
        def __init__(self):
            self.TotalEdgesCount = 0
            self.NodeKey = None
            self.OutgoingEdgesCount = None
            self.IncomingEdgesCount = None
            self.IsConnectedFromTerrain = False
            self.IsConnectedToTerrain = False

class DummyRoadsManager:
    def __init__(self):
        self.RoadGraphNodes = None
        self.TerrainGraphConnections = None
        self.GraphTerrainConnections = None
        self.RoadConnectionAdded = None
        self.RoadConnectionRemoved = None

class RoadGraphPath:
    def __init__(self):
        self.Path = None
        self.StartTile = None
        self.GoalTile = None
        from Mafi import Fix32
        self.TotalDistance = Fix32()

class RoadPathSegment:
    def __init__(self):
        self.IsValid = False
        self.Entity = None
        self.LaneIndex = 0

class GraphTerrainConnection:
    def __init__(self):
        self.RoadNodeId = 0
        self.TerrainTile = None
        self.RoadLaneType = None
        self.IsFromTerrainToRoad = False
        self.Entity = None
