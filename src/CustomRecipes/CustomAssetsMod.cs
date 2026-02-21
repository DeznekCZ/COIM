using Mafi;
using Mafi.Core.Mods;
using CustomAssets.Data.Mod;

namespace CustomAssets
{
	public sealed class CustomAssetsMod : DataOnlyMod {

		public CustomAssetsMod(ModManifest manifest) : base(manifest) {
			Log.Info($"{manifest.Id}: constructed");
		}

		public override void RegisterPrototypes(ProtoRegistrator registrator) {
			Log.Info($"{Manifest.Id}: registering prototypes");
			CustomAssetManager.Clear();
		}
	}
}