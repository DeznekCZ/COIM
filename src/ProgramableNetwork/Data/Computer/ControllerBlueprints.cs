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
			string userGivenName)
		{
			if (library == null || context == null || controller == null) {
				return Option<IBlueprint>.None;
			}

			// Route through the base game's clone helper rather than building the
			// EntityConfigData by hand.  CreateConfigFrom populates the OriginalEntityId,
			// Transform, IsPaused, GeneralPriority, etc. AND calls AddToConfig — without
			// these the blueprint is missing fields that the base game's blueprint
			// placer assumes are present (especially Transform, which it dereferences
			// during paste).  Skipping the helper was the source of the "blueprint menu
			// breaks after placement" crash.
			EntitiesCloneConfigHelper helper = GlobalDependencyResolver.Get<EntitiesCloneConfigHelper>();
			EntityConfigData data = helper.CreateConfigFrom(controller);

			Option<IBlueprint> created = library.AddBlueprint(
				library.Root,
				ImmutableArray.Create(data),
				ImmutableArray<TileSurfaceCopyPasteData>.Empty,
				ImmutableArray<TileSurfaceCopyPasteData>.Empty);

			if (created.HasValue)
			{
				library.RenameItem(created.Value, MakeTitle(userGivenName));
				int moduleCount = controller.Modules?.Count ?? 0;
				library.SetDescription(created.Value,
					$"{controller.Prototype.Strings.Name.TranslatedString}  ({moduleCount} modules)");
			}
			return created;
		}
	}
}
