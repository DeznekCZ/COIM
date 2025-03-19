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
        from Mafi import Fix32
        self.LastMonth = Fix32()
        from Mafi import Fix32
        self.ThisYear = Fix32()
        from Mafi import Fix32
        self.LastYear = Fix32()
        from Mafi import Fix32
        self.Lifetime = Fix32()
        self.HasAnyData = False
        self.HasAnyNonZeroData = False
        self.IsMonthlyEvent = False
        self.AreAnnualDataFull = False
class Fix32SumStats:

    def __init__(self):
        from Mafi import Fix32
        self.LastDay = Fix32()
        from Mafi import Fix32
        self.LastMonth = Fix32()
        from Mafi import Fix32
        self.ThisYear = Fix32()
        from Mafi import Fix32
        self.LastYear = Fix32()
        from Mafi import Fix32
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
        pass

class IntAvgStats:

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
class IntMaxStats:

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
class IntSumStats:

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
class ItemStats:

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
        pass

class IFuelStatsCollector:

    def __init__(self):
        pass

class FuelUsedBy:

    def __init__(self):
        pass

class IItemStatsEvents:

    def __init__(self):
        pass

class StatsDataRange:

    def __init__(self):
        pass

class IDataValuesFormatter:

    def __init__(self):
        pass

class RleSequence:

    def __init__(self):
        self.Count = int(0)
        self.CompressedCount = int(0)
        self.CompressedCapacity = int(0)
        self.HasAnyNonZeroData = False
        self.IsAllocated = False
        self.IsNotEmpty = False
        self.NewestValueOrDefault = None
class Enumerator:

    def __init__(self):
        self.Current = None
