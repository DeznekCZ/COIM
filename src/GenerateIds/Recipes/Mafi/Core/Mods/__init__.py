
class IMod:
    def __init__(self):
        self.Manifest = None
        self.IsUiOnly = False
        from Mafi import Option
        self.ModConfig = Option()
        self.JsonConfig = None

class IModConfig:
    def __init__(self):
        pass


class DataOnlyMod:
    def __init__(self):
        self.Manifest = None
        self.IsUiOnly = False
        from Mafi import Option
        self.ModConfig = Option()
        self.JsonConfig = None

class IModWithMaps:
    def __init__(self):
        self.Manifest = None
        self.IsUiOnly = False
        from Mafi import Option
        self.ModConfig = Option()
        self.JsonConfig = None

class IModData:
    def __init__(self):
        pass


class RegistrationContext:
    def __init__(self):
        self.PrototypesDb = None

class ModJsonConfig:
    CONFIG_FILE_NAME = ""
    def __init__(self):
        self.ModId = ""
        self.LoadedVersion = None
        self.IsEmpty = False
        self.Parameters = None

class ModJsonConfigParam:
    def __init__(self):
        self.CurrentValue = None
        self.Name = ""
        self.Description = ""
        self.DefaultValue = None
        self.Min = None
        self.Max = None
        self.MaxLength = None
        self.RegexPattern = ""

class ModManifest:
    MAX_NAME_LENGTH = 0
    MAX_SHORT_DESC_LENGTH = 0
    ID_KEY = ""
    ID_VERSION = ""
    ID_NAME = ""
    ID_DESC_SHORT = ""
    ID_DESC_LONG = ""
    ID_AUTHORS = ""
    ID_MIN_GAME_VERSION = ""
    ID_MAX_VERIFIED_GAME_VERSION = ""
    ID_PRIMARY_DLLS = ""
    ID_LINKS = ""
    ID_DEPS_MANDATORY = ""
    ID_DEPS_OPTIONAL = ""
    ID_INCOMPATIBLE_MODS = ""
    ID_NON_LOCKING_LOAD = ""
    ID_CAN_ADD_TO_SAVED_GAME = ""
    ID_CAN_REMOVE_FROM_SAVED_GAME = ""
    ID_PRIMARY_MOD_CLASS_NAME = ""
    def __init__(self):
        self.CanBeUnselectedWhenLoaded = False
        self.IsCoreMod = False
        self.IsDlcMod = False
        self.IsThirdParty = False
        self.RootDirectoryPath = ""
        self.AssetBundlesDirOverride = ""
        self.Id = ""
        self.Version = None
        self.DisplayName = ""
        self.DescriptionShort = ""
        self.DescriptionLong = ""
        self.Authors = None
        self.MinGameVersion = None
        self.MaxVerifiedGameVersion = None
        self.Links = None
        self.PrimaryDlls = None
        self.MandatoryDependencies = None
        self.OptionalDependencies = None
        self.IncompatibleModIds = None
        self.NonLockingDllLoad = False
        self.CanAddToSavedGame = False
        self.CanRemoveFromSavedGame = False
        self.PrimaryModClassName = ""
        self.LoadErrors = None
        from Mafi import Option
        self.DlcDefinition = Option()

class ModDependency:
    def __init__(self):
        self.Id = ""
        self.MinVersion = None

class ModsLoader:
    LoadedAndFailedMods = None
    SUPPORTER_DLC_RECORD = None
    TRAINS_DLC_RECORD = None
    PUBLISHED_DLCS = None
    STEAM_APP_ID = None
    ASSET_BUNDLES_DIR_NAME = ""
    DLCS_DIR_NAME = ""
    MANIFEST_FILE_NAME = ""
    CHANGELOG_NAME = ""
    COI_MOD_PREFIX = ""
    CORE_MOD_ID = ""
    CORE_BASE_MOD_ID = ""
    CORE_UNITY_MOD_ID = ""
    DLC_SUPPORTER_MOD_ID = ""
    DLC_TRAINS_MOD_ID = ""
    def __init__(self):
        pass


class AvailableModData:
    def __init__(self):
        self.Manifest = None

class LoadedModData:
    def __init__(self):
        from Mafi import Option
        self.LoadError = Option()
        self.Manifest = None
        self.ModType = Option()
        self.Assemblies = None

class InstantiatedModData:
    def __init__(self):
        self.Manifest = None
        self.LoadedModData = None
        from Mafi import Option
        self.Mod = Option()
        self.LoadError = Option()

class PublishedDlcRecord:
    def __init__(self):
        self.Id = ""
        self.Name = None
        self.Description = None
        self.SteamAppId = None
        self.ThumbnailPrefabPath = ""
        self.ProjectName = ""

class InvalidModData:
    def __init__(self):
        self.PathToDirectory = ""
        self.ErrorShort = None
        self.Error = ""

class ModsExtensions:
    def __init__(self):
        pass


class ProtoRegistrator:
    def __init__(self):
        self.RegisteredDataClasses = None
        self.IgnoredDataClasses = None
        self.IsSandbox = False
        self.ActiveMod = None
        self.DisableAllProtoCosts = False
        self.DisableVehicleFuelConsumption = False
        self.AllMods = None
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
        self.WellPumpProtoBuilder = None
        self.TruckProtoBuilder = None

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
