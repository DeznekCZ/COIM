
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
from Mafi.Core.Entities.Static.Layout import LayoutEntityProto, ToolbarCategoryProto

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

class FuelPair:
    def __init__(
        self,
        fuelIn: ProductProto | ProductProto.ID | str,
        spentFuelOut: ProductProto | ProductProto.ID | str,
        durationSeconds: int
    ):
        """
        One nuclear-reactor fuel cycle. Used inside
        `build_nuclear_reactor(..., fuel_pairs=[FuelPair(...), ...])` to
        override the reactor's fuel chemistry list.

        Parameters:
            fuelIn:           ProductProto / id of the fresh fuel product.
            spentFuelOut:     ProductProto / id of the spent fuel product the
                              reactor returns.
            durationSeconds:  how long the fuel lasts at power level 1, in
                              seconds.

        Example:
            build_nuclear_reactor(
                reactorId   = "MyReactor",
                source      = "NuclearReactor",
                fuel_pairs  = [
                    FuelPair("Product_UraniumFuelRod",  "Product_UraniumSpent",  3600),
                    FuelPair("Product_PlutoniumMox",    "Product_PlutoniumSpent", 1800),
                ]
            )
        """
        self.fuelIn = fuelIn
        self.spentFuelOut = spentFuelOut
        self.durationSeconds = durationSeconds

class Port:
    def __init__(
        self,
        name: str,
        type: str,
        shape: str,
        position: Vector3i | (int, int, int) | (int, int), # type: ignore
        direction: str,
        canOnlyConnectToTransports: bool = False
    ):
        """
        Spec for a single I/O port on a machine layout. Consumed by `edit_machine_ports`
        and `clone_machine` to extend the connectivity surface of an existing or cloned
        building.

        Parameters:
            name:      single character port label. Used as the recipe-side port selector
                       (e.g. Product(..., port = 'w')). Must not collide with any port
                       already on the target machine's layout.
            type:      "input" or "output" (case-insensitive).
            shape:     IoPortShapeProto id string. Picks the physical transport shape the
                       port accepts. Common values from the base game:
                         "IoPortShape_Pipe"                 — fluid pipe
                         "IoPortShape_FlatConveyor"         — countable items on flat conveyor
                         "IoPortShape_LooseMaterialConveyor"— loose piles on covered conveyor
                         "IoPortShape_MoltenMetalChannel"   — molten metal channels
                       The shape's AllowedProductType must match recipes that route
                       products through this port.
            position:  relative tile coordinate on the layout where the port sits, as
                       (x, y, z) or (x, y) (z defaults to 0). Origin (0,0,0) is the
                       low-XY corner of the entity. The port itself "occupies" the edge
                       tile and points outward; the connected transport occupies the
                       neighboring tile in the `direction` of the port.
            direction: one of "+X", "-X", "+Y", "-Y" — direction the port faces, which is
                       also the direction the connected transport extends into.
            canOnlyConnectToTransports:
                       when True the port refuses direct machine-to-machine connections
                       and only accepts a transport (conveyor/pipe). Default False.

        Example:
            edit_machine_ports(
                machine = "ChemicalPlant",
                add_ports = [
                    Port(name='w', type='input',  shape='IoPortShape_Pipe',
                         position=(0, 1, 0), direction='-X'),
                    Port(name='o', type='output', shape='IoPortShape_FlatConveyor',
                         position=(3, 0, 0), direction='+X'),
                ]
            )
        """
        self.name = name
        self.type = type
        self.shape = shape
        self.position = position
        self.direction = direction
        self.canOnlyConnectToTransports = canOnlyConnectToTransports

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
        winding: str = "ccw",
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
                   recalculated otherwise).
        winding:   "ccw" (default) or "cw". Controls how face corners are wound during
                   fan-triangulation. Standard OBJ exports from Blender / Maya / 3ds Max
                   are CCW-from-outside, which Unity treats as front-facing — leave the
                   default. Set to "cw" only if your .obj loads inside-out (typical of
                   CW-authored or legacy files).
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

