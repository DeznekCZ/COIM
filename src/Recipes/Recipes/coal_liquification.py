from Mafi import Duration, Quantity, Vector2i
from Mafi.Base import Assets, Ids
#from Mafi.Core.Research import TechnologyProto
from CustomRecipes import add_unlock_recipe, add_unlock_machine, build_recipe, build_research, add_texture, Product

add_texture("Assets/Container.png", Assets.Base.Products.Icons.Coal_svg)

## Simple testing research
research = build_research(
    researchId = "CustomResearch_CoalLiquification",
    name = "Coal liquification",
    description = "Convert small amount of coal while presure is active to heavy oil",
    position = (20, 15),
    parents = [ Ids.Research.Cp2Packing ]
)

# TODO pøidat kyslík a dusík do komína
# TODO 

add_unlock_recipe(research, Ids.Machines.HydroCrackerT1, Ids.Recipes.FuelGasReforming)
add_unlock_recipe(research, Ids.Machines.Flare, Ids.Recipes.FlareFuelGas)
add_unlock_recipe(research, Ids.Machines.Flare, Ids.Recipes.FlareHeavyOil)
add_unlock_recipe(research, Ids.Machines.AirSeparator, Ids.Recipes.AirSeparation)
add_unlock_recipe(research, Ids.Machines.BoilerGas, Ids.Recipes.SteamGenerationFuelGas)
add_unlock_machine(research, Ids.Machines.Flare)
add_unlock_machine(research, Ids.Machines.AirSeparator)
add_unlock_machine(research, Ids.Machines.HydroCrackerT1)
add_unlock_machine(research, Ids.Machines.BoilerGas)

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
    # duration = Duration.FromSec(60),
    ingredients = [
        # allowed is any combination, port id is optional,
        # when udefined, input will be accetped in all eligible
        Product(Ids.Products.Coal, 5),
        Product(Ids.Products.Water, 1)
    ],
    products = [
        Product(Ids.Products.Exhaust, 10),
        Product(Ids.Products.HeavyOil, 5),
        Product(Ids.Products.FuelGas, 5)
    ]
)

## Simple testing recipe
#recipe = build_recipe(
#    recipeId = "CustomRecipe_AmmoniaExctraction",
#    name = "Ammonia Extraction",
#    description = "Convert small amount of coal while presure is active to heavy oil",
#    machine = Ids.Machines.BasicDieselDistiller,
#    # research definition is optional, it may be later added by
#    # add_unlock(researchId, machineId, build_recipe(Recipe_Class))
#    # in case is not define in eather case, it will be locked in game
#    research = research,
#    duration = Duration.FromSec(20),
#    ingredients = [
#        # allowed is any combination, port id is optional,
#        # when udefined, input will be accetped in all eligible
#        Product(Ids.Products.Coal, 4),
#        Product(Ids.Products.Water, 1)
#    ],
#    products = [
#        Product(Ids.Products.Exhaust, 10),
#        Product(Ids.Products.HeavyOil, 8),
#        Product(Ids.Products.Ammonia, 5)
#    ]
#)
