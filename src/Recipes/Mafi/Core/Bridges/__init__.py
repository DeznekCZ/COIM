
class BridgeLaneProto:
    def __init__(self):
        self.IconPath = ""
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

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
        self.OccupiedLanesSlots = 0
        self.IsBiDirectional = False
        self.Graphics = None
        self.IsPhantom = False

    class Gfx:
        Empty = None
        def __init__(self):
            self.IconPath = ""
            self.PreviewPath = ""

class BridgePathFinderPrecomputation:
    def __init__(self):
        pass


class BridgePlan:
    def __init__(self):
        self.IsEmpty = False
        self.IsNotEmpty = False
        self.Steps = None
        self.LastStepEndPosition = None
        self.Options = None
        self.IsStartPlaceHolder = False

class BridgePlanStep:
    def __init__(self):
        self.Proto = None
        self.EndDirection = None
        self.Transform = None
        self.Aabb = None
        self.OccupiedTilesRelative = None
        self.OccupiedVerticesRelative = None

class IBridgeRoadEntityProto:
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

class BridgeRoad:
    def __init__(self):
        self.CanBePaused = False
        from Mafi import Option
        self.Owner = Option()
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

class BridgeRoadProto:
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
        self.TrajectoryData = None
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

class BridgeRoadEntrance:
    def __init__(self):
        self.CanBePaused = False
        from Mafi import Option
        self.Owner = Option()
        self.Prototype = None
        self.RoadTerrainConnectionsCount = 0
        self.IsRoadGloballyClosed = False
        self.IsRoadClosedSelf = False
        self.GateClosedPercentage = None
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

class BridgeRoadEntranceProto:
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
        self.TrajectoryData = None
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

    class Gfx:
        def __init__(self):
            self.PrefabPath = ""
            self.PrefabOrigin = None
            self.IconPath = ""
            self.YawForGeneratedIcon = None
            self.VisualizedLayers = None
            self.Categories = None
            self.AnimationDataAssetPathBase = ""
            self.MeshStartOffset = None
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

class BridgeLanePendingChange:
    def __init__(self):
        self.OccupiedLanesSlots = 0
        self.LaneIndex = 0
        from Mafi import Option
        self.LaneProto = Option()

class IBridgeLanesChanges:
    def __init__(self):
        self.HasPendingLaneChanges = False

class BridgeSegment:
    def __init__(self):
        self.Prototype = None
        self.LaneSlots = None
        self.PendingLangeChanges = None
        self.HasPendingLaneChanges = False
        self.SingleLanesCount = 0
        from Mafi import Option
        self.NextSegment = Option()
        self.PreviousSegment = Option()
        self.CanBePaused = False
        self.MaintenanceCosts = None
        self.Maintenance = None
        self.IsIdleForMaintenance = False
        self.CustomTitle = Option()
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.Ports = None
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
        self.PillarFootHeights = None

class BridgeLaneSlot:
    def __init__(self):
        self.OccupiedLanesSlots = 0
        from Mafi import Option
        self.LaneProto = Option()
        self.Entity = Option()
        self.IsConnectedWithPreviousLane = False
        self.IsConnectedWithNextLane = False

class BridgeReverseLanePair:
    def __init__(self):
        self.Forward = None
        self.Backward = None

class BridgeSegmentProto:
    GROUND_TOLERANCE = None
    BRIDGE_RAMP_GRADIENT = None
    def __init__(self):
        self.EntityType = None
        self.LaneSpecs = None
        self.LanesCount = 0
        self.StartDirection = None
        self.EndDirection = None
        self.HasElevationChange = False
        self.ValidationSuppressFlag = None
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
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
        self.BridgeType = None
        self.Pillars = None
        self.OccupiedVertexToPillarIndex = None
        self.MaxPillarFootUndermine = None
        self.DeckThickness = None
        self.PillarCostPer10TilesOfHeight = None
        self.Width = None
        self.DeckTrajectory = None
        self.ConnectionPointAtStart = None
        self.ConnectionPointAtEnd = None
        self.IsStraight = False
        self.IsEntrance = False
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
            self.FoundationPrefabPath = ""
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

class BridgeLaneEntityDefinition:
    def __init__(self):
        self.LaneProto = None
        self.EntityProto = None

class BridgeLaneSpec:
    def __init__(self):
        self.RightOffset = None
        self.EntityDefinitions = None

class BridgePillarSpec:
    def __init__(self):
        self.ShapeRelative = None
        self.MaxPillarHeight = None
        self.MaxViolatingVerticesCount = 0
        self.EntityVertexIndices = None

class BridgesManager:
    def __init__(self):
        self.ConnectionNodes = None
        self.ChunkedBridgeEntities = None
        self.Priority = None
        self.BridgesLaneChangeManager = None

class BridgeConnectionNodeKey:
    def __init__(self):
        self.Position = None
        self.PositionSlim = None
        self.Orientation = None

class BridgeNodeOrientation:
    Deg0 = None
    Deg45 = None
    Deg90 = None
    Deg135 = None
    def __init__(self):
        self.AngleFlat = None
        self.OrientationIndex = None
        self.GradeFactor = None

class BridgeConnectionPoint:
    def __init__(self):
        self.AtEnd = False
        self.Entity = None
        self.AtStart = False

class SetBridgeLaneLayoutCmd:
    def __init__(self):
        self.AffectsSaveState = False
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.IsVerificationCmd = False
        self.Result = False
        self.HasError = False
        self.ErrorMessage = ""
        self.BridgeId = None
        self.LaneIndex = 0
        self.LaneProtoId = None

class BridgesLaneChangeManager:
    def __init__(self):
        pass


