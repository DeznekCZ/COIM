
class EditorIgnoreAttribute:
    def __init__(self):
        self.TypeId = None

class EditorSectionAttribute:
    def __init__(self):
        self.TypeId = None
        self.Label = ""
        self.Tooltip = ""
        self.IsHeader = False
        self.CollapsedByDefault = False

class EditorLabelAttribute:
    def __init__(self):
        self.TypeId = None
        self.Label = ""
        self.Tooltip = ""
        self.IsHeader = False

class EditorDropdownAttribute:
    def __init__(self):
        self.TypeId = None
        self.SourceDataMember = ""

class EditorClassNameAttribute:
    def __init__(self):
        self.TypeId = None

class EditorButtonAttribute:
    def __init__(self):
        self.TypeId = None
        from Mafi import Option
        self.ButtonText = Option()
        self.ButtonTooltip = ""
        self.IsPrimary = False
        self.Icon = None

class ObjEditorIcon:
    None = None
    View = None
    Delete = None
    Edit = None
    Clone = None
    def __init__(self):
        self.value__ = 0

class EditorEnforceOrderAttribute:
    def __init__(self):
        self.TypeId = None
        self.Order = 0

class EditorTextAreaAttribute:
    def __init__(self):
        self.TypeId = None
        self.LinesCount = 0
        self.AutoScale = False

class EditorValidationSourceAttribute:
    def __init__(self):
        self.TypeId = None
        self.MemberName = ""

class IEditorValidationAttribute:
    def __init__(self):
        pass


class EditorMaxLengthAttribute:
    def __init__(self):
        self.TypeId = None
        self.MaxLength = 0

class EditorRangeAttribute:
    def __init__(self):
        self.TypeId = None
