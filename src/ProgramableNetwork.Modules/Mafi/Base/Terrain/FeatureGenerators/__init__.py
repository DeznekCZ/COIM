class ExplicitOceanFloorRegion:

    def __init__(self):
        self.Polygon = None
        self.OceanDepth = None
        self.TransitionSize = None
class OceanFloorFeatureGenerator:

    def __init__(self):
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.SortingPriority = int(0)
        self.LastGenerationTime = None
        self.Config = None
class Configuration:

    def __init__(self):
        self.MinOceanDepth = None
        self.MaxOceanDepth = None
        self.OceanDepthIncreaseRatePer64Tiles = None
        from Mafi import Fix32
        self.DistanceBias = Fix32()
        self.InitialBedrockLayerThickness = None
        self.HeightBiasFn = None
        self.SortingPriorityAdjustment = int(0)
        self.RegionTranslationAmount = None
class PolygonSurfaceFeatureGenerator:

    def __init__(self):
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.SortingPriority = int(0)
        self.LastGenerationTime = None
        self.Config = None
class Configuration:

    def __init__(self):
        self.Polygon = None
        self.MaxInfluenceDistance = None
        self.BaseHeight = None
        self.SurfaceMaterial = None
        self.BaseMaterial = None
        self.BaseThickness = None
        self.BaseHeightFn = None
        self.SurfaceHeightFn = None
        from Mafi import Fix32
        self.SurfaceDepthMult = Fix32()
        self.MaxSurfaceThicknessFn = None
        self.AllowNonIntegerSurfaceHeights = False
        self.ContributesToOceanCoast = False
        self.DisableSubSurfaceGeneration = False
        self.SortingPriorityAdjustment = int(0)
        self.TotalGeneratedChunks = int(0)
        self.ChunksWithNoContribution = int(0)
        self.ChunksWithSurfaceAtMaxThickness = int(0)
class PolygonTerrainFeatureGenerator:

    def __init__(self):
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.SortingPriority = int(0)
        self.LastGenerationTime = None
        self.Config = None
class Configuration:

    def __init__(self):
        self.Polygon = None
        self.TerrainMaterial = None
        self.MaxInfluenceDistance = None
        self.BelowSurfaceMaxDepth = None
        self.ShapeInversionDepth = None
        self.BelowSurfaceExtraHeight = None
        self.TerrainBlendHeightRange = None
        self.UndergroundDepthMult = None
        self.IgnoreAsResource = False
        self.OnlyPlaceOnTopAboveGround = False
        self.OnlyReplaceExistingMaterials = False
        self.HeightFn = None
        from Mafi import Option
        self.SurfaceCoverMaterial = Option()
        self.SurfaceCoverThicknessFn = None
        self.SortingPriorityAdjustment = int(0)
        self.TotalGeneratedChunks = int(0)
        self.ChunksWithNoContribution = int(0)
class PolygonTerrainReplaceGenerator:

    def __init__(self):
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.SortingPriority = int(0)
        self.LastGenerationTime = None
        self.Config = None
class Configuration:

    def __init__(self):
        self.Polygon = None
        self.TerrainMaterial = None
        self.MaxInfluenceDistance = None
        self.TerrainBlendHeightRange = None
        self.ThicknessFn = None
        self.SortingPriorityAdjustment = int(0)
        self.TotalGeneratedChunks = int(0)
        self.ChunksWithNoContribution = int(0)
class SetHeightInAreaGenerator:

    def __init__(self):
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.SortingPriority = int(0)
        self.LastGenerationTime = None
        self.BoundingBox = None
        self.TargetHeight = None
        self.SetStrategy = None
class HeightSetStrategy:

    def __init__(self):
        pass

class ThrowExceptionTerrainFeature:

    def __init__(self):
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.SortingPriority = int(0)
        self.LastGenerationTime = None
        self.Config = None
class Configuration:

    def __init__(self):
        self.ThrowOnlyDuringParallelProcessing = False
        self.ThrowOnInit = False
        self.ThrowOnGenerateChunk = False
        self.SortingPriorityAdjustment = int(0)
class VirtualResourceFeatureGenerator:

    def __init__(self):
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.Config = None
        self.Radius = None
class Configuration:

    def __init__(self):
        self.VirtualResource = None
        self.ConfiguredCapacity = None
        self.Position = None
        self.MaxRadius = None
