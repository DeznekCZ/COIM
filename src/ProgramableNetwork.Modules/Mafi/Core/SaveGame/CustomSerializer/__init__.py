class CustomEntitiesSerializer:

    def __init__(self):
        pass

class CustomTextReader:
    NEW_LINE = None
    CR = None
    DELIMITER = None

    def __init__(self):
        self.Level = int(0)
        self.SaveVersion = int(0)
        self.MissingProtoIds = None
class CustomTextWriter:

    def __init__(self):
        self.Level = int(0)
