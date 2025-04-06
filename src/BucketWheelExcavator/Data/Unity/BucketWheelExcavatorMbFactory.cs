using Mafi.Unity.Entities;
using Mafi;
using UnityEngine;
using BucketWheelExcavator.Entity;

namespace BucketWheelExcavator.Unity
{
    [GlobalDependency(RegistrationMode.AsAllInterfaces, false, false)]
    public class BucketWheelExcavatorMbFactory : IEntityMbFactory<Entity.BucketWheelExcavator>, IFactory<Entity.BucketWheelExcavator, EntityMb>
    {
        private readonly ProtoModelFactory m_modelFactory;

        public BucketWheelExcavatorMbFactory(ProtoModelFactory modelFactory)
        {
            m_modelFactory = modelFactory;
        }

        public EntityMb Create(Entity.BucketWheelExcavator excavator)
        {
            Assert.That(excavator).IsNotNull();
            GameObject gameObject = m_modelFactory.CreateModelFor(excavator.Prototype);
            BucketWheelExcavatorMb excavatorMb = gameObject.AddComponent<BucketWheelExcavatorMb>();
            excavatorMb.Initialize(excavator);
            return excavatorMb;
        }
    }

}
