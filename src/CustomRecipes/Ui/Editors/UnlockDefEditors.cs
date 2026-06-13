using CustomAssets.Editor.Model;
using CustomAssets.Ui.Components;
using Mafi.Core.Factory.Machines;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Localization;

namespace CustomAssets.Ui.Editors {

    /// <c>add_unlock_recipe(research, machine, recipe)</c> — three id args.
    /// Research uses ResearchIdPicker; machine uses MachineIdPicker so
    /// pack-defined BuildMachineDefs surface as choices; recipe id uses
    /// the merged modded + game RecipeIdPicker.
    public sealed class UnlockRecipeDefEditor : DefEditor<UnlockRecipeDef> {

        private readonly ResearchIdPicker m_research;
        private readonly MachineIdPicker m_machine;
        private readonly RecipeIdPicker m_recipe;

        public UnlockRecipeDefEditor(PackModel model, ProtosDb protosDb) {
            m_research = AddField("research",
                new ResearchIdPicker(
                    model,
                    protosDb,
                    ownerDef: null,
                    getId: () => value?.ResearchId,
                    setId: id => { if (value != null) value.ResearchId = id; },
                    title: new LocStrFormatted("Pick research")),
                onRefresh: () => m_research.RefreshDisplay());

            m_machine = AddField("machine",
                new MachineIdPicker(
                    model, protosDb,
                    getId: () => value?.MachineId,
                    setId: id => { if (value != null) value.MachineId = id; },
                    getOwnerDef: () => value,
                    title: new LocStrFormatted("Pick machine"),
                    emptyLabel: new LocStrFormatted("(pick machine…)")),
                onRefresh: () => m_machine.RefreshDisplay());

            m_recipe = AddField("recipe id",
                new RecipeIdPicker(
                    model,
                    protosDb,
                    getId: () => value?.RecipeId,
                    setId: v => { if (value != null) value.RecipeId = v; },
                    variableResolver: id => {
                        if (value == null || string.IsNullOrEmpty(id)) return null;
                        return EditorHelpers.VariableResolverFor(value)?.Invoke(id);
                    },
                    title: new LocStrFormatted("Pick recipe to unlock")),
                onRefresh: () => m_recipe.RefreshDisplay());
        }
    }

    /// <c>add_unlock_product(research, product)</c> — two id args.
    public sealed class UnlockProductDefEditor : DefEditor<UnlockProductDef> {

        private readonly ResearchIdPicker m_research;
        private readonly ProtoPicker<ProductProto> m_product;

        public UnlockProductDefEditor(PackModel model, ProtosDb protosDb) {
            m_research = AddField("research",
                new ResearchIdPicker(
                    model,
                    protosDb,
                    ownerDef: null,
                    getId: () => value?.ResearchId,
                    setId: id => { if (value != null) value.ResearchId = id; },
                    title: new LocStrFormatted("Pick research")),
                onRefresh: () => m_research.RefreshDisplay());

            m_product = AddField("product",
                new ProtoPicker<ProductProto>(
                    protosDb,
                    getId: () => value?.ProductId,
                    setId: id => { if (value != null) value.ProductId = id; },
                    emptyLabel: new LocStrFormatted("(pick product…)"),
                    title: new LocStrFormatted("Pick product"),
                    variableResolver: id => {
                        if (value == null || string.IsNullOrEmpty(id)) return null;
                        return EditorHelpers.VariableResolverFor(value)?.Invoke(id);
                    }),
                onRefresh: () => m_product.RefreshDisplay());
        }
    }

    /// <c>add_unlock_machine(research, machine)</c> — two id args. Machine
    /// uses MachineIdPicker so pack-defined BuildMachineDefs are pickable.
    public sealed class UnlockMachineDefEditor : DefEditor<UnlockMachineDef> {

        private readonly ResearchIdPicker m_research;
        private readonly MachineIdPicker m_machine;

        public UnlockMachineDefEditor(PackModel model, ProtosDb protosDb) {
            m_research = AddField("research",
                new ResearchIdPicker(
                    model,
                    protosDb,
                    ownerDef: null,
                    getId: () => value?.ResearchId,
                    setId: id => { if (value != null) value.ResearchId = id; },
                    title: new LocStrFormatted("Pick research")),
                onRefresh: () => m_research.RefreshDisplay());

            m_machine = AddField("machine",
                new MachineIdPicker(
                    model, protosDb,
                    getId: () => value?.MachineId,
                    setId: id => { if (value != null) value.MachineId = id; },
                    getOwnerDef: () => value,
                    title: new LocStrFormatted("Pick machine"),
                    emptyLabel: new LocStrFormatted("(pick machine…)")),
                onRefresh: () => m_machine.RefreshDisplay());
        }
    }
}
