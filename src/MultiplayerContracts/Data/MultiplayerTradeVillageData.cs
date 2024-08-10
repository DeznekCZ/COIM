using Mafi.Base;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using Mafi.Core.Products;
using Mafi.Core.World.Entities;

namespace MultiplayerContracts
{
    internal class MultiplayerTradeVillageData : AValidatedData
    {
        protected override void RegisterDataInternal(ProtoRegistrator registrator)
        {
            registrator.PrototypesDb.Add(new MultiplayerTradeVillageProto(
                id: NewIds.MultiplayerTradeVillage,
                strings: Proto.CreateStr(NewIds.MultiplayerTradeVillage, "Friendly captain village", "Dock of your friendly captain"),
                graphics: new WorldMapEntityProto.Gfx(Assets.Base.Icons.WorldMap.WoodMine_svg, Assets.Base.Icons.WorldMap.WoodMine_svg),
                registrator.PrototypesDb,
                new EntityCosts(), //((EntityCostsTpl)new EntityCostsTpl.Builder().CP(1)).MapToEntityCosts(registrator),
                (i) => new EntityCosts() //((EntityCostsTpl)new EntityCostsTpl.Builder().CP(i)).MapToEntityCosts(registrator)
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
