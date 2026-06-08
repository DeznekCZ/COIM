using System.IO;
using Mafi;
using ProgramableNetwork.Runtime.Sim;
using Xunit;

namespace ProgramableNetwork.Runtime.Tests
{
    /// <summary>
    /// Golden-module fidelity test: load the real hysteresis.py through the source-linked interpreter
    /// and assert it behaves exactly as the module specifies (output latches high at >=High, low at
    /// <=Low, holds in the dead-band between). This exercises the whole pipeline — tokenizer, lexer,
    /// import resolution, field defaults, action + display, and the sim wrapper.
    /// </summary>
    public class HysteresisModuleTests
    {
        private static (SimController controller, SimModule module, Simulator sim) Setup()
        {
            string py = File.ReadAllText(TestPaths.Module("hysteresis.py"));
            var defs = ModuleLoader.LoadFile("hysteresis.py", py);
            ModuleDefinition def = Assert.Single(defs);

            SimController controller = new SimController();
            SimModule m = SimModuleFactory.Create(def, 1);
            controller.Modules.Add(m);

            // Drive the "Input" pin from a constant so we can sweep it.
            m.Connections["Input"] = new Connection { Kind = ConnectionKind.Constant, ConstantValue = Fix32.Zero };

            Simulator sim = new Simulator(controller);
            sim.Reset();
            return (controller, m, sim);
        }

        private static void Drive(SimModule m, int value) =>
            m.Connections["Input"].ConstantValue = Fix32.FromInt(value);

        [Fact]
        public void Metadata_is_read_from_python()
        {
            string py = File.ReadAllText(TestPaths.Module("hysteresis.py"));
            ModuleDefinition def = Assert.Single(ModuleLoader.LoadFile("hysteresis.py", py));

            Assert.Equal("Control: Hysteresis", def.Name);
            Assert.Equal("HYS", def.Symbol);
            Assert.Equal(1, def.Width);
            Assert.Contains(def.Inputs, p => p.Id == "Input");
            Assert.Contains(def.Outputs, p => p.Id == "Output");
            Assert.Contains(def.Fields, f => f.Id == "High" && f.Kind == FieldKind.Int32);
            Assert.Contains(def.Displays, d => d.Id == "active");
        }

        [Fact]
        public void Output_goes_high_at_or_above_high_threshold()
        {
            var (_, m, sim) = Setup();
            Drive(m, 100);
            sim.Step();
            Assert.True(m.Outputs["Output"].IsNotZero);
            Assert.Equal("on", m.Displays["active"]);
        }

        [Fact]
        public void Output_goes_low_at_or_below_low_threshold()
        {
            var (_, m, sim) = Setup();
            Drive(m, 100);
            sim.Step();                       // latch high first
            Drive(m, 0);
            sim.Step();
            Assert.True(m.Outputs["Output"].IsZero);
            Assert.Equal("", m.Displays["active"]);
        }

        [Fact]
        public void Output_holds_in_dead_band()
        {
            var (_, m, sim) = Setup();
            Drive(m, 100);
            sim.Step();                       // high
            Drive(m, 50);                     // between Low(1) and High(99)
            sim.Step();
            Assert.True(m.Outputs["Output"].IsNotZero); // unchanged

            Drive(m, 0);
            sim.Step();                       // low
            Drive(m, 50);
            sim.Step();
            Assert.True(m.Outputs["Output"].IsZero);     // still low
        }

        [Fact]
        public void Module_status_is_running_not_error()
        {
            var (_, m, sim) = Setup();
            Drive(m, 100);
            sim.Step();
            Assert.Equal(ModuleStatus.Running, m.Status);
            Assert.Equal("", m.Error);
        }
    }
}
