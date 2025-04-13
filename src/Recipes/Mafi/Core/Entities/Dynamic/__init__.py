
class DrivingData:
    def __init__(self):
        self.AxlesDistance = None
        self.MaxForwardsSpeed = None
        self.MaxBackwardsSpeed = None
        self.SteeringSpeedMult = None
        self.Acceleration = None
        self.Braking = None
        self.MaxSteeringAngle = None
        self.MaxSteeringSpeed = None
        from Mafi import Fix32
        self.BrakingConservativness = Fix32()
        self.SteeringAxleOffset = None
        self.NonSteeringAxleOffset = None
        self.CanTurnInPlace = False

class DrivingEntity:
    DEFAULT_DRIVING_TOLERANCE = None
    MAX_GOAL_ANGLE_BEFORE_STARTING = None
    def __init__(self):
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
        self.IsEngineOn = False
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
        self.CanBePaused = False
        self.IsEnabled = False
        self.IsNotEnabled = False
        self.IsPaused = False
        self.IsNotPaused = False
        self.RendererData = None
        self.Terrain = None
        self.SurfaceProvider = None
        self.LastDisruptedTile = None

class DrivingState:
    Stopped = None
    Stopping = None
    StopAndContinueForwards = None
    DrivingForwards = None
    DrivingBackwards = None
    TurningInPlace = None
    Paused = None
    StopAndContinueBackwards = None
    DrivingForwardsOnRoad = None
    def __init__(self):
        self.value__ = 0

class DrivingEntityProto:
    def __init__(self):
        self.CostToBuild = None
        self.DisruptsSurface = False
        self.IconPath = ""
        from Mafi.Core.Entities.Dynamic import DynamicEntityProto
        self.Id = DynamicEntityProto.ID()

        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.DrivingData = None
        from Mafi import Option
        self.FuelTankProto = Option()
        self.PathFindingParams = None
        self.NextTier = Option()
        self.UIOrder = 0.0
        self.EntitySize = None
        self.NavTolerance = None
        self.DisruptionByDistance = None
        self.BuildDurationPerProduct = None
        self.BuildExtraDuration = None
        self.Graphics = None
        self.VehicleQuotaCost = 0
        self.IsPhantom = False
        self.IsInitialized = False

class DynamicEntityProto:
    def __init__(self):
        self.IconPath = ""
        from Mafi.Core.Entities.Dynamic import DynamicEntityProto
        self.Id = DynamicEntityProto.ID()

        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.Graphics = None
        self.VehicleQuotaCost = 0
        self.IsPhantom = False
        self.IsInitialized = False

    class ID:
        def __init__(self):
            self.Value = ""

    class Gfx:
        def __init__(self):
            self.IconPath = ""
            self.IconIsCustom = False
            self.Color = None
            self.RendererIndex = 0

class DynamicGroundEntity:
    def __init__(self):
        self.Position2f = None
        self.Position3f = None
        self.GroundPositionTile2i = None
        self.GroundPositionTile = None
        self.Direction = None
        self.IsSpawned = False
        self.Speed = None
        self.ForceFlatGround = False
        self.Id = None
        self.DefaultTitle = None
        self.Prototype = None
        self.Context = None
        self.IsDestroyed = False
        self.CanBePaused = False
        self.IsEnabled = False
        self.IsNotEnabled = False
        self.IsPaused = False
        self.IsNotPaused = False
        self.RendererData = None
        self.Terrain = None
        self.SurfaceProvider = None
        self.LastDisruptedTile = None

class DynamicGroundEntityProto:
    def __init__(self):
        self.DisruptsSurface = False
        self.IconPath = ""
        from Mafi.Core.Entities.Dynamic import DynamicEntityProto
        self.Id = DynamicEntityProto.ID()

        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.EntitySize = None
        self.NavTolerance = None
        self.DisruptionByDistance = None
        self.BuildDurationPerProduct = None
        self.BuildExtraDuration = None
        self.Graphics = None
        self.VehicleQuotaCost = 0
        self.IsPhantom = False
        self.IsInitialized = False

    class Gfx:
        Empty = None
        def __init__(self):
            self.IconPath = ""
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

class DustParticlesSpec:
    def __init__(self):
        self.PrefabPath = ""
        self.DustScale = 0.0
        self.RelativePosition = None

class DynamicEntityDustParticlesSpec:
    def __init__(self):
        self.ParticlesPerSpeedMult = 0.0
        self.MinSpeed = None
        self.ParticlesPerDegreeMult = 0.0
        self.PrefabPath = ""
        self.DustScale = 0.0
        self.RelativePosition = None

class VehicleExhaustParticlesSpec:
    def __init__(self):
        self.GameObjectPaths = None
        self.BaseParticleRate = 0.0
        self.ParticlesSpeedRate = 0.0
        self.ParticlesAccelerationRate = 0.0
        self.BaseEmitSpeed = 0.0
        self.VariableEmitSpeed = 0.0
        self.MaxRate = 0.0
        self.InverseMaxParticleRate = 0.0

