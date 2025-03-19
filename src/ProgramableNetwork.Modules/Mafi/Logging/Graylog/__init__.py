class GelfMessage:

    def __init__(self):
        pass

class SyslogSeverity:

    def __init__(self):
        pass

class IErrorLoggerConfig:

    def __init__(self):
        self.DisableAnonymousErrorLogs = False
class GraylogLogger:

    def __init__(self):
        self.MessagesReceived = int(0)
        self.MessagesSent = int(0)
        self.IsLoggingStarted = False
class IGelfClient:

    def __init__(self):
        self.IsOperational = False
class UdpGelfClient:

    def __init__(self):
        self.IsOperational = False
