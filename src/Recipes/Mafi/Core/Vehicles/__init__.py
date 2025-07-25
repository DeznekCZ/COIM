
class AddVehicleReplacementTaskCmd:
    def __init__(self):
        self.AffectsSaveState = False
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.IsVerificationCmd = False
        self.Result = False
        self.HasError = False
        self.ErrorMessage = ""
        from Mafi.Core.Entities.Dynamic import DynamicEntityProto
        self.CurrentProtoId = DynamicEntityProto.ID()

        self.ReplacementProtoId = DynamicEntityProto.ID()

        self.ZoneId = None
        self.VehicleDepotId = None
        self.UnassignedOnly = False
        self.AssigneeId = None
        self.Limit = 0

class AutoBufferLogisticsHelper:
    def __init__(self):
        self.LogisticsInputMode = None
        self.LogisticsOutputMode = None

class BufferStrategy:
    Ignore = None
    def __init__(self):
        self.Priority = 0
        self.PriorityForRefueling = 0
        self.OptimalQuantity = None

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
        self.ErrorMessage = ""
        self.ZoneId = None
        self.Area = None

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
        self.ErrorMessage = ""

class EntityGeneralPriorityProvider:
    def __init__(self):
        pass


class JobStatistics:
    def __init__(self):
        self.Product = None
        self.Quantity = None
        self.JobsCount = 0

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
    COLOR_PALETTE = None
    DUMMY_ALL_ZONE = None
    DEFAULT_ZONE_MASK = None
    def __init__(self):
        self.Color = None
        self.IsDefaultZone = False
        self.Area = None
        self.CanConstructMask = None
        self.Name = None
        self.IsDestroyed = False
        self.ConstructionAllowedFrom = None
        self.Id = None
        self.ZoneIndex = 0
        self.Mask = None

class LogisticsZonesManager:
    DefaultZoneId = None
    PLAYER_ZONES_LIMIT = 0
    ZONES_LIMIT = 0
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
        self.VehiclesEnforcer = Option()
        self.AllowDeliveryAtDistanceWhenBlocked = False
        self.NumberOfVehiclesAssigned = 0
        self.PendingQuantity = None
        self.AllReservedJobs = None
        self.IsAvailableCached = False
        self.OptimalQuantityCached = None
        self.OptimalQuantityOrMaxCached = None
        self.RawPriorityCached = 0
        self.CombinedPriorityCached = 0
        self.IsFallbackOnly = False
        self.RemainingCapacityCached = None
        self.ZoneMask = None
        self.CanConstructMask = None
        self.Buffer = None

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
        self.VehiclesEnforcer = Option()
        self.AllowPickupAtDistanceWhenBlocked = False
        self.NumberOfVehiclesAssigned = 0
        self.PendingQuantity = None
        self.JobsCount = 0
        self.IsAvailableCached = False
        self.OptimalQuantityCached = None
        self.OptimalQuantityOrMaxCached = None
        self.RawPriorityCached = 0
        self.CombinedPriorityCached = 0
        self.AvailableQuantityCached = None
        self.UseFallbackIfNeeded = False
        self.ZoneMask = None
        self.CanConstructMask = None
        self.Buffer = None

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
        self.ErrorMessage = ""
        self.ZoneId = None

class RemoveVehicleReplacementTaskCmd:
    def __init__(self):
        self.AffectsSaveState = False
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.IsVerificationCmd = False
        self.Result = False
        self.HasError = False
        self.ErrorMessage = ""
        self.TaskId = None

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
        self.ErrorMessage = ""
        self.ZoneId = None
        self.Name = ""

class RobustNavHelper:
    DEFAULT_EXTRA_TOLERANCE_PER_RETRY = None
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
        self.Buffer = None
        self.Quantity = None

class SecondaryOutputBufferSpec:
    def __init__(self):
        self.Buffer = None
        self.Quantity = None

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
        self.ErrorMessage = ""
        self.ZoneId = None
        self.ColorIndex = 0

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
        self.ErrorMessage = ""
        self.ZoneId = None
        self.TruckId = None

class StaticPriorityProvider:
    Ignore = None
    LowestNoQuantityPreference = None
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
        self.ErrorMessage = ""
        self.ZoneId = None
        self.ConstructionZoneId = None

class VehicleBuffersRegistry:
    def __init__(self):
        self.AllowPartialTrucks = False
        self.NumberOfTrucksWaitingForJobs = 0

