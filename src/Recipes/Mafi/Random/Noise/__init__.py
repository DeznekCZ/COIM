
class ConstantNoise2D:
    def __init__(self):
        from Mafi import Fix32
        self.MeanValue = Fix32()
        self.Amplitude = Fix32()
        self.Period = Fix32()

class ExpBendNoise2D:
    MAX_VALUE = None
    def __init__(self):
        from Mafi import Fix32
        self.MeanValue = Fix32()
        self.Amplitude = Fix32()
        self.Period = Fix32()
        self.BaseNoise = None
        from Mafi import Fix64
        self.Amount = Fix64()
        self.Bias = Fix64()

class ExpBendNoiseParams:
    def __init__(self):
        self.Amount = None
        from Mafi import Fix32
        self.Bias = Fix32()

class LineDistanceNoise:
    def __init__(self):
        from Mafi import Fix32
        self.MeanValue = Fix32()
        self.Amplitude = Fix32()
        self.Period = Fix32()

class Noise2dTransform:
    def __init__(self):
        from Mafi import Fix32
        self.MeanValue = Fix32()
        self.Amplitude = Fix32()
        self.Period = Fix32()
        from Mafi import Fix64
        self.Multiplier = Fix64()
        self.Addend = Fix64()
        self.FrequencyMult = Fix32()

class Noise2dTransformAdd:
    def __init__(self):
        from Mafi import Fix32
        self.MeanValue = Fix32()
        self.Amplitude = Fix32()
        self.Period = Fix32()
        from Mafi import Fix64
        self.Addend = Fix64()

class Noise2dTransformMax:
    def __init__(self):
        from Mafi import Fix32
        self.MeanValue = Fix32()
        self.Amplitude = Fix32()
        self.Period = Fix32()
        from Mafi import Fix64
        self.MinValue = Fix64()

class Noise2dTransformMaxNoise:
    def __init__(self):
        from Mafi import Fix32
        self.MeanValue = Fix32()
        self.Amplitude = Fix32()
        self.Period = Fix32()
        self.BaseNoise = None
        self.OtherNoise = None

class Noise2dTransformMin:
    def __init__(self):
        from Mafi import Fix32
        self.MeanValue = Fix32()
        self.Amplitude = Fix32()
        self.Period = Fix32()
        from Mafi import Fix64
        self.MaxValue = Fix64()

class Noise2dTransformMinNoise:
    def __init__(self):
        from Mafi import Fix32
        self.MeanValue = Fix32()
        self.Amplitude = Fix32()
        self.Period = Fix32()
        self.BaseNoise = None
        self.OtherNoise = None

class Noise2dTransformMult:
    def __init__(self):
        from Mafi import Fix32
        self.MeanValue = Fix32()
        self.Amplitude = Fix32()
        self.Period = Fix32()
        from Mafi import Fix64
        self.Multiplier = Fix64()

class Noise2dTransformMultAdd:
    def __init__(self):
        from Mafi import Fix32
        self.MeanValue = Fix32()
        self.Amplitude = Fix32()
        self.Period = Fix32()
        from Mafi import Fix64
        self.Multiplier = Fix64()
        self.Addend = Fix64()

class Noise2dTransformParams:
    def __init__(self):
        from Mafi import Fix64
        self.Multiplier = Fix64()
        self.Addend = Fix64()
        from Mafi import Fix32
        self.FrequencyMult = Fix32()

class NoiseTurbulence:
    def __init__(self):
        from Mafi import Fix32
        self.MeanValue = Fix32()
        self.Amplitude = Fix32()
        self.Period = Fix32()
        self.BaseNoise = None
        self.Seed = None
        self.TurbulenceParams = None

class NoiseTurbulenceParams:
    def __init__(self):
        self.OctavesCount = 0
        self.Lacunarity = None
        self.Persistence = None

class PointDistanceNoise:
    def __init__(self):
        from Mafi import Fix32
        self.MeanValue = Fix32()
        self.Amplitude = Fix32()
        self.Period = Fix32()

