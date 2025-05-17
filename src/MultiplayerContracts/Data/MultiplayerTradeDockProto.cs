using Mafi;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Buildings.Cargo;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Prototypes;
using Mafi.Core.Terrain;
using Mafi.Serialization;
using System;

namespace MultiplayerContracts
{
    public class MultiplayerTradeDockProto : TradeDockProto
    {
        public override Type EntityType => typeof(MultiplayerTradeDock);

        public Quantity Capacity { get; }

        public MultiplayerTradeDockProto(ID id, Str strings, EntityLayout layout, EntityCosts costs, ImmutableArray<ImmutableArray<RectangleTerrainArea2iRelative>> reservedOceanAreasSets, Gfx graphics, Quantity? capacity = null)
            : base(id, strings, layout, costs, reservedOceanAreasSets, graphics)
        {
            Capacity = capacity ?? 1000.Quantity();
        }
    }
}