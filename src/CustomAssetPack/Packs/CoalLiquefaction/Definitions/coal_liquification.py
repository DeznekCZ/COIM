from Mafi import ColorRgba, Duration, Quantity, Vector2i
from Mafi.Base import Assets, Ids
from CustomAssets import Prefab, add_prefab_box, add_texture_material, add_unlock_recipe, add_toolbar_category, add_unlock_machine, build_product_fluid, build_product_loose, build_product_unit, add_unlock_product, build_recipe, build_research, add_texture, Product, dependencies, edit_recipe

## Simple testing research
research = build_research(
    researchId = "CustomResearch_CoalLiquification",
    name = "Coal liquification",
    description = "Convert small amount of coal while presure is active to heavy oil",
    position = (24, 11),
    parents = [ Ids.Research.Cp2Packing ]
)

dependencies("liquified_coal") # dependency within mod
add_unlock_product(research, "Product_CoalVapor")
add_unlock_product(research, "Product_CoalCondensate")
add_unlock_product(research, "Product_CoalGasImpure")

coalPyrolisis = build_recipe(
    recipeId = "CoalVaporization",
    name = "Coal pyrolisis",
    machine = Ids.Machines.BoilerCoal,
    ingredients = [
        Product(Ids.Products.Coal, 10)
    ],
    products = [
        Product("Product_CoalVapor", 5),
        Product(Ids.Products.Exhaust, 10)
    ],
    duration = Duration.FromSec(10),
    research = research
)

coalCondensation = build_recipe(
    recipeId = "CustomRecipe_CoalCondensation",
    name = "Coal condensation",
    machine = Ids.Machines.BoilerGas,
    ingredients = [
        Product("Product_CoalVapor", 10)
    ],
    products = [
        Product("Product_CoalCondensate", 5),
        Product(Ids.Products.HeavyOil, 6)
    ],
    duration = Duration.FromSec(30),
    research = research
)

coalCoking = build_recipe(
    recipeId = "CustomRecipe_CoalCoking",
    name = "Coal coking",
    machine = Ids.Machines.ExhaustScrubber,
    ingredients = [
        Product("Product_CoalCondensate", 20),
        Product(Ids.Products.Water, 10),
        Product(Ids.Products.Coal, 2)
    ],
    products = [
        Product("Product_CoalGasImpure", 40),
        Product(Ids.Products.SourWater, 12)
    ],
    duration = Duration.FromSec(40),
    research = research
)

# TODO remove iron and replace with custom mix of filtering material
# mixed from crushed iron and limestone,
coalGasRefiningGas = build_recipe(
    recipeId = "CustomRecipe_CoalLiquification",
    name = "Coal gas purifying",
    machine = Ids.Machines.BasicDieselDistiller,
    ingredients = [
        Product("Product_CoalGasImpure", 20),
        Product(Ids.Products.IronOre, 1)
    ],
    products = [
        Product(Ids.Products.FuelGas, 24, "Z")
    ],
    duration = Duration.FromSec(20),
    research = research
)

if product_exist("Product_FilterMediaIronLime"):
    coalGasRefiningGas = build_recipe(
        recipeId = "CustomRecipe_CoalLiquification_IronLime",
        name = "Coal gas purifying",
        machine = Ids.Machines.BasicDieselDistiller,
        ingredients = [
            Product("Product_CoalGasImpure", 20),
            Product("Product_FilterMediaIronLime", 1)
        ],
        products = [
            Product(Ids.Products.FuelGas, 24, "Z")
        ],
        duration = Duration.FromSec(20),
        research = research
    )

build_recipe(
    recipeId = "CustomRecipe_CoalGasBurning",
    name = "Coal gas burning",
    machine = Ids.Machines.Flare,
    ingredients = [
        Product("Product_CoalGasImpure", 24)
    ],
    products = [
        Product(Ids.Products.PollutedAir, 24, "VIRTUAL")
    ],
    research = research
)

add_unlock_recipe(research, Ids.Machines.HydroCrackerT1, Ids.Recipes.FuelGasReforming)
add_unlock_recipe(research, Ids.Machines.Flare, Ids.Recipes.FlareFuelGas)
add_unlock_recipe(research, Ids.Machines.Flare, Ids.Recipes.FlareHeavyOil)
add_unlock_recipe(research, Ids.Machines.AirSeparator, Ids.Recipes.AirSeparation)
add_unlock_recipe(research, Ids.Machines.BoilerGas, Ids.Recipes.SteamGenerationFuelGas)
add_unlock_recipe(research, Ids.Machines.WasteDump, Ids.Recipes.SourWaterDumping)

## supersteam
researchSuperSteam = build_research(
    researchId = "CustomResearch_SuperSteamCoalPyrolisis",
    name = "Coal liquification (super steam)",
    description = "Convert small amount of coal while presure is active to heavy oil",
    position = (160, 36),
    parents = [ Ids.Research.SuperPressSteam ]
)

coalSuperSteamPyrolisis = build_recipe(
    recipeId = "CustomRecipe_SuperSteamCoalPyrolisis",
    name = "Coal pyrolisis (super steam)",
    machine = Ids.Machines.BoilerCoal,
    ingredients = [
        Product(Ids.Products.Coal, 5),
        Product(Ids.Products.SteamSp, 8)
    ],
    products = [
        Product("Product_CoalVapor", 5),
        Product(Ids.Products.SteamDepleted, 8)
    ],
    duration = Duration.FromSec(10),
    research = researchSuperSteam
)

add_toolbar_category(
    categoryId = "CustomCategory_CoalLiquification",
    name = "Coal liquification",
    icon = add_texture("Assets/Products/Icons/CoalLiquificationBar.png"),
    parent = Ids.ToolbarCategories.Oil,
    entities = [
        Ids.Machines.Shredder,
        Ids.Machines.BoilerCoal,
        Ids.Machines.BoilerGas,
        Ids.Machines.BoilerElectric,
        Ids.Machines.ExhaustScrubber,
        Ids.Machines.BasicDieselDistiller
    ]
)
