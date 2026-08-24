using System;
using System.Collections.Generic;

namespace PythonAPI.Runtime
{
    /// <summary>
    /// A <see cref="Constructor"/> that also acts as a `with`-statement context
    /// manager. Registered for builtins like <c>build_recipe</c> that must keep
    /// returning a bare value (so <c>r = build_recipe(...)</c> and every existing
    /// consumer are unchanged) yet want to push/pop an ambient context when used
    /// as <c>with build_recipe(...) as r:</c>.
    ///
    /// <see cref="Statements.WithStatement"/> detects a `with` whose expression is
    /// a call to a <see cref="ContextConstructor"/>, invokes the call to get the
    /// result, then calls <see cref="Enter"/> before the body and <see cref="Exit"/>
    /// after it (in a finally). The invoked value is what gets bound to the `as`
    /// name — <see cref="Enter"/>/<see cref="Exit"/> are pure side effects (e.g.
    /// pushing the returned recipe onto the bind-context).
    /// </summary>
    public sealed class ContextConstructor : Constructor
    {
        private readonly Action<object, IDictionary<string, object>> m_enter;
        private readonly Action<object, IDictionary<string, object>> m_exit;

        public ContextConstructor(
                string[] argumentNames,
                FunctionCall value,
                Action<object, IDictionary<string, object>> enter,
                Action<object, IDictionary<string, object>> exit)
            : base(argumentNames, value)
        {
            m_enter = enter;
            m_exit = exit;
        }

        public void Enter(object result, IDictionary<string, object> context) => m_enter?.Invoke(result, context);

        public void Exit(object result, IDictionary<string, object> context) => m_exit?.Invoke(result, context);
    }
}
