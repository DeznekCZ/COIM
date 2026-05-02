using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    // Lexer emits one BreakStatement per `break` token.  Execution always
    // throws BreakException — the nearest enclosing for/while catches it.
    // If `break` appears outside a loop the exception escapes the script
    // and surfaces as a runtime error in the PLC's __run_error field, which
    // is the right behavior (Python raises SyntaxError; we get a noisier
    // but still-clear failure message).
    internal class BreakStatement : IStatement
    {
        public void Execute(IDictionary<string, object> context)
        {
            throw BreakException.Instance;
        }
    }
}