def edit_machine_ports(
        machine: MachineProto | MachineProto.ID | LayoutEntityProto | str,
        add_ports: list[Port] = []
    ) -> LayoutEntityProto:
    """
    Append one or more ports to an existing machine (or any LayoutEntityProto). The
    building's tile footprint, costs, graphics, and recipes are preserved — only the
    connectivity surface changes. Existing recipe port selectors (`Product(..., port=...)`)
    keep working because the original ports stay in place; new ports are simply added.

    Parameters:
        machine:    target building. Accepts MachineProto, MachineProto.ID, a string id,
                    or any LayoutEntityProto. Resolved through the prototypes DB.
        add_ports:  list of Port(...) specs. Each Port carries name, type, shape,
                    position, direction, and optional canOnlyConnectToTransports.

    Constraints:
        - Must be called during definition loading (before LockAndInitializeProtos). The
          Python load path always runs at the right time, so this is automatic — just
          don't try to invoke after game start.
        - Port names must not collide with existing port names on the target's layout.
          A collision raises ArgumentException at registration time.
        - The new port's `position` should sit on or just outside the building's tile
          footprint. The port label and arrow render in the empty tile adjacent to the
          port in the `direction` of the port. Off-footprint positions are allowed (the
          layout's tile coverage is left unchanged) but the in-game placement preview
          still has to find a connecting tile, so prefer positions on the building edge.

    Example — give the Chemical Plant an extra water input on its west edge:
        edit_machine_ports(
            machine = "ChemicalPlant",
            add_ports = [
                Port(name='w', type='input', shape='IoPortShape_Pipe',
                     position=(0, 1, 0), direction='-X'),
            ]
        )

    Returns:
        The same LayoutEntityProto (now with the additional ports).
    """
    pass

def clone_machine(
        machineId: MachineProto.ID | str,
        source: MachineProto | MachineProto.ID | str,
        name: str = None,
        description: str = None,
        add_ports: list[Port] = [],
        consumedPowerPerTick: int = None,
        research: ResearchNodeProto | ResearchNodeProto.ID | str | None = None,
        copy_recipes: bool = True,
        copy_layout: bool = True,
        copy_ports: bool = True,
        copy_graphics: bool = True,
        layout_str: str = None,
        lockedOnInit: bool = None
    ) -> MachineProto:
    """
    Clone an existing MachineProto under a new id, optionally with additional ports and
    an override power draw. Non-overridden fields (costs, graphics, animation, computing
    cost, etc.) are copied by reference from `source` — the clone is visually identical
    unless you swap its graphics later via reflection.

    Use this when you want a "variant" of a vanilla machine — e.g. a Chemical Plant with
    one extra loose-material input — without authoring a whole new proto from scratch.

    Parameters:
        machineId:             required. New unique MachineProto id (string or ID).
        source:                required. Template machine to copy. Resolved via the
                               prototypes DB; must already be registered.
        name:                  optional. Display name. Defaults to the source's name.
        description:           optional. Short description. Defaults to the source's.
        add_ports:             optional list of Port(...) specs to append on top of the
                               source's existing ports. Same uniqueness rules as
                               edit_machine_ports.
        consumedPowerPerTick:  optional kW override. When omitted, the clone draws the
                               same power as the source.
        research:              optional research node that unlocks this machine. When
                               set, the clone is locked-on-init by default and appended
                               to the research node's Units / IconsProtos.
        copy_recipes:          default True. When True, every recipe registered on the
                               source machine is republished onto the clone. Set False
                               to start with no recipes and hand-curate via build_recipe
                               (passing the new machine id).
        lockedOnInit:          override the auto-lock behavior (default True when
                               research is provided, False otherwise).

    Example — chemical plant with an extra loose input, kept locked behind research:
        clone_machine(
            machineId = "ChemicalPlantPlus",
            source    = "ChemicalPlant",
            name      = "Chemical Plant Plus",
            add_ports = [
                Port(name='L', type='input',
                     shape='IoPortShape_LooseMaterialConveyor',
                     position=(0, 2, 0), direction='-X'),
            ],
            research  = "CustomResearch_ChemicalPlantPlus"
        )
    """
    pass

