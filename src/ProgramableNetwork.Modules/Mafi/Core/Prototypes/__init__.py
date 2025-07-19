class CoreProtoTags:
    MechanicalShaft = None

    def __init__(self):
        pass

class CombineUnderProtoParam:

    def __init__(self):
        self.AllowedProtoType = None
class EntityCosts:
    None = None

    def __init__(self):
        pass

class EntityCostsTpl:
    Build = None

    def __init__(self):
        pass

class Builder:

    def __init__(self):
        pass

class MaintenanceCostsTpl:

    def __init__(self):
        pass

class IProtoWithPowerConsumption:

    def __init__(self):
        self.ElectricityConsumed = None
class IProtoWithPowerProduction:

    def __init__(self):
        self.ElectricityProduced = None
class IProtoWithUnityConsumption:

    def __init__(self):
        self.UnityMonthlyCost = None
class IProtoWithComputingConsumption:

    def __init__(self):
        self.ComputingConsumed = None
class IProtoWithRecipes:

    def __init__(self):
        self.Recipes = None
class IProtoWithUiRecipe:

    def __init__(self):
        self.Recipe = None
class IProtoWithUiRecipes:

    def __init__(self):
        self.Recipes = None
class IProtoWithAnimation:

    def __init__(self):
        self.AnimationParams = None
class Proto:
    AllPhantoms = None

    def __init__(self):
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
class ID:

    def __init__(self):
        pass

class Str:
    Empty = None

    def __init__(self):
        pass

class Gfx:
    EMPTY_PATH = None
    GENERATED_ICON_PATH_PREFIX = None
    GENERATED_ANIMATION_PATH_PREFIX = None

    def __init__(self):
        pass

class IProtoBuilder:

    def __init__(self):
        self.Registrator = None
        self.ProtosDb = None
class IProtoWithIconAndName:

    def __init__(self):
        self.QuantityFormatter = None
        self.IconPath = str(0)
        self.Strings = None
        self.Id = None
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsInitialized = False
        self.Mod = None
class IProtoWithIcon:

    def __init__(self):
        self.IconPath = str(0)
        self.Strings = None
        self.Id = None
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsInitialized = False
        self.Mod = None
class IProtoWithPropertiesUpdate:

    def __init__(self):
        self.Strings = None
        self.Id = None
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsInitialized = False
        self.Mod = None
class IProtoWithTiers:

    def __init__(self):
        self.TierData = None
        self.IconPath = str(0)
        self.Strings = None
        self.Id = None
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsInitialized = False
        self.Mod = None
class IProtoWithUpgrade:

    def __init__(self):
        self.UpgradeNonGeneric = None
        self.TierData = None
        self.IconPath = str(0)
        self.Strings = None
        self.Id = None
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsInitialized = False
        self.Mod = None
class IUpgradeData:

    def __init__(self):
        from Mafi import Option
        self.NextTierNonGeneric = Option()
        from Mafi import Option
        self.PreviousTierNonGeneric = Option()
class ITierData:

    def __init__(self):
        from Mafi import Option
        self.NextTierIndirect = Option()
        from Mafi import Option
        self.PreviousTierIndirect = Option()
        self.TierNumberForUi = int(0)
class UpgradeExtensions:

    def __init__(self):
        pass

class TierData:

    def __init__(self):
        from Mafi import Option
        self.NextTierIndirect = Option()
        from Mafi import Option
        self.PreviousTierIndirect = Option()
        self.TierNumberForUi = int(0)
class IProto:

    def __init__(self):
        self.Strings = None
        self.Id = None
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsInitialized = False
        self.Mod = None
class ProtoChecks:

    def __init__(self):
        pass

class INotInitializedProto:

    def __init__(self):
        pass

class ProtoInitException:

    def __init__(self):
        self.Message = str(0)
        self.Data = None
        self.InnerException = None
        self.TargetSite = None
        self.StackTrace = str(0)
        self.HelpLink = str(0)
        self.Source = str(0)
        self.HResult = int(0)
class InvalidProtoException:

    def __init__(self):
        self.Message = str(0)
        self.Data = None
        self.InnerException = None
        self.TargetSite = None
        self.StackTrace = str(0)
        self.HelpLink = str(0)
        self.Source = str(0)
        self.HResult = int(0)
class ProtoExtensions:

    def __init__(self):
        pass

class ProtosDb:

    def __init__(self):
        self.ProtosLockedOnInit = None
        self.PropertyIdsToTrack = None
class ProtosSerializerFactory:

    def __init__(self):
        pass

class NoProtoAllowedSerializerFactory:

    def __init__(self):
        pass

class Tag:

    def __init__(self):
        pass

class IProtoParam:

    def __init__(self):
        self.AllowedProtoType = None
class UnlockedProtosDb:

    def __init__(self):
        self.OnUnlockedSetChanged = None
class IUnlockedProtosConfig:

    def __init__(self):
        self.ShouldUnlockAllProtosOnInit = False
