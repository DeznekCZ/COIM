
class ComputingAvgStats:
    def __init__(self):
        self.LastDay = None
        self.LastMonth = None
        self.ThisYear = None
        self.LastYear = None
        self.Lifetime = None
        self.HasAnyData = False
        self.HasAnyNonZeroData = False
        self.IsMonthlyEvent = False
        self.AreAnnualDataFull = False

class ElectricityAvgStats:
    def __init__(self):
        self.LastDay = None
        self.LastMonth = None
        self.ThisYear = None
        self.LastYear = None
        self.Lifetime = None
        self.HasAnyData = False
        self.HasAnyNonZeroData = False
        self.IsMonthlyEvent = False
        self.AreAnnualDataFull = False

class Fix32AvgStats:
    def __init__(self):
        from Mafi import Fix32
        self.LastDay = Fix32()
        self.LastMonth = Fix32()
        self.ThisYear = Fix32()
        self.LastYear = Fix32()
        self.Lifetime = Fix32()
        self.HasAnyData = False
        self.HasAnyNonZeroData = False
        self.IsMonthlyEvent = False
        self.AreAnnualDataFull = False

class Fix32SumStats:
    def __init__(self):
        from Mafi import Fix32
        self.LastDay = Fix32()
        self.LastMonth = Fix32()
        self.ThisYear = Fix32()
        self.LastYear = Fix32()
        self.Lifetime = Fix32()
        self.HasAnyData = False
        self.HasAnyNonZeroData = False
        self.IsMonthlyEvent = False
        self.AreAnnualDataFull = False

class FuelStatsCollector:
    def __init__(self):
        self.Stats = None

    class StatsPerProduct:
        def __init__(self):
            self.Product = None
            self.TotalConsumedInVehicles = None
            self.TotalConsumedInCargoShips = None
            self.TotalConsumedInBattleship = None
            self.TotalConsumedInPowerGenerators = None

class IntAvgStats:
    def __init__(self):
        self.LastDay = 0
        self.LastMonth = 0
        self.ThisYear = 0
        self.LastYear = 0
        self.Lifetime = 0
        self.HasAnyData = False
        self.HasAnyNonZeroData = False
        self.IsMonthlyEvent = False
        self.AreAnnualDataFull = False

class IntMaxStats:
    def __init__(self):
        self.LastDay = 0
        self.LastMonth = 0
        self.ThisYear = 0
        self.LastYear = 0
        self.Lifetime = 0
        self.HasAnyData = False
        self.HasAnyNonZeroData = False
        self.IsMonthlyEvent = False
        self.AreAnnualDataFull = False

class IntSumStats:
    def __init__(self):
        self.LastDay = 0
        self.LastMonth = 0
        self.ThisYear = 0
        self.LastYear = 0
        self.Lifetime = 0
        self.HasAnyData = False
        self.HasAnyNonZeroData = False
        self.IsMonthlyEvent = False
        self.AreAnnualDataFull = False

class ItemStats:
    QUARTER_CENTURY = 0
    MAX_YEARS_OF_ANNUAL_DATA = 0
    MAX_MONTHS_OF_MONTHLY_DATA = 0
    MAX_DAYS_OF_DAILY_DATA = 0
    def __init__(self):
        self.HasAnyData = False
        self.HasAnyNonZeroData = False
        self.IsMonthlyEvent = False
        self.AreAnnualDataFull = False

class MechPowerAvgStats:
    def __init__(self):
        self.LastDay = None
        self.LastMonth = None
        self.ThisYear = None
        self.LastYear = None
        self.Lifetime = None
        self.HasAnyData = False
        self.HasAnyNonZeroData = False
        self.IsMonthlyEvent = False
        self.AreAnnualDataFull = False

class QuantityMaxStats:
    def __init__(self):
        self.LastDay = None
        self.LastMonth = None
        self.ThisYear = None
        self.LastYear = None
        self.Lifetime = None
        self.HasAnyData = False
        self.HasAnyNonZeroData = False
        self.IsMonthlyEvent = False
        self.AreAnnualDataFull = False

class QuantitySumStats:
    def __init__(self):
        self.LastDay = None
        self.LastMonth = None
        self.ThisYear = None
        self.LastYear = None
        self.Lifetime = None
        self.HasAnyData = False
        self.HasAnyNonZeroData = False
        self.IsMonthlyEvent = False
        self.AreAnnualDataFull = False

class StatsManager:
    def __init__(self):
        self.ProtosDb = None

class IFuelStatsCollector:
    def __init__(self):
        pass


class FuelUsedBy:
    Vehicle = None
    CargoShip = None
    BattleShip = None
    PowerGenerator = None
    def __init__(self):
        self.value__ = 0

class IItemStatsEvents:
    def __init__(self):
        pass


class StatsDataRange:
    Last120Days = None
    Last120Months = None
    Last100Years = None
    QuarterCenturies = None
    def __init__(self):
        self.value__ = 0

class IDataValuesFormatter:
    def __init__(self):
        pass


class RleSequence:
    DEFAULT_RLE_CAPACITY = 0
    MAX_REPS_PER_ENTRY = 0
    def __init__(self):
        self.Count = 0
        self.CompressedCount = 0
        self.CompressedCapacity = 0
        self.HasAnyNonZeroData = False
        self.IsAllocated = False
        self.IsNotEmpty = False
        self.NewestValueOrDefault = 0

    class Enumerator:
        def __init__(self):
            self.Current = 0
