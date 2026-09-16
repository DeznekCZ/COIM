using System.Collections.Generic;
using ProgramableNetwork.Ladder;
using Xunit;

namespace ProgramableNetwork.Runtime.Tests
{
    // Edit operations return new program TEXT, so every test here asserts on
    // text that must itself re-parse — which is the real contract: an edit can
    // never produce a program the parser would reject.
    public class LadderEditTests
    {
        private static LadderProgram Parse(string source)
        {
            return LadderParser.Parse(source);
        }

        // Every edit result must survive a round trip, or the editor could
        // write a module into a state it can't read back.
        private static LadderProgram Reparse(string text)
        {
            LadderProgram program = LadderParser.Parse(text);
            Assert.Equal(text, program.ToText());
            return program;
        }

        // ---- element edits ----

        [Fact]
        public void SetOperand_rewrites_one_contact()
        {
            LadderProgram program = Parse("NETWORK 1\n  A I.A\n  AN I.B\n  = Q.A");
            string text = LadderEdit.SetOperand(program, 0, 1, LadderOperand.ForInput("C"));

            LadderProgram after = Reparse(text);
            Assert.Equal("I.C", after.Networks[0].Code[1].Operand.ToString());
            Assert.Equal(LadderOp.AndNot, after.Networks[0].Code[1].Op);
        }

        [Fact]
        public void SetKind_flips_a_contact_but_keeps_its_branch_connector()
        {
            // Code is [0] A( [1] O I.A [2] O I.B [3] ) [4] = Q.A — index 2 is
            // the second leg.  It joins with O; turning it into an NC contact
            // must leave it joining with ON, or the rung becomes a series.
            LadderProgram program = Parse("NETWORK 1\n  A(\n  O I.A\n  O I.B\n  )\n  = Q.A");
            string text = LadderEdit.SetKind(program, 0, 2, LadderOp.AndNot);

            LadderProgram after = Reparse(text);
            Assert.Equal(LadderOp.OrNot, after.Networks[0].Code[2].Op);
            Assert.Equal(2, LadderLayout.Build(after.Networks[0]).Rungs[0].Rows);
        }

        [Fact]
        public void SetKind_refuses_to_turn_a_coil_into_a_contact()
        {
            LadderProgram program = Parse("NETWORK 1\n  A I.A\n  = Q.A");
            string text = LadderEdit.SetKind(program, 0, 1, LadderOp.And);

            LadderProgram after = Reparse(text);
            Assert.Equal(LadderOp.Assign, after.Networks[0].Code[1].Op);
        }

        [Fact]
        public void SetKind_switches_coil_flavour()
        {
            LadderProgram program = Parse("NETWORK 1\n  A I.A\n  = Q.A");
            string text = LadderEdit.SetKind(program, 0, 1, LadderOp.Set);

            LadderProgram after = Reparse(text);
            Assert.Equal(LadderOp.Set, after.Networks[0].Code[1].Op);
        }

        [Fact]
        public void SetPreset_and_SetCompare_edit_box_parameters()
        {
            LadderProgram program = Parse("NETWORK 1\n  A I.A\n  TON PT:=6\n  = Q.A");
            LadderProgram after = Reparse(LadderEdit.SetPreset(program, 0, 1, 20));
            Assert.Equal(20, after.Networks[0].Code[1].Preset);

            LadderProgram cmp = Parse("NETWORK 1\n  CMP I.A > 900\n  = Q.A");
            LadderProgram edited = Reparse(LadderEdit.SetCompare(
                cmp, 0, 0, LadderCompare.LessOrEqual, LadderOperand.ForLiteral(Mafi.Fix32.FromInt(50))));
            Assert.Equal(LadderCompare.LessOrEqual, edited.Networks[0].Code[0].Compare);
            Assert.Equal("50", edited.Networks[0].Code[0].Right.ToString());
        }

        // ---- structure ----

        [Fact]
        public void InsertAfter_adds_a_series_element()
        {
            LadderProgram program = Parse("NETWORK 1\n  A I.A\n  = Q.A");
            string text = LadderEdit.InsertAfter(program, 0, 0, new LadderInstruction(
                LadderOp.And, LadderOperand.ForInput("B"), null, LadderCompare.Greater, 0, null, -1, 0));

            LadderProgram after = Reparse(text);
            LadderRungLayout rung = LadderLayout.Build(after.Networks[0]).Rungs[0];
            Assert.Equal(1, rung.Rows);
            Assert.Equal(2, rung.Cells.Count);
            Assert.Equal("I.B", after.Networks[0].Code[1].Operand.ToString());
        }

        [Fact]
        public void AddParallelBranch_wraps_an_element_into_two_legs()
        {
            LadderProgram program = Parse("NETWORK 1\n  A I.A\n  AN I.B\n  = Q.A");
            string text = LadderEdit.AddParallelBranch(program, 0, 0, new LadderInstruction(
                LadderOp.And, LadderOperand.ForMemory(0), null, LadderCompare.Greater, 0, null, -1, 0));

            LadderProgram after = Reparse(text);
            LadderRungLayout rung = LadderLayout.Build(after.Networks[0]).Rungs[0];

            // Two rows now, and the branch bars exist.
            Assert.Equal(2, rung.Rows);
            Assert.Equal(2, rung.Branches.Count);
            Assert.Contains(rung.Cells, c => c.Row == 0 && c.Instruction.Operand.ToString() == "I.A");
            Assert.Contains(rung.Cells, c => c.Row == 1 && c.Instruction.Operand.ToString() == "M0");
        }

        [Fact]
        public void AddParallelBranch_refuses_a_coil()
        {
            LadderProgram program = Parse("NETWORK 1\n  A I.A\n  = Q.A");
            string text = LadderEdit.AddParallelBranch(program, 0, 1, new LadderInstruction(
                LadderOp.And, LadderOperand.ForMemory(0), null, LadderCompare.Greater, 0, null, -1, 0));

            LadderProgram after = Reparse(text);
            Assert.Equal(2, after.Networks[0].Code.Count);
        }

