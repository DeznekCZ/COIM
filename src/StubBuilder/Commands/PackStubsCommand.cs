using System;
using System.IO;

namespace CustomAssets.StubBuilder.Commands
{
    internal static class PackStubsCommand
    {
        private const string Usage =
@"StubBuilder pack-stubs - generate IntelliSense stubs for one or more pack folders.

USAGE
    StubBuilder.exe pack-stubs --pack <packDir> [--pack <packDir>...] --output-root <dir>

OPTIONS
    --pack <packDir>      Path to a pack folder (must contain manifest.json).
                          May be repeated to scan multiple packs in one invocation.
    --output-root <dir>   Where to emit <modId>/__init__.py files
                          (e.g. src\Recipes\Generated).
    -h, --help            Show this help.
";

        public static int Run(string[] args)
        {
            string outputRoot = null;
            var packs = new System.Collections.Generic.List<string>();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-h":
                    case "--help":
                        Console.WriteLine(Usage);
                        return 0;
                    case "--pack":
                        packs.Add(NextValue(args, ref i, "--pack"));
                        break;
                    case "--output-root":
                        outputRoot = NextValue(args, ref i, "--output-root");
                        break;
                    default:
                        throw new ArgumentException($"Unknown option '{args[i]}'.");
                }
            }

            if (packs.Count == 0)
                throw new ArgumentException("At least one --pack is required.");
            if (string.IsNullOrEmpty(outputRoot))
                throw new ArgumentException("--output-root is required.");

            outputRoot = Path.GetFullPath(outputRoot);
            int written = 0;
            foreach (var packDir in packs)
            {
                var resolved = Path.GetFullPath(packDir);
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

                var outFile = Path.Combine(outputRoot, modId, "__init__.py");
                StubWriter.WritePackStub(outFile, modId, ids, assets, config);
                Console.WriteLine($"[StubBuilder] {modId}: {ids.Recipes.Count} recipes, {ids.Research.Count} research, {ids.Products.Count} products, {ids.Machines.Count} machines, {ids.ToolbarCategories.Count} categories, {assets.Assets.Count} assets, {assets.AssetBundles.Count} bundles, {config.Fields.Count} config -> {outFile}");
                written++;
            }
            Console.WriteLine($"[StubBuilder] Wrote {written} stub file(s).");
            return 0;
        }

        private static string NextValue(string[] args, ref int i, string flag)
        {
            if (i + 1 >= args.Length) throw new ArgumentException($"Option '{flag}' requires a value.");
            return args[++i];
        }
    }
}
