
class ArraysPoolExtensions:
    def __init__(self):
        pass


class ArrayAssertionExtensions:
    def __init__(self):
        pass


class Assert:
    ASSERT_CONDITIONAL = ""
    ASSERT_CONDITIONAL_DEBUG_ONLY = ""
    ENABLED = False
    def __init__(self):
        pass


class AssertionExtensions:
    def __init__(self):
        pass


class BoolAssertionExtensions:
    def __init__(self):
        pass


class CheckExtensions:
    def __init__(self):
        pass


class CheckException:
    def __init__(self):
        self.Message = ""
        self.Data = None
        self.InnerException = None
        self.TargetSite = None
        self.StackTrace = ""
        self.HelpLink = ""
        self.Source = ""
        self.HResult = 0

class DictAssertionExtensions:
    def __init__(self):
        pass


class Fix64AssertionAndChecksExtensions:
    def __init__(self):
        pass


class DoubleAssertionAndChecksExtensions:
    def __init__(self):
        pass


class Fix32AssertionAndChecksExtensions:
    def __init__(self):
        pass


class FloatAssertionAndChecksExtensions:
    def __init__(self):
        pass


class IntAssertionAndChecksExtensions:
    def __init__(self):
        pass


class LongAssertionAndChecksExtensions:
    def __init__(self):
        pass


class PercentAssertionAndChecksExtensions:
    def __init__(self):
        pass


class UintAssertionAndChecksExtensions:
    def __init__(self):
        pass


class GenericAssertionExtensions:
    def __init__(self):
        pass


class HashSetAssertionExtensions:
    def __init__(self):
        pass


class IEnumerableAssertionExtensions:
    def __init__(self):
        pass


class IIndexableAssertions:
    def __init__(self):
        pass


class ImmutableArrayAssertionExtensions:
    def __init__(self):
        pass


class IntAssertionExtensions:
    def __init__(self):
        pass


class ListAssertionExtensions:
    def __init__(self):
        pass


class LystAssertionExtensions:
    def __init__(self):
        pass


class OptionAssertionExtensions:
    def __init__(self):
        pass


class QueueueAssertionExtensions:
    def __init__(self):
        pass


class ReadOnlyDictionaryAssertionExtensions:
    def __init__(self):
        pass


class SetAssertionExtensions:
    def __init__(self):
        pass


class SmallImmutableArrayAssertionExtensions:
    def __init__(self):
        pass


class StringAssertionExtensions:
    def __init__(self):
        pass


class TypeAssertionExtensions:
    def __init__(self):
        pass


class BuildInfoData:
    def __init__(self):
        self.IsDebug = False
        self.IsDevOnly = False
        self.IsAlphaOnly = False
        self.AssertionsEnabled = False
        self.TracingEnabled = False

class BuildInfo:
    Data = None
    COUNT = 0
    IS_DEBUG = False
    IS_DEV_ONLY = False
    IS_ALPHA_ONLY = False
    CHEATS_ENABLED = False
    SHORT_DURATION_BUILD = False
    def __init__(self):
        pass


class IIndexableExtensions:
    def __init__(self):
        pass


class ReadOnlyArrayAssertionExtensions:
    def __init__(self):
        pass


class IResolver:
    def __init__(self):
        pass


class DependencyResolver:
    def __init__(self):
        self.ResolvedObjects = None

class DependencyResolverException:
    def __init__(self):
        self.Message = ""
        self.Data = None
        self.InnerException = None
        self.TargetSite = None
        self.StackTrace = ""
        self.HelpLink = ""
        self.Source = ""
        self.HResult = 0

class DependencyResolverBuilder:
    def __init__(self):
        pass


    class DependencyRegistrar:
        def __init__(self):
            pass


class GenericDependencyImplementations:
    def __init__(self):
        self.DependencyType = None

    class GenericImplementation:
        def __init__(self):
            self.ImplementationType = None
            self.InheritedType = None

class GlobalDependencyAttribute:
    def __init__(self):
        self.TypeId = None
        self.RegistrationMode = None
        self.OnlyInDebug = False
        self.OnlyInDevOnly = False

class NotGlobalDependencyAttribute:
    def __init__(self):
        self.TypeId = None

class DependencyRegisteredManuallyAttribute:
    def __init__(self):
        self.TypeId = None
        self.Reason = ""

class RegistrationMode:
    AsSelf = None
    AsAllInterfaces = None
    AsEverything = None
    def __init__(self):
        self.value__ = 0

class MultiDependencyAttribute:
    def __init__(self):
        self.TypeId = None

class EnumeratorExtensions:
    def __init__(self):
        pass


class Event:
    def __init__(self):
        self.PrimaryThread = None

class EventNonSaveable:
    def __init__(self):
        self.RegisteredEventsCount = 0

class ArrayExtensions:
    def __init__(self):
        pass


class BoolExtensions:
    def __init__(self):
        pass


class CollectionsExtensions:
    def __init__(self):
        pass


class EnumerableExtensions:
    def __init__(self):
        pass


class FatalGameException:
    def __init__(self):
        self.Message = ""
        self.Data = None
        self.InnerException = None
        self.TargetSite = None
        self.StackTrace = ""
        self.HelpLink = ""
        self.Source = ""
        self.HResult = 0

class FloatExtensions:
    def __init__(self):
        pass


class IntExtensions:
    def __init__(self):
        pass


class LongExtensions:
    def __init__(self):
        pass


class IReadOnlyDictExtensions:
    def __init__(self):
        pass


class LinqExtensions:
    def __init__(self):
        pass


class DataStats:
    def __init__(self):
        self.Min = 0.0
        self.Max = 0.0
        self.Sum = 0.0
        self.Mean = 0.0
        self.Median = 0.0
        self.Count = 0.0
        self.StdDev = 0.0

class MeanAndStdDev:
    def __init__(self):
        self.Mean = 0.0
        self.StdDev = 0.0

class ObjectExtensions:
    def __init__(self):
        pass


class SetExtensions:
    def __init__(self):
        pass


class StreamExtensions:
    def __init__(self):
        pass


class StringExtensions:
    def __init__(self):
        pass


class TypeExtensions:
    def __init__(self):
        pass


class Fix32:
    from Mafi import Fix32
    Zero = Fix32()
    One = Fix32()
    Quarter = Fix32()
    Half = Fix32()
    Two = Fix32()
    Three = Fix32()
    Four = Fix32()
    Eight = Fix32()
    Epsilon = Fix32()
    EpsilonNear = Fix32()
    MinValue = Fix32()
    MaxValue = Fix32()
    MinIntValue = Fix32()
    MaxIntValue = Fix32()
    Tau = Fix32()
    Sqrt2 = Fix32()
    Sqrt3 = Fix32()
    Sqrt5 = Fix32()
    OneOverSqrt2 = Fix32()
    OneOverSqrt3 = Fix32()
    OneOverSqrt5 = Fix32()
    FRACTIONAL_BITS = 0
    FRACTION_RANGE = 0
    MAX_DIGITS_PRECISION = 0
    def __init__(self):
        self.IsZero = False
        self.IsNotZero = False
        self.IsPositive = False
        self.IsNotPositive = False
        self.IsNegative = False
        self.IsNotNegative = False
        self.IsInteger = False
        self.IntegerPart = 0
        self.IntegerPartNonNegative = 0
        from Mafi import Fix32
        self.FractionalPart = Fix32()
        self.FractionalPartNonNegative = Fix32()
        self.HalfFast = Fix32()
        self.DivBy4Fast = Fix32()
        self.DivBy8Fast = Fix32()
        self.DivBy16Fast = Fix32()
        self.DivBy32Fast = Fix32()
        self.DivBy64Fast = Fix32()
        self.Times2Fast = Fix32()
        self.Times4Fast = Fix32()
        self.Times5Fast = Fix32()
        self.Times8Fast = Fix32()
        self.Times12Fast = Fix32()
        self.RawValue = 0

class Fix64:
    from Mafi import Fix64
    Zero = Fix64()
    Half = Fix64()
    One = Fix64()
    Two = Fix64()
    Three = Fix64()
    Four = Fix64()
    Eight = Fix64()
    Epsilon = Fix64()
    EpsilonNear = Fix64()
    MinValue = Fix64()
    MaxValue = Fix64()
    MinIntValue = Fix64()
    MaxIntValue = Fix64()
    EpsilonFix32NearOneSqr = Fix64()
    Tau = Fix64()
    TauOver4 = Fix64()
    Sqrt2 = Fix64()
    OneOverSqrt2 = Fix64()
    MAX_DIGITS_PRECISION = 0
    def __init__(self):
        self.IsZero = False
        self.IsNotZero = False
        self.IsPositive = False
        self.IsNotPositive = False
        self.IsNegative = False
        self.IsNotNegative = False
        self.IsInteger = False
        self.IntegerPart = 0
        from Mafi import Fix64
        self.FractionalPart = Fix64()
        self.HalfFast = Fix64()
        self.Times2Fast = Fix64()
        self.Times4Fast = Fix64()
        self.Times8Fast = Fix64()
        self.RawValue = 0

class Percent:
    Zero = None
    Epsilon = None
    One = None
    Hundred = None
    Fifty = None
    MinValue = None
    MaxValue = None
    Tau = None
    def __init__(self):
        self.IsZero = False
        self.IsNotZero = False
        self.IsNearHundred = False
        self.IsPositive = False
        self.IsNotPositive = False
        self.IsNegative = False
        self.IsNotNegative = False
        self.IsWithin0To100PercIncl = False
        self.IntegerPart = 0
        self.FractionalPart = None
        self.HalfFast = None
        self.TimesTwoFast = None
        self.RawValue = 0