        [Fact]
        public void Delete_removes_an_element()
        {
            LadderProgram program = Parse("NETWORK 1\n  A I.A\n  AN I.B\n  = Q.A");
            LadderProgram after = Reparse(LadderEdit.Delete(program, 0, 1));

            Assert.Equal(2, after.Networks[0].Code.Count);
            Assert.Equal("I.A", after.Networks[0].Code[0].Operand.ToString());
        }

        [Fact]
        public void Deleting_the_second_leg_collapses_the_branch()
        {
            // This is the operation most likely to leave junk behind: without
            // the collapse pass the text keeps an "A( ... )" wrapping one leg.
            LadderProgram program = Parse("NETWORK 1\n  A(\n  O I.A\n  O M0\n  )\n  AN I.B\n  = M0");
            string text = LadderEdit.Delete(program, 0, 2);

            Assert.DoesNotContain("A(", text);
            Assert.DoesNotContain(")", text);

            LadderProgram after = Reparse(text);
            LadderRungLayout rung = LadderLayout.Build(after.Networks[0]).Rungs[0];
            Assert.Equal(1, rung.Rows);
            Assert.Equal(2, rung.Cells.Count);
        }

        [Fact]
        public void Deleting_both_legs_removes_the_group_entirely()
        {
            LadderProgram program = Parse("NETWORK 1\n  A(\n  O I.A\n  O M0\n  )\n  AN I.B\n  = M0");
            LadderProgram once = Reparse(LadderEdit.Delete(program, 0, 2));
            string text = LadderEdit.Delete(once, 0, 0);

            Assert.DoesNotContain("(", text);
            LadderProgram after = Reparse(text);
            Assert.Equal(2, after.Networks[0].Code.Count);
        }

        [Fact]
        public void Collapsing_a_nested_group_promotes_its_connector()
        {
            // Inner group is an OR leg; when it collapses to one element that
            // element has to keep joining with O, or the outer branch is lost.
            // [0] A( [1] O I.A [2] O( [3] O I.B [4] O I.C [5] ) [6] ) [7] = Q.A
            LadderProgram program = Parse(
                "NETWORK 1\n  A(\n  O I.A\n  O(\n  O I.B\n  O I.C\n  )\n  )\n  = Q.A");
            string text = LadderEdit.Delete(program, 0, 4);

            LadderProgram after = Reparse(text);
            LadderRungLayout rung = LadderLayout.Build(after.Networks[0]).Rungs[0];

            // Still two parallel legs: I.A and I.B.
            Assert.Equal(2, rung.Rows);
            Assert.Equal(2, rung.Cells.Count);
        }

        [Fact]
        public void Delete_ignores_a_bracket()
        {
            LadderProgram program = Parse("NETWORK 1\n  A(\n  O I.A\n  O M0\n  )\n  = Q.A");
            string text = LadderEdit.Delete(program, 0, 0);

            LadderProgram after = Reparse(text);
            Assert.Equal(2, LadderLayout.Build(after.Networks[0]).Rungs[0].Rows);
        }

        [Fact]
        public void Out_of_range_edits_leave_the_program_alone()
        {
            LadderProgram program = Parse("NETWORK 1\n  A I.A\n  = Q.A");
            Assert.Equal(program.ToText(), LadderEdit.SetOperand(program, 9, 0, LadderOperand.ForInput("B")));
            Assert.Equal(program.ToText(), LadderEdit.DeleteNetwork(program, 9));
        }

        // ---- networks ----

        [Fact]
        public void AddNetwork_appends_an_empty_network()
        {
            // Empty on purpose: seeding a contact and a coil would put operands
            // in the program that the player never chose.
            LadderProgram program = Parse("NETWORK 1\n  A I.A\n  = Q.A");
            LadderProgram after = Reparse(LadderEdit.AddNetwork(program, "pump"));

            Assert.Equal(2, after.Networks.Count);
            Assert.Equal(2, after.Networks[1].Number);
            Assert.Equal("pump", after.Networks[1].Title);
            Assert.Empty(after.Networks[1].Code);
        }

        [Fact]
        public void InsertFirst_fills_an_empty_network()
        {
            LadderProgram program = Reparse(LadderEdit.AddNetwork(Parse("NETWORK 1\n  A I.A\n  = Q.A"), ""));

            LadderProgram after = Reparse(LadderEdit.InsertFirst(program, 1, new LadderInstruction(
                LadderOp.And, LadderOperand.ForInput("B"), null, LadderCompare.Greater, 0, null, -1, 0)));

            Assert.Single(after.Networks[1].Code);
            Assert.Equal("I.B", after.Networks[1].Code[0].Operand.ToString());
        }

        [Fact]
        public void An_empty_network_parses_and_runs_as_a_no_op()
        {
            // The editor's natural intermediate state has to be legal, or every
            // "add network" would have to invent placeholder elements.
            LadderProgram program = Reparse("NETWORK 1\n\nNETWORK 2  // later");

            Assert.Equal(2, program.Networks.Count);
            Assert.Empty(program.Networks[0].Code);
            Assert.Equal(0, program.ElementCount);

            // And it still lays out, so the canvas can draw the empty rung.
            Assert.Single(LadderLayout.Build(program.Networks[0]).Rungs);
        }

        [Fact]
        public void A_network_without_a_coil_is_allowed()
        {
            LadderProgram program = Reparse("NETWORK 1\n  A    I.A");
            Assert.Single(program.Networks[0].Code);
        }

