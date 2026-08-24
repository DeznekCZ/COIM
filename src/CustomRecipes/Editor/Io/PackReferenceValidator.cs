using System.Collections.Generic;
using System.IO;
using CustomAssets.Data.Mod;
using CustomAssets.Editor.Model;

namespace CustomAssets.Editor.Io {

    /// <summary>
    /// Checks that every reference one definition makes to ANOTHER definition in
    /// the same pack is resolvable at runtime — i.e. that it is either
    ///
    ///   (a) linked through a Python variable, so the reference is a live object
    ///       rather than an id looked up in the proto database, or
    ///   (b) pointing at a definition that is created EARLIER in load order.
    ///
    /// Anything else is a forward reference by bare id: the emitted call names a
    /// prototype that does not exist yet, and the pack fails to load.
    ///
    /// This complements <see cref="PackValidator"/>, which catches the same
    /// mistake one level down — a Python VARIABLE used above its assignment. That
    /// check is AST-based and file-local; this one is model-based and spans
    /// files, because a bare id reference leaves no variable behind to check.
    ///
    /// Conservative by design: an id that matches no definition in this pack is
    /// assumed to be a vanilla or other-mod prototype and is ignored. That
    /// direction produces missed warnings rather than false alarms, which is the
    /// right trade for a gate that blocks saving.
    /// </summary>
    public static class PackReferenceValidator {

        /// One unresolvable reference.
        public sealed class Violation {
            /// The definition that CONTAINS the bad reference.
            public DefBase Source;
            /// The definition being referenced too early.
            public DefBase Target;
            /// Which argument holds it, e.g. "machine" — for the message.
            public string Field;
            public string TargetId;

            public PackIssue ToIssue() {
                return new PackIssue {
                    SourceFile = Source?.SourceFile,
                    Line = Source != null && Source.SourceStartLine > 0
                        ? Source.SourceStartLine : 1,
                    Message = Describe(this)
                };
            }
        }

        /// Every forward-by-id reference in the pack. Empty when the pack is fine.
        public static List<Violation> Validate(PackModel model, LoadedPack pack) {
            var violations = new List<Violation>();
            if (model?.Definitions == null) return violations;

            Dictionary<string, int> fileOrder = PackLoadOrder.Compute(pack);

            // Index every id this pack DEFINES. Later duplicates don't overwrite
            // the first — the earliest creation is what a reference resolves
            // against, so keeping the earliest is the correct (and lenient) read.
            var definedBy = new Dictionary<string, DefBase>(System.StringComparer.Ordinal);
            foreach (DefBase d in model.Definitions) {
                string id = DefinedId(d);
                if (string.IsNullOrEmpty(id)) continue;
                if (!definedBy.ContainsKey(id)) definedBy[id] = d;
            }

            foreach (DefBase source in model.Definitions) {
                foreach (KeyValuePair<string, string> reference in OutgoingRefs(source)) {
                    string id = reference.Value;
                    if (string.IsNullOrEmpty(id)) continue;

                    // (a) linked by variable — the emitter writes the bare
                    // variable name, so the value is passed as a live object and
                    // load order doesn't come into it. PackValidator separately
                    // checks that the variable itself is assigned above its use.
                    if (source.SourceFileVariables != null
                            && source.SourceFileVariables.ContainsKey(id)) {
                        continue;
                    }

                    if (!definedBy.TryGetValue(id, out DefBase target)) continue;  // vanilla / other mod
                    if (ReferenceEquals(target, source)) continue;

                    // (b) defined earlier?
                    if (positionOf(target, fileOrder) < positionOf(source, fileOrder)) continue;

                    violations.Add(new Violation {
                        Source = source, Target = target,
                        Field = reference.Key, TargetId = id,
                    });
                }
            }
            return violations;
        }

        public static string Describe(Violation v) {
            string where = v.Target?.SourceFile != null
                ? Path.GetFileName(v.Target.SourceFile)
                  + (v.Target.SourceStartLine > 0 ? ":" + v.Target.SourceStartLine : "")
                : "elsewhere in this pack";
            return "'" + (v.Source?.DisplayId ?? v.Source?.Kind) + "' references "
                + (string.IsNullOrEmpty(v.Field) ? "" : v.Field + " = ")
                + "'" + v.TargetId + "', which this pack defines LATER (" + where
                + "). The COI runtime creates prototypes in load order, so the id"
                + " does not exist yet at this point. Give the definition a"
                + " variable and reference that instead, or move it earlier.";
        }

