
class WorldMapLocId:
    def __init__(self):
        self.Value = 0

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

class TravelingFleet:
    EXPLORATION_COST_IN_KM = 0
    def __init__(self):
        self.CanBePaused = False
        self.WorkersNeeded = 0
        self.OnLocationFullyExplored = None
        self.PreviousLocationId = None
        self.CurrentLocationId = None
        self.NextLocationId = None
        self.WorldPosition = None
        self.DockedPosition = None
        self.Position3f = None
        self.Path = None
        self.TravelProgress = None
        self.IsMoving = False
        self.Cargo = None
        self.RefugeesCount = 0
        self.IsAtHomeCell = False
        self.IsAutoReturnEnabled = False
        self.IsDocked = False
        self.Dock = None
        from Mafi import Option
        self.PendingDockAssignment = Option()
        self.Direction = None
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
        self.IsIdle = False
        self.LocationState = None
        self.RemainingTransitionDuration = None
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.CustomTitle = Option()
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
        Docking = None
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


class FleetLocationState:
    AtWorld = None
    ArrivingFromWorld = None
    Docked = None
    DepartingToWorld = None
    ExploreInProgress = None
    BattleInProgress = None
    def __init__(self):
        self.value__ = 0

class TravelingFleetProto:
    def __init__(self):
        self.EntityType = None
        self.Costs = None
        from Mafi.Core.Entities import EntityProto
        self.Id = EntityProto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.StartingHp = None
        self.MinOperableHp = None
        self.ShipyardOffset = None
        self.CargoAndRefugeesCapacity = 0
        self.DockTransitionDuration = None
        self.InitialHullProto = None
        self.InitialEngine = None
        self.InitialBridge = None
        self.Graphics = None
        self.IsPhantom = False
        self.IsInitialized = False

    class Gfx:
        EMPTY = None
        def __init__(self):
            self.EngineSoundPath = ""
            self.ArrivalSoundPath = ""
            self.DepartureSoundPath = ""
            self.Color = None
            self.RendererIndex = 0

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
