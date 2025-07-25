
class TrainStationFuel:
    def __init__(self):
        self.Prototype = None
        self.CapacityPrimary = None
        self.CurrentPrimaryQuantity = None
        self.CapacitySecondary = None
        self.CurrentSecondaryQuantity = None
        self.AnimationParams = None
        self.Upgrader = None
        self.AnimationStatesProvider = None
        self.IsWorking = False
        self.CanChangeRailTrackDirection = False
        self.CanChangeCriticality = False
        self.CanAddToSuperBlock = False
        self.CanRemoveFromSuperBlock = False
        self.IsDefaultCritical = False
        self.CanBePaused = False
        from Mafi import Option
        self.ElectricityConsumer = Option()
        self.CanWorkOnLowPower = False
        self.TrackProto = None
        self.TrainTrackId = None
        self.Direction = None
        self.TrackEntityId = None
        self.TrackTransform = None
        self.TrackCenterTile = None
        self.TrackPosition2f = None
        self.TrackPosition3f = None
        self.IsTrackConstructed = False
        self.Waypoints = None
        self.CustomTitle = Option()
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.Ports = None
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
        self.WorkersNeeded = 0
        self.HasWorkersCached = False
        self.PowerRequired = None

class TrainStationModule:
    def __init__(self):
        self.Prototype = None
        self.IsForLoading = False
        self.ProductType = None
        self.ConnectionPercent = None
        self.LoadUnloadPercent = None
        self.CanReleaseWagon = False
        self.IsFull = False
        self.IsEmpty = False
        self.IsConnected = False
        self.IsLoading = False
        self.IsUnloading = False
        self.ShouldConnectToWagon = False
        self.ShouldDisconnectFromWagon = False
        self.AreParticlesEnabled = False
        self.TransferQuantity = None
        self.Capacity = None
        from Mafi import Option
        self.Buffer = Option()
        self.StoredProduct = Option()
        self.StoredProductQuantity = None
        self.AnimationParams = None
        self.AnimationStatesProvider = None
        self.Upgrader = None
        self.CanChangeRailTrackDirection = False
        self.CanChangeCriticality = False
        self.CanAddToSuperBlock = False
        self.CanRemoveFromSuperBlock = False
        self.IsDefaultCritical = False
        self.CanBePaused = False
        self.ElectricityConsumer = Option()
        self.CanWorkOnLowPower = False
        self.TrackProto = None
        self.TrainTrackId = None
        self.Direction = None
        self.TrackEntityId = None
        self.TrackTransform = None
        self.TrackCenterTile = None
        self.TrackPosition2f = None
        self.TrackPosition3f = None
        self.IsTrackConstructed = False
        self.Waypoints = None
        self.CustomTitle = Option()
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.Ports = None
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
        self.WorkersNeeded = 0
        self.HasWorkersCached = False
        self.PowerRequired = None

class TrainStationRoot:
    def __init__(self):
        self.Prototype = None
        self.ModuleLimits = None
        self.TrainLimit = 0
        self.CanChangeRailTrackDirection = False
        self.CanChangeCriticality = False
        self.CanAddToSuperBlock = False
        self.CanRemoveFromSuperBlock = False
        self.IsDefaultCritical = False
        self.CanBePaused = False
        from Mafi import Option
        self.ElectricityConsumer = Option()
        self.CanWorkOnLowPower = False
        self.TrackProto = None
        self.TrainTrackId = None
        self.Direction = None
        self.TrackEntityId = None
        self.TrackTransform = None
        self.TrackCenterTile = None
        self.TrackPosition2f = None
        self.TrackPosition3f = None
        self.IsTrackConstructed = False
        self.Waypoints = None
        self.CustomTitle = Option()
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.Ports = None
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
        self.WorkersNeeded = 0
        self.HasWorkersCached = False
        self.PowerRequired = None

class LevelCrossingsData:
    def __init__(self):
        pass


