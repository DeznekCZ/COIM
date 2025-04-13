
class LandfillOnTerrainManager:
    def __init__(self):
        self.Stats = None
        self.LandfillProduct = None

class OceanTerrainManager:
    OCEAN_THRESHOLD = None
    MIN_CONNECTED_TILES_FOR_FLOODING = 0
    MAX_TILES_PER_TICK = 0
    def __init__(self):
        self.QueuedTilesCount = 0
        self.ProcessedLastTick = 0

class RectangleTerrainArea2i:
    def __init__(self):
        self.AreaTiles = 0
        self.IsEmpty = False
        self.IsNotEmpty = False
        self.CenterCoord = None
        self.CenterCoordF = None
        self.PlusXTileIncl = None
        self.PlusYTileIncl = None
        self.PlusXyTileIncl = None
        self.PlusXCoordExcl = None
        self.PlusYCoordExcl = None
        self.PlusXyCoordExcl = None
        self.Chunk64Area = None
        self.Origin = None
        self.Size = None

class RectangleTerrainArea2iRelative:
    def __init__(self):
        self.AreaTiles = 0
        self.Origin = None
        self.Size = None

class TerrainManager:
    MAX_DISRUPTION_DEPTH = None
    BedrockLayerThicknessDefault = None
    TERRAIN_GEN_PROGRESS_STEPS = 0
    TERRAIN_GEN_APPLY_STEPS = 0
    TERRAIN_GEN_LOAD_STEPS = 0
    TERRAIN_DIMENSION_MULTIPLE = 0
    TERRAIN_DIMENSION_MULTIPLE_BITS = 0
    TERRAIN_DIMENSION_MIN = 0
    TERRAIN_DIMENSION_MULTIPLE_MASK = 0
    MIN_OFF_LIMITS_SIZE = 0
    DEFAULT_OFF_LIMITS_SIZE = 0
    MAX_OFF_LIMITS_SIZE = 0
    TERRAIN_DIMENSION_MAX = 0
    TERRAIN_DIMENSION_MAX_BITS = 0
    TERRAIN_AREA_MAX = 0
    TERRAIN_AREA_MAX_BITS = 0
    def __init__(self):
        self.TerrainWidth = 0
        self.TerrainHeight = 0
        self.TerrainTilesCount = 0
        self.MaxTileCoord = None
        self.TerrainArea = None
        self.TerrainAreaChunks = None
        self.MinOnLimits = None
        self.MaxOnLimitsExcl = None
        self.MaxOnLimitsIncl = None
        self.TerrainSize = None
        self.Chunk8PerWidth = 0
        self.Chunk8PerHeight = 0
        self.Chunk8TotalCount = 0
        self.Chunk64PerWidth = 0
        self.Chunk64PerHeight = 0
        self.Chunk64TotalCount = 0
        self.FourSideNeighborsDeltas = None
        self.FourSideNeighborsDeltasIndices = None
        self.FourCornerNeighborsDeltas = None
        self.EightNeighborsDeltas = None
        self.FourTileCornersDeltas = None
        self.Bedrock = None
        self.TerrainDataGenerated = None
        self.TerrainGeneratedButNotLoaded = None
        self.TerrainGenerated = None
        self.HeightChanged = None
        self.TileMaterialsOnlyChanged = None
        self.TileFlagsChanged = None
        self.OceanFlagChanged = None
        self.TileCustomSurfaceChanged = None
        self.IsProcessingPhysics = False
        self.IsProcessingDisruption = False
        self.BlocksBuildingsFlagsMask = None
        self.BlocksVehiclesFlagsMask = None
        self.TerrainMaterials = None
        self.MinedProducts = None
        self.DisruptedMaterialIds = None
        self.RecoveredMaterialIds = None
        self.TerrainSurfaces = None
        self.IsGeneratingTerrain = False
        self.IsGeneratingTerrainLegacy = False
        self.HeightsData = None
        self.TileLayersData = None
        self.TileSurfacesData = None
        self.MaterialLayersOverflowData = None
        self.TileFlags = None
        self.Item = None
        self.OffLimitsSize = None
        self.BlocksBuildingsOrVehiclesMask = None
        self.OffLimitsDisabled = False
        self.ExtendTerrainByCloningCount = 0
        self.MapCacheDisabled = False
        self.MapCacheForceEnableWrite = False
        self.WasLoadedFromCache = False
        self.InitialMapCreationSaveVersion = 0
        self.TerrainMaterialSlimIdManager = None
        self.TileSurfaceDecalsSlimIdManager = None
        self.TileSurfacesSlimIdManager = None

    class TerrainData:
        def __init__(self):
            self.Size = None
            from Mafi import Option
            self.HeightSnapshot = Option()
            self.Width = 0
            self.Height = 0
            self.Heights = None
            self.MaterialLayers = None
            self.Surfaces = None
            self.Flags = None
            self.MaterialLayersOverflow = None
            self.MaterialLayersOverflowFreeIndices = None
            self.ChangedTiles = None
            self.SavedFlagsMask = None

