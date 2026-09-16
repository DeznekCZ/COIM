namespace ProgramableNetwork.Ladder;

// Second compile pass: does THIS module's tier allow the program the player
// wrote?  Kept separate from LadderParser so the parse is tier-agnostic — the
// graphical editor can read and render a Full-tier program inside a Basic
// module in order to tell the player what to upgrade, instead of failing to
// load it at all.
//
// Every message names the tier to swap up to, because "unavailable" without a
// next step is the kind of error that makes a player think the mod is broken.
public static class LadderValidator
{
	public static void Enforce(LadderProgram program, LadderTier tier)
	{
		LadderLimits limits = LadderLimits.For(tier);

		// Empty networks are free — see LadderCapability.EffectiveNetworkCount.
		// Charging for one would make "+ Network" fail the tier check before the
		// player had put anything in it.
		int used = LadderCapability.EffectiveNetworkCount(program);
		if (used > limits.MaxNetworks)
		{
			throw new LadderParseException(0, upgrade(tier,
				$"this module runs {limits.MaxNetworks} networks and the program uses {used}"));
		}

		foreach (LadderNetwork network in program.Networks)
		{
			int channels = LadderCapability.ChannelsOf(network);
			if (channels > limits.MaxChannels)
			{
				throw new LadderParseException(0, upgrade(tier,
					$"network {network.Number} is {channels} channels wide and this module runs {limits.MaxChannels}"));
			}

			// Depth, not width.  A branch may carry any number of legs — that is
			// one bracket pair however many there are, exactly as a chain of `O`
			// terms is one nest-stack entry in STL.  What costs is putting a
			// branch inside a branch, and it is counted per network because that
			// is the scope the nesting stack lives in.
			int nesting = LadderCapability.NestingOf(network);
			if (nesting > limits.MaxNesting)
			{
				throw new LadderParseException(0, upgrade(tier,
					$"network {network.Number} nests branches {nesting} deep and this module allows "
					+ $"{limits.MaxNesting}"));
			}

			foreach (LadderInstruction instruction in network.Code)
			{
				enforceInstruction(instruction, limits, tier);
			}
		}
	}

	private static void enforceInstruction(LadderInstruction instruction, LadderLimits limits, LadderTier tier)
	{
		switch (instruction.Op)
		{
		case LadderOp.TimerOn:
		case LadderOp.TimerOff:
		case LadderOp.TimerPulse:
		case LadderOp.CountUp:
		case LadderOp.Compare:
			if (!limits.AllowBoxes)
			{
				throw new LadderParseException(instruction.Line, upgrade(tier,
					$"the {LadderInstruction.MnemonicOf(instruction.Op)} box is not available on this module"));
			}
			break;
		case LadderOp.EdgeRising:
		case LadderOp.EdgeFalling:
			if (!limits.AllowEdges)
			{
				throw new LadderParseException(instruction.Line, upgrade(tier,
					$"edge detection ({LadderInstruction.MnemonicOf(instruction.Op)}) is not available on this module"));
			}
			break;
		case LadderOp.Toggle:
			// Gated with the edges: all three keep state between scans, and a
			// tier that can't remember a bit can't run any of them.
			if (!limits.AllowEdges)
			{
				throw new LadderParseException(instruction.Line, upgrade(tier,
					"a toggle coil is not available on this module"));
			}
			break;
		}

		enforceOperand(instruction.Operand, instruction.Line, limits, tier);
		enforceOperand(instruction.Right, instruction.Line, limits, tier);
		enforceOperand(instruction.Reset, instruction.Line, limits, tier);
	}

	private static void enforceOperand(LadderOperand operand, int line, LadderLimits limits, LadderTier tier)
	{
		if (operand == null)
		{
			return;
		}
		if (operand.Kind == LadderOperandKind.Memory && !limits.AllowMemory)
		{
			throw new LadderParseException(line, upgrade(tier,
				$"memory bit '{operand}' is not available on this module"));
		}
		if (operand.Kind == LadderOperandKind.Bus && !limits.AllowBus)
		{
			throw new LadderParseException(line, upgrade(tier,
				$"bus pin '{operand}' is not available on this module"));
		}
	}

	// Appends the concrete next step.  At Full there is nothing to upgrade to,
	// so the message stops at the diagnosis.
	private static string upgrade(LadderTier tier, string problem)
	{
		if (tier >= LadderTier.Full)
		{
			return problem + ".";
		}
		LadderTier next = tier + 1;
		return $"{problem}. Swap it for the {LadderLimits.NameOf(next)} ladder module.";
	}
}