class RngSeed64:
    def __init__(self):
        self.Value = None

class Hash:
    def __init__(self):
        pass


class IEventOwner:
    def __init__(self):
        self.IsDestroyed = False

class IEvent:
    def __init__(self):
        pass


class IEventNonSaveable:
    def __init__(self):
        pass


class TgaImageUtils:
    def __init__(self):
        pass


class Rgb:
    def __init__(self):
        self.R = None
        self.G = None
        self.B = None

class Log:
    AcceptedLogTypes = None
    DebugInfos = None
    LOG_FILE_RETENTION_DAYS = 0
    def __init__(self):
        pass


class ISimStepProvider:
    def __init__(self):
        self.CurrentSimStep = 0
        self.IsTerminated = False

class MafiMath:
    from Mafi import Fix32
    ONE_MINUS_TAU_OVER_4 = Fix32()
    ONE_MINUS_TAU_OVER_2 = Fix32()
    TAU_OVER_2_MINUS_2 = Fix32()
    DEFAULT_FLOAT_TOLERANCE = 0.0
    TAU_D = None
    SQRT2 = None
    TAU = 0.0
    DEG_2_RAD = 0.0
    RAD_2_DEG = 0.0
    RAD_2_DEG_D = None
    def __init__(self):
        pass


class CubicCoeffs:
    def __init__(self):
        from Mafi import Fix32
        self.A = Fix32()
        self.B = Fix32()
        self.V1 = Fix32()
        self.V20 = Fix32()

class MonotoneCubicCoeffs:
    def __init__(self):
        from Mafi import Fix32
        self.V1 = Fix32()
        from Mafi import Fix64
        self.C1 = Fix64()
        self.C2 = Fix64()
        self.C3 = Fix64()

class Make:
    def __init__(self):
        pass


class Aabb:
    def __init__(self):
        self.IsValid = False
        self.Size = None
        self.Min = None
        self.Max = None

class AngleDegrees1f:
    Zero = None
    MinValue = None
    MaxValue = None
    Epsilon = None
    HalfDegree = None
    OneDegree = None
    Deg90 = None
    Deg179 = None
    Deg180 = None
    Deg270 = None
    Deg360 = None
    def __init__(self):
        from Mafi import Fix32
        self.Radians = Fix32()
        self.Normalized = None
        self.DirectionVector = None
        self.Rotated180Deg = None
        self.Abs = None
        self.Sign = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IsPositive = False
        self.IsNotPositive = False
        self.IsNegative = False
        self.IsNotNegative = False
        from Mafi import Fix64
        self.Squared = Fix64()
        self.Degrees = Fix32()

class AngleDegrees1fExtensions:
    def __init__(self):
        pass


class AngleSlim:
    Zero = None
    MinValue = None
    MaxValue = None
    def __init__(self):
        from Mafi import Fix32
        self.Degrees = Fix32()
        self.RadiansAsFloat = 0.0
        self.IsZero = False
        self.IsNotZero = False
        self.IsPositive = False
        self.IsNotPositive = False
        self.IsNegative = False
        self.IsNotNegative = False
        self.Squared = 0
        self.Raw = None

class ColorRgba:
    Black = None
    DarkDarkGray = None
    DarkGray = None
    Gray = None
    LightGray = None
    White = None
    Red = None
    DarkRed = None
    Brown = None
    Orange = None
    Gold = None
    Yellow = None
    LightYellow = None
    DarkYellow = None
    GreenYellow = None
    Green = None
    DarkGreen = None
    Turquoise = None
    Cyan = None
    Blue = None
    LightBlue = None
    CornflowerBlue = None
    Magenta = None
    Purple = None
    Empty = None
    def __init__(self):
        self.R = None
        self.G = None
        self.B = None
        self.A = None
        self.IsEmpty = False
        self.IsNotEmpty = False
        self.Rgba = None

class ColorRgbaExtensions:
    def __init__(self):
        pass


class ColorUniversal:
    def __init__(self):
        self.Rgba = None
        self.R = 0.0
        self.G = 0.0
        self.B = 0.0
        self.A = 0.0
        from Mafi import Fix32
        self.H = Fix32()
        self.S = Fix32()
        self.L = Fix32()

class Direction90:
    PlusX = None
    PlusY = None
    MinusX = None
    MinusY = None
    AllFourDirections = None
    PlusMinusX = None
    PlusMinusY = None
    PLUS_X_INDEX = 0
    PLUS_Y_INDEX = 0
    MINUS_X_INDEX = 0
    MINUS_Y_INDEX = 0
    def __init__(self):
        self.DirectionVector = None
        self.RotatedPlus90 = None
        self.RotatedMinus90 = None
        self.Rotated180 = None
        self.As3d = None
        self.DirectionIndex = 0

class Direction903d:
    PlusX = None
    PlusY = None
    PlusZ = None
    MinusX = None
    MinusY = None
    MinusZ = None
    AllSixDirections = None
    PlusMinusX = None
    PlusMinusY = None
    PlusMinusZ = None
    PLUS_X_INDEX = 0
    PLUS_Y_INDEX = 0
    PLUS_Z_INDEX = 0
    MINUS_X_INDEX = 0
    MINUS_Y_INDEX = 0
    MINUS_Z_INDEX = 0
    def __init__(self):
        self.DirectionVector = None
        self.DirectionIndex = 0

class Fix32Extensions:
    def __init__(self):
        pass


class Fix64Extensions:
    def __init__(self):
        pass


class NeighborCoord:
    All4Neighbors = None
    PlusX = None
    MinusX = None
    PlusY = None
    MinusY = None
    PLUS_X_INDEX = 0
    MINUS_X_INDEX = 0
    PLUS_Y_INDEX = 0
    MINUS_Y_INDEX = 0
    def __init__(self):
        self.Dx = 0
        self.Dy = 0
        self.Opposite = None
        self.Left = None
        self.Right = None
        self.Index = 0

class Px:
    Zero = None
    MinValue = None
    MaxValue = None
    Auto = None
    POINTS_MULTIPLIER = 0
    def __init__(self):
        self.Abs = None
        self.Sign = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IsPositive = False
        self.IsNotPositive = False
        self.IsNegative = False
        self.IsNotNegative = False
        self.Squared = 0.0
        self.Pixels = 0.0

class Rotation90:
    Deg0 = None
    Deg90 = None
    Deg180 = None
    Deg270 = None
    AllRotations = None
    DEG_0_INDEX = 0
    DEG_90_INDEX = 0
    DEG_180_INDEX = 0
    DEG_270_INDEX = 0
    def __init__(self):
        self.Dx = 0
        self.Dy = 0
        self.Is90Or270Deg = False
        self.RotatedPlus90 = None
        self.RotatedMinus90 = None
        self.Rotated180 = None
        self.DirectionVector = None
        self.Angle = None
        self.Quaternion = None
        self.AngleIndex = 0

class Vector2f:
    Zero = None
    One = None
    UnitX = None
    UnitY = None
    MinValue = None
    MaxValue = None
    def __init__(self):
        from Mafi import Fix32
        self.Sum = Fix32()
        from Mafi import Fix64
        self.Product = Fix64()
        self.Length = Fix32()
        self.LengthSqr = Fix64()
        self.IsZero = False
        self.IsNotZero = False
        self.IncrementX = None
        self.IncrementY = None
        self.DecrementX = None
        self.DecrementY = None
        self.ReflectX = None
        self.ReflectY = None
        self.RoundedVector2i = None
        self.CeiledVector2i = None
        self.FlooredVector2i = None
        self.IsNormalized = False
        self.Normalized = None
        self.Angle = None
        self.LeftOrthogonalVector = None
        self.RightOrthogonalVector = None
        self.AbsValue = None
        self.Signs = None
        self.Times2Fast = None
        self.Times4Fast = None
        self.Times8Fast = None
        self.HalfFast = None
        self.DivBy4Fast = None
        self.DivBy8Fast = None
        self.DivBy16Fast = None
        self.Yx = None
        self.DotSelf = Fix64()
        self.X = Fix32()
        self.Y = Fix32()

class Vector2fExtensions:
    def __init__(self):
        pass


class Vector2fAssertions:
    def __init__(self):
        pass


class Vector2i:
    Zero = None
    One = None
    UnitX = None
    UnitY = None
    MinValue = None
    MaxValue = None
    def __init__(self):
        self.Sum = 0
        self.Product = 0
        self.ProductInt = 0
        from Mafi import Fix32
        self.Length = Fix32()
        self.LengthInt = 0
        self.LengthSqrInt = 0
        self.LengthSqr = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IncrementX = None
        self.IncrementY = None
        self.DecrementX = None
        self.DecrementY = None
        self.ReflectX = None
        self.ReflectY = None
        self.Vector2f = None
        self.Angle = None
        self.LeftOrthogonalVector = None
        self.RightOrthogonalVector = None
        self.AbsValue = None
        self.Signs = None
        self.Times2Fast = None
        self.Times4Fast = None
        self.Times8Fast = None
        self.HalfFast = None
        self.DivBy4Fast = None
        self.DivBy8Fast = None
        self.DivBy16Fast = None
        self.Yx = None
        self.AnglePositive = None
        self.Vector2fUnclamped = None
        self.X = 0
        self.Y = 0

class Vector2iExtensions:
    def __init__(self):
        pass