class VehicleCargo:
    MAX_MIXED_PRODUCT_COUNT = 0
    def __init__(self):
        self.IsEmpty = False
        self.IsNotEmpty = False
        self.TotalQuantity = None
        self.FirstOrPhantom = None
        self.Count = 0
        self.LifetimeLoadedQuantity = None

class VehicleJobs:
    def __init__(self):
        self.Count = 0

class VehicleJobStatsManager:
    def __init__(self):
        self.GeneralJobsStats = None
        self.MiningJobsStats = None
        self.RefuelingJobsStats = None

class VehicleRecoveryManager:
    def __init__(self):
        pass


class VehicleReplacementTask:
    def __init__(self):
        self.State = None
        self.VehiclesReplaced = 0
        self.TaskId = None
        self.CurrentProto = None
        self.ReplacementProto = None
        self.LogisticsZone = None
        from Mafi import Option
        self.VehicleDepot = Option()
        self.UnassignedOnly = False
        self.Assignee = Option()
        self.Limit = 0
        self.ActiveReplacements = None

    class TaskState:
        InProgress = None
        Finished = None
        Cancelled = None
        def __init__(self):
            self.value__ = 0

class VehiclesManager:
    VEHICLE_RECOVERY_COST = None
    def __init__(self):
        self.OnVehicleDespawned = None
        self.AllVehicles = None
        self.Trucks = None
        self.Excavators = None
        self.TreeHarvesters = None
        self.TreePlanters = None
        self.VehiclesLimitLeft = 0
        self.MaxVehiclesLimit = 0
        self.m_onVehicleDespawned = None

class VehiclesReplacer:
    def __init__(self):
        self.ActiveReplacementTasks = None
        self.FinishedReplacementTasks = None

class VehicleSurfaceProvider:
    SURFACE_REL_HEIGHT = None
    def __init__(self):
        self.EntityHeights = None
        self.OnVehicleSurfaceChanged = None

    class SurfaceHeights:
        def __init__(self):
            self.Height = None
            self.Count = None
            self.ConstructedCount = None

class OutputPriorityRequest:
    def __init__(self):
        self.Buffer = None
        self.PendingQuantity = None
        self.IsForRefuelling = False

class IInputBufferPriorityProvider:
    def __init__(self):
        pass


class IOutputBufferPriorityProvider:
    def __init__(self):
        pass


class ILogisticsConfig:
    def __init__(self):
        self.InitialVehiclesCap = 0

class IVehicleBuffersRegistry:
    def __init__(self):
        pass


class BalancingJobSpec:
    def __init__(self):
        self.Truck = None
        self.ProductQuantity = None
        from Mafi import Option
        self.InputBuffer = Option()
        self.DumpDesignation = Option()
        self.ExtraDumpDesignations = Option()
        self.SurfacePlaceDesignations = Option()
        self.SurfaceClearDesignations = Option()
        self.OutputBuffer = Option()
        self.SecondaryInputBuffers = Option()
        self.SecondaryOutputBuffers = Option()

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
        self.VehiclesLimitLeft = 0
        self.MaxVehiclesLimit = 0
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
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None

class LogisticsZoneFast:
    def __init__(self):
        self.IsEmpty = False
        self.Zone = None
        self.Mask = None
        self.CanConstructMask = None

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
    Navigating = None
    GoalReachedSuccessfully = None
    FailGoalUnreachable = None
    def __init__(self):
        self.value__ = 0

class RotatingCabinDriverProto:
    def __init__(self):
        self.MaxSpeedPerTick = None
        self.MaxAccelerationPerTick = None
        self.MaxBrakingPerTick = None
        from Mafi import Fix32
        self.BrakingConservativeness = Fix32()

class IVehicleCargo:
    def __init__(self):
        self.IsEmpty = False
        self.IsNotEmpty = False
        self.TotalQuantity = None
        self.FirstOrPhantom = None
        self.Count = 0
        self.LifetimeLoadedQuantity = None

class VehicleFuelConsumption:
    None = None
    Idle = None
    Full = None
    def __init__(self):
        self.value__ = 0

class VehicleQueueAssertions:
    def __init__(self):
        pass


class VehicleStats:
    def __init__(self):
        self.Owned = 0
        self.OwnedInZone = 0
        self.Assignable = 0

class IVehicleSurfaceProvider:
    def __init__(self):
        self.OnVehicleSurfaceChanged = None
