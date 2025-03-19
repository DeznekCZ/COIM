class Line2f:

    def __init__(self):
        self.Direction = None
class Line2i:

    def __init__(self):
        self.Direction = None
        self.DirectionNormalized = None
        self.CenterPt = None
        from Mafi import Fix32
        self.SegmentLength = Fix32()
        self.Line2f = None
class Line3i:

    def __init__(self):
        self.Direction = None
        self.DirectionNormalized = None
        self.CenterPt = None
        from Mafi import Fix32
        self.SegmentLength = Fix32()
        self.Line3f = None
class Polygon2f:

    def __init__(self):
        pass

class Polygon2fFast:

    def __init__(self):
        self.VerticesCount = int(0)
        self.EdgeVectors = None
        self.EdgeLengthsSqr = None
        self.BoundingBoxMin = None
        self.BoundingBoxMax = None
        self.BoundingBox = None
class Polygon2fMutable:

    def __init__(self):
        self.Vertices = None
        self.Item = None
class Polygon2i:

    def __init__(self):
        pass

class Polygon2iMutable:

    def __init__(self):
        self.Vertices = None
class Polygon3f:

    def __init__(self):
        pass

class Polygon3fMutable:

    def __init__(self):
        self.Vertices = None
        self.RoundControlPointHeightsToWholeTiles = False
        self.Item = None
class Polygon3i:

    def __init__(self):
        pass

class Polygon3iMutable:

    def __init__(self):
        self.Vertices = None
class Ratio2i:

    def __init__(self):
        self.Item = int(0)
class Rect2i:

    def __init__(self):
        self.Size = None
class UnitQuaternion4f:

    def __init__(self):
        from Mafi import Fix32
        self.X = Fix32()
        from Mafi import Fix32
        self.Y = Fix32()
        from Mafi import Fix32
        self.Z = Fix32()
        self.Vector4f = None
        from Mafi import Fix32
        self.Length = Fix32()
        from Mafi import Fix64
        self.LengthSqr = Fix64()
        self.IsNormalized = False
        self.Renormalized = None
        self.Conjugated = None
class UnityCameraPose:

    def __init__(self):
        pass

class UnityMatrix4:

    def __init__(self):
        pass

class IFunctionFix32:

    def __init__(self):
        pass

class IFunctionFix32Factory:

    def __init__(self):
        pass

class PassThroughFunctionFix32Factory:

    def __init__(self):
        pass

class Line3f:

    def __init__(self):
        self.Direction = None
class LineRasterizer:

    def __init__(self):
        pass

class Enumerator:

    def __init__(self):
        self.Current = None
class Plane:

    def __init__(self):
        pass

class Polygon2iFast:

    def __init__(self):
        pass

class Polygon3iFast:

    def __init__(self):
        pass

class Polygon3fFast:

    def __init__(self):
        pass

class RayCaster:

    def __init__(self):
        pass

class UnitQuaternion4fAssertionExtensions:

    def __init__(self):
        pass

