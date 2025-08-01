class DefaultLogisticsModeManager:

    def __init__(self):
        self.DisableLogisticsOnPlacement = False
class ToggleDefaultLogisticsMode:

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
class EntitiesCommandsProcessor:
    UnityPerSurfaceTile = None

    def __init__(self):
        pass

class PlanningModeManager:

    def __init__(self):
        self.IsPlanningModeEnabled = False
class SetPlanningModeEnabledCmd:

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
class UpgradeEntityCmd:

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
class ReplaceEntityCmd:

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
class QuickRemoveEntityRefundCmd:

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
class SetConstructionPausedCmd:

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
class StackerCommandsProcessor:

    def __init__(self):
        pass

class SetEntityNameCmd:

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
class ToggleEnabledCmd:

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
class ToggleEnabledGroupCmd:

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
class SetEntityEnabledCmd:

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
class DefaultEntityFactory:

    def __init__(self):
        pass

class DrawArrowWileBuildingProtoParam:

    def __init__(self):
        self.AllowedProtoType = None
class EmissionParams:

    def __init__(self):
        pass

class EntitiesBuilder:

    def __init__(self):
        pass

class EntitiesCloneConfigHelper:

    def __init__(self):
        pass

class EntitiesCreator:

    def __init__(self):
        pass

class EntityAddReason:
    New = None
    Move = None

    def __init__(self):
        pass

class EntityRemoveReason:
    Remove = None
    Collapse = None

    def __init__(self):
        pass

class IEntitiesManager:

    def __init__(self):
        self.EntityAdded = None
        self.EntityAddedFull = None
        self.EntityRemoved = None
        self.EntityRemovedFull = None
        self.StaticEntityAdded = None
        self.StaticEntityRemoved = None
        self.OnUpgradeToBePerformed = None
        self.OnUpgradeJustPerformed = None
        self.EntityPauseStateChanged = None
        self.EntityEnabledChanged = None
        self.OnEntityVisualChanged = None
        self.Entities = None
class EntitiesManager:

    def __init__(self):
        self.EntityAdded = None
        self.EntityAddedFull = None
        self.StaticEntityAdded = None
        self.EntityRemoved = None
        self.EntityRemovedFull = None
        self.StaticEntityRemoved = None
        self.EntityPauseStateChanged = None
        self.EntityEnabledChanged = None
        self.OnUpgradeToBePerformed = None
        self.OnUpgradeJustPerformed = None
        self.OnEntityVisualChanged = None
        self.Entities = None
        self.EntitiesCount = int(0)
class Entity:

    def __init__(self):
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
class EntityConfigData:

    def __init__(self):
        from Mafi import Option
        self.EntityType = Option()
        from Mafi import Option
        self.ProtoModName = Option()
        self.Transform = None
        from Mafi import Option
        self.Trajectory = Option()
        self.Pillars = None
        self.LogisticsInputMode = None
        self.LogisticsOutputMode = None
        self.IsLogisticsInputDisabled = None
        self.IsLogisticsOutputDisabled = None
        self.GeneralPriority = None
        self.ElectricityGenerationPriority = None
        self.IsElectricitySurplusGenerator = None
        self.IsElectricitySurplusConsumer = None
        from Mafi import Option
        self.CustomTitle = Option()
        self.IsPaused = None
        self.IsConstructionPaused = None
        self.AssignedVehicles = None
        self.MultipleBuffers = None
        self.NotifyOnFullBuffer = None
        self.FuelImportUntilPercent = None
        self.FuelExportFromPercent = None
        self.Recipes = None
        self.AllowNonAssignedOutput = None
        self.AssignedOutputs = None
        self.AssignedInputs = None
        self.NotifyOnLowReserve = None
        self.AllowedProducts = None
        self.AssignedProducts = None
        self.TrackDirection = None
        self.TrackPillarBlocksBitmap = None
        self.TrackFlags = None
        self.TrackCriticalBlocksBitmap = None
        self.TrackSuperBlocksBitmap = None
        self.TrackSuperBlocksExplicitBitmap = None
class EntityIdsHolder:

    def __init__(self):
        self.MappedIds = None
class ConfigSerializationContext:

    def __init__(self):
        pass

class EntityContext:

    def __init__(self):
        pass

class EntityLogisticsMode:
    Auto = None
    On = None
    Off = None

    def __init__(self):
        pass

class IEntityProto:

    def __init__(self):
        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.Id = None
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsInitialized = False
        self.Mod = None
