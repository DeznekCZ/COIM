using PythonAPI.Expressions;
using PythonAPI.Runtime;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PythonAPI.Statements
{
    /// <summary>
    /// `with &lt;expr&gt; [as &lt;name&gt;]:` block. Runs the body with an optional
    /// context manager active. Resolution order:
    ///   1. If the expression is a call to a <see cref="ContextConstructor"/>
    ///      (e.g. <c>build_recipe(...)</c>): invoke it for the result, call
    ///      <c>Enter(result)</c>, bind the result to the <c>as</c> name, run the
    ///      body, then <c>Exit(result)</c> in a finally.
    ///   2. Else evaluate the value; if it is an <see cref="IContextManager"/>,
    ///      use <c>__enter__()</c> / <c>__exit__()</c>.
    ///   3. Else just bind the value to the <c>as</c> name and run the body.
    ///
    /// Public members mirror <see cref="IfStatement"/> so PackLoader can walk the
    /// body (bind_recipe calls nested under a recipe's `with`).
    /// </summary>
    public class WithStatement : IStatement
    {
        public IExpression ContextExpr { get; }
        public string AsName { get; }
        public Block Block { get; }

        public int StartLine;
        public int EndLine;

        public WithStatement(IExpression contextExpr, string asName, Block block)
        {
            ContextExpr = contextExpr;
            AsName = asName;
            Block = block;
        }

        public void Execute(IDictionary<string, object> context)
        {
            ContextConstructor ctxCtor =
                ContextExpr is CallExpression ce && ce.Calle.GetValue(context) is ContextConstructor cc
                    ? cc : null;

            object value = ContextExpr.GetValue(context);
            object bound = value;
            IContextManager cm = ctxCtor == null ? value as IContextManager : null;

            if (ctxCtor != null) {
                ctxCtor.Enter(value, context);
            } else if (cm != null) {
                bound = cm.__enter__();
            }

            if (!string.IsNullOrEmpty(AsName)) {
                context[AsName] = bound;
            }

            try {
                foreach (IStatement item in Block.statements) {
                    item.Execute(context);
                }
            } finally {
                if (ctxCtor != null) {
                    ctxCtor.Exit(value, context);
                } else if (cm != null) {
                    cm.__exit__();
                }
            }
        }

        public async Task ExecuteAsync(IDictionary<string, object> context)
        {
            ContextConstructor ctxCtor =
                ContextExpr is CallExpression ce && (await ce.Calle.GetValueAsync(context)) is ContextConstructor cc
                    ? cc : null;

            object value = await ContextExpr.GetValueAsync(context);
            object bound = value;
            IContextManager cm = ctxCtor == null ? value as IContextManager : null;

            if (ctxCtor != null) {
                ctxCtor.Enter(value, context);
            } else if (cm != null) {
                bound = cm.__enter__();
            }

            if (!string.IsNullOrEmpty(AsName)) {
                context[AsName] = bound;
            }

            try {
                foreach (IStatement item in Block.statements) {
                    await item.ExecuteAsync(context);
                }
            } finally {
                if (ctxCtor != null) {
                    ctxCtor.Exit(value, context);
                } else if (cm != null) {
                    cm.__exit__();
                }
            }
        }
    }
}
