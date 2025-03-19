class AnimalQuickTradeHandler:

    def __init__(self):
        self.MessageOnDelivery = None
        self.DescriptionOfTrade = None
class QuickTradeCmd:

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
class QuickTradeProvider:

    def __init__(self):
        self.DescriptionOfTrade = None
        self.MessageOnDelivery = None
        self.HasPriceIncreased = False
        self.IsSoldOut = False
        self.CurrentStep = int(0)
class QuickTradePairProto:

    def __init__(self):
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class IVirtualProductQuickTradeHandler:

    def __init__(self):
        self.MessageOnDelivery = None
        self.DescriptionOfTrade = None
