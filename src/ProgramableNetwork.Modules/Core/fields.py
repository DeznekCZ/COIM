from Mafi import Fix32
from Mafi.Core.Entities import Entity
from Core.mafi import fix

class Field:
    def __init__(self, field_id: str, name: str, short_desc: str):
        self.id = field_id
        self.name = name
        self.short_desc = short_desc

class Int32Field(Field):
    def __init__(self, field_id: str, name: str, short_desc="", default_value=0, show_in_tooltip=False):
        super().__init__(field_id, name, short_desc)
        self.default_value = default_value
        self.show_in_tooltip = show_in_tooltip

class Fix32Field(Field):
    def __init__(self, field_id: str, name: str, short_desc="", default_value=fix(0), show_in_tooltip=False):
        super().__init__(field_id, name, short_desc)
        self.default_value = default_value
        self.show_in_tooltip = show_in_tooltip

class StringField(Field):
    # multilined: when True the field's editor in the inspector becomes a multi-line
    # text area (~4 rows tall) instead of the default single-line input.
    # show_in_tooltip: when True the module's hover tooltip (only in Edit mode) lists
    # this field's current value alongside any error/status text.
    def __init__(self, field_id: str, name: str, short_desc="", default_value="", multilined=False, show_in_tooltip=False):
        super().__init__(field_id, name, short_desc)
        self.default_value = default_value
        self.multilined = multilined
        self.show_in_tooltip = show_in_tooltip

class BooleanField(Field):
    def __init__(self, field_id: str, name: str, short_desc="", default_value=False, show_in_tooltip=False):
        super().__init__(field_id, name, short_desc)
        self.default_value = default_value
        self.show_in_tooltip = show_in_tooltip

class EntityField(Field):
    def __init__(self, entity_type: type[Entity] | list[type[Entity]], field_id: str, name: str, short_desc: str = None, distance: float = 5, entity_filter=lambda module, entity: True, show_in_tooltip=False):
        super().__init__(field_id, name, short_desc)
        self.type = entity_type;
        self.distance = distance
        self.filter = entity_filter
        self.show_in_tooltip = show_in_tooltip

class FieldValue:
    def __init__(self, module): pass
    def get(self, name: str, default: Fix32) -> Fix32: pass
    def get_bool(self, name: str, default: bool) -> bool: pass
    def get_int(self, name: str, default: int) -> int: pass
    def get_ent(self, name: str) -> Entity: pass
    def get_str(self, name: str, default: str) -> str: pass
    def set(self, name: str, value: Fix32): pass
    def set_bool(self, name: str, value: bool): pass
    def set_int(self, name: str, value: int): pass
    def set_str(self, name: str, value: str): pass

class FieldOrInputValue:
    def __init__(self, module): pass
    def get(self, name: str, default: Fix32) -> Fix32: pass
    def get_bool(self, name: str, default: bool) -> bool: pass
    def get_int(self, name: str, default: int) -> int: pass
    def get_ent(self, name: str) -> Entity: pass