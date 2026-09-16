using System.Collections.Generic;

namespace ProgramableNetwork.Ladder;

// How much ladder a module is allowed to run.
//
// A tier is a PROTOTYPE, not a runtime property: each one registers as its own
// ModuleProto with its own research unlock, so an unresearched tier is absent
// from the module picker entirely and its instructions simply don't exist for
// the player yet.  The three protos share a swap group, so upgrading a placed
// ladder is a swap that keeps the cables and the program text.
//
//   Basic     two networks, five channels, the module's own pins only.
//             Enough for an interlock; no way to remember anything.
//   Extended  memory bits and edge detection — the point where a ladder stops
//             being combinational and starts holding state.
//   Full      timers, counters, compares, and the controller bus.
//
// LadderCapability.Analyze reports the lowest tier a given program needs, which
// is what turns "that instruction is unavailable" into "swap this module up to
// Extended" in the error text.
public enum LadderTier : byte
{
	Basic = 0,
	Extended = 1,
	Full = 2,
}

// The hard ceilings for one tier.
public sealed class LadderLimits
{
	// A "channel" is one element column across a network — the rung's width.
	// Series depth and parallel branch length both count against it, so five
	// channels means at most five symbols between the two rails.
	public readonly int MaxNetworks;
	public readonly int MaxChannels;

	// How deeply branches may be NESTED inside one network.
	//
	// This is the Siemens model, and it is deliberately not a cap on how many
	// parallel legs a branch has.  In STL a chain of plain `O` terms costs
	// nothing — any number of legs ride one bracket pair — while every `A(` /
	// `O(` pushes an entry onto a nesting stack that is only seven deep.  Legs
	// are free; depth is the scarce resource.  Ours is scarcer still, and scaled
	// to the tier.
	//
	// Width is already bounded by MaxChannels, which counts every symbol in the
	// network — so a wide branch cannot run away even though its legs are free.
	// Depth 1 is a plain branch; depth 2 is a branch inside a branch.
	public readonly int MaxNesting;

	public readonly bool AllowMemory;
	public readonly bool AllowEdges;
	public readonly bool AllowBus;
	public readonly bool AllowBoxes;

	private LadderLimits(
		int maxNetworks, int maxChannels, int maxNesting, bool memory, bool edges, bool bus, bool boxes)
	{
		MaxNetworks = maxNetworks;
		MaxChannels = maxChannels;
		MaxNesting = maxNesting;
		AllowMemory = memory;
		AllowEdges = edges;
		AllowBus = bus;
		AllowBoxes = boxes;
	}

	public static LadderLimits For(LadderTier tier)
	{
		switch (tier)
		{
		case LadderTier.Basic:
			// Contacts and coils wired straight to the module's own pins, with
			// branches that may nest two deep.  Enough to build an interlock, not
			// enough to build state.
			return new LadderLimits(2, 5, 2, memory: false, edges: false, bus: false, boxes: false);
		case LadderTier.Extended:
			// Latching and edges — the point where a ladder stops being
			// combinational and starts remembering things.
			return new LadderLimits(4, 8, 4, memory: true, edges: true, bus: false, boxes: false);
		default:
			// Timers, counters, compares, and the controller bus.
			return new LadderLimits(8, 12, 6, memory: true, edges: true, bus: true, boxes: true);
		}
	}

	public static string NameOf(LadderTier tier)
	{
		switch (tier)
		{
		case LadderTier.Basic:
			return "Basic";
		case LadderTier.Extended:
			return "Extended";
		default:
			return "Full";
		}
	}
}

// Why a program needs the tier it needs.  Surfaced to the player verbatim when
// the controller can't supply the computing, so it has to read as an
// explanation rather than a diagnostic code.
public sealed class LadderCapabilityReport
{
	public readonly LadderTier RequiredTier;
	public readonly string Reason;
	public readonly int NetworkCount;
	public readonly int WidestNetwork;
	public readonly int DeepestNetwork;

