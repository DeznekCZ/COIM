from Core.categories import DefaultCategories
from Core.fields import EntityField
from Core.module import DefaultControllers, Module, ModuleStatus
from Core.io import Output

from Mafi.Core.Entities.Static import StaticEntity
from Mafi.Core.Factory.ElectricPower import IElectricityConsumingEntity
from Mafi.Core.Population import IEntityWithWorkers

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
    categories = [ DefaultCategories.Connection, DefaultCategories.ConnectionRead ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        e = self.Field.get_ent("entity")
        if e is not None:
            if e is IElectricityConsumingEntity and e.ElectricityConsumer.Value is not None:
                self.Output.set_bool("power", not e.ElectricityConsumer.Value.NotEnoughPower)
            else:
                self.Output.set_bool("power", True)
            if e is IEntityWithWorkers:
                self.Output.set_bool("workers", e.WorkersNeeded == 0)
            else:
                self.Output.set_bool("workers", True)
            #presumption is StaticEntity by field restriction
            self.Output.set_bool("constructed", e.IsConstructed)
            self.Output.set_bool("pause", e.CanBePaused and e.IsPaused)
        else:
            self.Output.set_bool("power", False)
            self.Output.set_bool("workers", False)
            self.Output.set_bool("constructed", True)
            self.Output.set_bool("pause", True)
            return ModuleStatus.Error