class WorldMapLocId:

    def __init__(self):
        pass

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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
class TravelingFleet:

    def __init__(self):
        self.CanBePaused = False
        self.WorkersNeeded = int(0)
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
        self.RefugeesCount = int(0)
        self.IsAtHomeCell = False
        self.IsAutoReturnEnabled = False
        self.IsDocked = False
        self.Dock = None
        from Mafi import Option
        self.PendingDockAssignment = Option()
        self.Direction = None
        self.CrewRequired = int(0)
        self.HasAllRequiredCrew = False
        self.CurrentCrew = int(0)
        self.MaxHp = int(0)
        self.CurrentHp = int(0)
        self.MissingHp = int(0)
        self.NeedsRepair = False
        self.MissingHpPercent = None
        self.HpPercent = None
        self.MinOperableHp = int(0)
        self.HasEnoughHpToOperate = False
        self.CanOperate = False
        self.ExplorationProgress = None
        self.FuelBuffer = None
        self.FuelQuantity = None
        self.IsIdle = False
        self.LocationState = None
        self.RemainingTransitionDuration = None
        self.GeneralPriority = int(0)
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        from Mafi import Option
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
class TravelingFleetManager:

    def __init__(self):
        self.HasFleet = False
        self.TravelingFleet = None
        self.FarthestLocationVisited = int(0)
class LocationVisitCheckResult:

    def __init__(self):
        pass

class WorldMap:

    def __init__(self):
        self.HomeLocation = None
        self.Locations = None
        self.LocationsDict = None
        self.LocationsCount = int(0)
        self.Connections = None
        from Mafi import Option
        self.Item = Option()
class WorldMapCargoManager:

    def __init__(self):
        pass

class WorldCargoData:

    def __init__(self):
        pass

class WorldMapConnection:

    def __init__(self):
        pass

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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
class WorldMapLocation:

    def __init__(self):
        self.Id = None
        self.Name = str(0)
        self.Position = None
        self.State = None
        from Mafi import Option
        self.Loot = Option()
        from Mafi import Option
        self.EntityProto = Option()
        from Mafi import Option
        self.Entity = Option()
        from Mafi import Option
        self.Enemy = Option()
        from Mafi import Option
        self.Graphics = Option()
        self.IsEnemyKnown = False
        self.IsScannedByRadar = False
class WorldMapLoot:

    def __init__(self):
        self.IsEmpty = False
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
        self.ErrorMessage = str(0)
class LocationVisitReason:

    def __init__(self):
        pass

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

    def __init__(self):
        pass

class TravelingFleetProto:

    def __init__(self):
        self.EntityType = None
        self.Costs = None
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class Gfx:

    def __init__(self):
        pass

class WorldMapLocationState:

    def __init__(self):
        pass

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
class Node:

    def __init__(self):
        from Mafi import Fix32
        self.CurrentCost = Fix32()
        self.ParentOnPath = None
        self.PathLength = int(0)
        self.IsVisitedFromStart = False
        self.IsVisited = False
        self.IsProcessed = False
        self.HasParent = False
        self.IsInitialized = False
