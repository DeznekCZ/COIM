
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
        self.UpgradableProto = None
        self.Prototype = None
        self.DockEntityProto = None
        self.CanBePaused = False
        self.IsAccessBlocked = False
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
        self.CurrentModificationRequest = None
        self.AssignedShip = Option()
        self.HasHighCargoUnloadPrio = False
        self.IsFull = False
        self.TotalStoredQuantity = None
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
        self.TierData = None
        self.MinGroundHeight = None
        self.MaxGroundHeight = None
        self.InterfaceRange = None
        self.ArriveDuration = None
        self.DepartDuration = None
        self.DockOffset = None
        self.PathabilityQueryMask = None
        self.ShipHeightClass = None
        self.EntityType = None
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
        self.CanRepair = False
        self.CargoCapacity = None
        from Mafi import Option
        self.FleetProto = Option()
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
