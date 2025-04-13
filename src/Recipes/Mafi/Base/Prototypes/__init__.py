
class PopNeedDiseaseTrigger:
    def __init__(self):
        pass


class TrashDiseaseTrigger:
    def __init__(self):
        pass


class PopNeedDiseaseProto:
    def __init__(self):
        from Mafi import Option
        self.CustomTrigger = Option()
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.Need = None
        self.HealthPenalty = None
        self.MonthlyMortalityRate = None
        self.DurationInMonths = 0
        self.MinDistanceTraveled = 0
        self.Reason = None
        self.IsPhantom = False
        self.IsInitialized = False

class TrashDiseaseProto:
    def __init__(self):
        from Mafi import Option
        self.CustomTrigger = Option()
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.HealthPenalty = None
        self.MonthlyMortalityRate = None
        self.DurationInMonths = 0
        self.MinDistanceTraveled = 0
        self.Reason = None
        self.IsPhantom = False
        self.IsInitialized = False
