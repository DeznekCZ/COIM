using Mafi;
using Mafi.Core.Mods;
using CustomAssets.Data.Mod;

namespace CustomAssets
{
	public class CustomAssetsPack<T> : DataOnlyMod {

		public CustomAssetsPack(ModManifest manifest) : base(manifest) {
			Log.Info($"{manifest.Id}: constructed");
		}

		public override void RegisterPrototypes(ProtoRegistrator registrator) {
			Log.Info($"{Manifest.Id}: registering prototypes");
			new CustomAssetRegistrator().RegisterData(registrator);

		}
	}
}
