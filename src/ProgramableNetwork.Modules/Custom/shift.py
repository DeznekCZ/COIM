from Core.categories import DefaultCategories
from Core.fields import Int32Field
from Core.io import Input, Output
from Mafi import Fix32
from Core.module import DefaultControllers, Module

# File written by Nightinggale
# Refactored to a single extensible Runtime_Shift — base 2 channels (0, 1)
# plus up to 6 player-added extension pin pairs (channels 2..7) on the right
# edge of the module.  Both input and output sides extend in lock-step so
# every channel always has a matching destination.  Legacy saves Shift_2/4/7
# migrate through the `deprecates` table.
#
# Channel iteration uses recursion (no for-loops in the custom parser); the
# pin id at ordinal i comes from `effective_input_id(i)` so the function
# works for any extension count without hardcoding a name table.

class Runtime_Shift(Module):
    name = "Control: Shift"
    description = "Cyclic shifter: <b>index</b> picks how far to rotate the channel inputs (<b>0</b>, <b>1</b>, plus any added extensions) onto the matching outputs.  Output <b>index</b> echoes the wrapped index.  Add more channel pin pairs from the right edge of the module."
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

    # 2 static channels + up to 6 ext = 8 channels total.  Both sides paired.
    input_extensions = 6
    output_extensions = 6

    # Save-compat: each removed fixed-arity proto maps to this one with the
    # paired extension counts that reproduce its channel count.
    #   Shift_2 → 0 ext (2 channels)
    #   Shift_4 → 2 ext (4 channels)
    #   Shift_7 → 5 ext (7 channels)
    deprecates = [
        ["Runtime_Shift_2", 0, 0],
        ["Runtime_Shift_4", 2, 2],
        ["Runtime_Shift_7", 5, 5]
    ]

    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        # Channel count = total effective inputs minus the static "index" pin.
        # `effective_input_id(0)` is "index"; channels start at ordinal 1.
        num_inputs = self.effective_input_count - 1
        if num_inputs < 1:
            return

        shift_offset = self.Input.get_int("index", 0)
        # Same as the original Shift_4/_7: wrap into the channel range with
        # `%` for non-negative indices.  Negative indices land in
        # implementation-defined territory (the legacy module had dead code
        # for them too) — fall through and let it modulo as-is; out-of-range
        # outputs simply get "" and the Output.set call no-ops.
        if shift_offset >= num_inputs:
            shift_offset = shift_offset % num_inputs
        self.Output.set_int("index", shift_offset)

        self._set_output(0, num_inputs, shift_offset)

    def _set_output(self, index, num_inputs, shift_offset):
        out_index = index + shift_offset
        if out_index >= num_inputs:
            out_index = out_index - num_inputs
        # Channel ordinals are 1..num_inputs (slot 0 is the "index" pin),
        # so add 1 when looking up effective ids.
        self.Output.set(
            self.effective_output_id(out_index + 1),
            self.Input.get(self.effective_input_id(index + 1), Fix32.Zero))
        index = index + 1
        if index < num_inputs:
            self._set_output(index, num_inputs, shift_offset)
