class CargoShip:
    SAVER_FUEL_MULT = None
    SAVER_TRAVEL_DURATION_MULT = None

    def __init__(self):
        self.CanBePaused = False
        from Mafi import Option
        self.CustomTitle = Option()
        self.RemainingTransitionDuration = None
        self.State = None
        self.LastDockedStatus = None
        self.NonEmptyModules = None
        self.Modules = None
        self.JourneyDuration = None
        self.DockTransitionDurationForCurrentJourney = None
        self.IsFuelReductionEnabled = False
        self.CanPayWithUnityIfOutOfFuel = False
        self.CargoDepot = None
        self.FuelProto = None
        self.FuelBuffer = None
        self.Position2f = None
        self.Position3f = None
        self.Direction = None
        self.IsDocked = False
        self.DepartureRequestedByPlayer = False
        self.GeneralPriority = int(0)
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
        self.WorkersNeeded = int(0)
        self.HasWorkersCached = False
        self.RendererData = None
class ForceLeaveMode:
    None = None
    LeftForUpgrade = None
    LeftForDestroy = None
    LeftForBlocked = None
    LeftForFuelTypeChange = None

    def __init__(self):
        pass

class ShipState:
    ArrivingFromWorld = None
    Docked = None
    DepartingToWorld = None
    AtWorldGoingForCargo = None
    AtWorldReturningHome = None

    def __init__(self):
        pass

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
        pass

class CargoShipFactory:

    def __init__(self):
        pass

class CargoShipProto:

    def __init__(self):
        self.EntityType = None
        self.IconPath = str(0)
        self.Costs = None
        self.Id = None
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
class FuelData:

    def __init__(self):
        pass

class Gfx:
    EMPTY = None

    def __init__(self):
        self.IconPath = str(0)
class CargoShipProtoBuilder:

    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None
class State:

    def __init__(self):
        pass

class ICargoShipFactory:

    def __init__(self):
        pass

