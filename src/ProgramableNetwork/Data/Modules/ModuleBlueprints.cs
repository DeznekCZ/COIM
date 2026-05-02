using Mafi;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Blueprints;
using System;
using System.Collections.Generic;

namespace ProgramableNetwork
{
	/// <summary>
	/// Central helpers for the "save module as blueprint" feature.
	///
	/// Storage: each blueprint is one EntityConfigData wrapping a synthetic Controller
	/// that contains exactly the user-picked module on its first slot. We piggy-back on
	/// <see cref="Controller.AddToConfig"/> so the on-disk shape is identical to a
	/// regular controller blueprint — meaning the entry shows up in the base-game
	/// blueprints library too (with its generic icon).  Our own picker scans the
	/// library for entries whose title starts with <see cref="TitlePrefix"/> and
	/// renders them with a real ModuleView preview.
	///
	/// This is purely client-local — <see cref="BlueprintsLibrary"/> is per-player.
	/// No multiplayer command involved; saving from one client doesn't sync to others.
	/// </summary>
	public static class ModuleBlueprints
	{
		// All player-saved module blueprints get this prefix on their library Title so
		// the picker scanner can identify them without interrogating the payload.
		public const string TitlePrefix = "[PN-Module]-";

		// Folder name used when auto-creating a dedicated subfolder for these blueprints.
		// If the folder doesn't exist yet, we save into the library root and let the
		// player move them.  Avoids needing to mutate folder tree on every save.
		public const string FolderName = "[PN-Module]";

		public static string MakeTitle(string userGivenName)
		{
			string trimmed = (userGivenName ?? "").Trim();
			return string.IsNullOrEmpty(trimmed) ? TitlePrefix.TrimEnd('-') : TitlePrefix + trimmed;
		}

		public static bool IsPnModuleBlueprint(IBlueprint bp)
		{
			return bp != null && bp.Name != null && bp.Name.StartsWith(TitlePrefix);
		}

		// Walks the library tree (root folder + nested folders, depth-first) and yields
		// every blueprint whose title starts with our prefix.  Defensive against null
		// references that can show up in partially loaded libraries.
		public static IEnumerable<IBlueprint> EnumerateAll(BlueprintsLibrary library)
		{
			if (library?.Root == null) {
				yield break;
			}
			foreach (var bp in walk(library.Root)) {
				yield return bp;
			}
		}

		private static IEnumerable<IBlueprint> walk(IBlueprintsFolder folder)
		{
			if (folder == null) {
				yield break;
			}
			if (folder.Blueprints != null)
			{
				foreach (var bp in folder.Blueprints.AsEnumerable())
				{
					if (IsPnModuleBlueprint(bp)) {
						yield return bp;
					}
				}
			}
			if (folder.Folders != null)
			{
				foreach (var sub in folder.Folders.AsEnumerable())
				{
					foreach (var bp in walk(sub)) {
						yield return bp;
					}
				}
			}
		}

		/// <summary>
		/// Snapshots the given module + its host controller into an EntityConfigData,
		/// then registers the resulting blueprint in <paramref name="library"/> with
		/// the title <c>[PN-Module]-{userGivenName}</c>.
		///
		/// The host controller is included as-is (single module on the first slot)
		/// so any future placement keeps the controller wrapper, exactly per the
		/// design discussion — no special "single module" payload format.
		/// </summary>
		public static Option<IBlueprint> Save(
			BlueprintsLibrary library,
			ConfigSerializationContext context,
			Module module,
			string userGivenName)
		{
			if (library == null || context == null || module?.Controller == null) {
				return Option<IBlueprint>.None;
			}

			Controller src = module.Controller;
			// Use the base game's clone helper to produce a complete, placeable
			// EntityConfigData (Transform, OriginalEntityId, IsPaused, GeneralPriority,
			// auxiliary state, AND the result of Controller.AddToConfig).  Constructing
			// the config by hand was the source of the "blueprint menu breaks after
			// placing" crash — the base placer dereferences fields like Transform that
			// we hadn't populated.
			//
			// CreateConfigFrom captures every module on the source controller; we then
			// overwrite controller_modules in-place with just the one the player asked
			// to save.  A short save/restore dance around the picked module's Row,
			// Column, and InputModules keeps the snapshot clean of cable connections
			// (stale refs to other modules) and re-anchors it to (0, 0).
			EntitiesCloneConfigHelper helper = GlobalDependencyResolver.Get<EntitiesCloneConfigHelper>();
			EntityConfigData data = helper.CreateConfigFrom(src);

			int origRow = module.Row;
			int origCol = module.Column;
			Mafi.Collections.Dict<string, ModuleConnector> origInputs = module.InputModules;
			module.Row = 0;
			module.Column = 0;
			// Swap in an empty Dict for the duration of the write.  Replacing the
			// reference (rather than calling .Clear()) keeps the original Dict intact
			// in case anything else holds a snapshot of it.
			typeof(Module)
				.GetProperty(nameof(Module.InputModules))
				.SetValue(module, new Mafi.Collections.Dict<string, ModuleConnector>());
			try
			{
				// Replace the multi-module array CreateConfigFrom wrote with a single-module
				// array.  Same key, same writer fn — base game treats it identically.
				data.SetArray<Module>(
					"controller_modules",
					ImmutableArray.Create(module),
					Module.Serialize);
			}
			finally
			{
				// Restore live state — the source controller still owns this module
				// at its actual position with its actual cables, we were just
				// temporarily writing a clean snapshot.
				module.Row = origRow;
				module.Column = origCol;
				typeof(Module)
					.GetProperty(nameof(Module.InputModules))
					.SetValue(module, origInputs);
			}

			Option<IBlueprint> created = library.AddBlueprint(
				library.Root,
				ImmutableArray.Create(data),
				ImmutableArray<TileSurfaceCopyPasteData>.Empty,
				ImmutableArray<TileSurfaceCopyPasteData>.Empty);

			if (created.HasValue)
			{
				library.RenameItem(created.Value, MakeTitle(userGivenName));
				library.SetDescription(created.Value,
					$"{module.Prototype.Symbol}  {module.Prototype.Strings.Name.TranslatedString}");
			}
			return created;
		}

