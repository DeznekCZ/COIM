# Battery pack — product definitions.
#
# Products (raw + battery states):
#   - Product_Lead                          (loose, anode-mud)     input to lead-acid assembly
#   - Product_Lithium                       (unit, slab)           input to lithium-ion assembly
#   - Product_LeadAcidBatteryEmpty          (unit, box)            output of assembly; input to Charger
#   - Product_LeadAcidBatteryCharged        (unit, box)            output of Charger; usable / Discharger input
#   - Product_LithiumIonBatteryEmpty        (unit, barrel)         output of assembly; input to Charger
#   - Product_LithiumIonBatteryCharged      (unit, barrel)         output of Charger; usable / Discharger input
#
# Visual:
#   - Lead-acid uses ConsumerElectronics_prefab (boxy crate) + TriangleHorizontal packing (3 boxes
#     side-by-side per conveyor tile).
#   - Lithium-ion uses ChemicalFuel_prefab (barrel/drum shape) + Triangle packing (3 cylinders
#     triangulated per tile).
#
# Art placeholders: vanilla icons are reused. Drop real PNGs into Assets/Products/Icons/ and
# switch the icon= arguments to your own paths once art is ready.

from Mafi import ColorRgba, Quantity
from Mafi.Base import Assets, Ids
from Mafi.Core.Products import CountableProductStackingMode
from CustomAssets import build_product_loose, build_product_unit

# ---------------------------------------------------------------------------
# Raw / intermediate metals
# ---------------------------------------------------------------------------

# Lead is a LOOSE product (anode-mud granules). Modelled this way so ChemicalPlant's
# 1 unit-out + 1 loose-out can carry BOTH Copper and Lead from the same recovery
# recipe. Matches real electrolytic-refining chemistry (lead from anode mud).
#
# Earlier load failure ("Cannot get empty port for product: Product_Lead") was caused
# by an intermediate state where this was build_product_unit — Lead came out as a
# second unit output and competed with Copper for CP's single unit-out port. With
# Lead as loose it claims the loose-out port instead, leaving the unit-out for Copper.
build_product_loose(
    productId     = "Product_Lead",
    name          = "Lead",
    description   = "Granulated lead recovered from copper-electrolysis anode mud. Primary anode/cathode material for lead-acid batteries.",
    icon          = Assets.Base.Products.Icons.Iron_svg,                # placeholder
    material      = Assets.Base.Products.Loose.Slag_mat,                # dark-grey reference pile
    color         = ColorRgba.DarkGray,
    particleColor = (90, 90, 95),
    isStorable    = True,
    isRecyclable  = False,
    isRough       = False,
    dumpsAs       = Ids.TerrainMaterials.Gravel
)

build_product_unit(
    productId         = "Product_Lithium",
    name              = "Lithium",
    description       = "Lithium metal pack — extracted from brine via electrolysis. Energy-dense; required for lithium-ion cells.",
    icon              = Assets.Base.Products.Icons.Salt_svg,                        # placeholder
    prefab            = Assets.Base.Products.Countable.Plastic_prefab,               # placeholder
    isStorable        = True,
    packingMode       = CountableProductStackingMode.Triangle,
    allowPackingNoise = True
)

# ---------------------------------------------------------------------------
# Lead-acid batteries: empty (from assembly) and charged (from Charger)
# ---------------------------------------------------------------------------

build_product_unit(
    productId         = "Product_LeadAcidBatteryEmpty",
    name              = "Lead-acid battery (empty)",
    description       = "Newly assembled lead-acid battery with no charge. Send to a Battery Charger to fill, or recycle.",
    icon              = Assets.Base.Products.Icons.Electronics1_svg,                # placeholder
    prefab            = Assets.Base.Products.Countable.ConsumerElectronics_prefab,   # boxy crate
    isStorable        = True,
    packingMode       = CountableProductStackingMode.TriangleHorizontal,             # boxes side-by-side
    allowPackingNoise = False
)

build_product_unit(
    productId         = "Product_LeadAcidBatteryCharged",
    name              = "Lead-acid battery (charged)",
    description       = "Charged lead-acid battery. Storable; supply to settlements as consumer electronics, or discharge in a Battery Discharger to recover electricity.",
    icon              = Assets.Base.Products.Icons.Electronics1_svg,                # placeholder
    prefab            = Assets.Base.Products.Countable.ConsumerElectronics_prefab,   # boxy crate
    isStorable        = True,
    packingMode       = CountableProductStackingMode.TriangleHorizontal,
    allowPackingNoise = False
)

# ---------------------------------------------------------------------------
# Lithium-ion intermediate cell + batteries (empty/charged from Charger)
# ---------------------------------------------------------------------------
# Two-step assembly chain (3-input limit on assemblers):
#   1. Lithium + Copper + Plastic        -> LithiumIonCell  (basic electrochemical unit)
#   2. LithiumIonCell + Electronics2     -> LithiumIonBatteryEmpty (cells wired with BMS)

build_product_unit(
    productId         = "Product_LithiumIonCell",
    name              = "Lithium-ion cell",
    description       = "Coiled lithium-ion cell — the basic electrochemical unit. Wire multiple cells with control electronics to assemble a battery pack.",
    icon              = Assets.Base.Products.Icons.Electronics4_svg,                # placeholder
    prefab            = Assets.Base.Products.Countable.UraniumRod_prefab,            # cylindrical cell shape
    isStorable        = True,
    packingMode       = CountableProductStackingMode.Triangle,
    allowPackingNoise = False
)

build_product_unit(
    productId         = "Product_LithiumIonBatteryEmpty",
    name              = "Lithium-ion battery (empty)",
    description       = "Newly assembled lithium-ion battery pack with no charge. Send to a Battery Charger to fill.",
    icon              = Assets.Base.Products.Icons.Electronics4_svg,                # placeholder
    prefab            = Assets.Base.Products.Countable.ChemicalFuel_prefab,          # barrel shape
    isStorable        = True,
    packingMode       = CountableProductStackingMode.Triangle,
    allowPackingNoise = False
)

build_product_unit(
    productId         = "Product_LithiumIonBatteryCharged",
    name              = "Lithium-ion battery (charged)",
    description       = "Charged lithium-ion battery pack. High energy density; storable, ideal for grid buffering or premium consumer electronics.",
    icon              = Assets.Base.Products.Icons.Electronics4_svg,                # placeholder
    prefab            = Assets.Base.Products.Countable.ChemicalFuel_prefab,          # barrel shape
    isStorable        = True,
    packingMode       = CountableProductStackingMode.Triangle,
    allowPackingNoise = False
)
