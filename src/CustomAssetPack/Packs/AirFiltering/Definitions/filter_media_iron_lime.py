from Mafi import Duration, Quantity
from Mafi.Base import Assets, Ids
from CustomAssets import add_loose_product_material, add_texture, build_product_loose, build_recipe, Product

# Pile material for our new loose product. Only the albedo is overridden; normals and the
# smooth/metal channel are inherited from the default reference material so the surface
# still reads as rocky/granular like the vanilla pile.
filter_media_mat = add_loose_product_material(
    path   = "Assets/AirFiltering/AirFilterIL_mat",
    albedo = "Assets/Products/FilterMediaIronLime.png",
    tiling = 8
)

# Icon for the new product. Reuse the vanilla filter media icon since it's close enough.
filter_media_icon = add_texture(
    path="Assets/Products/FilterMediaIronLimeIcon.png"
)

# New loose product: iron-limestone filter media. Composition differs from vanilla
# Product_FilterMedia (iron ore + limestone instead of vanilla inputs), so it has its own id.
#
# particleColor is set explicitly because LooseProductMaterialManager auto-derives it from the
# average of the pile albedo texture during ITS ctor — but our material isn't in AssetsDb at
# that point (CustomAssetManager runs after LPMM and patches the texture array slice
# retroactively), so the auto-derive samples the fallback material instead. Pin it to a value
# that matches the iron-limestone albedo to get the right conveyor-spill particle color.
filter_media_iron_lime = build_product_loose(
    productId     = "Product_FilterMediaIronLime",
    name          = "Filter media (iron + limestone)",
    description   = "Filter media produced from iron ore and limestone. Cheaper alternative to vanilla filter media when iron ore is plentiful.",
    icon          = filter_media_icon,
    material      = filter_media_mat,
    particleColor = (170, 165, 155),
    isStorable    = True,
    isRecyclable  = False,
    isRough       = True,
    isDumped      = False,
    dumpsAs       = Ids.TerrainMaterials.Gravel
)

# Producer recipe: 3 iron ore + 1 limestone -> 4 iron-limestone filter media.
# Lossless mixing in the Industrial Mixer; auto-available with the machine.
build_recipe(
    recipeId    = "CustomRecipe_AirFilterIL_Mixing",
    name        = "Filter media (iron + limestone) mixing",
    description = "Mix 1 iron ore + 3 limestone into 4 filter media. More efficient than vanilla filter media production when iron ore is plentiful.",
    machine     = Ids.Machines.IndustrialMixer,
    ingredients = [
        Product(Ids.Products.IronOre,    Quantity(1)),
        Product(Ids.Products.Limestone,  Quantity(3))
    ],
    products = [
        Product("Product_FilterMediaIronLime", Quantity(4))
    ],
    duration = Duration.FromSec(30)
)

# Producer recipe: 3 iron ore + 1 limestone -> 4 iron-limestone filter media.
# Lossless mixing in the Industrial Mixer; auto-available with the machine.
build_recipe(
    recipeId    = "CustomRecipe_AirFilterIL_Mixing_T2",
    name        = "Filter media (iron + limestone) mixing",
    description = "Mix 2 iron ore + 6 limestone into 4 filter media. More efficient than vanilla filter media production when iron ore is plentiful.",
    machine     = Ids.Machines.IndustrialMixerT2,
    ingredients = [
        Product(Ids.Products.IronOre,    Quantity(2)),
        Product(Ids.Products.Limestone,  Quantity(6))
    ],
    products = [
        Product("Product_FilterMediaIronLime", Quantity(8))
    ],
    duration = Duration.FromSec(30)
)

# Consumer recipe: add next recipe to air crubber research
build_recipe(
    recipeId    = "CustomRecipe_AirFilterIL_Scubbing",
    name        = "Air scrubber filter media (iron + limestone) consumption",
    description = "Use 4 iron-limestone filter media to scrub air. More efficient than vanilla filter media consumption when iron ore is plentiful.",
    machine     = Ids.Machines.ExhaustScrubber,
    research    = Ids.Research.ExhaustFiltration,
    ingredients = [
        Product(Ids.Products.Exhaust, Quantity(160)),
        Product(Ids.Products.Water,   Quantity(16)),
        Product("Product_FilterMediaIronLime", Quantity(2))
    ],
    products = [
        Product(Ids.Products.Sulfur, Quantity(4)),
        Product(Ids.Products.CarbonDioxide, Quantity(64)),
        Product(Ids.Products.SteamLo, Quantity(16)),
        Product(Ids.Products.Slag, Quantity(2))
    ],
    duration = Duration.FromSec(20)
);
