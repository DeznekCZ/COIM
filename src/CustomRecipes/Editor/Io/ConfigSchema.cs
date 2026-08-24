using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using CustomAssets.Data.Mod;

namespace CustomAssets.Editor.Io;

/// The four value kinds config.schema.json allows. `is_integer` distinguishes
/// <see cref="Int"/> from <see cref="Float"/> on disk; bool and string fields
/// carry no such marker.
public enum ConfigFieldKind
{
    Bool,
    Int,
    Float,
    String
}

/// When a field may be changed. Content-gating fields (`if config.enable_x:`) decide
/// which recipes and products get REGISTERED, so flipping one on a save that already
/// contains those protos is how a config change breaks a world — mark those
/// <see cref="OnAdd"/> and the settings UI locks them once the pack is in a game.
public enum ConfigFieldEditability
{
    /// Safe to change at any time — a rate, a name, a multiplier. The default when
    /// config.json says nothing.
    Always,

    /// Only meaningful when the mod is first added to a game. Locked afterwards
    /// (with an explicit override) because changing it can break existing saves.
    OnAdd
}

/// One editable field of a pack's config.json — the schema entry (name, kind,
/// constraints) together with its current value.
///
/// The runtime only ever reads `default` (see Data/Mod/ConfigLoader), so the value
/// a player edits and the schema default are the same slot; the rest is metadata
/// for the editor and for IntelliSense.
public sealed class ConfigField
{
    /// JSON keys this class models directly. Anything else found in a field object
    /// is preserved verbatim through <see cref="Extras"/> so a hand-authored file
    /// does not lose content by being saved from the UI.
    private static readonly HashSet<string> KnownKeys = new HashSet<string>(StringComparer.Ordinal) {
        "default", "is_integer", "description", "min", "max", "max_length", "regex", "editable",
        "expression"
    };

    /// JSON spelling of <see cref="ConfigFieldEditability.OnAdd"/>.
    public const string EditableOnAdd = "on_add";

    /// JSON spelling of <see cref="ConfigFieldEditability.Always"/> — also the
    /// meaning of an absent `editable` key.
    public const string EditableAlways = "always";

    private static readonly Regex NamePattern = new Regex("^[a-z][a-z0-9_]*$", RegexOptions.Compiled);

    /// Unmodelled keys of this field's JSON object, written back untouched.
    public readonly Dictionary<string, object> Extras = new Dictionary<string, object>(StringComparer.Ordinal);

    public string Name;
    public ConfigFieldKind Kind;

    /// Current value: bool / long / double / string, matching <see cref="Kind"/>.
    public object Value;

    public string Description;
    public double? Min;
    public double? Max;
    public int? MaxLength;
    public string Regex;

    /// Whether a player may change this field on a game that already has the pack.
    public ConfigFieldEditability Editable = ConfigFieldEditability.Always;

    /// Numeric fields only: an expression over the pack's OTHER config fields, computed
    /// at load time (see Data/Mod/ConfigExpressions). When set it wins over
    /// <see cref="Value"/>, which stays in the file as the fallback used if the
    /// expression fails to resolve. Null or empty = a plain literal field.
    public string Expression;

    /// True when this field's value is computed rather than typed.
    public bool HasExpression => !string.IsNullOrWhiteSpace(Expression);

    /// Only numbers can be computed — a bool gate or a display name has nothing
    /// meaningful to compute from, and the evaluator only returns numbers.
    public bool SupportsExpression => Kind == ConfigFieldKind.Int || Kind == ConfigFieldKind.Float;

    public ConfigField(string name, ConfigFieldKind kind, object value)
    {
        Name = name;
        Kind = kind;
        Value = value;
    }

    /// A field with the sensible empty value for its kind — what "+ add field" and
    /// a kind switch produce.
    public static ConfigField NewOfKind(string name, ConfigFieldKind kind)
    {
        return new ConfigField(name, kind, DefaultValueFor(kind));
    }

    public static object DefaultValueFor(ConfigFieldKind kind)
    {
        switch (kind)
        {
            case ConfigFieldKind.Bool:   return false;
            case ConfigFieldKind.Int:    return 0L;
            case ConfigFieldKind.Float:  return 0d;
            default:                     return "";
        }
    }

