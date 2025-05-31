using Mafi;
using Mafi.Base;
using Mafi.Base.Prototypes.Machines.ComputingEntities;
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
                iconPath: Mafi.Unity.Assets.Unity.UserInterface.EntityIcons.Computing_png,
                isTransportBuildAllowed: true,
                shortcutId: "NETWORK"
                )).SomeOption().ToImmutableArray();

            // Adapting existing
            registrator.PrototypesDb.Get<DataCenterProto>(Ids.DataCenters.DataCenter)
                .ValueOrNull?.Graphics.SetCategories(category);
            registrator.PrototypesDb.Get<MainframeProto>(Ids.DataCenters.Mainframe)
                .ValueOrNull?.Graphics.SetCategories(category);

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
                    categories: category
                )
            ));

            ControllerProto.RegisterPhantom(registrator);

            ControllerProto template = null;
            foreach (var (id, name, description, modules) /* Expand */
                in new (string id, string name, string description, Func<Controller, Action> modules)[]
                {
                    (
                        "FullStorage",
                        "Storage overflow",
                        "Reads storage and disables selected buildings connected by switch of modules (by default there is only one switch off)",
                        (controller) =>
                        {
                            int i = 0;

                            ModuleProto storageProto = registrator.PrototypesDb.Get<ModuleProto>(new Proto.ID("Connection_Storage".ModuleId())).Value;
                            Module storage = new Module(storageProto, controller.Context, controller);
                            Thread.Sleep(1);
                            controller.Modules.Add(storage);
                            controller.Rows[0][i++] = ModulePlacement.Origin(storage.Id);
                            controller.Rows[0][i++] = ModulePlacement.Rest(storage.Id);
                            controller.Rows[0][i++] = ModulePlacement.Rest(storage.Id);
                            controller.Rows[0][i++] = ModulePlacement.Rest(storage.Id);

                            ModuleProto ltProto = registrator.PrototypesDb.Get<ModuleProto>(new Proto.ID("Compare_Int_Greater".ModuleId())).Value;
                            Module lt = new Module(ltProto, controller.Context, controller);
                            Thread.Sleep(1);
                            controller.Modules.Add(lt);
                            controller.Rows[0][i++] = ModulePlacement.Origin(lt.Id);
                            controller.Rows[0][i++] = ModulePlacement.Rest(lt.Id);

                            lt.InputModules["a"] = new ModuleConnector(storage.Id, "fullness");

                            ModuleProto switchOffProto = registrator.PrototypesDb.Get<ModuleProto>(new Proto.ID("Connection_SwitchOff".ModuleId())).Value;
                            Module switchOff = new Module(switchOffProto, controller.Context, controller);
                            Thread.Sleep(1);
                            controller.Modules.Add(switchOff);
                            controller.Rows[0][i++] = ModulePlacement.Origin(switchOff.Id);

                            switchOff.InputModules["pause"] = new ModuleConnector(lt.Id, "c");

                            return () =>
                            {
                                lt.Field.Bool["field_b"] = true;
                                lt.Field.Integer["b"] = 99;
                            };
                        }
                    ),
                    (
                        "VehicleImport",
                        "Vehicle import",
                        "Reads storage and assing vehicle when amound of stored resources is bellow 50%",
                        (controller) =>
                        {
                            int i = 0;

                            ModuleProto storageProto = registrator.PrototypesDb.Get<ModuleProto>(new Proto.ID("Connection_Storage".ModuleId())).Value;
                            Module storage = new Module(storageProto, controller.Context, controller);
                            controller.Modules.Add(storage);
                            controller.Rows[0][i++] = ModulePlacement.Origin(storage.Id);
                            controller.Rows[0][i++] = ModulePlacement.Rest(storage.Id);
                            controller.Rows[0][i++] = ModulePlacement.Rest(storage.Id);
                            controller.Rows[0][i++] = ModulePlacement.Rest(storage.Id);

                            ModuleProto ltProto = registrator.PrototypesDb.Get<ModuleProto>(new Proto.ID("Compare_Int_Lower".ModuleId())).Value;
                            Module lt = new Module(ltProto, controller.Context, controller);
                            controller.Modules.Add(lt);
                            controller.Rows[0][i++] = ModulePlacement.Origin(lt.Id);
                            controller.Rows[0][i++] = ModulePlacement.Rest(lt.Id);

                            lt.InputModules["a"] = new ModuleConnector(storage.Id, "fullness");

                            ModuleProto vehicleProto = registrator.PrototypesDb.Get<ModuleProto>(new Proto.ID("Constant_Vehicle".ModuleId())).Value;
                            Module vehicle = new Module(vehicleProto, controller.Context, controller);
                            controller.Modules.Add(vehicle);
                            controller.Rows[0][i++] = ModulePlacement.Origin(vehicle.Id);

                            ModuleProto vehicleSetProto = registrator.PrototypesDb.Get<ModuleProto>(new Proto.ID("Connection_Vehicle_Set".ModuleId())).Value;
                            Module vehicleSet = new Module(vehicleSetProto, controller.Context, controller);
                            controller.Modules.Add(vehicleSet);
                            controller.Rows[0][i++] = ModulePlacement.Origin(vehicleSet.Id);
                            controller.Rows[0][i++] = ModulePlacement.Rest(vehicleSet.Id);
                            controller.Rows[0][i++] = ModulePlacement.Rest(vehicleSet.Id);
                            controller.Rows[0][i++] = ModulePlacement.Rest(vehicleSet.Id);

                            vehicleSet.InputModules["count"] = new ModuleConnector(lt.Id, "c");
                            vehicleSet.InputModules["vehicle"] = new ModuleConnector(vehicle.Id, "value");

                            return () =>
                            {
                                lt.Field.Bool["field_b"] = true;
                                lt.Field.Integer["b"] = 50;
                            };
                        }
                    )
                })
            {
                var protoId = NewIds.Controllers.ControllerTemplate(id);
                var next = registrator.PrototypesDb.Add(new ControllerProto(
                    id: protoId,
                    strings: Proto.CreateStr(protoId, name, description),
                    basedOn: originalTier1,
                    graphics: new LayoutEntityProto.Gfx(
                        prefabPath: NewAssets.Computers.Controller,
                        customIconPath: NewAssets.Computers.Icons.ControllerTemplate(id),
                        categories: category
                    ),
                    initModules: modules
                ));
                if (template != null)
                    template.SetNextTierIndirect(next);
                template = next;
            }

            var antenaT1 = registrator.PrototypesDb.Add(new AntenaProto(
                id: NewIds.Controllers.Antena,
                strings: Proto.CreateStr(NewIds.Controllers.Antena, "Antena", "Handles signal transfer for longer distance"),
                layout: registrator.LayoutParser.ParseLayoutOrThrow("[9]"),
                tier: 1,
                costs: ((EntityCostsTpl)Costs.Build.CP2(4).MaintenanceT1(2)).MapToEntityCosts(registrator),
                graphics: new LayoutEntityProto.Gfx(
                    prefabPath: NewAssets.Computers.Antena,
                    customIconPath: NewAssets.Computers.Icons.Antena,
                    categories: category
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
                    categories: category
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
                    categories: category
                )
            ));
        }
    }
}