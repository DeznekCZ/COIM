using Mafi;
using Mafi.Base;
using Mafi.Core.Mods;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProgramableNetwork.Python
{
    public class ModuleRegistrator
    {
        public static void Register(ProtoRegistrator registrator, string file)
        {
            Token[] tokens = Tokenizer.Parse(file);
            Block block = Lexer.Parse(tokens);

            Dictionary<string, object> context = new Dictionary<string, object>();
            foreach (IStatement statement in block.statements)
            {
                statement.Execute(context);
            }

            foreach (Class classEntry in context.Values
                .Where(v => v is Class c && c.baseTypes.Contains(typeof(Module))))
            {
                var builder = registrator.ModuleBuilderStart(classEntry.name);
                builder.SetName(classEntry.classContext["name"] as string);
                builder.SetSymbol(classEntry.classContext["symbol"] as string);

                if (classEntry.classContext.TryGetValue("inputs", out object inputs))
                    AddIO(inputs as IList, builder.AddInput);
                if (classEntry.classContext.TryGetValue("outputs", out object outputs))
                    AddIO(outputs as IList, builder.AddOutput);
                if (classEntry.classContext.TryGetValue("fields", out object fields))
                    AddFields(builder, fields as IList);
                if (classEntry.classContext.TryGetValue("action", out object action) ||
                    classEntry.classContext.TryGetValue("Action", out action))
                    AddAction(builder, classEntry, action as Method);
                if (classEntry.classContext.TryGetValue("categroies", out object categories))
                    AddCategories(builder, categories as List<object>);
                if (classEntry.classContext.TryGetValue("width", out object width))
                    builder.Width(Expressions.__int__(width));

                // TODO search for variable of device and categories
                builder.SetGfx(Assets.Base.Products.Icons.Vegetables_svg);
                builder.AddControllerDevice();

                builder.BuildAndAdd();
            }
        }

        private static void AddIO(IList modules, Func<string, string, ModuleProto.Builder> add)
        {
            foreach (ModuleConnectorProtoDefinition variable in modules ?? new List<ModuleConnectorProtoDefinition>())
            {
                add(variable.id, variable.name);
            }
        }

        private static void AddFields(ModuleProto.Builder builder, IList list)
        {
            foreach (IModuleFieldProtoDefinition variable in list)
            {
                if (variable is ModuleEntityFieldProtoDefinition entityField)
                {
                    builder.AddEntityField(entityField.type, entityField.id, entityField.name, entityField.desc);
                    continue;
                }

                if (variable is ModuleInt32FieldProtoDefinition int32Field)
                {
                    builder.AddInt32Field(int32Field.id, int32Field.name, int32Field.desc, int32Field.defaultValue);
                    continue;
                }

                if (variable is ModuleInt64FieldProtoDefinition int64Field)
                {
                    builder.AddInt64Field(int64Field.id, int64Field.name, int64Field.desc, int64Field.defaultValue);
                    continue;
                }

                if (variable is ModuleFix32FieldProtoDefinition fix32Field)
                {
                    builder.AddFix32Field(fix32Field.id, fix32Field.name, fix32Field.desc, fix32Field.defaultValue);
                    continue;
                }

                if (variable is ModuleStringFieldProtoDefinition stringField)
                {
                    builder.AddStringField(stringField.id, stringField.name, stringField.desc, stringField.defaultValue);
                    continue;
                }
            }
        }


        private static void AddAction(ModuleProto.Builder builder, Class classContext, Method action)
        {
            builder.Action((module) =>
            {
                ModuleWrapper wrapper = new ModuleWrapper(module, classContext);
                action.Self = wrapper;
                object ret = Expressions.__call__(action, new List<(string name, object value)>());
                return ret is ModuleStatus status ? status : ModuleStatus.Running;
            });
        }

        private static void AddCategories(ModuleProto.Builder builder, List<object> list)
        {
            foreach (object item in list)
            {
                builder.AddCategory(item as Category);
            }
        }
    }
}
