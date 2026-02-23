
class AlphaStaticIslandMap:
    def __init__(self):
        self.Name = ""

class BeachStaticIslandMap:
    def __init__(self):
        self.Name = ""

class CurlandMap:
    def __init__(self):
        self.Name = ""

class GoldenPeakStaticIslandMap:
    def __init__(self):
        self.Name = ""

class InsulaMortis:
    LocName = None
    LocDescription = None
    def __init__(self):
        self.Name = ""

class StaticIslandMapsRegistry:
    IslandMaps = None
    def __init__(self):
        pass


class StaticIslandMap:
    AlphaMap = None
    Beach = None
    Curland = None
    GoldenPeak = None
    YouShallNotPass = None
    InsulaMortis = None
    Crater = None
    def __init__(self):
        self.value__ = 0

class YouShallNotPassStaticIslandMap:
    LocName = None
    LocDescription = None
    def __init__(self):
        self.Name = ""
