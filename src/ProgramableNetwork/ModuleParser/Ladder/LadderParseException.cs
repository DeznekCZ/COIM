using System;

namespace ProgramableNetwork.Ladder;

// Compile-time failure in a ladder program.  Carries the 1-based source line
// so the editor can highlight the offending row; Message is already
// player-facing (it lands in the module's error status verbatim).
public class LadderParseException : Exception
{
	public readonly int Line;

	public LadderParseException(int line, string message)
		: base(line > 0 ? $"Line {line}: {message}" : message)
	{
		Line = line;
	}
}
