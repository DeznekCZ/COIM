class AddSurfaceDecalCmd:

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
class AddSurfaceDesignationsCmd:

    def __init__(self):
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.AffectsSaveState = False
        self.IsVerificationCmd = False
        self.Result = None
        self.HasError = False
        self.ErrorMessage = str(0)
class AddTerrainDesignationsCmd:

    def __init__(self):
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.AffectsSaveState = False
        self.IsVerificationCmd = False
        self.Result = None
        self.HasError = False
        self.ErrorMessage = str(0)
class BatchAddSurfaceDecalCmd:

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
class BatchRemoveSurfaceDecalCmd:

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
class BatchRemoveSurfacePlacingDesignationsCmd:

    def __init__(self):
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.AffectsSaveState = False
        self.IsVerificationCmd = False
        self.Result = None
        self.HasError = False
        self.ErrorMessage = str(0)
class DesignationData:

    def __init__(self):
        self.PlusXTileCoord = None
        self.PlusYTileCoord = None
        self.PlusXyTileCoord = None
        self.CenterTileCoord = None
        self.WithinChunkRelCoord = None
        self.WithinChunkRelIndex = int(0)
        self.ChunkCoord = None
class PasteSurfaceDesignationsCmd:

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
class RemoveDesignationsCmd:

    def __init__(self):
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.AffectsSaveState = False
        self.IsVerificationCmd = False
        self.Result = None
        self.HasError = False
        self.ErrorMessage = str(0)
class RemoveSurfaceDecalCmd:

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
class RemoveSurfaceDesignationsCmd:

    def __init__(self):
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.AffectsSaveState = False
        self.IsVerificationCmd = False
        self.Result = None
        self.HasError = False
        self.ErrorMessage = str(0)
class SurfaceDesignation:
    Size = None
    SURFACE_HEIGHT_TOLERANCE = None
    SIZE_BITS = None
    SIZE_MASK = None
    SIZE_TILES = None
    BITS_PER_CHUNK_EDGE = None
    DESIGNATIONS_PER_CHUNK_EDGE = None
    DESIGNATIONS_PER_CHUNK_EDGE_MASK = None
    MASK_LOCAL_COORD = None
    DESIGNATIONS_PER_CHUNK = None
    AREA_TILES = None

    def __init__(self):
        self.Name = None
        self.SizeTiles = int(0)
        self.IsPlacing = False
        self.ProtoId = None
        self.OriginTileCoord = None
        self.WithinChunkRelIndex = int(0)
        self.ChunkCoord = None
        self.CenterTileCoord = None
        self.Area = None
        self.AllFulfilled = None
        self.AllNonFulfilled = None
        self.IsDestroyed = False
        self.OriginTile = None
        self.NumberOfJobsAssigned = int(0)
        self.UnreachableVehiclesCount = None
        self.TilesFulfilledBitmap = None
        self.IsFulfilled = False
        self.IsNotFulfilled = False
        self.IsNotAssigned = False
        self.UnassignedTilesBitmap = None
        self.SurfaceTypeMap = None
        self.IsReadyToBeFulfilled = False
        self.LogisticsZoneMask = None
class EnumeratorHolder:

    def __init__(self):
        pass

class Enumerator:

    def __init__(self):
        self.Current = None
class SurfaceDesignationData:

    def __init__(self):
        self.PlusXTileCoord = None
        self.PlusYTileCoord = None
        self.PlusXyTileCoord = None
        self.CenterTileCoord = None
        self.WithinChunkRelCoord = None
        self.WithinChunkRelIndex = int(0)
        self.ChunkCoord = None
class SurfaceDesignationsManager:

    def __init__(self):
        self.DesignationAdded = None
        self.DesignationUpdated = None
        self.DesignationFulfilledChanged = None
        self.DesignationRemoved = None
        self.PlacingDesignations = None
        self.ClearingDesignations = None
        self.PlacingDesignationsDict = None
        self.ClearingDesignationsDict = None
        self.Count = int(0)
        self.TerrainManager = None
class TerrainDesignation:
    Size = None
    SURFACE_HEIGHT_TOLERANCE = None
    SIZE_BITS = None
    SIZE_MASK = None
    SIZE_TILES = None
    BITS_PER_CHUNK_EDGE = None
    DESIGNATIONS_PER_CHUNK_EDGE = None
    DESIGNATIONS_PER_CHUNK_EDGE_MASK = None
    MASK_LOCAL_COORD = None
    DESIGNATIONS_PER_CHUNK = None
    AREA_TILES_NO_OVERLAP = None
    AREA_TILE_VERTICES = None

    def __init__(self):
        self.Name = None
        self.SizeTiles = int(0)
        self.ProtoId = None
        from Mafi import Option
        self.Manager = Option()
        self.LogisticsZoneMask = None
        self.OriginTileCoord = None
        self.PlusXTileCoord = None
        self.PlusYTileCoord = None
        self.PlusXyTileCoord = None
        self.WithinChunkRelCoord = None
        self.WithinChunkRelIndex = int(0)
        self.ChunkCoord = None
        self.CenterTileCoord = None
        self.Origin3i = None
        self.PlusX3i = None
        self.PlusY3i = None
        self.PlusXy3i = None
        self.Area = None
        self.IsDestroyed = False
        self.Data = None
        self.IsFlat = False
        self.RampDirection = None
        self.MinTargetHeight = None
        self.MaxTargetHeight = None
        self.NumberOfJobsAssigned = int(0)
        self.TilesFulfilledBitmap = None
        self.IsFulfilled = False
        self.IsNotFulfilled = False
        self.IsMiningFulfilled = False
        self.IsMiningNotFulfilled = False
        self.IsDumpingFulfilled = False
        self.IsDumpingNotFulfilled = False
        self.IsForestry = False
        self.IsReadyToBeFulfilled = False
        self.IsMiningReadyToBeFulfilled = False
        self.IsDumpingReadyToBeFulfilled = False
        self.UnreachableVehiclesCount = None
        from Mafi import Option
        self.PlusXNeighbor = Option()
        from Mafi import Option
        self.PlusYNeighbor = Option()
        from Mafi import Option
        self.MinusXNeighbor = Option()
        from Mafi import Option
        self.MinusYNeighbor = Option()
