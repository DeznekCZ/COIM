class IRecipeExecutorForUi:

    def __init__(self):
        self.IsBoosted = False
        self.WorkedThisTick = False
        self.DurationMultiplier = None
class RecipeExecutorForUiExtensions:

    def __init__(self):
        pass

class ExecutorDurationInfoUi:

    def __init__(self):
        pass

class IRecipeForUi:

    def __init__(self):
        self.Id = None
        self.AllUserVisibleInputs = None
        self.AllUserVisibleOutputs = None
        self.Duration = None
class RecipeForUiData:

    def __init__(self):
        self.Id = None
        self.AllUserVisibleInputs = None
        self.AllUserVisibleOutputs = None
        self.Duration = None
class RecipeInput:

    def __init__(self):
        pass

class RecipeOutput:

    def __init__(self):
        pass

class RecipeProduct:

    def __init__(self):
        pass

class RecipeProto:

    def __init__(self):
        self.AllUserVisibleInputs = None
        self.AllUserVisibleOutputs = None
        self.Duration = None
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class ID:

    def __init__(self):
        pass

class RecipeProtoBuilder:

    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None
class State:

    def __init__(self):
        self.ProtosDb = None
        self.RecipeDuration = None
