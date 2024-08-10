using Mafi;
using Mafi.Base;
using Mafi.Core;
using Mafi.Core.Population;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.World.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiplayerContracts
{
    public partial class NewIds
    {
        public partial class Tags
        {
            public static readonly Tag MultiplayerVillage = Tag.Create<MultiplayerTradeVillageProto>("MultiplayerVillage");
        }
    }


    internal class MultiplayerTradeVillageProto : WorldMapMineProto
    {
        public override Type EntityType => typeof(MultiplayerTradeVillage);

        public MultiplayerTradeVillageProto(ID id, Str strings, Gfx graphics, ProtosDb protosDb, EntityCosts costs, Func<int, EntityCosts> costPerLevel, IEnumerable<Tag> tags = null)
            : base(id, strings,
                  producedProductPerStep: new ProductQuantity(protosDb.Get<ProductProto>(Ids.Products.Exhaust).ValueOrThrow("No extraus please!"), 10.Quantity()),
                  productionDuration: 60.Seconds(),
                  monthlyUpointsPerLevel: 0.Upoints(),
                  upointsCategory: protosDb.Get<UpointsCategoryProto>(new Proto.ID("UpointsCat_" + Ids.World.WaterWell.Value)).ValueOrThrow("Missing upoints category"),
                  costs: costs,
                  costPerLevel: costPerLevel,
                  maxLevel: 1,
                  quantityAvailable: 0.Quantity(),
                  graphics,
                  levelsPerUpgrade: 1,
                  startingLevel: 1,
                  tags?.Concat(new List<Tag> { NewIds.Tags.MultiplayerVillage }) ?? new List<Tag> { NewIds.Tags.MultiplayerVillage }
            )
        {
            SetAvailability(true);
        }
    }
}
