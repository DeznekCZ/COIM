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

        public RecipeDefEditor(ProtosDb protosDb, PackModel packModel) {
            m_protosDb  = protosDb;
            m_packModel = packModel;

            AddIdField();
            AddNameField();
            AddStringField("description",
                getter: d => d.Description,
                setter: (d, v) => d.Description = v,
                multiline: true);

            AddNullableIntField("duration (seconds, blank = default)",
                getter: d => d.DurationSeconds,
                setter: (d, v) => d.DurationSeconds = v);
            AddNullableIntField("power (percent, blank = default)",
                getter: d => d.PowerPercent,
                setter: (d, v) => d.PowerPercent = v);

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
            OnRefresh(rebuildDynamic);
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
            Column ingredientsCol = (Column)RecipeFormParts.BuildProductListEditor(
                m_protosDb,
                label: "ingredients", isInput: true, machine: machine,
                list: r.Ingredients,
                onChanged: refreshValidation);
            Column productsCol = (Column)RecipeFormParts.BuildProductListEditor(
                m_protosDb,
                label: "products", isInput: false, machine: machine,
                list: r.Products,
                onChanged: refreshValidation);
            ingredientsCol.FlexGrow(1f).FlexBasis(Percent.Zero);
            productsCol.FlexGrow(1f).FlexBasis(Percent.Zero);
            Row ioRow = new Row { ingredientsCol, productsCol };
            ioRow.Gap(4.pt()).AlignItemsStart();
            m_dynamicCol.Add(ioRow);

            m_dynamicCol.Add(validation);
        }
    }
}
