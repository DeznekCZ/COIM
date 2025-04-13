
class ResearchNodeProto:
    def __init__(self):
        from Mafi.Core.Research import ResearchNodeProto
        self.Id = ResearchNodeProto.ID()

        self.IsUnlockedFromStart = False
        self.Parents = None
        self.Difficulty = 0
        self.TotalStepsRequired = 0
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.Units = None
        self.UnlockingConditions = None
        self.GridPosition = None
        self.AnyParentCanUnlock = False
        self.Graphics = None
        self.ResolvedDescription = None
        self.IsPhantom = False
        self.IsInitialized = False

    class ID:
        def __init__(self):
            self.Value = ""

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
        self.ErrorMessage = ""

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
        self.ErrorMessage = ""
        from Mafi.Core.Research import ResearchNodeProto
        self.NodeId = ResearchNodeProto.ID()


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
        self.ErrorMessage = ""
        from Mafi.Core.Research import ResearchNodeProto
        self.NodeId = ResearchNodeProto.ID()

        self.IsEnqueue = False

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
        self.ErrorMessage = ""

class ResearchManager:
    def __init__(self):
        self.AllNodes = None
        from Mafi import Option
        self.CurrentResearch = Option()
        self.OptimalSteps = 0
        self.HasActiveLab = False
        self.WasLabEverBuilt = False
        self.ResearchedNodes = None
        self.ResearchQueue = None

class ResearchNodeState:
    NotResearched = None
    Researched = None
    InProgress = None
    def __init__(self):
        self.value__ = 0

class ResearchNode:
    def __init__(self):
        from Mafi import Fix32
        self.RemainingSteps = Fix32()
        self.Proto = None
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
        self.IndexInQueue = 0

    class InfoForUi:
        def __init__(self):
            self.IsInQueue = False
            self.IsLocked = False
            self.State = None
            self.IndexInQueue = 0
            self.CanBeEnqueued = False
            self.CanBeEnqueuedDirect = False
            self.CanBeDequeued = False
            self.IsLockedByCondition = False
            self.IsLockedByParents = False

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
            self.Builder = None

class ResearchCostsTpl:
    Build = None
    UnlockedFromStart = None
    def __init__(self):
        self.Difficulty = 0

    class Builder:
        def __init__(self):
            pass


class ResearchCostsAttribute:
    def __init__(self):
        self.TypeId = None
        self.Difficulty = 0

class ResearchNodeProtoBuilderExtensions:
    def __init__(self):
        pass


class TechnologyProto:
    def __init__(self):
        self.IconPath = ""
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.Graphics = None
        self.IsPhantom = False
        self.IsInitialized = False

    class Gfx:
        Empty = None
        def __init__(self):
            self.IconPath = ""

class UnlockingConditionGlobalStats:
    LIFETIME_PRODUCTION = None
    def __init__(self):
        self.ProductToTrack = None
        self.QuantityRequired = None
        self.Tooltip = None
        self.CurrentQuantity = None

    class Manager:
        def __init__(self):
            pass


class UnlockingConditionProtoRequired:
    def __init__(self):
        self.ProtoRequired = None

    class Manager:
        def __init__(self):
            pass


class IResearchNodeUnlockingCondition:
    def __init__(self):
        pass


class IResearchUnlockingConditionManager:
    def __init__(self):
        pass