def build_machine(
        machineId: MachineProto.ID | str,
        source: MachineProto | MachineProto.ID | str,
        name: str = None,
        description: str = None,
        add_ports: list[Port] = [],
        consumedPowerPerTick: int = None,
        research: ResearchNodeProto | ResearchNodeProto.ID | str | None = None,
        copy_recipes: bool = True,
        copy_layout: bool = True,
        copy_ports: bool = True,
        copy_graphics: bool = True,
        layout_str: str = None,
        lockedOnInit: bool = None
    ) -> MachineProto:
    """
    Sibling of build_generator — registers a NEW MachineProto whose layout / graphics /
    animation / costs are cloned from `source`, with optional name / description / power /
    port overrides on top. Functionally equivalent to clone_machine; the two names exist so
    a modder can pick whichever reads better in context. build_machine reads as "construct a
    new machine type", clone_machine reads as "duplicate this machine with tweaks". Both go
    through the same runtime path.

    Parameters:
        machineId:             required. New unique MachineProto id (string or ID).
        source:                required. Template machine to clone visuals + layout from.
                               Must already be registered in the prototypes DB.
        name:                  optional. Display name. Defaults to the source's name.
        description:           optional. Short description. Defaults to the source's.
        add_ports:             optional list of Port(...) specs appended to the source's
                               existing ports. Same uniqueness rules as edit_machine_ports.
        consumedPowerPerTick:  optional kW override. When omitted, the new machine draws
                               the same power per tick as the source.
        research:              optional research node that unlocks this machine. When set,
                               the new machine is locked-on-init by default and added to
                               the research node's Units / IconsProtos.
        copy_recipes:          default True. When True, every recipe registered on the
                               source is republished onto the new machine. Set False to
                               start empty and hand-curate via build_recipe.
        lockedOnInit:          override the auto-lock (default True when research is set).

    Example — water-cooled chemical plant variant with extra ports + recipe set carried over:
        build_machine(
            machineId            = "ChemicalPlantWaterCooled",
            source               = "ChemicalPlant",
            name                 = "Chemical Plant (water-cooled)",
            description          = "Lower power draw, requires water feed.",
            consumedPowerPerTick = 80,
            add_ports = [
                Port(name='w', type='input', shape='IoPortShape_Pipe',
                     position=(0, 1, 0), direction='-X'),
            ],
            research = "CustomResearch_WaterCooling"
        )
    """
    pass

def define_box_type(
        boxTypeId: str,
        token: str,
        heightFrom: int = 0,
        heightTo: int = None,
        constraint: str = None,
        surface: str = None,
        terrainMaterial: str = None,
        isRamp: bool = False
    ):
    """
    Register a reusable custom tile ("box") type for the visual layout editor
    and the runtime layout parser. The layout editor lists every define_box_type
    in its palette so a footprint can be painted with modder-defined tiles, and
    the runtime feeds each one to COI's layout parser as a CustomLayoutToken.

    Parameters:
        boxTypeId:       required. Unique id used to reference this box type.
        token:           required. The literal 3-character grid token, e.g.
                         "=0=" or "<#>". A '0' in the MIDDLE slot is a per-tile
                         height wildcard (a digit 1..9 chosen per tile). The
                         token's first character must NOT be an uppercase
                         letter A-Z (reserved for port names) or a port-direction
                         arrow (^ > v < +), or it would be mistaken for a port.
        heightFrom:      start height in tiles (0 = ground level).
        heightTo:        end height (exclusive). Omit to derive the height from
                         the per-tile wildcard digit; set it for a fixed ceiling.
        constraint:      optional tile constraint — "None" / "Ground" / "Ocean".
        surface:         optional terrain tile surface proto id placed on the tile.
        terrainMaterial: optional terrain material proto id applied to the tile.
        isRamp:          mark the token as a vehicle ramp.

    Example — a 1-tile-tall reinforced pad the editor can paint:
        define_box_type(
            boxTypeId = "ReinforcedPad",
            token     = "=0=",
            heightFrom = 0,
            constraint = "Ground"
        )
    """
    pass

# Alias — `layout_token(...)` reads better when defining inline grid tokens.
layout_token = define_box_type

def build_housing(
        housingId: str,
        source: str,
        name: str = None,
        description: str = None,
        capacity: int = None,
        upointsCapacity: int = None,
        research: ResearchNodeProto | ResearchNodeProto.ID | str | None = None,
        lockedOnInit: bool = None
    ):
    """
    Clone an existing SettlementHousingModuleProto under a new id. Inherits layout,
    graphics, costs, unity-bonus needs profile, and consumption increases from the
    source housing — only the modder-facing knobs (id / name / description / capacity
    / unity-points capacity / unlock research) are overridable.

    Modders typically use this to ship a re-skinned or differently-sized housing
    variant unlocked by their own research tree, without having to author the unity-
    bonus matrix from scratch.

    Parameters:
        housingId:        required. New unique StaticEntityProto id.
        source:           required. Id of the housing to clone (vanilla or modded).
                          Common base-game ids: "HousingT1" / "HousingT2" / "HousingT3".
        name:             optional. Display name. Defaults to the source's name.
        description:      optional. Short description. Defaults to the source's.
        capacity:         optional. Max population count. Defaults to source.
        upointsCapacity:  optional. Unity-points global capacity bonus. Defaults to source.
        research:         optional. Research node that unlocks this housing. When set,
                          the new housing is locked-on-init by default.
        lockedOnInit:     optional. Override the auto-lock behavior (default True when
                          research is set, False otherwise).

    Example — tier-2 housing variant with bigger capacity, gated behind own research:
        build_housing(
            housingId       = "HousingT2_Modded",
            source          = "HousingT2",
            name            = "Housing T2 (large)",
            capacity        = 80,
            upointsCapacity = 60,
            research        = "CustomResearch_LargeHousing"
        )
    """
    pass

