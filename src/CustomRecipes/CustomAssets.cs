using Mafi;
using Mafi.Core.Mods;
using CustomAssets.Data.Mod;

namespace CustomAssets
{
    public sealed class CustomAssets : DataOnlyMod {

		public override string Name => "Custom Assets";
		public override int Version => 015;

		// Mod constructor that lists mod dependencies as parameters.
		// This guarantee that all listed mods will be loaded before this mod.
		// It is a good idea to depend on both `Mafi.Core.CoreMod` and `Mafi.Base.BaseMod`.
		public CustomAssets() : base() {
            // You can use Log class for logging. These will be written to the log file
            // and can be also displayed in the in-game console with command `also_log_to_console`.
            Log.Info($"{Name}: constructed");
        }


        public override void RegisterPrototypes(ProtoRegistrator registrator) {
            Log.Info($"{Name}: registering prototypes");

            // Register all prototypes here.

            // Registers all products from this assembly. See ExampleModIds.Products.cs for examples.
            //registrator.RegisterAllProducts();
            //registrator.RegisterData<Terrain>();

            // Use data class registration to register other protos such as machines, recipes, etc.
            new AssetRegistrator().RegisterData(registrator);
            //registrator.RegisterData<AssetRegistrator>();

            // Registers all research from this assembly. See ExampleResearchData.cs for examples.
            //registrator.RegisterDataWithInterface<IResearchNodesData>();
        }
    }
}