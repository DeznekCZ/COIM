from Core.categories import DefaultCategories
from Core.fields import Int32Field, BooleanField
from Core.io import Input, Output
from Mafi import Fix32
from Core.module import DefaultControllers, Module

# File written by Nightinggale

class Runtime_hysteresis(Module):
    name = "Control: Hysteresis (1 input)"
    symbol = "↑↓"

    inputs = [
        Input("Input", "Input")
    ]

    outputs = [
        Output("Output", "Output")
    ]

    fields = [
        Int32Field("On", "On", "Output turns on when input is bigger than this", 100),
        Int32Field("Off", "Off", "Output turns off when input is smaller than this", 0),
        BooleanField("Invert", "Invert Output", "When on, the output will be on when input is low and off when input is high", False)

    ]
   
    width = 1
    
    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        input = self.Input.get_int("Input", 0)
        invert = self.Field.get_bool("Invert", False)
        if input >= self.Field.get_int("On", 0):
            self.Output.set_bool("Output", not invert)
        elif input <= self.Field.get_int("Off", 0):
            self.Output.set_bool("Output", invert)


class Runtime_hysteresis_3(Module):
    name = "Control: Hysteresis (3 inputs)"
    symbol = "↑↓"

    inputs = [
        Input("Input", "Input"),
        Input("On", "On", "Output turns on when input is bigger than this"),
        Input("Off", "Off", "Output turns off when input is smaller than this")
    ]

    outputs = [
        Output("Output", "Output")
    ]

    fields = [
        BooleanField("Invert", "Invert Output", "When on, the output will be on when input is low and off when input is high", False)

    ]

    width = 3

    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        input = self.Input.get("Input", Fix32.Zero)
        invert = self.Field.get_bool("Invert", False)
        if input >= self.Input.get("On", Fix32.Zero):
            self.Output.set_bool("Output", not invert)
        elif input <= self.Input.get("Off", Fix32.Zero):
            self.Output.set_bool("Output", invert)

