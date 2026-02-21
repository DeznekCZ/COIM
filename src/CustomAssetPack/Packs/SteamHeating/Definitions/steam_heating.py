from Mafi import ColorRgba, Duration, Percent, Quantity, Vector2i, Percent
from Mafi.Base import Assets, Ids
from CustomAssets import Prefab, add_prefab_box, add_texture_material, add_unlock_recipe, add_unlock_machine, build_product_fluid, build_product_loose, build_product_unit, add_unlock_product, build_recipe, build_research, add_texture, Product, dependencies, edit_recipe

## Super heating recipes
build_recipe(
    recipeId = "SteamHeater_HiSp",
    name = "Steam drying",
    machine = Ids.Machines.BoilerElectric,
    ingredients = [
        Product(Ids.Products.SteamHi, 2)
    ],
    products = [
        Product(Ids.Products.SteamSp, 2)
    ],
    duration = Duration.FromSec(10),
    research = Ids.Research.SuperPressSteam,
    power = 90
)

build_recipe(
    recipeId = "SteamHeater_HiSp2x",
    name = "Steam drying",
    machine = Ids.Machines.BoilerElectric,
    ingredients = [
        Product(Ids.Products.SteamHi, 4)
    ],
    products = [
        Product(Ids.Products.SteamSp, 4)
    ],
    duration = Duration.FromSec(10),
    research = Ids.Research.SuperPressSteam,
    power = 180
)

# High heating recipes
build_recipe(
    recipeId = "SteamHeater_LoHi",
    name = "Steam drying",
    machine = Ids.Machines.BoilerElectric,
    ingredients = [
        Product(Ids.Products.SteamLo, 2)
    ],
    products = [
        Product(Ids.Products.SteamHi, 2)
    ],
    duration = Duration.FromSec(10),
    research = Ids.Research.BoilerElectric,
    power = 60
)

build_recipe(
    recipeId = "SteamHeater_LoHi2x",
    name = "Steam drying",
    machine = Ids.Machines.BoilerElectric,
    ingredients = [
        Product(Ids.Products.SteamLo, 4)
    ],
    products = [
        Product(Ids.Products.SteamHi, 4)
    ],
    duration = Duration.FromSec(10),
    research = Ids.Research.BoilerElectric,
    power = 120
)

## Low heating recipes
build_recipe(
    recipeId = "SteamHeater_DeLo",
    name = "Steam drying",
    machine = Ids.Machines.BoilerElectric,
    ingredients = [
        Product(Ids.Products.SteamDepleted, 2)
    ],
    products = [
        Product(Ids.Products.SteamLo, 2)
    ],
    duration = Duration.FromSec(10),
    research = Ids.Research.BoilerElectric,
    power = 35
)

build_recipe(
    recipeId = "SteamHeater_DeLo2x",
    name = "Steam drying",
    machine = Ids.Machines.BoilerElectric,
    ingredients = [
        Product(Ids.Products.SteamDepleted, 4)
    ],
    products = [
        Product(Ids.Products.SteamLo, 4)
    ],
    duration = Duration.FromSec(10),
    research = Ids.Research.BoilerElectric,
    power = 70
)

## Low heating recipes
build_recipe(
    recipeId = "SteamHeater_DeHi",
    name = "Steam drying",
    machine = Ids.Machines.BoilerElectric,
    ingredients = [
        Product(Ids.Products.SteamDepleted, 2)
    ],
    products = [
        Product(Ids.Products.SteamHi, 2)
    ],
    duration = Duration.FromSec(10),
    research = Ids.Research.BoilerElectric,
    power = 90
)

build_recipe(
    recipeId = "SteamHeater_DeLoHi",
    name = "Steam drying",
    machine = Ids.Machines.BoilerElectric,
    ingredients = [
        Product(Ids.Products.SteamDepleted, 4)
    ],
    products = [
        Product(Ids.Products.SteamHi, 4)
    ],
    duration = Duration.FromSec(10),
    research = Ids.Research.BoilerElectric,
    power = 180
)
