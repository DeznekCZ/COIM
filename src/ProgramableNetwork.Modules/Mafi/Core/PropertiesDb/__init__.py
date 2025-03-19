class IPropertiesDb:

    def __init__(self):
        pass

class PropertiesDbExtensions:

    def __init__(self):
        pass

class IProperty:

    def __init__(self):
        self.Id = str(0)
class IPropertyExtensions:

    def __init__(self):
        pass

class BooleanPropertyProto:

    def __init__(self):
        self.PropertyId = str(0)
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class PropertyBoolean:

    def __init__(self):
        self.Id = str(0)
        self.OnChange = None
        self.Value = False
class DurationPropertyProto:

    def __init__(self):
        self.PropertyId = str(0)
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class PropertyDuration:

    def __init__(self):
        self.Id = str(0)
        self.OnChange = None
        self.Value = None
class PropertyModifiers:

    def __init__(self):
        pass

class PropertyPercentMult:

    def __init__(self):
        self.Id = str(0)
        self.OnChange = None
        self.Value = None
class PropertyPercentSum:

    def __init__(self):
        self.Id = str(0)
        self.OnChange = None
        self.Value = None
class PercentPropertyProto:

    def __init__(self):
        self.PropertyId = str(0)
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class PropertyType:

    def __init__(self):
        pass

class PropertyProto:

    def __init__(self):
        self.PropertyId = str(0)
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class PropsDb:

    def __init__(self):
        pass

class IPropertiesDbInternal:

    def __init__(self):
        pass

