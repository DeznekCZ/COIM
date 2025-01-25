using Mafi;
using Mafi.Core.Entities.Static;
using Mafi.Core.Prototypes;
using System;

namespace ProgramableNetwork.ModuleTester
{
    internal class FakeProto : StaticEntityProto
    {
        private ID id;

        public FakeProto(ID id)
            : base(id, CreateStr(id, "fake", ""), EntityCosts.None, Duration.OneTick, null, Gfx.Empty)
        {
            this.id = id;
        }

        public override Type EntityType => typeof(FakeEntity);
    }
}