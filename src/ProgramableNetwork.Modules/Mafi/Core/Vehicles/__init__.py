class AutoBufferLogisticsHelper:

    def __init__(self):
        self.LogisticsInputMode = None
        self.LogisticsOutputMode = None
class BufferStrategy:

    def __init__(self):
        pass

class ChangeLogisticsZoneAreaCmd:

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
class CreateNewLogisticsZoneCmd:

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

class LogisticsZone:

    def __init__(self):
        self.Color = None
        self.IsDefaultZone = False
        self.Area = None
        self.CanConstructMask = None
        self.Name = None
        self.IsDestroyed = False
        self.ConstructionAllowedFrom = None
class LogisticsZonesManager:

    def __init__(self):
        self.DefaultZone = None
        self.OnZoneAdded = None
        self.OnZoneRemoved = None
        self.OnZoneAreaChanged = None
        self.OnZoneConstructionChanged = None
        self.OnZoneColorChanged = None
        self.PlayerZonesFast = None
        self.AllZones = None
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
        self.PendingQuantity = None
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
        self.PendingQuantity = None
        self.JobsCount = int(0)
class RemoveLogisticsZoneCmd:

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
class RenameLogisticsZoneCmd:

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

class SetLogisticsZoneColorCmd:

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
class SetVehicleLogisticsZoneCmd:

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
class StaticPriorityProvider:

    def __init__(self):
        pass

class ToggleLogisticsZoneConstructionCmd:

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
        self.LifetimeLoadedQuantity = None
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

class IZoneMaskObserver:

    def __init__(self):
        self.ZoneMask = None
        self.Prototype = None
        self.CenterTile = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.Value = None
        self.ConstructionCost = None
        self.ConstructionState = None
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AlwaysUseCustomPfTargetTiles = False
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.DefaultTitle = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
class LogisticsZoneFast:

    def __init__(self):
        self.IsEmpty = False
class ILogisticsZonesManager:

    def __init__(self):
        self.DefaultZone = None
        self.AllZones = None
        self.PlayerZonesFast = None
        self.OnZoneAdded = None
        self.OnZoneRemoved = None
        self.OnZoneAreaChanged = None
        self.OnZoneConstructionChanged = None
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
        self.LifetimeLoadedQuantity = None
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