class TrainStationFuelProto:
    def __init__(self):
        self.EntityType = None
        self.CanBeElevatedOnSupports = False
        self.AnimationParams = None
        self.Upgrade = None
        self.UpgradeNonGeneric = None
        self.TierData = None
        self.TrajectoryLength = None
        self.BlocksCount = 0
        self.MaxSpeedTilesPerTick = None
        self.TrajectoryData = None
        self.TrainTrackHelper = None
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
        self.TransferPeriod = None
        self.PrimaryProduct = None
        self.SecondaryProduct = None
        self.PowerConsumption = None
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.CannotBeBuiltByPlayer = False
        self.ConstructionDurationPerProduct = None
        self.CollapseRubbleScale = None
        self.CustomBuriedTolerance = None
        self.CustomSuspendedTolerance = None
        self.VehicleGoalHeightAllowedRange = None
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False

    class Gfx:
        def __init__(self):
            self.VisualStylePrefabsLods = None
            self.Ties = None
            self.PrefabPath = ""
            self.PrefabOrigin = None
            self.IconPath = ""
            self.YawForGeneratedIcon = None
            self.VisualizedLayers = None
            self.Categories = None
            from Mafi import Option
            self.SignObjectName = Option()
            self.SignIconScale = 0.0
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

class TrainStationModuleProto:
    def __init__(self):
        self.EntityType = None
        self.CanBeElevatedOnSupports = False
        self.StorableProducts = None
        self.AnimationParams = None
        self.Upgrade = None
        self.UpgradeNonGeneric = None
        self.TierData = None
        self.TrajectoryLength = None
        self.BlocksCount = 0
        self.MaxSpeedTilesPerTick = None
        self.TrajectoryData = None
        self.TrainTrackHelper = None
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
        self.TransferPeriod = None
        self.TransferQuantity = None
        self.Capacity = None
        self.ConnectionCompletionPerStep = None
        self.ProductType = None
        self.PowerConsumption = None
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.CannotBeBuiltByPlayer = False
        self.ConstructionDurationPerProduct = None
        self.CollapseRubbleScale = None
        self.CustomBuriedTolerance = None
        self.CustomSuspendedTolerance = None
        self.VehicleGoalHeightAllowedRange = None
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False

    class Gfx:
        def __init__(self):
            self.VisualStylePrefabsLods = None
            self.Ties = None
            self.PrefabPath = ""
            self.PrefabOrigin = None
            self.IconPath = ""
            self.YawForGeneratedIcon = None
            self.VisualizedLayers = None
            self.Categories = None
            from Mafi import Option
            self.SignObjectName = Option()
            self.SignIconScale = 0.0
            self.SignInMaterialPath = ""
            self.SignOutMaterialPath = ""
            self.AnimateLoadUnload = False
            self.ParticlesParamsForLoading = None
            self.ParticlesParamsForUnloading = None
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

class TrainStationRootProto:
    def __init__(self):
        self.EntityType = None
        self.CanBeElevatedOnSupports = False
        self.TrajectoryLength = None
        self.BlocksCount = 0
        self.MaxSpeedTilesPerTick = None
        self.TrajectoryData = None
        self.TrainTrackHelper = None
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
        self.PowerConsumption = None
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.CannotBeBuiltByPlayer = False
        self.ConstructionDurationPerProduct = None
        self.CollapseRubbleScale = None
        self.CustomBuriedTolerance = None
        self.CustomSuspendedTolerance = None
        self.VehicleGoalHeightAllowedRange = None
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False

class TrainTracksData:
    R14_MAX_SPEED = None
    R22_MAX_SPEED = None
    TRACK_COST_PER_TILE = None
    TRACK_ELEVATED_COST_PER_TILE = None
    TRACK_RAIL_ONLY_PREFABS = None
    TRACK_WITH_BALLAST_PREFABS = None
    TRACK_ELEVATED_PREFABS = None
    SAMPLES_PER_10_TILES = 0
    def __init__(self):
        pass

