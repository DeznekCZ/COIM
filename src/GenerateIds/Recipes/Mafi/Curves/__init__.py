
class CubicBezierCurve1f:
    def __init__(self):
        self.ControlPointsCount = 0
        self.SegmentsCount = 0
        self.IsEmpty = False
        from Mafi import Fix32
        self.LastControlPoint = Fix32()
        self.StartDirectionNotNormalized = Fix32()
        self.EndDirectionNotNormalized = Fix32()
        self.Item = Fix32()
        self.ControlPoints = None

class CubicBezierCurve1fSampler:
    def __init__(self):
        pass


class CubicBezierCurve2f:
    def __init__(self):
        self.SegmentsCount = 0
        self.IsEmpty = False
        self.ControlPoints = None
        self.StartDirectionNotNormalized = None
        self.EndDirectionNotNormalized = None
        self.Item = None

class CubicBezierCurve2fExtensions:
    def __init__(self):
        pass


class CubicBezierCurve2fSampler:
    def __init__(self):
        from Mafi import Fix32
        self.CurveLengthApprox = Fix32()

class CubicBezierCurve2fSamplerCustom:
    def __init__(self):
        from Mafi import Fix32
        self.CurveLengthApprox = Fix32()

class CubicBezierCurve3f:
    def __init__(self):
        self.ControlPointsCount = 0
        self.SegmentsCount = 0
        self.IsEmpty = False
        self.LastControlPoint = None
        self.ControlPoints = None
        self.StartPoint = None
        self.EndPoint = None
        self.StartDirectionNotNormalized = None
        self.EndDirectionNotNormalized = None
        self.Item = None

class CubicBezierCurve3fSampler:
    def __init__(self):
        from Mafi import Fix32
        self.CurveLengthApprox = Fix32()

class CubicBezierCurve3fSamplerCustom:
    def __init__(self):
        from Mafi import Fix32
        self.CurveLengthApprox = Fix32()

class CubicBezierCurve3fBuilder:
    def __init__(self):
        self.ControlPointsCount = 0
        self.SegmentsCount = 0
        self.LastControlPoint = None

class CubicBezierCurveSegment3f:
    def __init__(self):
        self.IsLine = False
        self.P0 = None
        self.P1 = None
        self.P2 = None
        self.P3 = None
