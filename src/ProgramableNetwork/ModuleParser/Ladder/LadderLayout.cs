using System.Collections.Generic;

namespace ProgramableNetwork.Ladder;

// Turns a network's flat instruction list into a 2D grid the editor can draw.
//
// This is the half of "ladder editor" that has nothing to do with Unity: the
// statement list is serial, the ladder is planar, and reconstructing one from
// the other is where the bugs live.  Keeping it here means it compiles into the
// source-linked Runtime project and can be tested without the game — the same
// bargain the parser and scan machine take.
//
// The reconstruction mirrors how LadderParser emits brackets: inside a group,
// an element whose connector is O / ON starts a new parallel branch, and
// everything else extends the current series.  A( / O( open a nested group and
// ) closes it.

// One drawable symbol, with the index it came from so an edit can address it.
public sealed class LadderCell
{
	public readonly LadderInstruction Instruction;
	public readonly int Index;
	public readonly int Row;
	public readonly int Column;

	public LadderCell(LadderInstruction instruction, int index, int row, int column)
	{
		Instruction = instruction;
		Index = index;
		Row = row;
		Column = column;
	}
}

// A vertical junction bar. Parallel branches get two: one where the group
// opens, one where it closes.
public sealed class LadderBranchSpan
{
	public readonly int Column;
	public readonly int FromRow;
	public readonly int ToRow;
	public readonly bool IsOpening;

	// A code index of some element inside the group this bar belongs to, so the
	// editor can act on the GROUP from a bar the player clicked.  The bracket
	// itself is not usable for that: the layout drops brackets when it rebuilds
	// the tree, and a parallel section written without them has none to point at.
	// -1 when the group somehow holds no element.
	public readonly int AnchorIndex;

	public LadderBranchSpan(int column, int fromRow, int toRow, bool isOpening, int anchorIndex)
	{
		Column = column;
		FromRow = fromRow;
		ToRow = toRow;
		IsOpening = isOpening;
		AnchorIndex = anchorIndex;
	}
}

// One rung: a block of logic followed by the coils it drives.  A network is
// usually a single rung, but nothing stops a player writing logic, a coil, then
// more logic — so the layout models the general case.
public sealed class LadderRungLayout
{
	public readonly List<LadderCell> Cells;
	public readonly List<LadderCell> Coils;
	public readonly List<LadderBranchSpan> Branches;
	public readonly int Rows;
	public readonly int Columns;

	// Rows the CONTACT network occupies — i.e. how many parallel logic paths the
	// player actually authored with OR connectors.
	//
	// This is deliberately not `Rows`.  Two coils on one rung are two parallel
	// OUTPUTS, drawn stacked, and they push `Rows` to 2 without any branch
	// existing in the logic.  Charging that against the tier's branch budget (or
	// offering contact slots on the coil's row) invents a branch the player never
	// asked for, which is exactly the confusion this field exists to prevent.
	public readonly int LogicRows;

	public LadderRungLayout(
		List<LadderCell> cells,
		List<LadderCell> coils,
		List<LadderBranchSpan> branches,
		int rows,
		int columns,
		int logicRows)
	{
		Cells = cells;
		Coils = coils;
		Branches = branches;
		Rows = rows;
		Columns = columns;
		LogicRows = logicRows;
	}
}

public sealed class LadderNetworkLayout
{
	public readonly LadderNetwork Network;
	public readonly List<LadderRungLayout> Rungs;
	public readonly int Rows;
	public readonly int Columns;

	// The widest logic row count of any rung — the network's branch depth.
	public readonly int LogicRows;

	public LadderNetworkLayout(LadderNetwork network, List<LadderRungLayout> rungs)
	{
		Network = network;
		Rungs = rungs;
		int rows = 0;
		int columns = 0;
		int logicRows = 0;
		foreach (LadderRungLayout rung in rungs)
		{
			rows += rung.Rows;
			if (rung.Columns > columns)
			{
				columns = rung.Columns;
			}
			if (rung.LogicRows > logicRows)
			{
				logicRows = rung.LogicRows;
			}
		}
		Rows = rows;
		Columns = columns;
		LogicRows = logicRows;
	}
}

