
class HudStateManager:
    def __init__(self):
        pass


class LastSelectedTierTracker:
    def __init__(self):
        pass


    class TiersData:
        def __init__(self):
            self.LastSelected = None
            self.UpgradeChain = None

class NewProtosTracker:
    def __init__(self):
        pass


class UiCameraState:
    def __init__(self):
        self.CameraPose = None
        self.SavedPoses = None

    class Pose:
        def __init__(self):
            self.PivotPosition = None
            self.PivotHeight = None
            self.OrbitRadius = None
            self.YawAngle = None
            self.PitchAngle = None
