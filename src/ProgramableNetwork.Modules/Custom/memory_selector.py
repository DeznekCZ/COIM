from Core.categories import DefaultCategories
from Core.fields import Int32Field
from Core.io import Input, Output
from Mafi import Fix32
from Core.module import DefaultControllers, Module

# File written by Nightinggale

class Runtime_Memory_Selector_1(Module):
    name = "Control: Memory Selector (1 input)"
    symbol = "MS"
    inputs = [
        Input("1", "Signal input")
    ]
    outputs = [
        Output("output", "Signal output")
    ]
    
    fields = [
        Int32Field("default", "Default", "Output this when none of the inputs are positive", 0),
        Int32Field("field_1", "1 value", "Output this when input 1 is positive", 1)
    ]
    
    width = 1
    
    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        if self.Input.get_bool("in_1", False):
            self.Output.set("output", self.Field.get("1", Fix32.Zero))
        else:
            self.Output.set("output", self.Field.get("default", Fix32.Zero))


class Runtime_Memory_Selector_2(Module):
    name = "Control: Memory Selector (2 inputs)"
    symbol = "MS"
    inputs = [
        Input("in_1", "Input 1"),
        Input("in_2", "Input 2")
    ]
    outputs = [
        Output("output", "Signal output")
    ]
    
    fields = [
        Int32Field("default", "Default", "Output this when none of the inputs are positive", 0),
        Int32Field("field_1", "1 value", "Output this when input 1 is positive", 1),
        Int32Field("field_2", "2 value", "Output this when input 2 is positive", 2)
    ]
    
    width = 2
    
    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        if self.Input.get_bool("in_2", False):
            self.Output.set("output", self.Field.get("field_2", Fix32.Zero))
            return
 
        if self.Input.get_bool("in_1", False):
            self.Output.set("output", self.Field.get("field_1", Fix32.Zero))
            return

        self.Output.set("output", self.Field.get("default", Fix32.Zero))

class Runtime_Memory_Selector_4(Module):
    name = "Control: Memory Selector (4 inputs)"
    symbol = "MEM SELECT"
    inputs = [
        Input("in_1", "Input 1"),
        Input("in_2", "Input 2"),
        Input("in_3", "Input 3"),
        Input("in_4", "Input 4")
    ]
    outputs = [
        Output("output", "Signal output")
    ]
    
    fields = [
        Int32Field("default", "Default", "Output this when none of the inputs are positive", 0),
        Int32Field("field_1", "1 value", "Output this when input 1 is positive", 1),
        Int32Field("field_2", "2 value", "Output this when input 2 is positive", 2),
        Int32Field("field_3", "3 value", "Output this when input 3 is positive", 3),
        Int32Field("field_4", "4 value", "Output this when input 4 is positive", 4)
    ]
    
    width = 4
    
    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        if self.Input.get_bool("in_4", False):
            self.Output.set("output", self.Field.get("field_4", Fix32.Zero))
            return

        if self.Input.get_bool("in_3", False):
            self.Output.set("output", self.Field.get("field_3", Fix32.Zero))
            return

        if self.Input.get_bool("in_2", False):
            self.Output.set("output", self.Field.get("field_2", Fix32.Zero))
            return
 
        if self.Input.get_bool("in_1", False):
            self.Output.set("output", self.Field.get("field_1", Fix32.Zero))
            return

        self.Output.set("output", self.Field.get("default", Fix32.Zero))

