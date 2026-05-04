# Programable Network — Mod Overview

## Identity

| Field | Value |
|---|---|
| **Mod id** | `ProgramableNetwork` |
| **Display name** | Programable Network |
| **Version** | `1.0.12d` |
| **Authors** | DeznekCZ |
| **Game** | Captain of Industry |
| **Min game version** | `0.8.4` |
| **Max verified game version** | `0.8.4` |
| **Primary DLL** | `ProgramableNetwork.dll` |
| **Loadable into saves** | Yes (`can_add_to_saved_game: true`) |
| **Hot-reloadable** | Yes (`non_locking_dll_load: true`) |
| **Removal** | Only by replacing with the sanitizer build |
| **License** | See `LICENSE` at repo root |
| **Repo** | `c:\git\COIM_ProgramableNetwork` |

## What it adds

A logic-and-signals layer for Captain of Industry. The mod introduces a
**Controller** building that hosts a grid of **Modules** wired together with
typed cables. Modules read game state, perform arithmetic / boolean / control
logic, and write back commands or signals. Several display and signal-bus
entities round out the toolkit.

### Top-level entities (registered from C#)

- **Controller** — placeable building with a grid layout. Hosts a programmable
  network of modules. Inspector supports Add / Move / Edit modes.
- **Antenna** (AM / FM) — wireless signal bus. Channels carry single values
  (AM) or arrays of borrowed signals (FM) between distant controllers.
- **Display Entity** — colourised lights, 7-segment and 16-segment digit
  displays driven by module outputs.
- **Speaker** — audio cue triggered by signal.

### Module groups

Modules ship in groups, each registered by a `RegisterData` call in
`ModDefinition` and assembled via the `ModuleBuilderStart(...)` fluent API in
`Data/Mod/Modules/Modules.cs` (~2900 LOC). Categories visible to the player:

- **Constants** — integer, hex, product, crop, machine, vehicle…
- **Buttons** — manual triggers / latches.
- **Arithmetic** — sum, subtract, multiply, divide, modulo, etc. (extensible
  pin counts replace older fixed-arity variants — see *Deprecation table*).
- **Stats** — read live stats from buildings, machines, mines, farms, storages.
- **Forks** — fan-out / route signals.
- **Booleans** — and / or / xor / not (extensible).
- **Decisions** — conditional pickers (e.g. flip-flop, latch, memory).
- **Connections** — read / write to game entities (recipes, priorities,
  thermal storage, transports, etc.).
- **Comparation** — int compare (>, =, <, ≥, ≤).
- **Display** — 7-seg / 16-seg, ints, sliders, LEDs, toggles, images.
- **Radio AM / FM** — antenna read/write modules.
- **Variables** — named state slots shared across the controller.
- **Plc** *(its own ModuleGroup)* — runs user-authored Python code per tick
  via `PlcPy*`.
- **Special**: `Game_Pause` (debug-only).

## How user-authored content plugs in

The mod ships a Python "Custom" layer that loads at runtime — players can drop
`.py` files into the mod's `Modules/Custom/` folder to register new modules,
templates, or controller blueprints without compiling C#.

### Custom Python files (current ship)

Located under `src/ProgramableNetwork.Modules/Custom/`:

| File | Purpose |
|---|---|
| `clock.py` | Cyclic time source (configurable period & rollover). |
| `connection_isactive.py` | Reads the active state of a connected entity. |
| `connection_thermalstorage.py` | Reads thermal-storage charge level. |
| `controller_template.py` | "Basic hour clock" — a sample full Controller blueprint with wired-up modules. |
| `delay.py` | Delays a signal by N ticks. |
| `equal_selector.py` | Selects an output channel by equality match. |
| `flipflop.py` | Set/reset latch with extension pins. |
| `latch.py` | One-bit memory latch. |
| `max.py` | Max-of-inputs (extensible). |
| `memory_selector.py` | Indexed memory cell. |
| `notification.py` | Triggers a controller-level notification. |
| `randomizer.py` | Random integer source. |
| `shift.py` | Cyclic shifter — rotates channel inputs onto matching outputs. |
| `template.py` | Reference of a category-favorites + simple constants/displays template. |
| `timer.py` | One-shot / repeating timer. |

### Python "Core" API (read-only — published lib)

Under `src/ProgramableNetwork.Modules/Core/`. **Do not edit** — it's the public
surface other mod devs target.

| File | What it exposes |
|---|---|
| `module.py` | `Module` base class, default controllers, in/out helpers. |
| `template.py` | `Template`, `Controller` blueprint base. |
| `categories.py` | `DefaultCategories` (Connection, Arithmetic, Display, Control, Boolean, …). |
| `display.py` | Display registration helpers. |
| `fields.py` | `Int32Field`, `EntityField`, etc. |
| `io.py` | `Input` / `Output` proto helpers. |
| `errors.py` | Exception types surfaced to the parser. |
| `mafi.py` | Wrapper for Mafi types (e.g. `Fix32`). |
| `translate.py` | `tr(...)` for localized strings. |
| `ids.py` | (generated) IDs for all C#-registered modules. |

