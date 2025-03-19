class INotification:

    def __init__(self):
        self.IsSuppressed = False
        self.IsEntityIconSuppressed = False
        self.NotificationId = None
        self.Proto = None
        self.Message = None
        from Mafi import Option
        self.Entity = Option()
class INotificationsManager:

    def __init__(self):
        pass

class NotificationsManagerExtensions:

    def __init__(self):
        pass

class NotificationDismissCmd:

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
class EntityNotificationProto:

    def __init__(self):
        self.Id = None
        self.IsTimeLimited = False
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

class GeneralNotificationProto:

    def __init__(self):
        self.Id = None
        self.IsTimeLimited = False
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

class NotificationProto:

    def __init__(self):
        self.IsTimeLimited = False
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

class NotificationStyle:

    def __init__(self):
        pass

class NotificationType:

    def __init__(self):
        pass

class NotificationProtoBuilder:

    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None
class StateGeneral:

    def __init__(self):
        pass

class StateEntity:

    def __init__(self):
        pass

class NotificationsManager:

    def __init__(self):
        pass

class EntityNotificator:

    def __init__(self):
        self.IsValid = False
        self.IsActive = False
        self.NotificationId = None
class Notificator:

    def __init__(self):
        self.IsValid = False
        self.IsActive = False
        self.NotificationId = None
class EntityNotificatorWithProtoParam:

    def __init__(self):
        self.IsActive = False
        self.NotificationId = None
        self.Prototype = None
class NotificatorWithProtoParam:

    def __init__(self):
        self.IsActive = False
        self.NotificationId = None
        self.Prototype = None
