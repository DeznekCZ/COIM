using System.Collections.Generic;

namespace ProgramableNetwork.Ladder;

// The editor's working copy: a ladder you can EDIT, rather than one you re-derive
// from text after every keystroke.
//
// The first cut of the editor had no document at all.  Every operation took a
// parsed program, returned new program TEXT, and the editor fed that back through
// the session, which re-parsed it and rebuilt the world.  That is a clean rule
// and it does guarantee the diagram and the statement list can never disagree —
// but it makes the program itself immutable, and an immutable program cannot be
// SET UP.  Anything that takes more than one step to express (arm a branch, then
// choose its leg; mark a run, then bypass it; pick a symbol, then move it along
// the rung) has nowhere to live, because the round trip through text throws away
// everything that isn't already in the text.
//
// So the document is mutable and long-lived.  Edits land here, the diagram draws
// from here, and text is produced on demand — for the statement-list window, and
// for the module when the player saves.  Text stops being the thing being edited
// and goes back to being the file format, which is what it always was.
//
// Every operation returns bool: true if the document changed.  Refusals are
// silent and total — an operation that cannot be applied leaves the document
// exactly as it was, so a mis-click can never half-apply.
public sealed class LadderDocument
{
	private readonly List<LadderNetwork> m_networks = new List<LadderNetwork>();

	public LadderDocument()
	{
	}

	public LadderDocument(LadderProgram program)
	{
		if (program == null)
		{
			return;
		}
		foreach (LadderNetwork network in program.Networks)
		{
			m_networks.Add(new LadderNetwork(
				network.Number, network.Title, new List<LadderInstruction>(network.Code)));
		}
	}

	public IReadOnlyList<LadderNetwork> Networks => m_networks;

	public int NetworkCount => m_networks.Count;

	public string ToText()
	{
		return LadderProgram.TextOf(m_networks);
	}

	// A program view of the draft, for DRAWING and ANALYSIS only.
	//
	// The editor used to render whatever the last successful parse produced,
	// which meant one un-parseable moment froze the diagram: the statement list
	// showed the new text while the ladder still showed the old shape, with no
	// hint that the two had come apart. Rendering the draft instead means the
	// picture is always what the player just did.
	//
	// StateSlotCount is zero because nothing here is executable — the module
	// compiles from text, and only a real parse can allocate the machine's state.
	public LadderProgram ToProgram()
	{
		int elements = 0;
		foreach (LadderNetwork network in m_networks)
		{
			foreach (LadderInstruction instruction in network.Code)
			{
				if (!LadderInstruction.IsStructural(instruction.Op))
				{
					elements++;
				}
			}
		}
		return new LadderProgram(m_networks, 0, elements);
	}

	// ---- element edits ----

	public bool SetOperand(int network, int index, LadderOperand operand)
	{
		List<LadderInstruction> code = codeOf(network);
		if (code == null || index < 0 || index >= code.Count)
		{
			return false;
		}
		if (!CanAddress(code[index].Op, operand))
		{
			return false;
		}
		return applyAt(network, index, old => new LadderInstruction(
			old.Op, operand, old.Right, old.Compare, old.Preset, old.Reset, -1, 0));
	}

	// Can this kind of element point at this address?
	//
	// A coil DRIVES its address, so it needs one that can be written: an input
	// pin is fed from outside the module and a literal is a constant, so neither
	// can be a coil's target.  A contact only reads, so anything but a bare
	// number will do.
	//
	// This lives here rather than only in the parser because the document's whole
	// job is to be un-breakable: an edit must never be able to produce a program
	// that won't parse.  The editor asks the same question to decide which
	// addresses to even offer, so the two can't drift apart.
	public static bool CanAddress(LadderOp op, LadderOperand operand)
	{
		if (operand == null)
		{
			return false;
		}
		if (LadderInstruction.IsCoil(op))
		{
			return operand.IsWritable();
		}
		return operand.Kind != LadderOperandKind.Literal;
	}

