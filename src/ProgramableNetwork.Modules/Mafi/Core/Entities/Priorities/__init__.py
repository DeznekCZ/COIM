class GeneralPriorities:
    HIGHEST_PRIORITY = None
    LOWEST_ACTIONABLE_PRIORITY = None
    LOWEST_PRIORITY = None
    IGNORE = None
    DEFAULT = None
    HIGH = None
    VERY_HIGH = None
    SUPER_HIGH = None
    LOAN_PAYMENTS = None
    CONSTRUCTION_DECONSTRUCTION = None
    CONSTRUCTION_DECONSTRUCTION_PRIORITIZED = None
    CLEARING = None
    CARGO_DEPOT_MODULE_CARGO = None
    RUINS_EXPORT = None
    SHIPYARD_REPAIR_IMPORT_LOW = None
    SHIPYARD_DEFAULT_CARGO_EXPORT = None
    TRADE_DOCK_DEFAULT_CARGO_EXPORT = None
    VEHICLE_DEPOT_EXPORT = None
    STORAGE_CARGO_INCREASED = None
    VEHICLES = None
    SHIP = None
    CARGO_SHIPS = None
    TRANSPORTS_ZIPPERS = None
    STORAGE = None
    FARM = None
    POWER = None
    WORLD_MINES = None
    SETTLEMENT_MODULE = None
    VEHICLE_DEPOT = None

    def __init__(self):
        pass

class GlobalPrioritiesManager:
    CONSTRUCTION_PRIORITY_ID = None
    DECONSTRUCTION_PRIORITY_ID = None

    def __init__(self):
        self.ConstructionPriority = int(0)
        self.DeconstructionPriority = int(0)
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
        self.ErrorMessage = str(0)
class IEntityWithCustomPriority:

    def __init__(self):
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
class IEntityWithGeneralPriority:

    def __init__(self):
        self.GeneralPriority = int(0)
        self.IsGeneralPriorityVisible = False
        self.IsCargoAffectedByGeneralPriority = False
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
class EntityWithGeneralPriorityExtensions:

    def __init__(self):
        pass

class PriorityListsExtensions:

    def __init__(self):
        pass

