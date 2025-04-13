
class CountableProductProto:
    ProductType = None
    Phantom = None
    def __init__(self):
        self.QuantityFormatter = None
        self.Graphics = None
        self.IconPath = ""
        self.Type = None
        self.CanBeLoadedOnTruck = False
        self.TrackSourceProducts = False
        self.IsMixable = False
        self.SlimId = None
        self.DoNotNormalize = False
        from Mafi import Option
        self.DumpableProduct = Option()
        self.SourceProduct = None
        from Mafi.Core.Products import ProductProto
        self.Id = ProductProto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.MaxQuantityPerTransportedProduct = None
        self.IsStorable = False
        self.Radioactivity = 0
        self.PinToHomeScreenByDefault = False
        self.IsExcludedFromStats = False
        self.CanBeDiscarded = False
        self.CanBeDestroyedInShipyard = False
        self.IsWaste = False
        self.IsRecyclable = False
        self.IsPhantom = False
        self.IsInitialized = False

    class Gfx:
        Empty = None
        def __init__(self):
            self.PackingMode = None
            from Mafi import Option
            self.PrefabsPath = Option()
            self.IconIsCustom = False
            self.StackingOffsets = None
            self.AllowPackingNoise = False
            self.RotateSecondItem90Degs = False
            self.Color = None
            self.TransportColor = None
            self.TransportAccentColor = None

class CountableProductStackingMode:
    Auto = None
    Stacked = None
    StackedAlternating = None
    Triangle = None
    TriangleHorizontal = None
    Row = None
    def __init__(self):
        self.value__ = 0

class DetailLayerSpecProto:
    MAX_DETAIL_DENSITY = 0.0
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
        self.PrefabPath = ""
        self.TexturePath = ""
        self.UniformRandomSpawnProbability = 0.0
        self.UniformRandomSpawnProbabilityFar = 0.0
        self.YOffsetWeightMult = 0.0
        self.WindSensitivity = 0.0
        self.UpNormalWeight = 0.0
        self.TintColorAndWeight = None
        self.Variants = None
        self.VariantsTotalWeight = 0
        self.RandomSeed = 0
        self.IsPhantom = False
        self.IsInitialized = False

    class DetailVariant:
        def __init__(self):
            self.SpawnWeight = 0
            self.BaseScale = 0.0
            self.PositionRandomness = 0.0
            self.ScaleVariation = 0.0
            self.UvOriginAndSizePacked = None
            self.PositionRandomnessOverrSbyteMaxMult = 0.0
            self.ScaleVariationOverShortMaxMult = 0.0

class FluidProductProto:
    ProductType = None
    Phantom = None
    def __init__(self):
        self.QuantityFormatter = None
        self.Graphics = None
        self.IconPath = ""
        self.Type = None
        self.CanBeLoadedOnTruck = False
        self.TrackSourceProducts = False
        self.IsMixable = False
        self.SlimId = None
        self.DoNotNormalize = False
        from Mafi import Option
        self.DumpableProduct = Option()
        self.SourceProduct = None
        from Mafi.Core.Products import ProductProto
        self.Id = ProductProto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.MaxQuantityPerTransportedProduct = None
        self.IsStorable = False
        self.Radioactivity = 0
        self.PinToHomeScreenByDefault = False
        self.IsExcludedFromStats = False
        self.CanBeDiscarded = False
        self.CanBeDestroyedInShipyard = False
        self.IsWaste = False
        self.IsRecyclable = False
        self.IsPhantom = False
        self.IsInitialized = False

class FluidProductProtoBuilder:
    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None

    class State:
        def __init__(self):
            self.Builder = None

class DestroyReason:
    Cleared = None
    DumpedOnTerrain = None
    General = None
    UsedAsFuel = None
    Wasted = None
    QuickTrade = None
    Export = None
    Construction = None
    Maintenance = None
    Settlement = None
    Research = None
    Farms = None
    Cheated = None
    LoanPayment = None
    def __init__(self):
        self.value__ = 0

class CreateReason:
    InitialResource = None
    Imported = None
    MinedFromTerrain = None
    Produced = None
    General = None
    Cheated = None
    QuickTrade = None
    Loot = None
    Deconstruction = None
    Settlement = None
    Recycled = None
    Research = None
    Loan = None
    def __init__(self):
        self.value__ = 0

class IProductsManager:
    def __init__(self):
        self.AssetManager = None
        self.ProductStats = None
        self.SlimIdManager = None

class IProductsManagerExtensions:
    def __init__(self):
        pass


