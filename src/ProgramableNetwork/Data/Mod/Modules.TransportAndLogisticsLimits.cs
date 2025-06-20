using Mafi;
using Mafi.Core.Buildings.Storages;
using Mafi.Core.Mods;
using static Mafi.Unity.Assets.Unity;

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
                .Display(m => m.Display["t"] = $"#CAAAA00{UserInterface.Toolbar.Transports_svg}")
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
                .AddDisplay("p", "Percentage", 1, defaultText: "#CAAAA00100")
                .Display(m =>
                {
                    m.Display["p"] = $"#CAAAA00{m.Output["p"].IntegerPart}";
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
                .Display(m => m.Display["t"] = $"#C6688FF{UserInterface.Toolbar.Transports_svg}")
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
                .AddDisplay("p", "Percentage", 1, defaultText: "#CC6688FF0")
                .Display(m =>
                {
                    m.Display["p"] = $"#C6688FF{m.Output["p"].IntegerPart}";
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
                .Display(m => m.Display["t"] = $"#C00AA00{UserInterface.Toolbar.Vehicles_svg}")
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
                .AddDisplay("p", "Percentage", 1, defaultText: "#C00AA000")
                .Display(m =>
                {
                    m.Display["p"] = $"#C00AA00{m.Output["p"].IntegerPart}";
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
                .Display(m => m.Display["t"] = $"#CCC0000{UserInterface.Toolbar.Vehicles_svg}")
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
                .AddDisplay("p", "Percentage", 1, defaultText: "#CCC0000100")
                .Display(m =>
                {
                    m.Display["p"] = $"#CCC0000{m.Output["p"].IntegerPart}";
                })
                .AddControllerDevice()
                .BuildAndAdd();
        }
    }
}
