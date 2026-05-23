using Mafi;
using UnityEngine;

namespace NightMod;

/// <summary>
/// The moon: a visible disc plus a dim, shadowless directional "moonlight" that gives the terrain
/// some illumination at night. The game has no moon, so the mod builds both - a camera-facing
/// billboard quad with a procedural texture, and a directional light from the moon's direction.
/// The disc is painted with the current lunar phase - a crescent, half or gibbous shape - and the
/// billboard is spun so the lit limb points at the sun. <see cref="NightCycleRenderer"/> places,
/// phases and fades them as the moon rises and sets.
/// </summary>
public sealed class NightMoon {

	private const int TextureSize = 128;

	// Sky size of the disc, as a fraction of the camera's far distance: the same angular size as
	// the mod's sun disc, the way the real moon and sun appear.
	private const float AngularScale = 0.045f;

	// Peak intensity of the moonlight. Kept very low: the moonlight is a directional light, so on
	// the glossy ocean it reads as a bright specular sheen long before it does much for the land -
	// a higher value lights the sea an unnatural electric blue.
	private const float MoonLightIntensity = 0.05f;

	// Width of the soft band drawn across the phase terminator, in moon-radius fractions.
	private const float TerminatorSoftness = 0.045f;

	// The disc texture is only repainted once the lit fraction has drifted by this much, so the
	// phase shape updates smoothly without the cost of repainting every frame.
	private const float PhaseRepaintStep = 0.02f;

	/// <summary>Muted moon color the disc texture is tinted by (kept dim, not glaring).</summary>
	private static readonly Color MoonColor = new Color(0.6f, 0.62f, 0.58f);

	/// <summary>Color of the moonlight - only faintly cool, so it does not wash the night blue.</summary>
	private static readonly Color MoonLightColor = new Color(0.62f, 0.64f, 0.72f);

	private readonly GameObject m_disc;
	private readonly Mesh m_mesh;
	private readonly Material m_material;
	private readonly Texture2D m_texture;
	private readonly GameObject m_lightObject;
	private readonly Light m_light;

	private float m_paintedFraction = -1f;

	/// <summary>
	/// True while the moonlight is a shadow-casting light. It is shadowless by default (see the
	/// constructor), so this is false unless that is changed. The moon horizon mask keys off it so
	/// it never casts a stray shadow while the moonlight itself casts none.
	/// </summary>
	public bool CastsShadows => m_light != null && m_light.shadows != LightShadows.None;

	/// <summary>Unit vector from the world toward the moon, read off the moonlight's orientation.</summary>
	public Vector3 DirectionToMoon => m_lightObject != null
		? -m_lightObject.transform.forward
		: Vector3.up;

	public NightMoon() {
		// Moon disc: a billboard quad built from bare components (no collider, no physics module).
		m_mesh = createQuadMesh();
		m_disc = new GameObject("NightMod Moon");
		m_disc.AddComponent<MeshFilter>().sharedMesh = m_mesh;
		MeshRenderer renderer = m_disc.AddComponent<MeshRenderer>();
		renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		renderer.receiveShadows = false;
		m_texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false) {
			wrapMode = TextureWrapMode.Clamp
		};
		Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Transparent");
		if (shader != null) {
			m_material = new Material(shader) { mainTexture = m_texture };
			renderer.sharedMaterial = m_material;
			// Start fully lit; Update repaints it the moment a real phase is known.
			paintMoonPhase(1f);
		} else {
			Log.Warning("NightMod: no transparent shader found, the moon disc will not be drawn.");
		}
		m_disc.SetActive(false);

