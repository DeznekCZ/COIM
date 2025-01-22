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
            string name = this.name.ToString();

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
                            (dynamic[] args) => {
                                Type type = (Type)(object)args[0];
                                string id = (string)(object)args[1];
                                string name1 = (string)(object)args[2];
                                string desc = (string)(object)args[3];
                                Fix32 distance = (Fix32?)(object)args[4] ?? Fix32.FromInt(5);
                                return new ModuleEntityFieldProtoDefinition(
                                    type, id, name1, desc, distance
                                );
                            }, new string[] { "type", "id", "name", "desc", "defaultValue" }));
                    }
                    else if (argument.value == "Int32Field")
                    {
                        return (argument.value, new Constructor(
                            (dynamic[] args) => {
                                string id = (string)(object)args[0];
                                string name1 = (string)(object)args[1];
                                string desc = (string)(object)args[2];
                                int defaultValue = (int?)(object)args[3] ?? 0;
                                return new ModuleInt32FieldProtoDefinition(
                                    id, name1, desc, defaultValue
                                );
                            }, new string[] { "id", "name", "desc", "defaultValue" }));
                    }
                    else if (argument.value == "Int64Field")
                    {
                        return (argument.value, new Constructor(
                            (dynamic[] args) => {
                                string id = (string)(object)args[0];
                                string name1 = (string)(object)args[1];
                                string desc = (string)(object)args[2];
                                long defaultValue = (int?)(object)args[3] ?? 0;
                                return new ModuleInt64FieldProtoDefinition(
                                    id, name1, desc, defaultValue
                                );
                            }, new string[] { "id", "name", "desc", "defaultValue" }));
                    }
                    else if (argument.value == "StringField")
                    {
                        return (argument.value, new Constructor(
                            (dynamic[] args) => {
                                string id = (string)(object)args[0];
                                string name1 = (string)(object)args[1];
                                string desc = (string)(object)args[2];
                                string defaultValue = (string)(object)args[3] ?? "";
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
                            (dynamic[] args) => new ModuleConnectorProtoDefinition(
                                (string)(object)args[0],
                                (string)(object)args[1]
                            ), new string[] { "id", "name" }));
                    }
                    else if (argument.value == "Output")
                    {
                        return (argument.value, new Constructor(
                            (dynamic[] args) => new ModuleConnectorProtoDefinition(
                                (string)(object)args[0],
                                (string)(object)args[1]
                            ), new string[] { "id", "name" }));
                    }
                    else if (argument.value == "Display")
                    {
                        return (argument.value, new Constructor(
                            (dynamic[] args) => new ModuleConnectorProtoDefinition(
                                (string)(object)args[0],
                                (string)(object)args[1],
                                (int)(object)args[2],
                                (string)(object)args[3]
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

            else
            {
                throw new System.NotImplementedException();
            }
        }
    }
}