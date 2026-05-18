# Lithium-ion battery chain.
#
# Research: CustomResearch_LithiumIonBattery, parent = RoboticAssembly
#   (Assembly IV — the user wanted lithium tech gated behind robotized factories.)
#
# Production:
#   1. Lithium extraction  : ElectrolyzerT2  : Brine + Acid -> Lithium + Salt
#   2. Battery assembly    : AssemblyRoboticT1 : Lithium + Copper + Plastic + Electronics4
#                                                -> LithiumIonBattery
#   3. Consumer alt-recipe : AssemblyRoboticT1 : LithiumIonBattery + Electronics3 + Plastic
#                                                -> ConsumerElectronics
#
# All recipes gated by the same research node.

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

research_lithium = build_research(
    researchId  = "CustomResearch_LithiumIonBattery",
    name        = "Lithium-ion batteries",
    description = "Extract lithium from brine and assemble high-density lithium-ion battery packs in the robotic assembler.",
    position    = (180, 50),
    parents     = [ Ids.Research.RoboticAssembly ]
)

add_unlock_product(research_lithium, "Product_Lithium")
add_unlock_product(research_lithium, "Product_LithiumIonBatteryEmpty")
add_unlock_product(research_lithium, "Product_LithiumIonBatteryCharged")

# ---------------------------------------------------------------------------
# 1) Lithium extraction from brine
# ---------------------------------------------------------------------------
# Real-world basis: lithium is commercially produced by evaporating brine pools
# and processing the resulting lithium-rich liquor.
#
# Host: ChemicalPlant (flexible ports). The Electrolyzer/ElectrolyzerT2 were
# tempting but their port shapes don't fit: Electrolyzer is fluid-only (no unit
# in/out at all), and ElectrolyzerT2 (CopperElectrolysis) accepts only
# 1 fluid + 1 unit in -> 1 unit out, which can't carry the Brine + Acid + Salt
# mix we need. ChemicalPlant handles fluid+fluid -> unit+loose comfortably.

# Lithium chemistry is locked behind ChemicalPlant2: CP T1 has no loose-output
# port and the Salt by-product is a defining feature of the extraction reaction
# (real-world brine evaporation pools yield salt alongside lithium). Rather than
# offering a Salt-less T1 compromise, we gate the whole recipe behind T2 so the
# chemistry stays consistent with the lithium-ion research tier (post-RoboticAssembly,
# by which time players already have ChemicalPlant2 unlocked).

build_recipe(
    recipeId    = "CustomRecipe_LithiumExtraction_T2",
    name        = "Lithium extraction (brine chemistry)",
    description = "Treat acidified brine in the chemical plant to recover lithium metal, with salt as a by-product.",
    machine     = Ids.Machines.ChemicalPlant2,
    duration    = Duration.FromSec(30),
    research    = research_lithium,
    ingredients = [
        Product(Ids.Products.Brine, Quantity(32)),
        Product(Ids.Products.Acid,  Quantity(4))
    ],
    products = [
        Product("Product_Lithium", Quantity(4)),
        Product(Ids.Products.Salt, Quantity(4))
    ]
)

# ---------------------------------------------------------------------------
# 2) Lithium-ion battery assembly
# ---------------------------------------------------------------------------

# Two-step assembly chain (COI assemblers cap at 3 input ports):
#   Step 1: Cell production - multiple alternate recipes, same output.
#           Lithium + (Copper | Aluminum | Graphite path) + Plastic -> LithiumIonCell
#   Step 2: Battery pack    - cells wired together with control electronics.
#           LithiumIonCell + Electronics2 -> LithiumIonBatteryEmpty

# Step 1a: cells with copper current collectors (basic).
build_recipe(
    recipeId    = "CustomRecipe_LithiumIonCell_Copper",
    name        = "Lithium-ion cell (copper)",
    description = "Coil lithium with a copper current collector and plastic separator into electrochemical cells.",
    machine     = Ids.Machines.AssemblyRoboticT1,
    duration    = Duration.FromSec(20),
    research    = research_lithium,
    ingredients = [
        Product("Product_Lithium",     Quantity(1)),
        Product(Ids.Products.Copper,   Quantity(1)),
        Product(Ids.Products.Plastic,  Quantity(1))
    ],
    products = [
        Product("Product_LithiumIonCell", Quantity(4))
    ]
)

