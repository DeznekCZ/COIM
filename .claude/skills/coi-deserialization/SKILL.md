---
name: coi-deserialization
description: Use this skill when adding new persisted fields to Module or Controller in this Captain of Industry mod. Covers the BlobReader/BlobWriter pattern, version-gating rules, the critical "never reuse a version constant" rule, and the Deprecation table for migrating removed module IDs.
---

# Mafi BlobWriter / BlobReader serialization in this mod

This codebase uses Mafi's manual serialization API rather than auto-serialization. Every entity that stores runtime state is decorated with `[ManuallyWrittenSerialization]` and wires up static delegate thunks, then implements `SerializeData` / `DeserializeData` methods.

## The plumbing

```csharp
[ManuallyWrittenSerialization]
public partial class Controller : ... {
    private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
        (obj, writer) => ((Controller)obj).SerializeData(writer);
    private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
        (obj, reader) => ((Controller)obj).DeserializeData(reader);

    protected override void SerializeData(BlobWriter writer) { ... }
    protected override void DeserializeData(BlobReader reader) { ... }
}
```

`Module` (not a Mafi entity, serialized inline by the controller's `Lyst<Module>`) uses the same pattern — static delegates registered on the class, `protected void` methods (not virtual):

```csharp
[ManuallyWrittenSerialization]
public partial class Module {
    private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
        (obj, writer) => ((Module)obj).SerializeData(writer);
    private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
        (obj, reader) => ((Module)obj).DeserializeData(reader);
}
```

## Version stamp

**Every** `SerializeData` writes a version integer as the first meaningful field (after any base-class call):

```csharp
protected override void SerializeData(BlobWriter writer) {
    base.SerializeData(writer);
    writer.WriteInt(/*Version*/ Controller.MODULE_DISPLAY_EXTENSIONS); // always current max
    // ... rest of fields ...
}
```

`DeserializeData` reads it back first and stores it in a field:

```csharp
private int loadedVersion; // [DoNotSave] field — filled at deserialization time

protected void DeserializeData(BlobReader reader) {
    Id = reader.ReadLong();
    m_protoId = reader.ReadString();
    loadedVersion = reader.ReadInt();
    // ...
}
```

## Version constants (Controller.cs)

Version constants live as `public const int` in the inner `Versions` class of `Controller`:

```
MODULE_PYTHON_CODE         = 6   — added PLC cached lexer-node count
MODULE_EXTENSIONS          = 7   — added InputExtensionCount + OutputExtensionCount
MODULE_DISPLAY_EXTENSIONS  = 8   — added DisplayExtensionCount (separate bump from v7!)
```

The reason `DisplayExtensionCount` got its own bump (8 rather than staying at 7):  
In-progress dev saves existed at v7 that had been written with **only** two extension ints. Reading a third int from them would walk past the module's data block and corrupt the byte stream. A new constant forced a clean gate.

## How to add a new persisted field

1. **Define a new constant** — increment from the current highest:
   ```csharp
   public const int MODULE_MY_FEATURE = 9; // describe what changed
   ```

2. **Write it unconditionally** in `SerializeData` (always write current state):
   ```csharp
   writer.WriteInt(/*Version*/ Controller.MODULE_MY_FEATURE); // bump the stamp too
   // ...existing fields...
   writer.WriteInt(MyNewField);
   ```

3. **Gate the read** in `DeserializeData`:
   ```csharp
   if (loadedVersion >= Controller.MODULE_MY_FEATURE) {
       MyNewField = reader.ReadInt();
   } else {
       MyNewField = 0; // safe default for old saves
   }
   ```

### Critical rule: one constant per field group, never reuse

**Never** add two independently-shipped fields under the same version constant. If an old save was written at v8 and you add two fields both gated at `>= 9`, a save from mid-development that has only one of those fields will read the second int from the wrong position and corrupt everything downstream.

The pattern: **bump the constant, write the new stamp, gate the new read**.

## Module.DeserializeData — real annotated excerpt

```csharp
protected void DeserializeData(BlobReader reader) {
    Id            = reader.ReadLong();
    m_protoId     = reader.ReadString();
    loadedVersion = reader.ReadInt();
    IsPaused      = reader.ReadBool();

    // v2+: status enum (before that, always Running)
    Status = loadedVersion >= 2
        ? (ModuleStatus)reader.ReadInt()
        : ModuleStatus.Running;

    if (loadedVersion >= Controller.MODULE_COMPACT_DATA) {
        // v5+: flags byte gates which containers are present
        DataFlags flags = (DataFlags)reader.ReadByte();
        NumberData   = (flags & DataFlags.NumberData)   != 0
            ? Dict<string, int>.Deserialize(reader)
            : new Dict<string, int>();
        // ... other containers ...

        // v6+: PLC lexer-node count (only if CodeMetadata bit set)
        m_lexerNodeCount = (loadedVersion >= Controller.MODULE_PYTHON_CODE
                            && (flags & DataFlags.CodeMetadata) != 0)
            ? reader.ReadInt()
            : 0;

        // v7+: pin extension counts
        if (loadedVersion >= Controller.MODULE_EXTENSIONS) {
            InputExtensionCount  = reader.ReadInt();
            OutputExtensionCount = reader.ReadInt();
        } else {
            InputExtensionCount  = 0;
            OutputExtensionCount = 0;
        }

        // v8+: display extension count (own gate — v7 dev saves didn't have it)
        if (loadedVersion >= Controller.MODULE_DISPLAY_EXTENSIONS) {
            DisplayExtensionCount = reader.ReadInt();
        } else {
            DisplayExtensionCount = 0;
        }
    } else {
        // Legacy pre-v5 path: all dicts written unconditionally, no ArrayData
        NumberData   = Dict<string, int>.Deserialize(reader);
        // ...
    }
}
```

## Deprecation table — renaming or replacing removed module IDs

When a `ModuleProto` is removed (e.g. fixed-arity `Sum_4` replaced by extensible `Sum`), register a migration entry in `Modules.cs` inside `RegisterPrototypes`:

```csharp
// Sum_4 had 4 inputs = Sum base (2) + 2 extensions
Deprecation.RegisterDeprecation(
    new ModuleProto.ID("Sum_4".ModuleId()),
    new ModuleProto.ID("Sum".ModuleId()),
    inputExt: 2);

// Plain id rename, no pin adjustment needed
Deprecation.RegisterDeprecation(
    new ModuleProto.ID("OldName".ModuleId()),
    new ModuleProto.ID("NewName".ModuleId()));
```

`Deprecation.RegisterDeprecation` has two overloads:
- `(deprecated, replacement)` — plain id rename, no extension fixup.
- `(deprecated, replacement, inputExt:, outputExt:, displayExt:)` — also sets extension counts on the migrated module, reproducing the removed module's pin layout.

`Module.initContexts` (fired by `reader.RegisterInitAfterLoad`) consults the table when a save's stored `m_protoId` is not found in `ProtosDb`. If a migration exists, it swaps the prototype and applies the recorded extension counts. If no migration exists, the module is left as a visible phantom tombstone (not silently dropped).

## Pointers

- `src/ProgramableNetwork/Data/Modules/Module.cs` — `SerializeData`, `DeserializeData`, `initContexts` (lines ~398–580).
- `src/ProgramableNetwork/Data/Computer/Controller.cs` — version constants (lines ~55–73), `SerializeData` / `DeserializeData` (lines ~413–513).
- `src/ProgramableNetwork/Data/Mod/Deprecation.cs` — `Deprecation` class, `RegisterDeprecation`, `GetMigration`.
- `src/ProgramableNetwork/Data/Mod/Modules/Modules.cs` — deprecation registrations at top of `RegisterPrototypes` (lines ~60–91).
