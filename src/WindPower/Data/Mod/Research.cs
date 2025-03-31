using Mafi;
using Mafi.Base;
using Mafi.Core.Mods;
using Mafi.Core.Research;
using System;
using System.Collections.Generic;
using Mafi.Core.Prototypes;
using ResNodeID = Mafi.Core.Research.ResearchNodeProto.ID;

namespace WindPower
{
    public partial class NewIds
    {
        public partial class Research
        {
            public static ResNodeID Bridges = new ResNodeID("WindPower_T1");
        }
    }

    internal class Research : AValidatedData, IResearchNodesData
    {

        protected override void RegisterDataInternal(ProtoRegistrator registrator)
        {
            ResearchNodeProto research = registrator.PrototypesDb.Get<ResearchNodeProto>(Ids.Research.Cp2Packing).Value;

            var builder = registrator.ResearchNodeProtoBuilder.Start("Wind power", NewIds.Research.Bridges)
                .SetGridPosition(new Vector2i(20, -2))
                .SetCosts(ResearchCostsTpl.Build.SetDifficulty(4))
                .AddParents(research);

            builder.AddLayoutEntityToUnlock(NewIds.WindPower.WindTurbine_T1);

            builder.BuildAndAdd();
        }
    }
}