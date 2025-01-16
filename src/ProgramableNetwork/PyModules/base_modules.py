from module import *

class Sum(Module):
    name = "C = A + B"

    inputs = [
        Input("a", "A"),
        Input("b", "A")
    ]

    outputs = [
        Output("c", "C")
    ]

    def action(self):
        a = self.input.get("a", 0)
        b = self.input.get("b", 0)
        self.output.set("c", a + b)

class Connection_SwitchOff(Module):
    name = "Connection: Switch Off"
    inputs = [ Input("pause", "Pause") ]
    fields = [ StaticEntityField(
        "entity",
        "Connection device",
        "Any pausable building connectable by cable 20m from controller",
        20,
        lambda module, entity: entity.CanBePaused
    ) ]
    categories = [ DefaultCategories.Connection ]
    controllers = [ DefaultControllers.Basic ]

    def action(self):
        e = self.field.get_ent("entity")
        p = self.input.get_bool("pause", False)
        if e is not None:
            e.SetPause(p)

class Connection_IsActive(Module):
    name = "Connection: Status"
    outputs = [
        Output("power", "Has enough power"),
        Output("workers", "Has enough workers"),
        Output("constructed", "Is build"),
        Output("pause", "Is Paused")
    ]
    fields = [ StaticEntityField(
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
