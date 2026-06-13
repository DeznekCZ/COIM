---
name: csharp-serialized-class
description: Enforce the project convention for serializable Mafi entities — every saved class is marked [ManuallyWrittenSerialization], the serialization code lives in a sibling partial file (e.g. Foo.Serialization.cs), and every [InitAfterLoad] method sits in the main file directly after the constructor block, with its InitPriority and [OnlyForSaveCompatibility] attribute kept intact. Use when adding serialization, switching from generated to manual serializers, or tidying a class that mixes save/load code with logic.
---

# C# Serialized Class Convention

This is the project's house rule for any Mafi entity / command / data class that participates in save/load.

## The three rules

1. **Attribute**: the type is annotated `[ManuallyWrittenSerialization]` (from `Mafi.Serialization`). Never `[GenerateSerializer(...)]`.
2. **File split**: the type is declared `partial`, with all serialization code in a sibling file named exactly `<TypeName>.Serialization.cs`. The main file stays focused on state, behaviour, and lifecycle.
3. **Init-after-load placement**: every method marked `[InitAfterLoad]` lives in the **main file**, positioned **immediately after** the constructor block — before any other instance method. Only attach `[OnlyForSaveCompatibility]` if the method exists *purely* to handle a deprecated save shape (so it can be greppable for cleanup later); a normal cache-rebuild init does not need it.

## What goes in which file

**`<TypeName>.cs` (main file)** — keep:
- `using` directives needed for state / behaviour
- type declaration with `[ManuallyWrittenSerialization]` and the `partial` keyword
- fields, properties, events
- constructors (and `static` ctor / finalizer)
- **all `[InitAfterLoad]` methods**, immediately after the constructor block
- normal methods, operators, indexers
- nested types

**`<TypeName>.Serialization.cs` (partial sibling)** — move:
- the two `s_serializeDataDelayedAction` / `s_deserializeDataDelayedAction` static delegate fields
- `public static void Serialize(<Type> value, BlobWriter writer)`
- `public static <Type> Deserialize(BlobReader reader)`
- `protected override void SerializeData(BlobWriter writer)`
- `protected override void DeserializeData(BlobReader reader)`
- only the `using` directives those methods actually need

## How to apply

1. **Pick the target.** If `$ARGUMENTS` is a path, use it. Otherwise use the IDE-selected file. If neither is available, ask.
2. **Confirm it's a serialised type.** Look for any of: `[GenerateSerializer]` attribute, `Serialize` / `Deserialize` static methods, `SerializeData` / `DeserializeData` overrides, `[ManuallyWrittenSerialization]`. If none are present and the type doesn't extend a serialised base (`Entity`, `LayoutEntity`, `Vehicle`, `InputCommand`, etc.), it isn't a save-participating class — abort and tell the user.
3. **Switch the attribute** on the type declaration: `[GenerateSerializer(...)]` → `[ManuallyWrittenSerialization]`. Add the `Mafi.Serialization` using if missing.
4. **Make the type `partial`** if it isn't already.
5. **Create or update `<TypeName>.Serialization.cs`** sibling file:
   - Same namespace as the main file.
   - `partial` declaration with no base list (the main file owns the bases) and no attributes (`[ManuallyWrittenSerialization]` stays on the main file's declaration only, declared once).
   - Move the two static delegate fields, both static `Serialize`/`Deserialize` factories, and both `SerializeData`/`DeserializeData` overrides.
   - Trim the main file's usings to what the remaining code needs; copy only the necessary subset to the serialization file.
6. **Reposition `[InitAfterLoad]` methods** in the main file so they appear directly after the constructor block, before any other method. Preserve the `InitPriority` argument if non-default, but drop a redundant `(InitPriority.Normal)`. Only carry `[OnlyForSaveCompatibility]` if it was already there *and* the method clearly exists for backwards-save handling — otherwise drop it. If a method registered via `RegisterInitAfterLoad(this, nameof(...))` lacks `[InitAfterLoad]`, add the bare `[InitAfterLoad]` and tell the user.
7. **Verify the build** with the project's existing build command (e.g. `dotnet build ... -clp:ErrorsOnly`). On failure, report the specific error rather than guessing.

## Conventions inside the serialization file

- Use the standard delegate-pair pattern:
  ```csharp
  private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
      delegate (object obj, BlobWriter writer) { ((TypeName)obj).SerializeData(writer); };
  private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
      delegate (object obj, BlobReader reader) { ((TypeName)obj).DeserializeData(reader); };
  ```
- `Serialize` and `Deserialize` factories use `TryStartClassSerialization` / `TryStartClassDeserialization` and enqueue the delayed actions.
- `SerializeData` / `DeserializeData` start with `base.SerializeData(writer)` / `base.DeserializeData(reader)` and write/read fields in matched order. Always serialize a version int at the top so future migrations have a hook.
- Late-init: at the end of `DeserializeData`, register any `[InitAfterLoad]` callbacks via `reader.RegisterInitAfterLoad(this, nameof(method), InitPriority.X)`.

## Field and property serialization attributes

Even though serialization is manual, individual fields and properties may still carry serialization-related attributes — they live with the declaration in the main file and must be preserved exactly. Common examples:

- `[DoNotSave(0, null)]` — exclude this field/property from saves (typical for caches, runtime-resolved references like `m_proto`, or fields rebuilt by `[InitAfterLoad]`).
- `[GenerateSerializer]` on a nested struct/value type used as a field's type — stays on that type's declaration; do not strip it.
- Any other Mafi-provided marker attribute on a field or property.

Rules:
- Keep these attributes attached to the field/property in the main file. Never move them to the serialization partial.
- Don't add or remove `[DoNotSave]` while applying this skill — the persistence intent of each field is up to the author. If a field looks like it should be `[DoNotSave]` (e.g. it's a runtime cache rebuilt in `[InitAfterLoad]`), flag it to the user instead of changing the attribute silently.
- When `SerializeData` / `DeserializeData` are written manually, fields marked `[DoNotSave]` must NOT appear in either method body. If you find one that does, report the inconsistency.
- **Do not pass default arguments to serialization attributes — use the bare form.** All Mafi serialization attributes have defaults for every parameter, so the no-arg form is preferred:
  - `[DoNotSave]` instead of `[DoNotSave(0, null)]`
  - `[InitAfterLoad]` instead of `[InitAfterLoad(InitPriority.Normal)]`
  Only specify an argument when its value differs from the default — e.g. `[InitAfterLoad(InitPriority.Late)]` or `[DoNotSave(removedInSaveVersion: 3)]`.
