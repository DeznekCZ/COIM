# Explicit load order for the Batteries pack.
#
# The CustomAssets framework loads __init__.py first when it exists. We list
# each module via `import <name>` (sugar for dependencies("name")) — the
# framework recursively loads each sibling .py in this order:
#
#   1. battery_products  - product registrations (Lead, Lithium, Cell, all batteries).
#                          Must run first so later recipes can reference these IDs.
#   2. lead_acid         - lead chain: research + recovery + assembly + recycle + consumer alt.
#   3. lithium_ion       - lithium chain: research + extraction + cells + battery + recycle + consumer alt.
#   4. battery_charger   - charging recipes (empty -> charged) on assemblers.
#                          Depends on the *_Empty and *_Charged products from step 1, and the
#                          research nodes from steps 2-3.
#   5. battery_storage   - currently a no-op stub (see file for explanation).

import battery_products
import lead_acid
import lithium_ion
import battery_charger
import battery_storage