    /// Parse one `"name": { ... }` entry. Returns null when the object carries no
    /// `default` — the one key the runtime requires; such an entry is left in the
    /// file untouched rather than shown as an editable field it cannot represent.
    public static ConfigField FromJson(string name, Dictionary<string, object> obj)
    {
        if (obj == null || !obj.TryGetValue("default", out object raw))
        {
            return null;
        }

        bool? isInteger = obj.TryGetValue("is_integer", out object rawIsInt) && rawIsInt is bool b
            ? b
            : (bool?)null;

        ConfigFieldKind kind;
        object value;
        if (raw is bool boolValue)
        {
            kind = ConfigFieldKind.Bool;
            value = boolValue;
        }
        else if (raw is string stringValue)
        {
            kind = ConfigFieldKind.String;
            value = stringValue;
        }
        else if (raw is long longValue)
        {
            // No is_integer marker: an integral literal reads as an int, matching
            // how ConfigLoader hands the value to Python.
            kind = isInteger == false ? ConfigFieldKind.Float : ConfigFieldKind.Int;
            value = kind == ConfigFieldKind.Float ? (object)(double)longValue : longValue;
        }
        else if (raw is double doubleValue)
        {
            kind = isInteger == true ? ConfigFieldKind.Int : ConfigFieldKind.Float;
            value = kind == ConfigFieldKind.Int ? (object)(long)doubleValue : doubleValue;
        }
        else
        {
            return null;
        }

        ConfigField field = new ConfigField(name, kind, value) {
            Description = obj.TryGetValue("description", out object d) ? d as string : null,
            Min         = readNumber(obj, "min"),
            Max         = readNumber(obj, "max"),
            Regex       = obj.TryGetValue("regex", out object r) ? r as string : null,
            // Anything other than the explicit "on_add" reads as always-editable,
            // including a missing key and a value we don't recognise: the permissive
            // reading is what every config.json written before this key existed meant.
            Editable    = obj.TryGetValue("editable", out object e)
                          && string.Equals(e as string, EditableOnAdd, StringComparison.OrdinalIgnoreCase)
                ? ConfigFieldEditability.OnAdd
                : ConfigFieldEditability.Always,
            Expression  = obj.TryGetValue("expression", out object x) ? x as string : null
        };
        if (!field.SupportsExpression)
        {
            // An expression on a bool or a string is meaningless to the runtime; keep it
            // in Extras so the file does not lose it, but do not present it as one.
            if (!string.IsNullOrEmpty(field.Expression))
            {
                field.Extras["expression"] = field.Expression;
                field.Expression = null;
            }
        }
        double? maxLength = readNumber(obj, "max_length");
        if (maxLength.HasValue)
        {
            field.MaxLength = (int)maxLength.Value;
        }

        foreach (KeyValuePair<string, object> pair in obj)
        {
            if (!KnownKeys.Contains(pair.Key))
            {
                field.Extras[pair.Key] = pair.Value;
            }
        }
        return field;
    }

    /// Serialise back to the schema shape. Key order matches the documented layout
    /// (default → is_integer → constraints → description) so a saved file reads the
    /// same way a hand-written one does.
    public Dictionary<string, object> ToJson()
    {
        Dictionary<string, object> obj = new Dictionary<string, object>(StringComparer.Ordinal) {
            ["default"] = Value
        };

        if (Kind == ConfigFieldKind.Int || Kind == ConfigFieldKind.Float)
        {
            // Required by config.schema.json for integers, and the only thing that
            // tells ConfigLoader to hand Python a float rather than an int.
            obj["is_integer"] = Kind == ConfigFieldKind.Int;
            if (HasExpression)
            {
                // `default` above stays as the fallback the loader uses if this fails.
                obj["expression"] = Expression.Trim();
            }
            if (Min.HasValue)
            {
                obj["min"] = numberFor(Min.Value);
            }
            if (Max.HasValue)
            {
                obj["max"] = numberFor(Max.Value);
            }
        }

        if (Kind == ConfigFieldKind.String)
        {
            if (MaxLength.HasValue)
            {
                obj["max_length"] = (long)MaxLength.Value;
            }
            if (!string.IsNullOrEmpty(Regex))
            {
                obj["regex"] = Regex;
            }
        }

        if (Editable == ConfigFieldEditability.OnAdd)
        {
            // Only written when restrictive: an absent key already means "always",
            // so emitting it everywhere would churn every existing config.json.
            obj["editable"] = EditableOnAdd;
        }

        if (!string.IsNullOrEmpty(Description))
        {
            obj["description"] = Description;
        }

        foreach (KeyValuePair<string, object> pair in Extras)
        {
            obj[pair.Key] = pair.Value;
        }
        return obj;
    }

