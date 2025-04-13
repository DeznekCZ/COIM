
class IMessageNotification:
    def __init__(self):
        self.NotificationId = None

class MessageNotificationDismissCmd:
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
        self.MessageId = None

class IMessageNotificationsManager:
    def __init__(self):
        self.AllNotifications = None

class MessageNotificationsManager:
    def __init__(self):
        self.AllNotifications = None
