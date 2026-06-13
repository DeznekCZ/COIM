using System.Collections.Generic;
using CustomAssets.Editor.Model;
using CustomAssets.Ui.Components;
using Mafi;
using Mafi.Core.Factory.Machines;
using Mafi.Core.Factory.Recipes;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Editors {

    /// <summary>
    /// Editor body for an <see cref="EditRecipeDef"/> — the
    /// <c>edit_recipe(recipe, ...)</c> overlay that patches a few fields
    /// on an existing recipe instead of replacing it. Fewer fields than
    /// the full recipe form; no validation column (the original recipe
    /// already passed registration, the overlay only sets a subset).
    ///
    /// Header, comment field, source label, and Save button are appended
    /// by the dispatcher, so they aren't added here.
    ///
    /// Below the picker, the editor shows a live RecipeUi card for the
    /// currently-picked source recipe plus a "Fill from source" button.
    /// The fill action copies every patchable field (duration, ingredients,
    /// products, machine, power) from the source into the EditRecipeDef in
    /// one shot — manual rather than automatic, so modders writing a
    /// partial overlay ("just halve the duration") aren't surprised by an
    /// emit that suddenly carries every field of the source recipe.
    /// </summary>
    public sealed class EditRecipeDefEditor : DefEditor<EditRecipeDef> {

        private readonly ProtosDb m_protosDb;
        private readonly PackModel m_packModel;

        private readonly Label m_sourcePreviewLabel;
        private readonly Column m_sourcePreviewCol;
        private readonly ButtonText m_fillBtn;
        private readonly Column m_dynamicCol;

        public EditRecipeDefEditor(ProtosDb protosDb, PackModel packModel) {
            m_protosDb  = protosDb;
            m_packModel = packModel;

            Column recipeHolder = new Column();
            recipeHolder.AlignItemsStretch();
            AddField("recipe", recipeHolder, onRefresh: () => {
                recipeHolder.Clear();
                if (value == null) return;
                recipeHolder.Add(new RecipeIdPicker(
                    m_packModel, m_protosDb,
                    getId: () => value?.RecipeId,
                    setId: v => {
                        if (value != null) value.RecipeId = v;
                        refreshSourcePreview();
                    },
                    variableResolver: EditorHelpers.VariableResolverFor(value),
                    title: new LocStrFormatted("Pick recipe to edit")));
            });

            // Source preview + fill-from-source button. Sits directly under
            // the picker so the modder sees what they just picked and can
            // pull its fields in with one click if they want a full
            // overlay rather than a partial one.
            m_sourcePreviewLabel = new Label(new LocStrFormatted("source preview")).TinyFontSize();
            m_sourcePreviewCol = new Column();
            m_sourcePreviewCol.AlignItemsStretch().Gap(2.px());
            m_fillBtn = new ButtonText(
                new LocStrFormatted("Fill fields from source"),
                onFillFromSource);
            Column previewBlock = new Column {
                m_sourcePreviewLabel,
                m_sourcePreviewCol,
                m_fillBtn,
            };
            previewBlock.Gap(3.px()).AlignItemsStretch();
            AddField("original (source recipe)", previewBlock,
                onRefresh: refreshSourcePreview);

            AddNullableIntField("duration (seconds; blank = unchanged)",
                getter: d => d.DurationSeconds,
                setter: (d, v) => d.DurationSeconds = v);

            // Machine override picker. allowNone so the modder can leave
            // the original recipe's machine in place. MachineIdPicker so
            // pack-defined BuildMachineDefs surface as choices alongside
            // game machines.
            MachineIdPicker machinePicker = new MachineIdPicker(
                m_packModel, m_protosDb,
                getId: () => value?.MachineId,
                setId: id => {
                    if (value != null) value.MachineId = id;
                    rebuildDynamic();
                },
                getOwnerDef: () => value,
                title: new LocStrFormatted("Pick machine"),
                emptyLabel: new LocStrFormatted("(leave unchanged)"),
                allowNone: true);
            AddField("machine (optional override)", machinePicker,
                onRefresh: () => machinePicker.RefreshDisplay());

            Column researchHolder = new Column();
            researchHolder.AlignItemsStretch();
            AddField("research (optional)", researchHolder, onRefresh: () => {
                researchHolder.Clear();
                if (value == null) return;
                researchHolder.Add(new ResearchIdPicker(
                    m_packModel, m_protosDb, ownerDef: value,
                    getId: () => value?.ResearchId,
                    setId: id => { if (value != null) value.ResearchId = id; },
                    title: new LocStrFormatted("Pick research")));
            });

            AddNullableIntField("power (percent; blank = unchanged)",
                getter: d => d.PowerPercent,
                setter: (d, v) => d.PowerPercent = v);

            m_dynamicCol = new Column();
            m_dynamicCol.AlignItemsStretch().Gap(4.px());
            Add(m_dynamicCol);
            OnRefresh(rebuildDynamic);
        }

        // ---- Source preview --------------------------------------------------

        // Re-renders the RecipeUi card under the picker for the currently
        // selected source recipe. Modded recipes in the current pack take
        // priority (the modder is more likely to be patching something
        // they themselves authored than a vanilla recipe). Game recipes
        // come from ProtosDb with the standard typed-ref fallback. When
        // neither resolves we hide the fill button and show a hint.
        private void refreshSourcePreview() {
            m_sourcePreviewCol.Clear();
            if (value == null || string.IsNullOrEmpty(value.RecipeId)) {
                m_sourcePreviewLabel.Value(new LocStrFormatted(
                    "(pick a recipe to see its original values)"));
                m_fillBtn.Visible(false);
                return;
            }

            RecipeDef moddedSrc = findModdedSource(value.RecipeId);
            if (moddedSrc != null) {
                m_sourcePreviewLabel.Value(new LocStrFormatted(
                    (moddedSrc.Name ?? moddedSrc.RecipeId) + " — this pack"));
                RecipeUi ui = RecipePreviewBuilder.BuildForModded(moddedSrc, m_protosDb);
                if (ui != null) m_sourcePreviewCol.Add(ui);
                m_fillBtn.Visible(true);
                return;
            }

            RecipeProto gameSrc = findGameSource(value.RecipeId);
            if (gameSrc != null) {
                m_sourcePreviewLabel.Value(new LocStrFormatted(
                    gameSrc.Strings.Name.TranslatedString + " — game recipe"));
                m_sourcePreviewCol.Add(RecipePreviewBuilder.BuildForGame(gameSrc));
                m_fillBtn.Visible(true);
                return;
            }

            m_sourcePreviewLabel.Value(new LocStrFormatted(
                "(cannot resolve '" + value.RecipeId
                + "' — no preview / fill available)"));
            m_fillBtn.Visible(false);
        }

        // ---- Fill from source ------------------------------------------------

        // Copy every patchable field from the resolved source recipe into
        // the EditRecipeDef. Two resolution paths: modded recipe in the
        // current pack first (fields are already in our shape), then game
        // recipe via ProtosDb (extract Duration / AllInputs / AllOutputs /
        // PowerMultiplier and reverse-look the owning machine through
        // MachineProto.Recipes).
        //
        // Research is intentionally NOT prefilled — discovering which
        // research unlocks a given recipe means scanning every
        // ResearchNodeProto's Units for a RecipeUnlock that names this
        // recipe, which routinely returns more than one match (campaign +
        // sandbox unlock trees, optional unlocks). Leaving ResearchId
        // alone keeps the overlay's emit accurate; the modder can set it
        // explicitly with the research picker if needed.
        private void onFillFromSource() {
            if (value == null || string.IsNullOrEmpty(value.RecipeId)) return;

            RecipeDef moddedSrc = findModdedSource(value.RecipeId);
            if (moddedSrc != null) {
                value.DurationSeconds = moddedSrc.DurationSeconds;
                value.Ingredients     = cloneRefs(moddedSrc.Ingredients);
                value.Products        = cloneRefs(moddedSrc.Products);
                value.MachineId       = moddedSrc.MachineId;
                value.PowerPercent    = moddedSrc.PowerPercent;
                value.Dirty = true;
                Value(value);
                return;
            }

            RecipeProto gameSrc = findGameSource(value.RecipeId);
            if (gameSrc == null) return;

            value.DurationSeconds = gameSrc.Duration.SecondsFloored;
            value.Ingredients     = refsFromGameProducts(gameSrc.AllInputs);
            value.Products        = refsFromGameProducts(gameSrc.AllOutputs);
            value.MachineId       = findOwningMachineId(gameSrc);
            // PowerMultiplier is a Percent — IntegerPart maps a `Percent`
            // that stores 100% back to the int 100 the API takes as
            // `power=100`.
            value.PowerPercent    = gameSrc.PowerMultiplier.IntegerPart;
            value.Dirty = true;
            Value(value);
        }

        // ---- Lookups ---------------------------------------------------------

        private RecipeDef findModdedSource(string id) {
            if (m_packModel?.Recipes == null) return null;
            foreach (RecipeDef r in m_packModel.Recipes) {
                if (r.RecipeId == id) return r;
            }
            return null;
        }

        private RecipeProto findGameSource(string id) {
            if (m_protosDb == null) return null;
            Option<RecipeProto> direct = m_protosDb.Get<RecipeProto>(new Proto.ID(id));
            if (direct.HasValue) return direct.Value;
            string resolved = TypedRefResolver.ResolveOrNull(id);
            if (!string.IsNullOrEmpty(resolved)) {
                Option<RecipeProto> byPath = m_protosDb.Get<RecipeProto>(new Proto.ID(resolved));
                if (byPath.HasValue) return byPath.Value;
            }
            return null;
        }

        // Reverse-lookup: which machine's Recipes collection contains this
        // recipe? COI binds recipes to machines via MachineProto.Recipes,
        // so a forward iteration is the canonical way. Returns the first
        // hit — recipes are normally owned by a single machine. Null if
        // none owns it (rare; can happen for orphaned game recipes).
        private string findOwningMachineId(RecipeProto recipe) {
            if (m_protosDb == null || recipe == null) return null;
            foreach (MachineProto m in m_protosDb.All<MachineProto>()) {
                foreach (RecipeProto r in m.Recipes) {
                    if (r.Id.Value == recipe.Id.Value) return m.Id.Value;
                }
            }
            return null;
        }

        private static List<ProductRef> cloneRefs(List<ProductRef> source) {
            List<ProductRef> result = new List<ProductRef>();
            if (source == null) return result;
            foreach (ProductRef p in source) {
                result.Add(new ProductRef(p.ProductId, p.Quantity, p.Port));
            }
            return result;
        }

        private static List<ProductRef> refsFromGameProducts<T>(
                Mafi.Collections.ImmutableCollections.ImmutableArray<T> products)
                where T : RecipeProduct {
            List<ProductRef> result = new List<ProductRef>();
            foreach (T p in products) {
                // Ports.Length == 1 → use that single port name; otherwise
                // leave the port wildcarded (null → "*") so the overlay
                // keeps the recipe routable wherever the original machine
                // accepted it.
                string port = p.Ports.Length == 1 ? p.Ports[0].Name.ToString() : null;
                result.Add(new ProductRef(p.Product.Id.Value, p.Quantity.Value, port));
            }
            return result;
        }

        // ---- Existing dynamic refresh ---------------------------------------

        private void rebuildDynamic() {
            m_dynamicCol.Clear();
            if (value == null) return;

            EditRecipeDef er = value;
            MachineProto machine = RecipeFormParts.ResolveMachine(m_protosDb, er.MachineId);

            if (er.Ingredients == null) er.Ingredients = new List<ProductRef>();
            if (er.Products    == null) er.Products    = new List<ProductRef>();

            // Side-by-side ingredients / products columns - same layout as
            // the full RecipeDefEditor so the two recipe-editing surfaces
            // read identically. FlexBasis(0) + FlexGrow(1) splits width
            // evenly regardless of content.
            Column ingredientsCol = (Column)RecipeFormParts.BuildProductListEditor(
                m_protosDb,
                label: "ingredients", isInput: true, machine: machine,
                list: er.Ingredients, onChanged: null);
            Column productsCol = (Column)RecipeFormParts.BuildProductListEditor(
                m_protosDb,
                label: "products", isInput: false, machine: machine,
                list: er.Products, onChanged: null);
            ingredientsCol.FlexGrow(1f).FlexBasis(Percent.Zero);
            productsCol.FlexGrow(1f).FlexBasis(Percent.Zero);
            Row ioRow = new Row { ingredientsCol, productsCol };
            ioRow.Gap(4.pt()).AlignItemsStart();
            m_dynamicCol.Add(ioRow);
        }
    }
}
