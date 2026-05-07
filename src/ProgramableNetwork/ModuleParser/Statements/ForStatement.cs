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

            // Fast path for the lazy range iterable produced by PlcPy's
            // range(...) — drives the loop straight off the int triple
            // (Start / Stop / Step) so we skip both the IEnumerator state
            // machine allocation and the int→object boxing the generic
            // foreach below would do for every iteration.  The body
            // dispatch and break/continue handling are identical to the
            // generic path; only the iteration source differs.
            if (source is RangeIterable r)
            {
                ExecuteRange(r, context);
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

        // Allocation-free RangeIterable fast path.  Boxes each int once
        // (when storing into context[variable]), but skips the per-call
        // IEnumerator + the Length-of-N backing array the old List<int>
        // path required.  For range(10000) that drops the per-tick
        // allocation from 40 KB + iterator-state to just 10000 boxed
        // ints (and the Dict reuses its bucket so allocation cost is
        // O(items) not O(items × headers)).
        private void ExecuteRange(RangeIterable r, IDictionary<string, object> context)
        {
            int start = r.Start;
            int stop = r.Stop;
            int step = r.Step;
            int count = 0;

            if (step > 0)
            {
                for (int i = start; i < stop; i += step)
                {
                    if (count++ >= MAX_ITERATIONS)
                    {
                        throw new PythonRuntimeException(
                            $"`for {variable}`: exceeded {MAX_ITERATIONS} iterations in a single tick");
                    }
                    context[variable] = i;
                    try
                    {
                        foreach (IStatement statement in body.statements)
                        {
                            statement.Execute(context);
                        }
                    }
                    catch (ContinueException) { continue; }
                    catch (BreakException) { return; }
                }
            }
            else
            {
                for (int i = start; i > stop; i += step)
                {
                    if (count++ >= MAX_ITERATIONS)
                    {
                        throw new PythonRuntimeException(
                            $"`for {variable}`: exceeded {MAX_ITERATIONS} iterations in a single tick");
                    }
                    context[variable] = i;
                    try
                    {
                        foreach (IStatement statement in body.statements)
                        {
                            statement.Execute(context);
                        }
                    }
                    catch (ContinueException) { continue; }
                    catch (BreakException) { return; }
                }
            }
        }
    }
}
