using System;
using System.Collections.Generic;
using System.IO;
using CustomAssets.ModBuilder.Svg;

namespace CustomAssets.ModBuilder.Commands
{
    // Build-time SVG → PNG converter. Scans a directory (recursively) or a single file,
    // emitting a PNG next to each SVG. Skips PNGs whose mtime is newer than their SVG
    // (incremental — keeps no-op builds fast).
    internal static class Svg2PngCommand
    {
        private const string Usage =
@"ModBuilder svg2png - rasterize SVG icons to PNG via System.Drawing.

USAGE
    ModBuilder.exe svg2png <inputPath> [options]

ARGUMENTS
    <inputPath>             Either a single .svg file or a directory to scan
                            recursively for *.svg.

OPTIONS
    --size <n>              Output PNG dimensions (square). Default: 128.
    --supersample <n>       Internal render at n*size, downscale for AA. Default: 2.
    --output <path>         For single-file input, path of the PNG to write. Defaults
                            to <input>.png. Ignored for directory input.
    --force                 Always (re)render, ignoring mtimes.
    -h, --help              Show this help.
";

        public static int Run(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine(Usage);
                return 1;
            }

            string input = null;
            string output = null;
            int size = 128;
            int supersample = 2;
            bool force = false;

            for (int i = 0; i < args.Length; i++)
            {
                var a = args[i];
                switch (a)
                {
                    case "-h":
                    case "--help":
                        Console.WriteLine(Usage);
                        return 0;
                    case "--size":
                        size = int.Parse(NextValue(args, ref i, a));
                        break;
                    case "--supersample":
                        supersample = int.Parse(NextValue(args, ref i, a));
                        break;
                    case "--output":
                        output = NextValue(args, ref i, a);
                        break;
                    case "--force":
                        force = true;
                        break;
                    default:
                        if (input == null) input = a;
                        else
                        {
                            Console.Error.WriteLine($"[svg2png] Unexpected argument: {a}");
                            return 1;
                        }
                        break;
                }
            }

            if (string.IsNullOrEmpty(input))
            {
                Console.Error.WriteLine("[svg2png] Missing input path.");
                return 1;
            }

            var renderer = new SvgRenderer(size, supersample);
            int converted = 0;
            int skipped = 0;
            int failed = 0;

            if (File.Exists(input))
            {
                string outPath = output ?? Path.ChangeExtension(input, ".png");
                if (TryConvert(renderer, input, outPath, force))
                    converted++;
                else
                    skipped++;
            }
            else if (Directory.Exists(input))
            {
                foreach (var svg in Directory.EnumerateFiles(input, "*.svg", SearchOption.AllDirectories))
                {
                    string outPath = Path.ChangeExtension(svg, ".png");
                    try
                    {
                        if (TryConvert(renderer, svg, outPath, force))
                            converted++;
                        else
                            skipped++;
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"[svg2png] FAILED '{svg}': {ex.Message}");
                        failed++;
                    }
                }
            }
            else
            {
                Console.Error.WriteLine($"[svg2png] Input not found: {input}");
                return 1;
            }

            Console.WriteLine($"[svg2png] converted={converted} skipped={skipped} failed={failed}");
            return failed == 0 ? 0 : 1;
        }

        private static bool TryConvert(SvgRenderer r, string svgPath, string pngPath, bool force)
        {
            if (!force && File.Exists(pngPath))
            {
                var svgT = File.GetLastWriteTimeUtc(svgPath);
                var pngT = File.GetLastWriteTimeUtc(pngPath);
                if (pngT >= svgT)
                {
                    return false; // skipped, still fresh
                }
            }
            r.RenderToFile(svgPath, pngPath);
            Console.WriteLine($"[svg2png] {svgPath} -> {pngPath}");
            return true;
        }

        private static string NextValue(string[] args, ref int i, string flag)
        {
            if (i + 1 >= args.Length)
                throw new ArgumentException($"[svg2png] '{flag}' requires a value.");
            return args[++i];
        }
    }
}
