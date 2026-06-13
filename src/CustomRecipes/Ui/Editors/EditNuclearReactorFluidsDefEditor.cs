using CustomAssets.Editor.Model;
using CustomAssets.Ui.Components;
using Mafi;
using Mafi.Core.Factory.NuclearReactors;
using Mafi.Core.Ports.Io;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Editors {

    /// <c>edit_nuclear_reactor_fluids</c> editor — typed picker for the
    /// target reactor + coolant / water / steam product pickers + optional
    /// port-shape overrides. Mirrors the fluid section inside
    /// <see cref="NuclearReactorDefEditor"/> but stands alone (no full
    /// reactor clone), so the modder can patch a vanilla reactor's
    /// fluid chemistry in place.
    ///
    /// Port-letter fields (CoolantInPort / CoolantOutPort / WaterInPorts /
    /// SteamOutPorts) are intentionally not surfaced — the runtime infers
    /// them from the selected product against the target reactor's layout.
    /// The fields stay on the model so hand-edited .py files round-trip.
    public sealed class EditNuclearReactorFluidsDefEditor : DefEditor<EditNuclearReactorFluidsDef> {

        private readonly ProtosDb m_protosDb;
        private readonly PackModel m_packModel;
        private readonly Column m_reactorHolder;
        private readonly Column m_loadHolder;

        public EditNuclearReactorFluidsDefEditor(PackModel packModel, ProtosDb protosDb) {
            m_protosDb  = protosDb;
            m_packModel = packModel;

            // Reactor picker. setId rebuilds the Load-from-target button
            // beneath so the affordance toggles based on whether the
            // newly-picked reactor actually has fluid data to seed from.
            m_reactorHolder = new Column();
            m_reactorHolder.AlignItemsStretch();
            AddField("reactor (target NuclearReactorProto to patch)",
                m_reactorHolder, onRefresh: () => {
                    m_reactorHolder.Clear();
                    if (value == null) return;
                    m_reactorHolder.Add(new ProtoPicker<NuclearReactorProto>(
                        m_protosDb,
                        getId: () => value?.ReactorId,
                        setId: id => {
                            if (value != null) value.ReactorId = id;
                            rebuildLoadButton();
                        },
                        title: new LocStrFormatted("Pick target reactor"),
                        variableResolver: EditorHelpers.VariableResolverFor(value)));
                });

            // Load-existing-from-target affordance. Sits between the
            // reactor picker and the override fields — clicking it
            // populates every product/quantity slot from the target's
            // current values so the modder edits deltas. Hidden when no
            // reactor is selected.
            m_loadHolder = new Column();
            m_loadHolder.AlignItemsStretch();
            AddField("seed from target", m_loadHolder, onRefresh: rebuildLoadButton);

            // ---- Coolant / water / steam override fields ---------------
            // Every field defaults null → inherit from target. Product
            // pickers use allowNone so the modder can clear back to
            // "inherit"; port-shape pickers default null too and the
            // runtime infers from the coolant product type when omitted.

            addProductPicker("coolantIn (blank = inherit)",
                () => value?.CoolantInId,
                id => { if (value != null) value.CoolantInId = string.IsNullOrEmpty(id) ? null : id; },
                "Pick coolant IN product");

            addPortShapePicker("coolantInPortShape (blank = infer from coolant product)",
                () => value?.CoolantInPortShape,
                id => { if (value != null) value.CoolantInPortShape = string.IsNullOrEmpty(id) ? null : id; },
                "Pick coolant-IN port shape");

            addProductPicker("coolantOut (blank = inherit)",
                () => value?.CoolantOutId,
                id => { if (value != null) value.CoolantOutId = string.IsNullOrEmpty(id) ? null : id; },
                "Pick coolant OUT product");

            addPortShapePicker("coolantOutPortShape (blank = infer from coolant product)",
                () => value?.CoolantOutPortShape,
                id => { if (value != null) value.CoolantOutPortShape = string.IsNullOrEmpty(id) ? null : id; },
                "Pick coolant-OUT port shape");

            addProductPicker("waterIn product (blank = inherit)",
                () => value?.WaterInProductId,
                id => { if (value != null) value.WaterInProductId = string.IsNullOrEmpty(id) ? null : id; },
                "Pick water-IN product");

            AddNullableIntField("waterIn quantity (per power level; blank = inherit)",
                d => d.WaterInQuantity, (d, v) => d.WaterInQuantity = v);

            addProductPicker("steamOut product (blank = inherit)",
                () => value?.SteamOutProductId,
                id => { if (value != null) value.SteamOutProductId = string.IsNullOrEmpty(id) ? null : id; },
                "Pick steam-OUT product");

            AddNullableIntField("steamOut quantity (per power level; blank = inherit)",
                d => d.SteamOutQuantity, (d, v) => d.SteamOutQuantity = v);

            AddCommentField();
        }

        // ProductProto picker as a labelled field. The inner picker's
        // get/setId closures are recreated on every Value() swap so they
        // bind to the currently-bound def.
        private void addProductPicker(string label,
                System.Func<string> getId, System.Action<string> setId, string pickerTitle) {
            Column holder = new Column();
            holder.AlignItemsStretch();
            AddField(label, holder, onRefresh: () => {
                holder.Clear();
                if (value == null) return;
                holder.Add(new ProtoPicker<ProductProto>(
                    m_protosDb,
                    getId: getId,
                    setId: id => { setId(id); value.Dirty = true; },
                    title: new LocStrFormatted(pickerTitle),
                    variableResolver: EditorHelpers.VariableResolverFor(value),
                    allowNone: true));
            });
        }

        // IoPortShapeProto picker as a labelled field. Same closure-rebind
        // pattern as addProductPicker.
        private void addPortShapePicker(string label,
                System.Func<string> getId, System.Action<string> setId, string pickerTitle) {
            Column holder = new Column();
            holder.AlignItemsStretch();
            AddField(label, holder, onRefresh: () => {
                holder.Clear();
                if (value == null) return;
                holder.Add(new ProtoPicker<IoPortShapeProto>(
                    m_protosDb,
                    getId: getId,
                    setId: id => { setId(id); value.Dirty = true; },
                    title: new LocStrFormatted(pickerTitle),
                    variableResolver: EditorHelpers.VariableResolverFor(value),
                    allowNone: true));
            });
        }

        // Toggle the load-from-target affordance based on the currently
        // selected reactor. Hidden when no reactor / unresolvable id; shown
        // otherwise with a single button that copies the target's current
        // fluid data into the def's override fields and forces a full
        // editor refresh so every picker reflects the new values.
        private void rebuildLoadButton() {
            m_loadHolder.Clear();
            if (value == null) return;
            NuclearReactorProto target = string.IsNullOrEmpty(value.ReactorId)
                ? null
                : RecipeFormParts.ResolveProtoSafe<NuclearReactorProto>(m_protosDb, value.ReactorId);
            if (target == null) return;

            m_loadHolder.Add(new ButtonText(
                new LocStrFormatted("⇩ Load existing fluids from target"),
                () => {
                    if (value == null || target == null) return;
                    loadFluidsFromProto(target, value);
                    value.Dirty = true;
                    // Force every field to refresh against the freshly-
                    // populated def — Value() rebinds the whole form so
                    // ProtoPickers / int fields reflect the new strings.
                    Value(value);
                })
                .Tooltip(new LocStrFormatted(
                    "Populate coolant / water / steam pickers from the target reactor's current values. Edit deltas after.")));
        }

        // Copy the target reactor's current fluid wiring into the def's
        // override slots. Port-shape overrides are not loaded — they're
        // only meaningful when the modder is changing a product to a
        // different shape, so leaving them blank preserves "infer from
        // product" semantics. Port-letter fields (CoolantInPort etc.)
        // ARE loaded because they're already on the model and a
        // round-trip-safe representation of the target state, even
        // though the editor doesn't surface them.
        private static void loadFluidsFromProto(
                NuclearReactorProto target, EditNuclearReactorFluidsDef into) {
            into.CoolantInId  = target.CoolantIn?.Id.Value;
            into.CoolantOutId = target.CoolantOut?.Id.Value;
            into.CoolantInPort  = target.CoolantInPort  != default(char)
                ? target.CoolantInPort.ToString()  : null;
            into.CoolantOutPort = target.CoolantOutPort != default(char)
                ? target.CoolantOutPort.ToString() : null;
            into.WaterInProductId  = target.WaterInPerPowerLevel.Product?.Id.Value;
            into.WaterInQuantity   = target.WaterInPerPowerLevel.Quantity.Value;
            into.SteamOutProductId = target.SteamOutPerPowerLevel.Product?.Id.Value;
            into.SteamOutQuantity  = target.SteamOutPerPowerLevel.Quantity.Value;
            into.WaterInPorts      = target.WaterInPorts;
            into.SteamOutPorts     = target.SteamOutPorts;
        }
    }
}
