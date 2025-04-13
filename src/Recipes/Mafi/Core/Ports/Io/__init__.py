
class IoPort:
    MAX_TRANSFER_PER_TICK = None
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
        self.PortIndex = None
        self.Id = None
        self.OwnerEntity = None
        self.Spec = None
        self.RendererId = None

class PortSpec:
    def __init__(self):
        self.Name = None
        self.Type = None
        self.Shape = None
        self.CanOnlyConnectToTransports = False

class IoPortData:
    Invalid = None
    def __init__(self):
        self.IsValid = False
        self.IsConnected = False
        self.IsNotConnected = False
        self.Name = None
        self.AllowedProductType = None
        self.PortIndex = None
        from Mafi import Option
        self.ConnectedTo = Option()

class IoPortToken:
    Invalid = None
    def __init__(self):
        self.PortIndex = None
        self.Name = None

class IoPortShapeProto:
    def __init__(self):
        from Mafi.Core.Ports.Io import IoPortShapeProto
        self.Id = IoPortShapeProto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.LayoutChar = None
        self.AllowedProductType = None
        self.Graphics = None
        self.IsPhantom = False
        self.IsInitialized = False

    class ID:
        def __init__(self):
            self.Value = ""

    class Gfx:
        Empty = None
        def __init__(self):
            self.ConnectedPortPrefabPath = ""
            self.ConnectedPortPrefabPathLod3 = ""
            self.DisconnectedPortPrefabPath = ""
            self.DisconnectedPortPrefabPathLod3 = ""
            self.RendererIndexConnected = 0
            self.RendererIndexDisconnected = 0
            self.ShowWhenTwoTransportsConnect = False

class IIoPortsManager:
    def __init__(self):
        pass


class IoPortsManager:
    def __init__(self):
        self.PortConnTiles = None
        self.PortConnectionChanged = None
        from Mafi import Option
        self.Item = Option()
        self.PortsCount = 0
        self.Ports = None

class IoPortKey:
    def __init__(self):
        self.Position = None
        self.Direction = None

class IoPortTemplate:
    def __init__(self):
        self.Shape = None
        self.Type = None
        self.Name = None
        self.RelativePositionOfConnectedPort = None
        self.Spec = None
        self.RelativePosition = None
        self.RelativeDirection = None

class IPortProductResolverImpl:
    def __init__(self):
        self.ResolvedEntityType = None

class PortProductsResolver:
    def __init__(self):
        pass

