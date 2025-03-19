class EntityValidationResult:

    def __init__(self):
        self.IsSuccess = False
        self.IsError = False
class EntityValidationResultStatus:

    def __init__(self):
        pass

class IEntityAdditionValidator:

    def __init__(self):
        self.Priority = None
class IEntityPreAddValidator:

    def __init__(self):
        self.Priority = None
class EntityValidatorPriority:

    def __init__(self):
        pass

class IEntityAddRequest:

    def __init__(self):
        self.ReasonToAdd = None
class IEntityWithOccupiedTilesAddRequest:

    def __init__(self):
        self.Origin = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        from Mafi import Option
        self.IgnoreForCollisions = Option()
        self.RecordTileErrorsAndMetadata = False
        self.ReasonToAdd = None
class ILayoutEntityAddRequest:

    def __init__(self):
        self.Transform = None
        self.Layout = None
        self.Origin = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        from Mafi import Option
        self.IgnoreForCollisions = Option()
        self.RecordTileErrorsAndMetadata = False
        self.ReasonToAdd = None
class IAddRequestMetadata:

    def __init__(self):
        pass

class IEntityRemovalValidator:

    def __init__(self):
        self.Priority = None
