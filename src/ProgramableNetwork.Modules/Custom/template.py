
from Core.template import Template
from Core.mafi import fix
from Core.ids import Compare_Int_Greater, Compare_Int_Equal, Connection_Storage, Display_Int_2, Constant

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

class Display100dot0(Template, Display_Int_2):
    name = "[100|0]"
    def settings(self):
        self.Field.set_int("float", 1)