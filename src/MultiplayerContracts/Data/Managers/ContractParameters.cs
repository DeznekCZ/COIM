using Mafi;
using Mafi.Core;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Serialization;
using System;
using System.Text.RegularExpressions;

namespace MultiplayerContracts
{
    [GenerateSerializer(false, null, 0)]
    public class ContractParameters
    {
        public readonly ProductQuantity Supply;
        public readonly ProductQuantity Demand;

        public ContractParameters(ProductQuantity supply, ProductQuantity demand)
        {
            this.Supply = supply;
            this.Demand = demand;
        }

        protected void SerializeData(BlobWriter writer)
        {
            writer.WriteGeneric(Supply);
            writer.WriteGeneric(Demand);
        }

        protected void DeserializeData(BlobReader reader)
        {
            reader.SetField(this, nameof(Supply), reader.ReadGenericAs<ProductQuantity>());
            reader.SetField(this, nameof(Demand), reader.ReadGenericAs<ProductQuantity>());
        }

        public string ToJSON()
        {
            return $@"{{""Supply"":{QuatityToJSON(Supply)},""Demand"":{QuatityToJSON(Demand)}}}";
        }

        private string QuatityToJSON(ProductQuantity product)
        {
            return $@"{{""Product"":""{product.Product.Id.Value}"",""Quantity"":{product.Quantity.Value}}}";
        }

        public static ContractParameters FromJSON(string json, ProtosDb protosDb)
        {
            Match supply = new Regex("\"Supply\":\\{\"product\":\"(?<sp>\\w+)\",\"Quantity\":(?<sq>\\d+)").Match(json);
            Match demand = new Regex("\"Demand\":\\{\"product\":\"(?<dp>\\w+)\",\"Quantity\":(?<dq>\\d+)").Match(json);

            return new ContractParameters(
                new ProductQuantity(
                    protosDb.Get<ProductProto>(new Proto.ID(supply.Groups["sp"].Value)).ValueOrThrow("Missing prototype"),
                    int.Parse(supply.Groups["sq"].Value).Quantity()
                ),
                new ProductQuantity(
                    protosDb.Get<ProductProto>(new Proto.ID(demand.Groups["dp"].Value)).ValueOrThrow("Missing prototype"),
                    int.Parse(demand.Groups["dq"].Value).Quantity()
                )
            );
        }

        public static void Serialize(ContractParameters value, BlobWriter writer)
        {
            if (writer.TryStartClassSerialization(value))
            {
                writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
            }
        }

        public static ContractParameters Deserialize(BlobReader reader)
        {
            if (reader.TryStartClassDeserialization(out ContractParameters value, (Func<BlobReader, Type, ContractParameters>)null))
            {
                reader.EnqueueDataDeserialization(value, s_deserializeDataDelayedAction);
            }
            return value;
        }

        private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction = delegate (object obj, BlobWriter writer)
        {
            ((ContractParameters)obj).SerializeData(writer);
        };
        private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction = delegate (object obj, BlobReader reader)
        {
            ((ContractParameters)obj).DeserializeData(reader);
        };
    }
}