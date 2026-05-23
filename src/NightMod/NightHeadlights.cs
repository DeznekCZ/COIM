using System.Collections.Generic;
using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Dynamic;
using Mafi.Core.Trains;
using Mafi.Unity.Entities;
using UnityEngine;

namespace NightMod;

/// <summary>
/// Casts real spotlight cones from vehicle headlights so vehicles light up the ground around them
/// at night, and ties the vehicles' emissive headlight glow to the cycle so it is dark by day.
///
/// <para>The game's vehicles only ever <em>glow</em>: a <c>VehicleLightsController</c> brightens the
/// emissive headlight lens via the <c>_EmissionStrength</c> shader float, following the real game's
/// clock. No light is cast on the terrain. This component adds the missing cast light and overrides
/// the glow so both follow the day-night cycle.</para>
///
/// <para>Only entities that actually have headlights are lit - road <see cref="Vehicle"/>s (trucks,
/// excavators, tree machines) and <see cref="Locomotive"/>s - not buildings or billboards. Their
/// meshes are not CPU-readable, so the cones cannot be traced from the emissive texels; instead two
/// cones are placed at the front of each vehicle, aimed along its facing. The cone <em>reach</em> is
/// scaled by the vehicle's size and its <em>brightness</em> by the original headlight emission
/// strength. This is solved once per type and cached; the cones are built as children of the vehicle
/// so they follow it with no per-frame placement work, and stay lit through the night.</para>
/// </summary>
public sealed class NightHeadlights {

	// Real-time seconds between scene scans for newly spawned vehicles. Setup per vehicle is
	// one-shot; this only controls how soon a fresh vehicle gets its cones.
	private const float DiscoveryIntervalSeconds = 1.5f;

	// Cone spread, and a downward droop so the beam pools on the ground ahead of the vehicle.
	private const float SpotAngleDegrees = 58f;
	private const float PitchDownDegrees = 16f;

	// Cone reach is the vehicle's largest dimension times this factor, clamped. Kept generous so
	// the beam carries a good distance ahead of the vehicle.
	private const float RangeSizeFactor = 7f;
	private const float RangeMin = 18f;
	private const float RangeMax = 85f;

	// Cone brightness is the vehicle's original headlight emission strength times this factor,
	// clamped. The original strength is read from the material before the glow is overridden.
	private const float IntensityPerEmission = 1.2f;
	private const float IntensityMin = 1.1f;
	private const float IntensityMax = 4.5f;

	// Used when a vehicle's original emission strength reads as zero.
	private const float DefaultEmissionStrength = 1.5f;

	// Headlight placement within the vehicle's local bounds: out to the sides, low at the front.
	private const float HalfWidthFraction = 0.55f;
	private const float HeightFraction = -0.2f;
	private const float ForwardPadding = 0.3f;
	private const float MinHeadlightHalfSpread = 0.5f;

	// The value driven into the vehicle's emissive _EmissionStrength at full night, so the headlight
	// lens glows; it scales down with the cycle and reaches 0 by day.
	private const float EmissionGlowStrength = 2f;

	// Built-in forward rendering caps how many lights are drawn per-pixel per object
	// (QualitySettings.pixelLightCount). The mod raises it to at least this so a handful of overlap-
	// ping headlight cones all render instead of competing for a few slots.
	private const int TargetPixelLightCount = 12;

	/// <summary>Warm tungsten headlight color.</summary>
	private static readonly Color HeadlightColor = new Color(1f, 0.93f, 0.78f);

	// The game's shader float for the emissive headlight glow.
	private static readonly int s_emissionStrengthId = Shader.PropertyToID("_EmissionStrength");

	// Headlight layout solved once per vehicle prototype and reused for every vehicle of that type.
	private readonly Dictionary<EntityProto.ID, ProtoHeadlights> m_protoCache;
	// Rigs are keyed by the sim entity's id - that lasts the vehicle's whole life - not by the
	// render object, which the game pools and rebuilds (which used to lose the rig).
	private readonly Dictionary<EntityId, Rig> m_rigs;
	private readonly HashSet<EntityMb> m_nonVehicles;
	private readonly List<EntityId> m_stale;

