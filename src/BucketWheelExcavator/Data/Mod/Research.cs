using Mafi;
using Mafi.Base;
using Mafi.Core.Mods;
using Mafi.Core.Research;
using ResNodeID = Mafi.Core.Research.ResearchNodeProto.ID;
using BucketWheelExcavator.Entity;

namespace BucketWheelExcavator
{
    public partial class NewIds
    {
        public partial class Research
        {
        }
    }

    internal class Research : AValidatedData, IResearchNodesData
    {

        protected override void RegisterDataInternal(ProtoRegistrator registrator)
        {
            registrator.PrototypesDb
                .Get<ResearchNodeProto>(Mafi.Base.Ids.Research.VehicleAssembly3)
                .Value.AddUnlockable(
                    registrator.PrototypesDb
                        .Get<BucketWheelExcavatorProto>(NewIds.BucketWheelExcavator.BucketExcavator_T1)
                        .ValueOrThrow("Missing initialization"));
        }
    }
}