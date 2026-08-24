using System.Collections.Generic;
using CustomAssets.Editor.Model;
using Mafi;
using Mafi.Core.Factory.Recipes;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Unity.Ui.Library;

namespace CustomAssets.Ui.Components {

    /// <summary>
    /// Builds the in-game <see cref="RecipeUi"/> visualization (icon grid for
    /// inputs → duration → outputs) for either a registered <c>RecipeProto</c>
    /// or an in-progress modded <see cref="RecipeDef"/>. Used in
    /// <see cref="RecipeIdPicker"/> so the picker's option rows show the same
    /// recipe card the player sees on machine inspectors / codex / unlock
    /// pop-ups — no separate UI for the editor to maintain.
    ///
    /// Game recipes use <c>RecipeProto.AllInputs</c> / <c>AllOutputs</c> /
    /// <c>Duration</c> directly. Modded recipes don't have a proto yet (the
    /// pack is being authored), so we synthesize the same shape from
    /// <see cref="ProductRef"/> entries by resolving each product id through
    /// <see cref="ProtosDb"/> and constructing transient
    /// <see cref="RecipeInput"/> / <see cref="RecipeOutput"/> instances.
    /// Refs that don't resolve are skipped — a half-rendered preview beats
    /// throwing for a recipe the modder is still wiring up.
    /// </summary>
    public static class RecipePreviewBuilder {

        /// Build a preview for a registered RecipeProto. Caller owns the
        /// returned UI element.
        public static RecipeUi BuildForGame(RecipeProto recipe, Duration? duration = null) {
            RecipeUi ui = new RecipeUi();
            ui.AddBackground();
            foreach (RecipeInput inp in recipe.AllInputs)   ui.AddStaticInput(inp);
            foreach (RecipeOutput outp in recipe.AllOutputs) ui.AddStaticOutput(outp);
            // 0.3.0: duration lives on the per-machine binding, not the recipe.
            // The caller passes it when a specific binding is in view; otherwise
            // the preview simply omits the duration bar.
            if (duration.HasValue) ui.SetDuration(duration.Value);
            return ui;
        }

        /// Build a preview for a modded recipe still in PackModel. Resolves
        /// each ProductRef's id through ProtosDb. Returns null if the recipe
        /// has no resolvable products at all — caller falls back to text.
        public static RecipeUi BuildForModded(RecipeDef def, ProtosDb protosDb) {
            if (def == null || protosDb == null) return null;
            List<RecipeInput>  inputs  = resolveAll<RecipeInput>(def.Ingredients, protosDb,
                (p, q) => new RecipeInput(p, q));
            List<RecipeOutput> outputs = resolveAll<RecipeOutput>(def.Products, protosDb,
                (p, q) => new RecipeOutput(p, q));
            if (inputs.Count == 0 && outputs.Count == 0) return null;

            RecipeUi ui = new RecipeUi();
            ui.AddBackground();
            foreach (RecipeInput inp in inputs)   ui.AddStaticInput(inp);
            foreach (RecipeOutput outp in outputs) ui.AddStaticOutput(outp);
            if (def.DurationSeconds.HasValue) {
                ui.SetDuration(def.DurationSeconds.Value.Seconds());
            }
            return ui;
        }

        // ProductRef → typed RecipeProduct subclass via the supplied
        // constructor delegate. Same lookup pattern as ProtoPicker uses:
        // direct id then TypedRefResolver. Quantity is the integer the modder
        // entered, wrapped in a Quantity(int) — matches the runtime's own
        // build_recipe path.
        private static List<T> resolveAll<T>(
                List<ProductRef> refs,
                ProtosDb protosDb,
                System.Func<ProductProto, Quantity, T> ctor) {
            List<T> result = new List<T>();
            if (refs == null) return result;
            foreach (ProductRef r in refs) {
                ProductProto proto = ResolveProduct(r.ProductId, protosDb);
                if (proto == null) continue;
                result.Add(ctor(proto, new Quantity(r.Quantity)));
            }
            return result;
        }

        /// A modded <see cref="ProductRef"/>'s id → the game proto it names,
        /// or null when the id belongs to a product this pack defines (no
        /// proto yet) or resolves to nothing at all. Public because the
        /// picker's search text needs the same resolution the preview uses.
        public static ProductProto ResolveProduct(string id, ProtosDb protosDb) {
            if (string.IsNullOrEmpty(id) || protosDb == null) return null;
            Option<ProductProto> direct = protosDb.Get<ProductProto>(new Proto.ID(id));
            if (direct.HasValue) return direct.Value;
            // Typed-ref fallback (Ids.Products.X) — modders write these
            // alongside string-literal ids; the picker should preview both.
            string resolved = TypedRefResolver.ResolveOrNull(id);
            if (!string.IsNullOrEmpty(resolved)) {
                Option<ProductProto> byPath = protosDb.Get<ProductProto>(new Proto.ID(resolved));
                if (byPath.HasValue) return byPath.Value;
            }
            return null;
        }
    }
}
