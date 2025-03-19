from Core.categories import DefaultCategories
from Core.fields import EntityField
from Core.mafi import Fix32, fix
from Core.module import DefaultControllers, Module, ModuleStatus
from Core.io import Output

from Mafi.Base.Prototypes.Buildings.ThermalStorages import ThermalStorage

class Connection_ThermalStorage(Module):
    name = "Connection: Thermal Storage (charge)"
    symbol = "S-CHARGE"
    outputs = [
        Output("product", "Stored product"),
        Output("charged", "Charge level (percent)")
    ]
    fields = [
        EntityField(
            ThermalStorage,
            "entity",
            "Connection device",
            "Any thermal storage building",
            20
        )
    ]
    width = 4
    categories = [ DefaultCategories.Connection ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        entity = self.Field.get_ent("entity")

        if entity is ThermalStorage:
            if entity.HeatCapacity == Fix32.Zero:
                self.Output.set("charged", Fix32.Zero)
            else:
                self.Output.set("charged", fix(100) * fix(entity.HeatStored) / fix(entity.HeatCapacity))
            self.Output.set("product", self.tryGetAssignedProduct(entity))
            return ModuleStatus.Running

        self.Output.set("charged", Fix32.Zero)
        self.Output.set("product", Fix32.Zero)
        return ModuleStatus.Error;

    def tryGetAssignedProduct(self, entity):
        if entity.AssignedProduct is None or entity.AssignedProduct.Product is None:
            return Fix32.Zero

        return fix(entity.AssignedProduct.Product.SlimId.Value)