from Mafi import ColorRgba, Duration, Quantity, Vector2i
from Mafi.Base import Assets, Ids
from CustomRecipes import Prefab, add_prefab_box, add_texture_material, add_unlock_recipe, add_unlock_machine, build_product_fluid, build_product_loose, build_product_unit, add_unlock_product, build_recipe, build_research, add_texture, Product, edit_recipe

## Simple testing research
research = build_research(
    researchId = "CustomResearch_CoalLiquification",
    name = "Coal liquification",
    description = "Convert small amount of coal while presure is active to heavy oil",
    position = (20, 15),
    parents = [ Ids.Research.Cp2Packing ]
)

coalVapor = build_product_fluid(
    productId = "Product_CoalVapor",
    name = "Coal vapor",
    icon = add_texture("Assets/Products/Icons/CoalVapor.png"),
    description = "Is created by heating of coal",
    color = ColorRgba.DarkDarkGray,
    transportColor = ColorRgba.DarkGray,
    transportAccentColor = ColorRgba.CornflowerBlue,
    isStorable = False
)
coalVaporization = build_recipe(
    recipeId = "CoalVaporization",
    name = "Coal vaporization",
    machine = Ids.Machines.BoilerCoal,
    ingredients = [
        Product(Ids.Products.Water, 2),
        Product(Ids.Products.Coal, 10)
    ],
    products = [
        Product(coalVapor, 10),
        Product(Ids.Products.Exhaust, 10)
    ],
    duration = Duration.FromSec(20),
    research = research
)
add_unlock_product(research, coalVapor)

## Simple testing recipe
recipe = build_recipe(
    recipeId = "CustomRecipe_CoalLiquification",
    name = "Coal liquification",
    description = "Convert small amount of coal while presure is active to heavy oil",
    machine = Ids.Machines.BasicDieselDistiller,
    # research definition is optional, it may be later added by
    # add_unlock(researchId, machineId, build_recipe(Recipe_Class))
    # in case is not define in eather case, it will be locked in game
    research = research,
    duration = Duration.FromSec(20),
    ingredients = [
        # allowed is any combination, port id is optional,
        # when udefined, input will be accetped in all eligible
        # Product(Ids.Products.Coal, 5),
        Product(coalVapor, 2)
    ],
    products = [
        Product(Ids.Products.Ammonia, 1),
        Product(Ids.Products.HeavyOil, 5),
        Product(Ids.Products.FuelGas, 20)
    ]
)

add_unlock_recipe(research, Ids.Machines.HydroCrackerT1, Ids.Recipes.FuelGasReforming)
add_unlock_recipe(research, Ids.Machines.Flare, Ids.Recipes.FlareFuelGas)
add_unlock_recipe(research, Ids.Machines.Flare, Ids.Recipes.FlareHeavyOil)
add_unlock_recipe(research, Ids.Machines.Flare, Ids.Recipes.FlareAmmonia)
add_unlock_recipe(research, Ids.Machines.AirSeparator, Ids.Recipes.AirSeparation)
add_unlock_recipe(research, Ids.Machines.BoilerGas, Ids.Recipes.SteamGenerationFuelGas)
add_unlock_machine(research, Ids.Machines.Flare)
add_unlock_machine(research, Ids.Machines.AirSeparator)
add_unlock_machine(research, Ids.Machines.HydroCrackerT1)
add_unlock_machine(research, Ids.Machines.BoilerGas)
