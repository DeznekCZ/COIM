from Mafi import ColorRgba, Duration, Quantity, Vector2i
from Mafi.Base import Assets, Ids
from CustomAssets import Prefab, add_prefab_box, add_texture_material, add_toolbar_category, add_unlock_recipe, add_unlock_machine, build_product_fluid, build_product_loose, build_product_unit, add_unlock_product, build_recipe, build_research, add_texture, Product, dependencies, edit_recipe

## Simple testing research
dependencies("liquified_wood") # dependency within mod

## wood pelets
researchWoodgass = build_research(
    researchId = "CustomResearch_WoodPyrolisis",
    name = "Wood pyrolisis",
    description = "Convert small amount of wood while presure is active to medium oil",
    position = (88, 3),
    parents = [ Ids.Research.NaphthaReforming ]
)
add_unlock_product(researchWoodgass, "Product_WoodVapor")
#add_unlock_product(researchWoodgass, "Product_WoodCondensate")
add_unlock_product(researchWoodgass, "Product_WoodGasImpure")

woodPyrolisis = build_recipe(
    recipeId = "CustomRecipe_WoodPyrolisis",
    name = "Coal pyrolisis (super steam)",
    machine = Ids.Machines.BoilerCoal,
    ingredients = [
        Product(Ids.Products.Woodchips, 16)
    ],
    products = [
        Product("Product_WoodVapor", 5),
        Product(Ids.Products.Exhaust, 6)
    ],
    duration = Duration.FromSec(10),
    research = researchWoodgass
)

woodCondensation = build_recipe(
    recipeId = "CustomRecipe_WoodCondensation",
    name = "Coal condensation",
    machine = Ids.Machines.BoilerGas,
    ingredients = [
        Product("Product_WoodVapor", 10)
    ],
    products = [
        Product("Product_WoodGasImpure", 5),
        Product(Ids.Products.MediumOil, 6)
    ],
    duration = Duration.FromSec(30),
    research = researchWoodgass
)

woodGasRefiningGas = build_recipe(
    recipeId = "CustomRecipe_WoodGasPurification",
    name = "Wood gas purifying",
    machine = Ids.Machines.BasicDieselDistiller,
    ingredients = [
        Product("Product_WoodGasImpure", 10),
        Product(Ids.Products.Limestone, 1)
    ],
    products = [
        Product(Ids.Products.FuelGas, 12, "Z"),
        Product(Ids.Products.CarbonDioxide, 6, "S")
    ],
    duration = Duration.FromSec(20),
    research = researchWoodgass
)

build_recipe(
    recipeId = "CustomRecipe_WoodGasBurning",
    name = "Wood gas burning",
    machine = Ids.Machines.Flare,
    ingredients = [
        Product("Product_WoodGasImpure", 24)
    ],
    products = [
        Product(Ids.Products.PollutedAir, 18, "VIRTUAL")
    ],
    research = researchWoodgass
)

add_unlock_recipe(researchWoodgass, Ids.Machines.HydroCrackerT1, Ids.Recipes.FuelGasReforming)
add_unlock_recipe(researchWoodgass, Ids.Machines.Flare, Ids.Recipes.FlareFuelGas)
add_unlock_recipe(researchWoodgass, Ids.Machines.Flare, Ids.Recipes.FlareHeavyOil)
add_unlock_recipe(researchWoodgass, Ids.Machines.AirSeparator, Ids.Recipes.AirSeparation)
add_unlock_recipe(researchWoodgass, Ids.Machines.BoilerGas, Ids.Recipes.SteamGenerationFuelGas)

## supersteam
# DODO detect optional reseach and set the height depending on space
#      optional mod is: coal_liquefaction
researchSuperSteam = build_research(
    researchId = "CustomResearch_SuperSteamWoodPyrolisis",
    name = "Wood pyrolisis (super steam)",
    description = "Convert small amount of wood pelets while presure is active to medium oil",
    position = (160, 37),
    parents = [ Ids.Research.SuperPressSteam ]
)

woodSuperSteamPyrolisis = build_recipe(
    recipeId = "CustomRecipe_SuperSteamWoodPyrolisis",
    name = "Wood pyrolisis (super steam)",
    machine = Ids.Machines.BoilerCoal,
    ingredients = [
        Product(Ids.Products.Woodchips, 8),
        Product(Ids.Products.SteamSp, 4)
    ],
    products = [
        Product("Product_WoodVapor", 5),
        Product(Ids.Products.SteamDepleted, 4)
    ],
    duration = Duration.FromSec(10),
    research = researchSuperSteam
)

#add_unlock_machine(researchSuperSteam, machine = Ids.Machines.BoilerCoal)

add_toolbar_category(
    categoryId = "CustomCategory_WoodPyrolisis",
    name = "Wood pyrolisis",
    icon = add_texture("Assets/Products/Icons/WoodPyrolisisBar.png"),
    parent = Ids.ToolbarCategories.Oil,
    entities = [
        Ids.Machines.Shredder,
        Ids.Machines.BoilerCoal,
        Ids.Machines.BoilerGas,
        Ids.Machines.BoilerElectric,
        Ids.Machines.BasicDieselDistiller
    ]
)