def build_settlement_decoration(
        decorationId: str,
        source: str,
        name: str = None,
        description: str = None,
        upointsBonus: int = None,
        bonusRange: int = None,
        research: ResearchNodeProto | ResearchNodeProto.ID | str | None = None,
        lockedOnInit: bool = None
    ):
    """
    Clone an existing SettlementDecorationModuleProto under a new id. Inherits layout,
    graphics, costs from the source; overrides the two modder-facing knobs
    (unity-points bonus to nearby housing + bonus tile range).
    """
    pass

def build_settlement_food(
        foodModuleId: str,
        source: str,
        name: str = None,
        description: str = None,
        buffersCount: int = None,
        capacityPerBuffer: int = None,
        research: ResearchNodeProto | ResearchNodeProto.ID | str | None = None,
        lockedOnInit: bool = None
    ):
    """
    Clone an existing SettlementFoodModuleProto under a new id. Overrides:
    buffer count + capacity per buffer. Food module size scales with both.
    """
    pass

def build_settlement_isp(
        ispModuleId: str,
        source: str,
        name: str = None,
        description: str = None,
        computingPer100Pops: int = None,
        electricityConsumedKw: int = None,
        research: ResearchNodeProto | ResearchNodeProto.ID | str | None = None,
        lockedOnInit: bool = None
    ):
    """
    Clone an existing SettlementIspModuleProto under a new id. ISP modules supply
    the "internet" need; the per-pop computing requirement + electricity draw are
    the typical mod knobs. PopNeed and emission intensity come from source verbatim.
    """
    pass

def build_hospital(
        hospitalId: str,
        source: str,
        name: str = None,
        description: str = None,
        powerRequiredKw: int = None,
        buffersCount: int = None,
        capacityPerBuffer: int = None,
        suppliesPerHundredPopsPerMonth: int = None,
        research: ResearchNodeProto | ResearchNodeProto.ID | str | None = None,
        lockedOnInit: bool = None
    ):
    """
    Clone an existing HospitalProto under a new id. Override power draw, buffer
    capacity, and the per-100-pops monthly supplies consumption coefficient.
    """
    pass

def build_mine_tower(
        mineTowerId: str,
        source: str,
        name: str = None,
        description: str = None,
        research: ResearchNodeProto | ResearchNodeProto.ID | str | None = None,
        lockedOnInit: bool = None
    ):
    """
    Clone an existing MineTowerProto under a new id. Layout / costs / graphics /
    MineArea (origin + initial-size + max-edge) come from source — modders pick
    among the existing tower shapes when reskinning, not custom area sizes.
    """
    pass

def build_research_lab(
        researchLabId: str,
        source: str,
        name: str = None,
        description: str = None,
        electricityConsumedKw: int = None,
        computingConsumed: int = None,
        durationForRecipeSeconds: int = None,
        sciencePerRecipe: int = None,
        unityMonthlyCost: int = None,
        research: ResearchNodeProto | ResearchNodeProto.ID | str | None = None,
        lockedOnInit: bool = None
    ):
    """
    Clone an existing ResearchLabProto under a new id. Common knobs covered: power,
    computing, cycle duration, science per recipe, unity monthly cost. Tier index,
    consumed/produced product per recipe, and buffer capacities come from source.
    """
    pass

