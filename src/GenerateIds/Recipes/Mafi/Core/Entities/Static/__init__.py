
class ClearingChecker:
    def __init__(self):
        self.Priority = None

class OccupiedColumn:
    def __init__(self):
        self.From = 0
        self.ToExcl = 0
        self.Scale = 0
        self.IdForXySorting = 0

class ConstructionManager:
    EXTRA_CONSTRUCTION_DURATION = None
    def __init__(self):
        self.DeconstructionRatio = None
        self.EntityConstructed = None
        self.EntityPauseStateChanged = None
        self.OnResetConstructionAnimationState = None
        self.EntityConstructionNearlyFinished = None
        self.EntityStartedDeconstruction = None
        self.EntityConstructionStateChanged = None
        self.IsInstaBuildEnabled = False

class ConstructionProgress:
    def __init__(self):
        self.AlreadyRemovedCost = None
        self.Buffers = None
        self.ConstructionBuffers = None
        self.TotalCost = None
        self.Progress = None
        self.IsNearlyFinished = False
        self.CurrentSteps = 0
        self.MaxSteps = 0
        self.ExtraSteps = 0
        self.AlreadyProcessedSteps = 0
        self.AllowedSteps = 0
        self.IsInExtraStepPhase = False
        self.IsDone = False
        self.WasBlockedOnProductsLastSim = False
        self.IsDeconstruction = False
        self.IsUpgrade = False
        self.IsPaused = False
        self.TerrainDisruptionDisabled = False
        self.Owner = None
        self.WasFullyConstructed = False
        self.DurationPerProduct = None

class QuickDeliverCostHelper:
    def __init__(self):
        pass


class IConstructionProgress:
    def __init__(self):
        self.Buffers = None
        self.TotalCost = None
        self.AlreadyRemovedCost = None
        self.CurrentSteps = 0
        self.MaxSteps = 0
        self.ExtraSteps = 0
        self.Progress = None
        self.IsNearlyFinished = False
        self.WasBlockedOnProductsLastSim = False
        self.IsDeconstruction = False
        self.IsUpgrade = False
        self.IsPaused = False

class IConstructionProgressExtensions:
    def __init__(self):
        pass


class DefaultStaticEntityFactory:
    def __init__(self):
        pass


class EntityCollapseHelper:
    RUBBLE_ONLY_AFTER_CONSTR_PERCENT = None
    RUBBLE_PER_TILE_MIN = None
    RUBBLE_PER_TILE_MAX = None
    SIM_UPDATE_FREQ = 0
    def __init__(self):
        self.HasRemainingRubble = False
        self.RubbleMaterialProto = None

class IEntityConstructionProgress:
    def __init__(self):
        self.Entity = None
        self.IsPriority = False
        self.StandardPriorityOverride = 0
        self.Buffers = None
        self.TotalCost = None
        self.AlreadyRemovedCost = None
        self.CurrentSteps = 0
        self.MaxSteps = 0
        self.ExtraSteps = 0
        self.Progress = None
        self.IsNearlyFinished = False
        self.WasBlockedOnProductsLastSim = False
        self.IsDeconstruction = False
        self.IsUpgrade = False
        self.IsPaused = False

class EntityConstructionProgress:
    def __init__(self):
        self.Entity = None
        self.IsPriority = False
        self.StandardPriorityOverride = 0
        self.AlreadyRemovedCost = None
        self.Buffers = None
        self.ConstructionBuffers = None
        self.TotalCost = None
        self.Progress = None
        self.IsNearlyFinished = False
        self.CurrentSteps = 0
        self.MaxSteps = 0
        self.ExtraSteps = 0
        self.AlreadyProcessedSteps = 0
        self.AllowedSteps = 0
        self.IsInExtraStepPhase = False
        self.IsDone = False
        self.WasBlockedOnProductsLastSim = False
        self.IsDeconstruction = False
        self.IsUpgrade = False
        self.IsPaused = False
        self.TerrainDisruptionDisabled = False
        self.Owner = None
        self.WasFullyConstructed = False
        self.DurationPerProduct = None

class FreeConstructionManager:
    def __init__(self):
        self.EntityConstructionStateChanged = None
        self.DeconstructionRatio = None
        self.EntityConstructed = None
        self.EntityStartedDeconstruction = None
        self.EntityPauseStateChanged = None
        self.IsInstaBuildEnabled = False

class GlobalInputBuffer:
    def __init__(self):
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

class GlobalLogisticsInputBuffer:
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

class GlobalOutputBuffer:
    def __init__(self):
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

class GlobalLogisticsOutputBuffer:
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

