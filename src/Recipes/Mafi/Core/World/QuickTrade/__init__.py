
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
        self.ErrorMessage = ""
        from Mafi.Core.Prototypes import Proto
        self.TradeId = Proto.ID()


class QuickTradeProvider:
    def __init__(self):
        self.DescriptionOfTrade = None
        self.MessageOnDelivery = None
        self.HasPriceIncreased = False
        self.IsSoldOut = False
        self.CurrentStep = 0
        self.Prototype = None
        self.UpointsCost = None

class QuickTradePairProto:
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
        self.ProductToBuy = None
        self.ProductToPayWith = None
        self.UpointsPerTrade = None
        self.MaxSteps = 0
        self.CooldownPerStep = None
        self.TradesPerStep = 0
        self.CostMultiplierPerStep = None
        self.UnityMultiplierPerStep = None
        self.MinReputationRequired = 0
        self.IgnoreTradeMultipliers = False
        self.IsPhantom = False
        self.IsInitialized = False

class IVirtualProductQuickTradeHandler:
    def __init__(self):
        self.MessageOnDelivery = None
        self.DescriptionOfTrade = None
