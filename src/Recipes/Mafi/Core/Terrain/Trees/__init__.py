
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

        self.Position = None
        self.Id = None
        self.Proto = None
        self.PositionWithinTile = None
        self.Rotation = None
        self.BaseScale = None
        self.CreatedByTerrainGenerator = False
        self.PlantedAtHeight = None
        self.PlantedAtTick = 0

class TreeDataBase:
    def __init__(self):
        self.Proto = None
        self.Position = None
        self.Rotation = None
        self.Scale = None

class TreesManager:
    GENERATED_TREE_PLANTED_AT_TICK = 0
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
        self.Position = None
        self.Id = None
        self.TreeProto = None
        self.PositionWithinTile = None
        self.Rotation = None
        self.Scale = None
        self.PlantedAtHeight = None
        self.CreatedAtTick = None

class ForestProto:
    def __init__(self):
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.ForestFloorMaterial = None
        self.Trees = None
        self.IsPhantom = False
        self.IsInitialized = False

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
        self.TimeTo40PercentGrowth = None
        self.TimeTo60PercentGrowth = None
        self.TimeTo80PercentGrowth = None
        self.TimeTo100PercentGrowth = None
        self.QuantityFormatter = None
        self.IconPath = ""
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.Trees = None
        self.Graphics = None
        self.IsPhantom = False
        self.IsInitialized = False

class TreePlantingValidator:
    def __init__(self):
        self.Priority = None

class TreeProto:
    MAX_TREE_SPACING = 0
    MAX_BASE_SCALE_DEVIATION = None
    Percent20 = None
    Percent40 = None
    Percent60 = None
    Percent80 = None
    def __init__(self):
        self.Type = None
        self.EntityType = None
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.Costs = None
        self.Ports = None
        self.CannotBeReflected = False
        self.IsUnique = False
        self.AutoBuildMiniZippers = False
        self.ProductWhenHarvested = None
        self.QuantityFormatter = None
        from Mafi import Option
        self.ForestProto = Option()
        self.TreePlantingGroupProto = None
        self.Graphics = None
        self.TreeGraphics = None
        self.IconPath = ""
        self.MapEditorIconPath = ""
        self.Layout = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.BaseScaleStdDeviation = None
        self.MinForestFloorRadius = None
        self.MaxForestFloorRadius = None
        self.SpacingToOtherTree = 0
        self.IsDry = False
        self.ForestFloorMaterial = Option()
        self.IsPhantom = False
        self.IsInitialized = False

    class TreeGfx:
        Empty = None
        def __init__(self):
            self.PrefabPaths = None
            from Mafi import Option
            self.TrimmedTreePrefabPath = Option()
            self.TrimmedTreeLength = None
            self.MapEditorIconPath = ""
