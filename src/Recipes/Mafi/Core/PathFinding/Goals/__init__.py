
class AnnularVehicleGoal:
    def __init__(self):
        self.GoalPosition = None
        self.Distance = None
        self.GoalName = None
        self.IsInitialized = False

    class Factory:
        def __init__(self):
            pass


class DynamicEntityVehicleGoal:
    GOAL_INVALID_RETRY_PF_RADIUS = None
    def __init__(self):
        from Mafi import Option
        self.GoalVehicle = Option()
        self.GoalName = None
        self.IsInitialized = False

    class Factory:
        def __init__(self):
            self.m_vehicleSurfaceProvider = None

class IVehicleGoal:
    def __init__(self):
        self.GoalName = None
        self.IsInitialized = False

class IVehicleGoalFull:
    def __init__(self):
        self.GoalName = None
        self.IsInitialized = False

class PlantingVehicleGoal:
    def __init__(self):
        self.GoalPosition = None
        self.Distance = None
        self.GoalName = None
        self.IsInitialized = False

    class Factory:
        def __init__(self):
            pass


class StaticEntityVehicleGoal:
    def __init__(self):
        from Mafi import Option
        self.GoalStaticEntity = Option()
        self.UseCustomTarget = False
        self.GoalName = None
        self.IsInitialized = False

    class Factory:
        def __init__(self):
            pass


class TerrainDesignationVehicleGoal:
    def __init__(self):
        from Mafi import Option
        self.ActualGoalDesignation = Option()
        self.FoundGoalPosition = None
        self.PrimaryGoalDesignation = None
        self.ToleranceRadius = None
        self.GoalName = None
        self.IsInitialized = False

    class Factory:
        def __init__(self):
            pass


class TilePositionVehicleGoal:
    def __init__(self):
        self.GoalTile = None
        self.ToleranceRadius = None
        self.GoalName = None
        self.IsInitialized = False

    class Factory:
        def __init__(self):
            pass


class TreeVehicleGoal:
    def __init__(self):
        self.GoalTreeId = None
        self.TreeDistance = None
        self.GoalPosition = None
        self.Distance = None
        self.GoalName = None
        self.IsInitialized = False

    class Factory:
        def __init__(self):
            pass


class VehicleGoalBase:
    def __init__(self):
        self.IsInitialized = False
        self.GoalName = None

class VehicleGoalsFactory:
    def __init__(self):
        pass

