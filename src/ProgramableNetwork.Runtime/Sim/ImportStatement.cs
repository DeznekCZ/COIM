using Mafi;
using ProgramableNetwork.ModuleParser.Registrator.Definitions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace ProgramableNetwork.Python
{
    /// <summary>
    /// Simulator-side fork of the mod's <c>ImportStatement</c> (which is excluded from the source link
    /// because it uses <c>AssemblyBuilderAccess.ReflectionOnly</c>, unavailable on modern .NET). Logic
    /// is identical to the mod's resolver — it seeds the interpreter context with the same Input/Output/
    /// Display/Field constructors and Fix32 helpers — with two adaptations:
    ///   * <c>Core.ids</c> dynamic types use <c>AssemblyBuilderAccess.Run</c> instead of ReflectionOnly,
    ///   * the per-item loop is a plain foreach (the mod's <c>.Call().ToList()</c> Mafi extension is gone).
    /// Keep in sync with src/ProgramableNetwork/ModuleParser/Statements/ImportStatement.cs.
    /// </summary>
    public class ImportStatement : IStatement
    {
        public readonly IExpression name;
        public readonly List<Token> exportedItems;

        public ImportStatement(IExpression name, List<Token> exportedItems)
        {
            this.name = name;
            this.exportedItems = exportedItems;
        }

        public void Execute(IDictionary<string, object> context)
        {
            string name = this.name.Path;

            if (name == "Core.categories")
            {
                context["DefaultCategories"] = typeof(Category);
            }
            else if (name == "Core.fields")
            {
                foreach (Token argument in exportedItems)
                {
                    context[argument.value] = FieldConstructor(argument.value);
                }
            }
            else if (name == "Core.io")
            {
                foreach (Token argument in exportedItems)
                {
                    if (argument.value == "Input" || argument.value == "Output")
                    {
                        context[argument.value] = new Constructor(
                            (IArgumentValue[] args) => new ModuleConnectorProtoDefinition(
                                AsString(args, 0),
                                AsString(args, 1),
                                BoolKwarg(args, "shared", 2, false)),
                            new string[] { "id", "name", "shared" });
                    }
                    else if (argument.value == "Display")
                    {
                        context[argument.value] = typeof(DisplayConstructor);
                    }
                    else
                    {
                        throw new NotImplementedException($"IO type '{argument.value}' is not implemented");
                    }
                }
            }
            else if (name == "Core.module")
            {
                context["Module"] = typeof(Module);
                context["DefaultControllers"] = typeof(NewIds.Controllers);
                context["ModuleStatus"] = typeof(ModuleStatus);
            }
            else if (name == "Core.template")
            {
                context["Template"] = typeof(Template);
                context["Controller"] = typeof(ControllerTemplate);
                context["ControllerTemplate"] = typeof(ControllerTemplate);
            }
            else if (name == "Core.ids")
            {
                foreach (Token item in exportedItems)
                {
                    if (item.value == "*")
                    {
                        throw new PythonParseException(item, "Cannot use * for import of captain of industry classes");
                    }
                    context[item.value] = AssemblyBuilder
                        .DefineDynamicAssembly(new AssemblyName("ProgramableNetwork_ids"), AssemblyBuilderAccess.Run)
                        .DefineDynamicModule("Modules")
                        .DefineType(item.value, TypeAttributes.Public)
                        .CreateType();
                }
            }
            else if (name == "Core.mafi")
            {
                context["fix"] = new Constructor((args) => Expressions.__fix__(args[0].Value), new string[] { "value" });
                context["int"] = new Constructor((args) => Expressions.__int__(args[0].Value), new string[] { "value" });
                context["raw"] = new Constructor((args) => Expressions.__raw__(args[0].Value), new string[] { "value" });
                context["hex"] = new Constructor((args) => Fix32.FromRaw(Expressions.__int__(args[0].Value)), new string[] { "value" });
                context["Fix32"] = typeof(Fix32);
            }
            else if (name == "Core.errors")
            {
                context["Exception"] = new Constructor((args) => new Exception(args[0].Value as string), new string[] { "value" });
            }
            else if (name.StartsWith("Mafi"))
            {
                foreach (Token item in exportedItems)
                {
                    if (item.value == "*")
                    {
                        throw new PythonParseException(item, "Cannot use * for import of captain of industry classes");
                    }
                    if (item.value == "Fix32")
                    {
                        context[item.value] = typeof(Fix32);
                        continue;
                    }
                    if (item.value == "ColorRgba")
                    {
                        context[item.value] = typeof(ColorRgba);
                        continue;
                    }
                    // Best-effort resolution of any other Mafi.* type from loaded assemblies.
                    Type resolved = ResolveType(name + "." + item.value);
                    if (resolved == null)
                    {
                        throw new NotImplementedException($"Mafi type '{name}.{item.value}' is not available in the simulator.");
                    }
                    context[item.value] = resolved;
                }
            }
            else
            {
                throw new NotImplementedException(name);
            }
        }

        private static object FieldConstructor(string kind)
        {
            switch (kind)
            {
                case "EntityField":
                    return new Constructor((IArgumentValue[] args) =>
                    {
                        Type type = AsObject(args, 0) as Type;
                        Fix32 distance = AsFixOrDefault(args, 4, Fix32.FromInt(5));
                        return new ModuleEntityFieldProtoDefinition(
                            type ?? typeof(object), AsString(args, 1), AsString(args, 2), AsString(args, 3),
                            distance, BoolAt(args, 5, false));
                    }, new string[] { "type", "id", "name", "desc", "defaultValue", "show_in_tooltip" });

                case "BooleanField":
                    return new Constructor((IArgumentValue[] args) =>
                        new ModuleBooleanFieldProtoDefinition(
                            AsString(args, 0), AsString(args, 1), AsString(args, 2),
                            BoolAt(args, 3, false), false, BoolAt(args, 4, false)),
                        new string[] { "id", "name", "desc", "defaultValue", "show_in_tooltip" });

                case "Int32Field":
                    return new Constructor((IArgumentValue[] args) =>
                        new ModuleInt32FieldProtoDefinition(
                            AsString(args, 0), AsString(args, 1), AsString(args, 2),
                            IntAt(args, 3, 0), false, BoolAt(args, 4, false)),
                        new string[] { "id", "name", "desc", "defaultValue", "show_in_tooltip" });

                case "Fix32Field":
                    return new Constructor((IArgumentValue[] args) =>
                        new ModuleFix32FieldProtoDefinition(
                            AsString(args, 0), AsString(args, 1), AsString(args, 2),
                            AsFixOrDefault(args, 3, Fix32.Zero), false, BoolAt(args, 4, false)),
                        new string[] { "id", "name", "desc", "defaultValue", "show_in_tooltip" });

                case "Int64Field":
                    return new Constructor((IArgumentValue[] args) =>
                        new ModuleInt64FieldProtoDefinition(
                            AsString(args, 0), AsString(args, 1), AsString(args, 2),
                            IntAt(args, 3, 0), false, BoolAt(args, 4, false)),
                        new string[] { "id", "name", "desc", "defaultValue", "show_in_tooltip" });

                case "StringField":
                    return new Constructor((IArgumentValue[] args) =>
                        new ModuleStringFieldProtoDefinition(
                            AsString(args, 0), AsString(args, 1), AsString(args, 2),
                            AsString(args, 3) ?? "", false, BoolAt(args, 4, false), BoolAt(args, 5, false)),
                        new string[] { "id", "name", "desc", "defaultValue", "multilined", "show_in_tooltip" });

                default:
                    throw new NotImplementedException($"IO type '{kind}' is not implemented");
            }
        }

        // ---- argument extraction helpers (positional OrderedValue, named NamedValue) ----
        private static object AsObject(IArgumentValue[] args, int i) =>
            i < args.Length && args[i] is OrderedValue o ? o.Value : null;

        private static string AsString(IArgumentValue[] args, int i) => AsObject(args, i) as string;

        private static bool BoolAt(IArgumentValue[] args, int i, bool fallback) =>
            i < args.Length && args[i] is OrderedValue o && o.Value is bool b ? b : fallback;

        private static int IntAt(IArgumentValue[] args, int i, int fallback) =>
            i < args.Length && args[i] is OrderedValue o && o.Value is int n ? n : fallback;

        private static Fix32 AsFixOrDefault(IArgumentValue[] args, int i, Fix32 fallback)
        {
            object v = AsObject(args, i);
            if (v == null)
            {
                return fallback;
            }
            return Expressions.__fix__(v);
        }

        private static bool BoolKwarg(IArgumentValue[] args, string key, int positionalIndex, bool fallback)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] is NamedValue nv && (string)nv.Name == key && nv.Value is bool nb)
                {
                    return nb;
                }
            }
            if (positionalIndex < args.Length && args[positionalIndex] is OrderedValue ov && ov.Value is bool ob)
            {
                return ob;
            }
            return fallback;
        }

        private static Type ResolveType(string fullName)
        {
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    Type t = a.GetType(fullName, false);
                    if (t != null)
                    {
                        return t;
                    }
                }
                catch
                {
                    // ignore assemblies that refuse reflection
                }
            }
            return null;
        }
    }
}
