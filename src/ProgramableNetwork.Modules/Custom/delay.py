from Core.categories import DefaultCategories
from Core.io import Input, Output
from Core.module import DefaultControllers, Module

from Mafi import Fix32

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
