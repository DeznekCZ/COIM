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
        self.EntitiesCount = int(0)
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
class IShaftManager:

    def __init__(self):
        self.MechPowerProto = None
class ShaftManager:

    def __init__(self):
        self.ProvidedProducts = None
        self.MechPowerProto = None
        self.ShaftsCount = int(0)
        self.DefaultZeroCapacityShaft = None
class IShaftBuffer:

    def __init__(self):
        self.Shaft = None
        self.IsDestroyed = False
        self.Product = None
        self.UsableCapacity = None
        self.Capacity = None
        self.Quantity = None
