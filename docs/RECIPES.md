# CustomAssets — Recipes

How to define recipes and attach them to machines with the **CustomAssets** Python
API. This is the deep-dive companion to the [Recipes](MODDING.md#recipes) section of
the [Modder's Guide](MODDING.md).

> **What changed (0.3.0).** A recipe used to be created *and* bound to a machine in a
> single `build_recipe(machine=…)` call. As of the 0.3.0 game API a **recipe is
> machine-less**: it has inputs, outputs and a power multiplier, but **no machine, no
> duration, and no ports**. You attach it to one or more machines separately with
> `bind_recipe(...)`, and each binding carries **its own** duration and port mapping.
> The old one-shot form still works (see [Legacy form](#legacy-one-shot-form)).

## Table of contents

- [The two halves: `build_recipe` + `bind_recipe`](#the-two-halves)
- [`Product` and quantities](#product-and-quantities)
- [`PortMap` and port routing](#portmap-and-port-routing)
- [The `with` form (recommended for hand-authoring)](#the-with-form)
- [Binding to several machines](#binding-to-several-machines)
- [Unlocks](#unlocks)
- [Legacy one-shot form](#legacy-one-shot-form)
- [Reference](#reference)

## The two halves

`build_recipe(...)` makes the recipe. `bind_recipe(...)` attaches it to a machine.

```python
# 1) Define the recipe — inputs → outputs. No machine, no duration.
r = build_recipe(
    recipeId    = "CustomRecipe_CannedCorn_Sealing",
    name        = "Canned corn",
    description = "Fill empty cans with corn and seal them.",
    ingredients = [
        Product("Product_EmptyCansBox", Quantity(8)),
        Product(Ids.Products.Corn,      Quantity(8)),
    ],
    products = [
        Product("Product_CannedCorn",   Quantity(8)),
    ],
    power = 110,   # optional; percent of base machine power (applies on every machine)
)

# 2) Attach it to a machine, with THIS machine's cycle time.
bind_recipe(r, Ids.Machines.ChemicalPlant, duration = Duration.FromSec(30))
```

`build_recipe` returns the `RecipeProto`, so you pass `r` straight into `bind_recipe`.
You can also bind by id: `bind_recipe("CustomRecipe_CannedCorn_Sealing", ...)`.

## `Product` and quantities

Recipe inputs/outputs are `Product(product, quantity, port="*")` entries. **Quantities
live on the recipe** — they are the same no matter which machine runs it (a per-machine
throughput factor is `bind_recipe`'s `multiplier`).

```python
Product(Ids.Products.IronOre, Quantity(3))     # solid input
Product("Product_MyFluid",    Quantity(5))     # string id is fine
```

The `port` on a `Product` is only used by the **legacy** one-shot form. In the split
API, ports belong to the *binding*, not the recipe — see `PortMap` below.

## `PortMap` and port routing

Each machine binding decides which machine port each product is routed to. That mapping
is a list of `PortMap(product, port)` — **product id + port letter, no quantity**:

```python
bind_recipe(r, Ids.Machines.ExhaustScrubber,
    duration = Duration.FromSec(30),
    ports = [
        PortMap(Ids.Products.SteamLo,       "S"),
        PortMap(Ids.Products.CarbonDioxide, "G"),
    ])
```

Port letters are machine-specific. Read them from the machine's port view in the recipe
editor, or from `Data/machine_ports.json`, which CustomAssets regenerates on every game
load with the real port names and types. For example the Exhaust scrubber exposes
`E`=loose + `A B C D`=fluid inputs and `W Z`=loose + `X Y`=fluid outputs.

A selector can name **more than one port** — it's one character per port, with no
separator. `"AB"` means the product may use port A *or* B, which is how you hook a
high-throughput input up to two pipes. And **`"*"`** means "the first free compatible
slot", resolved at bind time — the same as leaving the product out of `ports=`
altogether, just written down explicitly. `"*"` can't be combined with letters
(`"A*"` is rejected).

```python
ports = [
    PortMap(Ids.Products.Exhaust,   "AB"),  # multi-port: may arrive on A or B
    PortMap(Ids.Products.Limestone, "A"),   # pinned to one port
    PortMap(Ids.Products.Sand,      "*"),   # first free compatible port
]
```

> **Outputs take at most one multi-port product per type.** Two fluid outputs both on
> `"*"` (or both on `"XY"`) is ambiguous — the game can't tell which goes where, and
> rejects the recipe. So outputs are normally pinned to a single letter each; multi-port
> selectors earn their keep on inputs.

Products you don't list auto-resolve the same way, and **outputs of the same type are
automatically spread across distinct ports** — so a recipe with two fluid outputs on a
two-fluid-port machine just works. Pinning explicitly is still the safer habit for
anything non-trivial: it's deterministic and survives a machine gaining extra ports
later.

> The visual recipe editor always writes a **complete** `ports=[PortMap(...)]` list
> (one entry per product) so a binding is fully self-describing. Hand-written calls may
> list only the ports they care about.

`PortMap` can only reference ports the machine already has. If you're **adding** a port
(via `add_ports=` / the port-list editor), that port doesn't exist on the running
session's proto — it is registered on the next game load. Save, restart, then bind to it.
See [When changes take effect](MODDING.md#when-changes-take-effect).

## The `with` form

For hand-authoring, a `with build_recipe(...) as r:` block reads nicely: the
`bind_recipe(...)` calls inside attach to the recipe implicitly, so you don't repeat it.

```python
with build_recipe(
        recipeId    = "CustomRecipe_Widget",
        name        = "Widget",
        description = "",
        ingredients = [Product("Product_Steel", Quantity(2))],
        products    = [Product("Product_Widget", Quantity(1))]) as r:
    bind_recipe("AssemblyT1", duration = Duration.FromSec(60))
    bind_recipe("AssemblyT2", duration = Duration.FromSec(30), multiplier = 2)
```

Notes:
- Inside the block the **machine is the only positional argument** to `bind_recipe`;
  pass everything else (`duration`, `ports`, …) by name.
- `as r` is optional. Add it only if you need to reference the recipe elsewhere (an
  `add_unlock_recipe`, or an explicit `bind_recipe(r, …)` outside the block); otherwise
  write `with build_recipe(...):`.
- The explicit form `bind_recipe(r, "Machine", …)` still works anywhere, inside or
  outside a `with` block.

## Binding to several machines

The whole point of the split: define once, bind many. Each binding has its own duration
and (optionally) throughput multiplier, so tiers can differ.

```python
r = build_recipe(
    recipeId = "CustomRecipe_LithiumExtraction",
    name = "Lithium extraction", description = "",
    ingredients = [Product(Ids.Products.Brine, Quantity(32)), Product(Ids.Products.Acid, Quantity(4))],
    products    = [Product("Product_Lithium", Quantity(4)),   Product(Ids.Products.Salt, Quantity(4))])

bind_recipe(r, Ids.Machines.ChemicalPlant,  duration = Duration.FromSec(60))
bind_recipe(r, Ids.Machines.ChemicalPlant2, duration = Duration.FromSec(30), multiplier = 2)
```

## Unlocks

Gate a recipe behind research with `add_unlock_recipe(research, machine, recipe)`, or as
a convenience pass `research=` to `bind_recipe` (it wires the unlock for that
recipe+machine pair):

```python
bind_recipe(r, Ids.Machines.ExhaustScrubber,
    duration = Duration.FromSec(30),
    research = Ids.Research.ExhaustFiltration)
```

A recipe unlock also **grants the machine** by default. Pass `unlock_machine = False`
(CustomAssets >= 0.4.2) when the recipe goes onto a machine the player already has —
otherwise the node hands over that machine, and the game locks it until the node is
researched, because it derives its game-start locked set from these same unlock entries:

```python
bind_recipe(r, Ids.Machines.ExhaustScrubber,
    duration = Duration.FromSec(30),
    research = Ids.Research.ExhaustFiltration,
    unlock_machine = False)
```

See [MODDING.md](MODDING.md#a-recipe-unlock-also-grants-the-machine--unlock_machine--false-opts-out-042).

## Legacy one-shot form

The pre-0.3.0 form — passing `machine=` (and `duration=`, `research=`) directly to
`build_recipe` — still works and is what the bundled sample packs use. It builds the
recipe **and** binds it to that one machine in a single call:

```python
build_recipe(
    recipeId    = "CustomRecipe_AirFilterIL_Mixing_T2",
    name        = "Filter media mixing (T2)",
    description = "",
    machine     = Ids.Machines.IndustrialMixerT2,
    duration    = Duration.FromSec(30),
    ingredients = [Product(Ids.Products.IronOre, Quantity(2)), Product(Ids.Products.Limestone, Quantity(6))],
    products    = [Product("Product_FilterMediaIronLime", Quantity(8))],
)
```

Prefer the split form for new content — it's clearer and binds to multiple machines
without copy-pasting. Opening a legacy recipe in the visual editor and saving it
upgrades it to the split form.

## Reference

The signatures below match `src/Recipes/CustomAssets/__init__.py` (your IDE's
IntelliSense source).

#### Build recipe `build_recipe`

Registers a machine-less recipe. `machine`/`research`/`duration` are **legacy** — only
used when `machine` is supplied (the one-shot form). Prefer `bind_recipe` for machines.

#### Bind recipe `bind_recipe`

Attaches `recipe` to `machine`. `duration` is per-machine. `ports` is a list of
`PortMap(product, port)`. `multiplier` scales all quantities on this machine.
`minPartialUtilization` (percent) lets the machine start a partial cycle. `research`
optionally wires the unlock, and `unlock_machine` (default `True`) decides whether that
unlock also grants the machine. Inside `with build_recipe(...)` the `recipe` may be
omitted (machine first).

#### Port assignment `PortMap`

One product → machine-port assignment for `bind_recipe`'s `ports`. Product id + port
letter; no quantity.

```python
edit_recipe(recipe, duration=None, ingredients=[], products=[], machine=None, research=None, power=None)
```

Edit an existing recipe in place. `duration`/`machine` now (re)bind to a machine (0.3.0:
duration is a property of the binding). Use sparingly on vanilla recipes.
