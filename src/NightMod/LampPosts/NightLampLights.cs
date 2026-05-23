using System.Collections.Generic;
using Mafi;
using Mafi.Unity;
using UnityEngine;

namespace NightMod.LampPosts;

/// <summary>
/// Casts the actual light from placed lamp posts. Lamp post <em>models</em> are cheap - the game
/// batches the shared mesh - but a cast <see cref="Light"/> cannot be instanced: each one is an
/// individual light the renderer has to account for. So this system keeps only a pool of real
/// spot lights and, every frame, hands them to the lamp posts nearest the camera; lamps beyond
/// the pool simply stay dark. Same nearest-N idea as <see cref="NightHeadlights"/>.
///
/// <para>Purely static layout entities in COI are rendered through the instanced renderer and
/// have no per-entity <c>EntityMb</c> to find in the scene. Instead this system reads from
/// <see cref="LampPost.All"/>, into which every live lamp self-registers, and computes each
/// lamp's world position from its tile <c>Transform</c>. The whole light spec - range, cone
/// angle, intensity, lens height, colour swatch - comes from the prototype's
/// <see cref="LampPostProto.Gfx"/>, so nothing about the light is hard-coded here.</para>
///
/// <para>A lamp's light is on only when <see cref="LampPost.IsLit"/> is true (powered + night),
/// after its own per-lamp turn-on delay has elapsed, and provided it is within the active pool.
/// Each lamp's hue and delay are deterministic functions of its entity id.</para>
/// </summary>
public sealed class NightLampLights {

	// Real-time seconds between scans for new or removed lamp posts.
	private const float RescanIntervalSeconds = 2f;

	// Real-time seconds between diagnostic status log lines.
	private const float StatusLogIntervalSeconds = 4f;

	// At most this many lamp posts cast a real light at once; the rest stay dark.
	private const int MaxActiveLamps = 32;

	// A spot light shines along its local +Z; this rotation pitches +Z to point straight down.
	private static readonly Quaternion AimDown = Quaternion.Euler(90f, 0f, 0f);

	private readonly GameObject m_container;
	private readonly Dictionary<LampPost, Rig> m_rigs;
	private readonly List<LampPost> m_stale;
	private readonly List<Rig> m_visible;

	private float m_rescanTimer;
	private float m_statusLogTimer;

	public NightLampLights() {
		m_container = new GameObject("NightMod Lamp Lights");
		m_rigs = new Dictionary<LampPost, Rig>();
		m_stale = new List<LampPost>();
		m_visible = new List<Rig>();
	}

	/// <summary>
	/// Drives the lamp lights for this frame. <paramref name="level"/> is the cycle's night
	/// strength - 0 in full daytime, ramping up through dusk, 1 through the night. With
	/// <paramref name="level"/> at 0, or no camera, every light is switched off.
	/// </summary>
	public void Update(float level, Camera camera) {
		// Scan and log unconditionally (even in daytime) so the diagnostic line is always accurate.
		rescan();
		logStatus(level, camera);

		if (level <= 0f || camera == null) {
			HideAll();
			return;
		}

		// Gather the lamps that are currently lit, then light the ones nearest the camera.
		Vector3 cameraPosition = camera.transform.position;
		m_visible.Clear();
		foreach (KeyValuePair<LampPost, Rig> pair in m_rigs) {
			LampPost lamp = pair.Key;
			Rig rig = pair.Value;
			if (lamp == null || lamp.IsDestroyed || !lamp.IsLit) {
				// Lamp is off (day, unpowered or removed): reset its turn-on delay so it has to
				// wait again the next time it comes on.
				rig.LitSince = -1f;
				rig.SetActive(false);
				continue;
			}
			if (rig.LitSince < 0f) {
				rig.LitSince = Time.time;
			}
			rig.CameraDistanceSqr = (rig.LensWorldPosition - cameraPosition).sqrMagnitude;
			m_visible.Add(rig);
		}
		m_visible.Sort((a, b) => a.CameraDistanceSqr.CompareTo(b.CameraDistanceSqr));

		for (int i = 0; i < m_visible.Count; i++) {
			Rig rig = m_visible[i];
			// A lamp lights only once it is within the pool AND its own random delay has passed.
			bool delayElapsed = Time.time - rig.LitSince >= rig.Delay;
			rig.SetActive(i < MaxActiveLamps && delayElapsed);
		}
	}

	/// <summary>Switches every lamp light off without destroying anything (used during the day).</summary>
	public void HideAll() {
		foreach (KeyValuePair<LampPost, Rig> pair in m_rigs) {
			pair.Value.LitSince = -1f;
			pair.Value.SetActive(false);
		}
	}

	/// <summary>Destroys every lamp light and the container. Call on shutdown.</summary>
	public void Destroy() {
		if (m_container != null) {
			Object.Destroy(m_container);
		}
		m_rigs.Clear();
	}

