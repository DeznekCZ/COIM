using Mafi.Core.Mods;
using Mafi.Core.Prototypes;

namespace CustomAssets
{
	// Abstract IMod base for runtime-emitted per-pack marker types.
	//
	// CustomAssetPack.RegisterDependencies emits a sealed subclass of this
	// class with Type.Name == Manifest.Id, instantiates it with the pack's
	// ModManifest, and registers the instance with the DependencyResolverBuilder
	// via AsSelf() so the runtime Type (the emitted one) is the registration key.
	//
	// The Manifest property is inherited from DataOnlyMod and is set by the
	// base constructor, satisfying "the registered marker must include the
	// manifest inside". No prototypes are registered by the marker — the real
	// pack-entry CustomAssetPack handles registration via CustomAssetRegistrator.
	public abstract class EmittedPackMod : DataOnlyMod {

		protected EmittedPackMod(ModManifest manifest, ModJsonConfig jsonConfig) : base(manifest) {
			// ReSharper disable VirtualMemberCallInConstructor
			JsonConfig.MergeSavedValues(jsonConfig.GetSavedValues());
		}

		public override void RegisterPrototypes(ProtoRegistrator registrator) { }
	}
}
