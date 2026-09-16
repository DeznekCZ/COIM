using System.Collections.Generic;
using Mafi;

namespace ProgramableNetwork.Ladder;

// Executes a LadderProgram, one scan per module tick.
//
// The evaluator is the classic statement-list machine: a single boolean
// accumulator (the RLO — "result of logic operation"), a first-check flag that
// makes the first contact of a logic string LOAD instead of AND, and a stack
// for bracketed branches.  Coils write the RLO without consuming it, which is
// what makes two coils in a row read as two parallel outputs on the same rung.
//
// All persistent state (edge memories, timer accumulators, counter values)
// lives in one int array so the module can drop it straight into NumberData
// and get it back on load without a bespoke serializer.
public sealed class LadderMachine
{
	private readonly LadderProgram m_program;
	private readonly int[] m_state;

	// Per-instruction conduction from the last scan, in program order
	// (networks in sequence, instructions within each).  The graphical editor
	// paints energized elements from this; nothing else reads it, so it is
	// only filled when the caller asks for it.
	private bool[] m_trace;
	private bool m_traceEnabled;

	private struct Frame
	{
		public bool SavedRlo;
		public bool SavedFirstCheck;
		public bool IsAnd;
	}

	public LadderMachine(LadderProgram program, int[] state)
	{
		m_program = program;
		m_state = new int[program.StateSlotCount];
		if (state != null)
		{
			int n = state.Length < m_state.Length ? state.Length : m_state.Length;
			for (int i = 0; i < n; i++)
			{
				m_state[i] = state[i];
			}
		}
	}

	public LadderProgram Program => m_program;

	// The array the module persists.  Handed out directly rather than copied —
	// the module writes it to NumberData at save time and never mutates it.
	public int[] State => m_state;

	public bool[] Trace => m_trace;

	public void EnableTrace(bool enabled)
	{
		m_traceEnabled = enabled;
		if (!enabled)
		{
			m_trace = null;
			return;
		}
		int count = 0;
		foreach (LadderNetwork network in m_program.Networks)
		{
			count += network.Code.Count;
		}
		if (m_trace == null || m_trace.Length != count)
		{
			m_trace = new bool[count];
		}
	}

