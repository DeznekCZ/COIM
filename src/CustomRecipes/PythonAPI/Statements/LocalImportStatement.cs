using PythonAPI.Expressions;
using PythonAPI.Runtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PythonAPI.Statements
{
    /// <summary>
    /// Top-level `import &lt;name&gt;` statement used by Definitions/__init__.py to declare
    /// load order. Treated as syntactic sugar for `dependencies("name")` — the framework's
    /// `dependencies` context entry recursively loads the named sibling .py file.
    ///
    /// Distinct from <see cref="ImportStatement"/> (the `from X import a, b` form) which
    /// imports symbols from Mafi / CustomAssets into the current context.
    /// </summary>
    public class LocalImportStatement : IStatement
    {
        public readonly string moduleName;

        public LocalImportStatement(string moduleName)
        {
            this.moduleName = moduleName;
        }

        public void Execute(IDictionary<string, object> context)
        {
            if (!context.TryGetValue("dependencies", out object dep) || !(dep is Constructor depCtor))
            {
                throw new InvalidOperationException(
                    "import statement: 'dependencies' helper not in context. " +
                    "This statement is only meaningful inside a Definitions/*.py file " +
                    "loaded by the CustomAssets framework.");
            }
            depCtor.Invoke(new[] { new NamedValue("dependencies", moduleName) });
        }

        public Task ExecuteAsync(IDictionary<string, object> context)
        {
            Execute(context);
            return Task.CompletedTask;
        }
    }
}
