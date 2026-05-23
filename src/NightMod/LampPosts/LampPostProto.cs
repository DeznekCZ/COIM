using System;
using Mafi;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.ElectricPower;
using Mafi.Core.Prototypes;
using UnityEngine;

namespace NightMod.LampPosts;

/// <summary>
/// Prototype for the lamp post building. It is a minimal powered layout entity: it draws a small
/// fixed amount of electricity and otherwise does nothing on the simulation side - the actual
/// light is a render-only effect driven by <see cref="NightLampLights"/>, configured from this
/// prototype's <see cref="Graphics"/>.
///
/// <para>The matching runtime entity is <see cref="LampPost"/>; the game's
/// <c>DefaultStaticEntityFactory</c> constructs it reflectively from <see cref="EntityType"/>, so
/// no custom entity factory has to be registered.</para>
/// </summary>
public sealed class LampPostProto : LayoutEntityProto, IProtoWithPowerConsumption {

	/// <summary>Fixed electricity the lamp post draws while it is enabled.</summary>
	public Electricity ElectricityConsumed { get; }

	/// <summary>The runtime entity type the game instantiates for this prototype.</summary>
	public override Type EntityType => typeof(LampPost);

	/// <summary>Strongly-typed graphics; hides the base <c>Graphics</c> with the lamp-specific Gfx.</summary>
	public new Gfx Graphics => (Gfx)base.Graphics;

	public LampPostProto(ID id, Str strings, EntityLayout layout, EntityCosts costs,
		Electricity electricityConsumed, Gfx graphics)
		: base(id, strings, layout, costs, graphics) {
		ElectricityConsumed = electricityConsumed;
	}

	/// <summary>
	/// Graphics for a lamp post: the standard layout-entity prefab data plus the cast-light
	/// definition (range, cone angle, intensity, lens position, colour swatch). The light system
	/// reads everything it needs to build a lamp's spot light from here, so the visuals are
	/// configurable per-prototype rather than hard-coded in the renderer.
	/// </summary>
	public new class Gfx : LayoutEntityProto.Gfx {

		/// <summary>Spot-light range, in Unity units (≈ metres).</summary>
		public readonly float LightRangeMeters;

		/// <summary>Spot-light cone angle, in degrees.</summary>
		public readonly float LightSpotAngleDegrees;

		/// <summary>Spot-light intensity when on.</summary>
		public readonly float LightIntensity;

		/// <summary>
		/// Local-space height (in Unity units, above the model base) of the lens the light shines
		/// from. The light system places the spot light here in world space.
		/// </summary>
		public readonly float LensLocalHeight;

		/// <summary>Light tint at the warm end of the per-lamp hue range.</summary>
		public readonly Color LightWarmColor;

		/// <summary>Light tint at the cool end of the per-lamp hue range.</summary>
		public readonly Color LightCoolColor;

		public Gfx(string prefabPath,
			float lightRangeMeters,
			float lightSpotAngleDegrees,
			float lightIntensity,
			float lensLocalHeight,
			Color lightWarmColor,
			Color lightCoolColor,
			ImmutableArray<ToolbarEntryData>? categories = null)
			: base(prefabPath, categories: categories) {
			LightRangeMeters = lightRangeMeters;
			LightSpotAngleDegrees = lightSpotAngleDegrees;
			LightIntensity = lightIntensity;
			LensLocalHeight = lensLocalHeight;
			LightWarmColor = lightWarmColor;
			LightCoolColor = lightCoolColor;
		}
	}
}
