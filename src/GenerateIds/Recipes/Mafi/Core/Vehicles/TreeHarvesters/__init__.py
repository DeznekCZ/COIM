
class TreeHarvester:
    MAX_SERVICE_DISTANCE = None
    NUM_SECTIONS_AT_MAX_TREE_SIZE = 0
    MIN_SECTIONS_PER_CUT = 0
    PROD_COUNTERS_LABELS = None
    def __init__(self):
        self.AllVehicles = None
        self.State = None
        self.StateChangedOnSimStep = None
        self.Cargo = None
        from Mafi import Option
        self.LastCutTreeProto = Option()
        self.NumSectionsToMake = 0
        self.NumCutsMade = 0
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
        self.LifetimeTreesHarvested = 0
        self.OngoingMonthlyData = None
        self.ProductivityCounterHistory = None
        self.ProductivityCounterLabels = None
        self.CanBePaused = False
        self.CustomTitle = Option()
        self.AssignedZone = Option()
        self.ZoneMask = None
        self.AssignedTo = Option()
        self.NeedsJob = False
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
        self.IsIdle = False
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
        self.TreeToBeCut = None
        self.TruckToBeLoaded = Option()
        self.FailedToRequestFuelTruck = False
        self.LastRefuelRequestIssue = None
        self.PathabilityProvider = None
        self.Terrain = None
        self.SurfaceProvider = None
        self.LastDisruptedTile = None

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
        self.value__ = 0

class TreeHarvesterProto:
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
        self.HarvestTimings = None
        self.RotatingCabinDriverProto = None
        self.TreeHarvestDistance = None
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

    class Timings:
        def __init__(self):
            self.ToPrepareForHarvestDuration = None
            self.ToTreeLayingDownDuration = None
            self.ToTreeAboveTruckDuration = None
            self.ToTreeOnTruckDuration = None
            self.ToArmUpDuration = None
            self.MoveToNextSectionDuration = None
            self.CutNextSectionDuration = None
            self.ToFoldedDuration = None
            self.CuttingDuration = None
            self.TrimmingDuration = None

    class Gfx:
        Empty = None
        def __init__(self):
            self.IconPath = ""
            self.CabinObjectPath = ""
            self.LeftTrackObjectPath = ""
            self.RightTrackObjectPath = ""
            self.SpacingBetweenTracks = None
            self.TrackTextureLength = None
            self.TreeHolderOffset = 0.0
            self.GripperWidth = 0.0
            self.IdleAnimStateName = ""
            self.PreparedForHarvestAnimStateName = ""
            self.TreeLayingDownAnimStateName = ""
            self.TreeAboveTruckAnimStateName = ""
            self.TreeOnTruckAnimStateName = ""
            self.TreeFromTruckAnimStateName = ""
            self.FoldedAnimStateName = ""
            self.HarvestedTreeParentObjectPath = ""
            self.RotatingHandObjectPath = ""
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
