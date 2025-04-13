
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
        self.ErrorMessage = ""
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.ProtoId = StaticEntityProto.ID()

        self.PivotPositions = None
        self.PillarHints = None
        self.StartDirection = None
        self.EndDirection = None
        self.DisablePortSnapping = False
        self.IsFree = False
        self.AllowDirectConnection = False

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
        self.ErrorMessage = ""
        self.TransportId = None

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
        self.ErrorMessage = ""
        self.TransportId = None

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
        self.ErrorMessage = ""
        self.TransportId = None
        self.StartPosition = None
        self.EndPosition = None
        self.QuickRemove = False

class CanBuildTransportResult:
    def __init__(self):
        self.RequestPivots = None
        self.RequestStartDirection = None
        self.RequestEndDirection = None
        from Mafi import Option
        self.NewTrajectory = Option()
        self.PivotsWereReversed = False
        self.NewTransportValue = None
        self.SupportedTiles = None
        self.MiniZipperAtStart = None
        self.MiniZipperAtEnd = None
        self.MiniZipJoinResultAtStart = None
        self.MiniZipJoinResultAtEnd = None
        self.ChangeDirectionNearStart = None
        self.ChangeDirectionNearEnd = None
        self.PortAtStart = Option()
        self.PortAtEnd = Option()

class CanCutOutTransportTrajResult:
    def __init__(self):
        from Mafi import Option
        self.StartSubTransport = Option()
        self.CutOutSubTransport = Option()
        self.EndSubTransport = Option()

class CanCutOutTransportResult:
    def __init__(self):
        self.CutOutFrom = None
        self.CutOutTo = None
        self.ReplacedTransport = None
        from Mafi import Option
        self.StartSubTransport = Option()
        self.CutOutSubTransport = Option()
        self.EndSubTransport = Option()

class CanCutOutTransportAtResult:
    def __init__(self):
        self.CutOutPosition = None
        self.ReplacedTransport = None
        from Mafi import Option
        self.StartSubTransport = Option()
        self.EndSubTransport = Option()

class CanPlaceMiniZipperAtResult:
    def __init__(self):
        self.CutOutResult = None
        self.ZipperProto = None

class MiniZipperAtResult:
    def __init__(self):
        self.IsValid = False
        self.ZipperProto = None
        self.Position = None

class CanChangeDirectionResult:
    def __init__(self):
        self.Transport = None
        self.NewDirection = None
        self.ChangeAtStart = False

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
        self.PreferredHeight = None
        self.ForcedStartDirection = None
        self.BannedStartDirections = None
        self.Flags = None

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
        self.value__ = 0

class TransportPfExploredTile:
    def __init__(self):
        self.Position = None
        self.ParentPosition = None
        self.IsProcessed = False
        self.PathLengthSteps = None

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
        self.LastDumpedMaterial = Option()
        self.IsDumpingActive = False
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
        self.MinDumpOffset = None
        self.DefaultDumpOffset = None
        self.DumpDelay = None
        self.DumpPeriod = None
        self.DumpHeadRelPos = None
        self.MaxProductsInQueue = 0
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
            self.ParticlesParams = None
            self.EmissionsParams = None
            from Mafi import Option
            self.MachineSoundPrefabPath = Option()
            self.HasSign = False
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

class Transport:
    MAX_TRANSPORT_WAYPOINTS = 0
    def __init__(self):
        self.LastInsertedProduct = None
        self.Prototype = None
        self.CanBePaused = False
        self.Value = None
        self.ConstructionCost = None
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.PfTargetTiles = None
        self.Trajectory = None
        self.LastPivotIndex = 0
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
        self.MovedStepsTotal = 0
        self.IsMoving = False
        self.IsFullyConnected = False
        self.TransportManager = None
        self.Upgrader = None
        self.Maintenance = None
        self.MaintenanceCosts = None
        self.DoNotAdjustTerrainDuringConstruction = False
        self.ProductsStateVersion = 0
        self.ProductsIndexBase = 0
        self.TransportColor = None
        self.TransportAccentColor = None
        self.PowerRequired = None
        from Mafi import Option
        self.ElectricityConsumer = Option()
        self.IsTooLongTransportNotificationOn = False
        self.IsProductsRemovalInProgress = False
        self.CenterTile = None
        self.Position2f = None
        self.Position3f = None
        self.ConstructionState = None
        self.IsConstructed = False
        self.IsNotConstructed = False
        self.IsBeingUpgraded = False
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
        def __init__(self):
            self.value__ = 0

class TransportFlow:
    Empty = None
    def __init__(self):
        self.Color = None
        self.IsFlowing = False
        self.HasProducts = False

class TransportConfigExtensions:
    def __init__(self):
        pass


class TransportCrossSection:
    Empty = None
    def __init__(self):
        self.StaticCrossSectionParts = None
        self.MovingCrossSectionParts = None

class TransportedProductMutable:
    def __init__(self):
        self.Quantity = None
        self.TrajectoryIndexRelative = None
        self.LastSeenIndexAbsoluteForUi = None
        self.SlimId = None
        self.IsImmediatelyBehindNextProduct = False
        self.SeqNumber = None

class TransportHelper:
    def __init__(self):
        pass


class TransportSupportableTile:
    def __init__(self):
        self.Position = None
        self.OccupiedTileIndex = 0
        self.PillarAttachmentType = None
        self.AttachmentRotation = None
        self.AttachmentFlipY = False

class TransportTileMetadata:
    def __init__(self):
        self.IsStraight = False
        self.IsStartFlat = False
        self.IsEndFlat = False
        self.StartDirection = None
        self.EndDirection = None
        self.StartType = None
        self.EndType = None