class ExcavatorProtoBuilder:
    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None

    class ExcavatorProtoBuilderState:
        def __init__(self):
            self.Builder = None

class IFuelTankReadonly:
    def __init__(self):
        self.Info = None
        self.Proto = None
        self.RemainingDuration = None

class FuelTank:
    def __init__(self):
        self.Info = None
        self.RemainingDuration = None
        self.Proto = None
        self.IsEmpty = False
        self.IsUnderReserve = False

    class TankInfo:
        NONE = None
        def __init__(self):
            self.IsNotNone = False
            self.Product = None
            self.Capacity = None
            self.Quantity = None
            self.PercentFull = None

class FuelTankProto:
    def __init__(self):
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.Product = None
        self.WasteProduct = None
        self.PollutionPercent = None
        self.Capacity = None
        self.Duration = None
        self.ReserveDuration = None
        self.IdleFuelConsumption = None
        self.IsPhantom = False
        self.IsInitialized = False

class FuelTankProtoBuilder:
    def __init__(self):
        pass


    class State:
        def __init__(self):
            pass


class PathfindingData:
    def __init__(self):
        pass


class IPathFindingVehicle:
    def __init__(self):
        self.Id = None
        self.PathFindingParams = None
        self.TrackExploredTiles = False
        from Mafi import Option
        self.NavigationGoal = Option()
        self.PathFindingResult = None
        self.NavigationFailedStreak = 0
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.DefaultTitle = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False

class PathFindingEntity:
    MAX_DIST_DURING_PF_SQR = 0
    STRUGGLING_STREAK_PRIO_PENALTY = None
    STRUGGLING_STREAK_MAX_PENALTY = None
    def __init__(self):
        self.IsNavigating = False
        self.NavigatedSuccessfully = False
        self.NavigationFailed = False
        self.NavigationFailedStreak = 0
        self.PfState = None
        self.TrackExploredTiles = False
        self.PfTask = None
        self.PathFindingResult = None
        from Mafi import Option
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
        self.IsEngineOn = False
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
        self.CanBePaused = False
        self.IsEnabled = False
        self.IsNotEnabled = False
        self.IsPaused = False
        self.IsNotPaused = False
        self.RendererData = None
        self.Terrain = None
        self.SurfaceProvider = None
        self.LastDisruptedTile = None

class PathFindingEntityState:
    Idle = None
    PathFinding = None
    DrivingToDestination = None
    DrivingToValidLocation = None
    FindingValidLocation = None
    ReadyToDriveToDestination = None
    def __init__(self):
        self.value__ = 0

class SmoothDriver:
    def __init__(self):
        from Mafi import Fix32
        self.Speed = Fix32()
        self.SpeedPercentBase = None
        self.LastStepSpeed = Fix32()
        self.Acceleration = Fix32()
        self.AccelerationPercent = None
        self.AccelerationPercentBase = None
        self.StepsToFullStop = 0
        self.DistanceToFullStop = Fix32()

class TruckProtoBuilder:
    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None

    class TruckProtoBuilderState:
        def __init__(self):
            self.Builder = None

class Vehicle:
    SPEED_WHEN_BROKEN = None
    SPEED_ON_LOW_FUEL = None
    def __init__(self):
        self.CanBePaused = False
        from Mafi import Option
        self.CustomTitle = Option()
        self.AssignedTo = Option()
        self.NeedsJob = False
        self.NeedsRefueling = False
        self.IsFuelTankEmpty = False
        self.CannotWorkDueToLowFuel = False
        self.CanRunWithNoFuel = False
        self.FuelTank = Option()
        self.JobsCount = 0
        self.IsEngineOn = False
        self.IsOnWayToDepotForScrap = False
        self.IsOnWayToDepotForReplacement = False
        self.ReplacementProto = Option()
        self.ReplaceQueued = False
        self.CanBeAssigned = False
        self.HasJobs = False
        self.HasTrueJob = False
        self.CurrentJob = Option()
        self.IsIdle = False
        self.CurrentJobInfo = None
        self.IsStuck = False
        self.Maintenance = None
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.IsNavigating = False
        self.NavigatedSuccessfully = False
        self.NavigationFailed = False
        self.NavigationFailedStreak = 0
        self.PfState = None
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
        self.WorkersNeeded = 0
        self.HasWorkersCached = False
        self.MaintenanceCosts = None
        self.IsIdleForMaintenance = False
        self.FailedToRequestFuelTruck = False
        self.LastRefuelRequestIssue = None
        self.Terrain = None
        self.SurfaceProvider = None
        self.LastDisruptedTile = None

class RefuelRequestIssue:
    None = None
    Failed = None
    FailedAsUnreachable = None
    def __init__(self):
        self.value__ = 0

class IVehicleFriend:
    def __init__(self):
        pass


class IAssignedVehicleEntityFriend:
    def __init__(self):
        pass