    /// Re-type the field, converting the current value where that is meaningful
    /// (int ↔ float, anything → its text form) instead of silently resetting it.
    public void ChangeKind(ConfigFieldKind kind)
    {
        if (kind == Kind)
        {
            return;
        }

        string text = ValueText();
        Kind = kind;
        if (!TryParseValue(text, out object converted, out string _))
        {
            converted = DefaultValueFor(kind);
        }
        Value = converted;

        // Constraints are kind-specific; carrying min/max onto a string field (or a
        // regex onto a number) would write JSON the schema rejects.
        if (kind != ConfigFieldKind.Int && kind != ConfigFieldKind.Float)
        {
            Min = null;
            Max = null;
        }
        if (kind != ConfigFieldKind.String)
        {
            MaxLength = null;
            Regex = null;
        }
        if (!SupportsExpression)
        {
            Expression = null;
        }
    }

    /// The value as the modder types it. Bools are handled by a toggle, not text,
    /// but still render here for the kind-conversion path above.
    public string ValueText()
    {
        switch (Value)
        {
            case null:     return "";
            case bool b:   return b ? "true" : "false";
            case long l:   return l.ToString(CultureInfo.InvariantCulture);
            case double d: return d.ToString("R", CultureInfo.InvariantCulture);
            default:       return Value.ToString();
        }
    }

    /// Parse text entered for this field's kind. `error` is a modder-facing sentence
    /// on failure; both out values are meaningful only on the return value.
    public bool TryParseValue(string text, out object value, out string error)
    {
        error = null;
        value = null;
        string trimmed = (text ?? "").Trim();

        switch (Kind)
        {
            case ConfigFieldKind.Bool:
                if (bool.TryParse(trimmed, out bool b))
                {
                    value = b;
                    return true;
                }
                error = "must be true or false";
                return false;

            case ConfigFieldKind.Int:
                if (!long.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out long l))
                {
                    error = "must be a whole number";
                    return false;
                }
                value = l;
                return checkRange(l, out error);

            case ConfigFieldKind.Float:
                if (!double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
                {
                    error = "must be a number";
                    return false;
                }
                value = d;
                return checkRange(d, out error);

            default:
                value = text ?? "";
                return checkText((string)value, out error);
        }
    }

    /// Problem with the field's CURRENT value, or null when it is fine. Used to gate
    /// Save so an out-of-range value never reaches config.json.
    public string ValidateValue()
    {
        switch (Kind)
        {
            case ConfigFieldKind.Int:
                return checkRange(Convert.ToDouble(Value ?? 0L, CultureInfo.InvariantCulture), out string intError)
                    ? null : intError;
            case ConfigFieldKind.Float:
                return checkRange(Convert.ToDouble(Value ?? 0d, CultureInfo.InvariantCulture), out string floatError)
                    ? null : floatError;
            case ConfigFieldKind.String:
                return checkText(Value as string ?? "", out string textError) ? null : textError;
            default:
                return null;
        }
    }

