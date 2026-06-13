# Example pack — canned corn.
#
# Showcases:
#   - add_unit_prefab() for unit-product prefabs (same mesh, different texture).
#   - build_product_unit() for registering unit (countable) products.
#   - build_recipe() with multiple input variants (3 metals → empty cans).
#   - Sealing recipe in the Chemical Plant.

from Mafi import Duration, Quantity
from Mafi.Base import Assets, Ids
from Mafi.Core.Products import CountableProductStackingMode
from CustomAssets import (
    add_unit_prefab,
    build_product_unit,
    build_recipe,
    Product,
    build_product_loose,
    build_nuclear_reactor,
    FuelPair
)

# ---------------------------------------------------------------------------
# Prefabs: empty cans and canned corn share the SAME mesh (Can.obj — an
# 8-segment cylinder authored by hand) and differ only in their albedo
# texture. The .obj loader handles vertex/UV de-duplication and triangulates
# n-gon faces, so the per-channel index lists in the file are honoured as-is.
# ---------------------------------------------------------------------------

CAN_MESH = "Assets/Models/Can.obj"

candu_rod = add_unit_prefab(
    path   = "Assets/CannedCorn/EmptyCansBox.prefab",
    albedo = "Assets/Products/EmptyCansBox.png",
    mesh = "Assets/Models/Candu_Rod.obj"
)

empty_cans_prefab = add_unit_prefab(
    path   = "Assets/CannedCorn/EmptyCansBox.prefab",
    albedo = "Assets/Products/EmptyCansBox.png",
    mesh = "Assets/Models/Candu_Rod.obj"
)

canned_corn_prefab = add_unit_prefab(
    path   = "Assets/CannedCorn/CannedCornBox.prefab",
    albedo = "Assets/Products/CannedCornBox.png",
    mesh = "Assets/Models/Candu_Rod.obj"
)

empty_cans = build_product_unit(
    productId = "Product_EmptyCansBox",
    name      = "Empty cans",
    icon      = Assets.Base.Products.Icons.Iron_svg,
    prefab    = empty_cans_prefab,
    description = "A box of empty open metal cans, ready to be filled and sealed.",
    isStorable = True,
    allowPackingNoise = True
)

canned_corn = build_product_unit(
    productId = "Product_CannedCorn",
    name      = "Canned corn",
    icon      = Assets.Base.Products.Icons.FoodPack_svg,
    prefab    = canned_corn_prefab,
    description = "Sealed cans of corn — a shelf-stable food product.",
    isStorable = True,
    allowPackingNoise = True
)

build_recipe(
    "CustomRecipe_EmptyCans_FromIron",
    "Empty cans (from iron)",
    "Stamp 1 iron sheet into 8 empty cans (boxed).",
    Ids.Machines.AssemblyManual,
    duration = Duration.FromSec(20),
    ingredients = [
        Product(Ids.Products.Iron, Quantity(1))
    ],
    products = [
        Product("Product_EmptyCansBox", Quantity(8))
    ]
)

build_recipe(
    "CustomRecipe_EmptyCans_FromSteel",
    "Empty cans (from steel)",
    "Stamp 1 steel sheet into 12 empty cans (boxed).",
    Ids.Machines.AssemblyManual,
    duration = Duration.FromSec(20),
    ingredients = [
        Product(Ids.Products.Steel, Quantity(1))
    ],
    products = [
        Product("Product_EmptyCansBox", Quantity(12))
    ]
)

build_recipe(
    "CustomRecipe_EmptyCans_FromAluminum",
    "Empty cans (from aluminum)",
    "Stamp 1 aluminum sheet into 16 empty cans (boxed).",
    Ids.Machines.AssemblyManual,
    duration = Duration.FromSec(20),
    ingredients = [
        Product(Ids.Products.Aluminum, Quantity(1))
    ],
    products = [
        Product("Product_EmptyCansBox", Quantity(16))
    ]
)

build_recipe(
    "CustomRecipe_CannedCorn_Sealing",
    "Canned corn",
    "Fill empty cans with corn and seal them.",
    Ids.Machines.ChemicalPlant,
    duration = Duration.FromSec(30),
    ingredients = [
        Product("Product_EmptyCansBox", Quantity(8)),
        Product(Ids.Products.Corn, Quantity(8))
    ],
    products = [
        Product("Product_CannedCorn", Quantity(8))
    ]
)

build_nuclear_reactor(
    reactorId              = "NewNuclearReactor_1",
    source                 = "NuclearReactor",
    name = "Any burnable burner",
    description = "Jaderný reaktor, který udržuje jadernou řetězovou reakci z tyčí obohaceného uranu. Reakce uvolňuje velké množství energie využité pro výrobu páry. Toto zařízení lze nastavit tak, aby efektivně poskytovalo až 90 MW elektřiny při plném výkonu. Pozor, vyhořelé palivo je radioaktivní a pokud není skladováno ve specializovaném zařízení, může ublížit populaci.",
    maxPowerLevel = 3,
    fuelCapacity = 40,
    minFuelToOperate = 16,
    processDurationSeconds = 10,
    computingConsumed = 0,
    fuel_pairs             = [
        FuelPair(fuelIn="Product_Coal", spentFuelOut="Product_Exhaust", durationSeconds=2),
        FuelPair(fuelIn="Product_Biomass", spentFuelOut="Product_Exhaust", durationSeconds=3),
        FuelPair(fuelIn="Product_Woodchips", spentFuelOut="Product_Exhaust", durationSeconds=2)
    ]
)

