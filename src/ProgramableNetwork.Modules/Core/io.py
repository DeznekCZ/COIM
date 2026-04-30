
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
        """ Creates a filler between displays with width of modules (floating sum up to module width) """
        pass

    def LED(input_id: str, name: str) -> DisplayDefinition:
        """ Creates a LED display (1 cell wide).  The action() can drive it via
            module.Display[id] = "1" / "" (truthy/empty string toggles the LED). """
        pass

    def Text(input_id: str, name: str, defaultText: str = "") -> DisplayDefinition:
        """ Creates a text display (1 cell wide).  The action() can update the shown
            text via module.Display[id] = "<text>". """
        pass

    def Icon(input_id: str, name: str, defaultText: str = "") -> DisplayDefinition:
        """ Creates an icon display (1 cell wide).  The action() can swap the icon
            via module.Display[id] = "<icon-path>". """
        pass

    def Slider(input_id: str, name: str, width: Fix32, min: Fix32 = 0, max: Fix32 = 1) -> DisplayDefinition:
        """ Creates a horizontal slider display, 1-4 cells wide.  The action() drives
            the slider position by writing strings to:
              - module.Display[input_id]          = current value (parsed as float)
              - module.Display[input_id + "_min"] = lower bound (overrides default min)
              - module.Display[input_id + "_max"] = upper bound (overrides default max)
            Bounds default to the (min, max) passed here when the auxiliary keys are
            not set.  The slider is read-only — it reflects the value, doesn't accept
            user drag input. """
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