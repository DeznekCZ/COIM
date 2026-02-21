using CustomAssets;
using CustomAssets.Data.Mod;
using Mafi.Core.Mods;

namespace CustomAssetPack;

public class CustomAssetPack : DataOnlyMod {

	public CustomAssetPack(ModManifest manifest) : base(manifest) { }

	public override void RegisterPrototypes(ProtoRegistrator registrator) {
		
		new CustomAssetRegistrator().RegisterData(registrator);
	}
}
