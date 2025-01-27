class ProtoID:
    def __init__(self):
        self.Value = ""

class EntityId:
    def __init__(self, value: int):
        self.Value = value

class IEntityWithWorkers:
    def __init__(self):
        self.WorkersNeeded = 0
        self.HasWorkersCached = False

class Electricity:
    def __init__(self, value):
        self.Value = value or 0;
        
Electricity.Zero = Electricity(0)

class IElectricityConsumerReadonly:
    def __init__(self):
        self.NotEnoughPower = False;

class IElectricityConsumingEntity:
    def __init__(self):
        self.ElectricityConsumer = IElectricityConsumerReadonly()
        self.HasWorkersCached = False

class Upoints:
    def __init__(self, value):
        self.Value = value or 0;

class UnityConsumer:
    def __init__(self):
        self.HasEnoughUnity = False;

class IUnityConsumingEntity:
    def __init__(self):
        self.UnityConsumer = UnityConsumer()
        self.MonthlyUnityConsumed    = Upoints()
        self.MaxMonthlyUnityConsumed = Upoints()
        self.UpointsCategoryId       = ProtoID()

class StaticEntity: pass
class StorageBase: pass
class Controller: pass
class Antena: pass
class SettlementHousingModule: pass
class SettlementFoodModule: pass
class SettlementTransformer: pass
class SettlementWasteModule: pass
class SettlementServiceModule: pass
class Machine: pass
class IEntityWithWorkers: pass
class IElectricityConsumingEntity: pass
class IUnityConsumingEntity: pass

class Entity(IElectricityConsumingEntity, IUnityConsumingEntity):
    """Represents: Mafi.Core.Entities.Entity"""
    def __init__(self, entity_id: EntityId):
        self.IsPaused = False
        self.Id = entity_id
        self.CanBePaused = True
        self.UnityConsumer = UnityConsumer()

    def SetPause(self, pause: bool):
        self.IsPaused = pause

class StaticEntity(Entity):
    """Represents: Mafi.Core.Entities.Static.StaticEntity"""
    pass