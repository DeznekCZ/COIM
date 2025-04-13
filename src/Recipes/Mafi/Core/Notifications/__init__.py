
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
        self.ErrorMessage = ""
        self.NotificationsIds = None

class EntityNotificationProto:
    def __init__(self):
        from Mafi.Core.Notifications import EntityNotificationProto
        self.Id = EntityNotificationProto.ID()

        self.IsTimeLimited = False
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.ExtraMessageForInspector = None
        self.Tutorial = None
        self.HideInNotificationPanel = False
        self.HideInInspector = False
        self.Style = None
        self.Type = None
        from Mafi import Option
        self.IconAssetPath = Option()
        self.EntityIconSpec = None
        self.SuppressEntityIconOnSuppress = False
        self.TimeToLive = None
        self.IsPhantom = False
        self.IsInitialized = False

    class ID:
        def __init__(self):
            self.Value = ""

class GeneralNotificationProto:
    def __init__(self):
        from Mafi.Core.Notifications import GeneralNotificationProto
        self.Id = GeneralNotificationProto.ID()

        self.IsTimeLimited = False
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.HideInNotificationPanel = False
        self.HideInInspector = False
        self.Style = None
        self.Type = None
        from Mafi import Option
        self.IconAssetPath = Option()
        self.EntityIconSpec = None
        self.SuppressEntityIconOnSuppress = False
        self.TimeToLive = None
        self.IsPhantom = False
        self.IsInitialized = False

    class ID:
        def __init__(self):
            self.Value = ""

class NotificationProto:
    def __init__(self):
        self.IsTimeLimited = False
        from Mafi.Core.Notifications import NotificationProto
        self.Id = NotificationProto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.HideInNotificationPanel = False
        self.HideInInspector = False
        self.Style = None
        self.Type = None
        from Mafi import Option
        self.IconAssetPath = Option()
        self.EntityIconSpec = None
        self.SuppressEntityIconOnSuppress = False
        self.TimeToLive = None
        self.IsPhantom = False
        self.IsInitialized = False

    class ID:
        def __init__(self):
            self.Value = ""

class NotificationStyle:
    Success = None
    Warning = None
    Critical = None
    def __init__(self):
        self.value__ = 0

class NotificationType:
    Continuous = None
    OneTimeOnly = None
    def __init__(self):
        self.value__ = 0

class NotificationProtoBuilder:
    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None

    class StateGeneral:
        def __init__(self):
            self.Builder = None

    class StateEntity:
        def __init__(self):
            self.Builder = None

class NotificationsManager:
    def __init__(self):
        pass


class EntityNotificator:
    def __init__(self):
        self.IsValid = False
        self.IsActive = False
        self.NotificationId = None
        self.Prototype = None

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
