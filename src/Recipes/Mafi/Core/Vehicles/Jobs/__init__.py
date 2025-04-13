
class AttachRocketToLaunchPadJob:
    def __init__(self):
        self.JobInfo = None
        self.IsTrueJob = False
        self.CurrentFuelConsumption = None
        self.WaitBehindQueueTipVehicle = False
        self.SkipNoMovementMonitoring = False
        self.Id = None
        self.IsDestroyed = False
        self.IsBeingCancelled = False
        self.IsCancelled = False

    class Factory:
        def __init__(self):
            pass


class CargoDeliveryJob:
    def __init__(self):
        self.IsTrueJob = False
        self.CurrentFuelConsumption = None
        self.GoalName = None
        self.CargoToDeliver = None
        self.JobInfo = None
        self.Id = None
        self.IsDestroyed = False
        self.IsBeingCancelled = False
        self.IsCancelled = False
        self.SkipNoMovementMonitoring = False
        self.WaitBehindQueueTipVehicle = False

    class Factory:
        def __init__(self):
            self.QueueingJobFactory = None
            self.NavigateToJobFactory = None
            self.StaticEntityGoalFactory = None
            self.VehicleJobStatsManager = None

class CargoPickUpJob:
    def __init__(self):
        self.IsTrueJob = False
        self.CurrentFuelConsumption = None
        self.GoalName = None
        self.CargoToPickup = None
        self.JobInfo = None
        self.Id = None
        self.IsDestroyed = False
        self.IsBeingCancelled = False
        self.IsCancelled = False
        self.SkipNoMovementMonitoring = False
        self.WaitBehindQueueTipVehicle = False

    class Factory:
        def __init__(self):
            self.NavigateToJobFactory = None
            self.QueueingJobFactory = None
            self.AssetTransactionManager = None

class ChainedNavigationJob:
    def __init__(self):
        self.JobInfo = None
        self.IsTrueJob = False
        self.CurrentFuelConsumption = None
        self.Id = None
        self.IsDestroyed = False
        self.IsBeingCancelled = False
        self.IsCancelled = False
        self.SkipNoMovementMonitoring = False

    class Factory:
        def __init__(self):
            pass


class CleanExcavatorJob:
    def __init__(self):
        self.JobInfo = None
        self.IsTrueJob = False
        self.CurrentFuelConsumption = None
        self.Id = None
        self.IsDestroyed = False
        self.IsBeingCancelled = False
        self.IsCancelled = False
        self.SkipNoMovementMonitoring = False

    class Factory:
        def __init__(self):
            pass


class DumpingJob:
    def __init__(self):
        self.JobInfo = None
        self.IsTrueJob = False
        self.CurrentFuelConsumption = None
        self.GoalName = None
        self.PrimaryDesignation = None
        self.ExtraDesignations = None
        self.Id = None
        self.IsDestroyed = False
        self.IsBeingCancelled = False
        self.IsCancelled = False
        self.SkipNoMovementMonitoring = False

    class Factory:
        def __init__(self):
            self.DumpingManager = None
            self.PathFindingManager = None
            self.TerrainManager = None
            self.DesignationGoalFactory = None
            self.TerrainDumpingManager = None
            self.UnreachableDesignationsManager = None
            self.VehicleJobStatsManager = None
            self.VehicleLastOutputBufferManager = None

class EmptyJob:
    def __init__(self):
        self.JobInfo = None
        self.IsTrueJob = False
        self.CurrentFuelConsumption = None
        self.SkipNoMovementMonitoring = False
        self.Id = None
        self.IsDestroyed = False
        self.IsBeingCancelled = False
        self.IsCancelled = False

class GetUnstuckJob:
    def __init__(self):
        self.JobInfo = None
        self.IsTrueJob = False
        self.CurrentFuelConsumption = None
        self.Id = None
        self.IsDestroyed = False
        self.IsBeingCancelled = False
        self.IsCancelled = False
        self.SkipNoMovementMonitoring = False

    class Factory:
        def __init__(self):
            pass


class MiningJob:
    def __init__(self):
        self.JobInfo = None
        self.IsTrueJob = False
        self.CurrentFuelConsumption = None
        self.TileToMine = None
        self.Id = None
        self.IsDestroyed = False
        self.IsBeingCancelled = False
        self.IsCancelled = False
        self.SkipNoMovementMonitoring = False

    class Factory:
        def __init__(self):
            self.MiningManager = None
            self.PathFindingManager = None
            self.FuelStationsManager = None
            self.TerrainManager = None
            self.TreesManager = None
            self.TerrainPropsManager = None
            self.DesignationGoalFactory = None
            self.UnreachableDesignationsManager = None

