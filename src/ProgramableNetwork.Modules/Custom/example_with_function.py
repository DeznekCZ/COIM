from Core.categories import DefaultCategories
from Core.fields import Int32Field
from Core.mafi import Fix32, fix
from Core.module import DefaultControllers, Module
from Core.io import Output, Input

class FunctionBased(Module):
    name = "[DO NOT USE]"
    symbol = "DNT"
    outputs = [
        Output("value", "Random value")
    ]
    fields = [ Int32Field(
        "seed",
        "Number seed",
        "Base value from where the value is generated"
    ) ]
    categories = [ DefaultCategories.Arithmetic ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        self.output.set("value", self.subcall());

    def subcall(self):
        return fix(5)