	// Switches a contact between NO / NC / edge, or a coil between = / =N / S / R
	// / T.  Turning a contact into a coil would change what the rung MEANS rather
	// than how it looks, so it is refused rather than silently applied.
	public bool SetKind(int network, int index, LadderOp kind)
	{
		return applyAt(network, index, old => {
			if (LadderInstruction.IsCoil(old.Op) != LadderInstruction.IsCoil(kind))
			{
				return old;
			}
			// Keep the connector flavour: an element that started a parallel
			// branch must keep starting one, or the rung silently reshapes.
			LadderOp next = isOrFlavor(old.Op) ? toOrFlavor(kind) : toAndFlavor(kind);
			return new LadderInstruction(
				next, old.Operand, old.Right, old.Compare, old.Preset, old.Reset, -1, 0);
		});
	}

	public bool SetPreset(int network, int index, int preset)
	{
		return applyAt(network, index, old => new LadderInstruction(
			old.Op, old.Operand, old.Right, old.Compare, preset, old.Reset, -1, 0));
	}

	public bool SetCompare(int network, int index, LadderCompare compare, LadderOperand right)
	{
		return applyAt(network, index, old => new LadderInstruction(
			old.Op, old.Operand, right, compare, old.Preset, old.Reset, -1, 0));
	}

	// ---- structure edits ----

	// Adds an element in series immediately after `index`.  The inserted element
	// always joins with AND — inserting into a series is what "after" means here.
	public bool InsertAfter(int network, int index, LadderInstruction inserted)
	{
		List<LadderInstruction> code = codeOf(network);
		if (code == null || !isPlaceable(inserted) || index < 0 || index >= code.Count)
		{
			return false;
		}
		code.Insert(index + 1, withOp(inserted, toAndFlavor(inserted.Op)));
		normalize(code);
		return true;
	}

	// Places the first element of an empty network.  InsertAfter needs something
	// to anchor to and a network the player just added has nothing.
	public bool InsertFirst(int network, LadderInstruction inserted)
	{
		List<LadderInstruction> code = codeOf(network);
		if (code == null || !isPlaceable(inserted))
		{
			return false;
		}
		code.Insert(0, withOp(inserted, toAndFlavor(inserted.Op)));
		normalize(code);
		return true;
	}

	// Parallels `leg` with the run of elements from `fromIndex` to `toIndex`:
	//
	//   A B C  →  A( A B C  O leg )
	//
	// A branch is an operator applied to a scope, which is how every real LAD
	// editor spells it, and the span form is what makes a seal-in authorable —
	// there the holding contact has to bypass the whole start chain, not just its
	// last symbol.
	public bool AddParallelBranch(int network, int fromIndex, int toIndex, LadderInstruction leg)
	{
		List<LadderInstruction> code = codeOf(network);
		if (code == null || !isPlaceable(leg))
		{
			return false;
		}
		if (fromIndex < 0 || toIndex >= code.Count || fromIndex > toIndex)
		{
			return false;
		}
		if (LadderInstruction.IsCoil(leg.Op) || LadderInstruction.IsStructural(leg.Op))
		{
			return false;
		}
		if (!isWrappable(code, fromIndex, toIndex))
		{
			return false;
		}

		LadderInstruction head = code[fromIndex];
		LadderOp open = isOrFlavor(head.Op) ? LadderOp.OpenOr : LadderOp.OpenAnd;

		// The span's head becomes the group's first element, so it loads rather
		// than continuing the outer chain; the group carries the connector the
		// head used to have.
		code[fromIndex] = withOp(head, toAndFlavor(head.Op));
		// Tail end first: inserting there leaves fromIndex valid.
		code.Insert(toIndex + 1, withOp(leg, toOrFlavor(leg.Op)));
		code.Insert(toIndex + 2, bracket(LadderOp.Close));
		code.Insert(fromIndex, bracket(open));
		normalize(code);
		return true;
	}

	// Removes one element.  Brackets left holding a single leg (or none) are
	// collapsed by normalize, so deleting the second leg of a branch puts the
	// rung back to a plain series rather than leaving an empty group.
	public bool Delete(int network, int index)
	{
		List<LadderInstruction> code = codeOf(network);
		if (code == null || index < 0 || index >= code.Count)
		{
			return false;
		}
		if (LadderInstruction.IsStructural(code[index].Op))
		{
			// Deleting a lone bracket would unbalance the network; the editor
			// addresses symbols, and brackets aren't symbols.
			return false;
		}
		code.RemoveAt(index);
		normalize(code);
		return true;
	}

