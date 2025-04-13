
class RocketTransporter:
    def __init__(self):
        self.OwningDepot = None
        from Mafi import Option
        self.AttachedRocketBase = Option()
        self.RocketHolderExtensionPerc = None
        self.CanBePaused = False
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

class RocketTransporterProvider:
    def __init__(self):
        pass


class IRocketTransporterOwner:
    def __init__(self):
        pass


class RocketTransporterProto:
    def __init__(self):
        self.EntityType = None
        self.CostToBuild = None
        self.DisruptsSurface = False
        self.IconPath = ""
        from Mafi.Core.Entities.Dynamic import DynamicEntityProto
        self.Id = DynamicEntityProto.ID()

        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.RocketProto = None
        self.RocketHolderExtensionDuration = None
        self.Graphics = None
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
        self.VehicleQuotaCost = 0
        self.IsPhantom = False
        self.IsInitialized = False

    class Gfx:
        def __init__(self):
            self.IconPath = ""
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