class AssignedTowers:

    def __init__(self):
        self.Count = int(0)
class Enumerator:

    def __init__(self):
        self.Current = None
class TerrainDesignationsManager:

    def __init__(self):
        self.DesignationAdded = None
        self.DesignationUpdated = None
        self.DesignationFulfilledChanged = None
        self.DesignationRemoved = None
        self.DesignationManagedTowersChanged = None
        self.DesignationReachabilityChanged = None
        from Mafi import Option
        self.GetFirstUnmanagedMineDesignation = Option()
        from Mafi import Option
        self.GetFirstUnmanagedForestryDesignation = Option()
        self.Designations = None
        self.DesignationsDict = None
        self.Count = int(0)
        self.AllowDesignationsOverEntities = False
        self.TerrainManager = None
        self.TreesManager = None
        self.TerrainPropsManager = None
        self.OccupancyManager = None
        self.EntitiesManager = None
class TerrainDumpingManager:

    def __init__(self):
        self.DumpingDesignations = None
        self.Count = int(0)
        self.ProductsAllowedToDump = None
        self.AllDumpableProducts = None
class UnreachableTerrainDesignationsManager:

    def __init__(self):
        self.VehiclesToClear = None
class VehicleLastOutputBufferManager:

    def __init__(self):
        pass

class IDesignation:

    def __init__(self):
        self.Name = None
        self.SizeTiles = int(0)
        self.ProtoId = None
        self.IsFulfilled = False
        self.IsNotFulfilled = False
        self.IsReadyToBeFulfilled = False
        self.IsDestroyed = False
        self.OriginTileCoord = None
        self.CenterTileCoord = None
        self.UnreachableVehiclesCount = None
        self.LogisticsZoneMask = None
class DesignationType:
    Flat = None
    RampDown = None
    RampUp = None

    def __init__(self):
        pass

class DesignationDataFactory:

    def __init__(self):
        pass

class DesignationZonesUpdater:

    def __init__(self):
        pass

class ITerrainDesignationBlockingEntityNoEdgeProto:

    def __init__(self):
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.IsUnique = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.Id = None
        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsInitialized = False
        self.Mod = None
class TerrainDesignationBlockingEntityNoEdgeProtoValidator:

    def __init__(self):
        self.Priority = None
class SurfaceDesignationDataFactory:

    def __init__(self):
        pass

class ISurfaceDesignationsManager:

    def __init__(self):
        self.PlacingDesignations = None
        self.ClearingDesignations = None
class ISurfaceDesignationsManagerInternal:

    def __init__(self):
        self.TerrainManager = None
        self.PlacingDesignations = None
        self.ClearingDesignations = None
class DesignationsPerProductCache:

    def __init__(self):
        self.LeftToPlace = None
        self.LeftToClear = None
class SurfaceDesignationProto:

    def __init__(self):
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
class TerrainDesignationProto:

    def __init__(self):
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
class Gfx:
    Empty = None

    def __init__(self):
        pass

class ITerrainDesignationsManager:

    def __init__(self):
        self.AllowDesignationsOverEntities = False
        self.TerrainManager = None
        self.TreesManager = None
        self.TerrainPropsManager = None
        self.OccupancyManager = None
        self.EntitiesManager = None
        self.DesignationAdded = None
        self.DesignationRemoved = None
        self.DesignationManagedTowersChanged = None
        self.DesignationFulfilledChanged = None
        self.Designations = None
class ITerrainDesignationsManagerInternal:

    def __init__(self):
        self.AllowDesignationsOverEntities = False
        self.TerrainManager = None
        self.TreesManager = None
        self.TerrainPropsManager = None
        self.OccupancyManager = None
        self.EntitiesManager = None
        self.DesignationAdded = None
        self.DesignationRemoved = None
        self.DesignationManagedTowersChanged = None
        self.DesignationFulfilledChanged = None
        self.Designations = None
class ITerrainDumpingManager:

    def __init__(self):
        self.AllDumpableProducts = None
        self.ProductsAllowedToDump = None
        self.DumpingDesignations = None
        self.Count = int(0)
class ITerrainMiningManager:

    def __init__(self):
        self.MiningDesignations = None
        self.MiningDesignationsCount = int(0)
class TerrainMiningManager:

    def __init__(self):
        self.MiningDesignations = None
        self.MiningDesignationsCount = int(0)
class IUnreachablesManager:

    def __init__(self):
        pass

