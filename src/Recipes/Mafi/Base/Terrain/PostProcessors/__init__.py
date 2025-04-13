
class CustomPropsPostProcessor:
    def __init__(self):
        self.Name = ""
        self.Id = 0
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.ParallelizationStrategy = None
        self.SortingPriority = 0
        self.PassCount = 0
        self.LastGenerationTime = None
        self.Config = None
        self.ConfigMutable = None

    class Configuration:
        def __init__(self):
            self.SortingPriorityAdjustment = 0
            self.CustomPlacedProps = None
            self.TerrainInfo = None

    class TerrainInfoForMaterial:
        def __init__(self):
            from Mafi.Core.Prototypes import Proto
            self.PropMaterialOverrideOld = Proto.ID()

            self.BelowPropMaterialOld = Proto.ID()

            self.PropMaterialOverride = None
            self.BelowPropMaterial = None

    class TerrainInfoForMaterialLookup:
        def __init__(self):
            self.PropMaterialOverride = None
            self.BelowPropMaterial = None

class CustomTreesPostProcessor:
    def __init__(self):
        self.Name = ""
        self.Id = 0
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.ParallelizationStrategy = None
        self.SortingPriority = 0
        self.PassCount = 0
        self.LastGenerationTime = None
        self.Config = None
        self.ConfigMutable = None

    class Configuration:
        def __init__(self):
            self.SortingPriorityAdjustment = 0
            self.CustomTreeList = None

class FlowersPostProcessor:
    MAX_FLOWER_SPREAD_DISTANCE_SQUARED = 0
    def __init__(self):
        self.Name = ""
        self.Id = 0
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.ParallelizationStrategy = None
        self.SortingPriority = 0
        self.PassCount = 0
        self.LastGenerationTime = None
        self.Config = None
        self.ConfigMutable = None

    class Configuration:
        def __init__(self):
            self.Seed = None
            self.SortingPriorityAdjustment = 0
            self.AddFlowerConfig = None
            self.FlowersConfigs = None

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
        self.Name = ""
        self.Id = 0
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.ParallelizationStrategy = None
        self.SortingPriority = 0
        self.PassCount = 0
        self.LastGenerationTime = None
        self.Config = None
        self.ConfigMutable = None

    class Configuration:
        def __init__(self):
            self.ForestFloorMaterialProto = None
            self.MinFloorThicknessFromOneTree = None
            self.MaxFloorThicknessFromOneTree = None
            self.SortingPriorityAdjustment = 0

class GeneratedPropsData:
    def __init__(self):
        pass


    class Chunk:
        def __init__(self):
            self.PropsAndSurfaceMaterials = None
            self.ClearedPolygons = None

class GeneratedTreesData:
    def __init__(self):
        pass


    class Chunk:
        def __init__(self):
            self.Trees = None
            self.ClearedPolygons = None

class GrassOnRocksPostProcessor:
    MAX_CHECK_DISTANCE = 0
    def __init__(self):
        self.Name = ""
        self.Id = 0
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.ParallelizationStrategy = None
        self.SortingPriority = 0
        self.PassCount = 0
        self.LastGenerationTime = None
        self.Config = None
        self.ConfigMutable = None

    class Configuration:
        def __init__(self):
            self.GrassMaterialProto = None
            self.MaxIncompatibleMatCheckDistance = 0
            self.MinAddedThickness = None
            self.MaxExaminedDepth = None
            self.MaxCoverPercentage = None
            self.ThresholdDeltaHeight = None
            self.SortingPriorityAdjustment = 0

class MixedSurfaceMaterialsPostProcessor:
    def __init__(self):
        self.Name = ""
        self.Id = 0
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.ParallelizationStrategy = None
        self.SortingPriority = 0
        self.PassCount = 0
        self.LastGenerationTime = None
        self.Config = None
        self.ConfigMutable = None

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
            self.SlopeRestrictionEnd = Fix32()
            self.SlopeTestDistance = None
            self.SortingPriorityAdjustment = 0

