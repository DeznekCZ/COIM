namespace ProgramableNetwork.Ladder;

// Text-in, text-out wrapper over LadderDocument.
//
// The real editing model is LadderDocument — mutable, long-lived, and what the
// graphical editor actually drives.  This layer exists for the callers that only
// ever wanted "apply one edit to this program and give me the text back": the
// statement-list side, and the whole existing test suite, which asserts the one
// property that matters most — an edit can never produce a program the parser
// would reject.
//
// Keeping it means there is still exactly one implementation of every operation.
// A bug fixed in the document is fixed here, and a test written here covers the
// document too.
public static class LadderEdit
{
	// ---- element edits ----

	public static string SetOperand(LadderProgram program, int network, int index, LadderOperand operand)
	{
		LadderDocument document = new LadderDocument(program);
		document.SetOperand(network, index, operand);
		return document.ToText();
	}

	public static string SetKind(LadderProgram program, int network, int index, LadderOp kind)
	{
		LadderDocument document = new LadderDocument(program);
		document.SetKind(network, index, kind);
		return document.ToText();
	}

	public static string SetPreset(LadderProgram program, int network, int index, int preset)
	{
		LadderDocument document = new LadderDocument(program);
		document.SetPreset(network, index, preset);
		return document.ToText();
	}

	public static string SetCompare(
		LadderProgram program, int network, int index, LadderCompare compare, LadderOperand right)
	{
		LadderDocument document = new LadderDocument(program);
		document.SetCompare(network, index, compare, right);
		return document.ToText();
	}

	// ---- structure edits ----

	public static string InsertAfter(
		LadderProgram program, int network, int index, LadderInstruction inserted)
	{
		LadderDocument document = new LadderDocument(program);
		document.InsertAfter(network, index, inserted);
		return document.ToText();
	}

	public static string InsertFirst(LadderProgram program, int network, LadderInstruction inserted)
	{
		LadderDocument document = new LadderDocument(program);
		document.InsertFirst(network, inserted);
		return document.ToText();
	}

	// Wraps the element at `index` in a branch group and hangs `leg` off it as a
	// second, parallel path:  X  →  A( X  O leg )
	public static string AddParallelBranch(
		LadderProgram program, int network, int index, LadderInstruction leg)
	{
		return AddParallelBranchOver(program, network, index, index, leg);
	}

	// The general form: parallels `leg` with the whole run from `fromIndex` to
	// `toIndex` inclusive.  See LadderDocument.AddParallelBranch.
	public static string AddParallelBranchOver(
		LadderProgram program, int network, int fromIndex, int toIndex, LadderInstruction leg)
	{
		LadderDocument document = new LadderDocument(program);
		document.AddParallelBranch(network, fromIndex, toIndex, leg);
		return document.ToText();
	}

	public static string MoveElement(LadderProgram program, int network, int index, int direction)
	{
		LadderDocument document = new LadderDocument(program);
		document.MoveElement(network, index, direction);
		return document.ToText();
	}

	public static int GroupStart(LadderProgram program, int network, int index)
	{
		return new LadderDocument(program).GroupStart(network, index);
	}

	public static string Delete(LadderProgram program, int network, int index)
	{
		LadderDocument document = new LadderDocument(program);
		document.Delete(network, index);
		return document.ToText();
	}

	// ---- network edits ----

	public static string AddNetwork(LadderProgram program, string title)
	{
		LadderDocument document = new LadderDocument(program);
		document.AddNetwork(title);
		return document.ToText();
	}

	public static string InsertNetworkAfter(LadderProgram program, int network, string title)
	{
		LadderDocument document = new LadderDocument(program);
		document.InsertNetworkAfter(network, title);
		return document.ToText();
	}

	public static string ClearNetwork(LadderProgram program, int network)
	{
		LadderDocument document = new LadderDocument(program);
		document.ClearNetwork(network);
		return document.ToText();
	}

	public static string DeleteNetwork(LadderProgram program, int network)
	{
		LadderDocument document = new LadderDocument(program);
		document.DeleteNetwork(network);
		return document.ToText();
	}

	public static string MoveNetwork(LadderProgram program, int network, int direction)
	{
		LadderDocument document = new LadderDocument(program);
		document.MoveNetwork(network, direction);
		return document.ToText();
	}

	public static string SetNetworkTitle(LadderProgram program, int network, string title)
	{
		LadderDocument document = new LadderDocument(program);
		document.SetNetworkTitle(network, title);
		return document.ToText();
	}
}
