
class IGameIdProvider:
    def __init__(self):
        self.GameId = 0
        self.SessionId = 0
        self.GameStartedAtUtc = None
        self.GameStartedAtVersion = ""

class IMapInfoProvider:
    def __init__(self):
        self.Name = ""
        self.MapVersion = 0

class IMapStartInfoProvider:
    def __init__(self):
        self.StartingLocationIndex = 0

class IMapEditorInfo:
    def __init__(self):
        pass


class JsonWriter:
    def __init__(self):
        pass


class LogEntry:
    def __init__(self):
        self.TimestampUtc = None
        self.Type = None
        self.Message = ""
        self.ThreadName = ""
        self.SimStep = None
        from Mafi import Option
        self.StackTrace = Option()
        self.Exception = Option()
        self.AdditionalIntData = Option()
        self.AdditionalStringData = Option()

class LogType:
    Exception = None
    Error = None
    Assert = None
    Warning = None
    Info = None
    GameProgress = None
    Debug = None
    All = None
    def __init__(self):
        self.value__ = 0
