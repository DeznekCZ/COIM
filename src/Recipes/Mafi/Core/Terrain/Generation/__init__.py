
class Chunk64Area:
    def __init__(self):
        self.TotalChunksCount = 0
        self.Area2i = None
        self.Origin = None
        self.Size = None

class CoastLinesData:
    def __init__(self):
        self.CoastLines = None

class CustomTerrainPostProcessorV2:
    def __init__(self):
        self.Name = ""
        self.Id = 0
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.ParallelizationStrategy = None
        self.SortingPriority = 0
        self.PassCount = 0
        self.LastGenerationTime = None

class EncodedImageAndMatrix:
    def __init__(self):
        self.ImageData = None
        self.CameraPose = None
        self.ViewProjectionMatrix = None
        self.FogDensity = 0.0

class FlatTerrainGenerator:
    def __init__(self):
        self.Height = None
        self.TerrainWidth = 0
        self.TerrainHeight = 0
        self.Bedrock = None
        self.DoNotCreateOcean = False

class MapEdgeType:
    def __init__(self):
        self.GroundTowardsMinusX = False
        self.GroundTowardsMinusY = False
        self.GroundTowardsPlusX = False
        self.GroundTowardsPlusY = False

class MapOffLimitsSize:
    Minimal = None
    Default = None
    def __init__(self):
        self.MinusX = None
        self.MinusY = None
        self.PlusX = None
        self.PlusY = None

class MapOtherResourceStats:
    def __init__(self):
        from Mafi.Core.Products import ProductProto
        self.ProductProtoId = ProductProto.ID()

        self.Quantity = None

class MapProductStats:
    def __init__(self):
        from Mafi.Core.Products import ProductProto
        self.ProductProtoId = ProductProto.ID()

        self.DisplayName = ""
        self.Quantity = None

class MapResourceLocation:
    def __init__(self):
        from Mafi.Core.Products import ProductProto
        self.ProductProtoId = ProductProto.ID()

        self.Position = None

class MapTerrainResourceStats:
    def __init__(self):
        from Mafi.Core.Prototypes import Proto
        self.MaterialProtoId = Proto.ID()

        self.VolumeTilesCubed = 0

class StartingLocationConfig:
    def __init__(self):
        self.StartingLocationIndex = 0
        self.SetStartingLocationIndex = 0

class StartingLocationPreview:
    def __init__(self):
        self.Position = None
        self.ShoreDirection = None
        from Mafi import Option
        self.Description = Option()
        self.Difficulty = None
        self.StartingLocationArea = None
        self.ExtraStartingResources = None

class TerrainGenerationContext:
    def __init__(self):
        self.Area = None
        self.ExtraData = None
        self.TerrainSize = None
        self.InitialMapCreationSaveVersion = 0
        self.DataOrigin = None
        self.ChunkArea = None
        self.Data = None
        self.BedrockMaterial = None

class TerrainGenerator:
    REPORT_PROGRESS_STEPS = 0
    def __init__(self):
        self.TerrainWidth = 0
        self.TerrainHeight = 0
        self.Bedrock = None
        self.DoNotCreateOcean = False
        self.TerrainChunkCoords = None

class TerrainGeneratorConfig:
    def __init__(self):
        self.TerrainChunkGeneratorType = None

class TextConfigurableNoise2dFactory:
    def __init__(self):
        self.Configuration = ""
        self.RebuildUi = False
        self.Parameters = None

class WorldRegionMap:
    MAX_MAP_FILE_NAME_LENGTH = 0
    def __init__(self):
        self.Size = None
        self.BedrockMaterial = None
        self.MapEdgeType = None
        self.OffLimitsSize = None
        self.TerrainFeatureGenerators = None
        self.TerrainPostProcessors = None
        self.VirtualResourcesGenerators = None
        self.StartingLocations = None
        self.BaseConfig = None
        self.TerrainFeatureGeneratorsList = None
        self.TerrainPostProcessorsList = None
        self.VirtualTerrainResourceGeneratorList = None
        self.StartingLocationsList = None

class WorldRegionMapAdditionalData:
    MAX_DEPTH_FOR_EASY_RESOURCES = None
    def __init__(self):
        self.NonOceanTilesCount = 0
        self.FlatNonOceanTilesCount = 0
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
        self.SetStartingLocationIndex = 0

class WorldRegionMapPreviewData:
    MAX_NAME_LENGTH = 0
    MAX_DESCRIPTION_LENGTH = 0
    MAX_AUTHOR_LENGTH = 0
    def __init__(self):
        self.Name = ""
        from Mafi import Option
        self.NameTranslationId = Option()
        self.Description = ""
        self.DescriptionTranslationId = Option()
        self.MapVersion = 0
        self.CreatedInSaveVersion = 0
        self.CreatedInGameVersion = ""
        self.AuthorName = ""
        self.IsPublished = False
        self.CreatedDateTimeUtc = None
        self.LastEditedDateTimeUtc = None
        self.Difficulty = None
        self.MapSize = None
        self.ThumbnailImageData = None
        self.RequiredMods = None
        self.IsProtected = False
        self.FilePath = Option()

