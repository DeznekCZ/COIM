
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

class Grate:
    def __init__(self):
        self.Prototype = None
        self.CanBePaused = False
        self.CurrentState = None
        self.PowerRequired = None
        from Mafi import Option
        self.ElectricityConsumer = Option()
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

    class State:
        Paused = None
        Working = None
        WaitingForInput = None
        OutputFull = None
        NotEnoughPower = None
        def __init__(self):
            self.value__ = 0

class GrateOutlet:
    def __init__(self):
        self.Prototype = None
        self.CanBePaused = False
        self.CurrentState = None
        self.PowerRequired = None
        from Mafi import Option
        self.ElectricityConsumer = Option()
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

    class State:
        Paused = None
        Working = None
        WaitingForInput = None
        OutputFull = None
        NotEnoughPower = None
        def __init__(self):
            self.value__ = 0

class GrateOutletProto:
    def __init__(self):
        self.EntityType = None
        self.AcceptedProductsForUiFilterFn = None
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
        self.PowerConsumption_Transferring = None
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

class GrateProto:
    def __init__(self):
        self.EntityType = None
        self.AcceptedProductsForUiFilterFn = None
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
        self.MiningPathCache = None
        self.MaxProductsInQueue = None
        self.MiningDuration = None
        self.MaxThicknessMinedPerMiningDuration = None
        self.MaxTilesScannedPerTick = None
        self.TransferDelay = None
        self.PowerConsumption_Idling = None
        self.PowerConsumption_Mining = None
        self.PowerConsumption_Transferring = None
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

class IProtoWithAcceptedProductsForUi:
    def __init__(self):
        self.AcceptedProductsForUiFilterFn = None
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

class IProtoWithThroughputForUi:
    def __init__(self):
        self.ThroughputPerTick = None
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

class StackerConfigExtensions:
    def __init__(self):
        pass


class StackerProto:
    def __init__(self):
        self.EntityType = None
        self.ElectricityConsumed = None
        self.ThroughputPerTick = None
        self.AcceptedProductsForUiFilterFn = None
        self.TierData = None
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
        self.MinDumpOffset = None
        self.DefaultDumpOffset = None
        self.DumpDelay = None
        self.DumpPeriod = None
        self.DumpHeadRelPos = None
        self.MaxProductsInQueue = 0
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
            self.RemoveUndergroundVertices = False
            self.HideBlockedPortsIcon = False
            self.Color = None
            self.RendererIndex = 0

class StackerTower:
    def __init__(self):
        self.Prototype = None
        self.QuantityInOutputBuffer = None
        self.CurrentPivotRelPosition = None
        self.DesiredPivotRelPosition = None
        self.CurrentBoomSetup = None
        self.DesiredBoomSetup = None
        self.IsFullAlertActive = False
        self.MaxSegments = 0
        self.DumpHeightOffset = None
        self.DumpRadiusLeft = None
        self.DumpRadiusRight = None
        from Mafi import Option
        self.SelectedProductForSlopePreview = Option()
        self.ConnectedRailSegmentsCount = 0
        self.CanBePaused = False
        self.Status = None
        self.IsDumpingActive = False
        self.AreParticlesEnabled = False
        self.LastDumpTick = None
        self.LastDumpedMaterial = Option()
        self.PowerRequired = None
        self.ElectricityConsumer = Option()
        self.WorkersNeeded = 0
        self.HasWorkersCached = False
        self.MaintenanceCosts = None
        self.Maintenance = None
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
        self.OngoingMonthlyData = None
        self.ProductivityCounterHistory = None
        self.ProductivityCounterLabels = None
        self.IsIdleForMaintenance = False

    class StatusEnum:
        Idle = None
        Paused = None
        WaitForInput = None
        NoWorkers = None
        NoPower = None
        DumpingFullfilled = None
        Moving = None
        Dumping = None
        Transferring = None
        Broken = None
        def __init__(self):
            self.value__ = 0

    class StackerTowerBoomSetup:
        def __init__(self):
            self.PivotRotation = None
            self.CartDistance = None

class StackerTowerCommandsProcessor:
    def __init__(self):
        pass


class StackerTowerSetFullAlertActiveCmd:
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
        self.EntityId = None
        self.Active = False

class StackerTowerSetDumpRadiusCmd:
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
        self.EntityId = None
        self.RadiusLeft = None
        self.RadiusRight = None

class StackerTowerSetMaxSegmentsCmd:
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
        self.EntityId = None
        self.SegmentsCount = 0

class StackerTowerSelectProductForSlopePreviewCmd:
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
        self.EntityId = None
        from Mafi import Option
        self.Product = Option()

class IStackerTowerRailProto:
    def __init__(self):
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

