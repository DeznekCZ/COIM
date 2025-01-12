using Mafi;
using Mafi.Base;
using Mafi.Core.Buildings.Settlements;
using Mafi.Core.Buildings.Storages;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.ElectricPower;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Mods;
using Mafi.Core.Population;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Components;
using RTG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramableNetwork
{
    internal class Modules : AValidatedData
    {
        static readonly string[] names = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "m", "n", "o", "p", "q" };

        protected override void RegisterDataInternal(ProtoRegistrator registrator)
        {
            registrator
                .ModuleBuilderStart("Constant", "Constant", "#", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Arithmetic)
                .AddCategory(Category.Boolean)
                .AddOutput("value", "Value")
                .AddInt32Field("number", "Number")
                .Action(m => { m.Output["value"] = m.Field["number", 0]; })
                .AddControllerDevice()
                .BuildAndAdd();

            Arithmetic(registrator);
            Comparation(registrator);
            Connections(registrator);
            Stats(registrator);
            Forks(registrator);
            Booleans(registrator);
            Decisions(registrator);
            Display(registrator);
            Radio(registrator);
        }

        private static void Arithmetic(ProtoRegistrator registrator)
        {
            registrator
                .ModuleBuilderStart("Sum", "C = A + B", "A+B", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Arithmetic)
                .AddInput("a", "A")
                .AddInput("b", "B")
                .AddOutput("c", "C")
                .Action(m => { m.Output["c"] = m.Input["a", 0] + m.Input["b", 0]; })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Sub", "C = A - B", "A-B", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Arithmetic)
                .AddInput("a", "A")
                .AddInput("b", "B")
                .AddOutput("c", "C")
                .Action(m => { m.Output["c"] = m.Input["a", 0] - m.Input["b", 0]; })
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
                .ModuleBuilderStart("Average", "Average", "~A", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Arithmetic)
                .AddInput("input", "Input")
                .AddOutput("count", "Average")
                .AddOutput("average", "Average")
                .AddInt32Field("count", "Count", "Maximum number of values to be counted in average with default: 10", 10)
                .Action(m => {
                    int desiredCount = m.Field["count", 1];
                    if (desiredCount < 1) desiredCount = 1;

                    Fix32 input = m.Input["input", 0].ToFix32();
                    Fix32 oldCount = (m.Output["count", 1] - 1).ToFix32();
                    Fix32 average = (m.Output["average", 0].ToFix32() * oldCount) + input;

                    m.Output["count"] = Math.Min(desiredCount, m.Output["count", 0] + 1);
                    m.Output["average"] = (average / m.Output["count"].ToFix32()).IntegerPart;
                })
                .AddControllerDevice()
                .BuildAndAdd();
        }

        private void Stats(ProtoRegistrator registrator)
        {
            registrator
                .ModuleBuilderStart("Stats_Unity", "Statistic: Unity", "UNI", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Stats)
                .AddOutput("v", "Unity value")
                // ADD behind settings
                .Action(m =>
                {
                    m.Output["v"] = m.Context.UpointsManager.Quantity.Value;
                })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Stats_Workers", "Statistic: Workers", "WRK", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Stats)
                .AddOutput("u", "Used workers")
                .AddOutput("a", "Available workers")
                .AddOutput("m", "Missing workers")
                .AddOutput("t", "Total workers")
                // ADD behind settings
                .Action(m =>
                {
                    m.Output["a"] = Math.Max(0, m.Context.WorkersManager.AmountOfFreeWorkersOrMissing);
                    m.Output["t"] = (int)(m.Context.WorkersManager as WorkersManager).TotalWorkersNeededStats.ThisYear + m.Output["a"];
                    m.Output["m"] = 0-Math.Min(0, m.Context.WorkersManager.AmountOfFreeWorkersOrMissing);
                    m.Output["u"] = m.Output["t", 0] - m.Output["a", 0];
                })
                .AddControllerDevice()
                .BuildAndAdd();

            // TODO: read electricity
            //registrator
            //    .ModuleBuilderStart("Stats_Electricity", "Statistic: Electricity", "PWR", Assets.Base.Products.Icons.Vegetables_svg)
            //    .AddCategory(Category.Stats)
            //    .AddOutput("u", "Used workers")
            //    .AddOutput("a", "Available workers")
            //    .AddOutput("m", "Missing workers")
            //    .AddOutput("t", "Total workers")
            //    // ADD behind settings or connect to power network
            //    .Action(m =>
            //    {
            //        new ElectricityManager(m, m, m, m, m, m);
            //    })
            //    .AddControllerDevice()
            //    .BuildAndAdd();
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
                    int index = m.Input["index", int.MaxValue];
                    int digits = count * 2;
                    string text = index.ToString($"D{digits}");
                    m.Display["index"] = text.Length > digits ? text.Substring(text.Length - digits) : text;

                    for (int i = 0; i < count; i++)
                    {
                        string name = names[i];
                        int value = m.Field[name, 0];
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
                    .ModuleBuilderStart($"Decision_Select_{i}", $"Select ({i-1} pins)", $"SEL-{i-1}", Assets.Base.Products.Icons.Vegetables_svg)
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
                    builder.AddInt32Field(names[j], names[j].ToUpper() + ": Index ≤", j);

                builder.AddInput("else", "Else");
                builder.BuildAndAdd();
            }
        }

        private static void Connections(ProtoRegistrator registrator)
        {
            registrator
                .ModuleBuilderStart("Connection_Controller_Input", "Connection: Controller (4 pin, input)", "C-IN", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Connection)
                .AddOutput("a", "A")
                .AddOutput("b", "B")
                .AddOutput("c", "C")
                .AddOutput("d", "D")
                .AddEntityField<Controller>("controller", "Connection device", "Name of output module, which must exist in target Controller", 20.ToFix32())
                .AddStringField("name", "Output Name", "C")
                .Action(m =>
                {
                    //Mafi.Log.Info("Update of input");
                    int entityId = m.Field["controller", 0];
                    string moduleType = "Connection_Controller_Output".ModuleId();
                    string noduleName = m.Field["name", ""];
                    if (noduleName.Length > 0 && m.Context.EntitiesManager
                            .TryGetEntity(new Mafi.Core.EntityId(entityId), out Controller controller))
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
                .AddInput("a", "A")
                .AddInput("b", "B")
                .AddInput("c", "C")
                .AddInput("d", "D")
                .AddStringField("name", "Name", "Name of output module, which must be selected in target Controller", "C")
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Connection_SwitchOff", "Connection: Switch Off", "S", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Connection)
                .AddCategory(Category.Command)
                .AddInput("pause", "Pause")
                .AddEntityField<StaticEntity>("entity", "Connection device", "Any pausable building connectable by cable 20m from controller", 20.ToFix32())
                .Action(m =>
                {
                    int entityId = m.Field["entity", 0];
                    int input = m.Input["pause", 0];
                    if (m.Context.EntitiesManager
                            .TryGetEntity(new Mafi.Core.EntityId(entityId), out StaticEntity entity)
                        && entity.CanBePaused)
                    {
                        entity.SetPaused(input > 0);
                        m.StatusIn["pause"] = ModuleStatus.Running;
                        return;
                    }
                    m.StatusIn["pause"] = ModuleStatus.Error;
                })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Connection_Storage", "Connection: Storage (input)", "STOCK", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Connection)
                .AddOutput("quantity", "Quantity")
                .AddOutput("capacity", "Capacity")
                .AddOutput("fullness", "Fullness in %")
                .AddOutput("product", "Product in #")
                .AddEntityField<StorageBase>("entity", "Connection device", "Storage connectable by cable 20m from controller", 20.ToFix32())
                // .AddInt32Field("buffer", "Storage slot (0-based)", 0)
                // TODO add filter input field
                .Action(m =>
                {
                    int entityId = m.Field["entity", 0];
                    int buffer = m.Input["buffer", 0];

                    if (m.Context.EntitiesManager
                            .TryGetEntity(new Mafi.Core.EntityId(entityId), out StorageBase entity))
                    {
                        m.Output["quantity"] = entity.CurrentQuantity.Value;
                        m.Output["capacity"] = entity.Capacity.Value;
                        m.Output["fullness"] = (int)(100f * entity.CurrentQuantity.Value / entity.Capacity.Value);

                        if (entity.StoredProduct.HasValue)
                        {
                            m.Output["product"] = (int)(uint)entity.StoredProduct.Value.SlimId.Value;
                        }
                        else
                        {
                            m.Output["product"] = -1;
                        }
                        return;
                    }

                    m.Output["quantity"] = 0;
                    m.Output["capacity"] = 0;
                    m.Output["fullness"] = 100;
                    m.Output["product"] = -1;
                })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Connection_Transport", "Connection: Transport (input)", "TRANS", Assets.Base.Products.Icons.Vegetables_svg, "Transport connectable by cable 20m from controller")
                .AddCategory(Category.Connection)
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
                    int entityId = m.Field["entity", 0];
                    int buffer = m.Input["buffer", 0];
                    int filterId = m.Input["product", 0];
                    bool fullstack = m.Field["fullstack", 0] > 0;

                    if (m.Context.EntitiesManager
                            .TryGetEntity(new Mafi.Core.EntityId(entityId), out Transport entity))
                    {
                        m.Output["quantity"] = entity.TransportedProducts
                                            .Where(p => filterId == 0 || p.SlimId.Value == filterId)
                                            .Select(p => p.Quantity.Value).Sum();
                        m.Output["capacity"] = entity.Trajectory.MaxProducts
                                             * (fullstack ? entity.Prototype.MaxQuantityPerTransportedProduct.Value : 1);
                        m.Output["fullness"] = (int)(100f * m.Output["quantity"] / m.Output["capacity"]);
                        m.Output["moving"] = entity.GetStatus() == Transport.Status.Moving ? 1 : 0;
                        return;
                    }

                    m.Output["quantity"] = 0;
                    m.Output["capacity"] = 0;
                    m.Output["fullness"] = 100;
                    m.Output["moving"] = 0;
                })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Connection_Settlement", "Connection: Settlement (population)", "SETTLE", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Connection)
                .AddCategory(Category.Stats)
                .AddOutput("pop_this", "Population (settlement module)")
                .AddOutput("pop_nearby", "Population (settlement total)")
                .AddInput("product", "First Product in #")
                .AddEntityField<SettlementHousingModule>("entity", "Connection device", 20.ToFix32())
                // TODO add filter input field
                .Action(m =>
                {
                    int entityId = m.Field["entity", 0];
                    int buffer = m.Input["buffer", 0];
                    int filterId = m.Input["product", 0];

                    if (m.Context.EntitiesManager
                            .TryGetEntity(new Mafi.Core.EntityId(entityId), out SettlementHousingModule entity))
                    {
                        m.Output["pop_this"] = entity.Population;
                        m.Output["pop_nearby"] = entity.Settlement.ValueOrNull?.Population ?? entity.Population;
                        return;
                    }

                    m.Output["quantity"] = 0;
                    m.Output["capacity"] = 0;
                    m.Output["fullness"] = 100;
                    m.Output["product"] = -1;
                })
                .AddControllerDevice()
                .BuildAndAdd();
        }

        private void Comparation(ProtoRegistrator registrator)
        {
            registrator
                .ModuleBuilderStart("Compare_Int_Equal", "Compare: A = B", "A=B", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Boolean)
                .AddCategory(Category.Arithmetic)
                .AddInput("a", "A")
                .AddInput("b", "B")
                .AddOutput("c", "C")
                .Action(m => { m.Output["c"] = m.Input["a", 0] == m.Input["b", 0] ? 1 : 0; })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Compare_Int_Greater", "Compare: A > B", "A>B", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Boolean)
                .AddCategory(Category.Arithmetic)
                .AddInput("a", "A")
                .AddInput("b", "B")
                .AddOutput("c", "C")
                .Action(m => { m.Output["c"] = m.Input["a", 0] > m.Input["b", 0] ? 1 : 0; })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Compare_Int_Lower", "Compare: A < B", "A<B", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Boolean)
                .AddCategory(Category.Arithmetic)
                .AddInput("a", "A")
                .AddInput("b", "B")
                .AddOutput("c", "C")
                .Action(m => { m.Output["c"] = m.Input["a", 0] < m.Input["b", 0] ? 1 : 0; })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Compare_Int_GreaterOrEqual", "Compare: A ≥ B", "A≥B", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Boolean)
                .AddCategory(Category.Arithmetic)
                .AddInput("a", "A")
                .AddInput("b", "B")
                .AddOutput("c", "C")
                .Action(m => { m.Output["c"] = m.Input["a", 0] >= m.Input["b", 0] ? 1 : 0; })
                .AddControllerDevice()
                .BuildAndAdd();

            registrator
                .ModuleBuilderStart("Compare_Int_LowerOrEqual", "Compare: A ≤ B", "A≤B", Assets.Base.Products.Icons.Vegetables_svg)
                .AddCategory(Category.Boolean)
                .AddCategory(Category.Arithmetic)
                .AddInput("a", "A")
                .AddInput("b", "B")
                .AddOutput("c", "C")
                .Action(m => { m.Output["c"] = m.Input["a", 0] <= m.Input["b", 0] ? 1 : 0; })
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
                .AddOutput("a", "A")
                .AddOutput("b", "B")
                .Action(m => {
                    int a = m.Input["a", 0];
                    int b = m.Input["b", 0];
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
                    int value = m.Input["a"];
                    string text = value.ToString($"D{digits}");
                    m.Display["a"] = text.Length > digits ? text.Substring(text.Length - digits) : text;
                };
            }
            foreach (int i in new int[] { 2, 4, 8, 16 })
            {
                registrator
                    .ModuleBuilderStart($"Display_Int_{i}", $"Display: {i*2} digits", $"F-{i}", Assets.Base.Products.Icons.Vegetables_svg)
                    .AddCategory(Category.Display)
                    .AddInput("a", "A")
                    .AddDisplay("a", "A", i)
                    .AddControllerDevice()
                    // dynamic
                    .Action(ModuleFunction(i * 2))
                    .BuildAndAdd();
            }
        }

        private void Radio(ProtoRegistrator registrator)
        {
            Action<Module> ReadSignals(int digits)
            {
                return (Module m) =>
                {
                    int entityId = m.Field["antena", 0];
                    if (!m.Context.EntitiesManager
                            .TryGetEntity(new Mafi.Core.EntityId(entityId), out Antena entity) ||
                            !(entity.DataBand is FMDataBand fm))
                    // TODO generate noise or read data
                    {
                        for (int i = 0; i < digits; i++)
                        {
                            m.Output[names[i]] = 0;
                        }
                    }
                    else
                    {
                        int[] signals = fm.Read(m.Field["fm", 0]);
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

                    int value = m.Input["a"];
                    string text = value.ToString($"D{digits}");
                    m.Display["a"] = text.Length > digits ? text.Substring(text.Length - digits) : text;
                };
            }
            foreach (int i in new int[] { 2, 4, 8, 16 })
            {
                var module = registrator
                    .ModuleBuilderStart($"Radio_In_FM_{i}", $"FM receiver ({i} signals)", $"FM\ni-{i}", Assets.Base.Products.Icons.Vegetables_svg)
                    .AddCategory(Category.Antene)
                    .AddCategory(Category.AnteneFM)
                    .AddCustomField("fm", "FM", "Listening frequency", () => 20, (CustomField field) => {
                        field.Builder.NewBtnGeneral("NvaluekHz")
                            .SetText(((171 + field.Reference.Value).ToFix32() * 0.5f.ToFix32()).ToStringRounded(1) + " kHz")
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
                                    field.Reference.Value += 45;
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
                                    field.Reference.Value += 45;
                                field.Refresh();
                            })
                            .SetSize(20, 20)
                            .AppendTo(field.Container);

                        field.Builder.NewBtnGeneral("N+0.5kHz")
                            .SetText(">")
                            .OnClick(() =>
                            {
                                field.Reference.Value += 1;
                                if (field.Reference.Value > 44)
                                    field.Reference.Value -= 45;
                                field.Refresh();
                            })
                            .SetSize(20, 20)
                            .AppendTo(field.Container);

                        field.Builder.NewBtnGeneral("N+5kHz")
                            .SetText(">>")
                            .OnClick(() =>
                            {
                                field.Reference.Value += 10;
                                if (field.Reference.Value > 44)
                                    field.Reference.Value -= 45;
                                field.Refresh();
                            })
                            .SetSize(20, 20)
                            .AppendTo(field.Container);

                        field.Builder.NewBtnGeneral("NendkHz")
                            .SetText(">|")
                            .OnClick(() =>
                            {
                                field.Reference.Value = 44;
                                field.Refresh();
                            })
                            .SetSize(20, 20)
                            .AppendTo(field.Container);
                    })
                    .AddEntityField<Antena>("antena", "Antena", distance: 5.ToFix32())
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
                    int entityId = m.Field["antena", 0];
                    if (m.Context.EntitiesManager
                            .TryGetEntity(new Mafi.Core.EntityId(entityId), out Antena entity) &&
                            (entity.DataBand is FMDataBand fm))
                    {
                        int[] signals = new int[digits];
                        for (int i = 0; i < digits; i++)
                        {
                            signals[i] = m.Input[names[i], 0];
                        }
                        fm.Update(m.Field["fm", 0], signals);
                    }
                };
            }
            foreach (int i in new int[] { 2, 4, 8, 16 })
            {
                var module = registrator
                    .ModuleBuilderStart($"Radio_Out_FM_{i}", $"FM broadcaster ({i} signals)", $"FM\no-{i}", Assets.Base.Products.Icons.Vegetables_svg)
                    .AddCategory(Category.Antene)
                    .AddCategory(Category.AnteneFM)
                    .AddCustomField("fm", "FM", "Broadcasting frequency", () => 20, (field) => {
                        field.Builder.NewBtnGeneral("NvaluekHz")
                            .SetText(((171 + field.Reference.Value).ToFix32() * 0.5f.ToFix32()).ToStringRounded(1) + " kHz")
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
                                    field.Reference.Value += 45;
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
                                    field.Reference.Value += 45;
                                field.Refresh();
                            })
                            .SetSize(20, 20)
                            .AppendTo(field.Container);

                        field.Builder.NewBtnGeneral("N+0.5kHz")
                            .SetText(">")
                            .OnClick(() =>
                            {
                                field.Reference.Value += 1;
                                if (field.Reference.Value > 44)
                                    field.Reference.Value -= 45;
                                field.Refresh();
                            })
                            .SetSize(20, 20)
                            .AppendTo(field.Container);

                        field.Builder.NewBtnGeneral("N+5kHz")
                            .SetText(">>")
                            .OnClick(() =>
                            {
                                field.Reference.Value += 10;
                                if (field.Reference.Value > 44)
                                    field.Reference.Value -= 45;
                                field.Refresh();
                            })
                            .SetSize(20, 20)
                            .AppendTo(field.Container);

                        field.Builder.NewBtnGeneral("NendkHz")
                            .SetText(">|")
                            .OnClick(() =>
                            {
                                field.Reference.Value = 44;
                                field.Refresh();
                            })
                            .SetSize(20, 20)
                            .AppendTo(field.Container);
                    })
                    .AddEntityField<Antena>("antena", "Antena", distance: 5.ToFix32())
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
    }
}
