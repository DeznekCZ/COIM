
class HeightmapFeaturePreviewChunkData:
    SIZE = 0
    def __init__(self):
        self.Chunk = None
        self.Area = None
        self.Dirty = False
        self.Heights = None

class HeightmapTopBottomPreviewChunkData:
    SIZE = 0
    def __init__(self):
        self.Chunk = None
        self.Area = None
        self.Dirty = False
        self.Heights = None

class PointFeaturePreviewChunkData:
    def __init__(self):
        self.Chunk = None
        self.Points = None