class Runtime_Memory_Selector_6(Module):
    name = "Control: Memory Selector (6 inputs)"
    symbol = "Memory Selector"
    inputs = [
        Input("in_1", "Input 1"),
        Input("in_2", "Input 2"),
        Input("in_3", "Input 3"),
        Input("in_4", "Input 4"),
        Input("in_5", "Input 5"),
        Input("in_6", "Input 6")
    ]
    outputs = [
        Output("output", "Signal output")
    ]
    
    fields = [
        Int32Field("default", "Default", "Output this when none of the inputs are positive", 0),
        Int32Field("field_1", "1 value", "Output this when input 1 is positive", 1),
        Int32Field("field_2", "2 value", "Output this when input 2 is positive", 2),
        Int32Field("field_3", "3 value", "Output this when input 3 is positive", 3),
        Int32Field("field_4", "4 value", "Output this when input 4 is positive", 4),
        Int32Field("field_5", "5 value", "Output this when input 5 is positive", 5),
        Int32Field("field_6", "6 value", "Output this when input 6 is positive", 6)
    ]
    
    width = 6
    
    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        if self.Input.get_bool("in_6", False):
            self.Output.set("output", self.Field.get("field_6", Fix32.Zero))
            return

        if self.Input.get_bool("in_5", False):
            self.Output.set("output", self.Field.get("field_5", Fix32.Zero))
            return

        if self.Input.get_bool("in_4", False):
            self.Output.set("output", self.Field.get("field_4", Fix32.Zero))
            return

        if self.Input.get_bool("in_3", False):
            self.Output.set("output", self.Field.get("field_3", Fix32.Zero))
            return

        if self.Input.get_bool("in_2", False):
            self.Output.set("output", self.Field.get("field_2", Fix32.Zero))
            return
 
        if self.Input.get_bool("in_1", False):
            self.Output.set("output", self.Field.get("field_1", Fix32.Zero))
            return

        self.Output.set("output", self.Field.get("default", Fix32.Zero))

class Runtime_Memory_Selector_8(Module):
    name = "Control: Memory Selector (8 inputs)"
    symbol = "Memory Selector"
    inputs = [
        Input("in_1", "Input 1"),
        Input("in_2", "Input 2"),
        Input("in_3", "Input 3"),
        Input("in_4", "Input 4"),
        Input("in_5", "Input 5"),
        Input("in_6", "Input 6"),
        Input("in_7", "Input 7"),
        Input("in_8", "Input 8")
    ]
    outputs = [
        Output("output", "Signal output")
    ]
    
    fields = [
        Int32Field("default", "Default", "Output this when none of the inputs are positive", 0),
        Int32Field("field_1", "1 value", "Output this when input 1 is positive", 1),
        Int32Field("field_2", "2 value", "Output this when input 2 is positive", 2),
        Int32Field("field_3", "3 value", "Output this when input 3 is positive", 3),
        Int32Field("field_4", "4 value", "Output this when input 4 is positive", 4),
        Int32Field("field_5", "5 value", "Output this when input 5 is positive", 5),
        Int32Field("field_6", "6 value", "Output this when input 6 is positive", 6),
        Int32Field("field_7", "7 value", "Output this when input 7 is positive", 7),
        Int32Field("field_8", "8 value", "Output this when input 8 is positive", 8)
    ]
    
    width = 8
    
    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        if self.Input.get_bool("in_8", False):
            self.Output.set("output", self.Field.get("field_8", Fix32.Zero))
            return

        if self.Input.get_bool("in_7", False):
            self.Output.set("output", self.Field.get("field_7", Fix32.Zero))
            return

        if self.Input.get_bool("in_6", False):
            self.Output.set("output", self.Field.get("field_6", Fix32.Zero))
            return

        if self.Input.get_bool("in_5", False):
            self.Output.set("output", self.Field.get("field_5", Fix32.Zero))
            return

        if self.Input.get_bool("in_4", False):
            self.Output.set("output", self.Field.get("field_4", Fix32.Zero))
            return

        if self.Input.get_bool("in_3", False):
            self.Output.set("output", self.Field.get("field_3", Fix32.Zero))
            return

        if self.Input.get_bool("in_2", False):
            self.Output.set("output", self.Field.get("field_2", Fix32.Zero))
            return
 
        if self.Input.get_bool("in_1", False):
            self.Output.set("output", self.Field.get("field_1", Fix32.Zero))
            return

        self.Output.set("output", self.Field.get("default", Fix32.Zero))


