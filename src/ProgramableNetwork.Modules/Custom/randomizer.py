from Core.categories import DefaultCategories
from Core.fields import Int32Field
from Core.mafi import fix
from Core.module import DefaultControllers, Module
from Core.io import Output, Input

class Randomizer(Module):
    name = "Randomizer"
    symbol = "RAN"
    outputs = [
        Output("value", "Random value")
    ]
    inputs = [
        Input("addition", "Grain value")
    ]
    fields = [ Int32Field(
        "seed",
        "Number seed",
        "Base value from where the value is generated"
    ) ]
    categories = [ DefaultCategories.Arithmetic ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        seed = self.Field.get("seed", fix(0))
        old_seed = self.Output.get("seed", fix(0))
        addition = self.Input.get("addition", fix(256))

        if addition.RawValue == 0:
            addition = fix(256)

        if old_seed != seed:
            raw = (seed + addition).RawValue
            self.Output.set("seed", seed)
            self.Output.set("value", fix((raw >> 4) ^ (raw << 4)))
        else:
            seed = self.Output.get("value", seed)
            raw = (seed + addition).RawValue
            self.Output.set("value", fix((raw >> 4) ^ (raw << 4)))