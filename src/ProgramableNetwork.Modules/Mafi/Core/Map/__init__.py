class CellEdge:

    def __init__(self):
        self.CenterTile = None
class ChunkHeightFromCellsProcessor:

    def __init__(self):
        pass

class IIslandMapGenerator:

    def __init__(self):
        self.Name = str(0)
class IslandMapGeneratorConfig:

    def __init__(self):
        self.IslandMapGeneratorType = None
class IStartLocationProvider:

    def __init__(self):
        self.StartingLocation = None
class NewMapStartLocationProvider:

    def __init__(self):
        self.StartingLocation = None
class IslandMap:

    def __init__(self):
        self.CellCoastLines = None
        self.Item = None
        self.StartingLocation = None
        self.Name = str(0)
        self.MapVersion = int(0)
class StartingLocation:

    def __init__(self):
        pass

class IslandMapExtensions:

    def __init__(self):
        pass

class TerrainPropMapData:

    def __init__(self):
        pass

class IslandMapConfig:

    def __init__(self):
        pass

class IslandMapDifficultyConfig:

    def __init__(self):
        self.MineableResourceSizeBonus = None
        self.CellHeightsBias = None
class MapCell:

    def __init__(self):
        self.IslandMap = None
        self.CenterTile = None
        self.CenterTileWithHeight = None
        self.Neighbors = None
        self.ValidNeighbors = None
        self.IsNotOnMapBoundary = False
        self.IsOcean = False
        self.IsNotOcean = False
        self.IsNextToOcean = False
        self.Chunks = None
        self.OuterRadius = None
        self.InnerRadius = None
        self.State = None
        from Mafi import Option
        self.EdgeTerrainFactory = Option()
        self.IsUnlocked = False
        self.IsAvailableToUnlock = False
        self.GroundHeight = None
        self.SurfaceGenerator = None
        self.IsUnlockedByDefault = False
class MapCellEdge:

    def __init__(self):
        self.CenterPoint = None
class MapCellState:

    def __init__(self):
        pass

class IMapCellFriend:

    def __init__(self):
        pass

class IMapCellGeneratorFriend:

    def __init__(self):
        pass

class IMapCellEdgeTerrainFactory:

    def __init__(self):
        pass

class MapCellsGenerator:

    def __init__(self):
        pass

class MapCellSurfaceGeneratorProto:

    def __init__(self):
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

class MapCellSurfaceGenerator:

    def __init__(self):
        self.Proto = None
class MapGenerationException:

    def __init__(self):
        self.Message = str(0)
        self.Data = None
        self.InnerException = None
        self.TargetSite = None
        self.StackTrace = str(0)
        self.HelpLink = str(0)
        self.Source = str(0)
        self.HResult = int(0)
class MapManager:

    def __init__(self):
        pass

class ProceduralIslandMapGenerator:

    def __init__(self):
        self.Name = str(0)
class ProceduralIslandMapGeneratorConfig:

    def __init__(self):
        self.MapRandomSeed = str(0)
        self.BedrockMaterialId = None
        from Mafi import Fix32
        self.AreaChunksSquaredApprox = Fix32()
        self.MinCellDiameter = None
        self.OceanCellDiameter = None
        self.CellMinMaxDiameterMult = int(0)
        self.CellExpansionTrials = int(0)
        from Mafi import Fix32
        self.CellSizeGrowthFromStartExp = Fix32()
        self.CellSizeGrowthFromCoastExp = float(0)
        self.MinCellHeight = None
        self.CellHeightDiffMean = None
        self.CellHeightDiffStdDev = None
        from Mafi import Fix32
        self.CellHeightMeanDistanceToStartMult = Fix32()
        self.CellHeightDiffMaxStdDev = None
        self.OceanShape = None
        from Mafi import Fix32
        self.StartingTerrainResourcesAmountMult = Fix32()
        self.ResourcesRichnessMultDistance = None
        from Mafi import Fix32
        self.ResourcesRichnessMultExpBase = Fix32()
        self.DefaultIslandCellSurfaceId = None
        self.DefaultOceanCellSurfaceId = None
        self.DefaultCliffMaterialId = None
class OceanShape:

    def __init__(self):
        pass

class SimpleVirtualResource:

    def __init__(self):
        self.Name = str(0)
        self.Priority = int(0)
        self.Product = None
        self.ConfiguredCapacity = None
        self.Quantity = None
        self.EmergencyQuantity = None
        self.Capacity = None
        self.Position = None
        self.MaxRadius = None
        self.ResourceColor = None
class SquareMapGenerator:

    def __init__(self):
        self.Name = str(0)
class SquareMapGeneratorConfig:

    def __init__(self):
        self.BedrockMaterialId = None
        self.TerrainWidth = int(0)
        self.TerrainHeight = int(0)
        self.OceanAtDirection = None
        self.OceanSize = int(0)
        self.GroundHeight = None
        self.SurfaceProtoId = None
        self.ForestProtoId = None
        self.ForestArea = None
class StaticIslandMapProvider:

    def __init__(self):
        self.Name = str(0)
class IStaticIslandMap:

    def __init__(self):
        self.Name = str(0)
class StaticIslandMapProviderConfig:

    def __init__(self):
        self.IslandMapType = None
        self.StartingLocation = None
        self.PreviewData = None
class StaticIslandMapPreviewData:

    def __init__(self):
        pass

class IslandMapDifficulty:

    def __init__(self):
        pass

class TestMapGenerator:

    def __init__(self):
        self.Name = str(0)
class TestMapGeneratorConfig:

    def __init__(self):
        self.BedrockMaterialId = None
        self.CellSurfaceIds = None
        self.CliffProtoId = None
        self.OceanHeight = None
        self.LandHeight = None
        self.ExtraTopCellsSurfaceId = None
        self.ExtraTopCellsHeight = None
        self.PrimaryForestProtoId = None
        self.SecondaryForestProtoId = None
        self.MineableResources = None
        self.MineableResourcesOnCliff = None
        self.TerrainPostProcessors = None
class TestMapResource:

    def __init__(self):
        pass

class UnlockMapCellCmd:

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
class MapCellId:

    def __init__(self):
        pass

