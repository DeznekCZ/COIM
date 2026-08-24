"""
Load-time test pack for the `with edit_recipe(...)` block form (0.4.0).

Every edit here patches a VANILLA recipe (verified against the game data), so
the pack needs no assets and no other file. Each edit is deliberately silly; the
point is to exercise every sub-action code path once.

Sub-actions covered, all inside `with edit_recipe(recipe):`:

  set_ingredient(product, quantity)   change an existing input amount
  set_product(product, quantity)      change an existing output amount
  remove_ingredient(product)          drop an input
  remove_product(product)             drop an output
  bind_recipe(machine, ...)           add the recipe to another machine
  unbind_recipe(machine)              detach a machine + its research unlock

If this pack loads with no errors in the log, the block form and every
sub-action work end to end. Opening it in the visual editor and saving with no
changes must leave the file byte-identical (the round-trip guard).

Reference (from the decompiled base game):
  IronOreCrushing  16 IronOre -> 16 IronOreCrushed          on Crusher, CrusherLarge
  SlagCrushing     8 Slag -> 8 SlagCrushed                  on Crusher, CrusherLarge
  WaterTreatment   80 WasteWater + 4 Sand + 4 Chlorine
                   -> 40 Water + 12 Sludge                  on WaterTreatmentPlant
  Crusher ports    input A, output X
"""

from Mafi import Duration, Quantity
from Mafi.Base import Ids
from CustomAssets import (edit_recipe, set_ingredient, set_product,
                          remove_ingredient, remove_product,
                          bind_recipe, unbind_recipe, PortMap)


# ---------------------------------------------------------------------------
# 1) Amount edits only — bump the single input and output of a crushing recipe.
#    Neither add nor remove: this is the "edit amount only" path.
# ---------------------------------------------------------------------------
with edit_recipe(Ids.Recipes.IronOreCrushing):
    set_ingredient(Ids.Products.IronOre, Quantity(20))
    set_product(Ids.Products.IronOreCrushed, Quantity(20))


# ---------------------------------------------------------------------------
# 2) Removal — WaterTreatment has three inputs and two outputs, so dropping one
#    of each leaves a still-valid recipe (WasteWater + Chlorine -> Water).
# ---------------------------------------------------------------------------
with edit_recipe(Ids.Recipes.WaterTreatment):
    remove_ingredient(Ids.Products.Sand)
    remove_product(Ids.Products.Sludge)


# ---------------------------------------------------------------------------
# 3) Machine changes — detach SlagCrushing from the large crusher (which also
#    removes the research unlock for that pair), then re-attach it. Silly on its
#    own, but it exercises both unbind and bind against a machine known to be
#    compatible (CrusherLarge originally carried this recipe). Crusher ports are
#    input A / output X.
# ---------------------------------------------------------------------------
with edit_recipe(Ids.Recipes.SlagCrushing):
    unbind_recipe(Ids.Machines.CrusherLarge)
    bind_recipe(Ids.Machines.CrusherLarge,
        duration   = Duration.FromSec(30),
        multiplier = 6,
        ports = [
            PortMap(Ids.Products.Slag,        "A"),
            PortMap(Ids.Products.SlagCrushed, "X")
        ])


# ---------------------------------------------------------------------------
# 4) Mixed edits with an `as` binding, so later code could reference the patched
#    recipe. Exercises the header's `as` clause round-trip.
# ---------------------------------------------------------------------------
with edit_recipe(Ids.Recipes.WaterTreatment) as treated:
    set_ingredient(Ids.Products.Chlorine, Quantity(6))
    set_product(Ids.Products.Water, Quantity(48))