class TerrainMaterialThickness:
    def __init__(self):
        self.IsEmpty = False
        self.IsNotEmpty = False
        self.AsSlim = None
        self.Material = None
        self.Thickness = None

class TerrainMaterialThicknessSlim:
    def __init__(self):
        self.SlimId = None
        self.SlimIdRaw = 0
        self.Thickness = None
        self.IsNone = False
        self.HasValue = False
        self.IsEmpty = False
        self.IsPositive = False
        self.RawData = None

class TerrainOccupancyManager:
    def __init__(self):
        self.Priority = None
        self.TileOccupancyChanged = None
        self.TerrainManager = None

class TerrainSurfaceManager:
    def __init__(self):
        pass


class TerrainTile:
    MIN_LAYER_THICKNESS = None
    MIN_LAYER_THICKNESS_PER_DEPTH = None
    TILE_SIZE_M = 0
    TILE_SIZE_M_HALF = 0
    TILE_AREA_M = 0
    TILE_VOLUME_M = 0
    RESERVED_FLAGS_COUNT = 0
    MAX_FLAGS_COUNT = 0
    FLAG_IS_ON_BOUNDARY = None
    FLAG_IS_OFF_LIMITS = None
    FLAG_IS_OCEAN = None
    def __init__(self):
        self.TileCoord = None
        self.IsOnTerrain = False
        self.IsOffLimitsOrInvalid = False
        self.ChunkCoord2i = None
        self.CoordAndIndex = None
        self.CornerTile3f = None
        self.CornerTile2f = None
        self.CenterTile2f = None
        self.CenterTile3f = None
        self.WithHeightFloored = None
        self.Height = None
        self.IsBlockingVehicles = False
        self.IsBlockingBuildings = False
        self.IsBlockingBuildingsOrVehicles = False
        self.IsOcean = False
        self.IsNotOcean = False
        self.Item = None
        self.PlusXNeighbor = None
        self.MinusXNeighbor = None
        self.PlusYNeighbor = None
        self.MinusYNeighbor = None
        self.PlusXyNeighbor = None
        self.LayersCount = 0
        self.LayersCountNoBedrock = 0
        self.IsAtBedrock = False
        self.FirstLayerSlimOrNoneNoBedrock = None
        self.FirstLayerSlim = None
        self.FirstLayer = None
        self.SecondLayerSlimOrNoneNoBedrock = None
        self.SecondLayerSlim = None
        self.SecondLayer = None
        self.ThirdLayerSlimOrNoneNoBedrock = None
        self.ThirdLayerSlim = None
        self.ThirdLayer = None
        self.TerrainManager = None
        self.TileCoordSlim = None
        self.DataIndex = None

class TileFlagReporter:
    def __init__(self):
        self.Name = ""
        self.TerrainManager = None
        self.FlagMask = None
        self.BlocksBuildings = False
        self.BlocksVehicles = False
        self.IsSaved = False

class TileSurfaceData:
    SIZE_BYTES = 0
    IS_AUTO_PLACED_SHIFT = 0
    HEIGHT_BITS_INCL_SIGN = 0
    def __init__(self):
        self.Height = None
        self.SurfaceSlimId = None
        self.TextureRotation = None
        self.RampRotation = None
        self.IsRamp = False
        self.IsAutoPlaced = False
        self.DecalSlimId = None
        self.DecalRotation = None
        self.IsDecalFlipped = False
        self.ColorKey = 0
        self.IsValid = False
        self.IsNotValid = False
        self.RawValue = 0
        self.RawValueDecal = 0

