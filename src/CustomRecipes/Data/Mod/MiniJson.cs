using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CustomAssets.Data.Mod {

    /// Minimal JSON reader producing nested Dictionary&lt;string, object&gt; / List&lt;object&gt; / primitives.
    /// Sufficient for config.json (small constrained schema). Throws FormatException on malformed input.
    /// Numbers are returned as long (no decimal) or double (with decimal).
    /// Avoids pulling Newtonsoft/System.Text.Json into the runtime.
    internal static class MiniJson {

        public static object Parse(string text) {
            int i = 0;
            SkipWs(text, ref i);
            var result = ReadValue(text, ref i);
            SkipWs(text, ref i);
            if (i < text.Length) throw new FormatException($"Trailing junk at index {i}.");
            return result;
        }

        private static object ReadValue(string s, ref int i) {
            SkipWs(s, ref i);
            if (i >= s.Length) throw new FormatException("Unexpected end of input.");
            char c = s[i];
            if (c == '{') return ReadObject(s, ref i);
            if (c == '[') return ReadArray(s, ref i);
            if (c == '"') return ReadString(s, ref i);
            if (c == 't' || c == 'f') return ReadBool(s, ref i);
            if (c == 'n') return ReadNull(s, ref i);
            if (c == '-' || (c >= '0' && c <= '9')) return ReadNumber(s, ref i);
            throw new FormatException($"Unexpected character '{c}' at index {i}.");
        }

        private static Dictionary<string, object> ReadObject(string s, ref int i) {
            i++; // {
            var dict = new Dictionary<string, object>();
            SkipWs(s, ref i);
            if (i < s.Length && s[i] == '}') { i++; return dict; }
            while (true) {
                SkipWs(s, ref i);
                var key = ReadString(s, ref i);
                SkipWs(s, ref i);
                if (i >= s.Length || s[i] != ':') throw new FormatException($"Expected ':' at index {i}.");
                i++;
                dict[key] = ReadValue(s, ref i);
                SkipWs(s, ref i);
                if (i >= s.Length) throw new FormatException("Unexpected end inside object.");
                if (s[i] == ',') { i++; continue; }
                if (s[i] == '}') { i++; return dict; }
                throw new FormatException($"Expected ',' or '}}' at index {i}.");
            }
        }

        private static List<object> ReadArray(string s, ref int i) {
            i++; // [
            var list = new List<object>();
            SkipWs(s, ref i);
            if (i < s.Length && s[i] == ']') { i++; return list; }
            while (true) {
                list.Add(ReadValue(s, ref i));
                SkipWs(s, ref i);
                if (i >= s.Length) throw new FormatException("Unexpected end inside array.");
                if (s[i] == ',') { i++; continue; }
                if (s[i] == ']') { i++; return list; }
                throw new FormatException($"Expected ',' or ']' at index {i}.");
            }
        }

        private static string ReadString(string s, ref int i) {
            if (s[i] != '"') throw new FormatException($"Expected string at index {i}.");
            i++;
            var sb = new StringBuilder();
            while (i < s.Length && s[i] != '"') {
                char c = s[i];
                if (c == '\\') {
                    i++;
                    if (i >= s.Length) throw new FormatException("Unterminated escape.");
                    char esc = s[i++];
                    switch (esc) {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            if (i + 4 > s.Length) throw new FormatException("Bad \\u escape.");
                            sb.Append((char)int.Parse(s.Substring(i, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                            i += 4;
                            break;
                        default: throw new FormatException($"Bad escape '\\{esc}'.");
                    }
                } else {
                    sb.Append(c);
                    i++;
                }
            }
            if (i >= s.Length) throw new FormatException("Unterminated string.");
            i++; // closing "
            return sb.ToString();
        }

        private static object ReadNumber(string s, ref int i) {
            int start = i;
            if (s[i] == '-') i++;
            while (i < s.Length && s[i] >= '0' && s[i] <= '9') i++;
            bool isFloat = false;
            if (i < s.Length && s[i] == '.') { isFloat = true; i++; while (i < s.Length && s[i] >= '0' && s[i] <= '9') i++; }
            if (i < s.Length && (s[i] == 'e' || s[i] == 'E')) {
                isFloat = true;
                i++;
                if (i < s.Length && (s[i] == '+' || s[i] == '-')) i++;
                while (i < s.Length && s[i] >= '0' && s[i] <= '9') i++;
            }
            string token = s.Substring(start, i - start);
            return isFloat
                ? (object)double.Parse(token, CultureInfo.InvariantCulture)
                : long.Parse(token, CultureInfo.InvariantCulture);
        }

        private static bool ReadBool(string s, ref int i) {
            if (i + 4 <= s.Length && s.Substring(i, 4) == "true") { i += 4; return true; }
            if (i + 5 <= s.Length && s.Substring(i, 5) == "false") { i += 5; return false; }
            throw new FormatException($"Bad bool at index {i}.");
        }

        private static object ReadNull(string s, ref int i) {
            if (i + 4 <= s.Length && s.Substring(i, 4) == "null") { i += 4; return null; }
            throw new FormatException($"Bad null at index {i}.");
        }

        private static void SkipWs(string s, ref int i) {
            while (i < s.Length && (s[i] == ' ' || s[i] == '\t' || s[i] == '\r' || s[i] == '\n')) i++;
        }
    }
}
