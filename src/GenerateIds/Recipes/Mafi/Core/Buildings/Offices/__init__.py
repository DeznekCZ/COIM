
class CaptainOffice:
    def __init__(self):
        self.UpgradableProto = None
        self.Prototype = None
        self.CanBePaused = False
        self.CurrentState = None
        from Mafi import Option
        self.ElectricityConsumer = Option()
        self.IsActive = False
        self.EmissionIntensity = None
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
        self.PowerRequired = None

    class State:
        None = None
        Paused = None
        NotEnoughWorkers = None
        NotEnoughPower = None
        Working = None
        def __init__(self):
            self.value__ = 0

class CaptainOfficeManager:
    def __init__(self):
        from Mafi import Option
        self.CaptainOffice = Option()
        self.OfficeBuilt = False
        self.IsOfficeActive = False
        self.OnOfficeActiveChanged = None

class CaptainOfficeProto:
    def __init__(self):
        self.EntityType = None
        self.Upgrade = None
        self.TierData = None
        self.ElectricityConsumed = None
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
        self.EmissionIntensity = None
        self.SupportsAdvancedEdicts = False
        self.TradeVolumeDiff = None
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

class OfficeBuilding:
    def __init__(self):
        self.UpgradableProto = None
        self.Prototype = None
        self.CanBePaused = False
        self.CurrentState = None
        self.FocusPointsLastTick = 0
        self.FocusPointsMaxAvailable = 0
        self.PointsMultiplier = None
        self.Maintenance = None
        from Mafi import Option
        self.ElectricityConsumer = Option()
        self.ComputingBoostStep = 0
        self.ComputingConsumer = Option()
        self.EmissionIntensity = None
        self.Progress = None
        self.InputBuffer = None
        self.OutputBuffer = None
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
        self.PowerRequired = None
        self.ComputingRequired = None
        self.MaintenanceCosts = None
        self.IsIdleForMaintenance = False

    class State:
        Paused = None
        Broken = None
        MissingSupplies = None
        MissingWorkers = None
        NotEnoughPower = None
        WorkingComputingLow = None
        Working = None
        def __init__(self):
            self.value__ = 0

class OfficeBuildingProto:
    def __init__(self):
        self.EntityType = None
        self.Upgrade = None
        self.TierData = None
        self.ElectricityConsumed = None
        self.Recipe = None
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
        self.ConsumedPerRecipe = None
        self.ProducedPerRecipe = None
        self.InputBufferCapacity = None
        self.OutputBufferCapacity = None
        self.DurationForRecipe = None
        self.MaxComputingBoostSteps = 0
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

class OfficeBuildingsManager:
    def __init__(self):
        self.AllFocuses = None
        self.FocusPointsAvailable = 0
        self.FocusPointsLastTick = 0
        self.FocusPointsMaxAvailable = 0
        self.FocusPointsRequired = 0

class SetOfficeFocusStepCmd:
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
        from Mafi.Core.Prototypes import Proto
        self.FocusId = Proto.ID()

        self.Diff = 0

class SetOfficeComputingBoostStepCmd:
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
        self.OfficeId = None
        self.Step = 0

class OfficeFocus:
    def __init__(self):
        self.CurrentActiveStep = 0
        self.TargetStep = 0
        self.PointsAssigned = 0
        self.PointsRequired = 0
        self.Prototype = None

class OfficeFocusProto:
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
        self.Implementation = None
        self.MaxStep = 0
        self.Graphics = None
        self.IsPhantom = False

    class DescriptionFunc:
        def __init__(self):
            self.Method = None
            self.Target = None

    class Gfx:
        Empty = None
        def __init__(self):
            self.IconPath = ""

class OfficeFocusWithProperties:
    def __init__(self):
        self.CurrentActiveStep = 0
        self.TargetStep = 0
        self.PointsAssigned = 0
        self.PointsRequired = 0
        self.Prototype = None

class OfficeFocusWithPropertiesProto:
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
        self.PropertiesPerStepToApply = None
        self.Implementation = None
        self.MaxStep = 0
        self.Graphics = None
        self.IsPhantom = False
