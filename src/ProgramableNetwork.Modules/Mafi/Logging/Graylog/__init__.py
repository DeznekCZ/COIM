class GelfMessage:

    def __init__(self):
        pass

class SyslogSeverity:

    def __init__(self):
        pass

class IErrorLoggerConfig:

    def __init__(self):
        self.DisableAnonymousErrorLogs = False
        self.IsRunningInUnityEditor = False
class GraylogLogger:

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
