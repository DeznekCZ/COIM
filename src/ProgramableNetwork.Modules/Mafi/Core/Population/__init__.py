class BirthRateCategoryProto:

    def __init__(self):
        self.Title = None
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
class DiseaseProto:

    def __init__(self):
        from Mafi import Option
        self.CustomTrigger = Option()
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
class FoodProto:

    def __init__(self):
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
class FoodCategoryProto:

    def __init__(self):
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
class HealthPointsCategoryProto:

    def __init__(self):
        self.Title = None
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
class IEntityWithWorkers:

    def __init__(self):
        self.WorkersNeeded = int(0)
        self.HasWorkersCached = False
        self.GeneralPriority = int(0)
        self.IsGeneralPriorityVisible = False
        self.IsCargoAffectedByGeneralPriority = False
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
class EntityWithWorkersExtensions:

    def __init__(self):
        pass

class MedicalSuppliesParam:

    def __init__(self):
        self.AllowedProtoType = None
class PopNeedProto:

    def __init__(self):
        self.IconPath = str(0)
        self.IsFoodNeed = False
        self.IsHealthcareNeed = False
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
class HealthData:

    def __init__(self):
        pass

class Gfx:
    Empty = None

    def __init__(self):
        self.IconPath = str(0)
class IDiseaseTrigger:

    def __init__(self):
        pass

class PopsHealthManager:
    MIN_HEALTH = None
    UPOINTS_PER_HEALTHPOINT = None
    UPOINTS_FOR_ABOVE_MIN = None
    BASE_HEALTH_DEFAULT = None

    def __init__(self):
        from Mafi import Option
        self.CurrentDisease = Option()
        self.CurrentDiseaseMonthsLeft = int(0)
        self.CurrentDiseaseMortality = None
        self.IsDiseaseMortalityIgnored = False
        self.HealthStats = None
        self.BirthStats = None
        self.IsPopulationGrowthPaused = False
class BirthStatistics:

    def __init__(self):
        self.LastMonthRecords = None
class Entry:

    def __init__(self):
        pass

class HealthStatistics:

    def __init__(self):
        self.LastMonthRecords = None
        self.GeneratedStats = None
        self.ConsumedStats = None
class Entry:

    def __init__(self):
        pass

class IUnityConsumingEntity:

    def __init__(self):
        self.MonthlyUnityConsumed = None
        self.MaxMonthlyUnityConsumed = None
        self.UpointsCategoryId = None
        from Mafi import Option
        self.UnityConsumer = Option()
        self.GeneralPriority = int(0)
        self.IsGeneralPriorityVisible = False
        self.IsCargoAffectedByGeneralPriority = False
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
class UnityConsumer:

    def __init__(self):
        self.Priority = int(0)
        self.IsDestroyed = False
        self.IsEnabled = False
        self.NotEnoughUnity = False
class IUnityConsumerFactory:

    def __init__(self):
        pass

class UnityConsumerFactory:

    def __init__(self):
        pass

class UpointsCategoryProto:

    def __init__(self):
        self.IsOneTimeAction = False
        self.IgnoreInStats = False
        self.HideInUI = False
        self.Title = None
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
class IUpointsManager:

    def __init__(self):
        self.Quantity = None
        self.QuickActionCostMultiplier = None
class UpointsManager:
    UNITY_BASE_CAP = None

    def __init__(self):
        self.DiffForLastMonth = None
        self.PossibleDiffForLastMonth = None
        self.DiffForLastMonthWithOneTimeActions = None
        self.Quantity = None
        self.TotalUnityCap = None
        self.Stats = None
        self.QuickActionCostMultiplier = None
class UpointsStats:

    def __init__(self):
        self.GeneratedStats = None
        self.ConsumedStats = None
        self.ThisMonthRecords = None
class Entry:

    def __init__(self):
        pass

class UpointsStatsCategoryProto:

    def __init__(self):
        self.Title = None
        self.IsOneTimeAction = False
        self.IgnoreInStats = False
        self.HideInUI = False
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
class IWorkersManager:

    def __init__(self):
        self.AmountOfFreeWorkersOrMissing = int(0)
        self.NumberOfWorkersWithheld = int(0)
        self.WorkersAmountChanged = None
class WorkersManager:

    def __init__(self):
        self.AmountOfFreeWorkers = int(0)
        self.AmountOfFreeWorkersOrMissing = int(0)
        self.NumberOfWorkersWithheld = int(0)
        self.WorkersAmountChanged = None
class WorkersStatsPerProto:

    def __init__(self):
        pass

