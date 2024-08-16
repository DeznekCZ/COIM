using Mafi;
using Mafi.Base;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using Mafi.Core.Products;
using Mafi.Core.Buildings.Cargo;
using Mafi.Core.World.Contracts;
using Mafi.Core.World.Entities;
using System.Linq;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Base.Prototypes.Buildings;

namespace MultiplayerContracts
{
    public partial class NewIds
    {
        public partial class Tags
        {
            public static readonly Tag MultiplayerContract = Tag.Create<ContractProto>("MultiplayerTrade");
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
                costs: Costs.Buildings.TradeDock.MapToEntityCosts(registrator),
                reservedOceanAreasSets: ShipyardData.AllApproachesAreas,
                graphics: new LayoutEntityProto.Gfx(
                    "Assets/Base/Buildings/TradeDock.prefab",
                    new RelTile3f(6, 0, 0),
                    default(Option<string>),
                    default(ColorRgba),
                    hideBlockedPortsIcon: false,
                    null,
                    registrator.GetCategoriesProtos(Ids.ToolbarCategories.Docks)
                 )
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
