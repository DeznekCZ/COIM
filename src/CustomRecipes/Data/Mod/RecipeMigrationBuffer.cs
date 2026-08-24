using Mafi;
using Mafi.Core.Factory.Recipes;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using System;
using System.Collections.Generic;

namespace CustomAssets.Data.Mod;

/// <summary>
/// Backs the Python <c>migrate_recipe(old, new, since)</c> call.
///
/// Declarations are BUFFERED while a pack's .py files execute and flushed to
/// <see cref="ProtoRegistrator.RegisterRecipeMigration(RecipeProto.ID, RecipeProto.ID, int)"/>
/// once the whole pack has loaded. Buffering matters because the game refuses a
/// migration whose source id is still present in ProtosDb, and at the moment a
/// migrate_recipe() line runs we cannot yet know whether a later file in the same
/// pack re-registers that id. Flushing at the end gives us the pack's final state.
/// </summary>
public static class RecipeMigrationBuffer {

	// COI gates a migration on `saveVersion < sinceVersion`, where saveVersion is the
	// GAME's global save counter (326 in 0.8.6). A pack's own semver has no relation
	// to it, so we map the pack version into a synthetic band far above any save
	// version the game will plausibly reach. Two consequences, both intended:
	//
	//   * Every migration we register ALWAYS applies, whatever the save's age. That is
	//     the correct semantic for a mod: once the old recipe id is gone from ProtosDb
	//     no newly written save can contain it, so a save that still does needs
	//     remapping regardless of when it was made. The game needs the version gate
	//     because it reuses the counter for unrelated save-format work; we do not.
	//
	//   * The encoded pack version still ORDERS entries, which is what the game
	//     actually needs it for: BuildAndStoreRecipeMigrations rejects a chain
	//     A->B->C unless the two hops sit in different version groups. Declaring the
	//     A->B hop in pack 0.4.0 and B->C in 0.5.0 satisfies that automatically.
	private const int SYNTHETIC_VERSION_BASE = 1_000_000_000;

	// Pending declarations per mod id, in declaration order.
	private static readonly Dictionary<string, List<PendingMigration>> s_pending =
		new Dictionary<string, List<PendingMigration>>();

	/// <summary>Drops any buffered declarations for a pack that is about to reload.</summary>
	public static void Reset(string modId) {
		s_pending[modId] = new List<PendingMigration>();
	}

	/// <summary>
	/// Discards a pack's buffered declarations because the pack failed to load, and says
	/// so in the log. Nothing is registered: the pack's recipes are absent too, so every
	/// entry would be skipped as "target not registered" anyway.
	///
	/// This is worth a warning rather than a silent drop. A failed pack does not stop the
	/// game, so the player continues with the pack's recipes unresolved — and the moment
	/// they save and reload, Machine.initSelf drops those recipes from every machine that
	/// had one assigned. The migrations were the thing that would have rescued them.
	/// </summary>
	public static void Abandon(string modId) {
		if (!s_pending.TryGetValue(modId, out List<PendingMigration> list) || list.Count == 0) {
			s_pending.Remove(modId);
			return;
		}
		s_pending.Remove(modId);

		Log.Warning(
			$"migrate_recipe[{modId}]: {list.Count} recipe migration(s) NOT applied because the pack " +
			"failed to load. Machines in existing saves still referencing the old recipe ids will lose " +
			"them if you save and reload. Fix the pack's load error before continuing this save.");
		foreach (PendingMigration m in list) {
			Log.Warning($"  • not applied: '{m.OldId}' -> '{m.NewId}'");
		}
	}

	/// <summary>Packs a pack semver into the synthetic save-version band. See the note above.</summary>
	public static int EncodePackVersion(int major, int minor, int patch) {
		return SYNTHETIC_VERSION_BASE + (major * 1_000_000) + (minor * 1_000) + patch;
	}

