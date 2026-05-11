
from Core.template import Template
from Core.mafi import fix
from Core.ids import Compare_Int_Greater, Compare_Int_Equal, Compare_Int_LessGreater, Connection_Storage, Display_Int, Constant

# How to get IDS, see Core.ids
# Even is not Id defined inside Core.ids, will be correctly displayed

class Favorites(Template, Connection_Storage):
    # This is a favorite section generator,
    # define all your favorite ids inside favorite clause """
    pass

class Const1(Template, Constant):
    name = "[1]"
    def settings(self):
        self.Field.set_int("number", 1)

class Const100(Template, Constant):
    name = "[100]"
    def settings(self):
        self.Field.set_int("number", 100)

class GreateThan99(Template, Compare_Int_Greater):
    name = "[A>99]"
    def settings(self):
        self.Field.set_bool("field_b", True)
        self.Field.set_int("b", 99)

class EqualTo100(Template, Compare_Int_Equal):
    name = "[A=100]"
    def settings(self):
        self.Field.set_bool("field_b", True)
        self.Field.set_int("b", 100)

class Display100dot0(Template, Display_Int):
    # Display_Int defaults to 2-cell / 4-digit width — same as the legacy
    # Display_Int_2 this template originally used.  Players can grow it
    # further from the inspector after placement; the template just sets
    # the float-precision field.
    name = "[100|0]"
    def settings(self):
        self.Field.set_int("float", 1)

# Mode variants of Compare_Int_LessGreater — `picker = True` opts each one
# into the regular module picker so the player picks a configured shape
# (binary / positive / sumup encoding) directly without dropping the
# unconfigured module first and editing the mode field.  The shared
# ModuleProto means every variant uses the same A / B inputs and L / G
# outputs; only the encoding of those outputs differs.

class CompareLessGreater_Binary(Template, Compare_Int_LessGreater):
    # Short bracket-style names match the existing template convention
    # (Const1 = "[1]", GreateThan99 = "[A>99]", etc.) and keep chip width
    # small inside the NewModule entry's body.
    name = "[0/1]"
    picker = True
    def settings(self):
        self.Field.set_int("mode", 0)

class CompareLessGreater_Positive(Template, Compare_Int_LessGreater):
    name = "[0/1/2]"
    picker = True
    def settings(self):
        self.Field.set_int("mode", 1)

class CompareLessGreater_Sumup(Template, Compare_Int_LessGreater):
    name = "[±1]"
    picker = True
    def settings(self):
        self.Field.set_int("mode", 2)