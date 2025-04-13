
class IAsyncSavable:
    def __init__(self):
        pass


class ASyncSaver:
    def __init__(self):
        self.Started = False
        self.Finished = False
        from Mafi import Option
        self.LastSavePath = Option()
        self.LastSaveError = None
        self.LastSaveException = Option()

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
        self.value__ = 0

class GameSaveInfo:
    Empty = None
    def __init__(self):
        self.IsEmpty = False
        self.GameDate = None
        self.MapName = ""
        self.Population = 0
        self.ResearchNodesUnlocked = 0
        self.ResearchNodesTotal = 0
        self.LaunchCount = 0
        self.Notes = ""
        self.ThumbnailImageSize = None
        self.ThumbnailImageBytes = None

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
        self.value__ = 0

class ModInfoRaw:
    def __init__(self):
        self.Name = ""
        self.Version = 0
        self.TypeStr = ""

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
        self.GameName = ""

class MapSerializer:
    HEADER_MAP_ASCII = None
    HEADER_MAP = None
    HEADER_MAP_PREVIEW_ASCII = None
    HEADER_MAP_PREVIEW = None
    HEADER_MAP_EXTRA_ASCII = None
    HEADER_MAP_EXTRA = None
    HEADER_MAP_DATA_ASCII = None
    HEADER_MAP_DATA = None
    MIN_COMPATIBLE_SAVE_VERSION = 0
    def __init__(self):
        self.LastSaveStartDuration = None
        self.LastSaveFinalizeDuration = None
        self.LastSaveTotalDuration = None

class LoadFailInfo:
    def __init__(self):
        self.FailReason = None
        self.SaveVersion = None
        self.MessageForPlayer = None

    class Reason:
        Version = None
        FileAccessIssue = None
        ModsMissing = None
        FileCorrupted = None
        Unknown = None
        def __init__(self):
            self.value__ = 0

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
        self.value__ = 0

class SaveHeader:
    def __init__(self):
        self.Header = None
        self.Version = 0
        self.CompressionType = None
        self.DataSize = 0
        self.DataCrc32Checksum = None
        self.DataSizeBeforeCompression = 0
        self.DataCrc32ChecksumBeforeCompression = None

class SaveLoadFileUtils:
    def __init__(self):
        pass


class SaveManager:
    AUTOSAVE_OPTIONS_MINUTES = None
    AUTOSAVE_OPTIONS_DEFAULT_INDEX = 0
    AUTOSAVE_DEFAULT_INTERVAL_MINUTES = 0
    def __init__(self):
        self.GameName = ""
        self.IsSavePending = False

class SaveResult:
    def __init__(self):
        self.Error = None
        from Mafi import Option
        self.FilePath = Option()
        self.Exception = Option()
