using System.Collections.Generic;
using Mafi;
using Mafi.Collections;
using Mafi.Serialization;

namespace ProgramableNetwork.Python
{
    // Serialiser for PlcPy's per-instance scratch context — the
    // Dict<string, object> that holds player-defined variables across the
    // init: → run: handoff and across run: ticks.
    //
    // The context is a fully dynamic Python-style scope, so the serialiser
    // round-trips a *whitelist* of types and silently drops anything else.
    // The drop-on-save behaviour is acceptable here because:
    //   1. Non-serialisable values (Constructors, Type, ModuleWrapper, …) are
    //      rebuilt by PlcPy on every tick anyway — they live on the context
    //      transiently as system bindings, never as player-mutable state.
    //   2. Player-authored variables that hold non-serialisable values
    //      (lists, dicts, complex wrappers) are typically initialised in
    //      init:, which re-runs after any non-Running result, so a save/
    //      reload simply triggers a fresh init rather than corrupting the
    //      script.
    //
    // Layout: WriteInt(version) + WriteInt(count) + `count` (key, type tag,
    // value) triplets.  The leading version int is the serializer's own
    // schema version — distinct from `Controller.MODULE_PLC_CONTEXT`.  The
    // module-level version gates the *presence* of the context block; this
    // inner version gates the *layout* of the entries within that block,
    // so we can introduce a new tag (e.g. lists) or rework the entry shape
    // without bumping the module-level constant.
    //
    // Type tags are stable — any new tag must use a fresh number, never
    // reuse a removed one (same rule as the Module version constants).
    public static class PlcContextSerializer
    {
        // Type tags.  Append-only; never reuse a removed tag.  Backed by
        // byte so the on-disk size stays one byte per entry header (the
        // explicit `: byte` matters — without it the underlying type would
        // default to int and bloat every entry by 3 bytes).
        public enum TypeTag : byte
        {
            Null   = 0,
            Bool   = 1,
            Int    = 2,
            Long   = 3,
            Float  = 4,
            Double = 5,
            String = 6,
            Fix32  = 7,
        }

        // Schema version for the body that follows the leading int.  Bump
        // (and gate the read) whenever the layout changes — adding a new
        // TypeTag is *not* a layout change (existing entries still parse
        // identically), but adding a new field per entry would be.
        //
        // V1: WriteInt(count) + count × (string key, byte tag, value bytes)
        public const int VERSION_V1 = 1;
        public const int CURRENT_VERSION = VERSION_V1;

        // Always writes a version int + count int even when ctx is
        // null/empty so the on-disk shape is identical regardless of
        // whether the player's script declared any vars.  Keeps the
        // version-gating in Module.cs simple ("if v >= N: read context")
        // without a sub-flag for emptiness.
        public static void Serialize(Dict<string, object> ctx, BlobWriter writer)
        {
            writer.WriteInt(CURRENT_VERSION);

            // First pass: count entries we're actually going to write so the
            // count int matches the body.  Skipping during the second pass
            // would desync count and entries.
            int writable = 0;
            if (ctx != null)
            {
                foreach (KeyValuePair<string, object> kv in ctx)
                {
                    if (kv.Key != null && IsSerialisable(kv.Value))
                    {
                        writable++;
                    }
                }
            }
            writer.WriteInt(writable);
            if (writable == 0) {
                return;
            }

            foreach (KeyValuePair<string, object> kv in ctx)
            {
                if (kv.Key == null || !IsSerialisable(kv.Value)) {
                    continue;
                }
                writer.WriteString(kv.Key);
                WriteValue(writer, kv.Value);
            }
        }

