
class ContractsManager:
    CAP_MULTIPLIER_LARGE_SHIPS = None
    def __init__(self):
        self.ActiveContracts = None
        self.ProfitMultiplier = None

    class ShipDepartureCheckResult:
        Ok = None
        NotEnoughUpoints = None
        WaitingForCargo = None
        def __init__(self):
            self.value__ = 0

    class EstablishCheckResult:
        Ok = None
        AlreadyActive = None
        VillageLevelLow = None
        ProductLocked = None
        LacksUpoints = None
        def __init__(self):
            self.value__ = 0

    class CancelCheckResult:
        Ok = None
        NotActive = None
        HasShipsAssigned = None
        def __init__(self):
            self.value__ = 0

class ToggleContractCmd:
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
        self.ContractId = Proto.ID()


class ContractProto:
    def __init__(self):
        self.UpointsPerMonth = None
        self.UpointsPer100ProductsBought = None
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
        self.ProductToBuy = None
        self.ProductToPayWith = None
        self.QuantityToPayWith = None
        self.AllProducts = None
        self.UpointsPerMonthBase = None
        self.UpointsPer100ProductsBoughtBase = None
        self.UpointsToEstablish = None
        self.MinReputationRequired = 0
        self.IsPhantom = False
