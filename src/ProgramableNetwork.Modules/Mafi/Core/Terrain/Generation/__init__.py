class Chunk64Area:

    def __init__(self):
        self.TotalChunksCount = int(0)
        self.Area2i = None
class CoastLinesData:

    def __init__(self):
        pass

class CustomTerrainPostProcessorV2:

    def __init__(self):
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.ParallelizationStrategy = None
        self.SortingPriority = int(0)
        self.PassCount = int(0)
        self.LastGenerationTime = None
class EncodedImageAndMatrix:

    def __init__(self):
        pass

class FlatTerrainGenerator:

    def __init__(self):
        self.Height = None
        self.TerrainWidth = int(0)
        self.TerrainHeight = int(0)
        self.Bedrock = None
        self.DoNotCreateOcean = False
class MapEdgeType:

    def __init__(self):
        pass

class MapOffLimitsSize:
    Minimal = None
    Default = None

    def __init__(self):
        pass

class MapOtherResourceStats:

    def __init__(self):
        pass

class MapProductStats:

    def __init__(self):
        pass

class MapResourceLocation:

    def __init__(self):
        pass

class MapTerrainResourceStats:

    def __init__(self):
        pass

class StartingLocationConfig:

    def __init__(self):
        self.StartingLocationIndex = int(0)
class StartingLocationPreview:

    def __init__(self):
        pass

class TerrainGenerationContext:

    def __init__(self):
        self.Area = None
        self.ExtraData = None
class TerrainGenerator:

    def __init__(self):
        self.TerrainWidth = int(0)
        self.TerrainHeight = int(0)
        self.Bedrock = None
        self.DoNotCreateOcean = False
        self.TerrainChunkCoords = None
class TerrainGeneratorConfig:

    def __init__(self):
        self.TerrainChunkGeneratorType = None
class TextConfigurableNoise2dFactory:

    def __init__(self):
        self.Configuration = str(0)
        self.RebuildUi = False
class WorldRegionMap:

    def __init__(self):
        self.Size = None
        self.BedrockMaterial = None
        self.MapEdgeType = None
        self.OffLimitsSize = None
        self.TerrainFeatureGenerators = None
        self.TerrainPostProcessors = None
        self.VirtualResourcesGenerators = None
        self.StartingLocations = None
class WorldRegionMapAdditionalData:

    def __init__(self):
        self.NonOceanTilesCount = int(0)
        self.FlatNonOceanTilesCount = int(0)
        self.StartingLocations = None
        self.EasyToReachTerrainResourcesStats = None
        self.TotalTerrainResourcesStats = None
        self.EasyToReachProductStats = None
        self.TotalProductStats = None
        self.EasyToReachOtherResourcesStats = None
        self.TotalOtherResourcesStats = None
        self.ResourceLocations = None
        self.TilesAtOrAboveElevationDataSorted = None
        self.PreviewImagesData = None
class WorldRegionMapBaseConfig:

    def __init__(self):
        self.MapEdgeType = None
        self.OffLimitsSize = None
class WorldRegionMapPlayerConfig:

    def __init__(self):
        self.SetStartingLocationIndex = int(0)
class WorldRegionMapPreviewData:

    def __init__(self):
        self.Name = str(0)
        from Mafi import Option
        self.NameTranslationId = Option()
        self.Description = str(0)
        from Mafi import Option
        self.DescriptionTranslationId = Option()
        self.MapVersion = int(0)
        self.CreatedInSaveVersion = int(0)
        self.CreatedInGameVersion = str(0)
        self.AuthorName = str(0)
        self.IsPublished = False
        self.CreatedDateTimeUtc = None
        self.LastEditedDateTimeUtc = None
        self.Difficulty = None
        self.MapSize = None
        self.ThumbnailImageData = None
        self.RequiredMods = None
        self.IsProtected = False
        from Mafi import Option
        self.FilePath = Option()
class ConfigurableNoise2dParamSpec:

    def __init__(self):
        pass

class ConfigurableNoise2dFactorySpec:

    def __init__(self):
        pass

class Block:

    def __init__(self):
        pass

class ConfigurableNoise2dParser:

    def __init__(self):
        self.InitialStatements = None
        self.TransformStatements = None
        self.ParameterTypeLookup = None
class InitialStatementData:

    def __init__(self):
        pass

class TransformStatementData:

    def __init__(self):
        pass

class FlatTerrainChunkGenerator:

    def __init__(self):
        pass

