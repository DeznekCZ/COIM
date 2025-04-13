
class Lift:
    BUFFER_SCALE = None
    DELAY_BASE = None
    DELAY_PER_HEIGHT = None
    def __init__(self):
        self.Prototype = None
        self.CanBePaused = False
        self.AnimationParams = None
        self.AnimationStatesProvider = None
        self.IsGeneralPriorityVisible = False
        from Mafi import Option
        self.ElectricityConsumer = Option()
        self.MaxBufferSize = None
        self.QuantityInOutputBuffer = None
        self.CustomTitle = Option()
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
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
        self.PowerRequired = None

class ILiftProto:
    def __init__(self):
        self.HeightDelta = None
        self.MinHeightDelta = None
        self.PortsShape = None
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.IsUnique = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.Mod = None

class LiftProto:
    def __init__(self):
        self.EntityType = None
        self.CanBeElevated = False
        self.CanPillarsPassThrough = False
        self.ElectricityConsumed = None
        self.PortsShape = None
        self.AnimationParams = None
        self.HeightDelta = None
        self.MinHeightDelta = None
        self.MaxThroughputPerStep = None
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.Layout = None
        self.Ports = None
        self.CloningDisabled = False
        self.IsUnique = False
        self.CannotBeReflected = False
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
            self.AscendingChildEnabled = ""
            self.DescendingChildEnabled = ""
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

class ReverseLiftCmd:
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
        self.LiftId = None
