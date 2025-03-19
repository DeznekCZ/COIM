class IMod:

    def __init__(self):
        self.Name = str(0)
        self.Version = int(0)
        self.IsUiOnly = False
        from Mafi import Option
        self.ModConfig = Option()
class IModConfig:

    def __init__(self):
        pass

class DataOnlyMod:

    def __init__(self):
        self.Name = str(0)
        self.Version = int(0)
        self.IsUiOnly = False
        from Mafi import Option
        self.ModConfig = Option()
class IModWithMaps:

    def __init__(self):
        self.Name = str(0)
        self.Version = int(0)
        self.IsUiOnly = False
        from Mafi import Option
        self.ModConfig = Option()
class IModData:

    def __init__(self):
        pass

class RegistrationContext:

    def __init__(self):
        pass

class ModsLoader:

    def __init__(self):
        pass

class ModGroup:

    def __init__(self):
        pass

class ModData:

    def __init__(self):
        self.IsFullyLoaded = False
        self.FailedToLoad = False
class ModsExtensions:

    def __init__(self):
        pass

class ProtoRegistrator:

    def __init__(self):
        self.RegisteredDataClasses = None
        self.DisableAllProtoCosts = False
        self.DisableVehicleFuelConsumption = False
class DataRegistrationException:

    def __init__(self):
        self.Message = str(0)
        self.Data = None
        self.InnerException = None
        self.TargetSite = None
        self.StackTrace = str(0)
        self.HelpLink = str(0)
        self.Source = str(0)
        self.HResult = int(0)
class ProtoRegistratorConfigDevOnly:

    def __init__(self):
        pass

