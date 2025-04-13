
class AirPollutionManager:
    def __init__(self):
        self.ProvidedProducts = None
        self.LastMonthHealthPenalty = None
        self.Stats = None
        self.StatsVehicles = None
        self.StatsShips = None

class GroundWaterManager:
    def __init__(self):
        pass


class IWeatherProvider:
    def __init__(self):
        pass


class AnyWeatherProvider:
    def __init__(self):
        pass


class IRadiationManager:
    def __init__(self):
        pass


class RadiationManager:
    def __init__(self):
        self.LastMonthPopsGrowthReduction = None
        self.Stats = None

class WaterPollutionManager:
    def __init__(self):
        self.ProvidedProducts = None
        self.LastMonthHealthPenalty = None
        self.Buffer = None
        self.Stats = None

class IWeatherManager:
    def __init__(self):
        self.CurrentWeather = None
        self.NextWeather = None
        self.RainIntensity = None
        self.SimSunIntensity = None
        self.WorldWetness = None

class WeatherManager:
    HALF_WEATHER_GFX_TRANSITION_DAYS = None
    def __init__(self):
        self.CurrentWeather = None
        self.NextWeather = None
        self.RainIntensity = None
        self.SimSunIntensity = None
        self.WorldWetness = None

class WeatherProto:
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
        self.SunIntensity = None
        self.RainIntensity = None
        self.Graphics = None
        self.IsPhantom = False
        self.IsInitialized = False

    class Gfx:
        Empty = None
        def __init__(self):
            self.SkyColor = None
            self.LightColor = None
            self.FogColor = None
            self.LightIntensity = 0.0
            self.FogIntensity = 0.0
            self.WindStrength = 0.0
            self.MinCloudIntensity = 0.0
            self.MaxCloudIntensity = 0.0
            self.ShadowsIntensityAbs = 0.0
            self.OceanChoppiness = 0.0
            self.LightningProbabilityPerTick = 0.0
            self.IconPath = ""
            from Mafi import Option
            self.SoundPrefabPath = Option()
