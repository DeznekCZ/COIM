using Mafi;
using Mafi.Collections;
using Mafi.Unity;
using UnityEngine;

namespace NightMod.LampPosts;

/// <summary>
/// Supplies the lamp post's 3D model to the game without an asset bundle. The game resolves an
/// entity's model by looking its prototype's prefab path up in <see cref="AssetsDb"/>; this class
/// builds a plain GameObject (one <see cref="MeshFilter"/> + one <see cref="MeshRenderer"/> over
/// the procedural <see cref="LampPostMesh"/>) and inserts it under that path before any lamp post
/// is ever spawned, so the lookup finds it.
///
/// <para>This mirrors the prefab-injection approach used by the COIM CustomAssets mod. Entity
/// models are resolved lazily per prototype, so there is no renderer-cache ordering hazard - the
/// injection just has to happen before the first lamp post is placed or loaded.</para>
/// </summary>
internal static class LampPostAssets {

	/// <summary>Asset path the <see cref="LampPostProto"/> references and this class registers.</summary>
	public const string PrefabPath = "Assets/NightMod/LampPost.prefab";

	private static bool s_injected;

	/// <summary>
	/// Builds and registers the lamp post prefab into <paramref name="assetsDb"/>. Idempotent -
	/// safe to call more than once and a no-op if the path is already present.
	/// </summary>
	public static void Inject(AssetsDb assetsDb) {
		if (s_injected) {
			return;
		}
		Dict<string, Object> loaded = (Dict<string, Object>)assetsDb.LoadedAssets;
		if (!loaded.ContainsKey(PrefabPath)) {
			loaded[PrefabPath] = buildPrefab();
			Log.Info($"NightMod: injected lamp post prefab '{PrefabPath}'.");
		}
		s_injected = true;
	}

	private static GameObject buildPrefab() {
		GameObject go = new GameObject("NightMod_LampPost");
		// HideAndDontSave keeps the source prefab out of the scene hierarchy and save file; the
		// game clones it per placed entity. It stays active so the clones come up active too.
		go.hideFlags = HideFlags.HideAndDontSave;
		go.AddComponent<MeshFilter>().sharedMesh = LampPostMesh.Shared;

		// Two materials, one per submesh: a matte pole/head and an emissive lens panel.
		MeshRenderer renderer = go.AddComponent<MeshRenderer>();
		renderer.sharedMaterials = new[] { buildBodyMaterial(), buildLensMaterial() };
		return go;
	}

	/// <summary>Matte dark material for the pole and head (submesh 0).</summary>
	private static Material buildBodyMaterial() {
		Material material = new Material(Shader.Find("Standard")) {
			name = "NightMod_LampPost_Body",
			// A dark fixture; at night the cast cones light it, by day it just reads as a dark post.
			color = new Color(0.20f, 0.21f, 0.23f),
		};
		// Matte, non-metallic: the Standard shader defaults to a glossy half-smooth surface, which
		// gave the post unwanted reflections. Drive it fully matte so it reads as a painted pole.
		material.SetFloat("_Metallic", 0f);
		material.SetFloat("_Glossiness", 0.1f);
		return material;
	}

	/// <summary>
	/// Emissive material for the lens panel under the head (submesh 1). The emission makes the
	/// underside read as a glowing bulb, so the cast light cone visibly originates from a lit
	/// surface rather than thin air.
	/// </summary>
	private static Material buildLensMaterial() {
		Material material = new Material(Shader.Find("Standard")) {
			name = "NightMod_LampPost_Lens",
			color = new Color(1f, 0.96f, 0.85f),
		};
		material.SetFloat("_Metallic", 0f);
		material.SetFloat("_Glossiness", 0.2f);
		material.EnableKeyword("_EMISSION");
		material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
		// A bright warm-white emission (HDR, above 1) so the panel clearly glows.
		material.SetColor("_EmissionColor", new Color(1f, 0.92f, 0.72f) * 1.8f);
		return material;
	}
}
