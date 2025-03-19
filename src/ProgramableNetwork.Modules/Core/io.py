
from Mafi import Fix32

class Output:
    def __init__(self, output_id: str, name: str):
        self.id = output_id
        self.name = name

class Input:
    def __init__(self, input_id: str, name: str):
        self.id = input_id
        self.name = name

class InputValue:
    def __init__(self, module):
        self.module = module

    def set(self, name: str, value: Fix32) -> None:
        self.module.number_data["in__" + name] = value

    def get(self, name: str, default: float) -> Fix32:
        return self.module.number_data["in__" + name] or default

    def set_bool(self, name: str, value: bool) -> None:
        if value:
            self.module.number_data["in__" + name] = 1
        else:
            self.module.number_data["in__" + name] = 0

    def get_bool(self, name: str, default: bool) -> bool:
        return (self.module.number_data["in__" + name] or default) > 0

    def __getitem__(self, name: str) -> Fix32:
        return self.get(name, Fix32.Zero)

    def __setitem__(self, name: str, value: Fix32) -> Fix32:
        self.set(name, value)
        return value

class OutputValue:
    def __init__(self, module):
        self.module = module

    def set(self, name: str, value: Fix32) -> None:
        self.module.number_data["out__" + name] = value

    def get(self, name: str, default: Fix32) -> Fix32:
        return self.module.number_data["out__" + name] or default

    def set_bool(self, name: str, value: bool) -> None:
        if value:
            self.module.number_data["out__" + name] = 1
        else:
            self.module.number_data["out__" + name] = 0

    def get_bool(self, name: str, default: bool) -> bool:
        return (self.module.number_data["out__" + name] or default) > 0

    def __getitem__(self, name: str) -> Fix32:
        return self.get(name, Fix32.Zero)

    def __setitem__(self, name: str, value: Fix32) -> Fix32:
        self.set(name, value)
        return value