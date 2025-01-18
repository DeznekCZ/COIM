

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

    def set(self, name: str, value: float) -> None:
        self.module.number_data["in__" + name] = value

    def get(self, name: str, default: float) -> float:
        return self.module.number_data["in__" + name] or default

    def set_bool(self, name: str, value: bool) -> None:
        if value:
            self.module.number_data["in__" + name] = 1
        else:
            self.module.number_data["in__" + name] = 0

    def get_bool(self, name: str, default: bool) -> bool:
        return (self.module.number_data["in__" + name] or default) > 0

class OutputValue:
    def __init__(self, module):
        self.module = module

    def set(self, name: str, value: float) -> None:
        self.module.number_data["out__" + name] = value

    def get(self, name: str, default: float) -> float:
        return self.module.number_data["out__" + name] or default

    def set_bool(self, name: str, value: bool) -> None:
        if value:
            self.module.number_data["out__" + name] = 1
        else:
            self.module.number_data["out__" + name] = 0

    def get_bool(self, name: str, default: bool) -> bool:
        return (self.module.number_data["out__" + name] or default) > 0