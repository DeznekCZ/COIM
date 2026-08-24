# CustomAssets — Modder's Guide

A short, practical reference for adding products, recipes, materials, prefabs, machines, research and toolbar categories to **Captain of Industry** through the **CustomAssets** framework.

Everything you ship lives in **Python definition files** that the framework parses on load. No C# required for content packs.

> **🚧 Coming soon — in-game GUI mod builder.** The workflow described below uses an editor + command-line build today. A separate **GUI tool** is planned that will run alongside (or inside) the game and let modders compose products, recipes, research and toolbars visually, then emit the same `Definitions/*.py` files this guide documents. The Python definitions and the underlying API will stay stable across the transition — anything you author by hand today will still build and run when the GUI ships. See [Roadmap](#roadmap) for the current status.

---

## Table of contents

1. [What is a pack](#what-is-a-pack)
2. [Quick start](#quick-start)
3. [Creating a new project from scratch](#creating-a-new-project-from-scratch)
4. [Pack layout](#pack-layout)
5. [`manifest.json`](#manifestjson)
6. [How definitions are loaded](#how-definitions-are-loaded)
7. [Pack configuration — `config.json`](#pack-configuration--configjson)
8. [PythonAPI dialect — gotchas vs. CPython](#pythonapi-dialect--gotchas-vs-cpython)
9. [Defining items step by step](#defining-items-step-by-step)
10. [Authoring images and meshes](#authoring-images-and-meshes)
11. [API reference](#api-reference)
    - [Products](#products) — loose, unit, fluid
    - [Visuals](#visuals) — materials, prefabs, textures
    - [Recipes](#recipes) — `build_recipe` + `bind_recipe`, `PortMap` (see [RECIPES.md](RECIPES.md))
    - [Research & toolbars](#research--toolbars)
    - [Machines](#machines)
    - [Entity costs](#entity-costs) — re-pricing machines, buildings, vehicles, trains
    - [Crops](#crops) — retuning growth, yield and soil demands
    - [Unlocks](#unlocks)
    - [Existence checkers](#existence-checkers)
12. [Build & deploy](#build--deploy)
13. [Distribution layout](#distribution-layout)
14. [Worked example — canned corn](#worked-example--canned-corn)
15. [When changes take effect](#when-changes-take-effect)
16. [Troubleshooting](#troubleshooting)
17. [Roadmap](#roadmap)

---

## What is a pack

A **pack** is a self-contained mod that depends on the **CustomAssets** library mod for its runtime. Each pack:

- Ships as a single `.zip` placed in `%APPDATA%\Captain of Industry\Mods\`.
- Declares its dependency on `CustomAssets >= <version>` in its `manifest.json`.
- Adds content (products, recipes, materials, prefabs, ...) by writing **Python definition files** under `Definitions/`.
- May ship raw assets (PNG textures, OBJ meshes) under `Assets/` that its definitions reference.

You can author one pack with everything, or split related content into multiple packs that depend on each other.

---

## Quick start

Scaffold a new pack with `ModBuilder`:

```powershell
ModBuilder.exe new MyMod_Foo --display-name "Foo Industry [MyMod]" --author "YourName"
```

This produces:

```
MyMod_Foo/
├── manifest.json
├── Definitions/
│   └── example.py
└── .gitignore
```

Edit `Definitions/example.py`, then build:

```powershell
ModBuilder.exe build MyMod_Foo
```

…which compiles a small DLL, deploys an unpacked copy to the game's `Mods\MyMod_Foo\` folder for testing, and produces `MyMod_Foo_<version>.zip` ready for distribution.

---

## Creating a new project from scratch

Skip this section if you used `ModBuilder.exe new` above — it already gave you a complete project. This section covers the manual route for when you want to integrate a pack into an existing repo or copy from a sibling pack.

### Prerequisites

| Tool | Purpose | Notes |
|---|---|---|
| **.NET 8 SDK** (or newer) | Building everything | `dotnet --version` should report 8.x or 9.x. |
| **PowerShell 7+** | Running build scripts on Windows | Pre-installed on modern Windows. |
| **Visual Studio 2022** *(optional)* | IDE with one-click build, debugger, IntelliSense for both C# and Python | The free **Community** edition is enough. Install with **".NET desktop development"** + **"Python development"** workloads. Rider works as a fallback. |
| **VS Code** *(alternative)* | Lighter-weight editor | Install the **C#**, **Python** and **C# Dev Kit** extensions. |
| **GIMP / Photoshop / Krita** | Painting textures | Any image editor that exports PNG with alpha. |
| **Blender** *(optional)* | Authoring `.obj` meshes | Only needed if you want custom geometry beyond the default boxes. |

### Two environment variables

Set these once for your machine — every build script reads them.

| Variable | Value | What it points at |
|---|---|---|
| `COI_ROOT` | `C:\Program Files (x86)\Steam\steamapps\common\Captain of Industry` | The game install directory. The build needs the managed DLLs under `Captain of Industry_Data\Managed\`. |
| `COI_MODS` | `%APPDATA%\Captain of Industry\Mods` | The folder the game reads at startup. The post-build step copies your pack here so you can test without manually copying every iteration. |

Set them in **System Properties → Environment Variables** (Windows), or via PowerShell:

```powershell
[Environment]::SetEnvironmentVariable('COI_ROOT', 'C:\Program Files (x86)\Steam\steamapps\common\Captain of Industry', 'User')
[Environment]::SetEnvironmentVariable('COI_MODS', "$env:APPDATA\Captain of Industry\Mods", 'User')
```

Restart your terminal / VS / Rider after setting them.

### Creating the pack folder

Inside this repo (or any sibling layout that has a `CustomAssetPack/Packs/` parent), create a folder for your pack:

```
src/CustomAssetPack/Packs/MyMod_Foo/
├── manifest.json
└── Definitions/
    └── example.py
```

Easiest path: copy an existing pack folder (e.g. `CannedCorn/`) and rename. Then edit `manifest.json` (change `id`, `display_name`, `version`, `description_*`, `primary_dlls`) and replace `Definitions/*.py` with your content.

The `BuildPack` MSBuild target in `src/CustomAssetPack/CustomAssetPack.csproj` auto-discovers every subfolder under `Packs/` that has a `manifest.json`. There's no global registry to update — drop in a folder, build, you're done.

### IDE setup for IntelliSense

Once you have the **CustomAssets** library mod installed (in `%APPDATA%\Captain of Industry\Mods\CustomAssets\`), it ships an `API/` folder with typed Python stubs for every function and id documented in this guide. Point your IDE there:

- **Visual Studio** (Python Tools): `Tools → Options → Python → Search Paths → Add` →
  `%APPDATA%\Captain of Industry\Mods\CustomAssets\API`
- **VS Code** (Pylance): add to your workspace `settings.json`:
  ```jsonc
  {
    "python.analysis.extraPaths": [
      "${env:APPDATA}/Captain of Industry/Mods/CustomAssets/API"
    ]
  }
  ```
- **Rider**: `File → Settings → Languages & Frameworks → Python → Interpreter Paths → Add`.

After this, typing `from Mafi.Base import Ids` and then `Ids.Products.<Tab>` lists every vanilla product id with type info; `from CustomAssets import build_recipe` shows the docstring inline.

---

## Pack layout

```
MyMod_Foo/
├── manifest.json                      # required — pack metadata
├── config.json                        # optional — runtime config defaults
├── Definitions/                       # required — your Python definition files
│   ├── recipes.py
│   ├── products.py
│   └── …
├── Assets/                            # optional — raw assets referenced by Definitions/
│   ├── Models/   <name>.obj           # custom meshes (Wavefront .obj)
│   ├── Products/ <name>.png           # textures
│   └── Icons/    <name>.svg           # icons
└── AssetBundles/                      # optional — Unity AssetBundles, only if you author in Unity
```

The runtime walks `Definitions/*.py` in alphabetical order and executes each file. Use `dependencies("other_file")` (without `.py`) at the top of a file when you need a deterministic load order.

---

## `manifest.json`

```json
{
  "id": "MyMod_Foo",
  "display_name": "Foo Industry [MyMod]",
  "version": "0.1.0",
  "description_short": "Adds foo and a recipe to the basic Assembler.",
  "description_long": "Longer description shown in the in-game mod manager. Markdown is **not** rendered.",
  "authors": ["YourName"],
  "mod_dependencies": [
    "CustomAssets>=0.1.9"
  ],
  "can_add_to_saved_game": true,
  "can_remove_from_saved_game": true,
  "non_locking_dll_load": true,
  "min_game_version": "0.8.4",
  "primary_dlls": ["MyMod_Foo.dll"]
}
```

Field cheat-sheet:

| Field | Meaning |
|---|---|
| `id` | Internal pack id. Must match the folder name and DLL name. |
| `display_name` | Shown in the in-game mod manager. |
| `version` | Semver. Used in the zip filename. |
| `mod_dependencies` | Other mods this pack requires. List `CustomAssets>=<min>` so the loader fails fast if the lib is missing. |
| `can_add_to_saved_game` | Set `true` when the pack only adds content (no breaking schema changes). |
| `can_remove_from_saved_game` | `true` if removing this pack mid-save is safe (rare). |
| `non_locking_dll_load` | `true` to allow hot-reload during development. |
| `min_game_version` | Minimum COI version. |
| `primary_dlls` | The DLL ModBuilder generates for this pack. |

---

## How definitions are loaded

The framework discovers every `.py` file under `Definitions/` and runs them through a built-in interpreter. Each file shares one global namespace per pack, so you can define values in one file and reference them in another (subject to load order).

For deterministic ordering across files use the `dependencies()` helper:

```python
# at the top of canned_corn_recipes.py
dependencies("canned_corn_products")    # loads canned_corn_products.py first
```

All built-in API functions (`build_recipe`, `add_unit_prefab`, …) are available without imports — they live in the `CustomAssets` module. The only `import`s you need are for typed values and IDs:

```python
from Mafi import Duration, Quantity
from Mafi.Base import Assets, Ids
from Mafi.Core.Products import CountableProductStackingMode
from CustomAssets import (
    add_unit_prefab,
    build_product_unit,
    build_recipe,
    Product
)
```

`Mafi.Base.Ids` exposes typed handles to every vanilla ID — `Ids.Products.Iron`, `Ids.Machines.AssemblyManual`, `Ids.Recipes.IronPlateProduction`, etc. Always prefer these over raw strings.

`Mafi.Base.Assets` exposes asset paths — `Assets.Base.Products.Icons.FoodPack_svg`, `Assets.Base.Products.Loose.FilterMedia_mat`, etc. **The constant *name* uses underscores (`_mat`, `_svg`); its *value* uses dotted file extensions (`.mat`, `.svg`)**. Don't hand-write these strings — use the typed constant.

---

## Pack configuration — `config.json`

A pack can ship an optional `config.json` next to `manifest.json`. Its fields are read once when the pack loads and handed to your definitions as `config`, so you can **turn parts of a pack on and off without touching the Python**.

### The file

`config.json` is a *schema*: every top-level key is a field (`snake_case`), and its object carries the value plus metadata for tooling. Point `$schema` at `config.schema.json` to get validation and completion in your IDE.

```json
{
    "$schema": "../../config.schema.json",
    "enable_tier2_mixing": {
        "default": true,
        "description": "Register the Industrial Mixer T2 variant of the recipe."
    },
    "scrubber_media_per_batch": {
        "default": 2,
        "is_integer": true,
        "min": 1,
        "max": 8,
        "description": "Filter media consumed by one air-scrubbing batch."
    }
}
```

| Key | Meaning |
|---|---|
| `default` | The value your definitions see. `bool`, `str`, integer or floating-point number. |
| `is_integer` | **Required for numbers.** `true` → the value arrives as an int, `false` → as a float. |
| `description` | Shown in tooling and copied into the generated IntelliSense stub. |
| `min` / `max` / `max_length` / `regex` | Validation metadata for numbers and strings. |
| `editable` | `"always"` (default) or `"on_add"` — see [Fields a player must not change later](#fields-a-player-must-not-change-later). |
| `expression` | Numbers only — compute the value from the pack's other fields. See [Computed fields](#computed-fields). |

### Editing config.json from the game

There are two in-game surfaces, aimed at two different people:

- **Pack Settings** — a window on the game's main toolbar (Ctrl+Shift+O), available in an **ordinary game**, not just the sandbox. One tab per activated mod that exposes settings; values only, written back to each pack's own `config.json`. This is the player-facing panel: it never adds or re-types a field.
- **CFG Config fields** — a button on the pack card inside the Recipe Editor (which is sandbox-gated, so it is the author's tool). Add and remove fields, set each one's kind (bool / int / float / text), default, description, range or format constraints, and editability. Writes the current pack's `config.json`, preserving `$schema` and anything the editor doesn't model. It opens in the editor's main pane — the same area a definition's form uses — and the button stays lit while that pane is showing, next to 🔗 (dependencies and mod info) and TT (translations), which work the same way.

Config field descriptions are picked up by the translations editor (as `config.<field>.description`), alongside the mod's own `display_name` and descriptions from `manifest.json`.

Both are safe to ignore — `config.json` remains a plain file you can edit by hand.

### Fields a player must not change later

A config value that gates content decides which recipes and products get **registered**. Turning one off on a game that already contains that content can leave the save referring to protos that no longer exist. Mark such fields:

```json
"enable_tier2_mixing": {
    "default": true,
    "editable": "on_add",
    "description": "Register the Industrial Mixer T2 variant."
}
```

`"on_add"` means *only meant to be set when the mod is added to a game*. The settings panel renders those fields locked with a per-field unlock, and carries a standing warning that changing a mod's settings can break a save. An absent `editable` key means `"always"`, so existing packs are unaffected.

Use `"always"` (or omit it) for rates, multipliers and names — anything that changes a number without changing what exists.

### Computed fields

A number field can be computed from the pack's **other** config fields instead of being a fixed value:

```json
"scrubber_media_per_batch": { "default": 2, "is_integer": true },
"mixer_t2_output": {
    "default": 8,
    "is_integer": true,
    "expression": "scrubber_media_per_batch * 4"
}
```

- The syntax is the same one you write in `Definitions/*.py`; only this pack's own config fields are in scope.
- Expressions may build on other computed fields. A cycle, an unknown name or a non-numeric result is reported in the log, and the field falls back to its `default` — a broken expression never takes the pack down.
- `default` stays in the file as that fallback, which is also what an older version of the framework reads.
- Integer division applies to two ints, exactly as in a definition file: `5 / 2` is `2`. Write `5 / 2.0` for `2.5`.

This key is authored by hand — the CFG dialog treats a config field as a constant and offers no expression box, because the natural place for a calculation is where the value is *used*. See the next section.

### Expressions in definition fields

A numeric field in a definition — a recipe amount, a duration — can hold an expression instead of a fixed number:

```python
build_recipe(
    ...
    ingredients = [Product(Ids.Products.IronOre, Quantity(config.batch_size))],
    duration    = Duration.FromSec(config.smelt_seconds * 2)
)
```

In the editor those fields carry an **fx** button. It opens a composer with a chip per config field of the pack, the operators, and a number box — click to build the expression, or just type it, including names the composer doesn't know (a variable defined earlier in the same file). A live preview evaluates it against the pack's current config values through the same evaluator the game uses at load time. Clearing the expression returns the field to the plain number it had.

Supported today: **recipe ingredient / product amounts** and **recipe durations** (`build_recipe`, `bind_recipe`, `edit_recipe`).

> **This also fixes a data-loss bug.** Before, the editor could only read a literal from those slots: a hand-written `Quantity(config.batch_size)` loaded as `0` and was written back as `Quantity(0)` the first time you saved that recipe. Expressions now round-trip verbatim.

### Config values are constants

Pack code can read `config` but not write it — `config.enable_x = False` and `config["enable_x"] = False` are refused. A definition file that rewrote settings mid-load would make the settings panel describe a game that isn't running. Copy the value into a variable if you need to derive from it:

```python
batch = config.scrubber_media_per_batch * 2
```

### Reading it from Python

```python
from CustomAssets import build_recipe, config, Product
```

Each field is an attribute: `config.enable_tier2_mixing`, `config.scrubber_media_per_batch`. Values behave exactly like literals written into the file — an integer field compares and multiplies like `2`, a bool like `True`.

The subscript form reads the same value and is the one to use when the field name is computed:

```python
tier = "t2"
if config["enable_" + tier]:
    ...
```

Both forms raise the same error for a field `config.json` does not define.

### Gating definitions

`if` / `elif` / `else` are supported by the interpreter, so a definition simply lives inside a branch. **A `build_*` / `add_*` call in a branch that does not run is never registered** — the product, recipe or machine does not exist in that game at all.

```python
if config.enable_tier2_mixing:
    build_recipe(
        recipeId = "CustomRecipe_AirFilterIL_Mixing_T2",
        machine  = Ids.Machines.IndustrialMixerT2,
        ...
    )
```

Chains work too, and so does gating a value rather than a whole definition:

```python
if config.difficulty == "hard":
    ore_cost = 6
elif config.difficulty == "normal":
    ore_cost = 4
else:
    ore_cost = 2

build_recipe(
    ...
    ingredients = [Product(Ids.Products.IronOre, Quantity(ore_cost))]
)
```

Whole *files* can be skipped the same way — put the `if` at the top and indent the file's body — but prefer one guarded block per definition; it keeps the source readable in the in-game editor, which shows each definition with the condition that guards it.

### Fields that may be missing

Reading a field that `config.json` does not define is an error, and it aborts the whole file. That is what you want for a typo, but not for a field you added in a later version of the pack — a player carrying an old `config.json` would lose everything in that file. For those, use:

```python
media_per_batch = config.get("scrubber_media_per_batch", 2)    # value, or the fallback
if config.has("experimental_recipes"):                          # presence test
    ...
```

A missing field reports what it was looking for and where:

```
config has no field "enable_tier3" — add it to …\CustomAssets_AirFiltering\config.json
or use config.get("enable_tier3", <fallback>). Defined: enable_tier2_mixing, scrubber_media_per_batch.
```

### IntelliSense

`config` is typed in the `CustomAssets` stub, so `config.get(...)` / `config.has(...)` complete in your IDE. StubBuilder additionally writes a `class Config` listing this pack's fields and their defaults into the per-pack stub — that class is documentation only; read values through `from CustomAssets import config`.

> **When changes take effect.** `config.json` is read once, while the pack loads. Editing it — by hand or through either dialog — applies after the save is reloaded, not immediately.

---

## PythonAPI dialect — gotchas vs. CPython

The interpreter is **Python-shaped, not full CPython**. Differences worth knowing:

- ✅ **Multi-line parenthesised `from X import (a, b, c)` works.**
- ❌ **No trailing commas** in function calls, lists, or tuples. `f(a, b,)` and `[1, 2,]` fail. Strip the trailing comma.
- ✅ Named arguments work the same as CPython.
- ✅ Lists, tuples, basic arithmetic, string concatenation work.
- ✅ **Subscripting `x[key]`** — dictionaries such as `config` by name, and lists, tuples and strings by position (negative counts from the end, `items[-1]`). Assignment (`items[0] = …`, `config["k"] = …`) works too. ❌ Slices (`items[1:3]`) are rejected at parse time.
- ✅ `if` / `elif` / `else` work, including around definitions — see [Pack configuration](#pack-configuration--configjson). `with` blocks work for recipe binding (see [RECIPES.md](RECIPES.md)).
- ❌ Comprehensions, decorators, `try`/`except`, loops, async, classes-with-methods, etc. — assume not supported. Outside of conditionals, stick to straight-line code.

If a definition fails to load you'll see a `PythonParseException` in the log pointing at the offending line:

```
[12:0]: Expected token 'name', got: ("    " [indent] at <file>:12:0)
```

That's almost always one of the gotchas above.

---

## Defining items step by step

The general workflow for adding a new product is:

1. **Decide the type.** Loose pile (ore, gravel, sand), unit/countable (cans, microchips, bricks), or fluid (water, oil, gas). Each maps to a different `build_product_*` function and visual.
2. **Author the visuals** (textures, optional `.obj` mesh). See [Authoring images and meshes](#authoring-images-and-meshes).
3. **Register the visual** with `add_loose_product_material` (loose) or `add_unit_prefab` (unit). For fluids only colors are needed — no asset.
4. **Register the product** with the corresponding `build_product_*`, passing the visual you just registered.
5. **Wire recipes** with `build_recipe` so the product can be produced (and consumed) somewhere in the factory chain.
6. *(Optional)* gate behind research with `add_unlock_recipe`/`add_unlock_product` if you want it locked at game start.

Order matters: a recipe references products, products reference materials/prefabs, materials/prefabs reference textures. Within a single `.py` file, do them top-to-bottom in that order. Across multiple files, use `dependencies("...")` to enforce the load order.

### Naming conventions

Pick a short prefix unique to your pack and apply it consistently. The CustomAssets library already uses `Custom*` for vanilla-ish names; pick something different like your mod id.

| What | Convention | Example |
|---|---|---|
| Product id | `Product_<PascalCase>` | `Product_CannedCorn` |
| Recipe id | `<Prefix>_Recipe_<PascalCase>` *or* `CustomRecipe_<PascalCase>` | `MyMod_Recipe_CannedCorn` |
| Research id | `<Prefix>_Research_<PascalCase>` | `MyMod_Research_FoodPreservation` |
| Asset path | `Assets/<PackId>/<Subfolder>/<Name>.<ext>` | `Assets/MyMod/Products/CannedCorn.png` |
| Mat path | `Assets/<PackId>/<Name>_mat` *(no extension; the renderer adds it)* | `Assets/MyMod/CannedCornBox_mat` |
| Prefab path | `Assets/<PackId>/<Name>.prefab` | `Assets/MyMod/CannedCornBox.prefab` |
| Mesh path | `Assets/Models/<Name>.obj` | `Assets/Models/Can.obj` |

Stick to one casing style across all of your ids — the loader is case-sensitive.

### A minimum-viable item — three lines

The absolute shortest path from "no product" to "a product on a conveyor":

```python
from Mafi import Quantity
from Mafi.Base import Ids
from CustomAssets import add_loose_product_material, build_product_loose, build_recipe, Product

mat = add_loose_product_material(
    path   = "Assets/MyMod/MyOre_mat",
    albedo = "Assets/MyMod/Products/MyOre.png"          # drop a 512×512 PNG here
)

build_product_loose(
    productId = "Product_MyOre",
    name      = "My ore",
    icon      = "Assets/MyMod/Icons/MyOre.svg",
    material  = mat,
    isStorable = True
)

# A way to obtain it — a recycling recipe so you can test without authoring a miner.
build_recipe(
    recipeId    = "MyMod_Recipe_MyOreFromScrap",
    name        = "My ore from iron scrap",
    machine     = Ids.Machines.Crusher,
    ingredients = [Product(Ids.Products.IronScrap, Quantity(2))],
    products    = [Product("Product_MyOre", Quantity(3))]
)
```

Three drops (PNG + SVG icon + the .py) and you have a working in-game pile.

### Composing more complex products

Once the minimum-viable shape works, build outward by adding:

- **More recipes** for variant inputs (the canned-corn example produces empty cans from iron *or* steel *or* aluminum).
- **A research gate** — `build_research(...)` then pass `research=` on the recipe (or call `add_unlock_recipe` after the fact).
- **Localized strings** — the `name` and `description` you pass are the English strings; localization wiring is in the broader workstream.
- **Toolbar categorization** — `add_toolbar_category` if your pack adds buildings.

---

## Authoring images and meshes

This section covers the art side: how to produce the PNGs, SVGs and `.obj` files the API consumes.

### Texture formats

| Use | Format | Recommended size | Notes |
|---|---|---|---|
| Loose pile albedo | PNG (RGB or RGBA) | 256×256 or 512×512 | Should be **seamlessly tileable** if you use `tiling > 1`. |
| Unit prefab albedo | PNG (RGB or RGBA) | 256×256 to 1024×1024 | UVs come from your `.obj`, see below. |
| Icon | SVG | Any (vector) | Square aspect. The vanilla COI icons in `Assets.Base.Products.Icons.*` are SVGs and a good size reference. |
| Normal map | PNG | Same as albedo | Optional. Tangent-space, OpenGL-style (G channel up). |
| Metallic / smoothness | PNG | Same as albedo | Channel-packed: R = metallic, A = smoothness. |

Authoring tools that work well:

- **GIMP** (free) — full-featured raster editor.
- **Photoshop** — industry standard.
- **Krita** (free) — strong painting tool.
- **Inkscape** (free) — best free SVG editor for icons.
- **Affinity Designer** — paid, good SVG support.

### Tileable loose-pile textures

Loose products render as a single repeating texture across a large pile. Two requirements:

1. **The image must wrap seamlessly across its edges**. A non-tileable image produces visible vertical/horizontal seams every time the pile crosses a tile-multiple of the texture size.
2. **High-frequency detail looks better than smooth gradients**. The pile shader uses the average of the pixels for the particle (conveyor-spill) color, and any large flat gradient becomes a flat blob.

The simplest way to get a tileable PNG:

- In GIMP: `Filters → Map → Tile…` to repeat the image, paint over the seams in the centre, then crop back.
- In Photoshop: `Filter → Other → Offset…` with a wrap-around offset, then heal-brush the seams.
- In Krita: paint with the **wrap-around mode** enabled (`View → Wrap-around mode`).

If your authored texture is e.g. 256×256 but you want it to look denser, use `tiling=4` on `add_loose_product_material` to repeat it 4×4 inside the pile slice. The product still works without a tileable source — it just visibly seams every 256 px.

### Unit-prefab textures

For unit prefabs, the texture maps to the geometry through the `.obj`'s UV coordinates (`vt` lines). The framework's default box mesh uses a basic (0,0)–(1,1) wrap on each face. The bundled `Can.obj` wraps the side once horizontally (u: 0 → 1) and stretches vertically (v: 0 → 1, base at v=0).

Practical advice:

- **Front-and-centre your label or symbol.** For a cylindrical can, paint the label in a vertical strip 1/4 of the way across the texture; with the side wrapping once around (u: 0–1), the label appears once on the visible side of the can on the conveyor.
- **Author at the resolution your detail demands** — a small 64×64 is enough for a flat-coloured indicator label; 512×512 is the sweet spot for a cylinder-side label with readable text.
- **Use SRGB** for albedo (not linear). PNG defaults to SRGB; only worry about it if your editor offers a colour-space switch.
- **No transparency for surfaces.** Unit-prefab albedos are sampled as RGB. A PNG with alpha is fine but the alpha is ignored on the body. (Use alpha only for icons.)

### Icons

In-game icons are SVG. Always use vanilla icons where possible — they're already styled correctly, ship with the game, and cost nothing to reference:

```python
icon = Assets.Base.Products.Icons.FoodPack_svg          # ✓ recommended
icon = "Assets/MyMod/Icons/CannedCorn.svg"              # custom — only if no vanilla fits
```

When authoring custom icons, match the vanilla style:

- **Square** aspect (typically 64×64 or 128×128 SVG canvas).
- **Outline + flat fill** at low contrast — avoid photorealism; the icons render small.
- **Centered** subject with even padding.
- Open a vanilla icon (extract `Assets.Base.Products.Icons.FoodPack.svg` from the game bundles via Unity tooling, or eyeball the in-game look) and copy its line weight, palette and silhouette density.

### Custom meshes (`.obj`)

Wavefront `.obj` is the only mesh format the framework reads. To export from Blender:

1. **Author your model at metric scale** (1 Blender unit = 1 m). The framework imports vertex coordinates verbatim as Unity meters.
2. **Place the mesh origin at the bottom-center** of the bounding box (so the prefab sits on the conveyor surface).
3. **Apply all transforms** before exporting (`Object → Apply → All Transforms`) — the loader doesn't read transforms, only vertex positions.
4. **Triangulate** (optional but cleaner): `Mesh → Clean Up → Triangulate Faces` (or let the loader fan-triangulate at load time).
5. **UV unwrap** the model (`UV → Unwrap` after selecting all faces in edit mode). Without UVs, the texture renders as a solid colour from `(0,0)`.
6. **Export**: `File → Export → Wavefront (.obj)` with options:
   - ✅ `Selection Only` (if you have other things in the scene)
   - ✅ `Apply Modifiers`
   - ✅ `Include UVs`
   - ⬜ `Write Materials` *(unchecked — the `.mtl` file is ignored; materials come from the API)*
   - **Forward axis**: `-Z`, **Up axis**: `Y` (defaults).
   - ⬜ `Triangulated Mesh` *(optional)*
   - **Path Mode**: `Auto` is fine.

The loader's contract:

| Element | Read? | Notes |
|---|---|---|
| `v` (positions) | ✅ | In meters. |
| `vt` (UVs) | ✅ | If absent, vertices map to UV (0,0). |
| `vn` (normals) | ✅ | If absent, normals are auto-recalculated from face winding. |
| `f` (faces) | ✅ | N-gons fan-triangulated. Negative indices supported. Right-handed CCW automatically reversed to Unity's left-handed CW. |
| `o`, `g`, `s` (groups) | ❌ ignored | Single-mesh import only. |
| `mtllib`, `usemtl` | ❌ ignored | Materials come from `add_unit_prefab`. |

If your model renders inside-out, the OBJ winding is the opposite of expected — re-export with `Forward axis = -Z`.

If your model renders solid black or with shading artefacts, normals weren't authored / exported — easiest fix is to omit `vn` lines from the export and let `RecalculateNormals` compute them.

### Unity AssetBundles (advanced)

Everything above (PNG, SVG, OBJ) is the **lightweight content path** — fast to iterate, no Unity Editor required. For richer content the framework also accepts **Unity AssetBundles**, the same packaging Captain of Industry itself uses for its built-in art.

When to use AssetBundles instead of the lightweight path:

- You need **animated meshes** (skinned mesh + animation clips) — the OBJ loader is single-mesh, no animation.
- You need **shaders** beyond Standard / `Mafi/LooseMaterialPile` — bundles can ship custom HLSL/ShaderLab.
- You need **prefab hierarchies** with multiple GameObjects, particle systems, lights, audio sources.
- You need **post-process volumes**, **scriptable rendering pipeline assets**, etc.
- You're already an experienced Unity author and prefer the Editor's tooling for materials and lighting.

**Authoring** (high-level — full Unity workflow is out of scope for this guide):

1. Open a Unity project that targets the **same Unity LTS the game ships with** (currently Unity 6 LTS — check the game's `Captain of Industry_Data\globalgamemanagers` if unsure). Mismatched versions produce silent breakage at load time.
2. Author your prefabs, materials, meshes, and shaders as normal.
3. Assign each asset to a **named AssetBundle** via the Inspector (bottom-right of the asset's inspector → AssetBundle dropdown).
4. Build the bundle: in Unity, `Assets → Build AssetBundles` after dropping a small build script under `Editor/`. Output goes to a folder of your choosing.
5. **Copy the resulting bundle file** (and its `.manifest` sibling) into your pack's `AssetBundles/` folder.

**Pack layout with bundles:**

```
MyMod_Foo/
├── manifest.json
├── Definitions/*.py
└── AssetBundles/
    ├── mymod_foo                 # the bundle file (no extension)
    └── mymod_foo.manifest        # the bundle's manifest (Unity-generated)
```

**Build behaviour.** When `ModBuilder.exe build` (or the IDE's `BuildPack` MSBuild target) sees a non-empty `AssetBundles/` folder, it switches to **bundle mode** automatically (`--mode auto` is the default; pass `--mode bundle` to force, `--mode generic` to opt out). Bundle mode wires extra `Copy` steps so the bundle files end up under `<deployed mod>/AssetBundles/`.

**Referencing bundle assets from Python.** Assets in a bundle are addressed by the Unity asset path you authored them under. For example, a prefab saved in Unity at `Assets/MyMod/MyMachine.prefab` and assigned to bundle `mymod_foo` is reachable as:

```python
from CustomAssets import build_product_unit
from Mafi.Base import Assets

build_product_unit(
    productId = "Product_MyItem",
    name      = "My item",
    icon      = "Assets/MyMod/Icons/MyItem.svg",
    prefab    = "Assets/MyMod/MyItem.prefab",     # path inside the bundle, used verbatim
    isStorable = True
)
```

You don't need `add_unit_prefab` when the prefab already exists in a bundle — pass the asset path string directly to `prefab=`. The framework's `AssetsDb.TryGetSharedPrefab(path)` resolves it through the loaded bundle.

**Tradeoffs vs. the lightweight path:**

|  | Lightweight (PNG + OBJ) | AssetBundle |
|---|---|---|
| Tools needed | Image editor, optional Blender | Unity Editor + the right LTS |
| Iteration speed | Fast — drop file, reload save | Slow — open Unity, change, rebuild bundle, copy, reload |
| Mesh complexity | Single-mesh single-material | Anything Unity supports |
| Animations | ❌ | ✅ |
| Custom shaders | ❌ | ✅ |
| Multi-GameObject prefabs | ❌ | ✅ |
| Pipeline/post-FX | ❌ | ✅ |
| Repo size | Small | Bundle binaries are large; consider Git LFS |

For the typical "add a product + a couple of recipes + a custom-looking unit prefab" use case, the lightweight path is enough and far easier. Reach for bundles when you hit one of the explicit limitations above.

---

## API reference

The signatures below match `src/Recipes/CustomAssets/__init__.py` (which is also what your IDE picks up for IntelliSense once you have the CustomAssets API folder in your Python path).

### Conventions

- Anywhere a parameter accepts `<Type> | <Type>.ID | str`, you can pass: a typed `Mafi.Base.Ids.*` value, a `Proto.ID(...)` object, or a plain string id like `"Product_Iron"` / `"AssemblyManual"`.
- All linear distances are in **meters**. (1 COI tile = 2 m. 1 Unity unit = 1 m.)
- All durations are `Duration` (use `Duration.FromSec(n)` or `Duration.FromMin(n)`).
- All quantities are `Quantity` (use `Quantity(n)` or pass an `int`).

### Products

#### `build_product_loose(productId, name, icon, material, …) → LooseProductProto`

A pile (loose-aggregate) product like ore or gravel. Renders on conveyors as a continuous heap.

```python
build_product_loose(
    productId    = "Product_FilterMediaIronLime",
    name         = "Filter media (iron + limestone)",
    description  = "...",
    icon         = Assets.Base.Products.Icons.FilterMedia_svg,
    material     = filter_media_mat,                    # from add_loose_product_material
    particleColor = (170, 165, 155),                    # optional; auto-derived from albedo when omitted
    isStorable   = True,
    isRecyclable = False,
    isRough      = True                                 # rocky pile mesh; otherwise smooth/sand-like
)
```

#### `build_product_unit(productId, name, icon, prefab, …) → CountableProductProto`

A countable item like a bucket, plate, or microchip. Renders on conveyors in groups of 1–3 per tile.

```python
build_product_unit(
    productId         = "Product_CannedCorn",
    name              = "Canned corn",
    description       = "...",
    icon              = Assets.Base.Products.Icons.FoodPack_svg,
    prefab            = canned_corn_prefab,             # from add_unit_prefab
    isStorable        = True,
    packingMode       = CountableProductStackingMode.Triangle,
    allowPackingNoise = True,                           # random yaw rotation per packed item
    rotateSecondPackedItem90Degs = False                # alternating-orientation packing
)
```

`packingMode` accepts a `CountableProductStackingMode` enum value or a case-insensitive string (`"Triangle"`, `"Row"`, `"Stacked"`, `"StackedAlternating"`, `"TriangleHorizontal"`, `"Auto"`).

`Auto` picks `Triangle` when both mesh-X and mesh-Z extents are < 0.5 m, otherwise falls through to single-item placement. For circular items the triangle layout leaves only ~15 % gap between items — that's the layout's nature, not a bug.

#### `build_product_fluid(productId, name, icon, …) → FluidProductProto`

A fluid product like water or oil.

```python
build_product_fluid(
    productId            = "Product_MyFluid",
    name                 = "My fluid",
    icon                 = "Assets/MyMod/Icons/MyFluid.svg",
    color                = (50, 120, 200),              # in-pipe color
    transportColor       = (40, 100, 180),
    transportAccentColor = (20, 50, 90),
    isStorable           = True,
    canBeDiscarded       = True
)
```

### Visuals

#### `add_loose_product_material(path, albedo, normals=None, metallic=None, reference=None, tiling=1) → Mat`

Builds a pile-rendering material. Use this *before* `build_product_loose` and pass the result as `material=`.

```python
filter_media_mat = add_loose_product_material(
    path     = "Assets/MyMod/FilterMediaIronLime_mat",
    albedo   = "Assets/Products/FilterMediaIronLime.png",   # mod-relative
    normals  = None,                                        # inherits from reference
    metallic = None,                                        # inherits from reference
    tiling   = 4                                            # repeat the texture 4×4 across the pile slice
)
```

- `albedo`, `normals`, `metallic` accept a single texture or a list. List shape exists for forward compatibility — currently only the first entry is used per product.
- `reference` defaults to `Assets.Base.Products.Loose.FilterMedia_mat` (a vanilla pile carrier). Override if you want to start from a different vanilla material.
- `tiling=N` makes the source PNG repeat N×N times within the array slice. Use a higher value when your authored texture has chunky high-frequency detail. The PNG must be seamless across edges for clean tiling.

#### `add_unit_prefab(path, albedo, normals=None, metallic=None, reference=None, width=0.5, height=0.2, depth=0.5, mesh=None) → Prefab`

Builds a single-GameObject prefab for a unit product. Pass the result as `prefab=` to `build_product_unit`.

```python
empty_cans_prefab = add_unit_prefab(
    path   = "Assets/MyMod/EmptyCansBox.prefab",
    albedo = "Assets/Products/EmptyCansBox.png",
    mesh   = "Assets/Models/Can.obj"                # optional; falls back to a box from width/height/depth
)
```

- All distances in **meters**. Origin at the **bottom-center** so the prefab sits naturally on the conveyor.
- `mesh` is a Wavefront `.obj` (mod-relative). When given, `width`/`height`/`depth` are ignored. The `.obj` must be a single mesh; n-gon faces are fan-triangulated; UVs are honoured if present, normals recalculated otherwise; right-handed CCW winding (standard OBJ) is reversed at load to Unity's left-handed CW.
- `reference` defaults to Unity's Standard shader, which is sufficient for unit products.

#### `add_texture(path, replace=None) → Tex`

Loads a raw texture by path. Used as a building block for the helpers above; rarely called directly.

#### `add_texture_material(path, texture=None, reference=None, shader=None) → Mat`

Generic material builder. Use `add_loose_product_material` instead for pile materials — the generic builder doesn't auto-default the carrier reference.

#### `add_prefab_box(path, texture, width=0.5, height=0.2, depth=0.5) → Prefab`

Legacy prefab builder for single-textured boxes on the Standard shader. Kept for back-compat; prefer `add_unit_prefab`.

### Recipes

As of 0.3.0 a recipe is **machine-less**: `build_recipe(...)` defines the inputs →
outputs, and `bind_recipe(...)` attaches it to each machine with that machine's own
duration and port routing. See **[RECIPES.md](RECIPES.md)** for the full guide
(`with` form, `PortMap`, multi-machine binding, unlocks, and the legacy one-shot form).

```python
# define the recipe, then bind it to one or more machines
r = build_recipe(
    recipeId    = "MyMod_CannedCorn_Sealing",
    name        = "Canned corn",
    description = "Fill empty cans with corn and seal them.",
    ingredients = [Product("Product_EmptyCansBox", Quantity(8)), Product(Ids.Products.Corn, Quantity(8))],
    products    = [Product("Product_CannedCorn", Quantity(8))],
)
bind_recipe(r, Ids.Machines.ChemicalPlant, duration = Duration.FromSec(30))
```

#### Legacy one-shot form

The pre-0.3.0 form still works and is often the quickest to hand-write for a
single-machine recipe: pass `machine=` (and optionally `duration=`, `research=`,
`power=`) straight to `build_recipe`, which builds the recipe **and** binds it to that
one machine in the same call. This is what the bundled sample packs use.

```python
build_recipe(
    recipeId    = "MyMod_CannedCorn_Sealing",
    name        = "Canned corn",
    description = "Fill empty cans with corn and seal them.",
    machine     = Ids.Machines.ChemicalPlant,
    duration    = Duration.FromSec(30),
    ingredients = [
        Product("Product_EmptyCansBox", Quantity(8)),
        Product(Ids.Products.Corn,      Quantity(8))
    ],
    products = [
        Product("Product_CannedCorn",   Quantity(8))
    ],
    research = Ids.Researches.ChemicalPlant,    # optional; locked-on-init when given
    power    = 110                              # optional; percent of base machine power
)
```

To put the same recipe on several machine tiers with the legacy form, copy the call and
change `machine=` — or use the split `bind_recipe(...)` form above, which avoids the
copy-paste. See [RECIPES.md](RECIPES.md) for the full comparison.

#### `edit_recipe(recipe, …)`

Edit an existing recipe in place — change ingredients, products, duration, power, or
attach to a research. Use sparingly; modifying vanilla recipes can interact poorly with
other mods.

```python
edit_recipe(
    recipe   = Ids.Recipes.AnimalFeedFromCorn,
    duration = Duration.FromSec(45),
    products = [Product(Ids.Products.AnimalFeed, Quantity(3))]
)
```

### Research & toolbars

#### `build_research(researchId, name, costs=1, position=(0,0), parents=None, icon=None) → ResearchNodeProto`

```python
build_research(
    researchId = "MyMod_Research_FoodPreservation",
    name       = "Food preservation",
    description = "Unlocks canned-food production lines.",
    costs      = 4,
    position   = (12, 6),
    parents    = [Ids.Researches.FoodIndustry],
    icon       = "Assets/MyMod/Icons/FoodPreservation.svg"
)
```

#### `add_toolbar_category(categoryId, name, icon, parent, entities) → ToolbarCategoryProto`

Creates a build-menu category and assigns the given entities to it.

```python
add_toolbar_category(
    categoryId = "Toolbar_MyMod_Food",
    name       = "Food",
    icon       = "Assets/MyMod/Icons/Food.svg",
    parent     = Ids.ToolbarCategories.Production,
    entities   = [Ids.Machines.ChemicalPlant, Ids.Machines.Bakery]
)
```

### Machines

#### `add_machine(name, entityId, layout, prefabPath, …) → MachineProto`

Adds a new machine. Most modders won't need this — the existing vanilla machines + custom recipes cover ~95 % of use cases. New machines require Unity-authored AssetBundles and entity-layout strings; out of scope for this guide. See the example pack `CustomAssetPack/Packs/<existing pack>/` for a worked-out reference.

### Entity costs

#### `edit_entity_costs(entity, workers, maintenance, …, products, multiplyPercent) → EntityProto`

Changes what an existing entity costs to build. The price lives on `EntityProto`, the shared base of everything buildable, so a single call reaches machines, buildings, **trucks, excavators, tree harvesters, locomotives, cargo wagons and ships** alike — there is no separate call per vehicle type.

Every argument except `entity` is an independent override; omitting one leaves that part of the vanilla cost alone.

```python
# Restate a price outright.
edit_entity_costs(
    entity              = Ids.Vehicles.TruckT3,
    workers             = 1,
    maintenance         = 6.0,
    maintenanceProduct  = Ids.Products.MaintenanceT2,
    products            = [
        Product(Ids.Products.VehicleParts3, Quantity(120)),
        Product(Ids.Products.Rubber, Quantity(90))
    ]
)

# Or just scale what the game already charges. Prefer this for whole-game
# rebalances: it keeps working when a game update retunes the base price.
edit_entity_costs(entity = Ids.Vehicles.ExcavatorT3,      multiplyPercent = 150)  # 1.5x
edit_entity_costs(entity = Ids.Trains.LocomotiveT2Diesel, multiplyPercent = 75)   # 0.75x
```

`multiplyPercent` scales the build materials only — never maintenance, which has its own argument. When both are given, `products` is applied first and then scaled.

Two facets are conditional, because the game only bills them to entities built to pay them:

| Argument | Applies when | Otherwise |
| --- | --- | --- |
| `workers` | the entity is staffed | logged as a warning and skipped |
| `maintenance*` | the entity is maintained | logged as a warning and skipped |

The visual editor hides those rows for a target that can't use them, so you only see fields that will actually take effect.

### Crops

`add_crop(...)` registers a new crop and `clone_crop(...)` seeds one from an existing crop. To change a crop the game **already has** — so every farm growing it and every existing save picks up the new numbers — use:

#### `edit_crop(crop, productProduced, multiplyYieldPercent, …) → CropProto`

Same argument names as `add_crop` / `clone_crop`, so there's only one crop vocabulary. Everything except `crop` is optional and leaves that rate alone when omitted.

```python
# Slower, richer corn.
edit_crop(
    crop               = "Corn",
    growthDurationDays = 120,
    productProduced    = Product(Ids.Products.Corn, Quantity(60))
)

# A light global nerf that keeps working across game updates.
edit_crop(crop = "Wheat",   multiplyYieldPercent = 80)
edit_crop(crop = "Soybean", multiplyYieldPercent = 80)
```

Two things worth knowing:

- `consumedFertilityPercentPerDay` accepts **negative** values, which *replenish* the soil rather than drain it — that's how green-manure crops work, so it is deliberately not clamped.
- A farm's own yield and demands multipliers still apply on top of whatever you set here; they scale these rates rather than replacing them.

### Unlocks

If your recipe needs to be hidden behind a research that's NOT the recipe's `research=`, attach it explicitly:

```python
add_unlock_recipe(research = Ids.Researches.X, machine = Ids.Machines.Y, recipe = Ids.Recipes.Z)
add_unlock_machine(research = Ids.Researches.X, machine = Ids.Machines.Y)
add_unlock_product(research = Ids.Researches.X, product = Ids.Products.Z)
```

#### A recipe unlock also grants the machine — `unlock_machine = False` opts out (0.4.2+)

The `machine` on `add_unlock_recipe` names the machine the recipe **runs on**, and by
default the node hands the player that machine too. Same for the `research=` shortcut on
`build_recipe` / `bind_recipe` / `edit_recipe`. That has always been the behaviour, and
it stays the default so nothing an existing pack does changes meaning.

It is very often not what you want. The game derives its game-start **locked** set from
exactly these unlock entries (`ResearchManager.LockProtosFromResearchTree`), so listing a
machine on a node both grants it on research *and* takes it away until then. Adding one
recipe to a vanilla Flare locks the Flare. And a node that binds several recipes across
several machines hands out every one of those machines.

So: pass `unlock_machine = False` whenever the recipe goes onto a machine the player
already has, which is most of the time.

```python
# Recipe only — the Flare is left exactly as the vanilla tree had it.
add_unlock_recipe(research, Ids.Machines.Flare, Ids.Recipes.FlareFuelGas,
                  unlock_machine = False)

# The default: one node that delivers the machine AND its first recipe.
add_unlock_recipe(research, myMachine, myRecipe)

# Same flag on the research= shortcuts.
bind_recipe(r, Ids.Machines.Flare, research = myResearch, unlock_machine = False)
```

Passing `unlock_machine` requires `CustomAssets >= 0.4.2`; packs pinning an older version
get the machine unlock as they always did. When the machine genuinely is the point of the
node, `add_unlock_machine(research, machine)` next to a recipe-only unlock says the same
thing more plainly.

For anything that is **not** a machine — buildings, trucks, excavators, tree harvesters, locomotives, cargo wagons, ships — use the entity-shaped sibling instead:

```python
add_unlock_entity(research = "MyResearch", entity = Ids.Vehicles.ExcavatorAmphibious)
add_unlock_entity(research = "MyResearch", entity = Ids.Trains.LocomotiveT2Hydrogen)
```

To take something back **off** a node there is a single call, not four — removal matches by prototype id, so it doesn't need the product/machine/entity/recipe split that adding does:

```python
# stop a vanilla node from handing out a product
remove_unlock(research = Ids.Researches.X, target = Ids.Products.Z)

# same call for a machine, a vehicle, a building…
remove_unlock(research = Ids.Researches.X, target = Ids.Machines.Y)

# a recipe is unlocked per machine, so name the machine to drop just that pair
remove_unlock(research = Ids.Researches.X, target = Ids.Recipes.Z, machine = Ids.Machines.Y)
```

`target` and `machine` are **not** resolved against the prototype database, so removing an unlock for something a game update dropped is safe: it's a no-op with a note in the log, not a load error. Same when the node never unlocked the target in the first place. The node's icon for the removed target is cleaned up too, but only once nothing that stayed still needs it — dropping one recipe doesn't strip the machine icon from the node's other recipes on that machine.

Removing a recipe unlock leaves the recipe **bound** to the machine; it only stops the node from unlocking it. To detach the recipe from the machine as well, use `unbind_recipe(machine)` inside `with edit_recipe(recipe):` — that does both.

### Existence checkers

`product_exist(product) → bool` — `True` when the given product id is currently registered. Use it to gate optional content on the presence of products from other mods or specific game versions:

```python
if product_exist("Product_FilterMediaIronLime"):
    build_recipe(...)
```

---

## Build & deploy

Two flows:

### From Visual Studio / Rider

Open `src/CustomRecipes/CustomAssets.sln`. Build the `CustomAssetPack` project — the `BuildPack` MSBuild target invokes `ModBuilder.exe build` for every pack folder under `src/CustomAssetPack/Packs/`, and:

1. Generates a `Generated/<id>/<id>.csproj` per pack from the embedded template.
2. Compiles each pack DLL.
3. Copies the `Definitions/`, `Assets/`, and `manifest.json` into `%APPDATA%\Captain of Industry\Mods\<id>\` for testing.
4. Produces `<id>_<version>.zip` next to it for distribution. The zip wraps everything inside a top-level `<id>/` folder so extracting yields a clean drop-in mod folder.

### From the command line

```powershell
cd src\CustomAssetPack
dotnet build -c Release
```

Same outputs. CI/headless build path.

### Iterating on Python definitions only

The Python files are loaded at runtime — no DLL rebuild needed when you only change `Definitions/*.py` or assets under `Assets/`. Just **copy the changed file directly** into `%APPDATA%\Captain of Industry\Mods\<id>\…` and reload the save (or restart the game).

---

## Distribution layout

The produced zip looks like:

```
MyMod_Foo_0.1.0.zip
└── MyMod_Foo/
    ├── manifest.json
    ├── MyMod_Foo.dll
    ├── Definitions/
    │   └── *.py
    └── Assets/
        └── *
```

Users extract it into `%APPDATA%\Captain of Industry\Mods\` and the game picks it up on next launch.

The **main library mod** (`CustomAssets`) ships with an extra `API/` subfolder containing IntelliSense stubs:

```
CustomAssets/
├── manifest.json
├── CustomAssets.dll
└── API/
    ├── CustomAssets/__init__.py            # the API documented here
    └── Mafi/                               # full typed stubs for Ids, Assets, etc.
        └── …/__init__.py
```

Pack authors point their IDE at this `API/` folder (or at the upstream `src/Recipes/`) for autocomplete on every function and id.

---

## Worked example — canned corn

A complete pack that adds canned corn, made from empty metal cans + corn in the Chemical Plant.

`Definitions/canned_corn.py`:

```python
from Mafi import Duration, Quantity
from Mafi.Base import Assets, Ids
from Mafi.Core.Products import CountableProductStackingMode
from CustomAssets import (
    add_unit_prefab,
    build_product_unit,
    build_recipe,
    Product
)

# Shared mesh; both products differ only in albedo.
CAN_MESH = "Assets/Models/Can.obj"

empty_cans_prefab = add_unit_prefab(
    path   = "Assets/CannedCorn/EmptyCansBox.prefab",
    albedo = "Assets/Products/EmptyCansBox.png",
    mesh   = CAN_MESH
)

canned_corn_prefab = add_unit_prefab(
    path   = "Assets/CannedCorn/CannedCornBox.prefab",
    albedo = "Assets/Products/CannedCornBox.png",
    mesh   = CAN_MESH
)

empty_cans = build_product_unit(
    productId         = "Product_EmptyCansBox",
    name              = "Empty cans",
    description       = "A box of empty open metal cans, ready to be filled and sealed.",
    icon              = Assets.Base.Products.Icons.Iron_svg,
    prefab            = empty_cans_prefab,
    isStorable        = True,
    packingMode       = CountableProductStackingMode.Triangle,
    allowPackingNoise = True
)

canned_corn = build_product_unit(
    productId         = "Product_CannedCorn",
    name              = "Canned corn",
    description       = "Sealed cans of corn — a shelf-stable food product.",
    icon              = Assets.Base.Products.Icons.FoodPack_svg,
    prefab            = canned_corn_prefab,
    isStorable        = True,
    packingMode       = CountableProductStackingMode.Triangle,
    allowPackingNoise = True
)

# Three input variants — copy any of these and change `machine` to add to higher tiers.
build_recipe(
    recipeId    = "CustomRecipe_EmptyCans_FromIron",
    name        = "Empty cans (from iron)",
    description = "Stamp 1 iron sheet into 8 empty cans (boxed).",
    machine     = Ids.Machines.AssemblyManual,
    duration    = Duration.FromSec(20),
    ingredients = [Product(Ids.Products.Iron, Quantity(1))],
    products    = [Product("Product_EmptyCansBox", Quantity(8))]
)
build_recipe(
    recipeId    = "CustomRecipe_EmptyCans_FromSteel",
    name        = "Empty cans (from steel)",
    description = "Stamp 1 steel sheet into 12 empty cans (boxed).",
    machine     = Ids.Machines.AssemblyManual,
    duration    = Duration.FromSec(20),
    ingredients = [Product(Ids.Products.Steel, Quantity(1))],
    products    = [Product("Product_EmptyCansBox", Quantity(12))]
)
build_recipe(
    recipeId    = "CustomRecipe_EmptyCans_FromAluminum",
    name        = "Empty cans (from aluminum)",
    description = "Stamp 1 aluminum sheet into 16 empty cans (boxed).",
    machine     = Ids.Machines.AssemblyManual,
    duration    = Duration.FromSec(20),
    ingredients = [Product(Ids.Products.Aluminum, Quantity(1))],
    products    = [Product("Product_EmptyCansBox", Quantity(16))]
)

# Sealing in the Chemical Plant.
build_recipe(
    recipeId    = "CustomRecipe_CannedCorn_Sealing",
    name        = "Canned corn",
    description = "Fill empty cans with corn and seal them.",
    machine     = Ids.Machines.ChemicalPlant,
    duration    = Duration.FromSec(30),
    ingredients = [
        Product("Product_EmptyCansBox", Quantity(8)),
        Product(Ids.Products.Corn,      Quantity(8))
    ],
    products = [Product("Product_CannedCorn", Quantity(8))]
)
```

The full source is in `src/CustomAssetPack/Packs/CannedCorn/`.

---

## When changes take effect

Not everything you author shows up in the running game right away. The dividing line is **when the game reads the thing you changed**.

Protos — products, machines, entity clones, ports, research nodes, toolbar categories — are registered **once, during load**. The game then builds its build-menu lists, unlock trees, and port layouts from that snapshot and never re-reads them. So a def you add mid-session is written to your pack source correctly, but the current session has no way to notice it.

| What you changed | Visible when |
|---|---|
| Added a port (`ports=` / `add_ports=`) | **After a game restart.** The building's port layout is baked into the proto at registration. |
| Cloned an entity (`clone_*` defs) | **After a game restart.** Build-menu and toolbar lists are assembled at load. |
| Added a product, machine, research node, or toolbar category | **After a game restart.** Same reason — new protos need a registration pass. |
| Edited an existing recipe's numbers (`edit_recipe`) | **After a game restart**, but existing machines keep running; they pick up the new values on the next load. |
| Translations (`Translations/<lang>.json`) | **After a game restart.** |

The in-game editor shows an inline "takes effect after a game restart" note next to the controls this applies to — that note is not an error. Your edit was saved; it just can't appear in a list that was built before you made it.

**Practical loop:** batch your structural edits, save the pack, then restart once — rather than restarting after each individual def.

---

## Troubleshooting

Game logs are at `%APPDATA%\Captain of Industry\Logs\<timestamp>_<id>.log`. Check the latest file when something doesn't work.

| Symptom | Likely cause |
|---|---|
| `Modules was not loaded` `CheckException` | A `Definitions/*.py` file failed to parse — usually a [dialect gotcha](#pythonapi-dialect--gotchas-vs-cpython). The exception above it points at the file and column. |
| `Asset 'X.prefab' (GameObject) was not found in any bundle` | The prefab path you registered doesn't match the path the proto's `Graphics` references. Double-check the `path=` argument on `add_unit_prefab` and that the proto uses the prefab returned from it. |
| `reference material 'X' not found in AssetsDb` | The `reference=` in `add_loose_product_material` resolves to a path that isn't loaded. Use `Assets.Base.Products.Loose.<X>_mat` (typed constant) instead of writing the path manually — the constant names use `_mat` but the actual path is `.mat` (dotted). |
| Pile renders as flat grey instead of your texture | The carrier reference fell through to the Standard shader. Either (a) `reference=` is wrong, or (b) the texture didn't load (check `LoadModTexture` warnings in the log). |
| Unit product renders as a 0.5×0.5×0.5 grey box | `ProductsRenderer` cached the missing-prefab fallback because something failed during prefab build. Check the log for `[CustomUnitPrefabHook]` lines — they'll either confirm the rebuild (`product 'X' slot N rebuilt`) or warn about a reflection mismatch. |
| Cans visually overlap on the conveyor in Triangle packing | `Triangle` is geometrically tight by design. Reduce mesh dimensions, or switch to `Row` / `TriangleHorizontal` for more spacing. |
| Added a port / cloned a machine but it isn't in the in-game list | Expected — protos are registered at load time. See [When changes take effect](#when-changes-take-effect). Save the pack and restart the game. |
| Empty `Mods\` folder after build | The build script logs `Adresář … se zipuje do …` near the end. If you don't see this, the `BuildPack` target didn't run — confirm you built the `CustomAssetPack` project, not just `CustomAssets`. |

If you hit something not in the table, post the log block in the Discord modding channel.

---

## Roadmap

This section captures planned, not-yet-shipped pieces of the modder workflow. The current `Definitions/*.py` API stays the source of truth — anything below is built on top of it, never replacing it.

### In-game / GUI mod builder *(planned)*

The current authoring loop is:

> open Python file in IDE → write definitions → run `dotnet build` → reload the game → check log → repeat

A future tool will let modders do the same thing **without leaving the game** (or with a companion app), through a visual UI:

- **Product editor** — pick the type (loose/unit/fluid), drop a texture, drop a mesh, set name/description/icon, preview in a small viewport.
- **Recipe editor** — pick a machine, drag input/output products onto its ports, set duration/power, preview the resulting recipe card.
- **Research/toolbar editor** — drag-and-drop the tech tree placement and the build-menu category.
- **Live save & reload** — the tool emits `Definitions/*.py` text matching this guide and triggers a reload, so the in-game change is visible within seconds.

The plan is **emit, not replace.** The GUI writes the same Python files an experienced modder would write by hand, so:

- Anything authored today by hand keeps working.
- Anything authored by the GUI is plain readable text — version-controllable, copy-paste-able, hand-editable afterward.
- Advanced features that aren't in the GUI yet stay accessible by editing the `.py` directly.

**Status:** scoped, not yet started. Tracked alongside the other open workstreams (decompile fixes, per-pack localization, this doc). Discord channel is the place to nudge priority.

### Alternative definition languages — YAML / Lua *(planned)*

Today the only supported format is the embedded Python dialect documented in this guide. Two additional front-ends are on the roadmap:

- **YAML** — declarative, no logic. The most common ask from non-programmer modders. The framework would parse a YAML manifest like:

  ```yaml
  products:
    - id: Product_CannedCorn
      type: unit
      name: Canned corn
      icon: Assets.Base.Products.Icons.FoodPack_svg
      prefab:
        mesh: Assets/Models/Can.obj
        albedo: Assets/Products/CannedCornBox.png
      packing: Triangle
  recipes:
    - id: MyMod_Recipe_CannedCorn
      machine: Ids.Machines.ChemicalPlant
      duration: 30s
      ingredients: [{ Product_EmptyCansBox: 8 }, { Product_Corn: 8 }]
      products:    [{ Product_CannedCorn: 8 }]
  ```

  …and translate it into the same C# proto-builder calls the Python interpreter does today. Best for content-only packs (no conditionals).

- **Lua** — full scripting language, sandboxed. Drop-in replacement for the Python dialect for modders who already know Lua. Conditionals, loops, helper functions — the things the embedded Python dialect doesn't fully support.

Both would coexist with Python — the framework would dispatch on file extension (`.py` / `.yaml` / `.lua`) and run all of them through the same underlying API. A pack could mix formats freely.

**Status:** scoped, not yet started. YAML is the higher-priority of the two (broader audience). Lua waits until a sandboxed Lua VM choice is settled (NLua vs. MoonSharp vs. embedded).

### Other in-flight workstreams

- **Per-pack localization wiring.** Today `name` and `description` strings are English only. A planned change routes them through Mafi's localization system so packs can ship `en.po` / `de.po` / `cs.po` etc.
- **Game-decompile parity audit.** Some Mafi internal types we reach into via reflection (`ProductsRenderer.initializeDrawData`, `getPackedOffsets`, etc.) can shift between game patches. A regression-test harness is on the list.
- **Game-stub generation in StubBuilder.** The `game-stubs` subcommand of `StubBuilder` is currently scaffolded but not implemented; today the same job is handled by `src/GenerateIds`. Folding both into one tool is on the list.

If a feature you need isn't here, mention it on the Discord modding channel — that drives the order things get done in.
