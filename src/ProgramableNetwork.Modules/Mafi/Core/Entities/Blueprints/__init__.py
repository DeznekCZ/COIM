class IBlueprint:

    def __init__(self):
        self.GameVersion = str(0)
        self.SaveVersion = int(0)
        self.Items = None
        self.Surfaces = None
        from Mafi import Option
        self.ProtosThatFailedToLoad = Option()
        self.MostFrequentProtos = None
        self.AllDistinctProtos = None
        self.Name = str(0)
        self.Desc = str(0)
class IBlueprintItem:

    def __init__(self):
        self.Name = str(0)
        self.Desc = str(0)
class IBlueprintsFolder:

    def __init__(self):
        self.IsEmpty = False
        from Mafi import Option
        self.ParentFolder = Option()
        self.Folders = None
        self.Blueprints = None
        self.PreviewProtos = None
        self.Name = str(0)
        self.Desc = str(0)
class BlueprintsLibrary:

    def __init__(self):
        self.LibraryStatus = None
        self.NumberOfBackupsAvailable = int(0)
        self.Root = None
class Status:

    def __init__(self):
        pass

