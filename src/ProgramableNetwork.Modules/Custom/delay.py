from Core.categories import DefaultCategories
from Core.fields import Int32Field
from Core.io import Input, Output
from Core.module import DefaultControllers, Module

from Mafi import Fix32


# Reference example for the Module.Array API.  Implements a configurable
# multi-tick delay as an array-copy shift register: every tick reads slot[0],
# shifts the buffer left by one, and writes the new input at the end.  All of
# that happens inside Module.Array.shift_left_with() — Python's parser has no
# for/while loop, so the loop is in C#.  Resize seeds new slots with the most
# recent sample so the output stays continuous when the delay field grows.
class Runtime_Delay_1(Module):
    name = "Control: Delay (configurable)"
    description = "Delays input <b>input</b> by the number of ticks set in the <b>delay</b> field, then emits the same signal on <b>output</b>. A delay of 1 (or less) passes the value through unchanged. Maximum delay is 64 ticks. When the field is increased the new tail of the buffer is seeded with the most recent sample so the output stays continuous; when shrunk the oldest pending samples continue to emit in order."
    symbol = "DLY"
    inputs = [
        Input("input", "Signal input")
    ]
    outputs = [
        Output("output", "Signal output")
    ]
    fields = [
        Int32Field("delay", "Delay", "How many ticks the output should be delayed (capped at 64)", 1)
    ]
    width = 1
    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        input = self.Input.get("input", Fix32.Zero)
        delay = self.Field.get_int("delay", 1)
        if delay > 64:
            delay = 64

        # Pass-through fast path — drop the buffer if the field shrank.
        if delay <= 1:
            if self.Array.length > 0:
                self.Array.resize(0)
            self.Output.set("output", input)
            return

        # Keep buffer sized to the field.  On grow, seed new tail slots with
        # the most recent sample so the output stays continuous; on shrink,
        # resize() truncates from the tail (oldest pending samples keep
        # emitting in order).
        old_length = self.Array.length
        if old_length != delay:
            last_value = Fix32.Zero
            if old_length > 0:
                last_value = self.Array.get(old_length - 1, Fix32.Zero)
            self.Array.resize(delay, last_value)

        # Atomic shift-register step: shift left, push input at end, return
        # what was at slot[0].  That value has been waiting `delay` ticks.
        oldest = self.Array.shift_left_with(input)
        self.Output.set("output", oldest)


class Runtime_Delay_2(Module):
    name = "Control: Delay (2 ticks)"
    description = "Two-stage shift register: outputs <b>1</b> and <b>2</b> carry the input from 1 and 2 ticks ago respectively, refreshed each tick from <b>0</b>."
    symbol = "DLY"
    inputs = [
        Input("0", "Signal input")
    ]
    outputs = [
        Output("1", "Delay by 1 tick"),
        Output("2", "Delay by 2 ticks")
    ]
    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        a = self.Input.get("0", Fix32.Zero)
        self.Output.set("2", self.Output.get("1", Fix32.Zero))
        self.Output.set("1", a)

class Runtime_Delay_4(Module):
    name = "Control: Delay (4 ticks)"
    description = "Four-stage shift register: outputs <b>1</b>..<b>4</b> carry the input from 1 to 4 ticks ago respectively, refreshed each tick from <b>0</b>."
    symbol = "DELAY"
    inputs = [
        Input("0", "Signal input")
    ]
    outputs = [
        Output("1", "Delay by 1 tick"),
        Output("2", "Delay by 2 ticks"),
        Output("3", "Delay by 3 ticks"),
        Output("4", "Delay by 4 ticks")
    ]
    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        a = self.Input.get("0", Fix32.Zero)
        self.Output.set("4", self.Output.get("3", Fix32.Zero))
        self.Output.set("3", self.Output.get("2", Fix32.Zero))
        self.Output.set("2", self.Output.get("1", Fix32.Zero))
        self.Output.set("1", a)

class Runtime_Delay_6(Module):
    name = "Control: Delay (6 ticks)"
    description = "Six-stage shift register: outputs <b>1</b>..<b>6</b> carry the input from 1 to 6 ticks ago respectively, refreshed each tick from <b>0</b>."
    symbol = "DELAY"
    inputs = [
        Input("0", "Signal input")
    ]
    outputs = [
        Output("1", "Delay by 1 tick"),
        Output("2", "Delay by 2 ticks"),
        Output("3", "Delay by 3 ticks"),
        Output("4", "Delay by 4 ticks"),
        Output("5", "Delay by 5 ticks"),
        Output("6", "Delay by 6 ticks")
    ]
    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        a = self.Input.get("0", Fix32.Zero)
        self.Output.set("6", self.Output.get("5", Fix32.Zero))
        self.Output.set("5", self.Output.get("4", Fix32.Zero))
        self.Output.set("4", self.Output.get("3", Fix32.Zero))
        self.Output.set("3", self.Output.get("2", Fix32.Zero))
        self.Output.set("2", self.Output.get("1", Fix32.Zero))
        self.Output.set("1", a)

class Runtime_Delay_8(Module):
    name = "Control: Delay (8 ticks)"
    description = "Eight-stage shift register: outputs <b>1</b>..<b>8</b> carry the input from 1 to 8 ticks ago respectively, refreshed each tick from <b>0</b>."
    symbol = "DELAY"
    inputs = [
        Input("0", "Signal input")
    ]
    outputs = [
        Output("1", "Delay by 1 tick"),
        Output("2", "Delay by 2 ticks"),
        Output("3", "Delay by 3 ticks"),
        Output("4", "Delay by 4 ticks"),
        Output("5", "Delay by 5 ticks"),
        Output("6", "Delay by 6 ticks"),
        Output("7", "Delay by 7 ticks"),
        Output("8", "Delay by 8 ticks")
    ]
    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        a = self.Input.get("0", Fix32.Zero)
        self.Output.set("8", self.Output.get("7", Fix32.Zero))
        self.Output.set("7", self.Output.get("6", Fix32.Zero))
        self.Output.set("6", self.Output.get("5", Fix32.Zero))
        self.Output.set("5", self.Output.get("4", Fix32.Zero))
        self.Output.set("4", self.Output.get("3", Fix32.Zero))
        self.Output.set("3", self.Output.get("2", Fix32.Zero))
        self.Output.set("2", self.Output.get("1", Fix32.Zero))
        self.Output.set("1", a)
