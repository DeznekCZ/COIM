class NuclearReactor:

    def __init__(self):
        self.MaxPowerLevel = int(0)
        self.MaxPowerLevelPercent = None
        self.MeltdownAtHeat = int(0)
        self.Prototype = None
        self.CanBePaused = False
        self.Upgrader = None
        self.LogisticsInputControl = None
        self.LogisticsOutputControl = None
        self.IsLogisticsInputDisabled = False
        self.IsLogisticsOutputDisabled = False
        self.IsCargoAffectedByGeneralPriority = False
        self.Maintenance = None
        self.CurrentState = None
        self.ActiveRecipes = None
        self.AllRecipes = None
        self.EmissionIntensity = None
        self.SoundParams = None
        self.IsSoundOn = False
        self.ComputingRequired = None
        from Mafi import Option
        self.ComputingConsumer = Option()
        self.DurationMultiplier = None
        self.HeatAmount = int(0)
        self.OptimalHeatForCurrentPower = int(0)
        self.StartEmergencyCoolingAtHeat = int(0)
        self.IsInMeltdown = False
        self.CurrentPowerLevel = None
        self.TargetPowerLevel = None
        self.CoolantInBuffer = None
        self.CoolantOutBuffer = None
        self.AllowedFuel = None
        self.IsAutomaticPowerRegulationSupported = False
        self.IsAutomaticPowerRegulationEnabled = False
        from Mafi import Option
        self.EnrichmentInputBuffer = Option()
        from Mafi import Option
        self.EnrichmentOutputBuffer = Option()
        from Mafi import Option
        self.CustomTitle = Option()
        self.GeneralPriority = int(0)
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
        self.IsBoosted = False
        self.WorkedThisTick = False
class Recipe:

    def __init__(self):
        self.Id = None
        self.AllUserVisibleInputs = None
        self.AllUserVisibleOutputs = None
        self.Duration = None
class State:

    def __init__(self):
        pass

class NuclearReactorSetPowerLevelCmd:

    def __init__(self):
        self.AffectsSaveState = False
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.IsVerificationCmd = False
        self.Result = False
        self.HasError = False
        self.ErrorMessage = str(0)
class NuclearReactorToggleAllowedFuelCmd:

    def __init__(self):
        self.AffectsSaveState = False
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.IsVerificationCmd = False
        self.Result = False
        self.HasError = False
        self.ErrorMessage = str(0)
class NuclearReactorToggleAutomaticRegulationCmd:

    def __init__(self):
        self.AffectsSaveState = False
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.IsVerificationCmd = False
        self.Result = False
        self.HasError = False
        self.ErrorMessage = str(0)
class NuclearReactorProto:

    def __init__(self):
        self.EntityType = None
        self.ComputingConsumed = None
        self.Upgrade = None
        self.UpgradeNonGeneric = None
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
class Gfx:

    def __init__(self):
        self.PrefabPath = str(0)
        self.PrefabOrigin = None
        self.IconPath = str(0)
        self.VisualizedLayers = None
        self.Categories = None
class FuelData:

    def __init__(self):
        self.IsEmpty = False
class EnrichmentData:

    def __init__(self):
        pass

