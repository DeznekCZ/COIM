from Mafi.Core.Entities import Entity

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
    def __init__(self, entity_type: type[Entity] | list[type[Entity]], field_id: str, name: str, short_desc: str = None, distance: float = 5, entity_filter=lambda module, entity: True):
        super().__init__(field_id, name, short_desc)
        self.type = entity_type;
        self.distance = distance
        self.filter = entity_filter

class FieldValue:
    def __init__(self, module):
        self.module = module

    def set(self, name: str, value: float) -> None:
        self.module.number_data["field__" + name] = value

    def get(self, name: str, default: float) -> float:
        return self.module.number_data["field__" + name] or default

    def set_str(self, name: str, value: str) -> None:
        self.module.string_data["field__" + name] = value or ""

    def get_str(self, name: str, default = "") -> str:
        return self.module.string_data["field__" + name] or default

    def set_ent(self, name: str, value: Entity = None) -> None:
        if Entity is None:
            self.module.number_data["field__" + name] = 0
        else:
            self.module.number_data["field__" + name] = value.Id.Value

    def get_ent(self, name: str) -> Entity:
        return self.module.number_data["field__" + name]