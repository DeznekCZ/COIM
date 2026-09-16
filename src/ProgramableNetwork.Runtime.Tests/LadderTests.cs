using System.Collections.Generic;
using Mafi;
using ProgramableNetwork.Ladder;
using Xunit;

namespace ProgramableNetwork.Runtime.Tests
{
    // Exercises the LAD compiler and scan machine headlessly.  The whole point
    // of keeping ILadderIo abstract is that these run without the game.
    public class LadderTests
    {
        // Stand-in for the module binding.  Mirrors the real process-image
        // rule: outputs buffer until EndScan, memory bits and bus pins land
        // immediately.
        private sealed class FakeIo : ILadderIo
        {
            private readonly Dictionary<string, Fix32> m_values = new Dictionary<string, Fix32>();
            private readonly Dictionary<string, Fix32> m_pending = new Dictionary<string, Fix32>();

            public int Writes;

            public void BeginScan()
            {
                m_pending.Clear();
            }

            public Fix32 Read(LadderOperand operand)
            {
                return m_values.TryGetValue(operand.ToString(), out Fix32 value) ? value : Fix32.Zero;
            }

            public void Write(LadderOperand operand, Fix32 value)
            {
                Writes++;
                if (operand.Kind == LadderOperandKind.Output)
                {
                    m_pending[operand.ToString()] = value;
                }
                else
                {
                    m_values[operand.ToString()] = value;
                }
            }

            public void EndScan()
            {
                foreach (KeyValuePair<string, Fix32> kv in m_pending)
                {
                    m_values[kv.Key] = kv.Value;
                }
            }

            public FakeIo On(string key)
            {
                m_values[key] = Fix32.One;
                return this;
            }

            public FakeIo Off(string key)
            {
                m_values[key] = Fix32.Zero;
                return this;
            }

            public FakeIo Num(string key, int value)
            {
                m_values[key] = Fix32.FromInt(value);
                return this;
            }

            public bool Bit(string key)
            {
                return m_values.TryGetValue(key, out Fix32 value) && value > Fix32.Zero;
            }
        }

        private static LadderMachine Compile(string source, LadderTier tier = LadderTier.Full)
        {
            LadderProgram program = LadderParser.Parse(source);
            LadderValidator.Enforce(program, tier);
            return new LadderMachine(program, null);
        }

        // ---------- parsing ----------

