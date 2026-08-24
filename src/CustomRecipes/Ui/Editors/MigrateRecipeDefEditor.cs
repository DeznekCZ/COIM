using CustomAssets.Editor.Model;
using CustomAssets.Ui.Components;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Editors {

    /// <c>migrate_recipe(old, new, since)</c> — tombstone for a recipe this pack
    /// used to ship, remapping it in saves that still reference the old id.
    ///
    /// Two deliberate departures from the other def editors:
    ///
    ///   • `old` is a PLAIN TEXT field, not a RecipeIdPicker. A picker offers ids
    ///     that currently exist (RecipeIdPicker reads PackModel.Recipes), and the
    ///     whole point of this field is an id that no longer does. Offering a live
    ///     picker there would invite the modder to select a recipe they still ship,
    ///     which the game then rejects at startup.
    ///
    ///   • The header spells out that the statement must outlive the recipe it
    ///     describes. Every other def can be deleted freely once the modder stops
    ///     wanting it; deleting this one silently strands every save still holding
    ///     the old id, and nothing later can reconstruct the mapping.
    public sealed class MigrateRecipeDefEditor : DefEditor<MigrateRecipeDef> {

        private readonly RecipeIdPicker m_new;

        public MigrateRecipeDefEditor(PackModel model, ProtosDb protosDb) {
            Add(new Label(new LocStrFormatted(
                    "Keep this statement forever. It is the only record that the old recipe id "
                    + "ever existed — remove it and every save still using that id loses the "
                    + "recipe from its machines with no way back."))
                .Class(Cls.fontMonospace).TinyFontSize());

            AddStringField("old (removed recipe id — must no longer be built by this pack)",
                getter: d => d?.OldRecipeId,
                setter: (d, v) => d.OldRecipeId = v,
                monospace: true);

            m_new = AddField("new (replacement recipe)",
                new RecipeIdPicker(
                    model,
                    protosDb,
                    getId: () => value?.NewRecipeId,
                    setId: v => { if (value != null) value.NewRecipeId = v; },
                    variableResolver: id => {
                        if (value == null || string.IsNullOrEmpty(id)) return null;
                        return EditorHelpers.VariableResolverFor(value)?.Invoke(id);
                    },
                    title: new LocStrFormatted("Pick the replacement recipe")),
                onRefresh: () => m_new.RefreshDisplay());

            AddOptionalStringField("since (pack version this rename shipped in — blank uses the manifest version)",
                getter: d => d?.Since,
                setter: (d, v) => d.Since = v);

            AddCommentField();
        }
    }
}