class Vector2iAssertions:
    def __init__(self):
        pass


class Vector3f:
    Zero = None
    One = None
    UnitX = None
    UnitY = None
    UnitZ = None
    MinValue = None
    MaxValue = None
    def __init__(self):
        self.Xy = None
        from Mafi import Fix32
        self.Sum = Fix32()
        self.Length = Fix32()
        from Mafi import Fix64
        self.LengthSqr = Fix64()
        self.IsZero = False
        self.IsNotZero = False
        self.IncrementX = None
        self.IncrementY = None
        self.IncrementZ = None
        self.DecrementX = None
        self.DecrementY = None
        self.DecrementZ = None
        self.ReflectX = None
        self.ReflectY = None
        self.ReflectZ = None
        self.RoundedVector3i = None
        self.CeiledVector3i = None
        self.FlooredVector3i = None
        self.IsNormalized = False
        self.Normalized = None
        self.Angle = None
        self.AbsValue = None
        self.Signs = None
        self.Times2Fast = None
        self.Times4Fast = None
        self.Times8Fast = None
        self.HalfFast = None
        self.DivBy4Fast = None
        self.DivBy8Fast = None
        self.DivBy16Fast = None
        self.Orthogonal = None
        self.LengthSqrAsFix32 = Fix32()
        self.X = Fix32()
        self.Y = Fix32()
        self.Z = Fix32()

class Vector3fExtensions:
    def __init__(self):
        pass


class Vector3fAssertions:
    def __init__(self):
        pass


class Vector3i:
    Zero = None
    One = None
    UnitX = None
    UnitY = None
    UnitZ = None
    MinValue = None
    MaxValue = None
    def __init__(self):
        self.Xy = None
        self.Sum = 0
        from Mafi import Fix32
        self.Length = Fix32()
        self.LengthInt = 0
        self.LengthSqrInt = 0
        self.LengthSqr = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IncrementX = None
        self.IncrementY = None
        self.IncrementZ = None
        self.DecrementX = None
        self.DecrementY = None
        self.DecrementZ = None
        self.ReflectX = None
        self.ReflectY = None
        self.ReflectZ = None
        self.Vector3f = None
        self.Angle = None
        self.AbsValue = None
        self.Signs = None
        self.Times2Fast = None
        self.Times4Fast = None
        self.Times8Fast = None
        self.HalfFast = None
        self.DivBy4Fast = None
        self.DivBy8Fast = None
        self.DivBy16Fast = None
        self.SortedComponents = None
        self.X = 0
        self.Y = 0
        self.Z = 0

class Vector3iExtensions:
    def __init__(self):
        pass


class Vector3iAssertions:
    def __init__(self):
        pass


class Vector4f:
    Zero = None
    One = None
    UnitX = None
    UnitY = None
    UnitZ = None
    UnitW = None
    MinValue = None
    MaxValue = None
    def __init__(self):
        self.Xy = None
        self.Xyz = None
        from Mafi import Fix32
        self.Sum = Fix32()
        self.Length = Fix32()
        from Mafi import Fix64
        self.LengthSqr = Fix64()
        self.IsZero = False
        self.IsNotZero = False
        self.IncrementX = None
        self.IncrementY = None
        self.IncrementZ = None
        self.IncrementW = None
        self.DecrementX = None
        self.DecrementY = None
        self.DecrementZ = None
        self.DecrementW = None
        self.ReflectX = None
        self.ReflectY = None
        self.ReflectZ = None
        self.ReflectW = None
        self.RoundedVector4i = None
        self.CeiledVector4i = None
        self.FlooredVector4i = None
        self.IsNormalized = False
        self.Normalized = None
        self.Angle = None
        self.AbsValue = None
        self.Signs = None
        self.Times2Fast = None
        self.Times4Fast = None
        self.Times8Fast = None
        self.HalfFast = None
        self.DivBy4Fast = None
        self.DivBy8Fast = None
        self.DivBy16Fast = None
        self.X = Fix32()
        self.Y = Fix32()
        self.Z = Fix32()
        self.W = Fix32()

class Vector4fExtensions:
    def __init__(self):
        pass


class Vector4fAssertions:
    def __init__(self):
        pass


class Vector4i:
    Zero = None
    One = None
    UnitX = None
    UnitY = None
    UnitZ = None
    UnitW = None
    MinValue = None
    MaxValue = None
    def __init__(self):
        self.Xy = None
        self.Xyz = None
        self.Sum = 0
        from Mafi import Fix32
        self.Length = Fix32()
        self.LengthInt = 0
        self.LengthSqrInt = 0
        self.LengthSqr = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IncrementX = None
        self.IncrementY = None
        self.IncrementZ = None
        self.IncrementW = None
        self.DecrementX = None
        self.DecrementY = None
        self.DecrementZ = None
        self.DecrementW = None
        self.ReflectX = None
        self.ReflectY = None
        self.ReflectZ = None
        self.ReflectW = None
        self.Vector4f = None
        self.Angle = None
        self.AbsValue = None
        self.Signs = None
        self.Times2Fast = None
        self.Times4Fast = None
        self.Times8Fast = None
        self.HalfFast = None
        self.DivBy4Fast = None
        self.DivBy8Fast = None
        self.DivBy16Fast = None
        self.X = 0
        self.Y = 0
        self.Z = 0
        self.W = 0

class Vector4iExtensions:
    def __init__(self):
        pass


class Vector4iAssertions:
    def __init__(self):
        pass


class Matrix2i:
    Zero = None
    Identity = None
    def __init__(self):
        self.M00 = 0
        self.M01 = 0
        self.M10 = 0
        self.M11 = 0

class PercentExtensions:
    def __init__(self):
        pass


class PxExtensions:
    def __init__(self):
        pass


class Ray3f:
    def __init__(self):
        self.Origin = None
        self.Direction = None

class Ray2f:
    def __init__(self):
        self.Origin = None
        self.Direction = None

class Vector2fExts:
    def __init__(self):
        pass


class Vector2fAssertionExtensions:
    def __init__(self):
        pass


class Option:
    from Mafi import Option
    None = Option()
    def __init__(self):
        pass


class OptionExtensions:
    def __init__(self):
        pass


class OptionPlus:
    from Mafi import Option
    None = Option()
    def __init__(self):
        pass


class GameVersion:
    FULL_DISPLAY_VALUE = None
    NAME = ""
    MAJOR_VERSION = ""
    MINOR_VERSION = ""
    REVISION_VERSION = ""
    HOTFIX_NAME = ""
    FULL_VERSION = ""
    UNKNOWN_VERSION_NAME = ""
    def __init__(self):
        pass


class AssemblyInfo:
    TITLE = ""
    VERSION = ""
    VERSION_FULL = ""
    COMPANY = ""
    PRODUCT = ""
    COPYRIGHT = ""
    BUILD_TIMESTAMP_UTC_BINARY = 0
    BUILD_ID = None
    def __init__(self):
        pass


class IRandom:
    def __init__(self):
        pass


class IRandomExtensions:
    def __init__(self):
        pass


class CanBeNullAttribute:
    def __init__(self):
        self.TypeId = None

class NotNullAttribute:
    def __init__(self):
        self.TypeId = None

class ItemNotNullAttribute:
    def __init__(self):
        self.TypeId = None

class ItemCanBeNullAttribute:
    def __init__(self):
        self.TypeId = None

class StringFormatMethodAttribute:
    def __init__(self):
        self.FormatParameterName = ""
        self.TypeId = None

class ValueProviderAttribute:
    def __init__(self):
        self.Name = ""
        self.TypeId = None

class InvokerParameterNameAttribute:
    def __init__(self):
        self.TypeId = None

class NotifyPropertyChangedInvocatorAttribute:
    def __init__(self):
        self.ParameterName = ""
        self.TypeId = None

class ContractAnnotationAttribute:
    def __init__(self):
        self.Contract = ""
        self.ForceFullStates = False
        self.TypeId = None

class LocalizationRequiredAttribute:
    def __init__(self):
        self.Required = False
        self.TypeId = None

class CannotApplyEqualityOperatorAttribute:
    def __init__(self):
        self.TypeId = None

class BaseTypeRequiredAttribute:
    def __init__(self):
        self.BaseType = None
        self.TypeId = None

class UsedImplicitlyAttribute:
    def __init__(self):
        self.UseKindFlags = None
        self.TargetFlags = None
        self.TypeId = None

class MeansImplicitUseAttribute:
    def __init__(self):
        self.UseKindFlags = None
        self.TargetFlags = None
        self.TypeId = None

class ImplicitUseKindFlags:
    Default = None
    Access = None
    Assign = None
    InstantiatedWithFixedConstructorSignature = None
    InstantiatedNoFixedConstructorSignature = None
    def __init__(self):
        self.value__ = 0

class ImplicitUseTargetFlags:
    Default = None
    Itself = None
    Members = None
    WithMembers = None
    def __init__(self):
        self.value__ = 0

class PublicAPIAttribute:
    def __init__(self):
        self.Comment = ""
        self.TypeId = None

class InstantHandleAttribute:
    def __init__(self):
        self.TypeId = None

class PureAttribute:
    def __init__(self):
        self.TypeId = None

class MustUseReturnValueAttribute:
    def __init__(self):
        self.Justification = ""
        self.TypeId = None

class ProvidesContextAttribute:
    def __init__(self):
        self.TypeId = None

class PathReferenceAttribute:
    def __init__(self):
        self.BasePath = ""
        self.TypeId = None

