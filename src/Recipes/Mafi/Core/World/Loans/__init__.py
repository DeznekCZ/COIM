
class AcceptLoanCmd:
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
        from Mafi.Core.Products import ProductProto
        self.ProductId = ProductProto.ID()

        self.ToBorrow = None
        self.YearsTotal = 0

class ActiveLoan:
    def __init__(self):
        self.LeftToPay = None
        self.AnnualPayment = None
        self.InterestRate = None
        self.YearsLeft = 0
        self.YearsPaid = 0
        self.NextPaymentDate = None
        self.InterestPaid = None
        self.InterestAddedToDebt = None
        self.RemainingInterest = None
        self.InterestRateDisabled = False
        self.BalanceLog = None
        self.Id = None
        self.Product = None
        self.RatioToMax = None
        self.PaymentDelayedNotif = None
        self.StartingDept = None
        self.StartDate = None
        self.BufferPriority = 0

    class BalanceLogEntry:
        def __init__(self):
            self.Diff = None
            self.Date = None

class LoansManager:
    YEARS_AVAILABLE = None
    YEARS_AVAILABLE_DEFAULT_INDEX = 0
    MAX_PAYMENT_DELAY = None
    PAYMENT_FREQUENCY = None
    PAYMENT_BUFFER_OPENS_BEFORE = None
    from Mafi import Fix32
    SCORE_PENALTY_ON_MISSED_PAYMENT = Fix32()
    SCORE_BONUS_ON_PAYMENT = Fix32()
    SCORE_AUTO_RESTORE = Fix32()
    def __init__(self):
        from Mafi import Fix32
        self.CreditScore = Fix32()
        self.InterestRate = None
        self.LoanLimitMultiplier = Fix32()
        self.MaxActiveLoans = 0
        self.Fee = None
        self.ActiveLoans = None

class MakeLoanOverpaymentCmd:
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
        from Mafi.Core.Products import ProductProto
        self.ProductId = ProductProto.ID()

        self.LoanId = None
        self.ToPay = None

class SetLoanBufferPriorityCmd:
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
        self.LoanId = None
        self.Priority = 0

class LoansDifficultyParams:
    from Mafi import Fix32
    StartingScore = Fix32()
    MinScore = Fix32()
    MaxScore = Fix32()
    MaxAnnualPaymentToProductionRatio = None
    def __init__(self):
        pass


class LoansDifficulty:
    Easy = None
    Medium = None
    Hard = None
    def __init__(self):
        self.value__ = 0