class ICellEdgeResourceGeneratorFactory:

    def __init__(self):
        self.Name = str(0)
        self.Priority = int(0)
        self.GenerateNearStartLocation = False
        self.AllowOnStartingCell = False
class ICellResourceGeneratorFactory:

    def __init__(self):
        self.Name = str(0)
        self.Priority = int(0)
        self.GenerateNearStartLocation = False
        self.AllowOnStartingCell = False
class ICellSurfaceGenerator:

    def __init__(self):
        self.Proto = None
class ICellVirtualResourceFactory:

    def __init__(self):
        self.Name = str(0)
        self.Priority = int(0)
        self.GenerateNearStartLocation = False
        self.AllowOnStartingCell = False
class IGlobalResourceGeneratorFactory:

    def __init__(self):
        self.Name = str(0)
        self.Priority = int(0)
        self.GenerateNearStartLocation = False
        self.AllowOnStartingCell = False
class IResourceGeneratorFactory:

    def __init__(self):
        self.Name = str(0)
        self.Priority = int(0)
        self.GenerateNearStartLocation = False
        self.AllowOnStartingCell = False
class ITerrainChunkGenerator:

    def __init__(self):
        pass

class ITerrainPostProcessor:

    def __init__(self):
        pass

class ChunkTerrainData:

    def __init__(self):
        pass

class TileTerrainData:

    def __init__(self):
        pass

class ITerrainResource:

    def __init__(self):
        self.Name = str(0)
        self.Position = None
        self.MaxRadius = None
        self.Priority = int(0)
        self.ResourceColor = None
class ITerrainResourceGenerator:

    def __init__(self):
        self.Name = str(0)
        self.Position = None
        self.MaxRadius = None
        self.Priority = int(0)
        self.ResourceColor = None
class ITerrainResourceChunkGenerator:

    def __init__(self):
        pass

class TerrainGeneratorBasePriority:

    def __init__(self):
        pass

class IVirtualTerrainResource:

    def __init__(self):
        self.Product = None
        self.ConfiguredCapacity = None
        self.Capacity = None
        self.Quantity = None
        self.EmergencyQuantity = None
        self.Name = str(0)
        self.Position = None
        self.MaxRadius = None
        self.Priority = int(0)
        self.ResourceColor = None
class IVirtualTerrainResourceFriend:

    def __init__(self):
        pass

class IVirtualTerrainResourceExtensions:

    def __init__(self):
        pass

class IWorldRegionMap:

    def __init__(self):
        self.Size = None
        self.BedrockMaterial = None
        self.MapEdgeType = None
        self.OffLimitsSize = None
        self.TerrainFeatureGenerators = None
        self.TerrainPostProcessors = None
        self.VirtualResourcesGenerators = None
        self.StartingLocations = None
class IVirtualTerrainResourceGenerator:

    def __init__(self):
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
class IWorldRegionMapPreviewData:

    def __init__(self):
        from Mafi import Option
        self.NameTranslationId = Option()
        self.Description = str(0)
        from Mafi import Option
        self.DescriptionTranslationId = Option()
        self.CreatedInGameVersion = str(0)
        self.AuthorName = str(0)
        self.IsPublished = False
        self.CreatedDateTimeUtc = None
        self.LastEditedDateTimeUtc = None
        self.Difficulty = None
        self.MapSize = None
        self.ThumbnailImageData = None
        self.RequiredMods = None
        self.IsProtected = False
        from Mafi import Option
        self.FilePath = Option()
        self.Name = str(0)
        self.MapVersion = int(0)
class IWorldRegionMapAdditionalData:

    def __init__(self):
        self.NonOceanTilesCount = int(0)
        self.FlatNonOceanTilesCount = int(0)
        self.StartingLocations = None
        self.EasyToReachTerrainResourcesStats = None
        self.TotalTerrainResourcesStats = None
        self.EasyToReachProductStats = None
        self.TotalProductStats = None
        self.EasyToReachOtherResourcesStats = None
        self.TotalOtherResourcesStats = None
        self.ResourceLocations = None
        self.TilesAtOrAboveElevationDataSorted = None
        self.PreviewImagesData = None
class StartingLocationDifficulty:

    def __init__(self):
        pass

class IMapCacheManager:

    def __init__(self):
        from Mafi import Option
        self.LoadedMapCacheData = Option()
        self.LoadResult = None
        self.SaveResult = None
class MapCacheSaveResult:

    def __init__(self):
        pass

class MapCacheLoadResult:

    def __init__(self):
        pass

