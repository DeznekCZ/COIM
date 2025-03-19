class TreeId:

    def __init__(self):
        self.IsValid = False
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
class TreeData:

    def __init__(self):
        self.IsValid = False
        self.HarvestedProductId = None
        self.Position = None
class TreeDataBase:

    def __init__(self):
        pass

class TreesManager:

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
        self.TreesCount = int(0)
        self.Stumps = None
        self.PreviewTrees = None
        self.SelectedToHarvestCount = int(0)
        self.ReservedCount = int(0)
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
class ForestProto:

    def __init__(self):
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class ITreesManager:

    def __init__(self):
        self.TreesCount = int(0)
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
        self.SelectedToHarvestCount = int(0)
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
        self.IconPath = str(0)
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class TreePlantingValidator:

    def __init__(self):
        self.Priority = None
class TreeProto:

    def __init__(self):
        self.Type = None
        self.EntityType = None
        self.Id = None
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
        self.IconPath = str(0)
        self.MapEditorIconPath = str(0)
        self.Layout = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class TreeGfx:

    def __init__(self):
        pass

