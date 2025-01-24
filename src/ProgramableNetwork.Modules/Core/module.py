from Core.categories import Category
from Core.fields import Field, FieldValue
from Core.entities import Entity
from Core.io import Input, Output, InputValue, OutputValue

class DefaultControllers:
    Controller = "ProgramableNetwork_Controller"

class Module:
    name: str = "module display name"
    inputs: list[Input] = []
    outputs: list[Output] = []
    fields: list[Field] = []
    categories: list[Category] = []
    controllers: list[str] = []

    def __init__(self):
        # defines an interface to data of input inside module
        self.input = InputValue(self)
        # defines an interface to data of output inside module
        self.output = OutputValue(self)
        # defines an interface to data of field inside module
        self.field = FieldValue(self)
        # defines an interface to raw data inside module
        self.number_data = {}
        # defines an interface to raw data inside module
        self.string_data = {}

    def init(self, prototype):
        pass

    def action(self):
        pass
