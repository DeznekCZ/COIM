using Mafi;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindPower.Entity
{
    public class WindTurbineProto : LayoutEntityProto
    {
        public WindTurbineProto(ID id, Str strings, EntityLayout layout, EntityCosts costs, Gfx graphics, Electricity generatedPower, MechPower brakingPower, int gondolaHeight, Duration? constructionDurationPerProduct = null, Upoints? boostCost = null, bool cannotBeBuiltByPlayer = false, bool isUnique = false, bool cannotBeReflected = false, bool autoBuildMiniZippers = false, bool doNotStartConstructionAutomatically = false, IEnumerable<Tag> tags = null)
            : base(id, strings, layout, costs, graphics, constructionDurationPerProduct, boostCost, cannotBeBuiltByPlayer, isUnique, cannotBeReflected, autoBuildMiniZippers, doNotStartConstructionAutomatically, tags)
        {
            GeneratedPower = generatedPower;
            BrakingPower = brakingPower;
            GondolaHeight = gondolaHeight;
        }

        public override Type EntityType => typeof(WindTurbine);

        public Electricity GeneratedPower { get; }
        public MechPower BrakingPower { get; }
        public int GondolaHeight { get; }
    }
}