        [Fact]
        public void DeleteNetwork_renumbers_the_rest()
        {
            LadderProgram program = Parse(
                "NETWORK 1\n  A I.A\n  = Q.A\n"
                + "NETWORK 2\n  A I.B\n  = Q.B\n"
                + "NETWORK 3\n  A I.C\n  = Q.C");

            LadderProgram after = Reparse(LadderEdit.DeleteNetwork(program, 0));

            Assert.Equal(2, after.Networks.Count);
            Assert.Equal(1, after.Networks[0].Number);
            Assert.Equal(2, after.Networks[1].Number);
            Assert.Equal("I.B", after.Networks[0].Code[0].Operand.ToString());
        }

        [Fact]
        public void SetNetworkTitle_survives_the_round_trip()
        {
            LadderProgram program = Parse("NETWORK 1\n  A I.A\n  = Q.A");
            LadderProgram after = Reparse(LadderEdit.SetNetworkTitle(program, 0, "start / stop"));

            Assert.Equal("start / stop", after.Networks[0].Title);
        }

        // ---- the shape edits leave behind ----

        [Fact]
        public void A_build_up_from_scratch_stays_parseable_at_every_step()
        {
            // Walks the operations a player would actually perform, asserting
            // the program re-parses after each one.
            LadderProgram program = Parse("NETWORK 1\n  A I.A\n  = Q.A");

            program = Reparse(LadderEdit.InsertAfter(program, 0, 0, new LadderInstruction(
                LadderOp.AndNot, LadderOperand.ForInput("B"), null, LadderCompare.Greater, 0, null, -1, 0)));

            program = Reparse(LadderEdit.AddParallelBranch(program, 0, 0, new LadderInstruction(
                LadderOp.And, LadderOperand.ForOutput("A"), null, LadderCompare.Greater, 0, null, -1, 0)));

            program = Reparse(LadderEdit.SetNetworkTitle(program, 0, "start / stop"));

            // Which is exactly the starter program PlcLad ships.
            LadderRungLayout rung = LadderLayout.Build(program.Networks[0]).Rungs[0];
            Assert.Equal(2, rung.Rows);
            Assert.Equal(3, rung.Cells.Count);
            Assert.Single(rung.Coils);
            Assert.Equal(LadderTier.Basic, LadderCapability.Analyze(program).RequiredTier);
        }

        // ---- branching as a scoped operator ----

        [Fact]
        public void AddParallelBranchOver_parallels_a_whole_chain()
        {
            // The seal-in: the holding contact has to sit beside the ENTIRE
            // start chain, not beside its last symbol.  Without a span this
            // shape is simply not authorable in the diagram.
            LadderProgram program = Parse("NETWORK 1\n  A I.A\n  A I.B\n  = Q.A");
            string text = LadderEdit.AddParallelBranchOver(program, 0, 0, 1, new LadderInstruction(
                LadderOp.And, LadderOperand.ForOutput("A"), null, LadderCompare.Greater, 0, null, -1, 0));

            LadderProgram after = Reparse(text);
            LadderRungLayout rung = LadderLayout.Build(after.Networks[0]).Rungs[0];
            Assert.Equal(2, rung.Rows);
            Assert.Equal(3, rung.Cells.Count);
            // Both original contacts stayed on the top row; only the new leg is
            // on the second, so the branch spans the pair.
            Assert.Equal(2, rung.Cells.FindAll(c => c.Row == 0).Count);
            Assert.Single(rung.Cells.FindAll(c => c.Row == 1));
        }

        [Fact]
        public void AddParallelBranchOver_refuses_a_span_that_crosses_a_bracket()
        {
            // Code is [0] A( [1] A I.A [2] O I.B [3] ) [4] A I.C — a span of
            // 2..4 would put a "(" between an existing pair and unbalance it.
            LadderProgram program = Parse("NETWORK 1\n  A(\n  A I.A\n  O I.B\n  )\n  A I.C\n  = Q.A");
            string text = LadderEdit.AddParallelBranchOver(program, 0, 2, 4, new LadderInstruction(
                LadderOp.And, LadderOperand.ForInput("D"), null, LadderCompare.Greater, 0, null, -1, 0));

            Assert.Equal(program.ToText(), text);
        }

        [Fact]
        public void AddParallelBranchOver_refuses_a_span_containing_a_coil()
        {
            LadderProgram program = Parse("NETWORK 1\n  A I.A\n  = Q.A");
            string text = LadderEdit.AddParallelBranchOver(program, 0, 0, 1, new LadderInstruction(
                LadderOp.And, LadderOperand.ForInput("B"), null, LadderCompare.Greater, 0, null, -1, 0));

            Assert.Equal(program.ToText(), text);
        }

        [Fact]
        public void AddParallelBranchOver_refuses_a_coil_as_the_new_leg()
        {
            LadderProgram program = Parse("NETWORK 1\n  A I.A\n  = Q.A");
            string text = LadderEdit.AddParallelBranchOver(program, 0, 0, 0, new LadderInstruction(
                LadderOp.Assign, LadderOperand.ForOutput("B"), null, LadderCompare.Greater, 0, null, -1, 0));

            Assert.Equal(program.ToText(), text);
        }

        [Fact]
        public void GroupStart_stops_at_the_enclosing_bracket()
        {
            // [0] A( [1] A I.A [2] O I.B [3] ) [4] A I.C [5] = Q.A
            LadderProgram program = Parse("NETWORK 1\n  A(\n  A I.A\n  O I.B\n  )\n  A I.C\n  = Q.A");

            // Inside the group, the chain starts just after the "(".
            Assert.Equal(1, LadderEdit.GroupStart(program, 0, 2));
            // Outside it, the group is one opaque step and the chain starts at
            // the head of the network — so a branch over it wraps the lot.
            Assert.Equal(0, LadderEdit.GroupStart(program, 0, 4));
        }

        // ---- outputs are not branches ----

