
## ACT AS BUILD IN FUNCTIONS
## when is not included by block, automatically import them all

from Mafi import ColorRgba, Duration, Quantity, Vector2f, Vector2i, Vector3f, Vector3i, Percent
from Mafi.Core.Prototypes import Proto
from Mafi.Core.Factory.Recipes import RecipeProto
from Mafi.Core.Factory.Machines import MachineProto
from Mafi.Core.Research import ResearchCostsTpl, ResearchNodeProto
from Mafi.Core.Products import LooseProductProto, ProductProto
from Mafi.Core.Entities.Static import StaticEntityProto
from Mafi.Core.Entities.Dynamic import DynamicEntityProto
from Mafi.Core.Entities.Static.Layout import ToolbarCategoryProto

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
    from CustomAssets import Prefab

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

def product_exist(product: ProductProto | ProductProto.ID | str) -> bool:
    """
    Returns True if a ProductProto with the given id is currently registered in the
    prototypes database. Useful for guarding optional recipes that depend on products
    introduced by other mods or specific game versions:

        if product_exist("Product_FilterMediaIronLime"):
            build_recipe(...)
    """
    pass

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

def add_loose_product_material(
        path: str,
        albedo: str | Tex | list[str | Tex],
        normals: str | Tex | list[str | Tex] = None,
        metallic: str | Tex | list[str | Tex] = None,
        reference: str | Mat = None,
        tiling: float | int = 1,
    ) -> Mat:
    """
    Register a Material specifically for a loose-pile product. Wraps the same plumbing as
    add_texture_material but exposes the three texture channels the COI pile shader uses
    (`_AlbedoTex`, `_NormalsTex`, `_SmoothMetalTex`) as named parameters.

    Parameters:
        path:      asset path the new material will be registered under, e.g.
                   "Assets/MyMod/MyOreMaterial".
        albedo:    required. Single texture (mod-relative path or Tex) OR a list of textures.
                   For a single LooseProductProto, the first entry is what ends up rendered
                   (LPMM allocates one texture-array slice per product).
        normals:   optional. Same shape as `albedo`. If a single texture and `albedo` is a
                   list of N, replicated to N. If a list, count must match `albedo`. When
                   omitted, the cloned reference material's normals are kept ("copied from
                   reference").
        metallic:  optional. Same as normals — provides the smoothness (A) + metallic (R)
                   channel-packed texture for `_SmoothMetalTex`.
        reference: optional. Path to an existing pile material (vanilla COI or another
                   custom one) to clone shader + properties from. Defaults to
                   `Assets/Base/Products/Loose/FilterMedia_mat` so the new material always
                   carries the correct property layout — Mafi's pile shader is bundled,
                   not `Shader.Find`-able.
        tiling:    optional, default 1. Texture repetition factor when blitting into the
                   array slice. tiling=4 means the source is repeated 4×4 times within the
                   slice, so each rock/grain renders at 1/4 the size. Useful when the source
                   PNG has chunky high-frequency detail and you want it to look denser.
                   Source PNG must be tileable (seamless across edges) for clean results.
    """
    pass

def add_texture_material(
        path: str,
        texture: str | Tex = None,
        reference: str | Mat = None,
        shader: str = None,
    ) -> Mat:
    """
    Register a Material at `path`. The texture file is loaded from the mod folder.

    Parameters:
        path:      asset path the new material will be registered under, e.g.
                   "Assets/MyMod/MyOreMaterial".
        texture:   path within the mod (or a Tex object) of the albedo image file.
        reference: optional. Path to an existing material (vanilla COI or another
                   custom material) whose shader + properties should be cloned.
                   The new texture replaces the cloned albedo. Use this for loose
                   piles, fluids, etc. so the new material matches the game's look:
                       reference = Assets.Base.Products.Loose.FilterMedia_mat
        shader:    optional. Explicit shader name to use when constructing fresh
                   (alternative to `reference`). Falls back to "Standard" if not found.
        If neither reference nor shader is provided, falls back to the Standard
        shader with the engine's default material as a base.
    """
    pass

