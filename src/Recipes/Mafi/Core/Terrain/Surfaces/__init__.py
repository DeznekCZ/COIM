
class TileSurfaceDecalSlimId:
    PhantomId = None
    def __init__(self):
        self.IsPhantom = False
        self.IsNotPhantom = False
        self.Value = None

class TileSurfaceDecalsSlimIdManager:
    def __init__(self):
        self.PhantomProto = None
        self.MaxIdValue = 0
        self.ManagedProtos = None

class SurfaceDecalCategoryProto:
    def __init__(self):
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.Order = 0.0
        self.IsPhantom = False
        self.IsInitialized = False

class SurfaceDecalValidator:
    def __init__(self):
        self.Priority = None

class TerrainTileSurfaceDecalProto:
    from Mafi.Core.Prototypes import Proto
    PHANTOM_ID = Proto.ID('__PHANTOM__TILE_SURFACE_DECAL__')
    Phantom = None
    def __init__(self):
        self.SlimId = None
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.Graphics = None
        self.IconPath = ""
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.IsPhantom = False
        self.IsInitialized = False

    class Gfx:
        Empty = None
        def __init__(self):
            self.IconPath = ""
            from Mafi import Option
            self.Category = Option()
            self.AlbedoTexturePath = ""
            self.Color = None
            self.RendererIndex = 0
