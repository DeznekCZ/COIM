from Core.categories import DefaultCategories
from Core.fields import EntityField
from Core.module import DefaultControllers, Module
from Core.entities import StaticEntity
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
    controllers = [ DefaultControllers.Basic ]

    def action(self):
        e = self.field.get_ent("entity")
        if e is not None:
            self.output.set_bool("power", e.EnergyConsummer.HasEnough)
            self.output.set_bool("workers", e.WorkerConsummer.HasEnough)
            self.output.set_bool("constructed", e.IsConstucted)
            self.output.set_bool("pause", e.IsPaused)
        else:
            self.output.set_bool("power", True)
            self.output.set_bool("workers", True)
            self.output.set_bool("constructed", False)
            self.output.set_bool("pause", True)