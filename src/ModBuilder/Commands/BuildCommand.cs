using System;
using System.IO;

namespace CustomAssets.ModBuilder.Commands
{
    internal static class BuildCommand
    {
        private const string Usage =
@"ModBuilder build - generate <id>.cs/.csproj from manifest.json and build the mod DLL.

USAGE
    ModBuilder.exe build <packDir> [options]

ARGUMENTS
    <packDir>                Directory containing manifest.json (and optionally
                             Definitions/, Assets/, AssetBundles/, config.json).

OPTIONS
    --coi-root <path>        Game install root (containing 'Captain of Industry_Data').
                             Default: $env:COI_ROOT.
    --coi-mods <path>        Mods folder where the built mod is deployed.
                             Default: $env:COI_MODS, or %APPDATA%\Captain of Industry\Mods.
    --lib-csproj <path>      Path to CustomAssets.csproj (use when working from this repo).
    --lib-dll <path>         Path to a prebuilt CustomAssets.dll (alternative to --lib-csproj).
    --generated-root <dir>   Where to emit the generated <id>.cs/.csproj files.
                             Default: <packDir>\.build.
    --mode <auto|generic|bundle>   Loading path. 'auto' picks 'bundle' if the pack
                             contains a non-empty AssetBundles\ folder. Default: auto.
    --configuration <name>   MSBuild configuration. Default: Release.
    --supporter              Reference Mafi.Supporter.dll.
    --trains-dlc             Reference Mafi.TrainsDlc(.Unity).dll.
    --no-build               Generate files only; skip the dotnet build invocation.
    --id-out <file>          Write the resolved mod id to this file (one line, no newline).
    --csproj-out <file>      Write the absolute path of the generated csproj to this file.
    -h, --help               Show this help.
";

        public static int Run(string[] args)
        {
            var options = Parse(args);
            if (options == null) return 1;
            return new PackBuilder(options).Run();
        }

        private static BuildOptions Parse(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine(Usage);
                return null;
            }

            var options = new BuildOptions();
            for (int i = 0; i < args.Length; i++)
            {
                var a = args[i];
                switch (a)
                {
                    case "-h":
                    case "--help":
                        Console.WriteLine(Usage);
                        return null;
                    case "--coi-root":
                        options.CoiRoot = NextValue(args, ref i, a);
                        break;
                    case "--coi-mods":
                        options.CoiMods = NextValue(args, ref i, a);
                        break;
                    case "--lib-csproj":
                        options.LibCsproj = NextValue(args, ref i, a);
                        break;
                    case "--lib-dll":
                        options.LibDll = NextValue(args, ref i, a);
                        break;
                    case "--generated-root":
                        options.GeneratedRoot = NextValue(args, ref i, a);
                        break;
                    case "--mode":
                        options.Mode = ParseMode(NextValue(args, ref i, a));
                        break;
                    case "--configuration":
                        options.Configuration = NextValue(args, ref i, a);
                        break;
                    case "--supporter":
                        options.Supporter = true;
                        break;
                    case "--trains-dlc":
                        options.TrainsDlc = true;
                        break;
                    case "--no-build":
                        options.NoBuild = true;
                        break;
                    case "--id-out":
                        options.IdOut = NextValue(args, ref i, a);
                        break;
                    case "--csproj-out":
                        options.CsprojOut = NextValue(args, ref i, a);
                        break;
                    default:
                        if (a.StartsWith("--"))
                            throw new ArgumentException($"Unknown option '{a}'. Use --help for usage.");
                        if (options.PackDir != null)
                            throw new ArgumentException($"Multiple pack directories specified ('{options.PackDir}', '{a}').");
                        options.PackDir = a;
                        break;
                }
            }

            if (string.IsNullOrEmpty(options.PackDir))
                throw new ArgumentException("Missing required <packDir> argument. Use --help for usage.");
            options.PackDir = Path.GetFullPath(options.PackDir);
            if (!Directory.Exists(options.PackDir))
                throw new DirectoryNotFoundException($"Pack directory not found: {options.PackDir}");

            options.CoiRoot = options.CoiRoot ?? Environment.GetEnvironmentVariable("COI_ROOT");
            options.CoiMods = options.CoiMods ?? Environment.GetEnvironmentVariable("COI_MODS")
                ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                "Captain of Industry", "Mods");

            if (string.IsNullOrEmpty(options.CoiRoot))
                throw new ArgumentException("COI_ROOT not set. Pass --coi-root or set the env var.");

            options.GeneratedRoot = options.GeneratedRoot ?? Path.Combine(options.PackDir, ".build");
            options.GeneratedRoot = Path.GetFullPath(options.GeneratedRoot);

            return options;
        }

        private static string NextValue(string[] args, ref int i, string flag)
        {
            if (i + 1 >= args.Length)
                throw new ArgumentException($"Option '{flag}' requires a value.");
            return args[++i];
        }

        private static PackMode ParseMode(string value)
        {
            switch (value.ToLowerInvariant())
            {
                case "auto": return PackMode.Auto;
                case "generic": return PackMode.Generic;
                case "bundle": return PackMode.Bundle;
                default:
                    throw new ArgumentException($"Invalid --mode value '{value}'. Expected: auto | generic | bundle.");
            }
        }
    }
}
