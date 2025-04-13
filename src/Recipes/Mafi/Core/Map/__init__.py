
class CellEdge:
    def __init__(self):
        self.CenterTile = None
        self.C1 = None
        self.C2 = None

class ChunkHeightFromCellsProcessor:
    def __init__(self):
        pass


class IIslandMapGenerator:
    def __init__(self):
        self.Name = ""

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
    MAP_VERSION_LATEST = 0
    def __init__(self):
        self.CellCoastLines = None
        self.Item = None
        self.StartingLocation = None
        self.Name = ""
        self.MapVersion = 0
        self.MapName = ""
        self.Cells = None
        self.ControlPoints = None
        self.CellControlPoints = None
        self.NonCellControlPoints = None
        self.CellEdgePoints = None
        self.Bedrock = None
        self.ResourcesGenerators = None
        self.PropGenerationParams = None
        self.TerrainProps = None
        self.CellEdgeTerrainGenerators = None
        self.AllTerrainGenerators = None
        self.VirtualResources = None
        self.TerrainPostProcessors = None
        self.ProtosDb = None
        self.Config = None
        self.DifficultyConfig = None
        self.TerrainWidth = 0
        self.TerrainHeight = 0
        self.Chunks = None

class StartingLocation:
    def __init__(self):
        self.Position = None
        self.ShoreDirection = None

class IslandMapExtensions:
    def __init__(self):
        pass


class TerrainPropMapData:
    def __init__(self):
        from Mafi.Core.Prototypes import Proto
        self.ProtoId = Proto.ID()

        self.Position = None
        self.HeightOffset = None
        self.RotationYaw = None
        self.RotationPitch = None
        self.RotationRoll = None
        self.Scale = None
        self.VariantIndex = 0

class IslandMapConfig:
    Default = None
    def __init__(self):
        self.OceanFloorFlatDistance = None
        self.OceanFloorBaseHeight = None
        self.OceanFloorHeightPerDistanceFromCoast = None
        self.DoNotCreateOcean = False

class IslandMapDifficultyConfig:
    def __init__(self):
        self.MineableResourceSizeBonus = None
        self.CellHeightsBias = None

class MapCell:
    MIN_GROUND_HEIGHT = None
    DEFAULT_OCEAN_FLOOR_HEIGHT = None
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
        self.Id = None
        self.CenterPointIndex = 0
        self.PerimeterIndices = None
        self.NeighborCellsIndices = None
        self.NeighborCellsValidIndices = None
        self.IsOnMapBoundary = False
        self.DisableHeightBiasFromConfig = False

class MapCellEdge:
    def __init__(self):
        self.CenterPoint = None
        self.From = None
        self.To = None
        self.EdgeIndex = 0

class MapCellState:
    NotAvailable = None
    PendingAvailableToUnlock = None
    AvailableToUnlock = None
    PendingUnlocked = None
    Unlocked = None
    def __init__(self):
        self.value__ = 0

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
        from Mafi.Core.Map import MapCellSurfaceGeneratorProto
        self.Id = MapCellSurfaceGeneratorProto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.SurfaceMaterialsTopToBottom = None
        self.SurfaceMaterialsTopToBottomAlt = None
        self.Generator = None
        self.AltSurfaceNoiseParams = None
        self.AltSurfNoiseTurbulenceParams = None
        from Mafi import Fix64
        self.AltNoiseStartTransition = Fix64()
        self.AltNoiseEndTransition = Fix64()
        self.IsPhantom = False
        self.IsInitialized = False

    class ID:
        def __init__(self):
            self.Value = ""

class MapCellSurfaceGenerator:
    def __init__(self):
        self.Proto = None
        from Mafi import Option
        self.AltSurfaceNoise = Option()

class MapGenerationException:
    def __init__(self):
        self.Message = ""
        self.Data = None
        self.InnerException = None
        self.TargetSite = None
        self.StackTrace = ""
        self.HelpLink = ""
        self.Source = ""
        self.HResult = 0

class MapManager:
    def __init__(self):
        self.Map = None

class ProceduralIslandMapGenerator:
    NAME = ""
    def __init__(self):
        self.Name = ""

