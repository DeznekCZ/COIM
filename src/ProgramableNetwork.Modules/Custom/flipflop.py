from Core.categories import DefaultCategories
from Core.fields import Int32Field
from Core.io import Input, Output, Display
from Mafi import Fix32
from Core.module import DefaultControllers, Module

# File written by Nightinggale
# Optimized: each class extracts a `_latch(in_name, out_name)` and a
# `_led(out_name, led_name)` helper, plus `_show_write()` for the enable
# icon.  IO names are literal so no per-tick string allocation is involved.

SAVE_ICON = "Assets/Unity/UserInterface/General/Save.svg"
WRITE_ICON_ON = "#CAAAA00" + SAVE_ICON
WRITE_ICON_OFF = "#C606060" + SAVE_ICON


class Runtime_FlipFlop_1(Module):
    name = "Control: Flip-Flop (1 input)"
    description = "When <b>enable</b> is on, copies input <b>A</b> to output <b>A</b> and remembers it; while <b>enable</b> is off, the output keeps its last stored value. Single-input D flip-flop."
    symbol = "FLIP-F"
    inputs = [
        Input("enable", "Enable"),
        Input("in_1", "A")
    ]
    outputs = [
        Output("out_1", "A")
    ]
    displays = [
        Display.Icon("write", "Write Enable", WRITE_ICON_OFF),
        Display.LED("led_1", "Stored A")
    ]

    width = 2

    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        if not self.Input.get_bool("enable", False):
            return
        self._latch("in_1", "out_1")

    def Display(self):
        self._show_write()
        self._led("out_1", "led_1")

    def _latch(self, in_name, out_name):
        self.Output.set(out_name, self.Input.get(in_name, Fix32.Zero))

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


class Runtime_FlipFlop_2(Module):
    name = "Control: Flip-Flop (2 inputs)"
    description = "When <b>enable</b> is on, copies inputs <b>A</b> and <b>B</b> to the matching outputs and remembers them; while <b>enable</b> is off, outputs keep their last stored values. Two-channel D flip-flop."
    symbol = "FLIP-FLOP"
    inputs = [
        Input("enable", "Enable"),
        Input("in_1", "A"),
        Input("in_2", "B")
    ]
    outputs = [
        Output("out_1", "A"),
        Output("out_2", "B")
    ]
    displays = [
        Display.Icon("write", "Write Enable", WRITE_ICON_OFF),
        Display.LED("led_1", "Stored A"),
        Display.LED("led_2", "Stored B")
    ]

    width = 3

    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def Action(self):
        if not self.Input.get_bool("enable", False):
            return
        self._latch("in_1", "out_1")
        self._latch("in_2", "out_2")

    def Display(self):
        self._show_write()
        self._led("out_1", "led_1")
        self._led("out_2", "led_2")

    def _latch(self, in_name, out_name):
        self.Output.set(out_name, self.Input.get(in_name, Fix32.Zero))

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


class Runtime_FlipFlop_3(Module):
    name = "Control: Flip-Flop (3 inputs)"
    description = "When <b>enable</b> is on, copies inputs <b>A</b>, <b>B</b>, <b>C</b> to the matching outputs and remembers them; while <b>enable</b> is off, outputs keep their last stored values. Three-channel D flip-flop."
    symbol = "FLIP-FLOP"
    inputs = [
        Input("enable", "Enable"),
        Input("in_1", "A"),
        Input("in_2", "B"),
        Input("in_3", "C")
    ]
    outputs = [
        Output("out_1", "A"),
        Output("out_2", "B"),
        Output("out_3", "C")
    ]
    displays = [
        Display.Icon("write", "Write Enable", WRITE_ICON_OFF),
        Display.LED("led_1", "Stored A"),
        Display.LED("led_2", "Stored B"),
        Display.LED("led_3", "Stored C")
    ]

    width = 4

    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def Action(self):
        if not self.Input.get_bool("enable", False):
            return
        self._latch("in_1", "out_1")
        self._latch("in_2", "out_2")
        self._latch("in_3", "out_3")

    def Display(self):
        self._show_write()
        self._led("out_1", "led_1")
        self._led("out_2", "led_2")
        self._led("out_3", "led_3")

    def _latch(self, in_name, out_name):
        self.Output.set(out_name, self.Input.get(in_name, Fix32.Zero))

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


