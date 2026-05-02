from Core.categories import DefaultCategories
from Core.fields import Int32Field
from Core.io import Input, Output
from Mafi import Fix32
from Core.module import DefaultControllers, Module

# File written by Nightinggale
# Optimized: scans descending from the highest-numbered input via recursion.
# Input names are pre-built once as a class-level list (slot 0 valid here —
# inputs are indexed in_0..in_6) so there are no per-tick string allocations.

class Runtime_Equal_Selector_7(Module):
    name = "Control: Equal Selector (7 inputs)"
    description = "Compares <b>main</b> against inputs <b>in_0</b>..<b>in_6</b>; outputs the index of the first match on <b>matching_index</b> and sets <b>output</b> to 1. If nothing matches, <b>matching_index</b> is 99 and <b>output</b> is 0."
    symbol = "MS"
    inputs = [
        Input("main", "Main Input"),
        Input("in_0", "Input 0"),
        Input("in_1", "Input 1"),
        Input("in_2", "Input 2"),
        Input("in_3", "Input 3"),
        Input("in_4", "Input 4"),
        Input("in_5", "Input 5"),
        Input("in_6", "Input 6")
    ]
    outputs = [
        Output("matching_index", "Index of equal input"),
        Output("output", "Match with any input")
    ]

    width = 8

    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    INPUT_NAMES = ["in_0", "in_1", "in_2", "in_3", "in_4", "in_5", "in_6"]

    def action(self):
        self._scan(self.Input.get("main", Fix32.Zero), 6)

    def _scan(self, main, n):
        if n < 0:
            self.Output.set_int("matching_index", 99)
            self.Output.set_bool("output", False)
            return
        if main == self.Input.get(self.INPUT_NAMES[n], Fix32.Zero):
            self.Output.set_int("matching_index", n)
            self.Output.set_bool("output", True)
            return
        self._scan(main, n - 1)
