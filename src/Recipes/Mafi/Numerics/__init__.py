
class Line2f:
    def __init__(self):
        self.Direction = None
        self.P0 = None
        self.P1 = None

class Line2i:
    def __init__(self):
        self.Direction = None
        self.DirectionNormalized = None
        self.CenterPt = None
        from Mafi import Fix32
        self.SegmentLength = Fix32()
        self.Line2f = None
        self.P0 = None
        self.P1 = None

class Line3i:
    def __init__(self):
        self.Direction = None
        self.DirectionNormalized = None
        self.CenterPt = None
        from Mafi import Fix32
        self.SegmentLength = Fix32()
        self.Line3f = None
        self.P0 = None
        self.P1 = None

class Polygon2f:
    def __init__(self):
        self.Vertices = None

class Polygon2fFast:
    def __init__(self):
        self.VerticesCount = 0
        self.EdgeVectors = None
        self.EdgeLengthsSqr = None
        self.BoundingBoxMin = None
        self.BoundingBoxMax = None
        self.BoundingBox = None
        self.VerticesExt = None

class Polygon2fMutable:
    def __init__(self):
        self.Vertices = None
        self.Item = None
        self.MinVertexCount = 0
        self.MaxVertexCount = 0

class Polygon2i:
    def __init__(self):
        self.Vertices = None

class Polygon2iMutable:
    def __init__(self):
        self.Vertices = None
        self.MinVertexCount = 0
        self.MaxVertexCount = 0

class Polygon3f:
    def __init__(self):
        self.Vertices = None

class Polygon3fMutable:
    def __init__(self):
        self.Vertices = None
        self.RoundControlPointHeightsToWholeTiles = False
        self.Item = None
        self.MinVertexCount = 0
        self.MaxVertexCount = 0
        self.ClampZMinMax = 0

class Polygon3i:
    def __init__(self):
        self.Vertices = None

class Polygon3iMutable:
    def __init__(self):
        self.Vertices = None
        self.MinVertexCount = 0
        self.MaxVertexCount = 0

class Ratio2i:
    def __init__(self):
        self.Item = 0
        self.A = 0
        self.B = 0

class Rect2i:
    def __init__(self):
        self.Size = None
        self.Min = None
        self.Max = None

class UnitQuaternion4f:
    Identity = None
    def __init__(self):
        from Mafi import Fix32
        self.X = Fix32()
        self.Y = Fix32()
        self.Z = Fix32()
        self.Vector4f = None
        self.Length = Fix32()
        from Mafi import Fix64
        self.LengthSqr = Fix64()
        self.IsNormalized = False
        self.Renormalized = None
        self.Conjugated = None
        self.Xyz = None
        self.W = Fix32()

class UnityCameraPose:
    def __init__(self):
        self.PositionX = 0.0
        self.PositionY = 0.0
        self.PositionZ = 0.0
        self.RotationX = 0.0
        self.RotationY = 0.0
        self.RotationZ = 0.0
        self.RotationW = 0.0
        self.VerticalFieldOfView = 0.0

class UnityMatrix4:
    def __init__(self):
        self.M00 = 0.0
        self.M10 = 0.0
        self.M20 = 0.0
        self.M30 = 0.0
        self.M01 = 0.0
        self.M11 = 0.0
        self.M21 = 0.0
        self.M31 = 0.0
        self.M02 = 0.0
        self.M12 = 0.0
        self.M22 = 0.0
        self.M32 = 0.0
        self.M03 = 0.0
        self.M13 = 0.0
        self.M23 = 0.0
        self.M33 = 0.0

class IFunctionFix32:
    def __init__(self):
        pass


class IFunctionFix32Factory:
    def __init__(self):
        pass


class PassThroughFunctionFix32Factory:
    def __init__(self):
        self.Value = None

class Line3f:
    def __init__(self):
        self.Direction = None
        self.From = None
        self.To = None

class LineRasterizer:
    def __init__(self):
        self.From = None
        self.To = None
        self.SkipFirst = False

    class Enumerator:
        def __init__(self):
            self.Current = None

class Plane:
    YzPlane = None
    XyPlane = None
    XzPlane = None
    def __init__(self):
        self.Normal = None
        from Mafi import Fix32
        self.DistanceFromOrigin = Fix32()

class Polygon2iFast:
    def __init__(self):
        self.VerticesExt = None
        self.VerticesCount = 0
        self.EdgeVectors = None
        self.EdgeLengthsSqr = None
        self.BoundingBoxMin = None
        self.BoundingBoxMax = None

class Polygon3iFast:
    def __init__(self):
        self.VerticesExt = None
        self.VerticesCount = 0
        self.EdgeVectors = None
        self.EdgeLengthsSqr = None
        self.BoundingBoxMin = None
        self.BoundingBoxMax = None

class Polygon3fFast:
    def __init__(self):
        self.VerticesExt = None
        self.VerticesCount = 0
        self.EdgeVectors = None
        self.EdgeLengthsSqr = None
        self.EdgeLengths2DSqr = None
        self.BoundingBoxMin = None
        self.BoundingBoxMax = None

class RayCaster:
    def __init__(self):
        pass


class UnitQuaternion4fAssertionExtensions:
    def __init__(self):
        pass

