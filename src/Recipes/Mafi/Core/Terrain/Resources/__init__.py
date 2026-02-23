
class ProductResource:
    def __init__(self):
        self.Product = None
        self.Height = None
        self.Depth = None

class ProductVirtualResource:
    def __init__(self):
        self.Product = None
        self.VirtualThickness = None

class TerrainResourcesProvider:
    def __init__(self):
        self.LooseTerrainProducts = None
        self.VirtualResourceProducts = None

class TerrainTileResources:
    def __init__(self):
        self.Tile = None
        self.Products = None
        self.VirtualResources = None
