from Core.categories import DefaultCategories
from Core.io import Input, Output
from Mafi import Fix32
from Core.module import DefaultControllers, Module

# File written by Nightinggale
# Refactored to a single extensible Runtime_Equal_Selector — base "main" + 1
# data input (in_0); up to 6 player-added data pins (in_1..in_6) on the right
# edge.  Replaces the legacy Equal_Selector_7; saves migrate via `deprecates`.
#
# Iteration scans descending from the highest active input via recursion so
# the lowest matching index wins.  Pin ids are pulled from the effective list
# rather than a hardcoded name table.

class Runtime_Equal_Selector(Module):
    name = "Control: Equal Selector"
    description = "Compares <b>main</b> against the connected data inputs (<b>in_0</b>, plus any added extensions); outputs the index of the lowest match on <b>matching_index</b> and sets <b>output</b> to 1.  If nothing matches, <b>matching_index</b> is 99 and <b>output</b> is 0.  Add more input pins from the right edge of the module."
    symbol = "MS"
    inputs = [
        Input("main", "Main Input"),
        Input("in_0", "Input 0")
    ]
    outputs = [
        Output("matching_index", "Index of equal input"),
        Output("output", "Match with any input")
    ]

    width = 2

    # 1 static data pin (in_0) + up to 6 extensions (in_1..in_6) = 7 data pins.
    # The "main" pin doesn't count toward extensions; it's always at slot 0.
    input_extensions = 6

    # Multi-char ids — must be specified explicitly; the default namer handles
    # single-char ids only.
    input_extension_names = [
        ["in_1", "Input 1"],
        ["in_2", "Input 2"],
        ["in_3", "Input 3"],
        ["in_4", "Input 4"],
        ["in_5", "Input 5"],
        ["in_6", "Input 6"]
    ]

    deprecates = [
        ["Runtime_Equal_Selector_7", 6]
    ]

    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        # effective_input_count = 1 ("main") + 1 ("in_0") + active extensions.
        # Data-pin slot range is [1, count); start the descending scan at the
        # highest active data pin so a tie returns the lowest matching index.
        count = self.effective_input_count
        if count <= 1:
            self.Output.set_int("matching_index", 99)
            self.Output.set_bool("output", False)
            return
        self._scan(self.Input.get("main", Fix32.Zero), count - 1)

    def _scan(self, main, slot):
        if slot < 1:
            self.Output.set_int("matching_index", 99)
            self.Output.set_bool("output", False)
            return
        name = self.effective_input_id(slot)
        if main == self.Input.get(name, Fix32.Zero):
            # Data-pin "in_N" lives at slot N+1 ("main" is slot 0, "in_0" is slot 1).
            # Report the data-pin index N — i.e. slot - 1 — to match the legacy id.
            self.Output.set_int("matching_index", slot - 1)
            self.Output.set_bool("output", True)
            return
        self._scan(main, slot - 1)
