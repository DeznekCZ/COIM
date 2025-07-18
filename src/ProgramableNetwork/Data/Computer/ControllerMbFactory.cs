using Mafi;
using Mafi.Unity.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ProgramableNetwork.Data.Computer
{
    [GlobalDependency(RegistrationMode.AsAllInterfaces)]
    public class ControllerMbFactory : IEntityMbFactory<Controller>, IFactory<Controller, EntityMb>
    {
        private readonly ProtoModelFactory m_modelFactory;

        public ControllerMbFactory(ProtoModelFactory modelFactory)
        {
            m_modelFactory = modelFactory;
        }

        public EntityMb Create(Controller controller)
        {
            Assert.That(controller).IsNotNull();
            GameObject gameObject = m_modelFactory.CreateModelFor(controller.Prototype);
            ControllerMb controllerMb = gameObject.AddComponent<ControllerMb>();
            controllerMb.Initialize(controller);
            return controllerMb;
        }
    }
}
