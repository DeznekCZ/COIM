using System;
using System.IO;

namespace CustomAssets.StubBuilder.Commands
{
    internal static class ExternalModStubsCommand
    {
        private const string Usage =
@"StubBuilder external-mod-stubs - extract IDs from already-installed mods to make
their custom IDs available for IntelliSense in your own pack.

USAGE
    StubBuilder.exe external-mod-stubs --mod-folder <dir> [--mod-folder <dir>...] --output-root <dir>

OPTIONS
    --mod-folder <dir>    Path to an installed mod folder (must contain manifest.json
                          and Definitions/*.py). Typically under
                          %APPDATA%\Captain of Industry\Mods\<modId>.
                          May be repeated.
    --output-root <dir>   Where to emit external/<modId>/__init__.py files
                          (e.g. src\Recipes\Generated).
    -h, --help            Show this help.
";

        public static int Run(string[] args)
        {
            string outputRoot = null;
            var folders = new System.Collections.Generic.List<string>();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-h":
                    case "--help":
                        Console.WriteLine(Usage);
                        return 0;
                    case "--mod-folder":
                        folders.Add(NextValue(args, ref i, "--mod-folder"));
                        break;
                    case "--output-root":
                        outputRoot = NextValue(args, ref i, "--output-root");
                        break;
                    default:
                        throw new ArgumentException($"Unknown option '{args[i]}'.");
                }
            }

            if (folders.Count == 0)
                throw new ArgumentException("At least one --mod-folder is required.");
            if (string.IsNullOrEmpty(outputRoot))
                throw new ArgumentException("--output-root is required.");

            var externalRoot = Path.Combine(Path.GetFullPath(outputRoot), "external");
            int written = 0;
            foreach (var folder in folders)
            {
                var resolved = Path.GetFullPath(folder);
                var manifestPath = Path.Combine(resolved, "manifest.json");
                if (!File.Exists(manifestPath))
                {
                    Console.Error.WriteLine($"[StubBuilder] Skipping '{resolved}': no manifest.json.");
                    continue;
                }

                var modId = ManifestUtil.ReadId(manifestPath);
                var ids = IdScanner.ScanDirectory(Path.Combine(resolved, "Definitions"));
                var assets = AssetScanner.Scan(resolved);
                var config = ConfigScanner.Scan(resolved);

                if (ids.IsEmpty && assets.IsEmpty && config.IsEmpty)
                {
                    Console.WriteLine($"[StubBuilder] {modId}: nothing extractable, skipping.");
                    continue;
                }

                var outFile = Path.Combine(externalRoot, modId, "__init__.py");
                StubWriter.WritePackStub(outFile, modId, ids, assets, config);
                Console.WriteLine($"[StubBuilder] external/{modId}: {ids.Recipes.Count}+{ids.Research.Count}+{ids.Products.Count}+{ids.Machines.Count}+{ids.ToolbarCategories.Count} ids, {assets.Assets.Count}+{assets.AssetBundles.Count} assets, {config.Fields.Count} config -> {outFile}");
                written++;
            }
            Console.WriteLine($"[StubBuilder] Wrote {written} external stub file(s).");
            return 0;
        }

        private static string NextValue(string[] args, ref int i, string flag)
        {
            if (i + 1 >= args.Length) throw new ArgumentException($"Option '{flag}' requires a value.");
            return args[++i];
        }
    }
}
