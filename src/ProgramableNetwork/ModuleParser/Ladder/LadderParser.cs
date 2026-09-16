using System;
using System.Collections.Generic;
using System.Globalization;

namespace ProgramableNetwork.Ladder;

// Text → LadderProgram.
//
// The source is line-oriented: one instruction per line, `NETWORK <n>` as the
// separator, `//` for comments.  There is no expression grammar to speak of,
// which is the point — the ladder grid is the authoring surface and this text
// is its file format, so it stays trivially machine-writable (see
// LadderProgram.ToText) and trivially diffable when a player pastes one into
// a forum post.
//
// Structural ceilings are enforced against the FULL tier here, because those
// are the limits of the runtime itself.  Which tier a given program actually
// needs — and whether the module is allowed to run it — is a separate question
// answered by LadderCapability.Analyze.
public static class LadderParser
{
	public static LadderProgram Parse(string source)
	{
		LadderLimits ceiling = LadderLimits.For(LadderTier.Full);
		List<LadderNetwork> networks = new List<LadderNetwork>();

		int stateSlots = 0;
		int elements = 0;

		string[] lines = (source ?? "").Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

		int currentNumber = 0;
		string currentTitle = null;
		int currentHeaderLine = 0;
		List<LadderInstruction> currentCode = null;

		for (int i = 0; i < lines.Length; i++)
		{
			int lineNo = i + 1;
			string raw = lines[i];

			// Split the comment off before trimming so a title can carry
			// leading spaces the player typed deliberately.
			string comment = "";
			int commentAt = raw.IndexOf("//", StringComparison.Ordinal);
			if (commentAt >= 0)
			{
				comment = raw.Substring(commentAt + 2).Trim();
				raw = raw.Substring(0, commentAt);
			}

			string line = raw.Trim();
			if (line.Length == 0)
			{
				continue;
			}

			if (line.StartsWith("NETWORK", StringComparison.OrdinalIgnoreCase))
			{
				if (currentCode != null)
				{
					networks.Add(finishNetwork(currentNumber, currentTitle, currentCode, currentHeaderLine, ceiling));
				}

				string rest = line.Substring("NETWORK".Length).Trim();
				if (rest.Length == 0 || !int.TryParse(rest, NumberStyles.Integer, CultureInfo.InvariantCulture, out currentNumber))
				{
					throw new LadderParseException(lineNo,
						"NETWORK must be followed by a number — write for example 'NETWORK 1'.");
				}
				currentTitle = comment;
				currentHeaderLine = lineNo;
				currentCode = new List<LadderInstruction>();
				continue;
			}

			if (currentCode == null)
			{
				throw new LadderParseException(lineNo,
					$"'{line}' appears before any NETWORK header. Start the program with 'NETWORK 1'.");
			}

			LadderInstruction instruction = parseInstruction(line, lineNo, ref stateSlots);
			currentCode.Add(instruction);
			if (!LadderInstruction.IsStructural(instruction.Op))
			{
				elements++;
			}
		}

		if (currentCode != null)
		{
			networks.Add(finishNetwork(currentNumber, currentTitle, currentCode, currentHeaderLine, ceiling));
		}

		if (networks.Count == 0)
		{
			throw new LadderParseException(0, "The program is empty. Start with 'NETWORK 1'.");
		}

		LadderProgram program = new LadderProgram(networks, stateSlots, elements);

		// The runtime ceiling is counted after the fact, and only over networks
		// that hold something: empty ones neither draw nor run, and the graphical
		// editor creates them on purpose and waits to be filled.
		int used = LadderCapability.EffectiveNetworkCount(program);
		if (used > ceiling.MaxNetworks)
		{
			throw new LadderParseException(0,
				$"Too many networks — a ladder module runs at most {ceiling.MaxNetworks}.");
		}
		return program;
	}

