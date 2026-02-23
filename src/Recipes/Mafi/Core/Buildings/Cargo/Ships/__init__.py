
class CargoShipV2:
    SAVER_FUEL_MULT = None
    SAVER_TRAVEL_DURATION_MULT = None
    def __init__(self):
        from Mafi import Option
        self.CustomTitle = Option()
        self.NonEmptyModules = None
        self.Modules = None
        self.JourneyDuration = None
        self.IsFuelReductionEnabled = False
        self.JobProvider = None
        self.CanPayWithUnityIfOutOfFuel = False
        self.AssignedDepot = Option()
        self.AssignedDockEntity = Option()
        self.FuelProto = None
        self.FuelBuffer = None
        self.FuelData = None
        self.NeedsToDepart = False
        self.FuelConsumptionMultiplier = None
        self.DepartureRequestedByPlayer = False
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.GoingForCargo = False
        self.ArrivedForCargo = False
        self.ArrivedForCargoThisTick = False
        self.ArrivedHome = False
        self.ArrivedHomeThisTick = False
        self.CanBePaused = False
        self.IsAtWorld = False
        self.IsEngineOn = False
        self.IsDocked = False
        self.LastWorldDepartData = None
        self.TravelDirection = None
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
        self.PendingFuelToChangeTo = Option()
        self.OnModuleAdded = None
        self.OnModuleRemoved = None
        self.DockedAt = Option()
        self.JobsContext = None
        self.PathabilityProvider = None
        self.Terrain = None
        self.SurfaceProvider = None
        self.LastDisruptedTile = None

class CargoShip:
    def __init__(self):
        self.CanBePaused = False
        from Mafi import Option
        self.CustomTitle = Option()
        self.State = None
        self.LastDockedStatus = None
        self.IsFuelReductionEnabled = False
        self.CanPayWithUnityIfOutOfFuel = False
        self.CargoDepot = None
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.Id = None
        self.DefaultTitle = None
        self.Prototype = None
        self.Context = None
        self.IsDestroyed = False
        self.IsEnabled = False
        self.IsNotEnabled = False
        self.IsPaused = False
        self.IsNotPaused = False
        self.WorkersNeeded = 0
        self.HasWorkersCached = False
        self.PendingFuelToChangeTo = Option()

    class ForceLeaveMode:
        None = None
        LeftForUpgrade = None
        LeftForDestroy = None
        LeftForBlocked = None
        LeftForFuelTypeChange = None
        def __init__(self):
            self.value__ = 0

    class ShipState:
        ArrivingFromWorld = None
        Docked = None
        DepartingToWorld = None
        AtWorldGoingForCargo = None
        AtWorldReturningHome = None
        ArrivingFromWorldNotYetPathed = None
        def __init__(self):
            self.value__ = 0

    class DockedStatus:
        Ok = None
        NoModulesBuilt = None
        NotEnoughFuel = None
        Paused = None
        ShipIsBeingUnloaded = None
        NothingToPickUp = None
        NotEnoughToPickUp = None
        NotEnoughWorkers = None
        def __init__(self):
            self.value__ = 0

class CargoShipFactory:
    def __init__(self):
        pass


class CargoShipProto:
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
        self.MaximumModulesCount = 0
        self.AvailableModules = None
        self.AvailableFuels = None
        self.CapacityMultiplier = None
        self.Graphics = None
        self.PathFindingParams = None
        self.DrivingData = None
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

    class FuelData:
        def __init__(self):
            self.FuelProto = None
            self.FuelPerJourneyBase = None
            self.FuelPerJourneyPerModule = None
            from Mafi import Option
            self.LockingProto = Option()
            self.CompatibleFuels = None
            self.PollutionPercent = None
            self.Cost = None
            self.CustomGraphics = Option()

    class Gfx:
        EMPTY = None
        def __init__(self):
            self.IconPath = ""
            self.FrontPrefabPath = ""
            self.BackPrefabPath = ""
            self.EmptyModulePrefabPath = ""
            self.ArrivalSoundPath = ""
            self.DepartureSoundPath = ""
            self.BasicBoxColliderSize = None
            self.ModuleSlotLength = None
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

class ICargoShipFactory:
    def __init__(self):
        pass


class CargoShipAssignedToDockJobProviderBase:
    def __init__(self):
        self.LastDockedStatus = None

    class DockedStatus:
        None = None
        NoModulesBuilt = None
        NotEnoughFuel = None
        TransferringCargo = None
        NothingToPickUp = None
        NotEnoughToPickUp = None
        NotEnoughWorkers = None
        def __init__(self):
            self.value__ = 0

class CargoShipContractJobProvider:
    def __init__(self):
        self.LastDockedStatus = None

class CargoShipWorldCargoJobProvider:
    def __init__(self):
        self.LastDockedStatus = None

class ICargoShipJobProviderReadonly:
    def __init__(self):
        pass


class ICargoShipJobProvider:
    def __init__(self):
        pass


class CargoShipDefaultJobProvider:
    def __init__(self):
        pass