        [Fact]
        public void Stacked_coils_are_drawn_as_rows_but_are_not_logic_rows()
        {
            LadderProgram program = Parse("NETWORK 1\n  A I.A\n  = Q.A\n  = Q.B\n  = Q.C");
            LadderRungLayout rung = LadderLayout.Build(program.Networks[0]).Rungs[0];

            Assert.Equal(3, rung.Rows);
            Assert.Equal(1, rung.LogicRows);
            // Three outputs must not spend the tier's branch budget.
            Assert.Equal(1, LadderCapability.RowsOf(program.Networks[0]));
            Assert.Equal(LadderTier.Basic, LadderCapability.Analyze(program).RequiredTier);
        }

        // ---- per-network actions ----

        [Fact]
        public void InsertNetworkAfter_puts_an_empty_network_below_and_renumbers()
        {
            LadderProgram program = Parse(
                "NETWORK 1  // first\n  A I.A\n  = Q.A\n\nNETWORK 2  // second\n  A I.B\n  = Q.B");
            string text = LadderEdit.InsertNetworkAfter(program, 0, "");

            LadderProgram after = Reparse(text);
            Assert.Equal(3, after.Networks.Count);
            Assert.Empty(after.Networks[1].Code);
            // The one that was network 2 is now network 3, and kept its title.
            Assert.Equal("second", after.Networks[2].Title);
            Assert.Equal(3, after.Networks[2].Number);
        }

        [Fact]
        public void ClearNetwork_empties_it_but_keeps_its_place_and_title()
        {
            LadderProgram program = Parse(
                "NETWORK 1  // latch\n  A I.A\n  = Q.A\n\nNETWORK 2\n  A I.B\n  = Q.B");
            string text = LadderEdit.ClearNetwork(program, 0);

            LadderProgram after = Reparse(text);
            Assert.Equal(2, after.Networks.Count);
            Assert.Empty(after.Networks[0].Code);
            Assert.Equal("latch", after.Networks[0].Title);
            // Nothing below it moved.
            Assert.Equal(2, after.Networks[1].Code.Count);
        }

        [Fact]
        public void Deleting_the_over_limit_network_puts_the_program_back_in_range()
        {
            // The trap this fixes: a third network on a Basic module fails the
            // tier check, and the network is empty so there is nothing in it to
            // select.  Deleting it by index has to be enough to recover.
            LadderProgram program = Parse("NETWORK 1\n  A I.A\n  = Q.A\nNETWORK 2\n  A I.B\n  = Q.B");
            program = Reparse(LadderEdit.AddNetwork(program, ""));
            Assert.Equal(3, program.Networks.Count);

            program = Reparse(LadderEdit.DeleteNetwork(program, 2));
            Assert.Equal(2, program.Networks.Count);
            LadderValidator.Enforce(program, LadderTier.Basic);
        }

        // ---- reordering ----

        [Fact]
        public void MoveElement_swaps_two_contacts_in_a_series()
        {
            LadderProgram program = Parse("NETWORK 1\n  A I.A\n  AN I.B\n  = Q.A");
            LadderProgram after = Reparse(LadderEdit.MoveElement(program, 0, 0, 1));

            Assert.Equal("I.B", after.Networks[0].Code[0].Operand.ToString());
            Assert.Equal("I.A", after.Networks[0].Code[1].Operand.ToString());
            // The KIND travels with the symbol, so the NC contact is still NC.
            Assert.Equal(LadderOp.AndNot, after.Networks[0].Code[0].Op);
            Assert.Equal(LadderOp.And, after.Networks[0].Code[1].Op);
        }

        [Fact]
        public void MoveElement_keeps_the_branch_shape_when_swapping_inside_a_leg()
        {
            // [0] A( [1] A I.A [2] A I.B [3] O I.C [4] ) [5] = Q.A — swapping 1
            // and 2 must leave the first leg two long and the second one long.
            LadderProgram program = Parse(
                "NETWORK 1\n  A(\n  A I.A\n  A I.B\n  O I.C\n  )\n  = Q.A");
            LadderProgram after = Reparse(LadderEdit.MoveElement(program, 0, 1, 1));

            Assert.Equal("I.B", after.Networks[0].Code[1].Operand.ToString());
            Assert.Equal("I.A", after.Networks[0].Code[2].Operand.ToString());
            LadderRungLayout rung = LadderLayout.Build(after.Networks[0]).Rungs[0];
            Assert.Equal(2, rung.Rows);
            Assert.Equal(2, rung.Cells.FindAll(c => c.Row == 0).Count);
        }

        [Fact]
        public void MoveElement_refuses_to_cross_a_bracket_or_the_coil()
        {
            // Index 2 is the last symbol of the first leg; index 3 is ")".
            LadderProgram program = Parse(
                "NETWORK 1\n  A(\n  A I.A\n  A I.B\n  )\n  = Q.A");
            Assert.Equal(program.ToText(), LadderEdit.MoveElement(program, 0, 2, 1));

            // A contact can't swap with the coil that ends the rung.
            LadderProgram plain = Parse("NETWORK 1\n  A I.A\n  = Q.A");
            Assert.Equal(plain.ToText(), LadderEdit.MoveElement(plain, 0, 0, 1));
        }

        [Fact]
        public void MoveElement_reorders_stacked_outputs()
        {
            LadderProgram program = Parse("NETWORK 1\n  A I.A\n  = Q.A\n  S Q.B");
            LadderProgram after = Reparse(LadderEdit.MoveElement(program, 0, 1, 1));

            Assert.Equal(LadderOp.Set, after.Networks[0].Code[1].Op);
            Assert.Equal("Q.B", after.Networks[0].Code[1].Operand.ToString());
            Assert.Equal(LadderOp.Assign, after.Networks[0].Code[2].Op);
        }