class IConstructionManager:
    def __init__(self):
        self.DeconstructionRatio = None
        self.IsInstaBuildEnabled = False
        self.EntityConstructed = None
        self.EntityStartedDeconstruction = None
        self.EntityConstructionStateChanged = None
        self.EntityPauseStateChanged = None

class ConstructionState:
    NotInitialized = None
    NotStarted = None
    InConstruction = None
    Constructed = None
    PreparingUpgrade = None
    BeingUpgraded = None
    PendingDeconstruction = None
    InDeconstruction = None
    Deconstructed = None
    Invalid = None
    def __init__(self):
        self.value__ = 0

class IEntityWithMultipleProductsToAssign:
    def __init__(self):
        self.BuffersPerSlot = None
        self.SupportedProducts = None
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.ConstructionState = None
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AlwaysUseCustomPfTargetTiles = False
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None

class IEntityWithQuickRemove:
    def __init__(self):
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None

class QuickRemoveFromEntityCmd:
    def __init__(self):
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.AffectsSaveState = False
        self.IsVerificationCmd = False
        self.Result = None
        self.HasError = False
        self.ErrorMessage = ""
        self.EntityId = None

class ILayoutEntity:
    def __init__(self):
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.ConstructionState = None
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AlwaysUseCustomPfTargetTiles = False
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None

class IProductBufferReadOnly:
    def __init__(self):
        self.Product = None
        self.UsableCapacity = None
        self.Capacity = None
        self.Quantity = None

class IProductBuffer:
    def __init__(self):
        self.Product = None
        self.UsableCapacity = None
        self.Capacity = None
        self.Quantity = None

class IProductBufferReadOnlyExtensions:
    def __init__(self):
        pass


class ProductBufferExtensions:
    def __init__(self):
        pass


class IStaticEntity:
    def __init__(self):
        self.Prototype = None
        self.CenterTile = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.ConstructionState = None
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AlwaysUseCustomPfTargetTiles = False
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None

class IStaticEntityExtensions:
    def __init__(self):
        pass


class ConstrCubeSpec:
    def __init__(self):
        self.Position = None
        self.Height = None
        self.Volume = None
        self.ScaleX = None
        self.ScaleY = None
        self.ScaleZ = None
        self.TransitionHeightTiles = None

class IEntityAssignedAsOutput:
    def __init__(self):
        self.AssignedInputs = None
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.ConstructionState = None
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AlwaysUseCustomPfTargetTiles = False
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None

class IEntityAssignedAsInput:
    def __init__(self):
        self.AssignedOutputs = None
        self.AllowNonAssignedOutput = False
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.ConstructionState = None
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AlwaysUseCustomPfTargetTiles = False
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None

class IStaticEntityWithQueue:
    def __init__(self):
        pass


class IVirtualBufferProvider:
    def __init__(self):
        self.ProvidedProducts = None

class IVirtualBuffersMap:
    def __init__(self):
        pass


class OccupiedTerrainVertexManger:
    def __init__(self):
        pass


    class TileVertexData:
        def __init__(self):
            self.Entity = None
            self.VertexIndex = 0

class OceanAreaRecoverHelper:
    def __init__(self):
        pass


class RecoverOceanAccessCmd:
    def __init__(self):
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.AffectsSaveState = False
        self.IsVerificationCmd = False
        self.Result = None
        self.HasError = False
        self.ErrorMessage = ""
        self.EntityWithOceanAreas = None
        self.MaxTilesToRecover = 0

class RecoverOceanResult:
    def __init__(self):
        self.BlockedByTerrainCount = 0
        self.BlockingEntityId = None

class ProductBuffer:
    def __init__(self):
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

class DisableQuickBuildParam:
    Instance = None
    def __init__(self):
        self.AllowedProtoType = None

class ProtoWithReservedOceanValidator:
    def __init__(self):
        self.Priority = None

class OceanAreaValidationMetadata:
    def __init__(self):
        self.Area = None
        self.Color = None
        self.StripesScale = 0.0
        self.StripesAngle = 0.0

class IStaticEntityWithReservedOcean:
    def __init__(self):
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.ConstructionState = None
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AlwaysUseCustomPfTargetTiles = False
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None

class IProtoWithReservedOcean:
    def __init__(self):
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.IsUnique = False
        self.AutoBuildMiniZippers = False
        self.CanMoveUpDownWhenInvalidPlacement = False
        self.Graphics = None
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsInitialized = False
        self.IsObsolete = False
        self.Mod = None

class ReservedOceanAreaState:
    MAX_AREAS_IN_SET = 0
    def __init__(self):
        self.AreasSetsValidity = None
        self.HasAnyValidAreaSet = False
        self.FirstValidAreasSetIndex = 0
        self.AreasSetsValidityChanged = None
        self.IsNoValidAreasNotificationActive = False
        self.Proto = None
        self.AreasSets = None

