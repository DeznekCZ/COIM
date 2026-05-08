using System;
using System.IO;
using System.Reflection;

namespace CustomAssets.ModBuilder
{
    internal static class TemplateLoader
    {
        public static string Load(string resourceFileName)
        {
            var assembly = typeof(TemplateLoader).Assembly;
            var rootNamespace = typeof(TemplateLoader).Namespace;
            var fullName = $"{rootNamespace}.Templates.{resourceFileName}";
            using (var stream = assembly.GetManifestResourceStream(fullName))
            {
                if (stream == null)
                    throw new InvalidOperationException(
                        $"Embedded template '{fullName}' not found. Available: {string.Join(", ", assembly.GetManifestResourceNames())}");
                using (var reader = new StreamReader(stream))
                    return reader.ReadToEnd();
            }
        }
    }
}
