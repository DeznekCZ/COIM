
class SetShiftsCountForMineCmd:
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
        self.EntityId = None
        self.ShiftsCount = 0

class WorldMapCargoShipWreck:
    def __init__(self):
        self.IsOwnedByPlayer = False
        self.CanBePaused = False
        self.CostToRepair = None
        self.OnConstructionDone = None
        self.OnAllConstructionProductsAvailable = None
        self.IsBeingRepaired = False
        self.IsRepaired = False
        self.IsBeingUpgraded = False
        self.IsUnderConstruction = False
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.NeedsProductsForConstruction = False
        self.Location = None
        self.Id = None
        self.DefaultTitle = None
        self.Prototype = None
        self.Context = None
        self.IsDestroyed = False
        self.IsEnabled = False
        self.IsNotEnabled = False
        self.IsPaused = False
        self.IsNotPaused = False

class WorldMapEntity:
    def __init__(self):
        self.IsOwnedByPlayer = False
        self.Location = None
        self.Id = None
        self.DefaultTitle = None
        self.Prototype = None
        self.Context = None
        self.IsDestroyed = False
        self.CanBePaused = False
        self.IsEnabled = False
        self.IsNotEnabled = False
        self.IsPaused = False
        self.IsNotPaused = False

class WorldMapMine:
    def __init__(self):
        from Mafi import Option
        self.CustomTitle = Option()
        self.DefaultTitle = None
        self.CanBePaused = False
        self.Level = 0
        self.MaxProductionSteps = 0
        self.ProductionStep = 0
        self.Prototype = None
        self.WorkersNeeded = 0
        self.WorkedLastTick = False
        self.Product = None
        self.ProgressDone = None
        self.Buffer = None
        self.UnityConsumer = Option()
        self.CurrentState = None
        self.MaxMonthlyUnityConsumed = None
        self.MonthlyUnityConsumed = None
        from Mafi.Core.Prototypes import Proto
        self.UpointsCategoryId = Proto.ID()

        self.Maintenance = None
        self.CostToRepair = None
        self.QuantityAvailable = None
        self.PriceToUpgrade = None
        self.UpgradeTitle = None
        self.UpgradeExists = False
        self.UpgradeIcon = ""
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.IsOwnedByPlayer = False
        self.OnConstructionDone = None
        self.OnAllConstructionProductsAvailable = None
        self.IsBeingRepaired = False
        self.IsRepaired = False
        self.IsBeingUpgraded = False
        self.IsUnderConstruction = False
        self.ConstructionProgress = Option()
        self.NeedsProductsForConstruction = False
        self.Location = None
        self.Id = None
        self.Context = None
        self.IsDestroyed = False
        self.IsEnabled = False
        self.IsNotEnabled = False
        self.IsPaused = False
        self.IsNotPaused = False
        self.HasWorkersCached = False
        self.MaintenanceCosts = None
        self.IsIdleForMaintenance = False

    class State:
        None = None
        Broken = None
        Paused = None
        NotEnoughWorkers = None
        NotEnoughUnity = None
        ResourceDepleted = None
        FullStorage = None
        Working = None
        def __init__(self):
            self.value__ = 0

class WorldMapRepairableEntity:
    def __init__(self):
        self.IsOwnedByPlayer = False
        self.CostToRepair = None
        self.OnConstructionDone = None
        self.OnAllConstructionProductsAvailable = None
        self.IsBeingRepaired = False
        self.IsRepaired = False
        self.IsBeingUpgraded = False
        self.IsUnderConstruction = False
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.NeedsProductsForConstruction = False
        self.Location = None
        self.Id = None
        self.DefaultTitle = None
        self.Prototype = None
        self.Context = None
        self.IsDestroyed = False
        self.CanBePaused = False
        self.IsEnabled = False
        self.IsNotEnabled = False
        self.IsPaused = False
        self.IsNotPaused = False

