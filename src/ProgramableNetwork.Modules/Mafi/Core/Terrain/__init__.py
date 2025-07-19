class CheatRestoreTerrainCmd:

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
class LandfillOnTerrainManager:

    def __init__(self):
        self.Stats = None
        self.LandfillProduct = None
class OceanTerrainManager:
    OCEAN_THRESHOLD = None
    MIN_CONNECTED_TILES_FOR_FLOODING = None
    MAX_TILES_PER_TICK = None

    def __init__(self):
        self.QueuedTilesCount = int(0)
        self.ProcessedLastTick = int(0)
class PolygonTerrainArea2i:

    def __init__(self):
        self.BoundingBoxCenter = None
        self.IsEmpty = False
        self.IsNotEmpty = False
class RectangleTerrainArea2i:

    def __init__(self):
        self.AreaTiles = int(0)
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
class RectangleTerrainArea2iRelative:

    def __init__(self):
        self.AreaTiles = int(0)
class TerrainManager:
    HEIGHT_AT_OCEAN = None
    MAX_DISRUPTION_DEPTH = None
    BedrockLayerThicknessDefault = None
    TERRAIN_GEN_PROGRESS_STEPS = None
    TERRAIN_GEN_APPLY_STEPS = None
    TERRAIN_GEN_LOAD_STEPS = None
    TERRAIN_DIMENSION_MULTIPLE = None
    TERRAIN_DIMENSION_MULTIPLE_BITS = None
    TERRAIN_DIMENSION_MIN = None
    TERRAIN_DIMENSION_MULTIPLE_MASK = None
    MIN_OFF_LIMITS_SIZE = None
    DEFAULT_OFF_LIMITS_SIZE = None
    MAX_OFF_LIMITS_SIZE = None
    TERRAIN_DIMENSION_MAX = None
    TERRAIN_DIMENSION_MAX_BITS = None
    TERRAIN_AREA_MAX = None
    TERRAIN_AREA_MAX_BITS = None

    def __init__(self):
        self.TerrainWidth = int(0)
        self.TerrainHeight = int(0)
        self.TerrainTilesCount = int(0)
        self.MaxTileCoord = None
        self.TerrainArea = None
        self.TerrainAreaChunks = None
        self.MinOnLimits = None
        self.MaxOnLimitsExcl = None
        self.MaxOnLimitsIncl = None
        self.TerrainSize = None
        self.Chunk8PerWidth = int(0)
        self.Chunk8PerHeight = int(0)
        self.Chunk8TotalCount = int(0)
        self.Chunk64PerWidth = int(0)
        self.Chunk64PerHeight = int(0)
        self.Chunk64TotalCount = int(0)
        self.Chunk256PerWidth = int(0)
        self.Chunk256PerHeight = int(0)
        self.Chunk256TotalCount = int(0)
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
        self.DisruptedMaterialParentsIds = None
        self.TerrainSurfaces = None
        self.IsGeneratingTerrain = False
        self.IsGeneratingTerrainLegacy = False
        self.HeightsData = None
        self.TileLayersData = None
        self.TileSurfacesData = None
        self.MaterialLayersOverflowData = None
        self.TileFlags = None
        self.Item = None
class TerrainData:

    def __init__(self):
        self.Size = None
        from Mafi import Option
        self.HeightSnapshot = Option()
class TerrainMaterialThickness:

    def __init__(self):
        self.IsEmpty = False
        self.IsNotEmpty = False
        self.AsSlim = None
class TerrainMaterialThicknessSlim:

    def __init__(self):
        self.SlimId = None
        self.SlimIdRaw = int(0)
        self.Thickness = None
        self.IsNone = False
        self.HasValue = False
        self.IsEmpty = False
        self.IsPositive = False
class TerrainOccupancyManager:

    def __init__(self):
        self.Priority = None
        self.TileOccupancyChanged = None
class TerrainSurfaceManager:

    def __init__(self):
        pass

class TerrainTile:
    ONE_OVER_TILE_SIZE_M = None
    MIN_LAYER_THICKNESS = None
    MIN_LAYER_THICKNESS_PER_DEPTH = None
    TILE_SIZE_M = None
    TILE_SIZE_M_HALF = None
    TILE_AREA_M = None
    TILE_VOLUME_M = None
    RESERVED_FLAGS_COUNT = None
    MAX_FLAGS_COUNT = None
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
        self.LayersCount = int(0)
        self.LayersCountNoBedrock = int(0)
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
class TileFlagReporter:

    def __init__(self):
        pass

class TileSurfaceData:
    SIZE_BYTES = None
    IS_AUTO_PLACED_SHIFT = None
    HEIGHT_BITS_INCL_SIGN = None

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
        self.ColorKey = int(0)
        self.IsValid = False
        self.IsNotValid = False
class TileSurfaceSlimId:
    PhantomId = None

    def __init__(self):
        self.IsPhantom = False
        self.IsNotPhantom = False
class TileSurfacesSlimIdManager:

    def __init__(self):
        self.PhantomProto = None
        self.MaxIdValue = int(0)
class VirtualResourceManager:

    def __init__(self):
        pass

class FakeTerrainOccupancyManager:

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
class RectangleTerrainArea2iRelativeExtensions:

    def __init__(self):
        pass

class TerrainTileSurfaceProto:
    PHANTOM_PRODUCT_ID = None
    Phantom = None

    def __init__(self):
        self.IconPath = str(0)
        self.SlimId = None
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
class Gfx:
    Empty = None

    def __init__(self):
        self.TextureSpec = None
        self.EdgesSpec = None
class TileSurfaceTextureSpec:
    Empty = None

    def __init__(self):
        pass

class TileSurfacesEdgesSpec:
    Empty = None

    def __init__(self):
        pass

class TerrainChunk:
    Size = None
    Size2i = None
    BITS_TILES_PER_EDGE = None
    MASK_LOCAL_COORD = None
    TILES_PER_EDGE = None
    MAX_LOCAL_COORD = None
    TILES_PER_CHUNK = None

    def __init__(self):
        pass

class TileMaterialLayers:

    def __init__(self):
        pass

class TileMaterialLayerOverflow:

    def __init__(self):
        pass

class TerrainLayerEnumerator:

    def __init__(self):
        self.Current = None
class MaterialConversionResult:
    Empty = None

    def __init__(self):
        self.IsEmpty = False
        self.IsNotEmpty = False
class TerrainManagerConfig:

    def __init__(self):
        self.AllowDenormalizedTerrainSize = False
        self.DisableMarkingOfOffLimitsAreas = False
        self.ExtendTerrainByCloningCount = int(0)
        self.MapCacheDisabled = False
        self.MapCacheForceEnableWrite = False
        self.EnableHeightSnapshotting = False
class TerrainMaterialThicknessSlimExtensions:

    def __init__(self):
        pass

class ITerrainOccupancyManager:

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
