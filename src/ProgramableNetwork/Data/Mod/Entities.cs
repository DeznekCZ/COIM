using Mafi;
using Mafi.Base;
using Mafi.Base.Prototypes.Machines.ComputingEntities;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.Datacenters;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using ProgramableNetwork.Data.Speaker;
using System;
using System.Threading;

namespace ProgramableNetwork
{
    public partial class NewIds
    {
        public partial class Controllers
        {
            public static readonly Proto.ID Category = new Proto.ID("ProgramableNetwork_Category");
            public static readonly StaticEntityProto.ID Controller = new StaticEntityProto.ID("ProgramableNetwork_Computer");
            public static readonly StaticEntityProto.ID Antena = new StaticEntityProto.ID("ProgramableNetwork_Antena");
            public static readonly StaticEntityProto.ID AntenaT2 = new StaticEntityProto.ID("ProgramableNetwork_AntenaT2");
            public static readonly StaticEntityProto.ID Database = new StaticEntityProto.ID("ProgramableNetwork_Database");
            public static readonly StaticEntityProto.ID Speaker = new StaticEntityProto.ID("ProgramableNetwork_Speaker");
            public static StaticEntityProto.ID ControllerTemplate(string id) => new StaticEntityProto.ID($"ProgramableNetwork_Computer_{id}");
        }
    }

    public partial class NewAssets
    {
        public partial class Computers
        {
            public partial class Icons
            {
                public static readonly string Controller = "Assets/ProgramableNetwork/Computer/Icon.png";
                public static readonly string Antena = "Assets/ProgramableNetwork/Antena/Icon.png";
                public static readonly string Speaker = "Assets/ProgramableNetwork/Speaker/Icon.png";
                public static string ControllerTemplate(string id) => $"Assets/ProgramableNetwork/Computer/Icon_{id}.png";
            }

            public static readonly string Controller = "Assets/ProgramableNetwork/Computer/Computer.prefab";
            public static readonly string Antena = "Assets/ProgramableNetwork/Antena/Antena.prefab";
            public static readonly string AntenaT2 = "Assets/ProgramableNetwork/Antena/AntenaT2.prefab";
            public static readonly string Speaker = "Assets/ProgramableNetwork/Speaker/Speaker.prefab";
        }
    }

    internal class Entities : AValidatedData
    {


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
            var originalTier1 = registrator.PrototypesDb.Add(new ControllerProto(
                id: NewIds.Controllers.Controller,
                strings: Proto.CreateStr(NewIds.Controllers.Controller, "Controller", "Handles basic operations and automatization"),
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
            ControllerTemplate[] values = GetControllerTemplates(registrator);
            foreach (var (id, name, description, modules) /* Expand */ in values)
            {
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
                    initModules: modules
                ));
                template?.SetNextTierIndirect(next);
                template = next;
            }

            var antenaT1 = registrator.PrototypesDb.Add(new AntenaProto(
                id: NewIds.Controllers.Antena,
                strings: Proto.CreateStr(NewIds.Controllers.Antena, "Antena", "Handles signal transfer for longer distance"),
                layout: registrator.LayoutParser.ParseLayoutOrThrow("[4]"),
                tier: 1,
                costs: ((EntityCostsTpl)Costs.Build.CP2(4).MaintenanceT1(2)).MapToEntityCosts(registrator),
                graphics: new LayoutEntityProto.Gfx(
                    prefabPath: NewAssets.Computers.Antena,
                    customIconPath: NewAssets.Computers.Icons.Antena,
                    categories: registrator.GetCategoriesProtos(NewIds.Controllers.Category)
                )
            ));

            var antenaT2 = registrator.PrototypesDb.Add(new AntenaProto(
                id: NewIds.Controllers.AntenaT2,
                strings: Proto.CreateStr(NewIds.Controllers.AntenaT2, "Antena II", "Handles signal transfer for longer distance (100% bonus to range)"),
                layout: registrator.LayoutParser.ParseLayoutOrThrow("[9][9]", "[9][9]"),
                tier: 2,
                costs: ((EntityCostsTpl)Costs.Build.CP3(8).MaintenanceT2(2)).MapToEntityCosts(registrator),
                graphics: new LayoutEntityProto.Gfx(
                    prefabPath: NewAssets.Computers.AntenaT2,
                    customIconPath: NewAssets.Computers.Icons.Antena,
                    categories: registrator.GetCategoriesProtos(NewIds.Controllers.Category)
                ),
                distanceBoost: Fix32.Two
            ));