public static class LadderLayout
{
	public static LadderNetworkLayout Build(LadderNetwork network)
	{
		List<LadderRungLayout> rungs = new List<LadderRungLayout>();
		List<LadderInstruction> code = network.Code;

		int at = 0;
		while (at < code.Count)
		{
			// A rung runs to the end of the trailing coil group: logic, then
			// one or more coils.  Everything after that starts the next rung.
			int logicEnd = at;
			while (logicEnd < code.Count && !isCoil(code[logicEnd].Op))
			{
				logicEnd++;
			}
			int coilEnd = logicEnd;
			while (coilEnd < code.Count && isCoil(code[coilEnd].Op))
			{
				coilEnd++;
			}
			// No coil at all (the parser rejects this, but a half-edited
			// document in the editor can hit it) — treat the remainder as one
			// coil-less rung so it still draws.
			if (coilEnd == logicEnd && logicEnd == code.Count)
			{
				coilEnd = code.Count;
			}

			rungs.Add(buildRung(code, at, logicEnd, coilEnd));
			at = coilEnd > at ? coilEnd : at + 1;
		}

		if (rungs.Count == 0)
		{
			rungs.Add(new LadderRungLayout(
				new List<LadderCell>(), new List<LadderCell>(), new List<LadderBranchSpan>(), 1, 0, 1));
		}
		return new LadderNetworkLayout(network, rungs);
	}

	private static bool isCoil(LadderOp op)
	{
		return LadderInstruction.IsCoil(op);
	}

	private static LadderRungLayout buildRung(List<LadderInstruction> code, int start, int logicEnd, int coilEnd)
	{
		int cursor = start;
		Node root = buildGroup(code, ref cursor, logicEnd);
		measure(root);

		List<LadderCell> cells = new List<LadderCell>();
		List<LadderBranchSpan> branches = new List<LadderBranchSpan>();
		place(root, 0, 0, cells, branches);

		// Coils hang off the right of the rung, stacked — two coils in a row
		// are two parallel outputs on the same rung.
		List<LadderCell> coils = new List<LadderCell>();
		int coilRow = 0;
		for (int i = logicEnd; i < coilEnd; i++)
		{
			coils.Add(new LadderCell(code[i], i, coilRow, root.Width));
			coilRow++;
		}

		int logicRows = root.Height < 1 ? 1 : root.Height;
		int rows = logicRows > coilRow ? logicRows : coilRow;
		return new LadderRungLayout(
			cells, coils, branches, rows, root.Width + (coils.Count > 0 ? 1 : 0), logicRows);
	}

	// ---- tree reconstruction ----

	private abstract class Node
	{
		public int Width;
		public int Height;

		// Where this subtree's opening bracket sits in the flat list, or -1 when
		// it has none.  The tree itself drops brackets, but the editor addresses
		// a GROUP by its bracket — pointing at a leaf inside it instead would
		// name the innermost group the leaf belongs to, which for a leg that
		// starts with a nested branch is the wrong one.
		public int BracketIndex = -1;
	}

	private sealed class ElementNode : Node
	{
		public LadderInstruction Instruction;
		public int Index;
	}

	private sealed class SeriesNode : Node
	{
		public readonly List<Node> Items = new List<Node>();
	}

	private sealed class ParallelNode : Node
	{
		public readonly List<Node> Branches = new List<Node>();
	}

