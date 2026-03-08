using Mafi;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Unity.Entities;
using Mafi.Unity.Entities.Static;
using UnityEngine;

namespace ProgramableNetwork.Data.DisplayEntity
{

    public class DisplayEntityMb : StaticEntityMb, IEntityMbWithRenderUpdate, IEntityMbWithSyncUpdate
    {
        private new DisplayEntity Entity => (DisplayEntity)base.Entity;
		private Transform m_cocpit;
        private Transform m_arm;
        private Transform m_extender;
        private Transform m_wheel;
        private Lyst<Transform> m_bucketTransforms;
        private Lyst<Tile3f> m_bucketTiles;

        public void Initialize(DisplayEntity display)
        {
            base.Initialize(display);
            Entity.DisplayManager.Init(this);
        }

        public void RenderUpdate(GameTime time)
        {
            Entity.DisplayManager.RenderUpdate(time);
        }

        public void SyncUpdate(GameTime time)
        {
            Entity.DisplayManager.SyncUpdate(time);
        }
    }
}
