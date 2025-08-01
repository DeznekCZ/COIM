using Mafi;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramableNetwork.Data.DisplayEntity
{
    public class DisplayEntityProto : LayoutEntityProto, ILayoutEntityProto, IProtoWithPropertiesUpdate, ILayoutEntityProtoWithElevation, IProtoWithTiers, IProtoWithUpgrade
    {
        public override Type EntityType { get; } = typeof(DisplayEntity);
        public Electricity WorkingPower { get; }
        public Electricity IddlePower { get; }
        public Fix32 WorkingDistance { get; set; }
        public Func<DisplayEntity, IDisplayEntityManager> DisplayManagerFactory { get; }
        public bool CanBeElevated { get; }
        public bool CanPillarsPassThrough { get; }
        public ITierData TierData => Upgrade.TierData;
        public UpgradeData Upgrade { get; }

        public DisplayEntityProto(ID id, Str strings, EntityLayout layout, EntityCosts costs, Gfx graphics, Func<DisplayEntity, IDisplayEntityManager> manager,
            int tierNumber,
            bool canBeElevated = true,
            bool canPillarsPassTrough = true,
            Fix32? distanceBoost = null,
            Upoints? boostCost = null,
            Electricity? workingPower = null,
            Electricity? iddlePower = null,
            IEnumerable<Tag> tags = null)
            : base(id, strings, layout, costs, graphics, constructionDurationPerProduct: Duration.FromSec(0.5), boostCost ?? 0.25.Upoints(), cannotBeBuiltByPlayer: false, isUnique: false, cannotBeReflected: false, autoBuildMiniZippers: false, doNotStartConstructionAutomatically: false, tags: tags)
        {
            this.WorkingPower = workingPower ?? Electricity.FromKw(5);
            this.IddlePower = iddlePower ?? Electricity.FromKw(2);
            this.WorkingDistance = distanceBoost ?? Fix32.One;
            this.DisplayManagerFactory = manager;
            this.CanBeElevated = canBeElevated;
            this.CanPillarsPassThrough = canPillarsPassTrough;
            this.Upgrade = new UpgradeData(this);
            this.Upgrade.TierData.TierNumberForUi = tierNumber;
        }
    }
}
