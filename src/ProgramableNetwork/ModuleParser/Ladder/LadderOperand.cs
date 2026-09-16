using System;
using System.Globalization;
using Mafi;

namespace ProgramableNetwork.Ladder;

// What a contact / coil / compare argument points at.
//
//   I.<pin>          module input pin, by pin id (extensions included)
//   Q.<pin>          module output pin, by pin id
//   M<0..31>         module-local memory bit; all 32 pack into one int so the
//                    whole bank costs a single NumberData entry
//   B.<bus>.<pin>    a ControllerBus pin, by bus name + pin name
//   <number>         literal, only meaningful as a CMP argument
//
// Deliberately NOT included in v1: network variables.  The bus supersedes the
// controller-local variable modules, and a global network variable already
// reaches a ladder through the existing VariableNetwork_Read module feeding an
// input pin — no new syntax needed for it.
public enum LadderOperandKind : byte
{
	Input = 0,
	Output = 1,
	Memory = 2,
	Bus = 3,
	Literal = 4,
}

public sealed class LadderOperand
{
	// One int of memory bits.  Raising this means widening the packing in the
	// module binding, so it's a hard limit rather than a tunable.
	public const int MemoryBitCount = 32;

	public readonly LadderOperandKind Kind;

	// Input / Output: the pin id.  Bus: the bus name.  Empty otherwise.
	public readonly string Name;

	// Bus only: the pin name within the bus.  Empty otherwise.
	public readonly string Member;

	// Memory only: the bit index, 0 .. MemoryBitCount-1.  -1 otherwise.
	public readonly int Index;

	// Literal only.  Fix32.Zero otherwise.
	public readonly Fix32 Literal;

	private LadderOperand(LadderOperandKind kind, string name, string member, int index, Fix32 literal)
	{
		Kind = kind;
		Name = name ?? "";
		Member = member ?? "";
		Index = index;
		Literal = literal;
	}

	public static LadderOperand ForInput(string pin)
	{
		return new LadderOperand(LadderOperandKind.Input, pin, "", -1, Fix32.Zero);
	}

	public static LadderOperand ForOutput(string pin)
	{
		return new LadderOperand(LadderOperandKind.Output, pin, "", -1, Fix32.Zero);
	}

	public static LadderOperand ForMemory(int index)
	{
		return new LadderOperand(LadderOperandKind.Memory, "", "", index, Fix32.Zero);
	}

	public static LadderOperand ForBus(string bus, string pin)
	{
		return new LadderOperand(LadderOperandKind.Bus, bus, pin, -1, Fix32.Zero);
	}

	public static LadderOperand ForLiteral(Fix32 value)
	{
		return new LadderOperand(LadderOperandKind.Literal, "", "", -1, value);
	}

	// Parses one operand token.  `line` is only carried into the exception so
	// the editor can point at the offending row.
	public static LadderOperand Parse(string text, int line)
	{
		string t = (text ?? "").Trim();
		if (t.Length == 0)
		{
			throw new LadderParseException(line, "Expected an operand.");
		}

		if (t.StartsWith("I.", StringComparison.OrdinalIgnoreCase))
		{
			string pin = t.Substring(2);
			if (pin.Length == 0)
			{
				throw new LadderParseException(line, "Input operand is missing a pin name — write for example 'I.A'.");
			}
			return ForInput(pin);
		}

		if (t.StartsWith("Q.", StringComparison.OrdinalIgnoreCase))
		{
			string pin = t.Substring(2);
			if (pin.Length == 0)
			{
				throw new LadderParseException(line, "Output operand is missing a pin name — write for example 'Q.A'.");
			}
			return ForOutput(pin);
		}

		if (t.StartsWith("B.", StringComparison.OrdinalIgnoreCase))
		{
			string rest = t.Substring(2);
			int dot = rest.IndexOf('.');
			if (dot <= 0 || dot == rest.Length - 1)
			{
				throw new LadderParseException(line,
					$"Bus operand '{t}' must name a bus and a pin — write for example 'B.main.level'.");
			}
			return ForBus(rest.Substring(0, dot), rest.Substring(dot + 1));
		}

		// M<digits> — checked before the numeric fallback so 'M0' never reads
		// as a literal.
		if ((t[0] == 'M' || t[0] == 'm') && t.Length > 1 && IsAllDigits(t, 1))
		{
			int index = int.Parse(t.Substring(1), CultureInfo.InvariantCulture);
			if (index < 0 || index >= MemoryBitCount)
			{
				throw new LadderParseException(line,
					$"Memory bit '{t}' is out of range — this module has M0 to M{MemoryBitCount - 1}.");
			}
			return ForMemory(index);
		}

		if (TryParseLiteral(t, out Fix32 value))
		{
			return ForLiteral(value);
		}

		throw new LadderParseException(line,
			$"'{t}' is not a valid operand. Use I.<pin>, Q.<pin>, M0-M{MemoryBitCount - 1}, B.<bus>.<pin>, or a number.");
	}

	// True when this operand names a place a coil can drive.  Literals and
	// inputs are read-only; everything else can be written.
	public bool IsWritable()
	{
		return Kind == LadderOperandKind.Output
			|| Kind == LadderOperandKind.Memory
			|| Kind == LadderOperandKind.Bus;
	}

	// Round-trips through LadderParser.Parse — the graphical editor writes the
	// program back out through this.
	public override string ToString()
	{
		switch (Kind)
		{
		case LadderOperandKind.Input:
			return "I." + Name;
		case LadderOperandKind.Output:
			return "Q." + Name;
		case LadderOperandKind.Memory:
			return "M" + Index.ToString(CultureInfo.InvariantCulture);
		case LadderOperandKind.Bus:
			return "B." + Name + "." + Member;
		default:
			return FormatLiteral(Literal);
		}
	}

	private static bool IsAllDigits(string s, int from)
	{
		for (int i = from; i < s.Length; i++)
		{
			if (s[i] < '0' || s[i] > '9')
			{
				return false;
			}
		}
		return true;
	}

	private static bool TryParseLiteral(string t, out Fix32 value)
	{
		value = Fix32.Zero;
		if (int.TryParse(t, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int whole))
		{
			value = Fix32.FromInt(whole);
			return true;
		}
		if (double.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out double real))
		{
			value = Fix32.FromDouble(real);
			return true;
		}
		return false;
	}

	// Prints a Fix32 back as the shortest text that re-parses to the same
	// value — integers stay integers so a program the player typed as "900"
	// doesn't come back as "900.0".
	private static string FormatLiteral(Fix32 value)
	{
		if (value.IsInteger)
		{
			return value.IntegerPart.ToString(CultureInfo.InvariantCulture);
		}
		return value.ToDouble().ToString("0.###", CultureInfo.InvariantCulture);
	}
}
