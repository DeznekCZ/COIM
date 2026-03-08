using Mafi;
using Mafi.Base;
using Mafi.Core;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using ProgramableNetwork.Python;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProgramableNetwork.Data.Mod
{
    public class ControllerTemplates : AValidatedData
    {
        private static List<Class> UserTemplates = [];

        public static void Clear()
        {
            UserTemplates.Clear();
        }

        protected override void RegisterDataInternal(ProtoRegistrator registrator)
        {
            var pillars = new EntityLayoutParams(
                customPlacementRange: new ThicknessIRange(0, TransportPillarProto.MAX_PILLAR_HEIGHT.Value - 1),
                customTokens: new CustomLayoutToken[]
                {
                    new CustomLayoutToken("|0|", (param, height) =>
                    {
                        return new LayoutTokenSpec(
                            constraint: LayoutTileConstraint.UsingPillar,
                            heightFrom: 0,
                            heightToExcl: height,
                            maxTerrainHeight: 0,
                            minTerrainHeight: 0
                        );
                    })
                }
            );

            ToolbarCategoryProto transportToolbarCategoryProto = registrator.PrototypesDb.Get<ToolbarCategoryProto>(Ids.ToolbarCategories.Transports).ValueOrThrow("Missing game category");
            var category = registrator.PrototypesDb.Add(new ToolbarCategoryProto(
                id: NewIds.Controllers.Category,
                strings: Proto.CreateStr(NewIds.Controllers.Category, "Network", "Contains buildings for for work with network (computation, controller)"),
                order: transportToolbarCategoryProto.Order + 1,
                iconPath: Mafi.Unity.Assets.Unity.UserInterface.General.Connect128_png,
                isTransportBuildAllowed: true,
                shortcutId: "NETWORK"
            ));

            // New entities
            ControllerProto originalTier1 = registrator.PrototypesDb.Add(new ControllerProto(
                id: NewIds.Controllers.Controller,
                strings: Proto.CreateStr(NewIds.Controllers.Controller, "Controller",
					@"Handles basic operations and automatization

The controller can use maintenance from T1 to T3 base on layout of the modules:
 - T1 is used when no modules requiring teraflops are needed
 - T2 is used when at least 1 teraflop is used
 - T3 is used when more than 10 teraflops are used"),
                layout: registrator.LayoutParser.ParseLayoutOrThrow(pillars, "[1]"),
                costs: ((EntityCostsTpl)Mafi.Base.Costs.Build.CP2(4)).MapToEntityCosts(registrator),
                allowedModules: (module) => module.AllowedDevices.Contains(NewIds.Controllers.Controller),
                graphics: new LayoutEntityProto.Gfx(
                    prefabPath: NewAssets.Computers.Controller,
                    customIconPath: NewAssets.Computers.Icons.Controller,
                    categories: registrator.GetCategoriesProtos(NewIds.Controllers.Category)
                )
            ));

            ControllerProto.RegisterPhantom(registrator);

            ControllerProto template = null;
            IEnumerable<ControllerTemplate> values = GetControllerTemplates(registrator, originalTier1);
            foreach (var (id, name, description, color, modules) /* Expand */ in values)
            {
                TryLoadTexture(NewAssets.Computers.Icons.ControllerTemplate(id));

                var protoId = NewIds.Controllers.ControllerTemplate(id);
                var next = registrator.PrototypesDb.Add(new ControllerProto(
                    id: protoId,
                    strings: Proto.CreateStr(protoId, name, description),
                    basedOn: originalTier1,
                    graphics: new LayoutEntityProto.Gfx(
                        prefabPath: NewAssets.Computers.Controller,
                        customIconPath: NewAssets.Computers.Icons.ControllerTemplate(id),
                        categories: registrator.GetCategoriesProtos(NewIds.Controllers.Category)
                    ),
                    defaultColor: color,
                    initModules: modules
                ));
                template?.SetNextTierIndirect(next);
                template = next;

                Log.Info($"Controller template: {protoId} created");
            }
        }

        private void TryLoadTexture(string assetPath)
        {
            Texture2D texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            string basePath = ModDefinition.StaticManifest.RootDirectoryPath;
            byte[] image = File.ReadAllBytes(Path.Combine(basePath, assetPath));
            if (!texture2D.LoadImage(image)) {
                Log.Exception(new ArgumentException($"Could not load an image: {assetPath}"));
            } else {
                CustomAssetManager.Alternations.Add(assetPath, texture2D);
            }
        }

        public static IEnumerable<ControllerTemplate> GetControllerTemplates(ProtoRegistrator registrator, ControllerProto basedOn)
        {
            yield return
                new ControllerTemplate(
                    "FullStorage",
                    "Storage overflow",
                    "Reads storage and disables selected buildings connected by switch of modules (by default there is only one switch off)",
                    ColorRgba.LightGray,
                    (controller) =>
                    {
                        int i = 0;

                        Module storage = AddToController(registrator, controller, 0, ref i, "Connection_Storage");
                        Module lt = AddToController(registrator, controller, 0, ref i, "Compare_Int_Greater");
                        Module switchOff = AddToController(registrator, controller, 0, ref i, "Connection_SwitchOff");

                        lt["a"] = storage["fullness"];
                        switchOff["pause"] = lt["c"];

                        return () =>
                        {
                            lt.Field.Bool["field_b"] = true;
                            lt.Field.Integer["b"] = 99;
                        };
                    }
                );
            yield return
                new ControllerTemplate(
                    "VehicleImport",
                    "Vehicle import",
                    "Reads storage and assing vehicle when amound of stored resources is bellow 50%",
                    ColorRgba.Orange,
                    (controller) =>
                    {
                        int i = 0;

                        Module storage = AddToController(registrator, controller, 0, ref i, "Connection_Storage");
                        Module lt = AddToController(registrator, controller, 0, ref i, "Compare_Int_Lower");
                        Module vehicle = AddToController(registrator, controller, 0, ref i, "Constant_Vehicle");
                        Module vehicleSet = AddToController(registrator, controller, 0, ref i, "Connection_Vehicle_Set");

                        lt["a"] = storage["fullness"];
                        vehicleSet["count"] = lt["c"];
                        vehicleSet["vehicle"] = vehicle["value"];

                        return () =>
                        {
                            lt.Field.Bool["field_b"] = true;
                            lt.Field.Integer["b"] = 50;
                        };
                    }
                );

            Log.Info($"User defined controller templates: {UserTemplates.Count}");
            foreach (var item in UserTemplates)
            {
                string id = item.name;
                string name = item.classContext.TryGetValue("name", out object oname)
                    ? (string)oname : "Template controller";
                string description = item.classContext.TryGetValue("description", out object odescritption)
                    ? (string)odescritption : "";
                ColorRgba color = item.classContext.TryGetValue("color", out object ocolor)
                    ? (ColorRgba)ocolor : basedOn.DefaultColor;

                Log.Info($"User controller template: {id} found");
                yield return new ControllerTemplate(
                    id,
                    name,
                    description,
                    color,
                    new ControllerWrapper(registrator, item).Generate
                );
            }
        }

        private static Module AddToController(ProtoRegistrator registrator, Controller controller, int row, ref int column, string moduleProto)
        {
            ModuleProto storageProto = registrator.PrototypesDb.Get<ModuleProto>(new Proto.ID(moduleProto.ModuleId())).Value;
            Module module = new Module(storageProto, controller.Context, controller);

            controller.Modules.Add(module);
            controller.Rows[row][column++] = ModulePlacement.Origin(module.Id);
            int width = module.Layout.GetWidth(module);
            for (int j = 1; j < width; j++) {
                controller.Rows[row][column++] = ModulePlacement.Rest(module.Id);
            }

            Thread.Sleep(1); // increment module id, because is based on time
            return module;
        }

        public static void AddControllers(List<Class> allControllers)
        {
            UserTemplates.AddRange(allControllers);
        }
    }
}
