from Core.categories import DefaultCategories
from Core.fields import Int32Field
from Core.io import Input, Output
from Core.module import DefaultControllers, Module

from Mafi import Fix32


# Reference example for the Module.Array API.  Implements a configurable
# multi-tick delay as an array-copy shift register: every tick reads slot[0],
# shifts the buffer left by one, and writes the new input at the end.  All of
# that happens inside Module.Array.shift_left_with() — Python's parser has no
# for/while loop, so the loop is in C#.  Resize seeds new slots with the most
# recent sample so the output stays continuous when the delay field grows.
class Runtime_Delay_1(Module):
    name = "Control: Delay (configurable)"
    description = "Delays input <b>input</b> by the number of ticks set in the <b>delay</b> field, then emits the same signal on <b>output</b>. A delay of 1 (or less) passes the value through unchanged. Maximum delay is 64 ticks. When the field is increased the new tail of the buffer is seeded with the most recent sample so the output stays continuous; when shrunk the oldest pending samples continue to emit in order."
    symbol = "DLY"
    inputs = [
        Input("input", "Signal input")
    ]
    outputs = [
        Output("output", "Signal output")
    ]
    fields = [
        Int32Field("delay", "Delay", "How many ticks the output should be delayed (capped at 64)", 1)
    ]
    width = 1
    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        input = self.Input.get("input", Fix32.Zero)
        delay = self.Field.get_int("delay", 1)
        if delay > 64:
            delay = 64

        # Pass-through fast path — drop the buffer if the field shrank.
        if delay <= 1:
            if self.Array.length > 0:
                self.Array.resize(0)
            self.Output.set("output", input)
            return

        # Keep buffer sized to the field.  On grow, seed new tail slots with
        # the most recent sample so the output stays continuous; on shrink,
        # resize() truncates from the tail (oldest pending samples keep
        # emitting in order).
        old_length = self.Array.length
        if old_length != delay:
            last_value = Fix32.Zero
            if old_length > 0:
                last_value = self.Array.get(old_length - 1, Fix32.Zero)
            self.Array.resize(delay, last_value)

        # Atomic shift-register step: shift left, push input at end, return
        # what was at slot[0].  That value has been waiting `delay` ticks.
        oldest = self.Array.shift_left_with(input)
        self.Output.set("output", oldest)


# Multi-tap shift register: outputs <b>1</b>..<b>N</b> carry the input from 1
# to N ticks ago.  Refactored from the legacy Delay_2/4/6/8 set into a single
# extensible Runtime_Delay_Tap — base 1 output ("1") plus up to 7 player-
# added taps ("2"..."8") on the right edge of the module.  Default extension
# namer continues digit pin ids (so "1" → "2", "3", ...).  The shift uses the
# Output cache from the previous tick: slot[N] = old slot[N-1], cascaded down.
class Runtime_Delay_Tap(Module):
    name = "Control: Delay (taps)"
    description = "Multi-stage shift register: each output tap holds the input from N ticks ago, where N matches the tap label.  Add more taps from the right edge of the module."
    symbol = "DLY"
    inputs = [
        Input("0", "Signal input")
    ]
    outputs = [
        Output("1", "Delay by 1 tick")
    ]

    width = 2

    # 1 static tap + up to 7 ext = 8 taps total — covers Delay_8.
    output_extensions = 7

    # Each entry is [old_id, input_ext, output_ext, display_ext].  The custom
    # parser only supports list literals (no tuples, no None), so we use 0 to
    # mean "leave at default".  Delay_Tap has no input extensions, so
    # input_ext=0 is the right default.
    deprecates = [
        ["Runtime_Delay_2", 0, 1],
        ["Runtime_Delay_4", 0, 3],
        ["Runtime_Delay_6", 0, 5],
        ["Runtime_Delay_8", 0, 7]
    ]

    categories = [ DefaultCategories.Control ]
    controllers = [ DefaultControllers.Controller ]

    def action(self):
        a = self.Input.get("0", Fix32.Zero)
        # effective_output_count = 1 + active output extensions; cascade from
        # the highest tap down so each slot reads the *previous* tick's value
        # of the slot one to its left before being overwritten this tick.
        n = self.effective_output_count
        if n > 1:
            self._shift(n - 1)
        self.Output.set("1", a)

    def _shift(self, slot):
        # `slot` is the ordinal index in EffectiveOutputs (0-based).  Slot 0
        # is "1", slot 1 is "2", and so on.  Cascade ends when slot is 1
        # (output "2" reads from output "1", which gets refreshed below).
        if slot <= 0:
            return
        dst = self.effective_output_id(slot)
        src = self.effective_output_id(slot - 1)
        self.Output.set(dst, self.Output.get(src, Fix32.Zero))
        self._shift(slot - 1)
