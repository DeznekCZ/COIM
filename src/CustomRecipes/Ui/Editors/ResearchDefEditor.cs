using System;
using CustomAssets.Data.Mod;
using CustomAssets.Editor.Model;
using CustomAssets.Ui.Components;
using Mafi;
using Mafi.Core.Prototypes;
using Mafi.Core.Products;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Editors {

    /// <summary>
    /// Editor for <see cref="ResearchDef"/>. First concrete <see cref="DefEditor{T}"/>
    /// subclass, demonstrating the constructor-builds-layout / observer-driven
    /// rebind pattern. Other research-form sites in the editor still go through
    /// the legacy <c>buildResearchForm</c> helper; this class is the migration
    /// target.
    /// </summary>
    public sealed class ResearchDefEditor : NamedDefEditor<ResearchDef> {

        private readonly TextField m_id;
        private readonly TextField m_name;
        private readonly TextField m_description;
        private readonly TextField m_costsInt;
        private readonly TextField m_costsExpr;
        private readonly TextField m_posX;
        private readonly TextField m_posY;
        private readonly AssetPathPicker m_icon;
        private readonly ProtoPicker<ProductProto> m_tier;

        public ResearchDefEditor(LoadedPack pack, PackModel model, ProtosDb protosDb) {
            // Each AddField call:
            //   1. wraps the field in a labeled column
            //   2. adds the column to this editor's body
            //   3. registers an onRefresh closure that rebinds the field's
            //      display value from `value` whenever Value() swaps
            // The OnValueChanged handlers are wired once at construction
            // time and write back through `value` directly — so swapping
            // Value() updates display but doesn't need to re-attach
            // listeners.
            m_id = AddField("id",
                new TextField().OnValueChanged(v => { if (value != null) value.ResearchId = v; }),
                onRefresh: () => m_id.Text(value.ResearchId ?? ""));

            m_name = AddField("name",
                new TextField().OnValueChanged(v => { if (value != null) value.Name = v; }),
                onRefresh: () => m_name.Text(value.Name ?? ""));

            m_description = AddField("description",
                new TextField()
                    .Multiline(true)
                    .SetTextAreaMinHeight(48.px())
                    .OnValueChanged(v => { if (value != null) value.Description = v; }),
                onRefresh: () => m_description.Text(value.Description ?? ""));

            // Costs — bare int when set; raw expression for typed-ref shapes
            // like ResearchCostsTpl.Tier2(). Editing the numeric field clears
            // the raw expression and vice versa so the emitter has a single
            // source of truth.
            m_costsInt = AddField(
                "cost (research points; leave blank to omit)",
                new TextField()
                    .PositiveIntegersOnly()
                    .OnValueChanged(v => {
                        if (value == null) return;
                        if (string.IsNullOrEmpty(v)) {
                            value.CostsAsInt = null;
                        } else if (int.TryParse(v, out int parsed)) {
                            value.CostsAsInt = parsed;
                            value.CostsExpression = null;
                        }
                    }),
                onRefresh: () =>
                    m_costsInt.Text(value.CostsAsInt.HasValue
                        ? value.CostsAsInt.Value.ToString()
                        : ""));

            m_costsExpr = AddField(
                "cost (typed-ref expression — clear to switch to plain int)",
                new TextField()
                    .Class(Cls.fontMonospace)
                    .OnValueChanged(v => {
                        if (value != null) value.CostsExpression = string.IsNullOrEmpty(v) ? null : v;
                    }),
                onRefresh: () => m_costsExpr.Text(value.CostsExpression ?? ""));

            // Tier picker — product picker filtered to research-pack items
            // the lab consumes (LabEquipment / LabEquipment2 / …). The
            // picker captures `() => value.TierProductId` so it always
            // resolves against the current value.
            m_tier = AddField(
                "tier (research-pack product the lab consumes; (none) to leave unset)",
                ResearchPackProductPicker.Build(
                    protosDb,
                    getId: () => value?.TierProductId,
                    setId: id => { if (value != null) value.TierProductId = string.IsNullOrEmpty(id) ? null : id; },
                    title: new LocStrFormatted("Pick research-pack tier")),
                onRefresh: () => m_tier.RefreshDisplay());

            // Position — two int fields side-by-side. AddField wraps the
            // composite row so the (x:, y:) layout reads as a single field
            // with its label above.
            Row posRow = new Row();
            posRow.Gap(4.pt()).AlignItemsCenter();
            posRow.Add(new Label(new LocStrFormatted("x:")));
            m_posX = new TextField().PositiveIntegersOnly().Width(60.px())
                .OnValueChanged(v => { if (value != null) value.PositionX = parseNullableInt(v); });
            posRow.Add(m_posX);
            posRow.Add(new Label(new LocStrFormatted("y:")));
            m_posY = new TextField().PositiveIntegersOnly().Width(60.px())
                .OnValueChanged(v => { if (value != null) value.PositionY = parseNullableInt(v); });
            posRow.Add(m_posY);
            AddField("position (x, y in research tree)", posRow, onRefresh: () => {
                m_posX.Text(value.PositionX.HasValue ? value.PositionX.Value.ToString() : "");
                m_posY.Text(value.PositionY.HasValue ? value.PositionY.Value.ToString() : "");
            });

            m_icon = AddField(
                "icon (pack asset or Mafi typed-ref; (none) to omit)",
                new AssetPathPicker(
                    pack,
                    getPath: () => value?.IconPath,
                    setPath: v => { if (value != null) value.IconPath = string.IsNullOrEmpty(v) ? null : v; },
                    allowMafiAssets: true,
                    title: new LocStrFormatted("Pick research icon"),
                    variableCandidates: () => EditorHelpers.AssetVariablesIn(
                        model, value, CustomAssets.Editor.AssetsCatalog.AssetKind.Image)),
                onRefresh: () => m_icon.RefreshDisplay());
        }

        private static int? parseNullableInt(string s) {
            if (string.IsNullOrWhiteSpace(s)) return null;
            return int.TryParse(s.Trim(), out int v) ? v : (int?)null;
        }
    }
}
