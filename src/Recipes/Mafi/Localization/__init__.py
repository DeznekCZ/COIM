
class LocStrFormatted:
    Empty = None
    def __init__(self):
        self.IsEmptyOrNull = False
        self.IsNotEmpty = False
        self.Value = ""

class LocalizationManager:
    CurrentLangInfo = None
    TrInfoStr = ""
    CurrentCultureInfo = None
    TranslationWarnings = None
    TranslationErrors = None
    LanguagesAvailable = None
    EN_US_CULTURE_INFO_ID = ""
    TODO_HIDE = ""
    HIDE_HIDE = ""
    def __init__(self):
        pass


    class LocData:
        def __init__(self):
            self.TranslatedStrings = None

    class LangInfo:
        def __init__(self):
            self.CultureInfoId = ""
            self.LanguageTitle = ""
            self.FileName = ""
            self.PercentTranslated = None
            self.PluralFormsCount = 0
            self.PluralIndexFunction = None
            self.UsesSymbols = False

class Loc:
    NAME_SUFFIX = ""
    DESC_SUFFIX = ""
    def __init__(self):
        pass


class LocStr:
    Empty = None
    def __init__(self):
        self.AsFormatted = None
        self.Id = ""
        self.TranslatedString = ""

class LocStr1:
    Empty = None
    def __init__(self):
        self.Id = ""

class LocStr2:
    Empty = None
    def __init__(self):
        self.Id = ""

class LocStr3:
    Empty = None
    def __init__(self):
        self.Id = ""

class LocStr4:
    Empty = None
    def __init__(self):
        self.Id = ""

class LocStr1Plural:
    Empty = None
    def __init__(self):
        self.Id = ""

class LocalizationUtils:
    def __init__(self):
        pass


class TranslationExtensions:
    def __init__(self):
        pass

