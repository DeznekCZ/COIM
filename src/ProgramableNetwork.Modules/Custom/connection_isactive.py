from Core.categories import DefaultCategories
from Core.fields import EntityField
from Core.mafi import fix
from Core.module import DefaultControllers, Module
from Core.entities import IEntityWithWorkers, StaticEntity, IElectricityConsumingEntity
from Core.io import Output

class Connection_IsActive(Module):
    name = "Connection: Status"
    symbol = "STAT"
    outputs = [
        Output("power", "Has enough power"),
        Output("workers", "Has enough workers"),
        Output("constructed", "Is build"),
        Output("pause", "Is Paused")
    ]
    fields = [ EntityField(
        StaticEntity,
        "entity",
        "Connection device",
        "Any pausable building connectable by cable 20m from controller",
        20
    ) ]
    categories = [ DefaultCategories.Connection ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        e = self.field.get_ent("entity")
        if e is not None:
            if e is IElectricityConsumingEntity and e.ElectricityConsumer.Value is not None:
                self.output.set_bool("power", not e.ElectricityConsumer.Value.NotEnoughPower)
            else:
                self.output.set_bool("power", True)
            if e is IEntityWithWorkers:
                self.output.set_bool("workers", e.WorkersNeeded == 0)
            else:
                self.output.set_bool("workers", True)
            #presumption is StaticEntity by field restriction
            self.output.set_bool("constructed", e.IsConstructed)
            self.output.set_bool("pause", e.CanBePaused and e.IsPaused)
        else:
            self.output.set_bool("power", False)
            self.output.set_bool("workers", False)
            self.output.set_bool("constructed", True)
            self.output.set_bool("pause", True)