using Mafi;
using Mafi.Base;
using Mafi.Core.Mods;
using Mafi.Core.Research;
using System;
using System.Collections.Generic;
using Mafi.Core.Prototypes;
using ResNodeID = Mafi.Core.Research.ResearchNodeProto.ID;
using System.Linq;
using Mafi.Core.UnlockingTree;
using Mafi.Core.Entities.Static;
using Mafi.Core.Buildings.Ramps;

namespace ProgramableNetwork
{
    public partial class NewIds
    {
        public partial class Research
        {
            public static ResNodeID Bridges = new ResNodeID("Bridges_Research");
        }
    }

    internal class Research : AValidatedData, IResearchNodesData
    {

        protected override void RegisterDataInternal(ProtoRegistrator registrator)
        {
            ResearchNodeProto research = registrator.PrototypesDb.Get<ResearchNodeProto>(Ids.Research.VehicleRamps).Value;

            var builder = registrator.ResearchNodeProtoBuilder.Start("Bridges", NewIds.Research.Bridges)
                .SetGridPosition(new Vector2i(20, 35))
                .SetCosts(ResearchCostsTpl.Build.SetDifficulty(4))
                .AddParents(research);

            for (int h = 1;	h <= 4; h++)
            {
                builder.AddLayoutEntityToUnlock(NewIds.Bridges.Floor_1x1_H(h));
                builder.AddLayoutEntityToUnlock(NewIds.Bridges.Floor_1x1_P(h));
                builder.AddLayoutEntityToUnlock(NewIds.Bridges.Ramp_1x3_R(h));
            }

            //builder.AddLayoutEntityToUnlock(NewIds.Bridges.Floor_1x1_HO);
            //builder.AddLayoutEntityToUnlock(NewIds.Bridges.Floor_1x1_PO);

            builder.BuildAndAdd();
        }
    }

    public static class ReseachExtensions
    {
        public static ResearchNodeProtoBuilder.State AddProtosToUnlockNoIcon(this ResearchNodeProtoBuilder.State builder, IEnumerable<Proto.ID> protos) {
            foreach (var item in protos)
            {
                builder.AddProtoUnlockNoIcon(item);
            }
            return builder;
        }
        public static ResearchNodeProtoBuilder.State AddProtosToUnlock<T>(this ResearchNodeProtoBuilder.State builder, IEnumerable<Proto.ID> protos)
            where T : Proto, IProtoWithIcon
        {
            foreach (var item in protos)
            {
                builder.AddProtoToUnlock<T>(item);
            }
            return builder;
        }
        public static IEnumerable<Proto.ID> WithGenericId<T>(this IEnumerable<T> protos)
        {
            foreach (var item in protos)
            {
                yield return new Proto.ID((string)item.GetType().GetField("Value").GetValue(item));
            }
        }
    }
}