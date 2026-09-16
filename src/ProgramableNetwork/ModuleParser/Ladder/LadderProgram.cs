using System.Collections.Generic;
using System.Text;

namespace ProgramableNetwork.Ladder;

// A compiled ladder program: the networks in scan order, plus the sizes the
// runtime needs up front.
//
// This object is immutable and holds no per-tick state — LadderMachine owns
// that — so the module can cache one instance across ticks and only rebuild it
// when the source text changes, exactly like PlcPy caches its parsed Block.
public sealed class LadderProgram
{
	public readonly List<LadderNetwork> Networks;

	// How many ints LadderMachine needs to persist edges, timers and counters.
	public readonly int StateSlotCount;

	// Non-structural instructions across the whole program.  Drives the
	// module's computing cost and the "N elements" readout in the editor.
	public readonly int ElementCount;

	public LadderProgram(List<LadderNetwork> networks, int stateSlotCount, int elementCount)
	{
		Networks = networks;
		StateSlotCount = stateSlotCount;
		ElementCount = elementCount;
	}

	// Canonical text, byte-for-byte re-parseable.  The graphical editor edits
	// the object graph and writes it back through here; that text is what
	// lands in the module's `code` field.
	public string ToText()
	{
		return TextOf(Networks);
	}

	// Same rendering, from a bare network list.  The editor's operations build
	// modified lists and need text back without first constructing a Program
	// (whose slot / element counts would be stale anyway — they're recomputed
	// when the text is re-parsed).
	public static string TextOf(List<LadderNetwork> networks)
	{
		StringBuilder sb = new StringBuilder();
		for (int n = 0; n < networks.Count; n++)
		{
			LadderNetwork network = networks[n];
			if (n > 0)
			{
				sb.Append('\n');
			}
			sb.Append("NETWORK ").Append(network.Number);
			if (network.Title.Length > 0)
			{
				sb.Append("  // ").Append(network.Title);
			}
			sb.Append('\n');

			foreach (LadderInstruction instruction in network.Code)
			{
				sb.Append("  ").Append(TextOf(instruction)).Append('\n');
			}
		}
		return sb.ToString().TrimEnd('\n');
	}

	private static string TextOf(LadderInstruction instruction)
	{
		string mnemonic = LadderInstruction.MnemonicOf(instruction.Op);
		// NOT joins the brackets in taking no operand, so it renders bare rather
		// than as a mnemonic padded out to an empty argument column.
		if (LadderInstruction.IsStructural(instruction.Op) || instruction.Op == LadderOp.Not)
		{
			return mnemonic;
		}

		StringBuilder sb = new StringBuilder();
		sb.Append(mnemonic.PadRight(5));

		switch (instruction.Op)
		{
		case LadderOp.TimerOn:
		case LadderOp.TimerOff:
		case LadderOp.TimerPulse:
			sb.Append("PT:=").Append(instruction.Preset);
			break;
		case LadderOp.CountUp:
			sb.Append("PV:=").Append(instruction.Preset);
			if (instruction.Reset != null)
			{
				sb.Append(" R:=").Append(instruction.Reset);
			}
			break;
		case LadderOp.Compare:
			sb.Append(instruction.Operand)
				.Append(' ')
				.Append(LadderInstruction.SymbolOf(instruction.Compare))
				.Append(' ')
				.Append(instruction.Right);
			break;
		default:
			sb.Append(instruction.Operand);
			break;
		}
		return sb.ToString();
	}
}
