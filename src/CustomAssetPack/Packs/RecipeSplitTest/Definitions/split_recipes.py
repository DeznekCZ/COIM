"""
Load-time test pack for the 0.3.0 recipe split.

Every recipe below is throwaway (the transformations are deliberately silly) —
the point is to exercise each code path in build_recipe / bind_recipe. Only
VANILLA products and machines are used, so the pack needs no assets.

If this pack loads with no errors in the log, all five paths work:

  1. `with build_recipe(...) as r:`  — context form, one recipe on TWO machines
                                       with different durations + a multiplier
  2. `with build_recipe(...):`       — context form WITHOUT an `as` clause
  3. explicit                        — machine-less build_recipe + bind_recipe(r, machine)
  4. multi-same-type outputs         — two fluid + two loose outputs on one machine
  5. legacy                          — one-shot build_recipe(machine=..., duration=...)

PORTS
-----
Every binding below assigns a port for every product. Port letters are
machine-specific; these come from the machine's own layout:

  IndustrialMixer / IndustrialMixerT2
      inputs   A B C = loose,  D = fluid
      outputs  X     = loose,  Y = fluid

  ExhaustScrubber
      inputs   E     = loose,  A B C D = fluid
      outputs  W Z   = loose,  X Y     = fluid

(You can read these for any machine in the recipe editor's port view, or from
Data/machine_ports.json which CustomAssets regenerates on each game load.)

A port selector can be:

  "A"    a single port
  "AB"   MULTIPLE ports — one character per port, no separator. The product may
         use any of them, so "AB" lets a product be fed from (or dispensed to)
         both A and B. Handy for a high-throughput input you want to hook up
         with two pipes. See recipe 3 below.
  "*"    the first free compatible slot, resolved at bind time — the same
         auto-assignment you get by leaving the product out of `ports=`
         entirely, just written down explicitly. "*" cannot be combined with
         letters ("A*" is rejected).

Mixing is fine: pin the products that matter and leave the rest on "*"
(see recipe 2).

One caveat for OUTPUTS: at most one product of a given type may resolve to
several ports. Two fluid outputs both on "*" (or both on "XY") is ambiguous —
the game can't tell which goes where. That's why outputs are usually pinned to
a single letter each, while inputs are where multi-port selectors earn their
keep.

Omitting `ports=` lets every product auto-resolve, and same-type outputs are
spread across distinct ports automatically — that is what stops a multi-fluid-
output recipe from failing with "output port matching is ambiguous". Assigning
them explicitly, as below, is the safer habit: it is deterministic and survives
a machine gaining extra ports later.
"""

from Mafi import Duration, Quantity
from Mafi.Base import Ids
from CustomAssets import build_recipe, bind_recipe, Product, PortMap


# ---------------------------------------------------------------------------
# 1) `with ... as r:` — one recipe, two machines, different durations.
#    Inside the block bind_recipe takes the MACHINE as its only positional
#    argument; the recipe comes from the enclosing context. The T2 binding also
#    exercises `multiplier` (same recipe, double throughput).
#
#    Both tiers share a layout, so both bindings use the same port letters —
#    but each binding carries its own copy, which is the point of the split.
# ---------------------------------------------------------------------------
with build_recipe(
        recipeId    = "SplitTest_GravelMixing",
        name        = "Split test — gravel mixing",
        description = "Iron ore + limestone into gravel. Bound to two mixer tiers.",
        ingredients = [
            Product(Ids.Products.IronOre,   Quantity(1)),
            Product(Ids.Products.Limestone, Quantity(3))
        ],
        products = [
            Product(Ids.Products.Gravel, Quantity(4))
        ]) as gravel_mixing:

    bind_recipe(Ids.Machines.IndustrialMixer,
        duration = Duration.FromSec(60),
        ports = [
            PortMap(Ids.Products.IronOre,   "A"),
            PortMap(Ids.Products.Limestone, "B"),
            PortMap(Ids.Products.Gravel,    "X")
        ])

    bind_recipe(Ids.Machines.IndustrialMixerT2,
        duration   = Duration.FromSec(30),
        multiplier = 2,
        ports = [
            PortMap(Ids.Products.IronOre,   "A"),
            PortMap(Ids.Products.Limestone, "B"),
            PortMap(Ids.Products.Gravel,    "X")
        ])


