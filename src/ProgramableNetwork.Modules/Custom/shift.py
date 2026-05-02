from Core.categories import DefaultCategories
from Core.fields import Int32Field
from Core.io import Input, Output
from Mafi import Fix32
from Core.module import DefaultControllers, Module

# File written by Nightinggale
# Optimized: input/output names are pre-built once as a class-level list
# (IO_NAMES) and indexed by the loop variable.  This replaces both the
# 17-line `str()` map and the per-tick string concatenations the original
# would have done with `"" + n` — zero string allocations per tick.

class Runtime_Shift_2(Module):
    name = "Control: Shift (2 inputs)"
    description = "Cyclic shifter for 2 channels: <b>index</b> (mod 2) selects how far to rotate inputs <b>0</b>, <b>1</b> onto outputs <b>0</b>, <b>1</b>. Output <b>index</b> echoes the wrapped index."
    symbol = "SHIFT"

    inputs = [
        Input("index", "Index"),
        Input("0", "Input 0"),
        Input("1", "Input 1")
    ]

    outputs = [
        Output("index", "Index"),
        Output("0", "Output 0"),
        Output("1", "Output 1")
    ]

    width = 3

    categories = [DefaultCategories.Control]
    controllers = [DefaultControllers.Controller]

    def action(self):
        # the action isn't copy paste like in the other sizes
        # the reason being that this is so simple that writing each case is reasonable and it executes faster.
        index = self.Input.get_int("index", 0)
        index = index % 2
        self.Output.set_int("index", index)

        if index == 0:
            self.Output.set("0", self.Input.get("0", Fix32.Zero))
            self.Output.set("1", self.Input.get("1", Fix32.Zero))
        else:
            self.Output.set("0", self.Input.get("1", Fix32.Zero))
            self.Output.set("1", self.Input.get("0", Fix32.Zero))


class Runtime_Shift_4(Module):
    name = "Control: Shift (4 inputs)"
    description = "Cyclic shifter for 4 channels: <b>index</b> (mod 4) selects how far to rotate inputs <b>0</b>..<b>3</b> onto outputs <b>0</b>..<b>3</b>. Output <b>index</b> echoes the wrapped index."
    symbol = "SHIFT"

    inputs = [
        Input("index", "Index"),
        Input("0", "Input 0"),
        Input("1", "Input 1"),
        Input("2", "Input 2"),
        Input("3", "Input 3")
    ]

    outputs = [
        Output("index", "Index"),
        Output("0", "Output 0"),
        Output("1", "Output 1"),
        Output("2", "Output 2"),
        Output("3", "Output 3")
    ]

    fields = [
        Int32Field("inputs used", "Number of inputs used", "Select how many inputs should be used. The same amount of outputs will then be used, leaving the rest of the outputs not updating.", 4)
    ]

    width = 5

    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    # Pre-built name table — one allocation at class-load time, reused every
    # tick.  Indexed by the input/output number; slot 0 unused so that the
    # natural numeric index maps directly without subtracting.
    IO_NAMES = ["0", "1", "2", "3"]

    def action(self):
        num_inputs = 4

        requested_num_inputs = self.Field.get_int("inputs used", num_inputs)
        if requested_num_inputs < 2:
            requested_num_inputs = 2
        if requested_num_inputs < num_inputs:
            num_inputs = requested_num_inputs

        # set index to the range matching the number of inputs
        shift_offset = self.Input.get_int("index", 0)
        if shift_offset < 0:
            # negative numbers are shifted to positive.
            # multiplying with num_inputs will ensure the post modulo number won't be affected by this offset.
            temp = shift_offset * num_inputs
            shift_count = shift_offset - temp
        if shift_offset >= num_inputs:
            shift_offset = shift_offset % num_inputs
        self.Output.set_int("index", shift_offset)

        self.set_output(0, num_inputs, shift_offset)

    def set_output(self, index, num_inputs, shift_offset):
        out_index = index + shift_offset
        if out_index >= num_inputs:
            out_index = out_index - num_inputs
        self.Output.set(self.IO_NAMES[out_index], self.Input.get(self.IO_NAMES[index], Fix32.Zero))
        index = index + 1
        if index < num_inputs:
            self.set_output(index, num_inputs, shift_offset)


class Runtime_Shift_7(Module):
    name = "Control: Shift (7 inputs)"
    description = "Cyclic shifter for 7 channels: <b>index</b> (mod 7) selects how far to rotate inputs <b>0</b>..<b>6</b> onto outputs <b>0</b>..<b>6</b>. Output <b>index</b> echoes the wrapped index."
    symbol = "SHIFT"

    inputs = [
        Input("index", "Index"),
        Input("0", "Input 0"),
        Input("1", "Input 1"),
        Input("2", "Input 2"),
        Input("3", "Input 3"),
        Input("4", "Input 4"),
        Input("5", "Input 5"),
        Input("6", "Input 6")
    ]

    outputs = [
        Output("index", "Index"),
        Output("0", "Output 0"),
        Output("1", "Output 1"),
        Output("2", "Output 2"),
        Output("3", "Output 3"),
        Output("4", "Output 4"),
        Output("5", "Output 5"),
        Output("6", "Output 6")
    ]

    fields = [
        Int32Field("inputs used", "Number of inputs used", "Select how many inputs should be used. The same amount of outputs will then be used, leaving the rest of the outputs not updating.", 7)
    ]

    width = 8

    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    IO_NAMES = ["0", "1", "2", "3", "4", "5", "6"]

    def action(self):
        num_inputs = 7

        requested_num_inputs = self.Field.get_int("inputs used", num_inputs)
        if requested_num_inputs < 2:
            requested_num_inputs = 2
        if requested_num_inputs < num_inputs:
            num_inputs = requested_num_inputs

        # set index to the range matching the number of inputs
        shift_offset = self.Input.get_int("index", 0)
        if shift_offset < 0:
            # negative numbers are shifted to positive.
            # multiplying with num_inputs will ensure the post modulo number won't be affected by this offset.
            temp = shift_offset * num_inputs
            shift_count = shift_offset - temp
        if shift_offset >= num_inputs:
            shift_offset = shift_offset % num_inputs
        self.Output.set_int("index", shift_offset)

        self.set_output(0, num_inputs, shift_offset)

    def set_output(self, index, num_inputs, shift_offset):
        out_index = index + shift_offset
        if out_index >= num_inputs:
            out_index = out_index - num_inputs
        self.Output.set(self.IO_NAMES[out_index], self.Input.get(self.IO_NAMES[index], Fix32.Zero))
        index = index + 1
        if index < num_inputs:
            self.set_output(index, num_inputs, shift_offset)
