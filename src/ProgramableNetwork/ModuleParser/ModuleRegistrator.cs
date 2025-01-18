using Mafi;
using Mafi.Base;
using Mafi.Core.Buildings.Settlements;
using Mafi.Core.Buildings.Storages;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Factory.Machines;
using Mafi.Core.Mods;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProgramableNetwork.Python
{
    public class ModuleRegistrator
    {
        public static void Register(ProtoRegistrator registrator, Block block)
        {
            foreach (ClassDefinition classEntry in block.classes.Values)
            {
                if (!classEntry.baseClasses.Any(b => b.Concat == "Module"))
                    continue;

                var builder = registrator.ModuleBuilderStart(classEntry.Name);
                var variables = classEntry.Variables;

                builder.SetName(variables["name"].StringValue);
                builder.SetSymbol(variables["symbol"].StringValue);

                AddIO(builder, variables, "inputs", "Input", builder.AddInput);
                AddIO(builder, variables, "outputs", "Output", builder.AddOutput);
                AddFields(builder, variables);
                AddAction(builder, classEntry);

                // TODO search for variable of device and categories
                builder.SetGfx(Assets.Base.Products.Icons.Vegetables_svg);
                builder.AddControllerDevice();

                builder.BuildAndAdd();
            }
        }

        private static void AddIO(ModuleProto.Builder builder, Dictionary<string, IExpression> variables, string listName, string constructorName, Func<string, string, ModuleProto.Builder> add)
        {
            if (variables.TryGetValue(listName, out IExpression expression) && expression is ListValue list)
            {
                foreach (IExpression variable in list)
                {
                    if (variable is Call call && call.Name == constructorName)
                    {
                        List<IExpression> arguments = call.Arguments;
                        add(arguments[0].StringValue, arguments[1].StringValue);
                    }
                }
            }
        }

        private static void AddFields(ModuleProto.Builder builder, Dictionary<string, IExpression> variables)
        {
            if (variables.TryGetValue("fields", out IExpression expression) && expression is ListValue list)
            {
                foreach (IExpression variable in list)
                {
                    if (!(variable is Call call))
                    {
                        throw new Exception($"Unexpected expression: {variable}");
                    }

                    if (call.Name == "EntityField")
                    {
                        List<IExpression> arguments = call.Arguments;

                        // TODO fix getting of type name by import
                        EntityType entityType = (EntityType)Enum.Parse(typeof(EntityType), ((QualifiedName)arguments[0]).Concat);
                        string id = arguments[1].StringValue;
                        string name = arguments[2].StringValue;
                        string desc = arguments[3]?.StringValue ?? null;

                        switch (entityType)
                        {
                            case EntityType.Entity: builder.AddEntityField<Entity>(id, name, desc); break;
                            case EntityType.StaticEntity: builder.AddEntityField<StaticEntity>(id, name, desc); break;
                            case EntityType.StorageBase: builder.AddEntityField<StorageBase>(id, name, desc); break;
                            case EntityType.Controller: builder.AddEntityField<Controller>(id, name, desc); break;
                            case EntityType.Antena: builder.AddEntityField<Antena>(id, name, desc); break;
                            case EntityType.Machine: builder.AddEntityField<Machine>(id, name, desc); break;
                            case EntityType.SettlementHousingModule: builder.AddEntityField<SettlementHousingModule>(id, name, desc); break;
                            case EntityType.SettlementFoodModule: builder.AddEntityField<SettlementFoodModule>(id, name, desc); break;
                            case EntityType.SettlementTransformer: builder.AddEntityField<SettlementTransformer>(id, name, desc); break;
                            case EntityType.SettlementWasteModule: builder.AddEntityField<SettlementWasteModule>(id, name, desc); break;
                            case EntityType.SettlementServiceModule: builder.AddEntityField<SettlementServiceModule>(id, name, desc); break;

                            default:
                                throw new NotImplementedException($"Entity type '{entityType}' is not implemented");
                        }
                        continue;
                    }

                    if (call.Name == "Int32Field")
                    {
                        List<IExpression> arguments = call.Arguments;
                        string id = arguments[0].StringValue;
                        string name = arguments[1].StringValue;
                        string desc = arguments[2]?.StringValue ?? null;
                        int value = arguments[3]?.IntValue ?? 0;
                        builder.AddInt32Field(id, name, value);
                        continue;
                    }

                    if (call.Name == "Int64Field")
                    {
                        List<IExpression> arguments = call.Arguments;
                        string id = arguments[0].StringValue;
                        string name = arguments[1].StringValue;
                        string desc = arguments[2]?.StringValue ?? null;
                        long value = arguments[3]?.LongValue ?? 0;
                        builder.AddInt64Field(id, name, value);
                        continue;
                    }

                    if (call.Name == "StringField")
                    {
                        List<IExpression> arguments = call.Arguments;
                        string id = arguments[0].StringValue;
                        string name = arguments[1].StringValue;
                        string desc = arguments[2]?.StringValue ?? null;
                        string value = arguments[3]?.StringValue ?? "";
                        builder.AddStringField(id, name, value);
                        continue;
                    }
                }
            }
        }


        private static void AddAction(ModuleProto.Builder builder, ClassDefinition classEntry)
        {
            if (classEntry.Functions.TryGetValue("Action", out Function function)
                || classEntry.Functions.TryGetValue("action", out function))
            {
                builder.Action((module) =>
                {
                    ModuleWrapper wrapper = new ModuleWrapper(module);
                    Dictionary<string, dynamic> context = new Dictionary<string, dynamic>
                    {
                        { function.Arguments[0], wrapper }
                    };
                    function.Execute(context);
                });
            }
        }
    }
}
