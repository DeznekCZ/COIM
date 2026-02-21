
class CoreProtoTags:
    MechanicalShaft = None
    def __init__(self):
        pass


class CombineUnderProtoParam:
    def __init__(self):
        self.AllowedProtoType = None
        self.Proto = None

class EntityCosts:
    None = None
    def __init__(self):
        self.BaseConstructionCost = None
        self.Workers = 0
        self.DefaultPriority = 0
        self.Maintenance = None

class EntityCostsTpl:
    Build = None
    def __init__(self):
        pass


    class Builder:
        def __init__(self):
            pass


    class MaintenanceCostsTpl:
        def __init__(self):
            self.Product = None
            self.Quantity = None
            self.ExtraBufferDuration = None
            self.InitialMaintenanceBoost = None

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
    def __init__(self):
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

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
        self.IsPhantom = False

    class ID:
        def __init__(self):
            self.Value = ""

    class Str:
        Empty = None
        def __init__(self):
            self.Name = None
            self.DescShort = None

    class Gfx:
        EMPTY_PATH = ""
        def __init__(self):
            pass


class IProtoBuilder:
    def __init__(self):
        self.Registrator = None
        self.ProtosDb = None

class IProtoWithIconAndName:
    def __init__(self):
        self.QuantityFormatter = None
        self.IconPath = ""
        self.Strings = None
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.IsLocked = False
        self.IsUnlocked = False
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsInitialized = False
        self.IsObsolete = False
        self.Mod = None

class IProtoWithIcon:
    def __init__(self):
        self.IconPath = ""
        self.Strings = None
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.IsLocked = False
        self.IsUnlocked = False
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsInitialized = False
        self.IsObsolete = False
        self.Mod = None

class IProtoWithParticleColor:
    def __init__(self):
        self.ParticleColor = None
        self.Strings = None
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.IsLocked = False
        self.IsUnlocked = False
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsInitialized = False
        self.IsObsolete = False
        self.Mod = None

class IProtoWithPreviewIcons:
    def __init__(self):
        self.Strings = None
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.IsLocked = False
        self.IsUnlocked = False
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsInitialized = False
        self.IsObsolete = False
        self.Mod = None

class IProtoWithPropertiesUpdate:
    def __init__(self):
        self.Strings = None
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.IsLocked = False
        self.IsUnlocked = False
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsInitialized = False
        self.IsObsolete = False
        self.Mod = None

class IProtoWithTiers:
    def __init__(self):
        self.TierData = None
        self.IconPath = ""
        self.Strings = None
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.IsLocked = False
        self.IsUnlocked = False
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsInitialized = False
        self.IsObsolete = False
        self.Mod = None

class IProtoWithUpgrade:
    def __init__(self):
        self.Upgrade = None
        self.TierData = None
        self.IconPath = ""
        self.Strings = None
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.IsLocked = False
        self.IsUnlocked = False
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsInitialized = False
        self.IsObsolete = False
        self.Mod = None

class IProtoWithUpgradeAndCustomUi:
    def __init__(self):
        self.Upgrade = None
        self.TierData = None
        self.IconPath = ""
        self.Strings = None
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.IsLocked = False
        self.IsUnlocked = False
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsInitialized = False
        self.IsObsolete = False
        self.Mod = None

class ITierData:
    def __init__(self):
        from Mafi import Option
        self.NextTierIndirect = Option()
        self.PreviousTierIndirect = Option()
        self.TierNumberForUi = 0

class UpgradeExtensions:
    def __init__(self):
        pass


class UpgradeData:
    def __init__(self):
        from Mafi import Option
        self.NextTier = Option()
        self.PreviousTier = Option()
        self.SkipFromReplaceFlow = False
        self.CannotDowngrade = False
        self.CannotSkipUpgrade = False
        self.CannotMove = False
        self.TierData = None

class TierData:
    def __init__(self):
        from Mafi import Option
        self.NextTierIndirect = Option()
        self.PreviousTierIndirect = Option()
        self.TierNumberForUi = 0

class IProto:
    def __init__(self):
        self.Strings = None
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.IsLocked = False
        self.IsUnlocked = False
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsInitialized = False
        self.IsObsolete = False
        self.Mod = None

class InvalidProto:
    def __init__(self):
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

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
        self.IsPhantom = False

class ProtoChecks:
    def __init__(self):
        pass


class INotInitializedProto:
    def __init__(self):
        pass


class IProtoWithAssets:
    def __init__(self):
        self.Strings = None
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.IsLocked = False
        self.IsUnlocked = False
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsInitialized = False
        self.IsObsolete = False
        self.Mod = None

class IProtoWithValidationSuppressFlag:
    def __init__(self):
        self.ValidationSuppressFlag = None
        self.Strings = None
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.IsLocked = False
        self.IsUnlocked = False
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsInitialized = False
        self.IsObsolete = False
        self.Mod = None

class ProtoInitException:
    def __init__(self):
        self.Message = ""
        self.Data = None
        self.InnerException = None
        self.TargetSite = None
        self.StackTrace = ""
        self.HelpLink = ""
        self.Source = ""
        self.HResult = 0

class InvalidProtoException:
    def __init__(self):
        self.Message = ""
        self.Data = None
        self.InnerException = None
        self.TargetSite = None
        self.StackTrace = ""
        self.HelpLink = ""
        self.Source = ""
        self.HResult = 0

class ProtoExtensions:
    def __init__(self):
        pass


class ProtosDb:
    def __init__(self):
        self.ActiveMod = None
        self.ProtosLockedOnInit = None
        self.Phantoms = None
        self.PropertyIdsToTrack = None

class ProtosSerializerFactory:
    def __init__(self):
        pass


class NoProtoAllowedSerializerFactory:
    def __init__(self):
        pass


class Tag:
    def __init__(self):
        self.TargetType = None
        self.Id = ""

class IProtoParam:
    def __init__(self):
        self.AllowedProtoType = None

class UnlockedProtosDb:
    def __init__(self):
        self.OnUnlockedSetChanged = None
        self.OnProtoUnlocked = None

class IUnlockedProtosConfig:
    def __init__(self):
        self.ShouldUnlockAllProtosOnInit = False
