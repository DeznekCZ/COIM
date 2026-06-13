using System;
using CustomAssets.Ui.Editors;
using Mafi;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Components {

    /// <summary>
    /// Structured picker for a <c>Product("id", quantity)</c> expression
    /// — used by the generator editor's inputProduct / outputProduct
    /// fields. Two-row layout: a product picker + quantity input on top
    /// (the common case), plus a fallback raw-expression text field
    /// underneath that surfaces only when the underlying expression
    /// can't be structurally represented (variable refs, unusual
    /// constructor shapes, etc.).
    ///
    /// Edits through the picker/qty fields rewrite the bound expression
    /// to canonical <c>Product("id", N)</c> form. Edits through the raw
    /// fallback write back verbatim, preserving the modder's text. On
    /// <see cref="RefreshDisplay"/> the component re-parses the bound
    /// value and switches between structured + raw modes automatically.
    /// </summary>
    public sealed class ProductExpressionPicker : Column {

        private readonly Func<string> m_getExpression;
        private readonly Action<string> m_setExpression;

        private readonly ProtoPicker<ProductProto> m_picker;
        private readonly TextField m_qty;
        private readonly Column m_rawWrap;
        private readonly TextField m_raw;

        // Structured state derived from the current expression. The picker
        // reads getId from `m_productId`; the qty field reads from
        // `m_quantity`. Both mutators rewrite the bound expression via
        // setExpression so the model stays the canonical source of truth.
        private string m_productId;
        private int m_quantity = 1;

        // Reentrancy guard for refresh-driven field updates. Setting
        // `m_qty.Text(...)` from RefreshDisplay would otherwise fire
        // OnValueChanged → updateExpression → setExpression in a loop.
        private bool m_suppressFieldEvents;

        public ProductExpressionPicker(ProtosDb protosDb,
                Func<string> getExpression,
                Action<string> setExpression,
                bool allowNone = false,
                LocStrFormatted? emptyLabel = null) {
            m_getExpression = getExpression;
            m_setExpression = setExpression;

            m_picker = new ProtoPicker<ProductProto>(
                protosDb,
                getId: () => m_productId,
                setId: id => {
                    if (m_suppressFieldEvents) return;
                    m_productId = id;
                    updateExpression();
                },
                emptyLabel: emptyLabel ?? new LocStrFormatted("(pick a product...)"),
                title: new LocStrFormatted("Pick product"),
                allowNone: allowNone);

            m_qty = new TextField()
                .Text("1")
                .PositiveIntegersOnly()
                .ForFieldSetMinWidth(36.px())
                .Width(80.px());
            m_qty.OnValueChanged(v => {
                if (m_suppressFieldEvents) return;
                if (int.TryParse(v, out int parsed)) {
                    int clamped = parsed < 1 ? 1 : parsed;
                    m_quantity = clamped;
                    if (clamped != parsed) {
                        m_suppressFieldEvents = true;
                        try { m_qty.Text(clamped.ToString()); }
                        finally { m_suppressFieldEvents = false; }
                    }
                    updateExpression();
                }
            });

            Row pickRow = new Row {
                m_picker.FlexGrow(1f),
                m_qty
            };
            pickRow.Gap(2.pt()).AlignItemsCenter();
            Add(pickRow);

            // Raw-expression fallback. Shown only when the underlying
            // expression doesn't parse as Product("id", N) — typically a
            // variable reference or unusual constructor call. Editing it
            // bypasses the picker and writes the value verbatim.
            m_raw = new TextField()
                .Class(Cls.fontMonospace);
            m_raw.OnValueChanged(v => {
                if (m_suppressFieldEvents) return;
                m_setExpression?.Invoke(string.IsNullOrEmpty(v) ? null : v);
            });
            m_rawWrap = new Column {
                new Label(new LocStrFormatted("raw expression (used instead of picker for non-Product(...) shapes)"))
                    .TinyFontSize().Color(ColorRgba.LightGray),
                m_raw
            };
            m_rawWrap.AlignItemsStretch().Visible(false);
            Add(m_rawWrap);

            this.AlignItemsStretch();
        }

        /// Re-read the bound expression and update the picker / qty / raw
        /// fields to match. Call from the parent editor's
        /// <c>OnRefresh</c> observer whenever the bound def changes.
        public void RefreshDisplay() {
            m_suppressFieldEvents = true;
            try {
                string expr = m_getExpression?.Invoke() ?? "";
                if (RecipeFormParts.TryParseProductExpression(expr, out string pid, out int qty)) {
                    m_productId = pid;
                    m_quantity = qty < 1 ? 1 : qty;
                    m_picker.RefreshDisplay();
                    m_qty.Text(m_quantity.ToString());
                    m_raw.Text("");
                    m_rawWrap.Visible(false);
                } else {
                    m_productId = null;
                    m_quantity = 1;
                    m_picker.RefreshDisplay();
                    m_qty.Text("1");
                    m_raw.Text(expr);
                    // Only surface the raw row when there's something
                    // worth surfacing — empty expression means "no value
                    // set" and shouldn't add visual noise.
                    m_rawWrap.Visible(!string.IsNullOrEmpty(expr));
                }
            } finally {
                m_suppressFieldEvents = false;
            }
        }

        // Rewrite the bound expression to canonical Product("id", N) form
        // whenever the picker or qty field changes. Clearing the picker
        // (allowNone) sets the expression to null; otherwise we re-emit
        // even when only the quantity changed so the model stays in sync.
        private void updateExpression() {
            if (string.IsNullOrEmpty(m_productId)) {
                m_setExpression?.Invoke(null);
                return;
            }
            int q = m_quantity < 1 ? 1 : m_quantity;
            m_setExpression?.Invoke(RecipeFormParts.RenderProductExpression(m_productId, q));
            // Picker change wins over the raw fallback — hide the raw
            // row so it can't visually contradict the canonical value.
            m_suppressFieldEvents = true;
            try {
                m_raw.Text("");
                m_rawWrap.Visible(false);
            } finally {
                m_suppressFieldEvents = false;
            }
        }
    }
}
