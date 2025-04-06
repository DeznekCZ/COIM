from Core.categories import DefaultCategories
from Core.fields import Int32Field
from Core.io import Input, Output
from Mafi import Fix32
from Core.module import DefaultControllers, Module

# File written by Nightinggale

class Runtime_Shift_2(Module):
    name = "Control: Shift (2 inputs)"
    symbol = "SHIFT"

    inputs = [
        Input("index", "Index"),
        Input("0", "Input 0"),
        Input("1", "Input 1")
    ]

    outputs = [
        Output("index", "Index"),
        Output("0", "Output 0"),
        Output("1", "Output 1")
    ]

    width = 3

    categories = [DefaultCategories.Control]
    controllers = [DefaultControllers.Controller]

    def action(self):
        # the action isn't copy paste like in the other sizes
        # the reason being that this is so simple that writing each case is reasonable and it executes faster.
        index = self.Input.get_int("index", 0)
        index = index % 2
        self.Output.set_int("index", index)

        if index == 0:
            self.Output.set("0", self.Input.get("0", Fix32.Zero))
            self.Output.set("1", self.Input.get("1", Fix32.Zero))
        else:
            self.Output.set("0", self.Input.get("1", Fix32.Zero))
            self.Output.set("1", self.Input.get("0", Fix32.Zero))



class Runtime_Shift_4(Module):
    name = "Control: Shift (4 inputs)"
    symbol = "SHIFT"

    inputs = [
        Input("index", "Index"),
        Input("0", "Input 0"),
        Input("1", "Input 1"),
        Input("2", "Input 2"),
        Input("3", "Input 3")
    ]

    outputs = [
        Output("index", "Index"),
        Output("0", "Output 0"),
        Output("1", "Output 1"),
        Output("2", "Output 2"),
        Output("3", "Output 3")
    ]

    fields = [
        Int32Field("inputs used", "Number of inputs used", "Select how many inputs should be used. The same amount of outputs will then be used, leaving the rest of the outputs not updating.", 4)
    ]
   
    width = 5
    
    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        num_inputs = 4

        # Everything below this line is intentionally set to not be specific to the number of inputs.
        # This makes it easier to copy paste as well as opening up for some class inheritance reuse of code.

        requested_num_inputs = self.Field.get_int("inputs used", num_inputs)
        if requested_num_inputs < 2:
            requested_num_inputs = 2
        if requested_num_inputs < num_inputs:
            num_inputs = requested_num_inputs


        # set index to the range matching the number of inputs
        shift_offset = self.Input.get_int("index", 0)
        if shift_offset < 0:
            # negative numbers are shifted to positive.
            # multiplying with num_inputs will ensure the post modulo number won't be affected by this offset.
            temp = shift_offset * num_inputs
            shift_count = shift_offset - temp
        if shift_offset >= num_inputs:
            shift_offset = shift_offset % num_inputs
        self.Output.set_int("index", shift_offset)

        self.set_output(0, num_inputs, shift_offset)

    def str(self, value, num_inputs):
        if value >= num_inputs:
            value = value - num_inputs

        if value == 0:
            return "0"
        if value == 1:
            return "1"
        if value == 2:
            return "2"
        if value == 3:
            return "3"
        if value == 4:
            return "4"
        if value == 5:
            return "5"
        if value == 6:
            return "6"
        if value == 7:
            return "7"


    def get_output(self, index, num_inputs):
        if index >= num_inputs:
            index = index - num_inputs
        return index

    def set_output(self, index, num_inputs, shift_offset):
        input = self.str(index, num_inputs)
        output = self.str(index + shift_offset, num_inputs)
        self.Output.set(output, self.Input.get(input, Fix32.Zero))
        index = index + 1
        if index < num_inputs:
            self.set_output(index, num_inputs, shift_offset)

class Runtime_Shift_7(Module):
    name = "Control: Shift (7 inputs)"
    symbol = "SHIFT"

    inputs = [
        Input("index", "Index"),
        Input("0", "Input 0"),
        Input("1", "Input 1"),
        Input("2", "Input 2"),
        Input("3", "Input 3"),
        Input("4", "Input 4"),
        Input("5", "Input 5"),
        Input("6", "Input 6")
    ]

    outputs = [
        Output("index", "Index"),
        Output("0", "Output 0"),
        Output("1", "Output 1"),
        Output("2", "Output 2"),
        Output("3", "Output 3"),
        Output("4", "Output 4"),
        Output("5", "Output 5"),
        Output("6", "Output 6")
    ]

    fields = [
        Int32Field("inputs used", "Number of inputs used", "Select how many inputs should be used. The same amount of outputs will then be used, leaving the rest of the outputs not updating.", 7)
    ]

    width = 8

    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        num_inputs = 7

        # Everything below this line is intentionally set to not be specific to the number of inputs.
        # This makes it easier to copy paste as well as opening up for some class inheritance reuse of code.

        requested_num_inputs = self.Field.get_int("inputs used", num_inputs)
        if requested_num_inputs < 2:
            requested_num_inputs = 2
        if requested_num_inputs < num_inputs:
            num_inputs = requested_num_inputs


        # set index to the range matching the number of inputs
        shift_offset = self.Input.get_int("index", 0)
        if shift_offset < 0:
            # negative numbers are shifted to positive.
            # multiplying with num_inputs will ensure the post modulo number won't be affected by this offset.
            temp = shift_offset * num_inputs
            shift_count = shift_offset - temp
        if shift_offset >= num_inputs:
            shift_offset = shift_offset % num_inputs
        self.Output.set_int("index", shift_offset)

        self.set_output(0, num_inputs, shift_offset)

    def str(self, value, num_inputs):
        if value >= num_inputs:
            value = value - num_inputs

        if value == 0:
            return "0"
        if value == 1:
            return "1"
        if value == 2:
            return "2"
        if value == 3:
            return "3"
        if value == 4:
            return "4"
        if value == 5:
            return "5"
        if value == 6:
            return "6"
        if value == 7:
            return "7"


    def get_output(self, index, num_inputs):
        if index >= num_inputs:
            index = index - num_inputs
        return index

    def set_output(self, index, num_inputs, shift_offset):
        input = self.str(index, num_inputs)
        output = self.str(index + shift_offset, num_inputs)
        self.Output.set(output, self.Input.get(input, Fix32.Zero))
        index = index + 1
        if index < num_inputs:
            self.set_output(index, num_inputs, shift_offset)
