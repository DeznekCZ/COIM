
class ITutorialProgressTracker:
    def __init__(self):
        pass


class AlwaysNewTutorialProgressTracker:
    def __init__(self):
        pass


class MarkMessageAsReadCmd:
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
        from Mafi.Core.Prototypes import Proto
        self.ProtoId = Proto.ID()


class Message:
    def __init__(self):
        self.IsRead = False
        self.NotificationTitle = None
        self.Title = None
        self.BgColor = None
        self.IconPath = ""
        self.Content = None
        self.Proto = None

class MessageGroupProto:
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
        self.Order = 0
        self.IsPhantom = False

class MessageProto:
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
        self.ForceOpen = False
        self.IgnoreInSandbox = False
        self.MessageType = None
        self.Content = None
        self.CurrentVersion = 0
        from Mafi import Option
        self.Group = Option()
        self.AlwaysNotify = False
        self.UnlockSilentlyFromStart = False
        self.IsPhantom = False

class InGameMessageType:
    Message = None
    Tutorial = None
    Warning = None
    def __init__(self):
        self.value__ = 0

class MessagesManager:
    def __init__(self):
        self.OnNewMessage = None
        self.AllMessages = None

class MessageTrigger:
    def __init__(self):
        pass


class MessageTriggerOnProtoUnlocked:
    def __init__(self):
        pass


class MessageTriggerOnEntityConstructed:
    def __init__(self):
        pass


class MessageTriggerOnEntityConstructedOrProductRunningOut:
    def __init__(self):
        pass


class MessageTriggerOnShipRepair:
    def __init__(self):
        pass


class MessageTriggerDelayed:
    def __init__(self):
        pass


class MessageTriggerGlobalProductLow:
    def __init__(self):
        pass


class MessageTriggerOnEvent:
    def __init__(self):
        pass


class MessageTriggerOnQuantityProduced:
    def __init__(self):
        pass


class MessageTriggerProto:
    def __init__(self):
        self.Implementation = None
        self.IsNotAvailable = False
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
        self.MessageProto = None
        self.IsPhantom = False

class MessageTriggerOnProtoUnlockedProto:
    def __init__(self):
        self.Implementation = None
        self.IsNotAvailable = False
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
        self.UnlockedProto = None
        self.MessageProto = None
        self.IsPhantom = False

class MessageTriggerOnQuantityProducedProto:
    def __init__(self):
        self.Implementation = None
        self.IsNotAvailable = False
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
        self.Product = None
        self.Quantity = None
        self.MessageProto = None
        self.IsPhantom = False

class MessageTriggerOnEntityConstructedProto:
    def __init__(self):
        self.Implementation = None
        self.IsNotAvailable = False
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.ConstructedProtoId = StaticEntityProto.ID()

        self.MessageProto = None
        self.IsPhantom = False

class MessageTriggerOnEntityConstructedOrProductRunningOutProto:
    def __init__(self):
        self.Implementation = None
        self.IsNotAvailable = False
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.ConstructedProtoId = StaticEntityProto.ID()

        self.ProductQuantityToTrigger = None
        self.MessageProto = None
        self.IsPhantom = False

class MessageTriggerOnShipRepairProto:
    def __init__(self):
        self.Implementation = None
        self.IsNotAvailable = False
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
        self.MessageProto = None
        self.IsPhantom = False

class MessageTriggerDelayedProto:
    def __init__(self):
        self.Implementation = None
        self.IsNotAvailable = False
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
        self.DelayDays = 0
        self.MessageProto = None
        self.IsPhantom = False

class MessageTriggerGlobalProductLowProto:
    def __init__(self):
        self.Implementation = None
        self.IsNotAvailable = False
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
        from Mafi.Core.Products import ProductProto
        self.ProductId = ProductProto.ID()

        self.MinQuantity = None
        self.MessageProto = None
        self.IsPhantom = False

class MessageTriggerOnEventProto:
    def __init__(self):
        self.Implementation = None
        self.IsNotAvailable = False
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
        self.DelayDays = 0
        self.GetEvent = None
        self.MessageProto = None
        self.IsPhantom = False
