using Mafi;
using System;
using System.Collections.Generic;

namespace ProgramableNetwork
{
    internal static class SignalBufferPool
    {
        public const int Size = 16;
        private const int MaxPooled = 64;

        private static readonly Stack<Fix32[]> s_pool = new Stack<Fix32[]>();

        public static Fix32[] Rent()
        {
            return s_pool.Count > 0 ? s_pool.Pop() : new Fix32[Size];
        }

        public static void Return(Fix32[] buffer)
        {
            if (buffer == null || buffer.Length != Size) {
				return;
			}
			if (s_pool.Count >= MaxPooled) {
				return;
			}
			Array.Clear(buffer, 0, Size);
            s_pool.Push(buffer);
        }
    }
}