	// Swaps an element with its neighbour, which is how reordering a rung works:
	// the CONNECTORS belong to the positions, not to the symbols, so the two
	// symbols exchange places while the shape of the diagram stays put.
	//
	// Refuses to cross a bracket (that would move a symbol into or out of a
	// branch leg, which is a reshape, not a reorder) and refuses to swap a coil
	// past a contact (that would move logic to the far side of the rung's end).
	// Reordering coils among themselves is fine — that just reorders outputs.
	public bool MoveElement(int network, int index, int direction)
	{
		List<LadderInstruction> code = codeOf(network);
		if (code == null || (direction != -1 && direction != 1))
		{
			return false;
		}
		int other = index + direction;
		if (index < 0 || index >= code.Count || other < 0 || other >= code.Count)
		{
			return false;
		}

		LadderInstruction a = code[index];
		LadderInstruction b = code[other];
		if (LadderInstruction.IsStructural(a.Op) || LadderInstruction.IsStructural(b.Op))
		{
			return false;
		}
		if (LadderInstruction.IsCoil(a.Op) != LadderInstruction.IsCoil(b.Op))
		{
			return false;
		}

		code[index] = withOp(b, isOrFlavor(a.Op) ? toOrFlavor(b.Op) : toAndFlavor(b.Op));
		code[other] = withOp(a, isOrFlavor(b.Op) ? toOrFlavor(a.Op) : toAndFlavor(a.Op));
		normalize(code);
		return true;
	}

	// Moves the point where a branch rejoins the main line, which on the diagram
	// is the closing junction bar sliding one symbol left or right.
	//
	//   direction +1:  ( a  O b )  c   →   ( a  c  O b )
	//   direction -1:  ( a  c  O b )     →   ( a  O b )  c
	//
	// The symbol crossing the bar always belongs to the FIRST leg, because that
	// leg is the main line — it is the row the bar is being dragged along, and
	// the element sitting to the right of the bar is drawn on it.  Writing this
	// as "insert before the first O at the group's top level" is what keeps the
	// picture and the statement list saying the same thing.
	//
	// `index` may be any element inside the group, or its closing bracket.
	public bool MoveBranchEnd(int network, int index, int direction)
	{
		List<LadderInstruction> code = codeOf(network);
		if (code == null || (direction != -1 && direction != 1))
		{
			return false;
		}

		int open = groupOpenFor(code, index);
		if (open < 0)
		{
			return false;
		}
		int close = matchingClose(code, open);
		if (close < 0)
		{
			return false;
		}
		int firstLegEnd = firstLegEndOf(code, open, close);

		if (direction > 0)
		{
			// Absorb the symbol just past the bar into the first leg.
			int taken = close + 1;
			if (taken >= code.Count || !movableAcrossBranchEnd(code[taken]))
			{
				return false;
			}
			LadderInstruction moved = code[taken];
			code.RemoveAt(taken);
			code.Insert(firstLegEnd, withOp(moved, toAndFlavor(moved.Op)));
		}
		else
		{
			// Release the last symbol of the first leg out past the bar.  The leg
			// may not be emptied: a branch with a legless side is not a branch,
			// and the player would have to undo a collapse they didn't ask for.
			// The leg runs from open+1 to firstLegEnd-1, so it needs two symbols
			// for one of them to be able to leave.
			int released = firstLegEnd - 1;
			if (released <= open + 1 || !movableAcrossBranchEnd(code[released]))
			{
				return false;
			}
			LadderInstruction moved = code[released];
			code.RemoveAt(released);
			// One fewer element inside the group, so its close moved up by one.
			code.Insert(close, withOp(moved, toAndFlavor(moved.Op)));
		}

		normalize(code);
		return true;
	}