    /// Problem with the field's NAME, or null. The pattern is config.schema.json's
    /// `propertyNames`; it is also what makes the name usable as `config.<name>`.
    public string ValidateName()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            return "field name is empty";
        }
        if (!NamePattern.IsMatch(Name))
        {
            return "'" + Name + "' must be snake_case: a lowercase letter, then letters, digits or _";
        }
        return null;
    }

    /// Human-readable range/format hint shown under the value editor.
    public string ConstraintHint()
    {
        switch (Kind)
        {
            case ConfigFieldKind.Int:
            case ConfigFieldKind.Float:
                if (Min.HasValue && Max.HasValue)
                {
                    return formatNumber(Min.Value) + " … " + formatNumber(Max.Value);
                }
                if (Min.HasValue)
                {
                    return "min " + formatNumber(Min.Value);
                }
                if (Max.HasValue)
                {
                    return "max " + formatNumber(Max.Value);
                }
                return "";
            case ConfigFieldKind.String:
                List<string> parts = new List<string>();
                if (MaxLength.HasValue)
                {
                    parts.Add("max " + MaxLength.Value + " chars");
                }
                if (!string.IsNullOrEmpty(Regex))
                {
                    parts.Add("matching " + Regex);
                }
                return string.Join(", ", parts);
            default:
                return "";
        }
    }

    public static string FormatNumberForUi(double value)
    {
        return formatNumber(value);
    }

    private bool checkRange(double value, out string error)
    {
        error = null;
        if (Min.HasValue && value < Min.Value)
        {
            error = "must be at least " + formatNumber(Min.Value);
            return false;
        }
        if (Max.HasValue && value > Max.Value)
        {
            error = "must be at most " + formatNumber(Max.Value);
            return false;
        }
        return true;
    }

    private bool checkText(string value, out string error)
    {
        error = null;
        if (MaxLength.HasValue && value.Length > MaxLength.Value)
        {
            error = "must be at most " + MaxLength.Value + " characters";
            return false;
        }
        if (!string.IsNullOrEmpty(Regex))
        {
            try
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(value, Regex))
                {
                    error = "must match " + Regex;
                    return false;
                }
            }
            catch (ArgumentException ex)
            {
                // A malformed pattern is the schema's problem, not the value's —
                // say so rather than blaming what the modder just typed.
                error = "regex '" + Regex + "' is not valid: " + ex.Message;
                return false;
            }
        }
        return true;
    }

    private static double? readNumber(Dictionary<string, object> obj, string key)
    {
        if (!obj.TryGetValue(key, out object raw))
        {
            return null;
        }
        if (raw is long l)
        {
            return l;
        }
        if (raw is double d)
        {
            return d;
        }
        return null;
    }

    // Whole values go back as integers so a min of 0 doesn't become "0.0".
    private static object numberFor(double value)
    {
        return value == Math.Floor(value) && Math.Abs(value) < long.MaxValue
            ? (object)(long)value
            : value;
    }

    private static string formatNumber(double value)
    {
        return value == Math.Floor(value) && Math.Abs(value) < long.MaxValue
            ? ((long)value).ToString(CultureInfo.InvariantCulture)
            : value.ToString("R", CultureInfo.InvariantCulture);
    }
}

/// A pack's config.json as an editable ordered field list.
///
/// Round-trips through <see cref="MiniJson"/> / <see cref="MiniJsonWriter"/>: the
/// `$schema` line and any entry this editor cannot model are preserved, so saving
/// from the UI never costs a hand-authored file its content.
public sealed class ConfigSchema
{
    public const string FileName = "config.json";

    /// Editable fields, in file order.
    public readonly List<ConfigField> Fields = new List<ConfigField>();

    /// Top-level entries that are not fields — `$schema` and friends. Written
    /// before the fields so the file keeps its usual shape.
    public readonly Dictionary<string, object> Preamble = new Dictionary<string, object>(StringComparer.Ordinal);

    /// Absolute path of the pack's config.json — whether or not it exists yet.
    public readonly string Path;

    /// False when the pack ships no config.json. Saving creates one.
    public readonly bool Existed;

    /// Set when the file exists but could not be read or parsed. Non-null means the
    /// caller must NOT save over it — doing so would discard content we failed to
    /// understand.
    public readonly string LoadError;

    private ConfigSchema(string path, bool existed, string loadError)
    {
        Path = path;
        Existed = existed;
        LoadError = loadError;
    }