class SourceTemplateAttribute:
    def __init__(self):
        self.TypeId = None

class MacroAttribute:
    def __init__(self):
        self.Expression = ""
        self.Editable = 0
        self.Target = ""
        self.TypeId = None

class AspMvcAreaMasterLocationFormatAttribute:
    def __init__(self):
        self.Format = ""
        self.TypeId = None

class AspMvcAreaPartialViewLocationFormatAttribute:
    def __init__(self):
        self.Format = ""
        self.TypeId = None

class AspMvcAreaViewLocationFormatAttribute:
    def __init__(self):
        self.Format = ""
        self.TypeId = None

class AspMvcMasterLocationFormatAttribute:
    def __init__(self):
        self.Format = ""
        self.TypeId = None

class AspMvcPartialViewLocationFormatAttribute:
    def __init__(self):
        self.Format = ""
        self.TypeId = None

class AspMvcViewLocationFormatAttribute:
    def __init__(self):
        self.Format = ""
        self.TypeId = None

class AspMvcActionAttribute:
    def __init__(self):
        self.AnonymousProperty = ""
        self.TypeId = None

class AspMvcAreaAttribute:
    def __init__(self):
        self.AnonymousProperty = ""
        self.TypeId = None

class AspMvcControllerAttribute:
    def __init__(self):
        self.AnonymousProperty = ""
        self.TypeId = None

class AspMvcMasterAttribute:
    def __init__(self):
        self.TypeId = None

class AspMvcModelTypeAttribute:
    def __init__(self):
        self.TypeId = None

class AspMvcPartialViewAttribute:
    def __init__(self):
        self.TypeId = None

class AspMvcSuppressViewErrorAttribute:
    def __init__(self):
        self.TypeId = None

class AspMvcDisplayTemplateAttribute:
    def __init__(self):
        self.TypeId = None

class AspMvcEditorTemplateAttribute:
    def __init__(self):
        self.TypeId = None

class AspMvcTemplateAttribute:
    def __init__(self):
        self.TypeId = None

class AspMvcViewAttribute:
    def __init__(self):
        self.TypeId = None

class AspMvcViewComponentAttribute:
    def __init__(self):
        self.TypeId = None

class AspMvcViewComponentViewAttribute:
    def __init__(self):
        self.TypeId = None

class AspMvcActionSelectorAttribute:
    def __init__(self):
        self.TypeId = None

class HtmlElementAttributesAttribute:
    def __init__(self):
        self.Name = ""
        self.TypeId = None

class HtmlAttributeValueAttribute:
    def __init__(self):
        self.Name = ""
        self.TypeId = None

class RazorSectionAttribute:
    def __init__(self):
        self.TypeId = None

class CollectionAccessAttribute:
    def __init__(self):
        self.CollectionAccessType = None
        self.TypeId = None

class CollectionAccessType:
    None = None
    Read = None
    ModifyExistingContent = None
    UpdatedContent = None
    def __init__(self):
        self.value__ = 0

class AssertionMethodAttribute:
    def __init__(self):
        self.TypeId = None

class AssertionConditionAttribute:
    def __init__(self):
        self.ConditionType = None
        self.TypeId = None

class AssertionConditionType:
    IS_TRUE = None
    IS_FALSE = None
    IS_NULL = None
    IS_NOT_NULL = None
    def __init__(self):
        self.value__ = 0

class TerminatesProgramAttribute:
    def __init__(self):
        self.TypeId = None

class LinqTunnelAttribute:
    def __init__(self):
        self.TypeId = None

class NoEnumerationAttribute:
    def __init__(self):
        self.TypeId = None

class RegexPatternAttribute:
    def __init__(self):
        self.TypeId = None

class NoReorderAttribute:
    def __init__(self):
        self.TypeId = None

class XamlItemsControlAttribute:
    def __init__(self):
        self.TypeId = None

class XamlItemBindingOfItemsControlAttribute:
    def __init__(self):
        self.TypeId = None

class AspChildControlTypeAttribute:
    def __init__(self):
        self.TagName = ""
        self.ControlType = None
        self.TypeId = None

class AspDataFieldAttribute:
    def __init__(self):
        self.TypeId = None

class AspDataFieldsAttribute:
    def __init__(self):
        self.TypeId = None

class AspMethodPropertyAttribute:
    def __init__(self):
        self.TypeId = None

class AspRequiredAttributeAttribute:
    def __init__(self):
        self.Attribute = ""
        self.TypeId = None

class AspTypePropertyAttribute:
    def __init__(self):
        self.CreateConstructorReferences = False
        self.TypeId = None

class RazorImportNamespaceAttribute:
    def __init__(self):
        self.Name = ""
        self.TypeId = None

class RazorInjectionAttribute:
    def __init__(self):
        self.Type = ""
        self.FieldName = ""
        self.TypeId = None

class RazorDirectiveAttribute:
    def __init__(self):
        self.Directive = ""
        self.TypeId = None

class RazorPageBaseTypeAttribute:
    def __init__(self):
        self.BaseType = ""
        self.PageName = ""
        self.TypeId = None

class RazorHelperCommonAttribute:
    def __init__(self):
        self.TypeId = None

class RazorLayoutAttribute:
    def __init__(self):
        self.TypeId = None

class RazorWriteLiteralMethodAttribute:
    def __init__(self):
        self.TypeId = None

class RazorWriteMethodAttribute:
    def __init__(self):
        self.TypeId = None

class RazorWriteMethodParameterAttribute:
    def __init__(self):
        self.TypeId = None

class SaveVersion:
    BRANCH_MAP = None
    CURRENT_SAVE_VERSION = 0
    MIN_COMPATIBLE_SAVE_VERSION = 0
    V171_SAVE_SIM_SPEED = 0
    V170_OPTIMIZED_TILE_SERIALIZATION = 0
    V169_MAP_CACHE = 0
    V168_PATHFINDER_3D_GOAL_MIN_DIST = 0
    V167_SAVE_CHECKSUM = 0
    V166_VEHICLE_RECOVERY = 0
    V165_FUEL_NOTIF = 0
    V164_DESIGNATION_STUMP_ADD = 0
    V163_REFUEL_FIX2 = 0
    V162_REFUEL_FIX = 0
    V161_MAP_NAME_LOGGING = 0
    V160_GOAL_TRACKING = 0
    V159_STARTING_LOCATION_ORDER = 0
    V158_FIX_FLOWER_CONFIG = 0
    V157_MAP_SAVE_VERSION = 0
    V156_SORTER_NOTIF = 0
    V155_MAX_REPLACED_DEPTH = 0
    V154_SCREENSHOT_FOG_DENSITY = 0
    V153_SORTER_CHANGES = 0
    V152_TOO_MANY_SHIPS = 0
    V151_EROSION_IGNORE = 0
    V150_PROTECTED_MAP = 0
    V149_REMOVED_UNUSED_PLAYER_CONFIG = 0
    V148_INTERACTION_MODE_FOR_FLATTEN_AND_RAMP = 0
    V147_EROSION_SUPPRESSION_REGIONS = 0
    V146_UPDATE_2_MAP_FIX_PROP_TER_MAT = 0
    V141_UPDATE_2_SURFACE_ON_LOWERED = 0
    V141_UPDATE_2_MAP_TR_ID = 0
    V141_UPDATE_2_INITIAL_MAP_CREATION = 0
    V141_UPDATE_2_BETTER_BEDROCK_IN_MAPS = 0
    V141_UPDATE_2_EXTRA_MAP_STATS = 0
    V140_UPDATE_2 = 0
    V132_REMOVED_FULFILLED_DESIGNATION = 0
    V131_CENTURY_STATS = 0
    V130_TERRAIN_PHYSICS_UPDATE_RAMP = 0
    V129_REACTOR_SIMPLIFICATION = 0
    V128_MACHINE_BUFFER_LEAK = 0
    V127_OFF_LIMITS_DESIGNATIONS = 0
    V126_TERRAIN_MANAGER_FIELDS = 0
    V125_TREE_HARVESTER_UNREACHABLES = 0
    V124_PROPAGATE_NO_PORT_SNAP = 0
    V123_FORESTRY_EVENTS = 0
    V122_EXCAVATOR_FIXES = 0
    V121_TRANSPORT_QUICK_DELIVER = 0
    V120_FARMABLE_MANAGER = 0
    V119_UNREACHABLE_TREE_CHUNKS = 0
    V118_TERRAIN_LOAD_CACHE = 0
    V117_TREE_PLACEMENT_FIX = 0
    V116_NUCLEAR_WASTE_FIX = 0
    V115_CAMERA_SAVE = 0
    V114_CACHE_DUMPING_DESIGNATIONS = 0
    V113_QUICK_DELIVER_FROM_TRUCKS = 0
    V112_FORESTRY_ASSIGN_OUTPUT = 0
    V111_CLONE_CONFIG_ON_REPLACE = 0
    V110_STUCK_EXCAVATOR = 0
    V109_DEPRIORITIZE_FAILED_NAVS = 0
    V108_PRIORITIES_ORDER_FIX = 0
    V107_FORESTRY_STORAGE_ASSIGNMENT = 0
    V106_NAVIGATION_THROTTLING = 0
    V105_NAVIGATION_STRUGGLING = 0
    V104_PRODUCTS_LEAK = 0
    V103_NO_MINING_PREFERRED = 0
    V102_PLANTER_UNREACHABLE = 0
    V101_NOTIFICATOR_MESS = 0
    V100_LANDFILL_STATS = 0
    V99_OCEAN_AREA_NOTIFS = 0
    V98_TREES_AND_STUMPS = 0
    V97_CONTRACTS_CONFIG = 0
    V96_UPDATE1 = 0
    def __init__(self):
        pass


    class MinBranchVersion:
        def __init__(self):
            self.SteamBranchName = ""
            self.MinSaveVersion = 0
            self.LatestGameVersion = ""

