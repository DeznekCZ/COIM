using CustomAssets.Data.Mod;
using CustomAssets.Editor.Model;
using CustomAssets.Ui.Components;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Editors {

    /// <c>build_product_fluid</c> editor — common product header (via base)
    /// + three bool flags appended to the shared wrap-row + three RGB tuple
    /// color fields.
    public sealed class ProductFluidDefEditor : ProductDefEditor<ProductFluidDef> {

        private readonly CollapsibleGroup m_colorGroup;
        private readonly UiComponent m_colorSwatch;
        private readonly Label m_colorValue;
        private readonly Mafi.Unity.Ui.Library.RgbColorPicker m_colorPicker;
        private readonly Label m_colorRaw;
        private readonly CollapsibleGroup m_transportColorGroup;
        private readonly UiComponent m_transportColorSwatch;
        private readonly Label m_transportColorValue;
        private readonly Mafi.Unity.Ui.Library.RgbColorPicker m_transportColorPicker;
        private readonly Label m_transportColorRaw;
        private readonly CollapsibleGroup m_accentColorGroup;
        private readonly UiComponent m_accentColorSwatch;
        private readonly Label m_accentColorValue;
        private readonly Mafi.Unity.Ui.Library.RgbColorPicker m_accentColorPicker;
        private readonly Label m_accentColorRaw;

        public ProductFluidDefEditor(LoadedPack pack, PackModel model, ProtosDb protosDb)
                : base(pack, model, protosDb) {

            AddBoolFlag("isStorable",     d => d.IsStorable,     (d, v) => d.IsStorable     = v);
            AddBoolFlag("isWaste",        d => d.IsWaste,        (d, v) => d.IsWaste        = v);
            AddBoolFlag("canBeDiscarded", d => d.CanBeDiscarded, (d, v) => d.CanBeDiscarded = v);

            m_colorGroup = EditorHelpers.BuildColorField("color (RGB tuple)",
                () => value?.ColorExpression,
                v => { if (value != null) value.ColorExpression = string.IsNullOrEmpty(v) ? null : v; },
                out m_colorSwatch, out m_colorValue, out m_colorPicker, out m_colorRaw);
            AddField("", m_colorGroup, onRefresh: () =>
                EditorHelpers.RefreshColorField(value.ColorExpression,
                    m_colorSwatch, m_colorValue, m_colorPicker, m_colorRaw));

            m_transportColorGroup = EditorHelpers.BuildColorField("transportColor (RGB tuple)",
                () => value?.TransportColorExpression,
                v => { if (value != null) value.TransportColorExpression = string.IsNullOrEmpty(v) ? null : v; },
                out m_transportColorSwatch, out m_transportColorValue,
                out m_transportColorPicker, out m_transportColorRaw);
            AddField("", m_transportColorGroup, onRefresh: () =>
                EditorHelpers.RefreshColorField(value.TransportColorExpression,
                    m_transportColorSwatch, m_transportColorValue,
                    m_transportColorPicker, m_transportColorRaw));

            m_accentColorGroup = EditorHelpers.BuildColorField("transportAccentColor (RGB tuple)",
                () => value?.TransportAccentColorExpression,
                v => { if (value != null) value.TransportAccentColorExpression = string.IsNullOrEmpty(v) ? null : v; },
                out m_accentColorSwatch, out m_accentColorValue,
                out m_accentColorPicker, out m_accentColorRaw);
            AddField("", m_accentColorGroup, onRefresh: () =>
                EditorHelpers.RefreshColorField(value.TransportAccentColorExpression,
                    m_accentColorSwatch, m_accentColorValue,
                    m_accentColorPicker, m_accentColorRaw));
        }
    }
}
