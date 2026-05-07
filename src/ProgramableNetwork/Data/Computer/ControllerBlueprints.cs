using Mafi;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Blueprints;

namespace ProgramableNetwork
{
	/// <summary>
	/// Companion to <see cref="ModuleBlueprints"/> for saving entire controllers
	/// (not just single modules).  Module blueprints are useful for the picker-
	/// driven "drop a configured module into a controller" flow which the Python
	/// PLC templates already cover; this helper covers the other side — saving a
	/// fully configured controller (with its modules, layout, color, speed, and
	/// any internal cable connections) into the base game's
	/// <see cref="BlueprintsLibrary"/> so the player can paste the same controller
	/// elsewhere on the world map.
	///
	/// The on-disk shape is the standard <see cref="EntityConfigData"/> produced
	/// by <see cref="Controller.AddToConfig"/>, so the entry is also placeable
	/// from the base-game blueprint browser without any custom paste path.
	///
	/// Like the module variant, this is purely client-local — BlueprintsLibrary
	/// is per-player and saving from one client doesn't sync to others.
	/// </summary>
	public static class ControllerBlueprints
	{
		// All player-saved controller blueprints get this prefix on their library
		// title so they can be told apart from module blueprints (which use
		// [PN-Module]-) and from arbitrary controller blueprints the player makes
		// via the base copy-paste UI (no prefix).
		public const string TitlePrefix = "[PN-Controller]-";

		public static string MakeTitle(string userGivenName)
		{
			string trimmed = (userGivenName ?? "").Trim();
			return string.IsNullOrEmpty(trimmed) ? TitlePrefix.TrimEnd('-') : TitlePrefix + trimmed;
		}

		public static bool IsPnControllerBlueprint(IBlueprint bp)
		{
			return bp != null && bp.Name != null && bp.Name.StartsWith(TitlePrefix);
		}

		/// <summary>
		/// Snapshots the controller's full state via its existing
		/// <see cref="Controller.AddToConfig"/> implementation, registers the
		/// resulting blueprint in <paramref name="library"/>, and titles it
		/// <c>[PN-Controller]-{userGivenName}</c>.
		///
		/// All modules, their per-module data, internal cable connections, and the
		/// controller's color/speed are captured.  No mutation of the live
		/// controller — AddToConfig is read-only.
		/// </summary>
		public static Option<IBlueprint> Save(
			BlueprintsLibrary library,
			ConfigSerializationContext context,
			Controller controller,
			EntitiesCloneConfigHelper cloneConfigHelper,
			string userGivenName)
		{
			if (library == null || context == null || controller == null) {
				return Option<IBlueprint>.None;
			}

			// Route through the base game's clone cloneConfigHelper rather than building the
			// EntityConfigData by hand.  CreateConfigFrom populates the OriginalEntityId,
			// Transform, IsPaused, GeneralPriority, etc. AND calls AddToConfig — without
			// these the blueprint is missing fields that the base game's blueprint
			// placer assumes are present (especially Transform, which it dereferences
			// during paste).  Skipping the cloneConfigHelper was the source of the "blueprint menu
			// breaks after placement" crash.
			EntityConfigData data = cloneConfigHelper.CreateConfigFrom(controller);

			// Don't go through library.AddBlueprint — it always runs normalizeBlueprintPositions
			// which, for a single-entity blueprint, collapses Position to (0, 0, 0).  When the
			// player later places the blueprint, the placer's GetEstPlacementHeight returns
			// 0 - terrain[(0, 0)].Z and clamps it into the controller's PlacementHeightRange
			// (0, MAX_PILLAR_HEIGHT-1).  If world (0, 0) sits below sea level (negative
			// terrain Z), the clamp yields a positive RelativeHeight and the controller is
			// placed that many tiles ABOVE the cursor's terrain — hovering in mid-air.
			//
			// Base game cut/copy paste works because it never normalizes: Position.Xy stays
			// at the original tile and Position.Z == terrain[Position.Xy].Z, so
			// GetEstPlacementHeight is reliably 0.  Mirror that here by going through the
			// public string round-trip with doNotNormalizePositions=true; the blueprint keeps
			// the controller's original Transform on disk and places at terrain on paste.
			return saveWithoutNormalize(library, ImmutableArray.Create(data), MakeTitle(userGivenName),
				$"{controller.Prototype.Strings.Name.TranslatedString}  ({controller.Modules?.Count ?? 0} modules)");
		}

		internal static Option<IBlueprint> saveWithoutNormalize(
			BlueprintsLibrary library,
			ImmutableArray<EntityConfigData> items,
			string title,
			string description)
		{
			if (!library.TryCreateBlueprint(
				name: title,
				items: items,
				surfaceData: ImmutableArray<TileSurfaceCopyPasteData>.Empty,
				decalData: ImmutableArray<TileSurfaceCopyPasteData>.Empty,
				out IBlueprint blueprint,
				out string error,
				doNotNormalizePositions: true))
			{
				Log.Error("[Blueprint] TryCreateBlueprint failed: " + error);
				return Option<IBlueprint>.None;
			}

			string code = library.ConvertToString(blueprint);
			if (!library.TryAddBlueprintFromString(library.Root, code, out IBlueprintItem added))
			{
				Log.Error("[Blueprint] TryAddBlueprintFromString rejected the round-trip blueprint");
				return Option<IBlueprint>.None;
			}

			if (added is IBlueprint addedBlueprint)
			{
				library.SetDescription(addedBlueprint, description ?? "");
				return addedBlueprint.SomeOption();
			}
			return Option<IBlueprint>.None;
		}
	}
}