class Swap:
    def __init__(self):
        pass


class ThreadUtils:
    ThreadNameFast = ""
    ThreadIdAndNameNameFast = ""
    def __init__(self):
        pass


class Tracing:
    IsEnabled = False
    RecordedEventsCount = 0
    TRACING_CONDITIONAL = ""
    BEGIN_PHASE_NAME = None
    END_PHASE_NAME = None
    INSTANT_PHASE_NAME = None
    IS_AVAILABLE = False
    def __init__(self):
        pass


    class EventRecord:
        def __init__(self):
            self.StartRecord = None
            self.TotalDuration = 0
            self.ChildrenDuration = 0

    class TraceRecord:
        def __init__(self):
            self.Phase = None
            self.Name = ""
            self.Category = ""
            self.ThreadName = ""
            self.Microseconds = 0
            self.ExtraStr = ""

class EditorIgnoreAttribute:
    def __init__(self):
        self.TypeId = None

class EditorSectionAttribute:
    def __init__(self):
        self.TypeId = None
        self.Label = ""
        self.Tooltip = ""
        self.IsHeader = False
        self.CollapsedByDefault = False

class EditorLabelAttribute:
    def __init__(self):
        self.TypeId = None
        self.Label = ""
        self.Tooltip = ""
        self.IsHeader = False
        self.IsError = False

class EditorDropdownAttribute:
    def __init__(self):
        self.TypeId = None
        self.SourceDataMember = ""

class EditorClassNameAttribute:
    def __init__(self):
        self.TypeId = None

class EditorButtonAttribute:
    def __init__(self):
        self.TypeId = None
        from Mafi import Option
        self.ButtonText = Option()
        self.ButtonTooltip = ""
        self.IsPrimary = False
        self.Icon = None

class ObjEditorIcon:
    None = None
    View = None
    Delete = None
    Edit = None
    Clone = None
    Randomize = None
    def __init__(self):
        self.value__ = 0

class EditorEnforceOrderAttribute:
    def __init__(self):
        self.TypeId = None
        self.Order = 0

class EditorCollectionAttribute:
    def __init__(self):
        self.TypeId = None
        self.AllowReorder = False
        self.AllowRemoval = False

class EditorReadonlyAttribute:
    def __init__(self):
        self.TypeId = None

class EditorRebuildIfTrueAttribute:
    def __init__(self):
        self.TypeId = None

class EditorCollapseObjectAttribute:
    def __init__(self):
        self.TypeId = None

class EditorTextAreaAttribute:
    def __init__(self):
        self.TypeId = None
        self.LinesCount = 0
        self.AutoScale = False

class EditorValidationSourceAttribute:
    def __init__(self):
        self.TypeId = None
        self.MemberName = ""

class IEditorValidationAttribute:
    def __init__(self):
        pass


class EditorMaxLengthAttribute:
    def __init__(self):
        self.TypeId = None
        self.MaxLength = 0

class EditorRangePercentAttribute:
    def __init__(self):
        self.TypeId = None

class EditorRangeAttribute:
    def __init__(self):
        self.TypeId = None

class PerfCounter:
    PERF_COUNTERS_ENABLED = False
    def __init__(self):
        pass


class ThreadAssert:
    IsDisabled = False
    def __init__(self):
        pass


class ThreadType:
    Any = None
    Main = None
    Sim = None
    def __init__(self):
        self.value__ = 0

class ThreadNames:
    MAIN = ""
    SIMULATION = ""
    def __init__(self):
        pass


class VisualDebug:
    IsEmpty = False
    Categories = None
    def __init__(self):
        pass


    class IData:
        def __init__(self):
            self.Tiles = None
            self.Spheres = None
            self.Lines = None

    class Data:
        def __init__(self):
            self.Tiles = None
            self.Spheres = None
            self.Lines = None

class AngleDegrees1fAssertionAndChecksExtensions:
    def __init__(self):
        pass


class ComputingAssertionAndChecksExtensions:
    def __init__(self):
        pass


class DurationAssertionAndChecksExtensions:
    def __init__(self):
        pass


class ElectricityAssertionAndChecksExtensions:
    def __init__(self):
        pass


class FrequencyAssertionAndChecksExtensions:
    def __init__(self):
        pass


class GameDateAssertionAndChecksExtensions:
    def __init__(self):
        pass


class HeightTilesFAssertionAndChecksExtensions:
    def __init__(self):
        pass


class HeightTilesIAssertionAndChecksExtensions:
    def __init__(self):
        pass


class MechPowerAssertionAndChecksExtensions:
    def __init__(self):
        pass


class PartialQuantityAssertionAndChecksExtensions:
    def __init__(self):
        pass


class QuantityAssertionAndChecksExtensions:
    def __init__(self):
        pass


class QuantityLargeAssertionAndChecksExtensions:
    def __init__(self):
        pass


class RelGameDateAssertionAndChecksExtensions:
    def __init__(self):
        pass


class RelTile1fAssertionAndChecksExtensions:
    def __init__(self):
        pass


class RelTile1iAssertionAndChecksExtensions:
    def __init__(self):
        pass


class SimStepAssertionAndChecksExtensions:
    def __init__(self):
        pass


class ThicknessTilesFAssertionAndChecksExtensions:
    def __init__(self):
        pass


class ThicknessTilesIAssertionAndChecksExtensions:
    def __init__(self):
        pass


class UpointsAssertionAndChecksExtensions:
    def __init__(self):
        pass


class EntityLayoutExtensions:
    def __init__(self):
        pass


class OceanAreaDrawExtensions:
    def __init__(self):
        pass


class RecipeProtoBuilderExtensions:
    def __init__(self):
        pass


class BattleTriggerPriority:
    Zero = None
    MinValue = None
    MaxValue = None
    Highest = None
    Fleet = None
    Building = None
    Lowest = None
    NotSupported = None
    def __init__(self):
        self.Abs = None
        self.Sign = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IsPositive = False
        self.IsNotPositive = False
        self.IsNegative = False
        self.IsNotNegative = False
        self.Squared = 0
        self.Value = 0

class Chunk256:
    Zero = None
    One = None
    UnitX = None
    UnitY = None
    MinValue = None
    MaxValue = None
    DIMENSION_TILES = 0
    DIMENSION_TILES_BITS = 0
    def __init__(self):
        self.OriginTile2i = None
        self.Sum = 0
        self.Product = 0
        from Mafi import Fix32
        self.Length = Fix32()
        self.LengthInt = 0
        self.LengthSqrInt = 0
        self.LengthSqr = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IncrementX = None
        self.IncrementY = None
        self.DecrementX = None
        self.DecrementY = None
        self.Angle = None
        self.X = None
        self.Y = None

class Chunk2i:
    Zero = None
    One = None
    UnitX = None
    UnitY = None
    MinValue = None
    MaxValue = None
    def __init__(self):
        self.Tile2i = None
        self.Area = None
        self.CenterTile2i = None
        self.PlusXNeighbor = None
        self.MinusXNeighbor = None
        self.PlusYNeighbor = None
        self.MinusYNeighbor = None
        self.MinusXyNeighbor = None
        self.AsPackedUint = None
        self.AsSlim = None
        self.Vector2i = None
        self.Sum = 0
        self.Product = 0
        self.ProductInt = 0
        from Mafi import Fix32
        self.Length = Fix32()
        self.LengthInt = 0
        self.LengthSqrInt = 0
        self.LengthSqr = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IncrementX = None
        self.IncrementY = None
        self.DecrementX = None
        self.DecrementY = None
        self.ReflectX = None
        self.ReflectY = None
        self.Angle = None
        self.LeftOrthogonalVector = None
        self.RightOrthogonalVector = None
        self.AbsValue = None
        self.Signs = None
        self.Times2Fast = None
        self.Times4Fast = None
        self.Times8Fast = None
        self.HalfFast = None
        self.DivBy4Fast = None
        self.DivBy8Fast = None
        self.DivBy16Fast = None
        self.X = 0
        self.Y = 0

class Chunk2iSlim:
    Zero = None
    One = None
    UnitX = None
    UnitY = None
    MinValue = None
    MaxValue = None
    def __init__(self):
        self.AsFull = None
        self.Vector2i = None
        self.Sum = 0
        self.Product = 0
        from Mafi import Fix32
        self.Length = Fix32()
        self.LengthInt = 0
        self.LengthSqrInt = 0
        self.LengthSqr = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IncrementX = None
        self.IncrementY = None
        self.DecrementX = None
        self.DecrementY = None
        self.Angle = None
        self.X = None
        self.Y = None
        self.XyPacked = None

class Computing:
    Zero = None
    MinValue = None
    MaxValue = None
    def __init__(self):
        self.Quantity = None
        self.Abs = None
        self.Sign = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IsPositive = False
        self.IsNotPositive = False
        self.IsNegative = False
        self.IsNotNegative = False
        self.Squared = 0
        self.Value = 0