class IBridgePathFinder:
    def __init__(self):
        self.Options = None
        self.Start = None
        self.Goal = None
        self.NodesProcessed = 0
        self.NodesInHeap = 0
        self.InvalidNodes = 0

class BridgePathFinder:
    PRECOMPUTED_DATA_FILE_EXTENSION = ""
    MAX_SEARCH_RANGE = 0
    PRECOMPUTED_HEURISTICS_RANGE = 0
    PRECOMPUTED_HEURISTICS_HEIGHT = 0
    PRECOMPUTED_HEURISTICS_DIR_NAME = ""
    MAX_PRECOMPUTED_HEURISTICS_KEY = 0
    EXIT_HEURISTICS_RANGE = 0
    EXIT_MAX_HEIGHT = 0
    MAX_EXIT_HEURISTICS_KEY = 0
    def __init__(self):
        self.Options = None
        self.Start = None
        self.Goal = None
        self.NodesProcessed = 0
        self.NodesInHeap = 0
        self.InvalidNodes = 0
        self.AllPieces = None
        self.ProcessedNodes = None

    class PieceInfo:
        def __init__(self):
            self.MustBeFirstPiece = False
            self.MustBeLastPiece = False
            self.Proto = None
            self.AllPiecesIndex = None
            self.Transform = None
            self.Trajectory = None
            self.IsTrajectoryReversed = False
            self.AngleTraversed = None
            self.EndOffsetFromStart = None
            self.OccupiedTilesRelative = None
            self.OccupiedVerticesRelative = None
            self.MaxGradient = 0
            from Mafi import Fix32
            self.Cost = Fix32()
            self.BoundingBox = None
            self.UnpenalizedCost = Fix32()

    class NodeConstraints:
        def __init__(self):
            self.AngleSinceStart = None
            self.Depth = None
            self.NumDirectionChanges = None

    class Node:
        def __init__(self):
            self.StartOffset = None
            self.PieceIndex = 0
            from Mafi import Fix32
            self.CurrentCost = Fix32()
            self.ParentNodeIndex = 0
            self.NodeConstraints = None

class BridgePathFinderOptions:
    def __init__(self):
        self.StartBridgeType = None
        self.PreferredHeight = None
        self.ForcedStartDirection = None
        self.ForcedEndDirectionA = None
        self.ForcedEndDirectionB = None
        self.Flags = None
        from Mafi import Option
        self.ConnectingSegmentProto = Option()

class BridgeTrainTrack:
    def __init__(self):
        from Mafi import Option
        self.Owner = Option()
        self.Prototype = None
        self.CanBePaused = False
        self.TrackProto = None
        self.TrackEntityId = None
        self.TrainTrackId = None
        self.TrackTransform = None
        self.Direction = None
        self.TrackCenterTile = None
        self.TrackPosition2f = None
        self.TrackPosition3f = None
        self.IsTrackDestroyed = False
        self.IsTrackEnabled = False
        self.CanChangeRailTrackDirection = False
        self.CanChangeCriticality = False
        self.CanAddToSuperBlock = False
        self.CanRemoveFromSuperBlock = False
        self.IsDefaultCritical = False
        self.Waypoints = None
        self.PillarBlocksBitmap = None
        self.Pillars = None
        self.UpgradableProto = None
        self.Poles = None
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

class BridgeTrainTrackProto:
    def __init__(self):
        self.EntityType = None
        self.IsElevated = False
        self.IsStraight = False
        self.ElectrificationType = None
        from Mafi import Option
        self.StationWaypointProto = Option()
        self.ElevationFlippedProto = Option()
        self.TrajectoryLength = None
        self.BlocksCount = 0
        self.CanBeElevatedOnSupports = False
        self.MaxSpeedTilesPerTick = None
        self.TrajectoryData = None
        self.TrainTrackHelper = None
        self.Upgrade = None
        self.TierData = None
        self.IgnoreMissingSupport = False
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
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
        self.TrackGraphics = None
        self.LaneIndex = 0
        self.HasElevationChange = False
        self.IgnoreInPathFinder = False
        self.CurveRadius = None
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

class BridgeTypeEnum:
    Viaduct = None
    Truss2Lane = None
    Truss4Lane = None
    CableStayed = None
    def __init__(self):
        self.value__ = None

class BridgeType:
    Truss2Lane = None
    Truss4Lane = None
    CableStayed = None
    def __init__(self):
        self.Id = ""
        self.TypeId = None
        self.PillarMaxDepth = None
        self.PillarsAtEnds = False
        self.GridSizeXy = None
        self.GridSizeZ = None
        self.SlopeStartDeltaHeight = None
        self.MinHeightAboveGround = None
        self.TurnRadius = None
        self.PillarExtentsAlongTrajectory = None

class CanBuildBridgeResult:
    def __init__(self):
        self.RequestStartDirection = None
        self.RequestEndDirection = None
        from Mafi import Option
        self.NewPlan = Option()

class CreateBridgeFromPlanCmd:
    def __init__(self):
        self.AffectsSaveState = False
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.IsVerificationCmd = False
        self.Result = False
        self.HasError = False
        self.ErrorMessage = ""
        self.Plan = None

class DebugGameRendererBridges:
    def __init__(self):
        pass


class IBridgeLaneEntity:
    def __init__(self):
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
        self.Owner = Option()

class CachedValidTileInfo:
    def __init__(self):
        self.State = None
        self.DirectionPacked = None

class ValidTileInfo:
    def __init__(self):
        self.Position = None
        self.Direction = None
        self.State = None

class ValidTileState:
    Unknown = None
    Valid = None
    CannotPath = None
    def __init__(self):
        self.value__ = None