Custom `.py` files may import only from `Core.*` and `Mafi.*` — the runtime
parser does **not** support stdlib or third-party modules.

## Deprecation / save compatibility

`ModDefinition.RegisterPrototypes` wires up redirects from removed fixed-arity
prototypes to the surviving extensible ones (set in
`Data/Mod/Modules/Modules.cs`). Examples:

- `Sum_4` → `Sum` + 2 input extensions
- `Sum_8` → `Sum` + 6 input extensions
- `Boolean_And_4` → `Boolean_And_2` + 2 input extensions
- `Boolean_Or_4` → `Boolean_Or_2` + 2 input extensions
- `Display_Int_2/4/8/16` → `Display_Int` + 0/2/6/14 display extensions
- `Runtime_Shift_2/4/7` → `Runtime_Shift` + 0/2/5 input+output extensions

Saves made before the extensible-pin refactor load through this map and the
correct extension counts are restored.

Module serialization version is gated by constants in
`Data/Computer/Controller.cs`:

- `MODULE_EXTENSIONS = 7` — added Input/Output extension counts
- `MODULE_DISPLAY_EXTENSIONS = 8` — added Display extension count

`Module.DeserializeData` reads each int conditionally on these versions; older
saves load with the missing fields defaulted to 0.

## Build / deployment pipeline

`src/ProgramableNetwork/ProgramableNetwork.csproj`:

- Reads version, author and display title from `manifest.json` at build time
  via the embedded `ProcessManifest` MSBuild task.
- Requires env / project vars `COI_ROOT` (game install) and `COI_MODS`
  (`%APPDATA%\Captain of Industry\Mods` by default).
- **`CopyDllsToUnityProject`** — copies the built `.dll`/`.pdb`/`.xml` into
  `..\ExampleMod.Unity\Assets\Dlls\`.
- **`DeployToModsFolder`** — copies into `$(COI_MODS)\$(TargetName)\`:
  `manifest.json`, `thumbnail.png`, `config.json` (optional), the `.dll`,
  `.pdb` (Debug only), AssetBundles, Assets (`.pdn` excluded), Modules,
  Core API, Translations, then zips the whole folder as
  `<TargetName>_<ManifestVersion>.zip`.

## Source layout (high-level)

```
src/
├── ProgramableNetwork/                      C# mod assembly
│   ├── ModDefinition.cs                     entry point
│   ├── manifest.json                        mod metadata
│   ├── thumbnail.png                        mod thumbnail (deployed)
│   ├── ProgramableNetwork.csproj            build + deploy targets
│   ├── Assets/                              icons / images
│   ├── Translations/                        i18n
│   └── Data/
│       ├── Antene/                          AM/FM antenna entity + commands
│       ├── Computer/                        Controller building, view, commands
│       ├── DataBand/                        signal-bus protos & channels
│       ├── DisplayEntity/                   light/seg displays
│       ├── Mod/                             registrators (Modules, Research, etc.)
│       ├── Modules/                         Module base, fields, layout, ports
│       └── Speaker/                         speaker proto
└── ProgramableNetwork.Modules/              Python "Custom" + "Core" runtime
    ├── Core/                                published API (do not edit)
    └── Custom/                              shipped sample modules
```

## Development conventions

These are pulled from the team's living memory and repeated throughout the
codebase — keep an eye out when contributing:

- **`Modules/Core/` and `Modules/Mafi/`** are a **published** Python API for
  other modders. Never modify them. Add features on the C# side or in
  individual `Custom/*.py` files (each Custom file should stay
  self-contained).
- **Python imports**: only `from Core.*` and `from Mafi.*` resolve. The
  runtime parser doesn't support stdlib or third-party modules.
- **Locale strings**: never lambdify `NewTr.*` `LocStr` fields — fix
  frozen-translation bugs via a rebind + `LaterText`, not by converting the
  field to a property. See the `coi-mod-translations` skill.
- **CLI build** is broken (pre-existing C# 14 syntax in `MyExtensions.cs`).
  Build through the IDE.
- **`using UnityEngine;`** is forbidden in Mafi UI files — collides with
  Mafi types. Use `UnityEngine.X` fully qualified inline.
- **Vanilla `Settlement*` types** are housing buildings, not controller
  modules — don't reference them as a pattern for `ModuleProto` work.
- **Transparent `ButtonIcon`** uses `Button.IconOnly`, not a faked
  `Background(SetA(0))`.

## Pointers

- README & install steps: [README.md](../README.md)
- Translation skill: `.claude/skills/coi-mod-translations/SKILL.md`
- COI decompiled cache (when investigating game internals):
  `C:/Users/zdeno/AppData/Local/Temp/coi-src` (Grep) /
  `/tmp/coi-src` (Bash)
