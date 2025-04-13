
class BattleFleet:
    def __init__(self):
        self.IsInBattle = False
        self.IsNotInBattle = False
        self.Entities = None
        self.Name = ""
        self.IsHuman = False

class BattleResult:
    def __init__(self):
        self.Winner = None
        self.Loser = None
        self.WinnerStats = None
        self.LoserStats = None
        self.BattleLog = None
        from Mafi import Option
        self.PlayerResult = Option()

class PlayersBattleResult:
    def __init__(self):
        self.PlayerFleetStats = None
        self.OpponentFleetStats = None
        self.PlayerWon = False
        self.PlayerWasAttacker = False
        self.BattleRoundsCount = 0

class FleetBattleResultStats:
    def __init__(self):
        self.Name = ""
        self.DamageDone = 0
        self.DamageMissed = 0
        self.DamageTaken = 0
        self.ArmorDamageReduction = 0

class IBattleSimulator:
    def __init__(self):
        from Mafi import Option
        self.OngoingBattle = Option()
        self.AllBattleResults = None

class BattleSimulator:
    def __init__(self):
        from Mafi import Option
        self.OngoingBattle = Option()
        self.AllBattleResults = None
        self.Config = None

class IBattleState:
    def __init__(self):
        self.Attacker = None
        self.Defender = None
        self.BattleRound = 0
        from Mafi import Option
        self.Result = Option()

class BattleState:
    def __init__(self):
        self.Attacker = None
        self.Defender = None
        from Mafi import Option
        self.Result = Option()
        self.BattleRound = 0
        self.BattleSortedEntities = None
        self.BattleLog = None
        self.BattleSimConfig = None
        self.Random = None

class IBattleAware:
    def __init__(self):
        pass


class DestructibleFleetPartProto:
    def __init__(self):
        self.IconPath = ""
        from Mafi.Core.Fleet import FleetEntityPartProto
        self.Id = FleetEntityPartProto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.MaxHp = 0
        self.HitWeight = 0
        self.Value = None
        self.Graphics = None
        self.ExtraCrew = None
        self.IsPhantom = False
        self.IsInitialized = False

class DestructibleFleetPart:
    def __init__(self):
        self.RecoverableHp = 0
        self.CurrentHp = 0
        self.IsInBattle = False
        self.DamageTakenDuringCurrentBattle = 0
        self.ArmorDamageReductionDuringCurrentBattle = 0
        self.HpPercent = None
        self.RecoverableHpPercent = None
        self.IsDestroyed = False
        self.IsNotDestroyed = False
        self.DestructiblePartProto = None
        self.MaxHp = None
        self.HitWeight = None

class FleetBridgePartProto:
    def __init__(self):
        self.IconPath = ""
        from Mafi.Core.Fleet import FleetEntityPartProto
        self.Id = FleetEntityPartProto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.Graphics = None
        self.Value = None
        self.ExtraCrew = None
        self.IsPhantom = False
        self.IsInitialized = False

    class Gfx:
        Empty = None
        def __init__(self):
            self.PrefabPath = ""
            self.IconPath = ""
            from Mafi import Option
            self.GameObjectToShow = Option()

class FleetEnginePartProto:
    def __init__(self):
        self.IconPath = ""
        from Mafi.Core.Fleet import FleetEntityPartProto
        self.Id = FleetEntityPartProto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        from Mafi import Fix32
        self.DistancePerStep = Fix32()
        self.DistancePerFuel = Fix32()
        self.FuelCapacity = None
        self.Value = None
        self.Graphics = None
        self.ExtraCrew = None
        self.IsPhantom = False
        self.IsInitialized = False

class FleetEntity:
    RepairCostForValue = None
    RepairCostForValueDestroyedPart = None
    from Mafi import Fix32
    DistancePerStepInBattle = Fix32()
    def __init__(self):
        self.Name = None
        self.BattlePriority = None
        self.HitChanceWeight = None
        self.ExtraRoundsToEscape = None
        self.FuelTankCapacity = None
        self.MinCrewNeeded = 0
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
        self.RoundsToEscape = 0
        self.HasEscaped = False
        self.HasNotEscaped = False
        self.IsFacingLeft = False
        self.CanContinueBattle = False
        from Mafi import Fix32
        self.Position = Fix32()
        self.CrewNeeded = None
        self.DistancePerStep = Fix32()
        self.DistancePerFuel = Fix32()
        self.Armor = None
        from Mafi import Option
        self.PreviouslyAttackedEntity = Option()

class IFleetEntityFriend:
    def __init__(self):
        pass


class FleetEntityHullProtoBuilder:
    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None

    class State:
        def __init__(self):
            self.Builder = None

class FleetEntityModificationRequest:
    def __init__(self):
        self.SlotsData = None
        self.HullId = None

class SlotModification:
    def __init__(self):
        from Mafi.Core.Prototypes import Proto
        self.SlotId = Proto.ID()

        self.Part = None

class FleetEntityPartProto:
    def __init__(self):
        self.IconPath = ""
        from Mafi.Core.Fleet import FleetEntityPartProto
        self.Id = FleetEntityPartProto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.Value = None
        self.Graphics = None
        self.ExtraCrew = None
        self.IsPhantom = False
        self.IsInitialized = False

    class ID:
        def __init__(self):
            self.Value = ""

class FleetEntityGfx:
    Empty = None
    def __init__(self):
        self.IconPath = ""
        from Mafi import Option
        self.GameObjectToShow = Option()

class FleetEntitySlot:
    def __init__(self):
        from Mafi import Option
        self.ExistingPart = Option()
        self.Proto = None

