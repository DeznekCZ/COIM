using System;
using Mafi;
using UnityEngine;

namespace NightMod;

/// <summary>
/// The night starfield. The game has no night - and so no stars - of its own, so the mod builds
/// one: a dome of small star sprites baked into a single mesh and parked around the camera. Each
/// star quad sits on a unit sphere facing the sphere's center, so with the dome centered on the
/// camera every star faces the camera no matter where it looks, with no per-frame billboarding.
///
/// <para>The dome radius is set by the caller, small enough that the whole sphere sits inside the
/// game's skybox. The skybox is a flattened, depth-writing dome that arcs low overhead; a star
/// drawn beyond it is simply hidden, which is why a large star sphere shows only a belt of stars
/// around the horizon. A dome that nests inside the skybox keeps every star in front of it.</para>
///
/// <para>The dome turns slowly about the celestial pole - a point fixed at the observer's latitude
/// above the horizon - the way the real night sky wheels. As it turns, each star is faded out by
/// its own elevation, so a star sinking past the horizon vanishes there instead of carrying on
/// over the ground (the small nested dome sits in front of the distant terrain, so it cannot rely
/// on the terrain's depth to hide it).</para>
///
/// <para><see cref="NightCycleRenderer"/> fades the whole field in as the sun sets and washes it
/// back out under cloud cover, so the stars show on clear nights and vanish on overcast ones.</para>
/// </summary>
public sealed class NightStars {

	// Number of stars baked into the dome. Four vertices each, so well under the 16-bit mesh limit.
	private const int StarCount = 500;

	private const int TextureSize = 32;

	// Angular size range of a star sprite, as a fraction of the dome radius. Because both the
	// sprite and its distance scale with the radius, this is the on-screen size whatever the
	// radius works out to. Kept small so stars read as crisp points of light rather than blobs.
	private const float MinStarSize = 0.0018f;
	private const float MaxStarSize = 0.0055f;

	// Per-star brightness range. Even the faintest star keeps some glow so it still reads.
	private const float MinStarBrightness = 0.45f;
	private const float MaxStarBrightness = 1f;

	// Fixed seed so the starfield is identical every session - a star pattern that jumped around
	// between loads would be obvious and wrong.
	private const int Seed = 0x4E696768;

	// World-up component (the sine of elevation) over which a star fades in above the horizon: it
	// is fully hidden at or below the low mark and fully shown by the high mark, so no star is ever
	// drawn under the horizon as the dome drifts.
	private const float HorizonFadeLowY = -0.03f;
	private const float HorizonFadeHighY = 0.12f;

	/// <summary>Warm and cool white the star tints are drawn between, for a little color variety.</summary>
	private static readonly Color WarmStar = new Color(1f, 0.93f, 0.86f);
	private static readonly Color CoolStar = new Color(0.82f, 0.88f, 1f);

	private readonly GameObject m_dome;
	private readonly Mesh m_mesh;
	private readonly Material m_material;
	private readonly Texture2D m_texture;

	// Baked unit-sphere direction of each star, kept so the per-frame horizon fade can tell which
	// stars have drifted below the horizon. Filled by createStarfieldMesh.
	private Vector3[] m_starDirections;

	// Per-vertex color buffer (four entries per star): RGB is the baked brightness and tint, alpha
	// is rewritten every frame with the star's horizon fade. Reused so the fade allocates nothing.
	private Color[] m_vertexColors;

	public NightStars() {
		m_texture = createStarTexture();
		m_mesh = createStarfieldMesh();
		m_dome = new GameObject("NightMod Stars");
		m_dome.AddComponent<MeshFilter>().sharedMesh = m_mesh;
		MeshRenderer renderer = m_dome.AddComponent<MeshRenderer>();
		renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		renderer.receiveShadows = false;
		Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Transparent");
		if (shader != null) {
			m_material = new Material(shader) { mainTexture = m_texture };
			// One step below the transparent queue, so the moon and sun discs always draw over the
			// stars instead of the draw order deciding it.
			m_material.renderQueue = 2999;
			renderer.sharedMaterial = m_material;
		} else {
			Log.Warning("NightMod: no transparent shader found, the stars will not be drawn.");
		}
		m_dome.SetActive(false);
	}

	/// <summary>
	/// Places the starfield for this frame. <paramref name="skyRotation"/> turns the dome about the
	/// celestial pole, <paramref name="radius"/> is the dome's world radius (kept small enough to
	/// sit inside the skybox) and <paramref name="visibility"/> 0 hides it, 1 shows it fully.
	/// </summary>
	public void Update(Camera camera, Quaternion skyRotation, float radius, float visibility) {
		if (camera == null || visibility <= 0f || m_material == null) {
			Hide();
			return;
		}
		if (!m_dome.activeSelf) {
			m_dome.SetActive(true);
		}

		// The dome stays centered on the camera so every star keeps facing it; it is only rotated
		// and scaled.
		m_dome.transform.position = camera.transform.position;
		m_dome.transform.rotation = skyRotation;
		m_dome.transform.localScale = Vector3.one * radius;

		// Horizon fade: as the dome drifts, fade each star out by its current elevation so none is
		// drawn under the horizon. The dome is small enough to sit in front of the distant terrain,
		// so it cannot lean on the terrain's depth to hide a sunken star. A star's world-up
		// component is the sine of its elevation, which is all the fade needs. InverseLerp maps
		// that to 0..1 across the horizon band; the cubic then eases the band's two ends.
		for (int i = 0; i < StarCount; i++) {
			Vector3 worldDirection = skyRotation * m_starDirections[i];
			float t = Mathf.InverseLerp(HorizonFadeLowY, HorizonFadeHighY, worldDirection.y);
			float fade = t * t * (3f - 2f * t);
			int v = i * 4;
			m_vertexColors[v + 0].a = m_vertexColors[v + 1].a =
				m_vertexColors[v + 2].a = m_vertexColors[v + 3].a = fade;
		}
		m_mesh.colors = m_vertexColors;

		// Per-star brightness and tint live in the mesh's vertex colors; the material's alpha is the
		// single global fade applied on top.
		Color color = Color.white;
		color.a = Mathf.Clamp01(visibility);
		m_material.color = color;
	}

