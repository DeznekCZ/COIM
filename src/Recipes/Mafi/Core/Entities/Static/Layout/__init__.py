
class EntityLayout:
    VEHICLE_INACCESSIBLE_HEIGHT = None
    VEHICLE_INACCESSIBLE_HEIGHT_REL = None
    ANY_COMPATIBLE_PORT = ""
    def __init__(self):
        self.CoreSize = None
        self.SourceLayoutStr = ""
        self.LayoutTiles = None
        self.TerrainVertices = None
        self.VehicleSurfaceHeights = None
        self.Ports = None
        self.LayoutParams = None
        self.LayoutMinHeight = None
        self.LayoutSize = None
        self.OriginTile = None
        self.CoreMin = None
        self.CoreMax = None
        self.TilesCount = 0
        self.CombinedConstraint = None
        self.CollapseVerticesThreshold = 0
        self.PlacementHeightRange = None

class TerrainVertexRel:
    def __init__(self):
        self.Coord = None
        self.OccupiedThickness = None
        self.Constraint = None
        from Mafi import Option
        self.TerrainMaterial = Option()
        self.TerrainHeight = None
        self.MinTerrainHeight = None
        self.MaxTerrainHeight = None
        self.VehicleSurfaceRelHeight = None
        self.ContributingTiles = 0
        self.LowestTileIndex = 0

class EntityLayoutParams:
    DEFAULT = None
    def __init__(self):
        self.EnforceEmptySurface = False
        from Mafi import Option
        self.IgnoreTilesForCore = Option()
        self.CustomTokens = None
        self.PortsCanOnlyConnectToTransports = False
        from Mafi.Core.Prototypes import Proto
        self.HardenedFloorSurfaceId = Proto.ID()

        self.CustomVertexDataLayout = Option()
        self.CustomVertexTransformFn = Option()
        self.CustomCollapseVerticesThreshold = None
        self.CustomPlacementRange = None
        self.CustomPortHeights = None

class LayoutTokenSpec:
    def __init__(self):
        self.HeightFrom = None
        self.HeightToExcl = None
        self.Constraint = None
        self.TerrainSurfaceHeight = None
        self.MinTerrainHeight = None
        self.MaxTerrainHeight = None
        self.VehicleHeight = None
        self.TerrainMaterialId = None
        self.SurfaceId = None
        self.IsRamp = False
        self.IsPort = False
        self.PortHeight = 0

class EntityLayoutParser:
    VEHICLE_SURFACE_EXTRA_THICKNESS = None
    from Mafi.Core.Prototypes import Proto
    DEFAULT_HARDENED_SURFACE = Proto.ID('DefaultConcrete_TerrainSurface')
    DEFAULT_HARDENED_MATERIAL = Proto.ID('HardenedRock_Terrain')
    MAX_TERRAIN_SURFACE_DIFF_TILES = 0
    def __init__(self):
        pass


class CustomLayoutToken:
    def __init__(self):
        self.Token = ""
        self.CreateTokenSpecFn = None

class ILayoutEntityProtoWithElevation:
    def __init__(self):
        self.CanBeElevated = False
        self.CanPillarsPassThrough = False

class ILayoutEntityProtoWithElevationValidator:
    def __init__(self):
        self.Priority = None

class InvalidEntityLayoutException:
    def __init__(self):
        self.Message = ""
        self.Data = None
        self.InnerException = None
        self.TargetSite = None
        self.StackTrace = ""
        self.HelpLink = ""
        self.Source = ""
        self.HResult = 0

class LayoutEntity:
    def __init__(self):
        from Mafi import Option
        self.CustomTitle = Option()
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.Ports = None
        self.Value = None
        self.ConstructionCost = None
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

class LayoutEntityBase:
    def __init__(self):
        self.Value = None
        self.ConstructionCost = None
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

class LayoutEntityAddRequestFactory:
    def __init__(self):
        pass


