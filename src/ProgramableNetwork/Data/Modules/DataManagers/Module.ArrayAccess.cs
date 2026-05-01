using Mafi;
using System;

namespace ProgramableNetwork
{
    public partial class Module
    {
        // Convenience wrapper around <see cref="Module.ArrayData"/> — bounds-safe
        // indexer, length, and lifecycle helpers.  Direct access to the raw
        // <c>Fix32[]</c> via <see cref="Module.ArrayData"/> stays available for hot
        // inner loops where the per-call bounds check is unwanted.
        //
        // Exposed to Python as `module.Array` (see ModuleWrapper.ArraySetter).
        public class ArrayAccess
        {
            private readonly Module m_module;

            internal ArrayAccess(Module module)
            {
                m_module = module;
            }

            // Current length — same as Module.ArrayData.Length but safe against
            // a null reference (treats null as zero-length).
            public int Length => m_module.ArrayData?.Length ?? 0;

            // Bounds-safe getter.  Returns Fix32.Zero for out-of-range indices
            // so callers don't need their own guard (matches the get-or-default
            // shape of OutputData / InputData).
            public Fix32 this[int idx]
            {
                get => (idx >= 0 && idx < Length) ? m_module.ArrayData[idx] : Fix32.Zero;
                set { if (idx >= 0 && idx < Length) m_module.ArrayData[idx] = value; }
            }

            // Bounds-safe getter with explicit default for callers that want to
            // distinguish "out-of-range" from "zero".
            public Fix32 this[int idx, Fix32 defaultValue]
            {
                get => (idx >= 0 && idx < Length) ? m_module.ArrayData[idx] : defaultValue;
            }

            // Resize the buffer.  Growing fills new tail slots with `fillNew`
            // (default Fix32.Zero); shrinking truncates from the tail.  Reuses the
            // existing array reference when length is unchanged so callers can
            // call this every tick without allocating.  Negative sizes are
            // clamped to zero.  The `fillNew` overload exists so Python modules
            // (where for-loops aren't available) can seed fresh slots in one call.
            public void Resize(int size) => Resize(size, Fix32.Zero);

            public void Resize(int size, Fix32 fillNew)
            {
                if (size < 0) {
                    size = 0;
                }
                var current = m_module.ArrayData ?? System.Array.Empty<Fix32>();
                if (current.Length == size) {
                    return;
                }
                var grown = new Fix32[size];
                int copy = Math.Min(current.Length, size);
                if (copy > 0) {
                    System.Array.Copy(current, grown, copy);
                }
                if (fillNew.RawValue != 0) {
                    for (int i = copy; i < size; i++) {
                        grown[i] = fillNew;
                    }
                }
                m_module.ArrayData = grown;
            }

            // Zero every slot in-place.  Does NOT shrink the buffer — use
            // <see cref="Resize"/> for that.  Cheaper than Resize(0) + Resize(N)
            // when the size is staying the same.
            public void Clear()
            {
                var arr = m_module.ArrayData;
                if (arr == null) {
                    return;
                }
                for (int i = 0; i < arr.Length; i++) {
                    arr[i] = Fix32.Zero;
                }
            }

            // Atomic shift-register step: shifts every slot one position left
            // (slot[i] = slot[i+1]), writes <paramref name="incoming"/> to the
            // last slot, and returns the value that was previously in slot[0].
            // Empty arrays just echo the incoming value back, matching what an
            // unbuffered pass-through would do.
            //
            // Exposed so Python modules can implement an array-copy delay/FIR
            // without writing a per-tick loop (the parser only supports if/else
            // statements, not for/while).
            public Fix32 ShiftLeftWith(Fix32 incoming)
            {
                var arr = m_module.ArrayData;
                if (arr == null || arr.Length == 0) {
                    return incoming;
                }
                Fix32 oldest = arr[0];
                int last = arr.Length - 1;
                for (int i = 0; i < last; i++) {
                    arr[i] = arr[i + 1];
                }
                arr[last] = incoming;
                return oldest;
            }
        }
    }
}
