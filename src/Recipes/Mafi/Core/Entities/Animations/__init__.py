
class AnimationParams:
    def __init__(self):
        self.AnimationStateName = ""

class IAnimationStateFactory:
    def __init__(self):
        pass


class NoAnimationsAnimationStateFactory:
    def __init__(self):
        pass


class AnimationStateFactory:
    def __init__(self):
        pass


class AnimationWithPauseState:
    def __init__(self):
        self.AnimationStateName = ""

class AnimationWithPauseParams:
    def __init__(self):
        self.TotalDuration = None
        self.FillMode = None
        self.PauseAt = None
        self.PauseForDuration = None
        self.BaseSpeed = None
        self.AnimationStateName = ""

    class Mode:
        ExtendPauseToFit = None
        ScaleAnimationSpeedToFit = None
        def __init__(self):
            self.value__ = 0

class IAnimationState:
    def __init__(self):
        self.AnimationStateName = ""

class IAnimationStateImpl:
    def __init__(self):
        self.AnimationStateName = ""

class LoopAnimationState:
    def __init__(self):
        self.AnimationStateName = ""

class LoopAnimationParams:
    FullSpeed = None
    def __init__(self):
        self.Speed = None
        self.PlayBackwardsWhenFlipped = False
        self.AnimationStateName = ""

class RepeatAnimationState:
    def __init__(self):
        self.AnimationStateName = ""

class RepeatableAnimationParams:
    def __init__(self):
        self.TotalDuration = None
        self.RepeatCount = None
        self.ChangeSpeedToFit = False
        self.CustomSpeed = None
        self.DelayedStartAt = None
        self.AnimationStateName = ""
