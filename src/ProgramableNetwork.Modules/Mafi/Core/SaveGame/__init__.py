class IAsyncSavable:

    def __init__(self):
        pass

class ASyncSaver:

    def __init__(self):
        self.Started = False
        self.Finished = False
class SaveWriteStatus:
    Unknown = None
    OldTmpDeleted = None
    OldTmpFailToDelete = None
    NoOldTmp = None
    TmpFileWritten = None
    ChecksumComputed = None
    ChecksumVerified = None
    OldSaveDeleted = None
    NoOldSave = None

    def __init__(self):
        pass

class GameSaveInfo:
    Empty = None

    def __init__(self):
        self.IsEmpty = False
class IGameSaveInfoProvider:

    def __init__(self):
        pass

class SimpleGameSaveInfoProvider:

    def __init__(self):
        pass

class SaveChecksumValidationResults:
    FailUnknown = None
    FailException = None
    FailChecksum = None
    FailDataSize = None
    FailChecksumBeforeCompression = None
    FailDataSizeBeforeCompression = None
    Success = None

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
        self.MaxAutoSavesCount = None
class ISaveManager:

    def __init__(self):
        self.GameName = str(0)
class MapSerializer:
    HEADER_MAP_ASCII = None
    HEADER_MAP = None
    HEADER_MAP_PREVIEW_ASCII = None
    HEADER_MAP_PREVIEW = None
    HEADER_MAP_EXTRA_ASCII = None
    HEADER_MAP_EXTRA = None
    HEADER_MAP_DATA_ASCII = None
    HEADER_MAP_DATA = None
    MIN_COMPATIBLE_SAVE_VERSION = None

    def __init__(self):
        self.LastSaveStartDuration = None
        self.LastSaveFinalizeDuration = None
        self.LastSaveTotalDuration = None
class LoadFailInfo:

    def __init__(self):
        pass

class Reason:
    VersionTooOld = None
    VersionTooNew = None
    FileAccessIssue = None
    ModsMissing = None
    FileCorrupted = None
    Unknown = None

    def __init__(self):
        pass

class ModHelper:

    def __init__(self):
        pass

class PassThroughSaveCompressor:

    def __init__(self):
        pass

class SaveCompressionType:
    NoCompression = None
    Gzip = None

    def __init__(self):
        pass

class SaveHeader:

    def __init__(self):
        pass

class SaveLoadFileUtils:

    def __init__(self):
        pass

class SaveManager:
    AUTOSAVE_OPTIONS_MINUTES = None
    AUTOSAVE_OPTIONS_DEFAULT_INDEX = None
    AUTOSAVE_DEFAULT_INTERVAL_MINUTES = None
    MAX_AUTOSAVES_COUNT_OPTIONS = None
    MAX_AUTOSAVES_COUNT_DEFAULT_INDEX = None
    MAX_AUTOSAVES_COUNT_DEFAULT = None

    def __init__(self):
        self.AutosaveMinInterval = None
        from Mafi import Option
        self.LastSaveFilePath = Option()
        from Mafi import Option
        self.LastAutoSaveFilePath = Option()
        self.GameName = str(0)
        self.IsSavePending = False
class SaveResult:

    def __init__(self):
        pass

