using System;
using System.Collections.Generic;
using System.IO;
using CustomAssets.Python;
using PythonAPI;
using PythonAPI.Statements;

namespace CustomAssets.Data.Mod;

/// Evaluates the `expression` of a numeric config.json field.
///
/// A numeric field may carry an expression instead of a fixed number, computed from
/// the pack's other config fields:
///
///     "belt_speed":   { "default": 4, "is_integer": true },
///     "belt_speed_t2": { "default": 8, "is_integer": true, "expression": "belt_speed * 2" }
///
/// Expressions run through the pack interpreter itself, so the syntax a modder writes
/// here is the syntax they already know from Definitions/*.py, and the editor's
/// preview cannot disagree with what the game will compute — both call this class.
///
/// Only the pack's own fields are in scope. No game protos, no imports: a config value
/// is read before anything else exists, and keeping the scope tiny is what makes the
/// evaluation safe to run on a file that arrived with a mod.
public static class ConfigExpressions
{
    /// Name the expression's result is bound to internally. Underscore-prefixed so it
    /// cannot collide with a config field, whose names are `^[a-z][a-z0-9_]*$`.
    private const string ResultName = "__config_result__";

    /// Hard stop on how many times the resolver re-sweeps the field list. Each pass
    /// must resolve at least one expression to continue, so this only ever bites on a
    /// pathological file; the real terminator is "a pass that changed nothing".
    private const int MaxPasses = 32;

    /// Evaluate one expression against <paramref name="values"/>.
    /// Returns false with a modder-facing <paramref name="error"/> on a parse failure,
    /// an unknown name, or a non-numeric result.
    public static bool TryEvaluate(string expression, IDictionary<string, object> values,
            out object result, out string error)
    {
        result = null;
        error = null;

        if (string.IsNullOrWhiteSpace(expression))
        {
            error = "expression is empty";
            return false;
        }

        try
        {
            // Wrap as an assignment so we can reuse the ordinary statement pipeline
            // rather than needing a public "parse a bare expression" entry point.
            Token[] tokens = Tokenizer.ParseLines(
                new FileInfo(ConfigLoader.ConfigFileName),
                new[] { ResultName + " = " + expression.Trim() });
            Block block = Lexer.Parse(tokens);

            Dictionary<string, object> context = new Dictionary<string, object>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, object> pair in values)
            {
                context[pair.Key] = pair.Value;
            }

            foreach (IStatement statement in block.statements)
            {
                statement.Execute(context);
            }

            if (!context.TryGetValue(ResultName, out object raw))
            {
                error = "expression produced no value";
                return false;
            }
            if (!(raw is int) && !(raw is float) && !(raw is double) && !(raw is long))
            {
                error = "expression must produce a number, got "
                    + (raw == null ? "None" : raw.GetType().Name);
                return false;
            }
            result = raw;
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    /// Resolve every expression in <paramref name="expressions"/> into
    /// <paramref name="values"/>, in dependency order.
    ///
    /// Expressions may reference fields that are themselves expressions, and config.json
    /// has no declaration order to lean on, so we sweep repeatedly: each pass evaluates
    /// whatever now resolves, and stops when a pass changes nothing. What is left over
    /// is a cycle or a reference to a name that does not exist — reported through
    /// <paramref name="onError"/>, with the field keeping its literal `default`.
    public static void ResolveAll(IDictionary<string, object> values,
            IDictionary<string, string> expressions, Action<string, string> onError)
    {
        if (expressions == null || expressions.Count == 0)
        {
            return;
        }

        Dictionary<string, string> pending = new Dictionary<string, string>(StringComparer.Ordinal);
        Dictionary<string, object> fallbacks = new Dictionary<string, object>(StringComparer.Ordinal);

        foreach (KeyValuePair<string, string> pair in expressions)
        {
            pending[pair.Key] = pair.Value;
            // An UNRESOLVED computed field must not be visible to the others: leaving its
            // literal default in scope would let a cycle (`a = b + 1`, `b = a + 1`)
            // quietly resolve against that default and produce a plausible wrong number
            // instead of being reported. Each one comes back only once it is computed —
            // or at the end, as its fallback.
            if (values.TryGetValue(pair.Key, out object literal))
            {
                fallbacks[pair.Key] = literal;
                values.Remove(pair.Key);
            }
        }

        Dictionary<string, string> lastErrors = new Dictionary<string, string>(StringComparer.Ordinal);

        for (int pass = 0; pass < MaxPasses && pending.Count > 0; pass++)
        {
            List<string> resolved = new List<string>();
            foreach (KeyValuePair<string, string> pair in pending)
            {
                if (TryEvaluate(pair.Value, values, out object result, out string error))
                {
                    values[pair.Key] = result;
                    resolved.Add(pair.Key);
                }
                else
                {
                    lastErrors[pair.Key] = error;
                }
            }

            if (resolved.Count == 0)
            {
                // Nothing moved, so nothing will: what remains depends on a name that
                // does not exist, or on itself.
                break;
            }
            foreach (string key in resolved)
            {
                pending.Remove(key);
                lastErrors.Remove(key);
            }
        }

        foreach (KeyValuePair<string, string> pair in pending)
        {
            if (fallbacks.TryGetValue(pair.Key, out object fallback))
            {
                values[pair.Key] = fallback;
            }
            onError?.Invoke(pair.Key,
                lastErrors.TryGetValue(pair.Key, out string error) ? error : "could not be resolved");
        }
    }
}
