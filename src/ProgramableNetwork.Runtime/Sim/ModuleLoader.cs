using System;
using System.Collections;
using System.Collections.Generic;
using Mafi;
using ProgramableNetwork.ModuleParser.Registrator.Definitions;
using ProgramableNetwork.Python;

namespace ProgramableNetwork.Runtime.Sim
{
    /// <summary>
    /// Browser-side equivalent of the mod's <c>ModuleRegistrator</c>: tokenize + parse + execute a
    /// Python module file, then read the resulting <c>classContext</c> into a <see cref="ModuleDefinition"/>
    /// (metadata for both preview and simulation) and capture the action/Init/Display methods.
    /// Mirrors ModuleRegistrator.Register so the web app sees exactly what the game registers.
    /// </summary>
    public static class ModuleLoader
    {
        /// <summary>
        /// Parse every supplied .py source. Core/* stubs are parsed for their side effects (imports)
        /// in the same shared context order the game uses; only classes deriving from <c>Module</c>
        /// become <see cref="ModuleDefinition"/>s.
        /// </summary>
        public static List<ModuleDefinition> Load(IEnumerable<PySource> sources)
        {
            List<ModuleDefinition> result = new List<ModuleDefinition>();
            foreach (PySource src in sources)
            {
                result.AddRange(LoadFile(src.FileName, src.Content));
            }
            return result;
        }

        public static List<ModuleDefinition> LoadFile(string fileName, string content)
        {
            Token[] tokens = Tokenizer.ParseString(content, fileName);
            Block block = Lexer.Parse(tokens);

            Dictionary<string, object> context = new Dictionary<string, object>();
            foreach (IStatement statement in block.statements)
            {
                statement.Execute(context);
            }

            List<ModuleDefinition> defs = new List<ModuleDefinition>();
            foreach (object value in context.Values)
            {
                if (!(value is Class c) || !Contains(c.baseTypes, typeof(Module)))
                {
                    continue;
                }
                defs.Add(Build(c));
            }
            return defs;
        }

        private static ModuleDefinition Build(Class c)
        {
            ModuleDefinition def = new ModuleDefinition
            {
                Id = c.name,
                PythonClass = c
            };

            IDictionary<string, object> ctx = c.classContext;

            def.Name = Get(ctx, "name") as string ?? c.name;
            def.Symbol = Get(ctx, "symbol") as string ?? "";
            def.Description = Get(ctx, "description") as string ?? "";

            if (ctx.TryGetValue("width", out object width))
            {
                def.Width = Expressions.__int__(width);
            }

            AddPins(Get(ctx, "inputs") as IList, def.Inputs);
            AddPins(Get(ctx, "outputs") as IList, def.Outputs);
            AddDisplays(Get(ctx, "displays") as IList, def);
            AddFields(Get(ctx, "fields") as IList, def);
            AddCategories(Get(ctx, "categories") as IList, def);

            def.InputExtensions = IntOr(ctx, "input_extensions", 0);
            def.OutputExtensions = IntOr(ctx, "output_extensions", 0);
            def.DisplayExtensions = IntOr(ctx, "display_extensions", 0);

            def.ActionMethod = (Get(ctx, "action") ?? Get(ctx, "Action")) as Method;
            def.InitMethod = Get(ctx, "Init") as Method;
            def.DisplayMethod = (Get(ctx, "display") ?? Get(ctx, "Display")) as Method;

            return def;
        }

        private static void AddPins(IList list, List<PinDef> target)
        {
            if (list == null)
            {
                return;
            }
            foreach (object item in list)
            {
                if (item is ModuleConnectorProtoDefinition d)
                {
                    target.Add(new PinDef(d.id, d.name, d.shared));
                }
            }
        }

        private static void AddDisplays(IList list, ModuleDefinition def)
        {
            if (list == null)
            {
                return;
            }
            // Each entry is a DisplayConstructorAction (lambda over ModuleProto.Builder). Run them
            // against a recording builder to recover the display metadata — reuses the real
            // DisplayConstructor (LED/Text/Icon/Slider) semantics.
            ModuleProto.Builder builder = new ModuleProto.Builder();
            foreach (object item in list)
            {
                if (item is DisplayConstructorAction action)
                {
                    action(builder);
                }
            }
            def.Displays.AddRange(builder.Displays);
        }

        private static void AddFields(IList list, ModuleDefinition def)
        {
            if (list == null)
            {
                return;
            }
            foreach (object item in list)
            {
                FieldDef f = ToFieldDef(item);
                if (f != null)
                {
                    def.Fields.Add(f);
                }
            }
        }

        private static FieldDef ToFieldDef(object item)
        {
            switch (item)
            {
                case ModuleInt32FieldProtoDefinition d:
                    return new FieldDef { Id = d.id, Name = d.name, Description = d.desc, Kind = FieldKind.Int32, Default = d.defaultValue, OverrideInput = d.overrideInput };
                case ModuleInt64FieldProtoDefinition d:
                    return new FieldDef { Id = d.id, Name = d.name, Description = d.desc, Kind = FieldKind.Int64, Default = d.defaultValue, OverrideInput = d.overrideInput };
                case ModuleFix32FieldProtoDefinition d:
                    return new FieldDef { Id = d.id, Name = d.name, Description = d.desc, Kind = FieldKind.Fix32, Default = d.defaultValue, OverrideInput = d.overrideInput };
                case ModuleBooleanFieldProtoDefinition d:
                    return new FieldDef { Id = d.id, Name = d.name, Description = d.desc, Kind = FieldKind.Boolean, Default = d.defaultValue, OverrideInput = d.overrideInput };
                case ModuleStringFieldProtoDefinition d:
                    return new FieldDef { Id = d.id, Name = d.name, Description = d.desc, Kind = FieldKind.String, Default = d.defaultValue, OverrideInput = d.overrideInput };
                case ModuleEntityFieldProtoDefinition d:
                    return new FieldDef { Id = d.id, Name = d.name, Description = d.desc, Kind = FieldKind.Entity, EntityType = d.type?.Name };
                default:
                    return null;
            }
        }

        private static void AddCategories(IList list, ModuleDefinition def)
        {
            if (list == null)
            {
                return;
            }
            foreach (object item in list)
            {
                if (item is Category cat)
                {
                    def.Categories.Add(cat.Id);
                }
            }
        }

        private static object Get(IDictionary<string, object> ctx, string key) =>
            ctx.TryGetValue(key, out object v) ? v : null;

        private static int IntOr(IDictionary<string, object> ctx, string key, int fallback) =>
            ctx.TryGetValue(key, out object v) && v != null ? Expressions.__int__(v) : fallback;

        private static bool Contains(Type[] types, Type t)
        {
            if (types == null)
            {
                return false;
            }
            foreach (Type x in types)
            {
                if (x == t)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public sealed class PySource
    {
        public string FileName;
        public string Content;

        public PySource(string fileName, string content)
        {
            FileName = fileName;
            Content = content;
        }
    }
}
