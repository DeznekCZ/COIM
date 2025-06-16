class GeneralPriorities:

    def __init__(self):
        pass

class GlobalPrioritiesManager:

    def __init__(self):
        self.ConstructionPriority = int(0)
        self.DeconstructionPriority = int(0)
class SetGlobalPriorityCmd:

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
class IEntityWithCustomPriority:

    def __init__(self):
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
class IEntityWithGeneralPriority:

    def __init__(self):
        self.GeneralPriority = int(0)
        self.IsGeneralPriorityVisible = False
        self.IsCargoAffectedByGeneralPriority = False
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
class EntityWithGeneralPriorityExtensions:

    def __init__(self):
        pass

class PriorityListsExtensions:

    def __init__(self):
        pass

