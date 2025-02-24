from Core.categories import Category
from Core.fields import Field, FieldValue
from Core.entities import Entity
from Core.io import Input, Output, InputValue, OutputValue
from Core.translate import LocStr, LocStr1, LocStrFormatted

class DefaultControllers:
    Controller = "ProgramableNetwork_Controller"

class ModuleStatusValue:
    pass

class ModuleStatus:
    Init = ModuleStatusValue()
    Running = ModuleStatusValue()
    Error = ModuleStatusValue()
    Paused = ModuleStatusValue()

class Module:
    name: str = "module display name"
    inputs: list[Input] = []
    outputs: list[Output] = []
    fields: list[Field] = []
    categories: list[Category] = []
    controllers: list[str] = []
    width: int = 0

    def __init__(self):
        # defines an interface to data of input inside module
        self.Input = InputValue(self)
        # defines an interface to data of output inside module
        self.Output = OutputValue(self)
        # defines an interface to data of field inside module
        self.Field = FieldValue(self)
        # defines an interface to raw data inside module
        self.NumberData = {}
        # defines an interface to raw data inside module
        self.StringData = {}
        # defines status of module
        self.Status = ModuleStatus.Init
        # defines an Tooltip/Error info
        self.Error = ""
        # set an display state to info
        self.Info = False
        # set an display state to warning
        self.Warning = False
    
    def Tr(key: str, default: str) -> LocStr | LocStr1 | LocStrFormatted:
      """Creates and translation string for modules"""
      pass

    def Init(self, prototype):
        pass

    def Action(self):
        pass
