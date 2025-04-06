using Mafi;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Entities.Dynamic;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.PathFinding;
using Mafi.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BucketWheelExcavator.Entity
{
    public class BucketWheelExcavatorProto : LayoutEntityProto
    {
        public BucketWheelExcavatorProto(ID id, Str strings, EntityLayout layout, EntityCosts costs, Gfx graphics, ImmutableArray<FuelTankProto> fuelTanks, int workersNeeded, bool cannotBeReflected, Duration? constructionDurationPerProduct)
            : base(id, strings, layout, costs, graphics, cannotBeReflected: cannotBeReflected, constructionDurationPerProduct: constructionDurationPerProduct)
        {
            FuelTanks = fuelTanks;
            WorkersNeeded = workersNeeded;
        }

        public override Type EntityType => typeof(BucketWheelExcavator);

        public ImmutableArray<FuelTankProto> FuelTanks { get; }
        public int WorkersNeeded { get; }
    }
}
