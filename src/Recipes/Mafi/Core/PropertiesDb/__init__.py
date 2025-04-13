
class IPropertiesDb:
    def __init__(self):
        pass


class PropertiesDbExtensions:
    def __init__(self):
        pass


class IProperty:
    def __init__(self):
        self.Id = ""

class IPropertyExtensions:
    def __init__(self):
        pass


class BooleanPropertyProto:
    def __init__(self):
        self.PropertyId = ""
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.IsPhantom = False
        self.IsInitialized = False

class PropertyBoolean:
    def __init__(self):
        self.Id = ""
        self.OnChange = None
        self.Value = False

class DurationPropertyProto:
    def __init__(self):
        self.PropertyId = ""
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.IsPhantom = False
        self.IsInitialized = False

class PropertyDuration:
    def __init__(self):
        self.Id = ""
        self.OnChange = None
        self.Value = None

class PropertyModifiers:
    from Mafi import Option
    NO_GROUP = Option()
    def __init__(self):
        pass


class PropertyPercentMult:
    def __init__(self):
        self.Id = ""
        self.OnChange = None
        self.Value = None

class PropertyPercentSum:
    def __init__(self):
        self.Id = ""
        self.OnChange = None
        self.Value = None

class PercentPropertyProto:
    def __init__(self):
        self.PropertyId = ""
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.IsPhantom = False
        self.IsInitialized = False

    class PropertyType:
        Multiplier = None
        Diff = None
        def __init__(self):
            self.value__ = 0

class PropertyProto:
    def __init__(self):
        self.PropertyId = ""
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.IsPhantom = False
        self.IsInitialized = False

class PropsDb:
    def __init__(self):
        pass


class IPropertiesDbInternal:
    def __init__(self):
        pass

