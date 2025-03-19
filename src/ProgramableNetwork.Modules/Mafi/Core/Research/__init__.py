class ResearchNodeProto:

    def __init__(self):
        self.Id = None
        self.IsUnlockedFromStart = False
        self.Parents = None
        self.Difficulty = int(0)
        self.TotalStepsRequired = int(0)
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class ID:

    def __init__(self):
        pass

class Gfx:

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

    def __init__(self):
        pass

class ResearchNode:

    def __init__(self):
        from Mafi import Fix32
        self.RemainingSteps = Fix32()
        self.Proto = None
        from Mafi import Fix32
        self.StepsDone = Fix32()
        self.State = None
        self.IsLockedByCondition = False
        self.LockedByConditions = None
        self.Children = None
        self.Parents = None
        self.AnyParentCanUnlock = False
        self.Units = None
        self.IsLockedByParents = False
        self.IsLocked = False
        from Mafi import Option
        self.LabRequired = Option()
        self.ProgressInPerc = None
        self.GridPosition = None
        self.CanBeEnqueued = False
        self.CanBeEnqueuedDirect = False
        self.CanBeDequeued = False
        self.IndexInQueue = int(0)
class InfoForUi:

    def __init__(self):
        self.IsInQueue = False
        self.IsLocked = False
class IResearchNodeFriend:

    def __init__(self):
        pass

class ResearchNodeProtoBuilder:

    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None
class State:

    def __init__(self):
        self.Units = None
class ResearchCostsTpl:
    Build = None
    UnlockedFromStart = None

    def __init__(self):
        pass

class Builder:

    def __init__(self):
        pass

class ResearchCostsAttribute:

    def __init__(self):
        self.TypeId = None
class ResearchNodeProtoBuilderExtensions:

    def __init__(self):
        pass

class TechnologyProto:

    def __init__(self):
        self.IconPath = str(0)
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class Gfx:

    def __init__(self):
        self.IconPath = str(0)
class UnlockingConditionGlobalStats:

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

class IResearchNodeUnlockingCondition:

    def __init__(self):
        pass

class IResearchUnlockingConditionManager:

    def __init__(self):
        pass

