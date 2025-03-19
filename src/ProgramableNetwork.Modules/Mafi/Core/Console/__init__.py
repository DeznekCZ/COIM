class ConsoleCommandAttribute:

    def __init__(self):
        self.TypeId = None
class GameCommand:

    def __init__(self):
        pass

class GameCommandsExecutor:

    def __init__(self):
        self.Commands = None
class GameCommandResult:

    def __init__(self):
        pass

class GameConsole:

    def __init__(self):
        pass

class ConsoleMessage:

    def __init__(self):
        pass

class GameConsoleCmd:

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
class GameConsoleCommandsExecutor:

    def __init__(self):
        pass

class IGameConsole:

    def __init__(self):
        pass

class IGameConsoleExtension:

    def __init__(self):
        pass

