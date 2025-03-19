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
        self.ErrorMessage = str(0)
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
        self.Level = int(0)
        self.MaxProductionSteps = int(0)
        self.ProductionStep = int(0)
        self.Prototype = None
        self.WorkersNeeded = int(0)
        self.WorkedLastTick = False
        self.Product = None
        self.ProgressDone = None
        self.Buffer = None
        from Mafi import Option
        self.UnityConsumer = Option()
        self.CurrentState = None
        self.MaxMonthlyUnityConsumed = None
        self.MonthlyUnityConsumed = None
        self.UpointsCategoryId = None
        self.Maintenance = None
        self.CostToRepair = None
        self.QuantityAvailable = None
        self.PriceToUpgrade = None
        self.UpgradeTitle = None
        self.UpgradeExists = False
        self.UpgradeIcon = str(0)
        self.GeneralPriority = int(0)
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
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

    def __init__(self):
        pass

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
        self.PopsAvailable = int(0)
        self.Reputation = int(0)
        self.PriceToUpgrade = None
        self.UpgradeTitle = None
        self.UpgradeExists = False
        self.UpgradeIcon = str(0)
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
class DefaultWorldMapEntityFactory:

    def __init__(self):
        pass

class IUpgradableWorldEntity:

    def __init__(self):
        self.PriceToUpgrade = None
        self.UpgradeTitle = None
        self.UpgradeExists = False
        self.UpgradeIcon = str(0)
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
        self.IconPath = str(0)
        self.Costs = None
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
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
        self.IconPath = str(0)
        self.EntityType = None
        self.Costs = None
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
        self.WorldMapIconPath = str(0)
class WorldMapLocationGfxProto:

    def __init__(self):
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class WorldMapMineProto:

    def __init__(self):
        self.EntityType = None
        self.IconPath = str(0)
        self.Costs = None
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class WorldMapVillageProto:

    def __init__(self):
        self.EntityType = None
        self.IconPath = str(0)
        self.Costs = None
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class ProductToLend:

    def __init__(self):
        pass

