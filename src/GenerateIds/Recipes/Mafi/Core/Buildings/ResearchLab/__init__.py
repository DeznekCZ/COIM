
class ResearchLab:
    PROD_COUNTERS_LABELS = None
    def __init__(self):
        self.UpgradableProto = None
        self.Prototype = None
        self.CanBePaused = False
        self.CurrentState = None
        self.AnimationParams = None
        self.AnimationStatesProvider = None
        from Mafi import Option
        self.ElectricityConsumer = Option()
        self.ComputingConsumer = Option()
        self.CurrentResearch = Option()
        self.MaxMonthlyUnityConsumed = None
        self.MonthlyUnityConsumed = None
        from Mafi.Core.Prototypes import Proto
        self.UpointsCategoryId = Proto.ID()

        self.NotEnoughPower = False
        self.UnityConsumer = Option()
        self.EmissionIntensity = None
        self.Maintenance = None
        self.IsBlockedOnAdvancedResearch = False
        self.InputBuffer = Option()
        self.OutputBuffer = Option()
        self.Progress = None
        from Mafi import Fix32
        self.StepsPerRecipe = Fix32()
        self.OngoingMonthlyData = None
        self.ProductivityCounterHistory = None
        self.ProductivityCounterLabels = None
        self.CanDisableLogisticsInput = False
        self.CanDisableLogisticsOutput = False
        self.LogisticsInputMode = None
        self.LogisticsOutputMode = None
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
        self.MaintenanceCosts = None
        self.IsIdleForMaintenance = False
        self.PowerRequired = None
        self.ComputingRequired = None
        self.EfficiencyMultiplier = None

    class State:
        Paused = None
        Broken = None
        Working = None
        Idle = None
        MissingInput = None
        MissingWorkers = None
        NotEnoughUpoints = None
        NotEnoughPower = None
        NotEnoughComputing = None
        ResearchTooDifficult = None
        NotEnoughSpacePoints = None
        def __init__(self):
            self.value__ = 0

class ResearchLabProto:
    def __init__(self):
        self.EntityType = None
        self.Upgrade = None
        self.TierData = None
        self.ElectricityConsumed = None
        self.ComputingConsumed = None
        self.UnityMonthlyCost = None
        self.UpointsCategory = None
        self.AnimationParams = None
        from Mafi import Fix32
        self.SpacePointsConsumedPerRecipe = Fix32()
        self.Recipes = None
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
        self.TierIndex = 0
        self.DurationOfRecipe = None
        self.SciencePerRecipe = Fix32()
        self.SciencePerMinute = Fix32()
        self.ConsumedPerRecipe = None
        self.ProducedPerRecipe = None
        self.InputBufferCapacity = None
        self.OutputBufferCapacity = None
        self.EmissionIntensity = None
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
