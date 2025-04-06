from Core.categories import DefaultCategories
from Core.fields import Int32Field
from Core.io import Input, Output
from Mafi import Fix32
from Core.module import DefaultControllers, Module

# File written by Nightinggale

class Runtime_FlipFlop_1(Module):
    name = "Control: Flip-Flop (1 input)"
    symbol = "FLIP-F"
    inputs = [
        Input("enable", "Enable"),
        Input("in_1", "A")
    ]
    outputs = [
        Output("out_1", "A")
    ]
   
    
    width = 2
    
    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        if not self.Input.get_bool("enable", False):
            return

        self.Output.set("out_1", self.Input.get("in_1", Fix32.Zero))
        

class Runtime_FlipFlop_2(Module):
    name = "Control: Flip-Flop (2 inputs)"
    symbol = "FLIP-FLOP"
    inputs = [
        Input("enable", "Enable"),
        Input("in_1", "A"),
        Input("in_2", "B")
    ]
    outputs = [
        Output("out_1", "A"),
        Output("out_2", "B")
    ]
   
    
    width = 3
    
    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        if not self.Input.get_bool("enable", False):
            return

        self.Output.set("output", self.Input.get("in_1", Fix32.Zero))
        self.Output.set("output", self.Input.get("in_1", Fix32.Zero))
        
        

class Runtime_FlipFlop_3(Module):
    name = "Control: Flip-Flop (3 inputs)"
    symbol = "FLIP-FLOP"
    inputs = [
        Input("enable", "Enable"),
        Input("in_1", "A"),
        Input("in_2", "B"),
        Input("in_3", "C")
    ]
    outputs = [
        Output("out_1", "A"),
        Output("out_2", "B"),
        Output("out_3", "C")
    ]
   
    
    width = 4
    
    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        if not self.Input.get_bool("enable", False):
            return

        self.Output.set("out_1", self.Input.get("in_1", Fix32.Zero))
        self.Output.set("out_2", self.Input.get("in_2", Fix32.Zero))
        self.Output.set("out_3", self.Input.get("in_3", Fix32.Zero))
        
        

class Runtime_FlipFlop_4(Module):
    name = "Control: Flip-Flop (4 inputs)"
    symbol = "FLIP-FLOP"
    inputs = [
        Input("enable", "Enable"),
        Input("in_1", "A"),
        Input("in_2", "B"),
        Input("in_3", "C"),
        Input("in_4", "D")
    ]
    outputs = [
        Output("out_1", "A"),
        Output("out_2", "B"),
        Output("out_3", "C"),
        Output("out_4", "D")
    ]
   
    
    width = 5
    
    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        if not self.Input.get_bool("enable", False):
            return

        self.Output.set("out_1", self.Input.get("in_1", Fix32.Zero))
        self.Output.set("out_2", self.Input.get("in_2", Fix32.Zero))
        self.Output.set("out_3", self.Input.get("in_3", Fix32.Zero))
        self.Output.set("out_4", self.Input.get("in_4", Fix32.Zero))
        
        

class Runtime_FlipFlop_7(Module):
    name = "Control: Flip-Flop (7 inputs)"
    symbol = "FLIP-FLOP"
    inputs = [
        Input("enable", "Enable"),
        Input("in_1", "A"),
        Input("in_2", "B"),
        Input("in_3", "C"),
        Input("in_4", "D"),
        Input("in_5", "E"),
        Input("in_6", "F"),
        Input("in_7", "G")
    ]
    outputs = [
        Output("out_1", "A"),
        Output("out_2", "B"),
        Output("out_3", "C"),
        Output("out_4", "D"),
        Output("out_5", "E"),
        Output("out_6", "F"),
        Output("out_7", "G")
    ]
   
    
    width = 8
    
    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        if not self.Input.get_bool("enable", False):
            return

        self.Output.set("out_1", self.Input.get("in_1", Fix32.Zero))
        self.Output.set("out_2", self.Input.get("in_2", Fix32.Zero))
        self.Output.set("out_3", self.Input.get("in_3", Fix32.Zero))
        self.Output.set("out_4", self.Input.get("in_4", Fix32.Zero))
        self.Output.set("out_5", self.Input.get("in_5", Fix32.Zero))
        self.Output.set("out_6", self.Input.get("in_6", Fix32.Zero))
        self.Output.set("out_7", self.Input.get("in_7", Fix32.Zero))
        
