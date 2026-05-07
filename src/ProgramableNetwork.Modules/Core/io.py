
from email.policy import default
from Mafi import Fix32

class Output:
    def __init__(self, output_id: str, name: str, shared: bool = False):
        """ Declares a module output pin.  When `shared=True`, the display
            `name` is registered ONCE under the shared translation key
            `ProgramableNetwork_PinOrField_<name>` and reused by every other
            module that opts in with the same label — translation files no
            longer accumulate one entry per module per duplicated label. """
        self.id = output_id
        self.name = name
        self.shared = shared

class Input:
    def __init__(self, input_id: str, name: str, shared: bool = False):
        """ Declares a module input pin.  When `shared=True`, the display
            `name` is registered ONCE under the shared translation key
            `ProgramableNetwork_PinOrField_<name>` and reused by every other
            module that opts in with the same label — translation files no
            longer accumulate one entry per module per duplicated label. """
        self.id = input_id
        self.name = name
        self.shared = shared

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

class ArrayValue:
    """ Per-module Fix32 scratch array.  One array per module, persisted with the
        save game.  Use this for ring buffers, history windows, and any other
        index-keyed runtime state — preferable to abusing Output names since the
        array survives field changes only when YOU resize it, has O(1) indexed
        access, and survives no string-key allocation on the hot path.

        Bounds are checked: get() returns the supplied default for out-of-range
        indices, set() silently no-ops, so you don't need your own guard. """

    def __init__(self, module): pass

    @property
    def length(self) -> int:
        """ Current size of the array (0 by default for newly-created modules). """
        pass

    def get(self, idx: int, default: Fix32) -> Fix32:
        """ Read slot `idx` (0-based).  Returns `default` if `idx` is out of range. """
        pass

    def set(self, idx: int, value: Fix32) -> None:
        """ Write slot `idx`.  Silently no-ops if `idx` is out of range — call
            resize() first if you need to grow the array. """
        pass

    def resize(self, size: int, fill_new: Fix32 = 0) -> None:
        """ Grow or shrink the array.  Existing slots are preserved up to
            min(old_size, size); new tail slots (when growing) are filled with
            `fill_new` (defaults to zero).  Negative sizes are clamped to 0.
            Cheap when the size is unchanged.  Allocates a new array on every
            actual size change — call sparingly.

            The `fill_new` parameter exists so Python modules can seed new
            buffer slots without writing a per-tick loop (the parser only
            supports if/else statements). """
        pass

    def clear(self) -> None:
        """ Zero every slot in place.  Does NOT change the array length — use
            resize(0) if you also want to drop the buffer. """
        pass

    def shift_left_with(self, incoming: Fix32) -> Fix32:
        """ Atomic array-copy shift-register step.  Shifts every slot one
            position to the left (slot[i] = slot[i+1]), writes `incoming` to
            the last slot, and returns the value that was previously in
            slot[0].  Empty arrays just echo `incoming` back unchanged.

            Use this to implement a delay / FIR / sliding window from Python
            without a per-tick loop. """
        pass