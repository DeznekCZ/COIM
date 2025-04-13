
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
        self.ErrorMessage = ""
        from Mafi.Core.Prototypes import Proto
        self.ProtoId = Proto.ID()

        self.Area = None
        self.Rotation = None
        self.IsReflected = False
        self.ColorKey = 0

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
        self.ErrorMessage = ""
        from Mafi.Core.Prototypes import Proto
        self.ProtoId = Proto.ID()

        self.Data = None

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
        self.ErrorMessage = ""
        from Mafi.Core.Prototypes import Proto
        self.ProtoId = Proto.ID()

        self.Data = None

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
        self.ErrorMessage = ""
        self.Data = None

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
        self.ErrorMessage = ""
        self.Data = None

class DesignationData:
    def __init__(self):
        self.PlusXTileCoord = None
        self.PlusYTileCoord = None
        self.PlusXyTileCoord = None
        self.CenterTileCoord = None
        self.WithinChunkRelCoord = None
        self.WithinChunkRelIndex = 0
        self.ChunkCoord = None
        self.OriginTile = None
        self.OriginTargetHeight = None
        self.PlusXTargetHeight = None
        self.PlusYTargetHeight = None
        self.PlusXyTargetHeight = None
        self.CenterTargetHeight = None

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
        self.ErrorMessage = ""
        self.Origins = None

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
        self.ErrorMessage = ""
        self.Area = None

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
        self.ErrorMessage = ""
        self.Origins = None
        self.Area = None

class SurfaceDesignation:
    Size = None
    SURFACE_HEIGHT_TOLERANCE = None
    SIZE_BITS = 0
    SIZE_MASK = 0
    SIZE_TILES = 0
    BITS_PER_CHUNK_EDGE = 0
    DESIGNATIONS_PER_CHUNK_EDGE = 0
    DESIGNATIONS_PER_CHUNK_EDGE_MASK = 0
    MASK_LOCAL_COORD = 0
    DESIGNATIONS_PER_CHUNK = 0
    AREA_TILES = 0
    def __init__(self):
        self.Name = None
        self.SizeTiles = 0
        self.IsPlacing = False
        from Mafi.Core.Prototypes import Proto
        self.ProtoId = Proto.ID()

        self.OriginTileCoord = None
        self.WithinChunkRelIndex = 0
        self.ChunkCoord = None
        self.CenterTileCoord = None
        self.Area = None
        self.AllFulfilled = None
        self.AllNonFulfilled = None
        self.IsDestroyed = False
        self.OriginTile = None
        self.NumberOfJobsAssigned = 0
        self.UnreachableVehiclesCount = None
        self.TilesFulfilledBitmap = None
        self.IsFulfilled = False
        self.IsNotFulfilled = False
        self.IsNotAssigned = False
        self.UnassignedTilesBitmap = None
        self.SurfaceTypeMap = None
        self.IsReadyToBeFulfilled = False
        self.Prototype = None
        self.Manager = None
        self.SurfaceProtoSlimId = None

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
        self.WithinChunkRelIndex = 0
        self.ChunkCoord = None
        self.OriginTile = None
        self.UnassignedTilesBitmap = None
        self.SurfaceProtoSlimId = None

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
        self.Count = 0
        self.TerrainManager = None

class TerrainDesignation:
    Size = None
    SURFACE_HEIGHT_TOLERANCE = None
    SIZE_BITS = 0
    SIZE_MASK = 0
    SIZE_TILES = 0
    BITS_PER_CHUNK_EDGE = 0
    DESIGNATIONS_PER_CHUNK_EDGE = 0
    DESIGNATIONS_PER_CHUNK_EDGE_MASK = 0
    MASK_LOCAL_COORD = 0
    DESIGNATIONS_PER_CHUNK = 0
    AREA_TILES_NO_OVERLAP = 0
    AREA_TILE_VERTICES = 0
    def __init__(self):
        self.Name = None
        self.SizeTiles = 0
        from Mafi.Core.Prototypes import Proto
        self.ProtoId = Proto.ID()

        from Mafi import Option
        self.Manager = Option()
        self.OriginTileCoord = None
        self.PlusXTileCoord = None
        self.PlusYTileCoord = None
        self.PlusXyTileCoord = None
        self.WithinChunkRelCoord = None
        self.WithinChunkRelIndex = 0
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
        self.NumberOfJobsAssigned = 0
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
        self.PlusXNeighbor = Option()
        self.PlusYNeighbor = Option()
        self.MinusXNeighbor = Option()
        self.MinusYNeighbor = Option()
        self.Prototype = None
        self.ManagedByTowers = None

    class AssignedTowers:
        def __init__(self):
            self.Count = 0

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
        self.Designations = None
        self.DesignationsDict = None
        self.Count = 0
        self.AllowDesignationsOverEntities = False
        self.TerrainManager = None
        self.TreesManager = None
        self.TerrainPropsManager = None
        self.OccupancyManager = None
        self.EntitiesManager = None

class TerrainDumpingManager:
    def __init__(self):
        self.DumpingDesignations = None
        self.Count = 0
        self.ProductsAllowedToDump = None
        self.AllDumpableProducts = None

class UnreachableTerrainDesignationsManager:
    def __init__(self):
        pass


class VehicleLastOutputBufferManager:
    def __init__(self):
        pass


class IDesignation:
    def __init__(self):
        self.Name = None
        self.SizeTiles = 0
        from Mafi.Core.Prototypes import Proto
        self.ProtoId = Proto.ID()

        self.IsFulfilled = False
        self.IsNotFulfilled = False
        self.IsReadyToBeFulfilled = False
        self.IsDestroyed = False
        self.OriginTileCoord = None
        self.CenterTileCoord = None
        self.UnreachableVehiclesCount = None

class DesignationType:
    Flat = None
    RampDown = None
    RampUp = None
    def __init__(self):
        self.value__ = 0

class DesignationDataFactory:
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
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsAvailable = False
        self.IsNotAvailable = False
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
        self.Product = None
        self.TotalToPlace = None
        self.ReservedToPlace = None
        self.TotalToClear = None
        self.ReservedToClear = None
        self.Placement = None
        self.Clearing = None

class SurfaceDesignationProto:
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
        self.IsPlacing = False
        self.IsPhantom = False
        self.IsInitialized = False

class TerrainDesignationProto:
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
        self.PreferInitialBelowTerrain = False
        self.IsFulfilledFn = None
        from Mafi import Option
        self.IsFulfilledMiningFn = Option()
        self.IsFulfilledDumpingFn = Option()
        self.CanOverflowReservations = False
        self.MaxAssignedEntities = 0
        self.DisplayWarningWhenNotOwned = False
        self.WarningWhenNotOwned = None
        self.ShouldUpdateTowerNotificationOnFulfilledChanged = False
        self.IsTerraforming = False
        self.Graphics = None
        self.IsPhantom = False
        self.IsInitialized = False

    class Gfx:
        Empty = None
        def __init__(self):
            self.ColorCanBeFulfilled = None
            self.ColorCanNotBeFulfilled = None
            self.ColorIsFulfilled = None
            self.ColorRemove = None

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
        self.Count = 0

class ITerrainMiningManager:
    def __init__(self):
        self.MiningDesignations = None
        self.MiningDesignationsCount = 0

class TerrainMiningManager:
    def __init__(self):
        self.MiningDesignations = None
        self.MiningDesignationsCount = 0

class IUnreachablesManager:
    def __init__(self):
        pass

