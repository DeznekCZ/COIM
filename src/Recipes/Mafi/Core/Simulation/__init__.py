
class Calendar:
    SIM_STEPS_PER_DAY = 0
    TICKS_PER_MONTH = 0
    TICKS_PER_YEAR = 0
    def __init__(self):
        self.NewYear = None
        self.NewYearEnd = None
        self.NewMonthStart = None
        self.NewMonth = None
        self.NewMonthEnd = None
        self.NewDay = None
        self.NewDayEnd = None
        self.CurrentDate = None
        self.RealTime = None

class GameOverManager:
    def __init__(self):
        self.IsGameOver = False
        self.OnGameOver = None

class GameSpeedChangeCmd:
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
        self.NewSpeedMultiplier = 0

class GameVictoryManager:
    def __init__(self):
        self.IsGameVictorious = False
        self.OnGameVictory = None

class SetSimPauseStateCmd:
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
        self.IsPaused = False

class SimLoopEvents:
    def __init__(self):
        self.CurrentStep = None
        self.StepsSinceLoad = None
        self.CurrentState = None
        self.IsSimPaused = False
        self.SimSpeedMult = 0
        self.IsInSimLoop = False
        self.UpdateBeforeCmdProc = None
        self.UpdateAfterCmdProc = None
        self.UpdateAfterSync = None
        self.UpdateStart = None
        self.ParallelUpdateStart = None
        self.Update = None
        self.ParallelUpdateEnd = None
        self.UpdateEnd = None
        self.ReadGameStateFrequent = None
        self.UpdateEndForUi = None
        self.Sync = None
        self.BeforeSave = None
        self.CurrentSimStep = 0
        self.IsTerminated = False

class IGameOverManager:
    def __init__(self):
        self.IsGameOver = False
        self.OnGameOver = None

class ICalendar:
    def __init__(self):
        self.NewYear = None
        self.NewYearEnd = None
        self.NewMonthStart = None
        self.NewMonth = None
        self.NewMonthEnd = None
        self.NewDay = None
        self.NewDayEnd = None
        self.CurrentDate = None
        self.RealTime = None

class ISimLoopEvents:
    def __init__(self):
        self.CurrentStep = None
        self.CurrentState = None
        self.IsSimPaused = False
        self.SimSpeedMult = 0
        self.IsInSimLoop = False
        self.UpdateBeforeCmdProc = None
        self.UpdateAfterCmdProc = None
        self.UpdateAfterSync = None
        self.UpdateStart = None
        self.ParallelUpdateStart = None
        self.Update = None
        self.ParallelUpdateEnd = None
        self.UpdateEnd = None
        self.UpdateEndForUi = None
        self.ReadGameStateFrequent = None
        self.Sync = None
        self.BeforeSave = None

class SimLoopState:
    None = None
    UpdateAfterSync = None
    CommandsProcessing = None
    UpdateStart = None
    Update = None
    UpdateEnd = None
    UpdateEndForUi = None
    Sync = None
    Terminated = None
    ReadGameStateFrequent = None
    ParallelUpdateStart = None
    ParallelUpdateEnd = None
    def __init__(self):
        self.value__ = 0
