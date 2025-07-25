using Mafi;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Buildings.Farms;
using Mafi.Core.Entities.Static;
using Mafi.Unity;
using Mafi.Unity.Entities;
using Mafi.Unity.Entities.Static;
using System.Net.Sockets;
using UnityEngine;

namespace BucketWheelExcavator.Unity
{

    public class BucketWheelExcavatorMb : StaticEntityMb, IEntityMbWithRenderUpdate, IEntityMbWithSyncUpdate
    {
        private Entity.BucketWheelExcavator m_excavator;
        private Transform m_cocpit;
        private Transform m_arm;
        private Transform m_extender;
        private Transform m_wheel;
        private Lyst<Transform> m_bucketTransforms;
        private Lyst<Tile3f> m_bucketTiles;

        public void Initialize(Entity.BucketWheelExcavator excavator)
        {
            base.Initialize(excavator);
            m_excavator = excavator;

            m_cocpit = base.transform.Find("Cocpit");
            m_arm = m_cocpit.Find("Arm");
            m_extender = m_arm.Find("Extender");
            m_wheel = m_extender.Find("Wheel");

            Vector3 old;
            old = m_cocpit.localEulerAngles;
            old.y = m_excavator.Direction.ToFloat();
            m_cocpit.localEulerAngles = old;

            old = m_extender.localPosition;
            old.z = m_excavator.Distance.ToFloat().Max(12).Min(24);
            m_extender.localPosition = old;

            old = m_arm.localEulerAngles;
            old.x = m_excavator.Height.ToFloat();
            m_arm.localEulerAngles = old;

            m_bucketTransforms = new Lyst<Transform>();
            m_bucketTiles = new Lyst<Tile3f>();

            foreach (Transform item in m_wheel)
            {
                m_bucketTransforms.Add(item);
                m_bucketTiles.Add(item.position.ToTile3f());
            }

            for (int i = 0; i < m_bucketTransforms.Count; i++)
            {
                Transform transform = m_bucketTransforms[i];
                m_bucketTiles[i] = transform.position.ToTile3f();
                //Debug.Log("Bucket position: " + m_bucketTiles[i]);
            }

            m_excavator.Buckets = m_bucketTiles;
        }

        public void RenderUpdate(GameTime time)
        {
            if (time.IsGamePaused) return;

            Vector3 old;

            if (m_excavator.IsEnabled && m_excavator.ConstructionState == ConstructionState.Constructed)
            {
                old = m_wheel.localEulerAngles;
                m_wheel.Rotate(Vector3.left, 1); // -x
            }

            // when is not enable set color of light to red
        }

        public void SyncUpdate(GameTime time)
        {
            if (time.IsGamePaused) return;

            Vector3 old;

            old = m_cocpit.localEulerAngles;
            old.y = m_excavator.Direction.ToFloat();
            m_cocpit.localEulerAngles = old;

            old = m_extender.localPosition;
            old.z = m_excavator.Distance.ToFloat().Max(12).Min(24);
            m_extender.localPosition = old;

            old = m_arm.localEulerAngles;
            old.x = m_excavator.Height.ToFloat();
            m_arm.localEulerAngles = old;

            for (int i = 0; i < m_bucketTransforms.Count; i++)
            {
                Transform transform = m_bucketTransforms[i];
                m_bucketTiles[i] = transform.position.ToTile3f();
                //Debug.Log("Bucket position: " + m_bucketTiles[i]);
            }

            m_excavator.Buckets = m_bucketTiles;
        }
    }
}
