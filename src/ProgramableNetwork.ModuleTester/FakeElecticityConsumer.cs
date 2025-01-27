using Mafi;
using Mafi.Core.Factory.ElectricPower;

namespace ProgramableNetwork.ModuleTester
{
    internal class FakeElecticityConsumer : IElectricityConsumerReadonly
    {
        private FakeEntity fakeEntity;

        public FakeElecticityConsumer(FakeEntity fakeEntity)
        {
            this.fakeEntity = fakeEntity;
        }

        public bool IsEnabled => throw new System.NotImplementedException();

        public int Priority => throw new System.NotImplementedException();

        public bool NotEnoughPower => false;

        public bool DidConsumeLastTick => throw new System.NotImplementedException();

        public IElectricityConsumingEntity Entity => throw new System.NotImplementedException();

        public bool IsSurplusConsumer => throw new System.NotImplementedException();

        public Electricity PowerCharged => throw new System.NotImplementedException();

        public Electricity PowerRequired => throw new System.NotImplementedException();
    }
}