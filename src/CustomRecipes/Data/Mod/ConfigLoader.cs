using System;
using System.Collections.Generic;
using System.IO;
using Mafi;

namespace CustomAssets.Data.Mod {

    /// Reads &lt;modBasePath&gt;/config.json (schema-style: each top-level key is a field with
    /// {"default": ..., "is_integer": ..., ...}) and exposes a flat name→value dictionary.
    /// A numeric field may also carry an "expression" computed from the pack's other
    /// fields; see <see cref="ConfigExpressions"/>. Values are edited either by hand or
    /// through the in-game settings panel, which writes the same 'default' slot.
    internal static class ConfigLoader {

        public const string ConfigFileName = "config.json";

        public static ConfigValues Load(string modBasePath) {
            var configPath = Path.Combine(modBasePath, ConfigFileName);
            bool exists = File.Exists(configPath);
            // Carries the path either way so a `config.<field>` miss can say where the
            // field was expected — including the "pack ships no config.json at all" case.
            var values = new ConfigValues(configPath, exists);
            if (!exists) return values;

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

            // Field metadata is needed twice: once now for the literal defaults, and
            // again after the expression pass to re-apply int/float coercion to the
            // computed results.
            var fields = new Dictionary<string, Dictionary<string, object>>(StringComparer.Ordinal);
            var expressions = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var pair in root) {
                if (pair.Key.StartsWith("$")) continue; // skip $schema and similar metadata
                if (!(pair.Value is Dictionary<string, object> field)) continue;
                if (!field.TryGetValue("default", out var defaultValue)) continue;
                fields[pair.Key] = field;
                values[pair.Key] = NormalizeNumber(pair.Key, defaultValue, field);
                if (field.TryGetValue("expression", out var rawExpression)
                        && rawExpression is string expression
                        && !string.IsNullOrWhiteSpace(expression)) {
                    expressions[pair.Key] = expression;
                }
            }

            // Expressions see the literal defaults of every other field, so a computed
            // field can build on a plain one (and on another computed one). A field that
            // fails to resolve keeps its 'default' — a broken expression must degrade to
            // the authored fallback, not take the whole pack down.
            ConfigExpressions.ResolveAll(values, expressions, (name, error) =>
                Log.Warning($"ConfigLoader: field '{name}' expression failed in {configPath} "
                    + $"({error}); using its default instead."));

            foreach (var pair in expressions) {
                if (values.TryGetValue(pair.Key, out object computed)) {
                    values[pair.Key] = NormalizeNumber(pair.Key, computed, fields[pair.Key]);
                }
            }

            Log.Info($"ConfigLoader: loaded {values.Count} field(s) from {configPath}"
                + (expressions.Count > 0 ? $" ({expressions.Count} computed)" : ""));
            return values;
        }

        /// MiniJson returns long for integer literals and double for fractional ones, but the
        /// interpreter understands neither: Expressions.__fix__ / __float__ / __int__ accept
        /// int and float only, so a long or double config value throws
        /// "Can not convert to __fix__" the moment it is used in an `if` or in arithmetic.
        /// Coerce to exactly what the same number written as a .py literal would produce
        /// (NumberConstant: int when it fits, else float), so config values behave like
        /// hand-written constants. is_integer=false forces the float form.
        private static object NormalizeNumber(string name, object raw, Dictionary<string, object> field) {
            bool declaredFloat = field.TryGetValue("is_integer", out var isInt) && isInt is bool b && !b;

            if (raw is long l) {
                if (!declaredFloat && l >= int.MinValue && l <= int.MaxValue) {
                    return (int)l;
                }
                if (!declaredFloat) {
                    Log.Warning($"ConfigLoader: field '{name}' default {l} does not fit in an integer; exposing it as a float.");
                }
                return (float)l;
            }

            if (raw is double d) {
                return declaredFloat || d != Math.Floor(d) || Math.Abs(d) > int.MaxValue
                    ? (object)(float)d
                    : (int)d;
            }

            // int / float reach here only for a computed value (the interpreter's own
            // numeric types); coerce so an is_integer field never leaks the other one.
            if (raw is int i) {
                return declaredFloat ? (object)(float)i : i;
            }

            if (raw is float f) {
                return declaredFloat || f != Math.Floor(f) || Math.Abs(f) > int.MaxValue
                    ? (object)f
                    : (int)f;
            }

            return raw;
        }
    }
}
