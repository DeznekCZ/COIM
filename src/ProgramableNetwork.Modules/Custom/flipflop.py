from Core.categories import DefaultCategories
from Core.io import Input, Output, Display
from Mafi import Fix32
from Core.module import DefaultControllers, Module

# File written by Nightinggale
# Refactored to a single extensible Runtime_FlipFlop — base 1 channel
# (in_1 / out_1) plus up to 6 player-added channel pairs.  Both input
# and output sides extend in lock-step (paired data pins).  Legacy saves
# FlipFlop_1/2/3/4/7 migrate through the `deprecates` table.
#
# The display row is fixed at the maximum 7 channels: a "write" enable
# icon plus seven LEDs (led_1..led_7).  LEDs for inactive channels stay
# dark via the `_led` helper, matching the original LEDs' off-state.

SAVE_ICON = "Assets/Unity/UserInterface/General/Save.svg"
WRITE_ICON_ON = "#CAAAA00" + SAVE_ICON
WRITE_ICON_OFF = "#C606060" + SAVE_ICON


class Runtime_FlipFlop(Module):
    name = "Control: Flip-Flop"
    description = "When <b>enable</b> is on, copies each connected channel input to the matching output and remembers it; while <b>enable</b> is off, outputs keep their last stored values. Add more channel pin pairs from the right edge of the module."
    symbol = "FLIP-FLOP"

    inputs = [
        Input("enable", "Enable"),
        Input("in_1", "A")
    ]
    outputs = [
        Output("out_1", "A")
    ]
    # Static displays: write-enable icon + the LED for the static channel.
    displays = [
        Display.Icon("write", "Write Enable", WRITE_ICON_OFF),
        Display.LED("led_1", "Stored A")
    ]
    # Per-output-extension LEDs — one materialises for each channel the player
    # adds via the right-edge "+" button, paired in lock-step with the matching
    # output pin (out_2 ↔ led_2, etc.).  `extension_displays_link = "output"`
    # tells the renderer to grow this list with the active output extensions.
    extension_displays = [
        Display.LED("led_2", "Stored B"),
        Display.LED("led_3", "Stored C"),
        Display.LED("led_4", "Stored D"),
        Display.LED("led_5", "Stored E"),
        Display.LED("led_6", "Stored F"),
        Display.LED("led_7", "Stored G")
    ]
    extension_displays_link = "output"

    # Base width = 2 cells (write icon + first LED).  Adding output extensions
    # widens the module on the right; the pin and the LED appear together.
    width = 2

    # 1 static + up to 6 ext = 7 channels total — covers Flip-Flop_7.
    input_extensions = 6
    output_extensions = 6

    # Multi-char pin ids (in_2.. / out_2..) need explicit names — the default
    # alphabet namer only handles single-char ids.
    input_extension_names = [
        ["in_2", "B"],
        ["in_3", "C"],
        ["in_4", "D"],
        ["in_5", "E"],
        ["in_6", "F"],
        ["in_7", "G"]
    ]
    output_extension_names = [
        ["out_2", "B"],
        ["out_3", "C"],
        ["out_4", "D"],
        ["out_5", "E"],
        ["out_6", "F"],
        ["out_7", "G"]
    ]

    deprecates = [
        ["Runtime_FlipFlop_1", 0, 0],
        ["Runtime_FlipFlop_2", 1, 1],
        ["Runtime_FlipFlop_3", 2, 2],
        ["Runtime_FlipFlop_4", 3, 3],
        ["Runtime_FlipFlop_7", 6, 6]
    ]

    # Pre-built LED-id table — looked up by channel ordinal in `Display(self)`.
    # Slot 0 unused so `LED_NAMES[channel]` maps the natural 1-based channel
    # number to its display id without any per-tick string concatenation
    # (the custom parser has no `str(int)` and we'd otherwise be paying for an
    # if-cascade every redraw).  Mirrors the IO_NAMES pattern in shift.py.
    LED_NAMES = ["", "led_1", "led_2", "led_3", "led_4", "led_5", "led_6", "led_7"]

    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        if not self.Input.get_bool("enable", False):
            return
        # Latch every effective data pin pair (slot 0 is "enable", channels 1..N).
        n = self.effective_input_count
        if n <= 1:
            return
        self._latch(1, n)

    def _latch(self, idx, n):
        if idx >= n:
            return
        in_name = self.effective_input_id(idx)
        out_name = self.effective_output_id(idx)
        self.Output.set(out_name, self.Input.get(in_name, Fix32.Zero))
        self._latch(idx + 1, n)

    def Display(self):
        self._show_write()
        # LEDs are paired with output channels — render one per active channel.
        # effective_output_count counts statics + active extensions; ordinal i
        # maps to LED_NAMES[i+1] (slot 0 is the placeholder for the index shift).
        self._led_scan(0, self.effective_output_count)

    def _led_scan(self, idx, n):
        if idx >= n:
            return
        self._led(self.effective_output_id(idx), self.LED_NAMES[idx + 1])
        self._led_scan(idx + 1, n)

    def _led(self, out_name, led_name):
        if self.Output.get(out_name, Fix32.Zero) != Fix32.Zero:
            self.Display.set(led_name, "stored")
        else:
            self.Display.set(led_name, "")

    def _show_write(self):
        if self.Input.get_bool("enable", False):
            self.Display.set("write", WRITE_ICON_ON)
        else:
            self.Display.set("write", WRITE_ICON_OFF)
