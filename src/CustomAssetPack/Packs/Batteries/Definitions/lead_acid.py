# Lead-acid battery chain.
#
# Research: CustomResearch_LeadAcidBattery, parent = SulfurProcessing
#
# Production:
#   1. Lead recovery       : ChemicalPlant      : ImpureCopper + Acid -> Copper + Lead
#      (Lead modelled as loose anode-mud, sharing CP's 1U-out + 1L-out with Copper.
#      Hooks into the vanilla copper chain at the electrolysis step - player runs
#      this recipe instead of the vanilla ElectrolyzerT2 path when they want lead.)
#   2. Battery assembly    : ChemicalPlant      : Lead + Acid + Plastic -> LeadAcidBatteryEmpty
#      (Loose Lead + fluid Acid + unit Plastic - assemblers don't take loose/fluid,
#      so the assembly is hosted on ChemicalPlant.)
#   3. Charging            : AssemblyElectrified : LeadAcidBatteryEmpty -> LeadAcidBatteryCharged
#      (See battery_charger.py. Power-only step, no fluid input.)
#   4. Recycling           : Shredder           : LeadAcidBatteryEmpty -> Lead + Plastic
#   5. Consumer alt-recipe : AssemblyElectrified : ChargedBattery + Electronics2 + Plastic
#                                                 -> ConsumerElectronics

from Mafi import Duration, Quantity
from Mafi.Base import Ids
from CustomAssets import (
    add_unlock_product,
    build_recipe,
    build_research,
    Product
)

# Load order is declared centrally in Definitions/__init__.py.

# ---------------------------------------------------------------------------
# Research
# ---------------------------------------------------------------------------

research_lead_acid = build_research(
    researchId  = "CustomResearch_LeadAcidBattery",
    name        = "Lead-acid batteries",
    description = "Recover lead from copper-electrolysis anode mud, combine with acid and plastic into rugged batteries.",
    position    = (80, 50),
    parents     = [ Ids.Research.SulfurProcessing ]
)

add_unlock_product(research_lead_acid, "Product_Lead")
add_unlock_product(research_lead_acid, "Product_LeadAcidBatteryEmpty")
add_unlock_product(research_lead_acid, "Product_LeadAcidBatteryCharged")

# ---------------------------------------------------------------------------
# 1) Lead recovery via copper electrolysis (Chemical Plant)
# ---------------------------------------------------------------------------
# Vanilla copper chain: Scrap+Coal -> Furnace -> MoltenCopper -> Caster ->
# ImpureCopper -> ElectrolyzerT2 (with Acid) -> Copper.
# Our alt: same ImpureCopper + Acid -> Copper, hosted on ChemicalPlant so Lead
# (loose anode mud) comes out alongside. Port shape: 1U-in + 1F-in -> 1U-out + 1L-out.

# build_recipe(
#     recipeId    = "CustomRecipe_LeadRecovery",
#     name        = "Copper electrolysis (with lead recovery)",
#     description = "Electrolytically refine impure copper with acid in the chemical plant. Recovers lead from the anode mud as a by-product.",
#     machine     = Ids.Machines.ChemicalPlant,
#     duration    = Duration.FromSec(30),
#     research    = research_lead_acid,
#     ingredients = [
#         Product(Ids.Products.ImpureCopper, Quantity(2)),
#         Product(Ids.Products.Acid,         Quantity(2))
#     ],
#     products = [
#         Product(Ids.Products.Copper, Quantity(2)),
#         Product("Product_Lead",      Quantity(1))
#     ]
# )

build_recipe(
    recipeId    = "CustomRecipe_LeadRecovery_T2",
    name        = "Copper electrolysis (with lead recovery)",
    description = "Electrolytically refine impure copper with acid in the chemical plant. Recovers lead from the anode mud as a by-product.",
    machine     = Ids.Machines.ChemicalPlant2,
    duration    = Duration.FromSec(30),
    research    = research_lead_acid,
    ingredients = [
        Product(Ids.Products.ImpureCopper, Quantity(4)),
        Product(Ids.Products.Acid,         Quantity(4))
    ],
    products = [
        Product(Ids.Products.Copper, Quantity(4)),
        Product("Product_Lead",      Quantity(2))
    ]
)

# ---------------------------------------------------------------------------
# 2) Lead-acid battery assembly (Chemical Plant)
# ---------------------------------------------------------------------------
# Assembler can't take loose Lead or fluid Acid - host on ChemicalPlant instead.
# Port shape: 1L-in + 1F-in + 1U-in -> 1U-out. Fits CP cleanly.

build_recipe(
    recipeId    = "CustomRecipe_LeadAcidBattery_Assembly",
    name        = "Lead-acid battery assembly",
    description = "Combine lead, sulfuric acid and plastic casing into rugged batteries in the chemical plant.",
    machine     = Ids.Machines.ChemicalPlant,
    duration    = Duration.FromSec(40),
    research    = research_lead_acid,
    ingredients = [
        Product("Product_Lead",       Quantity(4)),
        Product(Ids.Products.Acid,    Quantity(2)),
        Product(Ids.Products.Plastic, Quantity(1))
    ],
    products = [
        Product("Product_LeadAcidBatteryEmpty", Quantity(4))
    ]
)

build_recipe(
    recipeId    = "CustomRecipe_LeadAcidBattery_Assembly_T2",
    name        = "Lead-acid battery assembly",
    description = "Combine lead, sulfuric acid and plastic casing into rugged batteries in the chemical plant.",
    machine     = Ids.Machines.ChemicalPlant2,
    duration    = Duration.FromSec(40),
    research    = research_lead_acid,
    ingredients = [
        Product("Product_Lead",       Quantity(8)),
        Product(Ids.Products.Acid,    Quantity(4)),
        Product(Ids.Products.Plastic, Quantity(2))
    ],
    products = [
        Product("Product_LeadAcidBatteryEmpty", Quantity(8))
    ]
)

# ---------------------------------------------------------------------------
# 3) Recycling: empty batteries -> raw materials (Shredder)
# ---------------------------------------------------------------------------
# Shredder has only 1 output port — recover Lead only (plastic is lost in the shred).

build_recipe(
    recipeId    = "CustomRecipe_LeadAcidBattery_Recycle",
    name        = "Lead-acid battery recycling",
    description = "Shred empty lead-acid batteries to recover lead. Plastic case and residual acid are consumed.",
    machine     = Ids.Machines.Shredder,
    duration    = Duration.FromSec(30),
    research    = research_lead_acid,
    ingredients = [
        Product("Product_LeadAcidBatteryEmpty", Quantity(4))
    ],
    products = [
        Product("Product_Lead", Quantity(3))
    ]
)

# ---------------------------------------------------------------------------
# 4) Consumer-electronics alt-recipe (settlement sink)
# ---------------------------------------------------------------------------

build_recipe(
    recipeId    = "CustomRecipe_ConsumerElectronics_WithLeadAcid",
    name        = "Consumer electronics (with lead-acid)",
    description = "Alternate consumer-electronics recipe that uses lead-acid batteries as a power-supply component.",
    machine     = Ids.Machines.AssemblyElectrified,
    duration    = Duration.FromSec(40),
    research    = research_lead_acid,
    ingredients = [
        Product("Product_LeadAcidBatteryCharged", Quantity(2)),
        Product(Ids.Products.Electronics2,        Quantity(2)),
        Product(Ids.Products.Plastic,             Quantity(1))
    ],
    products = [
        Product(Ids.Products.ConsumerElectronics, Quantity(2))
    ]
)
