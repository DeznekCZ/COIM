using System.Collections.Generic;

namespace ProgramableNetwork.Ladder;

// One statement-list opcode.  The ladder grid and this list are the same
// program: every graphical element maps to exactly one instruction, and the
// bracket pair (OpenAnd / OpenOr / Close) is how a parallel branch is spelled
// in text.  Siemens does the same thing with LAD and STL, which is why the
// text form is what gets persisted.
public enum LadderOp : byte
{
	// Contacts.  "Not" variants are the normally-closed symbol.
	And = 0,
	AndNot = 1,
	Or = 2,
	OrNot = 3,

	// Branch grouping — OpenAnd is "A(", OpenOr is "O(", Close is ")".
	OpenAnd = 4,
	OpenOr = 5,
	Close = 6,

	// Edge detection on the operand; one scan of power per transition.
	EdgeRising = 7,
	EdgeFalling = 8,

	// Coils.
	Assign = 9,
	Set = 10,
	Reset = 11,

	// Boxes.  Each consumes the rung's power as its enable and replaces it
	// with its own output.
	TimerOn = 12,
	TimerOff = 13,
	CountUp = 14,
	Compare = 15,

	// Inverts the rung's power where it stands.  Takes no address — it shapes
	// the signal rather than reading one, which is why it is neither a contact
	// nor a box.
	Not = 16,

	// Negated coil: writes the INVERSE of the rung every scan.
	AssignNot = 17,

	// Toggle coil: flips its address on each rising edge of the rung.  Holds
	// its own output in state rather than reading the address back, so it stays
	// correct even when something else writes the same address.
	Toggle = 18,

	// Pulse timer: a rising edge on the rung starts a fixed-length pulse, and
	// the pulse runs to completion whatever the rung does afterwards.
	TimerPulse = 19,
}

public enum LadderCompare : byte
{
	Greater = 0,
	Less = 1,
	Equal = 2,
	GreaterOrEqual = 3,
	LessOrEqual = 4,
	NotEqual = 5,
}

public sealed class LadderInstruction
{
	public readonly LadderOp Op;

	// Contacts / coils: what is read or driven.  Compare: the left argument.
	// Null for the bracket opcodes.
	public readonly LadderOperand Operand;

	// Compare only: the right argument and the test.
	public readonly LadderOperand Right;
	public readonly LadderCompare Compare;

	// TimerOn / TimerOff: PT in ticks.  CountUp: PV.
	public readonly int Preset;

	// CountUp only: optional reset input; null when the counter has none.
	public readonly LadderOperand Reset;

	// First index into the machine's persistent state array, or -1 when this
	// instruction is stateless.  Edges and timers claim one slot, counters two.
	public readonly int StateSlot;

	// 1-based source line, for error messages and editor highlighting.
	public readonly int Line;

	public LadderInstruction(
		LadderOp op,
		LadderOperand operand,
		LadderOperand right,
		LadderCompare compare,
		int preset,
		LadderOperand reset,
		int stateSlot,
		int line)
	{
		Op = op;
		Operand = operand;
		Right = right;
		Compare = compare;
		Preset = preset;
		Reset = reset;
		StateSlot = stateSlot;
		Line = line;
	}

	// How many state slots an opcode needs.  Kept next to the enum so adding
	// a stateful instruction can't forget to size the array.
	public static int StateSlotsFor(LadderOp op)
	{
		switch (op)
		{
		case LadderOp.EdgeRising:
		case LadderOp.EdgeFalling:
		case LadderOp.TimerOn:
		case LadderOp.TimerOff:
			return 1;
		// count + previous edge state
		case LadderOp.CountUp:
		// output value + previous rung state
		case LadderOp.Toggle:
		// ticks remaining + previous rung state
		case LadderOp.TimerPulse:
			return 2;
		default:
			return 0;
		}
	}

	// Brackets are structure, not elements — they don't draw a symbol in the
	// ladder view and don't count toward the module's computing cost.
	public static bool IsStructural(LadderOp op)
	{
		return op == LadderOp.OpenAnd || op == LadderOp.OpenOr || op == LadderOp.Close;
	}

	// Writes an address instead of reading one, and so belongs at the right rail
	// at the end of a rung.
	//
	// Every layer used to carry its own copy of this test, which is exactly the
	// kind of duplication that lets a new opcode be half-added.  One definition,
	// used by the layout, the edit ops and the editor alike.
	public static bool IsCoil(LadderOp op)
	{
		return op == LadderOp.Assign
			|| op == LadderOp.Set
			|| op == LadderOp.Reset
			|| op == LadderOp.AssignNot
			|| op == LadderOp.Toggle;
	}

	// Draws as a rectangle with parameters rather than as a contact, and
	// replaces the rung's power with its own result.
	public static bool IsBox(LadderOp op)
	{
		return op == LadderOp.TimerOn
			|| op == LadderOp.TimerOff
			|| op == LadderOp.TimerPulse
			|| op == LadderOp.CountUp
			|| op == LadderOp.Compare;
	}

	public static string MnemonicOf(LadderOp op)
	{
		switch (op)
		{
		case LadderOp.And:
			return "A";
		case LadderOp.AndNot:
			return "AN";
		case LadderOp.Or:
			return "O";
		case LadderOp.OrNot:
			return "ON";
		case LadderOp.OpenAnd:
			return "A(";
		case LadderOp.OpenOr:
			return "O(";
		case LadderOp.Close:
			return ")";
		case LadderOp.EdgeRising:
			return "FP";
		case LadderOp.EdgeFalling:
			return "FN";
		case LadderOp.Assign:
			return "=";
		case LadderOp.Set:
			return "S";
		case LadderOp.Reset:
			return "R";
		case LadderOp.TimerOn:
			return "TON";
		case LadderOp.TimerOff:
			return "TOF";
		case LadderOp.CountUp:
			return "CTU";
		case LadderOp.TimerPulse:
			return "TP";
		case LadderOp.Not:
			return "NOT";
		case LadderOp.AssignNot:
			return "=N";
		case LadderOp.Toggle:
			return "T";
		default:
			return "CMP";
		}
	}

	public static string SymbolOf(LadderCompare compare)
	{
		switch (compare)
		{
		case LadderCompare.Greater:
			return ">";
		case LadderCompare.Less:
			return "<";
		case LadderCompare.Equal:
			return "=";
		case LadderCompare.GreaterOrEqual:
			return ">=";
		case LadderCompare.LessOrEqual:
			return "<=";
		default:
			return "<>";
		}
	}
}

// A numbered rung group.  Networks run top to bottom, once per module tick.
public sealed class LadderNetwork
{
	public readonly int Number;
	public readonly string Title;
	public readonly List<LadderInstruction> Code;

	public LadderNetwork(int number, string title, List<LadderInstruction> code)
	{
		Number = number;
		Title = title ?? "";
		Code = code;
	}
}
