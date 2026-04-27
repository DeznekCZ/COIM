from Core.categories import DefaultCategories
from Core.fields import Int32Field
from Core.mafi import fix
from Core.module import DefaultControllers, Module
from Core.io import Output, Input, Display

class Boolean_Latch(Module):
    name = "Boolean: NOR-Latch"
    description = "When signal set is triggered, output will hold value 1, when signal reset is triggered, output will hold 0. Can be created by two OR gates connected by not C."
    symbol = "NORL"
    outputs = [
        Output("on", "Stored signal")
    ]
    inputs = [
        Input("set", "Set"),
        Input("reset", "Reset")
    ]
    displays = [
        Display.Filler(fix(1)),
        Display.LED("on", "Stored signal")
    ]
    width = 2
    categories = [ DefaultCategories.Boolean, DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def Action(self):
        if self.Input.get_bool("set", False):
            self.Output.set_bool("on", True)

        elif self.Input.get_bool("reset", False):
            self.Output.set_bool("on", False)

    def Display(self):
        if self.Output.get_bool("on", False):
            self.Display.set("on", "stored")
        else:
            self.Display.set("on", "")
