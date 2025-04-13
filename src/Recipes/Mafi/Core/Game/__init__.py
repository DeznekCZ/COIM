
class IDeterminismValidatorConfig:
    def __init__(self):
        self.DeterminismValidationEnabled = False
        self.DeterminismValidationFrequencySteps = None
        self.DeterminismDisableCommandsForwarding = False

class StartNewGameArgs:
    def __init__(self):
        self.Mods = None
        self.Configs = None
        self.FsHelper = None

class LoadGameArgs:
    def __init__(self):
        self.Stream = None
        self.FsHelper = None
        self.AlreadyReadHeader = None

class LoadGameArgsFromFile:
    def __init__(self):
        self.Save = None
        self.FsHelper = None
        self.ThirdPartyModsToAdd = None

class LoadGameArgsFromMapFile:
    def __init__(self):
        self.SaveNameOrPath = ""
        self.FsHelper = None
        self.ExtraModsToAdd = None

class GameBuilderException:
    def __init__(self):
        self.Message = ""
        self.Data = None
        self.InnerException = None
        self.TargetSite = None
        self.StackTrace = ""
        self.HelpLink = ""
        self.Source = ""
        self.HResult = 0

class SpecialSerializerFactories:
    def __init__(self):
        self.SpecialSerializersForConfigs = None
        self.SpecialSerializersForGame = None

class GameDifficultyApplier:
    DifficultyChangeTimeout = None
    def __init__(self):
        self.CurrentDate = None
        self.OnDifficultyChanged = None
        self.DifficultyConfig = None
        self.ChangeLog = None

class ChangeGameDifficultyCmd:
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
        self.NewDifficulty = None

class GameDifficultyChangeLog:
    def __init__(self):
        self.AllChangeSets = None

    class ChangeSet:
        def __init__(self):
            self.DateChanged = None
            self.Changes = None

class GameDifficultyOptionChange:
    def __init__(self):
        self.ValueMemberName = ""
        self.OldValue = None
        self.NewValue = None
        self.IsNewEasier = False

class GameDifficultyPreset:
    Easy = None
    Normal = None
    Hard = None
    def __init__(self):
        self.value__ = 0

class PropDifficultyRating:
    Easy = None
    Standard = None
    Hard = None
    def __init__(self):
        self.value__ = 0