- **`[OnlyForSaveCompatibility]` is not part of the standard pattern.** Only use it when the annotated method/field exists *solely* to handle a deprecated save shape so it can be greppable for cleanup later. Don't attach it to a routine `[InitAfterLoad]` cache rebuild.
- **Mark `[DoNotSave]` on every field that is just a cached copy of prototype data and never mutated at runtime.** Examples: a `MaintenanceCosts` field set from `Prototype.Costs.Maintenance` in the constructor or in `[InitAfterLoad]`, a `m_proto`/`m_protoId` resolver pair, an `IEntityMaintenanceProvider` resolved from DI, the `BucketWheelDesignationManager` reference. These don't need persistence — they can be re-derived from the prototype or injected service after load.
  - Distinguish from fields that are *initialized* from the prototype but then mutated (e.g. `m_armHeight = prototype.ArmPitchMaxDeg;` followed by `stepAngleToward(ref m_armHeight, ...)` later). Those are state, not copies — they MUST be serialized.
  - When in doubt, grep the field name for any non-constructor write site. If there's at least one, it's state. If not, it's a copy → `[DoNotSave]`.
- **Exception — object version constants.** When you DO need to pass a save-version-related argument (e.g. `removedInSaveVersion`, or the version int written by `SerializeData`), prefer a named constant on the type over a magic literal so the version is explicit and greppable. **The version constants live in the serialization partial file** (`<TypeName>.Serialization.cs`), not the main file — they're a save/load concern. Both the main file (for `[DoNotSave(removedInSaveVersion: SAVE_VERSION_2)]`) and the serialization file (for `writer.WriteInt(SAVE_VERSION)`) reference them through the partial class.
  ```csharp
  // In <TypeName>.Serialization.cs
  public partial class TypeName
  {
      private const int SAVE_VERSION = 2;
      // historical versions stay around for use by [DoNotSave(removedInSaveVersion: ...)]:
      private const int SAVE_VERSION_1 = 1;
      ...
      protected override void SerializeData(BlobWriter writer)
      {
          base.SerializeData(writer);
          writer.WriteInt(SAVE_VERSION);
          ...
      }
  }
  ```
  ```csharp
  // In <TypeName>.cs
  public partial class TypeName : BaseType
  {
      [DoNotSave(removedInSaveVersion: SAVE_VERSION_1)]
      private int m_oldField;
  }
  ```

## What NOT to do

- Don't leave `[GenerateSerializer]` anywhere on a serialised type once converted.
- Don't put `[InitAfterLoad]` methods in the serialization partial file — they belong with the constructor in the main file.
- Don't drop or change the `InitPriority` value on an existing init method — preserve it.
- Don't touch a file the project memory marks as protected; report it instead.
- Don't merge `Serialize` / `Deserialize` factories with `SerializeData` / `DeserializeData` — keep both pairs.