
class IMod:
    def __init__(self):
        self.Name = ""
        self.Version = 0
        self.IsUiOnly = False
        from Mafi import Option
        self.ModConfig = Option()

class IModConfig:
    def __init__(self):
        pass


class DataOnlyMod:
    def __init__(self):
        self.Name = ""
        self.Version = 0
        self.IsUiOnly = False
        from Mafi import Option
        self.ModConfig = Option()

class IModWithMaps:
    def __init__(self):
        self.Name = ""
        self.Version = 0
        self.IsUiOnly = False
        from Mafi import Option
        self.ModConfig = Option()

class IModData:
    def __init__(self):
        pass


class RegistrationContext:
    def __init__(self):
        self.PrototypesDb = None

class ModsLoader:
    ASSET_BUNDLES_DIR_NAME = ""
    DLCS_DIR_NAME = ""
    def __init__(self):
        pass


class ModGroup:
    Core = None
    Dlc = None
    ThirdParty = None
    def __init__(self):
        self.value__ = 0

class ModData:
    def __init__(self):
        self.IsFullyLoaded = False
        self.FailedToLoad = False
        self.Group = None
        self.Name = ""
        self.ModType = None
        from Mafi import Option
        self.AssetsPath = Option()
        self.Version = None
        self.Exception = Option()

class ModsExtensions:
    def __init__(self):
        pass


class ProtoRegistrator:
    def __init__(self):
        self.RegisteredDataClasses = None
        self.DisableAllProtoCosts = False
        self.DisableVehicleFuelConsumption = False
        self.PrototypesDb = None
        self.LayoutParser = None
        self.BeaconProtoBuilder = None
        self.HouseProtoBuilder = None
        self.DataCenterProtoBuilder = None
        self.FluidProductProtoBuilder = None
        self.MachineProtoBuilder = None
        self.MineTowerProtoBuilder = None
        self.MoltenProductProtoBuilder = None
        self.NotificationProtoBuilder = None
        self.RecipeProtoBuilder = None
        self.ResearchLabProtoBuilder = None
        self.ResearchNodeProtoBuilder = None
        self.FleetEntityHullProtoBuilder = None
        self.SettlementModuleProtoBuilder = None
        self.RuinsProtoBuilder = None
        self.StorageProtoBuilder = None
        self.CargoDepotModuleProtoBuilder = None
        self.CargoDepotProtoBuilder = None
        self.RainwaterHarvesterProtoBuilder = None
        self.VehicleDepotProtoBuilder = None
        self.FuelTankProtoBuilder = None
        self.FuelStationProtoBuilder = None
        self.CargoShipProtoBuilder = None
        self.WellPumpProtoBuilder = None
        self.TruckProtoBuilder = None
        self.ExcavatorProtoBuilder = None

class DataRegistrationException:
    def __init__(self):
        self.Message = ""
        self.Data = None
        self.InnerException = None
        self.TargetSite = None
        self.StackTrace = ""
        self.HelpLink = ""
        self.Source = ""
        self.HResult = 0

class ProtoRegistratorConfigDevOnly:
    def __init__(self):
        self.DisableAllProtoCosts = False
        self.DisableVehicleFuelConsumption = False
