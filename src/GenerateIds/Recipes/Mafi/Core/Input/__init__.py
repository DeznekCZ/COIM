
class IInputScheduler:
    def __init__(self):
        self.OnCommandProcessed = None
        self.ProcessedCommandsInThisSessionAffectingSave = 0

class IInputCommandsProcessor:
    def __init__(self):
        pass


class IInputCommand:
    def __init__(self):
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.HasError = False
        self.ErrorMessage = ""
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
        self.ErrorMessage = ""

class InputScheduler:
    def __init__(self):
        self.ProcessedCommandsInThisSessionAffectingSave = 0
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
        self.ErrorMessage = ""
        from Mafi.Core.Prototypes import Proto
        self.MaterialId = Proto.ID()

        self.Selection = None
        self.Height = None

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
        self.ErrorMessage = ""
        self.Selection = None
        self.TargetHeight = None

class TilesRectSelection:
    def __init__(self):
        self.Area = 0
        self.VerticesCount = 0
        self.Center = None
        self.Origin = None
        self.Size = None

    class Enumerator:
        def __init__(self):
            self.Current = None
