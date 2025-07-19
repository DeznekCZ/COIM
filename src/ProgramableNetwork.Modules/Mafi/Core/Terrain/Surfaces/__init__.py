class TileSurfaceDecalSlimId:
    PhantomId = None

    def __init__(self):
        self.IsPhantom = False
        self.IsNotPhantom = False
class TileSurfaceDecalsSlimIdManager:

    def __init__(self):
        self.PhantomProto = None
        self.MaxIdValue = int(0)
class SurfaceDecalCategoryProto:

    def __init__(self):
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
class SurfaceDecalValidator:

    def __init__(self):
        self.Priority = None
class TerrainTileSurfaceDecalProto:
    PHANTOM_ID = None
    Phantom = None

    def __init__(self):
        self.SlimId = None
        self.Id = None
        self.Graphics = None
        self.IconPath = str(0)
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
        self.IconPath = str(0)
