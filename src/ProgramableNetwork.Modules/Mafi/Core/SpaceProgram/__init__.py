class AddLaunchPadCargoBufferCmd:

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
class Asteroid:

    def __init__(self):
        self.Radius = None
        self.TrueRadius = None
        self.TotalVolumeTiles3 = int(0)
        self.TotalQuantity = None
        self.MineableProducts = None
        self.DiscoveryFinishedSteps = None
        self.PercentDiscovered = None
        self.IsBeingDiscovered = False
        self.IsDiscovered = False
        self.ToOrbitFinishedSteps = None
        self.PercentTravelled = None
        self.IsTravellingToOrbit = False
        self.ReachedOrbit = False
        self.IsDropped = False
        self.DroppedAt = None
class AsteroidsManager:
    DISCOVERED_ASTEROIDS_CAPACITY_BASE = None
    DISCOVERED_ASTEROIDS_CAPACITY_PER_STATION_LEVEL = None
    ASTEROID_DROP_STABILIZATION_DURATION = None
    SPACE_PROBE_PARTS_PER_ASTEROID = None
    BOOSTERS_PER_ROCKET = None
    TOTAL_TRAVEL_TIME_TO_ORBIT = None
    TOTAL_DISCOVERY_TIME = None
    PURE_ASTEROID_PROBABILITY = None
    PURE_ASTEROID_QUANTITY_MULT = None
    TWO_PURE_MATERIALS_ASTEROID_PROBABILITY = None
    TWO_PURE_MATERIALS_ASTEROID_QUANTITY_MULT = None
    TWO_PURE_MATERIALS_ASTEROID_MAX_RATIO = None
    FILLER_AND_MATERIAL_ASTEROID_PROBABILITY = None
    FILLER_AND_MATERIAL_ASTEROID_QUANTITY_MULT = None
    MATERIAL_TO_FILLER_ASTEROID_QUANTITY_MAX_RATIO = None
    FILLER_ONLY_ASTEROID_QUANTITY_MULT = None
    ASTEROID_VOLUME_MIN = None
    ASTEROID_VOLUME_MAX = None
    ASTEROID_VOLUME_MEAN_MIN = None
    ASTEROID_VOLUME_MEAN_MAX = None
    ASTEROID_VOLUME_STD_MIN = None
    ASTEROID_VOLUME_STD_MAX = None
    ASTEROID_VOLUME_STD_MAX_DEVIATION = None
    ASTEROID_SIZE_PROGRESSION_PER_FIND = None
    ASTEROID_SIZE_PROGRESSION_PER_DROP = None

    def __init__(self):
        from Mafi import Option
        self.AsteroidBeingDropped = Option()
        self.DroppedAsteroidPosition = None
        self.AsteroidDropDurationLeft = None
        self.AsteroidsActive = None
        self.AsteroidsBeingDiscovered = None
        self.AsteroidsTravellingToOrbit = None
        self.AsteroidsDropped = None
        self.AsteroidMaterials = None
        self.DiscoveredAsteroidsLimit = int(0)
class BringAsteroidToOrbitCmd:

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
class BuildOrUpgradeSpaceStationCmd:

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
class CancelBuildOrUpgradeOfSpaceStationCmd:

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
class DiscardAsteroidCmd:

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
class DowngradeSpaceStationCmd:

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
class DropAsteroidAtCmd:

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
class FakeRocketLaunchManager:

    def __init__(self):
        self.LaunchesCount = int(0)
        self.LaunchExp = int(0)
        self.RocketLaunched = None
class LaunchRocketCmd:

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
class OrbitManager:
    MAX_ORBIT_CARGO_LIMIT = None

    def __init__(self):
        from Mafi import Option
        self.SpaceStation = Option()
        self.OnStationChanged = None
        self.IsStationConstructionPending = False
        self.Buffers = None
        self.HighestStationTierAchieved = int(0)
class RemoveLaunchPadCargoBufferCmd:

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
class ReorderLaunchPadCargoBufferCmd:

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
class RocketEntity:

    def __init__(self):
        self.Prototype = None
        self.CanBePaused = False
        self.Position = None
        self.Cargo = None
        self.FuelBuffer = None
        self.FuelBufferSecondary = None
        self.HasFullFuel = False
        self.LaunchedFor = None
        self.Acceleration = None
        self.GainedAltitude = None
        self.IsLaunched = False
        self.IsExploded = None
        self.IsSoundOn = False
        self.SoundParams = None
        self.FlightProgress = None
        from Mafi import Option
        self.Owner = Option()
        self.Id = None
        self.DefaultTitle = None
        self.Context = None
        self.IsDestroyed = False
        self.IsEnabled = False
        self.IsNotEnabled = False
        self.IsPaused = False
        self.IsNotPaused = False
        self.RendererData = None
