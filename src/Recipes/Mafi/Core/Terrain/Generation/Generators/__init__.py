
class CellEdgeTerrainGenerator:
    def __init__(self):
        self.Name = ""
        self.Position = None
        self.MaxRadius = None
        self.Priority = 0
        self.ResourceColor = None
        self.Cell1 = None
        self.Cell2 = None
        self.DeltaPriority = 0
        self.CliffResourceProto = None
        self.TransitionRadius = None
        self.ExtraTransitionRadiusPerHeightDiff = None
        self.BoundaryComplexity = None
        self.TopRadius = None
        self.ExtraThickness = None
        self.BlobShapeSeed = None
        self.TurbulenceParams = None
        self.ClampTerrainAboveTopCell = False

class CellEdgeTerrainGeneratorFactory:
    def __init__(self):
        self.DeltaPriority = 0
        self.CliffResourceProto = None
        self.TransitionRadius = None
        self.ExtraTransitionRadiusPerHeightDiff = None
        self.BoundaryComplexity = None
        self.TopRadius = None
        self.ExtraThickness = None
        self.RandomSeed = None
        self.TurbulenceParams = None
        self.ClampAboveTopCell = False

class LineBlobTerrainResourceGenerator:
    def __init__(self):
        self.Name = ""
        self.Position = None
        self.MaxRadius = None
        self.Priority = 0
        self.ResourceColor = None
        self.From = None
        self.To = None
        self.DeltaPriority = 0
        self.ResourceProto = None
        self.ResourceRadius = None
        self.TransitionRadius = None
        self.ExtraFalloffTransitionRadius = None
        self.TransitionFn = None
        self.BaseNoiseParams = None
        self.BelowSurfaceExtraHeight = None
        self.BelowSurfaceMaxDepth = None
        self.ShapeInversionDepth = None
        self.GroundLevelBias = None
        self.NoiseSeed = None
        self.TurbulenceParams = None
        self.SteppedNoiseParams = None
        self.IsRidged = False
        self.ReplacePreviousResource = False
        self.OnlyPlaceOnTopAboveGround = False
        self.OnlyReplaceExistingMaterials = False
        self.LimitToParentCellHeight = False
        from Mafi import Fix32
        self.SigmoidCenterDistance = Fix32()
        self.SigmoidSmoothness = Fix32()
        from Mafi import Option
        self.SurfaceCoverResourceProto = Option()
        self.SurfaceCoverThickness = None
        self.UseMineableResourceConfig = False
        self.CoordWarpNoiseParams = None
        self.HeightBiasAtFromPoint = None
        self.HeightBiasAtToPoint = None

class TerrainPropGenerationParams:
    def __init__(self):
        self.BaseNoise = None
        self.SpacingNoise = None
        from Mafi import Fix32
        self.BaseNoisePeriod = Fix32()
        self.SpacingRadiusNoiseParams = None
        self.MinSpacingRadius = 0
        self.MaxSpacingRadius = 0
        self.NoiseSeed = None

class TreesResourceGenerator:
    def __init__(self):
        self.Name = ""
        self.Position = None
        self.MaxRadius = None
        self.Priority = 0
        self.ResourceColor = None
        self.From = None
        self.To = None
        self.DeltaPriority = 0
        self.ForestProto = None
        self.ResourceRadius = None
        self.TransitionRadius = None
        from Mafi import Fix32
        self.BaseNoisePeriod = Fix32()
        self.SpacingRadiusNoiseParams = None
        self.MinSpacingRadius = Fix32()
        self.MaxSpacingRadius = Fix32()
        self.NoiseSeed = None
        self.LimitToParentCellHeight = False
        from Mafi import Option
        self.LimitToMaterialProto = Option()
