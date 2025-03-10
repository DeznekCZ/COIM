
class Category:
    def __init__(self, name) -> None:
        self.Name = name

class DefaultCategories:
    Connection = Category("Connection")
    ConnectionRead = Category("ConnectionRead")
    ConnectionWrite = Category("ConnectionWrite")
    Arithmetic = Category("Arithmetic")
    Display = Category("Display")
    Control = Category("Control")
    Boolean = Category("Boolean")