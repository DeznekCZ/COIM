using CustomAssets.Editor.Model;
using CustomAssets.Ui.Components;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Editors {

    /// Editor for one product sub-action inside a `with edit_recipe(...)` block:
    /// <c>set_ingredient</c> / <c>set_product</c> / <c>remove_ingredient</c> /
    /// <c>remove_product</c>. A single editor covers all four — the def's
    /// <see cref="RecipeProductActionDef.IsInput"/> / <c>IsRemoval</c> flags are
    /// fixed when the action is created (by the add menu) and are shown read-only
    /// here, so the modder edits the product and, for a set, the amount.
    ///
    /// The quantity field only appears for a set action; a removal names a product
    /// and nothing else.
    public sealed class RecipeProductActionDefEditor : DefEditor<RecipeProductActionDef> {

        private readonly ProductExpressionPicker m_product;

        public RecipeProductActionDefEditor(ProtosDb protosDb) {
            // Read-only reminder of which of the four verbs this is — the add menu
            // chose it, and changing verb means a different statement, so it is not
            // editable here.
            Add(new Label(new LocStrFormatted(verbHint()))
                .Class(Cls.fontMonospace).TinyFontSize());

            m_product = AddField("product",
                new ProductExpressionPicker(
                    protosDb,
                    getExpression: () => value?.ProductId,
                    setExpression: v => { if (value != null) value.ProductId = v; },
                    emptyLabel: new LocStrFormatted("(pick a product...)")),
                onRefresh: () => m_product.RefreshDisplay());

            // Amount — set actions only. AddNullableIntField accepts a bare number
            // or blank; the emitter wraps it in Quantity(N). A removal ignores it.
            AddNullableIntField("amount (set actions only; the new quantity)",
                getter: d => d?.Quantity,
                setter: (d, v) => { if (d != null) d.Quantity = v; });

            AddCommentField();
        }

        // The read-only header can't read `value` at construction time (it binds
        // later), so refresh the hint on each rebind.
        private string verbHint() {
            if (value == null) return "edit_recipe sub-action";
            string verb = (value.IsRemoval ? "remove_" : "set_")
                + (value.IsInput ? "ingredient" : "product");
            return value.IsRemoval
                ? verb + "(product) — drops this "
                    + (value.IsInput ? "ingredient" : "product") + " from the recipe."
                : verb + "(product, quantity) — sets the amount of an EXISTING "
                    + (value.IsInput ? "ingredient" : "product")
                    + " (it does not add a new one).";
        }
    }

    /// Editor for <c>unbind_recipe(machine, research=…)</c> inside a
    /// `with edit_recipe(...)` block — detaches the recipe from a machine and
    /// removes the research unlock(s) for that pair. Research is optional; leaving
    /// it blank cleans up every node that unlocked the pair (validated against the
    /// protos at runtime).
    public sealed class UnbindRecipeDefEditor : DefEditor<UnbindRecipeDef> {

        private readonly MachineIdPicker m_machine;
        private readonly ResearchIdPicker m_research;

        public UnbindRecipeDefEditor(PackModel model, ProtosDb protosDb) {
            Add(new Label(new LocStrFormatted(
                    "Detaches the recipe from this machine and removes the research unlock "
                    + "for that pair. A save that had this recipe on the machine drops it on "
                    + "load — this affects existing savegames, not just new ones."))
                .Class(Cls.fontMonospace).TinyFontSize());

            m_machine = AddField("machine (to detach from)",
                new MachineIdPicker(
                    model, protosDb,
                    getId: () => value?.MachineId,
                    setId: v => { if (value != null) value.MachineId = v; },
                    getOwnerDef: () => value,
                    title: new LocStrFormatted("Pick machine")),
                onRefresh: () => m_machine.RefreshDisplay());

            m_research = AddField("research (optional — narrows unlock removal to one node)",
                new ResearchIdPicker(
                    model, protosDb, ownerDef: null,
                    getId: () => value?.ResearchId,
                    setId: v => { if (value != null) value.ResearchId = v; },
                    title: new LocStrFormatted("Pick research")),
                onRefresh: () => m_research.RefreshDisplay());

            AddCommentField();
        }
    }
}
