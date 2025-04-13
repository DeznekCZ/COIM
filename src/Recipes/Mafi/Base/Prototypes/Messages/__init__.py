
class GoalToConstructVehicle:
    def __init__(self):
        self.Title = ""
        self.IsLocked = False
        self.IsCompleted = False
        self.Prototype = None

    class Proto:
        TITLE_CONSTRUCT = None
        def __init__(self):
            self.Implementation = None
            from Mafi.Core.Prototypes import Proto
            self.Id = Proto.ID()

            self.Strings = None
            self.IsNotPhantom = False
            self.Mod = None
            self.Tags = None
            self.IsNotAvailable = False
            self.IsAvailable = False
            self.IsObsolete = False
            self.ProtoToBuild = None
            self.NumberToOwnSinceStart = 0
            self.Tutorial = None
            self.TutorialUnlock = None
            self.Tip = None
            self.LockedByIndex = 0
            self.IsPhantom = False
            self.IsInitialized = False