class NoMapCacheManager:

    def __init__(self):
        from Mafi import Option
        self.LoadedMapCacheData = Option()
        self.SaveResult = None
        self.LoadResult = None
class MapCacheManager:

    def __init__(self):
        from Mafi import Option
        self.LoadedMapCacheData = Option()
        self.SaveResult = None
        self.LoadResult = None
class MapCellTerrainChunkGenerator:

    def __init__(self):
        pass

class TerrainGenerationBuffer:

    def __init__(self):
        self.IsEmpty = False
        self.BaseSurfaceHeight = None
        self.SurfaceHeight = None
        self.LowestMaterialBottomHeight = None
        from Mafi import Option
        self.TopMaterial = Option()
class ITerrainGenerationExtraData:

    def __init__(self):
        pass

class ITerrainExtraDataRegistrator:

    def __init__(self):
        pass

class ITerrainGenerator:

    def __init__(self):
        self.TerrainWidth = int(0)
        self.TerrainHeight = int(0)
        self.Bedrock = None
        self.DoNotCreateOcean = False
class GeneratedTerrainData:

    def __init__(self):
        pass

class TerrainGeneratorChunkData:

    def __init__(self):
        self.Chunk = None
        self.Area = None
class ITerrainGeneratorV2:

    def __init__(self):
        pass

class ITerrainFeatureBase:

    def __init__(self):
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
class ITerrainFeatureGenerator:

    def __init__(self):
        self.SortingPriority = int(0)
        self.LastGenerationTime = None
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
class TerrainFeaturePriorityBase:

    def __init__(self):
        pass

class TerrainPostProcessorPriorityBase:

    def __init__(self):
        pass

class TerrainFeatureResourceInfo:

    def __init__(self):
        pass

class ITerrainFeatureWithOceanCoast:

    def __init__(self):
        self.SortingPriority = int(0)
        self.LastGenerationTime = None
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
class IEditableTerrainFeatureWithDisplayedRadius:

    def __init__(self):
        self.Radius = None
        self.Is2D = False
        self.CanRotate = False
        self.Config = None
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
class IEditableTerrainFeature:

    def __init__(self):
        self.Is2D = False
        self.CanRotate = False
        self.Config = None
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
class HandleData:

    def __init__(self):
        pass

class ITerrainFeatureWithPreview:

    def __init__(self):
        self.Is2D = False
        self.CanRotate = False
        self.Config = None
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
class IPostProcessorWithPreview:

    def __init__(self):
        self.Is2D = False
        self.CanRotate = False
        self.Config = None
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
class ITerrainFeatureWithSimUpdate:

    def __init__(self):
        self.Is2D = False
        self.CanRotate = False
        self.Config = None
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
class ITerrainFeatureWithSyncUpdate:

    def __init__(self):
        self.Is2D = False
        self.CanRotate = False
        self.Config = None
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
class ITerrainFeatureConfig:

    def __init__(self):
        pass

class ITerrainFeatureConfigWithInit:

    def __init__(self):
        pass

class ITerrainFeaturePreview:

    def __init__(self):
        self.Chunk = None
class IEditableTerrainFeaturePreview:

    def __init__(self):
        pass

class ITerrainPostProcessorV2:

    def __init__(self):
        self.ParallelizationStrategy = None
        self.SortingPriority = int(0)
        self.PassCount = int(0)
        self.LastGenerationTime = None
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
class IStartingLocationV2:

    def __init__(self):
        self.Order = int(0)
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
class TerrainGeneratorV2:

    def __init__(self):
        pass

class TerrainGeneratorV2Config:

    def __init__(self):
        self.MaxDegreeOfParallelism = None
class MaxDegreeOfParallelism:

    def __init__(self):
        pass

class MaxDegreeOfParallelismExtensions:

    def __init__(self):
        pass

class TerrainPostProcessorParallelizationStrategy:

    def __init__(self):
        pass

class VirtualResourceData:

    def __init__(self):
        pass

class IWorldRegionMapFactory:

    def __init__(self):
        pass

class WorldRegionMapFactoryConfig:

    def __init__(self):
        self.FactoryType = None
        from Mafi import Option
        self.Config = Option()
class StaticWorldRegionMapFactory:

    def __init__(self):
        pass

class Config:

    def __init__(self):
        self.Map = None
        self.PreviewData = None
class FileWorldRegionMapFactory:

    def __init__(self):
        pass

class Config:

    def __init__(self):
        pass

