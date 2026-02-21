
class Truck:
    PROD_COUNTERS_LABELS = None
    def __init__(self):
        self.ProductType = None
        self.Cargo = None
        self.TotalCargoQuantity = None
        self.IsEmpty = False
        self.IsNotEmpty = False
        self.IsFull = False
        self.IsNotFull = False
        self.RemainingCapacity = None
        self.IsIdle = False
        self.Capacity = None
        self.IsDumping = False
        from Mafi import Option
        self.LayoutEntity = Option()
        self.IsCannotDeliverNotificationActive = False
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
        self.DefaultProduct = Option()
        self.DumpingOfAllCargoPending = False
        self.FailedToRequestFuelTruck = False
        self.LastRefuelRequestIssue = None
        self.PathabilityProvider = None
        self.Terrain = None
        self.SurfaceProvider = None
        self.LastDisruptedTile = None

    class DumpTileFn:
        def __init__(self):
            self.Method = None
            self.Target = None

class TruckQueue:
    def __init__(self):
        self.Owner = None
        from Mafi import Option
        self.FirstVehicle = Option()
        self.WaitingTrucksCount = 0
        self.ArrivingTrucksCount = 0
        self.TrucksCount = 0
        self.IsEnabled = False

class AttachmentProto:
    def __init__(self):
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
        self.EligibleProductsFilter = None
        self.KeepOnEvenIfNotNeeded = False
        self.Graphics = None
        self.IsPhantom = False

    class Gfx:
        Empty = None
        def __init__(self):
            self.IconPath = ""
            self.PrefabPath = ""
            self.ColorsMap = None
            self.IconIsCustom = False

class DumpAttachmentProto:
    def __init__(self):
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
        self.Graphics = None
        self.EligibleProductsFilter = None
        self.KeepOnEvenIfNotNeeded = False
        self.IsPhantom = False

    class Gfx:
        Empty = None
        def __init__(self):
            self.IconPath = ""
            self.SmoothPileObjectPath = ""
            self.RoughPileObjectPath = ""
            self.PileTextureParams = None
            self.OffsetEmpty = None
            self.OffsetFull = None
            from Mafi import Option
            self.AnimationStateName = Option()
            self.PrefabPath = ""
            self.ColorsMap = None
            self.IconIsCustom = False

class FlatBedAttachmentProto:
    def __init__(self):
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
        self.Graphics = None
        self.EligibleProductsFilter = None
        self.KeepOnEvenIfNotNeeded = False
        self.IsPhantom = False

    class Gfx:
        Empty = None
        def __init__(self):
            self.MaxProductRenderCapacity = 0
            self.ProductRenderOffsets = None
            self.IconPath = ""
            self.ShelfName = ""
            self.PrefabPath = ""
            self.ColorsMap = None
            self.IconIsCustom = False

class TankAttachmentProto:
    def __init__(self):
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
        self.Graphics = None
        self.EligibleProductsFilter = None
        self.KeepOnEvenIfNotNeeded = False
        self.IsPhantom = False

    class Gfx:
        Empty = None
        def __init__(self):
            self.IconPath = ""
            self.IconChildName = ""
            self.BodyChildName = ""
            self.DefaultColor = None
            self.DefaultAccentColor = None
            self.PrefabPath = ""
            self.ColorsMap = None
            self.IconIsCustom = False

class TruckProto:
    def __init__(self):
        self.EntityType = None
        self.AllowedProducts = None
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
        self.ProductType = None
        self.CapacityBase = None
        self.Attachments = None
        self.AttachmentWhenEmpty = Option()
        self.VehicleGroup = None
        self.MaxDumpingDistance = None
        self.MinDumpingDistance = None
        self.MaxSurfaceProcessingDistance = None
        self.MinSurfaceProcessingDistance = None
        self.DumpedThicknessByDistance = None
        self.MinDumpIterationsWithoutQuantityChanged = 0
        self.DumpIterationDuration = None
        self.CargoPickupDuration = None
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

    class Gfx:
        Empty = None
        def __init__(self):
            self.IconPath = ""
            self.SteeringWheelsSubmodelPaths = None
            self.StaticWheelsSubmodelPaths = None
            self.WheelDiameter = None
            self.LeftTrackModelName = ""
            self.RightTrackModelName = ""
            self.SpacingBetweenTracks = None
            self.TrackTextureLength = None
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

class ITruckQueueObserver:
    def __init__(self):
        pass