	// Moves where a branch SPLITS — the opening junction bar sliding left or
	// right, the mirror image of MoveBranchEnd.
	//
	//   direction -1:  x  ( a  O b )   →   ( x  a  O b )
	//   direction +1:  ( x  a  O b )   →   x  ( a  O b )
	//
	// Sliding the bar left pulls the symbol before the branch inside, so every
	// other leg now bypasses it too; sliding it right pushes the first leg's head
	// back out onto the main line, where all the legs feed through it again.
	// Between the two ends, a branch can be moved anywhere along the rung without
	// being deleted and rebuilt.
	public bool MoveBranchStart(int network, int index, int direction)
	{
		List<LadderInstruction> code = codeOf(network);
		if (code == null || (direction != -1 && direction != 1))
		{
			return false;
		}

		int open = groupOpenFor(code, index);
		if (open < 0)
		{
			return false;
		}
		int close = matchingClose(code, open);
		if (close < 0)
		{
			return false;
		}

		if (direction < 0)
		{
			// Absorb the symbol in front of the bar into the first leg's head.
			int taken = open - 1;
			if (taken < 0 || !movableAcrossBranchEnd(code[taken]))
			{
				return false;
			}
			LadderInstruction moved = code[taken];
			code.RemoveAt(taken);
			// The bracket slid down one with the removal, so the head of the
			// first leg is now at `open`.
			code.Insert(open, withOp(moved, toAndFlavor(moved.Op)));
		}
		else
		{
			// Release the first leg's head back onto the main line.  As with the
			// other end, the leg may not be emptied.
			int released = open + 1;
			if (firstLegEndOf(code, open, close) <= released + 1
				|| !movableAcrossBranchEnd(code[released]))
			{
				return false;
			}
			LadderInstruction moved = code[released];
			code.RemoveAt(released);
			code.Insert(open, withOp(moved, toAndFlavor(moved.Op)));
		}

		normalize(code);
		return true;
	}

	public bool CanMoveBranchStart(int network, int index, int direction)
	{
		List<LadderInstruction> code = codeOf(network);
		if (code == null || (direction != -1 && direction != 1))
		{
			return false;
		}
		int open = groupOpenFor(code, index);
		if (open < 0)
		{
			return false;
		}
		int close = matchingClose(code, open);
		if (close < 0)
		{
			return false;
		}
		if (direction < 0)
		{
			return open > 0 && movableAcrossBranchEnd(code[open - 1]);
		}
		return firstLegEndOf(code, open, close) > open + 2
			&& movableAcrossBranchEnd(code[open + 1]);
	}

	// Moves one leg of a branch closer to or further from the main line — on the
	// diagram, the leg swapping rows with its neighbour.
	//
	// Parallel legs are order-independent as LOGIC, so this is a drawing choice:
	// which route reads as the normal one and which as the bypass.  It is not
	// only cosmetic though, because the main line is the leg both junction bars
	// extend along — promoting a leg to row 0 is how you choose which one the
	// end handles will grow.
	//
	// `group` names the branch — its bracket, or anything in it — and `member`
	// says which leg, by pointing at anything on that leg.  They are separate
	// because a leg can itself begin with a nested branch: resolving the group
	// from the member would then find the inner one and move the wrong rows.
	//
	// direction -1 moves it towards the main line, +1 away from it.
	public bool MoveBranchLeg(int network, int group, int member, int direction)
	{
		List<LadderInstruction> code = codeOf(network);
		if (code == null || (direction != -1 && direction != 1))
		{
			return false;
		}
		int open = groupOpenFor(code, group);
		if (open < 0)
		{
			return false;
		}
		int close = matchingClose(code, open);
		if (close < 0)
		{
			return false;
		}

		List<int> starts = legStartsOf(code, open, close);
		int leg = legIndexOf(starts, close, member);
		if (leg < 0 || leg + direction < 0 || leg + direction >= starts.Count)
		{
			return false;
		}

		// Always swap the lower-numbered leg with the one after it, so there is
		// one piece of index arithmetic rather than two mirrored ones.
		int upper = direction < 0 ? leg - 1 : leg;
		int upperStart = starts[upper];
		int lowerStart = starts[upper + 1];
		int lowerEnd = upper + 2 < starts.Count ? starts[upper + 2] : close;

		List<LadderInstruction> above = code.GetRange(upperStart, lowerStart - upperStart);
		List<LadderInstruction> below = code.GetRange(lowerStart, lowerEnd - lowerStart);
		code.RemoveRange(upperStart, lowerEnd - upperStart);
		code.InsertRange(upperStart, below);
		code.InsertRange(upperStart + below.Count, above);

		// The connector belongs to the POSITION, not to the leg: whatever ends up
		// first in the group loads, and whatever follows joins with OR.  Swapping
		// the slices without this would merge the two legs into one series.
		int firstHead = upperStart;
		int secondHead = upperStart + below.Count;
		code[firstHead] = withOp(code[firstHead], upper == 0
			? toAndFlavor(code[firstHead].Op)
			: toOrFlavor(code[firstHead].Op));
		code[secondHead] = withOp(code[secondHead], toOrFlavor(code[secondHead].Op));

		normalize(code);
		return true;
	}

