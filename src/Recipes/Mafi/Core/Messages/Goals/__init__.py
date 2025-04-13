
class GoalProto:
    def __init__(self):
        self.Implementation = None
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.Tutorial = None
        self.TutorialUnlock = None
        self.Tip = None
        self.LockedByIndex = 0
        self.IsPhantom = False
        self.IsInitialized = False

    class TutorialUnlockMode:
        DoNotUnlock = None
        UnlockSilently = None
        UnlockAndNotify = None
        def __init__(self):
            self.value__ = 0

class Goal:
    def __init__(self):
        self.Title = ""
        self.IsLocked = False
        self.IsCompleted = False
        self.Prototype = None

class GoalListTrigger:
    def __init__(self):
        self.Version = 0

class IGoalListTriggerData:
    def __init__(self):
        self.Implementation = None
        self.Version = 0

class GoalToReachProductStatsValue:
    def __init__(self):
        self.Title = ""
        self.IsLocked = False
        self.IsCompleted = False
        self.Prototype = None

    class Proto:
        def __init__(self):
            self.Implementation = None
            from Mafi.Core.Prototypes import Proto
            self.Id = Proto.ID()

            self.Strings = None
            self.IsNotPhantom = False
            self.Mod = None
            self.Tags = None
            self.IsNotAvailable = False
            self.IsAvailable = False
            self.IsObsolete = False
            self.ProtoToTrack = None
            self.MinQuantityRequired = None
            self.HideCount = False
            self.StatsSelector = None
            self.Tutorial = None
            self.TutorialUnlock = None
            self.Tip = None
            self.LockedByIndex = 0
            self.IsPhantom = False
            self.IsInitialized = False

class GoalToConstructStaticEntity:
    def __init__(self):
        self.Title = ""
        self.IsLocked = False
        self.IsCompleted = False
        self.Prototype = None

    class Proto:
        TITLE_BUILD = None
        TITLE_BUILD_ANOTHER = None
        TITLE_BUILD_AND_CONNECT = None
        TITLE_RESEARCH_AND_BUILD = None
        TITLE_RESEARCH_AND_UPGRADE = None
        def __init__(self):
            self.Implementation = None
            from Mafi.Core.Prototypes import Proto
            self.Id = Proto.ID()

            self.Strings = None
            self.IsNotPhantom = False
            self.Mod = None
            self.Tags = None
            self.IsNotAvailable = False
            self.IsAvailable = False
            self.IsObsolete = False
            self.ProtosToBuild = None
            self.TitleFunc = None
            self.Tutorial = None
            self.TutorialUnlock = None
            self.Tip = None
            self.LockedByIndex = 0
            self.IsPhantom = False
            self.IsInitialized = False

class GoalToConstructNumberOfStaticEntities:
    def __init__(self):
        self.Title = ""
        self.IsLocked = False
        self.IsCompleted = False
        self.Prototype = None

    class Proto:
        def __init__(self):
            self.Implementation = None
            from Mafi.Core.Prototypes import Proto
            self.Id = Proto.ID()

            self.Strings = None
            self.IsNotPhantom = False
            self.Mod = None
            self.Tags = None
            self.IsNotAvailable = False
            self.IsAvailable = False
            self.IsObsolete = False
            self.ProtosIds = None
            self.EntitiesCount = 0
            self.Title = None
            self.Tutorial = None
            self.TutorialUnlock = None
            self.Tip = None
            self.LockedByIndex = 0
            self.IsPhantom = False
            self.IsInitialized = False

class GoalToResearchNode:
    def __init__(self):
        self.Title = ""
        self.IsLocked = False
        self.IsCompleted = False
        self.Prototype = None

    class Proto:
        def __init__(self):
            self.Implementation = None
            from Mafi.Core.Prototypes import Proto
            self.Id = Proto.ID()

            self.Strings = None
            self.IsNotPhantom = False
            self.Mod = None
            self.Tags = None
            self.IsNotAvailable = False
            self.IsAvailable = False
            self.IsObsolete = False
            self.NodeToResearch = None
            self.Title = None
            self.Tutorial = None
            self.TutorialUnlock = None
            self.Tip = None
            self.LockedByIndex = 0
            self.IsPhantom = False
            self.IsInitialized = False