class WorldMapVillage:
    def __init__(self):
        self.CanBePaused = False
        self.CostToRepair = None
        self.QuickTradesIndexable = None
        self.IsPopsAdoptionSupported = False
        self.IsPopsAdoptionAvailable = False
        self.PopsAvailable = 0
        self.Reputation = 0
        self.PriceToUpgrade = None
        self.UpgradeTitle = None
        self.UpgradeExists = False
        self.UpgradeIcon = ""
        self.IsOwnedByPlayer = False
        self.OnConstructionDone = None
        self.OnAllConstructionProductsAvailable = None
        self.IsBeingRepaired = False
        self.IsRepaired = False
        self.IsBeingUpgraded = False
        self.IsUnderConstruction = False
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.NeedsProductsForConstruction = False
        self.Location = None
        self.Id = None
        self.DefaultTitle = None
        self.Prototype = None
        self.Context = None
        self.IsDestroyed = False
        self.IsEnabled = False
        self.IsNotEnabled = False
        self.IsPaused = False
        self.IsNotPaused = False
        self.QuickTrades = None

class DefaultWorldMapEntityFactory:
    def __init__(self):
        pass


class IUpgradableWorldEntity:
    def __init__(self):
        self.PriceToUpgrade = None
        self.UpgradeTitle = None
        self.UpgradeExists = False
        self.UpgradeIcon = ""
        self.IsOwnedByPlayer = False
        self.Location = None
        self.DefaultTitle = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False

class WorldMapCargoShipWreckProto:
    def __init__(self):
        self.EntityType = None
        self.IconPath = ""
        self.Costs = None
        from Mafi.Core.Entities import EntityProto
        self.Id = EntityProto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.CostToRepair = None
        self.Graphics = None
        self.IsPhantom = False
        self.IsInitialized = False

class IWorldMapEntity:
    def __init__(self):
        self.IsOwnedByPlayer = False
        self.Location = None
        self.DefaultTitle = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False

class IWorldMapRepairableEntity:
    def __init__(self):
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.IsUnderConstruction = False
        self.IsRepaired = False
        self.NeedsProductsForConstruction = False
        self.OnConstructionDone = None
        self.OnAllConstructionProductsAvailable = None
        self.IsOwnedByPlayer = False
        self.Location = None
        self.DefaultTitle = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False

class WorldMapEntityProto:
    def __init__(self):
        self.IconPath = ""
        self.EntityType = None
        self.Costs = None
        from Mafi.Core.Entities import EntityProto
        self.Id = EntityProto.ID()

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
            self.WorldMapIconPath = ""
            self.Color = None
            self.RendererIndex = 0

class WorldMapLocationGfxProto:
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
        self.IconPath = ""
        self.Size = None
        self.IsPhantom = False
        self.IsInitialized = False

class WorldMapMineProto:
    def __init__(self):
        self.EntityType = None
        self.IconPath = ""
        self.Costs = None
        from Mafi.Core.Entities import EntityProto
        self.Id = EntityProto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.ProducedProductPerStep = None
        self.ProductionDuration = None
        self.UpointsCategory = None
        self.MonthlyUpointsPerLevel = None
        self.CostPerLevel = None
        self.Level = 0
        self.MaxLevel = 0
        self.LevelsPerUpgrade = 0
        self.QuantityAvailable = None
        self.Graphics = None
        self.IsPhantom = False
        self.IsInitialized = False

class WorldMapVillageProto:
    def __init__(self):
        self.EntityType = None
        self.IconPath = ""
        self.Costs = None
        from Mafi.Core.Entities import EntityProto
        self.Id = EntityProto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.UpointsPerPopToAdopt = None
        self.CostPerLevel = None
        self.QuickTrades = None
        self.Contracts = None
        self.ProductsToLend = None
        self.MaxReputation = 0
        self.StartingReputation = 0
        self.MinReputationNeededToAdopt = 0
        self.DurationPerNewPopPerReputationLevel = None
        self.Graphics = None
        self.IsPhantom = False
        self.IsInitialized = False

    class ProductToLend:
        def __init__(self):
            self.Product = None
            self.BorrowFromStart = False
