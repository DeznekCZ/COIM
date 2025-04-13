
class DoNotSaveAttribute:
    def __init__(self):
        self.TypeId = None
        self.RemovedInSaveVersion = None
        from Mafi import Option
        self.ResolveAfterLoad = Option()

class DoNotSaveCreateNewOnLoadAttribute:
    def __init__(self):
        self.TypeId = None
        self.CreateNewInstanceCode = ""
        self.RemovedInSaveVersion = None

class RenamedInVersionAttribute:
    def __init__(self):
        self.TypeId = None
        self.RenamedInSaveVersion = None
        self.OldName = ""

class GenerateSerializer:
    def __init__(self):
        self.TypeId = None
        self.CustomClassDataSerialization = False
        from Mafi import Option
        self.SerializeAsSingleton = Option()
        self.NewInVersion = None

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
        self.DeserializeMethodName = ""

class IgnoreMissingSerializer:
    def __init__(self):
        self.TypeId = None

class LoadCtorAttribute:
    def __init__(self):
        self.TypeId = None

class NewInSaveVersionAttribute:
    def __init__(self):
        self.TypeId = None
        self.Version = 0
        from Mafi import Option
        self.CustomSortingName = Option()
        self.CustomValueWhenNotLoaded = Option()
        self.DefaultValueFromResolver = Option()
        self.GlobalDepTypeOverride = Option()

class MemberRemovedInSaveVersionAttribute:
    def __init__(self):
        self.TypeId = None
        self.RemovedInVersion = 0
        self.Name = ""
        self.Type = None
        self.WasNewInVersion = None
        self.WasSerializedUsingNonVariableEncoding = False

class OnlyForSaveCompatibilityAttribute:
    def __init__(self):
        self.TypeId = None

class SerializeUsingNonVariableEncodingAttribute:
    def __init__(self):
        self.TypeId = None

class InitAfterLoadAttribute:
    def __init__(self):
        self.TypeId = None
        self.Priority = None

class InitPriority:
    ImmediatelyAfterSelfDeserialized = None
    Highest = None
    High = None
    Normal = None
    Low = None
    Lowest = None
    def __init__(self):
        self.value__ = 0

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
        self.DelayedDeserializationsCount = 0
        self.LoadedSaveVersion = 0

class BlobReaderExtensions:
    def __init__(self):
        pass


class BlobSubReader:
    def __init__(self):
        self.IsDone = False
        self.IsNotDone = False
        self.InputStream = None
        self.DelayedDeserializationsCount = 0
        self.SubStream = None
        self.LoadedSaveVersion = 0

class BlobWriter:
    def __init__(self):
        self.DelayedSerializationsCount = 0

class ISpecialSerializerFactory:
    def __init__(self):
        pass


class ISpecialSerializerFactoryCustom:
    def __init__(self):
        pass


class CorruptedSaveException:
    def __init__(self):
        self.Message = ""
        self.Data = None
        self.InnerException = None
        self.TargetSite = None
        self.StackTrace = ""
        self.HelpLink = ""
        self.Source = ""
        self.HResult = 0
        self.MessageForPlayer = None
        self.DoNotOfferBugReport = False

class IncompatibleSaveVersionException:
    def __init__(self):
        self.Message = ""
        self.Data = None
        self.InnerException = None
        self.TargetSite = None
        self.StackTrace = ""
        self.HelpLink = ""
        self.Source = ""
        self.HResult = 0
        self.SaveFileVersion = 0

class Crc32:
    def __init__(self):
        pass


class Crc32WriteStream:
    def __init__(self):
        self.CanRead = False
        self.CanSeek = False
        self.CanWrite = False
        self.Length = 0
        self.Position = 0
        self.Crc32 = None
        self.CanTimeout = False
        self.ReadTimeout = 0
        self.WriteTimeout = 0

class CSharpGen:
    def __init__(self):
        pass


class ParameterData:
    def __init__(self):
        self.ParentType = None
        self.ParameterInfo = None

class CSharpGenCtorAttribute:
    def __init__(self):
        self.TypeId = None

class GameLoadException:
    def __init__(self):
        self.Message = ""
        self.Data = None
        self.InnerException = None
        self.TargetSite = None
        self.StackTrace = ""
        self.HelpLink = ""
        self.Source = ""
        self.HResult = 0

class GameSaveException:
    def __init__(self):
        self.Message = ""
        self.Data = None
        self.InnerException = None
        self.TargetSite = None
        self.StackTrace = ""
        self.HelpLink = ""
        self.Source = ""
        self.HResult = 0

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
        self.Length = 0
        self.BaseStream = None
        self.DelayedSerializationsCount = 0

class PrintingStream:
    def __init__(self):
        self.CanRead = False
        self.CanSeek = False
        self.CanWrite = False
        self.Length = 0
        self.Position = 0
        self.CanTimeout = False
        self.ReadTimeout = 0
        self.WriteTimeout = 0

class ReflectionUtils:
    def __init__(self):
        pass


class UniqueId:
    def __init__(self):
        self.IsObjectId = False
        self.IsTypeId = False
        self.Id = 0
