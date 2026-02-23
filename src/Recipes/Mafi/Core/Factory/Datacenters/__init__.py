
class DataCenter:
    def __init__(self):
        self.CanBePaused = False
        self.CurrentState = None
        self.IsCargoAffectedByGeneralPriority = False
        self.RacksCount = 0
        self.MaxComputingGenerationCapacity = None
        self.PowerRequired = None
        self.ServersCounts = None
        from Mafi import Option
        self.ElectricityConsumer = Option()
        self.Maintenance = None
        self.MaintenanceCosts = None
        self.RacksMaintenance = None
        self.WorkersNeeded = 0
        self.CoolantInBuffer = None
        self.CoolantOutBuffer = None
        self.CoolantInPerMonth = None
        self.CoolantOutPerMonth = None
        self.OngoingMonthlyData = None
        self.ProductivityCounterHistory = None
        self.ProductivityCounterLabels = None
        self.CustomTitle = Option()
        self.GeneralPriority = 0
        self.IsGeneralPriorityVisible = False
        self.Ports = None
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
        self.IsIdleForMaintenance = False
        self.HasWorkersCached = False

    class State:
        Working = None
        NoRacks = None
        Paused = None
        Broken = None
        NotEnoughWorkers = None
        NotEnoughElectricity = None
        NotEnoughCoolant = None
        FullOutput = None
        def __init__(self):
            self.value__ = 0

class DataCenterProto:
    def __init__(self):
        self.EntityType = None
        from Mafi.Core.Factory.Datacenters import DataCenterProto
        self.Id = DataCenterProto.ID()

        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.IconPath = ""
        self.CanMoveUpDownWhenInvalidPlacement = False
        self.IsUnique = False
        self.CloningDisabled = False
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
        self.RacksCapacity = 0
        self.CoolantIn = None
        self.CoolantOut = None
        self.CoolantCapacity = None
        self.CoolantInPorts = None
        self.CoolantOutPorts = None
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
            self.RackPositions = None
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

    class RackPosition:
        Empty = None
        def __init__(self):
            self.Position = None
            self.Rotation = None

    class ID:
        def __init__(self):
            self.Value = ""

class DataCenterProtoBuilder:
    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None

    class State:
        def __init__(self):
            self.Builder = None

class DataCenterToggleRackCmd:
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
        self.DataCenterId = None
        from Mafi.Core.Prototypes import Proto
        self.ServerRackId = Proto.ID()

        self.Difference = 0

class ServerRackProto:
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
        self.ConsumedPowerPerTick = None
        self.CreatedComputingPerTick = None
        self.ProductToAddThis = None
        self.ProductToRemoveThis = None
        self.CoolantInPerMonth = None
        self.CoolantOutPerMonth = None
        self.Maintenance = None
        self.Graphics = None
        self.IsPhantom = False

    class Gfx:
        Empty = None
        def __init__(self):
            self.IconPath = ""
            self.PrefabPath = ""
            self.FrontPanels = None
