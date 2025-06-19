using Mafi;
using Mafi.Base;
using Mafi.Base.Prototypes.Trains;
using Mafi.Core;
using Mafi.Core.Buildings.Cargo;
using Mafi.Core.Buildings.Cargo.Modules;
using Mafi.Core.Buildings.Farms;
using Mafi.Core.Buildings.Mine;
using Mafi.Core.Buildings.Offices;
using Mafi.Core.Buildings.OreSorting;
using Mafi.Core.Buildings.Settlements;
using Mafi.Core.Buildings.Storages;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Dynamic;
using Mafi.Core.Entities.Priorities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.ElectricPower;
using Mafi.Core.Factory.Machines;
using Mafi.Core.Factory.NuclearReactors;
using Mafi.Core.Factory.Recipes;
using Mafi.Core.Factory.Sorters;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Factory.WellPumps;
using Mafi.Core.Maintenance;
using Mafi.Core.Mods;
using Mafi.Core.Population;
using Mafi.Core.Products;
using Mafi.Core.Trains;
using Mafi.Core.Vehicles;
using Mafi.Unity.InputControl;
using ProgramableNetwork.Data.Antene;
using ProgramableNetwork.Data.DataBand;
using ProgramableNetwork.Data.DisplayEntity;
using ProgramableNetwork.Data.DisplayEntity.Displays;
using ProgramableNetwork.Data.Speaker;
using ProgramableNetwork.Data.Variables;
using System;
using System.Linq;
using System.Reflection;
using static Mafi.Base.Assets.Base.Buildings;
using static Mafi.Unity.Assets.Unity;
using CargoDepot = Mafi.Core.Buildings.Cargo.CargoDepot;
using LayoutEntity = Mafi.Core.Entities.Static.Layout.LayoutEntity;
using Transport = Mafi.Core.Factory.Transports.Transport;
using Vehicle = Mafi.Core.Entities.Dynamic.Vehicle;

namespace ProgramableNetwork
{
    internal partial class Modules : AValidatedData
    {
        private void TransportAndLogisticsLimits(ProtoRegistrator registrator)
        {
            Transport(registrator);
            Logistics(registrator);
        }

        private void Transport(ProtoRegistrator registrator)
        {
            registrator
                .ModuleBuilderStart("Connection_Storage_Flow_In_Set", "Connection: Flow (in, set)", "FS")
                .AddCategory(Category.Connection)
                .AddCategory(Category.ConnectionWrite)
                .AddCategory(Category.Control)
                .Width(2)
                .AddInput("p", "Percentage")
                .AddEntityField<Storage>("s", "Storage")
                .Action(m =>
                {
                    Storage storage = m.Field.Entity<Storage>("s");
                    if (storage is null)
                    {
                        m.SetError("Storage is not connected");
                        return ModuleStatus.Error;
                    }

                    Percent percentage = Percent.FromPercentVal((m.Input.Integer["p"] / 10) * 10);
                    if (percentage != storage.TransportUntilPercent)
                        storage.SetTransportUntilPercent(percentage);

                    return ModuleStatus.Running;
                })
                .AddDisplayFiller(1)
                .AddDisplay("t", "Type", 1, image: true)
                .Display(m => m.Display["t"] = UserInterface.Toolbar.Transports_svg)
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Connection_Storage_Flow_In_Get", "Connection: Flow (in, get)", "FG")
                .AddCategory(Category.Connection)
                .AddCategory(Category.ConnectionWrite)
                .AddCategory(Category.Control)
                .Width(1)
                .AddOutput("p", "Percentage")
                .AddEntityField<Storage>("s", "Storage")
                .Action(m =>
                {
                    Storage storage = m.Field.Entity<Storage>("s");
                    if (storage is null)
                    {
                        m.SetError("Storage is not connected");
                        return ModuleStatus.Error;
                    }

                    m.Output["p"] = storage.TransportUntilPercent.ToIntPercentRounded();
                    return ModuleStatus.Running;
                })
                .AddDisplay("p", "Percentage", 1)
                .Display(m =>
                {
                    m.Display["p"] = m.Output["p"].IntegerPart.ToString();
                })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Connection_Storage_Flow_Out_Set", "Connection: Flow (out, set)", "FS")
                .AddCategory(Category.Connection)
                .AddCategory(Category.ConnectionWrite)
                .AddCategory(Category.Control)
                .Width(2)
                .AddInput("p", "Percentage")
                .AddEntityField<Storage>("s", "Storage")
                .Action(m =>
                {
                    Storage storage = m.Field.Entity<Storage>("s");
                    if (storage is null)
                    {
                        m.SetError("Storage is not connected");
                        return ModuleStatus.Error;
                    }

                    Percent percentage = Percent.FromPercentVal((m.Input.Integer["p"] / 10) * 10);
                    if (percentage != storage.TransportFromPercent)
                        storage.SetTransportFromPercent(percentage);

                    return ModuleStatus.Running;
                })
                .AddDisplayFiller(1)
                .AddDisplay("t", "Type", 1, image: true)
                .Display(m => m.Display["t"] = UserInterface.Toolbar.Transports_svg)
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Connection_Storage_Flow_Out_Get", "Connection: Flow (out, get)", "FG")
                .AddCategory(Category.Connection)
                .AddCategory(Category.ConnectionWrite)
                .AddCategory(Category.Control)
                .Width(1)
                .AddOutput("p", "Percentage")
                .AddEntityField<Storage>("s", "Storage")
                .Action(m =>
                {
                    Storage storage = m.Field.Entity<Storage>("s");
                    if (storage is null)
                    {
                        m.SetError("Storage is not connected");
                        return ModuleStatus.Error;
                    }

                    m.Output["p"] = storage.TransportFromPercent.ToIntPercentRounded();
                    return ModuleStatus.Running;
                })
                .AddDisplay("p", "Percentage", 1)
                .Display(m =>
                {
                    m.Display["p"] = m.Output["p"].IntegerPart.ToString();
                })
                .AddControllerDevice()
                .BuildAndAdd();
        }

