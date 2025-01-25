from Core.categories import DefaultCategories
from Core.fields import EntityField
from Core.mafi import fix
from Core.module import DefaultControllers, Module, ModuleStatus
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
            self.Error = "No building is connected"
            return ModuleStatus.Error