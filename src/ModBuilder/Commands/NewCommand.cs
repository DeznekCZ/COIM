using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace CustomAssets.ModBuilder.Commands
{
    internal static class NewCommand
    {
        private const string Usage =
@"ModBuilder new - scaffold a new pack folder ready for `ModBuilder build`.

USAGE
    ModBuilder.exe new <modId> [options]

ARGUMENTS
    <modId>                  The mod id. Becomes the folder name AND the 'id' field
                             of the new manifest.json. Must match
                             [a-zA-Z0-9][a-zA-Z0-9_-]* (the COI mod id pattern).

OPTIONS
    --at <parentDir>         Where to create the <modId>/ folder.
                             Default: current working directory.
    --display-name <text>    'display_name' field. Default: <modId>.
    --author <name>          'authors' field (single author). Default: 'TODO'.
    --description <text>     'description_short' and 'description_long'.
                             Default: 'TODO: describe your mod.'
    --no-example             Skip Definitions/example.py.
    --no-config              Skip config.json.
    --force                  Overwrite the target folder if it already exists.
    -h, --help               Show this help.

OUTPUT LAYOUT
    <parentDir>/<modId>/
        manifest.json          (filled with sensible defaults)
        config.json            (two example fields; player-tunable settings)
        Definitions/
            __init__.py        (explicit load order via dependencies())
            example.py         (commented-out build_recipe template)
        .gitignore             (ignores build artifacts)
";

        private static readonly Regex ModIdPattern =
            new Regex(@"^[a-zA-Z0-9][a-zA-Z0-9_-]*$", RegexOptions.Compiled);

        public static int Run(string[] args)
        {
            string modId = null;
            string parentDir = null;
            string displayName = null;
            string author = "TODO";
            string description = "TODO: describe your mod.";
            bool includeExample = true;
            bool includeConfig = true;
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
                    case "--at":
                        parentDir = NextValue(args, ref i, a);
                        break;
                    case "--display-name":
                        displayName = NextValue(args, ref i, a);
                        break;
                    case "--author":
                        author = NextValue(args, ref i, a);
                        break;
                    case "--description":
                        description = NextValue(args, ref i, a);
                        break;
                    case "--no-example":
                        includeExample = false;
                        break;
                    case "--no-config":
                        includeConfig = false;
                        break;
                    case "--force":
                        force = true;
                        break;
                    default:
                        if (a.StartsWith("--"))
                            throw new ArgumentException($"Unknown option '{a}'. Use --help for usage.");
                        if (modId != null)
                            throw new ArgumentException($"Multiple mod ids specified ('{modId}', '{a}').");
                        modId = a;
                        break;
                }
            }

            if (string.IsNullOrEmpty(modId))
                throw new ArgumentException("Missing required <modId> argument. Use --help for usage.");
            if (!ModIdPattern.IsMatch(modId))
                throw new ArgumentException($"Invalid mod id '{modId}'. Must match [a-zA-Z0-9][a-zA-Z0-9_-]*.");

            parentDir = Path.GetFullPath(parentDir ?? Environment.CurrentDirectory);
            displayName = displayName ?? modId;

            var modDir = Path.Combine(parentDir, modId);
            if (Directory.Exists(modDir) && !force)
                throw new IOException($"Target directory '{modDir}' already exists. Use --force to overwrite.");

            Directory.CreateDirectory(modDir);
            Directory.CreateDirectory(Path.Combine(modDir, "Definitions"));

            WriteFile(
                Path.Combine(modDir, "manifest.json"),
                Render("NewMod.manifest.tmpl", new Dictionary<string, string>
                {
                    ["{{ID}}"] = modId,
                    ["{{DISPLAY_NAME}}"] = JsonEscape(displayName),
                    ["{{AUTHOR}}"] = JsonEscape(author),
                    ["{{DESCRIPTION}}"] = JsonEscape(description),
                }));

            WriteFile(
                Path.Combine(modDir, ".gitignore"),
                Render("NewMod.gitignore.tmpl", new Dictionary<string, string>()));

            if (includeConfig)
            {
                WriteFile(
                    Path.Combine(modDir, "config.json"),
                    Render("NewMod.config.tmpl", new Dictionary<string, string>()));
            }

            if (includeExample)
            {
                WriteFile(
                    Path.Combine(modDir, "Definitions", "example.py"),
                    Render("NewMod.example.tmpl", new Dictionary<string, string>
                    {
                        ["{{ID}}"] = modId,
                    }));

                // __init__.py declares explicit load order. Only meaningful with at least
                // one module to load, so it ships alongside example.py (controlled by the
                // same --no-example flag).
                WriteFile(
                    Path.Combine(modDir, "Definitions", "__init__.py"),
                    Render("NewMod.init.tmpl", new Dictionary<string, string>
                    {
                        ["{{ID}}"] = modId,
                    }));
            }

            Console.WriteLine($"[ModBuilder] Created new mod scaffold at {modDir}");
            Console.WriteLine("[ModBuilder] Next steps:");
            Console.WriteLine($"  1. Edit manifest.json (description, author).");
            Console.WriteLine($"  2. Replace Definitions/example.py with your recipes.");
            Console.WriteLine($"  3. Build:  ModBuilder.exe build \"{modDir}\" --coi-root <COI install> --lib-dll <CustomAssets.dll>");
            return 0;
        }

        private static string Render(string templateName, Dictionary<string, string> values)
        {
            var template = TemplateLoader.Load(templateName);
            foreach (var kv in values)
                template = template.Replace(kv.Key, kv.Value);
            return template;
        }

        private static void WriteFile(string path, string content)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, content);
            Console.WriteLine($"[ModBuilder] wrote {path}");
        }

        private static string NextValue(string[] args, ref int i, string flag)
        {
            if (i + 1 >= args.Length)
                throw new ArgumentException($"Option '{flag}' requires a value.");
            return args[++i];
        }

        private static string JsonEscape(string value)
        {
            if (value == null) return string.Empty;
            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }
    }
}