	public bool CanMoveBranchLeg(int network, int group, int member, int direction)
	{
		List<LadderInstruction> code = codeOf(network);
		if (code == null || (direction != -1 && direction != 1))
		{
			return false;
		}
		int open = groupOpenFor(code, group);
		if (open < 0)
		{
			return false;
		}
		int close = matchingClose(code, open);
		if (close < 0)
		{
			return false;
		}
		List<int> starts = legStartsOf(code, open, close);
		int leg = legIndexOf(starts, close, member);
		return leg >= 0 && leg + direction >= 0 && leg + direction < starts.Count;
	}

	// Whether the branch end can move in `direction` — the editor asks so it can
	// leave the handle out rather than draw one that refuses every click.
	public bool CanMoveBranchEnd(int network, int index, int direction)
	{
		List<LadderInstruction> code = codeOf(network);
		if (code == null || (direction != -1 && direction != 1))
		{
			return false;
		}
		int open = groupOpenFor(code, index);
		if (open < 0)
		{
			return false;
		}
		int close = matchingClose(code, open);
		if (close < 0)
		{
			return false;
		}
		if (direction > 0)
		{
			int taken = close + 1;
			return taken < code.Count && movableAcrossBranchEnd(code[taken]);
		}
		int released = firstLegEndOf(code, open, close) - 1;
		return released > open + 1 && movableAcrossBranchEnd(code[released]);
	}

	// Only a plain symbol crosses the bar.  A bracket would unbalance the list,
	// and dragging a whole nested group through a junction is a reshape with no
	// single obvious answer — the player can move its contents instead.
	private static bool movableAcrossBranchEnd(LadderInstruction instruction)
	{
		return !LadderInstruction.IsStructural(instruction.Op)
			&& !LadderInstruction.IsCoil(instruction.Op);
	}

	// Where each parallel leg of a group begins, in code order — leg 0 is the one
	// drawn on the main line.
	//
	// The `i > open + 1` guard is the same one countBranches and the layout both
	// apply: a group's FIRST element loads the leg, so an O connector on it is
	// noise rather than a separator.  Without the guard a group written as
	// "A( O a O b )" — which draws as two legs — was read as three, and pulling a
	// symbol into it inserted ahead of `a` and split the leg apart.
	private static List<int> legStartsOf(List<LadderInstruction> code, int open, int close)
	{
		List<int> starts = new List<int>();
		if (close <= open + 1)
		{
			return starts;
		}
		starts.Add(open + 1);

		int depth = 0;
		for (int i = open + 1; i < close; i++)
		{
			LadderOp op = code[i].Op;
			if (depth == 0 && i > open + 1 && isOrFlavor(op))
			{
				starts.Add(i);
			}
			if (op == LadderOp.OpenAnd || op == LadderOp.OpenOr)
			{
				depth++;
			}
			else if (op == LadderOp.Close)
			{
				depth--;
			}
		}
		return starts;
	}

