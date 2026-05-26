using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CustomAssets.Data.Mod {

    /// <summary>
    /// Minimal JSON writer that serializes nested
    /// <c>Dictionary&lt;string, object&gt;</c> / <c>List&lt;object&gt;</c> /
    /// primitives. Matches what <see cref="MiniJson"/> produces so a parse →
    /// mutate → write round-trip is safe.
    ///
    /// Output is pretty-printed with 2-space indentation to keep manifest.json
    /// diffable when modders inspect it after the editor saves. Keys are emitted
    /// in the dictionary's enumeration order, which for ordinary
    /// <c>Dictionary&lt;,&gt;</c> is insertion order in practice — sufficient for
    /// our use case where we read the file, tweak one or two lists, and write
    /// back.
    /// </summary>
    internal static class MiniJsonWriter {

        public static string Write(object value) {
            StringBuilder sb = new StringBuilder();
            writeValue(sb, value, 0);
            return sb.ToString();
        }

        private static void writeValue(StringBuilder sb, object v, int indent) {
            if (v == null) { sb.Append("null"); return; }
            switch (v) {
                case string s: writeString(sb, s); return;
                case bool b: sb.Append(b ? "true" : "false"); return;
                case int i: sb.Append(i.ToString(CultureInfo.InvariantCulture)); return;
                case long l: sb.Append(l.ToString(CultureInfo.InvariantCulture)); return;
                case double d: sb.Append(d.ToString("R", CultureInfo.InvariantCulture)); return;
                case float f: sb.Append(f.ToString("R", CultureInfo.InvariantCulture)); return;
                case Dictionary<string, object> obj: writeObject(sb, obj, indent); return;
                case List<object> arr: writeArray(sb, arr, indent); return;
                default:
                    // Unknown types fall back to a quoted string of ToString().
                    // Safer than crashing — the modder gets readable output
                    // even if some weird value snuck in via the parser.
                    writeString(sb, v.ToString());
                    return;
            }
        }

        private static void writeObject(StringBuilder sb, Dictionary<string, object> obj, int indent) {
            if (obj.Count == 0) { sb.Append("{}"); return; }
            sb.Append("{\n");
            int i = 0;
            string pad = new string(' ', (indent + 1) * 2);
            foreach (KeyValuePair<string, object> kvp in obj) {
                sb.Append(pad);
                writeString(sb, kvp.Key);
                sb.Append(": ");
                writeValue(sb, kvp.Value, indent + 1);
                if (i < obj.Count - 1) sb.Append(',');
                sb.Append('\n');
                i++;
            }
            sb.Append(new string(' ', indent * 2)).Append('}');
        }

        private static void writeArray(StringBuilder sb, List<object> arr, int indent) {
            if (arr.Count == 0) { sb.Append("[]"); return; }
            sb.Append("[\n");
            string pad = new string(' ', (indent + 1) * 2);
            for (int i = 0; i < arr.Count; i++) {
                sb.Append(pad);
                writeValue(sb, arr[i], indent + 1);
                if (i < arr.Count - 1) sb.Append(',');
                sb.Append('\n');
            }
            sb.Append(new string(' ', indent * 2)).Append(']');
        }

        private static void writeString(StringBuilder sb, string s) {
            sb.Append('"');
            if (s == null) { sb.Append('"'); return; }
            foreach (char c in s) {
                switch (c) {
                    case '\\': sb.Append("\\\\"); break;
                    case '"':  sb.Append("\\\""); break;
                    case '\n': sb.Append("\\n");  break;
                    case '\r': sb.Append("\\r");  break;
                    case '\t': sb.Append("\\t");  break;
                    default:
                        if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4"));
                        else sb.Append(c);
                        break;
                }
            }
            sb.Append('"');
        }
    }
}
