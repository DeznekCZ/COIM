class ResearchNodeProto:
    DEFAULT_COST_FN = None
    DEFAULT_DESC_FN = None

    def __init__(self):
        self.Id = None
        self.IsUnlockedFromStart = False
        self.Parents = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
class ID:

    def __init__(self):
        pass

class CostPerLevelFunc:

    def __init__(self):
        self.Method = None
        self.Target = None
class DescPerTimesDoneFunc:

    def __init__(self):
        self.Method = None
        self.Target = None
class Gfx:
    Empty = None

    def __init__(self):
        self.Icons = None
class ResearchCheatFinishCmd:

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
class ResearchStartCmd:

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
class ResearchQueueDequeueCmd:

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
class ResearchStopCmd:

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
class ResearchManager:

    def __init__(self):
        self.AllNodes = None
        from Mafi import Option
        self.CurrentResearch = Option()
        self.OptimalSteps = int(0)
        self.HasActiveLab = False
        self.WasLabEverBuilt = False
        self.ResearchedNodes = None
        self.ResearchQueue = None
class ResearchNodeState:
    NotResearched = None
    Researched = None
    InProgress = None

    def __init__(self):
        pass

class ResearchNode:

    def __init__(self):
        self.RemainingSteps = None
        self.ScienceCost = None
        self.BaseScienceCost = None
        self.ScienceCostLocStr = None
        self.Proto = None
        self.StepsDone = None
        self.State = None
        self.IsLockedByCondition = False
        self.Children = None
        self.Parents = None
        self.AnyParentCanUnlock = False
        self.Units = None
        self.IsLocked = False
        from Mafi import Option
        self.LabRequired = Option()
        self.ProgressInPerc = None
        self.GridPosition = None
        self.CanBeEnqueued = False
        self.CanBeEnqueuedDirect = False
        self.CanBeDequeued = False
        self.IndexInQueue = int(0)
        self.RequiresSpacePoints = False
class InfoForUi:

    def __init__(self):
        self.IsInQueue = False
        self.IsLocked = False
class IResearchNodeFriend:

    def __init__(self):
        self.Parents = None
class ResearchNodeProtoBuilder:

    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None
class State:

    def __init__(self):
        self.Units = None
class ResearchNodeProtoBuilderExtensions:

    def __init__(self):
        pass

class TechnologyProto:

    def __init__(self):
        self.IconPath = str(0)
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
class Gfx:
    Empty = None

    def __init__(self):
        self.IconPath = str(0)
class UnlockingConditionGlobalStats:
    LIFETIME_PRODUCTION = None

    def __init__(self):
        pass

class Manager:

    def __init__(self):
        pass

class UnlockingConditionProtoRequired:

    def __init__(self):
        pass

class Manager:

    def __init__(self):
        pass

class UnlockingConditionSpaceStation:

    def __init__(self):
        self.IsSatisfied = False
class Manager:

    def __init__(self):
        pass

class IResearchNodeUnlockingCondition:

    def __init__(self):
        pass

class IResearchUnlockingConditionManager:

    def __init__(self):
        pass

