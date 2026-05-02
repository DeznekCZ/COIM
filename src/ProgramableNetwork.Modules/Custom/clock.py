from Core.categories import DefaultCategories
from Core.fields import Int32Field, BooleanField
from Core.io import Input, Output
from Mafi import Fix32
from Core.module import DefaultControllers, Module

# File written by Nightinggale
# Optimized: hot-path field/input reads are pulled into locals so that each
# tick performs each lookup once instead of two or three times (every
# `self.Field`/`self.Input`/`self.Output` access also allocates a wrapper).

class Runtime_Clock_1(Module):
    name = "Control: Clock (1 output)"
    description = "Outputs an integer counter on <b>clock</b> that increments every <b>clock_period</b> ticks and wraps to zero at <b>max_count</b>. With the <b>mode</b> field on, output instead pulses true only on the tick where the counter would change. An optional <b>reset</b> input holds the clock at zero and suppresses output while non-zero; counting resumes when reset goes back to zero."
    symbol = "CLK"

    inputs = [
        Input("reset", "Reset (hold non-zero to freeze at 0)")
    ]

    outputs = [
        Output("clock", "Clock")
    ]

    fields = [
        BooleanField("mode", "Pulse mode", "True: true only during the tick where the output changes. False: show counter", False),
        Int32Field("clock_period", "Clock Period", "Updates between each update of the output", 1),
        Int32Field("max_count", "Max Output", "Output resets when count reaches this value", 2)
    ]

    width = 1

    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        pulse_mode = self.Field.get_bool("mode", False)

        # Reset held — freeze the sub-tick counter and the public output at 0,
        # suppress any pulse, and exit before the increment runs.  Counting
        # resumes from zero on the first tick where reset goes back to 0.
        if self.Input.get_int("reset", 0) > 0:
            self.Output.set_int("sub_clock", 0)
            if pulse_mode:
                self.Output.set_bool("clock", False)
            else:
                self.Output.set_int("clock", 0)
            return

        sub_clock = self.Output.get_int("sub_clock", 0) + 1
        update = False

        if sub_clock >= self.Field.get_int("clock_period", 0):
            sub_clock = 0
            update = True

        self.Output.set_int("sub_clock", sub_clock)

        if pulse_mode:
            self.Output.set_bool("clock", update)
            return

        if sub_clock == 0:
            output = self.Output.get_int("clock", 0) + 1

            if output >= self.Field.get_int("max_count", 0):
                output = 0

            self.Output.set_int("clock", output)


class Runtime_Clock_2(Module):
    name = "Control: Clock (2 outputs)"
    description = "Outputs an integer counter on <b>clock</b> that increments every <b>clock_period</b> ticks and wraps to zero at <b>max_count</b>, plus an <b>update</b> boolean that pulses true on every tick where the counter changes. An optional <b>reset</b> input holds the clock at zero and suppresses output while non-zero; counting resumes when reset goes back to zero."
    symbol = "CLK"

    inputs = [
        Input("reset", "Reset (hold non-zero to freeze at 0)")
    ]

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
        # Reset held — freeze every output at 0 and exit before the increment.
        # Counting resumes from zero on the first tick where reset goes back to 0.
        if self.Input.get_int("reset", 0) > 0:
            self.Output.set_int("sub_clock", 0)
            self.Output.set_bool("update", False)
            self.Output.set_int("clock", 0)
            return

        sub_clock = self.Output.get_int("sub_clock", 0) + 1
        update = False

        if sub_clock >= self.Field.get_int("clock_period", 0):
            sub_clock = 0
            update = True

        self.Output.set_int("sub_clock", sub_clock)
        self.Output.set_bool("update", update)

        if sub_clock == 0:
            output = self.Output.get_int("clock", 0) + 1

            if output >= self.Field.get_int("max_count", 0):
                output = 0

            self.Output.set_int("clock", output)