class StackerTowerProto:
    def __init__(self):
        self.EntityType = None
        self.ElectricityConsumed = None
        self.TierData = None
        self.DumpingThroughputPerTick = None
        self.ThroughputPerTick = None
        self.AcceptedProductsForUiFilterFn = None
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
        self.RailProto = None
        self.PivotMaxSpeed = None
        self.PivotAcceleration = None
        self.CartMaxSpeed = None
        self.CartAcceleration = None
        self.RotationMaxSpeed = None
        self.RotationAcceleration = None
        self.CartHeight = None
        self.MaxBufferSize = None
        self.MaxBufferSizeExtraPerRailSegment = None
        self.DumpDelay = None
        self.TransferSpeedPerTick = None
        self.MaxTileChecksPerTick = 0
        self.MinDumpRadius = None
        self.MinDumpRadiusSqrInt = 0
        self.MaxDumpRadius = None
        self.MaxDumpRadiusSqrInt = 0
        from Mafi import Fix32
        self.ProtectionSlopeSteepness = Fix32()
        self.ProtectionSlopeMaxDistance = None
        self.MinDumpHeightOffset = None
        self.MaxDumpHeightOffset = None
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
            self.ParticleParams = None
            self.PrefabTowerPivotPath = ""
            self.PrefabTowerBoomPath = ""
            self.PrefabMovableCartPath = ""
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

class StackerTowerRail:
    def __init__(self):
        self.Prototype = None
        self.CanBePaused = False
        self.PfTargetTiles = None
        from Mafi import Option
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

class StackerTowerRailProto:
    def __init__(self):
        self.EntityType = None
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
        self.MaintenanceCosts = None
        self.SegmentLength = None
        self.RailHeight = None
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

class Transport:
    PROD_COUNTERS_LABELS = None
    MAX_TRANSPORT_WAYPOINTS = 0
    def __init__(self):
        self.LastInsertedProduct = None
        self.UpgradableProto = None
        self.Prototype = None
        self.CanBePaused = False
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
        self.IsTooLong = False
        self.OngoingMonthlyData = None
        self.ProductivityCounterHistory = None
        self.ProductivityCounterLabels = None
        self.IsProductsRemovalInProgress = False
        self.CenterTile = None
        self.Position2f = None
        self.Position3f = None
        self.AlwaysUseCustomPfTargetTiles = False
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
        ProductRemoval = None
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

class TransportCrossSection:
    Empty = None
    def __init__(self):
        self.StaticCrossSectionParts = None
        self.MovingCrossSectionParts = None

class TransportedProductMutable:
    def __init__(self):
        self.Quantity = None
        self.QuantityValue = 0
        self.IsImmediatelyBehindNextProduct = False
        self.TrajectoryIndexRelative = None
        self.LastSeenIndexAbsoluteForUi = None
        self.SlimId = None
        self.QuantityAndData = None
        self.SeqNumber = None

class TransportHelper:
    def __init__(self):
        pass


class TransportSupportInfo:
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
        self.ChunkIndex = None
        self.PartsIds = None

class TransportPillarProto:
    MAX_PILLAR_HEIGHT = None
    def __init__(self):
        self.EntityType = None
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
        self.Graphics = None
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
    LENGTH_PER_COST = None
    def __init__(self):
        self.EntityType = None
        self.BaseMaintenanceCost = None
        self.Upgrade = None
        self.TierData = None
        self.IconPath = ""
        self.CanGoUpDown = False
        self.NeedsPillars = False
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
        self.SurfaceRelativeHeight = None
        self.MaxQuantityPerTransportedProduct = None
        self.TransportedProductsSpacing = None
        self.SpeedPerTick = None
        self.ThroughputPerTick = None
        self.ThroughputPer60 = None
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
        self.Graphics = None
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
            self.IconPath = ""
            self.Categories = None
            self.IconIsCustom = False
            self.UsePerProductColoring = False
            self.RenderProducts = False
            self.CrossSectionLods = None
            self.MaterialPath = ""
            self.TransportUvLength = None
            self.RenderTransportedProducts = False
            self.SoundOnBuildPrefabPath = ""
            from Mafi import Option
            self.FlowIndicator = Option()
            self.VerticalConnectorPrefabPath = Option()
            self.PillarAttachments = None
            self.UvShiftY = 0.0
            self.CrossSectionScale = 0.0
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

        class TransportCrossSectionLod:
            def __init__(self):
                self.PixelsPerMeter = 0.0
                self.CrossSection = None
                self.SamplesPerCurvedSegment = 0

        class FlowIndicatorSpec:
            from Mafi import Fix32
            BIAS_TOWARD_ENDS = Fix32()
            def __init__(self):
                self.FramePrefabPath = ""
                self.FlowPrefabPath = ""
                self.GlassPrefabPath = ""
                self.SkipTransportLength = None
                self.PlacementGap = None
                self.LengthScale = 0.0
                self.CrossSectionScale = 0.0
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
        self.IsPaused = False
        self.IsDeconstruction = False

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
        self.TilesSupportInfo = None
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
        self.CurrentProto = None

class ITransportUpgraderFactory:
    def __init__(self):
        pass


class TransportUpgraderFactory:
    def __init__(self):
        self.EntityIdFactory = None
