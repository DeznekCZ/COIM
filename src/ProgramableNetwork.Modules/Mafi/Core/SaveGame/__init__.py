class IAsyncSavable:

    def __init__(self):
        pass

class ASyncSaver:

    def __init__(self):
        self.Started = False
        self.Finished = False
class SaveWriteStatus:

    def __init__(self):
        pass

class GameSaveInfo:

    def __init__(self):
        self.IsEmpty = False
class IGameSaveInfoProvider:

    def __init__(self):
        pass

class SimpleGameSaveInfoProvider:

    def __init__(self):
        pass

class SaveChecksumValidationResults:

    def __init__(self):
        pass

class ModInfoRaw:

    def __init__(self):
        pass

class GzipSaveCompressor:

    def __init__(self):
        pass

class ISaveCompressor:

    def __init__(self):
        pass

class ISaveConfig:

    def __init__(self):
        self.SaveCompressionType = None
        self.AutoSaveInterval = None
class ISaveManager:

    def __init__(self):
        self.GameName = str(0)
class MapSerializer:

    def __init__(self):
        self.LastSaveStartDuration = None
        self.LastSaveFinalizeDuration = None
        self.LastSaveTotalDuration = None
class LoadFailInfo:

    def __init__(self):
        pass

class Reason:

    def __init__(self):
        pass

class ModHelper:

    def __init__(self):
        pass

class PassThroughSaveCompressor:

    def __init__(self):
        pass

class SaveCompressionType:

    def __init__(self):
        pass

class SaveHeader:

    def __init__(self):
        pass

class SaveLoadFileUtils:

    def __init__(self):
        pass

class SaveManager:

    def __init__(self):
        self.GameName = str(0)
        self.IsSavePending = False
class SaveResult:

    def __init__(self):
        pass

