using Mafi;
using Mafi.Base;
using Mafi.Core.Economy;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Mods;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using UnityEngine;

namespace NightMod.LampPosts;

/// <summary>
/// Registers the <see cref="LampPostProto"/> with the game so the lamp post appears as a buildable
/// item in the decorations toolbar. Called from <see cref="NightMod.RegisterPrototypes"/>.
/// </summary>
internal static class LampPostRegistration {

	/// <summary>Prototype id of the lamp post building.</summary>
	public static readonly LampPostProto.ID LampPostId = new LampPostProto.ID("NightModLampPost");

	/// <summary>Electricity the lamp post draws while lit, in kilowatts.</summary>
	public static readonly Electricity PowerDrawKw = 400.Kw();

	/// <summary>Toolbar category the lamp post is filed under (the game's "Landmarks" decorations tab).</summary>
	private static readonly Proto.ID DecorationsLandmarksCategory = new Proto.ID("decorations_landmarksCategory");

	public static void Register(ProtoRegistrator registrator) {
		// Empty layout: the lamp post occupies no factory tiles, so the player can build it in a
		// tile corner or under other structures without it blocking anything - the same approach
		// the game uses for train-track poles.
		EntityLayout layout = EntityLayout.CreateEmpty(RelTile2i.Zero, RelTile2i.One);

		EntityCosts costs = new EntityCosts(
			AssetValue.FromProductId(Ids.Products.ConstructionParts, 5, registrator.PrototypesDb));

		// The light's whole definition lives here on the prototype's Gfx, so NightLampLights reads
		// it instead of hard-coding constants. Lens height matches the mesh so the light comes from
		// the emissive panel under the head.
		LampPostProto.Gfx graphics = new LampPostProto.Gfx(
			prefabPath: LampPostAssets.PrefabPath,
			lightRangeMeters: 18f,
			lightSpotAngleDegrees: 88f,
			lightIntensity: 4.2f,
			lensLocalHeight: LampPostMesh.LensLocalPosition.y,
			lightWarmColor: new Color(1f, 0.85f, 0.45f),
			lightCoolColor: new Color(0.55f, 0.7f, 1f),
			categories: registrator.GetCategoriesProtos(DecorationsLandmarksCategory));

		Proto.Str strings = Proto.CreateStr(LampPostId, "Lamp post",
			"A powered lamp post. It draws a small amount of electricity through the night to light " +
			"the ground around it, and nothing at all during the day.");

		LampPostProto proto = new LampPostProto(
			LampPostId, strings, layout, costs, PowerDrawKw, graphics);
		registrator.PrototypesDb.Add(proto);
		Log.Info($"NightMod: registered lamp post prototype '{LampPostId}'.");
	}
}
