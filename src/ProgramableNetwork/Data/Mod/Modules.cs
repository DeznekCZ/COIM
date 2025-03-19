using Mafi;
using Mafi.Base;
using Mafi.Core.Buildings.Cargo;
using Mafi.Core.Buildings.Farms;
using Mafi.Core.Buildings.Offices;
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
using Mafi.Core.Factory.Sorters;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Factory.WellPumps;
using Mafi.Core.Maintenance;
using Mafi.Core.Mods;
using Mafi.Core.Population;
using Mafi.Core.Products;
using Mafi.Core.Vehicles;
using Mafi.Unity.InputControl.TopStatusBar;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UserInterface.Components;
using System;
using System.Linq;
using System.Reflection;

namespace ProgramableNetwork
{
    internal partial class Modules : AValidatedData
    {
        static readonly string[] names = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "m", "n", "o", "p", "q" };

        protected override void RegisterDataInternal(ProtoRegistrator registrator)
        {
            Constants(registrator);
            Buttons(registrator);
            Arithmetic(registrator);
            Comparation(registrator);
            Connections(registrator);
            Stats(registrator);
            Forks(registrator);
            Booleans(registrator);
            Decisions(registrator);
            Display(registrator);
            RadioAM(registrator);
            RadioFM(registrator);

            // SPECIAL
            registrator
                .ModuleBuilderStart("Game_Pause", "Pause game (DEBUG)", "GP", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Control)
                .AddInput("pause", "Pause")
                .Action(m => {
                    m.Info = false;
                    if (m.Input["pause", 0] > Fix32.Zero)
                    {
                        GlobalDependencyResolver.Get<GameSpeedController>().RequestPause();
                        m.Info = true;
                    }
                    else
                    {
                        m.Info = false;
                    }
                })
                .AddControllerDevice()
                .BuildAndAdd();
        }

        private static void Constants(ProtoRegistrator registrator)
        {
            registrator
                .ModuleBuilderStart("Constant", "Constant (integer)", "#I", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Arithmetic)
                .AddOutput("value", "Value")
                .AddInt32Field("number", "Number")
                .Action(m => { m.Output["value"] = m.Field["number", 0]; })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Constant_Product", "Constant (product)", "#P", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Arithmetic)
                .AddOutput("value", "Value")
                .AddProductField("product", "Product")
                .Action(m => { m.Output["value"] = m.Field["product", 0]; })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Constant_Crop", "Constant (crop)", "#C", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Arithmetic)
                .AddOutput("value", "Value")
                .AddProductField("crop", "Crop", filter: FarmProductFilter)
                .Action(m => { m.Output["value"] = m.Field["crop", 0]; })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Constant_Machine", "Constant (machine)", "#M", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Arithmetic)
                .AddOutput("value", "Value")
                .AddEntityTypeField<MachineProto>("machine", "Machine")
                .Action(m => { m.Output["value"] = m.Field["machine", 0]; })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Constant_Vehicle", "Constant (vehicle)", "#V", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Arithmetic)
                .AddOutput("value", "Value")
                .AddEntityTypeField<DrivingEntityProto>("vehicle", "vehicle")
                .Action(m => { m.Output["value"] = m.Field["vehicle", 0]; })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Constant_Boolean", "Constant (boolean)", "#B", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Arithmetic)
                .AddOutput("value", "Value")
                .AddBooleanField("boolean", "Boolean")
                .Action(m => { m.Output["value"] = m.Field["boolean", 0]; })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Constant_Float", "Constant (float)", "#F", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Arithmetic)
                .AddOutput("value", "Value")
                .AddFix32Field("float", "Float")
                .Action(m => { m.Output["value"] = m.Field["float", 0]; })
                .AddControllerDevice()
                .BuildAndAdd();
        }

        private void Buttons(ProtoRegistrator registrator)
        {
            registrator
                .ModuleBuilderStart("Button_1", "Button (on/off)", "0/I", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Control)
                .AddOutput("value", "On - 1, Off - 0")
                .AddDisplay("toggle", "Toggle", 1, toggle: new [] { "( | )" })
                .Action(m => m.Output["value"] = (m.Display["toggle", ""].Length > 0) ? Fix32.One : Fix32.Zero)
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Button_Pass", "Button (pass value)", "0/I", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Control)
                .AddInput("value", "Value")
                .AddOutput("value", "Value")
                .AddDisplay("toggle", "Toggle", 1, toggle: new string[] { "⬇" })
                .Action(m => m.Output["value"] = (m.Display["toggle", ""].Length > 0) ? m.Input["value"] : Fix32.Zero)
                .AddControllerDevice()
                .BuildAndAdd();
        }

        private static void Arithmetic(ProtoRegistrator registrator)
        {
            registrator
                .ModuleBuilderStart("Sum", "C = A + B", "A+B", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Arithmetic)
                .AddInput("a", "A")
                .AddInput("b", "B")
                .AddBooleanField("field_b", "Use direct constant")
                .AddFix32Field("b", "B")
                .AddOutput("c", "Sum")
                .Action(m => { m.Output["c"] = m.Input["a", 0] + m.FieldOrInput["b", 0]; })
                .AddControllerDevice()
                .BuildAndAdd();

            Action<Module> SumFor(int i)
            {
                return (m) =>
                {
                    Fix32 value = 0;
                    for (int j = 0; j < i; j++)
                    {
                        value += m.Input[names[j], 0];
                    }
                    m.Output["sum"] = value;
                };
            }
            foreach (int i in new int[] { 4, 8 })
            {
                var sum = registrator
                    .ModuleBuilderStart($"Sum_{i}", $"C = A + .. ({i - 1})", $"A+({i - 1})", Assets.Base.Products.Icons.Vegetables_svg)
                    .AddCategory(Category.Arithmetic)
                    .AddOutput("sum", "Sum")
                    .Action(m => { m.Output["c"] = m.Input["a", 0] + m.Input["b", 0]; })
                    .AddControllerDevice();

                for (int j = 0; j < i; j++)
                    sum.AddInput(names[j], names[j].ToUpper());

                sum.Action(SumFor(i));
                sum.BuildAndAdd();
            }

            registrator
                .ModuleBuilderStart("Sub", "C = A - B", "A-B", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Arithmetic)
                .AddInput("a", "A")
                .AddInput("b", "B")
                .AddBooleanField("field_b", "Use direct constant")
                .AddFix32Field("b", "B")
                .AddOutput("c", "C")
                .Action(m => { m.Output["c"] = m.Input["a", 0] - m.FieldOrInput["b", 0]; })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Invert", "B = -A", "-A", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Arithmetic)
                .AddInput("a", "A")
                .AddOutput("b", "B")
                .Action(m => { m.Output["b"] = 0 - m.Input["a", 0]; })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Multiply", "C = A multiply by B", "A*B", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Arithmetic)
                .AddInput("a", "A")
                .AddInput("b", "B")
                .AddBooleanField("field_b", "Use direct constant")
                .AddFix32Field("b", "B")
                .AddOutput("c", "C")
                .Action(m => {
                    Fix32 a = m.Input["a", 0];
                    Fix32 b = m.FieldOrInput["b", 0];
                    Fix32 c = a * b;
                    m.Output["c"] = a * b;
                })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Divide", "C = A divide by B", "A/B", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Arithmetic)
                .AddInput("a", "A")
                .AddInput("b", "B")
                .AddBooleanField("field_b", "Use direct constant")
                .AddFix32Field("b", "B")
                .AddOutput("c", "C")
                .AddOutput("error", "Error")
                .Action(m => {
                    Fix32 a = m.Input["a", 0];
                    Fix32 b = m.FieldOrInput["b", 0];
                    if (b == 0)
                    {
                        m.Output["error"] = 1;
                        m.Output["c"] = Fix32.MaxValue;
                    }
                    else
                    {
                        m.Output["error"] = 0;
                        m.Output["c"] = a / b;
                    }
                })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Modulo", "C = A modulo B", "A%B", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Arithmetic)
                .AddInput("a", "A")
                .AddInput("b", "B")
                .AddBooleanField("field_b", "Use direct constant")
                .AddFix32Field("b", "B")
                .AddOutput("c", "C")
                .AddOutput("error", "Error")
                .Action(m => {
                    Fix32 a = m.Input["a", 0];
                    Fix32 b = m.FieldOrInput["b", 0];
                    if (b == 0)
                    {
                        m.Output["error"] = 1;
                        m.Output["c"] = 0;
                    }
                    else
                    {
                        m.Output["error"] = 0;
                        m.Output["c"] = a % b;
                    }
                })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Average", "Average", "~A", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Arithmetic)
                .AddInput("input", "Input")
                .AddOutput("count", "Average")
                .AddOutput("average", "Average")
                .AddInt32Field("count", "Count", "Maximum number of values to be counted in average with default: 10", 10)
                .Action(m => {
                    Fix32 desiredCount = m.Field["count", 1];
                    if (desiredCount < 1) desiredCount = 1;

                    Fix32 input = m.Input["input", 0];
                    Fix32 oldCount = (m.Output["count", 1] - 1);
                    Fix32 average = (m.Output["average", 0] * oldCount) + input;

                    m.Output["count"] = Min(desiredCount, m.Output["count", 0] + 1.ToFix32());
                    m.Output["average"] = (average / m.Output["count"]);
                })
                .AddControllerDevice()
                .BuildAndAdd();
        }

        private static Fix32 Min(Fix32 a, Fix32 b)
        {
            return a < b ? a : b;
        }

