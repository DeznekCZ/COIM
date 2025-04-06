from Core.categories import DefaultCategories
from Core.fields import Int32Field
from Core.io import Input, Output
from Mafi import Fix32
from Core.module import DefaultControllers, Module

# File written by Nightinggale

class Runtime_Equal_Selector_7(Module):
    name = "Control: Equal Selector (7 inputs)"
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

    def action(self):
        main = self.Input.get("main", Fix32.Zero)

        if main == self.Input.get("in_6", Fix32.Zero):
            self.Output.set_int("matching_index", 6)
            self.Output.set_bool("output", True)
            return

        if main == self.Input.get("in_5", Fix32.Zero):
            self.Output.set_int("matching_index", 5)
            self.Output.set_bool("output", True)
            return

        if main == self.Input.get("in_4", Fix32.Zero):
            self.Output.set_int("matching_index", 4)
            self.Output.set_bool("output", True)
            return

        if main == self.Input.get("in_3", Fix32.Zero):
            self.Output.set_int("matching_index", 3)
            self.Output.set_bool("output", True)
            return

        if main == self.Input.get("in_2", Fix32.Zero):
            self.Output.set_int("matching_index", 2)
            self.Output.set_bool("output", True)
            return

        if main == self.Input.get("in_1", Fix32.Zero):
            self.Output.set_int("matching_index", 1)
            self.Output.set_bool("output", True)
            return

        if main == self.Input.get("in_0", Fix32.Zero):
            self.Output.set_int("matching_index", 0)
            self.Output.set_bool("output", True)
            return

        self.Output.set_int("matching_index", 99)
        self.Output.set_bool("output", False)
