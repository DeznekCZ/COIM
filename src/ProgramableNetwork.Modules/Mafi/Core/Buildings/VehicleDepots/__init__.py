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
        from Mafi import Option
        self.ComputingConsumer = Option()
        self.SoundParams = None
        self.IsSoundOn = False
        self.CanWork = False
        self.CurrentState = None
        self.VehicleJobsCount = int(0)
        self.DoorOpenPerc = None
        self.VehicleQueue = None
        self.BuildQueue = None
        self.ReplaceQueue = None
        from Mafi import Option
        self.CurrentlyBuildVehicle = Option()
        self.Buffers = None
        from Mafi import Option
        self.VehicleConstructionProgress = Option()
        self.DestroyCallbackStarted = False
        self.CanDisableLogisticsInput = False
        self.CanDisableLogisticsOutput = False
        self.LogisticsInputMode = None
        self.LogisticsOutputMode = None
        from Mafi import Option
        self.CustomTitle = Option()
        self.GeneralPriority = int(0)
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
        self.WorkersNeeded = int(0)
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
        from Mafi import Option
        self.ComputingConsumer = Option()
        self.SoundParams = None
        self.IsSoundOn = False
        self.CanWork = False
        self.CurrentState = None
        self.VehicleJobsCount = int(0)
        self.DoorOpenPerc = None
        self.VehicleQueue = None
        self.BuildQueue = None
        self.ReplaceQueue = None
        from Mafi import Option
        self.CurrentlyBuildVehicle = Option()
        self.Buffers = None
        from Mafi import Option
        self.VehicleConstructionProgress = Option()
        self.DestroyCallbackStarted = False
        self.CanDisableLogisticsInput = False
        self.CanDisableLogisticsOutput = False
        self.LogisticsInputMode = None
        self.LogisticsOutputMode = None
        from Mafi import Option
        self.CustomTitle = Option()
        self.GeneralPriority = int(0)
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
        self.WorkersNeeded = int(0)
        self.HasWorkersCached = False
class State:

    def __init__(self):
        pass

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
        self.IconPath = str(0)
        self.Id = None
        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class Gfx:

    def __init__(self):
        self.PrefabPath = str(0)
        self.PrefabOrigin = None
        self.IconPath = str(0)
        self.VisualizedLayers = None
        self.Categories = None
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
        self.IconPath = str(0)
        self.Id = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class VehicleDepotProtoBuilder:

    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None
class State:

    def __init__(self):
        pass

