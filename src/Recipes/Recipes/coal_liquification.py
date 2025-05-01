from Mafi import Duration, Quantity, Vector2i
from Mafi.Base import Assets, Ids
#from Mafi.Core.Research import TechnologyProto
from CustomRecipes import Prefab, add_prefab_box, add_texture_material, add_unlock_recipe, add_unlock_machine, build_product_loose, build_product_unit, add_unlock_product, build_recipe, build_research, add_texture, Product

#add_texture("Assets/Container.png", Assets.Base.Products.Icons.Coal_svg)
texture = add_texture("Assets/Container.png")

build_product_loose(
    productId = "Product_CoalCoke",
    name = "Coke",
    icon = "Assets/Container.png",
    material = add_texture_material(path = "Assets/Container.mat", texture = texture),
    isStorable = True,
    isLocked = True,
    isRough = True,
    color = (0, 0, 0)
)

build_product_unit(
    productId = "Product_CoalBlock",
    name = "Coal block",
    icon = "Assets/Container.png",
    prefab = add_prefab_box("Assets/Container_Coal.prefab", "Assets/Container.png"),
    isStorable = True,
    isLocked = True
)

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
add_unlock_product(research, "Product_CoalCoke")
add_unlock_product(research, "Product_CoalBlock")

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
