
class TrainStationFuel:
    def __init__(self):
        self.Prototype = None
        self.UpgradableProto = None
        from Mafi import Option
        self.PrimaryProduct = Option()
        self.CapacityPrimary = None
        self.CurrentPrimaryQuantity = None
        self.SecondaryProduct = Option()
        self.CapacitySecondary = None
        self.CurrentSecondaryQuantity = None
        self.AnimationParams = None
        self.AnimationStatesProvider = None
        self.IsWorking = False
        self.CanReleaseAlignedLocomotive = False
        self.LoadPercent = None
        self.RequiresAlignment = False
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
        self.Poles = None
        self.IsTrackDestroyed = False
        self.IsTrackEnabled = False
        self.Waypoints = None
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
        self.WorkersNeeded = 0
        self.HasWorkersCached = False
        self.PowerRequired = None

class TrainStationFuelSetFuelCmd:
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
        self.ModuleId = None
        self.PrimaryFuelId = None

class TrainStationModule:
    def __init__(self):
        self.Prototype = None
        self.UpgradableProto = None
        self.IsForLoading = False
        self.ProductType = None
        self.ConnectionPercent = None
        self.AnimationConnectionPercent = None
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
        self.Poles = None
        self.IsTrackDestroyed = False
        self.IsTrackEnabled = False
        self.Waypoints = None
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
        self.WorkersNeeded = 0
        self.HasWorkersCached = False
        self.PowerRequired = None
        self.CurrentQuantity = None

class TrainStationRoot:
    def __init__(self):
        self.Prototype = None
        self.UpgradableProto = None
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
        self.Poles = None
        self.IsTrackDestroyed = False
        self.IsTrackEnabled = False
        self.Waypoints = None
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
        self.WorkersNeeded = 0
        self.HasWorkersCached = False
        self.PowerRequired = None

class TrainStationWaypoint:
    def __init__(self):
        self.Prototype = None
        self.UpgradableProto = None
        self.PillarBlocksBitmap = None
        self.Pillars = None
        self.TrainLimit = 0
        self.ModuleLimits = None
        self.CanChangeRailTrackDirection = False
        self.CanChangeCriticality = False
        self.CanAddToSuperBlock = False
        self.CanRemoveFromSuperBlock = False
        self.IsDefaultCritical = False
        self.CanBePaused = False
        self.TrackProto = None
        self.TrainTrackId = None
        self.Direction = None
        self.TrackEntityId = None
        self.TrackTransform = None
        self.TrackCenterTile = None
        self.TrackPosition2f = None
        self.TrackPosition3f = None
        self.Poles = None
        self.IsTrackDestroyed = False
        self.IsTrackEnabled = False
        self.Waypoints = None
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

class LevelCrossingsData:
    CROSSING_SMALL_STR = None
    CROSSING_LARGE_STR = None
    def __init__(self):
        pass


class LocomotivesData:
    from Mafi import Fix32
    ROLLING_RESISTANCE_C_T1 = Fix32()
    ROLLING_RESISTANCE_C_T2 = Fix32()
    COAL_POLLUTION_RATIO = None
    DIESEL_POLLUTION_RATIO = None
    def __init__(self):
        pass


class TrainDepotsData:
    def __init__(self):
        pass


class TrainStationFuelCommandsProcessor:
    def __init__(self):
        pass


class TrainStationFuelProto:
    def __init__(self):
        self.EntityType = None
        self.StorableProducts = None
        self.ElectrificationType = None
        self.AnimationParams = None
        self.AllowedFuels = None
        self.TrajectoryLength = None
        self.BlocksCount = 0
        self.CanBeElevatedOnSupports = False
        self.IsStraight = False
        self.IsElevated = False
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
        self.TransferPeriod = None
        self.RequiresAlignment = False
        self.PowerConsumption = None
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

    class FuelDefinition:
        def __init__(self):
            self.PrimaryProduct = None
            self.SecondaryProduct = None

    class Gfx:
        def __init__(self):
            self.VisualStylePrefabsLods = None
            self.PrefabRotation = None
            self.Ties = None
            self.PrefabPath = ""
            self.PrefabOrigin = None
            self.IconPath = ""
            self.YawForGeneratedIcon = None
            self.VisualizedLayers = None
            self.Categories = None
            self.AnimationDataAssetPathBase = ""
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
            self.RemoveUndergroundVertices = False
            self.HideBlockedPortsIcon = False
            self.Color = None
            self.RendererIndex = 0

