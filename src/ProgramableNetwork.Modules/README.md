# Writing custom modules, swap groups and templates

This folder holds the Python that defines Programable Network's modules. Each `.py` file in
**`Custom/`** is parsed and registered when the mod loads. `Core/` and `Mafi/` are **stub
libraries** (imports + type hints for your editor) — don't edit them; add your content under
`Custom/`.

> The Python here is read by a **custom parser**, not CPython. Only `from Core.*` and
> `from Mafi.*` imports work — no standard library, no `pip` packages. Keep each file
> self-contained.

## Contents
- [File layout & loading](#file-layout--loading)
- [Imports](#imports)
- [Defining a module](#defining-a-module)
- [The runtime `self` API](#the-runtime-self-api)
- [Growable pins (extensions)](#growable-pins-extensions)
- [Swap groups](#swap-groups)
- [Templates](#templates)
- [Deprecations](#deprecations)

---

## File layout & loading

- Put one or more classes per file in `Custom/`. File name is up to you.
- Load order is **pass 1**: every `SwapGroup` definition in every file (see
  [Swap groups](#swap-groups)); **pass 2**: every `Module`, then every `Template`. So a swap
  group is always available before the modules that join it, no matter which file each lives in.
- A parse error in one file is reported in the game log with the file name; fix and reload.

## Imports

| Import | Provides |
| --- | --- |
| `from Core.module import Module, DefaultControllers, ModuleStatus` | module base class, controller device, `Running`/`Error` |
| `from Core.io import Input, Output, Display` | pin / display constructors |
| `from Core.fields import EntityField, BooleanField, Int32Field, Fix32Field, Int64Field, StringField` | editable fields |
| `from Core.categories import DefaultCategories` | picker categories: `Connection`, `ConnectionRead`, `ConnectionWrite`, `Arithmetic`, `Display`, `Control`, `Boolean` |
| `from Core.mafi import fix, int, raw, hex, min, max, Fix32` | number helpers (`fix(1.5)`, `raw(x)`, `hex(x)`, `min(a, b)`, `max(xs)`) |
| `from Core.swap_groups import SwapGroups, SwapGroup` | built-in group id constants + the group base class |
| `from Core.template import Template, Controller, ControllerTemplate` | template base classes |
| `from Core.ids import Constant, Compare_Int_Greater, ...` | reference any module / Captain-of-Industry id by name |
| `from Core.errors import Exception` | raise parse/runtime errors |
| `from Mafi.<Namespace> import <Type>` | any Captain of Industry type |

## Defining a module

Subclass `Module` and set class attributes. A minimal module:

```python
from Core.module import Module, DefaultControllers, ModuleStatus
from Core.io import Input, Output

class Double(Module):
    name = "Double"                 # shown in the picker
    symbol = "2x"                   # short label drawn on the module
    description = "Outputs <b>a</b> * 2 to <b>c</b>."   # tooltip (simple <b> HTML allowed)
    inputs  = [ Input("a", "A") ]
    outputs = [ Output("c", "C") ]
    categories  = [ DefaultCategories.Arithmetic ]
    controllers = [ DefaultControllers.Controller ]     # required to appear on the controller

    def action(self):
        self.Output.set("c", self.Input.get("a", 0) * 2)
        return ModuleStatus.Running
```

### Attributes

| Attribute | Meaning |
| --- | --- |
| `name`, `symbol`, `description`, `hint` | display name, short label, tooltip, translator note |
| `inputs`, `outputs` | `[ Input("id", "Name"), ... ]` / `[ Output("id", "Name"), ... ]` |
| `fields` | `[ Int32Field("id", "Name", "desc", default), ... ]` — editable in the inspector |
| `displays` | `[ Display(...) ]` — on-module readouts |
| `categories` | one or more `DefaultCategories.*` (controls where it shows in the picker) |
| `controllers` | `[ DefaultControllers.Controller ]` |
| `width` | fixed cell width (otherwise auto) |
| `swap_groups` | groups this module can be swapped within — see [Swap groups](#swap-groups) |
| `input_extensions` / `output_extensions` / `display_extensions` | max growable pins/cells — see [Extensions](#growable-pins-extensions) |
| `deprecates` | ids this module supersedes — see [Deprecations](#deprecations) |

### Methods
- `def action(self):` — runs every tick; return `ModuleStatus.Running` or `ModuleStatus.Error`
  (`Action` also accepted).
- `def display(self):` — updates on-module displays (`Display` also accepted).
- `def Init(self):` — one-time setup when the module is placed.

## The runtime `self` API

Inside `action` / `display` / `Init`:

```python
self.Input.get("id", default)        # pin value (Fix32); default when unwired
self.Input.get_bool("id", False)     # pin as bool (> 0)
self.Output.get("id", default)       # last value written to an output
self.Output.set("id", value)         # write an output
self.Field.get("id", default)        # field value
self.Field.set_int("id", 1)          # write fields: set_int / set_bool / set (Fix32)
self.Info    = True                  # raise an info notification on the controller
self.Warning = True                  # raise a warning notification
```

Numbers are fixed-point `Fix32`; wrap literals with `fix(...)` from `Core.mafi` when mixing
with arithmetic (e.g. `self.Input.get("a", fix(256))`).

## Growable pins (extensions)

Let players add pins from the module's right edge:

```python
class Sum(Module):
    inputs  = [ Input("a", "A"), Input("b", "B") ]
    outputs = [ Output("c", "C") ]
    input_extensions = 6             # up to 6 extra inputs (auto-named c, d, e, ...)

    def action(self):
        total = self.Input.get("a", 0) + self.Input.get("b", 0)
        for pin in self.EffectiveInputs:     # includes active extensions
            ...
```

- `input_extensions` / `output_extensions` — max extra pins; auto-named by continuing the
  alphabet/number of the last static pin.
- `input_extension_names = [...]` / `output_extension_names = [...]` — explicit ids when the
  auto-namer doesn't fit (e.g. `in_1`, `in_2`).
- `link_input_output_extensions = True` — grow both sides in lock-step (paired channels).
- `display_extensions = N` — let the last display widget grow by N cells.
- `extension_displays = [...]` + `extension_displays_link = "input"|"output"` — one display per
  active pin on that side.

## Swap groups

A **swap group** is a set of modules with the *same pin layout* that a player can swap between
in place (the `⇄` button on the module name) without re-wiring. A module joins one or more
groups with `swap_groups`.

### Join a built-in group

```python
from Core.swap_groups import SwapGroups

class Notification_Info(Module):
    inputs = [ Input("in", "State") ]
    swap_groups = [ SwapGroups.Notifications ]
```

Built-in ids on `SwapGroups`: `Arithmetic`, `Comparison`, `Boolean`, `ConvertBridge`,
`ConstantInt`, `DisplayScalar`, `SegDisplayConn`, `SegDisplayArith`, `LogisticsModeSet`,
`LogisticsModeGet`, `Notifications`, `StorageLimitGet`, `StorageFlowSet`, `StorageLogisticsSet`.

### Define your own group

Subclass `SwapGroup` to create a named group, then reference its id from your modules. The
group is created in pass 1, so it exists before any module joins it — the two can even live in
different files.

```python
from Core.swap_groups import SwapGroup
from Core.categories import DefaultCategories

class MyOps(SwapGroup):
    id   = "my_ops"                          # the id modules reference
    name = "My Operations"                   # picker header
    # or, instead of name, reuse a category's label:
    # category = DefaultCategories.Arithmetic

class OpA(Module):
    symbol = "A"
    inputs  = [ Input("a", "A"), Input("b", "B") ]
    outputs = [ Output("c", "C") ]
    swap_groups = [ "my_ops" ]

class OpB(Module):
    symbol = "B"
    inputs  = [ Input("a", "A"), Input("b", "B") ]
    outputs = [ Output("c", "C") ]
    swap_groups = [ "my_ops" ]
```

Notes:
- Give swappable modules **matching pin ids** — the swap keeps cables on pins that still exist
  and drops the rest.
- If a module names a `swap_groups` id that no `SwapGroup` (and no built-in) defined, the group
  is created on first reference using the module's first category as its header.
- A group of a single module hides its swap button (nothing to swap to).

## Templates

Templates are pre-configured modules that appear in the picker's template section. Subclass
`Template` plus the module id (from `Core.ids`) you want to pre-fill, and set fields in
`settings`:

```python
from Core.template import Template
from Core.ids import Constant, Compare_Int_Greater

class Const100(Template, Constant):
    name = "[100]"
    def settings(self):
        self.Field.set_int("number", 100)

class GreaterThan99(Template, Compare_Int_Greater):
    name = "[A>99]"
    def settings(self):
        self.Field.set_bool("field_b", True)   # use the constant field instead of pin b
        self.Field.set_int("b", 99)
```

## Deprecations

When you replace a fixed-arity module with an extensible one, map old ids so existing saves
migrate. Each entry is `("OldId", input_ext, output_ext, display_ext)` (trailing values
optional):

```python
class Sum(Module):
    input_extensions = 6
    deprecates = [
        ("Sum_4", 2),      # old Sum_4 => Sum with 2 input extensions
        ("Sum_8", 6),
    ]
```
