using System;
using CustomAssets.Editor.Model;
using CustomAssets.Ui.Components;
using Mafi;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Editors {

    /// Right-pane editor for an <c>if</c>/<c>elif</c> clause. Mode dropdown
    /// chooses between free-text ("Custom") and a typed helper like
    /// productExists (product picker → <c>product_exist("Id")</c>). The
    /// "Save condition" button delegates the splice back to the window via
    /// <c>onSave</c>, which is responsible for writing the header line and
    /// triggering a rescan + tree rebuild.
    public sealed class IfBlockDefEditor : DefEditor<IfBlockDef> {

        private const string ModeCustom = "Custom";
        private const string ModeProductExists = "productExists";

        private readonly Dropdown<string> m_modeDropdown;
        private readonly Column m_customRow;
        private readonly TextField m_customField;
        private readonly Column m_productRow;
        private readonly ProtoPicker<ProductProto> m_productPicker;

        private string m_currentMode = ModeCustom;
        private string m_customText = "";
        private string m_productId;

        public IfBlockDefEditor(ProtosDb protosDb, Action<IfBlockDef, string> onSave) {

            m_modeDropdown = new Dropdown<string>(
                (option, index, isInDropdown) =>
                    new Label(new LocStrFormatted(option ?? "")));
            m_modeDropdown.SetOptions(new[] { ModeCustom, ModeProductExists });
            m_modeDropdown.OnValueChanged((v, _) => {
                m_currentMode = string.IsNullOrEmpty(v) ? ModeCustom : v;
                applyMode();
            });
            m_modeDropdown.MinWidth(180.px());
            AddField("condition kind", m_modeDropdown);

            m_customField = new TextField().Class(Cls.fontMonospace);
            m_customField.OnValueChanged(v => m_customText = v ?? "");
            m_customRow = new Column {
                new Label(new LocStrFormatted("condition (Python expression)"))
                    .Class(Cls.groupHeader),
                m_customField
            }.Class(Cls.group).Gap(4.px()).Padding(4.px());
            m_customRow.AlignItemsStretch();
            Add(m_customRow);

            m_productPicker = new ProtoPicker<ProductProto>(
                protosDb,
                getId: () => m_productId,
                setId: id => m_productId = id,
                emptyLabel: new LocStrFormatted("(pick product…)"),
                title: new LocStrFormatted("Pick product"));
            m_productRow = new Column {
                new Label(new LocStrFormatted("product (rendered as product_exist(\"…\"))"))
                    .Class(Cls.groupHeader),
                m_productPicker
            }.Class(Cls.group).Gap(4.px()).Padding(4.px());
            m_productRow.AlignItemsStretch();
            Add(m_productRow);

            ButtonText saveBtn = new ButtonText(
                new LocStrFormatted("Save condition"),
                () => {
                    if (value == null) return;
                    string newCondition = m_currentMode == ModeProductExists
                        ? FormatProductExistsCondition(m_productId)
                        : (m_customText ?? "");
                    onSave(value, newCondition);
                });
            Add(saveBtn);

            OnRefresh(() => {
                string detected = TryParseProductExistsCondition(value.Condition);
                m_currentMode = detected != null ? ModeProductExists : ModeCustom;
                m_customText  = detected != null ? "" : (value.Condition ?? "");
                m_productId   = detected;
                m_modeDropdown.SetValue(m_currentMode);
                m_customField.Text(m_customText);
                m_productPicker.RefreshDisplay();
                applyMode();
            });
        }

        private void applyMode() {
            m_customRow.Visible(m_currentMode == ModeCustom);
            m_productRow.Visible(m_currentMode == ModeProductExists);
        }

        /// Detect the <c>product_exist("ProductId")</c> shape and return the
        /// inner id; null for anything else (which keeps the editor in Custom
        /// mode). Accepts single or double quotes and tolerates whitespace
        /// inside the parens.
        public static string TryParseProductExistsCondition(string condition) {
            if (string.IsNullOrEmpty(condition)) return null;
            string s = condition.Trim();
            const string prefix = "product_exist(";
            if (!s.StartsWith(prefix) || !s.EndsWith(")")) return null;
            string inner = s.Substring(prefix.Length, s.Length - prefix.Length - 1).Trim();
            if (inner.Length < 2) return null;
            char q = inner[0];
            if (q != '"' && q != '\'') return null;
            if (inner[inner.Length - 1] != q) return null;
            return inner.Substring(1, inner.Length - 2);
        }

        public static string FormatProductExistsCondition(string productId) {
            return "product_exist(\"" + (productId ?? "") + "\")";
        }
    }
}
