
from email.policy import default
from Mafi import Fix32

class Output:
    def __init__(self, output_id: str, name: str):
        self.id = output_id
        self.name = name

class Input:
    def __init__(self, input_id: str, name: str):
        self.id = input_id
        self.name = name

class DisplayDefinition:
    pass

class Display:
    def Filler(width: Fix32) -> DisplayDefinition:
        """ Creates an filler between displays with width of modules (floating sum up to module width) """
        pass

    def LED(input_id: str, name: str):
        """ Creates a led display """
        pass

    def Text(input_id: str, name: str, defaultText: str = ""):
        """ Creates a text display """
        pass

    def Icon(input_id: str, name: str, defaultText: str = ""):
        """ Creates an icon display """
        pass

class InputValue:
    def __init__(self, module): pass
    def set(self, name: str, value: Fix32) -> None: pass
    def get(self, name: str, default: float) -> Fix32: pass
    def set_bool(self, name: str, value: bool) -> None: pass
    def get_bool(self, name: str, default: bool) -> bool: pass
    def set_int(self, name: str, value: bool) -> None: pass
    def get_int(self, name: str, default: bool) -> bool: pass

class FieldOrInputValue:
    def __init__(self, module): pass
    def get(self, name: str, default: Fix32) -> Fix32: pass
    def get_bool(self, name: str, default: bool) -> bool: pass
    def get_int(self, name: str, default: bool) -> bool: pass

class OutputValue:
    def __init__(self, module): pass
    def set(self, name: str, value: Fix32) -> None: pass
    def get(self, name: str, default: Fix32) -> Fix32: pass
    def set_bool(self, name: str, value: bool) -> None: pass
    def get_bool(self, name: str, default: bool) -> bool: pass
    def set_int(self, name: str, value: bool) -> None: pass
    def get_int(self, name: str, default: bool) -> bool: pass

class DisplayValue:
    def __init__(self, module): pass
    def set(self, name: str, value: str) -> None: pass
    def get(self, name: str, default: str) -> str: pass