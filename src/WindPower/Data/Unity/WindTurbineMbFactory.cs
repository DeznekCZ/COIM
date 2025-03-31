using Mafi.Core.Buildings.Beacons;
using Mafi.Core.Simulation;
using Mafi.Unity.Buildings;
using Mafi.Unity.Entities;
using Mafi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WindPower.Entity;
using Mafi.Core.Environment;
using Mafi.Core;

namespace WindPower.Unity
{
    [GlobalDependency(RegistrationMode.AsAllInterfaces, false, false)]
    public class WindTurbineMbFactory : IEntityMbFactory<WindTurbine>, IFactory<WindTurbine, EntityMb>
    {
        private readonly ProtoModelFactory m_modelFactory;
        private readonly RandomProvider m_randomProvider;

        public WindTurbineMbFactory(ProtoModelFactory modelFactory, RandomProvider randomProvider)
        {
            m_modelFactory = modelFactory;
            m_randomProvider = randomProvider;
        }

        public EntityMb Create(WindTurbine reactor)
        {
            Assert.That(reactor).IsNotNull();
            GameObject gameObject = m_modelFactory.CreateModelFor(reactor.Prototype);
            WindTurbineMb beaconMb = gameObject.AddComponent<WindTurbineMb>();
            beaconMb.Initialize(reactor, m_randomProvider);
            return beaconMb;
        }
    }

}
