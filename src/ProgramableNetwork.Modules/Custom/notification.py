from Core.categories import DefaultCategories
from Core.io import Input
from Core.module import DefaultControllers, Module, ModuleStatus

class Notification_Info(Module):
    name = "Notification: Info"
    description = "Sets the controller's <b>Info</b> flag from input <b>in</b>; the controller surfaces an info notification while the input is on."
    symbol = "NOTIF\nINF"
    inputs = [ Input("in", "State") ]
    width = 2
    categories = [ DefaultCategories.Display ]
    controllers = [ DefaultControllers.Controller ]

    def Action(self):
        self.Info = self.Input.get_bool("in", False)
        return ModuleStatus.Running

class Notification_Warning(Module):
    name = "Notification: Warning"
    description = "Sets the controller's <b>Warning</b> flag from input <b>in</b>; the controller surfaces a warning notification while the input is on."
    symbol = "NOTIF\nWRN"
    inputs = [ Input("in", "State") ]
    width = 2
    categories = [ DefaultCategories.Display ]
    controllers = [ DefaultControllers.Controller ]

    def Action(self):
        self.Warning = self.Input.get_bool("in", False)
        return ModuleStatus.Running

class Notification_Error(Module):
    name = "Notification: Error"
    description = "Returns Error status while input <b>in</b> is on so the controller raises an error notification; otherwise returns Running."
    symbol = "NOTIF\nERR"
    inputs = [ Input("in", "State") ]
    width = 2
    categories = [ DefaultCategories.Display ]
    controllers = [ DefaultControllers.Controller ]

    def Action(self):
        if self.Input.get_bool("in", False):
            return ModuleStatus.Error
        else:
            return ModuleStatus.Running