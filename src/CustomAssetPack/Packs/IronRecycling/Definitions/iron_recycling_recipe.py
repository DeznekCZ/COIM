from Mafi import Duration, Quantity, Vector2i
from Mafi.Base import Ids
#from Mafi.Core.Research import TechnologyProto
from CustomRecipes import add_unlock_recipe, build_recipe, build_research, Product

## Simple testing recipe
recipe = build_recipe(
    recipeId = "CustomRecipe_IronShredded",
    name = "Iron Shredded",
    description = "Shred the iron plates into scrap for re‑smelting",
    machine = Ids.Machines.Shredder,
    # research definition is optional, it may be later added by
    # add_unlock(researchId, machineId, build_recipe(Recipe_Class))
    # in case is not define in eather case, it will be locked in game
    research = Ids.Research.Compactor,
    # duration = Duration.FromSec(60),
    ingredients = [
        # allowed is any combination, port id is optional,
        # when udefined, input will be accetped in all eligible
        Product(Ids.Products.Iron, Quantity(24))
    ],
    products = [
        Product(Ids.Products.IronScrap, Quantity(20))
    ]
)

#research = build_research(
#    id = "CustomResearch_Iron_Shredded"
#    name = "Cheap Chips"
#    description = "Cheap Chips"
#    position = Vector2i(0, 20)
#    unlocks = [
#        CustomRecipe_IronShredded
#    ]
#)
