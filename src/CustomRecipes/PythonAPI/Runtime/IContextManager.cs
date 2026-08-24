using System.Threading.Tasks;

namespace PythonAPI.Runtime
{
    /// <summary>
    /// Minimal context-manager protocol for the interpreter's `with` statement.
    /// A value returned by the `with` expression that implements this interface
    /// participates in `with EXPR as x:` — <see cref="__enter__"/> runs on block
    /// entry (its return value is bound to <c>x</c>) and <see cref="__exit__"/>
    /// runs on block exit (including when the body throws).
    ///
    /// This is the general escape hatch. Built-in functions that want context
    /// behavior without returning a wrapper (e.g. <c>build_recipe</c>, whose bare
    /// <c>RecipeProto</c> return must stay usable everywhere) instead register as
    /// a <see cref="ContextConstructor"/>, which <see cref="Statements.WithStatement"/>
    /// checks first.
    /// </summary>
    public interface IContextManager
    {
        object __enter__();
        void __exit__();
    }

    /// <summary>Optional async variant; <see cref="Statements.WithStatement"/>
    /// uses it in async execution when present, else falls back to the sync form.</summary>
    public interface IAsyncContextManager
    {
        Task<object> __enter__async__();
        Task __exit__async__();
    }
}
