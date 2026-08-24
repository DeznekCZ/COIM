using CustomAssets.Editor.Model;
using CustomAssets.Ui.Components;
using Mafi.Core.Factory.Machines;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Editors {

    /// <c>add_unlock_recipe(research, machine, recipe, unlock_machine=True)</c>.
    /// Research uses ResearchIdPicker; machine uses MachineIdPicker so
    /// pack-defined BuildMachineDefs surface as choices; recipe id uses
    /// the merged modded + game RecipeIdPicker.
    ///
    /// The node grants the named machine along with the recipe unless
    /// <c>unlock_machine</c> is set to false — see UnlockRecipeDef.UnlockMachine
    /// for why a recipe added to a machine the player already has wants that.
    public sealed class UnlockRecipeDefEditor : DefEditor<UnlockRecipeDef> {

        private readonly ResearchIdPicker m_research;
        private readonly MachineIdPicker m_machine;
        private readonly RecipeIdPicker m_recipe;
        private readonly TextField m_unlockMachine;

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

            // Blank = the API default (true). Type "false" to make the node
            // teach only the recipe and leave the machine alone.
            m_unlockMachine = AddField(
                "unlock_machine (blank = also grant the machine; false = unlock the recipe only)",
                new TextField().OnValueChanged(v => {
                    if (value == null) return;
                    value.UnlockMachine = (v ?? "").Trim().ToLowerInvariant() != "false";
                }),
                onRefresh: () => m_unlockMachine.Text(value != null && !value.UnlockMachine ? "false" : ""));
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

    /// <c>add_unlock_entity(research, entity)</c> — the general form of
    /// <see cref="UnlockMachineDefEditor"/>. Uses the wide
    /// <see cref="EntityIdPicker"/> so vehicles and train cars appear alongside
    /// machines and buildings.
    public sealed class UnlockEntityDefEditor : DefEditor<UnlockEntityDef> {

        private readonly ResearchIdPicker m_research;
        private readonly EntityIdPicker m_entity;

        public UnlockEntityDefEditor(PackModel model, ProtosDb protosDb) {
            m_research = AddField("research",
                new ResearchIdPicker(
                    model,
                    protosDb,
                    ownerDef: null,
                    getId: () => value?.ResearchId,
                    setId: id => { if (value != null) value.ResearchId = id; },
                    title: new LocStrFormatted("Pick research")),
                onRefresh: () => m_research.RefreshDisplay());

            m_entity = AddField("entity (machine, building, vehicle or train car)",
                new EntityIdPicker(
                    model, protosDb,
                    getId: () => value?.EntityId,
                    setId: id => { if (value != null) value.EntityId = id; },
                    getOwnerDef: () => value,
                    title: new LocStrFormatted("Pick entity to unlock"),
                    emptyLabel: new LocStrFormatted("(pick entity…)"),
                    includeAllEntities: true),
                onRefresh: () => m_entity.RefreshDisplay());
        }
    }

    /// <c>remove_unlock(research, target, machine)</c> — the counterpart to the
    /// whole <c>add_unlock_*</c> family, and one editor rather than four: what a
    /// research node unlocks is matched by proto id, so the product / machine /
    /// entity / recipe split that adding needs does not apply to removal.
    ///
    /// The target picker deliberately does not browse the prototype database.
    /// The only things worth removing are the ones the chosen node already
    /// unlocks, so <see cref="ResearchUnlockPicker"/> lists exactly those — and
    /// fills in the machine itself when the pick is a recipe unlock.
    public sealed class RemoveUnlockDefEditor : DefEditor<RemoveUnlockDef> {

        private readonly ResearchIdPicker m_research;
        private readonly ResearchUnlockPicker m_target;
        private readonly MachineIdPicker m_machine;

        public RemoveUnlockDefEditor(PackModel model, ProtosDb protosDb) {
            Add(new Label(new LocStrFormatted(
                    "Takes something back off a research node. Pick the node first — the "
                    + "target list is what that node currently unlocks. Removing a target "
                    + "the node never unlocked is a no-op with a note in the log, not an "
                    + "error."))
                .Class(Cls.fontMonospace).TinyFontSize());

            m_research = AddField("research (node to take the unlock off)",
                new ResearchIdPicker(
                    model,
                    protosDb,
                    ownerDef: null,
                    getId: () => value?.ResearchId,
                    setId: id => { if (value != null) value.ResearchId = id; },
                    title: new LocStrFormatted("Pick research")),
                onRefresh: () => m_research.RefreshDisplay());

            m_target = AddField("target (product, machine, entity or recipe to remove)",
                new ResearchUnlockPicker(
                    model, protosDb,
                    getOwnerDef: () => value,
                    getResearchId: () => value?.ResearchId,
                    getTargetId: () => value?.TargetId,
                    setTargetId: id => { if (value != null) value.TargetId = id; },
                    setMachineId: id => { if (value != null) value.MachineId = id; },
                    // Picking a recipe unlock writes the machine too, so that
                    // field has to redraw or it keeps showing the previous pick.
                    onSelected: () => m_machine?.RefreshDisplay(),
                    title: new LocStrFormatted("Pick what to remove")),
                onRefresh: () => m_target.RefreshDisplay());

            m_machine = AddField("machine (optional — only scopes a RECIPE removal to one machine)",
                new MachineIdPicker(
                    model, protosDb,
                    getId: () => value?.MachineId,
                    setId: id => { if (value != null) value.MachineId = id; },
                    getOwnerDef: () => value,
                    title: new LocStrFormatted("Pick machine"),
                    emptyLabel: new LocStrFormatted("(every machine)"),
                    allowNone: true),
                onRefresh: () => m_machine.RefreshDisplay());
        }
    }
}
