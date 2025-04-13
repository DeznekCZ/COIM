
class BackgroundTaskRunner:
    def __init__(self):
        self.IsRunning = False
        self.WasOverTime = False
        self.LastOvertimeDuration = None
        self.LastWorkDuration = None

class GameLoopEvents:
    def __init__(self):
        self.CurrentState = None
        self.LastState = None
        self.GameWasLoaded = False
        self.SaveVersion = None
        self.IsTerminated = False
        self.SyncUpdateStart = None
        self.SyncUpdate = None
        self.SyncUpdateEnd = None
        self.InputUpdate = None
        self.InputUpdateEnd = None
        self.RenderUpdate = None
        self.RenderUpdateEnd = None
        self.Terminate = None
        self.OnProjectChanged = None

class INeedsSimUpdatesForInit:
    def __init__(self):
        self.NeedsMoreSimUpdates = False
        self.FailedInit = False
        self.FailedInitMessage = ""

class GameLoopState:
    None = None
    NewGameCreated = None
    NewGameInitialized = None
    InitState = None
    RendererInitState = None
    SyncUpdateStart = None
    SyncUpdate = None
    InputUpdate = None
    InputUpdateEnd = None
    RenderUpdate = None
    RenderUpdateEnd = None
    Terminate = None
    SyncUpdateEnd = None
    def __init__(self):
        self.value__ = 0

class GameRunnerException:
    def __init__(self):
        self.Message = ""
        self.Data = None
        self.InnerException = None
        self.TargetSite = None
        self.StackTrace = ""
        self.HelpLink = ""
        self.Source = ""
        self.HResult = 0

class IGameRunnerConfig:
    def __init__(self):
        self.DisableSimulationBackgroundThread = False

class IBackgroundTask:
    def __init__(self):
        pass


class IGameLoopEvents:
    def __init__(self):
        self.CurrentState = None
        self.LastState = None
        self.GameWasLoaded = False
        self.SaveVersion = None
        self.IsTerminated = False
        self.SyncUpdateStart = None
        self.SyncUpdate = None
        self.SyncUpdateEnd = None
        self.InputUpdate = None
        self.InputUpdateEnd = None
        self.RenderUpdate = None
        self.RenderUpdateEnd = None
        self.Terminate = None
        self.OnProjectChanged = None

class INewGameCreatedEvents:
    def __init__(self):
        pass


class IGameLoopEventsExtensions:
    def __init__(self):
        pass