class MixedCargoDeliveryJob:
    def __init__(self):
        self.IsTrueJob = False
        self.CurrentFuelConsumption = None
        self.JobInfo = None
        self.Truck = None
        self.Id = None
        self.IsDestroyed = False
        self.IsBeingCancelled = False
        self.IsCancelled = False
        self.SkipNoMovementMonitoring = False
        self.WaitBehindQueueTipVehicle = False

    class Factory:
        def __init__(self):
            self.QueueingJobFactory = None
            self.NavigateToJobFactory = None
            self.StaticEntityGoalFactory = None
            self.VehicleJobStatsManager = None

class NavigateToJob:
    def __init__(self):
        self.JobInfo = None
        self.IsTrueJob = False
        self.CurrentFuelConsumption = None
        self.GoalName = None
        self.Id = None
        self.IsDestroyed = False
        self.IsBeingCancelled = False
        self.IsCancelled = False
        self.SkipNoMovementMonitoring = False

    class Factory:
        def __init__(self):
            self.PathFindingManager = None

class QueueJobResultRef:
    def __init__(self):
        self.ReachedGoal = None

class RecoverVehicleJob:
    def __init__(self):
        self.JobInfo = None
        self.IsTrueJob = False
        self.CurrentFuelConsumption = None
        self.SkipNoMovementMonitoring = False
        self.Id = None
        self.IsDestroyed = False
        self.IsBeingCancelled = False
        self.IsCancelled = False

    class Factory:
        def __init__(self):
            pass


class RefuelOtherVehicleJob:
    def __init__(self):
        self.JobInfo = None
        self.IsTrueJob = False
        self.CurrentFuelConsumption = None
        self.VehicleToRefuel = None
        self.Id = None
        self.IsDestroyed = False
        self.IsBeingCancelled = False
        self.IsCancelled = False
        self.SkipNoMovementMonitoring = False

    class Factory:
        def __init__(self):
            pass


class RefuelSelfJob:
    def __init__(self):
        self.JobInfo = None
        self.IsTrueJob = False
        self.CurrentFuelConsumption = None
        self.CargoToPickup = None
        self.Vehicle = None
        self.Id = None
        self.IsDestroyed = False
        self.IsBeingCancelled = False
        self.IsCancelled = False
        self.SkipNoMovementMonitoring = False
        self.WaitBehindQueueTipVehicle = False

    class Factory:
        def __init__(self):
            self.QueueingJobFactory = None
            self.NavigateToJobFactory = None
            self.FuelStatsCollector = None
            self.StaticEntityGoalFactory = None

class RocketAssemblyAttachJob:
    def __init__(self):
        self.JobInfo = None
        self.IsTrueJob = False
        self.SkipNoMovementMonitoring = False
        self.CurrentFuelConsumption = None
        self.Id = None
        self.IsDestroyed = False
        self.IsBeingCancelled = False
        self.IsCancelled = False

    class Factory:
        def __init__(self):
            pass


class ScrapOrReplaceVehicleInDepotJob:
    def __init__(self):
        self.JobInfo = None
        self.IsTrueJob = False
        self.CurrentFuelConsumption = None
        self.SkipNoMovementMonitoring = False
        self.Id = None
        self.IsDestroyed = False
        self.IsBeingCancelled = False
        self.IsCancelled = False
        self.WaitBehindQueueTipVehicle = False

    class Factory:
        def __init__(self):
            self.VehicleQueueJobFactory = None
            self.VehiclesManager = None
            self.ConstructionManager = None

class SpawnJob:
    def __init__(self):
        self.JobInfo = None
        self.IsTrueJob = False
        self.CurrentFuelConsumption = None
        self.SkipNoMovementMonitoring = False
        self.Id = None
        self.IsDestroyed = False
        self.IsBeingCancelled = False
        self.IsCancelled = False

    class Factory:
        def __init__(self):
            pass


class SurfaceModificationJob:
    def __init__(self):
        self.JobInfo = None
        self.Vehicle = None
        self.IsTrueJob = False
        self.CurrentFuelConsumption = None
        self.GoalName = None
        self.Id = None
        self.IsDestroyed = False
        self.IsBeingCancelled = False
        self.IsCancelled = False
        self.SkipNoMovementMonitoring = False

    class Factory:
        def __init__(self):
            self.PathFindingManager = None
            self.TerrainManager = None
            self.DesignationGoalFactory = None
            self.SurfaceDesignationsManager = None
            self.UnreachableDesignationsManager = None
            self.VehicleJobStatsManager = None

