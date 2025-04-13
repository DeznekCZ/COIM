
class IComputingManager:
    def __init__(self):
        self.ComputingProductProto = None
        self.ProducedLastTick = None
        self.DemandedThisTick = None
        self.GenerationCapacityThisTick = None

class FluidIndicatorGfxParams:
    def __init__(self):
        self.SizePerTextureWidthMeters = 0.0
        self.DetailsScale = 0.0
        self.StillMovementScale = 0.0

class LoosePileTextureParams:
    Default = None
    def __init__(self):
        self.Scale = 0.0
        self.OffsetX = 0.0
        self.OffsetY = 0.0