	/// <summary>
	/// Emits a throttled diagnostic line covering the whole light pipeline, so a "no cone" report
	/// can be pinned to a stage: whether lamps are registered, whether they are lit, and whether
	/// cones are switched on. Logged even in daytime (where level is 0).
	/// </summary>
	private void logStatus(float level, Camera camera) {
		m_statusLogTimer -= Time.deltaTime;
		if (m_statusLogTimer > 0f) {
			return;
		}
		m_statusLogTimer = StatusLogIntervalSeconds;

		int lit = 0;
		int conesOn = 0;
		foreach (KeyValuePair<LampPost, Rig> pair in m_rigs) {
			LampPost lamp = pair.Key;
			Rig rig = pair.Value;
			if (lamp != null && !lamp.IsDestroyed && lamp.IsLit) {
				lit++;
			}
			if (rig.Light != null && rig.Light.gameObject.activeSelf) {
				conesOn++;
			}
		}
		Log.Info($"NightMod lamp lights — level={level:F2} (>0 = night), camera={camera != null}, " +
			$"registered={LampPost.All.Count}, rigs={m_rigs.Count}, lit (powered+night)={lit}, cones on={conesOn}");
	}

	/// <summary>
	/// Refreshes the set of tracked lamp posts from <see cref="LampPost.All"/>: drops rigs whose
	/// lamp has been destroyed (and destroys their lights) and builds rigs for any newly placed
	/// lamps. Runs on a timer, not every frame.
	/// </summary>
	private void rescan() {
		m_rescanTimer -= Time.deltaTime;
		if (m_rescanTimer > 0f && m_rigs.Count > 0) {
			return;
		}
		m_rescanTimer = RescanIntervalSeconds;

		// Drop rigs whose lamp has been destroyed.
		m_stale.Clear();
		foreach (KeyValuePair<LampPost, Rig> pair in m_rigs) {
			if (pair.Key == null || pair.Key.IsDestroyed) {
				m_stale.Add(pair.Key);
				if (pair.Value.Light != null) {
					Object.Destroy(pair.Value.Light.gameObject);
				}
			}
		}
		foreach (LampPost lamp in m_stale) {
			m_rigs.Remove(lamp);
			LampPost.All.Remove(lamp);
		}

		// Also prune destroyed entities from the registry itself - they may have been destroyed
		// without making it into a rig (e.g. before the first scan since they were placed).
		m_stale.Clear();
		foreach (LampPost lamp in LampPost.All) {
			if (lamp == null || lamp.IsDestroyed) {
				m_stale.Add(lamp);
			}
		}
		foreach (LampPost lamp in m_stale) {
			LampPost.All.Remove(lamp);
		}

		// Build a rig for every registered lamp we have not yet seen.
		foreach (LampPost lamp in LampPost.All) {
			if (m_rigs.ContainsKey(lamp)) {
				continue;
			}
			m_rigs[lamp] = build(lamp);
		}
	}

	/// <summary>
	/// Builds a light rig for one lamp: a spot light placed at the lamp's lens in world space,
	/// configured from the prototype's Gfx spec, tinted to that lamp's own colour.
	/// </summary>
	private Rig build(LampPost lamp) {
		LampPostProto.Gfx gfx = lamp.Prototype.Graphics;

		// World position of the lamp's base from its tile transform, then up to the lens.
		Vector3 baseWorld = lamp.Prototype.Layout.GetModelOrigin(lamp.Transform).ToVector3();
		Vector3 lensWorld = baseWorld + Vector3.up * gfx.LensLocalHeight;

		GameObject go = new GameObject($"NightMod Lamp Light #{lamp.Id.Value}");
		go.transform.SetParent(m_container.transform, worldPositionStays: false);
		go.transform.SetPositionAndRotation(lensWorld, AimDown);

		Light light = go.AddComponent<Light>();
		light.type = LightType.Spot;
		light.spotAngle = gfx.LightSpotAngleDegrees;
		light.range = gfx.LightRangeMeters;
		light.color = Color.Lerp(gfx.LightWarmColor, gfx.LightCoolColor, Mathf.Clamp01(lamp.LightHue01));
		light.intensity = gfx.LightIntensity;
		// Shadowless: a field of shadow-casting spots would be far too expensive, and the cones
		// only need to lift the ground out of darkness.
		light.shadows = LightShadows.None;
		// ForcePixel, not Auto: Auto lets the renderer downgrade or drop the light by its relevance
		// to the camera, which makes it flicker as the camera moves. ForcePixel keeps it stable.
		light.renderMode = LightRenderMode.ForcePixel;

		Log.Info($"NightMod: built lamp light for lamp id {lamp.Id.Value} at world {lensWorld} " +
			$"(hue {lamp.LightHue01:F2}, delay {lamp.TurnOnDelaySeconds:F1}s).");

		Rig rig = new Rig {
			Light = light,
			LensWorldPosition = lensWorld,
			Delay = lamp.TurnOnDelaySeconds,
		};
		rig.SetActive(false);
		return rig;
	}

	/// <summary>One lamp post's spot light, plus the data needed to schedule it.</summary>
	private sealed class Rig {

		public Light Light;

		/// <summary>World-space position of the lamp's lens. Lamp posts are static, so this is cached.</summary>
		public Vector3 LensWorldPosition;

		public float CameraDistanceSqr;

		/// <summary>This lamp's own turn-on delay in seconds (0..5), copied from the entity.</summary>
		public float Delay;

		/// <summary>Real time the lamp became lit, or negative while it is off - used to time the delay.</summary>
		public float LitSince = -1f;

		/// <summary>Shows or hides this rig's light, if it has one.</summary>
		public void SetActive(bool active) {
			if (Light != null && Light.gameObject.activeSelf != active) {
				Light.gameObject.SetActive(active);
			}
		}
	}
}
