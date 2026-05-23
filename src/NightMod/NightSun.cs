using Mafi;
using UnityEngine;

namespace NightMod;

/// <summary>
/// A small mod-drawn sun disc on the sky. Separate from the game's own skybox sun, this is a
/// billboard the mod fully controls: it follows the sun's arc, takes the current sunlight color,
/// and is hidden the moment the sun reaches the horizon. <see cref="NightCycleRenderer"/> places,
/// colors and fades it.
/// </summary>
public sealed class NightSun {

	private const int TextureSize = 128;

	// Sky size of the disc, as a fraction of the camera's far distance - a small, crisp circle.
	private const float AngularScale = 0.045f;

	private readonly GameObject m_disc;
	private readonly Mesh m_mesh;
	private readonly Material m_material;

	public NightSun() {
		m_mesh = createQuadMesh();
		m_disc = new GameObject("NightMod Sun");
		m_disc.AddComponent<MeshFilter>().sharedMesh = m_mesh;
		MeshRenderer renderer = m_disc.AddComponent<MeshRenderer>();
		renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		renderer.receiveShadows = false;
		Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Transparent");
		if (shader != null) {
			m_material = new Material(shader) { mainTexture = createSunTexture() };
			renderer.sharedMaterial = m_material;
		} else {
			Log.Warning("NightMod: no transparent shader found, the sun disc will not be drawn.");
		}
		m_disc.SetActive(false);
	}

	/// <summary>
	/// Places the sun disc for this frame. <paramref name="visibility"/> 0 hides it and 1 is fully
	/// opaque; <paramref name="elevation"/> and <paramref name="azimuth"/> are its sky angles, and
	/// <paramref name="color"/> tints it to match the current sunlight.
	/// </summary>
	public void Update(Camera camera, float elevation, float azimuth, float visibility, Color color) {
		if (camera == null || visibility <= 0f || m_material == null) {
			Hide();
			return;
		}
		if (!m_disc.activeSelf) {
			m_disc.SetActive(true);
		}

		Vector3 skyDirection = Quaternion.Euler(-elevation, azimuth, 0f) * Vector3.forward;
		float distance = camera.farClipPlane * 0.9f;
		m_disc.transform.position = camera.transform.position + skyDirection * distance;
		m_disc.transform.rotation = camera.transform.rotation;
		m_disc.transform.localScale = Vector3.one * (distance * AngularScale);

		color.a = Mathf.Clamp01(visibility);
		m_material.color = color;
	}

	/// <summary>Hides the sun disc without destroying it.</summary>
	public void Hide() {
		if (m_disc != null && m_disc.activeSelf) {
			m_disc.SetActive(false);
		}
	}

	/// <summary>Destroys the sun disc and its generated assets.</summary>
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
	}

	/// <summary>Builds a unit quad mesh centered on the origin.</summary>
	private static Mesh createQuadMesh() {
		Mesh mesh = new Mesh { name = "NightMod Sun Quad" };
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

	/// <summary>Builds a white sun texture: a solid bright core with a soft surrounding glow.</summary>
	private static Texture2D createSunTexture() {
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
				// A solid bright core, with a soft halo fading out around it.
				float core = 1f - Mathf.SmoothStep(0.42f, 0.6f, distance);
				float halo = (1f - Mathf.SmoothStep(0.5f, 1f, distance)) * 0.45f;
				float alpha = Mathf.Clamp01(core + halo);
				pixels[y * TextureSize + x] = new Color(1f, 1f, 1f, alpha);
			}
		}
		texture.SetPixels(pixels);
		texture.Apply();
		return texture;
	}
}