	public void Scan(ILadderIo io)
	{
		io.BeginScan();

		List<Frame> stack = new List<Frame>();
		int traceAt = 0;

		foreach (LadderNetwork network in m_program.Networks)
		{
			bool rlo = false;
			bool firstCheck = false;
			stack.Clear();

			foreach (LadderInstruction instruction in network.Code)
			{
				switch (instruction.Op)
				{
				case LadderOp.And:
					apply(ref rlo, ref firstCheck, isAnd: true, value: truthy(read(io, instruction.Operand)));
					break;
				case LadderOp.AndNot:
					apply(ref rlo, ref firstCheck, isAnd: true, value: !truthy(read(io, instruction.Operand)));
					break;
				case LadderOp.Or:
					apply(ref rlo, ref firstCheck, isAnd: false, value: truthy(read(io, instruction.Operand)));
					break;
				case LadderOp.OrNot:
					apply(ref rlo, ref firstCheck, isAnd: false, value: !truthy(read(io, instruction.Operand)));
					break;

				case LadderOp.EdgeRising:
				case LadderOp.EdgeFalling:
				{
					bool now = truthy(read(io, instruction.Operand));
					bool was = m_state[instruction.StateSlot] != 0;
					m_state[instruction.StateSlot] = now ? 1 : 0;
					bool pulse = instruction.Op == LadderOp.EdgeRising
						? now && !was
						: !now && was;
					apply(ref rlo, ref firstCheck, isAnd: true, value: pulse);
					break;
				}

				case LadderOp.OpenAnd:
				case LadderOp.OpenOr:
					stack.Add(new Frame
					{
						SavedRlo = rlo,
						SavedFirstCheck = firstCheck,
						IsAnd = instruction.Op == LadderOp.OpenAnd,
					});
					rlo = false;
					firstCheck = false;
					break;

				case LadderOp.Close:
				{
					// The parser guarantees balance, so the stack is never
					// empty here.
					Frame frame = stack[stack.Count - 1];
					stack.RemoveAt(stack.Count - 1);
					bool inner = rlo;
					if (!frame.SavedFirstCheck)
					{
						rlo = inner;
					}
					else if (frame.IsAnd)
					{
						rlo = frame.SavedRlo && inner;
					}
					else
					{
						rlo = frame.SavedRlo || inner;
					}
					firstCheck = true;
					break;
				}

				// Inverts the accumulator in place.  With the first-check flag
				// clear the accumulator is still false, so a network opening
				// with NOT starts out conducting — which is Siemens' behaviour
				// and the cheapest way to spell "always on".
				case LadderOp.Not:
					rlo = !rlo;
					firstCheck = true;
					break;

				case LadderOp.Assign:
					io.Write(instruction.Operand, rlo ? Fix32.One : Fix32.Zero);
					firstCheck = false;
					break;
				case LadderOp.AssignNot:
					io.Write(instruction.Operand, rlo ? Fix32.Zero : Fix32.One);
					firstCheck = false;
					break;

				case LadderOp.Toggle:
				{
					int value = m_state[instruction.StateSlot];
					bool was = m_state[instruction.StateSlot + 1] != 0;
					m_state[instruction.StateSlot + 1] = rlo ? 1 : 0;
					if (rlo && !was)
					{
						value = value != 0 ? 0 : 1;
						m_state[instruction.StateSlot] = value;
					}
					// Written every scan, not just on the flip: the address has
					// to keep reading the toggle's value even while nothing is
					// happening, or a buffered output would fall back to zero.
					io.Write(instruction.Operand, value != 0 ? Fix32.One : Fix32.Zero);
					firstCheck = false;
					break;
				}
				case LadderOp.Set:
					if (rlo)
					{
						io.Write(instruction.Operand, Fix32.One);
					}
					firstCheck = false;
					break;
				case LadderOp.Reset:
					if (rlo)
					{
						io.Write(instruction.Operand, Fix32.Zero);
					}
					firstCheck = false;
					break;

				case LadderOp.TimerOn:
				{
					int acc = m_state[instruction.StateSlot];
					acc = rlo ? (acc < instruction.Preset ? acc + 1 : instruction.Preset) : 0;
					m_state[instruction.StateSlot] = acc;
					rlo = acc >= instruction.Preset;
					firstCheck = true;
					break;
				}
				case LadderOp.TimerOff:
				{
					int acc = m_state[instruction.StateSlot];
					acc = rlo ? instruction.Preset : (acc > 0 ? acc - 1 : 0);
					m_state[instruction.StateSlot] = acc;
					rlo = acc > 0;
					firstCheck = true;
					break;
				}
				// Unlike TON, the rung only ARMS the pulse.  Once started it
				// runs its full length even if the rung drops on the very next
				// scan, which is the whole point of a pulse.
				case LadderOp.TimerPulse:
				{
					int left = m_state[instruction.StateSlot];
					bool was = m_state[instruction.StateSlot + 1] != 0;
					m_state[instruction.StateSlot + 1] = rlo ? 1 : 0;
					if (rlo && !was)
					{
						left = instruction.Preset;
					}
					else if (left > 0)
					{
						left--;
					}
					m_state[instruction.StateSlot] = left;
					rlo = left > 0;
					firstCheck = true;
					break;
				}

				case LadderOp.CountUp:
				{
					int count = m_state[instruction.StateSlot];
					bool was = m_state[instruction.StateSlot + 1] != 0;
					m_state[instruction.StateSlot + 1] = rlo ? 1 : 0;

					bool reset = instruction.Reset != null && truthy(read(io, instruction.Reset));
					if (reset)
					{
						count = 0;
					}
					else if (rlo && !was && count < instruction.Preset)
					{
						// Clamped at the preset so a counter left running for
						// a few in-game years can't roll the int over.
						count++;
					}
					m_state[instruction.StateSlot] = count;
					rlo = count >= instruction.Preset;
					firstCheck = true;
					break;
				}

				case LadderOp.Compare:
				{
					Fix32 left = read(io, instruction.Operand);
					Fix32 right = read(io, instruction.Right);
					apply(ref rlo, ref firstCheck, isAnd: true, value: test(left, instruction.Compare, right));
					break;
				}
				}

				if (m_traceEnabled && traceAt < m_trace.Length)
				{
					m_trace[traceAt] = rlo;
				}
				traceAt++;
			}
		}

		io.EndScan();
	}

	// Folds one boolean term into the accumulator.  When the first-check flag
	// is clear the term REPLACES the accumulator instead of combining with it —
	// that is what lets a network open with `A I.A` (a load) and what makes a
	// coil, which clears the flag, start a fresh logic string.
	private static void apply(ref bool rlo, ref bool firstCheck, bool isAnd, bool value)
	{
		if (!firstCheck)
		{
			rlo = value;
			firstCheck = true;
			return;
		}
		if (isAnd)
		{
			rlo = rlo && value;
		}
		else
		{
			rlo = rlo || value;
		}
	}

	// Literals never reach the world — resolving them here keeps every
	// ILadderIo implementation free of a special case it would only ever hit
	// from the right-hand side of a CMP.
	private static Fix32 read(ILadderIo io, LadderOperand operand)
	{
		if (operand.Kind == LadderOperandKind.Literal)
		{
			return operand.Literal;
		}
		return io.Read(operand);
	}

	// Signals in this mod are numeric; every boolean module treats "> 0" as
	// true, and a ladder contact has to agree with them.
	private static bool truthy(Fix32 value)
	{
		return value > Fix32.Zero;
	}

	private static bool test(Fix32 left, LadderCompare compare, Fix32 right)
	{
		switch (compare)
		{
		case LadderCompare.Greater:
			return left > right;
		case LadderCompare.Less:
			return left < right;
		case LadderCompare.Equal:
			return left == right;
		case LadderCompare.GreaterOrEqual:
			return left >= right;
		case LadderCompare.LessOrEqual:
			return left <= right;
		default:
			return left != right;
		}
	}
}