class Duration:
    Zero = None
    MinValue = None
    MaxValue = None
    OneTick = None
    OneSecond = None
    OneMinute = None
    OneDay = None
    OneMonth = None
    OneYear = None
    TICKS_PER_YEAR = 0
    def __init__(self):
        from Mafi import Fix64
        self.Millis = Fix64()
        self.Seconds = Fix64()
        self.SecondsFloored = 0
        self.Minutes = Fix64()
        from Mafi import Fix32
        self.Days = Fix32()
        self.Months = Fix32()
        self.Years = Fix32()
        self.Abs = None
        self.Sign = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IsPositive = False
        self.IsNotPositive = False
        self.IsNegative = False
        self.IsNotNegative = False
        self.Squared = 0
        self.Ticks = 0

class DurationExtensions:
    def __init__(self):
        pass


class Electricity:
    OneKw = None
    Zero = None
    MinValue = None
    MaxValue = None
    def __init__(self):
        self.Quantity = None
        self.Abs = None
        self.Sign = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IsPositive = False
        self.IsNotPositive = False
        self.IsNegative = False
        self.IsNotNegative = False
        self.Squared = 0
        self.Value = 0

class ElectricityExtensions:
    def __init__(self):
        pass


class Frequency:
    Zero = None
    MinValue = None
    MaxValue = None
    None = None
    EveryTick = None
    EverySecond = None
    def __init__(self):
        self.SecondsFloored = 0
        self.Abs = None
        self.Sign = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IsPositive = False
        self.IsNotPositive = False
        self.IsNegative = False
        self.IsNotNegative = False
        self.Squared = 0
        self.Ticks = 0

class GameDate:
    Zero = None
    MinValue = None
    MaxValue = None
    Inception = None
    FIRST_YEAR_NUMBER = 0
    def __init__(self):
        self.Day = 0
        self.Month = 0
        self.Year = 0
        self.RelGameDate = None
        self.FloorMonths = None
        self.CeilMonths = None
        self.MonthName = ""
        self.Abs = None
        self.Sign = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IsPositive = False
        self.IsNotPositive = False
        self.IsNegative = False
        self.IsNotNegative = False
        self.Squared = 0
        self.Value = 0

class Chunk256Assertions:
    def __init__(self):
        pass


class Chunk256Index:
    def __init__(self):
        self.Value = None

class Chunk2iExtensions:
    def __init__(self):
        pass


class Chunk2iAssertions:
    def __init__(self):
        pass


class Chunk2iSlimAssertions:
    def __init__(self):
        pass


class Chunk64Index:
    def __init__(self):
        self.Value = 0

class Chunk8Index:
    def __init__(self):
        self.Value = 0

class HeightTilesF:
    Zero = None
    MinValue = None
    MaxValue = None
    Epsilon = None
    One = None
    Half = None
    Quarter = None
    def __init__(self):
        self.Abs = None
        self.Sign = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IsPositive = False
        self.IsNotPositive = False
        self.IsNegative = False
        self.IsNotNegative = False
        from Mafi import Fix64
        self.Squared = Fix64()
        self.HeightI = None
        self.TilesHeightCeiled = None
        self.TilesHeightFloored = None
        self.TilesHeightRounded = None
        self.MissingThicknessToTile = None
        from Mafi import Fix32
        self.Value = Fix32()

class HeightTilesI:
    Zero = None
    MinValue = None
    MaxValue = None
    One = None
    def __init__(self):
        self.Abs = None
        self.Sign = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IsPositive = False
        self.IsNotPositive = False
        self.IsNegative = False
        self.IsNotNegative = False
        self.Squared = 0
        self.HeightTilesF = None
        self.AsSlim = None
        self.Value = 0

class HeightTilesISlim:
    def __init__(self):
        self.AsHeightTilesI = None
        self.Value = None

class MechPower:
    Zero = None
    MinValue = None
    MaxValue = None
    OneKw = None
    def __init__(self):
        self.Abs = None
        self.Sign = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IsPositive = False
        self.IsNotPositive = False
        self.IsNegative = False
        self.IsNotNegative = False
        self.Squared = 0
        self.Quantity = None
        from Mafi import Fix32
        self.ToMwSeconds = Fix32()
        self.Value = 0

class PartialQuantity:
    Zero = None
    MinValue = None
    MaxValue = None
    Epsilon = None
    One = None
    def __init__(self):
        self.Abs = None
        self.Sign = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IsPositive = False
        self.IsNotPositive = False
        self.IsNegative = False
        self.IsNotNegative = False
        from Mafi import Fix64
        self.Squared = Fix64()
        self.IntegerPart = None
        self.FractionalPart = None
        from Mafi import Fix32
        self.Value = Fix32()

class PartialQuantityLarge:
    Zero = None
    MinValue = None
    MaxValue = None
    def __init__(self):
        self.Abs = None
        self.Sign = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IsPositive = False
        self.IsNotPositive = False
        self.IsNegative = False
        self.IsNotNegative = False
        from Mafi import Fix64
        self.Squared = Fix64()
        self.IntegerPart = None
        self.AsPartial = None
        self.Value = Fix64()

class Quantity:
    Zero = None
    MinValue = None
    MaxValue = None
    One = None
    def __init__(self):
        self.Abs = None
        self.Sign = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IsPositive = False
        self.IsNotPositive = False
        self.IsNegative = False
        self.IsNotNegative = False
        self.Squared = 0
        self.AsLarge = None
        self.AsPartial = None
        self.Value = 0

class QuantityLarge:
    Zero = None
    MinValue = None
    MaxValue = None
    One = None
    def __init__(self):
        self.Abs = None
        self.Sign = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IsPositive = False
        self.IsNotPositive = False
        self.IsNegative = False
        self.IsNotNegative = False
        self.Squared = 0
        self.Value = 0

class RelGameDate:
    Zero = None
    MinValue = None
    MaxValue = None
    OneDay = None
    OneMonth = None
    OneYear = None
    DAYS_PER_MONTH = 0
    MONTHS_PER_YEAR = 0
    DAYS_PER_YEAR = 0
    def __init__(self):
        self.Abs = None
        self.Sign = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IsPositive = False
        self.IsNotPositive = False
        self.IsNegative = False
        self.IsNotNegative = False
        self.Squared = 0
        self.TotalDays = 0
        self.TotalMonthsFloored = 0
        self.TotalMonthsRounded = 0
        self.Days = 0
        self.Months = 0
        self.Years = 0
        self.Value = 0

class RelTile1f:
    Zero = None
    MinValue = None
    MaxValue = None
    Epsilon = None
    One = None
    Half = None
    Quarter = None
    def __init__(self):
        self.Abs = None
        self.Sign = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IsPositive = False
        self.IsNotPositive = False
        self.IsNegative = False
        self.IsNotNegative = False
        from Mafi import Fix64
        self.Squared = Fix64()
        self.ThicknessTilesF = None
        self.RoundedRelTile1i = None
        self.CeiledRelTile1i = None
        from Mafi import Fix32
        self.Value = Fix32()

class RelTile1i:
    Zero = None
    MinValue = None
    MaxValue = None
    One = None
    def __init__(self):
        self.Abs = None
        self.Sign = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IsPositive = False
        self.IsNotPositive = False
        self.IsNegative = False
        self.IsNotNegative = False
        self.Squared = 0
        self.RelTile1f = None
        self.Meters = 0
        self.Value = 0

class RelTile2f:
    Zero = None
    One = None
    UnitX = None
    UnitY = None
    MinValue = None
    MaxValue = None
    Half = None
    def __init__(self):
        self.Vector2f = None
        from Mafi import Fix32
        self.Sum = Fix32()
        from Mafi import Fix64
        self.Product = Fix64()
        self.Length = Fix32()
        self.LengthSqr = Fix64()
        self.IsZero = False
        self.IsNotZero = False
        self.IncrementX = None
        self.IncrementY = None
        self.DecrementX = None
        self.DecrementY = None
        self.ReflectX = None
        self.ReflectY = None
        self.IsNormalized = False
        self.Normalized = None
        self.Angle = None
        self.LeftOrthogonalVector = None
        self.RightOrthogonalVector = None
        self.AbsValue = None
        self.Signs = None
        self.Times2Fast = None
        self.Times4Fast = None
        self.Times8Fast = None
        self.HalfFast = None
        self.DivBy4Fast = None
        self.DivBy8Fast = None
        self.DivBy16Fast = None
        self.RoundedRelTile2i = None
        self.RelTile2iFloored = None
        self.RelTile2iCeiled = None
        self.RelTile2fFloored = None
        self.LengthTiles = None
        self.AsTile2f = None
        self.X = Fix32()
        self.Y = Fix32()

class RelTile2fExtensions:
    def __init__(self):
        pass


class RelTile2fAssertions:
    def __init__(self):
        pass


class RelTile2i:
    All4Neighbors = None
    All8Neighbors = None
    Zero = None
    One = None
    UnitX = None
    UnitY = None
    MinValue = None
    MaxValue = None
    def __init__(self):
        self.Vector2i = None
        self.Sum = 0
        self.Product = 0
        self.ProductInt = 0
        from Mafi import Fix32
        self.Length = Fix32()
        self.LengthInt = 0
        self.LengthSqrInt = 0
        self.LengthSqr = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IncrementX = None
        self.IncrementY = None
        self.DecrementX = None
        self.DecrementY = None
        self.ReflectX = None
        self.ReflectY = None
        self.Angle = None
        self.LeftOrthogonalVector = None
        self.RightOrthogonalVector = None
        self.AbsValue = None
        self.Signs = None
        self.Times2Fast = None
        self.Times4Fast = None
        self.Times8Fast = None
        self.HalfFast = None
        self.DivBy4Fast = None
        self.DivBy8Fast = None
        self.DivBy16Fast = None
        self.Vector2f = None
        self.RelTile2f = None
        self.RelTile2fCenter = None
        self.LengthAsFloat = 0.0
        self.X = 0
        self.Y = 0

