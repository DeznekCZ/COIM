
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
        self.DurationMultiplier = None
        self.IsBoosted = False

class IRecipeForUi:
    def __init__(self):
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.AllUserVisibleInputs = None
        self.AllUserVisibleOutputs = None
        self.Duration = None

class RecipeForUiData:
    def __init__(self):
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.AllUserVisibleInputs = None
        self.AllUserVisibleOutputs = None
        self.Duration = None

class RecipeInput:
    def __init__(self):
        self.Product = None
        self.Quantity = None
        self.Ports = None
        self.HideInUi = False
        self.IsPollution = False

class RecipeOutput:
    def __init__(self):
        self.TriggerAtStart = False
        self.Product = None
        self.Quantity = None
        self.Ports = None
        self.HideInUi = False
        self.IsPollution = False

class RecipeProduct:
    def __init__(self):
        self.Product = None
        self.Quantity = None
        self.Ports = None
        self.HideInUi = False
        self.IsPollution = False

class RecipeProto:
    def __init__(self):
        self.AllUserVisibleInputs = None
        self.AllUserVisibleOutputs = None
        self.Duration = None
        from Mafi.Core.Factory.Recipes import RecipeProto
        self.Id = RecipeProto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.AllInputs = None
        self.AllOutputs = None
        self.OutputsAtEnd = None
        self.OutputsAtStart = None
        self.MinUtilization = None
        self.QuantitiesGcd = 0
        self.ProductsDestroyReason = None
        self.DisableSourceProductsConversionLoss = False
        self.IsPhantom = False
        self.IsInitialized = False

    class ID:
        def __init__(self):
            self.Value = ""

class RecipeProtoBuilder:
    ANY_COMPATIBLE_PORT = ""
    VIRTUAL_PORT = ""
    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None

    class State:
        def __init__(self):
            self.ProtosDb = None
            self.RecipeDuration = None
            self.Builder = None
