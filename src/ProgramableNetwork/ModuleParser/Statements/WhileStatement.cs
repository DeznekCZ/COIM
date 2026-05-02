using Mafi;
using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
    // `while EXPR: BODY` — re-evaluates EXPR before every iteration, stops
    // when the value is null / False / Fix32(0).  Same per-tick iteration
    // cap as ForStatement to keep an infinite condition from hanging the
    // simulation (a `while True:` loop with no break would otherwise lock
    // the game thread).  Falsiness rules mirror IfStatement so `while x`
    // behaves consistently across control-flow constructs.
    internal class WhileStatement : IStatement
    {
        private const int MAX_ITERATIONS = 100_000;

        private readonly IExpression condition;
        private readonly Block body;

        public WhileStatement(IExpression condition, Block body)
        {
            this.condition = condition;
            this.body = body;
        }

        public void Execute(IDictionary<string, object> context)
        {
            int count = 0;
            while (IsTruthy(condition.GetValue(context)))
            {
                if (count++ >= MAX_ITERATIONS)
                {
                    throw new PythonRuntimeException(
                        $"`while`: exceeded {MAX_ITERATIONS} iterations in a single tick");
                }
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

        // Same rules as IfStatement.Executed — null is false, bool is
        // itself, anything else gets coerced to Fix32 and treated as
        // false when ≤ 0.  Centralised here so loop and if behavior match.
        private static bool IsTruthy(object value)
        {
            if (value is null)
            {
                return false;
            }
            if (value is bool b)
            {
                return b;
            }
            return Expressions.__fix__(value) > Fix32.Zero;
        }
    }
}