            antenaT1.SetNextTierIndirect(antenaT2);

            registrator.PrototypesDb.Add(new SpeakerProto(
                id: NewIds.Controllers.Speaker,
                strings: Proto.CreateStr(NewIds.Controllers.Speaker, "Speaker", "Play a sound when on, the sound may be triggered Controller, or may work manually"),
                layout: registrator.LayoutParser.ParseLayoutOrThrow("[3]"),
                costs: ((EntityCostsTpl)Costs.Build.CP2(4).MaintenanceT1(2)).MapToEntityCosts(registrator),
                graphics: new LayoutEntityProto.Gfx(
                    prefabPath: NewAssets.Computers.Speaker,
                    customIconPath: NewAssets.Computers.Icons.Speaker,
                    categories: registrator.GetCategoriesProtos(NewIds.Controllers.Category)
                )
            ));
        }

        private ControllerTemplate[] GetControllerTemplates(ProtoRegistrator registrator)
        {
            return new ControllerTemplate[]
            {
                new ControllerTemplate(
                    "FullStorage",
                    "Storage overflow",
                    "Reads storage and disables selected buildings connected by switch of modules (by default there is only one switch off)",
                    (controller) =>
                    {
                        int i = 0;
                        Module storage = AddToController(registrator, controller, 0, ref i, "Connection_Storage");
                        Thread.Sleep(1);

                        Module lt = AddToController(registrator, controller, 0, ref i, "Compare_Int_Greater");
                        Thread.Sleep(1);

                        Module switchOff = AddToController(registrator, controller, 0, ref i, "Connection_SwitchOff");
                        Thread.Sleep(1);

                        Connect(lt, "a", storage, "fullness");
                        Connect(switchOff, "pause", lt, "c");

                        return () =>
                        {
                            lt.Field.Bool["field_b"] = true;
                            lt.Field.Integer["b"] = 99;
                        };
                    }
                ),
                new ControllerTemplate(
                    "VehicleImport",
                    "Vehicle import",
                    "Reads storage and assing vehicle when amound of stored resources is bellow 50%",
                    (controller) =>
                    {
                        int i = 0;

                        Module storage = AddToController(registrator, controller, 0, ref i, "Connection_Storage");
                        Thread.Sleep(1);

                        Module lt = AddToController(registrator, controller, 0, ref i, "Compare_Int_Lower");
                        Thread.Sleep(1);

                        Module vehicle = AddToController(registrator, controller, 0, ref i, "Constant_Vehicle");
                        Thread.Sleep(1);

                        Module vehicleSet = AddToController(registrator, controller, 0, ref i, "Connection_Vehicle_Set");
                        Thread.Sleep(1);

                        Connect(lt, "a", storage, "fullness");
                        Connect(vehicleSet, "count", lt, "c");
                        Connect(vehicleSet, "vehicle", vehicle, "value");

                        return () =>
                        {
                            lt.Field.Bool["field_b"] = true;
                            lt.Field.Integer["b"] = 50;
                        };
                    }
                )
            };
        }

        private static void Connect(Module inputModule, string input, Module outputModule, string output)
        {
            inputModule.InputModules[input] = new ModuleConnector(outputModule.Id, output);
        }

        private static Module AddToController(ProtoRegistrator registrator, Controller controller, int row, ref int column, string moduleProto)
        {
            ModuleProto storageProto = registrator.PrototypesDb.Get<ModuleProto>(new Proto.ID(moduleProto.ModuleId())).Value;
            Module module = new Module(storageProto, controller.Context, controller);

            controller.Modules.Add(module);
            controller.Rows[row][column++] = ModulePlacement.Origin(module.Id);
            int width = module.Layout.GetWidth(module);
            for (int j = 1; j < width; j++)
                controller.Rows[row][column++] = ModulePlacement.Rest(module.Id);

            return module;
        }
    }
}