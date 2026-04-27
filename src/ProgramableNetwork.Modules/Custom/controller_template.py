
from Core.template import Controller
from Core.mafi import fix

# How to get IDS, see Core.ids
# Even is not Id defined inside Core.ids, will be correctly displayed

# used modules
from Core.ids import Arithmetic_Display_7SEG_B, Connection_Display_7SEG_B, Runtime_Clock_2
from Mafi import ColorRgba

# tier contains 16 columns, 4 rows
tier = 1

# Note image must be located within:
# Assets/ProgramableNetwork/Computer/Icon_{id}.png inside modification directory or asset bundle
# in this example: Assets/ProgramableNetwork/Computer/Icon_ClockExample.png
class ClockExample(Controller):
    name = "Basic hour clock"
    description = "Creates an controller configuration for recording time from point in which was controller built."
    color = ColorRgba.CornflowerBlue
    
    def modules(self):
        clock = self.add_module(Runtime_Clock_2, 0, 0);

        converterA = self.add_module(Arithmetic_Display_7SEG_B, 2, 0)
        converterB = self.add_module(Arithmetic_Display_7SEG_B, self.next_row, self.next_column)
        converterC = self.add_module(Arithmetic_Display_7SEG_B, self.next_row, self.next_column)
        converterD = self.add_module(Arithmetic_Display_7SEG_B, self.next_row, self.next_column)
        converterE = self.add_module(Arithmetic_Display_7SEG_B, self.next_row, self.next_column)
        converterF = self.add_module(Arithmetic_Display_7SEG_B, self.next_row, self.next_column)

        displayA = self.add_module(Connection_Display_7SEG_B, 3, 0)
        displayB = self.add_module(Connection_Display_7SEG_B, self.next_row, self.next_column)
        displayC = self.add_module(Connection_Display_7SEG_B, self.next_row, self.next_column)
        displayD = self.add_module(Connection_Display_7SEG_B, self.next_row, self.next_column)
        displayE = self.add_module(Connection_Display_7SEG_B, self.next_row, self.next_column)
        displayF = self.add_module(Connection_Display_7SEG_B, self.next_row, self.next_column)

        displayA["N"] = converterA["bits"]
        displayB["N"] = converterB["bits"]
        displayC["N"] = converterC["bits"]
        displayD["N"] = converterD["bits"]
        displayE["N"] = converterE["bits"]
        displayF["N"] = converterF["bits"]

        converterE["V"] = converterF["rest"]
        converterD["V"] = converterE["rest"]
        converterC["V"] = converterD["rest"]
        converterB["V"] = converterC["rest"]
        converterA["V"] = converterB["rest"]

        converterF["V"] = clock["clock"]

        # forward modules for settings phase
        return [clock]

    def settings(self, modules):
        modules[0].Field.set_int("max_count", 24*60*60)
        modules[0].Field.set_int("clock_period", 10) # each game second