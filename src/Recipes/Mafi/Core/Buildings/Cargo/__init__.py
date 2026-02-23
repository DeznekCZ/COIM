
class CargoDepot:
    SHIP_OFFSET = None
    def __init__(self):
        self.UpgradableProto = None
        self.Prototype = None
        self.DockEntityProto = None
        from Mafi import Option
        self.ContractAssigned = Option()
        self.CanBePaused = False
        self.CanAcceptShip = False
        self.IsAccessBlocked = False
        self.LogisticsInputControl = None
        self.LogisticsOutputControl = None
        self.IsLogisticsInputDisabled = False
        self.IsLogisticsOutputDisabled = False
        self.Modules = None
        self.SlotCount = 0
        self.CargoShip = Option()
        self.FuelBuffer = None
        self.OnModuleAdded = None
        self.OnModuleUpgraded = None
        self.OnModuleRemoved = None
        self.ReservedOceanAreaState = None
        self.ReservedOceanAreaStateV2 = Option()
        self.OceanAreaRequired = None
        self.OceanAreaDesired = None
        self.TileRequiredPathable = None
        self.PathabilityMask = None
        self.OceanAreaBlocked = None
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

class CargoDepotCheatFullFuelCmd:
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

class CargoDepotSetFuelSliderStepCmd:
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
        self.ImportStep = 0
        self.ExportStep = 0

class CargoDepotProto:
    MIN_GROUND_HEIGHT = None
    MAX_GROUND_HEIGHT = None
    def __init__(self):
        self.EntityType = None
        self.Upgrade = None
        self.TierData = None
        self.CargoShipProto = None
        self.MinGroundHeight = None
        self.MaxGroundHeight = None
        self.InterfaceRange = None
        self.ArriveDuration = None
        self.DepartDuration = None
        self.DockOffset = None
        self.PathabilityQueryMask = None
        self.ShipHeightClass = None
        from Mafi.Core.Buildings.Cargo import CargoDepotProto
        self.Id = CargoDepotProto.ID()

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
        self.ModuleSlots = None
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

    class ModuleSlotPosition:
        def __init__(self):
            self.Origin = None
            self.SlotSize = None

    class ID:
        def __init__(self):
            self.Value = ""

class CargoDepotProtoBuilder:
    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None

    class State:
        def __init__(self):
            self.Builder = None

class TradeDock:
    CARGO_EXPORT_PRIO_ID = ""
    def __init__(self):
        self.Prototype = None
        self.CanBePaused = False
        self.CanTrade = False
        self.HasHighCargoUnloadPrio = False
        self.LoanBuffers = None
        self.ReservedOceanProto = None
        self.ReservedOceanAreaState = None
        from Mafi import Option
        self.ReservedOceanAreaStateV2 = Option()
        self.OceanAreaRequired = None
        self.OceanAreaDesired = None
        self.TileRequiredPathable = None
        self.PathabilityMask = None
        self.OceanAreaBlocked = None
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

    class LoanPaymentBuffer:
        def __init__(self):
            self.IsOpenForDelivery = False
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
            self.Loan = None

class TradeDockToggleUnloadPriorityCmd:
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
        self.TradeDockId = None

class TradeDockManager:
    def __init__(self):
        from Mafi import Option
        self.TradeDock = Option()

class TradeDockProto:
    def __init__(self):
        self.EntityType = None
        self.MinGroundHeight = None
        self.MaxGroundHeight = None
        self.PathabilityQueryMask = None
        self.ShipHeightClass = None
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.IconPath = ""
        self.CanMoveUpDownWhenInvalidPlacement = False
        self.IsUnique = False
        self.CloningDisabled = False
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

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
