class AirPollutionManager:

    def __init__(self):
        self.ProvidedProducts = None
        self.LastMonthHealthPenalty = None
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
class WaterPollutionManager:

    def __init__(self):
        self.ProvidedProducts = None
        self.LastMonthHealthPenalty = None
        self.Buffer = None
class IWeatherManager:

    def __init__(self):
        self.CurrentWeather = None
        self.NextWeather = None
        self.RainIntensity = None
        self.SimSunIntensity = None
        self.WorldWetness = None
class WeatherManager:

    def __init__(self):
        self.CurrentWeather = None
        self.NextWeather = None
        self.RainIntensity = None
        self.SimSunIntensity = None
        self.WorldWetness = None
class WeatherProto:

    def __init__(self):
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class Gfx:

    def __init__(self):
        pass

