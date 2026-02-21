from Mafi import ColorRgba, Duration, Quantity, Vector2i
from Mafi.Base import Assets, Ids
from CustomAssets import Prefab, add_prefab_box, add_texture_material, add_unlock_recipe, add_unlock_machine, build_product_fluid, build_product_loose, build_product_unit, add_unlock_product, build_recipe, build_research, add_texture, Product, dependencies, edit_recipe

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
        Product(Ids.Products.Water, 20),
        Product(Ids.Products.Coal, 10)
    ],
    products = [
        Product("Product_CoalGasImpure", 40),
        Product(Ids.Products.Ammonia, 1),
        Product(Ids.Products.Coal, 8)
    ],
    duration = Duration.FromSec(40),
    research = research
)

coalGasRefiningGas = build_recipe(
    recipeId = "CustomRecipe_CoalLiquification",
    name = "Coal gas purifying",
    machine = Ids.Machines.BasicDieselDistiller,
    ingredients = [
        Product("Product_CoalGasImpure", 20),
        Product(Ids.Products.IronOre, 1)
    ],
    products = [
        Product(Ids.Products.FuelGas, 24, "Z"),
        Product(Ids.Products.Water, 6, "X")
    ],
    duration = Duration.FromSec(20),
    research = research
)

add_unlock_recipe(research, Ids.Machines.HydroCrackerT1, Ids.Recipes.FuelGasReforming)
add_unlock_recipe(research, Ids.Machines.Flare, Ids.Recipes.FlareFuelGas)
add_unlock_recipe(research, Ids.Machines.Flare, Ids.Recipes.FlareHeavyOil)
add_unlock_recipe(research, Ids.Machines.Flare, Ids.Recipes.FlareAmmonia)
add_unlock_recipe(research, Ids.Machines.AirSeparator, Ids.Recipes.AirSeparation)
add_unlock_recipe(research, Ids.Machines.BoilerGas, Ids.Recipes.SteamGenerationFuelGas)

## wood pelets
# researchWoodgass = build_research(
#     researchId = "CustomResearch_WoodPyrolisis",
#     name = "Wood liquification",
#     description = "Convert small amount of wood while presure is active to medium oil",
#     position = (88, 3),
#     parents = [ Ids.Research.NaphthaReforming ]
# )
# add_unlock_product(researchWoodgass, "Product_WoodVapor")
# #add_unlock_product(researchWoodgass, "Product_WoodCondensate")
# add_unlock_product(researchWoodgass, "Product_WoodGasImpure")
# 
# woodPyrolisis = build_recipe(
#     recipeId = "CustomRecipe_WoodPyrolisis",
#     name = "Coal pyrolisis (super steam)",
#     machine = Ids.Machines.BoilerCoal,
#     ingredients = [
#         Product(Ids.Products.Woodchips, 16)
#     ],
#     products = [
#         Product("Product_WoodVapor", 5),
#         Product(Ids.Products.Exhaust, 6)
#     ],
#     duration = Duration.FromSec(10),
#     research = researchWoodgass
# )
# 
# woodCondensation = build_recipe(
#     recipeId = "CustomRecipe_WoodCondensation",
#     name = "Coal condensation",
#     machine = Ids.Machines.BoilerGas,
#     ingredients = [
#         Product("Product_WoodVapor", 10)
#     ],
#     products = [
#         Product("Product_WoodGasImpure", 5),
#         Product(Ids.Products.MediumOil, 6)
#     ],
#     duration = Duration.FromSec(30),
#     research = researchWoodgass
# )
# 
# woodGasRefiningGas = build_recipe(
#     recipeId = "CustomRecipe_WoodGasPurification",
#     name = "Wood gas purifying",
#     machine = Ids.Machines.BasicDieselDistiller,
#     ingredients = [
#         Product("Product_WoodGasImpure", 10),
#         Product(Ids.Products.Limestone, 1)
#     ],
#     products = [
#         Product(Ids.Products.FuelGas, 12, "Z"),
#         Product(Ids.Products.CarbonDioxide, 6, "S")
#     ],
#     duration = Duration.FromSec(20),
#     research = researchWoodgass
# )
#add_unlock_machine(researchWoodgass, machine = Ids.Machines.BoilerCoal)
#add_unlock_machine(researchWoodgass, machine = Ids.Machines.BoilerGas)
#add_unlock_machine(researchWoodgass, machine = Ids.Machines.BasicDieselDistiller)

## supersteam
researchSuperSteam = build_research(
    researchId = "CustomResearch_SuperSteamCoalPyrolisis",
    name = "Coal liquification (super steam)",
    description = "Convert small amount of coal while presure is active to heavy oil",
    position = (160, 33),
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

# woodSuperSteamPyrolisis = build_recipe(
#     recipeId = "CustomRecipe_SuperSteamWoodPyrolisis",
#     name = "Wood pyrolisis (super steam)",
#     machine = Ids.Machines.BoilerCoal,
#     ingredients = [
#         Product(Ids.Products.Woodchips, 8),
#         Product(Ids.Products.SteamSp, 4)
#     ],
#     products = [
#         Product("Product_WoodVapor", 5),
#         Product(Ids.Products.SteamDepleted, 4)
#     ],
#     duration = Duration.FromSec(10),
#     research = researchSuperSteam
# )

#add_unlock_machine(researchSuperSteam, machine = Ids.Machines.BoilerCoal)
