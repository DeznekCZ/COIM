
class CustomEntitiesSerializer:
    def __init__(self):
        pass


class CustomTextReader:
    NEW_LINE = None
    CR = None
    DELIMITER = None
    def __init__(self):
        self.Level = 0
        self.SaveVersion = 0
        self.MissingProtoIds = None

class CustomTextWriter:
    def __init__(self):
        self.Level = 0
        self.SaveVersion = 0
