
class BirthRateCategoryProto:
    def __init__(self):
        self.Title = None
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

class DiseaseProto:
    def __init__(self):
        from Mafi import Option
        self.CustomTrigger = Option()
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.HealthPenalty = None
        self.MonthlyMortalityRate = None
        self.DurationInMonths = 0
        self.MinDistanceTraveled = 0
        self.Reason = None
        self.IsPhantom = False
        self.IsInitialized = False

class FoodProto:
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
        self.Product = None
        self.FoodCategory = None
        self.IsPhantom = False
        self.IsInitialized = False

class FoodCategoryProto:
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
        self.HasHealthBenefit = False
        self.IsPhantom = False
        self.IsInitialized = False

class HealthPointsCategoryProto:
    def __init__(self):
        self.Title = None
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

class IEntityWithWorkers:
    def __init__(self):
        self.WorkersNeeded = 0
        self.HasWorkersCached = False
        self.GeneralPriority = 0
        self.IsGeneralPriorityVisible = False
        self.IsCargoAffectedByGeneralPriority = False
        self.DefaultTitle = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False

class EntityWithWorkersExtensions:
    def __init__(self):
        pass


class MedicalSuppliesParam:
    def __init__(self):
        self.AllowedProtoType = None
        self.HealthPointsWhenProvided = None
        self.MortalityDeductionWhenProvided = None

class PopNeedProto:
    def __init__(self):
        self.IsFoodNeed = False
        self.IsHealthcareNeed = False
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.Unity = None
        self.HealthGiven = None
        self.Graphics = None
        self.ConsumptionMultiplierProperty = None
        self.UnityMultiplierProperty = None
        self.UpointsCategory = None
        self.IsPhantom = False
        self.IsInitialized = False

    class HealthData:
        def __init__(self):
            self.Diff = None
            self.HealthPointsCategory = None

    class Gfx:
        Empty = None
        def __init__(self):
            self.IconPath = ""

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
        self.CurrentDiseaseMonthsLeft = 0
        self.CurrentDiseaseMortality = None
        self.IsDiseaseMortalityIgnored = False
        self.HealthStats = None
        self.BirthStats = None
        self.IsPopulationGrowthPaused = False
        self.BornTotal = None
        self.LostTotal = None
        self.UpointsForHealthLastMonth = None

class BirthStatistics:
    def __init__(self):
        self.LastMonthRecords = None
        self.BirthRateThisMonth = None
        self.BirthRateThisMonthAlreadyApplied = None
        self.BirthRateLastMonth = None

    class Entry:
        def __init__(self):
            self.Change = None
            self.Max = None
            self.Category = None

class HealthStatistics:
    def __init__(self):
        self.LastMonthRecords = None
        self.GeneratedStats = None
        self.ConsumedStats = None
        self.HealthThisMonth = None
        self.HealthLastMonth = None
        self.GeneratedTotalStats = None
        self.ConsumedTotalStats = None

    class Entry:
        def __init__(self):
            self.Change = None
            self.Max = None
            self.Category = None

class IUnityConsumingEntity:
    def __init__(self):
        self.MonthlyUnityConsumed = None
        self.MaxMonthlyUnityConsumed = None
        from Mafi.Core.Prototypes import Proto
        self.UpointsCategoryId = Proto.ID()

        from Mafi import Option
        self.UnityConsumer = Option()
        self.GeneralPriority = 0
        self.IsGeneralPriorityVisible = False
        self.IsCargoAffectedByGeneralPriority = False
        self.DefaultTitle = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False

class UnityConsumer:
    def __init__(self):
        self.Priority = 0
        self.IsDestroyed = False
        self.IsEnabled = False
        self.NotEnoughUnity = False
        self.MonthlyUnity = None

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
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.HideCount = False
        self.StatsCategory = None
        self.Graphics = None
        self.IsPhantom = False
        self.IsInitialized = False

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
        self.UnityProto = None

class UpointsStats:
    def __init__(self):
        self.GeneratedStats = None
        self.ConsumedStats = None
        self.ThisMonthRecords = None
        self.GeneratedTotalStats = None
        self.ConsumedTotalStats = None

    class Entry:
        def __init__(self):
            self.Exchanged = None
            self.Max = None
            self.Title = None
            self.Category = None

class UpointsStatsCategoryProto:
    def __init__(self):
        self.Title = None
        self.IsOneTimeAction = False
        self.IgnoreInStats = False
        self.HideInUI = False
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

class IWorkersManager:
    def __init__(self):
        self.AmountOfFreeWorkersOrMissing = 0
        self.NumberOfWorkersWithheld = 0
        self.WorkersAmountChanged = None

class WorkersManager:
    def __init__(self):
        self.AmountOfFreeWorkers = 0
        self.AmountOfFreeWorkersOrMissing = 0
        self.NumberOfWorkersWithheld = 0
        self.WorkersAmountChanged = None
        self.TotalWorkersNeededStats = None

    class WorkersStatsPerProto:
        def __init__(self):
            self.Proto = None
            self.EntitiesTotal = 0
            self.WorkersAssigned = 0
            self.WorkersNeeded = 0
