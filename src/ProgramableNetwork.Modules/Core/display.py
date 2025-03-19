
from Mafi import Fix32

class Text:
    def __init__(self, display_id: str, name: str):
        self.id = display_id
        self.name = name

class Power:
    def __init__(self, display_id: str, name = "Power"):
        self.id = display_id
        self.name = name

class LED:
    def __init__(self, display_id: str, name = "LED"):
        self.id = display_id
        self.name = name

class DisplayValue:
    def __init__(self, module):
        self.module = module

    def set(self, name: str, value: str) -> None:
        self.module.string_data["display__" + name] = value

    def get(self, name: str, default: str) -> str:
        return self.module.string_data["display__" + name] or default

    def __getitem__(self, name: str) -> str:
        return self.get(name, "")

    def __setitem__(self, name: str, value: str) -> str:
        self.set(name, value)
        return value