        [Fact]
        public void MoveNetwork_reorders_and_renumbers()
        {
            LadderProgram program = Parse(
                "NETWORK 1  // first\n  A I.A\n  = Q.A\n\nNETWORK 2  // second\n  A I.B\n  = Q.B");
            LadderProgram after = Reparse(LadderEdit.MoveNetwork(program, 1, -1));

            Assert.Equal("second", after.Networks[0].Title);
            Assert.Equal(1, after.Networks[0].Number);
            Assert.Equal("first", after.Networks[1].Title);
            Assert.Equal(2, after.Networks[1].Number);
        }

        [Fact]
        public void MoveNetwork_refuses_to_run_off_either_end()
        {
            LadderProgram program = Parse("NETWORK 1\n  A I.A\n  = Q.A\n\nNETWORK 2\n  A I.B\n  = Q.B");

            Assert.Equal(program.ToText(), LadderEdit.MoveNetwork(program, 0, -1));
            Assert.Equal(program.ToText(), LadderEdit.MoveNetwork(program, 1, 1));
        }

        // ---- the document is a DRAFT: edits accumulate on one object ----

        [Fact]
        public void Document_accumulates_edits_without_a_round_trip_through_text()
        {
            // This is the property the editor needs and the text-in/text-out API
            // could not give it: several operations applied in sequence, each one
            // addressing the result of the last, with no re-parse in between.
            LadderDocument document = new LadderDocument(Parse("NETWORK 1\n  A I.A\n  = Q.A"));

            Assert.True(document.InsertAfter(0, 0, new LadderInstruction(
                LadderOp.AndNot, LadderOperand.ForInput("B"), null, LadderCompare.Greater, 0, null, -1, 0)));
            Assert.True(document.AddParallelBranch(0, 0, 0, new LadderInstruction(
                LadderOp.And, LadderOperand.ForOutput("A"), null, LadderCompare.Greater, 0, null, -1, 0)));
            Assert.True(document.AddNetwork("second"));
            Assert.True(document.MoveNetwork(1, -1));

            LadderProgram after = Reparse(document.ToText());
            Assert.Equal(2, after.Networks.Count);
            Assert.Equal("second", after.Networks[0].Title);
            Assert.Empty(after.Networks[0].Code);

            // The seal-in survived intact in the network that moved down.
            LadderRungLayout rung = LadderLayout.Build(after.Networks[1]).Rungs[0];
            Assert.Equal(2, rung.Rows);
            Assert.Equal(3, rung.Cells.Count);
        }

        [Fact]
        public void Document_refusals_leave_it_untouched()
        {
            LadderDocument document = new LadderDocument(Parse("NETWORK 1\n  A I.A\n  = Q.A"));
            string before = document.ToText();

            // A coil can't be a branch leg, and index 1 is the coil anyway.
            Assert.False(document.AddParallelBranch(0, 0, 1, new LadderInstruction(
                LadderOp.And, LadderOperand.ForInput("B"), null, LadderCompare.Greater, 0, null, -1, 0)));
            Assert.False(document.MoveElement(0, 0, 1));
            Assert.False(document.Delete(0, 99));
            Assert.False(document.MoveNetwork(0, 1));

            Assert.Equal(before, document.ToText());
        }

        [Fact]
        public void Document_refuses_to_point_a_coil_at_an_input()
        {
            // The editor offered I.A for a coil, built "= I.A", and the parser
            // rejected it on save. The document has to be the thing that says no,
            // so an edit can never produce a program that won't parse.
            LadderDocument document = new LadderDocument(Parse("NETWORK 1\n  A I.A\n  = Q.A"));

            Assert.False(document.SetOperand(0, 1, LadderOperand.ForInput("A")));
            Assert.False(document.InsertAfter(0, 0, new LadderInstruction(
                LadderOp.Assign, LadderOperand.ForInput("B"), null, LadderCompare.Greater, 0, null, -1, 0)));

            // A contact reading an input is of course fine.
            Assert.True(document.SetOperand(0, 0, LadderOperand.ForInput("B")));
            Reparse(document.ToText());
        }

        // The editor's output position at the right rail anchors on the LAST
        // instruction of the network, whatever it is, so appending a coil has to
        // work when the thing it lands after is another coil.  A rung driving two
        // outputs is two stacked coils, not two rungs.
        [Fact]
        public void A_second_coil_appends_after_the_first()
        {
            LadderDocument document = new LadderDocument(Parse("NETWORK 1\n  A I.A\n  = Q.A"));
            List<LadderInstruction> code = document.Networks[0].Code;

            Assert.True(document.InsertAfter(0, code.Count - 1, new LadderInstruction(
                LadderOp.Assign, LadderOperand.ForOutput("B"), null, LadderCompare.Greater, 0, null, -1, 0)));

            LadderProgram after = Reparse(document.ToText());
            Assert.Equal(LadderOp.Assign, after.Networks[0].Code[2].Op);
            Assert.Equal("Q.B", after.Networks[0].Code[2].Operand.ToString());

            // Both coils hang off the same rung, stacked — the layout must not
            // have split them into two rungs.
            LadderNetworkLayout layout = LadderLayout.Build(after.Networks[0]);
            Assert.Single(layout.Rungs);
            Assert.Equal(2, layout.Rungs[0].Coils.Count);
            Assert.Equal(1, layout.Rungs[0].LogicRows);
        }

        // A coil-less network is what the editor draws while the player is still
        // building the logic, and the output position stays clickable throughout —
        // so appending the rung's first coil after a closing bracket has to work.
        [Fact]
        public void The_first_coil_appends_after_a_branch()
        {
            LadderDocument document = new LadderDocument(
                Parse("NETWORK 1\n  A(\n  O I.A\n  O I.B\n  )"));
            List<LadderInstruction> code = document.Networks[0].Code;

            Assert.True(document.InsertAfter(0, code.Count - 1, new LadderInstruction(
                LadderOp.Assign, LadderOperand.ForOutput("A"), null, LadderCompare.Greater, 0, null, -1, 0)));

            LadderProgram after = Reparse(document.ToText());
            Assert.Equal(LadderOp.Assign, after.Networks[0].Code[4].Op);
            Assert.Single(LadderLayout.Build(after.Networks[0]).Rungs[0].Coils);
        }