		// Moonlight: a shadowless directional light so night terrain is not pitch black.
		m_lightObject = new GameObject("NightMod Moon Light");
		m_light = m_lightObject.AddComponent<Light>();
		m_light.type = LightType.Directional;
		// No shadows: a second shadow-casting directional light produces stray shadow blobs in the
		// built-in render pipeline. Night simply has no cast shadows, which reads fine.
		m_light.shadows = LightShadows.None;
		m_light.color = MoonLightColor;
		m_light.intensity = 0f;
		m_lightObject.SetActive(false);
	}

	/// <summary>
	/// Places the moon for this frame. <paramref name="visibility"/> 0 hides it and 1 is fully
	/// risen; <paramref name="elevation"/> and <paramref name="azimuth"/> are its sky angles.
	/// <paramref name="litFraction"/> is the illuminated fraction (0 new, 1 full) and
	/// <paramref name="sunDirection"/> is the sky direction of the sun, used to turn the lit limb.
	/// </summary>
	public void Update(Camera camera, float elevation, float azimuth, float visibility,
		float litFraction, Vector3 sunDirection) {
		if (camera == null || visibility <= 0f) {
			Hide();
			return;
		}

		// Moonlight comes from the moon's direction (same angle convention as the sun light); a
		// thin crescent casts far less light on the terrain than a full moon does.
		if (!m_lightObject.activeSelf) {
			m_lightObject.SetActive(true);
		}
		m_lightObject.transform.eulerAngles = new Vector3(elevation, azimuth, 0f);
		m_light.intensity = visibility * MoonLightIntensity * (0.3f + 0.7f * Mathf.Clamp01(litFraction));

		// Disc billboard, placed far away in the moon's sky direction and facing the camera.
		if (m_material != null) {
			// The phase shape drifts slowly, so the texture is only repainted now and then.
			if (Mathf.Abs(litFraction - m_paintedFraction) > PhaseRepaintStep) {
				paintMoonPhase(litFraction);
			}
			if (!m_disc.activeSelf) {
				m_disc.SetActive(true);
			}
			Vector3 skyDirection = Quaternion.Euler(-elevation, azimuth, 0f) * Vector3.forward;
			float distance = camera.farClipPlane * 0.9f;
			m_disc.transform.position = camera.transform.position + skyDirection * distance;
			// Spin the billboard in its own plane so the painted bright limb points at the sun.
			float limbAngle = brightLimbAngle(camera, skyDirection, sunDirection);
			m_disc.transform.rotation = camera.transform.rotation * Quaternion.Euler(0f, 0f, limbAngle);
			m_disc.transform.localScale = Vector3.one * (distance * AngularScale);
			Color color = MoonColor;
			color.a = Mathf.Clamp01(visibility);
			m_material.color = color;
		}
	}

	/// <summary>Hides the moon disc and switches off the moonlight, without destroying anything.</summary>
	public void Hide() {
		if (m_disc != null && m_disc.activeSelf) {
			m_disc.SetActive(false);
		}
		if (m_lightObject != null && m_lightObject.activeSelf) {
			m_lightObject.SetActive(false);
		}
	}

	/// <summary>Destroys the moon objects and their generated assets.</summary>
	public void Destroy() {
		if (m_disc != null) {
			Object.Destroy(m_disc);
		}
		if (m_mesh != null) {
			Object.Destroy(m_mesh);
		}
		if (m_material != null) {
			Object.Destroy(m_material);
		}
		if (m_texture != null) {
			Object.Destroy(m_texture);
		}
		if (m_lightObject != null) {
			Object.Destroy(m_lightObject);
		}
	}

	/// <summary>
	/// Screen-plane angle, in degrees from screen-right, that the moon's bright limb should point
	/// toward so it faces the sun. The sunward direction is taken tangent to the sky sphere at the
	/// moon, then read off in the camera's right/up plane.
	/// </summary>
	private static float brightLimbAngle(Camera camera, Vector3 moonDirection, Vector3 sunDirection) {
		Vector3 toSun = sunDirection - moonDirection * Vector3.Dot(sunDirection, moonDirection);
		if (toSun.sqrMagnitude < 1e-8f) {
			return 0f;
		}
		float right = Vector3.Dot(toSun, camera.transform.right);
		float up = Vector3.Dot(toSun, camera.transform.up);
		return Mathf.Atan2(up, right) * Mathf.Rad2Deg;
	}

	/// <summary>Builds a unit quad mesh centered on the origin.</summary>
	private static Mesh createQuadMesh() {
		Mesh mesh = new Mesh { name = "NightMod Moon Quad" };
		mesh.vertices = new[] {
			new Vector3(-0.5f, -0.5f, 0f),
			new Vector3(0.5f, -0.5f, 0f),
			new Vector3(-0.5f, 0.5f, 0f),
			new Vector3(0.5f, 0.5f, 0f),
		};
		mesh.uv = new[] {
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f),
		};
		mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
		mesh.RecalculateBounds();
		return mesh;
	}

	/// <summary>
	/// Paints the moon disc texture for the given illuminated fraction (0 new, 1 full): a bright
	/// circle with faint craters and a soft edge, with the unlit side faded to transparent across
	/// a soft terminator so a crescent shows only its sunlit sliver. The lit hemisphere is the +x
	/// side of the texture; <see cref="Update"/> spins the disc so that side points at the sun.
	/// </summary>
	private void paintMoonPhase(float litFraction) {
		float f = Mathf.Clamp01(litFraction);
		// Phase terminator: a pixel is sunlit where its x exceeds terminator * sqrt(1 - y^2). The
		// terminator runs from the right limb at new moon (f=0) to the left limb at full (f=1).
		float terminator = Mathf.Cos(Mathf.PI * f);
		float center = (TextureSize - 1) * 0.5f;
		// Faint craters as (x, y, radius), all in moon-radius fractions.
		Vector3[] craters = {
			new Vector3(-0.30f, 0.25f, 0.18f),
			new Vector3(0.28f, 0.10f, 0.13f),
			new Vector3(0.05f, -0.32f, 0.20f),
			new Vector3(-0.15f, -0.12f, 0.10f),
		};
		Color[] pixels = new Color[TextureSize * TextureSize];
		for (int y = 0; y < TextureSize; y++) {
			for (int x = 0; x < TextureSize; x++) {
				float nx = (x - center) / center;
				float ny = (y - center) / center;
				float distance = Mathf.Sqrt(nx * nx + ny * ny);
				float alpha = 1f - Mathf.SmoothStep(0.93f, 1f, distance);
				// Slight limb darkening toward the edge for a rounded look.
				float brightness = Mathf.Lerp(1f, 0.78f, Mathf.Clamp01(distance));
				foreach (Vector3 crater in craters) {
					float dx = nx - crater.x;
					float dy = ny - crater.y;
					float craterDistance = Mathf.Sqrt(dx * dx + dy * dy);
					brightness *= Mathf.Lerp(0.7f, 1f, Mathf.Clamp01(craterDistance / crater.z));
				}
				// Fade the unlit side out across a soft terminator band.
				float edge = terminator * Mathf.Sqrt(Mathf.Max(0f, 1f - ny * ny));
				float lit = Mathf.SmoothStep(-TerminatorSoftness, TerminatorSoftness, nx - edge);
				pixels[y * TextureSize + x] = new Color(brightness, brightness, brightness, alpha * lit);
			}
		}
		m_texture.SetPixels(pixels);
		m_texture.Apply();
		m_paintedFraction = f;
	}
}