class RelTile2iExtensions:
    def __init__(self):
        pass


class RelTile2iAssertions:
    def __init__(self):
        pass


class RelTile3f:
    Zero = None
    One = None
    UnitX = None
    UnitY = None
    UnitZ = None
    MinValue = None
    MaxValue = None
    Half = None
    def __init__(self):
        self.Xy = None
        self.Vector3f = None
        from Mafi import Fix32
        self.Sum = Fix32()
        self.Length = Fix32()
        from Mafi import Fix64
        self.LengthSqr = Fix64()
        self.IsZero = False
        self.IsNotZero = False
        self.IncrementX = None
        self.IncrementY = None
        self.IncrementZ = None
        self.DecrementX = None
        self.DecrementY = None
        self.DecrementZ = None
        self.ReflectX = None
        self.ReflectY = None
        self.ReflectZ = None
        self.IsNormalized = False
        self.Normalized = None
        self.Angle = None
        self.AbsValue = None
        self.Signs = None
        self.Times2Fast = None
        self.Times4Fast = None
        self.Times8Fast = None
        self.HalfFast = None
        self.DivBy4Fast = None
        self.DivBy8Fast = None
        self.DivBy16Fast = None
        self.Tile3i = None
        self.RelTile3iRounded = None
        self.LengthTiles1f = None
        self.WidthTiles1f = None
        self.HeightTiles1f = None
        self.X = Fix32()
        self.Y = Fix32()
        self.Z = Fix32()

class RelTile3fExtensions:
    def __init__(self):
        pass


class RelTile3fAssertions:
    def __init__(self):
        pass


class RelTile3i:
    Zero = None
    One = None
    UnitX = None
    UnitY = None
    UnitZ = None
    MinValue = None
    MaxValue = None
    def __init__(self):
        self.Xy = None
        self.Vector3i = None
        self.Sum = 0
        from Mafi import Fix32
        self.Length = Fix32()
        self.LengthInt = 0
        self.LengthSqrInt = 0
        self.LengthSqr = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IncrementX = None
        self.IncrementY = None
        self.IncrementZ = None
        self.DecrementX = None
        self.DecrementY = None
        self.DecrementZ = None
        self.ReflectX = None
        self.ReflectY = None
        self.ReflectZ = None
        self.Angle = None
        self.AbsValue = None
        self.Signs = None
        self.Times2Fast = None
        self.Times4Fast = None
        self.Times8Fast = None
        self.HalfFast = None
        self.DivBy4Fast = None
        self.DivBy8Fast = None
        self.DivBy16Fast = None
        self.CornerRelTile3f = None
        self.CenterRelTile3f = None
        self.Vector3f = None
        self.Thickness = None
        self.LengthAsFloat = 0.0
        self.ToSlim = None
        self.X = 0
        self.Y = 0
        self.Z = 0

class RelTile3iExtensions:
    def __init__(self):
        pass


class RelTile3iAssertions:
    def __init__(self):
        pass


class SimStep:
    Zero = None
    MinValue = None
    MaxValue = None
    One = None
    STEPS_PER_SECOND = 0
    SECONDS_PER_STEP = 0.0
    def __init__(self):
        self.Abs = None
        self.Sign = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IsPositive = False
        self.IsNotPositive = False
        self.IsNegative = False
        self.IsNotNegative = False
        self.Squared = 0
        self.Value = 0

class ThicknessTilesF:
    Zero = None
    MinValue = None
    MaxValue = None
    Epsilon = None
    Quarter = None
    Half = None
    One = None
    Two = None
    def __init__(self):
        self.Abs = None
        self.Sign = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IsPositive = False
        self.IsNotPositive = False
        self.IsNegative = False
        self.IsNotNegative = False
        from Mafi import Fix64
        self.Squared = Fix64()
        self.CeiledThicknessTilesI = None
        self.FlooredThicknessTilesI = None
        self.RoundedThicknessTilesI = None
        self.HalfFast = None
        from Mafi import Fix32
        self.Meters = Fix32()
        self.Value = Fix32()

class ThicknessTilesI:
    Zero = None
    MinValue = None
    MaxValue = None
    One = None
    Two = None
    def __init__(self):
        self.Abs = None
        self.Sign = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IsPositive = False
        self.IsNotPositive = False
        self.IsNegative = False
        self.IsNotNegative = False
        self.Squared = 0
        self.ThicknessTilesF = None
        self.AsSlim = None
        self.AsSemiSlim = None
        self.Value = 0

class ThicknessTilesISemiSlim:
    def __init__(self):
        self.AsThicknessTilesI = None
        self.AsThicknessTilesF = None
        self.Value = None

class ThicknessTilesISlim:
    def __init__(self):
        self.AsThicknessTilesI = None
        self.AsThicknessTilesF = None
        self.Value = None

class Tile2f:
    Zero = None
    One = None
    UnitX = None
    UnitY = None
    MinValue = None
    MaxValue = None
    def __init__(self):
        self.Vector2f = None
        from Mafi import Fix32
        self.Sum = Fix32()
        from Mafi import Fix64
        self.Product = Fix64()
        self.Length = Fix32()
        self.LengthSqr = Fix64()
        self.IsZero = False
        self.IsNotZero = False
        self.IncrementX = None
        self.IncrementY = None
        self.DecrementX = None
        self.DecrementY = None
        self.ReflectX = None
        self.ReflectY = None
        self.IsNormalized = False
        self.Normalized = None
        self.Angle = None
        self.LeftOrthogonalVector = None
        self.RightOrthogonalVector = None
        self.Times2Fast = None
        self.Times4Fast = None
        self.Times8Fast = None
        self.HalfFast = None
        self.DivBy4Fast = None
        self.DivBy8Fast = None
        self.DivBy16Fast = None
        self.Tile2i = None
        self.Tile2iRounded = None
        self.Tile2iCeiled = None
        self.X = Fix32()
        self.Y = Fix32()

class Tile2fExtensions:
    def __init__(self):
        pass


class Tile2fAssertions:
    def __init__(self):
        pass


class Tile2i:
    Zero = None
    One = None
    UnitX = None
    UnitY = None
    MinValue = None
    MaxValue = None
    def __init__(self):
        self.Vector2i = None
        self.Sum = 0
        self.Product = 0
        self.ProductInt = 0
        from Mafi import Fix32
        self.Length = Fix32()
        self.LengthInt = 0
        self.LengthSqrInt = 0
        self.LengthSqr = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IncrementX = None
        self.IncrementY = None
        self.DecrementX = None
        self.DecrementY = None
        self.ReflectX = None
        self.ReflectY = None
        self.Angle = None
        self.LeftOrthogonalVector = None
        self.RightOrthogonalVector = None
        self.Times2Fast = None
        self.Times4Fast = None
        self.Times8Fast = None
        self.HalfFast = None
        self.DivBy4Fast = None
        self.DivBy8Fast = None
        self.DivBy16Fast = None
        self.AsSlim = None
        self.Vector2f = None
        self.RelTile2i = None
        self.ChunkCoord2i = None
        self.ChunkCoord2iSlim = None
        self.TileInChunkCoord = None
        self.TileInChunkCoordSlim = None
        self.CornerTile2f = None
        self.CenterTile2f = None
        self.X = 0
        self.Y = 0
        self.XyPacked = None

class Tile2iExtensions:
    def __init__(self):
        pass


class Tile2iAssertions:
    def __init__(self):
        pass


class Tile2iIndex:
    def __init__(self):
        self.PlusXNeighborUnchecked = None
        self.MinusXNeighborUnchecked = None
        self.Value = 0

class Tile2iSlim:
    Zero = None
    One = None
    UnitX = None
    UnitY = None
    MinValue = None
    MaxValue = None
    def __init__(self):
        self.Vector2i = None
        self.Sum = 0
        self.Product = 0
        from Mafi import Fix32
        self.Length = Fix32()
        self.LengthInt = 0
        self.LengthSqrInt = 0
        self.LengthSqr = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IncrementX = None
        self.IncrementY = None
        self.DecrementX = None
        self.DecrementY = None
        self.Angle = None
        self.AsFull = None
        self.CornerTile2f = None
        self.CenterTile2f = None
        self.ChunkCoord2i = None
        self.X = None
        self.Y = None
        self.XyPacked = None

class Tile2iSlimAssertions:
    def __init__(self):
        pass


class Tile3f:
    Zero = None
    One = None
    UnitX = None
    UnitY = None
    UnitZ = None
    MinValue = None
    MaxValue = None
    def __init__(self):
        self.Xy = None
        self.Vector3f = None
        from Mafi import Fix32
        self.Sum = Fix32()
        self.Length = Fix32()
        from Mafi import Fix64
        self.LengthSqr = Fix64()
        self.IsZero = False
        self.IsNotZero = False
        self.IncrementX = None
        self.IncrementY = None
        self.IncrementZ = None
        self.DecrementX = None
        self.DecrementY = None
        self.DecrementZ = None
        self.ReflectX = None
        self.ReflectY = None
        self.ReflectZ = None
        self.IsNormalized = False
        self.Normalized = None
        self.Angle = None
        self.Times2Fast = None
        self.Times4Fast = None
        self.Times8Fast = None
        self.HalfFast = None
        self.DivBy4Fast = None
        self.DivBy8Fast = None
        self.DivBy16Fast = None
        self.Tile3i = None
        self.Tile2i = None
        self.Tile3iRounded = None
        self.RelTile3f = None
        self.Height = None
        self.X = Fix32()
        self.Y = Fix32()
        self.Z = Fix32()

