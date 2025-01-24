using Mafi;
using Mafi.Core.Buildings.Settlements;
using Mafi.Core.Buildings.Storages;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities;
using Mafi.Core.Factory.Machines;
using System;
using System.Collections.Generic;
using System.Linq;
using Mafi.Core.Prototypes;
using Mafi.Core.Population;
using Mafi.Core.Factory.ElectricPower;

namespace ProgramableNetwork.Python
{
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

            if (name == "Core.categories") // ignore other types
                context["DefaultCategories"] = typeof(Category);

            else if (name == "Core.entities")
            {
                exportedItems.Select(argument =>
                {
                    EntityType entityType = (EntityType)Enum.Parse(typeof(EntityType), argument.value);

                    switch (entityType)
                    {
                        case EntityType.Entity: return (argument.value, typeof(Entity));
                        case EntityType.StaticEntity: return (argument.value, typeof(StaticEntity));
                        case EntityType.StorageBase: return (argument.value, typeof(StorageBase));
                        case EntityType.Controller: return (argument.value, typeof(Controller));
                        case EntityType.Antena: return (argument.value, typeof(Antena));
                        case EntityType.Machine: return (argument.value, typeof(Machine));
                        case EntityType.SettlementHousingModule: return (argument.value, typeof(SettlementHousingModule));
                        case EntityType.SettlementFoodModule: return (argument.value, typeof(SettlementFoodModule));
                        case EntityType.SettlementTransformer: return (argument.value, typeof(SettlementTransformer));
                        case EntityType.SettlementWasteModule: return (argument.value, typeof(SettlementWasteModule));
                        case EntityType.SettlementServiceModule: return (argument.value, typeof(SettlementServiceModule));
                        case EntityType.IEntityWithWorkers: return (argument.value, typeof(IEntityWithWorkers));
                        case EntityType.IElectricityConsumingEntity: return (argument.value, typeof(IElectricityConsumingEntity));
                        case EntityType.IUnityConsumingEntity: return (argument.value, typeof(IUnityConsumingEntity));

                        default:
                            throw new NotImplementedException($"Entity type '{entityType}' is not implemented");
                    }
                }).Call(p => context[p.value] = p.Item2).ToList();
            }

