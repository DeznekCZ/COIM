namespace PythonAPI.Expressions;

/// Implemented by dictionary-backed context objects that want to control the error
/// text a modder sees when a `.member` read misses — e.g. `config`, which can point
/// at the config.json it actually loaded instead of listing bare key names.
///
/// Consulted by <see cref="PropertyExpression"/> only when the key is genuinely
/// absent; a present key (even one holding null) resolves normally.
public interface IMissingMemberMessage
{
    /// Full message for the KeyNotFoundException raised when <paramref name="name"/>
    /// is read off this object. Should name both the missing member and where the
    /// values came from.
    string DescribeMissingMember(string name);
}