	// Which leg `index` sits on, or -1 when it is not inside the group at all.
	private static int legIndexOf(List<int> starts, int close, int index)
	{
		for (int i = starts.Count - 1; i >= 0; i--)
		{
			if (index >= starts[i])
			{
				return index < close ? i : -1;
			}
		}
		return -1;
	}

	// The index just past the last element of a group's first leg: where leg 1
	// begins, or the closing bracket when there is only one leg.
	private static int firstLegEndOf(List<LadderInstruction> code, int open, int close)
	{
		List<int> starts = legStartsOf(code, open, close);
		return starts.Count > 1 ? starts[1] : close;
	}

	// The opening bracket of the group `index` sits in, or -1 when it is not in
	// one.  Accepts the group's own brackets so the editor can hand back
	// whatever the player clicked.
	private static int groupOpenFor(List<LadderInstruction> code, int index)
	{
		if (index < 0 || index >= code.Count)
		{
			return -1;
		}
		if (code[index].Op == LadderOp.OpenAnd || code[index].Op == LadderOp.OpenOr)
		{
			return index;
		}

		int depth = 0;
		for (int i = index; i > 0; i--)
		{
			LadderOp op = code[i - 1].Op;
			if (op == LadderOp.Close)
			{
				depth++;
			}
			else if (op == LadderOp.OpenAnd || op == LadderOp.OpenOr)
			{
				if (depth == 0)
				{
					return i - 1;
				}
				depth--;
			}
		}
		return -1;
	}

	// ---- network edits ----

	public bool AddNetwork(string title)
	{
		return InsertNetworkAfter(m_networks.Count - 1, title);
	}

	// Adds an empty network directly below `network`; -1 puts it first.
	//
	// Deliberately EMPTY.  Seeding a contact and a coil would put addresses in
	// the program the player never chose.
	public bool InsertNetworkAfter(int network, string title)
	{
		int at = network < 0 ? 0 : network + 1;
		if (at > m_networks.Count)
		{
			at = m_networks.Count;
		}
		m_networks.Insert(at, new LadderNetwork(0, title ?? "", new List<LadderInstruction>()));
		renumber();
		return true;
	}

	public bool DeleteNetwork(int network)
	{
		if (network < 0 || network >= m_networks.Count)
		{
			return false;
		}
		m_networks.RemoveAt(network);
		renumber();
		return true;
	}

	// Empties a network without removing it: position and title stay, so nothing
	// below it renumbers under the player mid-edit.
	public bool ClearNetwork(int network)
	{
		List<LadderInstruction> code = codeOf(network);
		if (code == null || code.Count == 0)
		{
			return false;
		}
		code.Clear();
		return true;
	}

	// Networks run top to bottom once per tick, so their order is program order —
	// moving one is a real edit, not a cosmetic one.
	public bool MoveNetwork(int network, int direction)
	{
		int other = network + direction;
		if (network < 0 || network >= m_networks.Count || other < 0 || other >= m_networks.Count)
		{
			return false;
		}
		LadderNetwork moved = m_networks[network];
		m_networks[network] = m_networks[other];
		m_networks[other] = moved;
		renumber();
		return true;
	}

	public bool SetNetworkTitle(int network, string title)
	{
		if (network < 0 || network >= m_networks.Count)
		{
			return false;
		}
		LadderNetwork old = m_networks[network];
		if (old.Title == (title ?? ""))
		{
			return false;
		}
		m_networks[network] = new LadderNetwork(old.Number, title ?? "", old.Code);
		return true;
	}

	// ---- plumbing ----

	// An element is placeable if it exists, isn't a bracket (the layout owns
	// those), and — when it carries an address — points at one it is allowed to.
	// NOT and the boxes carry no address at all, which is fine.
	private static bool isPlaceable(LadderInstruction instruction)
	{
		if (instruction == null || LadderInstruction.IsStructural(instruction.Op))
		{
			return false;
		}
		if (instruction.Operand == null)
		{
			return instruction.Op == LadderOp.Not || LadderInstruction.IsBox(instruction.Op);
		}
		return CanAddress(instruction.Op, instruction.Operand);
	}

	private List<LadderInstruction> codeOf(int network)
	{
		if (network < 0 || network >= m_networks.Count)
		{
			return null;
		}
		return m_networks[network].Code;
	}

