class EntityLayout:

    def __init__(self):
        self.CoreSize = None
class TerrainVertexRel:

    def __init__(self):
        pass

class EntityLayoutParams:

    def __init__(self):
        pass

class LayoutTokenSpec:

    def __init__(self):
        pass

class EntityLayoutParser:

    def __init__(self):
        pass

class CustomLayoutToken:

    def __init__(self):
        pass

class ILayoutEntityProtoWithElevation:

    def __init__(self):
        self.CanBeElevated = False
        self.CanPillarsPassThrough = False
class ILayoutEntityProtoWithElevationValidator:

    def __init__(self):
        self.Priority = None
class InvalidEntityLayoutException:

    def __init__(self):
        self.Message = str(0)
        self.Data = None
        self.InnerException = None
        self.TargetSite = None
        self.StackTrace = str(0)
        self.HelpLink = str(0)
        self.Source = str(0)
        self.HResult = int(0)
class LayoutEntity:

    def __init__(self):
        from Mafi import Option
        self.CustomTitle = Option()
        self.GeneralPriority = int(0)
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
        pass

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
        from Mafi import Option
        self.Metadata = Option()
class EntityPlacementPhase:

    def __init__(self):
        pass

class ILayoutEntityProto:

    def __init__(self):
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.IsUnique = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.Id = None
        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.Mod = None
class LayoutEntityProto:

    def __init__(self):
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
class VisualizedLayers:

    def __init__(self):
        pass

class ILayoutEntitySlotBasedValidator:

    def __init__(self):
        pass

class LayoutEntitySlot:

    def __init__(self):
        pass

class LayoutTile:

    def __init__(self):
        pass

class LayoutTileConstraint:

    def __init__(self):
        pass

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
class OccupiedVertexRelative:

    def __init__(self):
        self.RelCoord = None
        self.FromHeightRel = None
        self.ToHeightRelExcl = None
        self.VerticalSize = None
        self.Constraint = None
class OccupiedVertexRelativeExtensions:

    def __init__(self):
        pass

class ToolbarCategoryProto:

    def __init__(self):
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
