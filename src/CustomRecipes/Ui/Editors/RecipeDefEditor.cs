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
    /// Editor body for a <see cref="RecipeDef"/> — id, name, description,
    /// duration, power, research, machine picker, plus the machine-dependent
    /// section (port-layout view, ingredient/product lists, validation
    /// column). Bold header, comment field, source label, and per-entry
    /// Save button are appended by the dispatcher (matches the pattern used
    /// for every other typed def editor), so they're intentionally NOT
    /// added here.
    ///
    /// Stable scalar fields use the usual <c>AddField</c> helpers and
    /// rebind in place via <see cref="DefEditor{T}.Value"/>. The
    /// machine-dependent section lives in a nested column that the editor
    /// clears and rebuilds whenever the machine changes or the bound
    /// value swaps.
    /// </summary>
    public sealed class RecipeDefEditor : NamedDefEditor<RecipeDef> {

        private readonly ProtosDb m_protosDb;
        private readonly PackModel m_packModel;

        private readonly Column m_dynamicCol;

        /// Host for the machine-binding list.
        private readonly Column m_machinesCol;

        /// Raised after a binding is added/removed so the window can rebuild the
        /// tree (bindings are listed there as children of this recipe).
        private readonly Action m_onBindingsChanged;

        public RecipeDefEditor(ProtosDb protosDb, PackModel packModel,
                Action onBindingsChanged = null) {
            m_protosDb  = protosDb;
            m_packModel = packModel;
            m_onBindingsChanged = onBindingsChanged;

            AddIdField();
            AddNameField();
            AddStringField("description",
                getter: d => d.Description,
                setter: (d, v) => d.Description = v,
                multiline: true);

            // 0.3.0: duration is NOT a recipe property — it belongs to each
            // machine binding and is edited per machine tab (see BindRecipeDefEditor).
            // A legacy recipe's loaded DurationSeconds is migrated into a binding
            // on save. Only recipe-level fields live on this form.
            AddNullableIntField("power (percent, blank = default)",
                getter: d => d.PowerPercent,
                setter: (d, v) => d.PowerPercent = v);

            // Comma-separated rather than a list widget: entries are free text by
            // nature (they name recipes that no longer exist, so nothing can offer
            // or validate them) and a supersession list is normally two or three
            // ids written once. A row-per-entry editor would cost more than it buys.
            AddOptionalStringField(
                "replaces (comma-separated ids this recipe supersedes — keep forever)",
                getter: d => d?.Replaces == null || d.Replaces.Count == 0
                    ? null
                    : string.Join(", ", d.Replaces),
                setter: (d, v) => d.Replaces = EditorHelpers.SplitIdList(v),
                monospace: true);

            // Research picker rebinds against value's SourceFileVariables, so
            // a fresh picker is constructed on each Value() swap.
            Column researchHolder = new Column();
            researchHolder.AlignItemsStretch();
            AddField("research (optional - pick (none) to clear)",
                researchHolder,
                onRefresh: () => {
                    researchHolder.Clear();
                    if (value == null) return;
                    researchHolder.Add(new ResearchIdPicker(
                        m_packModel, m_protosDb, ownerDef: value,
                        getId: () => value?.ResearchId,
                        setId: id => { if (value != null) value.ResearchId = id; },
                        title: new LocStrFormatted("Pick research")));
                });

            // Machine picker - changing the machine invalidates the port
            // dropdowns on every ingredient/product row, so rebuildDynamic
            // is called from the picker's setId. MachineIdPicker (vs. the
            // raw ProtoPicker) surfaces this pack's own BuildMachineDefs
            // alongside the game machines, and prefers a variable binding
            // when the source file already has
            // `myMachine = build_machine(...)` earlier — the recipe then
            // stores `myMachine` and the emitter writes it bare.
            MachineIdPicker machinePicker = new MachineIdPicker(
                m_packModel, m_protosDb,
                getId: () => value?.MachineId,
                setId: id => {
                    if (value != null) value.MachineId = id;
                    rebuildDynamic();
                },
                getOwnerDef: () => value,
                title: new LocStrFormatted("Pick machine"),
                emptyLabel: new LocStrFormatted("(pick a machine...)"));
            AddField("machine", machinePicker,
                onRefresh: () => machinePicker.RefreshDisplay());

            // Dynamic section (port layout, validation, ingredient + product
            // lists). Added once to the editor body; cleared + repopulated
            // by rebuildDynamic.
            m_dynamicCol = new Column();
            m_dynamicCol.AlignItemsStretch().Gap(4.px());
            Add(m_dynamicCol);

            // Machine bindings, one TAB per machine. Nested on the recipe
            // (RecipeDef.Bindings), so add/remove mutate the recipe directly and
            // the emitter writes them as the `with build_recipe(...)` body.
            m_machinesCol = new Column();
            m_machinesCol.AlignItemsStretch().Gap(4.px());
            Add(m_machinesCol);

            OnRefresh(() => { rebuildDynamic(); rebuildMachines(); });
        }

        // ---- Machine tabs ----------------------------------------------------

        /// [add] / [remove] row above a tab strip; the active tab's body is a
        /// BindRecipeDefEditor bound to that machine's binding (duration, port
        /// map, multiplier, partial-utilization, research all live there).
        private void rebuildMachines() {
            m_machinesCol.Clear();
            if (value == null || m_packModel == null) return;

            // Bindings are ordinary statements in the model (each with its own
            // source range, so each saves/deletes on its own); they're linked back
            // to this recipe by BindRecipeDef.OwnerRecipe.
            List<BindRecipeDef> bindings =
                new List<BindRecipeDef>(m_packModel.BindingsOf(value));

            // Legacy recipes are already migrated by PackLoader.migrateLegacyRecipe
            // at load time, so by the time the form opens `value` is always in the
            // split shape. All that's left is to warn that the FILE on disk still
            // holds the old one-shot call until it gets saved.
            if (value.LoadedAsLegacy) {
                m_machinesCol.Add(new Label(new LocStrFormatted(
                        "⚠ old format: this recipe was written as build_recipe(machine = …). "
                        + "Its machine, duration and ports have been moved into the machine list "
                        + "below — saving rewrites the file as a `with build_recipe(...):` block."))
                    .Class(Cls.fontMonospace).TinyFontSize());
            }

            m_machinesCol.Add(new Label(new LocStrFormatted(
                "machines (select one in the tree to edit it):")));

            // One row per bound machine: name + remove. The binding's own fields
            // (duration, ports, multiplier, …) are NOT edited here — each binding
            // is listed under this recipe in the tree and opens in the editor
            // pane, the same as any other statement inside a block.
            foreach (BindRecipeDef b in bindings) {
                BindRecipeDef captured = b;
                string label = string.IsNullOrEmpty(b.MachineId) ? "(pick machine)" : b.MachineId;
                if (b.DurationSeconds.HasValue) label += "   " + b.DurationSeconds.Value + "s";

                Row row = new Row();
                row.Gap(2.pt()).AlignItemsCenter();
                row.Add(new Label(new LocStrFormatted(label)).FlexGrow(1f));
                row.Add(new ButtonText(new LocStrFormatted("✕"), () => {
                    m_packModel.Definitions.Remove(captured);
                    rebuildMachines();
                    m_onBindingsChanged?.Invoke();
                }));
                m_machinesCol.Add(row);
            }

            if (bindings.Count == 0) {
                m_machinesCol.Add(new Label(new LocStrFormatted(
                    "(none — press + add machine; the recipe then emits as a `with` block)")));
            }

            m_machinesCol.Add(new ButtonText(new LocStrFormatted("+ add machine"), () => {
                // A new binding is a pending statement: it goes into the model
                // straight away so it shows in the tree under this recipe and can
                // be edited or removed before ever being written.
                //
                // WHERE it belongs depends on whether the recipe is already a
                // `with` block in the file:
                //
                //   already a block  → its body is a real scope. Key the binding
                //     into it, so the tree renders it inside the block alongside
                //     the existing bindings and saving splices it into that body.
                //
                //   not a block yet  → there is nothing to splice into. Leave it
                //     scope-less; the recipe's own save opens the block and writes
                //     this binding as its body (PackEmitter.RenderRecipe).
                //
                // Getting this wrong is invisible until save: a scope-less binding
                // on a recipe that IS a block renders nowhere, and a block-scoped
                // one on a recipe that ISN'T lands at end-of-file.
                value.EmitAsWithBlock = true;
                BindRecipeDef added = new BindRecipeDef {
                    OwnerRecipe = value,
                    RecipeId = value.RecipeId,
                    SourceFile = value.SourceFile,
                    SourceFileVariables = value.SourceFileVariables,
                };
                if (value.TryGetBlockHeaderLine(out int headerLine)) {
                    added.ScopeKey = "block:" + headerLine;
                }
                m_packModel.Definitions.Add(added);
                rebuildMachines();
                m_onBindingsChanged?.Invoke();
            }));
        }

        private void rebuildDynamic() {
            m_dynamicCol.Clear();
            if (value == null) return;

            RecipeDef r = value;
            MachineProto machine = RecipeFormParts.ResolveMachine(m_protosDb, r.MachineId);

            // Machine port layout summary.
            m_dynamicCol.Add(RecipeFormParts.BuildMachinePortsView(machine, r.MachineId));

            // Validation column - allocated up front so row edits refresh
            // it via PopulateValidationColumn instead of rebuilding the
            // whole dynamic region (which would lose focus on the field the
            // user is typing in).
            Column validation = new Column();
            validation.Gap(1.pt()).PaddingTopBottom(2.pt());
            Action refreshValidation = () =>
                RecipeFormParts.PopulateValidationColumn(m_protosDb, validation, r, machine);
            refreshValidation();

            // Side-by-side ingredients / products columns. FlexBasis(0) +
            // FlexGrow(1) on each side means both share width evenly.
            if (r.Ingredients == null) r.Ingredients = new List<ProductRef>();
            if (r.Products    == null) r.Products    = new List<ProductRef>();
            // showPort: false — a recipe's products carry no port in the split
            // model; routing is chosen per machine binding (see the machine list
            // below, and each binding's own port map).
            Column ingredientsCol = (Column)RecipeFormParts.BuildProductListEditor(
                m_protosDb,
                label: "ingredients", isInput: true, machine: machine,
                list: r.Ingredients,
                onChanged: refreshValidation, showPort: false);
            Column productsCol = (Column)RecipeFormParts.BuildProductListEditor(
                m_protosDb,
                label: "products", isInput: false, machine: machine,
                list: r.Products,
                onChanged: refreshValidation, showPort: false);
            ingredientsCol.FlexGrow(1f).FlexBasis(Percent.Zero);
            productsCol.FlexGrow(1f).FlexBasis(Percent.Zero);
            Row ioRow = new Row { ingredientsCol, productsCol };
            ioRow.Gap(4.pt()).AlignItemsStart();
            m_dynamicCol.Add(ioRow);

            m_dynamicCol.Add(validation);
        }
    }
}
