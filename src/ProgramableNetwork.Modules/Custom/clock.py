from Core.categories import DefaultCategories
from Core.fields import Int32Field, BooleanField
from Core.io import Input, Output
from Mafi import Fix32
from Core.module import DefaultControllers, Module

# File written by Nightinggale

class Runtime_Clock_1(Module):
    name = "Control: Clock (1 output)"
    symbol = "CLK"

    outputs = [
        Output("clock", "Clock")
    ]

    fields = [
        BooleanField("mode", "Show count", "True: show counter. False: true only during the tick where the output changes", False),
        Int32Field("clock_period", "Clock Period", "Updates between each update of the output", 1),
        Int32Field("max_count", "Max Output", "Output resets when count reaches this value", 2)
    ]
   
    width = 1
    
    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        sub_clock = self.Output.get_int("sub_clock", 0)
        sub_clock = sub_clock + 1
        update = False

        if sub_clock >= self.Field.get_int("clock_period", 0):
            sub_clock = 0
            update = True

        self.Output.set_int("sub_clock", sub_clock)

        if self.Field.get_bool("mode", False):
            self.Output.set_bool("clock", update)
            return

        if sub_clock == 0:
            output = self.Output.get_int("clock", 0)
            output = output + 1

            if output >= self.Field.get_int("max_count", 0):
                output = 0

            self.Output.set_int("clock", output)
        

class Runtime_Clock_2(Module):
    name = "Control: Clock (2 outputs)"
    symbol = "CLOCK"

    outputs = [
        Output("update", "Updated this tick"),
        Output("clock", "Clock")
    ]

    fields = [
        Int32Field("clock_period", "Clock Period", "Updates between each update of the output", 1),
        Int32Field("max_count", "Reset value", "Output resets when count reaches this value", 2)
    ]
   
    width = 2
    
    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        sub_clock = self.Output.get_int("sub_clock", 0)
        sub_clock = sub_clock + 1
        update = False

        if sub_clock >= self.Field.get_int("clock_period", 0):
            sub_clock = 0
            update = True

        self.Output.set_int("sub_clock", sub_clock)
        self.Output.set_bool("update", update)

        if sub_clock == 0:
            output = self.Output.get_int("clock", 0)
            output = output + 1

            if output >= self.Field.get_int("max_count", 0):
                output = 0

            self.Output.set_int("clock", output)
        