	/// <summary>
	/// Records one migrate_recipe() call. <paramref name="sinceLiteral"/> is the pack
	/// version the rename shipped in ("0.4.0"); null means "this pack's current
	/// manifest version", which is the common case.
	/// </summary>
	public static void Declare(
			string modId,
			string oldRecipeId,
			string newRecipeId,
			string sinceLiteral,
			ProtoRegistrator registrator) {
		if (string.IsNullOrWhiteSpace(oldRecipeId)) {
			throw new ArgumentException("migrate_recipe: 'old' must be a non-empty recipe id.");
		}
		if (string.IsNullOrWhiteSpace(newRecipeId)) {
			throw new ArgumentException("migrate_recipe: 'new' must be a non-empty recipe id.");
		}
		if (string.Equals(oldRecipeId, newRecipeId, StringComparison.Ordinal)) {
			throw new ArgumentException(
				$"migrate_recipe[{oldRecipeId}]: 'old' and 'new' are the same id — nothing to migrate.");
		}

		int since = resolveSince(oldRecipeId, sinceLiteral, registrator);
		if (!s_pending.TryGetValue(modId, out List<PendingMigration> list)) {
			list = new List<PendingMigration>();
			s_pending[modId] = list;
		}
		list.Add(new PendingMigration(oldRecipeId, newRecipeId, since, sinceLiteral));
	}

	/// <summary>
	/// Hands every buffered declaration for the pack to the game, skipping the ones the
	/// game would hard-throw on. A skip is a warning, never an exception: the pack's
	/// migrate_recipe() line stays in the .py file untouched, so the record of the
	/// removal survives and the modder can act on the warning at their own pace.
	///
	/// Chains are RESOLVED HERE rather than pushed onto the author. Declaration order
	/// and `since` values therefore do not matter: A->B and B->C may appear in either
	/// order, in either file, at the same pack version. See flattenChains.
	/// </summary>
	public static void Flush(string modId, ProtoRegistrator registrator) {
		if (!s_pending.TryGetValue(modId, out List<PendingMigration> list)) {
			return;
		}
		s_pending.Remove(modId);

		// One group for the whole pack. Safe only because flattenChains has already
		// removed every A->B->C hop, which is the sole thing the game's same-group
		// chain check rejects.
		int groupVersion = 0;
		foreach (PendingMigration p in list) {
			if (p.SinceVersion > groupVersion) {
				groupVersion = p.SinceVersion;
			}
		}

		foreach (PendingMigration m in flattenChains(modId, list)) {
			RecipeProto.ID oldId = new RecipeProto.ID(m.OldId);
			RecipeProto.ID newId = new RecipeProto.ID(m.NewId);

			// The game throws "migration source is still registered in the DB" at
			// startup for this. Skipping keeps the player's game loadable.
			if (registrator.PrototypesDb.TryGetProto<Proto>((Proto.ID)oldId, out _)) {
				Log.Warning(
					$"migrate_recipe[{m.OldId}]: skipped — a recipe with the OLD id is still " +
					$"registered by '{modId}'. A migration replaces the old recipe, so remove its " +
					"build_recipe(...) definition (keep this migrate_recipe line) or drop the migration.");
				continue;
			}
			if (!registrator.PrototypesDb.TryGetProto<RecipeProto>((Proto.ID)newId, out _)) {
				Log.Warning(
					$"migrate_recipe[{m.OldId}]: skipped — target recipe '{m.NewId}' is not registered. " +
					"Declare the migration after the replacement recipe is built, and check the id spelling.");
				continue;
			}

			try {
				registrator.RegisterRecipeMigration(oldId, newId, groupVersion);
				Log.Info($"migrate_recipe[{modId}]: '{m.OldId}' -> '{m.NewId}'");
			} catch (Exception ex) {
				Log.Warning($"migrate_recipe[{m.OldId}]: rejected by the game — {ex.Message}");
			}
		}
	}