class ConfigurableNoise2dParamSpec:
    def __init__(self):
        self.Name = ""
        self.Type = None
        from Mafi import Option
        self.Description = Option()

class ConfigurableNoise2dFactorySpec:
    def __init__(self):
        self.Parameters = None
        self.StatementsBlocks = None

    class Block:
        def __init__(self):
            self.Name = ""
            self.Statements = None

class ConfigurableNoise2dParser:
    def __init__(self):
        self.InitialStatements = None
        self.TransformStatements = None
        self.ParameterTypeLookup = None

    class InitialStatementData:
        def __init__(self):
            self.Name = ""
            from Mafi import Option
            self.Description = Option()
            self.Parameters = None
            self.FactoryFn = None

    class TransformStatementData:
        def __init__(self):
            self.Name = ""
            from Mafi import Option
            self.Description = Option()
            self.Parameters = None
            self.FactoryFn = None

class FlatTerrainChunkGenerator:
    def __init__(self):
        pass


class ICellEdgeResourceGeneratorFactory:
    def __init__(self):
        self.Name = ""
        self.Priority = 0
        self.GenerateNearStartLocation = False
        self.AllowOnStartingCell = False

class ICellResourceGeneratorFactory:
    def __init__(self):
        self.Name = ""
        self.Priority = 0
        self.GenerateNearStartLocation = False
        self.AllowOnStartingCell = False

class ICellSurfaceGenerator:
    def __init__(self):
        self.Proto = None

class ICellVirtualResourceFactory:
    def __init__(self):
        self.Name = ""
        self.Priority = 0
        self.GenerateNearStartLocation = False
        self.AllowOnStartingCell = False

class IGlobalResourceGeneratorFactory:
    def __init__(self):
        self.Name = ""
        self.Priority = 0
        self.GenerateNearStartLocation = False
        self.AllowOnStartingCell = False

class IResourceGeneratorFactory:
    def __init__(self):
        self.Name = ""
        self.Priority = 0
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
        self.ChunkCoord = None
        self.Data = None

class TileTerrainData:
    def __init__(self):
        self.SurfaceHeight = None
        self.Products = None
        self.TreeData = None
        self.TerrainPropData = None

class ITerrainResource:
    def __init__(self):
        self.Name = ""
        self.Position = None
        self.MaxRadius = None
        self.Priority = 0
        self.ResourceColor = None

class ITerrainResourceGenerator:
    def __init__(self):
        self.Name = ""
        self.Position = None
        self.MaxRadius = None
        self.Priority = 0
        self.ResourceColor = None

class ITerrainResourceChunkGenerator:
    def __init__(self):
        pass


class TerrainGeneratorBasePriority:
    CellEdges = None
    Resources = None
    Trees = None
    def __init__(self):
        self.value__ = 0

class IVirtualTerrainResource:
    def __init__(self):
        self.Product = None
        self.ConfiguredCapacity = None
        self.Capacity = None
        self.Quantity = None
        self.EmergencyQuantity = None
        self.Name = ""
        self.Position = None
        self.MaxRadius = None
        self.Priority = 0
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
        self.Name = ""
        self.Id = 0
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False

class IWorldRegionMapPreviewData:
    def __init__(self):
        from Mafi import Option
        self.NameTranslationId = Option()
        self.Description = ""
        self.DescriptionTranslationId = Option()
        self.CreatedInGameVersion = ""
        self.AuthorName = ""
        self.IsPublished = False
        self.CreatedDateTimeUtc = None
        self.LastEditedDateTimeUtc = None
        self.Difficulty = None
        self.MapSize = None
        self.ThumbnailImageData = None
        self.RequiredMods = None
        self.IsProtected = False
        self.FilePath = Option()
        self.Name = ""
        self.MapVersion = 0

class IWorldRegionMapAdditionalData:
    def __init__(self):
        self.NonOceanTilesCount = 0
        self.FlatNonOceanTilesCount = 0
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
    Easy = None
    Medium = None
    Hard = None
    Insane = None
    def __init__(self):
        self.value__ = 0

class IMapCacheManager:
    def __init__(self):
        from Mafi import Option
        self.LoadedMapCacheData = Option()
        self.LoadResult = None
        self.SaveResult = None

class MapCacheSaveResult:
    Unknown = None
    Success = None
    InvalidChecksum = None
    FailedToWrite = None
    def __init__(self):
        self.value__ = 0

