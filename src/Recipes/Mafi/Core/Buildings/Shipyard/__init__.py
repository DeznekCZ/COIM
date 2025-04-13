
class ShipModificationState:
    None = None
    Preparing = None
    Prepared = None
    Applying = None
    def __init__(self):
        self.value__ = 0

class Shipyard:
    FUEL_IMPORT_PRIO_ID = ""
    FUEL_EXPORT_PRIO_ID = ""
    CARGO_EXPORT_PRIO_ID = ""
    SHIP_REPAIR_IMPORT_PRIO_ID = ""
    WORLD_CARGO_IMPORT_PRIO_ID = ""
    def __init__(self):
        self.Prototype = None
        self.CanBePaused = False
        self.FuelBuffer = None
        self.CanRepair = False
        self.IsAutoRepairEnabled = False
        self.IsRepairing = False
        from Mafi import Option
        self.RepairProgress = Option()
        self.CargoInputPaused = False
        self.CanPerformModifications = False
        self.CanCancelModifications = False
        self.CanApplyModification = False
        self.ModificationProgress = Option()
        self.ModificationState = None
        self.DockedFleet = Option()
        self.Upgrader = None
        self.HasHighCargoUnloadPrio = False
        self.IsFull = False
        self.TotalStoredQuantity = None
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
        self.WorldEntityToConstruct = Option()

    class ShipRepairBufferPriorityProvider:
        def __init__(self):
            pass


    class WorldCargoImportPriorityProvider:
        def __init__(self):
            pass


class ShipyardToggleUnloadPriorityCmd:
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
        self.ShipyardId = None

class ShipyardToggleAutoRepairCmd:
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
        self.ShipyardId = None

class ShipyardSetRepairingCmd:
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
        self.ShipyardId = None
        self.IsRepairing = False

class ShipyardCheatFullFuelCmd:
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
        self.ShipyardId = None

class ShipayardSetFuelSliderStepCmd:
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
        self.ShipyardId = None
        self.ImportStep = 0
        self.ExportStep = 0

class ShipyardWorldEntityConstructionToggle:
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
        self.ShipyardId = None
        self.WorldEntityIdToConstruct = None

class ShipyardToggleWorksPauseCmd:
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
        self.ShipyardId = None

class ShipyardMakePrimaryCmd:
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
        self.ShipyardId = None

class ShipyardDiscardProductCmd:
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
        self.ShipyardId = None
        from Mafi.Core.Products import ProductProto
        self.ProductId = ProductProto.ID()


class ShipyardProto:
    def __init__(self):
        self.Upgrade = None
        self.UpgradeNonGeneric = None
        self.ReservedOceanAreasSets = None
        self.MinGroundHeight = None
        self.MaxGroundHeight = None
        self.EntityType = None
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
        self.CanRepair = False
        self.CargoCapacity = None
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
