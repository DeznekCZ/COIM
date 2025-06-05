using Mafi;
using Mafi.Unity.Entities;
using ProgramableNetwork.Data.DisplayEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ProgramableNetwork.Data.Antene
{
    [GlobalDependency(RegistrationMode.AsAllInterfaces)]
    public class AntenaMbFactory : IEntityMbFactory<Antena>, IFactory<Antena, EntityMb>
    {
        private readonly ProtoModelFactory m_modelFactory;

        public AntenaMbFactory(ProtoModelFactory modelFactory)
        {
            m_modelFactory = modelFactory;
        }

        public EntityMb Create(Antena antena)
        {
            Assert.That(antena).IsNotNull();
            GameObject gameObject = m_modelFactory.CreateModelFor(antena.Prototype);
            AntenaMb antenaMb = gameObject.AddComponent<AntenaMb>();
            antenaMb.Initialize(antena);
            return antenaMb;
        }
    }
}
