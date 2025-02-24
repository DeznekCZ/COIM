from Core.categories import DefaultCategories
from Core.io import Input
from Core.module import DefaultControllers, Module, ModuleStatus

class Notification_Info(Module):
    name = "Notification: Info"
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