
class LocStrFormatted:
    Empty = None
    def __init__(self):
        self.IsEmptyOrNull = False
        self.IsNotEmpty = False
        self.Value = ""

class IntegerSiSuffixFormatter:
    def __init__(self):
        pass


class LocalizationManager:
    CurrentLangInfo = None
    CurrentCultureInfo = None
    TranslationWarnings = None
    TranslationErrors = None
    LanguagesAvailable = None
    KNOWN_MISSING_IDS = None
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

class LocStrExtensions:
    def __init__(self):
        pass


class LocalizationUtils:
    def __init__(self):
        pass


class TranslationExtensions:
    def __init__(self):
        pass

