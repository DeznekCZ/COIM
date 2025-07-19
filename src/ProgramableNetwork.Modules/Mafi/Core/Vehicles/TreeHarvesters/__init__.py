class TreeHarvester:
    MAX_SERVICE_DISTANCE = None
    NUM_SECTIONS_AT_MAX_TREE_SIZE = None
    MIN_SECTIONS_PER_CUT = None

    def __init__(self):
        self.AllVehicles = None
        self.State = None
        self.StateChangedOnSimStep = None
        self.Cargo = None
        from Mafi import Option
        self.LastCutTreeProto = Option()
        self.NumSectionsToMake = int(0)
        self.NumCutsMade = int(0)
        from Mafi import Option
        self.ForestryTower = Option()
        self.CurrentStateDuration = None
        self.CurrentStateRemaining = None
        self.ArmStateChangeSpeedFactor = None
        self.CabinDirection = None
        self.CabinDirectionRelative = None
        self.IsCabinAtTarget = False
        self.CabinTarget = None
        self.TruckQueue = None
        self.DidNotFindTreeToHarvest = False
        self.MaxServiceRadius = None
        self.LifetimeTreesHarvested = int(0)
        self.CanBePaused = False
        from Mafi import Option
        self.CustomTitle = Option()
        from Mafi import Option
        self.AssignedZone = Option()
        self.ZoneMask = None
        from Mafi import Option
        self.AssignedTo = Option()
        self.NeedsJob = False
        self.NeedsRefueling = False
        self.IsFuelTankEmpty = False
        self.CannotWorkDueToLowFuel = False
        self.CanRunWithNoFuel = False
        from Mafi import Option
        self.FuelTank = Option()
        self.JobsCount = int(0)
        self.IsEngineOn = False
        self.IsOnWayToDepotForScrap = False
        self.IsOnWayToDepotForReplacement = False
        from Mafi import Option
        self.ReplacementProto = Option()
        self.ReplaceQueued = False
        self.CanBeAssigned = False
        self.HasJobs = False
        self.HasTrueJob = False
        from Mafi import Option
        self.CurrentJob = Option()
        self.IsIdle = False
        self.IsEngineIdle = False
        self.CurrentJobInfo = None
        self.IsStuck = False
        self.Maintenance = None
        self.GeneralPriority = int(0)
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.IsNavigating = False
        self.NavigatedSuccessfully = False
        self.NavigationFailed = False
        self.NavigationFailedStreak = int(0)
        self.PfState = None
        self.TrackExploredTiles = False
        self.PfTask = None
        self.PathFindingResult = None
        from Mafi import Option
        self.NavigationGoal = Option()
        self.PathFindingParams = None
        from Mafi import Option
        self.UnreachableGoal = Option()
        self.IsStrugglingToNavigate = False
        from Mafi import Option
        self.CurrentPathSegment = Option()
        self.DrivingData = None
        self.Target = None
        self.CurrentOrLastDrivingTarget = None
        self.IsDriving = False
        self.IsMoving = False
        self.Speed = None
        self.SpeedPercentOfPeak = None
        self.AccelerationPercentOfPeak = None
        self.SteeringAngle = None
        self.SteeringAccelerationPercent = None
        self.DistanceToFullStop = None
        from Mafi import Fix64
        self.LifetimeDistanceTraveled = Fix64()
        self.TargetIsTerminal = False
        self.DrivingState = None
        self.SpeedFactor = None
        self.CurrentRoadSegmentOrDefault = None
        self.IsDrivingOnRoad = False
        self.Position2f = None
        self.Position3f = None
        self.GroundPositionTile2i = None
        self.GroundPositionTile = None
        self.Direction = None
        self.IsSpawned = False
        self.ForceFlatGround = False
        self.Id = None
        self.DefaultTitle = None
        self.Prototype = None
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
class TreeHarvesterJobProvider:

    def __init__(self):
        pass

class TreeHarvesterState:
    Idle = None
    PositioningArm = None
    CuttingTree = None
    LayingTreeDown = None
    BranchTrimming = None
    RaisingTreeUp = None
    TreeIsUp = None
    PositioningForUnload = None
    UnloadingTree = None
    ReturningFromUnloadWithCargo = None
    ReturningFromUnloadToIdle = None
    FoldingArm = None
    CuttingSection = None

    def __init__(self):
        pass

class TreeHarvesterProto:

    def __init__(self):
        self.EntityType = None
        from Mafi import Option
        self.FuelTankProto = Option()
        self.CostToBuild = None
        self.DisruptsSurface = False
        self.IconPath = str(0)
        self.Id = None
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
class Timings:

    def __init__(self):
        pass

class Gfx:
    Empty = None

    def __init__(self):
        self.IconPath = str(0)
