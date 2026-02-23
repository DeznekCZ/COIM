
class ResearchNodeProto:
    Phantom = None
    DEFAULT_COST_FN = None
    DEFAULT_DESC_FN = None
    def __init__(self):
        from Mafi.Core.Research import ResearchNodeProto
        self.Id = ResearchNodeProto.ID()

        self.IsUnlockedFromStart = False
        self.Parents = None
        self.Units = None
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
        self.UnlockingConditions = None
        self.ResearchDuration = None
        self.GridPosition = None
        self.AnyParentCanUnlock = False
        self.Graphics = None
        self.ResolvedDescription = None
        self.SpacePointsRequiredFromLevel = 0
        self.MaxResearchCount = 0
        self.PropertiesPerIncrement = None
        self.CostFn = None
        self.IsPhantom = False

    class ID:
        def __init__(self):
            self.Value = ""

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
            self.IconsProtos = None

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
    ASSETS_NON_DEMO = ""
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
        self.RemainingSteps = 0
        self.ScienceCost = 0
        self.BaseScienceCost = 0
        self.ScienceCostLocStr = None
        self.Proto = None
        self.StepsDone = 0
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
        self.IsLockedByDemo = False
        self.CanBeEnqueued = False
        self.CanBeEnqueuedDirect = False
        self.CanBeDequeued = False
        self.IndexInQueue = 0
        self.RequiresSpacePoints = False
        self.LockedByConditions = None
        self.TimesResearched = 0

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
            self.IsLockedByDemo = False

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
            self.Builder = None

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
        self.Graphics = None
        self.IsPhantom = False

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


class UnlockingConditionSpaceStation:
    def __init__(self):
        self.IsSatisfied = False
        self.MinTierRequired = 0

    class Manager:
        def __init__(self):
            pass


class IResearchNodeUnlockingCondition:
    def __init__(self):
        pass


class IResearchUnlockingConditionManager:
    def __init__(self):
        pass

