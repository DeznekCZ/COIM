using Mafi;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Economy;
using Mafi.Core.Prototypes;
using Mafi.Core.World.Contracts;
using Mafi.Core.World.Entities;
using Mafi.Core.World.QuickTrade;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiplayerContracts
{
    internal class MultiplayerTradeVillageProto : WorldMapVillageProto
    {
        public override Type EntityType => typeof(MultiplayerTradeVillage);

        public MultiplayerTradeVillageProto(ID id, Str strings, Gfx graphics, ProtosDb protosDb, IEnumerable<Tag> tags = null)
            : base(id, strings,
                  minReputationNeededToAdopt: 0,
                  startingReputation: 1,
                  upointsPerPopToAdopt: 100.Upoints(),
                  costPerLevel: (i) => AssetValue.Empty,
                  quickTrades: new List<QuickTradePairProto>().ToImmutableArray(),
                  contracts: protosDb.All<MultiplayerContractProto>().Select(proto => (ContractProto)proto).ToImmutableArray(),
                  productsToLend: new List<ProductToLend>().ToImmutableArray(),
                  graphics,
                  tags?.Concat(new List<Tag> { NewIds.Tags.MultiplayerVillage }) ?? new List<Tag> { NewIds.Tags.MultiplayerVillage }
            )
        {
        }
    }
}
