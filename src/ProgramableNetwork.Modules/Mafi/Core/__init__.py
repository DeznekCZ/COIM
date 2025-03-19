class FileSystemHelper:

    def __init__(self):
        self.GameDataRootDirPath = str(0)
        self.GameDataRootDirPathLegacy = str(0)
        self.WorkDirPath = str(0)
class IFileSystemHelper:

    def __init__(self):
        self.GameDataRootDirPath = str(0)
        self.GameDataRootDirPathLegacy = str(0)
        self.WorkDirPath = str(0)
class FileType:

    def __init__(self):
        pass

class SaveFileInfo:

    def __init__(self):
        pass

class SaveFileGroup:

    def __init__(self):
        pass

class FileSystemHelperExtensions:

    def __init__(self):
        pass

class ProductQuantityAssertionExtensions:

    def __init__(self):
        pass

class ProtoAssertionExtensions:

    def __init__(self):
        pass

class BuildInfo:
    Data = None

    def __init__(self):
        pass

class CoreMod:

    def __init__(self):
        self.Name = str(0)
        self.Version = int(0)
        self.IsUiOnly = False
        from Mafi import Option
        self.ModConfig = Option()
        self.Config = None
class CoreModConfig:

    def __init__(self):
        from Mafi import Option
        self.LoadedWorldMapName = Option()
        self.DisableTerrainPhysics = False
        self.DisableTerrainSurfaceSimulation = False
        self.DisablePathFinding = False
        self.DisableMultiThreadTerrainGeneration = False
        self.DisableBoundaryCellAutoUnlock = False
        self.DisableResourcesGeneration = False
        from Mafi import Option
        self.LoadedIslandMapName = Option()
        self.DisableLockedCellsTerrainGeneration = False
        self.ShouldUnlockAllProtosOnInit = False
        self.LogCommandsAsCSharp = False
        self.IsInstaBuildEnabled = False
        self.IsGodModeEnabled = False
        self.DisableSimulationBackgroundThread = False
        self.DeterminismValidationEnabled = False
        self.DeterminismValidationFrequencySteps = None
        self.DeterminismDisableCommandsForwarding = False
        self.DefenderExtraBattlePriority = int(0)
        self.MaxBattleRounds = int(0)
        self.StartingExtraFleetDistance = int(0)
        self.PossibleEscapeDistance = int(0)
        self.ShipEscapeHpThreshold = None
        self.BaseRoundsToEscape = int(0)
        self.ChanceForSameEntityRepeatedFire = None
        self.ChanceForDisabledEnemyFire = None
        self.ExtraMissChanceWhenEscaping = None
        self.MaxArmorReduction = None
        self.RecoverableHpMultiplier = None
        self.HullDamageMultWhenPartIsHit = None
        self.StartingPopulation = int(0)
        self.SaveTraceOnSimOvertime = False
        self.SaveTraceOnSimOvertimeMinDelay = None
        self.SaveTimingLogPeriod = None
        self.InitialVehiclesCap = int(0)
        self.AlwaysSunny = False
class GameTime:

    def __init__(self):
        from Mafi import Fix64
        self.TimeSinceStartMs = Fix64()
        from Mafi import Fix64
        self.TimeSinceLoadMs = Fix64()
        from Mafi import Fix64
        self.TotalElapsedSeconds = Fix64()
        self.SimStepsCount = int(0)
        self.SimStepsSinceLoad = int(0)
        from Mafi import Fix64
        self.TotalElapsedSimStepsSmooth = Fix64()
        self.DeltaSimStepsApprox = None
        from Mafi import Fix32
        self.TimeSinceLastSimUpdateMs = Fix32()
        self.DeltaTimeMs = float(0)
        self.FrameTimeSec = float(0)
        self.AbsoluteT = float(0)
        self.RelativeT = float(0)
        self.DeltaT = float(0)
        self.IsGamePaused = False
        self.GameSpeedMult = int(0)
        from Mafi import Fix32
        self.CurrSimUpdateDurationMs = Fix32()
class IGodModeConfig:

    def __init__(self):
        self.IsGodModeEnabled = False
class IdsCore:

    def __init__(self):
        pass

class Buildings:

    def __init__(self):
        pass

class TerrainDesignators:

    def __init__(self):
        pass

class Technology:

    def __init__(self):
        pass

class Products:

    def __init__(self):
        pass

class TerrainMaterials:

    def __init__(self):
        pass

class TerrainTileSurfaces:

    def __init__(self):
        pass

class Notifications:

    def __init__(self):
        pass

class PropertyIds:

    def __init__(self):
        pass

class UpointsStatsCategories:

    def __init__(self):
        pass

class UpointsCategories:

    def __init__(self):
        pass

class HealthPointsCategories:

    def __init__(self):
        pass

class BirthRateCategories:

    def __init__(self):
        pass

class PopNeeds:

    def __init__(self):
        pass

class World:

    def __init__(self):
        pass

class Transports:

    def __init__(self):
        pass

class Messages:

    def __init__(self):
        pass

class Weather:

    def __init__(self):
        pass

class ToolbarCategories:

    def __init__(self):
        pass

class TrCore:

    def __init__(self):
        pass

class MapsLoadingHelper:

    def __init__(self):
        pass

class NotificationId:

    def __init__(self):
        self.IsValid = False
class ThicknessIRange:

    def __init__(self):
        self.Height = None
class LooseProductQuantity:

    def __init__(self):
        self.ProductQuantity = None
        self.IsEmpty = False
        self.IsNotEmpty = False
class PartialProductQuantity:

    def __init__(self):
        self.IsEmpty = False
        self.IsNotEmpty = False
class ProductQuantity:

    def __init__(self):
        self.IsEmpty = False
        self.IsNotEmpty = False
class ProductQuantityLarge:

    def __init__(self):
        self.IsEmpty = False
        self.IsNotEmpty = False
class EntityId:

    def __init__(self):
        self.IsValid = False
        self.IsNotValid = False
class Factory:

    def __init__(self):
        pass

class EntityIdOption:
    None = None

    def __init__(self):
        self.HasValue = False
        self.ToNullable = None
class IoPortId:

    def __init__(self):
        self.IsValid = False
class Factory:

    def __init__(self):
        pass

class MessageNotificationId:

    def __init__(self):
        self.IsValid = False
class Factory:

    def __init__(self):
        pass

class VehicleJobId:

    def __init__(self):
        self.IsValid = False
class Factory:

    def __init__(self):
        pass

class ProtoBuilderException:

    def __init__(self):
        self.Message = str(0)
        self.Data = None
        self.InnerException = None
        self.TargetSite = None
        self.StackTrace = str(0)
        self.HelpLink = str(0)
        self.Source = str(0)
        self.HResult = int(0)
class ProtoDepAttribute:

    def __init__(self):
        self.TypeId = None
class RandomProvider:

    def __init__(self):
        pass

class ICoreRandom:

    def __init__(self):
        pass

class RandomGeneratorType:

    def __init__(self):
        pass

class RandomSeedConfig:

    def __init__(self):
        self.MasterRandomSeed = str(0)
class TileTransform:

    def __init__(self):
        self.Transform90RotFlip = None
        self.TransformMatrix = None
class Transform90RotFlip:

    def __init__(self):
        self.Rotation90 = None
        self.IsFlipped = False
class IInitializer:

    def __init__(self):
        self.IsBeingLoaded = False
class ITracingConfig:

    def __init__(self):
        self.SaveTraceOnSimOvertime = False
        self.SaveTraceOnSimOvertimeMinDelay = None
        self.SaveTimingLogPeriod = None
class TruckCaps:

    def __init__(self):
        pass