class TreeHarvesterLoadTruckJob:
    def __init__(self):
        self.JobInfo = None
        self.SkipNoMovementMonitoring = False
        self.IsTrueJob = False
        self.CurrentFuelConsumption = None
        self.Id = None
        self.IsDestroyed = False
        self.IsBeingCancelled = False
        self.IsCancelled = False

    class Factory:
        def __init__(self):
            pass


class TreeHarvestingJob:
    def __init__(self):
        self.JobInfo = None
        self.IsTrueJob = False
        self.CurrentFuelConsumption = None
        self.Id = None
        self.IsDestroyed = False
        self.IsBeingCancelled = False
        self.IsCancelled = False
        self.SkipNoMovementMonitoring = False

    class Factory:
        def __init__(self):
            self.TreeGoalFactory = None
            self.HarvestingManager = None
            self.PathFindingManager = None
            self.UnreachablesManager = None

class TreePlantingJob:
    def __init__(self):
        self.JobInfo = None
        self.IsTrueJob = False
        self.CurrentFuelConsumption = None
        self.PlantingPosition = None
        self.PlantingProto = None
        self.Id = None
        self.IsDestroyed = False
        self.IsBeingCancelled = False
        self.IsCancelled = False
        self.SkipNoMovementMonitoring = False

    class Factory:
        def __init__(self):
            self.TreeGoalFactory = None
            self.PathFindingManager = None
            self.UnreachablesManager = None

class VehicleJob:
    def __init__(self):
        self.Id = None
        self.JobInfo = None
        self.IsDestroyed = False
        self.IsTrueJob = False
        self.CurrentFuelConsumption = None
        self.IsBeingCancelled = False
        self.IsCancelled = False
        self.SkipNoMovementMonitoring = False

class VehicleJobsSequence:
    def __init__(self):
        self.IsEmpty = False
        self.IsNotEmpty = False
        from Mafi import Option
        self.CurrentJob = Option()
        self.CurrentFuelConsumption = None
        self.ContainsTrueJob = False
        self.Count = 0
        self.Item = None

class WaitingJob:
    def __init__(self):
        self.JobInfo = None
        self.IsTrueJob = False
        self.CurrentFuelConsumption = None
        self.SkipNoMovementMonitoring = False
        self.Id = None
        self.IsDestroyed = False
        self.IsBeingCancelled = False
        self.IsCancelled = False

    class Factory:
        def __init__(self):
            pass


class IJobWithPreNavigation:
    def __init__(self):
        self.GoalName = None

class ICargoDeliveryJob:
    def __init__(self):
        self.CargoToDeliver = None
        self.IsDestroyed = False
        self.SkipNoMovementMonitoring = False
        self.CurrentFuelConsumption = None
        self.Id = None
        self.JobInfo = None

class ICargoPickUpJob:
    def __init__(self):
        self.CargoToPickup = None
        self.IsDestroyed = False
        self.SkipNoMovementMonitoring = False
        self.CurrentFuelConsumption = None
        self.Id = None
        self.JobInfo = None

class IQueueTipJob:
    def __init__(self):
        self.WaitBehindQueueTipVehicle = False
        self.IsDestroyed = False
        self.SkipNoMovementMonitoring = False
        self.CurrentFuelConsumption = None
        self.Id = None
        self.JobInfo = None

class IVehicleJobReadOnly:
    def __init__(self):
        self.Id = None
        self.JobInfo = None

class IVehicleJob:
    def __init__(self):
        self.IsDestroyed = False
        self.SkipNoMovementMonitoring = False
        self.CurrentFuelConsumption = None
        self.Id = None
        self.JobInfo = None

class IVehicleJobWithOwner:
    def __init__(self):
        self.Vehicle = None
        self.IsDestroyed = False
        self.SkipNoMovementMonitoring = False
        self.CurrentFuelConsumption = None
        self.Id = None
        self.JobInfo = None

class IVehicleJobObserver:
    def __init__(self):
        pass


class JobResult:
    Empty = None
    def __init__(self):
        from Mafi import Option
        self.NextJob = Option()
        self.Cargo = None

class ParkAndWaitJobFactory:
    def __init__(self):
        pass


class ReturnHomeJob:
    def __init__(self):
        pass


    class Factory:
        def __init__(self):
            pass


class VehicleQueueJobFactory:
    def __init__(self):
        self.PathFindingManager = None
        self.StaticEntityGoalFactory = None
        self.DynamicEntityGoalFactory = None
