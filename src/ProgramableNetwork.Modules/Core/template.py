
from Core.fields import FieldValue
from Core.module import ModuleStatus

class Template:
    """ Define methods in format:
    
    def template_id(self: Template, name = "Template name")
        self.Field.set("a", Fix32.One)
    
    """

    def settings(self):
        """ Defines default setup for module """
        pass

    def __init__(self):
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
        # accessor to prototype
        self.Prototype = None