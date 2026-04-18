#define UNITY_DEBUG
using System;
using System.Collections.Generic;
using Mafi;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core;
using Mafi.Serialization;
using Mafi.Unity.Entities;
using Mafi.Unity.Entities.Static;
using UnityEngine;
using WindPower.Data.Unity;
using WindPower.Entity;
using Random = UnityEngine.Random;

namespace WindPower.Unity; 

public class WindTurbineMb : StaticEntityMb, IEntityMbWithRenderUpdate {
	private WindTurbine m_windTrubine;
	private Transform m_gondola;
	private Transform m_rotor;
	private float m_maximumRpm;
	private float m_rotationDirection;
	private BlurEffect m_blurEffect;

	public void Initialize(WindTurbine windTurbine) {
		base.Initialize(windTurbine);
		m_windTrubine = windTurbine;
		m_maximumRpm = m_windTrubine.Prototype.Graphics.MaximumRpm.ToFloat().Abs();
		m_rotationDirection = m_windTrubine.Prototype.Graphics.MaximumRpm.ToFloat().Sign();

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
		if (windTurbine.Prototype.Graphics.BlurMeshPaths.IsNotValidOrEmpty == false) {
			m_blurEffect = base.gameObject.AddComponent<BlurEffect>();
			m_blurEffect.blurModels = windTurbine.Prototype.Graphics.BlurMeshPaths
				.Select(path => gameObject.transform.Find(path)?.gameObject)
				.ToImmutableArray();
			m_blurEffect.blurConfig = windTurbine.Prototype.Graphics.BlurConfig;
			m_blurEffect.SetAnglePerSecond(0);
		}
		// TODO blades rotation
	}

	public void RenderUpdate(GameTime time) {
		if (time.IsGamePaused) {
			if (m_blurEffect) {
				m_blurEffect.SetAnglePerSecond(0);
			}
			return;
		}
		if (m_windTrubine is null) {
			return;
		}
		if (m_gondola is null) {
			return;
		}
		Fix32 current = m_gondola.eulerAngles.y.ToFix32();
		int direction = (m_windTrubine.WindDirection - current).Sign();
		Fix32 abs = (m_windTrubine.WindDirection - current).Abs().Min(1.ToFix32());
		if (abs > 0) {
			m_gondola.Rotate(Vector3.up, time.DeltaTimeMs * time.GameSpeedMult
				* 0.01f * direction * abs.ToFloat()); // TODO tune rotation speed
		}

		// update rotation of blades
		if (m_rotor is null) {
			return;
		}
		float anglePerSecond = m_windTrubine.Speed.ToFloat() * m_maximumRpm * 360 / 60 * time.GameSpeedMult;
		float anglePerFrame = time.DeltaTimeMs * 0.001f * anglePerSecond * m_rotationDirection;
		m_rotor.Rotate(Vector3.forward, anglePerFrame);
		if (m_blurEffect) {
			m_blurEffect.SetAnglePerSecond(anglePerSecond);
		}
	}
}
