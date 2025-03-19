class ResearchLab:

    def __init__(self):
        self.Prototype = None
        self.CanBePaused = False
        self.CurrentState = None
        self.AnimationParams = None
        self.AnimationStatesProvider = None
        from Mafi import Fix32
        self.ResearchPointsPerMonth = Fix32()
        from Mafi import Option
        self.ElectricityConsumer = Option()
        from Mafi import Option
        self.ComputingConsumer = Option()
        from Mafi import Option
        self.CurrentResearch = Option()
        self.MaxMonthlyUnityConsumed = None
        self.MonthlyUnityConsumed = None
        self.UpointsCategoryId = None
        self.Upgrader = None
        self.NotEnoughPower = False
        from Mafi import Option
        self.UnityConsumer = Option()
        self.EmissionIntensity = None
        self.Maintenance = None
        self.IsBlockedOnAdvancedResearch = False
        from Mafi import Option
        self.InputBuffer = Option()
        from Mafi import Option
        self.OutputBuffer = Option()
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
        self.MaintenanceCosts = None
        self.IsIdleForMaintenance = False
        self.PowerRequired = None
        self.ComputingRequired = None
class State:

    def __init__(self):
        pass

class ResearchLabProto:

    def __init__(self):
        self.EntityType = None
        self.Upgrade = None
        self.UpgradeNonGeneric = None
        self.ElectricityConsumed = None
        self.ComputingConsumed = None
        self.UnityMonthlyCost = None
        self.UpointsCategory = None
        self.AnimationParams = None
        self.Recipes = None
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
class ResearchLabProtoBuilder:

    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None
class State:

    def __init__(self):
        pass

