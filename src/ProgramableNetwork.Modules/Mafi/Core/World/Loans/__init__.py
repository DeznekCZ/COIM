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

    def __init__(self):
        pass

class LoansDifficulty:

    def __init__(self):
        pass

