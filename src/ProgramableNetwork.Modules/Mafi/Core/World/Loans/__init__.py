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
        self.ErrorMessage = str(0)
class ActiveLoan:

    def __init__(self):
        self.LeftToPay = None
        self.AnnualPayment = None
        self.InterestRate = None
        self.YearsLeft = int(0)
        self.YearsPaid = int(0)
        self.NextPaymentDate = None
        self.InterestPaid = None
        self.InterestAddedToDebt = None
        self.RemainingInterest = None
        self.InterestRateDisabled = False
        self.BalanceLog = None
class BalanceLogEntry:

    def __init__(self):
        pass

class LoansManager:
    YEARS_AVAILABLE = None
    YEARS_AVAILABLE_DEFAULT_INDEX = None
    MAX_PAYMENT_DELAY = None
    PAYMENT_FREQUENCY = None
    PAYMENT_BUFFER_OPENS_BEFORE = None
    SCORE_PENALTY_ON_MISSED_PAYMENT = None
    SCORE_BONUS_ON_PAYMENT = None
    SCORE_AUTO_RESTORE = None
    MIN_LOAN = None

    def __init__(self):
        from Mafi import Fix32
        self.CreditScore = Fix32()
        self.InterestRate = None
        from Mafi import Fix32
        self.LoanLimitMultiplier = Fix32()
        self.MaxActiveLoans = int(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
class LoansDifficultyParams:
    StartingScore = None
    MinScore = None
    MaxScore = None
    MaxAnnualPaymentToProductionRatio = None

    def __init__(self):
        pass

class LoansDifficulty:
    Easy = None
    Medium = None
    Hard = None

    def __init__(self):
        pass

