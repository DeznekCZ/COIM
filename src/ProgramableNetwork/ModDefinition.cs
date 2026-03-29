using Mafi;
using Mafi.Base;
using Mafi.Core;
using Mafi.Core.Mods;
using ProgramableNetwork.Data.Mod;
using ProgramableNetwork.Data.Modules;
using System;
using System.IO;
using ProgramableNetwork.Data.DisplayEntity;
using UnityEngine;

namespace ProgramableNetwork
{
    public sealed class ModDefinition : DataOnlyMod {

        // Mod constructor that lists mod dependencies as parameters.
        // This guarantee that all listed mods will be loaded before this mod.
        // It is a good idea to depend on both `Mafi.Core.CoreMod` and `Mafi.Base.BaseMod`.
        public ModDefinition(ModManifest manifest) : base(manifest) {
            // You can use Log class for logging. These will be written to the log file
            // and can be also displayed in the in-game console with command `also_log_to_console`.
            Log.Info($"{nameof(ProgramableNetwork)}: constructed");
        }


        public override void RegisterPrototypes(ProtoRegistrator registrator) {
            Log.Info($"{nameof(ProgramableNetwork)}: registering prototypes");
            CustomAssetManager.Clear();
            
			registrator.PrototypesDb.RegisterPhantom(ModuleProto.Phantom);

#if DEBUG
            // Register all prototypes here.

            // Registers all products from this assembly. See ExampleModIds.Products.cs for examples.
            registrator.RegisterAllProducts();
            //registrator.RegisterData<Terrain>();

            // Use data class registration to register other protos such as machines, recipes, etc.
            registrator.RegisterDataWithInterface<IModuleGroup>();
            registrator.RegisterData<PyModules>();
            registrator.RegisterData<DataBands>();
            registrator.RegisterData<ControllerTemplates>();
            registrator.RegisterData<Entities>();
            registrator.RegisterData<Displays>();
            registrator.RegisterData<ControllerNotification>();
            registrator.RegisterData<ModuleIdsGenerator>();

            // Registers all research from this assembly. See ExampleResearchData.cs for examples.
            registrator.RegisterDataWithInterface<IResearchNodesData>();
#elif RELEASE
            // Sanitizer code
			ControllerProto.RegisterPhantom(registrator);
			DisplayEntityProto.RegisterPhantom(registrator);
#endif
        }
    }

}