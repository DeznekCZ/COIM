class DoNotSaveAttribute:

    def __init__(self):
        self.TypeId = None
class DoNotSaveCreateNewOnLoadAttribute:

    def __init__(self):
        self.TypeId = None
class RenamedInVersionAttribute:

    def __init__(self):
        self.TypeId = None
class GenerateSerializer:

    def __init__(self):
        self.TypeId = None
class ManuallyWrittenSerializationAttribute:

    def __init__(self):
        self.TypeId = None
class DisableDirectCallSerializationAttribute:

    def __init__(self):
        self.TypeId = None
class SkipDuringDeterminismValidation:

    def __init__(self):
        self.TypeId = None
class SerializeAsGlobalDepAttribute:

    def __init__(self):
        self.TypeId = None
class DeserializeUsingMethodAttribute:

    def __init__(self):
        self.TypeId = None
class IgnoreMissingSerializer:

    def __init__(self):
        self.TypeId = None
class LoadCtorAttribute:

    def __init__(self):
        self.TypeId = None
class NewInSaveVersionAttribute:

    def __init__(self):
        self.TypeId = None
class MemberRemovedInSaveVersionAttribute:

    def __init__(self):
        self.TypeId = None
class OnlyForSaveCompatibilityAttribute:

    def __init__(self):
        self.TypeId = None
class SerializeUsingNonVariableEncodingAttribute:

    def __init__(self):
        self.TypeId = None
class InitAfterLoadAttribute:

    def __init__(self):
        self.TypeId = None
class InitPriority:

    def __init__(self):
        pass

class SerializeNullAsEmptyArrayAttribute:

    def __init__(self):
        self.TypeId = None
class BoolSerializer:
    Instance = None

    def __init__(self):
        self.SerializeAction = None
        self.DeserializeFunction = None
class ByteSerializer:
    Instance = None

    def __init__(self):
        self.SerializeAction = None
        self.DeserializeFunction = None
class SByteSerializer:
    Instance = None

    def __init__(self):
        self.SerializeAction = None
        self.DeserializeFunction = None
class CharSerializer:
    Instance = None

    def __init__(self):
        self.SerializeAction = None
        self.DeserializeFunction = None
class ShortSerializer:
    Instance = None

    def __init__(self):
        self.SerializeAction = None
        self.DeserializeFunction = None
class UShortSerializer:
    Instance = None

    def __init__(self):
        self.SerializeAction = None
        self.DeserializeFunction = None
class IntSerializer:
    Instance = None

    def __init__(self):
        self.SerializeAction = None
        self.DeserializeFunction = None
class UIntSerializer:
    Instance = None

    def __init__(self):
        self.SerializeAction = None
        self.DeserializeFunction = None
class LongSerializer:
    Instance = None

    def __init__(self):
        self.SerializeAction = None
        self.DeserializeFunction = None
class ULongSerializer:
    Instance = None

    def __init__(self):
        self.SerializeAction = None
        self.DeserializeFunction = None
class FloatSerializer:
    Instance = None

    def __init__(self):
        self.SerializeAction = None
        self.DeserializeFunction = None
class DoubleSerializer:
    Instance = None

    def __init__(self):
        self.SerializeAction = None
        self.DeserializeFunction = None
class StringSerializer:
    Instance = None

    def __init__(self):
        self.SerializeAction = None
        self.DeserializeFunction = None
class TypeSerializer:
    Instance = None

    def __init__(self):
        self.SerializeAction = None
        self.DeserializeFunction = None
class BlobReader:

    def __init__(self):
        self.InputStream = None
        self.DelayedDeserializationsCount = int(0)
class BlobReaderExtensions:

    def __init__(self):
        pass

class BlobSubReader:

    def __init__(self):
        self.IsDone = False
        self.IsNotDone = False
        self.InputStream = None
        self.DelayedDeserializationsCount = int(0)
class BlobWriter:

    def __init__(self):
        self.DelayedSerializationsCount = int(0)
class ISpecialSerializerFactory:

    def __init__(self):
        pass

class ISpecialSerializerFactoryCustom:

    def __init__(self):
        pass

class CorruptedSaveException:

    def __init__(self):
        self.Message = str(0)
        self.Data = None
        self.InnerException = None
        self.TargetSite = None
        self.StackTrace = str(0)
        self.HelpLink = str(0)
        self.Source = str(0)
        self.HResult = int(0)
class IncompatibleSaveVersionException:

    def __init__(self):
        self.Message = str(0)
        self.Data = None
        self.InnerException = None
        self.TargetSite = None
        self.StackTrace = str(0)
        self.HelpLink = str(0)
        self.Source = str(0)
        self.HResult = int(0)
class Crc32:

    def __init__(self):
        pass

class Crc32WriteStream:

    def __init__(self):
        self.CanRead = False
        self.CanSeek = False
        self.CanWrite = False
        self.Length = None
        self.Position = None
        self.Crc32 = None
        self.CanTimeout = False
        self.ReadTimeout = int(0)
        self.WriteTimeout = int(0)
class CSharpGen:

    def __init__(self):
        pass

class ParameterData:

    def __init__(self):
        pass

class CSharpGenCtorAttribute:

    def __init__(self):
        self.TypeId = None
class GameLoadException:

    def __init__(self):
        self.Message = str(0)
        self.Data = None
        self.InnerException = None
        self.TargetSite = None
        self.StackTrace = str(0)
        self.HelpLink = str(0)
        self.Source = str(0)
        self.HResult = int(0)
class GameSaveException:

    def __init__(self):
        self.Message = str(0)
        self.Data = None
        self.InnerException = None
        self.TargetSite = None
        self.StackTrace = str(0)
        self.HelpLink = str(0)
        self.Source = str(0)
        self.HResult = int(0)
class GenericSerializersFactory:

    def __init__(self):
        pass

class IIsSafeAsHashKey:

    def __init__(self):
        pass

class LoadEventsCollector:

    def __init__(self):
        pass

class MemoryBlobWriter:

    def __init__(self):
        self.Length = None
        self.BaseStream = None
        self.DelayedSerializationsCount = int(0)
class PrintingStream:

    def __init__(self):
        self.CanRead = False
        self.CanSeek = False
        self.CanWrite = False
        self.Length = None
        self.Position = None
        self.CanTimeout = False
        self.ReadTimeout = int(0)
        self.WriteTimeout = int(0)
class ReflectionUtils:

    def __init__(self):
        pass

class UniqueId:

    def __init__(self):
        self.IsObjectId = False
        self.IsTypeId = False
