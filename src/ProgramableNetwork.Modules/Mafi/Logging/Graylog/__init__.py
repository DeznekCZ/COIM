class GelfMessage:
    VERSION = None

    def __init__(self):
        pass

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
        pass

class IErrorLoggerConfig:

    def __init__(self):
        self.DisableAnonymousErrorLogs = False
        self.IsRunningInUnityEditor = False
class GraylogLogger:
    MAX_MESSAGES_PER_BATCH = None

    def __init__(self):
        self.MessagesQueued = int(0)
        self.MessagesSent = int(0)
        self.MessagesDiscarded = int(0)
        self.IsLoggingStarted = False
class IGelfClient:

    def __init__(self):
        self.IsOperational = False
class UdpGelfClient:

    def __init__(self):
        self.IsOperational = False
