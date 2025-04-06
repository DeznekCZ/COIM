from Core.categories import DefaultCategories
from Core.fields import Int32Field, BooleanField
from Core.io import Input, Output
from Mafi import Fix32
from Core.module import DefaultControllers, Module

# File written by Nightinggale

class Runtime_Max_2(Module):
    name = "Max (2 inputs)"
    symbol = "MAX"

    inputs = [
        Input("A", "A"),
        Input("B", "B")
    ]

    outputs = [
        Output("min", "Min"),
        Output("max", "Max")
    ]
   
    width = 2
    
    categories = [ DefaultCategories.Arithmetic ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        min = self.Input.get("A", Fix32.Zero)
        max = min

        # using min as default means not connected inputs won't change min or max
        # this makes more sense with more inputs, like using 5 of 6 inputs
        temp = self.Input.get("B", min)
        if temp < min:
            min = temp
        if temp > max:
            max = temp

        self.Output.set("min", min)
        self.Output.set("max", max)


class Runtime_Max_3(Module):
    name = "Max (3 inputs)"
    symbol = "MAX"

    inputs = [
        Input("A", "A"),
        Input("B", "B"),
        Input("C", "C")
    ]

    outputs = [
        Output("min", "Min"),
        Output("max", "Max")
    ]

    width = 3

    categories = [ DefaultCategories.Arithmetic ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        min = self.Input.get("A", Fix32.Zero)
        max = min

        temp = self.Input.get("B", min)
        if temp < min:
            min = temp
        if temp > max:
            max = temp

        temp = self.Input.get("C", min)
        if temp < min:
            min = temp
        if temp > max:
            max = temp

        self.Output.set("min", min)
        self.Output.set("max", max)


class Runtime_Max_4(Module):
    name = "Max (4 inputs)"
    symbol = "MAX"

    inputs = [
        Input("A", "A"),
        Input("B", "B"),
        Input("C", "C"),
        Input("D", "D")
    ]

    outputs = [
        Output("min", "Min"),
        Output("max", "Max")
    ]

    width = 4

    categories = [ DefaultCategories.Arithmetic ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        min = self.Input.get("A", Fix32.Zero)
        max = min

        temp = self.Input.get("B", min)
        if temp < min:
            min = temp
        if temp > max:
            max = temp

        temp = self.Input.get("C", min)
        if temp < min:
            min = temp
        if temp > max:
            max = temp

        temp = self.Input.get("D", min)
        if temp < min:
            min = temp
        if temp > max:
            max = temp

        self.Output.set("min", min)
        self.Output.set("max", max)


class Runtime_Max_6(Module):
    name = "Max (6 inputs)"
    symbol = "MAX"

    inputs = [
        Input("A", "A"),
        Input("B", "B"),
        Input("C", "C"),
        Input("D", "D"),
        Input("E", "E"),
        Input("F", "F")
    ]

    outputs = [
        Output("min", "Min"),
        Output("max", "Max")
    ]

    width = 6

    categories = [ DefaultCategories.Arithmetic ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        min = self.Input.get("A", Fix32.Zero)
        max = min

        temp = self.Input.get("B", min)
        if temp < min:
            min = temp
        if temp > max:
            max = temp

        temp = self.Input.get("C", min)
        if temp < min:
            min = temp
        if temp > max:
            max = temp

        temp = self.Input.get("D", min)
        if temp < min:
            min = temp
        if temp > max:
            max = temp

        temp = self.Input.get("E", min)
        if temp < min:
            min = temp
        if temp > max:
            max = temp

        temp = self.Input.get("F", min)
        if temp < min:
            min = temp
        if temp > max:
            max = temp

        self.Output.set("min", min)
        self.Output.set("max", max)


class Runtime_Max_8(Module):
    name = "Max (8 inputs)"
    symbol = "MAX"

    inputs = [
        Input("A", "A"),
        Input("B", "B"),
        Input("C", "C"),
        Input("D", "D"),
        Input("E", "E"),
        Input("F", "F"),
        Input("G", "G"),
        Input("H", "H")
    ]

    outputs = [
        Output("min", "Min"),
        Output("max", "Max")
    ]

    width = 8

    categories = [ DefaultCategories.Arithmetic ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        min = self.Input.get("A", Fix32.Zero)
        max = min

        temp = self.Input.get("B", min)
        if temp < min:
            min = temp
        if temp > max:
            max = temp

        temp = self.Input.get("C", min)
        if temp < min:
            min = temp
        if temp > max:
            max = temp

        temp = self.Input.get("D", min)
        if temp < min:
            min = temp
        if temp > max:
            max = temp

        temp = self.Input.get("E", min)
        if temp < min:
            min = temp
        if temp > max:
            max = temp

        temp = self.Input.get("F", min)
        if temp < min:
            min = temp
        if temp > max:
            max = temp

        temp = self.Input.get("G", min)
        if temp < min:
            min = temp
        if temp > max:
            max = temp

        temp = self.Input.get("H", min)
        if temp < min:
            min = temp
        if temp > max:
            max = temp

        self.Output.set("min", min)
        self.Output.set("max", max)


        
