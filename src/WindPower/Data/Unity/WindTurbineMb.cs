using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Environment;
using Mafi.Core.Factory.ElectricPower;
using Mafi.Unity.Entities;
using Mafi.Unity.Entities.Static;
using Mafi.Unity.UiToolkit.Library.ObjectEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WindPower.Entity;

namespace WindPower.Unity
{

    public class WindTurbineMb : StaticEntityMb, IEntityMbWithRenderUpdate
    {
        private WindTurbine m_windTrubine;
        private IRandom m_random;
        private Transform m_gondola;
        private Transform m_rotor;
        private bool m_paused;

        public void Initialize(WindTurbine windTurbine, RandomProvider randomProvider)
        {
            base.Initialize(windTurbine);
            m_windTrubine = windTurbine;
            m_random = randomProvider.GetSimRandomFor(this);
            if (m_random is null)
            {
                Log.Warning("Missing random generator");
                return;
            }

            m_gondola = base.gameObject.transform.Find("Gondola");
            if (m_gondola is null)
            {
                Log.Warning("Missing 'Gondola' transform");
                return;
            }
            m_rotor = m_gondola.Find("Rotor");
            if (m_rotor is null)
            {
                Log.Warning("Missing 'Rotor' transform");
                return;
            }
        }

        public void RenderUpdate(GameTime time)
        {
            if (time.IsGamePaused) return;
            if (m_windTrubine is null) return;

            // update gondola direction
            if (m_random is null) return;
            if (m_gondola is null) return;
            m_gondola.Rotate(Vector3.up, time.DeltaTimeMs * 0.01f * (m_random.NextFloat() - 0.5f));

            // update rotation of blades
            if (m_rotor is null) return;
            m_rotor.Rotate(Vector3.forward, time.DeltaTimeMs * 0.6f * m_windTrubine.Speed.ToFloat());
        }
    }
}
