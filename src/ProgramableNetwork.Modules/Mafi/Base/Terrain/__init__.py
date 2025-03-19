class FlowersOnTerrainConfig:

    def __init__(self):
        self.FlowersConfigs = None
class FlowersConfig:

    def __init__(self):
        pass

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
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.Order = int(0)
        self.Config = None
class Configuration:

    def __init__(self):
        self.Position = None
        self.Direction = None
        self.Difficulty = None
        from Mafi import Option
        self.Description = Option()
        self.Order = int(0)
        self.StartingLocationArea = int(0)
        self.IsValid = False
        self.ValidationStatus = str(0)
        self.ValidationError = str(0)
        self.PlacementAttemptResults = None
class TerrainTexturedPorpsTerrainPostProcessor:

    def __init__(self):
        pass

class Config:

    def __init__(self):
        pass

class ConfigRecord:

    def __init__(self):
        pass

class RecordLookup:

    def __init__(self):
        pass

class CellSurfacesData:

    def __init__(self):
        pass

class TerrainDetailsData:

    def __init__(self):
        pass

class TerrainFeaturesTooltips:

    def __init__(self):
        pass

class TerrainGenPriority:

    def __init__(self):
        pass

class TerrainMaterialsData:

    def __init__(self):
        pass

class TerrainTileSurfaceDecalsData:

    def __init__(self):
        pass

class TerrainTileSurfacesData:

    def __init__(self):
        pass

