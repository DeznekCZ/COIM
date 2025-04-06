using Mafi;
using Mafi.Core;
using Mafi.Core.Buildings.Farms;
using Mafi.Unity.Entities;
using Mafi.Unity.Entities.Static;
using UnityEngine;

namespace BucketWheelExcavator.Unity
{

    public class BucketWheelExcavatorMb : StaticEntityMb, IEntityMbWithRenderUpdate
    {
        private Entity.BucketWheelExcavator m_excavator;
        private Transform m_cocpit;
        private Transform m_arm;
        private Transform m_extender;
        private Transform m_wheel;

        public void Initialize(Entity.BucketWheelExcavator excavator)
        {
            base.Initialize(excavator);
            m_excavator = excavator;

            m_cocpit = base.transform.Find("Cocpit");
            m_arm = m_cocpit.Find("Arm");
            m_extender = m_arm.Find("Extender");
            m_wheel = m_extender.Find("Wheel");
        }

        public void RenderUpdate(GameTime time)
        {
            if (time.IsGamePaused) return;

            Vector3 old;

            if (m_excavator.IsEnabled)
            {
                old = m_wheel.localEulerAngles;
                m_wheel.Rotate(Vector3.left, 1); // -x
            }  

            old = m_cocpit.localEulerAngles;
            old.y = m_excavator.Direction.ToFloat();
            m_cocpit.localEulerAngles = old;

            old = m_extender.localPosition;
            old.z = m_excavator.Distance.ToFloat().Max(12).Min(24);
            m_extender.localPosition = old;

            old = m_arm.localEulerAngles;
            old.x = m_excavator.Height.ToFloat();
            m_arm.localEulerAngles = old;
        }
    }
}