        [Fact]
        public void Parses_networks_and_counts_elements()
        {
            LadderProgram program = LadderParser.Parse(@"
NETWORK 1  // pump
  A    I.A
  AN   I.B
  =    Q.A
NETWORK 2  // heater
  A    M0
  =    Q.B");

            Assert.Equal(2, program.Networks.Count);
            Assert.Equal("pump", program.Networks[0].Title);
            Assert.Equal("heater", program.Networks[1].Title);
            Assert.Equal(5, program.ElementCount);
        }

        [Fact]
        public void Brackets_do_not_count_as_channels()
        {
            LadderProgram program = LadderParser.Parse(@"
NETWORK 1
  A(
  O    I.A
  O    M0
  )
  AN   I.B
  =    M0");

            Assert.Equal(4, LadderCapability.ChannelsOf(program.Networks[0]));
        }

        [Fact]
        public void ToText_round_trips()
        {
            const string source = @"NETWORK 1  // seal-in
  A(
  O    I.A
  O    M0
  )
  AN   I.B
  =    M0

NETWORK 2  // delay
  A    M0
  TON  PT:=6
  CMP  B.main.temp >= 900
  CTU  PV:=4 R:=I.C
  =    Q.A";

            LadderProgram once = LadderParser.Parse(source);
            string text = once.ToText();
            LadderProgram twice = LadderParser.Parse(text);

            Assert.Equal(text, twice.ToText());
            Assert.Equal(source.Replace("\r\n", "\n"), text);
        }

        [Fact]
        public void Operands_parse_every_kind()
        {
            Assert.Equal(LadderOperandKind.Input, LadderOperand.Parse("I.A", 1).Kind);
            Assert.Equal(LadderOperandKind.Output, LadderOperand.Parse("Q.B", 1).Kind);
            Assert.Equal(LadderOperandKind.Memory, LadderOperand.Parse("M7", 1).Kind);
            Assert.Equal(7, LadderOperand.Parse("M7", 1).Index);

            LadderOperand bus = LadderOperand.Parse("B.main.level", 1);
            Assert.Equal(LadderOperandKind.Bus, bus.Kind);
            Assert.Equal("main", bus.Name);
            Assert.Equal("level", bus.Member);

            Assert.Equal(Fix32.FromInt(900), LadderOperand.Parse("900", 1).Literal);
        }

        [Theory]
        [InlineData("NETWORK 1\n  A I.A\n  = I.B", "cannot drive")]
        [InlineData("NETWORK 1\n  A I.A\n  = 5", "cannot drive")]
        [InlineData("NETWORK 1\n  A(\n  A I.A\n  = Q.A", "never closed")]
        [InlineData("NETWORK 1\n  )\n  = Q.A", "never opened")]
        [InlineData("NETWORK 1\n  XYZ I.A\n  = Q.A", "not a ladder instruction")]
        [InlineData("NETWORK 1\n  A M99\n  = Q.A", "out of range")]
        [InlineData("A I.A", "before any NETWORK")]
        [InlineData("NETWORK 1\n  A 5\n  = Q.A", "plain number")]
        [InlineData("NETWORK 1\n  TON PT:=0\n  = Q.A", "above zero")]
        [InlineData("NETWORK 1\n  A I.A\n  CMP I.B\n  = Q.A", "CMP needs")]
        public void Rejects_bad_programs(string source, string expected)
        {
            LadderParseException error = Assert.Throws<LadderParseException>(() => LadderParser.Parse(source));
            Assert.Contains(expected, error.Message);
        }

        // ---------- bit logic ----------

        [Fact]
        public void Series_and_negated_contacts()
        {
            LadderMachine machine = Compile("NETWORK 1\n  A I.A\n  AN I.B\n  = Q.A");
            FakeIo io = new FakeIo();

            io.On("I.A").Off("I.B");
            machine.Scan(io);
            Assert.True(io.Bit("Q.A"));

            io.On("I.B");
            machine.Scan(io);
            Assert.False(io.Bit("Q.A"));
        }

        [Fact]
        public void Parallel_branch_ors_its_legs()
        {
            LadderMachine machine = Compile("NETWORK 1\n  A(\n  O I.A\n  O I.B\n  )\n  AN I.C\n  = Q.A");
            FakeIo io = new FakeIo();

            machine.Scan(io);
            Assert.False(io.Bit("Q.A"));

            io.On("I.B");
            machine.Scan(io);
            Assert.True(io.Bit("Q.A"));

            io.On("I.C");
            machine.Scan(io);
            Assert.False(io.Bit("Q.A"));
        }

        [Fact]
        public void Seal_in_latch_closes_within_one_scan()
        {
            // Network 1 writes M0; network 2 reads it the same scan.  This only
            // works because memory writes are immediate — it is the reason the
            // process image splits M from Q.
            LadderMachine machine = Compile(
                "NETWORK 1\n  A(\n  O I.A\n  O M0\n  )\n  AN I.B\n  = M0\n"
                + "NETWORK 2\n  A M0\n  = Q.A");
            FakeIo io = new FakeIo();

            io.On("I.A");
            machine.Scan(io);
            Assert.True(io.Bit("M0"));
            Assert.True(io.Bit("Q.A"));

            // Start released — the latch holds itself through its own contact.
            io.Off("I.A");
            machine.Scan(io);
            Assert.True(io.Bit("M0"));

            io.On("I.B");
            machine.Scan(io);
            Assert.False(io.Bit("M0"));
        }

        [Fact]
        public void Outputs_are_buffered_until_end_of_scan()
        {
            // Network 2 reads Q.A before the flush, so it sees the value from
            // the PREVIOUS scan, not the one network 1 just wrote.
            LadderMachine machine = Compile(
                "NETWORK 1\n  A I.A\n  = Q.A\n"
                + "NETWORK 2\n  A Q.A\n  = M0");
            FakeIo io = new FakeIo();

            io.On("I.A");
            machine.Scan(io);
            Assert.True(io.Bit("Q.A"));
            Assert.False(io.Bit("M0"));

            machine.Scan(io);
            Assert.True(io.Bit("M0"));
        }

        [Fact]
        public void Set_and_reset_coils_latch()
        {
            LadderMachine machine = Compile(
                "NETWORK 1\n  A I.A\n  S Q.B\n"
                + "NETWORK 2\n  A I.B\n  R Q.B");
            FakeIo io = new FakeIo();

            io.On("I.A");
            machine.Scan(io);
            Assert.True(io.Bit("Q.B"));

            io.Off("I.A");
            machine.Scan(io);
            Assert.True(io.Bit("Q.B"));

            io.On("I.B");
            machine.Scan(io);
            Assert.False(io.Bit("Q.B"));
        }

        [Fact]
        public void Two_coils_in_a_row_share_the_rung()
        {
            LadderMachine machine = Compile("NETWORK 1\n  A I.A\n  = Q.A\n  = Q.B");
            FakeIo io = new FakeIo();

            io.On("I.A");
            machine.Scan(io);
            Assert.True(io.Bit("Q.A"));
            Assert.True(io.Bit("Q.B"));
        }

        [Fact]
        public void Rising_edge_fires_for_one_scan_only()
        {
            LadderMachine machine = Compile("NETWORK 1\n  FP I.A\n  = M0");
            FakeIo io = new FakeIo();

            machine.Scan(io);
            Assert.False(io.Bit("M0"));

            io.On("I.A");
            machine.Scan(io);
            Assert.True(io.Bit("M0"));

            machine.Scan(io);
            Assert.False(io.Bit("M0"));
        }

        [Fact]
        public void Falling_edge_fires_on_release()
        {
            LadderMachine machine = Compile("NETWORK 1\n  FN I.A\n  = M0");
            FakeIo io = new FakeIo();

            io.On("I.A");
            machine.Scan(io);
            Assert.False(io.Bit("M0"));

            io.Off("I.A");
            machine.Scan(io);
            Assert.True(io.Bit("M0"));

            machine.Scan(io);
            Assert.False(io.Bit("M0"));
        }

        // ---------- boxes ----------

        [Fact]
        public void Timer_on_delays_then_holds()
        {
            LadderMachine machine = Compile("NETWORK 1\n  A I.A\n  TON PT:=3\n  = Q.A");
            FakeIo io = new FakeIo();

            io.On("I.A");
            for (int i = 0; i < 2; i++)
            {
                machine.Scan(io);
                Assert.False(io.Bit("Q.A"));
            }
            machine.Scan(io);
            Assert.True(io.Bit("Q.A"));

            machine.Scan(io);
            Assert.True(io.Bit("Q.A"));

            // Dropping power resets the accumulator, so the next run starts over.
            io.Off("I.A");
            machine.Scan(io);
            Assert.False(io.Bit("Q.A"));

            io.On("I.A");
            machine.Scan(io);
            Assert.False(io.Bit("Q.A"));
        }

        [Fact]
        public void Timer_off_holds_after_power_drops()
        {
            LadderMachine machine = Compile("NETWORK 1\n  A I.A\n  TOF PT:=2\n  = Q.A");
            FakeIo io = new FakeIo();

            io.On("I.A");
            machine.Scan(io);
            Assert.True(io.Bit("Q.A"));

            io.Off("I.A");
            machine.Scan(io);
            Assert.True(io.Bit("Q.A"));
            machine.Scan(io);
            Assert.False(io.Bit("Q.A"));
        }

        [Fact]
        public void Counter_counts_edges_and_resets()
        {
            LadderMachine machine = Compile("NETWORK 1\n  A I.A\n  CTU PV:=2 R:=I.C\n  = Q.A");
            FakeIo io = new FakeIo();

            // Holding the input does not keep counting — only transitions do.
            io.On("I.A");
            machine.Scan(io);
            machine.Scan(io);
            Assert.False(io.Bit("Q.A"));

            io.Off("I.A");
            machine.Scan(io);
            io.On("I.A");
            machine.Scan(io);
            Assert.True(io.Bit("Q.A"));

            io.On("I.C");
            machine.Scan(io);
            Assert.False(io.Bit("Q.A"));
        }

        [Fact]
        public void Compare_against_a_literal()
        {
            LadderMachine machine = Compile("NETWORK 1\n  CMP B.main.temp > 900\n  S Q.B");
            FakeIo io = new FakeIo();

            io.Num("B.main.temp", 500);
            machine.Scan(io);
            Assert.False(io.Bit("Q.B"));

            io.Num("B.main.temp", 1000);
            machine.Scan(io);
            Assert.True(io.Bit("Q.B"));
        }

        [Fact]
        public void Compare_operators_all_resolve()
        {
            FakeIo io = new FakeIo();
            io.Num("I.A", 5);

            Assert.True(Evaluate("CMP I.A >= 5", io));
            Assert.True(Evaluate("CMP I.A <= 5", io));
            Assert.True(Evaluate("CMP I.A = 5", io));
            Assert.False(Evaluate("CMP I.A <> 5", io));
            Assert.False(Evaluate("CMP I.A > 5", io));
            Assert.True(Evaluate("CMP I.A < 6", io));
        }

        private static bool Evaluate(string compare, FakeIo io)
        {
            LadderMachine machine = Compile("NETWORK 1\n  " + compare + "\n  = M0");
            machine.Scan(io);
            return io.Bit("M0");
        }

        // ---------- bus ----------

        [Fact]
        public void Bus_pins_read_and_drive()
        {
            LadderMachine machine = Compile(
                "NETWORK 1\n  A B.main.demand\n  AN I.B\n  = B.main.run\n"
                + "NETWORK 2\n  A B.main.run\n  = Q.A");
            FakeIo io = new FakeIo();

            io.On("B.main.demand");
            machine.Scan(io);
            Assert.True(io.Bit("B.main.run"));
            Assert.True(io.Bit("Q.A"));
        }

        // ---------- state persistence ----------

        [Fact]
        public void Machine_state_survives_a_reload()
        {
            const string source = "NETWORK 1\n  A I.A\n  TON PT:=5\n  = Q.A";
            LadderProgram program = LadderParser.Parse(source);

            LadderMachine before = new LadderMachine(program, null);
            FakeIo io = new FakeIo();
            io.On("I.A");
            before.Scan(io);
            before.Scan(io);
            before.Scan(io);

            // Save / load: only the int array crosses the boundary.
            LadderMachine after = new LadderMachine(LadderParser.Parse(source), before.State);
            after.Scan(io);
            after.Scan(io);
            Assert.True(io.Bit("Q.A"));
        }

        [Fact]
        public void State_slots_are_allocated_per_stateful_instruction()
        {
            LadderProgram program = LadderParser.Parse(
                "NETWORK 1\n  FP I.A\n  TON PT:=2\n  CTU PV:=2\n  = Q.A");

            // edge 1 + timer 1 + counter 2
            Assert.Equal(4, program.StateSlotCount);
        }

        // ---------- tiers ----------

        [Fact]
        public void Basic_tier_runs_plain_io_logic()
        {
            LadderProgram program = LadderParser.Parse("NETWORK 1\n  A I.A\n  AN I.B\n  = Q.A");
            LadderValidator.Enforce(program, LadderTier.Basic);
            Assert.Equal(LadderTier.Basic, LadderCapability.Analyze(program).RequiredTier);
        }

        [Theory]
        [InlineData("NETWORK 1\n  A M0\n  = Q.A", "memory bit")]
        [InlineData("NETWORK 1\n  FP I.A\n  = Q.A", "edge detection")]
        [InlineData("NETWORK 1\n  A B.main.x\n  = Q.A", "bus pin")]
        [InlineData("NETWORK 1\n  A I.A\n  TON PT:=2\n  = Q.A", "TON box")]
        public void Basic_tier_refuses_higher_features(string source, string expected)
        {
            LadderProgram program = LadderParser.Parse(source);
            LadderParseException error = Assert.Throws<LadderParseException>(
                () => LadderValidator.Enforce(program, LadderTier.Basic));

            Assert.Contains(expected, error.Message);
            Assert.Contains("Swap it for the", error.Message);
        }

        [Fact]
        public void Basic_tier_caps_networks_at_two()
        {
            LadderProgram program = LadderParser.Parse(
                "NETWORK 1\n  A I.A\n  = Q.A\n"
                + "NETWORK 2\n  A I.B\n  = Q.B\n"
                + "NETWORK 3\n  A I.C\n  = Q.C");

            LadderParseException error = Assert.Throws<LadderParseException>(
                () => LadderValidator.Enforce(program, LadderTier.Basic));
            Assert.Contains("2 networks", error.Message);
            Assert.Contains("Extended", error.Message);
        }

        [Fact]
        public void Basic_tier_caps_channels_at_five()
        {
            LadderProgram program = LadderParser.Parse(
                "NETWORK 1\n  A I.A\n  A I.B\n  A I.C\n  A I.D\n  A I.E\n  = Q.A");

            LadderParseException error = Assert.Throws<LadderParseException>(
                () => LadderValidator.Enforce(program, LadderTier.Basic));
            Assert.Contains("6 channels", error.Message);
        }

        [Fact]
        public void Extended_tier_allows_memory_but_not_boxes()
        {
            LadderProgram memory = LadderParser.Parse("NETWORK 1\n  A M0\n  FP I.A\n  = Q.A");
            LadderValidator.Enforce(memory, LadderTier.Extended);
            Assert.Equal(LadderTier.Extended, LadderCapability.Analyze(memory).RequiredTier);

            LadderProgram timer = LadderParser.Parse("NETWORK 1\n  A I.A\n  TON PT:=2\n  = Q.A");
            Assert.Throws<LadderParseException>(() => LadderValidator.Enforce(timer, LadderTier.Extended));
        }

        // Mirrors PlcLad.STARTER_PROGRAM.  It ships as the default `code` value
        // on every tier, so it has to compile on the NARROWEST one — and it
        // has to actually latch, since it seals in through its own output
        // rather than a memory bit it isn't allowed to use.
        [Fact]
        public void Starter_program_runs_on_the_basic_tier()
        {
            const string starter =
                "NETWORK 1  // start / stop latch\n"
                + "  A(\n"
                + "  O    I.A\n"
                + "  O    Q.A\n"
                + "  )\n"
                + "  AN   I.B\n"
                + "  =    Q.A";

            LadderProgram program = LadderParser.Parse(starter);
            LadderValidator.Enforce(program, LadderTier.Basic);
            Assert.Equal(LadderTier.Basic, LadderCapability.Analyze(program).RequiredTier);

            LadderMachine machine = new LadderMachine(program, null);
            FakeIo io = new FakeIo();

            io.On("I.A");
            machine.Scan(io);
            Assert.True(io.Bit("Q.A"));

            io.Off("I.A");
            machine.Scan(io);
            Assert.True(io.Bit("Q.A"));

            io.On("I.B");
            machine.Scan(io);
            Assert.False(io.Bit("Q.A"));

            io.Off("I.B");
            machine.Scan(io);
            Assert.False(io.Bit("Q.A"));
        }

        [Fact]
        public void Basic_tier_allows_a_wide_branch_but_only_two_levels()
        {
            // Three legs is one bracket pair — one level — so Basic runs it. The
            // width is paid for out of the channel budget instead: four symbols
            // of the five Basic allows.
            LadderProgram program = LadderParser.Parse(
                "NETWORK 1\n  A(\n  O I.A\n  O I.B\n  O I.C\n  )\n  = Q.A");

            Assert.Equal(1, LadderCapability.NestingOf(program.Networks[0]));
            Assert.Equal(4, LadderCapability.ChannelsOf(program.Networks[0]));
            LadderValidator.Enforce(program, LadderTier.Basic);

            // What Basic will not run is the width: six legs is seven symbols,
            // past the five channels it has.
            LadderProgram wide = LadderParser.Parse(
                "NETWORK 1\n  A(\n  O I.A\n  O I.B\n  O I.C\n  O I.D\n  O M0\n  O M1\n  )\n  = Q.A");
            LadderParseException error = Assert.Throws<LadderParseException>(
                () => LadderValidator.Enforce(wide, LadderTier.Basic));
            Assert.Contains("channels", error.Message);
            Assert.Contains("Extended", error.Message);
        }

        // The Siemens model: legs are free, depth is what is rationed. A chain of
        // `O` terms costs no nesting-stack entry, so a branch may be as wide as
        // the channel budget allows; every `A(` / `O(` costs one, so putting a
        // branch inside a branch is what the tier charges for. Counted per
        // network, the scope the nesting stack lives in.

        [Fact]
        public void Legs_are_free_however_many_there_are()
        {
            // Seven legs, one bracket pair, nesting depth 1 — and seven rows.
            string legs = "";
            for (int i = 0; i < 7; i++)
            {
                legs += "  O M" + i + "\n";
            }
            LadderProgram program = LadderParser.Parse("NETWORK 1\n  A(\n" + legs + "  )\n  = Q.A");

            Assert.Equal(1, LadderCapability.NestingOf(program.Networks[0]));
            Assert.Equal(7, LadderCapability.RowsOf(program.Networks[0]));
            LadderValidator.Enforce(program, LadderTier.Full);
        }

        [Fact]
        public void Nesting_is_what_costs()
        {
            // Two nested two-leg groups: never more than two legs at a level, but
            // two levels deep — which is all Basic allows.
            LadderProgram program = LadderParser.Parse(
                "NETWORK 1\n  A(\n  O I.A\n  O(\n  O I.B\n  O I.C\n  )\n  )\n  = Q.A");

            Assert.Equal(2, LadderCapability.NestingOf(program.Networks[0]));
            LadderValidator.Enforce(program, LadderTier.Basic);

            // A third level is over the Basic line.
            LadderProgram deeper = LadderParser.Parse(
                "NETWORK 1\n  A(\n  O I.A\n  O(\n  O I.B\n  O(\n  O I.C\n  O I.D\n  )\n  )\n  )\n  = Q.A");
            Assert.Equal(3, LadderCapability.NestingOf(deeper.Networks[0]));
            LadderParseException error = Assert.Throws<LadderParseException>(
                () => LadderValidator.Enforce(deeper, LadderTier.Basic));
            Assert.Contains("allows 2", error.Message);
            LadderValidator.Enforce(deeper, LadderTier.Extended);
        }

        [Fact]
        public void Nesting_is_counted_per_network_not_across_the_program()
        {
            // Two networks each nesting two deep is fine on Basic; the depths do
            // not add up, because each network's brackets balance on their own.
            string one = "NETWORK 1\n  A(\n  O I.A\n  O(\n  O I.B\n  O I.C\n  )\n  )\n  = Q.A";
            string two = "NETWORK 2\n  A(\n  O I.D\n  O(\n  O I.E\n  O I.F\n  )\n  )\n  = Q.B";
            LadderProgram program = LadderParser.Parse(one + "\n\n" + two);

            Assert.Equal(2, LadderCapability.NestingOf(program.Networks[0]));
            Assert.Equal(2, LadderCapability.NestingOf(program.Networks[1]));
            LadderValidator.Enforce(program, LadderTier.Basic);
        }

        [Fact]
        public void The_runtime_refuses_to_nest_deeper_than_the_top_tier()
        {
            string source = "NETWORK 1\n";
            for (int i = 0; i < 7; i++)
            {
                source += "  A(\n  A I.A\n";
            }
            source += "  O I.B\n";
            for (int i = 0; i < 7; i++)
            {
                source += "  )\n";
            }
            source += "  = Q.A";

            LadderParseException error = Assert.Throws<LadderParseException>(
                () => LadderParser.Parse(source));
            Assert.Contains("nests at most", error.Message);
        }

        [Fact]
        public void Capability_report_explains_the_requirement()
        {
            LadderProgram program = LadderParser.Parse("NETWORK 1\n  A I.A\n  TON PT:=2\n  = Q.A");
            LadderCapabilityReport report = LadderCapability.Analyze(program);

            Assert.Equal(LadderTier.Full, report.RequiredTier);
            Assert.Equal("a timer box", report.Reason);
            Assert.Equal(1, report.NetworkCount);
            Assert.Equal(3, report.WidestNetwork);
        }

        // ---- signal handlers ----

        [Fact]
        public void Not_inverts_the_rung()
        {
            LadderMachine machine = Compile("NETWORK 1\n  A I.A\n  NOT\n  = Q.A");
            FakeIo io = new FakeIo();

            machine.Scan(io);
            Assert.True(io.Bit("Q.A"));

            io.On("I.A");
            machine.Scan(io);
            Assert.False(io.Bit("Q.A"));
        }

        [Fact]
        public void Not_on_its_own_is_an_always_on_rung()
        {
            // The accumulator starts false with the first-check flag clear, so
            // a leading NOT conducts — the cheapest way to spell "always on".
            LadderMachine machine = Compile("NETWORK 1\n  NOT\n  = Q.A");
            FakeIo io = new FakeIo();

            machine.Scan(io);
            Assert.True(io.Bit("Q.A"));
        }

        [Fact]
        public void Negated_coil_writes_the_inverse_every_scan()
        {
            LadderMachine machine = Compile("NETWORK 1\n  A I.A\n  =N   Q.A");
            FakeIo io = new FakeIo();

            machine.Scan(io);
            Assert.True(io.Bit("Q.A"));

            io.On("I.A");
            machine.Scan(io);
            Assert.False(io.Bit("Q.A"));
        }

        [Fact]
        public void Toggle_coil_flips_once_per_rising_edge()
        {
            LadderMachine machine = Compile("NETWORK 1\n  A I.A\n  T    Q.A");
            FakeIo io = new FakeIo();

            machine.Scan(io);
            Assert.False(io.Bit("Q.A"));

            io.On("I.A");
            machine.Scan(io);
            Assert.True(io.Bit("Q.A"));

            // Held on — it must NOT keep flipping.
            machine.Scan(io);
            machine.Scan(io);
            Assert.True(io.Bit("Q.A"));

            io.Off("I.A");
            machine.Scan(io);
            Assert.True(io.Bit("Q.A"));

            io.On("I.A");
            machine.Scan(io);
            Assert.False(io.Bit("Q.A"));
        }

        [Fact]
        public void Pulse_timer_runs_its_full_length_after_the_rung_drops()
        {
            LadderMachine machine = Compile("NETWORK 1\n  A I.A\n  TP   PT:=3\n  = Q.A");
            FakeIo io = new FakeIo();

            io.On("I.A");
            machine.Scan(io);
            Assert.True(io.Bit("Q.A"));

            // The trigger goes away immediately; the pulse must not.
            io.Off("I.A");
            machine.Scan(io);
            Assert.True(io.Bit("Q.A"));
            machine.Scan(io);
            Assert.True(io.Bit("Q.A"));
            machine.Scan(io);
            Assert.False(io.Bit("Q.A"));
        }

        [Fact]
        public void The_new_handlers_round_trip_through_text()
        {
            string source = "NETWORK 1\n  A I.A\n  NOT\n  TP   PT:=4\n  =N   Q.A\n  T    Q.B";
            LadderProgram program = LadderParser.Parse(source);

            Assert.Equal(program.ToText(), LadderParser.Parse(program.ToText()).ToText());
        }

        [Fact]
        public void Toggle_needs_Extended_and_the_pulse_timer_needs_Full()
        {
            LadderProgram toggle = LadderParser.Parse("NETWORK 1\n  A I.A\n  T Q.A");
            Assert.Equal(LadderTier.Extended, LadderCapability.Analyze(toggle).RequiredTier);
            Assert.Throws<LadderParseException>(() => LadderValidator.Enforce(toggle, LadderTier.Basic));
            LadderValidator.Enforce(toggle, LadderTier.Extended);

            LadderProgram pulse = LadderParser.Parse("NETWORK 1\n  A I.A\n  TP PT:=2\n  = Q.A");
            Assert.Equal(LadderTier.Full, LadderCapability.Analyze(pulse).RequiredTier);
            Assert.Throws<LadderParseException>(() => LadderValidator.Enforce(pulse, LadderTier.Extended));

            // NOT and the negated coil are plain logic — they stay at Basic.
            LadderProgram basic = LadderParser.Parse("NETWORK 1\n  A I.A\n  NOT\n  =N Q.A");
            Assert.Equal(LadderTier.Basic, LadderCapability.Analyze(basic).RequiredTier);
            LadderValidator.Enforce(basic, LadderTier.Basic);
        }

        [Fact]
        public void Not_rejects_an_address()
        {
            LadderParseException error = Assert.Throws<LadderParseException>(
                () => LadderParser.Parse("NETWORK 1\n  NOT I.A\n  = Q.A"));
            Assert.Contains("takes no address", error.Message);
        }
    }
}
