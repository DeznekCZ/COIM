using System.Collections.Generic;
using System.Linq;
using ProgramableNetwork.Ladder;
using Xunit;

namespace ProgramableNetwork.Runtime.Tests
{
    // The statement list is serial and the ladder is planar; these cover the
    // reconstruction between them, which is the part of the editor most likely
    // to be quietly wrong.
    public class LadderLayoutTests
    {
        private static LadderNetworkLayout LayoutOf(string source)
        {
            LadderProgram program = LadderParser.Parse(source);
            return LadderLayout.Build(program.Networks[0]);
        }

        private static LadderCell At(LadderRungLayout rung, int row, int column)
        {
            return rung.Cells.FirstOrDefault(c => c.Row == row && c.Column == column);
        }

        [Fact]
        public void Series_lays_out_left_to_right_on_one_row()
        {
            LadderRungLayout rung = LayoutOf("NETWORK 1\n  A I.A\n  AN I.B\n  A I.C\n  = Q.A").Rungs[0];

            Assert.Equal(1, rung.Rows);
            Assert.Equal(4, rung.Columns);
            Assert.Empty(rung.Branches);

            Assert.Equal("I.A", At(rung, 0, 0).Instruction.Operand.ToString());
            Assert.Equal("I.B", At(rung, 0, 1).Instruction.Operand.ToString());
            Assert.Equal("I.C", At(rung, 0, 2).Instruction.Operand.ToString());

            Assert.Single(rung.Coils);
            Assert.Equal(3, rung.Coils[0].Column);
            Assert.Equal("Q.A", rung.Coils[0].Instruction.Operand.ToString());
        }

        [Fact]
        public void Parallel_branch_stacks_rows_and_emits_two_bars()
        {
            LadderRungLayout rung = LayoutOf(
                "NETWORK 1\n  A(\n  O I.A\n  O M0\n  )\n  AN I.B\n  = M0").Rungs[0];

            Assert.Equal(2, rung.Rows);

            // The branch occupies column 0 on two rows; the series contact
            // that follows sits at column 1.
            Assert.Equal("I.A", At(rung, 0, 0).Instruction.Operand.ToString());
            Assert.Equal("M0", At(rung, 1, 0).Instruction.Operand.ToString());
            Assert.Equal("I.B", At(rung, 0, 1).Instruction.Operand.ToString());

            Assert.Equal(2, rung.Branches.Count);
            LadderBranchSpan open = rung.Branches.First(b => b.IsOpening);
            LadderBranchSpan close = rung.Branches.First(b => !b.IsOpening);
            Assert.Equal(0, open.Column);
            Assert.Equal(1, close.Column);
            Assert.Equal(0, open.FromRow);
            Assert.Equal(1, open.ToRow);
        }

        [Fact]
        public void Branch_with_a_longer_leg_widens_the_group()
        {
            // Leg 2 is two elements wide, so the group is 2 columns and the
            // trailing contact lands at column 2.
            LadderRungLayout rung = LayoutOf(
                "NETWORK 1\n  A(\n  O I.A\n  O I.B\n  A I.C\n  )\n  AN I.D\n  = Q.A").Rungs[0];

            Assert.Equal(2, rung.Rows);
            Assert.Equal("I.A", At(rung, 0, 0).Instruction.Operand.ToString());
            Assert.Equal("I.B", At(rung, 1, 0).Instruction.Operand.ToString());
            Assert.Equal("I.C", At(rung, 1, 1).Instruction.Operand.ToString());
            Assert.Equal("I.D", At(rung, 0, 2).Instruction.Operand.ToString());

            LadderBranchSpan close = rung.Branches.First(b => !b.IsOpening);
            Assert.Equal(2, close.Column);
        }