class GoalToSetupMining:
    def __init__(self):
        self.Title = ""
        self.IsLocked = False
        self.IsCompleted = False
        self.Prototype = None

    class Proto:
        def __init__(self):
            self.Implementation = None
            from Mafi.Core.Prototypes import Proto
            self.Id = Proto.ID()

            self.Strings = None
            self.IsNotPhantom = False
            self.Mod = None
            self.Tags = None
            self.IsNotAvailable = False
            self.IsAvailable = False
            self.IsObsolete = False
            self.LooseProductProto = None
            self.Title = None
            self.Tutorial = None
            self.TutorialUnlock = None
            self.Tip = None
            self.LockedByIndex = 0
            self.IsPhantom = False
            self.IsInitialized = False

class GoalToSetupDumping:
    def __init__(self):
        self.Title = ""
        self.IsLocked = False
        self.IsCompleted = False
        self.Prototype = None

    class Proto:
        TITLE_DESIGNATE_DUMPING = None
        def __init__(self):
            self.Implementation = None
            from Mafi.Core.Prototypes import Proto
            self.Id = Proto.ID()

            self.Strings = None
            self.IsNotPhantom = False
            self.Mod = None
            self.Tags = None
            self.IsNotAvailable = False
            self.IsAvailable = False
            self.IsObsolete = False
            self.LooseProductProto = None
            self.Tutorial = None
            self.TutorialUnlock = None
            self.Tip = None
            self.LockedByIndex = 0
            self.IsPhantom = False
            self.IsInitialized = False

class GoalToReachRefugees:
    def __init__(self):
        self.Title = ""
        self.IsLocked = False
        self.IsCompleted = False
        self.Prototype = None

    class Proto:
        Title = None
        def __init__(self):
            self.Implementation = None
            from Mafi.Core.Prototypes import Proto
            self.Id = Proto.ID()

            self.Strings = None
            self.IsNotPhantom = False
            self.Mod = None
            self.Tags = None
            self.IsNotAvailable = False
            self.IsAvailable = False
            self.IsObsolete = False
            self.Tutorial = None
            self.TutorialUnlock = None
            self.Tip = None
            self.LockedByIndex = 0
            self.IsPhantom = False
            self.IsInitialized = False

class GoalToActivateRecipe:
    def __init__(self):
        self.Title = ""
        self.IsLocked = False
        self.IsCompleted = False
        self.Prototype = None

    class Proto:
        def __init__(self):
            self.Implementation = None
            from Mafi.Core.Prototypes import Proto
            self.Id = Proto.ID()

            self.Strings = None
            self.IsNotPhantom = False
            self.Mod = None
            self.Tags = None
            self.IsNotAvailable = False
            self.IsAvailable = False
            self.IsObsolete = False
            self.MachineRecipeToActivate = None
            self.Title = None
            self.EntitiesCount = 0
            self.Tutorial = None
            self.TutorialUnlock = None
            self.Tip = None
            self.LockedByIndex = 0
            self.IsPhantom = False
            self.IsInitialized = False

class GoalToExploreWithShip:
    def __init__(self):
        self.Title = ""
        self.IsLocked = False
        self.IsCompleted = False
        self.Prototype = None

    class Proto:
        def __init__(self):
            self.Implementation = None
            from Mafi.Core.Prototypes import Proto
            self.Id = Proto.ID()

            self.Strings = None
            self.IsNotPhantom = False
            self.Mod = None
            self.Tags = None
            self.IsNotAvailable = False
            self.IsAvailable = False
            self.IsObsolete = False
            self.Title = None
            self.Tutorial = None
            self.TutorialUnlock = None
            self.Tip = None
            self.LockedByIndex = 0
            self.IsPhantom = False
            self.IsInitialized = False