class ParticleErosionPostProcessor:
    MAX_PARTICLE_TRAVEL_DISTANCE = None
    def __init__(self):
        self.Name = ""
        self.SortingPriority = 0
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.Config = None
        self.Id = 0
        self.IsDisabled = False
        self.ParallelizationStrategy = None
        self.PassCount = 0
        self.LastGenerationTime = None
        self.ConfigMutable = None

    class Configuration:
        def __init__(self):
            self.AddSuppressionRegion = None
            self.AddNewPass = None
            self.SortingPriorityAdjustment = 0
            self.SuppressErosionRegions = None
            self.AddIgnoredMaterial = None
            self.IgnoredMaterials = None
            self.PassesConfigs = None
            self.ForceSerialProcessingUpdateAfterEachParticle = False

    class ParticleErosionConfig:
        def __init__(self):
            self.Name = ""
            self.LowestProcessedHeightUnderOcean = None
            self.Seed = None
            from Mafi import Fix32
            self.ParticlesPerTile = Fix32()
            self.MinParticleSteps = 0
            self.MaxParticleSteps = 0
            self.StepLength = None
            self.TerrainInteractionMult = Fix32()
            self.TerrainInteractionLifetimeExponent = Fix32()
            self.ErodeSpeedThreshold = Fix32()
            self.AccumulatedMaterialErodeThresholdMult = Fix32()
            self.MinSpeed = Fix32()
            self.MinGradient = Fix32()
            self.MomentumFactor = Fix32()
            self.GradientToVelocityMult = Fix32()
            self.ExtendedGradStep = None
            self.ExtendedGradWeight = Fix32()
            self.Friction = Fix32()
            self.MaxErosionPerStep = Fix32()
            self.MaxSpeedDelta = Fix32()
            self.SpeedSmoothingThreshold = Fix32()

    class ParticleInfo:
        def __init__(self):
            self.Position = None
            self.Velocity = None
            self.DeltaHeight = None
            self.MaterialAmount = None
            self.TerminationReason = None

    class TerminationReason:
        None = None
        ZeroGradient = None
        OutOfBounds = None
        LowVelocity = None
        GoingUp = None
        MaxStepsReached = None
        def __init__(self):
            self.value__ = 0

class PolygonFlattenPostProcessor:
    def __init__(self):
        self.Name = ""
        self.Id = 0
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.ParallelizationStrategy = None
        self.SortingPriority = 0
        self.PassCount = 0
        self.LastGenerationTime = None
        self.Config = None
        self.ConfigMutable = None

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
            self.BaseMaterial = Option()
            self.SortingPriorityAdjustment = 0

        class TargetHeightSourceEnum:
            ExplicitHeight = None
            MinOfControlPointsHeights = None
            MaxOfControlPointsHeights = None
            def __init__(self):
                self.value__ = 0

        class InteractionModeEnum:
            AddAndRemove = None
            AddOnly = None
            RemoveOnly = None
            def __init__(self):
                self.value__ = 0

class PolygonRampPostProcessor:
    def __init__(self):
        self.Name = ""
        self.Id = 0
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.ParallelizationStrategy = None
        self.SortingPriority = 0
        self.PassCount = 0
        self.LastGenerationTime = None
        self.Config = None
        self.ConfigMutable = None

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
            self.BaseMaterial = Option()
            self.SortingPriorityAdjustment = 0

class PolygonReplaceMaterialPostProcessor:
    def __init__(self):
        self.Name = ""
        self.Id = 0
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.ParallelizationStrategy = None
        self.SortingPriority = 0
        self.PassCount = 0
        self.LastGenerationTime = None
        self.Config = None
        self.ConfigMutable = None

    class Configuration:
        def __init__(self):
            self.Polygon = None
            self.TransitionDistance = None
            self.OldMaterial = None
            from Mafi import Option
            self.NewMaterial = Option()
            self.MaxReplacedThickness = None
            self.ReplacedMaterialThicknessMult = None
            self.SortingPriorityAdjustment = 0
            self.ProcessingPhase = None

class PolygonRestrictPlacementPostProcessor:
    def __init__(self):
        self.Name = ""
        self.Id = 0
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.ParallelizationStrategy = None
        self.SortingPriority = 0
        self.PassCount = 0
        self.LastGenerationTime = None
        self.Config = None
        self.ConfigMutable = None

    class Configuration:
        def __init__(self):
            self.Polygon = None
            self.ClearTrees = False
            self.ClearProps = False
            self.SortingPriorityAdjustment = 0

