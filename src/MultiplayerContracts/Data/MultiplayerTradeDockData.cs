using Mafi;
using Mafi.Base;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using Mafi.Core.Products;
using Mafi.Core.Buildings.Cargo;
using Mafi.Core.World.Contracts;
using Mafi.Core.World.Entities;
using System.Linq;

namespace MultiplayerContracts
{
    public partial class NewIds
    {
        public partial class Tags
        {
            public static readonly Tag MultiplayerContract = Tag.Create<ContractProto>("MultiplayerTrade");
            public static readonly Tag MultiplayerVillage = Tag.Create<WorldMapVillageProto>("MultiplayerVillage");
        }
    }

    internal class MultiplayerTradeDockData : AValidatedData
    {
        protected override void RegisterDataInternal(ProtoRegistrator registrator)
        {
            var originalProto = registrator.PrototypesDb.Get<TradeDockProto>(Ids.Buildings.TradeDock).ValueOrThrow("Missing original trade dock");

            registrator.PrototypesDb.Add(new MultiplayerTradeDockProto(
                id: NewIds.MultiplayerTradeDock,
                strings: Proto.CreateStr(NewIds.MultiplayerTradeDock, "Friendly captain dock", "Dock of your friendly captain"),
                layout: originalProto.Layout,
                costs: originalProto.Costs,
                reservedOceanAreasSets: originalProto.ReservedOceanAreasSets,
                graphics: originalProto.Graphics
            ));

            var productsToTrade = registrator.PrototypesDb.All<ProductProto>().ToList();
            var amounts = new int[] { 100, 250 };
            int totalContracts = 0;
            int totalUniqueContracts = 0;
            foreach (var product1 in productsToTrade)
            {
                if (!product1.IsStorable) continue;

                foreach (var product2 in productsToTrade)
                {
                    if (!product2.IsStorable) continue;
                    if (product1.Id == product2.Id) continue;

                    totalUniqueContracts++;

                    foreach (var amount1 in amounts)
                    {
                        foreach (var amount2 in amounts)
                        {
                            totalContracts++;
                            registrator.PrototypesDb.Add(new MultiplayerContractProto(
                                id: TradeContract.Id(product1.Id, amount1, product2.Id, amount2),
                                productToBuy: product1.Id.TradeQuantity(amount1, registrator.PrototypesDb),
                                productToPayWith: product2.Id.TradeQuantity(amount2, registrator.PrototypesDb),
                                upointsPerMonth: 0.05.Upoints(),
                                upointsPer100ProductsBought: 0.01.Upoints(),
                                minReputationRequired: 0
                            ));
                        }
                    }
                }
            }

            Log.Debug($"{ModDefinition.ModName}: Total contracts: {totalContracts}, TotalUnique: {totalUniqueContracts}");

            registrator.PrototypesDb.Add(new MultiplayerTradeVillageProto(
                id: NewIds.MultiplayerTradeVillage,
                strings: Proto.CreateStr(NewIds.MultiplayerTradeVillage, "Friendly captain village", "Dock of your friendly captain"),
                graphics: new WorldMapEntityProto.Gfx(Assets.Base.Icons.WorldMap.WoodMine_svg, Assets.Base.Icons.WorldMap.WoodMine_svg),
                registrator.PrototypesDb
            ));
        }
    }

    public class TradeContract
    {
        public static Proto.ID Id(ProductProto.ID id1, int amount1, ProductProto.ID id2, int amount2)
        {
            return new Proto.ID($"MTC_{id1.Value}_{amount1}_{id2.Value}_{amount2}");
        }
    }
}
