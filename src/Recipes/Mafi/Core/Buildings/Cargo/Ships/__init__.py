
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
        self.RendererData = None
        self.PendingFuelToChangeTo = Option()
        self.OnModuleAdded = None
        self.OnModuleRemoved = None

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
        self.IconPath = ""
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
        self.MaximumModulesCount = 0
        self.AvailableModules = None
        self.AvailableFuels = None
        self.DockTransitionDuration = None
        self.DockOffset = None
        self.Graphics = None
        self.IsPhantom = False
        self.IsInitialized = False

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
            self.EngineSoundPath = ""
            self.ArrivalSoundPath = ""
            self.DepartureSoundPath = ""
            self.BasicBoxColliderSize = None
            self.ModuleSlotLength = None
            self.Color = None
            self.RendererIndex = 0

class CargoShipProtoBuilder:
    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None

    class State:
        def __init__(self):
            self.Builder = None

class ICargoShipFactory:
    def __init__(self):
        pass

