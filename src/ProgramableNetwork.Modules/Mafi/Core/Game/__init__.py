class IDeterminismValidatorConfig:

    def __init__(self):
        self.DeterminismValidationEnabled = False
        self.DeterminismValidationFrequencySteps = None
        self.DeterminismDisableCommandsForwarding = False
class StartNewGameArgs:

    def __init__(self):
        pass

class LoadGameArgs:

    def __init__(self):
        pass

class LoadGameArgsFromFile:

    def __init__(self):
        pass

class LoadGameArgsFromMapFile:

    def __init__(self):
        pass

class GameBuilderException:

    def __init__(self):
        self.Message = str(0)
        self.Data = None
        self.InnerException = None
        self.TargetSite = None
        self.StackTrace = str(0)
        self.HelpLink = str(0)
        self.Source = str(0)
        self.HResult = int(0)
class SpecialSerializerFactories:

    def __init__(self):
        pass

class GameDifficultyApplier:

    def __init__(self):
        self.CurrentDate = None
        self.OnDifficultyChanged = None
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
        self.ErrorMessage = str(0)
class GameDifficultyChangeLog:

    def __init__(self):
        self.AllChangeSets = None
class ChangeSet:

    def __init__(self):
        pass

class GameDifficultyOptionChange:

    def __init__(self):
        pass

class GameDifficultyPreset:

    def __init__(self):
        pass

class PropDifficultyRating:

    def __init__(self):
        pass

class GameDifficultyConfig:
    AllPresets = None
    ExtraStartingMaterialDefault = None
    WorldMinesReservesDefault = None
    DeconstructionRefundDefault = None

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
class QuickRepairSetting:

    def __init__(self):
        pass

class WeatherDifficultySetting:

    def __init__(self):
        pass

class LogisticsPowerSetting:

    def __init__(self):
        pass

class DeconstructionRefundSetting:

    def __init__(self):
        pass

class ShipNoFuelSetting:

    def __init__(self):
        pass

class GroundwaterPumpLowSetting:

    def __init__(self):
        pass

class StarvationSetting:

    def __init__(self):
        pass

class WorldMinesNoUnitySetting:

    def __init__(self):
        pass

class VehiclesNoFuelSetting:

    def __init__(self):
        pass

class ConsumerBrokenSetting:

    def __init__(self):
        pass

class PowerLowSetting:

    def __init__(self):
        pass

class ComputingLowSetting:

    def __init__(self):
        pass

class OreSortingSetting:

    def __init__(self):
        pass

class IDiffSettingInfo:

    def __init__(self):
        self.ValueMemberName = str(0)
        self.Property = None
        self.Title = None
class PercentSettingInfo:

    def __init__(self):
        self.ValueMemberName = str(0)
        self.Title = None
        self.Property = None
class GameMechanics:

    def __init__(self):
        pass

class GameMechanicApplier:

    def __init__(self):
        self.Title = None
        self.Items = None
        self.Dependencies = None
        self.Conflicts = None
        self.IconPath = str(0)
class GameNameConfig:

    def __init__(self):
        self.LoadedFile = None
        self.GameName = str(0)
class GameStartArgs:

    def __init__(self):
        pass

class IConfig:

    def __init__(self):
        pass

class IConfigExtensions:

    def __init__(self):
        pass