        [Fact]
        public void Nested_branches_stack_correctly()
        {
            LadderRungLayout rung = LayoutOf(
                "NETWORK 1\n"
                + "  A(\n"
                + "  O I.A\n"
                + "  O(\n"
                + "  O I.B\n"
                + "  O I.C\n"
                + "  )\n"
                + "  )\n"
                + "  = Q.A").Rungs[0];

            // Outer branch: leg 1 is I.A, leg 2 is the nested pair — three rows.
            Assert.Equal(3, rung.Rows);
            Assert.Equal("I.A", At(rung, 0, 0).Instruction.Operand.ToString());
            Assert.Equal("I.B", At(rung, 1, 0).Instruction.Operand.ToString());
            Assert.Equal("I.C", At(rung, 2, 0).Instruction.Operand.ToString());
            Assert.Equal(4, rung.Branches.Count);
        }

        [Fact]
        public void Cells_carry_the_index_they_came_from()
        {
            LadderNetwork network = LadderParser.Parse(
                "NETWORK 1\n  A(\n  O I.A\n  O M0\n  )\n  AN I.B\n  = M0").Networks[0];
            LadderRungLayout rung = LadderLayout.Build(network).Rungs[0];

            // Every cell must point back at the instruction it renders, or the
            // editor cannot apply an edit to the right place.
            foreach (LadderCell cell in rung.Cells.Concat(rung.Coils))
            {
                Assert.Same(network.Code[cell.Index], cell.Instruction);
            }
        }

        [Fact]
        public void Multiple_coils_stack_on_the_same_rung()
        {
            LadderRungLayout rung = LayoutOf("NETWORK 1\n  A I.A\n  = Q.A\n  = Q.B").Rungs[0];

            Assert.Equal(2, rung.Coils.Count);
            Assert.Equal(0, rung.Coils[0].Row);
            Assert.Equal(1, rung.Coils[1].Row);
            Assert.Equal(rung.Coils[0].Column, rung.Coils[1].Column);
            Assert.Equal(2, rung.Rows);
        }

        [Fact]
        public void A_network_can_hold_more_than_one_rung()
        {
            LadderNetworkLayout layout = LayoutOf(
                "NETWORK 1\n  A I.A\n  = Q.A\n  A I.B\n  AN I.C\n  = Q.B");

            Assert.Equal(2, layout.Rungs.Count);
            Assert.Single(layout.Rungs[0].Cells);
            Assert.Equal(2, layout.Rungs[1].Cells.Count);
            Assert.Equal(2, layout.Rows);
        }

        [Fact]
        public void Boxes_occupy_a_single_cell()
        {
            LadderRungLayout rung = LayoutOf(
                "NETWORK 1\n  A M0\n  TON PT:=6\n  = M1").Rungs[0];

            Assert.Equal(1, rung.Rows);
            Assert.Equal(LadderOp.TimerOn, At(rung, 0, 1).Instruction.Op);
            Assert.Equal(6, At(rung, 0, 1).Instruction.Preset);
        }

        [Fact]
        public void Every_demo_network_lays_out_without_losing_an_instruction()
        {
            LadderProgram program = LadderParser.Parse(
                "NETWORK 1  // seal-in\n  A(\n  O I.A\n  O M0\n  )\n  AN I.B\n  = M0\n"
                + "NETWORK 2  // pump\n  A M0\n  A B.main.demand\n  = Q.A\n"
                + "NETWORK 3  // purge\n  A M0\n  TON PT:=6\n  = M1\n"
                + "NETWORK 4  // alarm\n  CMP B.main.temp > 900\n  S Q.B\n"
                + "NETWORK 5  // ack\n  FP I.D\n  R Q.B");

            foreach (LadderNetwork network in program.Networks)
            {
                LadderNetworkLayout layout = LadderLayout.Build(network);
                int placed = layout.Rungs.Sum(r => r.Cells.Count + r.Coils.Count);
                int drawable = network.Code.Count(c => !LadderInstruction.IsStructural(c.Op));

                Assert.Equal(drawable, placed);
            }
        }
    }
}
