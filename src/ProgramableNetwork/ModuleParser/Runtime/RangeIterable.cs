using System.Collections;
using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    // Lazy range-of-ints — the iterable PlcPy.range(start, stop, step) returns
    // instead of materialising a List<int>.  For a `for i in range(10000):`
    // loop this saves the 40 KB backing array (10000 × 4 bytes) plus the
    // per-add bookkeeping; the player only ever sees one int at a time so the
    // list was pure overhead.
    //
    // Implements IEnumerable<int> for compatibility with anything that just
    // wants to iterate (existing __getitem__ / __setitem__ / len / for paths
    // all reach the right branch through the interfaces below).  Indexing is
    // O(1) — the Nth value is start + N*step, computed on demand.
    //
    // ForStatement has a dedicated fast path that recognises RangeIterable
    // and iterates with a plain int loop, avoiding both the IEnumerator
    // allocation and the int→object boxing the IEnumerable path forces.  The
    // class still IS an IEnumerable so callers that don't bother with the
    // fast path (Expressions.__contains__, len()'s IEnumerable fallback,
    // etc.) keep working without a special case.
    public sealed class RangeIterable : IEnumerable<int>
    {
        public readonly int Start;
        public readonly int Stop;
        public readonly int Step;

        public RangeIterable(int start, int stop, int step)
        {
            // Step is required to be non-zero by the caller (PlcPy's range
            // constructor throws on step==0 before ever reaching here);
            // the assertion would otherwise loop forever in the enumerator.
            Start = start;
            Stop = stop;
            Step = step;
        }

        // Number of ints the range yields.  Branch on step's sign so we
        // compute "(stop - start) rounded toward zero, divided by step,
        // then ceiling'd" without special-casing each direction inline.
        // Returns 0 when the range is empty (start past stop in the
        // direction of step), matching Python's len(range(5, 5)) == 0.
        public int Length
        {
            get
            {
                if (Step > 0)
                {
                    if (Start >= Stop) {
                        return 0;
                    }
                    // Ceiling division — (a + b - 1) / b for positive a, b.
                    return (Stop - Start + Step - 1) / Step;
                }
                // Step < 0: stop is the lower bound.
                if (Start <= Stop) {
                    return 0;
                }
                int absStep = -Step;
                return (Start - Stop + absStep - 1) / absStep;
            }
        }

        // Indexer so `range(10)[3]` resolves to 3.  Used by the property-
        // reflection fallback in Expressions.__getitem__ — the public
        // `int this[int]` is exactly the shape that branch looks for.
        public int this[int index]
        {
            get
            {
                if (index < 0 || index >= Length) {
                    throw new System.ArgumentOutOfRangeException(
                        $"range index {index} out of range [0,{Length})");
                }
                return Start + index * Step;
            }
        }

        public IEnumerator<int> GetEnumerator()
        {
            if (Step > 0)
            {
                for (int i = Start; i < Stop; i += Step) {
                    yield return i;
                }
            }
            else
            {
                for (int i = Start; i > Stop; i += Step) {
                    yield return i;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