class IStaticEntityWithReservedOceanV2:
    def __init__(self):
        from Mafi import Option
        self.ReservedOceanAreaStateV2 = Option()
        self.OceanAreaRequired = None
        self.OceanAreaDesired = None
        self.OceanAreaBlocked = None
        self.TileRequiredPathable = None
        self.PathabilityMask = None
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.ConstructionState = None
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AlwaysUseCustomPfTargetTiles = False
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None

class IProtoWithReservedOceanV2:
    def __init__(self):
        self.MinGroundHeight = None
        self.MaxGroundHeight = None
        self.PathabilityQueryMask = None
        self.ShipHeightClass = None
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.IsUnique = False
        self.AutoBuildMiniZippers = False
        self.CanMoveUpDownWhenInvalidPlacement = False
        self.Graphics = None
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsInitialized = False
        self.IsObsolete = False
        self.Mod = None

class ReservedOceanAreaStateV2:
    def __init__(self):
        self.AreaRequired = None
        self.AreaDesired = None
        self.TileRequiredPathable = None
        self.IsOpen = False
        self.PathabilityMask = None
        self.Entity = None

class IEntityWithNoCollapse:
    def __init__(self):
        pass


class IEntityWithCustomTerrainInteraction:
    def __init__(self):
        pass


class StaticEntitiesTerrainInteractionManager:
    UPDATE_FREQ_TICKS = None
    TOLERANCE = None
    def __init__(self):
        self.Priority = None

class StaticEntity:
    def __init__(self):
        self.Prototype = None
        self.CenterTile = None
        self.Position2f = None
        self.Position3f = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.PfTargetTiles = None
        self.AlwaysUseCustomPfTargetTiles = False
        self.ConstructionState = None
        self.IsConstructed = False
        self.IsNotConstructed = False
        self.IsBeingUpgraded = False
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.DoNotAdjustTerrainDuringConstruction = False
        self.AreConstructionCubesDisabled = False
        self.Id = None
        self.DefaultTitle = None
        self.Context = None
        self.IsDestroyed = False
        self.CanBePaused = False
        self.IsEnabled = False
        self.IsNotEnabled = False
        self.IsPaused = False
        self.IsNotPaused = False
        self.RendererData = None

class StaticEntityOceanReservationManager:
    def __init__(self):
        pass


class IOceanAreaRecord:
    def __init__(self):
        self.Entity = None
        self.Area = None
        self.SetIndex = 0
        self.AreaIndex = 0
        self.NonOceanTilesIndices = None
        self.NonOceanTiles = 0

class StaticEntityOceanReservationManagerV2:
    MAX_OCEAN_FLOOR_HEIGHT = None
    MIN_OCEAN_DEPTH = 0
    def __init__(self):
        self.MonitoredAreasCount = 0

class StaticEntityPfTargetTiles:
    Empty = None
    def __init__(self):
        self.TilesCount = 0

class IStaticEntityProto:
    def __init__(self):
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsInitialized = False
        self.IsObsolete = False
        self.Mod = None

class StaticEntityProto:
    def __init__(self):
        self.IsUnique = False
        self.CloningDisabled = False
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.EntityType = None
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
        self.ConstructionDurationPerProduct = None
        self.CollapseRubbleScale = None
        self.CustomBuriedTolerance = None
        self.CustomSuspendedTolerance = None
        self.VehicleGoalHeightAllowedRange = None
        self.CannotBeBuiltByPlayer = False
        self.CannotBeDestroyedByFlood = False
        self.DoNotStartConstructionAutomatically = False
        self.Graphics = None
        self.IsPhantom = False

    class ID:
        def __init__(self):
            self.Value = ""

    class Gfx:
        Empty = None
        def __init__(self):
            self.HideBlockedPortsIcon = False
            self.Color = None
            self.RendererIndex = 0

class UniqueEntityValidator:
    def __init__(self):
        self.Priority = None

class UpgradeHelper:
    def __init__(self):
        pass


class IUpgradesManager:
    def __init__(self):
        pass


class UpgradesManager:
    def __init__(self):
        pass


class ReplacementRefund:
    def __init__(self):
        self.ToRefundAfterDone = None
        self.QuickRemoveRefund = False
        self.Entity = None
        self.Buffers = None

class OngoingReplacementData:
    def __init__(self):
        self.State = None
        self.NewProto = None
        from Mafi import Option
        self.ConfigToApply = Option()
        self.ApplyConfiguration = False

class VirtualBuffersMap:
    Empty = None
    def __init__(self):
        pass

