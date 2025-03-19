class GeneratorContext:

    def __init__(self):
        pass

class MembersGenerator:

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
class SerializerGenerator:

    def __init__(self):
        pass

class GenSpecContext:

    def __init__(self):
        pass

class SerializerGeneratorResult:

    def __init__(self):
        pass

class TypeSerializationSpec:

    def __init__(self):
        from Mafi import Option
        self.SerializedDueToDerivedClass = Option()
        self.HasBaseTypeWithSomethingToSerialize = False
        self.HasSomethingToSerialize = False
