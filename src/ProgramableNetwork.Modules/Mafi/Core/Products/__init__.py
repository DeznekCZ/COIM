class CountableProductProto:

    def __init__(self):
        self.QuantityFormatter = None
        self.Graphics = None
        self.IconPath = str(0)
        self.Type = None
        self.CanBeLoadedOnTruck = False
        self.TrackSourceProducts = False
        self.IsMixable = False
        self.SlimId = None
        self.DoNotNormalize = False
        from Mafi import Option
        self.DumpableProduct = Option()
        self.SourceProduct = None
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class Gfx:

    def __init__(self):
        self.PackingMode = None
        from Mafi import Option
        self.PrefabsPath = Option()
        self.IconIsCustom = False
class CountableProductStackingMode:

    def __init__(self):
        pass

class DetailLayerSpecProto:

    def __init__(self):
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class DetailVariant:

    def __init__(self):
        pass

class FluidProductProto:

    def __init__(self):
        self.QuantityFormatter = None
        self.Graphics = None
        self.IconPath = str(0)
        self.Type = None
        self.CanBeLoadedOnTruck = False
        self.TrackSourceProducts = False
        self.IsMixable = False
        self.SlimId = None
        self.DoNotNormalize = False
        from Mafi import Option
        self.DumpableProduct = Option()
        self.SourceProduct = None
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class FluidProductProtoBuilder:

    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None
class State:

    def __init__(self):
        pass

class DestroyReason:

    def __init__(self):
        pass

class CreateReason:

    def __init__(self):
        pass

class IProductsManager:

    def __init__(self):
        self.AssetManager = None
        self.ProductStats = None
        self.SlimIdManager = None
class IProductsManagerExtensions:

    def __init__(self):
        pass

class LooseProductProto:

    def __init__(self):
        from Mafi import Option
        self.TerrainMaterial = Option()
        self.CanBeOnTerrain = False
        self.LooseSlimId = None
        self.QuantityFormatter = None
        self.Graphics = None
        self.IconPath = str(0)
        self.Type = None
        self.CanBeLoadedOnTruck = False
        self.TrackSourceProducts = False
        self.IsMixable = False
        self.SlimId = None
        self.DoNotNormalize = False
        from Mafi import Option
        self.DumpableProduct = Option()
        self.SourceProduct = None
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class Gfx:

    def __init__(self):
        self.DisplayInResources = False
        from Mafi import Option
        self.PrefabsPath = Option()
        self.IconIsCustom = False
class LooseProductSlimId:
    PhantomId = None

    def __init__(self):
        self.IsPhantom = False
        self.IsNotPhantom = False
class LooseProductsSlimIdManager:

    def __init__(self):
        self.PhantomProto = None
        self.MaxIdValue = int(0)
class MoltenProductProto:

    def __init__(self):
        self.QuantityFormatter = None
        self.Graphics = None
        self.IconPath = str(0)
        self.Type = None
        self.CanBeLoadedOnTruck = False
        self.TrackSourceProducts = False
        self.IsMixable = False
        self.SlimId = None
        self.DoNotNormalize = False
        from Mafi import Option
        self.DumpableProduct = Option()
        self.SourceProduct = None
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class Gfx:

    def __init__(self):
        from Mafi import Option
        self.PrefabsPath = Option()
        self.IconIsCustom = False
class MoltenProductProtoBuilder:

    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None
class State:

    def __init__(self):
        pass

class PinnedProductsManager:

    def __init__(self):
        pass

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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
class ProductProto:

    def __init__(self):
        self.QuantityFormatter = None
        self.Graphics = None
        self.IconPath = str(0)
        self.Type = None
        self.CanBeLoadedOnTruck = False
        self.TrackSourceProducts = False
        self.IsMixable = False
        self.SlimId = None
        self.DoNotNormalize = False
        from Mafi import Option
        self.DumpableProduct = Option()
        self.SourceProduct = None
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class ID:

    def __init__(self):
        pass

class Gfx:

    def __init__(self):
        from Mafi import Option
        self.PrefabsPath = Option()
        self.IconIsCustom = False
class ProductSlimId:
    PhantomId = None

    def __init__(self):
        self.IsPhantom = False
        self.IsNotPhantom = False
class ProductsSlimIdManager:

    def __init__(self):
        self.PhantomProto = None
        self.MaxIdValue = int(0)
class ProductQuantityProto:

    def __init__(self):
        pass

class ProductsManager:

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
class ProductType:

    def __init__(self):
        pass

class TerrainMaterialProto:

    def __init__(self):
        self.MinedQuantityPerTileCubed = None
        from Mafi import Option
        self.WeatheredMaterialProto = Option()
        from Mafi import Option
        self.DisruptedMaterialProto = Option()
        from Mafi import Option
        self.RecoveredMaterialProto = Option()
        self.SlimId = None
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class Gfx:

    def __init__(self):
        pass

class DetailLayerSpec:

    def __init__(self):
        pass

class TerrainMaterialSlimId:
    PhantomId = None

    def __init__(self):
        self.IsPhantom = False
        self.IsNotPhantom = False
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
        self.MaxIdValue = int(0)
class VirtualProductProto:

    def __init__(self):
        self.TrackSourceProducts = False
        self.QuantityFormatter = None
        self.Graphics = None
        self.IconPath = str(0)
        self.Type = None
        self.CanBeLoadedOnTruck = False
        self.IsMixable = False
        self.SlimId = None
        self.DoNotNormalize = False
        from Mafi import Option
        self.DumpableProduct = Option()
        self.SourceProduct = None
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class VirtualResourceProductProto:

    def __init__(self):
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class Gfx:

    def __init__(self):
        pass

