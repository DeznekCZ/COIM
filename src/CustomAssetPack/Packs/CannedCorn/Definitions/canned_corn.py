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
    Product
)

# ---------------------------------------------------------------------------
# Prefabs: empty cans and canned corn share the SAME mesh (Can.obj — an
# 8-segment cylinder authored by hand) and differ only in their albedo
# texture. The .obj loader handles vertex/UV de-duplication and triangulates
# n-gon faces, so the per-channel index lists in the file are honoured as-is.
# ---------------------------------------------------------------------------

CAN_MESH = "Assets/Models/Can.obj"

empty_cans_prefab = add_unit_prefab(
    path   = "Assets/CannedCorn/EmptyCansBox.prefab",
    albedo = "Assets/Products/EmptyCansBox.png",
    mesh   = CAN_MESH
)

canned_corn_prefab = add_unit_prefab(
    path   = "Assets/CannedCorn/CannedCornBox.prefab",
    albedo = "Assets/Products/CannedCornBox.png",
    mesh   = CAN_MESH
)

# ---------------------------------------------------------------------------
# Products
# ---------------------------------------------------------------------------

empty_cans = build_product_unit(
    productId         = "Product_EmptyCansBox",
    name              = "Empty cans",
    description       = "A box of empty open metal cans, ready to be filled and sealed.",
    icon              = Assets.Base.Products.Icons.Iron_svg,
    prefab            = empty_cans_prefab,
    isStorable        = True,
    packingMode       = CountableProductStackingMode.Triangle,
    allowPackingNoise = True
)

canned_corn = build_product_unit(
    productId         = "Product_CannedCorn",
    name              = "Canned corn",
    description       = "Sealed cans of corn — a shelf-stable food product.",
    icon              = Assets.Base.Products.Icons.FoodPack_svg,
    prefab            = canned_corn_prefab,
    isStorable        = True,
    packingMode       = CountableProductStackingMode.Triangle,
    allowPackingNoise = True
)

# ---------------------------------------------------------------------------
# Empty-cans recipes — three input variants, all in the basic Assembler.
# Yields differ by metal: aluminum > steel > iron, mirroring real-world cost.
# Copy any of these and change `machine` to enable in higher assembler tiers.
# ---------------------------------------------------------------------------

build_recipe(
    recipeId    = "CustomRecipe_EmptyCans_FromIron",
    name        = "Empty cans (from iron)",
    description = "Stamp 1 iron sheet into 8 empty cans (boxed).",
    machine     = Ids.Machines.AssemblyManual,
    duration    = Duration.FromSec(20),
    ingredients = [Product(Ids.Products.Iron, Quantity(1))],
    products    = [Product("Product_EmptyCansBox", Quantity(8))]
)

build_recipe(
    recipeId    = "CustomRecipe_EmptyCans_FromSteel",
    name        = "Empty cans (from steel)",
    description = "Stamp 1 steel sheet into 12 empty cans (boxed).",
    machine     = Ids.Machines.AssemblyManual,
    duration    = Duration.FromSec(20),
    ingredients = [Product(Ids.Products.Steel, Quantity(1))],
    products    = [Product("Product_EmptyCansBox", Quantity(12))]
)

build_recipe(
    recipeId    = "CustomRecipe_EmptyCans_FromAluminum",
    name        = "Empty cans (from aluminum)",
    description = "Stamp 1 aluminum sheet into 16 empty cans (boxed).",
    machine     = Ids.Machines.AssemblyManual,
    duration    = Duration.FromSec(20),
    ingredients = [Product(Ids.Products.Aluminum, Quantity(1))],
    products    = [Product("Product_EmptyCansBox", Quantity(16))]
)

# ---------------------------------------------------------------------------
# Sealing recipe — Chemical Plant fills empty cans with corn and seals them.
# Picked because its port configuration is flexible enough to accept the
# (unit + unit → unit) shape; no other vanilla machine has the right ports.
# ---------------------------------------------------------------------------

build_recipe(
    recipeId    = "CustomRecipe_CannedCorn_Sealing",
    name        = "Canned corn",
    description = "Fill empty cans with corn and seal them.",
    machine     = Ids.Machines.ChemicalPlant,
    duration    = Duration.FromSec(30),
    ingredients = [
        Product("Product_EmptyCansBox", Quantity(8)),
        Product(Ids.Products.Corn,      Quantity(8))
    ],
    products = [Product("Product_CannedCorn", Quantity(8))]
)
