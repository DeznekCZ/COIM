class CubicBezierCurve2f:

    def __init__(self):
        self.ControlPointsCount = int(0)
        self.SegmentsCount = int(0)
        self.IsEmpty = False
        self.LastControlPoint = None
        self.Item = None
class CubicBezierCurve2fSampler:

    def __init__(self):
        pass

class CubicBezierCurve3f:

    def __init__(self):
        self.ControlPointsCount = int(0)
        self.SegmentsCount = int(0)
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
        self.ControlPointsCount = int(0)
        self.SegmentsCount = int(0)
        self.LastControlPoint = None
class CubicBezierCurveSegment3f:

    def __init__(self):
        self.IsLine = False
