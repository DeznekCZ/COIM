using System;

namespace CustomAssets.ModBuilder
{
    internal static class Program
    {
        private const string Usage =
@"ModBuilder - Captain of Industry mod build & scaffold tool.

USAGE
    ModBuilder.exe <command> [options]

COMMANDS
    build <packDir>      Generate <id>.cs/.csproj from manifest.json and build the
                         mod DLL (with deploy + zip). The default action.
    new <modId>          Scaffold a new pack folder with manifest.json,
                         Definitions/example.py, and .gitignore.
    svg2png <input>      Rasterize SVG icons to PNG. Input may be a single file
                         or a directory (scanned recursively). Incremental.
    png2svg <input>      Vectorize PNG icons to polygonal SVG (quantize +
                         contour-trace). Reverse of svg2png; lossy.

GLOBAL
    -h, --help           Show this help.

Run 'ModBuilder.exe <command> --help' for command-specific options.
";

        internal static int Main(string[] args)
        {
            if (args.Length == 0 || args[0] == "-h" || args[0] == "--help")
            {
                Console.WriteLine(Usage);
                return args.Length == 0 ? 1 : 0;
            }

            try
            {
                var rest = new string[args.Length - 1];
                Array.Copy(args, 1, rest, 0, rest.Length);

                switch (args[0])
                {
                    case "build":
                        return Commands.BuildCommand.Run(rest);
                    case "new":
                        return Commands.NewCommand.Run(rest);
                    case "svg2png":
                        return Commands.Svg2PngCommand.Run(rest);
                    case "png2svg":
                        return Commands.Png2SvgCommand.Run(rest);
                    default:
                        Console.Error.WriteLine($"[ModBuilder] Unknown command '{args[0]}'. Use --help.");
                        return 1;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ModBuilder] ERROR: {ex.Message}");
#if DEBUG
                Console.Error.WriteLine(ex);
#endif
                return 1;
            }
        }
    }
}
