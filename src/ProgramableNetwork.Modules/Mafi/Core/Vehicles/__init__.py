class AutoBufferLogisticsHelper:

    def __init__(self):
        self.LogisticsInputMode = None
        self.LogisticsOutputMode = None
class BufferStrategy:

    def __init__(self):
        pass

class EntityGeneralPriorityProvider:

    def __init__(self):
        pass

class JobStatistics:

    def __init__(self):
        pass

class KeepEmptyGeneralPriorityProvider:

    def __init__(self):
        pass

class KeepEmptyPriorityProvider:

    def __init__(self):
        pass

class KeepFullEntityPriorityProvider:

    def __init__(self):
        pass

class RegisteredInputBuffer:

    def __init__(self):
        self.StrategySlow = None
        self.RemainingCapacity = None
        self.Product = None
        self.IsEnabled = False
        self.IgnoreAssignedEntities = False
        self.Entity = None
        self.Position2f = None
        self.IsConstructionBuffer = False
        from Mafi import Option
        self.EntityAsAssignee = Option()
        self.HasAssignedOutputEntities = False
        from Mafi import Option
        self.VehiclesEnforcer = Option()
        self.AllowDeliveryAtDistanceWhenBlocked = False
        self.NumberOfVehiclesAssigned = int(0)
        self.AllReservedJobs = None
class RegisteredOutputBuffer:

    def __init__(self):
        self.StrategySlow = None
        self.AvailableQuantity = None
        self.AvailableQuantityForRefuel = None
        self.Product = None
        self.IsEnabled = False
        self.IgnoreAssignedEntities = False
        self.Entity = None
        self.Position2f = None
        self.IsConstructionBuffer = False
        from Mafi import Option
        self.EntityAsAssignee = Option()
        self.HasAssignedInputEntities = False
        from Mafi import Option
        self.VehiclesEnforcer = Option()
        self.AllowPickupAtDistanceWhenBlocked = False
        self.NumberOfVehiclesAssigned = int(0)
        self.JobsCount = int(0)
class RobustNavHelper:

    def __init__(self):
        from Mafi import Option
        self.TaskToInject = Option()
        self.IsNavigating = False
class RotatingCabinDriver:

    def __init__(self):
        self.CabinDirection = None
        self.CabinDirectionRelative = None
        self.IsCabinAtTarget = False
        self.CabinTarget = None
class SecondaryInputBufferSpec:

    def __init__(self):
        pass

class SecondaryOutputBufferSpec:

    def __init__(self):
        pass

class StaticPriorityProvider:

    def __init__(self):
        pass

class VehicleBuffersRegistry:

    def __init__(self):
        self.AllowPartialTrucks = False
        self.NumberOfTrucksWaitingForJobs = int(0)
class VehicleCargo:

    def __init__(self):
        self.IsEmpty = False
        self.IsNotEmpty = False
        self.TotalQuantity = None
        self.FirstOrPhantom = None
        self.Count = int(0)
class VehicleJobs:

    def __init__(self):
        self.Count = int(0)
class VehicleJobStatsManager:

    def __init__(self):
        self.GeneralJobsStats = None
        self.MiningJobsStats = None
        self.RefuelingJobsStats = None
class VehicleRecoveryManager:

    def __init__(self):
        pass

class VehiclesManager:

    def __init__(self):
        self.OnVehicleDespawned = None
        self.AllVehicles = None
        self.Trucks = None
        self.Excavators = None
        self.TreeHarvesters = None
        self.TreePlanters = None
        self.VehiclesLimitLeft = int(0)
        self.MaxVehiclesLimit = int(0)
class VehicleSurfaceProvider:

    def __init__(self):
        self.EntityHeights = None
        self.OnVehicleSurfaceChanged = None
class SurfaceHeights:

    def __init__(self):
        pass

class OutputPriorityRequest:

    def __init__(self):
        pass

class IInputBufferPriorityProvider:

    def __init__(self):
        pass

class IOutputBufferPriorityProvider:

    def __init__(self):
        pass

class DefaultVehicleFactory:

    def __init__(self):
        pass

class ILogisticsConfig:

    def __init__(self):
        self.InitialVehiclesCap = int(0)
class IVehicleBuffersRegistry:

    def __init__(self):
        pass

class BalancingJobSpec:

    def __init__(self):
        pass

class VehicleBuffersRegistryExtensions:

    def __init__(self):
        pass

class IVehicleForCargoJob:

    def __init__(self):
        self.RemainingCapacity = None
        self.CargoPickupDuration = None
        self.Cargo = None
        self.IsDriving = False
class IVehiclesManager:

    def __init__(self):
        self.VehiclesLimitLeft = int(0)
        self.MaxVehiclesLimit = int(0)
        self.OnVehicleDespawned = None
        self.AllVehicles = None
        self.Trucks = None
        self.Excavators = None
        self.TreeHarvesters = None
        self.TreePlanters = None
class IVehiclesManagerExtensions:

    def __init__(self):
        pass

class IRegisteredBuffer:

    def __init__(self):
        self.Entity = None
class RobustNavResult:

    def __init__(self):
        pass

class RotatingCabinDriverProto:

    def __init__(self):
        pass

class IVehicleCargo:

    def __init__(self):
        self.IsEmpty = False
        self.IsNotEmpty = False
        self.TotalQuantity = None
        self.FirstOrPhantom = None
        self.Count = int(0)
class VehicleFuelConsumption:

    def __init__(self):
        pass

class VehicleQueueAssertions:

    def __init__(self):
        pass

class VehicleStats:

    def __init__(self):
        pass

class IVehicleSurfaceProvider:

    def __init__(self):
        self.OnVehicleSurfaceChanged = None
