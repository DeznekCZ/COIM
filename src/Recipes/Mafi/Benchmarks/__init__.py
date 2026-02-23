
class InlinedOpEqBenchmark:
    def __init__(self):
        pass


    class NonInlinedOps:
        def __init__(self):
            self.Value = 0

    class InlinedOps:
        def __init__(self):
            self.Value = 0

class StaticGetterVsFieldBenchmark:
    ZeroProp = 0
    OneProp = 0
    TwoProp = 0
    ZeroField = 0
    OneField = 0
    TwoField = 0
    def __init__(self):
        pass

