using System;

namespace CustomAssets.StubBuilder
{
    internal static class Program
    {
        private const string Usage =
@"StubBuilder - generate Python IntelliSense stubs for COI mod authoring.

USAGE
    StubBuilder.exe <command> [options]

COMMANDS
    pack-stubs            Scan a pack's Definitions/*.py and Assets/ folder, emit
                          per-pack ID & asset stubs to <output-root>/<modId>/__init__.py.
    external-mod-stubs    Same as pack-stubs but applied to an installed mod's folder
                          under the COI Mods directory. Output: <output-root>/external/<modId>/.
    game-stubs            Walk Mafi/Mafi.Core/Mafi.Base assemblies and refresh
                          <output-root>/Mafi/**/__init__.py with type stubs.

GLOBAL
    -h, --help            Show this help.

Run 'StubBuilder.exe <command> --help' for command-specific options.
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
                    case "pack-stubs":
                        return Commands.PackStubsCommand.Run(rest);
                    case "external-mod-stubs":
                        return Commands.ExternalModStubsCommand.Run(rest);
                    case "game-stubs":
                        return Commands.GameStubsCommand.Run(rest);
                    default:
                        Console.Error.WriteLine($"[StubBuilder] Unknown command '{args[0]}'. Use --help.");
                        return 1;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[StubBuilder] ERROR: {ex.Message}");
#if DEBUG
                Console.Error.WriteLine(ex);
#endif
                return 1;
            }
        }
    }
}
