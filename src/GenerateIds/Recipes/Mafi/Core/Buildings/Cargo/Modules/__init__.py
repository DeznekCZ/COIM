
class CargoDepotAssignContractCmd:
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
        self.CargoDepotId = None
        self.ContractId = None

class CargoDepotManager:
    def __init__(self):
        self.HasShipOrDepot = False
        self.AmountOfShipsDiscovered = 0
        self.AmountOfShipsInUse = 0
        self.RepairedUnusedShips = None

class AvailableCargoShipData:
    def __init__(self):
        from Mafi import Option
        self.FuelProto = Option()
        self.FuelQuantity = None

class CargoDepotModule:
    def __init__(self):
        self.LogisticsInputControl = None
        self.LogisticsOutputControl = None
        self.UpgradableProto = None
        self.Prototype = None
        self.CanBePaused = False
        self.Maintenance = None
        self.IsIdleForMaintenance = False
        from Mafi import Option
        self.ElectricityConsumer = Option()
        self.CurrentState = None
        self.Depot = Option()
        self.UsableCapacity = None
        self.OnProductStoredChanged = None
        self.ImportPriority = 0
        self.ExportPriority = 0
        self.IsAnimatingImport = False
        self.CargoAnimationProgress = None
        self.IsCargoTransferAnimating = False
        self.IsPipeMoving = False
        self.PipeMovementProgress = None
        self.StoredProduct = Option()
        self.Capacity = None
        self.CurrentQuantity = None
        self.PercentFull = None
        self.IsEmpty = False
        self.IsNotEmpty = False
        self.IsFull = False
        self.IsNotFull = False
        self.IsLogisticsInputDisabled = False
        self.IsLogisticsOutputDisabled = False
        self.LastProductStoreStep = None
        self.CustomTitle = Option()
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.Ports = None
        self.Transform = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.PfTargetTiles = None
        self.CenterTile = None
        self.Position2f = None
        self.Position3f = None
        self.AlwaysUseCustomPfTargetTiles = False
        self.ConstructionState = None
        self.IsConstructed = False
        self.IsNotConstructed = False
        self.IsBeingUpgraded = False
        self.ConstructionProgress = Option()
        self.DoNotAdjustTerrainDuringConstruction = False
        self.AreConstructionCubesDisabled = False
        self.Id = None
        self.DefaultTitle = None
        self.Context = None
        self.IsDestroyed = False
        self.IsEnabled = False
        self.IsNotEnabled = False
        self.IsPaused = False
        self.IsNotPaused = False
        self.RendererData = None
        self.WorkersNeeded = 0
        self.HasWorkersCached = False
        self.PowerRequired = None
        self.MaintenanceCosts = None
        self.IsPipeDown = False

    class State:
        Paused = None
        Broken = None
        Working = None
        MissingWorkers = None
        NotEnoughPower = None
        Idle = None
        def __init__(self):
            self.value__ = 0

    class CargoDepotBuffer:
        def __init__(self):
            self.CurrentQuantityPercent = 0
            self.ImportUntilPercent = None
            self.ExportFromPercent = None
            self.CleaningMode = False
            self.IsFull = False
            self.IsNotFull = False
            self.IsEmpty = False
            self.IsNotEmpty = False
            self.Quantity = None
            self.ProductQuantity = None
            self.Capacity = None
            self.Product = None
            self.UsableCapacity = None
            self.IsDestroyed = False

class CargoDepotModuleConfigExtensions:
    def __init__(self):
        pass


class CargoDepotModuleClearProductCmd:
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
        self.ModuleId = None

class CargoDepotModuleProto:
    def __init__(self):
        self.EntityType = None
        self.Upgrade = None
        self.TierData = None
        self.HasPipeCraneAnimation = False
        self.ThroughputPerTick = None
        from Mafi.Core.Buildings.Cargo.Modules import CargoDepotModuleProto
        self.Id = CargoDepotModuleProto.ID()

        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.IconPath = ""
        self.CanMoveUpDownWhenInvalidPlacement = False
        self.IsUnique = False
        self.CloningDisabled = False
        self.Costs = None
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
        self.ConsumedPowerForCranePerTick = None
        self.ProductType = None
        self.QuantityPerExchange = None
        self.DurationPerExchange = None
        self.HasCraneAnimation = False
        self.PercentOfAnimationToDropCargoToShip = None
        self.Capacity = None
        self.TransferLimit = None
        self.TransferLimitDuration = None
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.ConstructionDurationPerProduct = None
        self.CollapseRubbleScale = None
        self.CustomBuriedTolerance = None
        self.CustomSuspendedTolerance = None
        self.VehicleGoalHeightAllowedRange = None
        self.CannotBeBuiltByPlayer = False
        self.CannotBeDestroyedByFlood = False
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False

    class Gfx:
        Empty = None
        def __init__(self):
            self.PrefabPath = ""
            self.PrefabOrigin = None
            self.IconPath = ""
            self.YawForGeneratedIcon = None
            self.VisualizedLayers = None
            self.Categories = None
            self.AnimationDataAssetPathBase = ""
            self.CranePrefabPath = ""
            self.IconIsCustom = False
            self.UseInstancedRendering = False
            self.UseSemiInstancedRendering = False
            self.SemiInstancedRenderingExcludedObjects = None
            self.MaxRenderedLod = 0
            self.DisableEmptyChildrenStripping = False
            self.InstancedRendererIndex = None
            self.AnimatedGameObjects = None
            self.AnimationLength = 0.0
            self.RemoveUndergroundVertices = False
            self.HideBlockedPortsIcon = False
            self.Color = None
            self.RendererIndex = 0

    class CraneAnimationControllerOverride:
        def __init__(self):
            self.Height = None
            self.AnimationControllerPath = ""

    class ID:
        def __init__(self):
            self.Value = ""

class CargoDepotModuleProtoBuilder:
    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None

    class State:
        def __init__(self):
            self.Builder = None

class CargoDepotModuleSetProductCmd:
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
        self.ModuleId = None
        from Mafi.Core.Products import ProductProto
        self.ProductId = ProductProto.ID()


class CargoShipDepartNowCmd:
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
        self.CargoShipId = None

class CargoShipPayWithUnityIfOutOfFuelCmd:
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
        self.CargoShipId = None
        self.PayWithUnity = False

class CargoShipReplaceFuelCmd:
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
        self.CargoShipId = None
        from Mafi.Core.Products import ProductProto
        self.FuelId = ProductProto.ID()


class CargoShipSetFuelSaverCmd:
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
        self.CargoShipId = None
        self.IsFuelSaverEnabled = False