class GoalToDiscoverWorldMine:
    def __init__(self):
        self.Title = ""
        self.IsLocked = False
        self.IsCompleted = False
        self.Prototype = None

    class Proto:
        def __init__(self):
            self.Implementation = None
            from Mafi.Core.Prototypes import Proto
            self.Id = Proto.ID()

            self.Strings = None
            self.IsNotPhantom = False
            self.Mod = None
            self.Tags = None
            self.IsNotAvailable = False
            self.IsAvailable = False
            self.IsObsolete = False
            self.Title = None
            self.ProductToMine = None
            self.RequireRepaired = False
            self.Tutorial = None
            self.TutorialUnlock = None
            self.Tip = None
            self.LockedByIndex = 0
            self.IsPhantom = False
            self.IsInitialized = False

class GoalToRepairShip:
    def __init__(self):
        self.Title = ""
        self.IsLocked = False
        self.IsCompleted = False
        self.Prototype = None

    class Proto:
        def __init__(self):
            self.Implementation = None
            from Mafi.Core.Prototypes import Proto
            self.Id = Proto.ID()

            self.Strings = None
            self.IsNotPhantom = False
            self.Mod = None
            self.Tags = None
            self.IsNotAvailable = False
            self.IsAvailable = False
            self.IsObsolete = False
            self.Title = None
            self.Tutorial = None
            self.TutorialUnlock = None
            self.Tip = None
            self.LockedByIndex = 0
            self.IsPhantom = False
            self.IsInitialized = False

class GoalToRefuelShip:
    def __init__(self):
        self.Title = ""
        self.IsLocked = False
        self.IsCompleted = False
        self.Prototype = None

    class Proto:
        def __init__(self):
            self.Implementation = None
            from Mafi.Core.Prototypes import Proto
            self.Id = Proto.ID()

            self.Strings = None
            self.IsNotPhantom = False
            self.Mod = None
            self.Tags = None
            self.IsNotAvailable = False
            self.IsAvailable = False
            self.IsObsolete = False
            self.Title = None
            self.Tutorial = None
            self.TutorialUnlock = None
            self.Tip = None
            self.LockedByIndex = 0
            self.IsPhantom = False
            self.IsInitialized = False

class GoalToManShip:
    def __init__(self):
        self.Title = ""
        self.IsLocked = False
        self.IsCompleted = False
        self.Prototype = None

    class Proto:
        def __init__(self):
            self.Implementation = None
            from Mafi.Core.Prototypes import Proto
            self.Id = Proto.ID()

            self.Strings = None
            self.IsNotPhantom = False
            self.Mod = None
            self.Tags = None
            self.IsNotAvailable = False
            self.IsAvailable = False
            self.IsObsolete = False
            self.Title = None
            self.Tutorial = None
            self.TutorialUnlock = None
            self.Tip = None
            self.LockedByIndex = 0
            self.IsPhantom = False
            self.IsInitialized = False

class GoalToBuildHousing:
    def __init__(self):
        self.Title = ""
        self.IsLocked = False
        self.IsCompleted = False
        self.Prototype = None

    class Proto:
        def __init__(self):
            self.Implementation = None
            from Mafi.Core.Prototypes import Proto
            self.Id = Proto.ID()

            self.Strings = None
            self.IsNotPhantom = False
            self.Mod = None
            self.Tags = None
            self.IsNotAvailable = False
            self.IsAvailable = False
            self.IsObsolete = False
            self.Title = None
            self.TargetHousingCount = 0
            self.Tutorial = None
            self.TutorialUnlock = None
            self.Tip = None
            self.LockedByIndex = 0
            self.IsPhantom = False
            self.IsInitialized = False