        private static Fix32 Min(Fix32 a, int b)
        {
            return a < b.ToFix32() ? a : b.ToFix32();
        }

        private static Fix32 Min(int a, Fix32 b)
        {
            return a.ToFix32() < b ? a.ToFix32() : b;
        }

        private void Stats(ProtoRegistrator registrator)
        {
            registrator
                .ModuleBuilderStart("Stats_Unity", "Connection: Office - Unity", "UNI", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Connection)
                .AddCategory(Category.ConnectionRead)
                .AddCategory(Category.Stats)
                .AddOutput("v", "Unity value")
                .AddEntityField<CaptainOffice>("office", "Captains office", "Must be placest next to Captains office", 2.ToFix32())
                .Action(m =>
                {
                    if (m.Field.Entity<CaptainOffice>("office") is null)
                        return ModuleStatus.Error;

                    m.Output["v"] = Fix32.FromRaw(m.Context.UpointsManager.Quantity.Value);
                    return ModuleStatus.Running;
                })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Stats_Workers", "Connection: Office - Workers", "WRK", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Connection)
                .AddCategory(Category.ConnectionRead)
                .AddCategory(Category.Stats)
                .AddOutput("u", "Used workers")
                .AddOutput("a", "Available workers")
                .AddOutput("m", "Missing workers")
                .AddOutput("t", "Total workers")
                .AddEntityField<CaptainOffice>("office", "Captains office", "Must be placest next to Captains office", 2.ToFix32())
                .Action(m =>
                {
                    if (m.Field.Entity<CaptainOffice>("office") is null)
                        return ModuleStatus.Error;

                    m.Output["a"] = Math.Max(0, m.Context.WorkersManager.AmountOfFreeWorkersOrMissing);
                    m.Output["t"] = (int)(m.Context.WorkersManager as WorkersManager).TotalWorkersNeededStats.ThisYear + m.Output["a"];
                    m.Output["m"] = 0-Math.Min(0, m.Context.WorkersManager.AmountOfFreeWorkersOrMissing);
                    m.Output["u"] = m.Output["t", 0] - m.Output["a", 0];
                    return ModuleStatus.Running;
                })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Stats_Electricity", "Statistic: Electricity", "PWR", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Stats)
                .AddOutput("consumption", "Consumption")
                .AddOutput("production", "Production")
                .AddOutput("capacity", "Power capacity")
                .AddOutput("usage", "Power usage (0-100)")
                .Width(4)
                .Action(m =>
                {
                    ElectricityManager electricity = m.Controller.ElectricityConsumer.Value.GetType()
                        .GetField("m_electricityManager", BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue(m.Controller.ElectricityConsumer.Value) as ElectricityManager;

                    Mafi.Core.Stats.ElectricityAvgStats consumption = electricity.ConsumptionStats;
                    Mafi.Core.Stats.ElectricityAvgStats production = electricity.ProductionStats;
                    Mafi.Core.Stats.ElectricityAvgStats capacity = electricity.GenerationCapacityStats;

                    m.Output["consumption"] = consumption.LastDay.Value.ToFix32();
                    m.Output["production"] = production.LastDay.Value.ToFix32();
                    m.Output["capacity"] = capacity.LastDay.Value.ToFix32();
                    m.Output["usage"] = 100.ToFix32() * (consumption.LastDay.Value.ToFix32() / capacity.LastDay.Value.ToFix32());
                })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Stats_Maintenance", "Connection: Maintenance", "MAINT", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Connection)
                .AddCategory(Category.ConnectionRead)
                .AddCategory(Category.Stats)
                .AddOutput("a", "Amount")
                .AddOutput("c", "Capacity")
                .AddOutput("p", "Percentage (0-100)")
                .AddOutput("u", "Use (monthly +surplus / -deficit)")
                .Width(4)
                .AddEntityField<MaintenanceDepot>("depot", "Maintenance depot", distance: 2.ToFix32())
                .AddProductField("m", "Maintenance tier", filter: (m, p) => p.Id.Value.StartsWith("Product_Virtual_MaintenanceT"))
                .Action(m =>
                {
                    MaintenanceDepot maintenanceDepot = m.Field.Entity<MaintenanceDepot>("depot");
                    if (maintenanceDepot == null)
                    {
                        m.SetError("Invalid maintenance product");
                        return ModuleStatus.Error;
                    }

                    ProductProto product = m.Field.Product("m");
                    if (product == null)
                    {
                        product = m.Context.ProtosDb.Get<ProductProto>(Ids.Products.MaintenanceT1).Value;
                        m.Field["m"] = Fix32.FromRaw(product.SlimId.Value);
                    }

                    ProductStats productStats = m.Context.ProductsManager.GetStatsFor(product);
                    IMaintenanceBufferReadonly buffer = ((MaintenanceManager)maintenanceDepot.GetType()
                        .GetField("m_maintenanceManager", BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue(maintenanceDepot))
                        .MaintenanceBuffers.First(b => b.Product == product);

                    //m.Output["a"] = productStats.GlobalQuantity.ToQuantity().Value.Value.ToFix32();
                    m.Output["a"] = buffer.Quantity.Value.ToFix32();
                    m.Output["c"] = buffer.Capacity.Value.ToFix32();
                    m.Output["p"] = 100.ToFix32() * (buffer.Quantity.Value.ToFix32() / buffer.Capacity.Value.ToFix32());
                    m.Output["u"] = (productStats.CreatedByProduction.LastMonth - productStats.UsedTotalStats.LastMonth)
                                        .ToQuantity().Value.Value.ToFix32();
                    return ModuleStatus.Running;
                })
                .AddControllerDevice()
                .BuildAndAdd();
        }

        private void Forks(ProtoRegistrator registrator)
        {
            Action<Module>[] actions = new Action<Module>[] {
                m => {
                    m.Output["a"] = m.Input["a"];
                    m.Output["b"] = m.Input["a"];
                },
                m => {
                    m.Output["a"] = m.Input["a"];
                    m.Output["b"] = m.Input["a"];
                    m.Output["c"] = m.Input["a"];
                    m.Output["d"] = m.Input["a"];
                },
            };
            foreach (int i in new int[] { 2, 4 })
            {
                var builder = registrator
                    .ModuleBuilderStart($"Fork_{i}", $"Fork: 1 pin to {i}", $"F-{i}", Assets.Base.Products.Icons.Vegetables_svg,
                        description: "Is used for organizing of pin connection. Is not required to use, output may be connected to multiple inputs")
                    .AddInput("a", "A")
                    .AddControllerDevice()
                    // dynamic
                    .Action(actions[(i / 2) - 1]);

                for (int j = 0; j < i; j++)
                    builder.AddOutput(names[j], names[j].ToUpper());

                builder.BuildAndAdd();
            }
        }

        private void Booleans(ProtoRegistrator registrator)
        {
            Action<Module>[] ands = new Action<Module>[] {
                (m) =>
                {
                    m.Output["a"] = (
                        m.Input["a", 0] > 0 &&
                        m.Input["b", 0] > 0
                    ) ? 1 : 0;
                    m.Output["b"] = m.Output["a"] > 0 ? 0 : 1;
                },
                (m) =>
                {
                    m.Output["a"] = (
                        m.Input["a", 0] > 0 &&
                        m.Input["b", 0] > 0 &&
                        m.Input["c", 0] > 0 &&
                        m.Input["d", 0] > 0
                    ) ? 1 : 0;
                    m.Output["b"] = m.Output["a"] > 0 ? 0 : 1;
                }
            };
            foreach (int i in new int[] { 2, 4 })
            {
                var builder = registrator
                    .ModuleBuilderStart($"Boolean_And_{i}", $"Boolean: AND ({i} pins)", $"AND-{i}", Assets.Base.Products.Icons.Vegetables_svg)
                    .AddCategory(Category.Boolean)
                    .AddOutput("b", "not A")
                    .AddOutput("a", "A")
                    .AddControllerDevice()
                    // dynamic
                    .Action(ands[(i / 2) - 1]);

                for (int j = 0; j < i; j++)
                    builder.AddInput(names[j], names[j].ToUpper());

                builder.BuildAndAdd();
            }
            Action<Module>[] ors = new Action<Module>[] {
                (m) =>
                {
                    m.Output["a"] = (
                        m.Input["a"] > 0 ||
                        m.Input["b"] > 0
                    ) ? 1 : 0;
                    m.Output["b"] = m.Output["a"] > 0 ? 0 : 1;
                },
                (m) =>
                {
                    m.Output["a"] = (
                        m.Input["a", 0] > 0 ||
                        m.Input["b", 0] > 0 ||
                        m.Input["c", 0] > 0 ||
                        m.Input["d", 0] > 0
                    ) ? 1 : 0;
                    m.Output["b"] = m.Output["a"] > 0 ? 0 : 1;
                }
            };
            foreach (int i in new int[] { 2, 4 })
            {
                var builder = registrator
                    .ModuleBuilderStart($"Boolean_Or_{i}", $"Boolean: OR ({i} pins)", $"OR-{i}", Assets.Base.Products.Icons.Vegetables_svg)
                    .AddCategory(Category.Boolean)
                    .AddOutput("b", "not A")
                    .AddOutput("a", "A")
                    .AddControllerDevice()
                    // dynamic
                    .Action(ors[(i / 2) - 1]);

                for (int j = 0; j < i; j++)
                    builder.AddInput(names[j], names[j].ToUpper());

                builder.BuildAndAdd();
            }
            registrator
                .ModuleBuilderStart($"Boolean_Xor", $"Boolean: XOR", $"XOR", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Boolean)
                .AddInput("a", "A")
                .AddInput("b", "B")
                .AddOutput("b", "not A")
                .AddOutput("a", "A")
                .AddControllerDevice()
                // dynamic
                .Action(m =>
                    {
                        m.Output["a"] = (
                            m.Input["a"] > 0 !=
                            m.Input["b"] > 0
                        ) ? 1 : 0;
                        m.Output["b"] = m.Output["a"] > 0 ? 0 : 1;
                    })
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart($"Boolean_Not", $"Boolean: NOT", $"nA", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Boolean)
                .AddInput("a", "A")
                .AddOutput("a", "not A")
                .AddControllerDevice()
                // dynamic
                .Action(m => m.Output["a"] = m.Input["a"] > 0 ? 0 : 1)
                .BuildAndAdd();
        }

        private void Decisions(ProtoRegistrator registrator)
        {
            Action<Module> Select(int count)
            {
                return m =>
                {
                    Fix32 index = m.Input["index", Fix32.MaxValue];
                    int digits = count * 2;
                    string text = index.IntegerPart.ToString($"D{digits}");
                    m.Display["index"] = text.Length > digits ? text.Substring(text.Length - digits) : text;

                    for (int i = 0; i < count; i++)
                    {
                        string name = names[i];
                        Fix32 value = m.Field[name, 0];
                        if (index <= value)
                        {
                            m.Output["selected"] = m.Input[name, 0];
                            return;
                        }
                    }
                    m.Output["selected"] = m.Input["else", 0];
                };
            }
            foreach (int i in new int[] { 4, 8 })
            {
                var builder = registrator
                    .ModuleBuilderStart($"Decision_Select_{i}", $"Select ({i-1} pins, integer)", $"SEL-{i-1}", Assets.Base.Products.Icons.Vegetables_svg)
                    .AddCategory(Category.Decision)
                    .AddCategory(Category.Control)
                    .AddInput("index", "Index")
                    .AddOutput("selected", "Selected")
                    .AddDisplay("index", "Index", i)
                    .AddControllerDevice()
                    // dynamic
                    .Action(Select(i - 2));

                for (int j = 0; j < i - 2; j++)
                    builder.AddInput(names[j], names[j].ToUpper());

                for (int j = 0; j < i - 2; j++)
                    builder.AddInt32Field(names[j], names[j].ToUpper() + ": Index ≤", defaultValue: j);

                builder.AddInput("else", "Else");
                builder.BuildAndAdd();
            }
        }

        private static void Connections(ProtoRegistrator registrator)
        {
            registrator
                .ModuleBuilderStart("Connection_Controller_Input", "Connection: Controller (4 pin, input)", "C-IN", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Connection)
                .AddCategory(Category.ConnectionRead)
                .AddOutput("a", "A")
                .AddOutput("b", "B")
                .AddOutput("c", "C")
                .AddOutput("d", "D")
                .AddEntityField<Controller>("controller", "Connection device", "Name of output module, which must exist in target Controller", distance: 20.ToFix32())
                .AddStringField("name", "Output Name", defaultValue: "C")
                .Action(m =>
                {
                    //Mafi.Log.Info("Update of input");
                    Controller controller = m.Field.Entity<Controller>("controller");
                    string moduleType = "Connection_Controller_Output".ModuleId();
                    string noduleName = m.Field["name", "C"];
                    if (noduleName.Length > 0 && controller != null)
                    {
                        //Mafi.Log.Info("Target entity found");
                        Module targetModule = controller.Modules.AsEnumerable()
                            .FirstOrDefault(mod => mod.Prototype.Id.Value == moduleType
                                                && mod.Field["name", ""] == noduleName);
                        if (targetModule != null)
                        {
                            m.Output["a"] = targetModule.Input["a", 0];
                            m.Output["b"] = targetModule.Input["b", 0];
                            m.Output["c"] = targetModule.Input["c", 0];
                            m.Output["d"] = targetModule.Input["d", 0];

                            m.StatusOut["a"] = ModuleStatus.Running;
                            m.StatusOut["b"] = ModuleStatus.Running;
                            m.StatusOut["c"] = ModuleStatus.Running;
                            m.StatusOut["d"] = ModuleStatus.Running;
                            return;
                        }
                    }
                    m.Output["a"] = 0;
                    m.Output["b"] = 0;
                    m.Output["c"] = 0;
                    m.Output["d"] = 0;
                })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Connection_Controller_Output", "Connection: Controller (4 pin, output)", "C-OUT", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Connection)
                .AddCategory(Category.ConnectionWrite)
                .AddInput("a", "A")
                .AddInput("b", "B")
                .AddInput("c", "C")
                .AddInput("d", "D")
                .AddStringField("name", "Name", "Name of output module, which must be selected in target Controller", defaultValue: "C")
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Connection_SwitchOff", "Connection: Switch Off", "S", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Connection)
                .AddCategory(Category.ConnectionWrite)
                .AddInput("pause", "Pause")
                .AddEntityField<StaticEntity>("entity", "Connection device", "Any pausable building connectable by cable 20m from controller", distance: 20.ToFix32(), filter: (m,e) => e.CanBePaused || e is CargoDepot)
                .Action(m =>
                {
                    StaticEntity entity = m.Field.Entity<StaticEntity>("entity");
                    Fix32 input = m.Input["pause", 0];
                    if (entity?.CanBePaused ?? false)
                    {
                        entity.SetPaused(input > 0);
                        return ModuleStatus.Running;
                    }
                    if (entity is CargoDepot cargo)
                    {
                        m.Warning = !cargo.CargoShip.HasValue;
                        if (cargo.CargoShip.HasValue)
                            cargo.CargoShip.Value.SetPaused(input > 0);
                        return ModuleStatus.Running;
                    }
                    return ModuleStatus.Error;
                })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Connection_Storage", "Connection: Storage", "STOCK", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Connection)
                .AddCategory(Category.ConnectionRead)
                .AddInput("product", "Product")
                .AddOutput("quantity", "Quantity")
                .AddOutput("capacity", "Capacity")
                .AddOutput("fullness", "Fullness in %")
                .AddOutput("product", "Product in #")
                .AddEntityField<LayoutEntity>("entity", "Connection device", "Storage connectable by cable 20m from controller", distance: 20.ToFix32(),
                    filter: (m, e) => e is StorageBase || // e is SettlementWasteModule
                                      e is SettlementFoodModule ||
                                      e is Hospital ||
                                      e is SettlementModuleProto
                    )
                .Action(m =>
                {
                    LayoutEntity entity = m.Field.Entity<LayoutEntity>("entity");

                    if (entity is StorageBase storage)
                    // entity is SettlementWasteModule
                    {
                        m.Output["quantity"] = storage.CurrentQuantity.Value;
                        m.Output["capacity"] = storage.Capacity.Value;
                        m.Output["fullness"] = (int)(100f * storage.CurrentQuantity.Value / storage.Capacity.Value);

                        if (storage.StoredProduct.HasValue)
                            m.Output["product"] = Fix32.FromRaw((int)(uint)storage.StoredProduct.Value.SlimId.Value);
                        else
                            m.Output["product"] = Fix32.Zero;
                        return ModuleStatus.Running;
                    }

                    if (entity is SettlementFoodModule foodModule)
                    {
                        if (m.Input["product", Fix32.Zero] == Fix32.Zero)
                        {
                            m.SetError("Product is not selected");
                            return ModuleStatus.Error;
                        }

                        ProductProto product = m.Input.Product("product");
                        var buffers = new[] { foodModule.GetBuffer(0).ValueOrNull, foodModule.GetBuffer(1).ValueOrNull };
                        return GetValueFromBuffers(m, product, buffers);
                    }

                    if (entity is Hospital hospital)
                    {
                        ProductProto product = m.Input.Product("product");
                        if (product is null)
                        {
                            m.SetError("Product is not selected");
                            return ModuleStatus.Error;
                        }

                        var buffers = new[] { hospital.GetBuffer(0).ValueOrNull, hospital.GetBuffer(1).ValueOrNull };
                        return GetValueFromBuffers(m, product, buffers);
                    }

                    if (entity is SettlementServiceModule module)
                    {
                        ProductProto product = m.Input.Product("product");
                        if (product is null)
                        {
                            m.SetError("Product is not selected");
                            return ModuleStatus.Error;
                        }

                        var buffers = new[] {
                            (IProductBuffer)module.GetType().GetField("m_inputBuffer", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(module),
                            ((Option<IProductBuffer>)module.GetType().GetField("m_inputBuffer", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(module)).ValueOrNull
                        };
                        return GetValueFromBuffers(m, product, buffers);
                    }

                    m.Output["quantity"] = 0;
                    m.Output["capacity"] = 0;
                    m.Output["fullness"] = 100;
                    m.Output["product"] = -1;
                    return ModuleStatus.Error;
                })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Connection_Transport", "Connection: Transport", "TRANS", Assets.Base.Products.Icons.Vegetables_svg, "Transport connectable by cable 20m from controller")
                .AddCategory(Category.Connection)
                .AddCategory(Category.ConnectionRead)
                .AddOutput("quantity", "Quantity")
                .AddOutput("capacity", "Capacity")
                .AddOutput("fullness", "Fullness in %")
                .AddOutput("moving", "Is moving")
                .AddInput("product", "Product filter")
                .AddEntityField<Transport>("entity", "Connection device", 20.ToFix32())
                .AddBooleanField("fullstack", "Cap fullness to 100%", "Bigger tiers of transport may display value over 100%. It's caused by maximum stack size. Activating this option will be the value normalized to 100%.")
                // TODO add filter input field
                .Action(m =>
                {
                    Transport entity = m.Field.Entity<Transport>("entity");
                    Fix32 buffer = m.Input["buffer", 0];
                    int filterId = m.Input["product", 0].RawValue;
                    bool fullstack = m.Field["fullstack", 0] > 0;

                    if (entity != null)
                    {
                        m.Output["quantity"] = entity.TransportedProducts
                                            .Where(p => filterId == 0 || p.SlimId.Value == filterId)
                                            .Select(p => p.Quantity.Value).Sum();
                        m.Output["capacity"] = entity.Trajectory.MaxProducts
                                             * (fullstack ? entity.Prototype.MaxQuantityPerTransportedProduct.Value : 1);
                        m.Output["fullness"] = (100.ToFix32() * m.Output["quantity"]) / m.Output["capacity"];
                        m.Output["moving"] = entity.GetStatus() == Transport.Status.Moving ? 1 : 0;
                        return;
                    }

                    m.Output["quantity"] = 0;
                    m.Output["capacity"] = 0;
                    m.Output["fullness"] = 100;
                    m.Output["moving"] = 0;
                    throw new Exception("Entity can not be read");
                })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Connection_Settlement", "Connection: Settlement (population)", "SETTLE", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Connection)
                .AddCategory(Category.ConnectionRead)
                .AddCategory(Category.Stats)
                .AddOutput("pop_this", "Population (settlement module)")
                .AddOutput("pop_nearby", "Population (settlement total)")
                .AddInput("product", "First Product in #")
                .AddEntityField<SettlementHousingModule>("entity", "Connection device", 20.ToFix32())
                // TODO add filter input field
                .Action(m =>
                {
                    SettlementHousingModule entity = m.Field.Entity<SettlementHousingModule>("entity");

                    if (entity != null)
                    {
                        m.Output["pop_this"] = entity.Population;
                        m.Output["pop_nearby"] = entity.Settlement.ValueOrNull?.Population ?? entity.Population;
                        return;
                    }

                    m.Output["pop_this"] = 0;
                    m.Output["pop_nearby"] = 0;
                })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Connection_NuclearReactor", "Connection: Nuclear Reactor", "NR", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Connection)
                .AddCategory(Category.ConnectionRead)
                .AddCategory(Category.ConnectionWrite)
                .AddInput("target", "Target power level")
                .AddOutput("heat", "Stored Heat")
                .AddOutput("meltdown", "Is in melt down")
                .AddOutput("power", "Actual power")
                .Width(4)
                .AddEntityField<NuclearReactor>("reactor", "Connection reactor", 2.ToFix32())
                .Action(m => {
                    var reactor = m.Field.Entity<NuclearReactor>("reactor");

                    m.Output["heat"] = reactor.HeatAmount.ToFix32();
                    m.Output["meltdown"] = reactor.IsInMeltdown ? 1.ToFix32() : 2.ToFix32();
                    m.Output["power"] = reactor.CurrentPowerLevel.ToFix32();

                    Percent target = m.Input["target", Fix32.Zero].ToPercent();
                    if (!reactor.IsInMeltdown && reactor.TargetPowerLevel != target)
                    {
                        reactor.SetTargetPowerLevel(target);
                    }
                })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Connection_Farm", "Connection: Farm", "FARM", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Connection)
                .AddCategory(Category.ConnectionWrite)
                .AddCategory(Category.ConnectionRead)
                .AddInput("crop", "Next growing crop")
                .AddInput("fertility", "Target fertility (default: 100)")
                .AddInput("fertilize", "Assign nature fertilizing (0/1)")
                .AddOutput("crop", "Actual growing crop")
                .AddOutput("water", "Actual water level (including tank)")
                .AddOutput("fertility", "Actual fertility")
                .AddOutput("fertilizer", "Actual fertilizer level")
                .Width(4)
                .AddEntityField<Farm>("farm", "Managed farm", "Farm placed next to controller (2 metres)", 2.ToFix32())
                .Action(m => {
                    var farm = m.Field.Entity<Farm>("farm");
                    if (farm is null)
                    {
                        m.SetError("Farm is not set");
                        return ModuleStatus.Error;
                    }

                    if (farm.CurrentCrop.ValueOrNull is null || farm.CurrentCrop.Value.ProductProduced.IsEmpty)
                        m.Output["crop"] = Fix32.Zero;
                    else
                        m.Output["crop"] = Fix32.FromRaw(farm.CurrentCrop.Value.Prototype.ProductProduced.Product.SlimId.Value);
                    m.Output["water"] = farm.ImportedWaterBuffer.Quantity.Value.ToFix32() + farm.SoilWaterBuffer.Quantity.Value.ToFix32();
                    m.Output["fertility"] = farm.Fertility.ToFix32() * 100;
                    if (farm.StoredFertilizerCapacity.Value > 0)
                        m.Output["fertilizer"] = (100f * farm.StoredFertilizerCount.Value / farm.StoredFertilizerCapacity.Value).ToFix32();
                    else
                        m.Output["fertilizer"] = Fix32.Zero;

                    farm.SetFertilityTarget((m.Input["fertility", Fix32.One * 100] / 100).ToPercent());

                    int nextSlot = (farm.ActiveScheduleIndex + 1) % 4;
                    if (m.Input["crop", Fix32.Zero] > Fix32.Zero)
                    { // try get crop type by output product
                        int slimId = m.Input["crop", Fix32.Zero].RawValue;
                        CropProto foundCrop = m.Context.ProtosDb
                            .Filter<CropProto>(crop => crop.ProductProduced.Product.SlimId.Value == slimId)
                            .FirstOrDefault();

                        if (foundCrop != null)
                        {
                            m.SetError("");
                            farm.AssignCropToSlot(foundCrop.CreateOption(), nextSlot);
                            return ModuleStatus.Running;
                        }
                        else
                        {
                            m.SetError("Invalid crop");
                            farm.AssignCropToSlot(m.Context.ProtosDb.Get<CropProto>(Ids.Crops.NoCrop), nextSlot);
                            return ModuleStatus.Error;
                        }
                    }
                    else if (m.Input["crop", Fix32.Zero] == 0 && m.Input["fertilize", Fix32.Zero] > Fix32.Zero)
                    {
                        farm.AssignCropToSlot(m.Context.ProtosDb.Get<CropProto>(Ids.Crops.GreenManure), nextSlot);
                        return ModuleStatus.Running;
                    }
                    else
                    {
                        farm.AssignCropToSlot(m.Context.ProtosDb.Get<CropProto>(Ids.Crops.NoCrop), nextSlot);
                        return ModuleStatus.Running;
                    }
                })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Connection_Import_Set", "Connection: Import (set)", "IMS", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Connection)
                .AddCategory(Category.ConnectionWrite)
                .AddInput("mode", "Import mode:\n  0 (auto),\n  1 (on),\n  2 (off)")
                .AddEntityField<IStaticEntity>("logistic", "Logistic building", "Any logistic building (20 metres)",
                    distance: 20.ToFix32(),
                    filter: (m, e) => e is IEntityWithLogisticsControl || e is IEntityWithSimpleLogisticsControl)
                .Action(m => {
                    IEntity entity = m.Field.Entity<IEntity>("logistic");
                    if (entity is IEntityWithLogisticsControl logistic)
                    {
                        logistic.SetLogisticsInputMode((EntityLogisticsMode)m.Input["mode", 0].IntegerPart);
                        return ModuleStatus.Running;
                    }
                    else if (entity is IEntityWithSimpleLogisticsControl simple)
                    {
                        simple.SetLogisticsInputDisabled(m.Input["mode", 0].IntegerPart == 2);
                        return ModuleStatus.Running;
                    }
                    else
                    {
                        m.SetError("Building is not connected");
                        return ModuleStatus.Error;
                    }
                })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Connection_Import_Get", "Connection: Import (get)", "IMG", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Connection)
                .AddCategory(Category.ConnectionRead)
                .AddOutput("mode", "Import mode:\n  0 (auto),\n  1 (on),\n  2 (off)")
                .AddEntityField<IStaticEntity>("logistic", "Logistic building", "Any logistic building (20 metres)",
                    distance: 20.ToFix32(),
                    filter: (m, e) => e is IEntityWithLogisticsControl || e is IEntityWithSimpleLogisticsControl)
                .Action(m => {
                    IEntity entity = m.Field.Entity<IEntity>("logistic");
                    if (entity is IEntityWithLogisticsControl logistic)
                    {
                        m.Output["mode"] = ((int)logistic.LogisticsInputMode).ToFix32();
                        return ModuleStatus.Running;
                    }
                    else if (entity is IEntityWithSimpleLogisticsControl simple)
                    {
                        m.Output["mode"] = (simple.IsLogisticsInputDisabled ? 2 : 1).ToFix32();
                        return ModuleStatus.Running;
                    }
                    else
                    {
                        m.SetError("Building is not connected");
                        return ModuleStatus.Error;
                    }
                })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Connection_Export_Set", "Connection: Export (set)", "EXS", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Connection)
                .AddCategory(Category.ConnectionWrite)
                .AddInput("mode", "Export mode:\n  0 (auto),\n  1 (on),\n  2 (off)")
                .AddEntityField<IStaticEntity>("logistic", "Logistic building", "Any logistic building (20 metres)",
                    distance: 20.ToFix32(),
                    filter: (m, e) => e is IEntityWithLogisticsControl || e is IEntityWithSimpleLogisticsControl)
                .Action(m => {
                    IEntity entity = m.Field.Entity<IEntity>("logistic");
                    if (entity is IEntityWithLogisticsControl logistic)
                    {
                        logistic.SetLogisticsOutputMode((EntityLogisticsMode)m.Input["mode", 0].IntegerPart);
                        return ModuleStatus.Running;
                    }
                    else if (entity is IEntityWithSimpleLogisticsControl simple)
                    {
                        simple.SetLogisticsOutputDisabled(m.Input["mode", 0].IntegerPart == 2);
                        return ModuleStatus.Running;
                    }
                    else
                    {
                        m.SetError("Building is not connected");
                        return ModuleStatus.Error;
                    }
                })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Connection_Export_Get", "Connection: Export (get)", "EXG", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Connection)
                .AddCategory(Category.ConnectionRead)
                .AddOutput("mode", "Export mode:\n  0 (auto),\n  1 (on),\n  2 (off)")
                .AddEntityField<IStaticEntity>("logistic", "Logistic building", "Any logistic building (20 metres)",
                    distance: 20.ToFix32(),
                    filter: (m, e) => e is IEntityWithLogisticsControl || e is IEntityWithSimpleLogisticsControl)
                .Action(m => {
                    IEntity entity = m.Field.Entity<IEntity>("logistic");
                    if (entity is IEntityWithLogisticsControl logistic)
                    {
                        m.Output["mode"] = ((int)logistic.LogisticsOutputMode).ToFix32();
                        return ModuleStatus.Running;
                    }
                    else if (entity is IEntityWithSimpleLogisticsControl simple)
                    {
                        m.Output["mode"] = (simple.IsLogisticsOutputDisabled ? 2 : 1).ToFix32();
                        return ModuleStatus.Running;
                    }
                    else
                    {
                        m.SetError("Building is not connected");
                        return ModuleStatus.Error;
                    }
                })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Connection_Priority_Set", "Connection: Priority (set)", "P-S", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Connection)
                .AddCategory(Category.ConnectionWrite)
                .AddInput("priority", "Priority: 1 - 15")
                .AddEntityField<IEntityWithGeneralPriority>("logistic", "Logistic building", "Any logistic building (20 metres)", 20.ToFix32())
                .Action(m => {
                    var logistic = m.Field.Entity<IEntityWithGeneralPriority>("logistic");
                    if (logistic is null)
                    {
                        m.SetError("Building is not connected");
                        return ModuleStatus.Error;
                    }
                    logistic.SetGeneralPriority(m.Input["priority", 8.ToFix32()].IntegerPart);
                    return ModuleStatus.Running;
                })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Connection_Priority_Get", "Connection: Priority (get)", "P-G", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Connection)
                .AddCategory(Category.ConnectionRead)
                .AddOutput("priority", "Priority: 1 - 15")
                .AddEntityField<IEntityWithGeneralPriority>("building", "Building", "Any building with configurable priority (20 metres)", 20.ToFix32())
                .Action(m => {
                    var logistic = m.Field.Entity<IEntityWithGeneralPriority>("building");
                    if (logistic is null)
                    {
                        m.SetError("Building is not connected");
                        return ModuleStatus.Error;
                    }
                    m.Output["priority"] = logistic.GeneralPriority;
                    return ModuleStatus.Running;
                })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Connection_Vehicle_Get", "Connection: Vehicle count (get)", "V-G", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Connection)
                .AddCategory(Category.ConnectionRead)
                .Width(4)
                .AddInput("vehicle", "Vehicle type")
                .AddOutput("count", "Vehicle count")
                .AddEntityField<IEntityAssignedWithVehicles>("building", "Building", "Any building with configurable priority (20 metres)", 20.ToFix32())
                .AddBooleanField("field_vehicle", "Select by settings", defaultValue: false)
                .AddEntityTypeField<DrivingEntityProto>("vehicle", "Vehicle", "Any suppoted vehicle type",
                    filter: (m,p) => m.Field.Entity<Entity>("building") is IEntityAssignedWithVehicles w && w.CanVehicleBeAssigned(p)) // TODO filter by building
                .Action(m => {
                    var logistic = m.Field.Entity<IEntityAssignedWithVehicles>("building");
                    if (logistic is null)
                    {
                        m.SetError("Building is not connected");
                        return ModuleStatus.Error;
                    }
                    DrivingEntityProto drivingEntity = m.FieldOrInput.EntityProtoIconified("vehicle") as DrivingEntityProto;
                    if (drivingEntity is null)
                    {
                        m.Output["count"] = logistic.AllVehicles.Count;
                        return ModuleStatus.Running;
                    }

                    if (!logistic.CanVehicleBeAssigned(drivingEntity))
                    {
                        m.SetError("Invalid vehicle type");
                        return ModuleStatus.Error;
                    }

                    m.Output["count"] = logistic.AllVehicles.Where(v => v.Prototype == drivingEntity).Count();
                    return ModuleStatus.Running;
                })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Connection_Vehicle_Set", "Connection: Vehicle count (set)", "V-S", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Connection)
                .AddCategory(Category.ConnectionRead)
                .Width(4)
                .AddInput("count", "Vehicle count")
                .AddInput("vehicle", "Vehicle type")
                .AddEntityField<IEntityAssignedWithVehicles>("building", "Building", "Any building with configurable priority (20 metres)", 20.ToFix32())
                .AddBooleanField("field_vehicle", "Select type by settings", defaultValue: false)
                .AddEntityTypeField<DrivingEntityProto>("vehicle", "Vehicle", "Any suppoted vehicle type",
                    filter: (m,p) => m.Field.Entity<Entity>("building") is IEntityAssignedWithVehicles w && w.CanVehicleBeAssigned(p)) // TODO filter by building
                .AddBooleanField("field_count", "Set count by settings", defaultValue: false)
                .AddInt32Field("count", "Vehicle count", defaultValue: 0)
                .Action(m => {
                    var logistic = m.Field.Entity<IEntityAssignedWithVehicles>("building");
                    if (logistic is null)
                    {
                        m.SetError("Building is not connected");
                        return ModuleStatus.Error;
                    }

                    DrivingEntityProto drivingEntity = m.FieldOrInput.EntityProtoIconified("vehicle") as DrivingEntityProto;

                    if (drivingEntity is null || !logistic.CanVehicleBeAssigned(drivingEntity))
                    {
                        m.SetError("Invalid vehicle type");
                        return ModuleStatus.Error;
                    }

                    int count = m.FieldOrInput["count", Fix32.Zero].IntegerPart;

                    int actualCount = logistic.AllVehicles.Where(v => v.Prototype == drivingEntity).Count();
                    if (actualCount < count)
                    {
                        logistic.AssignVehicle(GlobalDependencyResolver.Get<IVehiclesManager>(), drivingEntity);
                    }
                    else if (actualCount > count)
                    {
                        Vehicle veh = logistic.AllVehicles.Where(v => v.Prototype == drivingEntity).FirstOrDefault();
                        logistic.UnassignVehicle(veh, cancelJobs: false);
                    }
                    return ModuleStatus.Running;
                })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Connection_Filter_Get", "Connection: Filter (get)", "F-G", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Connection)
                .AddCategory(Category.ConnectionRead)
                .Width(2)
                .AddInput("index", "Storage compartment")
                .AddOutput("product", "Product type")
                .AddEntityField<LayoutEntity>("entity", "Connection device", "Storage connectable by cable 20m from controller", distance: 20.ToFix32(),
                    filter: (m, e) => e is StorageBase || // e is SettlementWasteModule
                                      e is SettlementFoodModule ||
                                      e is Hospital ||
                                      e is SettlementModuleProto ||
                                      e is IVirtualResourceMiningEntity ||
                                      e is Sorter // ||
                                      // e is OreSortingPlant
                    )
                .AddBooleanField("field_index", "Set index by settings", defaultValue: false)
                .AddInt32Field("index", "Storage compartment")
                .Action(m => {
                    LayoutEntity entity = m.Field.Entity<LayoutEntity>("entity");

                    if (entity is StorageBase storage)
                    // entity is SettlementWasteModule
                    {
                        if (storage.StoredProduct.HasValue)
                            m.Output["product"] = Fix32.FromRaw((int)(uint)storage.StoredProduct.Value.SlimId.Value);
                        else
                            m.Output["product"] = Fix32.Zero;
                        return ModuleStatus.Running;
                    }

                    if (entity is SettlementFoodModule foodModule)
                    {
                        ProductProto product = m.Input.Product("product");
                        var buffers = new Func<IProductBuffer>[] {
                            () => foodModule.GetBuffer(0).ValueOrNull,
                            () => foodModule.GetBuffer(1).ValueOrNull
                        };
                        return GetTypeFromBuffer(m, buffers);
        }

                    if (entity is Hospital hospital)
                    {
                        var buffers = new Func<IProductBuffer>[] {
                            () => hospital.GetBuffer(0).ValueOrNull,
                            () => hospital.GetBuffer(1).ValueOrNull
                        };
                        return GetTypeFromBuffer(m, buffers);
                    }

                    if (entity is SettlementServiceModule module)
                    {
                        var buffers = new Func<IProductBuffer>[] {
                            () => (IProductBuffer)module.GetType().GetField("m_inputBuffer", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(module),
                            () => ((Option<IProductBuffer>)module.GetType().GetField("m_inputBuffer", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(module)).ValueOrNull
                        };
                        return GetTypeFromBuffer(m, buffers);
                    }

                    if (entity is IVirtualResourceMiningEntity miner)
                    {
                        m.Output["product"] = Fix32.FromRaw((int)(uint)miner.ProductToMine.SlimId.Value);
                        return ModuleStatus.Running;
                    }

                    if (entity is Sorter sorter)
                    {
                        int index = m.FieldOrInput["index", Fix32.Zero].IntegerPart;
                        m.Output["product"] = Fix32.FromRaw((int)((uint?)sorter.FilteredProducts.Skip(index).FirstOrDefault()?.SlimId.Value ?? 0));
                        return ModuleStatus.Running;
                    }

                    //if (entity is OreSortingPlant oreSorter)
                    //{
                    //    int index = m.FieldOrInput["index", Fix32.Zero].IntegerPart;
                    //    char port = (char)('A' + index);
                    //    var first = oreSorter.ProductsData.Where(d => d.Value.OutputPort == port).FirstOrDefault();
                    //
                    //    m.Output["product"] = first.Value is null ? Fix32.Zero :
                    //        Fix32.FromRaw((int)(uint)first.Value.Buffer.Product.SlimId.Value);
                    //    return ModuleStatus.Running;
                    //}

                    m.Output["product"] = -1;
                    return ModuleStatus.Error;
                })
                .AddControllerDevice()
                .BuildAndAdd();

            //registrator
            //    .ModuleBuilderStart("Connection_Filter_Set", "Connection: Filter (set)", "F-S", Assets.Base.Products.Icons.Vegetables_svg)
            //    .AddCategory(Category.Connection)
            //    .AddCategory(Category.ConnectionRead)
            //    .Width(2)
            //    .AddInput("index", "Storage compartment")
            //    .AddOutput("product", "Product type")
            //    .AddEntityField<LayoutEntity>("entity", "Building with filter", "Connectable by cable 20m from controller", distance: 20.ToFix32(),
            //        filter: (m, e) => e is Storage ||
            //                          e is SettlementFoodModule ||
            //                          e is Hospital ||
            //                          e is Sorter
            //        )
            //    .AddBooleanField("field_index", "Set index by settings", defaultValue: false)
            //    .AddInt32Field("index", "Storage compartment")
            //    .Action(m => {
            //        LayoutEntity entity = m.Field.Entity<LayoutEntity>("entity");
            //
            //        if (entity is Storage storage)
            //        // entity is SettlementWasteModule
            //        {
            //            ProductProto product = m.FieldOrInput.Product("product");
            //            if (!(product is null) && storage.AssignProduct(product))
            //            {
            //                m.Warning = false;
            //                return ModuleStatus.Running;
            //            }
            //            m.Warning = true;
            //            return ModuleStatus.Running;
            //        }
            //
            //        if (entity is SettlementFoodModule foodModule)
            //        {
            //            int index = m.FieldOrInput["index", Fix32.Zero].IntegerPart;
            //            if (index >= 2 || index < 0)
            //            {
            //                m.SetError("Invalid compartment index");
            //                return ModuleStatus.Error;
            //            }
            //
            //            Option<ProductProto> product = m.FieldOrInput.Product("product").SomeOption();
            //            foodModule.SetProduct(product, index, false);
            //            if (product.HasValue || foodModule.GetBuffer(index).HasValue)
            //            {
            //                m.Warning = !(product.Value?.SlimId == foodModule.GetBuffer(index).Value?.Product.SlimId);
            //                return ModuleStatus.Running;
            //            }
            //            else
            //                return ModuleStatus.Running;
            //        }
            //
            //        if (entity is Hospital hospital)
            //        {
            //            int index = m.FieldOrInput["index", Fix32.Zero].IntegerPart;
            //            if (index >= 2 || index < 0)
            //            {
            //                m.SetError("Invalid compartment index");
            //                return ModuleStatus.Error;
            //            }
            //
            //            Option<ProductProto> product = m.FieldOrInput.Product("product").SomeOption();
            //            hospital.SetProduct(product, index, false);
            //            if (product.HasValue || hospital.GetBuffer(index).HasValue)
            //            {
            //                m.Warning = !(product.Value?.SlimId == hospital.GetBuffer(index).Value?.Product.SlimId);
            //                return ModuleStatus.Running;
            //            }
            //            else
            //                return ModuleStatus.Running;
            //        }
            //
            //        if (entity is Sorter sorter)
            //        {
            //            ProductProto newProduct = m.FieldOrInput.Product("product");
            //            if (m.NumberData.TryGetValue("old__product", out int product))
            //            {
            //                if (product != 0 && (newProduct is null || (int)(uint)newProduct.SlimId.Value != product))
            //                { // remove old from set
            //                    ProductProto oldProduct = sorter.FilteredProducts
            //                            .FirstOrDefault(p => (int)(uint)p.SlimId.Value == product);
            //                    if (oldProduct != null)
            //                        sorter.ToggleFilteredProduct(oldProduct);
            //                }
            //            }
            //            if (sorter.FilteredProducts
            //                      .FirstOrDefault(p => p.SlimId == newProduct.SlimId)
            //                      is null)
            //            { // add to set
            //                sorter.ToggleFilteredProduct(newProduct);
            //                m.NumberData["old__product"] = (int)(uint)newProduct.SlimId.Value;
            //            }
            //            return ModuleStatus.Running;
            //        }
            //
            //        m.Output["product"] = -1;
            //        return ModuleStatus.Error;
            //    })
            //    .AddControllerDevice()
            //    .BuildAndAdd();
        }

        private static ModuleStatus GetValueFromBuffers(Module m, ProductProto product, IProductBuffer[] buffers)
        {
            foreach (var buffer in buffers)
            {
                if (buffer is null) continue;
                if (buffer.Product.Id != product.Id) continue;

                return StorageValueFromBuffer(m, product, buffer);
            }

            m.SetError("Invalid product");
            return ModuleStatus.Error;
        }

        private static ModuleStatus GetTypeFromBuffer(Module m, Func<IProductBuffer>[] buffers)
        {
            if (buffers.Length == 1)
            {
                m.Output["product"] = Fix32.FromRaw(buffers[0].Invoke().Product.SlimId.Value);
                return ModuleStatus.Running;
            }

            int index = m.FieldOrInput["index", Fix32.Zero].IntegerPart;
            if (index >= buffers.Length || index < 0)
            {
                m.SetError("Invalid compartment index");
                return ModuleStatus.Error;
            }

            m.Output["product"] = Fix32.FromRaw(buffers[0].Invoke()?.Product.SlimId.Value ?? 0);
            return ModuleStatus.Running;
        }

        private static ModuleStatus StorageValueFromBuffer(Module m, ProductProto product, IProductBuffer buffer)
        {
            m.Output["quantity"] = buffer.Quantity.Value;
            m.Output["capacity"] = buffer.Capacity.Value;
            m.Output["fullness"] = (int)(100f * buffer.Quantity.Value / buffer.Capacity.Value);
            m.Output["product"] = Fix32.FromRaw(product.SlimId.Value);
            return ModuleStatus.Running;
        }

        private static bool FarmProductFilter(Module m, ProductProto product)
        {
            return m.Context.ProtosDb.First<CropProto>(crop => !crop.IsEmptyCrop && crop.ProductProduced.Product.SlimId == product.SlimId).HasValue;
        }

        private void Comparation(ProtoRegistrator registrator)
        {
            registrator
                .ModuleBuilderStart("Compare_Int_Equal", "Compare: A = B", "A=B", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Boolean)
                .AddCategory(Category.Arithmetic)
                .AddInput("a", "A")
                .AddInput("b", "B")
                .AddBooleanField("field_b", "Use direct constant")
                .AddFix32Field("b", "B")
                .AddOutput("c", "C")
                .Action(m => { m.Output["c"] = m.Input["a", 0] == m.FieldOrInput["b", 0] ? 1 : 0; })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Compare_Int_Greater", "Compare: A > B", "A>B", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Boolean)
                .AddCategory(Category.Arithmetic)
                .AddInput("a", "A")
                .AddInput("b", "B")
                .AddBooleanField("field_b", "Use direct constant")
                .AddFix32Field("b", "B")
                .AddOutput("c", "C")
                .Action(m => { m.Output["c"] = m.Input["a", 0] > m.FieldOrInput["b", 0] ? 1 : 0; })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Compare_Int_Lower", "Compare: A < B", "A<B", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Boolean)
                .AddCategory(Category.Arithmetic)
                .AddInput("a", "A")
                .AddInput("b", "B")
                .AddBooleanField("field_b", "Use direct constant")
                .AddFix32Field("b", "B")
                .AddOutput("c", "C")
                .Action(m => { m.Output["c"] = m.Input["a", 0] < m.FieldOrInput["b", 0] ? 1 : 0; })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Compare_Int_GreaterOrEqual", "Compare: A ≥ B", "A≥B", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Boolean)
                .AddCategory(Category.Arithmetic)
                .AddInput("a", "A")
                .AddInput("b", "B")
                .AddBooleanField("field_b", "Use direct constant")
                .AddFix32Field("b", "B")
                .AddOutput("c", "C")
                .Action(m => { m.Output["c"] = m.Input["a", 0] >= m.FieldOrInput["b", 0] ? 1 : 0; })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Compare_Int_LowerOrEqual", "Compare: A ≤ B", "A≤B", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Boolean)
                .AddCategory(Category.Arithmetic)
                .AddInput("a", "A")
                .AddInput("b", "B")
                .AddBooleanField("field_b", "Use direct constant")
                .AddFix32Field("b", "B")
                .AddOutput("c", "C")
                .Action(m => { m.Output["c"] = m.Input["a", 0] <= m.FieldOrInput["b", 0] ? 1 : 0; })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Compare_Int_Max", "Maximum: A or B", "MAX", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Boolean)
                .AddCategory(Category.Arithmetic)
                .AddCategory(Category.Decision)
                .AddCategory(Category.Control)
                .AddInput("a", "A")
                .AddInput("b", "B")
                .AddBooleanField("field_b", "Use direct constant")
                .AddFix32Field("b", "B")
                .AddOutput("b", "Low")
                .AddOutput("a", "High")
                .Action(m => {
                    Fix32 a = m.Input["a", 0];
                    Fix32 b = m.FieldOrInput["b", 0];
                    if (a > b)
                    {
                        m.Output["a"] = a;
                        m.Output["b"] = b;
                    }
                    else
                    {
                        m.Output["a"] = b;
                        m.Output["b"] = a;
                    }
                
                })
                .AddControllerDevice()
                .BuildAndAdd();
        }

        private void Display(ProtoRegistrator registrator)
        {
            // TODO add display
            // display float values
            Action<Module> ModuleFunction(int digits)
            {
                return (Module m) =>
                {
                    Fix32 value = m.Input["a"];
                    int floating = Math.Min(m.Field["float", 0].IntegerPart, digits);
                    int inting = Math.Max(Math.Min(digits - floating, digits), 0);

                    string full = inting > 0 ? value.IntegerPart.ToString($"D{inting}") : "";
                    string fract = floating > 0 ? (value.FractionalPartNonNegative * Math.Pow(10, floating).ToFix32())
                                            .IntegerPart.ToString($"D{floating}") : "";

                    m.Display["a"] = $"{full}|{fract}";
                };
            }
            foreach (int i in new int[] { 2, 4, 8, 16 })
            {
                registrator
                    .ModuleBuilderStart($"Display_Int_{i}", $"Display: {i*2} digits", $"F-{i}", Assets.Base.Products.Icons.Vegetables_svg)
                    .AddCategory(Category.Display)
                    .AddInput("a", "A")
                    .AddDisplay("a", "A", i)
                    .AddInt32Field("float", "Floating numbers", "Ammount of numbers displayed from fractional part", defaultValue: 0)
                    .AddControllerDevice()
                    // dynamic
                    .Action(ModuleFunction(i * 2))
                    .BuildAndAdd();
            }

            registrator
                .ModuleBuilderStart($"Display_Product", $"Display: product", $"F-P", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Display)
                .AddInput("a", "Product")
                .AddDisplay("a", "Product", 1, image: true)
                .AddControllerDevice()
                .Action(m => m.Display["a"] = m.Input.Product("a")?.IconPath)
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart($"Display_Entity", $"Display: entity", $"F-E", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Display)
                .AddInput("a", "Entity")
                .AddDisplay("a", "Entity", 1, image: true)
                .AddControllerDevice()
                .Action(m => m.Display["a"] = m.Input.EntityProtoIconified("a")?.IconPath)
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart($"Display_Bool", $"Display: LED", $"F-B", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Display)
                .AddInput("a", "Product")
                .AddDisplay("a", "Product", 1, toggle: new string[0])
                .AddControllerDevice()
                .Action(m => m.Display["a"] = m.Input["a", 0] > 0 ? "1" : "")
                .BuildAndAdd();
        }

        private void RadioFM(ProtoRegistrator registrator)
        {
            Action<Module> ReadSignals(int digits)
            {
                return (Module m) =>
                {
                    Antena entity = m.Field.Entity<Antena>("antena");
                    if (!(entity?.DataBand is FMDataBand fm))
                    // TODO generate noise or read data
                    {
                        for (int i = 0; i < digits; i++)
                        {
                            m.Output[names[i]] = 0;
                        }
                        m.Display["fm"] = "";
                    }
                    else
                    {
                        int value = m.Field["fm", 0].IntegerPart;
                        Fix32 displayValue = (171 + value).ToFix32() * 0.5f.ToFix32();
                        m.Display["fm"] = displayValue.ToStringRounded(1) + (digits > 4 ? " kHz" : "");

                        if (entity.IsPaused)
                        {
                            for (int i = 0; i < digits; i++)
                            {
                                m.Output[names[i]] = 0;
                            }
                        }
                        else
                        {
                            Fix32[] signals = fm.Read(m.Field["fm", 0].IntegerPart);
                            int minCount = Math.Min(signals.Length, digits);
                            for (int i = 0; i < minCount; i++)
                            {
                                m.Output[names[i]] = signals[i];
                            }
                            for (int i = minCount; i < digits; i++)
                            {
                                m.Output[names[i]] = 0;
                            }
                        }
                    }
                };
            }
            foreach (int i in new int[] { 2, 4, 8, 16 })
            {
                var module = registrator
                    .ModuleBuilderStart($"Radio_In_FM_{i}", $"FM receiver ({i} signals)", $"FM-R", Assets.Base.Products.Icons.Vegetables_svg)
                    .AddCategory(Category.Antene)
                    .AddCategory(Category.AnteneFM)
                    .AddCustomField("fm", "FM", "Listening frequency", () => 20, (CustomField field) => {
                        field.Builder.NewBtnGeneral("NvaluekHz")
                            .SetText(((171 + field.Reference.Value.IntegerPart).ToFix32() * 0.5f.ToFix32()).ToStringRounded(1) + " kHz")
                            .SetSize(60, 20)
                            .ToolTip(field.Inspector, field.ShortDesc, attached: true)
                            .AppendTo(field.Container);
                        field.Builder.NewBtnGeneral("NstartkHz")
                            .SetText("|<")
                            .OnClick(() =>
                            {
                                field.Reference.Value = 0;
                                field.Refresh();
                            })
                            .SetSize(20, 20)
                            .AppendTo(field.Container);
                        field.Builder.NewBtnGeneral("N-5kHz")
                            .SetText("<<")
                            .OnClick(() =>
                            {
                                field.Reference.Value -= 10;
                                if (field.Reference.Value < 0)
                                    field.Reference.Value += 46;
                                field.Refresh();
                            })
                            .SetSize(20, 20)
                            .AppendTo(field.Container);

                        field.Builder.NewBtnGeneral("N-0.5kHz")
                            .SetText("<")
                            .OnClick(() =>
                            {
                                field.Reference.Value -= 1;
                                if (field.Reference.Value < 0)
                                    field.Reference.Value += 46;
                                field.Refresh();
                            })
                            .SetSize(20, 20)
                            .AppendTo(field.Container);

                        field.Builder.NewBtnGeneral("N+0.5kHz")
                            .SetText(">")
                            .OnClick(() =>
                            {
                                field.Reference.Value += 1;
                                if (field.Reference.Value > 45)
                                    field.Reference.Value -= 46;
                                field.Refresh();
                            })
                            .SetSize(20, 20)
                            .AppendTo(field.Container);

                        field.Builder.NewBtnGeneral("N+5kHz")
                            .SetText(">>")
                            .OnClick(() =>
                            {
                                field.Reference.Value += 10;
                                if (field.Reference.Value > 45)
                                    field.Reference.Value -= 46;
                                field.Refresh();
                            })
                            .SetSize(20, 20)
                            .AppendTo(field.Container);

                        field.Builder.NewBtnGeneral("NendkHz")
                            .SetText(">|")
                            .OnClick(() =>
                            {
                                field.Reference.Value = 45;
                                field.Refresh();
                            })
                            .SetSize(20, 20)
                            .AppendTo(field.Container);
                    })
                    .AddEntityField<Antena>("antena", "Antena", distance: 5.ToFix32())
                    .AddDisplay("fm", "Frequency", i)
                    .AddControllerDevice()
                    // dynamic
                    .Action(ReadSignals(i));

                for (int j = 0; j < i; j++)
                {
                    module.AddOutput(names[j], names[j].ToUpper());
                }

                module.BuildAndAdd();
            }

            Action<Module> WriteSignals(int digits)
            {
                return (Module m) =>
                {
                    Antena entity = m.Field.Entity<Antena>("antena");
                    if (entity.DataBand is FMDataBand fm && !entity.IsPaused)
                    {
                        int value = m.Field["fm", 0].IntegerPart;
                        Fix32 displayValue = (171 + value).ToFix32() * 0.5f.ToFix32();
                        m.Display["fm"] = displayValue.ToStringRounded(1) + (digits > 4 ? " kHz" : "");

                        if (!entity.IsPaused)
                        {
                            Fix32[] signals = new Fix32[digits];
                            for (int i = 0; i < digits; i++)
                            {
                                signals[i] = m.Input[names[i], 0];
                            }
                            fm.Update(m.Field["fm", 0].IntegerPart, signals);
                        }
                    }
                };
            }
            foreach (int i in new int[] { 2, 4, 8, 16 })
            {
                var module = registrator
                    .ModuleBuilderStart($"Radio_Out_FM_{i}", $"FM broadcaster ({i} signals)", $"FM-B", Assets.Base.Products.Icons.Vegetables_svg)
                    .AddCategory(Category.Antene)
                    .AddCategory(Category.AnteneFM)
                    .AddCustomField("fm", "FM", "Broadcasting frequency", () => 20, (field) => {
                        field.Builder.NewBtnGeneral("NvaluekHz")
                            .SetText(((171 + field.Reference.Value.IntegerPart).ToFix32() * 0.5f.ToFix32()).ToStringRounded(1) + " kHz")
                            .SetSize(60, 20)
                            .ToolTip(field.Inspector, field.ShortDesc, attached: true)
                            .AppendTo(field.Container);
                        field.Builder.NewBtnGeneral("NstartkHz")
                            .SetText("|<")
                            .OnClick(() =>
                            {
                                field.Reference.Value = 0;
                                field.Refresh();
                            })
                            .SetSize(20, 20)
                            .AppendTo(field.Container);
                        field.Builder.NewBtnGeneral("N-5kHz")
                            .SetText("<<")
                            .OnClick(() =>
                            {
                                field.Reference.Value -= 10;
                                if (field.Reference.Value < 0)
                                    field.Reference.Value += 46;
                                field.Refresh();
                            })
                            .SetSize(20, 20)
                            .AppendTo(field.Container);

                        field.Builder.NewBtnGeneral("N-0.5kHz")
                            .SetText("<")
                            .OnClick(() =>
                            {
                                field.Reference.Value -= 1;
                                if (field.Reference.Value < 0)
                                    field.Reference.Value += 46;
                                field.Refresh();
                            })
                            .SetSize(20, 20)
                            .AppendTo(field.Container);

                        field.Builder.NewBtnGeneral("N+0.5kHz")
                            .SetText(">")
                            .OnClick(() =>
                            {
                                field.Reference.Value += 1;
                                if (field.Reference.Value > 45)
                                    field.Reference.Value -= 46;
                                field.Refresh();
                            })
                            .SetSize(20, 20)
                            .AppendTo(field.Container);

                        field.Builder.NewBtnGeneral("N+5kHz")
                            .SetText(">>")
                            .OnClick(() =>
                            {
                                field.Reference.Value += 10;
                                if (field.Reference.Value > 45)
                                    field.Reference.Value -= 46;
                                field.Refresh();
                            })
                            .SetSize(20, 20)
                            .AppendTo(field.Container);

                        field.Builder.NewBtnGeneral("NendkHz")
                            .SetText(">|")
                            .OnClick(() =>
                            {
                                field.Reference.Value = 45;
                                field.Refresh();
                            })
                            .SetSize(20, 20)
                            .AppendTo(field.Container);
                    })
                    .AddEntityField<Antena>("antena", "Antena", distance: 5.ToFix32())
                    .AddDisplay("fm", "Frequency", i)
                    .AddControllerDevice()
                    // dynamic
                    .Action(WriteSignals(i));

                for (int j = 0; j < i; j++)
                {
                    module.AddInput(names[j], names[j].ToUpper());
                }

                module.BuildAndAdd();
            }
        }

        private void RadioAM(ProtoRegistrator registrator)
        {
            registrator
                .ModuleBuilderStart($"Radio_In_AM", $"AM receiver", $"AM-R", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Antene)
                .AddCategory(Category.AnteneAM)
                .AddCustomField("am", "AM", "Listening frequency", () => 20, (CustomField field) => {
                    field.Builder.NewBtnGeneral("NvaluekHz")
                        .SetText(((53 + field.Reference.Value.IntegerPart).ToFix32() * 10.ToFix32()).ToStringRounded(0))
                        .SetSize(60, 20)
                        .ToolTip(field.Inspector, field.ShortDesc, attached: true)
                        .AppendTo(field.Container);
                    field.Builder.NewBtnGeneral("NstartkHz")
                        .SetText("|<")
                        .OnClick(() =>
                        {
                            field.Reference.Value = 0;
                            field.Refresh();
                        })
                        .SetSize(20, 20)
                        .AppendTo(field.Container);
                    field.Builder.NewBtnGeneral("N-5kHz")
                        .SetText("<<")
                        .OnClick(() =>
                        {
                            field.Reference.Value -= 10;
                            if (field.Reference.Value < 0)
                                field.Reference.Value += 118;
                            field.Refresh();
                        })
                        .SetSize(20, 20)
                        .AppendTo(field.Container);

                    field.Builder.NewBtnGeneral("N-0.5kHz")
                        .SetText("<")
                        .OnClick(() =>
                        {
                            field.Reference.Value -= 1;
                            if (field.Reference.Value < 0)
                                field.Reference.Value += 118;
                            field.Refresh();
                        })
                        .SetSize(20, 20)
                        .AppendTo(field.Container);

                    field.Builder.NewBtnGeneral("N+0.5kHz")
                        .SetText(">")
                        .OnClick(() =>
                        {
                            field.Reference.Value += 1;
                            if (field.Reference.Value > 117)
                                field.Reference.Value -= 118;
                            field.Refresh();
                        })
                        .SetSize(20, 20)
                        .AppendTo(field.Container);

                    field.Builder.NewBtnGeneral("N+5kHz")
                        .SetText(">>")
                        .OnClick(() =>
                        {
                            field.Reference.Value += 10;
                            if (field.Reference.Value > 117)
                                field.Reference.Value -= 118;
                            field.Refresh();
                        })
                        .SetSize(20, 20)
                        .AppendTo(field.Container);

                    field.Builder.NewBtnGeneral("NendkHz")
                        .SetText(">|")
                        .OnClick(() =>
                        {
                            field.Reference.Value = 117;
                            field.Refresh();
                        })
                        .SetSize(20, 20)
                        .AppendTo(field.Container);
                })
                .AddEntityField<Antena>("antena", "Antena", distance: 5.ToFix32())
                .AddDisplay("am", "Frequency", 2)
                .AddOutput("am", "AM signal")
                .AddControllerDevice()
                // dynamic
                .Action((Module m) =>
                {
                    Antena entity = m.Field.Entity<Antena>("antena");
                    if ((entity?.DataBand is AMDataBand am))
                    // TODO generate noise or read data
                    {
                        m.Output["am"] = am.Read(m.Field["am", Fix32.Zero].IntegerPart, Fix32.Zero);

                        // signal value
                        int value = m.Field["am", 0].IntegerPart;
                        Fix32 displayValue = (53 + value).ToFix32() * 10f.ToFix32();
                        m.Display["am"] = displayValue.ToStringRounded(0);
                    }
                    else
                    {
                        m.Output["am"] = Fix32.Zero;
                        m.Display["am"] = "";
                    }
                })
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart($"Radio_Out_AM", $"AM broadcaster", $"AM-B", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Antene)
                .AddCategory(Category.AnteneAM)
                .AddCustomField("am", "AM", "Broadcasting frequency", () => 20, (field) => {
                    field.Builder.NewBtnGeneral("NvaluekHz")
                        .SetText(((53 + field.Reference.Value.IntegerPart).ToFix32() * 10.ToFix32()).ToStringRounded(0))
                        .SetSize(60, 20)
                        .ToolTip(field.Inspector, field.ShortDesc, attached: true)
                        .AppendTo(field.Container);
                    field.Builder.NewBtnGeneral("NstartkHz")
                        .SetText("|<")
                        .OnClick(() =>
                        {
                            field.Reference.Value = 0;
                            field.Refresh();
                        })
                        .SetSize(20, 20)
                        .AppendTo(field.Container);
                    field.Builder.NewBtnGeneral("N-5kHz")
                        .SetText("<<")
                        .OnClick(() =>
                        {
                            field.Reference.Value -= 10;
                            if (field.Reference.Value < 0)
                                field.Reference.Value += 118;
                            field.Refresh();
                        })
                        .SetSize(20, 20)
                        .AppendTo(field.Container);

                    field.Builder.NewBtnGeneral("N-0.5kHz")
                        .SetText("<")
                        .OnClick(() =>
                        {
                            field.Reference.Value -= 1;
                            if (field.Reference.Value < 0)
                                field.Reference.Value += 118;
                            field.Refresh();
                        })
                        .SetSize(20, 20)
                        .AppendTo(field.Container);

                    field.Builder.NewBtnGeneral("N+0.5kHz")
                        .SetText(">")
                        .OnClick(() =>
                        {
                            field.Reference.Value += 1;
                            if (field.Reference.Value > 117)
                                field.Reference.Value -= 118;
                            field.Refresh();
                        })
                        .SetSize(20, 20)
                        .AppendTo(field.Container);

                    field.Builder.NewBtnGeneral("N+5kHz")
                        .SetText(">>")
                        .OnClick(() =>
                        {
                            field.Reference.Value += 10;
                            if (field.Reference.Value > 117)
                                field.Reference.Value -= 118;
                            field.Refresh();
                        })
                        .SetSize(20, 20)
                        .AppendTo(field.Container);

                    field.Builder.NewBtnGeneral("NendkHz")
                        .SetText(">|")
                        .OnClick(() =>
                        {
                            field.Reference.Value = 117;
                            field.Refresh();
                        })
                        .SetSize(20, 20)
                        .AppendTo(field.Container);
                })
                .AddEntityField<Antena>("antena", "Antena", distance: 5.ToFix32())
                .AddDisplay("am", "Frequency", 2)
                .AddInput("am", "AM signal")
                .AddControllerDevice()
                // dynamic
                .Action((Module m) =>
                {
                    Antena entity = m.Field.Entity<Antena>("antena");
                    if (entity.DataBand is AMDataBand am && !entity.IsPaused)
                    {
                        int value = m.Field["am", 0].IntegerPart;
                        Fix32 displayValue = (53 + value).ToFix32() * 10.ToFix32();
                        m.Display["am"] = displayValue.ToStringRounded(0);

                        if (!entity.IsPaused)
                        {
                            am.Update(m.Field["am", 0].IntegerPart, m.Input["am", Fix32.Zero]);
                        }
                    }
                })
                .BuildAndAdd();
        }
    }
}
