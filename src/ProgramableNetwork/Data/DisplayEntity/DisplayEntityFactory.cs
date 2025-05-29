using Mafi.Core.Entities;
using Mafi.Unity.Entities;
using Mafi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ProgramableNetwork.Data.DisplayEntity
{
    [GlobalDependency(RegistrationMode.AsAllInterfaces, false, false)]
    public class DisplayEntityMbFactory : IEntityMbFactory<DisplayEntity>, IFactory<DisplayEntity, EntityMb>
    {
        private readonly ProtoModelFactory m_modelFactory;

        public DisplayEntityMbFactory(ProtoModelFactory modelFactory)
        {
            m_modelFactory = modelFactory;
        }

        public EntityMb Create(DisplayEntity displayEntity)
        {
            Assert.That(displayEntity).IsNotNull();
            GameObject gameObject = m_modelFactory.CreateModelFor(displayEntity.Prototype);
            DisplayEntityMb excavatorMb = gameObject.AddComponent<DisplayEntityMb>();
            excavatorMb.Initialize(displayEntity);
            return excavatorMb;
        }
    }
}