        // Returns a fresh dict — caller assigns to Module.PlcContext.  The
        // dict is empty when count is 0; never returns null so the run path
        // can always read/write without a null-check.
        public static Dict<string, object> Deserialize(BlobReader reader)
        {
            int version = reader.ReadInt();
            // Future-proofing: if a save was written by a NEWER build with
            // a layout this build doesn't understand, surface a clear error
            // rather than corrupting the rest of the byte stream.  Reading
            // a v1 dict on a future v2 build is the case we'd handle by
            // adding `if (version >= V2) ...` here.
            if (version < VERSION_V1 || version > CURRENT_VERSION)
            {
                throw new System.InvalidOperationException(
                    "PlcContextSerializer: unknown context version " + version
                    + " (this build supports up to v" + CURRENT_VERSION + ")");
            }

            int count = reader.ReadInt();
            Dict<string, object> result = new Dict<string, object>();
            for (int i = 0; i < count; i++)
            {
                string key = reader.ReadString();
                object value = ReadValue(reader);
                result[key] = value;
            }
            return result;
        }

        // True for values the whitelist supports.  The system bindings PlcPy
        // installs on the context (Constructor, Type) are NOT in the
        // whitelist, so they're skipped when the player saves — a fresh
        // PlcPy.Action call will reinstall them on the next tick.
        public static bool IsSerialisable(object value)
        {
            return value is null
                || value is bool
                || value is int
                || value is long
                || value is float
                || value is double
                || value is string
                || value is Fix32;
        }

        private static void WriteValue(BlobWriter writer, object value)
        {
            switch (value)
            {
                case null:
                    WriteTag(writer, TypeTag.Null);
                    return;
                case bool b:
                    WriteTag(writer, TypeTag.Bool);
                    writer.WriteBool(b);
                    return;
                case int i:
                    WriteTag(writer, TypeTag.Int);
                    writer.WriteInt(i);
                    return;
                case long l:
                    WriteTag(writer, TypeTag.Long);
                    writer.WriteLong(l);
                    return;
                case float f:
                    WriteTag(writer, TypeTag.Float);
                    writer.WriteFloat(f);
                    return;
                case double d:
                    WriteTag(writer, TypeTag.Double);
                    writer.WriteDouble(d);
                    return;
                case string s:
                    WriteTag(writer, TypeTag.String);
                    writer.WriteString(s);
                    return;
                case Fix32 fx:
                    WriteTag(writer, TypeTag.Fix32);
                    Fix32.Serialize(fx, writer);
                    return;
            }
            // Unreachable — IsSerialisable gate filters non-whitelisted types
            // before WriteValue is called.
            throw new System.InvalidOperationException(
                "PlcContextSerializer.WriteValue: unsupported type " + value.GetType().Name);
        }

        private static object ReadValue(BlobReader reader)
        {
            TypeTag tag = (TypeTag)reader.ReadByte();
            switch (tag)
            {
                case TypeTag.Null:   return null;
                case TypeTag.Bool:   return reader.ReadBool();
                case TypeTag.Int:    return reader.ReadInt();
                case TypeTag.Long:   return reader.ReadLong();
                case TypeTag.Float:  return reader.ReadFloat();
                case TypeTag.Double: return reader.ReadDouble();
                case TypeTag.String: return reader.ReadString();
                case TypeTag.Fix32:  return Fix32.Deserialize(reader);
            }
            // An unknown tag indicates either a save corruption or a forward
            // compat scenario where a newer build wrote a tag this build
            // doesn't know about.  Throwing keeps the byte stream from
            // desyncing; the deserializer's outer try/catch surfaces a clear
            // load error rather than silently corrupting downstream data.
            throw new System.InvalidOperationException(
                "PlcContextSerializer.ReadValue: unknown type tag " + (byte)tag);
        }

        // Single byte on disk — the enum is `: byte` so the cast is a noop
        // at IL level.  Centralised here so the WriteValue cases above all
        // emit the same shape and a future shared-prelude (e.g. flags)
        // stays in one spot.
        private static void WriteTag(BlobWriter writer, TypeTag tag)
        {
            writer.WriteByte((byte)tag);
        }
    }
}