        // ---- moving where a branch rejoins the main line ----

        // The diagram shows this as the closing junction bar sliding right: the
        // symbol that was past it is now inside the parallel section, on the
        // main line's leg.
        [Fact]
        public void MoveBranchEnd_right_pulls_the_next_symbol_into_the_first_leg()
        {
            // (I.A OR Q.A) AND NOT I.B -> Q.A
            LadderDocument document = new LadderDocument(
                Parse("NETWORK 1\n  A(\n  A I.A\n  O Q.A\n  )\n  AN I.B\n  = Q.A"));

            Assert.True(document.MoveBranchEnd(0, 1, 1));

            LadderProgram after = Reparse(document.ToText());
            List<LadderInstruction> code = after.Networks[0].Code;
            // ((I.A AND NOT I.B) OR Q.A) -> Q.A
            Assert.Equal(LadderOp.OpenAnd, code[0].Op);
            Assert.Equal("I.A", code[1].Operand.ToString());
            Assert.Equal(LadderOp.AndNot, code[2].Op);
            Assert.Equal("I.B", code[2].Operand.ToString());
            Assert.Equal(LadderOp.Or, code[3].Op);
            Assert.Equal(LadderOp.Close, code[4].Op);
            Assert.Equal(LadderOp.Assign, code[5].Op);

            // Two legs still, and the first one is now two symbols wide — which
            // is exactly the bar having moved one column right.
            LadderRungLayout rung = LadderLayout.Build(after.Networks[0]).Rungs[0];
            Assert.Equal(2, rung.LogicRows);
            // Still one bracket level — sliding the bar reshapes the branch, it
            // does not nest a new one.
            Assert.Equal(1, LadderCapability.NestingOf(after.Networks[0]));
        }

        // A group whose first element carries an O connector draws as TWO legs —
        // the layout only starts a new leg when there is already something on the
        // current one.  Reading it as three split the top leg apart: the pulled-in
        // symbol landed ahead of the first element instead of after it, and the
        // branch grew a row instead of a column.
        [Fact]
        public void MoveBranchEnd_right_reads_the_first_leg_the_way_the_diagram_draws_it()
        {
            LadderDocument document = new LadderDocument(
                Parse("NETWORK 1\n  A(\n  O I.A\n  O Q.A\n  )\n  AN I.B\n  = Q.A"));
            Assert.Equal(2, LadderLayout.Build(document.Networks[0]).Rungs[0].LogicRows);

            Assert.True(document.MoveBranchEnd(0, 1, 1));

            LadderProgram after = Reparse(document.ToText());
            List<LadderInstruction> code = after.Networks[0].Code;
            Assert.Equal("I.A", code[1].Operand.ToString());
            Assert.Equal(LadderOp.AndNot, code[2].Op);
            Assert.Equal("I.B", code[2].Operand.ToString());
            Assert.Equal(LadderOp.Or, code[3].Op);

            // Still two legs, the first one now two symbols wide. Three rows here
            // would mean the branch had gained a leg it was never asked for.
            LadderRungLayout rung = LadderLayout.Build(after.Networks[0]).Rungs[0];
            Assert.Equal(2, rung.LogicRows);
        }

        [Fact]
        public void MoveBranchEnd_left_puts_it_back()
        {
            string before = "NETWORK 1\n  A(\n  A I.A\n  O Q.A\n  )\n  AN I.B\n  = Q.A";
            LadderDocument document = new LadderDocument(Parse(before));

            Assert.True(document.MoveBranchEnd(0, 1, 1));
            Assert.True(document.MoveBranchEnd(0, 1, -1));

            // A round trip is the same ladder, so the text has to match what the
            // parser emits for the original.
            Assert.Equal(Parse(before).ToText(), document.ToText());
        }

        [Fact]
        public void MoveBranchEnd_will_not_empty_the_first_leg()
        {
            // Moving left here would leave the top leg with nothing on it, which
            // is not a branch any more — the operation refuses rather than
            // collapsing something the player did not ask to collapse.
            LadderDocument document = new LadderDocument(
                Parse("NETWORK 1\n  A(\n  A I.A\n  O Q.A\n  )\n  = Q.A"));
            string before = document.ToText();

            Assert.False(document.CanMoveBranchEnd(0, 1, -1));
            Assert.False(document.MoveBranchEnd(0, 1, -1));
            Assert.Equal(before, document.ToText());
        }

        [Fact]
        public void MoveBranchEnd_stops_at_the_coil()
        {
            // Right of the bar is the rung's coil. Dragging the junction past it
            // would put an output inside a parallel section.
            LadderDocument document = new LadderDocument(
                Parse("NETWORK 1\n  A(\n  A I.A\n  O Q.A\n  )\n  = Q.A"));
            string before = document.ToText();

            Assert.False(document.CanMoveBranchEnd(0, 1, 1));
            Assert.False(document.MoveBranchEnd(0, 1, 1));
            Assert.Equal(before, document.ToText());
        }

        [Fact]
        public void MoveBranchEnd_refuses_outside_a_branch()
        {
            LadderDocument document = new LadderDocument(Parse("NETWORK 1\n  A I.A\n  = Q.A"));

            Assert.False(document.CanMoveBranchEnd(0, 0, 1));
            Assert.False(document.MoveBranchEnd(0, 0, 1));
        }

        // ---- moving where a branch splits ----

