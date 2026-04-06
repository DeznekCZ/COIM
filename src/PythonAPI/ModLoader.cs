using Mafi.Core.Mods;

namespace PythonAPI;

public class ModLoader(ModManifest manifest) : DataOnlyMod(manifest) {

	public override void RegisterPrototypes(ProtoRegistrator registrator) { /* No data to register */ }
}
