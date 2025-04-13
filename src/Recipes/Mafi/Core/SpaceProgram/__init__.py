
class LaunchRocketCmd:
    def __init__(self):
        self.AffectsSaveState = False
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.IsVerificationCmd = False
        self.Result = False
        self.HasError = False
        self.ErrorMessage = ""
        self.LaunchPadId = None

class RocketEntity:
    def __init__(self):
        self.Prototype = None
        self.CanBePaused = False
        self.Position = None
        self.FuelBuffer = None
        self.LaunchedFor = None
        self.Acceleration = None
        self.GainedAltitude = None
        self.IsLaunched = False
        self.IsExploded = None
        self.IsSoundOn = False
        self.SoundParams = None
        from Mafi import Option
        self.Owner = Option()
        self.Id = None
        self.DefaultTitle = None
        self.Context = None
        self.IsDestroyed = False
        self.IsEnabled = False
        self.IsNotEnabled = False
        self.IsPaused = False
        self.IsNotPaused = False
        self.RendererData = None

class RocketEntityBase:
    def __init__(self):
        from Mafi import Option
        self.Owner = Option()
        self.Id = None
        self.DefaultTitle = None
        self.Prototype = None
        self.Context = None
        self.IsDestroyed = False
        self.CanBePaused = False
        self.IsEnabled = False
        self.IsNotEnabled = False
        self.IsPaused = False
        self.IsNotPaused = False
        self.RendererData = None

class RocketLaunchManager:
    UNITY_PER_LAUNCH = None
    UNITY_PER_LAUNCH_DURATION_MONTHS = 0
    def __init__(self):
        self.LaunchExp = 0
        self.LaunchesSuccessesCount = 0
        self.LaunchesFailuresCount = 0
        self.LaunchesCount = 0
        self.RocketLaunched = None

class SetRocketAutoLaunchCmd:
    def __init__(self):
        self.AffectsSaveState = False
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.IsVerificationCmd = False
        self.Result = False
        self.HasError = False
        self.ErrorMessage = ""
        self.LaunchPadId = None
        self.AutoLaunch = False

class IRocketOwner:
    def __init__(self):
        from Mafi import Option
        self.AttachedRocketBase = Option()
        self.RendererData = None
        self.DefaultTitle = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False

class IRocketOwnerExtensions:
    def __init__(self):
        pass


class IRocketLaunchManager:
    def __init__(self):
        self.LaunchesCount = 0
        self.LaunchExp = 0
        self.RocketLaunched = None

class RocketProto:
    def __init__(self):
        self.EntityType = None
        self.Costs = None
        from Mafi.Core.Entities import EntityProto
        self.Id = EntityProto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.CargoCapacity = None
        self.LaunchFuel = None
        self.LaunchExp = 0
        self.AccelerationPerTick = None
        self.TotalFlightTime = None
        self.GroundOffset = None
        self.Graphics = None
        self.DurationToBuild = None
        self.IsPhantom = False
        self.IsInitialized = False

    class Gfx:
        def __init__(self):
            self.PrefabPath = ""
            self.SoundPrefabPath = ""
            self.Color = None
            self.RendererIndex = 0

class TransportedRocketBaseProto:
    def __init__(self):
        self.EntityType = None
        self.Costs = None
        from Mafi.Core.Entities import EntityProto
        self.Id = EntityProto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.DurationToBuild = None
        self.Graphics = None
        self.IsPhantom = False
        self.IsInitialized = False
