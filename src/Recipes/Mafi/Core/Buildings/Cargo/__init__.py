
class CargoDepot:
    def __init__(self):
        self.Prototype = None
        from Mafi import Option
        self.ContractAssigned = Option()
        self.CanBePaused = False
        self.CanAcceptShip = False
        self.Upgrader = None
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
        self.CustomTitle = Option()
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.Ports = None
        self.Value = None
        self.ConstructionCost = None
        self.Transform = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.PfTargetTiles = None
        self.CenterTile = None
        self.Position2f = None
        self.Position3f = None
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
        self.UpgradeNonGeneric = None
        self.CargoShipProto = None
        self.ReservedOceanAreasSets = None
        self.MinGroundHeight = None
        self.MaxGroundHeight = None
        from Mafi.Core.Buildings.Cargo import CargoDepotProto
        self.Id = CargoDepotProto.ID()

        self.Layout = None
        self.Ports = None
        self.CloningDisabled = False
        self.IsUnique = False
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.IconPath = ""
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.ModuleSlots = None
        self.DockingAnimationsPrefabPaths = None
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.CannotBeBuiltByPlayer = False
        self.ConstructionDurationPerProduct = None
        self.VehicleGoalHeightAllowedRange = None
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False
        self.IsInitialized = False

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
        self.CustomTitle = Option()
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.Ports = None
        self.Value = None
        self.ConstructionCost = None
        self.Transform = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.PfTargetTiles = None
        self.CenterTile = None
        self.Position2f = None
        self.Position3f = None
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
        self.ReservedOceanAreasSets = None
        self.MinGroundHeight = None
        self.MaxGroundHeight = None
        self.Layout = None
        self.Ports = None
        self.CloningDisabled = False
        self.IsUnique = False
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.IconPath = ""
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.CannotBeBuiltByPlayer = False
        self.ConstructionDurationPerProduct = None
        self.VehicleGoalHeightAllowedRange = None
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False
        self.IsInitialized = False
