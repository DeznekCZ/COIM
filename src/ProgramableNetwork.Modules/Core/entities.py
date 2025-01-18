class EntityId:
    def __init__(self, value: int):
        self.Value = value

class EnoughClass:
    def __init__(self):
        self.HasEnough = True

class Entity:
    """Represents: Mafi.Core.Entities.Entity"""
    def __init__(self, entity_id: EntityId):
        self.IsPaused = False
        self.Id = entity_id
        self.CanBePaused = True
        self.WorkerConsumer = EnoughClass()
        self.PowerConsumer = EnoughClass()

    def SetPause(self, pause: bool):
        self.IsPaused = pause

class StaticEntity(Entity):
    """Represents: Mafi.Core.Entities.Static.StaticEntity"""
    pass