class LooseProductProto:
    ProductType = None
    Phantom = None
    def __init__(self):
        from Mafi import Option
        self.TerrainMaterial = Option()
        self.CanBeOnTerrain = False
        self.LooseSlimId = None
        self.QuantityFormatter = None
        self.Graphics = None
        self.IconPath = ""
        self.Type = None
        self.CanBeLoadedOnTruck = False
        self.TrackSourceProducts = False
        self.IsMixable = False
        self.SlimId = None
        self.DoNotNormalize = False
        self.DumpableProduct = Option()
        self.SourceProduct = None
        from Mafi.Core.Products import ProductProto
        self.Id = ProductProto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.IsDumpedOnTerrainByDefault = False
        self.MaxQuantityPerTransportedProduct = None
        self.IsStorable = False
        self.Radioactivity = 0
        self.PinToHomeScreenByDefault = False
        self.IsExcludedFromStats = False
        self.CanBeDiscarded = False
        self.CanBeDestroyedInShipyard = False
        self.IsWaste = False
        self.IsRecyclable = False
        self.IsPhantom = False
        self.IsInitialized = False

    class Gfx:
        Empty = None
        def __init__(self):
            self.DisplayInResources = False
            from Mafi import Option
            self.PrefabsPath = Option()
            self.IconIsCustom = False
            self.PileMaterialAssetPath = ""
            self.UseRoughPileMeshes = False
            self.ResourcesVizColor = None
            self.Color = None
            self.TransportColor = None
            self.TransportAccentColor = None

class LooseProductSlimId:
    PhantomId = None
    def __init__(self):
        self.IsPhantom = False
        self.IsNotPhantom = False
        self.Value = None

class LooseProductsSlimIdManager:
    def __init__(self):
        self.PhantomProto = None
        self.MaxIdValue = 0
        self.SlimIdToLoose = None
        self.ManagedProtos = None

class MoltenProductProto:
    ProductType = None
    Phantom = None
    def __init__(self):
        self.QuantityFormatter = None
        self.Graphics = None
        self.IconPath = ""
        self.Type = None
        self.CanBeLoadedOnTruck = False
        self.TrackSourceProducts = False
        self.IsMixable = False
        self.SlimId = None
        self.DoNotNormalize = False
        from Mafi import Option
        self.DumpableProduct = Option()
        self.SourceProduct = None
        from Mafi.Core.Products import ProductProto
        self.Id = ProductProto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.MaxQuantityPerTransportedProduct = None
        self.IsStorable = False
        self.Radioactivity = 0
        self.PinToHomeScreenByDefault = False
        self.IsExcludedFromStats = False
        self.CanBeDiscarded = False
        self.CanBeDestroyedInShipyard = False
        self.IsWaste = False
        self.IsRecyclable = False
        self.IsPhantom = False
        self.IsInitialized = False

    class Gfx:
        Empty = None
        def __init__(self):
            from Mafi import Option
            self.PrefabsPath = Option()
            self.IconIsCustom = False
            self.MaterialPath = ""
            self.Color = None
            self.TransportColor = None
            self.TransportAccentColor = None

class MoltenProductProtoBuilder:
    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None

    class State:
        def __init__(self):
            self.Builder = None

class PinnedProductsManager:
    def __init__(self):
        self.AllPinnedProducts = None

class PinToggleCmd:
    def __init__(self):
        self.AffectsSaveState = False
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.IsVerificationCmd = False
        self.Result = False
        self.HasError = False
        self.ErrorMessage = ""
        from Mafi.Core.Products import ProductProto
        self.ProductToToggle = ProductProto.ID()


class PinnedProductReorderCmd:
    def __init__(self):
        self.AffectsSaveState = False
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.IsVerificationCmd = False
        self.Result = False
        self.HasError = False
        self.ErrorMessage = ""
        from Mafi.Core.Products import ProductProto
        self.ProductToMove = ProductProto.ID()

        self.NewIndex = 0

class ProductProto:
    Phantom = None
    def __init__(self):
        self.QuantityFormatter = None
        self.Graphics = None
        self.IconPath = ""
        self.Type = None
        self.CanBeLoadedOnTruck = False
        self.TrackSourceProducts = False
        self.IsMixable = False
        self.SlimId = None
        self.DoNotNormalize = False
        from Mafi import Option
        self.DumpableProduct = Option()
        self.SourceProduct = None
        from Mafi.Core.Products import ProductProto
        self.Id = ProductProto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.MaxQuantityPerTransportedProduct = None
        self.IsStorable = False
        self.Radioactivity = 0
        self.PinToHomeScreenByDefault = False
        self.IsExcludedFromStats = False
        self.CanBeDiscarded = False
        self.CanBeDestroyedInShipyard = False
        self.IsWaste = False
        self.IsRecyclable = False
        self.IsPhantom = False
        self.IsInitialized = False

    class ID:
        def __init__(self):
            self.Value = ""

    class Gfx:
        Empty = None
        def __init__(self):
            from Mafi import Option
            self.PrefabsPath = Option()
            self.IconIsCustom = False
            self.Color = None
            self.TransportColor = None
            self.TransportAccentColor = None

class ProductSlimId:
    PhantomId = None
    def __init__(self):
        self.IsPhantom = False
        self.IsNotPhantom = False
        self.Value = None

class ProductsSlimIdManager:
    def __init__(self):
        self.PhantomProto = None
        self.MaxIdValue = 0
        self.ManagedProtos = None

class ProductQuantityProto:
    def __init__(self):
        self.Product = None
        self.Quantity = None

class ProductsManager:
    RECYCLING_RATIO_BASE = None
    def __init__(self):
        self.AssetManager = None
        self.ProductStats = None
        self.SlimIdManager = None
        self.RecyclingRatio = None

