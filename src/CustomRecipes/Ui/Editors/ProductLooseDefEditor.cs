using CustomAssets.Data.Mod;
using CustomAssets.Editor.Model;
using CustomAssets.Ui.Components;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Editors {

    /// <c>build_product_loose</c> editor — common product header (via base)
    /// + six bool flags appended to the shared wrap-row + loose-specific
    /// material + color/particleColor + maxTransport + prefabPath + dumpsAs.
    public sealed class ProductLooseDefEditor : ProductDefEditor<ProductLooseDef> {

        private readonly TextField m_material;
        private readonly CollapsibleGroup m_colorGroup;
        private readonly UiComponent m_colorSwatch;
        private readonly Label m_colorValue;
        private readonly Mafi.Unity.Ui.Library.RgbColorPicker m_colorPicker;
        private readonly Label m_colorRaw;
        private readonly CollapsibleGroup m_particleGroup;
        private readonly UiComponent m_particleSwatch;
        private readonly Label m_particleValue;
        private readonly Mafi.Unity.Ui.Library.RgbColorPicker m_particlePicker;
        private readonly Label m_particleRaw;
        private readonly TextField m_maxTransport;
        private readonly TextField m_prefabPath;
        private readonly TextField m_dumpsAs;

        public ProductLooseDefEditor(LoadedPack pack, PackModel model, ProtosDb protosDb)
                : base(pack, model, protosDb) {

            // Bool flags populate the row reserved by the base right under
            // the research picker. Order here is the visual order.
            AddBoolFlag("isStorable",      d => d.IsStorable,      (d, v) => d.IsStorable      = v);
            AddBoolFlag("isWaste",         d => d.IsWaste,         (d, v) => d.IsWaste         = v);
            AddBoolFlag("isDumped",        d => d.IsDumped,        (d, v) => d.IsDumped        = v);
            AddBoolFlag("isRecyclable",    d => d.IsRecyclable,    (d, v) => d.IsRecyclable    = v);
            AddBoolFlag("isRough",         d => d.IsRough,         (d, v) => d.IsRough         = v);
            AddBoolFlag("pinToHomeScreen", d => d.PinToHomeScreen, (d, v) => d.PinToHomeScreen = v);

            m_material = AddField("material (asset path or expression)",
                new TextField()
                    .Class(Cls.fontMonospace)
                    .OnValueChanged(v => { if (value != null) value.MaterialExpression = v; }),
                onRefresh: () => m_material.Text(value.MaterialExpression ?? ""));

            // Color fields go through EditorHelpers.BuildColorField which
            // returns the CollapsibleGroup plus the inner widgets for the
            // refresh callback.
            m_colorGroup = EditorHelpers.BuildColorField(
                "color",
                () => value?.ColorExpression,
                v => { if (value != null) value.ColorExpression = string.IsNullOrEmpty(v) ? null : v; },
                out m_colorSwatch, out m_colorValue, out m_colorPicker, out m_colorRaw);
            AddField("", m_colorGroup, onRefresh: () =>
                EditorHelpers.RefreshColorField(value.ColorExpression,
                    m_colorSwatch, m_colorValue, m_colorPicker, m_colorRaw));

            m_particleGroup = EditorHelpers.BuildColorField(
                "particleColor",
                () => value?.ParticleColorExpression,
                v => { if (value != null) value.ParticleColorExpression = string.IsNullOrEmpty(v) ? null : v; },
                out m_particleSwatch, out m_particleValue, out m_particlePicker, out m_particleRaw);
            AddField("", m_particleGroup, onRefresh: () =>
                EditorHelpers.RefreshColorField(value.ParticleColorExpression,
                    m_particleSwatch, m_particleValue, m_particlePicker, m_particleRaw));

            m_maxTransport = AddField(
                "maxTransport (units per stack; blank = default 5)",
                new TextField()
                    .PositiveIntegersOnly()
                    .OnValueChanged(v => {
                        if (value != null) value.MaxTransport = EditorHelpers.ParseNullableInt(v);
                    }),
                onRefresh: () => m_maxTransport.Text(value.MaxTransport.HasValue
                    ? value.MaxTransport.Value.ToString() : ""));

            m_prefabPath = AddField(
                "prefabPath (override pile prefab; blank = default)",
                new TextField().OnValueChanged(v => {
                    if (value != null) value.PrefabPath = string.IsNullOrEmpty(v) ? null : v;
                }),
                onRefresh: () => m_prefabPath.Text(value.PrefabPath ?? ""));

            m_dumpsAs = AddField(
                "dumpsAs (terrain-material id; e.g. Ids.TerrainMaterials.Gravel)",
                new TextField()
                    .Class(Cls.fontMonospace)
                    .OnValueChanged(v => {
                        if (value != null) value.DumpsAsId = string.IsNullOrEmpty(v) ? null : v;
                    }),
                onRefresh: () => m_dumpsAs.Text(value.DumpsAsId ?? ""));
        }
    }
}
