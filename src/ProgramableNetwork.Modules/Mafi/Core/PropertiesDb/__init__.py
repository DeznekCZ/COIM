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
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
class PropertyBoolean:

    def __init__(self):
        self.Id = str(0)
        self.OnChange = None
        self.Value = False
        self.AllModifiers = None
class DurationPropertyProto:

    def __init__(self):
        self.PropertyId = str(0)
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
class PropertyDuration:

    def __init__(self):
        self.Id = str(0)
        self.OnChange = None
        self.Value = None
        self.AllModifiers = None
class PropertyGroups:
    RESEARCH = None
    FOCUS = None
    EDICT = None
    SPACE = None
    POPULATION = None

    def __init__(self):
        pass

class PropertyModifiers:
    NO_GROUP = None

    def __init__(self):
        pass

class PropertyPercentMult:

    def __init__(self):
        self.Id = str(0)
        self.OnChange = None
        self.Value = None
        self.AllModifiers = None
class PropertyPercentSum:

    def __init__(self):
        self.Id = str(0)
        self.OnChange = None
        self.Value = None
        self.AllModifiers = None
class PercentPropertyProto:

    def __init__(self):
        self.PropertyId = str(0)
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
class PropertyType:
    Multiplier = None
    Diff = None

    def __init__(self):
        pass

class PropertyProto:

    def __init__(self):
        self.PropertyId = str(0)
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
class PropsDb:

    def __init__(self):
        pass

class IPropertiesDbInternal:

    def __init__(self):
        pass