        [Fact]
        public void MoveBranchStart_left_pulls_the_symbol_in_front_into_the_branch()
        {
            // I.A AND (I.B OR Q.A) -> Q.A, with the branch starting after I.A.
            LadderDocument document = new LadderDocument(
                Parse("NETWORK 1\n  A I.A\n  A(\n  A I.B\n  O Q.A\n  )\n  = Q.A"));

            Assert.True(document.MoveBranchStart(0, 3, -1));

            LadderProgram after = Reparse(document.ToText());
            List<LadderInstruction> code = after.Networks[0].Code;
            // (I.A AND I.B) OR Q.A -> Q.A: the bypass now goes around I.A too.
            Assert.Equal(LadderOp.OpenAnd, code[0].Op);
            Assert.Equal("I.A", code[1].Operand.ToString());
            Assert.Equal("I.B", code[2].Operand.ToString());
            Assert.Equal(LadderOp.Or, code[3].Op);
            Assert.Equal(LadderOp.Close, code[4].Op);
            Assert.Equal(2, LadderLayout.Build(after.Networks[0]).Rungs[0].LogicRows);
        }

        [Fact]
        public void MoveBranchStart_right_puts_it_back()
        {
            string before = "NETWORK 1\n  A I.A\n  A(\n  A I.B\n  O Q.A\n  )\n  = Q.A";
            LadderDocument document = new LadderDocument(Parse(before));

            Assert.True(document.MoveBranchStart(0, 3, -1));
            Assert.True(document.MoveBranchStart(0, 2, 1));

            Assert.Equal(Parse(before).ToText(), document.ToText());
        }

        [Fact]
        public void MoveBranchStart_will_not_empty_the_first_leg()
        {
            LadderDocument document = new LadderDocument(
                Parse("NETWORK 1\n  A(\n  A I.A\n  O Q.A\n  )\n  = Q.A"));
            string before = document.ToText();

            // Nothing in front of the branch to pull in, and the top leg is one
            // symbol wide so nothing can leave it either.
            Assert.False(document.CanMoveBranchStart(0, 1, -1));
            Assert.False(document.CanMoveBranchStart(0, 1, 1));
            Assert.False(document.MoveBranchStart(0, 1, 1));
            Assert.Equal(before, document.ToText());
        }

        // ---- moving a leg's distance from the main line ----

        [Fact]
        public void MoveBranchLeg_swaps_a_leg_with_its_neighbour()
        {
            LadderDocument document = new LadderDocument(
                Parse("NETWORK 1\n  A(\n  A I.A\n  O I.B\n  O I.C\n  )\n  = Q.A"));

            // Promote the middle leg onto the main line.
            Assert.True(document.MoveBranchLeg(0, 0, 2, -1));

            LadderProgram after = Reparse(document.ToText());
            List<LadderInstruction> code = after.Networks[0].Code;
            Assert.Equal("I.B", code[1].Operand.ToString());
            Assert.Equal(LadderOp.And, code[1].Op);
            Assert.Equal("I.A", code[2].Operand.ToString());
            Assert.Equal(LadderOp.Or, code[2].Op);
            Assert.Equal("I.C", code[3].Operand.ToString());
            Assert.Equal(LadderOp.Or, code[3].Op);

            // Three legs still — swapping rows must not merge two of them into a
            // series, which is what dropping the connector fix-up would do.
            Assert.Equal(3, LadderLayout.Build(after.Networks[0]).Rungs[0].LogicRows);
        }

        [Fact]
        public void MoveBranchLeg_carries_the_whole_leg_not_just_its_head()
        {
            LadderDocument document = new LadderDocument(
                Parse("NETWORK 1\n  A(\n  A I.A\n  O I.B\n  A I.C\n  )\n  = Q.A"));

            // Leg 1 is "I.B I.C" in series; it has to arrive on row 0 intact.
            Assert.True(document.MoveBranchLeg(0, 0, 2, -1));

            LadderProgram after = Reparse(document.ToText());
            LadderRungLayout rung = LadderLayout.Build(after.Networks[0]).Rungs[0];
            Assert.Equal(2, rung.LogicRows);
            Assert.Equal(2, rung.Cells.FindAll(c => c.Row == 0).Count);
            Assert.Equal("I.B", rung.Cells.Find(c => c.Row == 0 && c.Column == 0).Instruction.Operand.ToString());
            Assert.Equal("I.C", rung.Cells.Find(c => c.Row == 0 && c.Column == 1).Instruction.Operand.ToString());
            Assert.Equal("I.A", rung.Cells.Find(c => c.Row == 1).Instruction.Operand.ToString());
        }

        [Fact]
        public void MoveBranchLeg_stops_at_the_ends()
        {
            LadderDocument document = new LadderDocument(
                Parse("NETWORK 1\n  A(\n  A I.A\n  O I.B\n  )\n  = Q.A"));
            string before = document.ToText();

            Assert.False(document.CanMoveBranchLeg(0, 0, 1, -1));
            Assert.False(document.CanMoveBranchLeg(0, 0, 2, 1));
            Assert.False(document.MoveBranchLeg(0, 0, 1, -1));
            Assert.Equal(before, document.ToText());

            Assert.True(document.CanMoveBranchLeg(0, 0, 1, 1));
            Assert.True(document.CanMoveBranchLeg(0, 0, 2, -1));
        }

        [Fact]
        public void CanAddress_matches_what_the_parser_accepts()
        {
            Assert.True(LadderDocument.CanAddress(LadderOp.And, LadderOperand.ForInput("A")));
            Assert.True(LadderDocument.CanAddress(LadderOp.Assign, LadderOperand.ForOutput("A")));
            Assert.True(LadderDocument.CanAddress(LadderOp.Toggle, LadderOperand.ForMemory(3)));
            Assert.False(LadderDocument.CanAddress(LadderOp.Assign, LadderOperand.ForInput("A")));
            Assert.False(LadderDocument.CanAddress(LadderOp.Set, LadderOperand.ForInput("A")));
            Assert.False(LadderDocument.CanAddress(LadderOp.And, null));
        }

        // ---- empty networks are free ----

