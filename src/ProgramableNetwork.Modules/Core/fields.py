from Mafi import Fix32
from Mafi.Core.Entities import Entity
from Core.mafi import fix

class Field:
    def __init__(self, field_id: str, name: str, short_desc: str):
        self.id = field_id
        self.name = name
        self.short_desc = short_desc

class Int32Field(Field):
    def __init__(self, field_id: str, name: str, short_desc="", default_value=0):
        super().__init__(field_id, name, short_desc)
        self.default_value = default_value

class Fix32Field(Field):
    def __init__(self, field_id: str, name: str, short_desc="", default_value=fix(0)):
        super().__init__(field_id, name, short_desc)
        self.default_value = default_value

class StringField(Field):
    def __init__(self, field_id: str, name: str, short_desc="", default_value=""):
        super().__init__(field_id, name, short_desc)
        self.default_value = default_value

class BooleanField(Field):
    def __init__(self, field_id: str, name: str, short_desc="", default_value=False):
        super().__init__(field_id, name, short_desc)
        self.default_value = default_value

class EntityField(Field):
    def __init__(self, entity_type: type[Entity] | list[type[Entity]], field_id: str, name: str, short_desc: str = None, distance: float = 5, entity_filter=lambda module, entity: True):
        super().__init__(field_id, name, short_desc)
        self.type = entity_type;
        self.distance = distance
        self.filter = entity_filter

class FieldValue:
    def __init__(self, module): pass
    def get(self, name: str, default: Fix32) -> Fix32: pass
    def get_bool(self, name: str, default: bool) -> bool: pass
    def get_int(self, name: str, default: int) -> int: pass
    def get_ent(self, name: str) -> Entity: pass
    def get_str(self, name: str, default: str) -> Entity: pass

class FieldOrInputValue:
    def __init__(self, module): pass
    def get(self, name: str, default: Fix32) -> Fix32: pass
    def get_bool(self, name: str, default: bool) -> bool: pass
    def get_int(self, name: str, default: int) -> int: pass
    def get_ent(self, name: str) -> Entity: pass