class MapCacheLoadResult:
    Unknown = None
    Success = None
    NoFile = None
    InvalidChecksum = None
    InvalidVersion = None
    FailedToRead = None
    def __init__(self):
        self.value__ = 0

class NoMapCacheManager:
    def __init__(self):
        from Mafi import Option
        self.LoadedMapCacheData = Option()
        self.SaveResult = None
        self.LoadResult = None

class MapCacheManager:
    HEADER_MAP_CACHE_ASCII = None
    HEADER_MAP_CACHE = None
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
        self.TreeData = None
        self.TerrainPropData = None

class ITerrainGenerationExtraData:
    def __init__(self):
        pass


class ITerrainExtraDataRegistrator:
    def __init__(self):
        pass


class ITerrainGenerator:
    def __init__(self):
        self.TerrainWidth = 0
        self.TerrainHeight = 0
        self.Bedrock = None
        self.DoNotCreateOcean = False

class GeneratedTerrainData:
    def __init__(self):
        self.Chunks = None

class TerrainGeneratorChunkData:
    SIZE = 0
    def __init__(self):
        self.Chunk = None
        self.Area = None
        self.Heights = None
        self.MaterialLayers = None
        self.Surfaces = None

class ITerrainGeneratorV2:
    def __init__(self):
        pass


class ITerrainFeatureBase:
    def __init__(self):
        self.Name = ""
        self.Id = 0
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False

class ITerrainFeatureGenerator:
    def __init__(self):
        self.SortingPriority = 0
        self.LastGenerationTime = None
        self.Name = ""
        self.Id = 0
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False

class TerrainFeaturePriorityBase:
    First = None
    TerrainResources = None
    TerrainSurfaces = None
    PostProcessors = None
    Last = None
    def __init__(self):
        self.value__ = 0

class TerrainPostProcessorPriorityBase:
    First = None
    Erosion = None
    RestrictPlacement = None
    GrassOnRocks = None
    Trees = None
    MixedSurfaces = None
    Flowers = None
    Props = None
    Last = None
    def __init__(self):
        self.value__ = 0

class TerrainFeatureResourceInfo:
    def __init__(self):
        self.Position = None
        self.ProductProto = None

class ITerrainFeatureWithOceanCoast:
    def __init__(self):
        self.SortingPriority = 0
        self.LastGenerationTime = None
        self.Name = ""
        self.Id = 0
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False

class IEditableTerrainFeatureWithDisplayedRadius:
    def __init__(self):
        self.Radius = None
        self.Is2D = False
        self.CanRotate = False
        self.Config = None
        self.Name = ""
        self.Id = 0
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False

class IEditableTerrainFeature:
    def __init__(self):
        self.Is2D = False
        self.CanRotate = False
        self.Config = None
        self.Name = ""
        self.Id = 0
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False

class HandleData:
    def __init__(self):
        self.Position = None
        self.Color = None
        self.IconColor = None
        from Mafi import Option
        self.IconAssetPath = Option()
        self.Height = None

class ITerrainFeatureWithPreview:
    def __init__(self):
        self.Is2D = False
        self.CanRotate = False
        self.Config = None
        self.Name = ""
        self.Id = 0
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False

class IPostProcessorWithPreview:
    def __init__(self):
        self.Is2D = False
        self.CanRotate = False
        self.Config = None
        self.Name = ""
        self.Id = 0
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False

class ITerrainFeatureWithSimUpdate:
    def __init__(self):
        self.Is2D = False
        self.CanRotate = False
        self.Config = None
        self.Name = ""
        self.Id = 0
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False

class ITerrainFeatureWithSyncUpdate:
    def __init__(self):
        self.Is2D = False
        self.CanRotate = False
        self.Config = None
        self.Name = ""
        self.Id = 0
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
        self.SortingPriority = 0
        self.PassCount = 0
        self.LastGenerationTime = None
        self.Name = ""
        self.Id = 0
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False

class IStartingLocationV2:
    def __init__(self):
        self.Order = 0
        self.Name = ""
        self.Id = 0
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False

class TerrainGeneratorV2:
    MAP_HEIGHT_CAP = 0
    def __init__(self):
        pass


class TerrainGeneratorV2Config:
    def __init__(self):
        self.MaxDegreeOfParallelism = None

class MaxDegreeOfParallelism:
    ProcessorsCount = None
    OneThread = None
    TwoThreads = None
    FourThreads = None
    EightThreads = None
    SixteenThreads = None
    def __init__(self):
        self.value__ = 0

class MaxDegreeOfParallelismExtensions:
    def __init__(self):
        pass


class TerrainPostProcessorParallelizationStrategy:
    AnalyzeAllThenApply = None
    AnalyzeInterleaveAndApply = None
    CustomSchedule = None
    def __init__(self):
        self.value__ = 0

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
            self.MapFilePath = ""
