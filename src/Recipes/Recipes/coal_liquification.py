from Mafi import Duration, Quantity, Vector2i
from Mafi.Base import Ids
#from Mafi.Core.Research import TechnologyProto
from CustomRecipes import add_unlock_recipe, build_recipe, build_research, Product

## Simple testing recipe
recipe = build_recipe(
    recipeId = "CustomRecipe_CoalLiquification",
    name = "Coal liquification",
    description = "Convert small amount of coal while presure is active to heavy oil",
    machine = Ids.Machines.ChemicalPlant,
    # research definition is optional, it may be later added by
    # add_unlock(researchId, machineId, build_recipe(Recipe_Class))
    # in case is not define in eather case, it will be locked in game
    research = Ids.Research.CrudeOilDistillation,
    # duration = Duration.FromSec(60),
    ingredients = [
        # allowed is any combination, port id is optional,
        # when udefined, input will be accetped in all eligible
        Product('Product_Coal', Quantity(5)),
        Product(Ids.Products.HeavyOil, 10),
        Product(Ids.Products.SteamHi, 1)
    ],
    products = [
        Product(Ids.Products.Exhaust, 5), # port id is not required, it is genrated automatically
        Product(Ids.Products.HeavyOil, 15, "Z")
    ]
)

#research = build_research(
#    id = "CustomResearch_CoalLiquification"
#    name = "Coal liquification"
#    description = "Convert small amount of coal while presure is active to heavy oil"
#    position = Vector2i(0, 20)
#    unlocks = [
#        CustomRecipe_CoalLiquification
#    ]
#)
