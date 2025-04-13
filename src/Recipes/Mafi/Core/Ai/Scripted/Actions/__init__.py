
class AssignVehicleToEntityAction:
    def __init__(self):
        self.ActionCoreType = None
        self.Description = ""

class BuildStaticEntityAction:
    def __init__(self):
        self.ActionCoreType = None
        self.Description = ""

class PlacementSpec:
    def __init__(self):
        self.SecondRow = False
        self.CustomSpacing = None
        self.AltLane = False
        self.Rotation = None
        from Mafi import Option
        self.RelativeTo = Option()
        self.RelativeToDirection = None
        self.RelativeOffset = None
        self.CustomPosition = None

class BuildStorageAction:
    def __init__(self):
        self.ActionCoreType = None
        self.Description = ""

class ScheduleCommandAction:
    def __init__(self):
        self.ActionCoreType = None
        self.Description = ""

class ScheduleVehicleConstructionAction:
    def __init__(self):
        self.ActionCoreType = None
        self.Description = ""

class SetAiPlayerStageAction:
    def __init__(self):
        self.ActionCoreType = None
        self.Description = ""

class SetEntityEnabledAction:
    def __init__(self):
        self.ActionCoreType = None
        self.Description = ""

class SetRecipeAction:
    def __init__(self):
        self.ActionCoreType = None
        self.Description = ""

class SetupFarmScheduleAction:
    def __init__(self):
        self.ActionCoreType = None
        self.Description = ""

class SetupTerrainDesignationsAction:
    def __init__(self):
        self.ActionCoreType = None
        self.Description = ""

class StartResearchAction:
    def __init__(self):
        self.ActionCoreType = None
        self.Description = ""

class ToggleLogisticsAction:
    def __init__(self):
        self.ActionCoreType = None
        self.Description = ""

class UpgradeStaticEntityAction:
    def __init__(self):
        self.ActionCoreType = None
        self.Description = ""

class WaitForEnoughUnityAction:
    def __init__(self):
        self.ActionCoreType = None
        self.Description = ""

class WaitForNewGlobalProductsAction:
    def __init__(self):
        self.ActionCoreType = None
        self.Description = ""

class WaitForNewVehiclesAction:
    def __init__(self):
        self.ActionCoreType = None
        self.Description = ""

class WaitForStaticEntitiesBuiltAction:
    def __init__(self):
        self.ActionCoreType = None
        self.Description = ""
