
class FlowersOnTerrainConfig:
    def __init__(self):
        self.FlowersConfigs = None

    class FlowersConfig:
        def __init__(self):
            from Mafi.Core.Prototypes import Proto
            self.FlowerMaterialId = Proto.ID()

            self.SpawnMaterialId = Proto.ID()

            self.SpawnProbabilityBase = None
            self.SpawnMaterialMinThickness = None
            self.MinDistanceFromOthers = None
            self.KeepExpandingFromLatestProbab = None
            self.CoreSizeMean = 0
            self.CoreSizeStdDev = 0
            self.AuxiliarySizeMean = 0
            self.AuxiliarySizeStdDev = 0

class FlowersTerrainPostProcessor:
    def __init__(self):
        pass


class GrassOnRocksTerrainPostProcessor:
    def __init__(self):
        pass


class MixedGrassTerrainPostProcessor:
    def __init__(self):
        pass


class SmoothTerrainPostProcessor:
    def __init__(self):
        pass


class StartingLocationV2:
    def __init__(self):
        self.Name = ""
        self.Id = 0
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.Order = 0
        self.Config = None
        self.ConfigMutable = None
        self.ValidationPerformed = False

    class Configuration:
        def __init__(self):
            self.Position = None
            self.Direction = None
            self.Difficulty = None
            from Mafi import Option
            self.Description = Option()
            self.Order = 0
            self.StartingLocationArea = 0
            self.IsValid = False
            self.ValidationStatus = ""
            self.ValidationError = ""
            self.PlacementAttemptResults = None

class TerrainTexturedPorpsTerrainPostProcessor:
    def __init__(self):
        pass


    class Config:
        def __init__(self):
            self.Records = None

    class ConfigRecord:
        def __init__(self):
            self.MaterialIds = None
            self.PropIds = None
            self.PropMaterialOverride = None
            self.BelowPropMaterial = None
            self.SpawnProbability = None
            self.MinScale = None
            self.MaxScale = None
            self.MaxHeightDelta = None
            self.PlacementHeightOffset = None

    class RecordLookup:
        def __init__(self):
            self.Props = None
            self.SpawnSeed = None
            self.SpawnProbability = None
            self.MinScale = None
            self.MaxScale = None
            self.MaxHeightDelta = None
            self.PlacementHeightOffset = None
            self.PropMaterialOverride = None
            self.BelowPropMaterial = None

class CellSurfacesData:
    def __init__(self):
        pass


class TerrainDetailsData:
    def __init__(self):
        pass


class TerrainFeaturesTooltips:
    SORTING_PRIORITY_ADJUSTMENT = ""
    ORDER_MATTERS_NOTE = ""
    WHERE_TO_EDIT_PRIORITY = ""
    MAX_INFLUENCE_DISTANCE_FOR_POLYGON = ""
    def __init__(self):
        pass


class TerrainGenPriority:
    SandCoast = None
    RockCliffCoast = None
    CoalRect = None
    SandPit = None
    IronOre = None
    CopperOre = None
    GoldOre = None
    LeafTrees = None
    ConiferTrees = None
    CrudeOil = None
    def __init__(self):
        self.value__ = 0

class TerrainMaterialsData:
    def __init__(self):
        pass


class TerrainTileSurfaceDecalsData:
    def __init__(self):
        pass


class TerrainTileSurfacesData:
    def __init__(self):
        pass