class Tile3fExtensions:
    def __init__(self):
        pass


class Tile3fAssertions:
    def __init__(self):
        pass


class Tile3i:
    Zero = None
    One = None
    UnitX = None
    UnitY = None
    UnitZ = None
    MinValue = None
    MaxValue = None
    def __init__(self):
        self.Xy = None
        self.Vector3i = None
        self.Sum = 0
        from Mafi import Fix32
        self.Length = Fix32()
        self.LengthInt = 0
        self.LengthSqrInt = 0
        self.LengthSqr = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IncrementX = None
        self.IncrementY = None
        self.IncrementZ = None
        self.DecrementX = None
        self.DecrementY = None
        self.DecrementZ = None
        self.ReflectX = None
        self.ReflectY = None
        self.ReflectZ = None
        self.Angle = None
        self.Times2Fast = None
        self.Times4Fast = None
        self.Times8Fast = None
        self.HalfFast = None
        self.DivBy4Fast = None
        self.DivBy8Fast = None
        self.DivBy16Fast = None
        self.AsSlim = None
        self.Height = None
        self.Tile2i = None
        self.ParentChunkCoord = None
        self.Vector3f = None
        self.CornerTile3f = None
        self.CenterTile3f = None
        self.CenterXyFloorZTile3f = None
        self.TileInChunkCoord = None
        self.X = 0
        self.Y = 0
        self.Z = 0

class Tile3iExtensions:
    def __init__(self):
        pass


class Tile3iAssertions:
    def __init__(self):
        pass


class TileInChunk2i:
    Zero = None
    One = None
    UnitX = None
    UnitY = None
    ChunkCenter = None
    def __init__(self):
        self.Vector2i = None
        self.Sum = 0
        self.Product = 0
        self.ProductInt = 0
        from Mafi import Fix32
        self.Length = Fix32()
        self.LengthInt = 0
        self.LengthSqrInt = 0
        self.LengthSqr = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IncrementX = None
        self.IncrementY = None
        self.DecrementX = None
        self.DecrementY = None
        self.ReflectX = None
        self.ReflectY = None
        self.Angle = None
        self.LeftOrthogonalVector = None
        self.RightOrthogonalVector = None
        self.AbsValue = None
        self.Signs = None
        self.Times2Fast = None
        self.Times4Fast = None
        self.Times8Fast = None
        self.HalfFast = None
        self.DivBy4Fast = None
        self.DivBy8Fast = None
        self.DivBy16Fast = None
        self.ChunkDataIndex = 0
        self.IsOnChunkBoundary = False
        self.X = 0
        self.Y = 0

class TileInChunk2iExtensions:
    def __init__(self):
        pass


class TileInChunk2iAssertions:
    def __init__(self):
        pass


class TileInChunk2iSlim:
    Zero = None
    One = None
    UnitX = None
    UnitY = None
    def __init__(self):
        self.Vector2i = None
        self.Sum = 0
        self.Product = 0
        from Mafi import Fix32
        self.Length = Fix32()
        self.LengthInt = 0
        self.LengthSqrInt = 0
        self.LengthSqr = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IncrementX = None
        self.IncrementY = None
        self.DecrementX = None
        self.DecrementY = None
        self.Angle = None
        self.X = None
        self.Y = None
        self.XyPacked = None

class TileInChunk2iSlimAssertions:
    def __init__(self):
        pass


class Upoints:
    Zero = None
    MinValue = None
    MaxValue = None
    Epsilon = None
    def __init__(self):
        self.Abs = None
        self.Sign = 0
        self.IsZero = False
        self.IsNotZero = False
        self.IsPositive = False
        self.IsNotPositive = False
        self.IsNegative = False
        self.IsNotNegative = False
        from Mafi import Fix64
        self.Squared = Fix64()
        from Mafi import Fix32
        self.Value = Fix32()

class HeightTilesFExts:
    def __init__(self):
        pass


class HeightTilesIExtensions:
    def __init__(self):
        pass


class MechPowerExtensions:
    def __init__(self):
        pass


class PartialQuantityExtensions:
    def __init__(self):
        pass


class PartialQuantityLargeExtensions:
    def __init__(self):
        pass


class QuantityExtensions:
    def __init__(self):
        pass


class RelTile1fExtensions:
    def __init__(self):
        pass


class RelTile1iExtensions:
    def __init__(self):
        pass


class RelTile3iSlim:
    Zero = None
    One = None
    UnitX = None
    UnitY = None
    UnitZ = None
    MinValue = None
    MaxValue = None
    def __init__(self):
        self.Xy = None
        self.RelTile3i = None
        self.Vector3i = None
        self.Vector3f = None
        self.Sum = 0
        from Mafi import Fix32
        self.Length = Fix32()
        self.LengthInt = 0
        self.LengthSqr = 0
        self.IsZero = False
        self.IsNotZero = False
        self.Angle = None
        self.IncrementX = None
        self.IncrementY = None
        self.IncrementZ = None
        self.DecrementX = None
        self.DecrementY = None
        self.DecrementZ = None
        self.X = None
        self.Y = None
        self.Z = None

class RelTile3iSlimAssertions:
    def __init__(self):
        pass


class ThicknessTilesFExtensions:
    def __init__(self):
        pass


class ThicknessTilesIExtensions:
    def __init__(self):
        pass


class ThicknessTilesISlimOption:
    None = None
    def __init__(self):
        self.Value = None
        self.HasValue = False
        self.IsNone = False
        self.RawValue = None

class ThicknessTilesISemiSlimOption:
    None = None
    def __init__(self):
        self.Value = None
        self.HasValue = False
        self.IsNone = False
        self.RawValue = None

class Tile2iAndIndex:
    def __init__(self):
        self.Index = None
        self.TileCoord = None
        self.TileCoordSlim = None
        self.ChunkCoord2i = None
        self.PlusXNeighborUnchecked = None
        self.MinusXNeighborUnchecked = None
        self.X = None
        self.Y = None
        self.IndexRaw = 0
        self.XyPacked = None
        self.XyIndexPacked = None

class Tile2iAndIndexRel:
    def __init__(self):
        self.X = None
        self.Y = None
        self.IndexDelta = 0
        self.XyPacked = None
        self.XyIndexPacked = None

class Tile3iSlim:
    Zero = None
    One = None
    UnitX = None
    UnitY = None
    UnitZ = None
    MinValue = None
    MaxValue = None
    def __init__(self):
        self.Xy = None
        self.Tile3i = None
        self.Vector3i = None
        self.Sum = 0
        from Mafi import Fix32
        self.Length = Fix32()
        self.LengthInt = 0
        self.LengthSqr = 0
        self.IsZero = False
        self.IsNotZero = False
        self.Angle = None
        self.IncrementX = None
        self.IncrementY = None
        self.IncrementZ = None
        self.DecrementX = None
        self.DecrementY = None
        self.DecrementZ = None
        self.X = None
        self.Y = None
        self.Z = None

class Tile3iSlimAssertions:
    def __init__(self):
        pass


class TileInChunk256:
    def __init__(self):
        self.AsTileOffset = None
        self.X = None
        self.Y = None
        self.Index = None

class UpointsExtensions:
    def __init__(self):
        pass


class IoPortType:
    Any = None
    Input = None
    Output = None
    def __init__(self):
        self.value__ = 0

class IoPortTypeExtensions:
    def __init__(self):
        pass


class ProductExtensions:
    def __init__(self):
        pass


class ProductAttribute:
    def __init__(self):
        self.TypeId = None

class ProtoParamForAttribute:
    def __init__(self):
        self.TypeId = None
        self.IdFieldName = ""

class UnreachableTerrainDesignationsManagerDebugRenderer:
    def __init__(self):
        pass


class DebugGameRendererRegistrator:
    def __init__(self):
        pass


class DebugGameRenderer:
    ImagesSaved = 0
    IsEnabled = False
    IsDisabled = False
    DependencyResolver = None
    EMPTY_DEBUG_GAME_MAP_DRAWING = None
    DEFAULT_PX_PER_TILE = 0
    def __init__(self):
        pass


class DebugGameMapDrawing:
    def __init__(self):
        from Mafi import Option
        self.Resolver = Option()
        self.IsEnabled = False
        self.IsNotEnabled = False
        self.From = None
        self.Size = None

class DebugGameRendererConfig:
    SaveVehiclePathFindingSuccesses = False
    SaveVehiclePathFindingFailures = False
    SaveVehiclePathFindingFailuresClosebyApproach = False
    SaveVehicleIdleTooLong = False
    SaveVehicleGettingUnstuck = False
    SaveVehicleReachabilityResults = False
    SaveVehicleHomeUnreachable = False
    SaveVehicleFailToReachHome = False
    SaveVehicleGoalUnreachable = False
    SaveVehicleGoalReplanDueToBlock = False
    SaveVehicleDriveTooFarWithoutPf = False
    SaveTransportPillarsSupportFailures = False
    SaveClearancePathabilityProviderValidationErrors = False
    SaveSuspiciouslyLongVehicleDriveTargets = False
    SaveLayoutEntityFailedToBuild = False
    DrawTileHeights = None
    def __init__(self):
        pass


class StateAssert:
    def __init__(self):
        pass

