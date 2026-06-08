from Core.categories import DefaultCategories
from Core.fields import Int32Field, BooleanField
from Core.io import Input, Output, Display
from Mafi import Fix32
from Core.module import DefaultControllers, Module

# File written by Nightinggale
# Inplements hysterisis in order to make it easier to avoid signals turning on/off repeatably around the trigger value.
# Vanilla has hysterisis in turbine auto-balance to make them run in long bursts rather than quickly on/off
#
# For more theory behind this
# https://en.wikipedia.org/wiki/Hysteresis

class Runtime_hysteresis(Module):
    name = "Control: Hysteresis"
    description = "Uses <b>hysteresis</b> have different threshold for turning a signal on or off."
    symbol = "HYS"

    inputs = [
        Input("Input", "Input")
    ]

    outputs = [
        Output("Output", "Output")
    ]

    fields = [
        Int32Field("High", "High", "Activate when output when input is >= this.<br>Uses input A instead if input pin is shown.", 99),
        Int32Field("Low", "Low", "Deactivate when output when input is <= this.<br>Uses input B instead if input pin is shown.", 1),
        BooleanField("DefaultOutput", "Value while active", "Toggles which end of the thresholds should turn output on", True)
    ]
    
    displays = [
        Display.LED("active", "Active")
    ]

    width = 1

    input_extensions = 2

    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        value = self.Input.get_int("Input", 0)
        
        if (value >= self.getHigh()):
            self.Output.set_bool("Output", self.Field.get_bool("DefaultOutput", False))
        elif (value <= self.getLow()):
            self.Output.set_bool("Output", not self.Field.get_bool("DefaultOutput", False))

    def getHigh(self):
        if (self.effective_input_count > 1):
            return self.Input.get_int("A", 0)
        else:
            return self.Field.get_int("High", 0)

    def getLow(self):
        if (self.effective_input_count == 3):
            return self.Input.get_int("B", 0)
        else:
            return self.Field.get_int("Low", 0)

    def Display(self):
        # show LED when enabled
        if self.Output.get_bool("Output", False):
            self.Display.set("active", "on")
        else:
            self.Display.set("active", "")
