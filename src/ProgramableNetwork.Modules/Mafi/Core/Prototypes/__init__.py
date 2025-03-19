class CoreProtoTags:

    def __init__(self):
        pass

class EntityCosts:

    def __init__(self):
        pass

class EntityCostsTpl:

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
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class ID:

    def __init__(self):
        pass

class Str:

    def __init__(self):
        pass

class Gfx:

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
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.Mod = None
class IProtoWithIcon:

    def __init__(self):
        self.IconPath = str(0)
        self.Strings = None
        self.Id = None
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.Mod = None
class IProtoWithPropertiesUpdate:

    def __init__(self):
        self.Strings = None
        self.Id = None
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.Mod = None
class IProtoWithUpgrade:

    def __init__(self):
        self.UpgradeNonGeneric = None
        self.Strings = None
        self.Id = None
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.Mod = None
class IUpgradeData:

    def __init__(self):
        from Mafi import Option
        self.NextTierNonGeneric = Option()
        from Mafi import Option
        self.PreviousTierNonGeneric = Option()
class IProto:

    def __init__(self):
        self.Strings = None
        self.Id = None
        self.IsAvailable = False
        self.IsNotAvailable = False
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
