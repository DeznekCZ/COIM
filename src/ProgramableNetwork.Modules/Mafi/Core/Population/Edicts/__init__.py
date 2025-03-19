class Edict:

    def __init__(self):
        self.IsEnabled = False
        self.IsActive = False
        self.LastReasonForNotBeingActive = str(0)
class EdictEnableCheckResult:

    def __init__(self):
        pass

class EdictCategoryProto:

    def __init__(self):
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class EdictProto:

    def __init__(self):
        self.IconPath = str(0)
        from Mafi import Option
        self.NextTier = Option()
        self.IsAdvanced = False
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class Gfx:

    def __init__(self):
        self.IconPath = str(0)
class EdictsManager:

    def __init__(self):
        pass

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
        self.ErrorMessage = str(0)
class EdictWithPropertiesProto:

    def __init__(self):
        self.IconPath = str(0)
        from Mafi import Option
        self.NextTier = Option()
        self.IsAdvanced = False
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class FoodConsumptionEdict:

    def __init__(self):
        self.IsEnabled = False
        self.IsActive = False
        self.LastReasonForNotBeingActive = str(0)
class FoodConsumptionEdictProto:

    def __init__(self):
        self.IconPath = str(0)
        from Mafi import Option
        self.NextTier = Option()
        self.IsAdvanced = False
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class PopsBoostEdict:

    def __init__(self):
        self.IsEnabled = False
        self.IsActive = False
        self.LastReasonForNotBeingActive = str(0)
class PopsBoostEdictProto:

    def __init__(self):
        self.IconPath = str(0)
        from Mafi import Option
        self.NextTier = Option()
        self.IsAdvanced = False
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class PopsEvictionEdict:

    def __init__(self):
        self.IsEnabled = False
        self.IsActive = False
        self.LastReasonForNotBeingActive = str(0)
class PopsEvictionEdictProto:

    def __init__(self):
        self.IconPath = str(0)
        from Mafi import Option
        self.NextTier = Option()
        self.IsAdvanced = False
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class PopsGrowthPauseEdictProto:

    def __init__(self):
        self.IconPath = str(0)
        from Mafi import Option
        self.NextTier = Option()
        self.IsAdvanced = False
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class PopsQuarantineEdict:

    def __init__(self):
        self.IsEnabled = False
        self.IsActive = False
        self.LastReasonForNotBeingActive = str(0)
class PopsQuarantineEdictProto:

    def __init__(self):
        self.IconPath = str(0)
        from Mafi import Option
        self.NextTier = Option()
        self.IsAdvanced = False
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class PopulationGrowthPauseEdict:

    def __init__(self):
        self.IsEnabled = False
        self.IsActive = False
        self.LastReasonForNotBeingActive = str(0)
class PropertiesApplierEdict:

    def __init__(self):
        self.IsEnabled = False
        self.IsActive = False
        self.LastReasonForNotBeingActive = str(0)
