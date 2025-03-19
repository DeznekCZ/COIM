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
        self.ErrorMessage = str(0)
class Message:

    def __init__(self):
        self.IsRead = False
        self.NotificationTitle = None
        self.Title = None
        self.BgColor = None
        self.IconPath = str(0)
        self.Content = None
class MessageGroupProto:

    def __init__(self):
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class MessageProto:

    def __init__(self):
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class InGameMessageType:

    def __init__(self):
        pass

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
        self.IsAvailable = False
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsObsolete = False
class MessageTriggerOnProtoUnlockedProto:

    def __init__(self):
        self.Implementation = None
        self.IsAvailable = False
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsObsolete = False
class MessageTriggerOnQuantityProducedProto:

    def __init__(self):
        self.Implementation = None
        self.IsAvailable = False
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsObsolete = False
class MessageTriggerOnEntityConstructedProto:

    def __init__(self):
        self.Implementation = None
        self.IsAvailable = False
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsObsolete = False
class MessageTriggerOnEntityConstructedOrProductRunningOutProto:

    def __init__(self):
        self.Implementation = None
        self.IsAvailable = False
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsObsolete = False
class MessageTriggerOnShipRepairProto:

    def __init__(self):
        self.Implementation = None
        self.IsAvailable = False
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsObsolete = False
class MessageTriggerDelayedProto:

    def __init__(self):
        self.Implementation = None
        self.IsAvailable = False
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsObsolete = False
class MessageTriggerGlobalProductLowProto:

    def __init__(self):
        self.Implementation = None
        self.IsAvailable = False
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsObsolete = False
class MessageTriggerOnEventProto:

    def __init__(self):
        self.Implementation = None
        self.IsAvailable = False
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsObsolete = False
