using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    // Companion to BreakStatement — throws ContinueException, caught per-
    // iteration by the enclosing loop.  See BreakStatement for the
    // outside-of-a-loop behavior.
    internal class ContinueStatement : IStatement
    {
        public void Execute(IDictionary<string, object> context)
        {
            throw ContinueException.Instance;
        }
    }
}
