using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace CustomAssets.ModBuilder.Commands
{
    /// <summary>
    /// Reports COI machine port shapes (from the curated Data/machine_ports.json catalog)
    /// and product types (live reflection on Mafi.Base.dll's Ids+Products attributes).
    /// Loads the JSON catalog at runtime so the same file backs the build-time CLI and
    /// the in-game framework.
    /// </summary>
    internal static class IntrospectCommand
    {
        private const string Usage =
@"ModBuilder introspect - inspect COI machine port shapes and product types.

USAGE
    ModBuilder.exe introspect machine <id>        Print port matrix for a machine.
    ModBuilder.exe introspect machines [<filter>] List known machines (substring filter optional).
    ModBuilder.exe introspect product <id>        Print product type (Countable/Fluid/Loose/Molten).
    ModBuilder.exe introspect products [<filter>] List products matching filter.

OPTIONS
    --coi-root <path>     Game install root (for reflecting on Mafi.Base.dll).
                          Default: $env:COI_ROOT, or the dev-tree path
                          ../CustomRecipes/bin/Release/net48.
    --catalog <path>      Override the machine_ports.json catalog location.
                          Default: <ModBuilder.exe folder>/Data/machine_ports.json.
    --format <text|json>  Output format. Default: text.
    -h, --help            Show this help.
";

        public static int Run(string[] args)
        {
            if (args.Length == 0 || args[0] == "-h" || args[0] == "--help")
            {
                Console.WriteLine(Usage);
                return args.Length == 0 ? 1 : 0;
            }

            string subcommand = args[0];
            string[] rest = args.Skip(1).ToArray();

            var opts = ParseOptions(rest, out string positional);
            if (opts == null) return 1;

            var catalog = LoadCatalog(opts.CatalogPath);
            if (catalog == null) return 1;

            switch (subcommand)
            {
                case "machine":
                    if (string.IsNullOrEmpty(positional))
                    {
                        Console.Error.WriteLine("[introspect machine] missing <id>. See --help.");
                        return 1;
                    }
                    return PrintMachine(catalog, positional, opts);

                case "machines":
                    return ListMachines(catalog, positional, opts);

                case "product":
                    if (string.IsNullOrEmpty(positional))
                    {
                        Console.Error.WriteLine("[introspect product] missing <id>. See --help.");
                        return 1;
                    }
                    return PrintProduct(positional, opts);

                case "products":
                    return ListProducts(positional, opts);

                default:
                    Console.Error.WriteLine($"[introspect] unknown subcommand '{subcommand}'. See --help.");
                    return 1;
            }
        }

        // ---------------------------------------------------------------------
        // Machine catalog (JSON)
        // ---------------------------------------------------------------------

        private static MachineCatalog LoadCatalog(string overridePath)
        {
            string sourceLabel = null;
            string path = overridePath ?? ResolveCatalogPath(out sourceLabel);
            if (path == null)
            {
                Console.Error.WriteLine("[introspect] no catalog found (runtime dump nor source-tree placeholder).");
                return null;
            }
            if (!File.Exists(path))
            {
                Console.Error.WriteLine($"[introspect] catalog not found: {path}");
                return null;
            }
            try
            {
                string json = File.ReadAllText(path);
                var catalog = JsonSerializer.Deserialize<MachineCatalog>(json, JsonOpts);
                if (overridePath == null && sourceLabel != null)
                {
                    Console.Error.WriteLine($"[introspect] using catalog: {sourceLabel} ({path})");
                }
                return catalog;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[introspect] failed to parse catalog '{path}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Resolve the best catalog file to read. Order:
        ///   1. Runtime-dumped catalog at &lt;COI_Mods&gt;/CustomAssets/Data/machine_ports.json
        ///      (written by the CustomAssets framework on game load — has real port names).
        ///   2. Source-tree / shipped catalog at &lt;exe&gt;/Data/machine_ports.json
        ///      (placeholder names — used when the runtime dump hasn't been generated yet).
        /// </summary>
        private static string ResolveCatalogPath(out string sourceLabel)
        {
            // 1. Runtime-dumped catalog under COI_MODS.
            string modsRoot = Environment.GetEnvironmentVariable("COI_MODS");
            if (string.IsNullOrEmpty(modsRoot))
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if (!string.IsNullOrEmpty(appData))
                    modsRoot = Path.Combine(appData, "Captain of Industry", "Mods");
            }
            if (!string.IsNullOrEmpty(modsRoot))
            {
                string runtimePath = Path.Combine(modsRoot, "CustomAssets", "Data", "machine_ports.json");
                if (File.Exists(runtimePath))
                {
                    sourceLabel = "runtime-dumped";
                    return runtimePath;
                }
            }

            // 2. Source-tree placeholder bundled with the exe.
            string exeDir = Path.GetDirectoryName(typeof(IntrospectCommand).Assembly.Location);
            string placeholder = Path.Combine(exeDir, "Data", "machine_ports.json");
            if (File.Exists(placeholder))
            {
                sourceLabel = "source-tree placeholder";
                return placeholder;
            }

            sourceLabel = null;
            return null;
        }

        private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        private static int PrintMachine(MachineCatalog catalog, string id, Options opts)
        {
            if (!catalog.Machines.TryGetValue(id, out var m))
            {
                Console.Error.WriteLine($"[introspect machine] '{id}' not in catalog.");
                Console.Error.WriteLine($"  Known machines: {string.Join(", ", catalog.Machines.Keys.OrderBy(s => s))}");
                return 1;
            }

            if (opts.Format == "json")
            {
                Console.WriteLine(JsonSerializer.Serialize(new { id, machine = m }, JsonOpts));
                return 0;
            }

            Console.WriteLine($"Machine: {id}  ({m.DisplayName ?? "(no display name)"})");
            Console.WriteLine();
            Console.WriteLine($"  Inputs ({m.Inputs?.Count ?? 0} ports):");
            PrintPorts(m.Inputs);
            Console.WriteLine();
            Console.WriteLine($"  Outputs ({m.Outputs?.Count ?? 0} ports):");
            PrintPorts(m.Outputs);
            if (!string.IsNullOrEmpty(m.Notes))
            {
                Console.WriteLine();
                Console.WriteLine($"  Notes: {m.Notes}");
            }
            return 0;
        }

        private static void PrintPorts(Dictionary<string, string> ports)
        {
            if (ports == null || ports.Count == 0)
            {
                Console.WriteLine("    (none)");
                return;
            }
            // Real COI port names are single chars (A..Z) embedded in the machine's layout
            // string. Placeholder names from the catalog start with 'in' or 'out' — flag
            // those so the reader knows the catalog hasn't been populated from the live DLL yet.
            int nameWidth = ports.Keys.Max(k => k.Length);
            foreach (var kv in ports)
            {
                bool placeholder = IsPlaceholder(kv.Key);
                string suffix = placeholder ? "   (placeholder — real name TBD from MachineProto.Ports)" : "";
                Console.WriteLine($"    {kv.Key.PadRight(nameWidth)} : {kv.Value}{suffix}");
            }
        }

        private static bool IsPlaceholder(string portName)
        {
            return portName != null
                && (portName.StartsWith("in") || portName.StartsWith("out"))
                && portName.Length > 2
                && char.IsDigit(portName[portName.Length - 1]);
        }

        private static int ListMachines(MachineCatalog catalog, string filter, Options opts)
        {
            var keys = catalog.Machines.Keys
                .Where(k => string.IsNullOrEmpty(filter) || k.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(k => k)
                .ToList();

            if (opts.Format == "json")
            {
                Console.WriteLine(JsonSerializer.Serialize(keys, JsonOpts));
                return 0;
            }

            if (keys.Count == 0)
            {
                Console.WriteLine("(no machines match)");
                return 0;
            }

            int nameWidth = keys.Max(k => k.Length);
            foreach (var k in keys)
            {
                var m = catalog.Machines[k];
                int inCount = m.Inputs?.Count ?? 0;
                int outCount = m.Outputs?.Count ?? 0;
                Console.WriteLine($"  {k.PadRight(nameWidth)}  in={inCount}  out={outCount}  {m.DisplayName ?? ""}");
            }
            return 0;
        }

        // ---------------------------------------------------------------------
        // Product types (reflection on Mafi.Base.dll)
        // ---------------------------------------------------------------------

        private static int PrintProduct(string id, Options opts)
        {
            var idsType = LoadIdsProductsType(opts);
            if (idsType == null) return 1;

            var field = idsType.GetField(id);
            if (field == null)
            {
                Console.Error.WriteLine($"[introspect product] '{id}' not found in Mafi.Base.Ids+Products.");
                return 1;
            }

            string type = TypeFromAttributes(field);
            if (opts.Format == "json")
            {
                Console.WriteLine(JsonSerializer.Serialize(new { id, type }, JsonOpts));
                return 0;
            }

            Console.WriteLine($"Product: {id}");
            Console.WriteLine($"  Type:  {type}");
            return 0;
        }

        private static int ListProducts(string filter, Options opts)
        {
            var idsType = LoadIdsProductsType(opts);
            if (idsType == null) return 1;

            var entries = idsType.GetFields()
                .Where(f => f.IsStatic && f.IsPublic)
                .Where(f => string.IsNullOrEmpty(filter) || f.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                .Select(f => new { Name = f.Name, Type = TypeFromAttributes(f) })
                .OrderBy(e => e.Name)
                .ToList();

            if (opts.Format == "json")
            {
                Console.WriteLine(JsonSerializer.Serialize(entries, JsonOpts));
                return 0;
            }

            if (entries.Count == 0)
            {
                Console.WriteLine("(no products match)");
                return 0;
            }

            int nameWidth = entries.Max(e => e.Name.Length);
            foreach (var e in entries)
            {
                Console.WriteLine($"  {e.Name.PadRight(nameWidth)}  {e.Type}");
            }
            return 0;
        }

        private static string TypeFromAttributes(FieldInfo field)
        {
            var typeNames = field.GetCustomAttributesData()
                .Select(a => a.AttributeType.Name)
                .ToList();
            if (typeNames.Contains("FluidProductAttribute"))     return "fluid";
            if (typeNames.Contains("MoltenProductAttribute"))    return "molten";
            if (typeNames.Contains("LooseProductAttribute"))     return "loose";
            if (typeNames.Contains("CountableProductAttribute")) return "unit";
            return "unknown";
        }

        private static Type LoadIdsProductsType(Options opts)
        {
            string dllPath = ResolveBaseDllPath(opts);
            if (dllPath == null) return null;
            try
            {
                var asm = Assembly.LoadFrom(dllPath);
                var t = asm.GetType("Mafi.Base.Ids+Products");
                if (t == null)
                {
                    Console.Error.WriteLine($"[introspect] Mafi.Base.Ids+Products not found in '{dllPath}'.");
                }
                return t;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[introspect] failed to load '{dllPath}': {ex.Message}");
                return null;
            }
        }

        private static string ResolveBaseDllPath(Options opts)
        {
            // Order: --coi-root flag, then COI_ROOT env, then the dev-tree fallback.
            string root = opts.CoiRoot ?? Environment.GetEnvironmentVariable("COI_ROOT");
            if (!string.IsNullOrEmpty(root))
            {
                string p = Path.Combine(root, "Captain of Industry_Data", "Managed", "Mafi.Base.dll");
                if (File.Exists(p)) return p;
            }

            // Dev-tree fallback: the existing bin/Release output of CustomRecipes.
            string exeDir = Path.GetDirectoryName(typeof(IntrospectCommand).Assembly.Location);
            string devPath = Path.GetFullPath(Path.Combine(exeDir, "..", "..", "..", "CustomRecipes", "bin", "Release", "net48", "Mafi.Base.dll"));
            if (File.Exists(devPath)) return devPath;

            Console.Error.WriteLine($"[introspect] Mafi.Base.dll not found. Pass --coi-root <gameInstallRoot> or set COI_ROOT.");
            Console.Error.WriteLine($"  Tried: '{Path.Combine(root ?? "(env unset)", "Captain of Industry_Data", "Managed", "Mafi.Base.dll")}'");
            Console.Error.WriteLine($"  Tried: '{devPath}'");
            return null;
        }

        // ---------------------------------------------------------------------
        // Option parsing
        // ---------------------------------------------------------------------

        private sealed class Options
        {
            public string CoiRoot;
            public string CatalogPath;
            public string Format = "text";
        }

        private static Options ParseOptions(string[] args, out string positional)
        {
            positional = null;
            var opts = new Options();
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
                        opts.CoiRoot = NextValue(args, ref i, a);
                        break;
                    case "--catalog":
                        opts.CatalogPath = NextValue(args, ref i, a);
                        break;
                    case "--format":
                        opts.Format = NextValue(args, ref i, a);
                        if (opts.Format != "text" && opts.Format != "json")
                        {
                            Console.Error.WriteLine($"[introspect] --format must be 'text' or 'json', got '{opts.Format}'.");
                            return null;
                        }
                        break;
                    default:
                        if (a.StartsWith("--"))
                        {
                            Console.Error.WriteLine($"[introspect] unknown option '{a}'. See --help.");
                            return null;
                        }
                        if (positional != null)
                        {
                            Console.Error.WriteLine($"[introspect] unexpected positional '{a}' (already have '{positional}').");
                            return null;
                        }
                        positional = a;
                        break;
                }
            }
            return opts;
        }

        private static string NextValue(string[] args, ref int i, string flag)
        {
            if (i + 1 >= args.Length)
                throw new ArgumentException($"[introspect] flag '{flag}' requires a value.");
            return args[++i];
        }

        // ---------------------------------------------------------------------
        // JSON DTOs
        // ---------------------------------------------------------------------

        internal sealed class MachineCatalog
        {
            public Dictionary<string, object> Schema { get; set; }
            public Dictionary<string, MachineEntry> Machines { get; set; } = new Dictionary<string, MachineEntry>();
        }

        internal sealed class MachineEntry
        {
            public string DisplayName { get; set; }
            public Dictionary<string, string> Inputs  { get; set; }
            public Dictionary<string, string> Outputs { get; set; }
            public string Notes { get; set; }
        }
    }
}
