
class CrossSectionVertex:
    def __init__(self):
        self.Coord = None
        self.Normal = None
        self.TextureCoordY = 0.0

class CrossSectionVertexFloat:
    def __init__(self):
        self.Coord = None
        self.Normal = None
        self.TextureCoordY = 0.0
        self.Color = None

class IconSpec:
    def __init__(self):
        self.Path = ""
        self.Color = None

class ToolbarIconSizeParam:
    TINY = None
    SMALLER = None
    SMALL = None
    DEFAULT = None
    LARGE = None
    XLARGE = None
    def __init__(self):
        self.AllowedProtoType = None
        self.Size = None