	// Per-network checks that can only run once the whole network is known:
	// bracket balance, width, and the "this rung drives nothing" mistake.
	private static LadderNetwork finishNetwork(
		int number,
		string title,
		List<LadderInstruction> code,
		int headerLine,
		LadderLimits ceiling)
	{
		int depth = 0;
		foreach (LadderInstruction instruction in code)
		{
			if (instruction.Op == LadderOp.OpenAnd || instruction.Op == LadderOp.OpenOr)
			{
				depth++;
			}
			else if (instruction.Op == LadderOp.Close)
			{
				depth--;
				if (depth < 0)
				{
					throw new LadderParseException(instruction.Line,
						"')' closes a branch that was never opened.");
				}
			}
		}

		if (depth != 0)
		{
			throw new LadderParseException(headerLine,
				$"Network {number} opens {depth} branch{(depth == 1 ? "" : "es")} that are never closed with ')'.");
		}

		// The runtime's own nesting ceiling, mirroring the fixed-size nesting
		// stack a real S7 has.  Which tier may use how much of it is a separate
		// question, answered by LadderValidator.
		int nesting = LadderCapability.NestingOf(new LadderNetwork(number, title, code));
		if (nesting > ceiling.MaxNesting)
		{
			throw new LadderParseException(headerLine,
				$"Network {number} nests branches {nesting} deep — a ladder module nests at most "
				+ $"{ceiling.MaxNesting}.");
		}

		// An empty network, or one with no coil, is ALLOWED.  Both are normal
		// intermediate states in the graphical editor — a player adds a network
		// and then fills it in — and rejecting them would force the editor to
		// invent placeholder elements just to keep the text parseable.  Neither
		// shape does anything at run time, which is the honest outcome for a
		// rung that drives nothing.

		LadderNetwork network = new LadderNetwork(number, title, code);
		int channels = LadderCapability.ChannelsOf(network);
		if (channels > ceiling.MaxChannels)
		{
			throw new LadderParseException(headerLine,
				$"Network {number} uses {channels} channels — the widest a ladder module supports is {ceiling.MaxChannels}.");
		}
		return network;
	}

	private static LadderInstruction parseInstruction(string line, int lineNo, ref int stateSlots)
	{
		// Brackets are their own tokens and never carry an operand.  Checked
		// before the generic split so 'A(' isn't read as mnemonic 'A('.
		if (line == ")")
		{
			return make(LadderOp.Close, lineNo, ref stateSlots);
		}
		if (startsWithBracket(line, "A", out string afterA))
		{
			requireEmpty(afterA, lineNo, "A(");
			return make(LadderOp.OpenAnd, lineNo, ref stateSlots);
		}
		if (startsWithBracket(line, "O", out string afterO))
		{
			requireEmpty(afterO, lineNo, "O(");
			return make(LadderOp.OpenOr, lineNo, ref stateSlots);
		}

		string mnemonic;
		string rest;
		int space = line.IndexOfAny(new[] { ' ', '\t' });
		if (space < 0)
		{
			mnemonic = line;
			rest = "";
		}
		else
		{
			mnemonic = line.Substring(0, space);
			rest = line.Substring(space + 1).Trim();
		}

		switch (mnemonic.ToUpperInvariant())
		{
		case "A":
			return contact(LadderOp.And, rest, lineNo, ref stateSlots);
		case "AN":
			return contact(LadderOp.AndNot, rest, lineNo, ref stateSlots);
		case "O":
			return contact(LadderOp.Or, rest, lineNo, ref stateSlots);
		case "ON":
			return contact(LadderOp.OrNot, rest, lineNo, ref stateSlots);
		case "FP":
			return contact(LadderOp.EdgeRising, rest, lineNo, ref stateSlots);
		case "FN":
			return contact(LadderOp.EdgeFalling, rest, lineNo, ref stateSlots);
		case "=":
			return coil(LadderOp.Assign, rest, lineNo, ref stateSlots);
		case "S":
			return coil(LadderOp.Set, rest, lineNo, ref stateSlots);
		case "R":
			return coil(LadderOp.Reset, rest, lineNo, ref stateSlots);
		case "=N":
			return coil(LadderOp.AssignNot, rest, lineNo, ref stateSlots);
		case "T":
			return coil(LadderOp.Toggle, rest, lineNo, ref stateSlots);
		case "NOT":
			if (rest.Length > 0)
			{
				throw new LadderParseException(lineNo,
					"'NOT' inverts the rung's power and takes no address.");
			}
			return make(LadderOp.Not, lineNo, ref stateSlots);
		case "TON":
			return timer(LadderOp.TimerOn, rest, lineNo, ref stateSlots);
		case "TOF":
			return timer(LadderOp.TimerOff, rest, lineNo, ref stateSlots);
		case "TP":
			return timer(LadderOp.TimerPulse, rest, lineNo, ref stateSlots);
		case "CTU":
			return counter(rest, lineNo, ref stateSlots);
		case "CMP":
			return compare(rest, lineNo, ref stateSlots);
		default:
			throw new LadderParseException(lineNo,
				$"'{mnemonic}' is not a ladder instruction. Expected one of "
				+ "A AN O ON A( O( ) FP FN NOT = =N S R T TON TOF TP CTU CMP.");
		}
	}