class GameDifficultyConfig:
    AllPresets = None
    ExtraStartingMaterialDefault = None
    WorldMinesReservesDefault = None
    DeconstructionRefundDefault = None
    EASY_MECHANICS = None
    HARD_MECHANICS = None
    GameDifficulty__CustomTitle = None
    GameDifficulty__EasyTitle = None
    GameDifficulty__EasyDescription = None
    GameDifficulty__EasyExplanation = None
    GameDifficulty__NormalTitle = None
    GameDifficulty__NormalDescription = None
    GameDifficulty__NormalExplanation = None
    GameDifficulty__AdmiralTitle = None
    GameDifficulty__AdmiralDescription = None
    GameDifficulty__AdmiralExplanation = None
    ExtraStartingMaterialInfo = None
    MaintenanceDiffInfo = None
    FuelConsumptionDiffInfo = None
    RainYieldDiffInfo = None
    BaseHealthDiffInfo = None
    ResourceMiningDiffInfo = None
    SettlementConsumptionDiffInfo = None
    WorldMinesReservesInfo = None
    FarmYieldInfo = None
    UnityProductionDiffInfo = None
    ConstructionCostsDiffInfo = None
    QuickRepairInfo = None
    WeatherDifficultyInfo = None
    PowerSettingInfo = None
    TreesGrowthInfo = None
    ExtraContractsProfitInfo = None
    DeconstructionRefundInfo = None
    LoansDifficultyInfo = None
    ShipsNoFuelInfo = None
    GroundwaterPumpLowInfo = None
    ResearchCostDiffInfo = None
    StarvationInfo = None
    WorldMinesNoUnityInfo = None
    VehiclesNoFuelInfo = None
    ConsumerBrokenInfo = None
    PowerLowInfo = None
    ComputingLowInfo = None
    QuickActionsCostInfo = None
    DiseaseMortalityDiffInfo = None
    OreSortingInfo = None
    SolarPowerDiffInfo = None
    PollutionDiffInfo = None
    AllOptions = None
    def __init__(self):
        self.ExtraStartingMaterial = None
        self.ExtraStartingMaterialMult = None
        self.MaintenanceDiff = None
        self.FuelConsumptionDiff = None
        self.RainYieldDiff = None
        self.BaseHealthDiff = None
        self.ResourceMiningDiff = None
        self.SettlementConsumptionDiff = None
        self.WorldMinesReservesDiff = None
        self.WorldMinesUnlimited = False
        self.FarmsYieldDiff = None
        self.UnityProductionDiff = None
        self.ConstructionCostsDiff = None
        self.CanQuickRepair = False
        self.QuickRepair = None
        self.WeatherDifficulty = None
        self.PowerSetting = None
        self.TreesGrowthDiff = None
        self.ExtraContractsProfit = None
        self.DeconstructionRefund = None
        self.LoansDifficulty = None
        self.ShipsNoFuel = None
        self.GroundwaterPumpLow = None
        self.ResearchCostDiff = None
        self.Starvation = None
        self.WorldMinesNoUnity = None
        self.VehiclesNoFuel = None
        self.ConsumerBroken = None
        self.PowerLow = None
        self.ComputingLow = None
        self.QuickActionsCostDiff = None
        self.DiseaseMortalityDiff = None
        self.OreSorting = None
        self.SolarPowerDiff = None
        self.PollutionDiff = None
        self.Name = None
        self.Description = None
        self.Explanation = None
        self.SelectedMechanics = None
        self.OriginalPreset = None

    class QuickRepairSetting:
        Enabled = None
        Disabled = None
        def __init__(self):
            self.value__ = 0

    class WeatherDifficultySetting:
        Easy = None
        Standard = None
        Dry = None
        def __init__(self):
            self.value__ = 0

    class LogisticsPowerSetting:
        DoNotConsume = None
        ConsumeIfPossible = None
        ConsumeAlways = None
        def __init__(self):
            self.value__ = 0

    class DeconstructionRefundSetting:
        Full = None
        Partial = None
        def __init__(self):
            self.value__ = 0

    class ShipNoFuelSetting:
        RunOnUnity = None
        StopWorking = None
        def __init__(self):
            self.value__ = 0

    class GroundwaterPumpLowSetting:
        SlowDown = None
        StopWorking = None
        def __init__(self):
            self.value__ = 0

    class StarvationSetting:
        ReducedWorkforce = None
        Death = None
        def __init__(self):
            self.value__ = 0

    class WorldMinesNoUnitySetting:
        SlowDown = None
        Stop = None
        def __init__(self):
            self.value__ = 0

    class VehiclesNoFuelSetting:
        SlowDown = None
        Stop = None
        def __init__(self):
            self.value__ = 0

    class ConsumerBrokenSetting:
        SlowDown = None
        Stop = None
        def __init__(self):
            self.value__ = 0

    class PowerLowSetting:
        SlowDown = None
        Stop = None
        def __init__(self):
            self.value__ = 0

    class ComputingLowSetting:
        SlowDown = None
        Stop = None
        def __init__(self):
            self.value__ = 0

    class OreSortingSetting:
        Disabled = None
        Enabled = None
        def __init__(self):
            self.value__ = 0

class IDiffSettingInfo:
    def __init__(self):
        self.ValueMemberName = ""
        self.Property = None
        self.Title = None

class PercentSettingInfo:
    def __init__(self):
        self.ValueMemberName = ""
        self.Title = None
        self.Property = None
        self.Tooltip = None
        self.Options = None

class GameMechanics:
    GameMechanic__Casual = None
    GameMechanic__Realism = None
    GameMechanic__Challenges = None
    Casual = None
    ResourcesBoost = None
    OreSorting = None
    Realism = None
    RealismPlus = None
    ReducedWorldMines = None
    def __init__(self):
        pass


class GameMechanicApplier:
    def __init__(self):
        self.Title = None
        self.Items = None
        self.Dependencies = None
        self.Conflicts = None
        self.IconPath = ""
        self.RecommendedFor = None

class GameNameConfig:
    def __init__(self):
        self.LoadedFile = None
        self.GameName = ""

class GameStartArgs:
    Empty = None
    def __init__(self):
        from Mafi import Option
        self.SaveName = Option()

class IConfig:
    def __init__(self):
        pass


class IConfigExtensions:
    def __init__(self):
        pass

