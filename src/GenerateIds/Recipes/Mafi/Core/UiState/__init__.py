
class HudStateManager:
    def __init__(self):
        self.AreNotificationsMuted = False

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

class StateForUi:
    Neutral = None
    Inactive = None
    Important = None
    Positive = None
    Warning = None
    Danger = None
    def __init__(self):
        self.value__ = 0

class ToolbarGroupProto:
    def __init__(self):
        self.IconPath = ""
        self.TierData = None
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

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
        self.IsPhantom = False
