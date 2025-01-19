using Mafi.Core.Entities;
using Mafi.Core.Prototypes;
using System;

namespace ProgramableNetwork.ModuleTester
{
    internal class FakeProto : EntityProto
    {
        private ID id;

        public FakeProto(ID id)
            : base(id, CreateStr(id, "fake", ""), EntityCosts.None, null)
        {
            this.id = id;
        }

        public override Type EntityType => typeof(FakeEntity);
    }
}