class TrainStationModuleProto:
    def __init__(self):
        self.EntityType = None
        self.ElectrificationType = None
        self.StorableProducts = None
        self.AnimationParams = None
        self.TrajectoryLength = None
        self.BlocksCount = 0
        self.CanBeElevatedOnSupports = False
        self.IsStraight = False
        self.IsElevated = False
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
        self.TransferPeriod = None
        self.TransferQuantity = None
        self.Capacity = None
        self.ConnectionCompletionPerStepWhenLoading = None
        self.ConnectionCompletionPerStepWhenUnloading = None
        self.IsProductSupported = None
        self.ProductType = None
        self.PowerConsumption = None
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
            self.VisualStylePrefabsLods = None
            self.PrefabRotation = None
            self.Ties = None
            self.PrefabPath = ""
            self.PrefabOrigin = None
            self.IconPath = ""
            self.YawForGeneratedIcon = None
            self.VisualizedLayers = None
            self.Categories = None
            self.AnimationDataAssetPathBase = ""
            from Mafi import Option
            self.SignObjectName = Option()
            self.SignIconScale = 0.0
            self.SignInMaterialPath = ""
            self.SignOutMaterialPath = ""
            self.AnimateLoadUnload = False
            self.AnimateConnectBeforeLoad = False
            self.AnimateConnectBeforeUnload = False
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
            self.RemoveUndergroundVertices = False
            self.HideBlockedPortsIcon = False
            self.Color = None
            self.RendererIndex = 0

class TrainStationRootProto:
    def __init__(self):
        self.EntityType = None
        self.ElectrificationType = None
        self.TrajectoryLength = None
        self.BlocksCount = 0
        self.CanBeElevatedOnSupports = False
        self.IsStraight = False
        self.IsElevated = False
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
        self.PowerConsumption = None
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

class TrainStationsData:
    T1_STATION_CAPACITY = None
    ELECTRIC_SUFFIX = None
    ICON_SCALE = 0.0
    FUEL_ICON_SCALE = 0.0
    STATION_ORDER = 0
    STATION_FUEL_ORDER = 0
    STATION_EMPTY_ORDER = 0
    def __init__(self):
        pass


class TrainStationWaypointProto:
    def __init__(self):
        self.EntityType = None
        self.ElectrificationType = None
        from Mafi import Option
        self.ElevationFlippedProto = Option()
        self.IsElevated = False
        self.SmallerInterTrackLayout = None
        self.TrajectoryLength = None
        self.BlocksCount = 0
        self.CanBeElevatedOnSupports = False
        self.IsStraight = False
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

class TrainTracksData:
    WAYPOINT_GROUND_PREFABS = None
    WAYPOINT_ELEVATED_PREFABS = None
    R14_MAX_SPEED = None
    R22_MAX_SPEED = None
    TRACK_COST_PER_TILE = None
    TRACK_ELEVATED_COST_PER_TILE = None
    TRACK_ELECTRIC_COST_PER_TILE = None
    TRACK_ELEVATED_ELECTRIC_COST_PER_TILE = None
    TRACK_RAIL_ONLY_PREFABS = None
    TRACK_WITH_BALLAST_PREFABS = None
    TRACK_ELEVATED_PREFABS = None
    SAMPLES_PER_10_TILES = 0
    def __init__(self):
        pass


class TrainWagonsData:
    T1_WAGON_CAPACITY = None
    T2_WAGON_CAPACITY = None
    from Mafi import Fix32
    TONS_PER_QUANTITY_T1 = Fix32()
    TONS_PER_QUANTITY_T2 = Fix32()
    def __init__(self):
        pass

