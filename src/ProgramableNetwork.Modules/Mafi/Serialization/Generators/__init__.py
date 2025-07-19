class GeneratorContext:

    def __init__(self):
        pass

class MembersGenerator:
    TYPE_TO_WRITE_METHOD_NAME = None
    TYPE_TO_READ_METHOD_NAME = None
    SAVE_VERSION_NAMES = None

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
        self.NeedsStaticSaveVersion = False
class SerializerGenerator:
    GENERATED_FILE_EXT = None
    SERIALIZE_METHOD_NAME = None
    DESERIALIZE_METHOD_NAME = None

    def __init__(self):
        pass

class GenSpecContext:

    def __init__(self):
        pass

class SerializerGeneratorResult:

    def __init__(self):
        pass

class TypeSerializationSpec:
    CTOR_ARG_PREFIX = None
    OBJ_NAME = None

    def __init__(self):
        from Mafi import Option
        self.SerializedDueToDerivedClass = Option()
        self.HasBaseTypeWithSomethingToSerialize = False
