using System;
using System.Collections.Generic;
using CustomAssets.Data.Mod;
using CustomAssets.Editor.Model;
using CustomAssets.Ui.Components;
using Mafi;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Editors {

    /// Shared base for the three `build_product_*` editor variants. Lays
    /// out the common header (id / name / description / icon / research)
    /// followed by a single wrap-row of bool flags that the concrete
    /// subclass populates via <see cref="AddBoolFlag"/>. Kind-specific
    /// non-bool fields (material, prefab, colors, …) appended after
    /// `base(...)` returns stack below the flag row.
    public abstract class ProductDefEditor<T> : NamedDefEditor<T> where T : ProductDefBase {

        // Exposed to subclasses so they can refresh display values from
        // `value` in their own onRefresh callbacks.
        protected readonly TextField m_id;
        protected readonly TextField m_name;
        protected readonly TextField m_description;
        protected readonly AssetPathPicker m_icon;
        protected readonly ResearchIdPicker m_research;
        protected readonly TextField m_isLocked;

        // Bool flag wrap-row that lives right under the research picker.
        // Subclasses fill it via AddBoolFlag(...) calls in their own
        // constructor body, after `base(...)` returns. The Row reference
        // stays live so appended toggles render in the slot the base
        // already placed in the column.
        private readonly Row m_flagRow;
        private readonly List<Action> m_flagRefreshers = new List<Action>();

        protected ProductDefEditor(LoadedPack pack, PackModel model, ProtosDb protosDb) {
            m_id = AddField("id",
                new TextField().OnValueChanged(v => { if (value != null) value.ProductId = v; }),
                onRefresh: () => m_id.Text(value.ProductId ?? ""));

            m_name = AddField("name",
                new TextField().OnValueChanged(v => { if (value != null) value.Name = v; }),
                onRefresh: () => m_name.Text(value.Name ?? ""));

            m_description = AddField("description",
                new TextField()
                    .Multiline(true)
                    .SetTextAreaMinHeight(48.px())
                    .OnValueChanged(v => { if (value != null) value.Description = v; }),
                onRefresh: () => m_description.Text(value.Description ?? ""));

            m_icon = AddField("icon (pack asset or Mafi typed-ref)",
                new AssetPathPicker(
                    pack,
                    getPath: () => value?.IconPath,
                    setPath: v => { if (value != null) value.IconPath = string.IsNullOrEmpty(v) ? null : v; },
                    allowMafiAssets: true,
                    title: new LocStrFormatted("Pick product icon"),
                    variableCandidates: () => EditorHelpers.AssetVariablesIn(
                        model, value, CustomAssets.Editor.AssetsCatalog.AssetKind.Image)),
                onRefresh: () => m_icon.RefreshDisplay());

            m_research = AddField("research (unlocks this product)",
                new ResearchIdPicker(
                    model,
                    protosDb,
                    ownerDef: null,
                    getId: () => value?.ResearchId,
                    setId: id => { if (value != null) value.ResearchId = string.IsNullOrEmpty(id) ? null : id; },
                    title: new LocStrFormatted("Pick research")),
                onRefresh: () => m_research.RefreshDisplay());

            // isLocked is tri-state rather than a toggle in the flag row,
            // because its runtime default is "locked when a research is set" —
            // so blank, True and False are three distinct outcomes and a plain
            // checkbox could not express "leave it to the default".
            m_isLocked = AddField(
                "isLocked (true / false / blank = locked only when a research is set)",
                new TextField().OnValueChanged(v => {
                    if (value == null)
                    {
                        return;
                    }
                    string normalized = (v ?? "").Trim().ToLowerInvariant();
                    if (normalized == "true")
                    {
                        value.IsLocked = true;
                    }
                    else if (normalized == "false")
                    {
                        value.IsLocked = false;
                    }
                    else
                    {
                        value.IsLocked = null;
                    }
                    MarkEdited();
                }),
                onRefresh: () => m_isLocked.Text(value.IsLocked.HasValue
                    ? (value.IsLocked.Value ? "true" : "false")
                    : ""));

            // Reserve the bool-grid slot now. The Row is empty at this
            // point; subclasses append toggles via AddBoolFlag(...).
            m_flagRow = new Row();
            m_flagRow.Wrap().Gap(4.px(), 4.px()).AlignItemsCenter();
            AddField("flags", m_flagRow, onRefresh: () => {
                for (int i = 0; i < m_flagRefreshers.Count; i++) m_flagRefreshers[i]();
            });
        }

        /// Append one toggle to the shared bool-flag row. Reads / writes
        /// flow through <c>value</c> so a Value() swap rebinds the toggle
        /// automatically. Returns the Toggle so callers can attach extra
        /// styling at the call site if they need to.
        protected Toggle AddBoolFlag(string label,
                Func<T, bool> getter, Action<T, bool> setter) {
            Toggle t = new Toggle(standalone: true);
            ((IComponentWithLabel)t).SetLabel(new LocStrFormatted(label));
            t.OnValueChanged(v => { if (value != null) setter(value, v); });
            m_flagRow.Add(t);
            m_flagRefreshers.Add(() => t.Value(value != null && getter(value)));
            return t;
        }
    }
}