	private float m_discoveryTimer;
	private float m_currentLevel;
	private readonly int m_originalPixelLightCount;

	public NightHeadlights() {
		m_protoCache = new Dictionary<EntityProto.ID, ProtoHeadlights>();
		m_rigs = new Dictionary<EntityId, Rig>();
		m_nonVehicles = new HashSet<EntityMb>();
		m_stale = new List<EntityId>();

		// Raise the per-pixel light cap so overlapping headlight cones all render. Without this the
		// built-in forward renderer keeps only a few per-pixel lights and drops the rest based on
		// their relevance to the camera - which is what makes the cones flicker as the camera moves.
		m_originalPixelLightCount = QualitySettings.pixelLightCount;
		if (m_originalPixelLightCount < TargetPixelLightCount) {
			QualitySettings.pixelLightCount = TargetPixelLightCount;
		}
	}

	/// <summary>
	/// Drives the headlights for this frame. <paramref name="level"/> is the cycle's headlight
	/// strength - 0 in full daytime, ramping up through dusk, 1 through the night and back down at
	/// dawn. New vehicles are discovered on a timer; brightness and the emissive glow are refreshed
	/// for every rig each frame. There is no distance culling - the cones are lit at any range, and
	/// Unity's own frustum culling is left to skip the ones that are off-screen.
	/// </summary>
	public void Update(float level) {
		m_currentLevel = Mathf.Clamp01(level);
		discover();
		foreach (KeyValuePair<EntityId, Rig> pair in m_rigs) {
			refreshRig(pair.Value);
		}
	}

	/// <summary>Switches every cone and headlight glow off (used during the day or when disabled).</summary>
	public void HideAll() {
		m_currentLevel = 0f;
		foreach (KeyValuePair<EntityId, Rig> pair in m_rigs) {
			refreshRig(pair.Value);
		}
	}

	/// <summary>Destroys every cone and restores the per-pixel light cap. Call on shutdown.</summary>
	public void Destroy() {
		foreach (KeyValuePair<EntityId, Rig> pair in m_rigs) {
			if (pair.Value.Root != null) {
				Object.Destroy(pair.Value.Root);
			}
		}
		m_rigs.Clear();
		QualitySettings.pixelLightCount = m_originalPixelLightCount;
	}

	/// <summary>
	/// Periodically scans the scene for newly spawned vehicles and gives each one its cones, and
	/// drops rigs whose vehicle is gone. Runs on a timer, not every frame - this is the only costly
	/// step, so it is kept off the per-frame path.
	/// </summary>
	private void discover() {
		m_discoveryTimer -= Time.deltaTime;
		if (m_discoveryTimer > 0f) {
			return;
		}
		m_discoveryTimer = DiscoveryIntervalSeconds;

		// Drop rigs whose sim entity - the vehicle itself, not its poolable render object - is gone.
		m_stale.Clear();
		foreach (KeyValuePair<EntityId, Rig> pair in m_rigs) {
			IEntity gameEntity = pair.Value.GameEntity;
			if (gameEntity == null || gameEntity.IsDestroyed) {
				m_stale.Add(pair.Key);
				if (pair.Value.Root != null) {
					Object.Destroy(pair.Value.Root);
				}
			}
		}
		foreach (EntityId id in m_stale) {
			m_rigs.Remove(id);
		}
		m_nonVehicles.RemoveWhere(e => e == null || e.IsDestroyed);

		// Link each render object to its rig - re-linking when the game has pooled and rebuilt it,
		// which keeps the rig (and so the headlights) alive across that - or set up a new rig.
		foreach (EntityMb mb in Object.FindObjectsByType<EntityMb>(FindObjectsSortMode.None)) {
			if (mb == null || !mb.IsInitialized || m_nonVehicles.Contains(mb)) {
				continue;
			}
			IEntity gameEntity = mb.Entity;
			if (gameEntity == null) {
				continue;
			}
			if (!(gameEntity is Vehicle) && !(gameEntity is Locomotive)) {
				m_nonVehicles.Add(mb);
				continue;
			}
			if (m_rigs.TryGetValue(gameEntity.Id, out Rig rig)) {
				if (rig.Mb != mb) {
					// The game rebuilt this vehicle's render object - rebuild the cones, now
					// parented under the new one.
					buildCones(rig, mb);
				}
				continue;
			}
			trySetUp(gameEntity, mb);
		}
	}

