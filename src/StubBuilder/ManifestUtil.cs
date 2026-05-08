using System;
using System.IO;
using System.Text.RegularExpressions;

namespace CustomAssets.StubBuilder
{
    internal static class ManifestUtil
    {
        // Lightweight manifest reader: we only need the "id" field. Avoids pulling in
        // System.Text.Json so StubBuilder stays a tiny single-assembly tool.
        private static readonly Regex IdRegex =
            new Regex(@"""id""\s*:\s*""([A-Za-z0-9][A-Za-z0-9_\-]*)""", RegexOptions.Compiled);

        public static string ReadId(string manifestPath)
        {
            var text = File.ReadAllText(manifestPath);
            var match = IdRegex.Match(text);
            if (!match.Success)
                throw new FormatException($"Could not find 'id' field in {manifestPath}.");
            return match.Groups[1].Value;
        }
    }
}
