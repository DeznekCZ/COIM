using System.Collections.Generic;
using System.Linq;
using PythonAPI.Expressions;

namespace CustomAssets.Data.Mod;

/// The `config` object a pack's .py files see: one entry per config.json field,
/// holding that field's effective value (currently the schema `default`).
///
/// It is a plain string→object dictionary because that is what
/// <see cref="PropertyExpression"/> resolves `config.my_field` through. The two
/// extra members exist for the gating case, where a field may legitimately be
/// absent (an older config.json shipped with the pack, a field added later):
///   • <c>config.get("name", fallback)</c> — value or fallback, never throws.
///   • <c>config.has("name")</c>          — presence test.
/// A real config field named `get`/`has` still wins over the helper: the
/// dictionary is consulted first and only a MISSING key falls through to methods.
internal sealed class ConfigValues : Dictionary<string, object>, IDictionary<string, object>, IMissingMemberMessage
{
    private const int MaxListedFields = 12;

    private readonly string m_configPath;
    private readonly bool m_configFileExists;

    public ConfigValues(string configPath, bool configFileExists)
    {
        m_configPath = configPath;
        m_configFileExists = configFileExists;
    }

    /// Config fields are CONSTANTS as far as pack code is concerned: `config.x = 1` or
    /// `config["x"] = 1` from a .py file is refused, because a definition file that
    /// rewrites the player's settings mid-load would make the settings panel lie about
    /// what the game is running.
    ///
    /// Re-implemented from the base Dictionary (which reports false) so the interpreter
    /// can detect it generically — see PropertyExpression and Expressions.__setitem__,
    /// which refuse to write to ANY read-only dictionary. The loader itself populates
    /// through the concrete Dictionary API and is unaffected.
    bool ICollection<KeyValuePair<string, object>>.IsReadOnly => true;

    // Lowercase on purpose: these are called from .py as `config.get(...)` /
    // `config.has(...)`, so they carry Python naming rather than C# naming.

    /// Value of <paramref name="name"/>, or <paramref name="fallback"/> when the
    /// field is not in config.json. The single optional parameter serves both
    /// `config.get("x")` and `config.get("x", 5)` — see Expressions.__invoke_member__.
    public object get(string name, object fallback = null)
    {
        return TryGetValue(name, out object value) ? value : fallback;
    }

    /// True when config.json defines <paramref name="name"/>. Use before reading a
    /// field that older versions of the pack's config.json may not have.
    public bool has(string name)
    {
        return ContainsKey(name);
    }

    /// Names the file the values came from, so a typo'd or stale field reports
    /// something actionable instead of a bare "key not present".
    public string DescribeMissingMember(string name)
    {
        if (!m_configFileExists)
        {
            return $"config has no field \"{name}\" — this pack has no config.json "
                + $"(expected at {m_configPath}). Use config.get(\"{name}\", <fallback>) "
                + "to read a field that may be absent.";
        }

        if (Count == 0)
        {
            return $"config has no field \"{name}\" — {m_configPath} defines no fields.";
        }

        string defined = string.Join(", ", Keys.OrderBy(k => k, System.StringComparer.Ordinal).Take(MaxListedFields));
        if (Count > MaxListedFields)
        {
            defined += ", …";
        }
        return $"config has no field \"{name}\" — add it to {m_configPath} "
            + $"or use config.get(\"{name}\", <fallback>). Defined: {defined}.";
    }
}
