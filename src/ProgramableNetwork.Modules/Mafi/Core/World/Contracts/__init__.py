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
        pass

class EstablishCheckResult:
    Ok = None
    AlreadyActive = None
    VillageLevelLow = None
    ProductLocked = None
    LacksUpoints = None

    def __init__(self):
        pass

class CancelCheckResult:
    Ok = None
    NotActive = None
    HasShipsAssigned = None

    def __init__(self):
        pass

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
        self.ErrorMessage = str(0)
class ContractProto:

    def __init__(self):
        self.UpointsPerMonth = None
        self.UpointsPer100ProductsBought = None
        self.Id = None
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