class GoalToBuildStorage:
    def __init__(self):
        self.Title = ""
        self.IsLocked = False
        self.IsCompleted = False
        self.Prototype = None

    class Proto:
        TITLE_BUILD_STORAGE = None
        def __init__(self):
            self.Implementation = None
            from Mafi.Core.Prototypes import Proto
            self.Id = Proto.ID()

            self.Strings = None
            self.IsNotPhantom = False
            self.Mod = None
            self.Tags = None
            self.IsNotAvailable = False
            self.IsAvailable = False
            self.IsObsolete = False
            self.Title = None
            from Mafi.Core.Entities.Static import StaticEntityProto
            self.StorageId = StaticEntityProto.ID()

            from Mafi.Core.Products import ProductProto
            self.ProductStoredId = ProductProto.ID()

            self.RequireImportSlider = False
            self.RequireExportSlider = False
            self.RequireLogisticsInputDisabled = False
            self.Tutorial = None
            self.TutorialUnlock = None
            self.Tip = None
            self.LockedByIndex = 0
            self.IsPhantom = False
            self.IsInitialized = False

class GoalToStockpileProducts:
    def __init__(self):
        self.Title = ""
        self.IsLocked = False
        self.IsCompleted = False
        self.Prototype = None

    class Proto:
        TITLE_STORE = None
        def __init__(self):
            self.Implementation = None
            from Mafi.Core.Prototypes import Proto
            self.Id = Proto.ID()

            self.Strings = None
            self.IsNotPhantom = False
            self.Mod = None
            self.Tags = None
            self.IsNotAvailable = False
            self.IsAvailable = False
            self.IsObsolete = False
            self.Title = None
            self.QuantityToStockpile = None
            from Mafi.Core.Products import ProductProto
            self.ProductToStoreId = ProductProto.ID()

            self.Tutorial = None
            self.TutorialUnlock = None
            self.Tip = None
            self.LockedByIndex = 0
            self.IsPhantom = False
            self.IsInitialized = False

class GoalToRepairCargoShip:
    def __init__(self):
        self.Title = ""
        self.IsLocked = False
        self.IsCompleted = False
        self.Prototype = None

    class Proto:
        def __init__(self):
            self.Implementation = None
            from Mafi.Core.Prototypes import Proto
            self.Id = Proto.ID()

            self.Strings = None
            self.IsNotPhantom = False
            self.Mod = None
            self.Tags = None
            self.IsNotAvailable = False
            self.IsAvailable = False
            self.IsObsolete = False
            self.Title = None
            self.Tutorial = None
            self.TutorialUnlock = None
            self.Tip = None
            self.LockedByIndex = 0
            self.IsPhantom = False
            self.IsInitialized = False

class GoalToPauseEntity:
    def __init__(self):
        self.Title = ""
        self.IsLocked = False
        self.IsCompleted = False
        self.Prototype = None

    class Proto:
        def __init__(self):
            self.Implementation = None
            from Mafi.Core.Prototypes import Proto
            self.Id = Proto.ID()

            self.Strings = None
            self.IsNotPhantom = False
            self.Mod = None
            self.Tags = None
            self.IsNotAvailable = False
            self.IsAvailable = False
            self.IsObsolete = False
            self.Title = None
            self.ProtoToPause = None
            self.Tutorial = None
            self.TutorialUnlock = None
            self.Tip = None
            self.LockedByIndex = 0
            self.IsPhantom = False
            self.IsInitialized = False

class GoalToConstructFuelStation:
    def __init__(self):
        self.Title = ""
        self.IsLocked = False
        self.IsCompleted = False
        self.Prototype = None

    class Proto:
        def __init__(self):
            self.Implementation = None
            from Mafi.Core.Prototypes import Proto
            self.Id = Proto.ID()

            self.Strings = None
            self.IsNotPhantom = False
            self.Mod = None
            self.Tags = None
            self.IsNotAvailable = False
            self.IsAvailable = False
            self.IsObsolete = False
            self.Title = None
            self.IsAssignedTruckRequired = False
            self.Tutorial = None
            self.TutorialUnlock = None
            self.Tip = None
            self.LockedByIndex = 0
            self.IsPhantom = False
            self.IsInitialized = False

