using System;
using System.Collections.Generic;
using CustomAssets.Editor;
using CustomAssets.Editor.Model;
using Mafi;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using CustomAssets.Ui.Components;

namespace CustomAssets.Ui.Editors {

    /// <summary>
    /// Shared helpers reused across <see cref="DefEditor{T}"/> subclasses —
    /// inline collapsible color pickers, color-expression parsing, numeric
    /// formatting / parsing helpers. Static so subclasses can call without
    /// taking an instance dependency.
    /// </summary>
    internal static class EditorHelpers {

        /// Comma-separated id text → trimmed id list, or null when nothing is left.
        /// Null (rather than an empty list) so the emitter omits the argument
        /// entirely instead of writing `replaces = []`.
        internal static List<string> SplitIdList(string text) {
            if (string.IsNullOrWhiteSpace(text)) {
                return null;
            }
            List<string> ids = new List<string>();
            foreach (string part in text.Split(',')) {
                string trimmed = part.Trim();
                if (trimmed.Length > 0) {
                    ids.Add(trimmed);
                }
            }
            return ids.Count == 0 ? null : ids;
        }

        /// Inline RgbColorPicker wrapped in a CollapsibleGroup (expanded by
        /// default). Writes back as a Python `(R, G, B)` tuple. Returns the
        /// CollapsibleGroup so the editor can add it as one component.
        ///
        /// The caller is responsible for binding refresh: on Value() swap,
        /// call <see cref="RefreshColorField"/> with the same getter to
        /// rebind both the picker and the header swatch/value chip.
        internal static CollapsibleGroup BuildColorField(string label,
                Func<string> getExpr, Action<string> setExpr,
                out UiComponent swatch, out Label valueLabel,
                out Mafi.Unity.Ui.Library.RgbColorPicker picker,
                out Label rawHint) {
            string initialExpr = getExpr();
            bool initialIsLiteral = TryParseColorExpression(initialExpr, out ColorRgba initialColor);
            ColorRgba effectiveColor = initialIsLiteral ? initialColor : ColorRgba.Gray;

            swatch = new UiComponent()
                .Size(18.px())
                .Border(1.px(), Theme.BorderColor, 3)
                .Background(effectiveColor);
            valueLabel = new Label(new LocStrFormatted(
                string.IsNullOrEmpty(initialExpr) ? "(unset)" : initialExpr));
            valueLabel.Class(Cls.fontMonospace).TinyFontSize();

            CollapsibleGroup group = new CollapsibleGroup(
                new LocStrFormatted(label), expanded: true);
            group.Header.Add(swatch);
            group.Header.Add(valueLabel);

            picker = new Mafi.Unity.Ui.Library.RgbColorPicker(new LocStrFormatted(label));
            picker.Value(effectiveColor);

            rawHint = new Label(new LocStrFormatted(
                initialIsLiteral || string.IsNullOrEmpty(initialExpr)
                    ? ""
                    : "current: " + initialExpr));
            rawHint.TinyFontSize().Color(ColorRgba.LightGray);
            if (initialIsLiteral || string.IsNullOrEmpty(initialExpr)) {
                rawHint.Visible(false);
            }

            // Capture references locally so the lambda doesn't depend on the
            // out-params having a particular binding state.
            UiComponent capturedSwatch = swatch;
            Label capturedValueLabel = valueLabel;
            Label capturedRawHint = rawHint;
            picker.OnColorChanged(c => {
                string expr = "(" + c.R + ", " + c.G + ", " + c.B + ")";
                setExpr(expr);
                capturedSwatch.Background(c);
                capturedValueLabel.Value(new LocStrFormatted(expr));
                capturedRawHint.Visible(false);
            });

            group.Body.Add(picker);
            group.Body.Add(rawHint);
            return group;
        }

        /// Rebind a color field's display widgets to a (possibly different)
        /// underlying expression. Called from <see cref="DefEditor{T}.Value"/>
        /// observers to keep the picker, swatch, value chip, and raw-hint
        /// all in sync after the editor's bound instance changes.
        internal static void RefreshColorField(string currentExpr,
                UiComponent swatch, Label valueLabel,
                Mafi.Unity.Ui.Library.RgbColorPicker picker, Label rawHint) {
            bool isLiteral = TryParseColorExpression(currentExpr, out ColorRgba c);
            ColorRgba effective = isLiteral ? c : ColorRgba.Gray;
            swatch.Background(effective);
            valueLabel.Value(new LocStrFormatted(
                string.IsNullOrEmpty(currentExpr) ? "(unset)" : currentExpr));
            picker.Value(effective);
            bool showRaw = !isLiteral && !string.IsNullOrEmpty(currentExpr);
            rawHint.Value(new LocStrFormatted(showRaw ? "current: " + currentExpr : ""));
            rawHint.Visible(showRaw);
        }

