class EntitiesCommandsProcessor:

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

    def __init__(self):
        pass

class EntityRemoveReason:

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

    def __init__(self):
        pass

class IEntityProto:

    def __init__(self):
        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.Id = None
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.Mod = None
class EntityProto:

    def __init__(self):
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
class ID:

    def __init__(self):
        pass

class Gfx:

    def __init__(self):
        pass

class EntityValidators:

    def __init__(self):
        pass

class IAssignableToFuelStation:

    def __init__(self):
        self.AssignedFuelStations = None
class IDynamicCostProvider:

    def __init__(self):
        self.ManagedProtoType = None
class IEntity:

    def __init__(self):
        self.DefaultTitle = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
class IRenderedEntity:

    def __init__(self):
        self.RendererData = None
        self.DefaultTitle = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
class IEntityWithAdditionRequest:

    def __init__(self):
        self.DefaultTitle = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
class IEntityWithPosition:

    def __init__(self):
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.DefaultTitle = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
class IAreaSelectableEntity:

    def __init__(self):
        self.RendererData = None
        self.DefaultTitle = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
class IAreaSelectableStaticEntity:

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
class IAnimatedEntity:

    def __init__(self):
        self.AnimationParams = None
        self.AnimationStatesProvider = None
        self.DefaultTitle = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
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
        self.DefaultTitle = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
class IEntityEnforcingAssignedVehicles:

    def __init__(self):
        self.AreOnlyAssignedVehiclesAllowed = False
        self.Position2f = None
        self.AllVehicles = None
        self.DefaultTitle = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
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

class IEntityWithCustomTitle:

    def __init__(self):
        from Mafi import Option
        self.CustomTitle = Option()
        self.DefaultTitle = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
class EntityNameExtensions:

    def __init__(self):
        pass

class IEntityWithEmission:

    def __init__(self):
        self.EmissionIntensity = None
        self.DefaultTitle = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
class IEntityWithLogisticsControl:

    def __init__(self):
        self.CanDisableLogisticsInput = False
        self.CanDisableLogisticsOutput = False
        self.LogisticsInputMode = None
        self.LogisticsOutputMode = None
        self.DefaultTitle = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
class IEntityWithSimpleLogisticsControl:

    def __init__(self):
        self.LogisticsInputControl = None
        self.LogisticsOutputControl = None
        self.IsLogisticsInputDisabled = False
        self.IsLogisticsOutputDisabled = False
        self.DefaultTitle = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
class LogisticsControl:

    def __init__(self):
        pass

class IEntityWithMaxServiceRadius:

    def __init__(self):
        self.MaxServiceRadius = None
        self.DefaultTitle = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
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
class IEntityWithParticles:

    def __init__(self):
        self.AreParticlesEnabled = False
        self.DefaultTitle = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
class IEntityWithSimUpdate:

    def __init__(self):
        self.DefaultTitle = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
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

    def __init__(self):
        pass

class IUpgradableEntity:

    def __init__(self):
        self.Upgrader = None
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
class IUpgrader:

    def __init__(self):
        self.PriceToUpgrade = None
        self.ConstructionCostToUpgrade = None
        self.UpgradeExists = False
        self.UpgradeTitle = None
        from Mafi import Option
        self.NextTier = Option()
        self.Icon = str(0)
class ParticlesParams:

    def __init__(self):
        pass

class TileSurfaceCopyPasteData:

    def __init__(self):
        pass

