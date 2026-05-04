from Core.categories import DefaultCategories
from Core.io import Input, Output
from Mafi import Fix32
from Core.module import DefaultControllers, Module

# File written by Nightinggale
# Refactored to a single extensible Runtime_Max — base 2 inputs (A, B) plus
# up to 6 player-added extension pins (C..H) on the right edge of the module.
# Replaces the old Max_2/3/4/6/8 fixed-arity siblings; legacy saves migrate
# through the `deprecates` table with the matching input extension count.
#
# Custom parser constraints (per Modules/ memory): no `for` / `range` / list
# comprehensions — iteration uses recursion, mirroring the existing
# equal_selector / shift modules in this folder.

class Runtime_Max(Module):
    name = "Max"
    description = "Outputs the smallest of all connected inputs (<b>A</b>, <b>B</b>, plus any added extensions) on <b>min</b> and the largest on <b>max</b>. Add more input pins from the right edge of the module. Unconnected inputs are ignored."
    symbol = "MAX"

    inputs = [
        Input("A", "A"),
        Input("B", "B")
    ]

    outputs = [
        Output("min", "Min"),
        Output("max", "Max")
    ]

    width = 2

    # 2 static + up to 6 extensions = 8 inputs total — covers the full range
    # of the deprecated Max_2/3/4/6/8 set without further fragmentation.
    input_extensions = 6

    # Save-compat: each old fixed-arity proto maps to this one with the
    # extension count that reproduces its pin layout.
    deprecates = [
        ["Runtime_Max_2", 0],
        ["Runtime_Max_3", 1],
        ["Runtime_Max_4", 2],
        ["Runtime_Max_6", 4],
        ["Runtime_Max_8", 6]
    ]

    categories = [ DefaultCategories.Arithmetic ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        n = self.effective_input_count
        if n == 0:
            return
        # Seed both extremes with the first input — using the seed as the
        # default for unconnected inputs in the recursion lets later
        # disconnected pins skip past min/max updates entirely.
        seed = self.Input.get(self.effective_input_id(0), Fix32.Zero)
        self._scan(1, n, seed, seed)

    def _scan(self, idx, n, current_min, current_max):
        if idx >= n:
            self.Output.set("min", current_min)
            self.Output.set("max", current_max)
            return
        v = self.Input.get(self.effective_input_id(idx), current_min)
        if v < current_min:
            current_min = v
        if v > current_max:
            current_max = v
        self._scan(idx + 1, n, current_min, current_max)