class GoalToAssignTrucksToTreeHarvester:
    def __init__(self):
        self.Title = ""
        self.IsLocked = False
        self.IsCompleted = False
        self.Prototype = None

    class Proto:
        def __init__(self):
            self.Implementation = None
            from Mafi.Core.Prototypes import Proto
            self.Id = Proto.ID()

            self.Strings = None
            self.IsNotPhantom = False
            self.Mod = None
            self.Tags = None
            self.IsNotAvailable = False
            self.IsAvailable = False
            self.IsObsolete = False
            self.Title = None
            self.NumberOfTrucksRequired = 0
            self.VehicleToAssignTo = None
            self.Tutorial = None
            self.TutorialUnlock = None
            self.Tip = None
            self.LockedByIndex = 0
            self.IsPhantom = False
            self.IsInitialized = False

class GoalToActivateEdict:
    def __init__(self):
        self.Title = ""
        self.IsLocked = False
        self.IsCompleted = False
        self.Prototype = None

    class Proto:
        def __init__(self):
            self.Implementation = None
            from Mafi.Core.Prototypes import Proto
            self.Id = Proto.ID()

            self.Strings = None
            self.IsNotPhantom = False
            self.Mod = None
            self.Tags = None
            self.IsNotAvailable = False
            self.IsAvailable = False
            self.IsObsolete = False
            self.Title = None
            self.EdictToActivate = None
            self.Tutorial = None
            self.TutorialUnlock = None
            self.Tip = None
            self.LockedByIndex = 0
            self.IsPhantom = False
            self.IsInitialized = False

class GoalListProto:
    def __init__(self):
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.Goals = None
        self.TriggerData = None
        self.Rewards = None
        self.IsLongTermTask = False
        self.IsPhantom = False
        self.IsInitialized = False

class GoalsList:
    def __init__(self):
        self.Title = None
        self.Goals = None
        self.IsCompleted = False
        self.ActivatedAtSimStep = None
        self.Prototype = None

class TutorialsConfig:
    def __init__(self):
        self.AreTutorialsEnabled = False

class GoalsManager:
    def __init__(self):
        self.CompletedGoals = None
        self.ActiveGoals = None
        self.OnGoalFinished = None

class GoalsListTriggerOnGoalListDone:
    def __init__(self):
        self.Version = 0

    class TriggerRule:
        AllSatisfied = None
        AnySatisfied = None
        def __init__(self):
            self.value__ = 0

    class Data:
        def __init__(self):
            self.Implementation = None
            self.Version = 0
            self.GoalListProtosIds = None
            self.Rule = None

class GoalsListTriggerOnMessageDelivered:
    def __init__(self):
        self.Version = 0

    class Data:
        def __init__(self):
            self.Implementation = None
            self.Version = 0
            from Mafi.Core.Prototypes import Proto
            self.MessageProtoId = Proto.ID()


class GoalsListTriggerToPauseBeacon:
    def __init__(self):
        self.Version = 0

    class Data:
        def __init__(self):
            self.Implementation = None
            self.Version = 0

class GoalsListTriggerForFarm:
    def __init__(self):
        self.Version = 0

    class Data:
        def __init__(self):
            self.Implementation = None
            self.Version = 0
            from Mafi.Core.Prototypes import Proto
            self.TriggerOnlyAfter = Proto.ID()

            self.NumberOfFarmsToRemoveThis = 0

class GoalsListTriggerOnGoalsOrProductLow:
    def __init__(self):
        self.Version = 0

    class TriggerRule:
        AllSatisfied = None
        AnySatisfied = None
        def __init__(self):
            self.value__ = 0

    class Data:
        def __init__(self):
            self.Implementation = None
            self.Version = 0
            self.GoalListProtosIds = None
            self.Rule = None
            self.ProductQuantityToTrigger = None

class MarkGoalAsFinishedCmd:
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
        self.ProtoId = Proto.ID()

        self.GoalWasSkipped = False
