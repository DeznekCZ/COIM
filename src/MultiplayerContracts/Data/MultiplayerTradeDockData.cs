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

            //registrator.PrototypesDb.Add(new MultiplayerTradeVillageProto(
            //    id: NewIds.MultiplayerTradeVillage,
            //    strings: Proto.CreateStr(NewIds.MultiplayerTradeVillage, "Friendly captain village", "Dock of your friendly captain"),
            //    graphics: new WorldMapEntityProto.Gfx(Assets.Base.Icons.WorldMap.WoodMine_svg, Assets.Base.Icons.WorldMap.WoodMine_svg),
            //    registrator.PrototypesDb
            //));
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
