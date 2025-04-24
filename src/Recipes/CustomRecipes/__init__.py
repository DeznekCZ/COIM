
## ACT AS BUILD IN FUNCTIONS
## when is not included by block, automatically import them all

from Mafi import Duration, Quantity, Vector2i
from Mafi.Core.Factory.Recipes import RecipeProto
from Mafi.Core.Factory.Machines import MachineProto
from Mafi.Core.Research import ResearchCostsTpl, ResearchNodeProto
from Mafi.Core.Products import ProductProto
from Mafi.Core.Entities.Static import StaticEntityProto
from Mafi.Core.Entities.Dynamic import DynamicEntityProto

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

def add_texture(path: str, replace: str = None) -> None:
    """
    Parameters:
        icon: path within mod, 
        replace: path of icon to be replaced with the modification,
            when used, the path within assets will be linked by replace value
    """
    pass

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

def build_research(
        researchId: ResearchNodeProto.ID | str,
        name: str,
        description: str,
        costs: ResearchCostsTpl | int = 1,
        position: Vector2i | (int, int) = (0,0), # type: ignore
        icon: str = None
    ) -> ResearchNodeProto:
    """
    At least one of products, vehicles, buildings, recipes must be defined

    Parameters:
        researchId: required - recipe unique identifier
        name: required - display name
        description: optional - description
        difficulty: amount of reaseach required to be done, default 1
        position: position in research tree
        parents: list - none, may conatain parent research
        icon: optional (path to image)
    """
    pass

def add_unlock_recipe(
        research: ResearchNodeProto | ResearchNodeProto.ID | str,
        machine: MachineProto | MachineProto.ID | str,
        proto: RecipeProto | RecipeProto.ID | str
    ):
    """ Adds recipe to existing research """
    pass

def add_unlock_machine(
        research: ResearchNodeProto | ResearchNodeProto.ID | str,
        machine: MachineProto | MachineProto.ID | str
    ):
    """ Adds recipe to existing research """
    pass