using CustomAssets.Editor.Model;
using CustomAssets.Ui.Components;
using Mafi;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Editors {

    /// <c>build_generator</c> editor — id / name / description /
    /// inputProduct + outputElectricityKw / outputProduct / source generator /
    /// duration / generationPriority / bufferCapacityMultiplier / research /
    /// lockedOnInit tri-state.
    public sealed class GeneratorDefEditor : NamedDefEditor<GeneratorDef> {

        private readonly TextField m_id;
        private readonly TextField m_name;
        private readonly TextField m_description;
        private readonly ProductExpressionPicker m_inputProduct;
        private readonly TextField m_outputElectricityKw;
        private readonly ProductExpressionPicker m_outputProduct;
        private readonly TextField m_source;
        private readonly TextField m_duration;
        private readonly TextField m_genPriority;
        private readonly TextField m_bufferMult;
        private readonly ResearchIdPicker m_research;
        private readonly TextField m_lockedOnInit;

        public GeneratorDefEditor(PackModel model, ProtosDb protosDb) {
            m_id = AddField("id",
                new TextField().OnValueChanged(v => { if (value != null) value.GeneratorId = v; }),
                onRefresh: () => m_id.Text(value.GeneratorId ?? ""));

            m_name = AddField("name",
                new TextField().OnValueChanged(v => { if (value != null) value.Name = v; }),
                onRefresh: () => m_name.Text(value.Name ?? ""));

            m_description = AddField("description",
                new TextField()
                    .Multiline(true)
                    .SetTextAreaMinHeight(48.px())
                    .OnValueChanged(v => { if (value != null) value.Description = v; }),
                onRefresh: () => m_description.Text(value.Description ?? ""));

            m_inputProduct = AddField(
                "inputProduct",
                new ProductExpressionPicker(
                    protosDb,
                    getExpression: () => value?.InputProductExpression,
                    setExpression: v => {
                        if (value != null) value.InputProductExpression = v;
                    }),
                onRefresh: () => m_inputProduct.RefreshDisplay());

            m_outputElectricityKw = AddField(
                "outputElectricityKw",
                new TextField()
                    .PositiveIntegersOnly()
                    .OnValueChanged(v => {
                        if (value != null) value.OutputElectricityKw = EditorHelpers.ParseNullableInt(v);
                    }),
                onRefresh: () => m_outputElectricityKw.Text(value.OutputElectricityKw.HasValue
                    ? value.OutputElectricityKw.Value.ToString() : ""));

            m_outputProduct = AddField(
                "outputProduct (optional)",
                new ProductExpressionPicker(
                    protosDb,
                    getExpression: () => value?.OutputProductExpression,
                    setExpression: v => {
                        if (value != null) value.OutputProductExpression = v;
                    },
                    allowNone: true,
                    emptyLabel: new LocStrFormatted("(no byproduct)")),
                onRefresh: () => m_outputProduct.RefreshDisplay());

            m_source = AddField(
                "source generator id (default DieselGeneratorT2)",
                new TextField()
                    .Class(Cls.fontMonospace)
                    .OnValueChanged(v => { if (value != null) value.SourceId = v; }),
                onRefresh: () => m_source.Text(value.SourceId ?? ""));

            m_duration = AddField(
                "duration (seconds; blank = inherit from source)",
                new TextField()
                    .PositiveIntegersOnly()
                    .OnValueChanged(v => {
                        if (value != null) value.DurationSeconds = EditorHelpers.ParseNullableInt(v);
                    }),
                onRefresh: () => m_duration.Text(value.DurationSeconds.HasValue
                    ? value.DurationSeconds.Value.ToString() : ""));

            m_genPriority = AddField("generationPriority (optional int)",
                new TextField()
                    .PositiveIntegersOnly()
                    .OnValueChanged(v => {
                        if (value != null) value.GenerationPriority = EditorHelpers.ParseNullableInt(v);
                    }),
                onRefresh: () => m_genPriority.Text(value.GenerationPriority.HasValue
                    ? value.GenerationPriority.Value.ToString() : ""));

            m_bufferMult = AddField("bufferCapacityMultiplier (optional int)",
                new TextField()
                    .PositiveIntegersOnly()
                    .OnValueChanged(v => {
                        if (value != null) value.BufferCapacityMultiplier = EditorHelpers.ParseNullableInt(v);
                    }),
                onRefresh: () => m_bufferMult.Text(value.BufferCapacityMultiplier.HasValue
                    ? value.BufferCapacityMultiplier.Value.ToString() : ""));

            m_research = AddField("research (unlocks this generator)",
                new ResearchIdPicker(
                    model,
                    protosDb,
                    ownerDef: null,
                    getId: () => value?.ResearchId,
                    setId: id => { if (value != null) value.ResearchId = string.IsNullOrEmpty(id) ? null : id; },
                    title: new LocStrFormatted("Pick research")),
                onRefresh: () => m_research.RefreshDisplay());

            // lockedOnInit is tri-state (null / true / false). Plain text
            // field accepting "true"/"false"/blank — a Toggle would lose the
            // "unset" state.
            m_lockedOnInit = AddField(
                "lockedOnInit (true / false / blank to inherit research default)",
                new TextField().OnValueChanged(v => {
                    if (value == null) return;
                    string s = (v ?? "").Trim().ToLowerInvariant();
                    if (s == "true") value.LockedOnInit = true;
                    else if (s == "false") value.LockedOnInit = false;
                    else value.LockedOnInit = null;
                }),
                onRefresh: () => m_lockedOnInit.Text(value.LockedOnInit.HasValue
                    ? (value.LockedOnInit.Value ? "true" : "false")
                    : ""));
        }
    }
}
