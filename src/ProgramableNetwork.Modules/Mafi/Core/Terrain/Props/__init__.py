class TerrainPropId:

    def __init__(self):
        self.IsValid = False
class ExplicitMapPropsTerrainPostProcessor:

    def __init__(self):
        pass

class QuickRemovePropsCmd:

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
class TerrainPropData:

    def __init__(self):
        self.IsValid = False
        self.Position = None
class PropVariant:

    def __init__(self):
        pass

class TerrainPropsManager:

    def __init__(self):
        self.TerrainProps = None
        self.TerrainTileToProp = None
        self.PropsCount = int(0)
        self.RemovedPropsCount = int(0)
        self.PropChangedAt = None
class PropsRemovalProcessor:

    def __init__(self):
        pass

class TerrainPropProto:

    def __init__(self):
        self.Id = None
        self.BoundingShape = None
        self.Extents = None
        self.BaseScale = None
        self.ProductWhenHarvested = None
        self.MaxSpawnDepth = None
        self.DespawnBuriedThreshold = None
        self.Variants = None
        self.Graphics = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class PropGfx:

    def __init__(self):
        pass

class TerrainPropBoundingShape:

    def __init__(self):
        pass

class ITerrainPropsManager:

    def __init__(self):
        pass