def build_product_loose(
        productId: ProductProto.ID | str,
        name: str,
        icon: str,
        material: Mat | str,
        color: ColorRgba | (int, int, int) = None, # type: ignore
        particleColor: ColorRgba | (int, int, int) = None, # type: ignore
        description: str = "",
        isDumped = False,
        isStorable = False,
        isRecyclable = False,
        isWaste = False,
        isRough = False,
        pinToHomeScreen = False,
        maxTransport: Quantity | int = None,
        prefabPath: str = None,
        dumpsAs: Proto.ID | str = None,
        research: ResearchNodeProto | ResearchNodeProto.ID | str | None = None
    ) -> LooseProductProto:
    """
    Register a loose (pile) product.

    Parameters:
        productId:      unique product id.
        name:           display name.
        icon:           icon path (within the mod's Assets/, or a vanilla Assets.* path).
                        SVG files are accepted: ModBuilder's `svg2png` step (run during
                        the mod build) rasterizes them to PNG, and at load time any
                        ".svg" icon path is transparently rewritten to ".png" of the
                        same name. Pass `icon = "Assets/MyIcon.svg"` and ship both the
                        .svg (source) and the generated .png in your mod.
        material:       pile material (Mat or asset path). Use add_texture_material(...,
                        reference=Assets.Base.Products.Loose.FilterMedia_mat, ...) to
                        produce one that matches the COI loose-pile look.
        color:          optional. Tint used in resource-overview UI (resourcesVizColor).
                        The actual pile appearance comes from `material`, not this color.
                        Defaults to white when omitted.
        particleColor:  optional. Color of conveyor-spill / mining particles for this
                        product. When omitted, the game auto-derives it from the average
                        of the pile albedo texture. Pass this to override that.
        isDumped:       dumped on terrain by default (vs. requires explicit player
                        marking). Effective only together with `dumpsAs`.
        isStorable:     can be stored.
        isRecyclable:   recyclable.
        isWaste:        marks as waste.
        isRough:        rough pile mesh (rocky/chunky); otherwise smooth (sand-like).
                        Also picks the default `prefabPath` if `prefabPath` is omitted.
        pinToHomeScreen: pinned to home-screen resource list by default.
        maxTransport:   max units per transported product (Quantity or int). Default 5.
        prefabPath:     override the pile prefab. Default picks rough/smooth pile based
                        on `isRough`.
        dumpsAs:        terrain-material id to transform into when dumped on the ground.
                        Pass a typed `Ids.TerrainMaterials.<X>` value or a string id like
                        "Gravel_Terrain". When set, the product becomes dumpable in-game
                        and its surface paints over with the chosen terrain. When omitted,
                        the product cannot be dumped on terrain regardless of `isDumped`
                        — the engine requires this mapping. Common picks:
                          Ids.TerrainMaterials.Gravel       — generic crushed/rocky
                          Ids.TerrainMaterials.Dirt         — soil-like
                          Ids.TerrainMaterials.Slag         — slag piles
                          Ids.TerrainMaterials.Compost      — organic
                          Ids.TerrainMaterials.Landfill     — waste
        research:       optional research node that unlocks this product. When set, the
                        product is locked-on-init by default and appended to the research
                        node's Units list as a ProductUnlock. Equivalent to calling
                        add_unlock_product(research, productId) after build_product_loose
                        with isLocked=True. The research node must already be registered
                        (define it earlier in the load order, e.g. via build_research).
    """
    pass

def add_unit_prefab(
        path: str,
        albedo: str | Tex | list[str | Tex],
        normals: str | Tex | list[str | Tex] = None,
        metallic: str | Tex | list[str | Tex] = None,
        reference: str | Mat = None,
        width: float = 0.5,
        height: float = 0.2,
        depth: float = 0.5,
        mesh: str = None,
    ) -> Prefab:
    """
    Register a prefab for a unit (countable) product. Produces a single-GameObject prefab
    carrying one mesh + one material — the exact shape COI's ProductsRenderer extracts via
    MeshFilter.sharedMesh + MeshRenderer.sharedMaterial. Use the returned Prefab as the
    `prefab` argument to build_product_unit().

    UNITS
    -----
    All distances in this method (width/height/depth, .obj vertex coordinates) are in
    METERS — the same convention Unity uses internally (1 Unity unit = 1 meter). One COI
    tile is 2 meters, so a 0.5m-wide item is a quarter of a tile across.

    For Auto packing to pick Triangle (3 items per tile), the mesh must satisfy
    sizeX < 0.5m AND sizeZ < 0.5m. Triangle layout spaces the three slots ~2*sizeX/sqrt(3)
    apart — for circular items (cylinders) this leaves a small visible gap; for items at
    the upper threshold (~0.5m) they will appear nearly touching, which is normal.

    Parameters:
        path:      asset path the prefab is registered under, e.g.
                   "Assets/MyMod/MyItem.prefab".
        albedo:    required. Single texture or list (only the first slice is used; list shape
                   matches add_loose_product_material for consistency).
        normals:   optional. Same shape rules as add_loose_product_material.
        metallic:  optional. Same shape rules as add_loose_product_material.
        reference: optional. Path to an existing material to clone. If omitted, the material
                   uses Unity's Standard shader — fine for most unit products since the unit-
                   product render path doesn't depend on a Mafi-specific shader.
        width, height, depth:
                   meters. Default 0.5 / 0.2 / 0.5. Used when `mesh` is not given. Origin
                   is at the bottom-center so the prefab sits on the conveyor naturally.
        mesh:      optional. Path to a Wavefront .obj file (mod-relative). When provided,
                   width/height/depth are ignored. Vertex coordinates in the .obj are also
                   meters. The .obj must be a single mesh; faces with >3 vertices are fan-
                   triangulated; UVs/normals are honoured if present (normals are
                   recalculated otherwise). Right-handed CCW winding (standard OBJ) is
                   reversed at load time to Unity's left-handed CW convention.
    """
    pass

