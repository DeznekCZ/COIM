class CustomPropsPostProcessor:

    def __init__(self):
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.ParallelizationStrategy = None
        self.SortingPriority = int(0)
        self.PassCount = int(0)
        self.LastGenerationTime = None
        self.Config = None
class Configuration:

    def __init__(self):
        self.SortingPriorityAdjustment = int(0)
class TerrainInfoForMaterial:

    def __init__(self):
        pass

class TerrainInfoForMaterialLookup:

    def __init__(self):
        pass

class CustomTreesPostProcessor:

    def __init__(self):
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.ParallelizationStrategy = None
        self.SortingPriority = int(0)
        self.PassCount = int(0)
        self.LastGenerationTime = None
        self.Config = None
class Configuration:

    def __init__(self):
        self.SortingPriorityAdjustment = int(0)
class FlowersPostProcessor:

    def __init__(self):
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.ParallelizationStrategy = None
        self.SortingPriority = int(0)
        self.PassCount = int(0)
        self.LastGenerationTime = None
        self.Config = None
class Configuration:

    def __init__(self):
        self.Seed = None
        self.SortingPriorityAdjustment = int(0)
class FlowersConfig:

    def __init__(self):
        self.FlowerMaterial = None
        self.SpawnMaterial = None
        self.SpawnProbability = None
        from Mafi import Fix32
        self.SpawnPointDistanceFalloff = Fix32()
        self.SpawnMaterialMinThickness = None
        self.SpawnMaterialMaxDepth = None
        self.SizeRandomnessMin = None
        self.SizeRandomnessMax = None
        self.MinDistanceFromOthers = None
        self.MaxInfluenceDistance = None
        self.Seed = None
        self.ThicknessFn = None
class ForestFloorPostProcessor:

    def __init__(self):
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.ParallelizationStrategy = None
        self.SortingPriority = int(0)
        self.PassCount = int(0)
        self.LastGenerationTime = None
        self.Config = None
class Configuration:

    def __init__(self):
        self.ForestFloorMaterialProto = None
        self.MinFloorThicknessFromOneTree = None
        self.MaxFloorThicknessFromOneTree = None
        self.SortingPriorityAdjustment = int(0)
class GeneratedPropsData:

    def __init__(self):
        pass

class Chunk:

    def __init__(self):
        pass

class GeneratedTreesData:

    def __init__(self):
        pass

class Chunk:

    def __init__(self):
        pass

class GrassOnRocksPostProcessor:

    def __init__(self):
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.ParallelizationStrategy = None
        self.SortingPriority = int(0)
        self.PassCount = int(0)
        self.LastGenerationTime = None
        self.Config = None
class Configuration:

    def __init__(self):
        self.GrassMaterialProto = None
        self.MaxIncompatibleMatCheckDistance = int(0)
        self.MinAddedThickness = None
        self.MaxExaminedDepth = None
        self.MaxCoverPercentage = None
        self.ThresholdDeltaHeight = None
        self.SortingPriorityAdjustment = int(0)
class MixedSurfaceMaterialsPostProcessor:

    def __init__(self):
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.ParallelizationStrategy = None
        self.SortingPriority = int(0)
        self.PassCount = int(0)
        self.LastGenerationTime = None
        self.Config = None
class Configuration:

    def __init__(self):
        self.SourceMaterialProto = None
        self.ReplacedMaterialProto = None
        self.MinReplacedThickness = None
        self.MaxReplacedThickness = None
        self.ReplacedThicknessFn = None
        self.MaxDepthSearched = None
        from Mafi import Fix32
        self.SlopeRestrictionStart = Fix32()
        from Mafi import Fix32
        self.SlopeRestrictionEnd = Fix32()
        self.SlopeTestDistance = None
        self.SortingPriorityAdjustment = int(0)
class ParticleErosionPostProcessor:

    def __init__(self):
        self.Name = str(0)
        self.SortingPriority = int(0)
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.Config = None
        self.Id = int(0)
        self.IsDisabled = False
        self.ParallelizationStrategy = None
        self.PassCount = int(0)
        self.LastGenerationTime = None
class Configuration:

    def __init__(self):
        self.AddSuppressionRegion = None
        self.AddNewPass = None
        self.SortingPriorityAdjustment = int(0)
class ParticleErosionConfig:

    def __init__(self):
        self.Name = str(0)
class ParticleInfo:

    def __init__(self):
        pass

class TerminationReason:

    def __init__(self):
        pass

class PolygonFlattenPostProcessor:

    def __init__(self):
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.ParallelizationStrategy = None
        self.SortingPriority = int(0)
        self.PassCount = int(0)
        self.LastGenerationTime = None
        self.Config = None
