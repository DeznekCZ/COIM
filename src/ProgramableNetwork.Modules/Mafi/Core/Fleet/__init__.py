class BattleFleet:

    def __init__(self):
        self.IsInBattle = False
        self.IsNotInBattle = False
        self.Entities = None
class BattleResult:

    def __init__(self):
        pass

class PlayersBattleResult:

    def __init__(self):
        pass

class FleetBattleResultStats:

    def __init__(self):
        pass

class IBattleSimulator:

    def __init__(self):
        from Mafi import Option
        self.OngoingBattle = Option()
        self.AllBattleResults = None
class BattleSimulator:

    def __init__(self):
        from Mafi import Option
        self.OngoingBattle = Option()
        from Mafi import Option
        self.LatestBattle = Option()
        self.AllBattleResults = None
class NotifyOnBattleOpenedCmd:

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
class IBattleState:

    def __init__(self):
        self.IsStarted = False
        self.Attacker = None
        self.Defender = None
        self.BattleRound = int(0)
        from Mafi import Option
        self.Result = Option()
        self.BattleLog = None
        self.Data = None
class BattleState:

    def __init__(self):
        self.IsStarted = False
        self.IsFinished = False
        self.Attacker = None
        self.Defender = None
        from Mafi import Option
        self.Result = Option()
        self.BattleRound = int(0)
        self.Data = None
        self.BattleSortedEntities = None
        self.BattleLog = None
        self.BattleSimConfig = None
class BattleStateData:

    def __init__(self):
        self.Attacker = None
        self.Defender = None
class FleetBattleData:

    def __init__(self):
        self.Entities = None
class FleetEntityBattleData:

    def __init__(self):
        self.Name = None
        self.Hull = None
        self.Armor = int(0)
        self.CurrentHp = int(0)
        self.MaxHp = int(0)
        self.IsDestroyed = False
        self.IsEscaping = False
        self.HasEscaped = False
        self.Weapons = None
class FleetWeaponBattleData:

    def __init__(self):
        self.Proto = None
        self.ShotsFired = int(0)
        self.DamageDealt = int(0)
        self.Range = int(0)
        self.MaxHp = int(0)
        self.CurrentHp = int(0)
class DestructiblePartBattleData:

    def __init__(self):
        self.Proto = None
        self.MaxHp = int(0)
        self.CurrentHp = int(0)
class IBattleAware:

    def __init__(self):
        pass

class DestructibleFleetPartProto:

    def __init__(self):
        self.IconPath = str(0)
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
class DestructibleFleetPart:

    def __init__(self):
        self.RecoverableHp = int(0)
        self.CurrentHp = int(0)
        self.IsInBattle = False
        self.DamageTakenDuringCurrentBattle = int(0)
        self.ArmorDamageReductionDuringCurrentBattle = int(0)
        self.HpPercent = None
        self.RecoverableHpPercent = None
        self.IsDestroyed = False
        self.IsNotDestroyed = False
class FleetBridgePartProto:

    def __init__(self):
        self.IconPath = str(0)
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

class FleetEnginePartProto:

    def __init__(self):
        self.IconPath = str(0)
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
class FleetEntity:
    RepairCostForValue = None
    RepairCostForValueDestroyedPart = None
    DistancePerStepInBattle = None

    def __init__(self):
        self.Name = None
        from Mafi import Option
        self.EntityName = Option()
        self.BattlePriority = None
        self.HitChanceWeight = None
        self.ExtraRoundsToEscape = None
        self.FuelTankCapacity = None
        self.MinCrewNeeded = int(0)
        self.Hull = None
        self.OnFuelCapacityChange = None
        self.Slots = None
        self.Weapons = None
        self.DestructibleParts = None
        self.Fleet = None
        self.IsInBattle = False
        self.IsDestroyed = False
        self.IsNotDestroyed = False
        self.HasNoUsableWeapon = False
        self.CanEscape = False
        self.IsEscaping = False
        self.RoundsToEscape = int(0)
        self.HasEscaped = False
        self.HasNotEscaped = False
        self.IsFacingLeft = False
        self.CanContinueBattle = False
        from Mafi import Fix32
        self.Position = Fix32()
