class DrivingData:

    def __init__(self):
        self.AxlesDistance = None
class DrivingEntity:

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
class DrivingState:

    def __init__(self):
        pass

class DrivingEntityProto:

    def __init__(self):
        self.CostToBuild = None
        self.DisruptsSurface = False
        self.IconPath = str(0)
        self.Id = None
        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class DynamicEntityProto:

    def __init__(self):
        self.IconPath = str(0)
        self.Id = None
        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class ID:

    def __init__(self):
        pass

class Gfx:

    def __init__(self):
        self.IconPath = str(0)
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
class DynamicGroundEntityProto:

    def __init__(self):
        self.DisruptsSurface = False
        self.IconPath = str(0)
        self.Id = None
        self.EntityType = None
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
        self.IconPath = str(0)
class DustParticlesSpec:

    def __init__(self):
        pass

class DynamicEntityDustParticlesSpec:

    def __init__(self):
        pass

class VehicleExhaustParticlesSpec:

    def __init__(self):
        pass

class ExcavatorProtoBuilder:

    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None
class ExcavatorProtoBuilderState:

    def __init__(self):
        pass

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

    def __init__(self):
        self.IsNotNone = False
class FuelTankProto:

    def __init__(self):
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
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
        self.NavigationFailedStreak = int(0)
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

    def __init__(self):
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
class PathFindingEntityState:

    def __init__(self):
        pass

class SmoothDriver:

    def __init__(self):
        from Mafi import Fix32
        self.Speed = Fix32()
        self.SpeedPercentBase = None
        from Mafi import Fix32
        self.LastStepSpeed = Fix32()
        from Mafi import Fix32
        self.Acceleration = Fix32()
        self.AccelerationPercent = None
        self.AccelerationPercentBase = None
        self.StepsToFullStop = int(0)
        from Mafi import Fix32
        self.DistanceToFullStop = Fix32()
class TruckProtoBuilder:

    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None
class TruckProtoBuilderState:

    def __init__(self):
        pass

class Vehicle:

    def __init__(self):
        self.CanBePaused = False
        from Mafi import Option
        self.CustomTitle = Option()
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
class RefuelRequestIssue:

    def __init__(self):
        pass

class IVehicleFriend:

    def __init__(self):
        pass

class IAssignedVehicleEntityFriend:

    def __init__(self):
        pass