		/// <summary>
		/// Returns the proto of the single module stored in <paramref name="bp"/>, or
		/// None if the blueprint is malformed (zero/multi-module) or its proto can't
		/// be resolved in the current ProtosDb (e.g. mod uninstalled).
		/// </summary>
		public static Option<ModuleProto> ResolveStoredProto(IBlueprint bp, Mafi.Core.Prototypes.ProtosDb protosDb)
		{
			if (bp == null || bp.Items.Length == 0 || protosDb == null) {
				return Option<ModuleProto>.None;
			}
			ImmutableArray<Module>? modules = bp.Items[0].GetArray<Module>("controller_modules", Module.Deserialize);
			if (modules == null || modules.Value.Length != 1) {
				return Option<ModuleProto>.None;
			}
			Module raw = modules.Value[0];
			if (raw == null) {
				return Option<ModuleProto>.None;
			}
			// raw.Prototype isn't wired up yet (Module.Deserialize stores only the proto id).
			// We use reflection-free access via the public surface: the deserialized module
			// carries m_protoId privately, but its public Prototype getter returns null until
			// initContexts runs.  Rather than dig it out here, just look through every loaded
			// proto and match by Id from the saved module's StringData/Prototype lookup —
			// done in TryExtractInto where we already have a Module instance and a Context.
			return protosDb.Get<ModuleProto>(GetStoredProtoId(raw));
		}

		/// <summary>
		/// Extracts the module proto id from a freshly-deserialized (uninitialized) Module.
		/// Module.Prototype isn't usable until initContexts runs, but the proto id was
		/// already read off the wire and stashed.  Falls back to empty id on missing data.
		/// </summary>
		private static Mafi.Core.Prototypes.Proto.ID GetStoredProtoId(Module m)
		{
			// The private string field m_protoId is the source of truth post-deserialize.
			// Use reflection — the field is private and there's no public accessor pre-init.
			System.Reflection.FieldInfo fi = typeof(Module).GetField(
				"m_protoId",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			string id = fi?.GetValue(m) as string ?? "";
			return new Mafi.Core.Prototypes.Proto.ID(id);
		}

		/// <summary>
		/// Builds a fresh <see cref="Module"/> on <paramref name="dst"/> from the
		/// blueprint payload, runs init, and returns it.  Caller is responsible
		/// for placing it via <see cref="Ui.ControllerView.TryPlaceAt"/> or similar.
		/// Returns None if the proto can't be resolved or the payload is malformed.
		/// </summary>
		public static Option<Module> TryExtractInto(IBlueprint bp, Controller dst)
		{
			if (bp == null || dst == null || bp.Items.Length == 0) {
				return Option<Module>.None;
			}
			ImmutableArray<Module>? modules = bp.Items[0].GetArray<Module>("controller_modules", Module.Deserialize);
			if (modules == null || modules.Value.Length != 1) {
				return Option<Module>.None;
			}
			Module raw = modules.Value[0];
			if (raw == null) {
				return Option<Module>.None;
			}
			// Wire the deserialized module to the new host and run the post-load init
			// that resolves its prototype.  initContexts is the same path used by
			// Controller.ApplyConfig, so behavior matches base-game blueprint pasting.
			raw.Context = dst.Context;
			raw.Controller = dst;
			try
			{
				raw.initContexts(-1);
			}
			catch (Exception e)
			{
				Log.Exception(e);
				return Option<Module>.None;
			}
			if (raw.Prototype == null || raw.Prototype == ModuleProto.Phantom) {
				return Option<Module>.None;
			}
			// Give it a fresh module Id so it doesn't collide with the saved one if the
			// player imports the same blueprint twice.  Mirrors the Module ctor approach.
			System.Reflection.PropertyInfo idProp = typeof(Module).GetProperty(
				nameof(Module.Id),
				System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
			idProp?.SetValue(raw, DateTime.UtcNow.Ticks);
			System.Threading.Thread.Sleep(1);

			foreach (IField field in raw.Prototype.Fields)
			{
				field.Validate(raw);
			}
			return raw.SomeOption();
		}
	}
}
