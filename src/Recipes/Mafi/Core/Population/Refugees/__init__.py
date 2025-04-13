
class RefugeesManager:
    def __init__(self):
        from Mafi import Option
        self.Beacon = Option()
        self.NextReward = Option()
        self.StepsDoneSoFar = None
        self.DurationLeft = None
