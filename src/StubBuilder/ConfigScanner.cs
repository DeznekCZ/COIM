using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace CustomAssets.StubBuilder
{
    internal sealed class ScannedConfig
    {
        // Field name -> Python type literal ("bool" | "int" | "float" | "str") plus default repr.
        public SortedDictionary<string, ConfigField> Fields { get; } = new SortedDictionary<string, ConfigField>();
        public bool IsEmpty => Fields.Count == 0;
    }

    internal sealed class ConfigField
    {
        public string PythonType { get; set; }
        public string DefaultRepr { get; set; }
        public string Description { get; set; }
    }

    /// Schema-aware config.json scanner. Matches the same constrained schema the runtime uses
    /// (top-level keys are field names, each value is { default, is_integer?, description?, ... }).
    /// We only need the *names* and types for IntelliSense, not real JSON parsing.
    internal static class ConfigScanner
    {
        private static readonly Regex FieldBlockRegex =
            new Regex(@"""([a-z][a-z0-9_]*)""\s*:\s*\{(.*?)\}", RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex DefaultBoolRegex =
            new Regex(@"""default""\s*:\s*(true|false)", RegexOptions.Compiled);
        private static readonly Regex DefaultStringRegex =
            new Regex(@"""default""\s*:\s*""((?:[^""\\]|\\.)*)""", RegexOptions.Compiled);
        private static readonly Regex DefaultNumberRegex =
            new Regex(@"""default""\s*:\s*(-?\d+(?:\.\d+)?)", RegexOptions.Compiled);
        private static readonly Regex IsIntegerRegex =
            new Regex(@"""is_integer""\s*:\s*(true|false)", RegexOptions.Compiled);
        private static readonly Regex DescriptionRegex =
            new Regex(@"""description""\s*:\s*""((?:[^""\\]|\\.)*)""", RegexOptions.Compiled);

        public static ScannedConfig Scan(string packDir)
        {
            var result = new ScannedConfig();
            var configPath = Path.Combine(packDir, "config.json");
            if (!File.Exists(configPath)) return result;

            string text;
            try { text = File.ReadAllText(configPath); }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[StubBuilder] Cannot read {configPath}: {ex.Message}");
                return result;
            }

            foreach (Match m in FieldBlockRegex.Matches(text))
            {
                var name = m.Groups[1].Value;
                if (name.StartsWith("$")) continue; // skip $schema etc.
                var body = m.Groups[2].Value;
                var field = ParseField(body);
                if (field != null) result.Fields[name] = field;
            }
            return result;
        }

        private static ConfigField ParseField(string body)
        {
            var boolMatch = DefaultBoolRegex.Match(body);
            if (boolMatch.Success)
                return new ConfigField {
                    PythonType = "bool",
                    DefaultRepr = boolMatch.Groups[1].Value == "true" ? "True" : "False",
                    Description = ExtractDescription(body),
                };

            var stringMatch = DefaultStringRegex.Match(body);
            if (stringMatch.Success)
                return new ConfigField {
                    PythonType = "str",
                    DefaultRepr = "'" + stringMatch.Groups[1].Value.Replace("'", "\\'") + "'",
                    Description = ExtractDescription(body),
                };

            var numberMatch = DefaultNumberRegex.Match(body);
            if (numberMatch.Success)
            {
                var raw = numberMatch.Groups[1].Value;
                var isIntegerMatch = IsIntegerRegex.Match(body);
                bool isInteger = !raw.Contains(".") &&
                    (!isIntegerMatch.Success || isIntegerMatch.Groups[1].Value == "true");
                return new ConfigField {
                    PythonType = isInteger ? "int" : "float",
                    DefaultRepr = raw,
                    Description = ExtractDescription(body),
                };
            }
            return null;
        }

        private static string ExtractDescription(string body)
        {
            var m = DescriptionRegex.Match(body);
            return m.Success ? m.Groups[1].Value : null;
        }
    }
}
