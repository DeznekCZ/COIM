
class TerrainPropId:
    Invalid = None
    def __init__(self):
        self.IsValid = False
        self.Position = None

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
        self.ErrorMessage = ""
        self.Area = None

class TerrainPropData:
    def __init__(self):
        self.IsValid = False
        self.Position = None
        self.Proto = None
        self.Id = None
        self.PositionWithinTile = None
        self.RotationYaw = None
        self.RotationPitch = None
        self.RotationRoll = None
        self.Scale = None
        self.PlacedAtHeight = None
        self.Variant = None
        self.PlacementHeightOffset = None

    class PropVariant:
        def __init__(self):
            self.UvOriginAndSizePackedTexIndex = None

class TerrainPropsManager:
    def __init__(self):
        self.TerrainProps = None
        self.TerrainTileToProp = None
        self.PropsCount = 0
        self.RemovedPropsCount = 0
        self.PropChangedAt = None

class PropsRemovalProcessor:
    COST_PER_PROP = None
    def __init__(self):
        pass


class TerrainPropProto:
    def __init__(self):
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

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
        self.AllowYawRandomization = False
        self.AllowPitchRandomization = False
        self.AllowRollRandomization = False
        self.DoesNotBlocksVehicles = False
        self.IsPhantom = False
        self.IsInitialized = False

    class PropGfx:
        Empty = None
        def __init__(self):
            self.PrefabPath = ""
            self.UseTerrainTextures = False
            self.BoundingBox = None
            self.IconPath = ""
            self.PreviewMatPath = ""

class TerrainPropBoundingShape:
    Rectangle = None
    Circle = None
    def __init__(self):
        self.value__ = 0

class ITerrainPropsManager:
    def __init__(self):
        pass