	public LadderCapabilityReport(
		LadderTier tier, string reason, int networkCount, int widestNetwork, int deepestNetwork)
	{
		RequiredTier = tier;
		Reason = reason ?? "";
		NetworkCount = networkCount;
		WidestNetwork = widestNetwork;
		DeepestNetwork = deepestNetwork;
	}
}

public static class LadderCapability
{
	// Lowest tier that can run this program, plus the first thing that pushed
	// it there.  Structural size and feature use are weighed together, so a
	// three-network program of plain contacts and a two-network program with
	// one timer both land above Basic — for different stated reasons.
	public static LadderCapabilityReport Analyze(LadderProgram program)
	{
		LadderTier tier = LadderTier.Basic;
		string reason = "contacts and coils on the module's own pins";

		int widest = 0;
		int deepest = 0;
		foreach (LadderNetwork network in program.Networks)
		{
			int channels = ChannelsOf(network);
			if (channels > widest)
			{
				widest = channels;
			}
			int nesting = NestingOf(network);
			if (nesting > deepest)
			{
				deepest = nesting;
			}
		}
		int networks = EffectiveNetworkCount(program);

		// Feature use first — a named feature is a better explanation than
		// "your program got wide", so it wins the Reason slot when both apply.
		foreach (LadderNetwork network in program.Networks)
		{
			foreach (LadderInstruction instruction in network.Code)
			{
				LadderTier needed = TierForInstruction(instruction, out string why);
				if (needed > tier)
				{
					tier = needed;
					reason = why;
				}
			}
		}

		LadderTier sizeTier = TierForSize(networks, widest, deepest, out string sizeWhy);
		if (sizeTier > tier)
		{
			tier = sizeTier;
			reason = sizeWhy;
		}

		return new LadderCapabilityReport(tier, reason, networks, widest, deepest);
	}

	// Networks that actually do something.
	//
	// An EMPTY network is not charged against the tier's budget. It draws no
	// symbols, runs no instructions and costs no computing — and it is the normal
	// intermediate state of graphical authoring, since "+ Network" deliberately
	// adds an empty one and waits for the player to fill it. Counting it would
	// make adding a network fail the tier check before the player had put
	// anything in it, which turns a routine step into a dead end.
	public static int EffectiveNetworkCount(LadderProgram program)
	{
		int count = 0;
		foreach (LadderNetwork network in program.Networks)
		{
			if (ChannelsOf(network) > 0)
			{
				count++;
			}
		}
		return count;
	}

	// Elements between the rails, brackets excluded.  This is the number the
	// tier's MaxChannels is compared against.
	public static int ChannelsOf(LadderNetwork network)
	{
		int n = 0;
		foreach (LadderInstruction instruction in network.Code)
		{
			if (!LadderInstruction.IsStructural(instruction.Op))
			{
				n++;
			}
		}
		return n;
	}

	// How deeply this network nests its branches — the deepest bracket depth it
	// reaches.  Compared against the tier's MaxNesting.
	//
	// This is counted PER NETWORK, the same scope Siemens gives its nesting
	// stack: the stack is saved and restored inside one network's scan, so what
	// another network does with brackets is none of this one's business.
	//
	// Legs are NOT counted.  Six contacts in parallel are one bracket pair and
	// one level, exactly as `A( A a O b O c O d O e O f )` is one nest-stack
	// entry in STL. Width is bounded by MaxChannels instead, which counts every
	// symbol in the network regardless of how they are wired.
	public static int NestingOf(LadderNetwork network)
	{
		int depth = 0;
		int deepest = 0;
		foreach (LadderInstruction instruction in network.Code)
		{
			if (instruction.Op == LadderOp.OpenAnd || instruction.Op == LadderOp.OpenOr)
			{
				depth++;
				if (depth > deepest)
				{
					deepest = depth;
				}
			}
			else if (instruction.Op == LadderOp.Close)
			{
				depth--;
			}
		}
		return deepest;
	}

