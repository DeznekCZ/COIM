using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CustomAssets.StubBuilder
{
    internal sealed class ScannedAssets
    {
        public SortedDictionary<string, string> Assets { get; } = new SortedDictionary<string, string>();
        public SortedDictionary<string, string> AssetBundles { get; } = new SortedDictionary<string, string>();
        public bool IsEmpty => Assets.Count == 0 && AssetBundles.Count == 0;
    }

    internal static class AssetScanner
    {
        public static ScannedAssets Scan(string packDir)
        {
            var result = new ScannedAssets();
            CollectFolder(Path.Combine(packDir, "Assets"), result.Assets, "Assets");
            CollectFolder(Path.Combine(packDir, "AssetBundles"), result.AssetBundles, "AssetBundles");
            return result;
        }

        private static void CollectFolder(string root, SortedDictionary<string, string> sink, string label)
        {
            if (!Directory.Exists(root)) return;
            var rootFull = Path.GetFullPath(root);
            foreach (var file in Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories))
            {
                var fullPath = Path.GetFullPath(file);
                var rel = fullPath.Substring(rootFull.Length).TrimStart(Path.DirectorySeparatorChar, '/');
                var posix = rel.Replace('\\', '/');
                var ident = MakeIdentifier(posix);
                var stored = label + "/" + posix;
                if (!sink.ContainsKey(ident))
                    sink.Add(ident, stored);
            }
        }

        private static string MakeIdentifier(string relPath)
        {
            var chars = relPath.Select(c =>
                char.IsLetterOrDigit(c) ? c :
                c == '.' ? '_' :
                c == '/' ? '_' :
                c == '-' ? '_' :
                c == ' ' ? '_' :
                '_').ToArray();
            var ident = new string(chars);
            if (ident.Length > 0 && char.IsDigit(ident[0])) ident = "_" + ident;
            return ident;
        }
    }
}
