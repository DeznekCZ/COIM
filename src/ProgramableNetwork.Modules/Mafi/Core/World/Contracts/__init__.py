class ContractsManager:

    def __init__(self):
        self.ActiveContracts = None
        self.ProfitMultiplier = None
class ShipDepartureCheckResult:

    def __init__(self):
        pass

class EstablishCheckResult:

    def __init__(self):
        pass

class CancelCheckResult:

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
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