        private void Logistics(ProtoRegistrator registrator)
        {
            registrator
                .ModuleBuilderStart("Connection_Storage_Logistics_In_Set", "Connection: Logistics (in, set)", "LS")
                .AddCategory(Category.Connection)
                .AddCategory(Category.ConnectionWrite)
                .AddCategory(Category.Control)
                .Width(2)
                .AddInput("p", "Percentage")
                .AddEntityField<Storage>("s", "Storage")
                .Action(m =>
                {
                    Storage storage = m.Field.Entity<Storage>("s");
                    if (storage is null)
                    {
                        m.SetError("Storage is not connected");
                        return ModuleStatus.Error;
                    }

                    Percent percentage = Percent.FromPercentVal((m.Input.Integer["p"] / 10) * 10);
                    if (percentage != storage.ImportUntilPercent)
                        storage.SetImportPercent(percentage);

                    return ModuleStatus.Running;
                })
                .AddDisplayFiller(1)
                .AddDisplay("t", "Type", 1, image: true)
                .Display(m => m.Display["t"] = UserInterface.Toolbar.Vehicles_svg)
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Connection_Storage_Logistics_In_Get", "Connection: Logistics (in, get)", "LG")
                .AddCategory(Category.Connection)
                .AddCategory(Category.ConnectionWrite)
                .AddCategory(Category.Control)
                .Width(1)
                .AddOutput("p", "Percentage")
                .AddEntityField<Storage>("s", "Storage")
                .Action(m =>
                {
                    Storage storage = m.Field.Entity<Storage>("s");
                    if (storage is null)
                    {
                        m.SetError("Storage is not connected");
                        return ModuleStatus.Error;
                    }

                    m.Output["p"] = storage.ImportUntilPercent.ToIntPercentRounded();
                    return ModuleStatus.Running;
                })
                .AddDisplay("p", "Percentage", 1)
                .Display(m =>
                {
                    m.Display["p"] = m.Output["p"].IntegerPart.ToString();
                })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Connection_Storage_Logistics_Out_Set", "Connection: Logistics (out, set)", "LS")
                .AddCategory(Category.Connection)
                .AddCategory(Category.ConnectionWrite)
                .AddCategory(Category.Control)
                .Width(2)
                .AddInput("p", "Percentage")
                .AddEntityField<Storage>("s", "Storage")
                .Action(m =>
                {
                    Storage storage = m.Field.Entity<Storage>("s");
                    if (storage is null)
                    {
                        m.SetError("Storage is not connected");
                        return ModuleStatus.Error;
                    }

                    Percent percentage = Percent.FromPercentVal((m.Input.Integer["p"] / 10) * 10);
                    if (percentage != storage.ExportFromPercent)
                        storage.SetExportPercent(percentage);

                    return ModuleStatus.Running;
                })
                .AddDisplayFiller(1)
                .AddDisplay("t", "Type", 1, image: true)
                .Display(m => m.Display["t"] = UserInterface.Toolbar.Vehicles_svg)
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Connection_Storage_Logistics_Out_Get", "Connection: Logistics (out, get)", "LG")
                .AddCategory(Category.Connection)
                .AddCategory(Category.ConnectionWrite)
                .AddCategory(Category.Control)
                .Width(1)
                .AddOutput("p", "Percentage")
                .AddEntityField<Storage>("s", "Storage")
                .Action(m =>
                {
                    Storage storage = m.Field.Entity<Storage>("s");
                    if (storage is null)
                    {
                        m.SetError("Storage is not connected");
                        return ModuleStatus.Error;
                    }

                    m.Output["p"] = storage.ExportFromPercent.ToIntPercentRounded();
                    return ModuleStatus.Running;
                })
                .AddDisplay("p", "Percentage", 1)
                .Display(m =>
                {
                    m.Display["p"] = m.Output["p"].IntegerPart.ToString();
                })
                .AddControllerDevice()
                .BuildAndAdd();
        }
    }
}
