
class Edict:
    def __init__(self):
        self.IsEnabled = False
        self.IsActive = False
        self.LastReasonForNotBeingActive = ""
        self.Prototype = None

    class EdictEnableCheckResult:
        def __init__(self):
            self.CanBeEnabled = False
            self.Explanation = ""

class EdictCategoryProto:
    def __init__(self):
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.IsPhantom = False
        self.IsInitialized = False

class EdictProto:
    ROMAN_NUMERALS = None
    def __init__(self):
        self.IconPath = ""
        from Mafi import Option
        self.NextTier = Option()
        self.IsAdvanced = False
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.Category = None
        self.MonthlyUpointsCost = None
        self.Implementation = None
        self.Graphics = None
        self.PreviousTier = Option()
        self.IsGeneratingUnity = False
        self.IsPhantom = False
        self.IsInitialized = False

    class Gfx:
        Empty = None
        def __init__(self):
            self.IconPath = ""

class EdictsManager:
    def __init__(self):
        self.AllEdicts = None

class ToggleEdictEnabledCmd:
    def __init__(self):
        self.AffectsSaveState = False
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.IsVerificationCmd = False
        self.Result = False
        self.HasError = False
        self.ErrorMessage = ""
        from Mafi.Core.Prototypes import Proto
        self.EdictProtoId = Proto.ID()


class EdictWithPropertiesProto:
    def __init__(self):
        self.IconPath = ""
        from Mafi import Option
        self.NextTier = Option()
        self.IsAdvanced = False
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.PropertiesToApply = None
        self.PropertyGroup = ""
        self.Category = None
        self.MonthlyUpointsCost = None
        self.Implementation = None
        self.Graphics = None
        self.PreviousTier = Option()
        self.IsGeneratingUnity = False
        self.IsPhantom = False
        self.IsInitialized = False

class FoodConsumptionEdict:
    def __init__(self):
        self.IsEnabled = False
        self.IsActive = False
        self.LastReasonForNotBeingActive = ""
        self.Prototype = None

class FoodConsumptionEdictProto:
    def __init__(self):
        self.IconPath = ""
        from Mafi import Option
        self.NextTier = Option()
        self.IsAdvanced = False
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.ConsumptionDiff = None
        self.Category = None
        self.MonthlyUpointsCost = None
        self.Implementation = None
        self.Graphics = None
        self.PreviousTier = Option()
        self.IsGeneratingUnity = False
        self.IsPhantom = False
        self.IsInitialized = False

class PopsBoostEdict:
    def __init__(self):
        self.IsEnabled = False
        self.IsActive = False
        self.LastReasonForNotBeingActive = ""
        self.Prototype = None

class PopsBoostEdictProto:
    def __init__(self):
        self.IconPath = ""
        from Mafi import Option
        self.NextTier = Option()
        self.IsAdvanced = False
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.PopsGrowthBoost = None
        self.Category = None
        self.MonthlyUpointsCost = None
        self.Implementation = None
        self.Graphics = None
        self.PreviousTier = Option()
        self.IsGeneratingUnity = False
        self.IsPhantom = False
        self.IsInitialized = False

class PopsEvictionEdict:
    def __init__(self):
        self.IsEnabled = False
        self.IsActive = False
        self.LastReasonForNotBeingActive = ""
        self.Prototype = None

class PopsEvictionEdictProto:
    def __init__(self):
        self.IconPath = ""
        from Mafi import Option
        self.NextTier = Option()
        self.IsAdvanced = False
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.MonthlyPopsEvictionRate = None
        self.Category = None
        self.MonthlyUpointsCost = None
        self.Implementation = None
        self.Graphics = None
        self.PreviousTier = Option()
        self.IsGeneratingUnity = False
        self.IsPhantom = False
        self.IsInitialized = False

class PopsGrowthPauseEdictProto:
    def __init__(self):
        self.IconPath = ""
        from Mafi import Option
        self.NextTier = Option()
        self.IsAdvanced = False
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.Category = None
        self.MonthlyUpointsCost = None
        self.Implementation = None
        self.Graphics = None
        self.PreviousTier = Option()
        self.IsGeneratingUnity = False
        self.IsPhantom = False
        self.IsInitialized = False

class PopsQuarantineEdict:
    def __init__(self):
        self.IsEnabled = False
        self.IsActive = False
        self.LastReasonForNotBeingActive = ""
        self.Prototype = None

class PopsQuarantineEdictProto:
    def __init__(self):
        self.IconPath = ""
        from Mafi import Option
        self.NextTier = Option()
        self.IsAdvanced = False
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.DiseaseReduction = None
        self.WorkersToWithhold = None
        self.Category = None
        self.MonthlyUpointsCost = None
        self.Implementation = None
        self.Graphics = None
        self.PreviousTier = Option()
        self.IsGeneratingUnity = False
        self.IsPhantom = False
        self.IsInitialized = False

class PopulationGrowthPauseEdict:
    def __init__(self):
        self.IsEnabled = False
        self.IsActive = False
        self.LastReasonForNotBeingActive = ""
        self.Prototype = None

class PropertiesApplierEdict:
    def __init__(self):
        self.IsEnabled = False
        self.IsActive = False
        self.LastReasonForNotBeingActive = ""
        self.Prototype = None