class ProceduralIslandMapGeneratorConfig:
    MAX_CELLS = 0
    def __init__(self):
        self.MapRandomSeed = ""
        from Mafi.Core.Prototypes import Proto
        self.BedrockMaterialId = Proto.ID()

        from Mafi import Fix32
        self.AreaChunksSquaredApprox = Fix32()
        self.MinCellDiameter = None
        self.OceanCellDiameter = None
        self.CellMinMaxDiameterMult = 0
        self.CellExpansionTrials = 0
        self.CellSizeGrowthFromStartExp = Fix32()
        self.CellSizeGrowthFromCoastExp = 0.0
        self.MinCellHeight = None
        self.CellHeightDiffMean = None
        self.CellHeightDiffStdDev = None
        self.CellHeightMeanDistanceToStartMult = Fix32()
        self.CellHeightDiffMaxStdDev = None
        self.OceanShape = None
        self.StartingTerrainResourcesAmountMult = Fix32()
        self.ResourcesRichnessMultDistance = None
        self.ResourcesRichnessMultExpBase = Fix32()
        from Mafi.Core.Map import MapCellSurfaceGeneratorProto
        self.DefaultIslandCellSurfaceId = MapCellSurfaceGeneratorProto.ID()

        self.DefaultOceanCellSurfaceId = MapCellSurfaceGeneratorProto.ID()

        self.DefaultCliffMaterialId = Proto.ID()


class OceanShape:
    HalfIsland = None
    FullAround = None
    def __init__(self):
        self.value__ = 0

class SimpleVirtualResource:
    def __init__(self):
        self.Name = ""
        self.Priority = 0
        self.Product = None
        self.ConfiguredCapacity = None
        self.Quantity = None
        self.EmergencyQuantity = None
        self.Capacity = None
        self.Position = None
        self.MaxRadius = None
        self.ResourceColor = None

class SquareMapGenerator:
    NAME = ""
    def __init__(self):
        self.Name = ""

class SquareMapGeneratorConfig:
    def __init__(self):
        from Mafi.Core.Prototypes import Proto
        self.BedrockMaterialId = Proto.ID()

        self.TerrainWidth = 0
        self.TerrainHeight = 0
        self.OceanAtDirection = None
        self.OceanSize = 0
        self.GroundHeight = None
        from Mafi.Core.Map import MapCellSurfaceGeneratorProto
        self.SurfaceProtoId = MapCellSurfaceGeneratorProto.ID()

        self.ForestProtoId = None
        self.ForestArea = None
        self.TerrainProps = None
        self.TerrainPostProcessors = None

class StaticIslandMapProvider:
    def __init__(self):
        self.Name = ""

class IStaticIslandMap:
    def __init__(self):
        self.Name = ""

class StaticIslandMapProviderConfig:
    def __init__(self):
        self.IslandMapType = None
        self.StartingLocation = None
        self.PreviewData = None

class StaticIslandMapPreviewData:
    def __init__(self):
        self.Name = None
        self.Description = None
        self.Difficulty = None
        self.IslandMapDataType = None
        self.StartingLocations = None
        self.PreviewPrefabPath = ""
        self.ResourcesStats = None
        self.TilesAtOrAboveElevationDataSorted = None
        self.MapSize = None
        self.NonOceanTilesCount = 0
        self.FlatNonOceanTilesCount = 0

class IslandMapDifficulty:
    Easy = None
    Medium = None
    Hard = None
    Insane = None
    def __init__(self):
        self.value__ = 0

class TestMapGenerator:
    OIL_DEPOSIT_CAP = None
    OIL_DEPOSIT_POS = None
    OIL_DEPOSIT_SIZE = None
    GROUNDWATER_DEPOSIT_CAP = None
    GROUNDWATER_DEPOSIT_POS = None
    GROUNDWATER_DEPOSIT_SIZE = None
    NAME = ""
    def __init__(self):
        self.Name = ""
        self.Config = None

class TestMapGeneratorConfig:
    def __init__(self):
        from Mafi.Core.Prototypes import Proto
        self.BedrockMaterialId = Proto.ID()

        self.CellSurfaceIds = None
        self.CliffProtoId = Proto.ID()

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
        from Mafi.Core.Prototypes import Proto
        self.ResourceId = Proto.ID()

        self.Size = None
        self.Depth = None
        self.Height = None

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
        self.ErrorMessage = ""
        self.MapCellId = None

class MapCellId:
    def __init__(self):
        self.Value = 0
