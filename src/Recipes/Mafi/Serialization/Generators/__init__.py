
class GeneratorContext:
    def __init__(self):
        self.AllTypes = None
        self.NonSerializedGlobalDeps = None
        self.TypeSerializationSpecs = None
        self.GlobalLog = None

class MembersGenerator:
    TYPE_TO_WRITE_METHOD_NAME = None
    TYPE_TO_READ_METHOD_NAME = None
    def __init__(self):
        pass


class MemberWrapper:
    def __init__(self):
        self.NewInSaveVersion = None
        from Mafi import Option
        self.DefaultValueFromResolver = Option()
        self.IsCtorArg = False
        self.IsLoadedAsGlobalDep = False
        self.IsDirectCallSerializationDisabled = False
        self.ShouldAssignToObj = False
        self.IsSerialized = False
        self.Owner = None
        self.IsField = False
        self.Name = ""
        self.Type = None
        self.LoadViaReflection = False
        self.NewInstanceOnLoad = Option()
        self.DeprecatedInSaveVersion = None
        self.RemovedInSaveVersion = None
        self.NameForSorting = ""
        self.CustomValueWhenNotLoaded = Option()
        self.GlobalDepTypeOverride = Option()
        self.SerializeUsingNonVariableEncoding = False
        self.SerializeNullAsEmptyArray = False
        self.DeserializeMethodName = Option()

class SerializerGenerator:
    GENERATED_FILE_EXT = ""
    SERIALIZE_METHOD_NAME = ""
    DESERIALIZE_METHOD_NAME = ""
    def __init__(self):
        pass


    class GenSpecContext:
        def __init__(self):
            self.NonSerializedGlobalDeps = None
            self.SpecialSerializedTypes = None
            from Mafi import Option
            self.GenSerializerAttr = Option()

class SerializerGeneratorResult:
    def __init__(self):
        self.FilePath = ""
        self.NewContents = ""

class TypeSerializationSpec:
    CTOR_ARG_PREFIX = ""
    OBJ_NAME = ""
    def __init__(self):
        from Mafi import Option
        self.SerializedDueToDerivedClass = Option()
        self.HasBaseTypeWithSomethingToSerialize = False
        self.HasSomethingToSerialize = False
        self.Type = None
        self.ClassName = ""
        self.FileName = ""
        self.Namespace = ""
        self.Usings = None
        self.Members = None
        self.InitCalls = None
        self.CtorArgs = None
        self.NonCtorArgs = None
        self.SerializeAsSingleton = Option()
        self.HasCustomDataSerialization = False
        self.TypeNewInSaveVersion = None
