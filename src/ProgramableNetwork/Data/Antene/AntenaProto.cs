using Mafi;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Prototypes;
using System;
using System.Collections.Generic;

namespace ProgramableNetwork
{

    public class AntenaProto : LayoutEntityProto, ILayoutEntityProto, IProtoWithPropertiesUpdate, IProtoWithTiers
    {
        public override Type EntityType { get; } = typeof(Antena);
        public Electricity WorkingPower { get; }
        public Electricity IddlePower { get; }
        public Fix32 DistanceBoost { get; set; }

        public ITierData TierData { get; }

        public AntenaProto(ID id, Str strings, EntityLayout layout, EntityCosts costs, Gfx graphics, int tier,
            Fix32? distanceBoost = null,
            Upoints? boostCost = null,
            Electricity? workingPower = null,
            Electricity? iddlePower = null,
            IEnumerable<Tag> tags = null)
            : base(id, strings, layout, costs, graphics, constructionDurationPerProduct: Duration.FromSec(10), boostCost ?? 0.25.Upoints(), cannotBeBuiltByPlayer: false, isUnique: false, cannotBeReflected: false, autoBuildMiniZippers: false, doNotStartConstructionAutomatically: false, tags: tags)
        {
            this.WorkingPower = workingPower ?? Electricity.FromKw(5);
            this.IddlePower = iddlePower ?? Electricity.FromKw(2);
            this.DistanceBoost = distanceBoost ?? Fix32.One;
            this.TierData = new TierData(this, tier);
        }
    }
}
