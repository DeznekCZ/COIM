using System;
using System.Collections.Generic;
using System.Linq;

namespace PythonAPI.Expressions;

/// Builds the message for a lookup that misses on a dictionary-backed value, so
/// `config.nope` (PropertyExpression) and `config["nope"]` (Expressions.__getitem__)
/// report the same thing. Owners implementing <see cref="IMissingMemberMessage"/>
/// supply their own text; everything else gets the keys that DO exist, which is
/// what a modder needs to spot a typo or a stale name.
public static class MissingMember
{
    private const int MaxListed = 12;

    /// <param name="owner">The value being indexed — may customise the message.</param>
    /// <param name="members">The entries that exist.</param>
    /// <param name="name">The key that was not found.</param>
    /// <param name="displayPath">How to name the failed lookup in the generic
    /// message (e.g. "config.nope"). Defaults to <paramref name="name"/>.</param>
    public static string Describe(
            object owner,
            IDictionary<string, object> members,
            string name,
            string displayPath = null)
    {
        if (owner is IMissingMemberMessage describer)
        {
            return describer.DescribeMissingMember(name);
        }

        string what = displayPath ?? name;
        if (members == null || members.Count == 0)
        {
            return $"\"{what}\" is not defined (the object is empty)";
        }

        string available = string.Join(", ", members.Keys.OrderBy(k => k, StringComparer.Ordinal).Take(MaxListed));
        if (members.Count > MaxListed)
        {
            available += ", …";
        }
        return $"\"{what}\" is not defined (available: {available})";
    }
}