        // Position in effective load order: (file index, line). A def with no
        // source range yet is pending and gets appended at end of file, so it
        // sorts last within its file.
        private static long positionOf(DefBase def, Dictionary<string, int> fileOrder) {
            int fileIndex = int.MaxValue;
            if (def?.SourceFile != null && fileOrder.TryGetValue(def.SourceFile, out int idx)) {
                fileIndex = idx;
            }
            long line = def != null && def.SourceStartLine > 0 ? def.SourceStartLine : int.MaxValue;
            return (long)fileIndex * int.MaxValue + line;
        }

        /// The id a definition CREATES, or null when it only references existing
        /// prototypes (every <c>edit_*</c>, <c>add_unlock_*</c> and
        /// <c>bind_recipe</c> call) or isn't a definition at all.
        ///
        /// <see cref="NamedDef"/> owns an id by construction. The asset
        /// registrations key off a path instead, which is what an
        /// <c>iconPath</c>/<c>prefabPath</c> reference resolves against.
        public static string DefinedId(DefBase def) {
            if (def is NamedDef named) return named.Id;
            if (def is TextureDef || def is MaterialLooseDef || def is PrefabBoxDef
                    || def is UnitPrefabDef || def is MaterialTextureDef) {
                return def.DisplayId;
            }
            return null;
        }