	// Element edits funnel through here so none of them can forget the two
	// guards that matter: the index has to exist, and it has to point at a
	// SYMBOL.  Brackets are structure the layout owns, not something the player
	// addresses — rewriting one would unbalance the network.
	private bool applyAt(int network, int index, System.Func<LadderInstruction, LadderInstruction> transform)
	{
		List<LadderInstruction> code = codeOf(network);
		if (code == null || index < 0 || index >= code.Count)
		{
			return false;
		}
		if (LadderInstruction.IsStructural(code[index].Op))
		{
			return false;
		}
		code[index] = transform(code[index]);
		normalize(code);
		return true;
	}

	// Networks are numbered by position, so adding, removing or moving one
	// renumbers the rest — otherwise the text would carry gaps that mean nothing.
	private void renumber()
	{
		for (int i = 0; i < m_networks.Count; i++)
		{
			m_networks[i] = new LadderNetwork(i + 1, m_networks[i].Title, m_networks[i].Code);
		}
	}

	private static LadderInstruction bracket(LadderOp op)
	{
		return new LadderInstruction(op, null, null, LadderCompare.Greater, 0, null, -1, 0);
	}

	private static LadderInstruction withOp(LadderInstruction source, LadderOp op)
	{
		return new LadderInstruction(
			op, source.Operand, source.Right, source.Compare, source.Preset, source.Reset, -1, 0);
	}

	private static bool isOrFlavor(LadderOp op)
	{
		return op == LadderOp.Or || op == LadderOp.OrNot || op == LadderOp.OpenOr;
	}

	private static LadderOp toAndFlavor(LadderOp op)
	{
		switch (op)
		{
		case LadderOp.Or:
			return LadderOp.And;
		case LadderOp.OrNot:
			return LadderOp.AndNot;
		case LadderOp.OpenOr:
			return LadderOp.OpenAnd;
		default:
			return op;
		}
	}

	private static LadderOp toOrFlavor(LadderOp op)
	{
		switch (op)
		{
		case LadderOp.And:
			return LadderOp.Or;
		case LadderOp.AndNot:
			return LadderOp.OrNot;
		case LadderOp.OpenAnd:
			return LadderOp.OpenOr;
		default:
			return op;
		}
	}

	// Can a bracket pair go around [from..to] without unbalancing anything? Only
	// if the span holds no coil and closes every group it opens.
	private static bool isWrappable(List<LadderInstruction> code, int from, int to)
	{
		int depth = 0;
		for (int i = from; i <= to; i++)
		{
			LadderOp op = code[i].Op;
			if (LadderInstruction.IsCoil(op))
			{
				return false;
			}
			if (op == LadderOp.OpenAnd || op == LadderOp.OpenOr)
			{
				depth++;
			}
			else if (op == LadderOp.Close)
			{
				depth--;
				if (depth < 0)
				{
					return false;
				}
			}
		}
		return depth == 0;
	}

	// Where the chain that `index` belongs to begins: the start of the innermost
	// group enclosing it, or the start of the network.  This is the span the
	// editor offers as "parallel to everything up to here".
	public int GroupStart(int network, int index)
	{
		List<LadderInstruction> code = codeOf(network);
		if (code == null || index < 0 || index >= code.Count)
		{
			return 0;
		}

		int depth = 0;
		for (int i = index; i > 0; i--)
		{
			LadderOp op = code[i - 1].Op;
			if (op == LadderOp.Close)
			{
				depth++;
			}
			else if (op == LadderOp.OpenAnd || op == LadderOp.OpenOr)
			{
				if (depth == 0)
				{
					return i;
				}
				depth--;
			}
		}
		return 0;
	}

