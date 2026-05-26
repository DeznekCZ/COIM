using Mafi.Core.Mods;

namespace CustomAssets
{
	// Minimal sealed concrete subclass of CustomAssetPackBase. All logic is in
	// the base class (which lives in the core CustomAssets.dll). This file is
	// the entire content of the per-pack CustomAssetPack.dll — its only job is
	// to provide a CLR Type that Mafi can find via primary_mod_class_name and
	// instantiate with the pack's ModManifest.
	//
	// Each pack folder ships its own copy of CustomAssetPack.dll loaded under
	// non_locking_dll_load → each pack gets a distinct CLR Assembly → each
	// pack's CustomAssetPack is a distinct System.Type. CustomAssetPackBase,
	// EmittedPackMod and all the runtime helpers resolve to the single base
	// CustomAssets.dll already loaded via the pack's mod_dependencies.
	public sealed class CustomAssetPack : CustomAssetPackBase {
		public CustomAssetPack(ModManifest manifest) : base(manifest) { }
	}
}