        [Fact]
        public void An_empty_network_is_not_charged_against_the_tier()
        {
            // Basic runs two networks. Adding a third EMPTY one must not stop the
            // module: it draws nothing, runs nothing and costs nothing, and it is
            // what "+ Network" deliberately creates.
            LadderProgram program = Parse(
                "NETWORK 1\n  A I.A\n  = Q.A\n\nNETWORK 2\n  A I.B\n  = Q.B");
            program = Reparse(LadderEdit.AddNetwork(program, ""));

            Assert.Equal(3, program.Networks.Count);
            Assert.Equal(2, LadderCapability.EffectiveNetworkCount(program));
            LadderValidator.Enforce(program, LadderTier.Basic);
            Assert.Equal(LadderTier.Basic, LadderCapability.Analyze(program).RequiredTier);
        }

        // ---- how many legs, and what a branch is scoped to ----
        //
        // Two questions worth settling against the real thing. Siemens caps
        // NESTING at seven nest-stack entries (A( / O(), but a chain of plain O
        // terms costs no stack at all — so legs-per-branch are unbounded and it
        // is depth that is limited. Rockwell says the same from the other side:
        // seven nested branch LEVELS per rung, with "add branch level" adding
        // legs freely. And a branch is anchored to a span of the rung, not to the
        // rung as a whole: several independent ones can sit on one rung.

        [Fact]
        public void A_branch_can_carry_more_than_two_legs()
        {
            LadderProgram program = Parse("NETWORK 1\n  A(\n  A I.A\n  O I.B\n  )\n  = Q.A");
            LadderDocument document = new LadderDocument(program);

            // Wrapping the group's CONTENTS (both legs) adds a flat third leg.
            int start = document.GroupStart(0, 1);
            int end = document.GroupEnd(0, 1);
            Assert.Equal(1, start);
            Assert.Equal(2, end);
            Assert.True(document.AddParallelBranch(0, start, end, new LadderInstruction(
                LadderOp.And, LadderOperand.ForInput("C"), null, LadderCompare.Greater, 0, null, -1, 0)));

            LadderProgram after = Reparse(document.ToText());
            LadderRungLayout rung = LadderLayout.Build(after.Networks[0]).Rungs[0];
            Assert.Equal(3, rung.Rows);
            Assert.Equal(3, rung.Cells.Count);
            // Flat, not nested: one bracket pair for three legs.
            Assert.Single(after.Networks[0].Code.FindAll(i => i.Op == LadderOp.OpenAnd));

            // And it keeps going — a fourth leg is no different from a third.
            document = new LadderDocument(after);
            Assert.True(document.AddParallelBranch(0, document.GroupStart(0, 1), document.GroupEnd(0, 1),
                new LadderInstruction(
                    LadderOp.And, LadderOperand.ForInput("D"), null, LadderCompare.Greater, 0, null, -1, 0)));
            Assert.Equal(4, LadderLayout.Build(Reparse(document.ToText()).Networks[0]).Rungs[0].Rows);
        }

        [Fact]
        public void Wrapping_one_leg_nests_instead_of_adding_a_leg()
        {
            // The distinction the two options in the wizard rest on: wrapping a
            // single leg builds a branch INSIDE that leg, which draws the same
            // number of rows but is a different circuit to read.
            LadderProgram program = Parse("NETWORK 1\n  A(\n  A I.A\n  O I.B\n  )\n  = Q.A");
            LadderProgram after = Reparse(LadderEdit.AddParallelBranchOver(program, 0, 1, 1,
                new LadderInstruction(
                    LadderOp.And, LadderOperand.ForInput("C"), null, LadderCompare.Greater, 0, null, -1, 0)));

            Assert.Equal(3, LadderLayout.Build(after.Networks[0]).Rungs[0].Rows);
            // Two bracket pairs — the new one is nested inside the old.
            Assert.Equal(2, after.Networks[0].Code.FindAll(
                i => i.Op == LadderOp.OpenAnd || i.Op == LadderOp.OpenOr).Count);
        }

        [Fact]
        public void Branching_is_scoped_to_a_span_so_one_rung_can_hold_several()
        {
            // Not per network, and not per rung: a branch wraps a run of columns,
            // so two independent ones sit side by side on the same rung.
            LadderProgram program = Parse(
                "NETWORK 1\n  A(\n  A I.A\n  O I.B\n  )\n  A(\n  A I.C\n  O I.D\n  )\n  = Q.A");

            LadderRungLayout rung = LadderLayout.Build(program.Networks[0]).Rungs[0];
            Assert.Equal(2, rung.Rows);
            Assert.Equal(4, rung.Cells.Count);
            // Two opening bars and two closing bars, at different columns.
            List<LadderBranchSpan> opening = rung.Branches.FindAll(b => b.IsOpening);
            Assert.Equal(2, opening.Count);
            Assert.NotEqual(opening[0].Column, opening[1].Column);
        }

        [Fact]
        public void Branches_nest_and_each_level_costs_a_row()
        {
            LadderProgram program = Parse(
                "NETWORK 1\n  A(\n  A(\n  A I.A\n  O I.B\n  )\n  O I.C\n  )\n  = Q.A");

            Assert.Equal(3, LadderCapability.RowsOf(program.Networks[0]));
        }

        [Fact]
        public void A_third_filled_network_still_needs_Extended()
        {
            LadderProgram program = Parse(
                "NETWORK 1\n  A I.A\n  = Q.A\n\nNETWORK 2\n  A I.B\n  = Q.B\n\nNETWORK 3\n  A I.C\n  = Q.C");

            Assert.Equal(3, LadderCapability.EffectiveNetworkCount(program));
            Assert.Throws<LadderParseException>(() => LadderValidator.Enforce(program, LadderTier.Basic));
            LadderValidator.Enforce(program, LadderTier.Extended);
        }
    }
}
