class IoPort:

    def __init__(self):
        self.Position = None
        self.Direction = None
        self.ShapePrototype = None
        self.Type = None
        self.Name = None
        from Mafi import Option
        self.ConnectedPort = Option()
        self.IsDestroyed = False
        self.IsConnected = False
        self.IsNotConnected = False
        self.IsEndPort = False
        self.IsConnectedAsInput = False
        self.IsConnectedAsOutput = False
        self.ExpectedConnectedPortCoord = None
        self.ExpectedConnectedPortDirection = None
class PortSpec:

    def __init__(self):
        pass

class IoPortData:

    def __init__(self):
        self.IsValid = False
        self.IsConnected = False
        self.IsNotConnected = False
class IoPortToken:

    def __init__(self):
        pass

class IoPortShapeProto:

    def __init__(self):
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class ID:

    def __init__(self):
        pass

class Gfx:

    def __init__(self):
        pass

class IIoPortsManager:

    def __init__(self):
        pass

class IoPortsManager:

    def __init__(self):
        self.PortConnTiles = None
        self.PortConnectionChanged = None
        from Mafi import Option
        self.Item = Option()
        self.PortsCount = int(0)
        self.Ports = None
class IoPortKey:

    def __init__(self):
        pass

class IoPortTemplate:

    def __init__(self):
        self.Shape = None
        self.Type = None
        self.Name = None
        self.RelativePositionOfConnectedPort = None
class IPortProductResolverImpl:

    def __init__(self):
        self.ResolvedEntityType = None
class PortProductsResolver:

    def __init__(self):
        pass