class RocketEntityBase:

    def __init__(self):
        from Mafi import Option
        self.Owner = Option()
        self.Id = None
        self.DefaultTitle = None
        self.Prototype = None
        self.Context = None
        self.IsDestroyed = False
        self.CanBePaused = False
        self.IsEnabled = False
        self.IsNotEnabled = False
        self.IsPaused = False
        self.IsNotPaused = False
        self.RendererData = None
class RocketLaunchManager:

    def __init__(self):
        self.LaunchExp = int(0)
        self.LaunchesSuccessesCount = int(0)
        self.LaunchesFailuresCount = int(0)
        self.LaunchesCount = int(0)
        self.RocketLaunched = None
        self.RocketsInFlight = None
class ScanForNewAsteroidCmd:

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
class SetOrbitCargoLimitCmd:

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
class SetRocketAutoLaunchCmd:

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
class SpaceStation:
    CRITICALLY_LOW_MAINTENANCE = None

    def __init__(self):
        self.OperatesAt = None
        self.UnityGeneratedLastTick = None
        self.ResearchEfficiencyBonusLastTick = None
        self.MaintenancePartsBuffer = None
        self.MaintenanceLevel = None
        self.MaintenanceBuffer = None
        self.MaintenanceBufferCapacity = None
        self.DegradesAt = None
        self.CrewAssigned = int(0)
        self.IsCrewDisabled = False
        self.NextCrewRefresh = None
        from Mafi import Option
        self.CrewSuppliesBuffer = Option()
        self.CrewSuppliesNeededPerMonth = None
        self.Data = None
        self.CurrentTier = int(0)
        self.CrewCapacity = int(0)
        self.CrewCapacityLeft = int(0)
        self.ResearchPointsProto = None
        from Mafi import Fix64
        self.ResearchPointsStored = Fix64()
        self.ResearchPointsCapacity = int(0)
        from Mafi import Fix64
        self.CurrentResearchPointsGenerationPerMonth = Fix64()
        from Mafi import Option
        self.ResearchSuppliesBuffer = Option()
        self.OngoingUpgradeCost = None
        self.UpgradePartsDelivered = None
class State:
    Working = None
    NoCrewSupplies = None
    NoMaintenanceParts = None
    MaintenanceCriticallyLow = None
    CrewLow = None
    NoResearchSupplies = None

    def __init__(self):
        pass

class ToggleRocketCountdownMuteCmd:

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
class ToggleSpaceStationCrewEnabledCmd:

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
class IAsteroidFriend:

    def __init__(self):
        pass

class AsteroidTerrainGenerator:
    ASTEROID_INFLATE_RADIUS_PERC = None

    def __init__(self):
        pass

class IRocketOwner:

    def __init__(self):
        from Mafi import Option
        self.AttachedRocketBase = Option()
        self.RendererData = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
class IRocketOwnerExtensions:

    def __init__(self):
        pass

class IRocketLaunchManager:

    def __init__(self):
        self.LaunchesCount = int(0)
        self.LaunchExp = int(0)
        self.RocketLaunched = None
class RocketProto:

    def __init__(self):
        self.EntityType = None
        self.CargoCapacity = None
        self.CrewCapacity = int(0)
        self.Costs = None
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

    def __init__(self):
        pass

class SpaceStationProto:
    CREW_ROTATION_DURATION = None
    CREW_ROTATION_REQUEST_TIME = None
    DEGRADES_AT = None
    MAINTENANCE_PARTS_BUFFER_RESERVE = None
    MAINTENANCE_LEVEL_LASTS_FOR = None
    CREW_SUPPLIES_BUFFER_RESERVE = None
    RESEARCH_SUPPLIES_BUFFER_RESERVE = None
    RESEARCH_POINTS_BUFFER_CAPACITY = None
    MIN_MAINTENANCE_PARTS_BUFFER_CAP = None
    ADVANCED_PARTS_TIER_FROM = None
    RESEARCH_TIER_FROM = None
    CREW_REQUIRED_FROM = None
    ASTEROIDS_SUPPORT_FROM = None

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
class StationTierData:

    def __init__(self):
        pass

class TransportedRocketBaseProto:

    def __init__(self):
        self.EntityType = None
        self.Costs = None
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
