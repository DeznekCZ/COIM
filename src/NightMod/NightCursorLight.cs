using UnityEngine;

namespace NightMod;

/// <summary>
/// A flashlight that follows the mouse cursor while the player is placing things in build mode, so
/// laying out buildings at night is not done blind. A point light hovers a little above the world
/// point under the cursor and lights the area around it.
///
/// <para>It only burns at night - its strength tracks the cycle's darkness - and switches off the
/// instant build mode ends, so it never adds a stray glow in daylight. <see cref="NightCycleRenderer"/>
/// drives it from the cursor's picked world coordinate.</para>
/// </summary>
public sealed class NightCursorLight {

	// The light hovers this far above the picked ground point, and reaches this far around it.
	private const float HeightAboveGround = 25f;
	private const float Range = 90f;
	private const float MaxIntensity = 2.5f;

	// Below this cycle darkness the flashlight is not worth showing (it would only be a daylight glow).
	private const float MinNightFactor = 0.05f;

	/// <summary>Warm flashlight color.</summary>
	private static readonly Color LightColor = new Color(1f, 0.95f, 0.82f);

	private readonly GameObject m_lightObject;
	private readonly Light m_light;

	public NightCursorLight() {
		m_lightObject = new GameObject("NightMod Cursor Light");
		m_light = m_lightObject.AddComponent<Light>();
		m_light.type = LightType.Point;
		m_light.range = Range;
		m_light.color = LightColor;
		m_light.intensity = 0f;
		// Shadowless: the flashlight only lifts the build area out of the dark, and a moving
		// shadow-casting light would be both costly and visually noisy.
		m_light.shadows = LightShadows.None;
		m_lightObject.SetActive(false);
	}

	/// <summary>
	/// Places the flashlight over the cursor for this frame. It shows only while build mode is
	/// active and the night is dark enough to need it; <paramref name="nightFactor"/> (0..1) sets
	/// how brightly it burns.
	/// </summary>
	public void Update(Vector3 cursorWorldPos, bool buildModeActive, float nightFactor) {
		if (!buildModeActive || nightFactor <= MinNightFactor) {
			Hide();
			return;
		}
		m_lightObject.transform.position = cursorWorldPos + Vector3.up * HeightAboveGround;
		m_light.intensity = MaxIntensity * Mathf.Clamp01(nightFactor);
		if (!m_lightObject.activeSelf) {
			m_lightObject.SetActive(true);
		}
	}

	/// <summary>Switches the flashlight off without destroying it.</summary>
	public void Hide() {
		if (m_lightObject != null && m_lightObject.activeSelf) {
			m_lightObject.SetActive(false);
		}
	}

	/// <summary>Destroys the flashlight. Call on shutdown.</summary>
	public void Destroy() {
		if (m_lightObject != null) {
			Object.Destroy(m_lightObject);
		}
	}
}
