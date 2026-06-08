using Mafi;
using Mafi.Core.Mods;
using Mafi.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using ProgramableNetwork.Ui;

namespace ProgramableNetwork
{
    /// <summary>
    /// Generates <c>ids.py</c> — a loadable Python descriptor of EVERY registered module prototype
    /// (both the Python-authored Custom modules AND the ones defined directly in C#). The web app
    /// parses it with the same interpreter it uses for the Custom files, so all modules appear in the
    /// preview/palette. Descriptor classes derive from <c>Module</c> and use the normal
    /// Input/Output/Display/*Field constructors but carry NO <c>action</c> — they're metadata only,
    /// so C# modules show in the webview but are inert in the simulator (their logic is compiled C#).
    ///
    /// Generation runs on demand from <see cref="ModWebExport"/> (pn_exportWebData), never at load.
    /// </summary>
    public class ModuleIdsGenerator : IModData
    {
        // Intentionally a no-op: ids.py is produced on demand by the export command, not at load time.
        public void RegisterData(ProtoRegistrator registrator)
        {
        }

        public static string Generate(IEnumerable<ModuleProto> modules)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# generated automatically from known prototypes (metadata only, no behavior)");
            sb.AppendLine("from Core.module import Module");
            sb.AppendLine("from Core.categories import DefaultCategories");
            sb.AppendLine("from Core.io import Input, Output, Display");
            sb.AppendLine("from Core.fields import Int32Field, Fix32Field, Int64Field, BooleanField, StringField");
            sb.AppendLine();

            foreach (ModuleProto item in modules.OrderBy(m => m.Id.Value))
            {
                string id = SafeIdentifier(item.Id.Value.Replace("ProgramableNetwork_Module_", ""));
                sb.AppendLine($"class {id}(Module):");
                sb.AppendLine($"    name = {Lit(item.Strings.Name.TranslatedString)}");
                sb.AppendLine($"    symbol = {Lit(item.Symbol ?? "")}");
                sb.AppendLine($"    width = {item.BaseWidth}");
                sb.AppendLine($"    description = {Lit(SafeDesc(item))}");

                string cats = string.Join(", ", item.Categories
                    .Select(CategoryMember)
                    .Where(m => m != null)
                    .Distinct()
                    .Select(m => "DefaultCategories." + m));
                if (cats.Length > 0)
                {
                    sb.AppendLine($"    categories = [{cats}]");
                }

                sb.AppendLine($"    inputs = [{string.Join(", ", item.Inputs.Select(c => $"Input({Lit(c.Id)}, {LitLoc(c.Name.Name)})"))}]");
                sb.AppendLine($"    outputs = [{string.Join(", ", item.Outputs.Select(c => $"Output({Lit(c.Id)}, {LitLoc(c.Name.Name)})"))}]");

                string fields = string.Join(", ", item.Fields.Select(FieldCtor).Where(s => s != null));
                if (fields.Length > 0)
                {
                    sb.AppendLine($"    fields = [{fields}]");
                }

                string displays = string.Join(", ", item.Displays.Select(DisplayCtor).Where(s => s != null));
                if (displays.Length > 0)
                {
                    sb.AppendLine($"    displays = [{displays}]");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        // ---- field constructors (only the kinds the web ImportStatement understands) ----
        private static string FieldCtor(IField f)
        {
            if (f == null)
            {
                return null;
            }
            string id = Lit(f.Id);
            string name = LitLoc(f.Name);
            Type t = f.GetType();

            if (t == typeof(BooleanField))
            {
                bool d = ((BooleanField)f).Default;
                return $"BooleanField({id}, {name}, \"\", {(d ? "True" : "False")})";
            }
            if (t == typeof(StringField))
            {
                string d = ((StringField)f).Default ?? "";
                return $"StringField({id}, {name}, \"\", {Lit(d)})";
            }
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(NumberField<>))
            {
                Type arg = t.GetGenericArguments()[0];
                object def = t.GetProperty("Default")?.GetValue(f);
                if (arg == typeof(Fix32))
                {
                    float v = def is Fix32 fx ? fx.ToFloat() : 0f;
                    return $"Fix32Field({id}, {name}, \"\", {v.ToString(CultureInfo.InvariantCulture)})";
                }
                if (arg == typeof(long))
                {
                    long lv = TryToLong(def);
                    return $"Int64Field({id}, {name}, \"\", {lv})";
                }
                return $"Int32Field({id}, {name}, \"\", {TryToInt(def)})";
            }
            // Entity / Product / Color / Custom / Info fields: skipped — the web app has no editor for
            // them and they are not needed for preview.
            return null;
        }

        // ---- display constructors (kind encoded in ModuleConnectorProto.DefaultText) ----
        private static string DisplayCtor(ModuleConnectorProto d)
        {
            if (d == null || string.IsNullOrEmpty(d.Id))
            {
                return null;
            }
            string id = Lit(d.Id);
            string name = LitLoc(d.Name.Name);
            string dt = d.DefaultText ?? "";
            if (dt.StartsWith("[led]"))
            {
                return $"Display.LED({id}, {name})";
            }
            if (dt.StartsWith("[image]"))
            {
                return $"Display.Icon({id}, {name}, \"\")";
            }
            return $"Display.Text({id}, {name}, \"\")";
        }

        // Maps a category to the first matching static member name on this mod's Category type so the
        // descriptor can emit `DefaultCategories.<Member>` (the web shim exposes the same members).
        private static Dictionary<string, string> s_catMembers;

        private static string CategoryMember(Category c)
        {
            if (s_catMembers == null)
            {
                s_catMembers = new Dictionary<string, string>();
                foreach (PropertyInfo p in typeof(Category).GetProperties(BindingFlags.Public | BindingFlags.Static))
                {
                    if (p.PropertyType == typeof(Category))
                    {
                        Category cat = (Category)p.GetValue(null);
                        if (cat != null && !s_catMembers.ContainsKey(cat.Id))
                        {
                            s_catMembers[cat.Id] = p.Name;
                        }
                    }
                }
            }
            return c != null && s_catMembers.TryGetValue(c.Id, out string member) ? member : null;
        }

        private static int TryToInt(object o)
        {
            try { return Convert.ToInt32(o); }
            catch { return 0; }
        }

        private static long TryToLong(object o)
        {
            try { return Convert.ToInt64(o); }
            catch { return 0; }
        }

        private static string SafeDesc(ModuleProto item)
        {
            try { return item.Strings.DescShort.TranslatedString; }
            catch { return ""; }
        }

        // Class name must be a valid Python identifier.
        private static string SafeIdentifier(string id)
        {
            StringBuilder sb = new StringBuilder(id.Length);
            foreach (char ch in id)
            {
                sb.Append(char.IsLetterOrDigit(ch) || ch == '_' ? ch : '_');
            }
            string result = sb.ToString();
            if (result.Length == 0 || char.IsDigit(result[0]))
            {
                result = "_" + result;
            }
            return result;
        }

        private static string LitLoc(LocStr s) => Lit(((LocStrFormatted)s).Value);

        // The module interpreter's tokenizer matches strings as `"[^"]*"` — it supports NO escape
        // sequences and NO triple-quotes, and reads line-by-line. So we cannot escape a `"`; we must
        // replace it (and any newline) so the literal stays a single, well-formed token.
        private static string Lit(string s)
        {
            if (s == null)
            {
                return "\"\"";
            }
            string clean = s
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("\"", "'");
            return "\"" + clean + "\"";
        }
    }
}
