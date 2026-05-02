using System;

namespace ProgramableNetwork.Python
{
    // Thrown by `break` to unwind through nested if/elif/else and inner
    // expression evaluation up to the closest enclosing for/while, where
    // ForStatement / WhileStatement catch it and stop iterating.  Modelled
    // on ReturnException — same "use exceptions for non-local control flow"
    // pattern the parser already uses for `return`.
    [Serializable]
    public class BreakException : Exception
    {
        public static readonly BreakException Instance = new BreakException();

        private BreakException() { }
    }
}
