
from Core.categories import DefaultCategories
from Core.fields import Int32Field, BooleanField
from Core.io import Input, Output
from Mafi import Fix32
from Core.module import DefaultControllers, Module

class Runtime_Timer(Module):
    name = "Control: Timer"
    description = "Counts up by 1 each tick on output <b>timer</b> until it reaches the period (input <b>timer_period</b> or the field, ~10 ticks/s); when it does, <b>done</b> turns on and the counter holds. Triggering <b>reset</b> clears <b>timer</b> and <b>done</b>."
    symbol = "TMR"
    inputs = [
        Input("reset", "Reset Timer"),
        Input("timer_period", "Timer Period")
    ]
    outputs = [
        Output("timer", "Timer"),
        Output("done", "Is done")
    ]
    fields = [
        Int32Field("timer_period", "Timer Period (ticks)", "Updates between each tick of the output, that is 10 times per second", 1, True)
    ]

    width = 2

    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        period = self.FieldOrInput.get_int("timer_period", 1)
        timer = self.Output.get_int("timer", 0)
        reset = self.Input.get_bool("reset", False)

        if reset:
            self.Output.set_int("timer", 0)
            self.Output.set_bool("done", False)
            return

        if period <= 0:
            period = 1 # Avoid endless non finished loop

        if timer == period:
            self.Output.set_int("done", 1)
            return

        # If timer reaches the period, it is done and should not increase anymore until reset
        timer = timer + 1
        self.Output.set_int("timer", timer)
        if timer >= period:
            self.Output.set_bool("done", True)
        else:
            self.Output.set_bool("done", False)