	private static Node buildGroup(List<LadderInstruction> code, ref int i, int end)
	{
		List<List<Node>> branches = new List<List<Node>>();
		List<Node> current = new List<Node>();

		while (i < end)
		{
			LadderInstruction instruction = code[i];

			if (instruction.Op == LadderOp.Close)
			{
				i++;
				break;
			}

			if (instruction.Op == LadderOp.OpenAnd || instruction.Op == LadderOp.OpenOr)
			{
				bool opensBranch = instruction.Op == LadderOp.OpenOr;
				int bracketAt = i;
				i++;
				Node nested = buildGroup(code, ref i, end);
				nested.BracketIndex = bracketAt;
				if (opensBranch && current.Count > 0)
				{
					branches.Add(current);
					current = new List<Node> { nested };
				}
				else
				{
					current.Add(nested);
				}
				continue;
			}

			bool startsBranch = instruction.Op == LadderOp.Or || instruction.Op == LadderOp.OrNot;
			ElementNode element = new ElementNode
			{
				Instruction = instruction,
				Index = i,
			};
			if (startsBranch && current.Count > 0)
			{
				branches.Add(current);
				current = new List<Node> { element };
			}
			else
			{
				current.Add(element);
			}
			i++;
		}

		branches.Add(current);

		if (branches.Count == 1)
		{
			return asSeries(branches[0]);
		}
		ParallelNode parallel = new ParallelNode();
		foreach (List<Node> branch in branches)
		{
			parallel.Branches.Add(asSeries(branch));
		}
		return parallel;
	}

	private static Node asSeries(List<Node> items)
	{
		if (items.Count == 1)
		{
			return items[0];
		}
		SeriesNode series = new SeriesNode();
		series.Items.AddRange(items);
		return series;
	}

	// ---- measure + place ----

	private static void measure(Node node)
	{
		if (node is ElementNode)
		{
			node.Width = 1;
			node.Height = 1;
			return;
		}

		if (node is SeriesNode series)
		{
			int width = 0;
			int height = 1;
			foreach (Node item in series.Items)
			{
				measure(item);
				width += item.Width;
				if (item.Height > height)
				{
					height = item.Height;
				}
			}
			node.Width = width;
			node.Height = height;
			return;
		}

		ParallelNode parallel = (ParallelNode)node;
		int widest = 1;
		int total = 0;
		foreach (Node branch in parallel.Branches)
		{
			measure(branch);
			if (branch.Width > widest)
			{
				widest = branch.Width;
			}
			total += branch.Height;
		}
		node.Width = widest;
		node.Height = total;
	}

	// Elements are top-aligned inside a taller container.  A real schematic
	// centres them; the renderer can do that from Row/Column without the layout
	// having to commit to a pixel model.
	private static void place(Node node, int row, int column, List<LadderCell> cells, List<LadderBranchSpan> branches)
	{
		if (node is ElementNode element)
		{
			cells.Add(new LadderCell(element.Instruction, element.Index, row, column));
			return;
		}

		if (node is SeriesNode series)
		{
			int at = column;
			foreach (Node item in series.Items)
			{
				place(item, row, at, cells, branches);
				at += item.Width;
			}
			return;
		}

		ParallelNode parallel = (ParallelNode)node;
		int branchRow = row;
		foreach (Node branch in parallel.Branches)
		{
			place(branch, branchRow, column, cells, branches);
			branchRow += branch.Height;
		}
		int lastRow = branchRow - 1;
		// The bracket when there is one; otherwise a leaf, which is enough to
		// draw with but not to edit — a parallel section written without brackets
		// has no group for an edit to move.
		int anchor = node.BracketIndex >= 0 ? node.BracketIndex : firstIndexOf(node);
		branches.Add(new LadderBranchSpan(column, row, lastRow, true, anchor));
		branches.Add(new LadderBranchSpan(column + node.Width, row, lastRow, false, anchor));
	}

	// The code index of the leftmost element the subtree holds — enough for the
	// editor to find the group again in the flat list.
	private static int firstIndexOf(Node node)
	{
		if (node is ElementNode element)
		{
			return element.Index;
		}
		if (node is SeriesNode series)
		{
			foreach (Node item in series.Items)
			{
				int index = firstIndexOf(item);
				if (index >= 0)
				{
					return index;
				}
			}
			return -1;
		}
		foreach (Node branch in ((ParallelNode)node).Branches)
		{
			int index = firstIndexOf(branch);
			if (index >= 0)
			{
				return index;
			}
		}
		return -1;
	}
}