class EntityAddRequestData:
    def __init__(self):
        self.Transform = None
        self.DisableMiniZipperPlacement = False
        from Mafi import Option
        self.IgnoreForCollisions = Option()
        self.RecordTileErrors = False

class DefaultLayoutEntityAddRequestFactory:
    def __init__(self):
        pass


class LayoutEntityAddRequest:
    def __init__(self):
        self.Proto = None
        self.Layout = None
        self.RecordTileErrorsAndMetadata = False
        self.PlacementPhase = None
        self.HasAdditionalErrorTiles = False
        self.Transform = None
        self.Origin = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        from Mafi import Option
        self.IgnoreForCollisions = Option()
        self.ReasonToAdd = None
        self.Metadata = Option()

class EntityPlacementPhase:
    First = None
    Final = None
    FirstAndFinal = None
    def __init__(self):
        self.value__ = 0

class ILayoutEntityProto:
    def __init__(self):
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

class LayoutEntityProto:
    DEFAULT_CONSTR_DUR_PER_PRODUCT = None
    def __init__(self):
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

    class VisualizedLayers:
        Empty = None
        def __init__(self):
            self.TerrainMaterials = None
            self.VirtualResources = None
            self.AllVisualizedProducts = None
            self.TerrainDesignators = False
            self.TreeDesignators = False

class ILayoutEntitySlotBasedValidator:
    def __init__(self):
        pass


class LayoutEntitySlot:
    def __init__(self):
        self.Transform = None
        self.AllowAnyRotationAndReflection = False

class LayoutTile:
    def __init__(self):
        self.Coord = None
        self.OccupiedThickness = None
        self.TerrainHeight = None
        self.MinTerrainHeight = None
        self.MaxTerrainHeight = None
        self.Constraint = None
        from Mafi import Option
        self.TerrainMaterialProto = Option()
        self.TileSurfaceProto = Option()
        self.HasVehicleSurface = False

class LayoutTileConstraint:
    None = None
    Ground = None
    Ocean = None
    UsingPillar = None
    DisableTerrainPhysics = None
    NoRubbleAfterCollapse = None
    def __init__(self):
        self.value__ = 0

class LayoutTileConstraintExtensions:
    def __init__(self):
        pass


class OccupiedTileRelative:
    def __init__(self):
        self.RelCoord = None
        self.FromHeightRel = None
        self.ToHeightRelExcl = None
        self.VerticalSize = None
        self.Constraint = None
        self.TileSurfaceRelHeight = None
        self.RelativeX = None
        self.RelativeY = None
        self.ConstraintSlim = None
        self.RelativeFrom = None
        self.VerticalSizeRaw = None
        self.TileSurfaceRelHeightRaw = None
        self.TileSurface = None

class OccupiedVertexRelative:
    def __init__(self):
        self.RelCoord = None
        self.FromHeightRel = None
        self.ToHeightRelExcl = None
        self.VerticalSize = None
        self.Constraint = None
        self.RelativeX = None
        self.RelativeY = None
        self.RelativeFromRaw = None
        self.VerticalSizeRaw = None
        self.TerrainHeightSlim = None
        self.TerrainHeightAfterDeconstructionSlim = None
        self.MinTerrainHeightOrMinValueRaw = None
        self.MaxTerrainHeightOrMaxValueRaw = None
        self.LowestTileIndex = None
        self.TerrainMaterial = None

class OccupiedVertexRelativeExtensions:
    def __init__(self):
        pass


class ToolbarCategoryProto:
    from Mafi.Core.Prototypes import Proto
    PHANTOM_CATEGORY_ID = Proto.ID('__PHANTOM__TOOLBAR_CAT__')
    Phantom = None
    def __init__(self):
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.Order = 0.0
        self.ShortcutId = ""
        self.IconPath = ""
        self.IsTransportBuildAllowed = False
        self.ContainsTransports = False
        self.IsPhantom = False
        self.IsInitialized = False
