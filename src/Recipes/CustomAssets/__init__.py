
## ACT AS BUILD IN FUNCTIONS
## when is not included by block, automatically import them all

from Mafi import ColorRgba, Duration, Quantity, Vector2f, Vector2i, Vector3f, Vector3i, Percent
from Mafi.Core.Factory.Recipes import RecipeProto
from Mafi.Core.Factory.Machines import MachineProto
from Mafi.Core.Research import ResearchCostsTpl, ResearchNodeProto
from Mafi.Core.Products import LooseProductProto, ProductProto
from Mafi.Core.Entities.Static import StaticEntityProto
from Mafi.Core.Entities.Dynamic import DynamicEntityProto

def dependencies(*dependencies: str):
    pass

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

class Model:
    def __init__(
        self,
        path: str,
        vertices: list[Vector3f | (float, float, float)], # type: ignore
        texcoords: list[Vector2f | (float, float)], # type: ignore
        triangles: list[Vector3i | (int, int, int)] # type: ignore
    ):
        self.path = path

class Prefab:
    from CustomRecipes import Prefab

    def __init__(
        self,
        path: str,
        position: Vector3f | (float, float, float), # type: ignore
        rotation: Vector3f | (float, float, float), # type: ignore
        scale: Vector3f | (float, float, float), # type: ignore
        children: list[Prefab | Model] = []
    ):
        self.path = path

class Mat:
    def __init__(
        self,
        path: str
    ):
        self.path = path

class Tex:
    def __init__(
        self,
        path: str
    ):
        self.path = path

def recipe_id(recipeId: str | RecipeProto) -> RecipeProto.ID:
    """ create recipe id from text value or from RecipeProto """
    return RecipeProto.ID(recipeId);

def add_texture(path: str, replace: str = None) -> Tex:
    """
    Parameters:
        icon: path within mod, 
        replace: path of icon to be replaced with the modification,
            when used, the path within assets will be linked by replace value
    """
    pass

def add_prefab_box(
        path: str,
        texture: str = None
    ) -> Prefab:
    """
    Parameters:
        icon: path within mod, 
        replace: path of icon to be replaced with the modification,
            when used, the path within assets will be linked by replace value
    """
    pass

def add_texture_material(
        path: str,
        texture: str = None
        # TODO add additional settings
        # like normal map, color, ...
    ) -> Mat:
    """
    Parameters:
        icon: path within mod, 
        replace: path of icon to be replaced with the modification,
            when used, the path within assets will be linked by replace value
    """
    pass

def build_product_loose(
        productId: ProductProto.ID | str,
        name: str,
        icon: str,
        color: ColorRgba | (int, int, int), # type: ignore
        material: Mat | str,
        description: str = "",
        isDumped = False,
        isStorable = False,
        isRecyclable = False,
        isWaste = False,
        isRough = False
    ) -> LooseProductProto:
    pass

def build_product_unit(
        productId: ProductProto.ID | str,
        name: str,
        icon: str,
        prefab: Prefab | str,
        maxTransport = Quantity(3),
        description: str = "",
        isStorable = False,
        isWaste = False
    ) -> LooseProductProto:
    pass

def build_product_fluid(
        productId: ProductProto.ID | str,
        name: str,
        icon: str,
        color: ColorRgba | (int, int, int) = None, # type: ignore
        transportColor: ColorRgba | (int, int, int) = None, # type: ignore
        transportAccentColor: ColorRgba | (int, int, int) = None, # type: ignore
        canBeDiscarded = True,
        description: str = "",
        isStorable = False,
        isWaste = False
    ) -> LooseProductProto:
    pass

def build_recipe(
        recipeId: RecipeProto.ID | str,
        name: str,
        description: str,
        machine: MachineProto.ID | MachineProto | str,
        research: ResearchNodeProto | ResearchNodeProto.ID | str | None = None,
        duration: Duration | None = Duration(60),
        ingredients: list[Product] | None = [],
        products: list[Product] | None = [],
        power: Percent | int | None = None
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
        power: optional - Percent value defining power consumption modification
    """
    pass

def edit_recipe(
        recipe: RecipeProto | RecipeProto.ID | str,
        duration: Duration | None = Duration(60),
        ingredients: list[Product] | None = [],
        products: list[Product] | None = [],
        machine: MachineProto.ID | MachineProto | str = None,
        research: ResearchNodeProto | ResearchNodeProto.ID | str | None = None,
        power: Percent | int | None = None
    ) -> RecipeProto:
    """
    Parameters:
        recipe: required - recipe unique identifier
        duration: default - 60 seconds
            may be redefined by Duration.FromSec(int) or by Duration.FromMin(int)
        machine: optional - machine to add the recipe (required for research or addition of recipe to other machine, but it may cause issues with recipe outputs)
        research: optional - default research (usually already existing)
            research definition is optional, it may be later added by
            add_unlock(researchId, machineId, build_recipe(Recipe_Class))
            in case is not define in eather case, it will be locked in game
        ingredients: list - none, empty or at least one Product in case products are empty
        products: list - none, empty or at least one Product in case ingredients are empty
        power: optional - Percent value defining power consumption modification
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
    """ Adds machine to existing research """
    pass

def add_unlock_product(
        research: ResearchNodeProto | ResearchNodeProto.ID | str,
        product: ProductProto | ProductProto.ID | str
    ):
    """ Adds product to existing research """
    pass