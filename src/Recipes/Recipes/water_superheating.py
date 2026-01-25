from Mafi import ColorRgba, Duration, Percent, Quantity, Vector2i
from Mafi.Base import Assets, Ids
from CustomAssets import Prefab, add_prefab_box, add_texture_material, add_unlock_recipe, add_unlock_machine, build_product_fluid, build_product_loose, build_product_unit, add_unlock_product, build_recipe, build_research, add_texture, Product, dependencies, edit_recipe

build_recipe(
    recipeId = "SteamHeater_HiSp",
    name = "Super heating high powered steam",
    machine = Ids.Machines.BoilerElectric,
    ingredients = [
        Product(Ids.Products.SteamHi, 2)
    ],
    products = [
        Product(Ids.Products.SteamSp, 2)
    ],
    duration = Duration.FromSec(10),
    research = Ids.Research.SuperPressSteam
)

build_recipe(
    recipeId = "SteamHeater_DeHi",
    name = "Heating depleted powered steam",
    machine = Ids.Machines.BoilerElectric,
    ingredients = [
        Product(Ids.Products.SteamDepleted, 2)
    ],
    products = [
        Product(Ids.Products.SteamHi, 2)
    ],
    duration = Duration.FromSec(10),
    research = Ids.Research.SuperPressSteam,
    power = Percent(75)
)

build_recipe(
    recipeId = "SteamHeater_LoHi",
    name = "Heating low powered steam",
    machine = Ids.Machines.BoilerElectric,
    ingredients = [
        Product(Ids.Products.SteamLo, 2)
    ],
    products = [
        Product(Ids.Products.SteamHi, 2)
    ],
    duration = Duration.FromSec(10),
    research = Ids.Research.SuperPressSteam,
    power = Percent(50)
)
