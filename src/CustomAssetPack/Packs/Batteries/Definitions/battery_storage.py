# Battery dischargers - real ElectricityGeneratorFromProductProto instances.
#
# ElectricityGeneratorFromProductProto isn't recipe-list-driven: each instance
# hard-codes a single InputProduct + OutputProduct + OutputElectricity tuple.
# So we build ONE generator per battery chemistry via build_generator(...).
# Both clone DieselGeneratorT2's layout/costs/graphics and just change the fuel.
#
# Round-trip targets (charger input vs. discharger output):
#   Lead-acid:    200 kW in vs 160 kW out  (~80% RTE)
#   Lithium-ion: 1000 kW in vs 900 kW out  (~90% RTE)

from Mafi import Duration, Quantity
from CustomAssets import build_generator, Product

# Load order is declared centrally in Definitions/__init__.py.

# ---------------------------------------------------------------------------
# Lead-acid discharger
# ---------------------------------------------------------------------------

build_generator(
    id                  = "BatteryDischarger_LeadAcid",
    name                = "Battery discharger (lead-acid)",
    description         = "Consumes charged lead-acid batteries to produce electricity; returns the empty cells for re-charging. Round-trip efficiency ~80%.",
    source              = "DieselGeneratorT2",
    inputProduct        = Product("Product_LeadAcidBatteryCharged", Quantity(1)),
    outputProduct       = Product("Product_LeadAcidBatteryEmpty",   Quantity(1)),
    outputElectricityKw = 160,
    duration            = Duration.FromSec(20),
    research            = "CustomResearch_LeadAcidBattery"
)

# ---------------------------------------------------------------------------
# Lithium-ion discharger
# ---------------------------------------------------------------------------

build_generator(
    id                  = "BatteryDischarger_LithiumIon",
    name                = "Battery discharger (lithium-ion)",
    description         = "Consumes charged lithium-ion batteries to produce electricity; returns the empty packs. Higher energy density than lead-acid; round-trip efficiency ~90%.",
    source              = "DieselGeneratorT2",
    inputProduct        = Product("Product_LithiumIonBatteryCharged", Quantity(1)),
    outputProduct       = Product("Product_LithiumIonBatteryEmpty",   Quantity(1)),
    outputElectricityKw = 900,
    duration            = Duration.FromSec(20),
    research            = "CustomResearch_LithiumIonBattery"
)
