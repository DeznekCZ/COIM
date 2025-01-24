from Core.categories import DefaultCategories
from Core.fields import Int32Field
from Core.mafi import Fix32, fix
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
        seed = self.field.get("seed", fix(0))
        old_seed = self.output.get("seed", fix(0))
        addition = self.input.get("addition", fix(256))

        if addition.RawValue == 0:
            addition = fix(256)

        if old_seed != seed:
            raw = (seed + addition).RawValue
            self.output.set("seed", seed)
            self.output.set("value", Fix32.FromRaw((raw >> 4) ^ (raw << 4)))
        else:
            seed = self.output.get("value", seed)
            raw = (seed + addition).RawValue
            self.output.set("value", Fix32.FromRaw((raw >> 4) ^ (raw << 4)))