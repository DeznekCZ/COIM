using Mafi;

namespace ProgramableNetwork.Ladder;

// Everything the ladder runtime is allowed to touch in the world.
//
// The machine itself knows nothing about Modules, Controllers or buses — it
// resolves operands through this interface, which is what keeps the whole
// compiler and evaluator inside the source-linked ModuleParser tree (and so
// unit-testable without the game).  The COI-side implementation lives next to
// the module registration.
public interface ILadderIo
{
	// Called once before the networks run.  The implementation latches the
	// input process image here so a scan sees a stable snapshot even if the
	// surrounding sim mutates a pin mid-tick.
	void BeginScan();

	// Unresolvable operands read as zero rather than throwing — an unwired pin
	// and a bus that the player hasn't created yet both behave like a contact
	// that simply isn't conducting, which is what a real PLC does too.
	Fix32 Read(LadderOperand operand);

	// Writes to outputs are expected to be buffered until EndScan; writes to
	// memory bits take effect immediately so a seal-in latch closes within a
	// single scan.
	void Write(LadderOperand operand, Fix32 value);

	// Flushes the output process image.
	void EndScan();
}
