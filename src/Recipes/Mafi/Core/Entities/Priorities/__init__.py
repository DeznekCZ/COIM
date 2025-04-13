
class GeneralPriorities:
    HIGHEST_PRIORITY = 0
    LOWEST_ACTIONABLE_PRIORITY = 0
    LOWEST_PRIORITY = 0
    IGNORE = 0
    DEFAULT = 0
    HIGH = 0
    VERY_HIGH = 0
    SUPER_HIGH = 0
    LOAN_PAYMENTS = 0
    CONSTRUCTION_DECONSTRUCTION = 0
    CONSTRUCTION_DECONSTRUCTION_PRIORITIZED = 0
    CLEARING = 0
    CARGO_DEPOT_MODULE_CARGO = 0
    RUINS_EXPORT = 0
    SHIPYARD_REPAIR_IMPORT_LOW = 0
    SHIPYARD_DEFAULT_CARGO_EXPORT = 0
    TRADE_DOCK_DEFAULT_CARGO_EXPORT = 0
    VEHICLE_DEPOT_EXPORT = 0
    STORAGE_CARGO_INCREASED = 0
    VEHICLES = 0
    SHIP = 0
    CARGO_SHIPS = 0
    TRANSPORTS_ZIPPERS = 0
    STORAGE = 0
    FARM = 0
    POWER = 0
    WORLD_MINES = 0
    SETTLEMENT_MODULE = 0
    VEHICLE_DEPOT = 0
    def __init__(self):
        pass


class GlobalPrioritiesManager:
    CONSTRUCTION_PRIORITY_ID = ""
    DECONSTRUCTION_PRIORITY_ID = ""
    def __init__(self):
        self.ConstructionPriority = 0
        self.DeconstructionPriority = 0

class SetGlobalPriorityCmd:
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
        self.PriorityId = ""
        self.Priority = 0

class IEntityWithCustomPriority:
    def __init__(self):
        self.DefaultTitle = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False

class IEntityWithGeneralPriority:
    def __init__(self):
        self.GeneralPriority = 0
        self.IsGeneralPriorityVisible = False
        self.IsCargoAffectedByGeneralPriority = False
        self.DefaultTitle = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False

class EntityWithGeneralPriorityExtensions:
    def __init__(self):
        pass


class PriorityListsExtensions:
    def __init__(self):
        pass

