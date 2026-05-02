using Mafi;
using System.Collections;
using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    // `for VAR in EXPR: BODY` — evaluates EXPR once, then iterates the
    // resulting IEnumerable.  Per-iteration: assigns the current item to
    // VAR in the running context, runs the body, and intercepts
    // ContinueException (skip to next item) / BreakException (stop the
    // loop).  Other exceptions propagate up so genuine runtime errors
    // still surface to the PLC's __run_error path.
    //
    // Per-tick iteration cap below.  Without it a player who iterates an
    // infinite generator (or recurses on Module.Array.shift_left_with) can
    // hang the simulation, which is unrecoverable for the game tick.  100k
    // is well above any realistic PLC use; if a script needs more it has
    // a real-time issue and should be redesigned.
    internal class ForStatement : IStatement
    {
        private const int MAX_ITERATIONS = 100_000;

        private readonly string variable;
        private readonly IExpression iterable;
        private readonly Block body;

        public ForStatement(string variable, IExpression iterable, Block body)
        {
            this.variable = variable;
            this.iterable = iterable;
            this.body = body;
        }

        public void Execute(IDictionary<string, object> context)
        {
            object source = iterable.GetValue(context);
            if (source is null)
            {
                return;
            }

            IEnumerable enumerable = source as IEnumerable;
            if (enumerable is null)
            {
                throw new PythonRuntimeException(
                    $"`for {variable} in EXPR`: EXPR is not iterable (got {source.GetType().Name})");
            }

            int count = 0;
            foreach (object item in enumerable)
            {
                if (count++ >= MAX_ITERATIONS)
                {
                    throw new PythonRuntimeException(
                        $"`for {variable}`: exceeded {MAX_ITERATIONS} iterations in a single tick");
                }
                context[variable] = item;
                try
                {
                    foreach (IStatement statement in body.statements)
                    {
                        statement.Execute(context);
                    }
                }
                catch (ContinueException)
                {
                    continue;
                }
                catch (BreakException)
                {
                    return;
                }
            }
        }
    }
}