class Runtime_FlipFlop_4(Module):
    name = "Control: Flip-Flop (4 inputs)"
    description = "When <b>enable</b> is on, copies inputs <b>A</b> through <b>D</b> to the matching outputs and remembers them; while <b>enable</b> is off, outputs keep their last stored values. Four-channel D flip-flop."
    symbol = "FLIP-FLOP"
    inputs = [
        Input("enable", "Enable"),
        Input("in_1", "A"),
        Input("in_2", "B"),
        Input("in_3", "C"),
        Input("in_4", "D")
    ]
    outputs = [
        Output("out_1", "A"),
        Output("out_2", "B"),
        Output("out_3", "C"),
        Output("out_4", "D")
    ]
    displays = [
        Display.Icon("write", "Write Enable", WRITE_ICON_OFF),
        Display.LED("led_1", "Stored A"),
        Display.LED("led_2", "Stored B"),
        Display.LED("led_3", "Stored C"),
        Display.LED("led_4", "Stored D")
    ]

    width = 5

    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        if not self.Input.get_bool("enable", False):
            return
        self._latch("in_1", "out_1")
        self._latch("in_2", "out_2")
        self._latch("in_3", "out_3")
        self._latch("in_4", "out_4")

    def Display(self):
        self._show_write()
        self._led("out_1", "led_1")
        self._led("out_2", "led_2")
        self._led("out_3", "led_3")
        self._led("out_4", "led_4")

    def _latch(self, in_name, out_name):
        self.Output.set(out_name, self.Input.get(in_name, Fix32.Zero))

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


class Runtime_FlipFlop_7(Module):
    name = "Control: Flip-Flop (7 inputs)"
    description = "When <b>enable</b> is on, copies inputs <b>A</b> through <b>G</b> to the matching outputs and remembers them; while <b>enable</b> is off, outputs keep their last stored values. Seven-channel D flip-flop."
    symbol = "FLIP-FLOP"
    inputs = [
        Input("enable", "Enable"),
        Input("in_1", "A"),
        Input("in_2", "B"),
        Input("in_3", "C"),
        Input("in_4", "D"),
        Input("in_5", "E"),
        Input("in_6", "F"),
        Input("in_7", "G")
    ]
    outputs = [
        Output("out_1", "A"),
        Output("out_2", "B"),
        Output("out_3", "C"),
        Output("out_4", "D"),
        Output("out_5", "E"),
        Output("out_6", "F"),
        Output("out_7", "G")
    ]
    displays = [
        Display.Icon("write", "Write Enable", WRITE_ICON_OFF),
        Display.LED("led_1", "Stored A"),
        Display.LED("led_2", "Stored B"),
        Display.LED("led_3", "Stored C"),
        Display.LED("led_4", "Stored D"),
        Display.LED("led_5", "Stored E"),
        Display.LED("led_6", "Stored F"),
        Display.LED("led_7", "Stored G")
    ]

    width = 8

    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def Action(self):
        if not self.Input.get_bool("enable", False):
            return
        self._latch("in_1", "out_1")
        self._latch("in_2", "out_2")
        self._latch("in_3", "out_3")
        self._latch("in_4", "out_4")
        self._latch("in_5", "out_5")
        self._latch("in_6", "out_6")
        self._latch("in_7", "out_7")

    def Display(self):
        self._show_write()
        self._led("out_1", "led_1")
        self._led("out_2", "led_2")
        self._led("out_3", "led_3")
        self._led("out_4", "led_4")
        self._led("out_5", "led_5")
        self._led("out_6", "led_6")
        self._led("out_7", "led_7")

    def _latch(self, in_name, out_name):
        self.Output.set(out_name, self.Input.get(in_name, Fix32.Zero))

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
