
class EntityValidationResult:
    Success = None
    def __init__(self):
        self.IsSuccess = False
        self.IsError = False
        self.IsSuppressedError = False
        self.ValidationStatus = None
        self.ErrorMessageForPlayer = None
        self.ErrorMessage = ""

class EntityValidationResultStatus:
    Valid = None
    Error = None
    FatalError = None
    SuppressedError = None
    def __init__(self):
        self.value__ = 0

class IEntityAdditionValidator:
    def __init__(self):
        self.Priority = None

class IEntityPreAddValidator:
    def __init__(self):
        self.Priority = None

class EntityValidatorPriority:
    Low = None
    Default = None
    High = None
    def __init__(self):
        self.value__ = 0

class IEntityAddRequest:
    def __init__(self):
        self.ReasonToAdd = None

class IEntityWithOccupiedTilesAddRequest:
    def __init__(self):
        self.Proto = None
        self.Origin = None
        self.BoundingRect = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        from Mafi import Option
        self.IgnoreForCollisions = Option()
        self.RecordTileErrorsAndMetadata = False
        self.SuppressFlags = None
        self.ReasonToAdd = None

class ILayoutEntityAddRequest:
    def __init__(self):
        self.Transform = None
        self.Layout = None
        self.Proto = None
        self.Origin = None
        self.BoundingRect = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        from Mafi import Option
        self.IgnoreForCollisions = Option()
        self.RecordTileErrorsAndMetadata = False
        self.SuppressFlags = None
        self.ReasonToAdd = None

class IAddRequestMetadata:
    def __init__(self):
        pass


class ValidationSuppressFlag:
    None = None
    TerrainCollision = None
    def __init__(self):
        self.value__ = 0

class IEntityRemovalValidator:
    def __init__(self):
        self.Priority = None
