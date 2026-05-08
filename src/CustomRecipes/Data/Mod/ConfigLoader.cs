using System;
using System.Collections.Generic;
using System.IO;
using Mafi;

namespace CustomAssets.Data.Mod {

    /// Reads &lt;modBasePath&gt;/config.json (schema-style: each top-level key is a field with
    /// {"default": ..., "is_integer": ..., ...}) and exposes a flat name→value dictionary.
    /// Currently only the 'default' value is surfaced. Future: overlay player-set values
    /// from the in-game settings UI when the integration is in place.
    internal static class ConfigLoader {

        public static Dictionary<string, object> Load(string modBasePath) {
            var configPath = Path.Combine(modBasePath, "config.json");
            var values = new Dictionary<string, object>();
            if (!File.Exists(configPath)) return values;

            string text;
            try { text = File.ReadAllText(configPath); }
            catch (Exception ex) {
                Log.Warning($"ConfigLoader: cannot read {configPath}: {ex.Message}");
                return values;
            }

            object parsed;
            try { parsed = MiniJson.Parse(text); }
            catch (Exception ex) {
                Log.Warning($"ConfigLoader: malformed JSON in {configPath}: {ex.Message}");
                return values;
            }

            if (!(parsed is Dictionary<string, object> root)) {
                Log.Warning($"ConfigLoader: {configPath} root must be an object.");
                return values;
            }

            foreach (var pair in root) {
                if (pair.Key.StartsWith("$")) continue; // skip $schema and similar metadata
                if (!(pair.Value is Dictionary<string, object> field)) continue;
                if (!field.TryGetValue("default", out var defaultValue)) continue;
                values[pair.Key] = NormalizeNumber(defaultValue, field);
            }

            Log.Info($"ConfigLoader: loaded {values.Count} field(s) from {configPath}");
            return values;
        }

        private static object NormalizeNumber(object raw, Dictionary<string, object> field) {
            // MiniJson returns long for integer literals and double for fractional literals.
            // If the field declares is_integer=false, coerce a long default to double so Python sees a float.
            if (raw is long l && field.TryGetValue("is_integer", out var isInt) && isInt is bool b && !b) {
                return (double)l;
            }
            return raw;
        }
    }
}
