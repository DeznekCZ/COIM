class RelGameDateTimer:

    def __init__(self):
        self.Remaining = None
        self.IsFinished = False
        self.IsNotFinished = False
class ResolvedDependenciesDuringSimVerifCmd:

    def __init__(self):
        self.IsVerificationCmd = False
        self.AffectsSaveState = False
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.Result = False
        self.HasError = False
        self.ErrorMessage = str(0)
class SetInstaBuildCmd:

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
class TickTimer:

    def __init__(self):
        self.Ticks = None
        self.StartedAtTicks = None
        self.IsFinished = False
        self.IsNotFinished = False
        self.PercentFinished = None
class TimelapseData:

    def __init__(self):
        self.PreviousInvokeStep = None
        from Mafi import Option
        self.PreviousCaptureError = Option()
        self.NextCaptureStep = None
        self.CapturedCount = int(0)
class TimelapseManager:

    def __init__(self):
        self.Data = None
class WaitHelper:

    def __init__(self):
        pass

class XorRsr128PlusGenerator:

    def __init__(self):
        pass

class BitmapFont5Px:

    def __init__(self):
        pass

class ChangelogUtils:

    def __init__(self):
        pass

class ChangelogEntry:

    def __init__(self):
        pass

class ChangelogSubEntry:

    def __init__(self):
        pass

class CoreConsoleCommands:

    def __init__(self):
        pass

class DelayedEventExtensions:

    def __init__(self):
        pass

class IInstaBuildManager:

    def __init__(self):
        self.IsInstaBuildEnabled = False
class IInstaBuildConfig:

    def __init__(self):
        self.IsInstaBuildEnabled = False
class LaunchUtils:

    def __init__(self):
        pass

class SerializationUtils:

    def __init__(self):
        pass

class UiSearchUtils:

    def __init__(self):
        pass

