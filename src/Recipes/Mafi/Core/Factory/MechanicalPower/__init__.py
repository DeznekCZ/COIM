
class IShaft:
    def __init__(self):
        self.IsDefaultNoCapacityShaft = False
        self.InertiaBuffer = None
        self.TotalAvailablePower = None
        self.CurrentInertia = None
        self.CurrentInputScale = None
        self.CurrentOutputScale = None
        self.ThroughputUtilization = None
        self.OutputAllowed = False
        self.InputAllowed = False
        self.IsDestroyed = False
        self.ConnectedEntities = None

class Shaft:
    def __init__(self):
        self.IsDefaultNoCapacityShaft = False
        self.InertiaBuffer = None
        self.EntitiesCount = 0
        self.ConnectedEntities = None
        self.TotalAvailablePower = None
        self.OutputAllowed = False
        self.InputAllowed = False
        self.CurrentInertia = None
        self.CurrentInputScale = None
        self.CurrentOutputScale = None
        self.ThroughputUtilization = None
        self.IsDestroyed = False
        self.AvailableCapacity = None
        self.TotalStoreCapacity = None

class ShaftInertiaProtoParam:
    def __init__(self):
        self.AllowedProtoType = None
        self.Inertia = None

class IShaftManager:
    def __init__(self):
        self.MechPowerProto = None

class ShaftManager:
    MAX_SHAFT_THROUGHPUT = None
    def __init__(self):
        self.ProvidedProducts = None
        self.MechPowerProto = None
        self.ShaftsCount = 0
        self.DefaultZeroCapacityShaft = None
        self.StoredMechPower = None

class IShaftBuffer:
    def __init__(self):
        self.Shaft = None
        self.IsDestroyed = False
        self.Product = None
        self.UsableCapacity = None
        self.Capacity = None
        self.Quantity = None