def edit_nuclear_reactor_ports(
        reactor: str,
        add_ports: list[Port] = []
    ):
    """
    Reactor-typed sibling of `edit_machine_ports`. Appends one or more new ports to an
    existing NuclearReactorProto's layout without cloning the whole reactor. Useful for
    wiring enrichment input/output (or any auxiliary connection) into a vanilla reactor.

    Parameters:
        reactor:    required. Id of the NuclearReactorProto to extend (vanilla or
                    modded). Common base-game ids: "NuclearReactor" / "NuclearReactorT2".
        add_ports:  list of Port(...) specs. Each Port carries name, type, shape,
                    position, direction, and optional canOnlyConnectToTransports.

    Constraints:
        - Must run during definition loading (the Python load path always does — just
          don't call after game start).
        - Port names must not collide with the reactor's existing fuel / water / steam
          / coolant ports. A collision raises ArgumentException at registration time.
        - Position should sit on or just outside the building's tile footprint, with
          `direction` pointing OUT of the building into the empty connecting tile.

    Example — add enrichment in/out ports to the vanilla reactor:
        edit_nuclear_reactor_ports(
            reactor   = "NuclearReactor",
            add_ports = [
                Port(name='e', type='input',  shape='IoPortShape_FlatConveyor',
                     position=(0, 2, 0), direction='-X'),
                Port(name='x', type='output', shape='IoPortShape_FlatConveyor',
                     position=(3, 2, 0), direction='+X'),
            ]
        )
    """
    pass

def edit_nuclear_reactor_fuels(
        reactor: str,
        add_fuels: list[FuelPair]
    ):
    """
    Append additional fuel cycles to an existing NuclearReactorProto WITHOUT cloning
    the whole reactor. Use when a mod just wants to drop a new fuel chemistry into a
    vanilla reactor (e.g. add CANDU rods to the base NuclearReactor) — much smaller
    surface than build_nuclear_reactor for that common case.

    Parameters:
        reactor:    required. Id of the reactor proto to extend (vanilla or modded).
                    Common base-game ids: "NuclearReactor" / "NuclearReactorT2".
        add_fuels:  required. List of FuelPair(fuelIn, spentFuelOut, durationSeconds)
                    entries to append to the reactor's fuel list.

    Caveat:
        The reactor's fuel-in / fuel-out port shapes are fixed at the source's first
        fuel pair's product types. Adding fuels of a DIFFERENT product type works
        in the fuel list but the physical port still only accepts the original
        type — the new fuel won't be insertable in-game. For different fuel types
        use build_nuclear_reactor with explicit fuelInPortShape / fuelOutPortShape.

    Example — add a CANDU rod to the vanilla reactor:
        edit_nuclear_reactor_fuels(
            reactor   = "NuclearReactor",
            add_fuels = [
                FuelPair(fuelIn="Product_Candu_rod", spentFuelOut="Product_SpentFuel", durationSeconds=120)
            ]
        )
    """
    pass

def build_nuclear_reactor(
        reactorId: str,
        source: str,
        name: str = None,
        description: str = None,
        maxPowerLevel: int = None,
        fuelCapacity: int = None,
        minFuelToOperate: int = None,
        processDurationSeconds: int = None,
        computingConsumed: int = None,
        fuel_pairs: list[FuelPair] = None,
        fuelInPortShape: str = None,
        fuelOutPortShape: str = None,
        research: ResearchNodeProto | ResearchNodeProto.ID | str | None = None,
        lockedOnInit: bool = None
    ):
    """
    Clone an existing NuclearReactorProto under a new id. Tunable: max power level,
    fuel capacity + minimum to operate, process duration, computing, AND the fuel
    cycle list (`fuel_pairs`). Structurally heavy fields that aren't yet exposed
    (coolant proto refs, port chars, enrichment data, water/steam product
    quantities, enrichment breeding ratios) still come from source — see the
    `EnrichmentData` notes in the C# proto for the full surface that could be
    added later.

    Parameters:
        fuel_pairs: optional list of FuelPair(fuelIn, spentFuelOut, durationSeconds).
                    When provided, REPLACES the source reactor's fuel list entirely;
                    a custom fuel chemistry (e.g. thorium → uranium-233) is added by
                    listing it here. When omitted (or empty), the source's fuel
                    pairs are inherited verbatim — including their breeding ratios.

        fuelInPortShape / fuelOutPortShape: optional IoPortShapeProto ids that
                    replace the source reactor's fuel-in / fuel-out port shapes
                    on the cloned layout. Use when the modder's fuel chemistry
                    is a different product TYPE than the source's (e.g. coal
                    is loose where uranium is unit) — the port has to physically
                    accept the new transport shape. Common values:
                      "IoPortShape_Pipe"                  — fluid
                      "IoPortShape_FlatConveyor"          — unit (countable)
                      "IoPortShape_LooseMaterialConveyor" — loose (pile)
                      "IoPortShape_MoltenMetalChannel"    — molten
                    When omitted, the runtime infers from the first fuel pair's
                    product type. Pass explicitly when the fuel-in and fuel-out
                    products differ in type (a mixed chemistry).
    """
    pass