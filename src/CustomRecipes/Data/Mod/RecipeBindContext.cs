using System.Collections.Generic;
using Mafi.Core.Factory.Recipes;

namespace CustomAssets.Data.Mod;

/// <summary>
/// Ambient "current recipe" stack backing the <c>with build_recipe(...) as r:</c>
/// authoring form. <c>build_recipe</c>'s <see cref="PythonAPI.Runtime.ContextConstructor"/>
/// pushes the created <see cref="RecipeProto"/> on block entry and pops it on exit;
/// <c>bind_recipe</c> reads <see cref="Current"/> when it is called without an
/// explicit recipe (the context form <c>bind_recipe("Machine", ...)</c>).
///
/// A stack (not a single value) supports nested <c>with</c> blocks. Thread-static
/// so concurrent pack loads don't cross-contaminate.
/// </summary>
internal static class RecipeBindContext
{
    [System.ThreadStatic]
    private static Stack<RecipeProto> s_stack;

    private static Stack<RecipeProto> Stack => s_stack ??= new Stack<RecipeProto>();

    /// Push on `with` entry. Pushes even a null so entry/exit stay balanced.
    public static void Push(RecipeProto recipe) => Stack.Push(recipe);

    /// Pop on `with` exit.
    public static void Pop() {
        if (Stack.Count > 0) Stack.Pop();
    }

    /// The innermost active recipe, or null when not inside a `with build_recipe`.
    public static RecipeProto Current => Stack.Count > 0 ? Stack.Peek() : null;
}
