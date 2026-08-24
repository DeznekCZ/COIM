using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using PythonAPI;
using PythonAPI.Arguments;
using PythonAPI.Expressions;

namespace CustomAssets.Editor.Io;

/// Renders a parsed <see cref="IExpression"/> back to source text.
///
/// The editor stores a numeric field as an int, and the emitter writes that int back
/// out. That silently DESTROYS anything the modder wrote that was not a literal:
/// `Quantity(config.media_per_batch)` used to load as 0 and save as `Quantity(0)`.
/// Printing lets the loader keep the original text alongside the number, so a
/// hand-written expression survives a round trip and the editor can offer an
/// expression box for the field.
///
/// Deliberately partial: it returns null the moment it meets a node it cannot render
/// faithfully, and every caller treats null as "no expression, use the number". A
/// wrong rendering would be far worse than none — it would rewrite the modder's file
/// with something that means something else.
public static class ExpressionPrinter
{
    private static readonly Dictionary<Type, string> BinaryOperators = new Dictionary<Type, string> {
        { typeof(AddExpression),          "+"   },
        { typeof(SubExpression),          "-"   },
        { typeof(MulExpression),          "*"   },
        { typeof(DivExpression),          "/"   },
        { typeof(DivIntExpression),       "//"  },
        { typeof(ModExpression),          "%"   },
        { typeof(PowerExpression),        "**"  },
        { typeof(BitAndExpression),       "&"   },
        { typeof(BitOrExpression),        "|"   },
        { typeof(BitXorExpression),       "^"   },
        { typeof(ShiftLeftExpression),    "<<"  },
        { typeof(ShiftRightExpression),   ">>"  },
        { typeof(EqualExpression),        "=="  },
        { typeof(NotEqualExpression),     "!="  },
        { typeof(GreaterExpression),      ">"   },
        { typeof(GreaterEqualExpression), ">="  },
        { typeof(LowerExpression),        "<"   },
        { typeof(LowerEqualExpression),   "<="  },
        { typeof(AndExpression),          "and" },
        { typeof(OrExpression),           "or"  },
        { typeof(InExpression),           "in"  },
        { typeof(IsExpression),           "is"  }
    };

    /// Source text for <paramref name="expression"/>, or null when any part of it
    /// cannot be rendered.
    public static string Print(IExpression expression)
    {
        return print(expression, nested: false);
    }

    /// Text for an expression that is NOT a plain number — the case the editor wants
    /// to preserve. Returns null for a literal (the caller already has it as a number)
    /// and for anything unprintable.
    public static string PrintIfNotLiteral(IExpression expression)
    {
        if (expression is NumberConstant)
        {
            return null;
        }
        return Print(expression);
    }

    private static string print(IExpression expression, bool nested)
    {
        switch (expression)
        {
            case null:
                return null;

            case NumberConstant number:
                return formatNumber(number.Value);

            case StringConstant text:
                // Double quotes to match what PackEmitter writes elsewhere.
                return "\"" + escape(text.Value) + "\"";

            case BooleanConst boolean:
                return boolean.BooleanValue ? "True" : "False";

            case NoneConst _:
                return "None";

            // VariableExpression (`my_var`) and PropertyExpression (`config.amount`,
            // `Ids.Products.IronOre`) both render through Path, which is exactly the
            // dotted source text.
            case VariableExpression variable:
                return variable.Path;

            case PropertyExpression property:
                return safePath(property);

            case NegativeExpression negative:
                return prefixed("-", negative.Operand, nested);

            case PositiveExpression positive:
                return prefixed("+", positive.Operand, nested);

            case NotExpression not:
                return prefixed("not ", not.Operand, nested);

            case CallExpression call:
                return printCall(call);

            case ListExpression list:
                return printList(list);

            // A parenthesised group parses as a one-element tuple, so this is also how
            // `(a + b) * 2` keeps its brackets on the way back out.
            case PyTuple tuple:
                return printTuple(tuple);

            case ABinaryOperatorExpression binary:
                return printBinary(binary, nested);

            default:
                return null;
        }
    }

    private static string printBinary(ABinaryOperatorExpression binary, bool nested)
    {
        if (!BinaryOperators.TryGetValue(binary.GetType(), out string op))
        {
            return null;
        }
        string left = print(binary.Left, nested: true);
        string right = print(binary.Right, nested: true);
        if (left == null || right == null)
        {
            return null;
        }
        string text = left + " " + op + " " + right;
        // Parenthesise whenever this operator sits inside another one. Cheaper — and
        // safer — than modelling precedence: `a * (b + c)` is never wrong, only wordy.
        return nested ? "(" + text + ")" : text;
    }

    private static string printCall(CallExpression call)
    {
        string callee = print(call.Calle, nested: true);
        if (callee == null)
        {
            return null;
        }

        StringBuilder sb = new StringBuilder(callee).Append('(');
        bool first = true;
        foreach (IArgument argument in call.Arguments)
        {
            string value = print(argument.Expression, nested: false);
            if (value == null)
            {
                return null;
            }
            if (!first)
            {
                sb.Append(", ");
            }
            first = false;
            if (argument is NamedArgument named)
            {
                sb.Append(named.Name).Append(" = ");
            }
            sb.Append(value);
        }
        return sb.Append(')').ToString();
    }

    private static string printList(ListExpression list)
    {
        StringBuilder sb = new StringBuilder("[");
        bool first = true;
        foreach (IExpression item in list.Items)
        {
            string value = print(item, nested: false);
            if (value == null)
            {
                return null;
            }
            if (!first)
            {
                sb.Append(", ");
            }
            first = false;
            sb.Append(value);
        }
        // No trailing comma — the dialect's lexer rejects `[1, 2,]`.
        return sb.Append(']').ToString();
    }

    private static string printTuple(PyTuple tuple)
    {
        StringBuilder sb = new StringBuilder("(");
        bool first = true;
        foreach (IExpression item in tuple.ExpressionLists)
        {
            string value = print(item, nested: false);
            if (value == null)
            {
                return null;
            }
            if (!first)
            {
                sb.Append(", ");
            }
            first = false;
            sb.Append(value);
        }
        return sb.Append(')').ToString();
    }

    private static string prefixed(string op, IExpression operand, bool nested)
    {
        string inner = print(operand, nested: true);
        if (inner == null)
        {
            return null;
        }
        string text = op + inner;
        return nested ? "(" + text + ")" : text;
    }

    // PropertyExpression.Path walks its inner expression's Path, which throws for
    // kinds that have none (a call result, an index). Decline instead of propagating.
    private static string safePath(PropertyExpression property)
    {
        try
        {
            return property.Path;
        }
        catch (NotImplementedException)
        {
            return null;
        }
    }

    private static string formatNumber(object value)
    {
        switch (value)
        {
            case int i:    return i.ToString(CultureInfo.InvariantCulture);
            case long l:   return l.ToString(CultureInfo.InvariantCulture);
            case float f:  return f.ToString("R", CultureInfo.InvariantCulture);
            case double d: return d.ToString("R", CultureInfo.InvariantCulture);
            default:       return null;
        }
    }

    private static string escape(string value)
    {
        if (value == null)
        {
            return "";
        }
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n");
    }
}
