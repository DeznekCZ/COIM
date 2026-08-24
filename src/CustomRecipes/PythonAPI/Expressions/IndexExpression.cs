using System.Collections.Generic;
using System.Threading.Tasks;
using PythonAPI.Range;
using PythonAPI.Runtime;

namespace PythonAPI.Expressions;

/// Subscript: `target[index]`. The lexer only ever builds this with a
/// <see cref="SingleItem"/> right-hand side — slices (`x[a:b]`) are rejected while
/// parsing (see Lexer.range) — so the index is a single evaluated key: a string for
/// dictionaries such as `config`, an int for lists, tuples and strings.
public class IndexExpression : ABinaryOperatorExpression
{
    public IndexExpression(IExpression left, IRange right) : base(left, right)
    {
    }

    /// Writable slot, so `target[index] = value` assigns. Both sides are evaluated
    /// once here — the getter and setter then act on the same target object, which
    /// keeps `x[k] = x[k] + 1` from evaluating the target expression twice.
    public override Reference<object> GetReference(IDictionary<string, object> context)
    {
        object target = left.GetValue(context);
        object index = right.GetValue(context);
        return reference(target, index);
    }

    public override async Task<Reference<object>> GetReferenceAsync(IDictionary<string, object> context)
    {
        object target = await left.GetValueAsync(context);
        object index = await right.GetValueAsync(context);
        return reference(target, index);
    }

    protected override object Evaluate(object left, object right)
    {
        if (right is Runtime.Range range)
        {
            return Expressions.__range__(left, range);
        }
        return Expressions.__index__(left, right);
    }

    private static Reference<object> reference(object target, object index)
    {
        return new Reference<object>(
            (v) => Expressions.__setitem__(target, index, v),
            () => Expressions.__getitem__(target, index));
    }
}
