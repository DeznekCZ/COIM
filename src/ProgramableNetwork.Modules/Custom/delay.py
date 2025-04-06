from Core.categories import DefaultCategories
from Core.fields import Int32Field
from Core.io import Input, Output
from Core.module import DefaultControllers, Module

from Mafi import Fix32

class Runtime_Delay_1(Module):
    name = "Control: Delay (multi tick)"
    symbol = "DLY"
    inputs = [
        Input("input", "Signal input")
    ]
    outputs = [
        Output("output", "Signal output")
    ]

    fields = [
        Int32Field("delay", "Delay", "Sets how many ticks the output should be delayed", 1)
    ]

    width = 1

    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        input = self.Input.get("input", Fix32.Zero)
        delay = self.Field.get_int("delay", 1)

        if delay <= 1:
            # No need to buffer delays if they are this short
            self.Output.set("output", input)
            return

        # Update the counter
        count = self.Output.get_int("count", 0)
        if count >= delay:
            count = 0

        # Generate a name for the buffer storage, which is unique to the count.
        # The simplest approach is to convert the int to a string.
        buffer_name = unicode(count)

        # Here it gets more complicated to follow what goes on.
        # The buffer is put on output and then input is put into the same buffer.
        # Since the name of the buffer follows the counter, each name is used at a fixed internal.
        # This will provide the same result as a list where you push to one end and pop from the other.
        # The difference being that here we don't have to worry about all the other values.
        self.Output.set("output", self.Output.get(buffer_name, Fix32.Zero))
        self.Output.set(buffer_name, input)


class Runtime_Delay_2(Module):
    name = "Control: Delay (2 ticks)"
    symbol = "DLY"
    inputs = [
        Input("0", "Signal input")
    ]
    outputs = [
        Output("1", "Delay by 1 tick"),
        Output("2", "Delay by 2 ticks")
    ]
    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        a = self.Input.get("0", Fix32.Zero)
        self.Output.set("2", self.Output.get("1", Fix32.Zero))
        self.Output.set("1", a)

class Runtime_Delay_4(Module):
    name = "Control: Delay (4 ticks)"
    symbol = "DELAY"
    inputs = [
        Input("0", "Signal input")
    ]
    outputs = [
        Output("1", "Delay by 1 tick"),
        Output("2", "Delay by 2 ticks"),
        Output("3", "Delay by 3 ticks"),
        Output("4", "Delay by 4 ticks")
    ]
    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        a = self.Input.get("0", Fix32.Zero)
        self.Output.set("4", self.Output.get("3", Fix32.Zero))
        self.Output.set("3", self.Output.get("2", Fix32.Zero))
        self.Output.set("2", self.Output.get("1", Fix32.Zero))
        self.Output.set("1", a)

class Runtime_Delay_6(Module):
    name = "Control: Delay (6 ticks)"
    symbol = "DELAY"
    inputs = [
        Input("0", "Signal input")
    ]
    outputs = [
        Output("1", "Delay by 1 tick"),
        Output("2", "Delay by 2 ticks"),
        Output("3", "Delay by 3 ticks"),
        Output("4", "Delay by 4 ticks"),
        Output("5", "Delay by 5 ticks"),
        Output("6", "Delay by 6 ticks")
    ]
    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        a = self.Input.get("0", Fix32.Zero)
        self.Output.set("6", self.Output.get("5", Fix32.Zero))
        self.Output.set("5", self.Output.get("4", Fix32.Zero))
        self.Output.set("4", self.Output.get("3", Fix32.Zero))
        self.Output.set("3", self.Output.get("2", Fix32.Zero))
        self.Output.set("2", self.Output.get("1", Fix32.Zero))
        self.Output.set("1", a)

class Runtime_Delay_8(Module):
    name = "Control: Delay (8 ticks)"
    symbol = "DELAY"
    inputs = [
        Input("0", "Signal input")
    ]
    outputs = [
        Output("1", "Delay by 1 tick"),
        Output("2", "Delay by 2 ticks"),
        Output("3", "Delay by 3 ticks"),
        Output("4", "Delay by 4 ticks"),
        Output("5", "Delay by 5 ticks"),
        Output("6", "Delay by 6 ticks"),
        Output("7", "Delay by 7 ticks"),
        Output("8", "Delay by 8 ticks")
    ]
    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        a = self.Input.get("0", Fix32.Zero)
        self.Output.set("8", self.Output.get("7", Fix32.Zero))
        self.Output.set("7", self.Output.get("6", Fix32.Zero))
        self.Output.set("6", self.Output.get("5", Fix32.Zero))
        self.Output.set("5", self.Output.get("4", Fix32.Zero))
        self.Output.set("4", self.Output.get("3", Fix32.Zero))
        self.Output.set("3", self.Output.get("2", Fix32.Zero))
        self.Output.set("2", self.Output.get("1", Fix32.Zero))
        self.Output.set("1", a)
