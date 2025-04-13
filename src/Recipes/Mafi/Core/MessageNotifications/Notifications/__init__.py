
class GameOverNotification:
    def __init__(self):
        self.NotificationId = None
        self.Title = None
        self.Message = None
        self.CanBeDismissed = False

class LocationExploredMessage:
    def __init__(self):
        self.NotificationId = None
        self.Location = None
        from Mafi import Option
        self.Loot = Option()
        self.UnlockedProtos = None

class NewMessageNotification:
    def __init__(self):
        self.NotificationId = None
        self.Message = None

class NewRefugeesMessage:
    def __init__(self):
        self.NotificationId = None
        self.AmountOfRefugeesAdded = 0
        self.Reward = None

class ResearchFinishedMessage:
    def __init__(self):
        self.NotificationId = None
        self.ResearchNode = None
        self.UnlockedProtos = None

class ShipInBattleNotification:
    def __init__(self):
        self.NotificationId = None
        self.Location = None
        self.BattleState = None