class IFleetEntityFriend:

    def __init__(self):
        pass

class FleetEntityHullProtoBuilder:

    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None
class State:

    def __init__(self):
        pass

class FleetEntityModificationRequest:

    def __init__(self):
        pass

class SlotModification:

    def __init__(self):
        pass

class FleetEntityPartProto:

    def __init__(self):
        self.IconPath = str(0)
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
class ID:

    def __init__(self):
        pass

class FleetEntityGfx:
    Empty = None

    def __init__(self):
        pass

class FleetEntitySlot:

    def __init__(self):
        from Mafi import Option
        self.ExistingPart = Option()
class FleetEntitySlotProto:

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
class SlotType:
    None = None
    Weapons = None
    Engines = None
    HullUpgrades = None
    Radars = None
    FuelTankUpgrades = None

    def __init__(self):
        pass

class Gfx:
    Empty = None

    def __init__(self):
        pass

class FleetEntityStats:

    def __init__(self):
        pass

class FleetFuelTankPartProto:

    def __init__(self):
        self.IconPath = str(0)
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
class FleetEntityHullProto:
    RepairDurationPerProduct = None

    def __init__(self):
        self.Id = None
        self.IconPath = str(0)
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

class ID:

    def __init__(self):
        pass

class FleetHull:

    def __init__(self):
        self.RecoverableHp = int(0)
        self.CurrentHp = int(0)
        self.IsInBattle = False
        self.DamageTakenDuringCurrentBattle = int(0)
        self.ArmorDamageReductionDuringCurrentBattle = int(0)
        self.HpPercent = None
        self.RecoverableHpPercent = None
        self.IsDestroyed = False
        self.IsNotDestroyed = False
class UpgradeHullProto:

    def __init__(self):
        self.IconPath = str(0)
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
class FleetWeaponProto:

    def __init__(self):
        self.AvgDamagePer10Rounds = int(0)
        self.Id = None
        self.IconPath = str(0)
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
class ID:

    def __init__(self):
        pass

class FleetWeapon:

    def __init__(self):
        self.Damage = int(0)
        self.Range = int(0)
        self.ReloadRounds = int(0)
        self.AccuracyAtMinRange = None
        self.AccuracyAtMaxRange = None
        self.RoundsUntilReloaded = int(0)
        self.IsReadyToFire = False
        self.FiredLastSim = False
        self.DamageDuringCurrentBattle = int(0)
        self.MissedDmgDuringCurrentBattle = int(0)
        self.AvgDamagePer10Rounds = int(0)
        self.RecoverableHp = int(0)
        self.CurrentHp = int(0)
        self.IsInBattle = False
        self.DamageTakenDuringCurrentBattle = int(0)
        self.ArmorDamageReductionDuringCurrentBattle = int(0)
        self.HpPercent = None
        self.RecoverableHpPercent = None
        self.IsDestroyed = False
        self.IsNotDestroyed = False
class IBattleSimConfig:

    def __init__(self):
        self.DefenderExtraBattlePriority = int(0)
        self.MaxBattleRounds = int(0)
        self.PossibleEscapeDistance = int(0)
        self.StartingExtraFleetDistance = int(0)
        self.ShipEscapeHpThreshold = None
        self.BaseRoundsToEscape = int(0)
        self.ChanceForSameEntityRepeatedFire = None
        self.ExtraMissChanceWhenEscaping = None
        self.MaxArmorReduction = None
        self.RecoverableHpMultiplier = None
        self.HullDamageMultWhenPartIsHit = None
class UpgradableInt:

    def __init__(self):
        self.BonusValue = int(0)
        self.BonusMultiplier = None
class UpgradableIntProto:

    def __init__(self):
        pass

class UpgradablePercent:

    def __init__(self):
        self.BonusValue = None
        self.BonusMultiplier = None
class UpgradablePercentProto:

    def __init__(self):
        pass

class BattleFleetId:

    def __init__(self):
        pass

