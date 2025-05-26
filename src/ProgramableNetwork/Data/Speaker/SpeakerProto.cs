using Mafi;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramableNetwork.Data.Speaker
{
    public class SpeakerProto : LayoutEntityProto, ILayoutEntityProto, IProtoWithPropertiesUpdate
    {
        public override Type EntityType { get; } = typeof(Speaker);
        public Electricity WorkingPower { get; }
        public Electricity IddlePower { get; }
        public Fix32 WorkingDistance { get; set; }

        public SpeakerProto(ID id, Str strings, EntityLayout layout, EntityCosts costs, Gfx graphics,
            Fix32? distanceBoost = null,
            Upoints? boostCost = null,
            Electricity? workingPower = null,
            Electricity? iddlePower = null,
            IEnumerable<Tag> tags = null)
            : base(id, strings, layout, costs, graphics, constructionDurationPerProduct: Duration.FromSec(10), boostCost ?? 0.25.Upoints(), cannotBeBuiltByPlayer: false, isUnique: false, cannotBeReflected: false, autoBuildMiniZippers: false, doNotStartConstructionAutomatically: false, tags: tags)
        {
            this.WorkingPower = workingPower ?? Electricity.FromKw(5);
            this.IddlePower = iddlePower ?? Electricity.FromKw(2);
            this.WorkingDistance = distanceBoost ?? Fix32.One;
        }
    }
}