            else if (name == "Core.fields")
            {
                exportedItems.Select(argument =>
                {
                    if (argument.value == "EntityField")
                    {
                        return (argument.value, new Constructor(
                            (IArgumentValue[] args) => {
                                Type type = (Type)(args[0] is OrderedValue o0 ? o0.Value : null);
                                string id = (string)(args[1] is OrderedValue o1 ? o1.Value : null);
                                string name1 = (string)(args[2] is OrderedValue o2 ? o2.Value : null);
                                string desc = (string)(args[3] is OrderedValue o3 ? o3.Value : null);

                                object dist = args.Length > 4 ? args[4] : null;
                                dist = dist is OrderedValue o4 ? o4.Value : null;

                                Fix32 distance = dist is null ? Fix32.FromInt(5)
                                               : dist is Fix32 fix32 ? fix32
                                               : dist is int i ? i.ToFix32()
                                               : dist is float f ? f.ToFix32()
                                               : Fix32.FromInt(5);

                                return new ModuleEntityFieldProtoDefinition(
                                    type, id, name1, desc, distance
                                );
                            }, new string[] { "type", "id", "name", "desc", "defaultValue" }));
                    }
                    else if (argument.value == "Int32Field")
                    {
                        return (argument.value, new Constructor(
                            (IArgumentValue[] args) => {
                                string id = (string)(args[0] is OrderedValue o0 ? o0.Value : null);
                                string name1 = (string)(args[1] is OrderedValue o1 ? o1.Value : null);
                                string desc = (string)(args[2] is OrderedValue o2 ? o2.Value : null);
                                int defaultValue = (int?)(args.Length > 3 && args[3] is OrderedValue o3 ? o3.Value : null) ?? 0;
                                return new ModuleInt32FieldProtoDefinition(
                                    id, name1, desc, defaultValue
                                );
                            }, new string[] { "id", "name", "desc", "defaultValue" }));
                    }
                    else if (argument.value == "Int64Field")
                    {
                        return (argument.value, new Constructor(
                            (IArgumentValue[] args) => {
                                string id = (string)(args[0] is OrderedValue o0 ? o0.Value : null);
                                string name1 = (string)(args[1] is OrderedValue o1 ? o1.Value : null);
                                string desc = (string)(args[2] is OrderedValue o2 ? o2.Value : null);
                                long defaultValue = (int?)(args[3] is OrderedValue o3 ? o3.Value : null) ?? 0;
                                return new ModuleInt64FieldProtoDefinition(
                                    id, name1, desc, defaultValue
                                );
                            }, new string[] { "id", "name", "desc", "defaultValue" }));
                    }
                    else if (argument.value == "StringField")
                    {
                        return (argument.value, new Constructor(
                            (IArgumentValue[] args) => {
                                string id = (string)(args[0] is OrderedValue o0 ? o0.Value : null);
                                string name1 = (string)(args[1] is OrderedValue o1 ? o1.Value : null);
                                string desc = (string)(args[2] is OrderedValue o2 ? o2.Value : null);
                                string defaultValue = (string)(args[3] is OrderedValue o3 ? o3.Value : null) ?? "";
                                return new ModuleStringFieldProtoDefinition(
                                    id, name1, desc, defaultValue
                                );
                            }, new string[] { "id", "name", "desc", "defaultValue" }));
                    }
                    else
                    {
                        throw new NotImplementedException($"IO type '{argument.value}' is not implemented");
                    }
                }).Call(p => context[p.value] = p.Item2).ToList();
            }

            else if (name == "Core.io")
            {
                exportedItems.Select(argument =>
                {
                    if (argument.value == "Input")
                    {
                        return (argument.value, new Constructor(
                            (IArgumentValue[] args) => new ModuleConnectorProtoDefinition(
                                (string)(args[0] is OrderedValue o0 ? o0.Value : null),
                                (string)(args[1] is OrderedValue o1 ? o1.Value : null)
                            ), new string[] { "id", "name" }));
                    }
                    else if (argument.value == "Output")
                    {
                        return (argument.value, new Constructor(
                            (IArgumentValue[] args) => new ModuleConnectorProtoDefinition(
                                (string)(args[0] is OrderedValue o0 ? o0.Value : null),
                                (string)(args[1] is OrderedValue o1 ? o1.Value : null)
                            ), new string[] { "id", "name" }));
                    }
                    else if (argument.value == "Display")
                    {
                        return (argument.value, new Constructor(
                            (IArgumentValue[] args) => new ModuleConnectorProtoDefinition(
                                (string)(args[0] is OrderedValue o0 ? o0.Value : null),
                                (string)(args[1] is OrderedValue o1 ? o1.Value : null),
                                (int)(args[2] is OrderedValue o2 ? o2.Value : null),
                                (string)(args[3] is OrderedValue o3 ? o3.Value : null)
                            ), new string[] { "id", "name", "digits", "defaultValue" }));
                    }
                    else
                    {
                        throw new NotImplementedException($"IO type '{argument.value}' is not implemented");
                    }
                }).Call(p => context[p.value] = p.Item2).ToList();
            }

            else if (name == "Core.module")
            {
                context["Module"] = typeof(Module);
                context["DefaultControllers"] = typeof(NewIds.Controllers);
            }

            else if (name == "Core.mafi")
            {
                context["fix"] = new Constructor((args) => Expressions.__fix__(args[0].Value), new string[] { "value" });
                context["Fix32"] = typeof(Fix32);
            }

            else
            {
                throw new System.NotImplementedException();
            }
        }
    }
}