# ---------------------------------------------------------------------------
# 2) `with ...:` WITHOUT an `as` clause. Nothing outside the block needs to
#    reference this recipe, so no variable is bound — the body still attaches
#    to it through the context.
#
#    Also shows MIXING a pinned port with "*": limestone is pinned to the first
#    loose input, while the sand output takes "*" — the first free compatible
#    slot, picked at bind time.
# ---------------------------------------------------------------------------
with build_recipe(
        recipeId    = "SplitTest_SandMilling",
        name        = "Split test — sand milling",
        description = "Limestone milled into sand. Single machine, no `as` clause.",
        ingredients = [Product(Ids.Products.Limestone, Quantity(4))],
        products    = [Product(Ids.Products.Sand,      Quantity(4))]):

    bind_recipe(Ids.Machines.IndustrialMixer,
        duration = Duration.FromSec(45),
        ports = [
            PortMap(Ids.Products.Limestone, "A"),   # pinned to loose input A
            PortMap(Ids.Products.Sand,      "*")    # "*" = first free compatible port
        ])


# ---------------------------------------------------------------------------
# 3) + 4) Explicit form (no `with`), AND the multi-same-type-output case.
#
#    This recipe has FOUR outputs: two loose (sulfur, slag) and two fluid
#    (carbon dioxide, low-pressure steam). The scrubber has two ports of each
#    type, so each same-type pair has to land on a DIFFERENT port — the case
#    that used to fail with "output port matching is ambiguous".
#
#    The exhaust input also demonstrates a MULTI-PORT selector: "AB" hooks it up
#    to both fluid inputs A and B, so the gas can be piped in from two sides.
#    Water then takes C. Outputs stay pinned to one letter each — see the
#    caveat in the header about only one same-type output being allowed to
#    resolve to several ports.
#
#    To regression-test the automatic distribution instead, delete the `ports=`
#    argument below: the framework then spreads W/Z and X/Y itself.
# ---------------------------------------------------------------------------
scrubbing = build_recipe(
    recipeId    = "SplitTest_MultiFluidScrubbing",
    name        = "Split test — multi-fluid scrubbing",
    description = "Two loose + two fluid outputs; checks same-type port spreading.",
    ingredients = [
        Product(Ids.Products.Exhaust, Quantity(120)),
        Product(Ids.Products.Water,   Quantity(12))
    ],
    products = [
        Product(Ids.Products.Sulfur,        Quantity(3)),
        Product(Ids.Products.CarbonDioxide, Quantity(48)),
        Product(Ids.Products.SteamLo,       Quantity(12)),
        Product(Ids.Products.Slag,          Quantity(1))
    ]
)

bind_recipe(scrubbing, Ids.Machines.ExhaustScrubber,
    duration = Duration.FromSec(30),
    ports = [
        # fluid inputs. "AB" = MULTIPLE ports: exhaust may arrive on A *or* B.
        PortMap(Ids.Products.Exhaust,       "AB"),
        PortMap(Ids.Products.Water,         "C"),
        # loose outputs -> W, Z   (two of the same type, so distinct ports)
        PortMap(Ids.Products.Sulfur,        "W"),
        PortMap(Ids.Products.Slag,          "Z"),
        # fluid outputs -> Y, X   (likewise)
        PortMap(Ids.Products.CarbonDioxide, "Y"),
        PortMap(Ids.Products.SteamLo,       "X")
    ])


# ---------------------------------------------------------------------------
# 5) LEGACY one-shot form — machine/duration passed straight to build_recipe.
#    Still supported so old packs keep working. Here the port goes on the
#    Product(...) entry itself (the pre-0.3.0 way) rather than in a PortMap.
#    Opening this recipe in the visual editor and saving upgrades it to the
#    `with` form above, carrying these ports over as PortMap entries.
# ---------------------------------------------------------------------------
build_recipe(
    recipeId    = "SplitTest_LegacyGravelWashing",
    name        = "Split test — legacy gravel washing",
    description = "Legacy one-shot form: machine + duration inline on build_recipe.",
    machine     = Ids.Machines.IndustrialMixerT2,
    duration    = Duration.FromSec(25),
    ingredients = [
        Product(Ids.Products.Gravel, Quantity(4), "A"),
        Product(Ids.Products.Water,  Quantity(4), "D")
    ],
    products = [
        Product(Ids.Products.Sand, Quantity(4), "X")
    ]
)