	/// <summary>
	/// Builds the cones for one vehicle, using its prototype's cached headlight layout (analysing
	/// the type the first time it is seen). Types with no detectable headlights are remembered so
	/// they are not inspected again.
	/// </summary>
	private void trySetUp(IEntity gameEntity, EntityMb mb) {
		EntityProto.ID protoId = gameEntity.Prototype.Id;
		if (!m_protoCache.TryGetValue(protoId, out ProtoHeadlights layout)) {
			layout = analyzeProto(mb, gameEntity.Prototype);
			m_protoCache[protoId] = layout;
		}
		if (layout.Emitters.Length == 0) {
			m_nonVehicles.Add(mb);
			return;
		}

		Rig rig = new Rig { GameEntity = gameEntity, Layout = layout };
		buildCones(rig, mb);
		m_rigs[gameEntity.Id] = rig;
		refreshRig(rig);
	}

	/// <summary>
	/// Builds, or rebuilds, a rig's cones - parented under the given render object. Parenting them
	/// to the vehicle is what keeps the cones glued to it: they move in the very same transform
	/// update as the vehicle, so the beam never lags or jitters behind it. The game's
	/// VehicleLightsController only strips child lights when the render object is first created -
	/// before any rig is set up - so the cones added here survive; if the game later rebuilds the
	/// render object, this simply runs again for the new one.
	/// </summary>
	private static void buildCones(Rig rig, EntityMb mb) {
		if (rig.Root != null) {
			Object.Destroy(rig.Root);
		}
		GameObject root = new GameObject("NightMod Headlights");
		root.transform.SetParent(mb.transform, false);
		EmitterPoint[] emitters = rig.Layout.Emitters;
		Light[] cones = new Light[emitters.Length];
		for (int i = 0; i < emitters.Length; i++) {
			cones[i] = createCone(root.transform, emitters[i], rig.Layout.Range, rig.Layout.Intensity);
		}
		rig.Mb = mb;
		rig.Root = root;
		rig.Cones = cones;
		rig.EmissiveMaterials = collectEmissiveMaterials(mb);
	}

	/// <summary>
	/// Builds one spotlight cone under the rig root, placed at the emitter. Position and intensity
	/// are set once and never change - as a child of the vehicle the cone follows it for free, and
	/// the light is only ever toggled fully on or off, never dimmed.
	/// </summary>
	private static Light createCone(Transform parent, EmitterPoint emitter, float range, float intensity) {
		GameObject go = new GameObject("Cone");
		go.transform.SetParent(parent, false);
		go.transform.localPosition = emitter.LocalPosition;
		go.transform.localRotation = Quaternion.LookRotation(emitter.LocalDirection, Vector3.up);
		Light light = go.AddComponent<Light>();
		light.type = LightType.Spot;
		light.spotAngle = SpotAngleDegrees;
		light.range = range;
		light.color = HeadlightColor;
		light.intensity = intensity;
		// Shadowless: a fleet of shadow-casting spots would be far too expensive, and the cones are
		// only meant to lift the ground out of darkness.
		light.shadows = LightShadows.Soft;
		// ForcePixel, not Auto: Auto lets the renderer downgrade or drop the light based on its
		// relevance to the camera, which makes the cone flicker as the camera moves. ForcePixel
		// keeps it a stable per-pixel light.
		light.renderMode = LightRenderMode.ForcePixel;
		return light;
	}

	/// <summary>
	/// The vehicle's instanced materials that carry the emissive headlight glow, gathered from
	/// <em>every</em> LOD (inactive renderers included). The glow override must reach all LODs - if
	/// only LOD 0 were driven, a vehicle far enough to show a lower LOD would keep glowing by day.
	/// </summary>
	private static Material[] collectEmissiveMaterials(EntityMb entity) {
		List<Material> result = new List<Material>();
		foreach (Renderer renderer in entity.GetComponentsInChildren<Renderer>(true)) {
			if (renderer == null) {
				continue;
			}
			foreach (Material material in renderer.sharedMaterials) {
				if (material != null && material.HasProperty(s_emissionStrengthId)
					&& !result.Contains(material)) {
					result.Add(material);
				}
			}
		}
		return result.ToArray();
	}

