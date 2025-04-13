
class ConsoleCommandAttribute:
    def __init__(self):
        self.TypeId = None
        self.InvokeOnMainThread = False
        self.InvokeDuringSync = False
        self.Documentation = ""
        from Mafi import Option
        self.CustomCommandName = Option()

class GameCommand:
    def __init__(self):
        self.CanonicalName = ""
        self.Documentation = ""
        self.ParametersDocStr = ""
        self.Target = None
        self.Method = None
        self.Parameters = None
        self.MandatoryParametersCount = 0
        self.InvokeOnMainThread = False
        self.InvokeDuringSync = False

class GameCommandsExecutor:
    def __init__(self):
        self.Commands = None

class GameCommandResult:
    def __init__(self):
        from Mafi import Option
        self.Result = Option()
        self.ErrorMessage = Option()

class GameConsole:
    def __init__(self):
        pass


    class ConsoleMessage:
        def __init__(self):
            self.Message = ""
            self.Color = None

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
        self.ErrorMessage = ""
        self.Command = ""

class GameConsoleCommandsExecutor:
    def __init__(self):
        self.Executor = None

class IGameConsole:
    def __init__(self):
        pass


class IGameConsoleExtension:
    def __init__(self):
        pass