class ApplyRecyclingRatioOnSourcesParam:
    def __init__(self):
        self.AllowedProtoType = None

class ProductStats:
    def __init__(self):
        self.UsedTotalStats = None
        self.UsedInDumpingStats = None
        self.UsedInConstructionStats = None
        self.UsedInMaintenanceStats = None
        self.UsedInSettlementStats = None
        self.UsedInResearchStats = None
        self.UsedInFarmsStats = None
        self.UsedInExportStats = None
        self.UsedAsFuelStats = None
        self.CreatedTotalStats = None
        self.CreatedByProduction = None
        self.CreatedByMiningStats = None
        self.CreatedByImportStats = None
        self.CreatedByDeconstructionStats = None
        self.CreatedByRecyclingStats = None
        self.CreatedByResearchStats = None
        self.CreatedBySettlementStats = None
        self.CreatedByQuickTradeLifetime = None
        self.GlobalQuantityStats = None
        self.StoredQuantityTotalStats = None
        self.StorageCapacity = None
        self.StoredAvailableQuantity = None
        self.StoredUnavailableQuantity = None
        self.StoredQuantityTotal = None
        self.GlobalQuantity = None
        self.SourceProducts = None
        self.Product = None
        self.ProductsManager = None

class ProductType:
    ANY = None
    NONE = None
    def __init__(self):
        pass


class TerrainMaterialProto:
    MINED_QUANTITY_PER_TILE_CUBED = None
    from Mafi.Core.Prototypes import Proto
    PHANTOM_ID = Proto.ID('__PHANTOM__TERRAIN_MATERIAL__')
    PhantomTerrainMaterialProto = None
    SUFFIX = ""
    def __init__(self):
        self.MinedQuantityPerTileCubed = None
        from Mafi import Option
        self.WeatheredMaterialProto = Option()
        self.DisruptedMaterialProto = Option()
        self.RecoveredMaterialProto = Option()
        self.SlimId = None
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.MinedProduct = None
        self.MinedQuantityMult = None
        self.WeatheredMaterialId = Proto.ID()

        self.DisruptedMaterialId = Proto.ID()

        self.RecoveredMaterialId = Proto.ID()

        self.RecoversUnderWater = False
        self.DisruptionRecoveryTime = None
        self.DisruptionSpeedMult = None
        self.DisruptWhenCollapsing = False
        self.MinCollapseHeightDiff = None
        self.MaxCollapseHeightDiff = None
        self.IsFarmable = False
        self.CanBeConvertedToForestFloor = False
        self.CanSpreadToNearbyMaterials = False
        self.IgnoreInEditor = False
        self.GrassGrowthOnTop = None
        self.Graphics = None
        self.IsForestFloor = False
        self.IsPhantom = False
        self.IsInitialized = False

    class Gfx:
        Empty = None
        def __init__(self):
            self.Color = None
            self.ParticleColor = None
            self.AlbedoHeightTexturePath = ""
            self.NormalSaoTexturePath = ""
            self.DetailLayers = None
            self.Dustiness = 0.0
            self.DustColor = None
            self.FullyWetSmoothnessDelta = 0.0
            self.FullyWetBrightnessDelta = 0.0

class DetailLayerSpec:
    def __init__(self):
        self.DetailLayerProto = None
        self.SpawnProbability = 0.0

class TerrainMaterialSlimId:
    PhantomId = None
    def __init__(self):
        self.IsPhantom = False
        self.IsNotPhantom = False
        self.Value = None

class TerrainMaterialSlimIdOption:
    None = None
    Phantom = None
    def __init__(self):
        self.Value = None
        self.HasValue = False
        self.IsNone = False

class TerrainMaterialsSlimIdManager:
    def __init__(self):
        self.PhantomProto = None
        self.MaxIdValue = 0
        self.ManagedProtos = None

class VirtualProductProto:
    ProductType = None
    Phantom = None
    def __init__(self):
        self.TrackSourceProducts = False
        self.QuantityFormatter = None
        self.Graphics = None
        self.IconPath = ""
        self.Type = None
        self.CanBeLoadedOnTruck = False
        self.IsMixable = False
        self.SlimId = None
        self.DoNotNormalize = False
        from Mafi import Option
        self.DumpableProduct = Option()
        self.SourceProduct = None
        from Mafi.Core.Products import ProductProto
        self.Id = ProductProto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.MaxQuantityPerTransportedProduct = None
        self.IsStorable = False
        self.Radioactivity = 0
        self.PinToHomeScreenByDefault = False
        self.IsExcludedFromStats = False
        self.CanBeDiscarded = False
        self.CanBeDestroyedInShipyard = False
        self.IsWaste = False
        self.IsRecyclable = False
        self.IsPhantom = False
        self.IsInitialized = False

class VirtualResourceProductProto:
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
        self.Product = None
        self.Graphics = None
        self.IsResourceFinal = False
        self.IsPhantom = False
        self.IsInitialized = False

    class Gfx:
        Empty = None
        def __init__(self):
            self.ResourcesVizColor = None
            self.ResourceBarsMaxHeight = None
