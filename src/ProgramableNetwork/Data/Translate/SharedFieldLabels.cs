using Mafi.Core.Prototypes;
using System.Collections.Generic;
using System.Text;

namespace ProgramableNetwork
{
    /// <summary>
    /// Marker wrapping a label whose translation should be registered ONCE under the
    /// shared key space (<c>ProgramableNetwork_PinOrField_&lt;name&gt;</c>) instead of
    /// being minted per-module (<c>ProgramableNetwork_Module_X__input__N</c>).
    ///
    /// Build one via <see cref="SharedFieldLabels.Shared(string)"/>, then pass into
    /// the <c>ModuleProto.Builder</c> overloads that take <c>SharedLabel</c>:
    /// <code>builder.AddInput("a", "A".Shared())</code>
    /// </summary>
    public readonly struct SharedLabel
    {
        public readonly string Label;
        public SharedLabel(string label) { Label = label; }
    }

    /// <summary>
    /// Single source of truth for shared module pin/field labels. Each unique
    /// English label is registered ONCE; subsequent <c>.Shared()</c> calls with the
    /// same label reuse the existing <see cref="Proto.Str"/>. The Builder doesn't
    /// need a registrator argument because Proto.CreateStr is itself static — the
    /// shared registry is scoped to the running mod instance.
    ///
    /// <see cref="Defaults"/> documents the high-frequency labels we expect to share
    /// across modules. The list is informational — any string can be used with
    /// <c>.Shared()</c> regardless of whether it appears here.
    /// </summary>
    public static class SharedFieldLabels
    {
        public static readonly string[] Defaults = new[]
        {
            // Single-letter pin labels (Arithmetic / Boolean / Decision modules)
            "A", "B", "C", "D", "E", "F", "G", "H",
            // Common semantic labels
            "Value", "Bits", "Index", "Frequency", "Product", "Percentage",
            "Min", "Max", "Default", "Enable", "Display", "Storage", "Dot",
            // Numbered inputs (used by Decision modules)
            "Input 1", "Input 2", "Input 3", "Input 4",
        };

        // Proto.CreateStr would throw on a duplicate ID, so the cache also acts as
        // a guard against re-registration when the same shared label appears on
        // multiple modules during proto registration.
        private static readonly Dictionary<string, Proto.Str> s_byLabel
            = new Dictionary<string, Proto.Str>();

        /// <summary>
        /// Marks <paramref name="label"/> for registration under the shared key
        /// space. Pass the result into Builder methods that accept a SharedLabel.
        /// </summary>
        public static SharedLabel Shared(this string label) => new SharedLabel(label);

        /// <summary>
        /// Resolves the SharedLabel to a <see cref="Proto.Str"/>, registering it
        /// under <c>ProgramableNetwork_PinOrField_&lt;sanitized&gt;</c> on first
        /// access. Idempotent: repeated calls with the same label return the same
        /// Proto.Str.
        /// </summary>
        public static Proto.Str Resolve(this SharedLabel shared)
        {
            string label = shared.Label ?? "";
            if (s_byLabel.TryGetValue(label, out Proto.Str existing)) {
                return existing;
            }
            Proto.Str str = Proto.CreateStr(
                new Proto.ID("ProgramableNetwork_PinOrField_" + Sanitize(label)),
                label,
                "");
            s_byLabel[label] = str;
            return str;
        }

        // Replace anything outside [A-Za-z0-9_] with `_`, collapsing runs so
        // "Input  1" → "Input_1" rather than "Input__1". Empty input → "_".
        private static string Sanitize(string label)
        {
            if (string.IsNullOrEmpty(label)) {
                return "_";
            }
            StringBuilder sb = new StringBuilder(label.Length);
            bool lastWasUnderscore = false;
            foreach (char c in label)
            {
                bool ok = (c >= 'A' && c <= 'Z')
                       || (c >= 'a' && c <= 'z')
                       || (c >= '0' && c <= '9')
                       || c == '_';
                if (ok)
                {
                    sb.Append(c);
                    lastWasUnderscore = false;
                }
                else if (!lastWasUnderscore)
                {
                    sb.Append('_');
                    lastWasUnderscore = true;
                }
            }
            return sb.Length == 0 ? "_" : sb.ToString();
        }
    }
}
