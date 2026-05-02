using System;

namespace ProgramableNetwork.Python
{
    // Thrown by `continue` to skip the remainder of the current loop body
    // and resume at the next iteration.  ForStatement / WhileStatement
    // catch it inside their per-iteration try block.  See BreakException
    // for the rationale of using exceptions for control flow.
    [Serializable]
    public class ContinueException : Exception
    {
        public static readonly ContinueException Instance = new ContinueException();

        private ContinueException() { }
    }
}