class FleetEntitySlotProto:
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
        self.TypeOfSlot = None
        self.EligibleItems = None
        self.Graphics = None
        self.IsPhantom = False
        self.IsInitialized = False

    class SlotType:
        None = None
        Weapons = None
        Engines = None
        HullUpgrades = None
        Radars = None
        FuelTankUpgrades = None
        def __init__(self):
            self.value__ = 0

    class Gfx:
        Empty = None
        def __init__(self):
            from Mafi import Option
            self.GoToShowIfEmpty = Option()

class FleetEntityStats:
    def __init__(self):
        self.HitPoints = 0
        self.Armor = 0
        self.AvgDamage = 0
        self.MaxWeaponRange = 0
        self.Crew = 0
        self.RadarRange = 0
        self.FuelTankCapacity = None

class FleetFuelTankPartProto:
    def __init__(self):
        self.IconPath = ""
        from Mafi.Core.Fleet import FleetEntityPartProto
        self.Id = FleetEntityPartProto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.AddedFuelCapacity = None
        self.Value = None
        self.Graphics = None
        self.ExtraCrew = None
        self.IsPhantom = False
        self.IsInitialized = False

class FleetEntityHullProto:
    RepairDurationPerProduct = None
    def __init__(self):
        from Mafi.Core.Fleet import FleetEntityHullProto
        self.Id = FleetEntityHullProto.ID()

        self.IconPath = ""
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.Graphics = None
        self.SlotGroups = None
        self.BattlePriority = 0
        self.HitChanceWeight = 0
        self.ExtraRoundsToEscape = 0
        self.MaxHp = 0
        self.HitWeight = 0
        self.Value = None
        self.ExtraCrew = None
        self.IsPhantom = False
        self.IsInitialized = False

    class Gfx:
        Empty = None
        def __init__(self):
            self.IconContentWidth = None
            self.IconContentTopOffset = None
            self.IconPath = ""
            from Mafi import Option
            self.GameObjectToShow = Option()

    class ID:
        def __init__(self):
            self.Value = ""

class FleetHull:
    def __init__(self):
        self.RecoverableHp = 0
        self.CurrentHp = 0
        self.IsInBattle = False
        self.DamageTakenDuringCurrentBattle = 0
        self.ArmorDamageReductionDuringCurrentBattle = 0
        self.HpPercent = None
        self.RecoverableHpPercent = None
        self.IsDestroyed = False
        self.IsNotDestroyed = False
        self.Proto = None
        self.BattlePriority = None
        self.HitChanceWeight = None
        self.ExtraRoundsToEscape = None
        self.RadarRange = None
        self.DestructiblePartProto = None
        self.MaxHp = None
        self.HitWeight = None

class UpgradeHullProto:
    def __init__(self):
        self.IconPath = ""
        from Mafi.Core.Fleet import FleetEntityPartProto
        self.Id = FleetEntityPartProto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.Value = None
        self.Graphics = None
        self.ExtraCrew = None
        self.IsPhantom = False
        self.IsInitialized = False

class FleetWeaponProto:
    def __init__(self):
        self.AvgDamagePer10Rounds = 0
        from Mafi.Core.Fleet import FleetWeaponProto
        self.Id = FleetWeaponProto.ID()

        self.IconPath = ""
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.Damage = 0
        self.Range = 0
        self.ReloadRounds = 0
        self.AccuracyAtMinRange = None
        self.AccuracyAtMaxRange = None
        self.MaxHp = 0
        self.HitWeight = 0
        self.Value = None
        self.Graphics = None
        self.ExtraCrew = None
        self.IsPhantom = False
        self.IsInitialized = False

    class ID:
        def __init__(self):
            self.Value = ""

class FleetWeapon:
    def __init__(self):
        self.Damage = 0
        self.Range = 0
        self.ReloadRounds = 0
        self.AccuracyAtMinRange = None
        self.AccuracyAtMaxRange = None
        self.RoundsUntilReloaded = 0
        self.IsReadyToFire = False
        self.FiredLastSim = False
        self.DamageDuringCurrentBattle = 0
        self.MissedDmgDuringCurrentBattle = 0
        self.AvgDamagePer10Rounds = 0
        self.RecoverableHp = 0
        self.CurrentHp = 0
        self.IsInBattle = False
        self.DamageTakenDuringCurrentBattle = 0
        self.ArmorDamageReductionDuringCurrentBattle = 0
        self.HpPercent = None
        self.RecoverableHpPercent = None
        self.IsDestroyed = False
        self.IsNotDestroyed = False
        self.Proto = None
        self.OwningEntity = None
        self.DestructiblePartProto = None
        self.MaxHp = None
        self.HitWeight = None

class IBattleSimConfig:
    def __init__(self):
        self.DefenderExtraBattlePriority = 0
        self.MaxBattleRounds = 0
        self.PossibleEscapeDistance = 0
        self.StartingExtraFleetDistance = 0
        self.ShipEscapeHpThreshold = None
        self.BaseRoundsToEscape = 0
        self.ChanceForSameEntityRepeatedFire = None
        self.ExtraMissChanceWhenEscaping = None
        self.MaxArmorReduction = None
        self.RecoverableHpMultiplier = None
        self.HullDamageMultWhenPartIsHit = None

class UpgradableInt:
    def __init__(self):
        self.BonusValue = 0
        self.BonusMultiplier = None
        self.BaseValue = 0

class UpgradableIntProto:
    def __init__(self):
        self.BonusValue = 0
        self.BonusMultiplier = None

class UpgradablePercent:
    def __init__(self):
        self.BonusValue = None
        self.BonusMultiplier = None
        self.BaseValue = None
        self.IsLimited0To100 = False

class UpgradablePercentProto:
    def __init__(self):
        self.BonusValue = None
        self.BonusMultiplier = None

class BattleFleetId:
    def __init__(self):
        self.Value = 0