class PolygonDistanceNoise:
    def __init__(self):
        from Mafi import Fix32
        self.MeanValue = Fix32()
        self.Amplitude = Fix32()
        self.Period = Fix32()

class PolygonSignedDistanceNoise:
    def __init__(self):
        from Mafi import Fix32
        self.MeanValue = Fix32()
        self.Amplitude = Fix32()
        self.Period = Fix32()

class RidgedNoise2D:
    def __init__(self):
        from Mafi import Fix32
        self.MeanValue = Fix32()
        self.Amplitude = Fix32()
        self.Period = Fix32()

class SimplexNoise2D:
    def __init__(self):
        from Mafi import Fix32
        self.MeanValue = Fix32()
        self.Amplitude = Fix32()
        self.Period = Fix32()
        self.Seed = None

class SimplexNoise2dParams:
    def __init__(self):
        from Mafi import Fix32
        self.MeanValue = Fix32()
        self.Amplitude = Fix32()
        self.Period = Fix32()

class SimplexNoise2dSeed:
    Invalid = None
    def __init__(self):
        self.Vector2f = None
        self.IsValid = False
        from Mafi import Fix32
        self.SeedX = Fix32()
        self.SeedY = Fix32()

class SimplexNoise2dSeed64:
    Invalid = None
    def __init__(self):
        self.IsValid = False
        from Mafi import Fix64
        self.SeedX = Fix64()
        self.SeedY = Fix64()

class SimplexNoise2dV2:
    def __init__(self):
        from Mafi import Fix32
        self.MeanValue = Fix32()
        self.Amplitude = Fix32()
        self.Period = Fix32()
        self.Seed = None

class SoftCapNoise2D:
    def __init__(self):
        from Mafi import Fix32
        self.MeanValue = Fix32()
        self.Amplitude = Fix32()
        self.Period = Fix32()
        self.Noise = None
        self.Parameters = None

class SoftCapNoiseParams:
    def __init__(self):
        from Mafi import Fix32
        self.CapStart = Fix32()
        self.CapEnd = Fix32()

class SteppedNoise2D:
    def __init__(self):
        from Mafi import Fix32
        self.MeanValue = Fix32()
        self.Amplitude = Fix32()
        self.Period = Fix32()
        self.Parameters = None

class SteppedNoiseParams:
    def __init__(self):
        from Mafi import Fix32
        self.StepSize = Fix32()
        self.StepSteepness = Fix32()

class SumOp:
    def __init__(self):
        from Mafi import Fix32
        self.MeanValue = Fix32()
        self.Amplitude = Fix32()
        self.Period = Fix32()

class WarpCoordsNoise:
    def __init__(self):
        from Mafi import Fix32
        self.MeanValue = Fix32()
        self.Amplitude = Fix32()
        self.Period = Fix32()

class ExpBendNoise2DExtensions:
    def __init__(self):
        pass


class INoise2D:
    def __init__(self):
        from Mafi import Fix32
        self.MeanValue = Fix32()
        self.Amplitude = Fix32()
        self.Period = Fix32()

class INoise2DExtensions:
    def __init__(self):
        pass


class INoise3D:
    def __init__(self):
        self.MeanValue = 0.0
        self.Amplitude = 0.0
        self.Period = 0.0

class LineTransitionFn:
    Linear = None
    Sine = None
    def __init__(self):
        self.value__ = 0

class NoiseBinaryOpExtensions:
    def __init__(self):
        pass


class NoisesAccumulatprOpExtensions:
    def __init__(self):
        pass


class NoiseTransform2DExtensions:
    def __init__(self):
        pass


class NoiseTurbulenceExtensions:
    def __init__(self):
        pass


class RidgedNoise2DExtensions:
    def __init__(self):
        pass


class SimplexNoise2dSeedExtensions:
    def __init__(self):
        pass


class SimplexNoise2dSeedV2Extensions:
    def __init__(self):
        pass


class SoftCapNoise2DExtensions:
    def __init__(self):
        pass


class SteppedNoise2DExtensions:
    def __init__(self):
        pass


class DisplaceCoordsNoise2DExtensions:
    def __init__(self):
        pass