class TransportStartEndType:
    Flat = None
    RampUp = None
    RampDown = None
    Vertical = None
    def __init__(self):
        self.value__ = 0

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
        self.value__ = 0

class TransportPathFinder:
    XY_SIZE = 0
    Z_SIZE = 0
    def __init__(self):
        self.CurrentStart = None
        self.CurrentGoal = None
        self.OriginalGoal = None
        from Mafi import Option
        self.CurrentTransportProto = Option()
        self.Options = None
        self.CurrentPfId = 0
        self.TotalStepsCount = 0
        self.QueueSize = 0

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
        self.ChunkIndex = None
        self.PartsIds = None

class TransportPillarProto:
    MAX_PILLAR_HEIGHT = None
    def __init__(self):
        self.EntityType = None
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
        self.Graphics = None
        self.ConstructionDurationPerProduct = None
        self.VehicleGoalHeightAllowedRange = None
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False
        self.IsInitialized = False

    class Gfx:
        Empty = None
        def __init__(self):
            self.CornerBeamsPrefabPath = ""
            self.CornerBasePrefabPath = ""
            self.SideFillPlusXPrefabPath = ""
            self.BaseWithSideFillsPrefabPath = ""
            self.HideBlockedPortsIcon = False
            self.Color = None
            self.RendererIndex = 0

class TransportPillarsBuilder:
    def __init__(self):
        self.PillarProto = None

class TransportProto:
    MAX_TERRAIN_PENETRATION = None
    def __init__(self):
        self.EntityType = None
        self.Upgrade = None
        self.UpgradeNonGeneric = None
        self.IconPath = ""
        self.CanGoUpDown = False
        self.NeedsPillars = False
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
        self.SurfaceRelativeHeight = None
        self.MaxQuantityPerTransportedProduct = None
        self.TransportedProductsSpacing = None
        self.SpeedPerTick = None
        self.ThroughputPerTick = None
        self.ProductSpacingWaypoints = 0
        self.ProductSpacing = None
        self.ZStepLength = None
        self.MaxPillarSupportRadius = None
        self.NeedsPillarsAtGround = False
        self.CanBeBuried = False
        from Mafi import Option
        self.TileSurfaceWhenOnGround = Option()
        self.PortsShape = None
        self.BaseElectricityCost = None
        self.CornersSharpnessPercent = None
        self.IsBuildable = False
        self.LengthPerCost = None
        self.AllowMixedProducts = False
        self.MaintenanceProduct = None
        self.MaintenancePerTile = None
        self.Graphics = None
        self.ConstructionDurationPerProduct = None
        self.VehicleGoalHeightAllowedRange = None
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False
        self.IsInitialized = False

    class Gfx:
        Empty = None
        def __init__(self):
            self.IconPath = ""
            self.IconIsCustom = False
            self.CrossSection = None
            self.UsePerProductColoring = False
            self.RenderProducts = False
            self.SamplesPerCurvedSegment = 0
            self.MaterialPath = ""
            self.TransportUvLength = None
            self.RenderTransportedProducts = False
            self.SoundOnBuildPrefabPath = ""
            from Mafi import Option
            self.FlowIndicator = Option()
            self.VerticalConnectorPrefabPath = Option()
            self.PillarAttachments = None
            self.UvShiftY = 0.0
            self.CrossSectionScale = None
            self.CrossSectionRadius = 0.0
            self.UseInstancedRendering = False
            self.MaxRenderedLod = 0
            self.InstancedRenderingData = Option()
            self.HideBlockedPortsIcon = False
            self.Color = None
            self.RendererIndex = 0

        class TransportInstancedRenderingData:
            def __init__(self):
                self.InstancedRendererIndex = None

        class FlowIndicatorSpec:
            from Mafi import Fix32
            BIAS_TOWARD_ENDS = Fix32()
            def __init__(self):
                self.FramePrefabPath = ""
                self.FlowPrefabPath = ""
                self.GlassPrefabPath = ""
                self.SkipTransportLength = None
                self.PlacementGap = None
                self.Parameters = None

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
        self.Layers = None
        self.BasePosition = None
        self.IsConstructed = False

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
        from Mafi import Option
        self.AttachedTransport = Option()
        self.AttachmentType = None
        self.AttachmentRotation = None
        self.Flags = None

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
        self.NotificationsManager = None

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
        self.MaxProducts = 0
        self.Price = None
        self.SupportableTiles = None
        self.TransportProto = None
        self.Pivots = None
        self.StartDirection = None
        self.EndDirection = None

class TransportWaypoint:
    def __init__(self):
        self.Position = None
        self.Rotation = None

class TransportWaypointRotation:
    def __init__(self):
        self.Yaw = None
        self.Pitch = None

class TransportFlowIndicatorPose:
    def __init__(self):
        self.Position = None
        self.Rotation = None
        self.PercentOfSection = None
        self.SegmentIndex = 0

class SubTransport:
    def __init__(self):
        self.OriginalTransport = None
        self.SubTrajectory = None

class TransportUpgrader:
    def __init__(self):
        self.UpgradeExists = False
        from Mafi import Option
        self.NextTier = Option()
        self.PriceToUpgrade = None
        self.ConstructionCostToUpgrade = None
        self.UpgradeTitle = None
        self.Icon = ""

class ITransportUpgraderFactory:
    def __init__(self):
        self.EntityIdFactory = None

class TransportUpgraderFactory:
    def __init__(self):
        self.EntityIdFactory = None
