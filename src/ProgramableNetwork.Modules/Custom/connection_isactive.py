from Core.categories import DefaultCategories
from Core.fields import EntityField
from Core.module import DefaultControllers, Module
from Core.entities import StaticEntity, IElectricityConsumingEntity, IWorkerConsumingEntity
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
            if e is IElectricityConsumingEntity:
                self.output.set_bool("power", e.EnergyConsummer.HasEnough)
            else:
                self.output.set_bool("power", True)
            if e is IEntityWithWorkers:
                self.output.set("workers", WorkersNeeded == 0)
            else:
                self.output.set_bool("workers", True)
            self.output.set_bool("constructed", e.IsConstucted)
            self.output.set_bool("pause", e.IsPaused)
        else:
            self.output.set_bool("power", True)
            self.output.set_bool("workers", True)
            self.output.set_bool("constructed", False)
            self.output.set_bool("pause", True)