class Configuration:

    def __init__(self):
        self.Polygon = None
        self.TargetHeightSource = None
        self.ExplicitHeight = None
        self.RoundHeightToInteger = False
        self.TransitionDistance = None
        self.InteractionMode = None
        self.SmoothAtTransitionStart = False
        self.SmoothAtTransitionEnd = False
        from Mafi import Option
        self.SurfaceMaterial = Option()
        self.SurfaceMaterialThickness = None
        self.AlwaysApplySurfaceMaterial = False
        from Mafi import Option
        self.BaseMaterial = Option()
        self.SortingPriorityAdjustment = int(0)
class TargetHeightSourceEnum:

    def __init__(self):
        pass

class InteractionModeEnum:

    def __init__(self):
        pass

class PolygonRampPostProcessor:

    def __init__(self):
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.ParallelizationStrategy = None
        self.SortingPriority = int(0)
        self.PassCount = int(0)
        self.LastGenerationTime = None
        self.Config = None
class Configuration:

    def __init__(self):
        self.SnapControlPointsToTerrain = False
        self.Polygon = None
        self.RoundControlPointHeightsToInteger = False
        self.TransitionDistance = None
        self.InteractionMode = None
        self.SmoothAtTransitionStart = False
        self.SmoothAtTransitionEnd = False
        from Mafi import Option
        self.SurfaceMaterial = Option()
        self.SurfaceMaterialThickness = None
        self.AlwaysApplySurfaceMaterial = False
        from Mafi import Option
        self.BaseMaterial = Option()
        self.SortingPriorityAdjustment = int(0)
class PolygonReplaceMaterialPostProcessor:

    def __init__(self):
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.ParallelizationStrategy = None
        self.SortingPriority = int(0)
        self.PassCount = int(0)
        self.LastGenerationTime = None
        self.Config = None
class Configuration:

    def __init__(self):
        self.Polygon = None
        self.TransitionDistance = None
        self.OldMaterial = None
        from Mafi import Option
        self.NewMaterial = Option()
        self.MaxReplacedThickness = None
        self.ReplacedMaterialThicknessMult = None
        self.SortingPriorityAdjustment = int(0)
        self.ProcessingPhase = None
class PolygonRestrictPlacementPostProcessor:

    def __init__(self):
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.ParallelizationStrategy = None
        self.SortingPriority = int(0)
        self.PassCount = int(0)
        self.LastGenerationTime = None
        self.Config = None
class Configuration:

    def __init__(self):
        self.Polygon = None
        self.ClearTrees = False
        self.ClearProps = False
        self.SortingPriorityAdjustment = int(0)
class PolygonSmoothPostProcessor:

    def __init__(self):
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.ParallelizationStrategy = None
        self.SortingPriority = int(0)
        self.PassCount = int(0)
        self.LastGenerationTime = None
        self.Config = None
class Configuration:

    def __init__(self):
        self.Polygon = None
        self.Passes = int(0)
        self.Strength = None
        self.TransitionDistance = None
        self.SortingPriorityAdjustment = int(0)
class PolygonTreeGeneratorPostProcessor:

    def __init__(self):
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.ParallelizationStrategy = None
        self.SortingPriority = int(0)
        self.PassCount = int(0)
        self.LastGenerationTime = None
        self.Config = None
class Configuration:

    def __init__(self):
        self.Trees = None
        self.Polygon = None
        self.MaxInfluenceDistance = None
        self.MinSpacing = None
        self.MaxSpacing = None
        self.MinFarmableMaterialThickness = None
        self.SlopeCheckDistance = int(0)
        self.MaxHeightDelta = None
        from Mafi import Option
        self.LimitToMaterialProto = Option()
        self.SpacingFunction = None
        self.SpawnFunction = None
        self.SortingPriorityAdjustment = int(0)
class SuppressErosionRegion:

    def __init__(self):
        self.Polygon = None
class TerrainChunk64BitMap:

    def __init__(self):
        self.BackingArray = None
class TerrainPropsPostProcessor:

    def __init__(self):
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.ParallelizationStrategy = None
        self.SortingPriority = int(0)
        self.PassCount = int(0)
        self.LastGenerationTime = None
        self.Config = None
class Configuration:

    def __init__(self):
        self.SortingPriorityAdjustment = int(0)
        self.CreateNewConfig = None
class PropProtoWithVariant:

    def __init__(self):
        pass

class PropSpawnConfig:

    def __init__(self):
        self.AddSpawnMaterial = None
        self.AddSpawnedProps = None
class RecordLookup:

    def __init__(self):
        pass

class TerrainUnderPropsPostProcessor:

    def __init__(self):
        self.Name = str(0)
        self.Id = int(0)
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.ParallelizationStrategy = None
        self.SortingPriority = int(0)
        self.PassCount = int(0)
        self.LastGenerationTime = None
        self.Config = None
class Configuration:

    def __init__(self):
        self.SortingPriorityAdjustment = int(0)
class TreeWithWeight:

    def __init__(self):
        pass

