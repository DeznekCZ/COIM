using Mafi;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.Terrain;

namespace MultiplayerContracts
{
    public static class MyExtensions
    {
        public static ImmutableArray<T> ToImmutableArray<T>(this Option<T> option) where T : class
        {
            return new Lyst<T>() { option.ValueOrNull }.ToImmutableArray();
        }

        public static ProductQuantity Of(this int count, ProductProto.ID id, ProtosDb protosDb) {
            return QuantityExtensions.Of(count, protosDb.Get<ProductProto>(id).ValueOrThrow($"missing product: {id.Value}"));
        }

        public static Proto.ID TradeContract(this ProductProto.ID product)
        {
            return new Proto.ID($"MPC_{product.Value}_1000");
        }

        public static ProductQuantity TradeQuantity(this ProductProto.ID product, int amount, ProtosDb protosDb)
        {
            return amount.Of(product, protosDb);
        }
    }
}
