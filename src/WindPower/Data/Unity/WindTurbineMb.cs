using Mafi;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Unity.Entities;
using Mafi.Unity.Entities.Static;
using UnityEngine;
using WindPower.Entity;

namespace WindPower.Unity; 

public class WindTurbineMb : StaticEntityMb, IEntityMbWithRenderUpdate {
	private WindTurbine m_windTrubine;
	private Transform m_gondola;
	private Transform m_rotor;
	private float m_speedMult;

	public void Initialize(WindTurbine windTurbine) {
		base.Initialize(windTurbine);
		m_windTrubine = windTurbine;
		m_speedMult = m_windTrubine.Prototype.Graphics.SpeedMultiplier.ToFloat();

		m_gondola = base.gameObject.transform.Find(windTurbine.Prototype.Graphics.GondolaGo);
		if (m_gondola is null) {
			Log.Warning("Missing 'Gondola' transform");
			return;
		}
		Vector3 newAngles = m_gondola.eulerAngles;
		newAngles.y = m_windTrubine.WindDirection.ToFloat();
		m_gondola.eulerAngles = newAngles;

		m_rotor = base.gameObject.transform.Find(windTurbine.Prototype.Graphics.RotorGo);
		if (m_rotor is null) {
			Log.Warning("Missing 'Rotor' transform");
			return;
		}

		m_rotor.Rotate(Vector3.forward, Random.Range(0, 360f));
		// TODO blades rotation
	}

	public void RenderUpdate(GameTime time) {
		if (m_windTrubine is null) {
			return;
		}
		if (time.IsGamePaused) {
			return;
		}
		// update gondola direction
		if (m_gondola is null) {
			return;
		}
		Fix32 current = m_gondola.eulerAngles.y.ToFix32();
		int direction = (m_windTrubine.WindDirection - current).Sign();
		Fix32 abs = (m_windTrubine.WindDirection - current).Abs().Min(1.ToFix32());
		if (abs > 0) {
			m_gondola.Rotate(Vector3.up, time.DeltaTimeMs * time.GameSpeedMult * 0.01f * m_speedMult * direction * abs.ToFloat());
		}

		// update rotation of blades
		if (m_rotor is null) {
			return;
		}
		m_rotor.Rotate(Vector3.forward, time.DeltaTimeMs * time.GameSpeedMult * 0.6f * m_speedMult * m_windTrubine.Speed.ToFloat());
	}
}
