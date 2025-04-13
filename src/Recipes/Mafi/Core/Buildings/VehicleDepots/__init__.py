
class VehicleDepot:
    def __init__(self):
        self.CanBePaused = False
        self.Prototype = None
        self.SpawnPosition = None
        self.DespawnPosition = None
        self.SpawnDirection = None
        self.SpawnDrivePosition = None
        self.DespawnDrivePosition = None
        self.Upgrader = None
        self.PowerRequired = None
        from Mafi import Option
        self.ElectricityConsumer = Option()
        self.ComputingRequired = None
        self.ComputingConsumer = Option()
        self.SoundParams = None
        self.IsSoundOn = False
        self.CanWork = False
        self.CurrentState = None
        self.VehicleJobsCount = 0
        self.DoorOpenPerc = None
        self.VehicleQueue = None
        self.BuildQueue = None
        self.ReplaceQueue = None
        self.CurrentlyBuildVehicle = Option()
        self.Buffers = None
        self.VehicleConstructionProgress = Option()
        self.DestroyCallbackStarted = False
        self.CanDisableLogisticsInput = False
        self.CanDisableLogisticsOutput = False
        self.LogisticsInputMode = None
        self.LogisticsOutputMode = None
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
        self.WorkersNeeded = 0
        self.HasWorkersCached = False

class VehicleDepotBase:
    def __init__(self):
        self.Prototype = None
        self.SpawnPosition = None
        self.DespawnPosition = None
        self.SpawnDirection = None
        self.SpawnDrivePosition = None
        self.DespawnDrivePosition = None
        self.Upgrader = None
        self.PowerRequired = None
        from Mafi import Option
        self.ElectricityConsumer = Option()
        self.ComputingRequired = None
        self.ComputingConsumer = Option()
        self.SoundParams = None
        self.IsSoundOn = False
        self.CanWork = False
        self.CurrentState = None
        self.VehicleJobsCount = 0
        self.DoorOpenPerc = None
        self.VehicleQueue = None
        self.BuildQueue = None
        self.ReplaceQueue = None
        self.CurrentlyBuildVehicle = Option()
        self.Buffers = None
        self.VehicleConstructionProgress = Option()
        self.DestroyCallbackStarted = False
        self.CanDisableLogisticsInput = False
        self.CanDisableLogisticsOutput = False
        self.LogisticsInputMode = None
        self.LogisticsOutputMode = None
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
        self.CanBePaused = False
        self.IsEnabled = False
        self.IsNotEnabled = False
        self.IsPaused = False
        self.IsNotPaused = False
        self.RendererData = None
        self.WorkersNeeded = 0
        self.HasWorkersCached = False

    class State:
        Idle = None
        Paused = None
        NotEnoughWorkers = None
        NotEnoughPower = None
        NotEnoughComputing = None
        Working = None
        def __init__(self):
            self.value__ = 0

class IDepotJob:
    def __init__(self):
        self.IsDestroyed = False
        self.SkipNoMovementMonitoring = False
        self.CurrentFuelConsumption = None
        self.Id = None
        self.JobInfo = None

class VehicleDepotBaseProto:
    def __init__(self):
        self.Upgrade = None
        self.UpgradeNonGeneric = None
        self.ElectricityConsumed = None
        self.BuildableEntities = None
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

        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.ConsumedPowerPerTick = None
        self.ConsumedComputingPerTick = None
        self.SpawnInterval = None
        self.SpawnPosition = None
        self.SpawnDriveTargetPosition = None
        self.SpawnDirection = None
        self.DespawnPosition = None
        self.DespawnDriveTargetPosition = None
        self.DoorOpenDuration = None
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
        Empty = None
        def __init__(self):
            self.PrefabPath = ""
            self.PrefabOrigin = None
            self.IconPath = ""
            self.VisualizedLayers = None
            self.Categories = None
            from Mafi import Option
            self.SoundPrefabPath = Option()
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

class VehicleDepotProto:
    def __init__(self):
        self.EntityType = None
        self.Upgrade = None
        self.UpgradeNonGeneric = None
        self.ElectricityConsumed = None
        self.BuildableEntities = None
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
        self.ConsumedPowerPerTick = None
        self.ConsumedComputingPerTick = None
        self.SpawnInterval = None
        self.SpawnPosition = None
        self.SpawnDriveTargetPosition = None
        self.SpawnDirection = None
        self.DespawnPosition = None
        self.DespawnDriveTargetPosition = None
        self.DoorOpenDuration = None
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.CannotBeBuiltByPlayer = False
        self.ConstructionDurationPerProduct = None
        self.VehicleGoalHeightAllowedRange = None
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False
        self.IsInitialized = False

class VehicleDepotProtoBuilder:
    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None

    class State:
        def __init__(self):
            self.Builder = None
