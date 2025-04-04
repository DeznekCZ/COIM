using Mafi;
using Mafi.Core;
using Mafi.Unity.Entities;
using Mafi.Unity.Entities.Static;
using UnityEngine;
using WindPower.Entity;

namespace WindPower.Unity
{

    public class WindTurbineMb : StaticEntityMb, IEntityMbWithRenderUpdate
    {
        private WindTurbine m_windTrubine;
        private Transform m_gondola;
        private Transform m_rotor;

        public void Initialize(WindTurbine windTurbine)
        {
            base.Initialize(windTurbine);
            m_windTrubine = windTurbine;

            m_gondola = base.gameObject.transform.Find("Gondola");
            if (m_gondola is null)
            {
                Log.Warning("Missing 'Gondola' transform");
                return;
            }
            Vector3 newAngles = m_gondola.eulerAngles;
            newAngles.y = m_windTrubine.WindDirection.ToFloat();
            m_gondola.eulerAngles = newAngles;

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
            if (m_gondola is null) return;
            Fix32 current = m_gondola.eulerAngles.y.ToFix32();
            int direction = (m_windTrubine.WindDirection - current).Sign();
            Fix32 abs = (m_windTrubine.WindDirection - current).Abs().Min(1.ToFix32());
            if (abs > 0)
                m_gondola.Rotate(Vector3.up, time.DeltaTimeMs * 0.01f * direction * abs.ToFloat());

            // update rotation of blades
            if (m_rotor is null) return;
            m_rotor.Rotate(Vector3.forward, time.DeltaTimeMs * 0.6f * m_windTrubine.Speed.ToFloat());
        }
    }
}