class PolygonSmoothPostProcessor:
    def __init__(self):
        self.Name = ""
        self.Id = 0
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.ParallelizationStrategy = None
        self.SortingPriority = 0
        self.PassCount = 0
        self.LastGenerationTime = None
        self.Config = None
        self.ConfigMutable = None

    class Configuration:
        def __init__(self):
            self.Polygon = None
            self.Passes = 0
            self.Strength = None
            self.TransitionDistance = None
            self.SortingPriorityAdjustment = 0

class PolygonTreeGeneratorPostProcessor:
    def __init__(self):
        self.Name = ""
        self.Id = 0
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.ParallelizationStrategy = None
        self.SortingPriority = 0
        self.PassCount = 0
        self.LastGenerationTime = None
        self.Config = None
        self.ConfigMutable = None

    class Configuration:
        def __init__(self):
            self.Trees = None
            self.Polygon = None
            self.MaxInfluenceDistance = None
            self.MinSpacing = None
            self.MaxSpacing = None
            self.MinFarmableMaterialThickness = None
            self.SlopeCheckDistance = 0
            self.MaxHeightDelta = None
            from Mafi import Option
            self.LimitToMaterialProto = Option()
            self.SpacingFunction = None
            self.SpawnFunction = None
            self.SortingPriorityAdjustment = 0
            self.AddNewTreeOption = None

class SuppressErosionRegion:
    def __init__(self):
        self.Polygon = None

class TerrainChunk64BitMap:
    def __init__(self):
        self.BackingArray = None

class TerrainPropsPostProcessor:
    def __init__(self):
        self.Name = ""
        self.Id = 0
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.ParallelizationStrategy = None
        self.SortingPriority = 0
        self.PassCount = 0
        self.LastGenerationTime = None
        self.Config = None
        self.ConfigMutable = None

    class Configuration:
        def __init__(self):
            self.SortingPriorityAdjustment = 0
            self.CreateNewConfig = None
            self.SpawnConfigs = None
            self.CustomPlacedProps = None

    class PropProtoWithVariant:
        def __init__(self):
            self.PropProto = None
            self.RestrictVariants = False
            self.MinVariantIndex = 0
            self.MaxVariantIndex = 0

    class PropSpawnConfig:
        def __init__(self):
            self.AddSpawnMaterial = None
            self.AddSpawnedProps = None
            self.Name = ""
            self.SpawnMaterials = None
            self.MinSpawnMaterialThickness = None
            self.MaxSpawnMaterialDepth = None
            self.SpawnedProps = None
            from Mafi import Option
            self.PropMaterialOverride = Option()
            self.BelowPropMaterial = Option()
            self.SpawnProbability = None
            self.MinScale = None
            self.MaxScale = None
            self.MaxHeightDelta = None
            self.PlacementHeightOffset = None
            self.PlacementHeightRandom = None
            self.Seed = None

    class RecordLookup:
        def __init__(self):
            self.Props = None
            self.SpawnSeed = None
            self.SpawnProbability = None
            self.MinScale = None
            self.MaxScale = None
            self.MinSpawnMaterialThickness = None
            self.MaxSpawnMaterialDepth = None
            self.MaxHeightDelta = None
            self.PlacementHeightOffset = None
            self.PlacementHeightRandom = None
            self.PropMaterialOverride = None
            self.BelowPropMaterial = None

class TerrainUnderPropsPostProcessor:
    def __init__(self):
        self.Name = ""
        self.Id = 0
        self.IsDisabled = False
        self.IsUnique = False
        self.IsImportable = False
        self.Is2D = False
        self.CanRotate = False
        self.ParallelizationStrategy = None
        self.SortingPriority = 0
        self.PassCount = 0
        self.LastGenerationTime = None
        self.Config = None
        self.ConfigMutable = None

    class Configuration:
        def __init__(self):
            self.SortingPriorityAdjustment = 0

class TreeWithWeight:
    def __init__(self):
        self.TreeProto = None
        self.Weight = 0
