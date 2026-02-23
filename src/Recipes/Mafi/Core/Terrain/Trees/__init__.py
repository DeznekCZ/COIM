
class TreeId:
    Invalid = None
    def __init__(self):
        self.IsValid = False
        self.Position = None

class DesignateHarvestedTreesCmd:
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
        self.Area = None
        self.AddToHarvest = False
        self.HarvestedProductId = None

class ForestFloorTerrainPostProcessor:
    def __init__(self):
        pass


class PrepareManualPlantTreeCmd:
    def __init__(self):
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.AffectsSaveState = False
        self.IsVerificationCmd = False
        self.Result = False
        self.HasError = False
        self.ErrorMessage = ""
        from Mafi.Core.Prototypes import Proto
        self.ProtoId = Proto.ID()

        self.Transform = None

class RemoveManualPlantTreeCmd:
    def __init__(self):
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.AffectsSaveState = False
        self.IsVerificationCmd = False
        self.Result = False
        self.HasError = False
        self.ErrorMessage = ""
        self.Id = None

class TreeData:
    def __init__(self):
        self.IsValid = False
        from Mafi.Core.Products import ProductProto
        self.HarvestedProductId = ProductProto.ID()

        self.Position2i = None
        self.Position2f = None
        self.Position3f = None
        self.Id = None
        self.Proto = None
        self.CreatedByTerrainGenerator = False
        self.PlantedAtHeight = None
        self.PlantedAtTick = None

class TreeDataBase:
    def __init__(self):
        self.Proto = None
        self.Position = None
        self.Rotation = None
        self.Scale = None

class TreesManager:
    GENERATED_TREE_PLANTED_AT_TICK = None
    from Mafi import Fix32
    STUMP_SINK_RATE_PER_MONTH = Fix32()
    MAX_FLOOR_THICKNESS_TOTAL = None
    def __init__(self):
        self.TreeAdded = None
        self.TreeRemoved = None
        self.StumpAdded = None
        self.StumpRemoved = None
        self.TreePreviewAdded = None
        self.TreePreviewRemoved = None
        self.TreeAddedToHarvest = None
        self.TreeRemovedFromHarvest = None
        self.ManualTreePlaced = None
        self.TreeCollapsed = None
        self.Trees = None
        self.TreesCount = 0
        self.Stumps = None
        self.PreviewTrees = None
        self.SelectedToHarvestCount = 0
        self.ReservedCount = 0
        self.Item = None

    class ITreesChunk:
        def __init__(self):
            self.Origin = None
            self.TreesNotSelected = None
            self.TreesSelectedToHarvest = None
            self.ReservedTrees = None

class TreeStumpData:
    def __init__(self):
        self.IsValid = False
        self.Position2f = None
        self.Position3f = None
        self.Id = None
        self.TreeProto = None
        self.Scale = None
        self.PlantedAtHeight = None
        self.CreatedAtTick = None
        self.TreePlantedAtTick = None

class ForestProto:
    def __init__(self):
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

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
        self.ForestFloorMaterial = None
        self.Trees = None
        self.IsPhantom = False

class ITreesManager:
    def __init__(self):
        self.TreesCount = 0
        self.Trees = None
        self.PreviewTrees = None
        self.Stumps = None
        self.StumpAdded = None
        self.StumpRemoved = None
        self.TreeAdded = None
        self.TreeRemoved = None
        self.TreeAddedToHarvest = None
        self.TreeRemovedFromHarvest = None
        self.ManualTreePlaced = None
        self.SelectedToHarvestCount = 0
        self.Item = None
        self.TreePreviewAdded = None
        self.TreePreviewRemoved = None

class ITreeHarvestingManager:
    def __init__(self):
        self.Item = None

class ITreePlantingManager:
    def __init__(self):
        self.TreePreviewAdded = None
        self.TreePreviewRemoved = None

class TreePlantingGroupProto:
    def __init__(self):
        self.ProductWhenHarvested = None
        self.QuantityFormatter = None
        self.IconPath = ""
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

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
        self.Trees = None
        self.YieldAt40PercentGrowth = None
        self.YieldAt60PercentGrowth = None
        self.YieldAt80PercentGrowth = None
        self.QuantityAt40PercentGrowth = None
        self.QuantityAt60PercentGrowth = None
        self.QuantityAt80PercentGrowth = None
        self.IsPhantom = False

class TreePlantingValidator:
    def __init__(self):
        self.Priority = None

class TreeProto:
    MAX_TREE_SPACING = 0
    MAX_BASE_SCALE_DEVIATION = None
    def __init__(self):
        self.Type = None
        self.EntityType = None
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.QuantityFormatter = None
        from Mafi import Option
        self.ForestProto = Option()
        self.TreePlantingGroupProto = None
        self.Graphics = None
        self.TreeGraphics = None
        self.IconPath = ""
        self.MapEditorIconPath = ""
        self.Layout = None
        self.RendererId = 0
        self.Costs = None
        self.Ports = None
        self.CannotBeReflected = False
        self.IsUnique = False
        self.AutoBuildMiniZippers = False
        self.CanMoveUpDownWhenInvalidPlacement = False
        self.ProductWhenHarvested = None
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
        self.AgeAtMaxGrowthBase = None
        self.MaxStumpAgeBase = None
        self.BaseScaleStdDeviation = None
        self.MinForestFloorRadius = None
        self.MaxForestFloorRadius = None
        self.SpacingToOtherTree = 0
        self.IsDry = False
        self.ForestFloorMaterial = Option()
        self.IsPhantom = False

    class TreeGfx:
        Empty = None
        def __init__(self):
            self.PrefabPaths = None
            self.TintColors = None
            from Mafi import Option
            self.TrimmedTreePrefabPath = Option()
            self.TrimmedTreeLength = None
            self.MapEditorIconPath = ""

class TreePrefabs:
    def __init__(self):
        self.TreePrefabPath = ""
        self.TreeCutPrefabPath = ""
        self.StumpPrefabPath = ""
