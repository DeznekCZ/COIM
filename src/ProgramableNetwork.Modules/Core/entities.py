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

class Entity:
    """Represents: Mafi.Core.Entities.Entity"""
    def __init__(self, entity_id: EntityId):
        self.IsPaused = False
        self.Id = entity_id
        self.CanBePaused = True

    def SetPause(self, pause: bool):
        self.IsPaused = pause

class StaticEntity(Entity):
    """Represents: Mafi.Core.Entities.Static.StaticEntity"""
    pass

class StorageBase(StaticEntity): pass
class Controller(StaticEntity): pass
class Antena(StaticEntity): pass
class SettlementHousingModule(StaticEntity): pass
class SettlementFoodModule(StaticEntity): pass
class SettlementTransformer(StaticEntity): pass
class SettlementWasteModule(StaticEntity): pass
class SettlementServiceModule(StaticEntity): pass
class Machine(StaticEntity): pass
class IEntityWithWorkers(Entity): pass
class IElectricityConsumingEntity(Entity): pass
class IUnityConsumingEntity(Entity): pass