
class DefaultTruckJobProvider:
    def __init__(self):
        pass


class FuelStationTruckJobProvider:
    def __init__(self):
        pass


class MineTowerTruckJobProvider:
    def __init__(self):
        pass


class TreeHarvesterTruckJobProvider:
    def __init__(self):
        pass


class TruckJobProviderBase:
    def __init__(self):
        pass


class TruckJobProviderContext:
    def __init__(self):
        self.VehicleJobStatsManager = None
        self.VehiclesManager = None
        self.TerrainDesignationManager = None
        self.TerrainDumpingManager = None
        self.FuelStationsManager = None
        self.TreeManager = None
        self.VehicleLastOutputBufferManager = None
        self.UnreachablesManager = None
        self.OreSortingPlantsManager = None
        self.PickUpJobFactory = None
        self.DeliveryJobFactory = None
        self.DumpJobFactory = None
        self.SurfaceJobFactory = None
        self.ChainedNavJobFactory = None
        self.NavigateToJobFactory = None
        self.WaitingJobFactory = None
        self.ParkAndWaitJobFactory = None
        self.VehicleBuffersRegistry = None
        self.TreeHarvestingJobFactory = None
        self.StaticEntityGoalFactory = None

class IFuelStationTruckJobProviderFactory:
    def __init__(self):
        pass


class FuelStationTruckJobProviderFactory:
    def __init__(self):
        pass


class MineTowerTruckJobProviderFactory:
    def __init__(self):
        pass


class IMineTowerTruckJobProviderFactory:
    def __init__(self):
        pass


class TreeHarvesterTruckJobProviderFactory:
    def __init__(self):
        pass