class EntityProto:

    def __init__(self):
        self.EntityType = None
        self.Costs = None
        self.Id = None
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
class ID:

    def __init__(self):
        pass

class Gfx:
    Empty = None

    def __init__(self):
        pass

class EntityValidators:

    def __init__(self):
        pass

class IAssignableToFuelStation:

    def __init__(self):
        self.AssignedFuelStations = None
class IEntity:

    def __init__(self):
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
class IRenderedEntity:

    def __init__(self):
        self.RendererData = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
class IEntityWithAdditionRequest:

    def __init__(self):
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
class IEntityWithPosition:

    def __init__(self):
        self.Position2f = None
        self.Position3f = None
class IAreaSelectableEntity:

    def __init__(self):
        self.RendererData = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
class IAreaSelectableStaticEntity:

    def __init__(self):
        self.Prototype = None
        self.CenterTile = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.ConstructionCost = None
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
class IAnimatedEntity:

    def __init__(self):
        self.AnimationParams = None
        self.AnimationStatesProvider = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
class AnimationStatesProvider:

    def __init__(self):
        self.AnimationStates = None
class AnimationState:

    def __init__(self):
        pass

class EntityExtensions:

    def __init__(self):
        pass

class IEntityAssignedWithVehicles:

    def __init__(self):
        self.Position2f = None
        self.AllVehicles = None
        self.ZoneMask = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
class IEntityEnforcingAssignedVehicles:

    def __init__(self):
        self.AreOnlyAssignedVehiclesAllowed = False
        self.Position2f = None
        self.AllVehicles = None
        self.ZoneMask = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
class AssignedVehiclesExtensions:

    def __init__(self):
        pass

class EntityEnforcingAssignedVehiclesExtensions:

    def __init__(self):
        pass

class IEntityObserver:

    def __init__(self):
        pass

class IEntityObserverForEnabled:

    def __init__(self):
        pass

class IEntityObserverForUpgrade:

    def __init__(self):
        pass

class IEntityObserverForPriority:

    def __init__(self):
        pass

class IEntityObserverForPorts:

    def __init__(self):
        pass

class IEntityWithCloneableConfig:

    def __init__(self):
        pass

class IObjectWithCustomTitle:

    def __init__(self):
        self.DefaultTitle = None
        from Mafi import Option
        self.CustomTitle = Option()
class EntityNameExtensions:

    def __init__(self):
        pass

class IEntityWithEmission:

    def __init__(self):
        self.EmissionIntensity = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
class IEntityWithLogisticsControl:

    def __init__(self):
        self.CanDisableLogisticsInput = False
        self.CanDisableLogisticsOutput = False
        self.LogisticsInputMode = None
        self.LogisticsOutputMode = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
class IEntityWithSimpleLogisticsControl:

    def __init__(self):
        self.LogisticsInputControl = None
        self.LogisticsOutputControl = None
        self.IsLogisticsInputDisabled = False
        self.IsLogisticsOutputDisabled = False
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
class LogisticsControl:
    Enabled = None
    DisabledButVisible = None
    NotAvailable = None

    def __init__(self):
        pass

class IEntityWithMaxServiceRadius:

    def __init__(self):
        self.MaxServiceRadius = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
class IEntityWithOutputToTerrain:

    def __init__(self):
        self.DumpHeightOffset = None
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.ConstructionCost = None
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
class IEntityWithParticles:

    def __init__(self):
        self.AreParticlesEnabled = False
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
class IEntityWithSimUpdate:

    def __init__(self):
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
class IEntityWithSound:

    def __init__(self):
        self.IsSoundOn = False
        self.SoundParams = None
class SoundParams:

    def __init__(self):
        self.SoundPrefabPath = str(0)
        self.Significance = None
        self.Loop = False
        self.FadeOnChange = False
        self.DoNotLimit = False
class SoundSignificance:
    VerySmall = None
    Small = None
    Normal = None
    Medium = None
    High = None

    def __init__(self):
        pass

class IObjectWithNestedNotificationTarget:

    def __init__(self):
        pass

class IObjectWithTitle:

    def __init__(self):
        self.DefaultTitle = None
class IUpgradableEntity:

    def __init__(self):
        self.UpgradableProto = None
        self.Prototype = None
        self.CenterTile = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.ConstructionCost = None
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
class IEntityWithReplaceVerification:

    def __init__(self):
        pass

class IUpgrader:

    def __init__(self):
        pass

class ParticlesParams:

    def __init__(self):
        pass

class TileSurfaceCopyPasteData:

    def __init__(self):
        pass