class TileSurfaceSlimId:
    PhantomId = None
    def __init__(self):
        self.IsPhantom = False
        self.IsNotPhantom = False
        self.Value = None

class TileSurfacesSlimIdManager:
    def __init__(self):
        self.PhantomProto = None
        self.MaxIdValue = 0
        self.ManagedProtos = None

class VirtualResourceManager:
    def __init__(self):
        pass


class FarmableManager:
    def __init__(self):
        pass


class IVirtualResourceManager:
    def __init__(self):
        pass


class LayoutEntityTerrainValidator:
    def __init__(self):
        self.Priority = None

class OccupiedTileRange:
    def __init__(self):
        self.From = None
        self.VerticalSize = None
        self.ToExcl = None
        self.Position = None
        self.FromRaw = None
        self.VerticalSizeRaw = None

class RectangleTerrainArea2iRelativeExtensions:
    def __init__(self):
        pass


class TerrainTileSurfaceProto:
    from Mafi.Core.Prototypes import Proto
    PHANTOM_PRODUCT_ID = Proto.ID('__PHANTOM__TILE_SURFACE__')
    Phantom = None
    def __init__(self):
        self.IconPath = ""
        self.SlimId = None
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.MaintenanceScale = None
        self.EdgeCompatibleWith = None
        self.CanBePlacedByPlayer = False
        self.CostPerTile = None
        self.Graphics = None
        self.IsPhantom = False
        self.IsInitialized = False

    class Gfx:
        Empty = None
        def __init__(self):
            self.TextureSpec = None
            self.EdgesSpec = None
            self.DustinessPerc = 0.0
            self.DustColor = None
            self.IconPath = ""

class TileSurfaceTextureSpec:
    Empty = None
    def __init__(self):
        self.AlbedoTexturePaths = None
        self.NormalTexturePaths = None
        self.SmoothMetalTexturePaths = None

class TileSurfacesEdgesSpec:
    Empty = None
    def __init__(self):
        self.AlbedoTextureEdgeFullPath = ""
        self.NormalsTextureEdgeFullPath = ""
        self.SmoothMetalTextureEdgeFullPath = ""
        self.AlbedoTextureEdgeHorizontalPath = ""
        self.NormalsTextureEdgeHorizontalPath = ""
        self.SmoothMetalTextureEdgeHorizontalPath = ""
        self.AlbedoTextureEdgeVerticalPath = ""
        self.NormalsTextureEdgeVerticalPath = ""
        self.SmoothMetalTextureEdgeVerticalPath = ""
        self.AlbedoTextureEdgeCornersPath = ""
        self.NormalsTextureEdgeCornersPath = ""
        self.SmoothMetalTextureEdgeCornersPath = ""

class TerrainChunk:
    Size = None
    Size2i = None
    BITS_TILES_PER_EDGE = 0
    MASK_LOCAL_COORD = 0
    TILES_PER_EDGE = 0
    MAX_LOCAL_COORD = 0
    TILES_PER_CHUNK = 0
    def __init__(self):
        pass


class TileMaterialLayers:
    def __init__(self):
        self.Count = 0
        self.First = None
        self.Second = None
        self.Third = None
        self.Fourth = None
        self.OverflowIndex = 0

class TileMaterialLayerOverflow:
    def __init__(self):
        self.Material = None
        self.OverflowIndex = 0

class TerrainLayerEnumerator:
    def __init__(self):
        self.Current = None

class MaterialConversionResult:
    Empty = None
    def __init__(self):
        self.IsEmpty = False
        self.IsNotEmpty = False
        self.TileAndIndex = None
        self.SourceMaterialThickness = None

class TerrainManagerConfig:
    def __init__(self):
        self.AllowDenormalizedTerrainSize = False
        self.DisableMarkingOfOffLimitsAreas = False
        self.ExtendTerrainByCloningCount = 0
        self.MapCacheDisabled = False
        self.MapCacheForceEnableWrite = False
        self.EnableHeightSnapshotting = False

class TerrainMaterialThicknessSlimExtensions:
    def __init__(self):
        pass


class OccupiedTilesExtensions:
    def __init__(self):
        pass


class UnstableTerrainValidator:
    def __init__(self):
        self.Priority = None

class UnstableTerrainMaterialParam:
    Instance = None
    def __init__(self):
        self.AllowedProtoType = None
