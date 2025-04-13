
class Ruins:
    def __init__(self):
        self.CanBePaused = False
        self.IsScrapping = False
        self.ScrapProgress = None
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
        self.IsEnabled = False
        self.IsNotEnabled = False
        self.IsPaused = False
        self.IsNotPaused = False
        self.RendererData = None

class RuinsScrapCmd:
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
        self.RuinsId = None
        self.IsScrapping = False

class RuinsProto:
    def __init__(self):
        self.EntityType = None
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
        self.DurationPerProduct = None
        self.ProductsGiven = None
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.CannotBeBuiltByPlayer = False
        self.ConstructionDurationPerProduct = None
        self.VehicleGoalHeightAllowedRange = None
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False
        self.IsInitialized = False

class RuinsProtoBuilder:
    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None

    class State:
        def __init__(self):
            self.Builder = None