	// The last index at the same nesting level as `index`, i.e. the element just
	// before the Close that ends the group enclosing it.  Returns -1 when `index`
	// is not inside a group at all.
	//
	// Paired with GroupStart this gives the whole span of an existing branch,
	// which is what "add another leg to this branch" has to wrap: wrapping the
	// group's CONTENTS produces one flat group with an extra leg, where wrapping
	// a single leg would nest a second group inside the first.
	public int GroupEnd(int network, int index)
	{
		List<LadderInstruction> code = codeOf(network);
		if (code == null || index < 0 || index >= code.Count)
		{
			return -1;
		}
		if (GroupStart(network, index) == 0 && !opensAt(code, 0))
		{
			return -1;
		}

		int depth = 0;
		for (int i = index; i < code.Count; i++)
		{
			LadderOp op = code[i].Op;
			if (op == LadderOp.OpenAnd || op == LadderOp.OpenOr)
			{
				depth++;
			}
			else if (op == LadderOp.Close)
			{
				if (depth == 0)
				{
					return i - 1;
				}
				depth--;
			}
		}
		return -1;
	}

	private static bool opensAt(List<LadderInstruction> code, int index)
	{
		if (index < 0 || index >= code.Count)
		{
			return false;
		}
		LadderOp op = code[index].Op;
		return op == LadderOp.OpenAnd || op == LadderOp.OpenOr;
	}

	// Collapses brackets that no longer earn their place, then tidies the
	// connector on whatever ends up first in each group.  Runs to a fixed point
	// because unwrapping one group can expose another.
	private static void normalize(List<LadderInstruction> code)
	{
		bool changed = true;
		while (changed)
		{
			changed = collapseOnce(code);
		}

		// The first element of a group (and of the network) is a LOAD, so an
		// O / ON connector there is noise.  Rewriting it keeps the text in the
		// shape the parser emits for the same ladder.
		if (code.Count > 0)
		{
			code[0] = withOp(code[0], toAndFlavor(code[0].Op));
		}
		for (int i = 0; i < code.Count - 1; i++)
		{
			if (code[i].Op == LadderOp.OpenAnd || code[i].Op == LadderOp.OpenOr)
			{
				code[i + 1] = withOp(code[i + 1], toAndFlavor(code[i + 1].Op));
			}
		}
	}

	private static bool collapseOnce(List<LadderInstruction> code)
	{
		for (int open = 0; open < code.Count; open++)
		{
			if (code[open].Op != LadderOp.OpenAnd && code[open].Op != LadderOp.OpenOr)
			{
				continue;
			}

			int close = matchingClose(code, open);
			if (close < 0)
			{
				continue;
			}

			// Empty group — drop both brackets.
			if (close == open + 1)
			{
				code.RemoveAt(close);
				code.RemoveAt(open);
				return true;
			}

			if (countBranches(code, open, close) > 1)
			{
				continue;
			}

			// Single leg: the group adds nothing, so unwrap it and give the
			// content the connector the group itself carried.
			LadderOp groupOp = code[open].Op;
			code.RemoveAt(close);
			code.RemoveAt(open);
			LadderOp inner = groupOp == LadderOp.OpenOr
				? toOrFlavor(code[open].Op)
				: toAndFlavor(code[open].Op);
			code[open] = withOp(code[open], inner);
			return true;
		}
		return false;
	}

	private static int matchingClose(List<LadderInstruction> code, int open)
	{
		int depth = 0;
		for (int i = open; i < code.Count; i++)
		{
			LadderOp op = code[i].Op;
			if (op == LadderOp.OpenAnd || op == LadderOp.OpenOr)
			{
				depth++;
			}
			else if (op == LadderOp.Close)
			{
				depth--;
				if (depth == 0)
				{
					return i;
				}
			}
		}
		return -1;
	}

	// How many parallel legs a group holds.  A leg starts at the group's first
	// element and at every top-level element (or nested group) that joins with an
	// OR connector.
	private static int countBranches(List<LadderInstruction> code, int open, int close)
	{
		int branches = 1;
		int depth = 0;
		for (int i = open + 1; i < close; i++)
		{
			LadderOp op = code[i].Op;
			if (depth == 0 && i > open + 1 && isOrFlavor(op))
			{
				branches++;
			}
			if (op == LadderOp.OpenAnd || op == LadderOp.OpenOr)
			{
				depth++;
			}
			else if (op == LadderOp.Close)
			{
				depth--;
			}
		}
		return branches;
	}
}
