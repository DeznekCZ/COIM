
class CargoShipCountableModuleProto:
    def __init__(self):
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.Graphics = None
        self.ProductType = None
        self.Capacity = None
        self.IsPhantom = False
        self.IsInitialized = False

    class Gfx:
        def __init__(self):
            self.ContainerPrefabPath = ""
            self.ContainersParentGoPath = ""
            self.ContainerPositions = None
            self.PrefabPath = ""

class CargoShipLooseModuleProto:
    def __init__(self):
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.Graphics = None
        self.ProductType = None
        self.Capacity = None
        self.IsPhantom = False
        self.IsInitialized = False

    class Gfx:
        def __init__(self):
            self.SmoothPilePath = ""
            self.RoughPilePath = ""
            self.OffsetEmpty = None
            self.OffsetFull = None
            self.PrefabPath = ""

class CargoShipModule:
    def __init__(self):
        from Mafi import Option
        self.StoredProduct = Option()
        self.UsableCapacity = None
        self.Quantity = None
        self.Capacity = None
        self.Prototype = None
        self.CargoShip = None
        self.DepotModule = None

class ICargoShipModuleFriend:
    def __init__(self):
        pass


class CargoShipModuleProto:
    def __init__(self):
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.Graphics = None
        self.ProductType = None
        self.Capacity = None
        self.IsPhantom = False
        self.IsInitialized = False

    class Gfx:
        EMPTY = None
        def __init__(self):
            self.PrefabPath = ""
