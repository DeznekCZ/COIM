
class Excavator:
    PROD_COUNTERS_LABELS = None
    def __init__(self):
        self.NeedsJob = False
        self.IsEmpty = False
        self.IsNotEmpty = False
        self.IsFull = False
        self.IsNotFull = False
        self.RemainingCapacity = None
        self.QueueEnabledChanged = None
        from Mafi import Option
        self.PrioritizedProduct = Option()
        self.Cargo = None
        self.MaxServiceRadius = None
        self.MineTower = Option()
        self.IsIdle = False
        self.State = None
        self.StateChangedOnSimStep = None
        self.IsShovelAtTarget = False
        self.ShovelState = None
        self.NextShovelState = None
        self.CabinDirection = None
        self.CabinDirectionRelative = None
        self.IsCabinAtTarget = False
        self.CabinTargetDelta = None
        self.CabinTarget = None
        self.TruckQueue = None
        self.CurrentMineTimings = None
        self.CurrentDumpTimings = None
        self.OngoingMonthlyData = None
        self.ProductivityCounterHistory = None
        self.ProductivityCounterLabels = None
        self.CanBePaused = False
        self.CustomTitle = Option()
        self.AssignedZone = Option()
        self.ZoneMask = None
        self.AssignedTo = Option()
        self.NeedsRefueling = False
        self.IsFuelTankEmpty = False
        self.CannotWorkDueToLowFuel = False
        self.CanRunWithNoFuel = False
        self.FuelTank = Option()
        self.IsEngineOn = False
        self.IsOnWayToDepotForScrap = False
        self.IsOnWayToDepotForReplacement = False
        self.ReplacementProto = Option()
        self.ReplaceQueued = False
        self.CanBeAssigned = False
        self.Maintenance = None
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.JobsCount = 0
        self.HasJobs = False
        self.HasTrueJob = False
        self.CurrentJob = Option()
        self.IsEngineIdle = False
        self.CurrentJobInfo = None
        self.IsNavigating = False
        self.NavigatedSuccessfully = False
        self.NavigationFailed = False
        self.NavigationFailedStreak = 0
        self.PfState = None
        self.IsStuck = False
        self.TrackExploredTiles = False
        self.PfTask = None
        self.PathFindingResult = None
        self.NavigationGoal = Option()
        self.PathFindingParams = None
        self.UnreachableGoal = Option()
        self.IsStrugglingToNavigate = False
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
        self.IsDrivingOnRoad = False
        self.CurrentRoadEntity = Option()
        self.CurrentRoadDirection = None
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
        self.WorkersNeeded = 0
        self.HasWorkersCached = False
        self.MaintenanceCosts = None
        self.IsIdleForMaintenance = False
        self.FailedToRequestFuelTruck = False
        self.LastRefuelRequestIssue = None
        self.PathabilityProvider = None
        self.Terrain = None
        self.SurfaceProvider = None
        self.LastDisruptedTile = None

class ExcavatorJobProvider:
    def __init__(self):
        pass


class ExcavatorState:
    Idle = None
    DoJob = None
    WaitingForTruck = None
    LoadTruck = None
    WaitingForShovel = None
    GettingUnstuck = None
    def __init__(self):
        self.value__ = 0

class ExcavatorShovelState:
    Tucked = None
    PrepareToMine = None
    Mine = None
    PrepareToDump = None
    Dump = None
    def __init__(self):
        self.value__ = 0

class ExcavatorConfigExtensions:
    def __init__(self):
        pass


class ExcavatorProto:
    def __init__(self):
        self.EntityType = None
        from Mafi import Option
        self.FuelTankProto = Option()
        self.CostToBuild = None
        self.VehicleClassId_RendererData = 0
        self.IsAmphibious = False
        self.DisruptsSurface = False
        self.IconPath = ""
        from Mafi.Core.Entities.Dynamic import DynamicEntityProto
        self.Id = DynamicEntityProto.ID()

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
        self.Capacity = None
        self.MinMiningDistance = None
        self.MaxMiningDistance = None
        self.MaxMiningOceanDepth = None
        self.MinedThicknessByDistance = None
        self.MineAnimations = None
        self.DumpAnimations = None
        self.RotatingCabinDriverProto = None
        self.Graphics = None
        self.DrivingData = None
        self.PathFindingParams = None
        self.VehicleQuotaCost = 0
        self.NextTier = Option()
        self.UIOrder = 0.0
        self.EntitySize = None
        self.NavTolerance = None
        self.DisruptionByDistance = None
        self.BuildDurationPerProduct = None
        self.BuildExtraDuration = None
        self.MaxWheelWaterSubmerge = None
        self.IsPhantom = False

    class AnimationSelector:
        def __init__(self):
            self.Method = None
            self.Target = None

    class MineTimings:
        def __init__(self):
            self.PrepareToMineAnimationId = 0
            self.MineAnimationId = 0
            self.SelectorPredicate = None
            self.PrepareToMineDuration = None
            self.MineDuration = None
            self.MineIterations = 0
            self.MineIterationDuration = None
            self.PrepareToMineAnimationName = ""
            self.MineAnimationName = ""

    class DumpTimings:
        def __init__(self):
            self.PrepareToDumpAnimationId = 0
            self.DumpAnimationId = 0
            self.SelectorPredicate = None
            self.PrepareToDumpDuration = None
            self.DumpDuration = None
            self.DumpDelay = None
            self.PrepareToDumpAnimationName = ""
            self.DumpAnimationName = ""

    class Gfx:
        Empty = None
        def __init__(self):
            self.IdleStateId = 0
            self.IconPath = ""
            self.CabinModelName = ""
            self.LeftTrackModelName = ""
            self.RightTrackModelName = ""
            self.SpacingBetweenTracks = None
            self.TrackTextureLength = None
            self.IdleStateName = ""
            self.PileParentPath = ""
            self.PileModelName = ""
            self.DigSounds = None
            self.DumpSounds = None
            self.CabinAngleCompensation = None
            self.PrefabPath = ""
            self.FrontContactPtsOffset = None
            self.RearContactPtsOffset = None
            self.DustParticles = None
            from Mafi import Option
            self.ExhaustParticlesSpec = Option()
            self.EngineSoundPath = ""
            self.MovementSoundPath = ""
            self.IconIsCustom = False
            self.Color = None
            self.RendererIndex = 0