	/// <summary>
	/// The vehicle's LOD 0 (highest-detail) mesh renderers. When the model has LOD groups, LOD 0's
	/// renderers are taken straight from them - including ones currently disabled because a lower
	/// LOD is on show - so the headlight analysis never depends on which LOD happens to be active.
	/// A model without LOD groups simply yields all its mesh renderers.
	/// </summary>
	private static List<MeshRenderer> getLod0Renderers(EntityMb entity) {
		List<MeshRenderer> result = new List<MeshRenderer>();
		LODGroup[] lodGroups = entity.GetComponentsInChildren<LODGroup>(true);
		foreach (LODGroup lodGroup in lodGroups) {
			LOD[] lods = lodGroup.GetLODs();
			if (lods.Length == 0) {
				continue;
			}
			foreach (Renderer renderer in lods[0].renderers) {
				if (renderer is MeshRenderer meshRenderer && meshRenderer != null) {
					result.Add(meshRenderer);
				}
			}
		}
		if (result.Count > 0) {
			return result;
		}
		// No LOD groups - just take every mesh renderer (inactive included, for safety).
		foreach (MeshRenderer renderer in entity.GetComponentsInChildren<MeshRenderer>(true)) {
			result.Add(renderer);
		}
		return result;
	}

	/// <summary>
	/// Refreshes one rig for the current cycle level. The cones and the emissive headlight glow are
	/// simply toggled on or off - on whenever the cycle calls for headlights, off otherwise - never
	/// dimmed. The cones do not need positioning here: they are children of the vehicle and follow
	/// it on their own.
	/// </summary>
	private void refreshRig(Rig rig) {
		bool lit = m_currentLevel > 0f;
		foreach (Light cone in rig.Cones) {
			if (cone != null && cone.gameObject.activeSelf != lit) {
				cone.gameObject.SetActive(lit);
				cone.intensity = lit ? rig.Layout.Intensity * m_currentLevel : 0f;
			}
		}

		// Toggle the game's emissive headlight glow with the cones - full glow at night, fully off
		// by day - so it follows this mod's cycle rather than the real game clock.
		// TODO must be cached per-material per-vehicle for efficiency, is not needed to update emission every frame
		float glow = EmissionGlowStrength * m_currentLevel;
		foreach (Material material in rig.EmissiveMaterials) {
			if (material != null) {
				material.SetFloat(s_emissionStrengthId, glow);
			}
		}
	}