        // Same parser as the old colorPickerField helper. Accepts:
        //   • (R, G, B) / (R, G, B, A) Python tuples
        //   • "#ffffff" / "#FFFFFFFF" quoted hex strings
        //   • #ffffff bare hex
        //   • ColorRgba.NamedConstant â€” resolved via reflection against
        //     the static readonly fields on Mafi.ColorRgba so the picker
        //     shows the right swatch for the built-in palette.
        internal static bool TryParseColorExpression(string expr, out ColorRgba color) {
            color = ColorRgba.Empty;
            if (string.IsNullOrWhiteSpace(expr)) return false;
            string s = expr.Trim();
            // ColorRgba.X typed-ref â€” Mafi exposes the named palette as
            // public static readonly fields (Black, DarkDarkGray, Brown,
            // âŠ¦). Reflect to pull the value; unknown name falls through to
            // the other formats below.
            if (s.StartsWith("ColorRgba.")) {
                string name = s.Substring("ColorRgba.".Length).Trim();
                if (tryResolveColorRgbaConstant(name, out color)) return true;
            }
            if (s.Length >= 2
                    && (s[0] == '"' || s[0] == '\'')
                    && s[s.Length - 1] == s[0]) {
                s = s.Substring(1, s.Length - 2).Trim();
            }
            if (s.StartsWith("#")) {
                return ColorRgba.TryParseHex(s.Substring(1), out color);
            }
            if (s.StartsWith("(") && s.EndsWith(")"))
                s = s.Substring(1, s.Length - 2);
            string[] parts = s.Split(',');
            if (parts.Length != 3 && parts.Length != 4) return false;
            if (!int.TryParse(parts[0].Trim(), out int r)) return false;
            if (!int.TryParse(parts[1].Trim(), out int g)) return false;
            if (!int.TryParse(parts[2].Trim(), out int b)) return false;
            int a = 255;
            if (parts.Length == 4 && !int.TryParse(parts[3].Trim(), out a)) return false;
            color = new ColorRgba(r, g, b, a);
            return true;
        }

        // Reflect once and cache the public static readonly ColorRgba
        // fields by name. Mafi's named-palette set is small and stable
        // across releases; reflection here is a one-shot cost on first
        // color-field opening.
        private static System.Collections.Generic.Dictionary<string, ColorRgba>
            s_colorRgbaConstants;

        private static bool tryResolveColorRgbaConstant(string name, out ColorRgba color) {
            color = ColorRgba.Empty;
            if (string.IsNullOrEmpty(name)) return false;
            if (s_colorRgbaConstants == null) {
                var map = new System.Collections.Generic.Dictionary<string, ColorRgba>(
                    System.StringComparer.Ordinal);
                foreach (var f in typeof(ColorRgba).GetFields(
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.Static)) {
                    if (f.FieldType == typeof(ColorRgba) && f.IsInitOnly) {
                        try { map[f.Name] = (ColorRgba)f.GetValue(null); }
                        catch { /* skip any field that fails to read */ }
                    }
                }
                s_colorRgbaConstants = map;
            }
            return s_colorRgbaConstants.TryGetValue(name, out color);
        }

        // Defs in the OWNER's source file that have a Python variable name
        // bound and a kind compatible with the asset-picker filter. Used as
        // the variableCandidates source for AssetPathPicker: a picker for
        // an "Image" field surfaces every Tex variable, a "Material" field
        // surfaces every material variable, etc. The owner def itself is
        // skipped so a self-referential pick is impossible.
        internal static IEnumerable<DefBase> AssetVariablesIn(
                PackModel model, DefBase owner, AssetsCatalog.AssetKind kind) {
            if (model?.Definitions == null) yield break;
            if (owner == null || string.IsNullOrEmpty(owner.SourceFile)) yield break;
            foreach (DefBase d in model.Definitions) {
                if (ReferenceEquals(d, owner)) continue;
                if (string.IsNullOrEmpty(d.VariableName)) continue;
                if (d.SourceFile != owner.SourceFile) continue;
                if (!variableMatchesKind(d, kind)) continue;
                yield return d;
            }
        }

        private static bool variableMatchesKind(DefBase d, AssetsCatalog.AssetKind kind) {
            switch (kind) {
                case AssetsCatalog.AssetKind.Image:
                    // Tex variables are the natural fit for an icon /
                    // texture path field. Materials and prefabs aren't
                    // textures themselves so we don't surface them here.
                    return d is TextureDef;
                case AssetsCatalog.AssetKind.Material:
                    return d is MaterialLooseDef || d is MaterialTextureDef;
                case AssetsCatalog.AssetKind.Prefab:
                    // Pack-side prefab variables are add_prefab_box /
                    // add_unit_prefab return values — both produce a
                    // GameObject the caller can pass into product /
                    // recipe definitions.
                    return d is PrefabBoxDef || d is UnitPrefabDef;
                case AssetsCatalog.AssetKind.Any:
                    return d is TextureDef
                        || d is MaterialLooseDef
                        || d is MaterialTextureDef
                        || d is PrefabBoxDef
                        || d is UnitPrefabDef;
                default:
                    return false;
            }
        }

        internal static int? ParseNullableInt(string s) {
            if (string.IsNullOrWhiteSpace(s)) return null;
            return int.TryParse(s.Trim(), out int v) ? v : (int?)null;
        }

        internal static double? ParseNullableDouble(string s) {
            if (string.IsNullOrWhiteSpace(s)) return null;
            return double.TryParse(s.Trim(),
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out double v) ? v : (double?)null;
        }

        internal static string FormatDouble(double v) {
            return v.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
        }

        // Per-def variable resolver: turns a Python variable name into the
        // underlying registered id by consulting the def's SourceFileVariables
        // map. Used by every typed picker that supports variable bindings.
        internal static Func<string, string> VariableResolverFor(
                CustomAssets.Editor.Model.DefBase def) {
            if (def == null || def.SourceFileVariables == null) return null;
            return id => {
                if (string.IsNullOrEmpty(id)) return null;
                return def.SourceFileVariables.TryGetValue(id, out string resolved)
                    ? resolved : null;
            };
        }
    }
}
