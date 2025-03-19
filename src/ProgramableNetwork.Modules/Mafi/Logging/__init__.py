class IGameIdProvider:

    def __init__(self):
        self.GameId = None
        self.SessionId = None
        self.GameStartedAtUtc = None
        self.GameStartedAtVersion = str(0)
class IMapInfoProvider:

    def __init__(self):
        self.Name = str(0)
        self.MapVersion = int(0)
class IMapStartInfoProvider:

    def __init__(self):
        self.StartingLocationIndex = int(0)
class IMapEditorInfo:

    def __init__(self):
        pass

class JsonWriter:

    def __init__(self):
        pass

class LogEntry:

    def __init__(self):
        pass

class LogType:

    def __init__(self):
        pass

