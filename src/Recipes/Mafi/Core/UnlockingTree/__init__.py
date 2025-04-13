
class INodeUnlocker:
    def __init__(self):
        pass


class IUnlockingNode:
    def __init__(self):
        self.Units = None

class NodeUnlocker:
    def __init__(self):
        pass


class IUnitUnlocker:
    def __init__(self):
        self.UnlockedType = None

class IUnlockNodeUnit:
    def __init__(self):
        self.HideInUI = False

class IUnlockUnitWithTitleAndIcon:
    def __init__(self):
        self.Title = None
        self.Description = None
        from Mafi import Option
        self.IconPath = Option()
        self.HideInUI = False

class ProductUnlock:
    def __init__(self):
        self.Title = None
        self.Description = None
        from Mafi import Option
        self.IconPath = Option()
        self.UnlockedProtos = None
        self.HideInUI = False
        self.Proto = None

class IProtoUnlock:
    def __init__(self):
        self.UnlockedProtos = None
        self.HideInUI = False

class ProtoUnlock:
    def __init__(self):
        self.UnlockedProtos = None
        self.HideInUI = False

class ProtoUnitUnlocker:
    def __init__(self):
        self.UnlockedType = None

class ProtoWithIconUnlock:
    def __init__(self):
        self.Title = None
        self.Description = None
        from Mafi import Option
        self.IconPath = Option()
        self.UnlockedProtos = None
        self.HideInUI = False
        self.Proto = None

class RecipeUnlock:
    def __init__(self):
        self.UnlockedProtos = None
        self.HideInUI = False
        self.Proto = None
        self.MachineProto = None

class RecyclingRatioIncreaseUnlock:
    def __init__(self):
        self.Title = None
        self.Description = None
        from Mafi import Option
        self.IconPath = Option()
        self.HideInUI = False
        self.RecyclingRatioIncrease = None

class RecyclingRatioIncreaseUnlocker:
    def __init__(self):
        self.UnlockedType = None

class VehicleLimitIncreaseUnlock:
    def __init__(self):
        self.Title = None
        self.Description = None
        from Mafi import Option
        self.IconPath = Option()
        self.HideInUI = False
        self.LimitIncrease = 0

class VehicleLimitIncreaseUnlocker:
    def __init__(self):
        self.UnlockedType = None