	/// <summary>Hides the starfield without destroying it.</summary>
	public void Hide() {
		if (m_dome != null && m_dome.activeSelf) {
			m_dome.SetActive(false);
		}
	}

	/// <summary>Destroys the starfield and its generated assets. Call on shutdown.</summary>
	public void Destroy() {
		if (m_dome != null) {
			UnityEngine.Object.Destroy(m_dome);
		}
		if (m_mesh != null) {
			UnityEngine.Object.Destroy(m_mesh);
		}
		if (m_material != null) {
			UnityEngine.Object.Destroy(m_material);
		}
		if (m_texture != null) {
			UnityEngine.Object.Destroy(m_texture);
		}
	}

	/// <summary>
	/// Bakes the whole starfield into one mesh: <see cref="StarCount"/> small quads scattered
	/// uniformly over a unit sphere, each lying in the sphere's tangent plane so it faces the
	/// center. Star size, brightness and tint vary per star and are carried in the vertex colors.
	/// Also fills <see cref="m_starDirections"/> and <see cref="m_vertexColors"/>, which the
	/// per-frame horizon fade in <see cref="Update"/> reads and rewrites.
	/// </summary>
	private Mesh createStarfieldMesh() {
		System.Random rng = new System.Random(Seed);
		m_starDirections = new Vector3[StarCount];
		m_vertexColors = new Color[StarCount * 4];
		Vector3[] vertices = new Vector3[StarCount * 4];
		Vector2[] uv = new Vector2[StarCount * 4];
		int[] triangles = new int[StarCount * 6];

		for (int i = 0; i < StarCount; i++) {
			// A direction uniformly distributed over the whole sphere (equal-area in z).
			double z = rng.NextDouble() * 2.0 - 1.0;
			double angle = rng.NextDouble() * Math.PI * 2.0;
			double ring = Math.Sqrt(Math.Max(0.0, 1.0 - z * z));
			Vector3 dir = new Vector3(
				(float)(ring * Math.Cos(angle)), (float)z, (float)(ring * Math.Sin(angle)));
			m_starDirections[i] = dir;

			// Two axes spanning the tangent plane at 'dir', so the quad sits flat against the dome.
			Vector3 reference = Mathf.Abs(dir.y) > 0.99f ? Vector3.right : Vector3.up;
			Vector3 right = Vector3.Normalize(Vector3.Cross(reference, dir));
			Vector3 up = Vector3.Cross(dir, right);

			float size = Mathf.Lerp(MinStarSize, MaxStarSize, (float)rng.NextDouble());
			float brightness = Mathf.Lerp(MinStarBrightness, MaxStarBrightness, (float)rng.NextDouble());
			Color tint = Color.Lerp(WarmStar, CoolStar, (float)rng.NextDouble()) * brightness;
			tint.a = 1f;

			int v = i * 4;
			vertices[v + 0] = dir - right * size - up * size;
			vertices[v + 1] = dir + right * size - up * size;
			vertices[v + 2] = dir - right * size + up * size;
			vertices[v + 3] = dir + right * size + up * size;
			uv[v + 0] = new Vector2(0f, 0f);
			uv[v + 1] = new Vector2(1f, 0f);
			uv[v + 2] = new Vector2(0f, 1f);
			uv[v + 3] = new Vector2(1f, 1f);
			m_vertexColors[v + 0] = m_vertexColors[v + 1] =
				m_vertexColors[v + 2] = m_vertexColors[v + 3] = tint;

			int t = i * 6;
			triangles[t + 0] = v + 0;
			triangles[t + 1] = v + 2;
			triangles[t + 2] = v + 1;
			triangles[t + 3] = v + 2;
			triangles[t + 4] = v + 3;
			triangles[t + 5] = v + 1;
		}

		Mesh mesh = new Mesh { name = "NightMod Starfield" };
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.colors = m_vertexColors;
		mesh.triangles = triangles;
		mesh.RecalculateBounds();
		return mesh;
	}

	/// <summary>Builds a small star sprite: a bright round core fading softly to transparent.</summary>
	private static Texture2D createStarTexture() {
		Texture2D texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false) {
			wrapMode = TextureWrapMode.Clamp
		};
		float center = (TextureSize - 1) * 0.5f;
		Color[] pixels = new Color[TextureSize * TextureSize];
		for (int y = 0; y < TextureSize; y++) {
			for (int x = 0; x < TextureSize; x++) {
				float nx = (x - center) / center;
				float ny = (y - center) / center;
				float distance = Mathf.Sqrt(nx * nx + ny * ny);
				// A soft falloff, squared so the core stays tight and crisp and the halo is faint.
				float alpha = 1f - Mathf.SmoothStep(0f, 1f, distance);
				alpha *= alpha;
				pixels[y * TextureSize + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
			}
		}
		texture.SetPixels(pixels);
		texture.Apply();
		return texture;
	}
}
