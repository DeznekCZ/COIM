class EntityId:
    def __init__(self, value: int):
        self.Value = value

class EnoughClass:
    def __init__(self):
        self.HasEnough = True

class Entity:
    def __init__(self, entity_id: EntityId):
        self.IsPaused = False
        self.Id = entity_id
        self.CanBePaused = True
        self.WorkerConsumer = EnoughClass()
        self.PowerConsumer = EnoughClass()

    def SetPause(self, pause: bool):
        self.IsPaused = pause

class Output:
    def __init__(self, output_id: str, name: str):
        self.id = output_id
        self.name = name

class Input:
    def __init__(self, input_id: str, name: str):
        self.id = input_id
        self.name = name

class Field:
    def __init__(self, field_id: str, name: str, short_desc: str):
        self.id = field_id
        self.name = name
        self.short_desc = short_desc

class Int32Field(Field):
    def __init__(self, field_id: str, name: str, short_desc="", default_value=0):
        super().__init__(field_id, name, short_desc)
        self.default_value = default_value

class StringField(Field):
    def __init__(self, field_id: str, name: str, short_desc="", default_value=""):
        super().__init__(field_id, name, short_desc)
        self.default_value = default_value

class EntityField(Field):
    def __init__(self, field_id: str, name: str, short_desc: str = None, distance: float = 5, entity_filter=lambda module, entity: True):
        super().__init__(field_id, name, short_desc)
        self.distance = distance
        self.filter = entity_filter

class StaticEntityField(Field):
    def __init__(self, field_id: str, name: str, short_desc: str = None, distance: float = 5, entity_filter=lambda module, entity: True):
        super().__init__(field_id, name, short_desc)
        self.distance = distance
        self.filter = entity_filter

class InputValue:
    def __init__(self, module):
        self.module = module

    def set(self, name: str, value: float) -> None:
        self.module.number_data["in__" + name] = value

    def get(self, name: str, default: float) -> float:
        return self.module.number_data["in__" + name] or default

    def set_bool(self, name: str, value: bool) -> None:
        if value:
            self.module.number_data["in__" + name] = 1
        else:
            self.module.number_data["in__" + name] = 0

    def get_bool(self, name: str, default: bool) -> bool:
        return (self.module.number_data["in__" + name] or default) > 0

class OutputValue:
    def __init__(self, module):
        self.module = module

    def set(self, name: str, value: float) -> None:
        self.module.number_data["out__" + name] = value

    def get(self, name: str, default: float) -> float:
        return self.module.number_data["out__" + name] or default

    def set_bool(self, name: str, value: bool) -> None:
        if value:
            self.module.number_data["out__" + name] = 1
        else:
            self.module.number_data["out__" + name] = 0

    def get_bool(self, name: str, default: bool) -> bool:
        return (self.module.number_data["out__" + name] or default) > 0

class FieldValue:
    def __init__(self, module):
        self.module = module

    def set(self, name: str, value: float) -> None:
        self.module.number_data["field__" + name] = value

    def get(self, name: str, default: float) -> float:
        return self.module.number_data["field__" + name] or default

    def set_str(self, name: str, value: str) -> None:
        self.module.string_data["field__" + name] = value or ""

    def get_str(self, name: str, default: "") -> str:
        return self.module.string_data["field__" + name] or default

    def set_ent(self, name: str, value: Entity = None) -> None:
        if Entity is None:
            self.module.number_data["field__" + name] = 0
        else:
            self.module.number_data["field__" + name] = value.Id.Value

    def get_ent(self, name: str) -> Entity:
        return self.module.number_data["field__" + name]

class DefaultCategories:
    Connection = "Connection"
    Display = "Display"

class DefaultControllers:
    Basic = "ProgramableNetwork_Controller"

class Module:
    name: str = None
    inputs: [Input] = None
    outputs: [Output] = None
    fields: [Field] = None
    categories: [str] = None
    controllers: [str] = None

    def __init__(self):
        # defines an interface to data of input inside module
        self.input = InputValue(self)
        # defines an interface to data of output inside module
        self.output = OutputValue(self)
        # defines an interface to data of field inside module
        self.field = FieldValue(self)
        # defines an interface to raw data inside module
        self.number_data = {}
        # defines an interface to raw data inside module
        self.string_data = {}

    def init(self, prototype):
        pass

    def action(self):
        pass