	private static bool startsWithBracket(string line, string mnemonic, out string rest)
	{
		rest = "";
		if (!line.StartsWith(mnemonic, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		string tail = line.Substring(mnemonic.Length).TrimStart();
		if (!tail.StartsWith("(", StringComparison.Ordinal))
		{
			return false;
		}
		rest = tail.Substring(1).Trim();
		return true;
	}

	private static void requireEmpty(string rest, int lineNo, string mnemonic)
	{
		if (rest.Length > 0)
		{
			throw new LadderParseException(lineNo, $"'{mnemonic}' opens a branch and takes no operand.");
		}
	}

	private static LadderInstruction contact(LadderOp op, string rest, int lineNo, ref int stateSlots)
	{
		LadderOperand operand = LadderOperand.Parse(rest, lineNo);
		if (operand.Kind == LadderOperandKind.Literal)
		{
			throw new LadderParseException(lineNo,
				$"A contact needs something to read — '{rest}' is a plain number. Numbers are only valid inside CMP.");
		}
		return make(op, lineNo, ref stateSlots, operand);
	}

	private static LadderInstruction coil(LadderOp op, string rest, int lineNo, ref int stateSlots)
	{
		LadderOperand operand = LadderOperand.Parse(rest, lineNo);
		if (!operand.IsWritable())
		{
			string what = operand.Kind == LadderOperandKind.Input
				? "an input pin — inputs are driven from outside the module"
				: "a plain number";
			throw new LadderParseException(lineNo,
				$"A coil cannot drive '{rest}': that is {what}. Drive Q.<pin>, M<n>, or B.<bus>.<pin>.");
		}
		return make(op, lineNo, ref stateSlots, operand);
	}

	private static LadderInstruction timer(LadderOp op, string rest, int lineNo, ref int stateSlots)
	{
		Dictionary<string, string> args = parseArgs(rest, lineNo);
		int preset = requireIntArg(args, "PT", lineNo, op);
		rejectUnknownArgs(args, new[] { "PT" }, lineNo, op);
		return make(op, lineNo, ref stateSlots, null, null, LadderCompare.Greater, preset);
	}

	private static LadderInstruction counter(string rest, int lineNo, ref int stateSlots)
	{
		Dictionary<string, string> args = parseArgs(rest, lineNo);
		int preset = requireIntArg(args, "PV", lineNo, LadderOp.CountUp);
		LadderOperand reset = null;
		if (args.TryGetValue("R", out string resetText))
		{
			reset = LadderOperand.Parse(resetText, lineNo);
			if (reset.Kind == LadderOperandKind.Literal)
			{
				throw new LadderParseException(lineNo, "CTU reset R:= needs something to read, not a number.");
			}
		}
		rejectUnknownArgs(args, new[] { "PV", "R" }, lineNo, LadderOp.CountUp);
		return make(LadderOp.CountUp, lineNo, ref stateSlots, null, null, LadderCompare.Greater, preset, reset);
	}

	private static LadderInstruction compare(string rest, int lineNo, ref int stateSlots)
	{
		if (!trySplitCompare(rest, out string leftText, out LadderCompare cmp, out string rightText))
		{
			throw new LadderParseException(lineNo,
				$"CMP needs '<left> <op> <right>' with op one of > < = >= <= <> — got '{rest}'.");
		}
		LadderOperand left = LadderOperand.Parse(leftText, lineNo);
		LadderOperand right = LadderOperand.Parse(rightText, lineNo);
		return make(LadderOp.Compare, lineNo, ref stateSlots, left, right, cmp);
	}

	// Finds the comparison operator outside of any operand text.  Two-character
	// operators are tested first so '>=' never reads as '>' followed by junk.
	private static bool trySplitCompare(string rest, out string left, out LadderCompare cmp, out string right)
	{
		string[] twoChar = { ">=", "<=", "<>" };
		LadderCompare[] twoCmp = { LadderCompare.GreaterOrEqual, LadderCompare.LessOrEqual, LadderCompare.NotEqual };
		for (int i = 0; i < twoChar.Length; i++)
		{
			int at = rest.IndexOf(twoChar[i], StringComparison.Ordinal);
			if (at > 0)
			{
				left = rest.Substring(0, at).Trim();
				right = rest.Substring(at + 2).Trim();
				cmp = twoCmp[i];
				return left.Length > 0 && right.Length > 0;
			}
		}

		char[] oneChar = { '>', '<', '=' };
		LadderCompare[] oneCmp = { LadderCompare.Greater, LadderCompare.Less, LadderCompare.Equal };
		for (int i = 0; i < oneChar.Length; i++)
		{
			int at = rest.IndexOf(oneChar[i]);
			if (at > 0)
			{
				left = rest.Substring(0, at).Trim();
				right = rest.Substring(at + 1).Trim();
				cmp = oneCmp[i];
				return left.Length > 0 && right.Length > 0;
			}
		}

		left = "";
		right = "";
		cmp = LadderCompare.Greater;
		return false;
	}

	// Parses 'KEY:=VALUE' / 'KEY=VALUE' pairs separated by whitespace.  Keys
	// are upper-cased so 'pt:=5' and 'PT:=5' both land.
	private static Dictionary<string, string> parseArgs(string rest, int lineNo)
	{
		Dictionary<string, string> args = new Dictionary<string, string>();
		if (rest.Length == 0)
		{
			return args;
		}
		string[] parts = rest.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string part in parts)
		{
			int at = part.IndexOf(":=", StringComparison.Ordinal);
			int skip = 2;
			if (at < 0)
			{
				at = part.IndexOf('=');
				skip = 1;
			}
			if (at <= 0 || at + skip >= part.Length)
			{
				throw new LadderParseException(lineNo,
					$"'{part}' is not a parameter — write them as NAME:=VALUE, for example 'PT:=6'.");
			}
			args[part.Substring(0, at).ToUpperInvariant()] = part.Substring(at + skip);
		}
		return args;
	}

	private static int requireIntArg(Dictionary<string, string> args, string key, int lineNo, LadderOp op)
	{
		if (!args.TryGetValue(key, out string text))
		{
			throw new LadderParseException(lineNo,
				$"{LadderInstruction.MnemonicOf(op)} needs a {key} — write for example '{LadderInstruction.MnemonicOf(op)} {key}:=6'.");
		}
		if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) || value <= 0)
		{
			throw new LadderParseException(lineNo, $"{key} must be a whole number of ticks above zero — got '{text}'.");
		}
		return value;
	}

	private static void rejectUnknownArgs(Dictionary<string, string> args, string[] allowed, int lineNo, LadderOp op)
	{
		foreach (string key in args.Keys)
		{
			bool ok = false;
			foreach (string a in allowed)
			{
				if (a == key)
				{
					ok = true;
					break;
				}
			}
			if (!ok)
			{
				throw new LadderParseException(lineNo,
					$"{LadderInstruction.MnemonicOf(op)} does not take '{key}'. Accepted: {string.Join(", ", allowed)}.");
			}
		}
	}

	// Single construction point so state-slot allocation can't be forgotten
	// when a new stateful opcode is added.
	private static LadderInstruction make(
		LadderOp op,
		int line,
		ref int stateSlots,
		LadderOperand operand = null,
		LadderOperand right = null,
		LadderCompare compare = LadderCompare.Greater,
		int preset = 0,
		LadderOperand reset = null)
	{
		int slots = LadderInstruction.StateSlotsFor(op);
		int slot = -1;
		if (slots > 0)
		{
			slot = stateSlots;
			stateSlots += slots;
		}
		return new LadderInstruction(op, operand, right, compare, preset, reset, slot, line);
	}
}
