
class IBlueprint:
    def __init__(self):
        self.GameVersion = ""
        self.SaveVersion = 0
        self.Items = None
        self.Surfaces = None
        from Mafi import Option
        self.ProtosThatFailedToLoad = Option()
        self.MostFrequentProtos = None
        self.AllDistinctProtos = None
        self.Name = ""
        self.Desc = ""

class IBlueprintItem:
    def __init__(self):
        self.Name = ""
        self.Desc = ""

class IBlueprintsFolder:
    def __init__(self):
        self.IsEmpty = False
        from Mafi import Option
        self.ParentFolder = Option()
        self.Folders = None
        self.Blueprints = None
        self.PreviewProtos = None
        self.Name = ""
        self.Desc = ""

class BlueprintsLibrary:
    def __init__(self):
        self.LibraryStatus = None
        self.NumberOfBackupsAvailable = 0
        self.Root = None
        self.PathToFile = ""

    class Status:
        NoLibraryFound = None
        LoadingInProgress = None
        LoadFailedDueToFormat = None
        LoadFailedNoAccess = None
        LoadSuccess = None
        SaveInProgress = None
        SaveFailed = None
        SaveDone = None
        SaveDoneBackupFailed = None
        def __init__(self):
            self.value__ = 0
