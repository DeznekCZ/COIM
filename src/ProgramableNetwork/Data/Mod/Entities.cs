using Mafi;
using Mafi.Base;
using Mafi.Base.Prototypes.Machines.ComputingEntities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.Datacenters;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;

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
            }

            public static readonly string Controller = "Assets/ProgramableNetwork/Computer/Computer.prefab";
            public static readonly string Antena = "Assets/ProgramableNetwork/Antena/Antena.prefab";
            public static readonly string AntenaT2 = "Assets/ProgramableNetwork/Antena/AntenaT2.prefab";
        }
    }

    internal class Entities : AValidatedData
    {
        protected override void RegisterDataInternal(ProtoRegistrator registrator)
        {
            ToolbarCategoryProto transportToolbarCategoryProto = registrator.PrototypesDb.Get<ToolbarCategoryProto>(Ids.ToolbarCategories.Transports).ValueOrThrow("Missing game category");
            var category = registrator.PrototypesDb.Add(new ToolbarCategoryProto(
                id: NewIds.Controllers.Category,
                strings: Proto.CreateStr(NewIds.Controllers.Category, "Network", "Contains buildings for for work with network (computation, controller)"),
                order: transportToolbarCategoryProto.Order + 1,
                iconPath: Mafi.Unity.Assets.Unity.UserInterface.EntityIcons.Computing_png,
                isTransportBuildAllowed: true,
                containsTransports: false,
                shortcutId: "NETWORK"
                )).SomeOption().ToImmutableArray();

            // Adapting existing
            registrator.PrototypesDb.Get<DataCenterProto>(Ids.DataCenters.DataCenter)
                .ValueOrNull?.Graphics.SetCategories(category);
            registrator.PrototypesDb.Get<MainframeProto>(Ids.DataCenters.Mainframe)
                .ValueOrNull?.Graphics.SetCategories(category);

            // New entities
            registrator.PrototypesDb.Add(new ControllerProto(
                id: NewIds.Controllers.Controller,
                strings: Proto.CreateStr(NewIds.Controllers.Controller, "Controller", "Handles basic operations and automatization"),
                layout: registrator.LayoutParser.ParseLayoutOrThrow("[1]"),
                costs: ((EntityCostsTpl)Costs.Build.CP2(4)).MapToEntityCosts(registrator),
                allowedModules: (module) => module.AllowedDevices.Contains(NewIds.Controllers.Controller),
                graphics: new LayoutEntityProto.Gfx(
                    prefabPath: NewAssets.Computers.Controller,
                    customIconPath: NewAssets.Computers.Icons.Controller,
                    categories: category
                )
            ));

            registrator.PrototypesDb.Add(new AntenaProto(
                id: NewIds.Controllers.Antena,
                strings: Proto.CreateStr(NewIds.Controllers.Antena, "Antena", "Handles signal transfer for longer distance"),
                layout: registrator.LayoutParser.ParseLayoutOrThrow("[3]"),
                costs: ((EntityCostsTpl)Costs.Build.CP2(4).MaintenanceT1(2)).MapToEntityCosts(registrator),
                graphics: new LayoutEntityProto.Gfx(
                    prefabPath: NewAssets.Computers.Antena,
                    customIconPath: NewAssets.Computers.Icons.Antena,
                    categories: category
                )
            ));

            registrator.PrototypesDb.Add(new AntenaProto(
                id: NewIds.Controllers.AntenaT2,
                strings: Proto.CreateStr(NewIds.Controllers.AntenaT2, "Antena II", "Handles signal transfer for longer distance (100% bonus to range)"),
                layout: registrator.LayoutParser.ParseLayoutOrThrow("[6][6]", "[6][6]"),
                costs: ((EntityCostsTpl)Costs.Build.CP3(8).MaintenanceT2(2)).MapToEntityCosts(registrator),
                graphics: new LayoutEntityProto.Gfx(
                    prefabPath: NewAssets.Computers.AntenaT2,
                    customIconPath: NewAssets.Computers.Icons.Antena,
                    categories: category
                ),
                distanceBoost: Fix32.Two
            ));
        }
    }
}
