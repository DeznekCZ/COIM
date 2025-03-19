class ClearingChecker:

    def __init__(self):
        self.Priority = None
class OccupiedColumn:

    def __init__(self):
        pass

class ConstructionManager:

    def __init__(self):
        self.DeconstructionRatio = None
        self.EntityConstructed = None
        self.EntityPauseStateChanged = None
        self.OnResetConstructionAnimationState = None
        self.EntityConstructionNearlyFinished = None
        self.EntityStartedDeconstruction = None
        self.EntityConstructionStateChanged = None
class ConstructionProgress:

    def __init__(self):
        self.AlreadyRemovedCost = None
        self.Buffers = None
        self.ConstructionBuffers = None
        self.TotalCost = None
        self.Progress = None
        self.IsNearlyFinished = False
        self.CurrentSteps = int(0)
        self.MaxSteps = int(0)
        self.ExtraSteps = int(0)
        self.AlreadyProcessedSteps = int(0)
        self.AllowedSteps = int(0)
        self.IsInExtraStepPhase = False
        self.IsDone = False
        self.WasBlockedOnProductsLastSim = False
        self.IsDeconstruction = False
        self.IsUpgrade = False
        self.IsPaused = False
        self.TerrainDisruptionDisabled = False
class QuickDeliverCostHelper:

    def __init__(self):
        pass

class IConstructionProgress:

    def __init__(self):
        self.Buffers = None
        self.TotalCost = None
        self.AlreadyRemovedCost = None
        self.CurrentSteps = int(0)
        self.MaxSteps = int(0)
        self.ExtraSteps = int(0)
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

    def __init__(self):
        self.HasRemainingRubble = False
class IEntityConstructionProgress:

    def __init__(self):
        self.Entity = None
        self.IsPriority = False
        self.Buffers = None
        self.TotalCost = None
        self.AlreadyRemovedCost = None
        self.CurrentSteps = int(0)
        self.MaxSteps = int(0)
        self.ExtraSteps = int(0)
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
        self.AlreadyRemovedCost = None
        self.Buffers = None
        self.ConstructionBuffers = None
        self.TotalCost = None
        self.Progress = None
        self.IsNearlyFinished = False
        self.CurrentSteps = int(0)
        self.MaxSteps = int(0)
        self.ExtraSteps = int(0)
        self.AlreadyProcessedSteps = int(0)
        self.AllowedSteps = int(0)
        self.IsInExtraStepPhase = False
        self.IsDone = False
        self.WasBlockedOnProductsLastSim = False
        self.IsDeconstruction = False
        self.IsUpgrade = False
        self.IsPaused = False
        self.TerrainDisruptionDisabled = False
class FreeConstructionManager:

    def __init__(self):
        self.EntityConstructionStateChanged = None
        self.EntityConstructed = None
        self.EntityStartedDeconstruction = None
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
        self.CurrentQuantityPercent = int(0)
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
        self.CurrentQuantityPercent = int(0)
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
        self.EntityConstructed = None
        self.EntityStartedDeconstruction = None
        self.EntityConstructionStateChanged = None
class ConstructionState:

    def __init__(self):
        pass

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
        self.Value = None
        self.ConstructionCost = None
        self.ConstructionState = None
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.DefaultTitle = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
class ILayoutEntity:

    def __init__(self):
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.Value = None
        self.ConstructionCost = None
        self.ConstructionState = None
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.DefaultTitle = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
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
        self.Value = None
        self.ConstructionCost = None
        self.ConstructionState = None
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.DefaultTitle = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
class IStaticEntityExtensions:

    def __init__(self):
        pass

class ConstrCubeSpec:

    def __init__(self):
        pass

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
        self.Value = None
        self.ConstructionCost = None
        self.ConstructionState = None
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.DefaultTitle = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
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
        self.Value = None
        self.ConstructionCost = None
        self.ConstructionState = None
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.DefaultTitle = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
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
        pass

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
        self.ErrorMessage = str(0)
class RecoverOceanResult:

    def __init__(self):
        pass

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

    def __init__(self):
        self.AllowedProtoType = None
class ProtoWithReservedOceanValidator:

    def __init__(self):
        self.Priority = None
class OceanAreaValidationMetadata:

    def __init__(self):
        self.Area = None
        self.Color = None
        self.StripesScale = float(0)
        self.StripesAngle = float(0)
class IStaticEntityWithReservedOcean:

    def __init__(self):
        self.ReservedOceanAreaState = None
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.Value = None
        self.ConstructionCost = None
        self.ConstructionState = None
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.DefaultTitle = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
class IProtoWithReservedOcean:

    def __init__(self):
        self.ReservedOceanAreasSets = None
        self.MinGroundHeight = None
        self.MaxGroundHeight = None
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.IsUnique = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.Id = None
        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.Mod = None
class ReservedOceanAreaState:

    def __init__(self):
        self.AreasSetsValidity = None
        self.HasAnyValidAreaSet = False
        self.FirstValidAreasSetIndex = int(0)
        self.AreasSetsValidityChanged = None
        self.IsNoValidAreasNotificationActive = False
class StaticEntitiesTerrainInteractionManager:

    def __init__(self):
        self.Priority = None
class StaticEntity:

    def __init__(self):
        self.Prototype = None
        self.CenterTile = None
        self.Position2f = None
        self.Position3f = None
        self.Value = None
        self.ConstructionCost = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.PfTargetTiles = None
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
        self.MonitoredAreas = None
class IOceanAreaRecord:

    def __init__(self):
        self.Entity = None
        self.Area = None
        self.SetIndex = int(0)
        self.AreaIndex = int(0)
        self.NonOceanTilesIndices = None
        self.NonOceanTiles = int(0)
class StaticEntityPfTargetTiles:

    def __init__(self):
        self.TilesCount = int(0)
class IStaticEntityProto:

    def __init__(self):
        self.Id = None
        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.Mod = None
class StaticEntityProto:

    def __init__(self):
        self.Id = None
        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class ID:

    def __init__(self):
        pass

class Gfx:

    def __init__(self):
        pass

class UniqueEntityValidator:

    def __init__(self):
        self.Priority = None
class AllowProductDiscountInUpgrade:

    def __init__(self):
        self.AllowedProtoType = None
class UpgradeHelper:

    def __init__(self):
        pass

class UpgradeCostResolver:

    def __init__(self):
        pass

class IUpgradesManager:

    def __init__(self):
        pass

class UpgradesManager:

    def __init__(self):
        pass

class VirtualBuffersMap:

    def __init__(self):
        pass

