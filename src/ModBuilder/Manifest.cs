using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CustomAssets.ModBuilder
{
    internal sealed class Manifest
    {
        private static readonly Regex VersionPattern =
            new Regex(@"^(\d+)\.(\d+)(\.(\d+)([a-z]?))?$", RegexOptions.Compiled);

        public string Id { get; private set; }
        public string DisplayTitle { get; private set; }
        public string Author { get; private set; }
        public string RawVersion { get; private set; }
        public string NormalizedVersion { get; private set; }

        public static Manifest Load(string manifestPath)
        {
            if (!File.Exists(manifestPath))
                throw new FileNotFoundException("manifest.json not found", manifestPath);

            using (var stream = File.OpenRead(manifestPath))
            using (var doc = JsonDocument.Parse(stream))
            {
                var root = doc.RootElement;

                var id = RequireString(root, "id", manifestPath);
                if (!Regex.IsMatch(id, @"^[a-zA-Z0-9][a-zA-Z0-9_-]*$"))
                    throw new FormatException($"Invalid mod id '{id}' in {manifestPath}: must match [a-zA-Z0-9][a-zA-Z0-9_-]*");

                var rawVersion = RequireString(root, "version", manifestPath);
                var normalized = NormalizeVersion(rawVersion);

                string displayTitle = id;
                if (root.TryGetProperty("display_name", out var displayProp) && displayProp.ValueKind == JsonValueKind.String)
                    displayTitle = displayProp.GetString();

                string author = "Unknown";
                if (root.TryGetProperty("authors", out var authorsProp) && authorsProp.ValueKind == JsonValueKind.Array)
                {
                    var names = authorsProp.EnumerateArray()
                        .Where(a => a.ValueKind == JsonValueKind.String)
                        .Select(a => a.GetString())
                        .Where(s => !string.IsNullOrEmpty(s));
                    var joined = string.Join(", ", names);
                    if (!string.IsNullOrEmpty(joined)) author = joined;
                }

                return new Manifest
                {
                    Id = id,
                    DisplayTitle = displayTitle,
                    Author = author,
                    RawVersion = rawVersion,
                    NormalizedVersion = normalized,
                };
            }
        }

        private static string RequireString(JsonElement root, string name, string manifestPath)
        {
            if (!root.TryGetProperty(name, out var prop) || prop.ValueKind != JsonValueKind.String)
                throw new FormatException($"manifest.json at {manifestPath} is missing required string field '{name}'");
            var value = prop.GetString();
            if (string.IsNullOrEmpty(value))
                throw new FormatException($"manifest.json at {manifestPath} has empty '{name}' field");
            return value;
        }

        private static string NormalizeVersion(string raw)
        {
            var match = VersionPattern.Match(raw);
            if (!match.Success)
                throw new FormatException($"Invalid version format '{raw}'. Expected major.minor[.patch[a-z]] (e.g. 0.1.8 or 0.0.2b)");

            var major = match.Groups[1].Value;
            var minor = match.Groups[2].Value;
            var patch = match.Groups[4].Success ? match.Groups[4].Value : "0";
            var suffix = match.Groups[5].Value;

            int revision = 0;
            if (!string.IsNullOrEmpty(suffix))
                revision = char.ToLowerInvariant(suffix[0]) - 'a' + 1;

            return $"{major}.{minor}.{patch}.{revision}";
        }
    }
}
