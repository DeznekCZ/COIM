# Battery -> charged-battery recipes (placeholder Charger).
#
# Python-only iteration: charging is hosted on existing assembler machines, with
# the recipe `power` percent set high to represent the electricity draw. A proper
# BatteryCharger machine + C# build_machine extension are a follow-up step.
#
# Round-trip targets (charger input vs. discharger output):
#   Lead-acid:    200 power x 20s = 4000 power-s in -> discharger 160 x 20s = 3200 out  (~80% RTE)
#   Lithium-ion: 1000 power x 20s = 20000 power-s in -> discharger 900 x 20s = 18000 out (~90% RTE)

from Mafi import Duration, Quantity
from Mafi.Base import Ids
from CustomAssets import build_recipe, Product

# Load order is declared centrally in Definitions/__init__.py.

# ---------------------------------------------------------------------------
# Lead-acid charging (Assembly II / Electrified)
# ---------------------------------------------------------------------------

build_recipe(
    recipeId    = "CustomRecipe_LeadAcidBattery_Charge",
    name        = "Lead-acid battery charging",
    description = "Charge an empty lead-acid battery. Consumes extra electricity above the assembler baseline; lower charging cost than lithium-ion.",
    machine     = Ids.Machines.AssemblyElectrified,
    research    = "CustomResearch_LeadAcidBattery",
    duration    = Duration.FromSec(20),
    power       = 200,
    ingredients = [
        Product("Product_LeadAcidBatteryEmpty", Quantity(1))
    ],
    products = [
        Product("Product_LeadAcidBatteryCharged", Quantity(1))
    ]
)

build_recipe(
    recipeId    = "CustomRecipe_LeadAcidBattery_Charge_T2",
    name        = "Lead-acid battery charging",
    description = "Charge an empty lead-acid battery. Consumes extra electricity above the assembler baseline; lower charging cost than lithium-ion.",
    machine     = Ids.Machines.AssemblyElectrifiedT2,
    research    = "CustomResearch_LeadAcidBattery",
    duration    = Duration.FromSec(20),
    power       = 200,
    ingredients = [
        Product("Product_LeadAcidBatteryEmpty", Quantity(2))
    ],
    products = [
        Product("Product_LeadAcidBatteryCharged", Quantity(2))
    ]
)

# ---------------------------------------------------------------------------
# Lithium-ion charging (Assembly IV / RoboticT1)
# ---------------------------------------------------------------------------

build_recipe(
    recipeId    = "CustomRecipe_LithiumIonBattery_Charge",
    name        = "Lithium-ion battery charging",
    description = "Charge an empty lithium-ion battery pack. Significantly higher electricity draw than lead-acid because the pack stores much more energy.",
    machine     = Ids.Machines.AssemblyRoboticT1,
    research    = "CustomResearch_LithiumIonBattery",
    duration    = Duration.FromSec(20),
    power       = 1000,
    ingredients = [
        Product("Product_LithiumIonBatteryEmpty", Quantity(1))
    ],
    products = [
        Product("Product_LithiumIonBatteryCharged", Quantity(1))
    ]
)

build_recipe(
    recipeId    = "CustomRecipe_LithiumIonBattery_Charge_T2",
    name        = "Lithium-ion battery charging",
    description = "Charge an empty lithium-ion battery pack. Significantly higher electricity draw than lead-acid because the pack stores much more energy.",
    machine     = Ids.Machines.AssemblyRoboticT2,
    research    = "CustomResearch_LithiumIonBattery",
    duration    = Duration.FromSec(20),
    power       = 1000,
    ingredients = [
        Product("Product_LithiumIonBatteryEmpty", Quantity(2))
    ],
    products = [
        Product("Product_LithiumIonBatteryCharged", Quantity(2))
    ]
)
