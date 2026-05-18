using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace CustomAssets.ModBuilder.Commands
{
    /// <summary>
    /// Reflection inspector for COI assemblies. Used to investigate type shapes
    /// (fields, properties, methods, enum/wrapper values) without hand-rolling
    /// PowerShell or external decompilers.
    /// </summary>
    internal static class ReflectCommand
    {
        private const string Usage =
@"ModBuilder reflect - inspect types in the COI assemblies.

USAGE
    ModBuilder.exe reflect type    <fullName>   Dump fields + properties + methods.
    ModBuilder.exe reflect fields  <fullName>   List instance + static fields.
    ModBuilder.exe reflect props   <fullName>   List properties.
    ModBuilder.exe reflect methods <fullName>   List methods (skip property accessors).
    ModBuilder.exe reflect statics <fullName>   List static field values (incl. wrapper enums
                                                 like Mafi.Core.Products.ProductType).
    ModBuilder.exe reflect find    <pattern>    Find types whose simple/full name contains pattern.

OPTIONS
    --coi-root <path>     Game install root. Default: $env:COI_ROOT or the dev-tree fallback.
    --dll <name>          Restrict to a single DLL (Mafi.dll, Mafi.Core.dll, Mafi.Base.dll).
                          Default: search all three.
    --bindings <flags>    Comma-separated subset of: public, nonpublic, static, instance, declared.
                          Default: public,nonpublic,static,instance (everything).
    --format <text|json>  Default: text.
    -h, --help            Show this help.

EXAMPLES
    ModBuilder.exe reflect type Mafi.Core.Ports.Io.IoPortTemplate
    ModBuilder.exe reflect statics Mafi.Core.Products.ProductType
    ModBuilder.exe reflect find PortShape
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
            if (string.IsNullOrEmpty(positional))
            {
                Console.Error.WriteLine($"[reflect {subcommand}] missing argument. See --help.");
                return 1;
            }

            var assemblies = LoadAssemblies(opts);
            if (assemblies == null) return 1;

            switch (subcommand)
            {
                case "type":    return DumpType(assemblies, positional, opts, fields: true, props: true, methods: true, statics: false);
                case "fields":  return DumpType(assemblies, positional, opts, fields: true, props: false, methods: false, statics: false);
                case "props":   return DumpType(assemblies, positional, opts, fields: false, props: true, methods: false, statics: false);
                case "methods": return DumpType(assemblies, positional, opts, fields: false, props: false, methods: true, statics: false);
                case "statics": return DumpType(assemblies, positional, opts, fields: false, props: false, methods: false, statics: true);
                case "find":    return FindTypes(assemblies, positional, opts);
                default:
                    Console.Error.WriteLine($"[reflect] unknown subcommand '{subcommand}'. See --help.");
                    return 1;
            }
        }

        // ---------------------------------------------------------------------
        // Assembly loading
        // ---------------------------------------------------------------------

        private static List<Assembly> LoadAssemblies(Options opts)
        {
            string baseDir = ResolveBaseDir(opts);
            if (baseDir == null) return null;

            var names = opts.Dll != null
                ? new[] { opts.Dll }
                : new[] { "Mafi.dll", "Mafi.Core.dll", "Mafi.Base.dll" };

            var list = new List<Assembly>();
            foreach (var n in names)
            {
                string path = Path.Combine(baseDir, n);
                if (!File.Exists(path))
                {
                    Console.Error.WriteLine($"[reflect] DLL not found: {path}");
                    continue;
                }
                try { list.Add(Assembly.LoadFrom(path)); }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[reflect] failed to load '{path}': {ex.Message}");
                }
            }
            if (list.Count == 0)
            {
                Console.Error.WriteLine($"[reflect] no assemblies loaded from '{baseDir}'.");
                return null;
            }
            return list;
        }

        private static string ResolveBaseDir(Options opts)
        {
            string root = opts.CoiRoot ?? Environment.GetEnvironmentVariable("COI_ROOT");
            if (!string.IsNullOrEmpty(root))
            {
                string managed = Path.Combine(root, "Captain of Industry_Data", "Managed");
                if (Directory.Exists(managed)) return managed;
            }
            string exeDir = Path.GetDirectoryName(typeof(ReflectCommand).Assembly.Location);
            string devDir = Path.GetFullPath(Path.Combine(exeDir, "..", "..", "..", "CustomRecipes", "bin", "Release", "net48"));
            if (Directory.Exists(devDir)) return devDir;
            Console.Error.WriteLine("[reflect] Could not locate COI assemblies. Pass --coi-root or set COI_ROOT.");
            return null;
        }

        // ---------------------------------------------------------------------
        // Resolve a Type by full name across loaded assemblies
        // ---------------------------------------------------------------------

        private static Type ResolveType(List<Assembly> assemblies, string fullName)
        {
            foreach (var a in assemblies)
            {
                var t = a.GetType(fullName);
                if (t != null) return t;
            }
            return null;
        }

        // ---------------------------------------------------------------------
        // Dump commands
        // ---------------------------------------------------------------------

        private sealed class FieldEntry   { public string name; public string type; public bool isStatic; public bool isPublic; }
        private sealed class PropEntry    { public string name; public string type; }
        private sealed class MethodEntry  { public string name; public string returnType; public bool isStatic; public List<string> parameters; }
        private sealed class StaticEntry  { public string name; public string type; public string value; }
        private sealed class TypeReport
        {
            public string fullName;
            public string assembly;
            public bool isEnum;
            public bool isClass;
            public bool isValueType;
            public List<FieldEntry>  fields;
            public List<PropEntry>   properties;
            public List<MethodEntry> methods;
            public List<StaticEntry> staticFields;
        }

        private static int DumpType(List<Assembly> assemblies, string typeName, Options opts,
                                    bool fields, bool props, bool methods, bool statics)
        {
            var t = ResolveType(assemblies, typeName);
            if (t == null)
            {
                Console.Error.WriteLine($"[reflect] type not found: '{typeName}'");
                Console.Error.WriteLine("  Try `reflect find <pattern>` to search by substring.");
                return 1;
            }

            var binding = opts.Bindings;
            var report = new TypeReport
            {
                fullName = t.FullName,
                assembly = t.Assembly.GetName().Name,
                isEnum = t.IsEnum,
                isClass = t.IsClass,
                isValueType = t.IsValueType
            };

            if (fields)
            {
                report.fields = t.GetFields(binding)
                    .Select(f => new FieldEntry { name = f.Name, type = f.FieldType.Name, isStatic = f.IsStatic, isPublic = f.IsPublic })
                    .ToList();
            }

            if (props)
            {
                report.properties = t.GetProperties(binding)
                    .Select(p => new PropEntry { name = p.Name, type = p.PropertyType.Name })
                    .ToList();
            }

            if (methods)
            {
                report.methods = t.GetMethods(binding)
                    .Where(m => !m.IsSpecialName)
                    .Select(m => new MethodEntry {
                        name = m.Name,
                        returnType = m.ReturnType.Name,
                        isStatic = m.IsStatic,
                        parameters = m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}").ToList()
                    })
                    .ToList();
            }

            if (statics)
            {
                report.staticFields = new List<StaticEntry>();
                foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                {
                    string value;
                    try { value = f.GetValue(null)?.ToString() ?? "(null)"; }
                    catch { value = "(unreadable)"; }
                    report.staticFields.Add(new StaticEntry { name = f.Name, type = f.FieldType.Name, value = value });
                }
            }

            if (opts.Format == "json")
            {
                Console.WriteLine(JsonSerializer.Serialize(report, JsonOpts));
                return 0;
            }

            // Text format
            Console.WriteLine($"Type: {t.FullName}");
            Console.WriteLine($"  Assembly: {t.Assembly.GetName().Name}");
            Console.WriteLine($"  IsEnum: {t.IsEnum}   IsValueType: {t.IsValueType}   IsClass: {t.IsClass}");

            if (report.fields != null)
            {
                Console.WriteLine();
                Console.WriteLine("  Fields:");
                foreach (var e in report.fields)
                    Console.WriteLine($"    {(e.isStatic ? "static " : "       ")}{e.type,-30} {e.name}");
            }
            if (report.properties != null)
            {
                Console.WriteLine();
                Console.WriteLine("  Properties:");
                foreach (var e in report.properties)
                    Console.WriteLine($"    {e.type,-30} {e.name}");
            }
            if (report.methods != null)
            {
                Console.WriteLine();
                Console.WriteLine("  Methods:");
                foreach (var e in report.methods)
                {
                    var paramStr = string.Join(", ", e.parameters);
                    Console.WriteLine($"    {(e.isStatic ? "static " : "       ")}{e.returnType,-20} {e.name}({paramStr})");
                }
            }
            if (report.staticFields != null)
            {
                Console.WriteLine();
                Console.WriteLine("  Static fields (with values):");
                foreach (var e in report.staticFields)
                    Console.WriteLine($"    {e.type,-30} {e.name,-30} = {e.value}");
            }
            return 0;
        }

        private static int FindTypes(List<Assembly> assemblies, string pattern, Options opts)
        {
            var hits = assemblies
                .SelectMany(a => SafeGetTypes(a).Select(t => new { Assembly = a.GetName().Name, Type = t }))
                .Where(x => x.Type.FullName != null
                    && x.Type.FullName.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(x => x.Type.FullName)
                .ToList();

            if (opts.Format == "json")
            {
                Console.WriteLine(JsonSerializer.Serialize(hits.Select(h => new { assembly = h.Assembly, fullName = h.Type.FullName }), JsonOpts));
                return 0;
            }

            if (hits.Count == 0) { Console.WriteLine("(no types match)"); return 0; }
            int asmWidth = hits.Max(h => h.Assembly.Length);
            foreach (var h in hits)
            {
                Console.WriteLine($"  {h.Assembly.PadRight(asmWidth)}  {h.Type.FullName}");
            }
            return 0;
        }

        private static IEnumerable<Type> SafeGetTypes(Assembly a)
        {
            try { return a.GetTypes(); }
            catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null); }
        }

        // ---------------------------------------------------------------------
        // Options
        // ---------------------------------------------------------------------

        private sealed class Options
        {
            public string CoiRoot;
            public string Dll;
            public BindingFlags Bindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
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
                    case "--coi-root": opts.CoiRoot = NextValue(args, ref i, a); break;
                    case "--dll":      opts.Dll     = NextValue(args, ref i, a); break;
                    case "--bindings": opts.Bindings = ParseBindings(NextValue(args, ref i, a)); break;
                    case "--format":
                        opts.Format = NextValue(args, ref i, a);
                        if (opts.Format != "text" && opts.Format != "json")
                        {
                            Console.Error.WriteLine($"[reflect] --format must be 'text' or 'json'");
                            return null;
                        }
                        break;
                    default:
                        if (a.StartsWith("--"))
                        {
                            Console.Error.WriteLine($"[reflect] unknown option '{a}'.");
                            return null;
                        }
                        if (positional != null)
                        {
                            Console.Error.WriteLine($"[reflect] unexpected extra positional '{a}'.");
                            return null;
                        }
                        positional = a;
                        break;
                }
            }
            return opts;
        }

        private static BindingFlags ParseBindings(string spec)
        {
            BindingFlags f = 0;
            foreach (var part in spec.Split(','))
            {
                switch (part.Trim().ToLowerInvariant())
                {
                    case "public":    f |= BindingFlags.Public; break;
                    case "nonpublic": f |= BindingFlags.NonPublic; break;
                    case "static":    f |= BindingFlags.Static; break;
                    case "instance":  f |= BindingFlags.Instance; break;
                    case "declared":  f |= BindingFlags.DeclaredOnly; break;
                }
            }
            return f;
        }

        private static string NextValue(string[] args, ref int i, string flag)
        {
            if (i + 1 >= args.Length)
                throw new ArgumentException($"[reflect] flag '{flag}' requires a value.");
            return args[++i];
        }

        private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNameCaseInsensitive = true
        };
    }
}