	// Parallel paths through a network, counted as the LOGIC rows its diagram
	// occupies.  This is how TALL the network draws — the editor uses it for
	// layout; it is no longer what the tier charges for.
	//
	// Coils are excluded on purpose.  Driving three outputs from one rung stacks
	// three coils and makes the rung three rows tall, but that is an output
	// branch, not a branch in the logic — every real LAD editor treats the two
	// separately.
	public static int RowsOf(LadderNetwork network)
	{
		int rows = 0;
		foreach (LadderRungLayout rung in LadderLayout.Build(network).Rungs)
		{
			if (rung.LogicRows > rows)
			{
				rows = rung.LogicRows;
			}
		}
		return rows;
	}

	private static LadderTier TierForInstruction(LadderInstruction instruction, out string reason)
	{
		switch (instruction.Op)
		{
		case LadderOp.TimerOn:
		case LadderOp.TimerOff:
			reason = "a timer box";
			return LadderTier.Full;
		case LadderOp.TimerPulse:
			reason = "a pulse timer box";
			return LadderTier.Full;
		case LadderOp.Toggle:
			// A toggle coil remembers its own output between scans, which is the
			// same line edges sit on: it is the point a ladder stops being
			// combinational.
			reason = "a toggle coil";
			return LadderTier.Extended;
		case LadderOp.CountUp:
			reason = "a counter box";
			return LadderTier.Full;
		case LadderOp.Compare:
			reason = "a compare box";
			return LadderTier.Full;
		case LadderOp.EdgeRising:
		case LadderOp.EdgeFalling:
			reason = "edge detection";
			return LadderTier.Extended;
		}

		LadderTier tier = LadderTier.Basic;
		reason = "contacts and coils on the module's own pins";

		LadderTier fromOperand = TierForOperand(instruction.Operand, out string operandWhy);
		if (fromOperand > tier)
		{
			tier = fromOperand;
			reason = operandWhy;
		}
		LadderTier fromRight = TierForOperand(instruction.Right, out string rightWhy);
		if (fromRight > tier)
		{
			tier = fromRight;
			reason = rightWhy;
		}
		LadderTier fromReset = TierForOperand(instruction.Reset, out string resetWhy);
		if (fromReset > tier)
		{
			tier = fromReset;
			reason = resetWhy;
		}
		return tier;
	}

	private static LadderTier TierForOperand(LadderOperand operand, out string reason)
	{
		if (operand == null)
		{
			reason = "";
			return LadderTier.Basic;
		}
		if (operand.Kind == LadderOperandKind.Bus)
		{
			reason = "a bus pin";
			return LadderTier.Full;
		}
		if (operand.Kind == LadderOperandKind.Memory)
		{
			reason = "a memory bit";
			return LadderTier.Extended;
		}
		reason = "";
		return LadderTier.Basic;
	}

	private static LadderTier TierForSize(int networks, int channels, int nesting, out string reason)
	{
		LadderLimits basic = LadderLimits.For(LadderTier.Basic);
		LadderLimits extended = LadderLimits.For(LadderTier.Extended);

		if (networks > extended.MaxNetworks
			|| channels > extended.MaxChannels
			|| nesting > extended.MaxNesting)
		{
			reason = SizeReason(networks, channels, nesting, extended);
			return LadderTier.Full;
		}
		if (networks > basic.MaxNetworks
			|| channels > basic.MaxChannels
			|| nesting > basic.MaxNesting)
		{
			reason = SizeReason(networks, channels, nesting, basic);
			return LadderTier.Extended;
		}
		reason = "";
		return LadderTier.Basic;
	}

	// Names whichever dimension actually overflowed, so the message points at
	// the thing the player would have to change.
	private static string SizeReason(int networks, int channels, int nesting, LadderLimits limits)
	{
		if (networks > limits.MaxNetworks)
		{
			return $"{networks} networks";
		}
		if (channels > limits.MaxChannels)
		{
			return $"{channels} channels in one network";
		}
		return $"branches nested {nesting} deep in one network";
	}
}
