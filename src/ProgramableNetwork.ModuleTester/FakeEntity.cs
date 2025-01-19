using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Factory.ElectricPower;
using Mafi.Core.Population;
using Mafi.Core.Prototypes;

namespace ProgramableNetwork.ModuleTester
{
    internal class FakeEntity : Entity,
        IElectricityConsumingEntity,
        IUnityConsumingEntity,
        IEntityWithWorkers
    {
        public FakeEntity(EntityId id, EntityProto prototype, EntityContext context) : base(id, prototype, context)
        {
        }

        public override bool CanBePaused => throw new System.NotImplementedException();

        public Electricity PowerRequired => throw new System.NotImplementedException();

        public Option<IElectricityConsumerReadonly> ElectricityConsumer => throw new System.NotImplementedException();

        public int GeneralPriority => throw new System.NotImplementedException();

        public bool IsGeneralPriorityVisible => throw new System.NotImplementedException();

        public bool IsCargoAffectedByGeneralPriority => throw new System.NotImplementedException();

        public Upoints MonthlyUnityConsumed => throw new System.NotImplementedException();

        public Upoints MaxMonthlyUnityConsumed => throw new System.NotImplementedException();

        public Proto.ID UpointsCategoryId => throw new System.NotImplementedException();

        public Option<UnityConsumer> UnityConsumer => throw new System.NotImplementedException();

        public int WorkersNeeded => throw new System.NotImplementedException();

        public bool HasWorkersCached { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    }
}