    public static ConfigSchema Load(string packRootPath)
    {
        string path = System.IO.Path.Combine(packRootPath ?? "", FileName);
        if (!File.Exists(path))
        {
            return new ConfigSchema(path, existed: false, loadError: null);
        }

        object parsed;
        try
        {
            parsed = MiniJson.Parse(File.ReadAllText(path));
        }
        catch (Exception ex)
        {
            return new ConfigSchema(path, existed: true, loadError: ex.Message);
        }

        if (!(parsed is Dictionary<string, object> root))
        {
            return new ConfigSchema(path, existed: true, loadError: "root must be a JSON object");
        }

        ConfigSchema schema = new ConfigSchema(path, existed: true, loadError: null);
        foreach (KeyValuePair<string, object> pair in root)
        {
            ConfigField field = pair.Key.StartsWith("$", StringComparison.Ordinal)
                ? null
                : ConfigField.FromJson(pair.Key, pair.Value as Dictionary<string, object>);
            if (field != null)
            {
                schema.Fields.Add(field);
            }
            else
            {
                schema.Preamble[pair.Key] = pair.Value;
            }
        }
        return schema;
    }

    /// Compute what the runtime will actually hand to Python: every field's value, with
    /// expression fields resolved against the others.
    ///
    /// Values are converted to the runtime's own numeric types (int / float) FIRST, so
    /// the preview cannot disagree with the game — notably `a / b` on two int fields is
    /// integer division in this dialect, and previewing it as a double would lie.
    /// <paramref name="errors"/> receives one entry per expression that failed to
    /// resolve (unknown name, cycle, non-numeric result).
    public Dictionary<string, object> EvaluateValues(out Dictionary<string, string> errors)
    {
        Dictionary<string, object> values = new Dictionary<string, object>(StringComparer.Ordinal);
        Dictionary<string, string> expressions = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (ConfigField field in Fields)
        {
            if (string.IsNullOrEmpty(field.Name))
            {
                continue;
            }
            values[field.Name] = runtimeValueOf(field);
            if (field.HasExpression)
            {
                expressions[field.Name] = field.Expression;
            }
        }

        Dictionary<string, string> failures = new Dictionary<string, string>(StringComparer.Ordinal);
        Data.Mod.ConfigExpressions.ResolveAll(values, expressions, (name, error) => failures[name] = error);
        errors = failures;
        return values;
    }

    private static object runtimeValueOf(ConfigField field)
    {
        switch (field.Kind)
        {
            case ConfigFieldKind.Int:
                return field.Value is long l ? (int)l : Convert.ToInt32(field.Value ?? 0);
            case ConfigFieldKind.Float:
                return field.Value is double d ? (float)d : Convert.ToSingle(field.Value ?? 0f);
            case ConfigFieldKind.Bool:
                return field.Value is bool b && b;
            default:
                return field.Value as string ?? "";
        }
    }

    /// First problem across all fields (duplicate or malformed names, out-of-range
    /// values), or null when the schema is safe to write.
    public string Validate()
    {
        HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (ConfigField field in Fields)
        {
            string nameError = field.ValidateName();
            if (nameError != null)
            {
                return nameError;
            }
            if (!seen.Add(field.Name))
            {
                return "duplicate field name '" + field.Name + "'";
            }
            string valueError = field.ValidateValue();
            if (valueError != null)
            {
                return field.Name + " " + valueError;
            }
        }
        return null;
    }

    /// Write config.json. Throws on a schema that does not validate, or when the
    /// file exists but failed to parse on load — both would lose data.
    public void Save()
    {
        if (LoadError != null)
        {
            throw new InvalidOperationException(
                FileName + " could not be read (" + LoadError + "); fix it by hand before saving from the editor.");
        }
        string error = Validate();
        if (error != null)
        {
            throw new InvalidOperationException(error);
        }

        Dictionary<string, object> root = new Dictionary<string, object>(StringComparer.Ordinal);
        foreach (KeyValuePair<string, object> pair in Preamble)
        {
            root[pair.Key] = pair.Value;
        }
        foreach (ConfigField field in Fields)
        {
            root[field.Name] = field.ToJson();
        }

        string directory = System.IO.Path.GetDirectoryName(Path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        File.WriteAllText(Path, MiniJsonWriter.Write(root) + "\n",
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    /// Relative `$schema` pointer for a brand-new file, so IDEs validate it. Packs
    /// live one folder below the mods root next to CustomAssets' own schema copy;
    /// the two-level path matches what ModBuilder's template writes.
    public void EnsureSchemaPointer(string relativePath = "../config.schema.json")
    {
        if (!Preamble.ContainsKey("$schema"))
        {
            Preamble["$schema"] = relativePath;
        }
    }
}
