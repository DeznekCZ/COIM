using System;
using System.Collections.Generic;
using CustomAssets.Editor.Model;
using CustomAssets.Ui.Components;
using Mafi;
using Mafi.Core.Factory.Machines;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Editors {

    /// <summary>
    /// Editor for a <see cref="BindRecipeDef"/> — the <c>bind_recipe(...)</c>
    /// statement that attaches a recipe to a machine (0.3.0 recipe split, where
    /// a recipe is machine-less and each machine binding carries its own
    /// duration + port mapping).
    ///
    /// A recipe usually has several of these (one per machine); the editor
    /// tree surfaces them as the recipe's "machines" sublist, and each opens
    /// this form. Layout mirrors the unlock editors: id pickers first, then the
    /// per-binding scalars, then a machine-aware port list rebuilt whenever the
    /// machine changes.
    /// </summary>
    public sealed class BindRecipeDefEditor : DefEditor<BindRecipeDef> {

        private readonly ProtosDb m_protosDb;
        private readonly PackModel m_packModel;

        private readonly RecipeIdPicker m_recipe;
        private readonly MachineIdPicker m_machine;
        private readonly ResearchIdPicker m_research;
        private readonly TextField m_unlockMachine;

        private readonly Column m_portsCol;

        /// <paramref name="showRecipePicker"/>: false when the editor is hosted
        /// inside a recipe's machine TAB — the recipe is the parent, so picking
        /// it there would be redundant (and wrong to change).
        public BindRecipeDefEditor(PackModel model, ProtosDb protosDb, bool showRecipePicker = true) {
            m_packModel = model;
            m_protosDb  = protosDb;

            if (showRecipePicker) {
                m_recipe = AddField("recipe id",
                    new RecipeIdPicker(
                        model, protosDb,
                        getId: () => value?.RecipeId,
                        setId: v => { if (value != null) value.RecipeId = v; },
                        variableResolver: id => {
                            if (value == null || string.IsNullOrEmpty(id)) return null;
                            return EditorHelpers.VariableResolverFor(value)?.Invoke(id);
                        },
                        title: new LocStrFormatted("Pick recipe to bind")),
                    onRefresh: () => m_recipe.RefreshDisplay());
            }

            // Machine change invalidates the port dropdowns, so rebuild the
            // port section from the picker's setId.
            m_machine = AddField("machine",
                new MachineIdPicker(
                    model, protosDb,
                    getId: () => value?.MachineId,
                    setId: id => { if (value != null) value.MachineId = id; rebuildPorts(); },
                    getOwnerDef: () => value,
                    title: new LocStrFormatted("Pick machine"),
                    emptyLabel: new LocStrFormatted("(pick machine…)")),
                onRefresh: () => m_machine.RefreshDisplay());

            AddExpressionIntField("duration (seconds, blank = default 60)",
                getter: d => d.DurationSeconds,
                setter: (d, v) => d.DurationSeconds = v,
                expressionGetter: d => d.DurationExpression,
                expressionSetter: (d, v) => d.DurationExpression = v);
            AddNullableIntField("multiplier (throughput, blank = 1)",
                getter: d => d.Multiplier,
                setter: (d, v) => d.Multiplier = v);
            AddNullableIntField("min partial utilization (percent, blank = none)",
                getter: d => d.MinPartialUtilizationPercent,
                setter: (d, v) => d.MinPartialUtilizationPercent = v);

            m_research = AddField("research (optional - pick (none) to clear)",
                new ResearchIdPicker(
                    model, protosDb, ownerDef: null,
                    getId: () => value?.ResearchId,
                    setId: id => { if (value != null) value.ResearchId = id; },
                    title: new LocStrFormatted("Pick research")),
                onRefresh: () => m_research.RefreshDisplay());

            // Blank = the API default (true), so an untouched binding emits no
            // argument at all. Only a typed "false" opts out, which is also the
            // only case the emitter writes.
            m_unlockMachine = AddField(
                "unlock_machine (blank = also grant the machine; false = unlock the recipe only)",
                new TextField().OnValueChanged(v => {
                    if (value == null) return;
                    value.UnlockMachine = (v ?? "").Trim().ToLowerInvariant() != "false";
                }),
                onRefresh: () => m_unlockMachine.Text(value != null && !value.UnlockMachine ? "false" : ""));

            // Machine-aware port map. Entries are optional overrides — products
            // not listed (or left "*") auto-resolve to any compatible port on
            // the machine at bind time.
            m_portsCol = new Column();
            m_portsCol.AlignItemsStretch().Gap(4.px());
            Add(m_portsCol);
            OnRefresh(rebuildPorts);
        }

        /// The recipe this binding belongs to — its ingredient/product lists are
        /// what the port dropdowns offer. Found by containment first (bindings are
        /// nested on their recipe), falling back to an id match for a stand-alone
        /// bind_recipe against a recipe declared elsewhere.
        private RecipeDef findOwningRecipe() {
            if (value == null) return null;
            // Direct link first: the model this editor was built with can be a
            // stale instance (editors are cached per def-type, a save+reload swaps
            // the model), in which case searching it finds nothing and the port
            // dropdowns come up empty — no products to offer, no "(auto) → …" hints.
            if (value.OwnerRecipe != null) return value.OwnerRecipe;
            if (m_packModel?.Definitions == null) return null;
            if (string.IsNullOrEmpty(value.RecipeId)) return null;
            foreach (DefBase d in m_packModel.Definitions) {
                if (d is RecipeDef r2 && r2.RecipeId == value.RecipeId) return r2;
            }
            return null;
        }

        private void rebuildPorts() {
            m_portsCol.Clear();
            if (value == null) return;
            if (value.Ports == null) value.Ports = new List<PortMapRef>();

            MachineProto machine = RecipeFormParts.ResolveMachine(m_protosDb, value.MachineId);
            m_portsCol.Add(RecipeFormParts.BuildMachinePortsView(machine, value.MachineId));

            // Port-centric assignment: one row per machine port, each picking which
            // of the recipe's products travels through it.
            RecipeDef owner = findOwningRecipe();
            m_portsCol.Add(RecipeFormParts.BuildPortAssignmentEditor(
                m_protosDb,
                machine: machine,
                recipeInputs:  owner?.Ingredients,
                recipeOutputs: owner?.Products,
                portMap: value.Ports,
                onChanged: rebuildPorts));
        }
    }
}
