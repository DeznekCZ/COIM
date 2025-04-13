
## ACT AS BUILD IN FUNCTIONS
## when is not included by block, automatically import them all

from Mafi import Duration, Quantity
from Mafi.Core.Factory.Recipes import RecipeProto
from Mafi.Core.Factory.Machines import MachineProto
from Mafi.Core.Research import ResearchNodeProto
from Mafi.Core.Products import ProductProto

class Product:
    def __init__(
        self,
        product: ProductProto | ProductProto.ID | str,
        quantity: Quantity | int,
        port: str = "*"
    ):
        """
        Parameters:
        product: required
        quantity: required
        port: optional, default '*'
        """
        self.product = product
        self.quantity = quantity
        self.port = port or "*"

def recipe_id(recipeId: str | RecipeProto) -> RecipeProto.ID:
    """ create recipe id from text value or from RecipeProto """
    return RecipeProto.ID(recipeId);

def build_recipe(
        recipeId: RecipeProto.ID | str,
        name: str,
        description: str,
        machine = MachineProto.ID | MachineProto | str,
        research: ResearchNodeProto | ResearchNodeProto.ID | str | None = None,
        duration: Duration | None = Duration(60),
        ingredients: list[Product] | None = [],
        products: list[Product] | None = []
    ) -> RecipeProto:
    """
    Parameters:
        recipeId: required - recipe unique identifier
        name: required - display name
        description: optional - description
        machine: required - machine to add the recipe
        research: optional - default research (usually already existing)
            research definition is optional, it may be later added by
            add_unlock(researchId, machineId, build_recipe(Recipe_Class))
            in case is not define in eather case, it will be locked in game
        duration: default - 60 seconds
            may be redefined by Duration.FromSec(int) or by Duration.FromMin(int)
        ingredients: list - none, empty or at least one Product in case products are empty
        products: list - none, empty or at least one Product in case ingredients are empty
    """
    pass

def build_research(undefined) -> ResearchNodeProto:
    """ NotImplementedException """
    pass

def add_unlock_recipe(
        research: ResearchNodeProto | ResearchNodeProto.ID | str,
        machine: MachineProto | MachineProto.ID | str,
        proto: RecipeProto | RecipeProto.ID | str
    ):
    """ Adds recipe to existing research """
    pass