def build_product_unit(
        productId: ProductProto.ID | str,
        name: str,
        icon: str,
        prefab: Prefab | str,
        maxTransport = Quantity(3),
        description: str = "",
        isStorable = False,
        isWaste = False,
        packingMode = None,
        allowPackingNoise: bool = False,
        rotateSecondPackedItem90Degs: bool = False,
        research: ResearchNodeProto | ResearchNodeProto.ID | str | None = None
    ) -> LooseProductProto:
    """
    Register a unit (countable) product.

    Parameters:
        productId, name, icon, prefab, maxTransport, description, isStorable, isWaste:
            standard fields.
        packingMode:
            How units are arranged on a single conveyor tile. Use the Mafi enum:
                from Mafi.Core.Products import CountableProductStackingMode
                ...
                packingMode = CountableProductStackingMode.Triangle
            Available values:
              - Auto               (default): picks Triangle/Row/Stacked from mesh size.
                                   For meshes with x<0.5 AND z<0.5 this becomes Triangle —
                                   three items per tile, the layout most modders want.
              - Triangle           : 3 per tile arranged in a triangle.
              - TriangleHorizontal : 3 per tile arranged horizontally.
              - Row                : items in a single row.
              - Stacked            : stacked vertically (for thin items).
              - StackedAlternating : stacked, every other item rotated.
            Case-insensitive strings also accepted ("Triangle", "row", etc.) for shorter call
            sites; the registrator parses them with Enum.Parse.
        allowPackingNoise:
            When True, each item gets a random yaw rotation noise applied for visual variety
            (so identical units don't all face the exact same direction on the conveyor).
        rotateSecondPackedItem90Degs:
            When True, every other packed item is rotated 90° (useful for items that look
            better with alternating orientation, e.g. boxes that visually tile when paired).
        research:
            Optional research node that unlocks this product. When set, the product is
            locked-on-init by default and appended to the research's Units list as a
            ProductUnlock — equivalent to add_unlock_product(research, productId) plus
            isLocked=True. The research must already be registered (define it earlier in
            the load order, e.g. via build_research).
    """
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

def build_generator(
        id: str | MachineProto.ID,
        name: str,
        inputProduct: Product,
        outputElectricityKw: int,
        outputProduct: Product | None = None,
        description: str = "",
        source: str | MachineProto.ID = "DieselGeneratorT2",
        duration: Duration | int | None = None,
        generationPriority: int | None = None,
        bufferCapacityMultiplier: int | None = None,
        research: ResearchNodeProto | ResearchNodeProto.ID | str | None = None,
        lockedOnInit: bool | None = None
    ):
    """
    Register a new electricity generator that consumes a product and produces electricity.

    Backed by Mafi.Base.Prototypes.Machines.PowerGenerators.ElectricityGeneratorFromProductProto
    — the same proto family as DieselGenerator / DieselGeneratorT2. Each instance hard-codes
    a SINGLE InputProduct -> Electricity (+ optional OutputProduct waste) mapping; the proto is
    not recipe-list-driven. Create one instance per fuel/chemistry.

    The implementation clones non-customizable plumbing (layout, costs, graphics, animation,
    destroy-reason) from a source generator (default: DieselGeneratorT2) and substitutes the
    fuel-relevant fields. The resulting machine looks like the source visually but consumes
    a different product.

    Parameters:
        id:                       new generator id (string or MachineProto.ID).
        name:                     display name shown in-game.
        inputProduct:             required. Product(...) describing the fuel and per-cycle quantity.
        outputElectricityKw:      required. kW generated per cycle.
        outputProduct:            optional. Product(...) for a waste byproduct (e.g. spent battery).
        description:              optional. Short description.
        source:                   id of a vanilla generator to clone non-customizable fields from.
                                  Default 'DieselGeneratorT2'. Must be an existing
                                  ElectricityGeneratorFromProductProto in the prototypes DB.
        duration:                 cycle time. Duration or int (seconds). Defaults to source's.
        generationPriority:       priority within the electricity grid. Defaults to source's.
        bufferCapacityMultiplier: internal buffer size factor. Defaults to source's.
        research:                 optional research node that unlocks this generator. When set,
                                  the generator is locked-on-init by default and added to the
                                  research node's Units list.
        lockedOnInit:             override the auto-lock behavior (default True when research is
                                  provided, False otherwise).

    Example:
        build_generator(
            id                  = "BatteryDischarger_LeadAcid",
            name                = "Battery discharger (lead-acid)",
            description         = "Discharges charged lead-acid batteries; returns the empty cells.",
            source              = "DieselGeneratorT2",
            inputProduct        = Product("Product_LeadAcidBatteryCharged", 1),
            outputProduct       = Product("Product_LeadAcidBatteryEmpty",   1),
            outputElectricityKw = 160,
            duration            = Duration.FromSec(20),
            research            = "CustomResearch_LeadAcidBattery"
        )
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

def add_toolbar_category(
        categoryId: Proto.ID | str,
        name: str,
        icon: str,
        parent: ToolbarCategoryProto | Proto.ID | str,
        entities: list[StaticEntityProto | StaticEntityProto.ID | str]
    ) -> ToolbarCategoryProto:
    """ Adds new category to selected entities """
    pass