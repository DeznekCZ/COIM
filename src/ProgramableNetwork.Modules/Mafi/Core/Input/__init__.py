class IInputScheduler:

    def __init__(self):
        self.OnCommandProcessed = None
        self.ProcessedCommandsInThisSessionAffectingSave = int(0)
class IInputCommandsProcessor:

    def __init__(self):
        pass

class IInputCommand:

    def __init__(self):
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.HasError = False
        self.ErrorMessage = str(0)
        self.ResultSet = False
        self.AffectsSaveState = False
        self.IsVerificationCmd = False
class InputCommand:

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
class InputScheduler:

    def __init__(self):
        self.ProcessedCommandsInThisSessionAffectingSave = int(0)
        self.OnCommandProcessed = None
class TerraformerDepositCmd:

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
class TerraformerRemoveCmd:

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
class TilesRectSelection:

    def __init__(self):
        self.Area = int(0)
        self.VerticesCount = int(0)
        self.Center = None
class Enumerator:

    def __init__(self):
        self.Current = None
