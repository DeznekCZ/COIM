using Mafi.Unity.Entities;
using Mafi;
using UnityEngine;
using WindPower.Entity;

namespace WindPower.Unity
{
    [GlobalDependency(RegistrationMode.AsAllInterfaces, false, false)]
    public class WindTurbineMbFactory : IEntityMbFactory<WindTurbine>, IFactory<WindTurbine, EntityMb>
    {
        private readonly ProtoModelFactory m_modelFactory;

        public WindTurbineMbFactory(ProtoModelFactory modelFactory)
        {
            m_modelFactory = modelFactory;
        }

        public EntityMb Create(WindTurbine reactor)
        {
            Assert.That(reactor).IsNotNull();
            GameObject gameObject = m_modelFactory.CreateModelFor(reactor.Prototype);
            WindTurbineMb beaconMb = gameObject.AddComponent<WindTurbineMb>();
            beaconMb.Initialize(reactor);
            return beaconMb;
        }
    }

}
