using System;
using System.Collections.Generic;
using Mafi.Core.Buildings.ResearchLab;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Localization;

namespace CustomAssets.Ui.Components {

    /// <summary>
    /// Helper that builds a <see cref="ProtoPicker{T}"/> over
    /// <see cref="ProductProto"/> filtered to research-pack products — the
    /// items a research lab consumes per recipe (LabEquipment, LabEquipment2,
    /// LabEquipment3, LabEquipment4 in the vanilla set, plus any modded
    /// research-pack products registered by other packs).
    ///
    /// The eligibility filter is computed once per session by enumerating
    /// every <see cref="ResearchLabProto"/> in <see cref="ProtosDb"/> and
    /// collecting its <c>ConsumedPerRecipe.Product.Id</c>. Any product that
    /// appears as the input of at least one research lab counts as a
    /// research-pack product; everything else is excluded so the popup stays
    /// scoped to the tier-selection problem instead of dumping every product
    /// in the game.
    /// </summary>
    public static class ResearchPackProductPicker {

        /// Build the picker. Stores the chosen product's id via
        /// <paramref name="setId"/> (or null when the user clears via the
        /// "(none)" row), reads the current value via <paramref name="getId"/>.
        public static ProtoPicker<ProductProto> Build(
                ProtosDb protosDb,
                Func<string> getId,
                Action<string> setId,
                LocStrFormatted? title = null) {
            HashSet<string> allowed = collectResearchPackIds(protosDb);
            return new ProtoPicker<ProductProto>(
                protosDb,
                getId: getId,
                setId: setId,
                filter: p => allowed.Contains(p.Id.Value),
                emptyLabel: new LocStrFormatted("(no research-pack tier set)"),
                title: title ?? new LocStrFormatted("Pick research-pack tier"),
                allowNone: true);
        }

        private static HashSet<string> s_cached;
        private static ProtosDb s_cachedFor;

        // Cache keyed by ProtosDb instance — protos don't change mid-session
        // but the editor might be opened across different game saves (each
        // with its own ProtosDb pointer), so invalidate when the instance
        // changes. Otherwise rebuilding on every picker open would walk the
        // ResearchLabProto set per click.
        private static HashSet<string> collectResearchPackIds(ProtosDb protosDb) {
            if (protosDb == null) return new HashSet<string>(StringComparer.Ordinal);
            if (s_cached != null && ReferenceEquals(s_cachedFor, protosDb)) {
                return s_cached;
            }
            HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (ResearchLabProto lab in protosDb.All<ResearchLabProto>()) {
                ProductProto p = lab.ConsumedPerRecipe.Product;
                if (p != null && !string.IsNullOrEmpty(p.Id.Value)) {
                    ids.Add(p.Id.Value);
                }
            }
            s_cached = ids;
            s_cachedFor = protosDb;
            return ids;
        }
    }
}
