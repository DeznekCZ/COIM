using Mafi;
using Mafi.Base;
using Mafi.Core;
using Mafi.Core.Mods;
using ProgramableNetwork.Data.Mod;
using ProgramableNetwork.Data.Modules;
using System;
using System.IO;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using ProgramableNetwork.Data.DisplayEntity;
using UnityEngine;

namespace ProgramableNetwork
{
    public sealed class ModDefinition : DataOnlyMod {

        // Mod constructor that lists mod dependencies as parameters.
        // This guarantee that all listed mods will be loaded before this mod.
        // It is a good idea to depend on both `Mafi.Core.CoreMod` and `Mafi.Base.BaseMod`.
        public ModDefinition(ModManifest manifest) : base(manifest) {
            // Load translations as early as possible: must run before any Loc.Str / Proto.CreateStr
            // call from this assembly (including ones triggered by static cctors during prototype
            // registration). Otherwise those LocStr instances may snapshot the English fallback.
            ModTranslations.Load(manifest);

            // You can use Log class for logging. These will be written to the log file
            // and can be also displayed in the in-game console with command `also_log_to_console`.
            Log.Info($"{nameof(ProgramableNetwork)}: constructed");
        }


        public override void RegisterPrototypes(ProtoRegistrator registrator) {
            Log.Info($"{nameof(ProgramableNetwork)}: registering prototypes");
            CustomAssetManager.Clear();

			registrator.PrototypesDb.RegisterPhantom(ModuleProto.Phantom);

            // Register all prototypes here.

            // Registers all products from this assembly. See ExampleModIds.Products.cs for examples.
            registrator.RegisterAllProducts();
            //registrator.RegisterData<Terrain>();

            // Use data class registration to register other protos such as machines, recipes, etc.
            registrator.RegisterDataWithInterface<IModuleGroup>();
            registrator.RegisterData<PyModules>();
            // DataBands and Entities take JsonConfig directly via constructor — instantiated
            // explicitly so the ProtoRegistrator's parameterless `RegisterData<T>()` path
            // (which calls `new T()`) doesn't have to thread the mod's config through a
            // static accessor.  Other data classes that don't need config keep the simpler
            // generic registration form.
            registrator.RegisterData(new DataBands(JsonConfig));
            registrator.RegisterData<ControllerTemplates>();
            registrator.RegisterData(new Entities(JsonConfig));
            registrator.RegisterData<Displays>();
            registrator.RegisterData<ControllerNotification>();
            registrator.RegisterData<ModuleIdsGenerator>();

            // Registers all research from this assembly. See ExampleResearchData.cs for examples.
            registrator.RegisterDataWithInterface<IResearchNodesData>();

            // Starter goal: place a controller once the Programable Network research is unlocked.
            // Must run after Entities (controller proto) and Research (research node) registered above.
            registrator.RegisterData<GoalsData>();

            // To dump every mod-registered en-US string to <modRoot>/Translations/en.json, run the
            // `pn_exportTranslations` console command (see ModConsoleCommands).
        }
    }

}