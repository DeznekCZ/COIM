
from Core.fields import FieldValue
from Core.module import Module, ModuleStatus
from Custom import template

class Template:
    """ Define class in format:

    from Core.ids import BaseModuleID

    class TemplateID(Template, BaseModuleID)
        name = "Display name of template"
        description = "Any additional information for this template"
    
        def settings(self)
            self.Field.set_int("number", 1),
            self.Field.set_int("b", 99)
    """

    def settings(self):
        """ Defines default setup for module """
        pass

    def __init__(self):
        # defines an interface to data of field inside module
        self.Field = FieldValue(self)
        # defines an interface to raw data inside module
        self.NumberData: dict[str, int] = {}
        # defines an interface to raw data inside module
        self.StringData: dict[str, str] = {}

class ModuleConnection:
    pass

class ModuleEntry(Template):

    def __getitem__(self, id: str) -> ModuleConnection:
        pass

    def __setitem__(self, id: str, connection: ModuleConnection) -> None:
        pass

class Controller:
    """ Define class in format:
    
    # used modules
    from Core.ids import BaseModuleID

    # tier contains 16 columns, 4 rows
    tier = 1

    # self.next_row and self.next_column automatically defines nex available slot

    class TemplateID(Controller, tier)
        name = "Display name of template"
        description = "Any additional information for this template"
    
        def modules(self)
            moduleA = self.add_module(BaseModuleID, 0, 0)
            moduleB = self.add_module(BaseModuleID, self.next_row, self.next_column)

            moduleB["A"] = module["A"]

            return [moduleA, moduleB]

        def settings(self, modules):
            modules[0].Field.set_int("number", 1),
            modules[0].Field.set_int("b", 99)
    """

    def __init__(self):
        self.next_row = 0;
        self.next_column = 0;

    def settings(self):
        """ Defines setup for controller """
        pass
    
    def add_module(self, moduleId: type, row: int, column: int) -> ModuleEntry:
        """ Defines new module """
        pass