        /// Every id this definition REFERENCES, as (argument label → id).
        ///
        /// Mirrors the <c>appendIdRef</c> call sites in
        /// <see cref="PackEmitter"/> — those are the arguments that get emitted
        /// as a bare id and therefore need the prototype to already exist. Raw
        /// <c>*Expression</c> fields are deliberately NOT walked: they hold
        /// verbatim source text that the emitter never rewrites, so the modder is
        /// already in control of what they say.
        public static IEnumerable<KeyValuePair<string, string>> OutgoingRefs(DefBase def) {
            switch (def) {
                case RecipeDef r:
                    yield return pair("machine", r.MachineId);
                    yield return pair("research", r.ResearchId);
                    foreach (var p in products("ingredient", r.Ingredients)) yield return p;
                    foreach (var p in products("product", r.Products)) yield return p;
                    break;
                case BindRecipeDef b:
                    yield return pair("recipe", b.RecipeId);
                    yield return pair("machine", b.MachineId);
                    yield return pair("research", b.ResearchId);
                    if (b.Ports != null) {
                        foreach (PortMapRef pm in b.Ports) yield return pair("ports", pm?.ProductId);
                    }
                    break;
                case EditRecipeDef e:
                    yield return pair("recipe", e.RecipeId);
                    yield return pair("machine", e.MachineId);
                    yield return pair("research", e.ResearchId);
                    foreach (var p in products("ingredient", e.Ingredients)) yield return p;
                    foreach (var p in products("product", e.Products)) yield return p;
                    break;
                case MigrateRecipeDef m:
                    // ONLY the replacement is a live reference. `OldRecipeId` names a
                    // recipe the pack must no longer define — validating it would flag
                    // every correct migration as a dangling reference, forever.
                    yield return pair("new", m.NewRecipeId);
                    break;
                case UnlockRecipeDef u:
                    yield return pair("research", u.ResearchId);
                    yield return pair("machine", u.MachineId);
                    yield return pair("recipe", u.RecipeId);
                    break;
                case UnlockProductDef u:
                    yield return pair("research", u.ResearchId);
                    yield return pair("product", u.ProductId);
                    break;
                case UnlockMachineDef u:
                    yield return pair("research", u.ResearchId);
                    yield return pair("machine", u.MachineId);
                    break;
                case UnlockEntityDef u:
                    yield return pair("research", u.ResearchId);
                    yield return pair("entity", u.EntityId);
                    break;
                case RemoveUnlockDef u:
                    yield return pair("research", u.ResearchId);
                    // `target` is deliberately NOT validated: the whole point of
                    // the call can be cleaning up an unlock for something a game
                    // update removed, so a target that no longer resolves is the
                    // expected case, not a dangling reference.
                    yield return pair("machine", u.MachineId);
                    break;
                case EditMachinePortsDef m:
                    yield return pair("machine", m.MachineId);
                    break;
                case EditEntityCostsDef c:
                    yield return pair("entity", c.EntityId);
                    yield return pair("maintenanceProduct", c.MaintenanceProductId);
                    foreach (var p in products("products", c.Products)) yield return p;
                    break;
                case BuildMachineDef m:
                    yield return pair("source", m.SourceId);
                    yield return pair("research", m.ResearchId);
                    break;
                case ResearchDef r:
                    yield return pair("tierProduct", r.TierProductId);
                    yield return pair("iconPath", r.IconPath);
                    if (r.Parents != null) {
                        foreach (string parent in r.Parents) yield return pair("parents", parent);
                    }
                    break;
                case ProductLooseDef p:
                    yield return pair("research", p.ResearchId);
                    yield return pair("iconPath", p.IconPath);
                    yield return pair("dumpsAs", p.DumpsAsId);
                    yield return pair("prefabPath", p.PrefabPath);
                    break;
                case ProductDefBase p:
                    yield return pair("research", p.ResearchId);
                    yield return pair("iconPath", p.IconPath);
                    break;
                case FarmDef f:
                    yield return pair("source", f.SourceId);
                    yield return pair("research", f.ResearchId);
                    yield return pair("waterCollectedProduct", f.WaterCollectedProductId);
                    break;
                case CropDef c:
                    yield return pair("source", c.SourceId);
                    yield return pair("research", c.ResearchId);
                    yield return pair("iconPath", c.IconPath);
                    yield return pair("prefabPath", c.PrefabPath);
                    if (c.Farms != null) {
                        foreach (string farm in c.Farms) yield return pair("farms", farm);
                    }
                    break;
                case EditCropDef c:
                    yield return pair("crop", c.CropId);
                    yield return pair("productProduced", c.ProductProducedId);
                    break;
                case NuclearReactorDef n:
                    yield return pair("source", n.SourceId);
                    yield return pair("research", n.ResearchId);
                    yield return pair("coolantIn", n.CoolantInId);
                    yield return pair("coolantOut", n.CoolantOutId);
                    yield return pair("waterInProduct", n.WaterInProductId);
                    yield return pair("steamOutProduct", n.SteamOutProductId);
                    foreach (var p in fuels(n.FuelPairs)) yield return p;
                    break;
                case EditNuclearReactorFuelsDef n:
                    yield return pair("reactor", n.ReactorId);
                    foreach (var p in fuels(n.AddFuels)) yield return p;
                    break;
                case EditNuclearReactorPortsDef n:
                    yield return pair("reactor", n.ReactorId);
                    break;
                case EditNuclearReactorFluidsDef n:
                    yield return pair("reactor", n.ReactorId);
                    yield return pair("coolantIn", n.CoolantInId);
                    yield return pair("coolantOut", n.CoolantOutId);
                    yield return pair("waterInProduct", n.WaterInProductId);
                    yield return pair("steamOutProduct", n.SteamOutProductId);
                    break;
                case EditNuclearReactorEnrichmentDef n:
                    yield return pair("reactor", n.ReactorId);
                    if (n.Enrichment != null) {
                        yield return pair("enrichment input", n.Enrichment.InputProductId);
                        yield return pair("enrichment output", n.Enrichment.OutputProductId);
                    }
                    break;
                case HousingDef h:
                    yield return pair("source", h.SourceId);
                    yield return pair("research", h.ResearchId);
                    break;
                case SettlementDecorationDef s:
                    yield return pair("source", s.SourceId);
                    yield return pair("research", s.ResearchId);
                    break;
                case SettlementFoodDef s:
                    yield return pair("source", s.SourceId);
                    yield return pair("research", s.ResearchId);
                    break;
                case SettlementIspDef s:
                    yield return pair("source", s.SourceId);
                    yield return pair("research", s.ResearchId);
                    break;
                case HospitalDef h:
                    yield return pair("source", h.SourceId);
                    yield return pair("research", h.ResearchId);
                    break;
                case MineTowerDef m:
                    yield return pair("source", m.SourceId);
                    yield return pair("research", m.ResearchId);
                    break;
                case ResearchLabDef r:
                    yield return pair("source", r.SourceId);
                    yield return pair("research", r.ResearchId);
                    break;
                case GeneratorDef g:
                    yield return pair("source", g.SourceId);
                    yield return pair("research", g.ResearchId);
                    break;
                case ToolbarCategoryDef t:
                    yield return pair("parent", t.ParentId);
                    break;
            }
        }

        private static KeyValuePair<string, string> pair(string field, string id) {
            return new KeyValuePair<string, string>(field, id);
        }

        private static IEnumerable<KeyValuePair<string, string>> products(
                string label, List<ProductRef> list) {
            if (list == null) yield break;
            foreach (ProductRef p in list) {
                if (p != null) yield return pair(label, p.ProductId);
            }
        }

        private static IEnumerable<KeyValuePair<string, string>> fuels(List<FuelPairRef> list) {
            if (list == null) yield break;
            foreach (FuelPairRef f in list) {
                if (f == null) continue;
                yield return pair("fuelIn", f.FuelIn);
                yield return pair("spentFuelOut", f.SpentFuelOut);
            }
        }
    }
}