# Step 1b: cells with aluminum cathode collector (higher yield).
build_recipe(
    recipeId    = "CustomRecipe_LithiumIonCell_Aluminum",
    name        = "Lithium-ion cell (aluminum)",
    description = "Coil lithium with an aluminum cathode collector — lighter and more efficient than copper.",
    machine     = Ids.Machines.AssemblyRoboticT1,
    duration    = Duration.FromSec(20),
    research    = research_lithium,
    ingredients = [
        Product("Product_Lithium",     Quantity(1)),
        Product(Ids.Products.Aluminum, Quantity(1)),
        Product(Ids.Products.Plastic,  Quantity(1))
    ],
    products = [
        Product("Product_LithiumIonCell", Quantity(5))
    ]
)

# Step 1c: cells with graphite anode (proper Li-ion chemistry, highest yield).
build_recipe(
    recipeId    = "CustomRecipe_LithiumIonCell_Graphite",
    name        = "Lithium-ion cell (graphite anode)",
    description = "Coil lithium against a graphite anode with copper collector — closest match to real Li-ion chemistry.",
    machine     = Ids.Machines.AssemblyRoboticT1,
    duration    = Duration.FromSec(20),
    research    = research_lithium,
    ingredients = [
        Product("Product_Lithium",     Quantity(1)),
        Product(Ids.Products.Copper,   Quantity(1)),
        Product(Ids.Products.Graphite, Quantity(1))
    ],
    products = [
        Product("Product_LithiumIonCell", Quantity(6))
    ]
)

# Step 2: battery pack assembly. Cells wired together with Electronics2 (BMS).
build_recipe(
    recipeId    = "CustomRecipe_LithiumIonBattery_Assembly",
    name        = "Lithium-ion battery assembly",
    description = "Combine lithium-ion cells with a battery-management electronics module into a finished pack.",
    machine     = Ids.Machines.AssemblyRoboticT1,
    duration    = Duration.FromSec(30),
    research    = research_lithium,
    ingredients = [
        Product("Product_LithiumIonCell",  Quantity(4)),
        Product(Ids.Products.Electronics2, Quantity(1))
    ],
    products = [
        Product("Product_LithiumIonBatteryEmpty", Quantity(2))
    ]
)

# T2 variant for AssemblyRoboticT2 (doubled throughput, same recipe shape).
build_recipe(
    recipeId    = "CustomRecipe_LithiumIonBattery_Assembly_T2",
    name        = "Lithium-ion battery assembly",
    description = "Combine lithium-ion cells with a battery-management electronics module into a finished pack.",
    machine     = Ids.Machines.AssemblyRoboticT2,
    duration    = Duration.FromSec(30),
    research    = research_lithium,
    ingredients = [
        Product("Product_LithiumIonCell",  Quantity(8)),
        Product(Ids.Products.Electronics2, Quantity(2))
    ],
    products = [
        Product("Product_LithiumIonBatteryEmpty", Quantity(4))
    ]
)

# ---------------------------------------------------------------------------
# Recycling (DISABLED for now)
# ---------------------------------------------------------------------------
# Shredder is unit-in -> loose-out only (vanilla: Iron -> IronScrap-loose). Both
# Lithium and LithiumIonCell are unit products, so neither fits Shredder's
# output port. Options when we come back to this:
#   - Make Lithium a loose product (parallel to Lead) AND move cell production
#     off the assembler to a host that accepts loose inputs.
#   - Add a custom recycling machine via build_machine with unit-out support.
# Until either lands, no Li-ion shredder recipes are registered.

# ---------------------------------------------------------------------------
# 3) Consumer-electronics alt-recipe (settlement sink, advanced)
# ---------------------------------------------------------------------------

build_recipe(
    recipeId    = "CustomRecipe_ConsumerElectronics_WithLithiumIon",
    name        = "Consumer electronics (with lithium-ion)",
    description = "Premium consumer-electronics recipe that uses lithium-ion batteries — fewer inputs but higher-tier supply chain.",
    machine     = Ids.Machines.AssemblyRoboticT1,
    duration    = Duration.FromSec(30),
    research    = research_lithium,
    ingredients = [
        Product("Product_LithiumIonBatteryCharged", Quantity(1)),
        Product(Ids.Products.Electronics3,          Quantity(1)),
        Product(Ids.Products.Plastic,        Quantity(1))
    ],
    products = [
        Product(Ids.Products.ConsumerElectronics, Quantity(2))
    ]
)
