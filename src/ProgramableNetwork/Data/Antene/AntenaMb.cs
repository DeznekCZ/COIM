using Mafi.Core;
using Mafi.Unity.Entities;
using Mafi.Unity.Entities.Static;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ProgramableNetwork.Data.Antene
{
    public class AntenaMb : StaticEntityMb, IEntityMbWithSyncUpdate
    {
        [SerializeField]
        private Transform rotatingAntena;
        private Antena antena;

        public void Initialize(Antena antena, StaticEntityTransform entityTransform)
        {
            base.Initialize(antena, entityTransform);
            this.antena = antena;
        }

        public void SyncUpdate(GameTime time)
        {
            if (antena.IsEnabled)
            {
                rotatingAntena.Rotate(Vector3.up, 360 / 60 * 1000 * time.DeltaTimeMs);
            }
        }
    }
}
