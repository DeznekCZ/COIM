
class WorldMapLocId:
    def __init__(self):
        self.Value = 0

class BattleShip:
    EXPLORATION_COST_IN_KM = 0
    def __init__(self):
        self.BattleFleet = None
        self.FleetEntity = None
        self.CanBePaused = False
        self.WorkersNeeded = 0
        from Mafi import Option
        self.CustomTitle = Option()
        self.OnLocationFullyExplored = None
        self.PreviousLocationId = None
        self.CurrentLocationId = None
        self.NextLocationId = None
        self.WorldPosition = None
        self.Path = None
        self.TravelProgress = None
        self.HasWorldMapPath = False
        self.Cargo = None
        self.RefugeesCount = 0
        self.IsAtHomeCell = False
        self.IsAutoReturnEnabled = False
        self.AssignedDock = Option()
        self.AssignedDockEntity = Option()
        self.CrewRequired = 0
        self.HasAllRequiredCrew = False
        self.CurrentCrew = 0
        self.MaxHp = 0
        self.CurrentHp = 0
        self.MissingHp = 0
        self.NeedsRepair = False
        self.MissingHpPercent = None
        self.HpPercent = None
        self.MinOperableHp = 0
        self.HasEnoughHpToOperate = False
        self.CanOperate = False
        self.ExplorationProgress = None
        self.FuelBuffer = None
        self.FuelQuantity = None
        self.InBattle = False
        self.IsExploring = False
        self.IsIdleInWorld = False
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
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
        self.HasWorkersCached = False
        self.DockedAt = Option()
        self.JobsContext = None
        self.PathabilityProvider = None
        self.Terrain = None
        self.SurfaceProvider = None
        self.LastDisruptedTile = None

class ExploreFinishCheatCmd:
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

class FleetLoadCrewCmd:
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

class FleetModificationsCancelCmd:
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

class FleetModificationsPrepareCmd:
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
        self.ModificationRequest = None

class FleetRepairCheatCmd:
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

class FleetToggleAutoReturnCmd:
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

class FleetUnloadCrewCmd:
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

class FleetUnloadFuelCmd:
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

class GoToLocationCmd:
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
        self.LocationId = None
        self.Reason = None

class TeleportFleetToLocationCheatCmd:
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
        self.LocationId = None

class TravelingFleet:
    def __init__(self):
        self.CanBePaused = False
        self.WorkersNeeded = 0
        from Mafi import Option
        self.CustomTitle = Option()
        self.PreviousLocationId = None
        self.CurrentLocationId = None
        self.NextLocationId = None
        self.WorldPosition = None
        self.RefugeesCount = 0
        self.IsAutoReturnEnabled = False
        self.Dock = None
        self.LocationState = None
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
        self.HasWorkersCached = False
        self.BattleFleet = None
        self.FleetEntity = None

class TravelingFleetManager:
    def __init__(self):
        self.HasFleet = False
        self.TravelingFleet = None
        self.FarthestLocationVisited = 0

    class LocationVisitCheckResult:
        Ok = None
        AlreadyHeadingThereOrPresent = None
        Damaged = None
        ShipIsBeingModified = None
        ShipIsBeingRepaired = None
        NotAccessible = None
        NotEnoughFuel = None
        NotEnoughCrew = None
        TooFar = None
        def __init__(self):
            self.value__ = 0

class WorldMap:
    def __init__(self):
        self.HomeLocation = None
        self.Locations = None
        self.LocationsDict = None
        self.LocationsCount = 0
        self.Connections = None
        from Mafi import Option
        self.Item = Option()
        self.Size = None

class WorldMapCargoManager:
    def __init__(self):
        pass


    class WorldCargoData:
        def __init__(self):
            self.Product = None
            self.Quantity = None
            self.Capacity = None

class WorldMapConnection:
    def __init__(self):
        self.Location1 = None
        self.Location2 = None

class WorldMapEntityCancelRepairCmd:
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
        self.EntityId = None

class WorldMapEntityStartRepairCmd:
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
        self.EntityId = None

class WorldMapEntityUpgradeCmd:
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
        self.EntityId = None

class WorldMapLocation:
    def __init__(self):
        self.Id = None
        self.Name = ""
        self.Position = None
        self.State = None
        from Mafi import Option
        self.Loot = Option()
        self.EntityProto = Option()
        self.Entity = Option()
        self.Enemy = Option()
        self.Graphics = Option()
        self.IsEnemyKnown = False
        self.IsScannedByRadar = False

class WorldMapLoot:
    def __init__(self):
        self.IsEmpty = False
        self.People = 0
        self.Products = None
        self.ProtosToUnlock = None
        self.IsTreasure = False

class WorldMapManager:
    def __init__(self):
        self.OnWorldEntityCreated = None
        self.Mines = None
        self.AllMinableProducts = None
        self.AllQuickTrades = None
        self.EntitiesUnderConstruction = None
        self.Map = None

class WorldMapSettlementAdoptPopsCmd:
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
        self.EntityId = None
        self.PopsCount = 0

class FleetLocationState:
    AtWorld = None
    ArrivingFromWorld = None
    Docked = None
    DepartingToWorld = None
    ExploreInProgress = None
    BattleInProgress = None
    ArrivingAtDock = None
    ArrivingFromWorldNotYetPathed = None
    def __init__(self):
        self.value__ = 0

class BattleShipProto:
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
        self.StartingHp = None
        self.MinOperableHp = None
        self.CargoAndRefugeesCapacity = 0
        self.InitialHullProto = None
        self.InitialEngine = None
        self.InitialBridge = None
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

    class Gfx:
        EMPTY = None
        def __init__(self):
            self.IconPath = ""
            self.ArrivalSoundPath = ""
            self.DepartureSoundPath = ""
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

class LocationVisitReason:
    General = None
    LoadCargo = None
    DeliverCargo = None
    def __init__(self):
        self.value__ = 0

class IWorldMapGenerator:
    def __init__(self):
        pass


class OnlyHomeLocationWorldMapGenerator:
    def __init__(self):
        pass


class LineWorldMapGenerator:
    def __init__(self):
        pass


class WorldMapLocationState:
    Hidden = None
    NotExplored = None
    Explored = None
    def __init__(self):
        self.value__ = 0

class IWorldMapManager:
    def __init__(self):
        self.Map = None
        self.OnWorldEntityCreated = None

class IWorldMapPathFinder:
    def __init__(self):
        pass


class WorldMapPathFinder:
    def __init__(self):
        self.SomePathAlreadyFound = False
        self.CurrentPfId = 0

    class Node:
        def __init__(self):
            from Mafi import Fix32
            self.CurrentCost = Fix32()
            self.ParentOnPath = None
            self.PathLength = 0
            self.IsVisitedFromStart = False
            self.IsVisited = False
            self.IsProcessed = False
            self.HasParent = False
            self.IsInitialized = False
            self.Location = None