	/// <summary>
	/// Works out a vehicle type's headlights once, from its LOD 0 model. The cone reach is scaled by
	/// the vehicle's size and the cone brightness by the original <c>_EmissionStrength</c> of its
	/// headlight material (read before the glow is overridden). Two cones are placed at the front of
	/// the vehicle - its meshes are not CPU-readable, so the emissive lens cannot be traced; the
	/// front of the bounds, aimed along the model's forward (local +X), is used instead. The layout
	/// is in vehicle-local space, so it transfers to every vehicle of the type.
	/// </summary>
	private static ProtoHeadlights analyzeProto(EntityMb entity, EntityProto proto) {
		Transform root = entity.transform;
		List<MeshRenderer> renderers = getLod0Renderers(entity);
		Bounds local = default;
		bool hasBounds = false;
		float originalStrength = 0f;

		foreach (MeshRenderer renderer in renderers) {
			if (renderer == null) {
				continue;
			}
			// Bounds from localBounds (the mesh AABB) - valid even for a LOD 0 renderer that is
			// currently disabled because a lower LOD is on show.
			Bounds rendererBounds = renderer.localBounds;
			Transform rendererTransform = renderer.transform;
			for (int corner = 0; corner < 8; corner++) {
				Vector3 localCorner = rendererBounds.center + new Vector3(
					(corner & 1) == 0 ? -rendererBounds.extents.x : rendererBounds.extents.x,
					(corner & 2) == 0 ? -rendererBounds.extents.y : rendererBounds.extents.y,
					(corner & 4) == 0 ? -rendererBounds.extents.z : rendererBounds.extents.z);
				Vector3 point = root.InverseTransformPoint(rendererTransform.TransformPoint(localCorner));
				if (!hasBounds) {
					local = new Bounds(point, Vector3.zero);
					hasBounds = true;
				} else {
					local.Encapsulate(point);
				}
			}
			foreach (Material material in renderer.sharedMaterials) {
				if (material != null && originalStrength <= 0f && material.HasProperty(s_emissionStrengthId)) {
					originalStrength = material.GetFloat(s_emissionStrengthId);
				}
			}
		}

		if (originalStrength < 0.01f) {
			originalStrength = DefaultEmissionStrength;
		}
		float intensity = Mathf.Clamp(originalStrength * IntensityPerEmission, IntensityMin, IntensityMax);

		if (!hasBounds) {
			Log.Warning($"NightMod: '{proto.Id}' has no LOD 0 renderers - no headlight cones added.");
			return new ProtoHeadlights(System.Array.Empty<EmitterPoint>(), RangeMin, intensity);
		}

		float range = Mathf.Clamp(maxComponent(local.size) * RangeSizeFactor, RangeMin, RangeMax);
		EmitterPoint[] emitters = buildFrontEmitters(local);
		Log.Info($"NightMod: '{proto.Id}' headlights - 2 cones at the front of the LOD 0 model, "
			+ $"range {range:0.0}, intensity {intensity:0.0}.");
		return new ProtoHeadlights(emitters, range, intensity);
	}

	/// <summary>
	/// Places a left and a right cone at the front of the vehicle's local bounds. The vehicle's
	/// forward is its model's local +X axis - the game spawns vehicle models facing +X, the
	/// heading-zero direction. The cones sit low on the front face, aimed forward and drooped down.
	/// </summary>
	private static EmitterPoint[] buildFrontEmitters(Bounds local) {
		Vector3 extents = local.extents;
		float frontX = local.center.x + extents.x + ForwardPadding;
		float height = local.center.y + extents.y * HeightFraction;
		float halfWidth = Mathf.Max(extents.z * HalfWidthFraction, MinHeadlightHalfSpread);

		// Local +X drooped downward (rotated around the local side axis, which is -Z for an +X beam).
		Vector3 direction = Quaternion.AngleAxis(PitchDownDegrees, Vector3.back) * Vector3.right;
		return new[] {
			new EmitterPoint(new Vector3(frontX, height, local.center.z - halfWidth), direction),
			new EmitterPoint(new Vector3(frontX, height, local.center.z + halfWidth), direction),
		};
	}

	private static float maxComponent(Vector3 v) {
		return Mathf.Max(v.x, Mathf.Max(v.y, v.z));
	}

	/// <summary>A headlight lens: where the cone sits and which way it points, in vehicle-local space.</summary>
	private readonly struct EmitterPoint {

		public readonly Vector3 LocalPosition;
		public readonly Vector3 LocalDirection;

		public EmitterPoint(Vector3 localPosition, Vector3 localDirection) {
			LocalPosition = localPosition;
			LocalDirection = localDirection;
		}
	}

	/// <summary>One vehicle type's solved headlight layout - cached and reused for every instance.</summary>
	private readonly struct ProtoHeadlights {

		public readonly EmitterPoint[] Emitters;
		public readonly float Range;
		public readonly float Intensity;

		public ProtoHeadlights(EmitterPoint[] emitters, float range, float intensity) {
			Emitters = emitters;
			Range = range;
			Intensity = intensity;
		}
	}

	/// <summary>
	/// One vehicle's headlights. <see cref="GameEntity"/> is the stable sim entity the rig is keyed
	/// by; <see cref="Mb"/> is its current render object, and the cones are parented under it.
	/// <see cref="Layout"/> is kept so the cones can be rebuilt if the game replaces that object.
	/// </summary>
	private sealed class Rig {

		public IEntity GameEntity;
		public EntityMb Mb;
		public ProtoHeadlights Layout;
		public GameObject Root;
		public Light[] Cones;
		public Material[] EmissiveMaterials;
	}
}
