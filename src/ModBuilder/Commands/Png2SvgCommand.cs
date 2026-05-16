using System;
using System.IO;
using CustomAssets.ModBuilder.Svg;

namespace CustomAssets.ModBuilder.Commands
{
    // Inverse of svg2png: trace a raster PNG into a polygonal SVG. Quantizes the input
    // to a small palette and emits one <path> per connected color region. Best results
    // on stylized / cartoony icons; photographic input traces poorly.
    internal static class Png2SvgCommand
    {
        private const string Usage =
@"ModBuilder png2svg - vectorize PNG icons into polygonal SVG.

USAGE
    ModBuilder.exe png2svg <inputPath> [options]

ARGUMENTS
    <inputPath>             A single .png file or a directory to scan
                            recursively for *.png. Existing .svg with the same
                            base name will be OVERWRITTEN.

OPTIONS
    --colors <n>            Palette size (top-n most frequent colors). Default: 4.
    --alpha <n>             Treat pixels with alpha < n as transparent. Default: 128.
    --simplify <e>          Douglas-Peucker tolerance in pixels. Higher = simpler
                            shape but more deviation from source. Default: 0.75.
    --min-region <n>        Skip regions smaller than n pixels. Default: 4.
    --outline-width <w>     Black outline thickness drawn around the whole silhouette
                            on top of the color fills. Default: 1.5. Set to 0 to
                            disable (equivalent to --no-outline).
    --outline-color <hex>   Outline color (#rgb or #rrggbb). Default: #000.
    --no-outline            Shortcut for --outline-width 0.
    --output <path>         For single-file input, target .svg path. Defaults to
                            <input>.svg. Ignored for directory input.
    -h, --help              Show this help.

NOTES
    - Output is polygonal — no curve fitting. Re-rasterizing via svg2png yields
      a slightly blockier image than the original.
    - The original PNG is NOT deleted; pair this tool with svg2png to round-trip,
      but plan to keep the SVG as your editable source and regenerate PNG on build.
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
            int colors = 4;
            int alpha = 128;
            double simplify = 0.75;
            int minRegion = 4;
            double outlineWidth = 1.5;
            System.Drawing.Color outlineColor = System.Drawing.Color.Black;

            for (int i = 0; i < args.Length; i++)
            {
                var a = args[i];
                switch (a)
                {
                    case "-h":
                    case "--help":
                        Console.WriteLine(Usage);
                        return 0;
                    case "--colors":
                        colors = int.Parse(NextValue(args, ref i, a));
                        break;
                    case "--alpha":
                        alpha = int.Parse(NextValue(args, ref i, a));
                        break;
                    case "--simplify":
                        simplify = double.Parse(NextValue(args, ref i, a), System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case "--min-region":
                        minRegion = int.Parse(NextValue(args, ref i, a));
                        break;
                    case "--outline-width":
                        outlineWidth = double.Parse(NextValue(args, ref i, a), System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case "--outline-color":
                        outlineColor = ParseHexColor(NextValue(args, ref i, a));
                        break;
                    case "--no-outline":
                        outlineWidth = 0;
                        break;
                    case "--output":
                        output = NextValue(args, ref i, a);
                        break;
                    default:
                        if (input == null) input = a;
                        else
                        {
                            Console.Error.WriteLine($"[png2svg] Unexpected argument: {a}");
                            return 1;
                        }
                        break;
                }
            }

            if (string.IsNullOrEmpty(input))
            {
                Console.Error.WriteLine("[png2svg] Missing input path.");
                return 1;
            }

            var tracer = new PngToSvgTracer(colors, alpha, simplify, minRegion, outlineColor, outlineWidth);
            int converted = 0;
            int failed = 0;

            if (File.Exists(input))
            {
                string outPath = output ?? Path.ChangeExtension(input, ".svg");
                try
                {
                    tracer.TraceToFile(input, outPath);
                    Console.WriteLine($"[png2svg] {input} -> {outPath}");
                    converted++;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[png2svg] FAILED '{input}': {ex.Message}");
                    failed++;
                }
            }
            else if (Directory.Exists(input))
            {
                foreach (var png in Directory.EnumerateFiles(input, "*.png", SearchOption.AllDirectories))
                {
                    string outPath = Path.ChangeExtension(png, ".svg");
                    try
                    {
                        tracer.TraceToFile(png, outPath);
                        Console.WriteLine($"[png2svg] {png} -> {outPath}");
                        converted++;
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"[png2svg] FAILED '{png}': {ex.Message}");
                        failed++;
                    }
                }
            }
            else
            {
                Console.Error.WriteLine($"[png2svg] Input not found: {input}");
                return 1;
            }

            Console.WriteLine($"[png2svg] converted={converted} failed={failed}");
            return failed == 0 ? 0 : 1;
        }

        private static string NextValue(string[] args, ref int i, string flag)
        {
            if (i + 1 >= args.Length)
                throw new ArgumentException($"[png2svg] '{flag}' requires a value.");
            return args[++i];
        }

        private static System.Drawing.Color ParseHexColor(string s)
        {
            if (string.IsNullOrEmpty(s)) return System.Drawing.Color.Black;
            string h = s.StartsWith("#") ? s.Substring(1) : s;
            if (h.Length == 3) h = "" + h[0] + h[0] + h[1] + h[1] + h[2] + h[2];
            if (h.Length != 6) throw new ArgumentException($"[png2svg] invalid --outline-color '{s}', expected #rgb or #rrggbb");
            int r = int.Parse(h.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            int g = int.Parse(h.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            int b = int.Parse(h.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            return System.Drawing.Color.FromArgb(r, g, b);
        }
    }
}
