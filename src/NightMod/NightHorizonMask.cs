using Mafi;
using UnityEngine;
using UnityEngine.Rendering;

namespace NightMod;

/// <summary>
/// An invisible shadow caster that fakes night-time darkness. It is a single large quad, never
/// drawn, re-aimed every frame to face the sun head-on and parked between the camera and the sun -
/// underground once the sun has set.
///
/// <para>A flat horizon plane at sea level (the mod's first attempt) fails at exactly the moment it
/// matters: when the sun nears the horizon its light grazes that plane almost edge-on, so the plane
/// collapses to a sliver in the shadow map and casts nothing reliable - which is why the ocean's
/// per-chunk shadows still leaked through. A quad that always faces the sun is instead rendered
/// face-on into the shadow map at full resolution, so it casts one solid, map-wide shadow over
/// everything behind it no matter how low the sun is.</para>
///
/// <para>The quad is placed a fraction of the directional light's shadow distance along the
/// direction of the sun: far enough that every shadow-receiving surface in view sits on its dark
/// side, but near enough that it is never culled out of the shadow cascade. Being a
/// <see cref="ShadowCastingMode.ShadowsOnly"/> caster it is never visible to the camera, and being
/// aimed under the world once the sun is down it cannot poke through the seabed either.</para>
///
/// <para><see cref="NightCycleRenderer"/> drives it, switching it on only while the sun is at or
/// below the horizon so it never shadows the world in daylight. The moon's light is shadowless and
/// passes straight through, so the moon needs no caster of its own.</para>
/// </summary>
public sealed class NightHorizonMask {

	// The caster is parked this multiple of the shadow distance toward the sun. It must be at least
	// 1: a shadow-receiving surface can lie a full shadow distance away in the sun's direction, so
	// only a caster at least that far toward the sun keeps every surface in view on its dark side -
	// below 1 leaves a lit band at the edge of the shadow range. The extra margin past 1 also lifts
	// the quad clear of the last cascade's sphere into the volume Unity sweeps toward the light for
	// shadow casters, so it casts steadily instead of flickering at the cascade boundary. It must
	// not be pushed so far it leaves even that swept volume, or the caster is culled entirely.
	private const float PlaceDistanceFactor = 1.3f;

	// The camera position is snapped to this grid before the caster is placed, so ordinary camera
	// motion does not nudge the caster every frame - a shadow caster that moves sub-pixel amounts
	// each frame makes its shadow shimmer. The shadow is map-wide and uniform, so the occasional
	// snap-jump of the caster is invisible.
	private const float AnchorGrid = 64f;

	// Shadow distance to assume when the quality setting reports nothing usable.
	private const float FallbackShadowDistance = 1500f;

	// Hard limits on the placement distance, so an extreme shadow-distance setting can neither pull
	// the quad into the visible scene nor fling it out to where it loses precision or gets culled.
	private const float MinPlaceDistance = 200f;
	private const float MaxPlaceDistance = 6000f;

	// Side length of the square quad, as a multiple of the placement distance - large enough that a
	// low sun, sighting along the quad, never sees terrain past any of its edges.
	private const float SizeFactor = 8f;

	private readonly GameObject m_quad;
	private readonly Mesh m_mesh;
	private readonly Material m_material;

	public NightHorizonMask() {
		m_mesh = createSunFacingQuad();
		m_quad = new GameObject("NightMod Horizon Mask");
		m_quad.AddComponent<MeshFilter>().sharedMesh = m_mesh;

		MeshRenderer renderer = m_quad.AddComponent<MeshRenderer>();
		// ShadowsOnly: the quad feeds the shadow map but is itself never rendered, so there is no
		// way for it to ever show through the seabed or anything else.
		renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
		renderer.receiveShadows = false;
		// The Standard shader ships with the game, so it is always in the build, and its
		// ShadowCaster pass is all this quad needs (its surface is never actually shaded).
		Shader shader = Shader.Find("Standard");
		if (shader != null) {
			m_material = new Material(shader);
			renderer.sharedMaterial = m_material;
		} else {
			Log.Warning("NightMod: Standard shader not found, the horizon mask will not darken night.");
		}

		m_quad.SetActive(false);
	}

	/// <summary>
	/// Re-aims the caster for this frame. <paramref name="directionToSun"/> is the (unit) vector
	/// from the world toward the sun. When <paramref name="active"/> is false, or the camera is
	/// missing, the caster is simply hidden.
	/// </summary>
	public void Update(Camera camera, Vector3 directionToSun, bool active) {
		if (m_quad == null) {
			return;
		}
		if (!active || camera == null) {
			Hide();
			return;
		}

		Vector3 toSun = directionToSun.normalized;
		float shadowDistance = QualitySettings.shadowDistance > 1f
			? QualitySettings.shadowDistance
			: FallbackShadowDistance;
		float placeDistance =
			Mathf.Clamp(shadowDistance * PlaceDistanceFactor, MinPlaceDistance, MaxPlaceDistance);
		float size = placeDistance * SizeFactor;

		// Park the quad between the camera and the sun, face-on to the sunlight. Every surface
		// within 'placeDistance' of the camera then lies on the quad's shadowed side; the quad
		// itself stays inside the shadow cascade, so it casts instead of being culled. The camera
		// position is snapped to a grid first so the quad holds still under ordinary camera motion.
		Vector3 cam = camera.transform.position;
		Vector3 anchor = new Vector3(
			Mathf.Round(cam.x / AnchorGrid) * AnchorGrid,
			Mathf.Round(cam.y / AnchorGrid) * AnchorGrid,
			Mathf.Round(cam.z / AnchorGrid) * AnchorGrid);
		m_quad.transform.position = anchor + toSun * placeDistance;
		m_quad.transform.rotation = Quaternion.LookRotation(toSun);
		m_quad.transform.localScale = new Vector3(size, size, 1f);

		if (!m_quad.activeSelf) {
			m_quad.SetActive(true);
		}
	}

	/// <summary>Hides the caster without destroying it.</summary>
	public void Hide() {
		if (m_quad != null && m_quad.activeSelf) {
			m_quad.SetActive(false);
		}
	}

	/// <summary>Destroys the caster and its generated assets. Call on shutdown.</summary>
	public void Destroy() {
		if (m_quad != null) {
			Object.Destroy(m_quad);
		}
		if (m_mesh != null) {
			Object.Destroy(m_mesh);
		}
		if (m_material != null) {
			Object.Destroy(m_material);
		}
	}

	/// <summary>
	/// A unit quad in the XY plane, single-sided with its one face turned toward the sun (local +Z,
	/// the axis <see cref="Update"/> aims at the sun). Only that face is rendered into the shadow
	/// map, so the caster is "seen" only from the sun's side. Its back - the side the camera is
	/// always on, since the quad sits between the camera and the sun - is back-face culled: a second
	/// guarantee, on top of <see cref="ShadowCastingMode.ShadowsOnly"/>, that it never shows.
	/// </summary>
	private static Mesh createSunFacingQuad() {
		Mesh mesh = new Mesh { name = "NightMod Horizon Mask Quad" };
		mesh.vertices = new[] {
			new Vector3(-0.5f, -0.5f, 0f),
			new Vector3(0.5f, -0.5f, 0f),
			new Vector3(-0.5f, 0.5f, 0f),
			new Vector3(0.5f, 0.5f, 0f),
		};
		mesh.normals = new[] {
			Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward,
		};
		// A single winding whose front face points along +Z, toward the sun.
		mesh.triangles = new[] { 0, 1, 2, 1, 3, 2 };
		mesh.RecalculateBounds();
		return mesh;
	}
}