	// Rewrites every declaration to point at the END of its chain, so A->B->C is
	// registered as A->C and B->C. This is exactly what the game's own
	// BuildFlatRecipeMigrations does at load time, hoisted forward so that:
	//
	//   * the author never has to discover that a re-rename needs a different `since`,
	//     and never gets bitten by two hops landing in the same version group;
	//   * declarations can appear in any order, in any file of the pack — B->C may be
	//     written before A->B;
	//   * `since` stops being load-bearing and is just documentation of when the rename
	//     shipped, which is what a pack version can honestly express.
	//
	// Conflicts and cycles are dropped with a warning rather than thrown, matching the
	// rest of Flush: a bad line never costs the player their game.
	private static List<PendingMigration> flattenChains(string modId, List<PendingMigration> list) {
		Dictionary<string, PendingMigration> byOld = new Dictionary<string, PendingMigration>(StringComparer.Ordinal);
		foreach (PendingMigration m in list) {
			if (byOld.TryGetValue(m.OldId, out PendingMigration existing)) {
				if (!string.Equals(existing.NewId, m.NewId, StringComparison.Ordinal)) {
					Log.Warning(
						$"migrate_recipe[{m.OldId}]: declared twice with different targets " +
						$"('{existing.NewId}' and '{m.NewId}') in '{modId}'. Keeping the first; " +
						"one old id can only migrate to one recipe.");
				}
				continue;
			}
			byOld[m.OldId] = m;
		}

		List<PendingMigration> result = new List<PendingMigration>();
		foreach (PendingMigration m in byOld.Values) {
			HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal) { m.OldId };
			string target = m.NewId;
			bool cyclic = false;
			while (byOld.TryGetValue(target, out PendingMigration next)) {
				if (!seen.Add(target)) {
					Log.Warning(
						$"migrate_recipe[{m.OldId}]: skipped — the migrations in '{modId}' form a cycle " +
						$"(revisited '{target}'). A recipe cannot migrate back into itself.");
					cyclic = true;
					break;
				}
				target = next.NewId;
			}
			if (cyclic) {
				continue;
			}
			result.Add(new PendingMigration(m.OldId, target, m.SinceVersion, m.SinceLabel));
		}
		return result;
	}

	// "0.4.0" / "0.4" / "1.2.3" -> synthetic version. Null or blank falls back to the
	// pack's own manifest version, which is what a modder means the vast majority of
	// the time: the rename ships in whatever they are releasing right now.
	private static int resolveSince(string oldRecipeId, string sinceLiteral, ProtoRegistrator registrator) {
		if (string.IsNullOrWhiteSpace(sinceLiteral)) {
			VersionSlim own = registrator.ActiveMod?.Manifest?.Version ?? default(VersionSlim);
			if (own.IsEmpty) {
				throw new ArgumentException(
					$"migrate_recipe[{oldRecipeId}]: the pack manifest has no version, so 'since' " +
					"cannot be defaulted. Pass it explicitly, e.g. since = \"0.4.0\".");
			}
			return EncodePackVersion(own.Major, own.Minor, own.Patch);
		}

		string[] parts = sinceLiteral.Trim().Split('.');
		if (parts.Length < 2 || parts.Length > 3) {
			throw new ArgumentException(
				$"migrate_recipe[{oldRecipeId}]: 'since' must be a pack version like \"0.4.0\", got \"{sinceLiteral}\".");
		}

		int[] nums = new int[3];
		for (int i = 0; i < parts.Length; i++) {
			if (!int.TryParse(parts[i], out nums[i]) || nums[i] < 0 || nums[i] > 999) {
				throw new ArgumentException(
					$"migrate_recipe[{oldRecipeId}]: 'since' component \"{parts[i]}\" is not a number in 0..999 " +
					$"(from \"{sinceLiteral}\").");
			}
		}
		return EncodePackVersion(nums[0], nums[1], nums[2]);
	}

	private readonly struct PendingMigration {

		public readonly string OldId;
		public readonly string NewId;
		public readonly int SinceVersion;
		public readonly string SinceLabel;

		public PendingMigration(string oldId, string newId, int sinceVersion, string sinceLabel) {
			OldId = oldId;
			NewId = newId;
			SinceVersion = sinceVersion;
			SinceLabel = string.IsNullOrWhiteSpace(sinceLabel) ? "manifest version" : sinceLabel;
		}
	}
}
