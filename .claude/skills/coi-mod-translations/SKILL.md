---
name: coi-mod-translations
description: Use this skill when working on UI text in this Captain of Industry mod (Mafi framework) and translations are showing English even though `Translations/<lang>.json` exists. Covers the LocStr-snapshot problem, how to wire new UI text correctly, and the existing rebind/LaterText infrastructure.
---

# Mafi LocStr translations in this mod

This codebase has solved a tricky load-order problem with the Mafi localization framework. When working on UI in this project, you must understand it or you'll regress translations.

## The core problem

`Mafi.Localization.LocStr` (and `LocStr1`, `LocStr2`, `LocStr3`, `LocStr4`, `LocStr1Plural`, ...) all hold their translated text in **fields**, not properties. The fields are populated **at construction**:

```csharp
public static readonly LocStr X = Loc.Str("Key", "English text", "");
//                                ^^^^^^^^^ snapshots s_data["Key"] right now
```

If `s_data` doesn't have `"Key"` yet (e.g. because mod translations haven't been spliced in), the LocStr is born with English in its frozen `TranslatedString` and **never updates** later, even after translations load.

COI's mod loader calls `LocalizationManager.ScanForStaticLocStrFields(modAssembly)` at load time, which force-runs every `static readonly LocStr` cctor in the assembly **before** `ModDefinition`'s constructor runs. So any LocStr field declared in `NewTr` (or any class loaded by the framework's scan) is already frozen-English by the time we get a chance to splice translations.

## What this codebase does about it

Two pieces of infrastructure, both in `src/ProgramableNetwork/Data/Translate/`:

### 1. `ModTranslations.Load(manifest)` — splice + rebind

Called from `ModDefinition`'s **constructor** (not `RegisterPrototypes` — too late). It:

1. Reads `Translations/<lang>.json` from the mod root.
2. Splices each entry into `LocalizationManager.s_data` via reflection.
3. Calls `LocalizationManager.ScanForStaticLocStrFields(...)` to force-init any cctors that haven't fired yet (those will pick up correct translations naturally).
4. Calls `RebindStaticLocStrs(...)` — walks every `static` field in the mod assembly whose type is in `Mafi.Localization` and has both an `Id` string field plus one or more other string fields (= translation slots), looks up `s_data[Id]`, reads the `ImmutableArray<string>` translation array on `LocData`, and copies `array[i]` into translation slot `i` via reflection. Boxed-write-back, so it works for both struct and class LocStr types.

**Don't move this call.** It must stay in the constructor:

```csharp
public ModDefinition(ModManifest manifest) : base(manifest) {
    ModTranslations.Load(manifest); // first thing
    Log.Info($"{nameof(ProgramableNetwork)}: constructed");
}
```

### 2. `LaterTextExtensions` — defer text application until first show

Even with rebind, **UI components built before rebind ran already captured the wrong text** into their internal display state. `LaterText` is the workaround:

```csharp
new Label()
    .LaterText(() => NewTr.Inspector.ComputingSpeed, this)  // host = window/inspector
    .TextAlign(TextAlignment.LeftMiddle)
```

The lambda is invoked once at registration (initial paint, may be English) and **once on first `OnShow`** of the host (by which time rebind has run, so the LocStr is correct). The list is cleared after first show — no more re-evaluation.

Available overloads:
- `Label.LaterText(Func<LocStrFormatted>, UiComponent host)` — chainable.
- `Label.LaterText(Func<LocStr>, UiComponent host)` — same, accepting LocStr-returning lambdas.
- `T.LaterText<T>(Func<LocStr>, UiComponent host, Action<T, LocStrFormatted> setter)` — for non-Label components (e.g. `(d, v) => d.Tooltip(v)` for a Display).

## Rules when adding new UI text

1. **Prefer declaring `static readonly LocStr` fields *inside the UI class that uses them*** rather than in `NewTr`. The class's cctor fires only when its first usage forces it, which is well after `ModTranslations.Load` runs in the ctor — so the LocStr is born already-translated.

   ```csharp
   public class MyDialog : FloatingColumn {
       private static readonly LocStr Title = Loc.Str("ProgramableNetwork_MyDialog_Title", "Configure", "");
       // ...
   }
   ```

2. **If you must use `NewTr.X`** (because the string is shared across files), wrap each UI consumer with `LaterText(() => NewTr.X, this)`. The rebind should make this work, but `LaterText` is the safety net for any window built before rebind completed.

3. **Never** call `.Value(NewTr.X.TranslatedString)` directly on a UI component during construction — that captures whatever the frozen string was at construction time.

4. **Don't convert `NewTr.X` fields to lambda properties.** The user has explicitly rejected this approach. The rebind is the supported fix.

5. **Don't skip the `Translations/en.json` step.** Run the `pn_exportTranslations` console command in-game to regenerate `en.json` after adding new `Loc.Str(...)` calls. Then translators copy it to `cs.json` etc. and translate.

## How to verify

When the mod runs with a non-English language, look in the log for:

```
[ProgramableNetwork] Loaded N translations for 'cs-CZ' from '...cs.json'
[ProgramableNetwork] Rebound M static LocStr fields (K keys not in s_data)
```

`M` should match the number of `static readonly LocStr*` fields in this assembly that have a corresponding entry in `<lang>.json`. `K` is the count of fields whose Id has no translation entry — those stay English (expected).

If you add a `LaterText(...)` call, the log will also show:

```
[LaterText] Registered host=<TypeName>#<id> ...
[LaterText] OnShow fired host=<TypeName>#<id> pending=<n>
[LaterText] OnShow finished host=<TypeName>#<id> cleared
```

If you see Registered but no `OnShow fired`, the host you passed doesn't dispatch `OnShow` — pick a different anchor (the immediate parent panel, or `this` on a window class).

## Pointers to the existing code

- `src/ProgramableNetwork/Data/Translate/ModTranslations.cs` — Load + RebindStaticLocStrs.
- `src/ProgramableNetwork/Data/Translate/LaterTextExtensions.cs` — LaterText extension and registry.
- `src/ProgramableNetwork/Data/Translate/NewTr.cs` — central LocStr declarations (legacy; prefer per-class declarations for new code).
- `src/ProgramableNetwork/ModDefinition.cs` — where `ModTranslations.Load(manifest)` is called from the ctor.
- `src/ProgramableNetwork/Translations/en.json`, `cs.json` — translation files.
- Example of `LaterText` usage in a real inspector: `src/ProgramableNetwork/Data/Computer/ControllerInspector.cs` (search for `.LaterText`).