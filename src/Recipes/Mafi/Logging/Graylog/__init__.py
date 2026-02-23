
class GelfMessage:
    VERSION = ""
    def __init__(self):
        self.ShortMessage = ""
        from Mafi import Option
        self.FullMessage = Option()
        self.Timestamp = None
        self.Level = None

class SyslogSeverity:
    Emergency = None
    Alert = None
    Critical = None
    Error = None
    Warning = None
    Notice = None
    Informational = None
    Debug = None
    def __init__(self):
        self.value__ = 0

class IErrorLoggerConfig:
    def __init__(self):
        self.DisableAnonymousErrorLogs = False
        self.IsRunningInUnityEditor = False

class GraylogLogger:
    HOST_NAME = ""
    MAX_MESSAGES_PER_BATCH = 0
    def __init__(self):
        self.MessagesQueued = 0
        self.MessagesSent = 0
        self.MessagesDiscarded = 0
        self.IsLoggingStarted = False

class IGelfClient:
    def __init__(self):
        self.IsOperational = False

class UdpGelfClient:
    def __init__(self):
        self.IsOperational = False
