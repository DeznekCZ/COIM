using System;
using System.Reflection;
using CustomAssets.Editor.Model;
using CustomAssets.Ui.Components;
using Mafi;
using Mafi.Core.Buildings.Mine;
using Mafi.Core.Buildings.ResearchLab;
using Mafi.Core.Buildings.Settlements;
using Mafi.Core.Factory.NuclearReactors;
using Mafi.Core.Ports.Io;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Editors {

    /// Shared base for the entity-clone editors that follow the same
    /// "id + source + name + description + N typed numeric overrides +
    /// research + lockedOnInit" shape (Decoration, Food, ISP, Hospital,
    /// MineTower, ResearchLab, NuclearReactor).
    ///
    /// Generic over TSourceProto so each subclass gets a typed
    /// <see cref="ProtoPicker{T}"/> for the source field (instead of the
    /// earlier plain text id), plus a "Fill fields from source" button
    /// driven by a subclass-supplied <c>fillFromSource</c> callback that
    /// copies proto values into the def's override fields.
    public abstract class EntityCloneDefEditor<TDef, TSourceProto> : NamedDefEditor<TDef>
            where TDef : NamedDef
            where TSourceProto : Mafi.Core.Entities.Static.Layout.LayoutEntityProto {

        protected readonly ProtosDb m_protosDb;
        protected readonly PackModel m_packModel;

        // Per-subclass plumbing — accessor delegates the base ctor needs
        // to read/write the def's source / research / lockedOnInit fields
        // (each subclass names them differently).
        private readonly Func<TDef, string> m_getSourceId;
        private readonly Action<TDef, string> m_setSourceId;

        // Holders re-rendered on every Value() swap so the captured `value`
        // reference inside each picker / button matches the currently
        // bound def.
        private readonly Column m_sourceHolder;
        private readonly Column m_fillButtonHolder;

        protected EntityCloneDefEditor(PackModel packModel, ProtosDb protosDb,
                string idLabel, string sourceLabel,
                Func<TDef, string> getSourceId, Action<TDef, string> setSourceId,
                Func<TDef, string> getResearchId, Action<TDef, string> setResearchId,
                Func<TDef, bool?> getLockedOnInit, Action<TDef, bool?> setLockedOnInit,
                Action<TDef, TSourceProto> fillFromSource = null,
                Func<TDef, string> getLayoutSourceStr = null,
                Action<TDef, string> setLayoutSourceStr = null) {
            m_getLayoutSourceStr = getLayoutSourceStr;
            m_setLayoutSourceStr = setLayoutSourceStr;
            m_protosDb  = protosDb;
            m_packModel = packModel;
            m_getSourceId = getSourceId;
            m_setSourceId = setSourceId;

            // A clone becomes a brand-new proto, and the build menu / unlock
            // lists are assembled once at load. Lead with that so the modder
            // doesn't hunt for a toolbar entry that can't exist yet.
            Add(new RestartNotice(
                "Cloned entities are registered when the game loads — save this " +
                "pack, then restart the game before looking for it in the build menu."));

            AddStringField(idLabel, d => d.Id, (d, v) => d.Id = v);
            AddStringField("name (defaults to source name)",
                d => d.Name, (d, v) => d.Name = v);
            AddStringField("description (defaults to source description)",
                getter: d => readDescription(d),
                setter: (d, v) => writeDescription(d, v),
                multiline: true);

            // Typed source picker. Rebuilt on every Value() swap so its
            // captured getId/setId closures reference the currently
            // bound def. variableResolver lets the picker render typed-
            // refs (e.g. Ids.X.NuclearReactor) when the modder used one
            // in the source field of a previous save.
            m_sourceHolder = new Column();
            m_sourceHolder.AlignItemsStretch();
            AddField(sourceLabel, m_sourceHolder, onRefresh: () => {
                m_sourceHolder.Clear();
                if (value == null) return;
                m_sourceHolder.Add(new ProtoPicker<TSourceProto>(
                    m_protosDb,
                    getId: () => value == null ? null : m_getSourceId(value),
                    setId: id => {
                        if (value != null) m_setSourceId(value, id);
                        rebuildFillButton();
                    },
                    title: new LocStrFormatted("Pick source"),
                    variableResolver: EditorHelpers.VariableResolverFor(value)));
            });

            // Fill-from-source button. Only built when a fillFromSource
            // callback is supplied (MineTowerDefEditor has no numeric
            // overrides, so no button). Rebuilt on every Value() swap.
            // Hidden when the source isn't pickable yet (no SourceId or
            // can't resolve in ProtosDb) so the modder can't click an
            // ineffective button.
            m_fillButtonHolder = new Column();
            m_fillButtonHolder.AlignItemsStretch();
            if (fillFromSource != null) {
                AddField("apply source values", m_fillButtonHolder, onRefresh: rebuildFillButton);
            }

            // Captured for the rebuild closure.
            this.m_fillFromSource = fillFromSource;
            this.m_getResearchIdForRefresh = getResearchId;

            buildOverrideFields();

            // See the earlier NRE fix — ResearchIdPicker's ctor calls
            // RefreshDisplay() immediately, which fires getId before any
            // Value() swap. Null-guard the getter.
            var researchPicker = AddField("research (optional; unlocks this entity)",
                new ResearchIdPicker(
                    m_packModel, m_protosDb, ownerDef: null,
                    getId: () => value == null ? null : getResearchId(value),
                    setId: id => { if (value != null) setResearchId(value, string.IsNullOrEmpty(id) ? null : id); },
                    title: new LocStrFormatted("Pick research")));
            OnRefresh(() => researchPicker.RefreshDisplay());

            AddStringField("lockedOnInit (true / false / blank to inherit research default)",
                getter: d => {
                    bool? v = getLockedOnInit(d);
                    return v.HasValue ? (v.Value ? "true" : "false") : "";
                },
                setter: (d, v) => {
                    string s = (v ?? "").Trim().ToLowerInvariant();
                    if (s == "true") setLockedOnInit(d, true);
                    else if (s == "false") setLockedOnInit(d, false);
                    else setLockedOnInit(d, null);
                });

            // Visual layout editor — replaces the old raw layout_str textarea.
            // Works for every entity-clone kind (settlements, mines, labs,
            // reactors) since they all implement ILayoutHostDef. Falls back to
            // a raw text editor internally when a layout uses tokens it can't
            // model. Blank layout = inherit source until the modder edits.
            AddLayoutEditor(m_packModel, m_protosDb);
        }

        private readonly Action<TDef, TSourceProto> m_fillFromSource;
        private readonly Func<TDef, string> m_getResearchIdForRefresh;

        // Optional layout-source-string accessors. When non-null, the base
        // ctor renders a monospace multi-line textarea bound to the def's
        // layout-string field, AND the Fill-from-source button pre-populates
        // it from src.Layout.SourceLayoutStr. The Python emitter writes
        // `layout_str = "..."` when set; the C# build_* ctor uses
        // buildLayoutFromOverrides to re-parse the override.
        private readonly Func<TDef, string> m_getLayoutSourceStr;
        private readonly Action<TDef, string> m_setLayoutSourceStr;

        // (Re)render the fill-from-source button. Disabled (not added)
        // when no source proto is currently resolvable; the button label
        // names the resolved source so the modder can confirm what they'd
        // be copying from.
        private void rebuildFillButton() {
            m_fillButtonHolder.Clear();
            if (value == null || m_fillFromSource == null) return;
            string sourceId = m_getSourceId(value);
            TSourceProto src = resolveSource(sourceId);
            if (src == null) {
                m_fillButtonHolder.Add(new Label(new LocStrFormatted(
                    "(pick a source above to enable Fill)"))
                    .Color(ColorRgba.LightGray).TinyFontSize());
                return;
            }
            string sourceLabel = src.Strings.Name.TranslatedString ?? src.Id.Value;
            m_fillButtonHolder.Add(new ButtonText(
                new LocStrFormatted("Fill fields from source: " + sourceLabel),
                () => {
                    if (value == null) return;
                    if (string.IsNullOrEmpty(value.Name) || value.Name.StartsWith("New ")) {
                        value.Name = src.Strings.Name.TranslatedString ?? src.Id.Value;
                    }
                    if (string.IsNullOrEmpty(readDescription(value))) {
                        writeDescription(value, src.Strings.DescShort.TranslatedString ?? "");
                    }
                    // Seed the editable layout-string from the source's
                    // SourceLayoutStr if the subclass exposes the field
                    // and the modder hasn't already typed something there.
                    if (m_getLayoutSourceStr != null && m_setLayoutSourceStr != null
                            && string.IsNullOrEmpty(m_getLayoutSourceStr(value))) {
                        m_setLayoutSourceStr(value, src.Layout.SourceLayoutStr);
                        // Drop the cached structured layout so the rebuilt
                        // panel re-parses from the freshly-seeded string.
                        value.Layout = null;
                    }
                    m_fillFromSource(value, src);
                    value.Dirty = true;
                    // Re-bind to replay every observer — the typed-int
                    // fields and the research picker re-read their
                    // current model value.
                    Value(value);
                }));
        }

        // Resolve a SourceId (string / typed-ref / variable) to its live
        // TSourceProto. Direct ProtosDb lookup; TypedRefResolver path is
        // out of scope here because the picker writes the canonical proto
        // id back into SourceId already.
        private TSourceProto resolveSource(string sourceId) {
            if (string.IsNullOrEmpty(sourceId)) return null;
            Option<TSourceProto> direct = m_protosDb.Get<TSourceProto>(new Proto.ID(sourceId));
            return direct.HasValue ? direct.Value : null;
        }

        // Reflection-based Description accessor. Same as the earlier
        // revision — every entity-clone def declares a `public string
        // Description` field so a single reflective accessor works for
        // all subclasses without each one needing to thread a delegate.
        protected virtual string readDescription(TDef d) {
            var f = typeof(TDef).GetField("Description");
            return f?.GetValue(d) as string;
        }

        protected virtual void writeDescription(TDef d, string v) {
            var f = typeof(TDef).GetField("Description");
            if (f != null) f.SetValue(d, v);
        }

        /// Override to add the typed numeric / override fields specific
        /// to this kind. Called from the base ctor after the standard
        /// id/name/description/source rows.
        protected abstract void buildOverrideFields();

        // Reflection helper for the Fix32 fields that are `private
        // readonly` on the source protos (HospitalProto.
        // SuppliesConsumedPerHundredPopsPerMonth and
        // SettlementIspModuleProto.m_computingPer100Pops). Mirrors the
        // helper in CustomAssetRegistrator; kept local so the editor
        // layer doesn't depend on that file.
        protected static Fix32 ReadPrivateFix32(object obj, string fieldName) {
            FieldInfo f = obj.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            return f == null ? Fix32.Zero : (Fix32)f.GetValue(obj);
        }
    }

    public sealed class SettlementDecorationDefEditor
            : EntityCloneDefEditor<SettlementDecorationDef, SettlementDecorationModuleProto> {
        public SettlementDecorationDefEditor(PackModel m, ProtosDb p) : base(m, p,
            "decorationId (new id for the decoration)",
            "source decoration (pick the proto to clone)",
            d => d.SourceId, (d, v) => d.SourceId = v,
            d => d.ResearchId, (d, v) => d.ResearchId = v,
            d => d.LockedOnInit, (d, v) => d.LockedOnInit = v,
            fillFromSource: (def, src) => {
                def.UpointsBonus = src.UpointsBonusToNearbyHousing.Value.IntegerPart;
                def.BonusRange   = src.BonusRange;
            },
            getLayoutSourceStr: d => d.LayoutSourceStr,
            setLayoutSourceStr: (d, v) => d.LayoutSourceStr = v) { }
        protected override void buildOverrideFields() {
            AddNullableIntField("upointsBonus (unity bonus to nearby housing; blank = inherit)",
                d => d.UpointsBonus, (d, v) => d.UpointsBonus = v);
            AddNullableIntField("bonusRange (tile range of the bonus; blank = inherit)",
                d => d.BonusRange, (d, v) => d.BonusRange = v);
        }
    }

    public sealed class SettlementFoodDefEditor
            : EntityCloneDefEditor<SettlementFoodDef, SettlementFoodModuleProto> {
        public SettlementFoodDefEditor(PackModel m, ProtosDb p) : base(m, p,
            "foodModuleId (new id for the food module)",
            "source food module (pick the proto to clone)",
            d => d.SourceId, (d, v) => d.SourceId = v,
            d => d.ResearchId, (d, v) => d.ResearchId = v,
            d => d.LockedOnInit, (d, v) => d.LockedOnInit = v,
            fillFromSource: (def, src) => {
                def.BuffersCount      = src.BuffersCount;
                def.CapacityPerBuffer = src.CapacityPerBuffer.Value;
            },
            getLayoutSourceStr: d => d.LayoutSourceStr,
            setLayoutSourceStr: (d, v) => d.LayoutSourceStr = v) { }
        protected override void buildOverrideFields() {
            AddNullableIntField("buffersCount (number of buffer slots; blank = inherit)",
                d => d.BuffersCount, (d, v) => d.BuffersCount = v);
            AddNullableIntField("capacityPerBuffer (quantity each buffer holds; blank = inherit)",
                d => d.CapacityPerBuffer, (d, v) => d.CapacityPerBuffer = v);
        }
    }

    public sealed class SettlementIspDefEditor
            : EntityCloneDefEditor<SettlementIspDef, SettlementIspModuleProto> {
        public SettlementIspDefEditor(PackModel m, ProtosDb p) : base(m, p,
            "ispModuleId (new id for the ISP module)",
            "source ISP module (pick the proto to clone)",
            d => d.SourceId, (d, v) => d.SourceId = v,
            d => d.ResearchId, (d, v) => d.ResearchId = v,
            d => d.LockedOnInit, (d, v) => d.LockedOnInit = v,
            fillFromSource: (def, src) => {
                // m_computingPer100Pops is private — read via reflection.
                def.ComputingPer100Pops   = ReadPrivateFix32(src, "m_computingPer100Pops").IntegerPart;
                def.ElectricityConsumedKw = (int)src.ElectricityConsumed.Value;
            },
            getLayoutSourceStr: d => d.LayoutSourceStr,
            setLayoutSourceStr: (d, v) => d.LayoutSourceStr = v) { }
        protected override void buildOverrideFields() {
            AddNullableIntField("computingPer100Pops (compute needed per 100 pops; blank = inherit)",
                d => d.ComputingPer100Pops, (d, v) => d.ComputingPer100Pops = v);
            AddNullableIntField("electricityConsumedKw (kW; blank = inherit)",
                d => d.ElectricityConsumedKw, (d, v) => d.ElectricityConsumedKw = v);
        }
    }

    public sealed class HospitalDefEditor : EntityCloneDefEditor<HospitalDef, HospitalProto> {
        public HospitalDefEditor(PackModel m, ProtosDb p) : base(m, p,
            "hospitalId (new id for the hospital)",
            "source hospital (pick the proto to clone)",
            d => d.SourceId, (d, v) => d.SourceId = v,
            d => d.ResearchId, (d, v) => d.ResearchId = v,
            d => d.LockedOnInit, (d, v) => d.LockedOnInit = v,
            fillFromSource: (def, src) => {
                def.PowerRequiredKw   = (int)src.PowerRequired.Value;
                def.BuffersCount      = src.BuffersCount;
                def.CapacityPerBuffer = src.CapacityPerBuffer.Value;
                // SuppliesConsumedPerHundredPopsPerMonth is private.
                def.SuppliesPerHundredPopsPerMonth =
                    ReadPrivateFix32(src, "SuppliesConsumedPerHundredPopsPerMonth").IntegerPart;
            },
            getLayoutSourceStr: d => d.LayoutSourceStr,
            setLayoutSourceStr: (d, v) => d.LayoutSourceStr = v) { }
        protected override void buildOverrideFields() {
            AddNullableIntField("powerRequiredKw (kW; blank = inherit)",
                d => d.PowerRequiredKw, (d, v) => d.PowerRequiredKw = v);
            AddNullableIntField("buffersCount (number of buffer slots; blank = inherit)",
                d => d.BuffersCount, (d, v) => d.BuffersCount = v);
            AddNullableIntField("capacityPerBuffer (quantity each buffer holds; blank = inherit)",
                d => d.CapacityPerBuffer, (d, v) => d.CapacityPerBuffer = v);
            AddNullableIntField("suppliesPerHundredPopsPerMonth (blank = inherit)",
                d => d.SuppliesPerHundredPopsPerMonth, (d, v) => d.SuppliesPerHundredPopsPerMonth = v);
        }
    }

    public sealed class MineTowerDefEditor : EntityCloneDefEditor<MineTowerDef, MineTowerProto> {
        public MineTowerDefEditor(PackModel m, ProtosDb p) : base(m, p,
            "mineTowerId (new id for the mine tower)",
            "source mine tower (pick the proto to clone)",
            d => d.SourceId, (d, v) => d.SourceId = v,
            d => d.ResearchId, (d, v) => d.ResearchId = v,
            d => d.LockedOnInit, (d, v) => d.LockedOnInit = v,
            fillFromSource: null,  // no numeric knobs to fill
            getLayoutSourceStr: d => d.LayoutSourceStr,
            setLayoutSourceStr: (d, v) => d.LayoutSourceStr = v) { }
        protected override void buildOverrideFields() { }
    }

    public sealed class FarmDefEditor : EntityCloneDefEditor<FarmDef, Mafi.Core.Buildings.Farms.FarmProto> {
        public FarmDefEditor(PackModel m, ProtosDb p) : base(m, p,
            "farmId (new id for the farm)",
            "source farm (pick the proto to clone)",
            d => d.SourceId, (d, v) => d.SourceId = v,
            d => d.ResearchId, (d, v) => d.ResearchId = v,
            d => d.LockedOnInit, (d, v) => d.LockedOnInit = v,
            fillFromSource: (def, src) => {
                // Percent.RawValue divides by 1000 to recover the integer
                // percent (100% → 100), matching EnrichmentRefEditor's
                // load-existing path.
                def.YieldMultiplierPercent    = src.YieldMultiplier.RawValue / 1000;
                def.DemandsMultiplierPercent  = src.DemandsMultiplier.RawValue / 1000;
                def.FertilityReplenishPercent = src.FertilityReplenishPerDay.RawValue / 1000;
                def.WaterCollectedProductId   = src.WaterCollectedPerDay.Product?.Id.Value;
                // PartialQuantity.Value is Fix32 — IntegerPart floors to
                // the whole-unit quantity, adequate for the modder's
                // starting point (they can bump it to a fractional
                // approximation via the source's exact Value if needed).
                def.WaterCollectedQuantity    = src.WaterCollectedPerDay.Quantity.Value.IntegerPart;
                def.WaterEvaporationPerDay    = src.WaterEvaporationPerDay.Value.IntegerPart;
                def.HasIrrigationAndFertilizerSupport = src.HasIrrigationAndFertilizerSupport;
                def.IsGreenhouse              = src.IsGreenhouse;
            },
            getLayoutSourceStr: d => d.LayoutSourceStr,
            setLayoutSourceStr: (d, v) => d.LayoutSourceStr = v) { }
        protected override void buildOverrideFields() {
            AddNullableIntField("yieldMultiplierPercent (100 = 1× base; blank = inherit)",
                d => d.YieldMultiplierPercent, (d, v) => d.YieldMultiplierPercent = v);
            AddNullableIntField("demandsMultiplierPercent (water/fertilizer scale; blank = inherit)",
                d => d.DemandsMultiplierPercent, (d, v) => d.DemandsMultiplierPercent = v);
            AddNullableIntField("fertilityReplenishPercent (natural regen/day; blank = inherit)",
                d => d.FertilityReplenishPercent, (d, v) => d.FertilityReplenishPercent = v);
            // Water-collected — two fields (product id + quantity) that
            // together form the Product(...) wrapper the emitter writes.
            AddStringField("waterCollectedProductId (product harvested on rainy days; blank = inherit)",
                d => d.WaterCollectedProductId,
                (d, v) => d.WaterCollectedProductId = string.IsNullOrEmpty(v) ? null : v);
            AddNullableIntField("waterCollectedQuantity (per rainy day; blank = inherit)",
                d => d.WaterCollectedQuantity, (d, v) => d.WaterCollectedQuantity = v);
            AddNullableIntField("waterEvaporationPerDay (idle loss; blank = inherit)",
                d => d.WaterEvaporationPerDay, (d, v) => d.WaterEvaporationPerDay = v);
            // Nullable bool as free-text field. Blank = inherit, "true"/"false"
            // toggle. Same idiom as lockedOnInit in the base ctor.
            AddStringField("hasIrrigationAndFertilizerSupport (true / false / blank = inherit)",
                getter: d => d.HasIrrigationAndFertilizerSupport.HasValue
                    ? (d.HasIrrigationAndFertilizerSupport.Value ? "true" : "false") : "",
                setter: (d, v) => {
                    string s = (v ?? "").Trim().ToLowerInvariant();
                    if (s == "true")       d.HasIrrigationAndFertilizerSupport = true;
                    else if (s == "false") d.HasIrrigationAndFertilizerSupport = false;
                    else                   d.HasIrrigationAndFertilizerSupport = null;
                });
            AddStringField("isGreenhouse (true / false / blank = inherit)",
                getter: d => d.IsGreenhouse.HasValue
                    ? (d.IsGreenhouse.Value ? "true" : "false") : "",
                setter: (d, v) => {
                    string s = (v ?? "").Trim().ToLowerInvariant();
                    if (s == "true")       d.IsGreenhouse = true;
                    else if (s == "false") d.IsGreenhouse = false;
                    else                   d.IsGreenhouse = null;
                });
        }
    }

    public sealed class ResearchLabDefEditor : EntityCloneDefEditor<ResearchLabDef, ResearchLabProto> {
        public ResearchLabDefEditor(PackModel m, ProtosDb p) : base(m, p,
            "researchLabId (new id for the research lab)",
            "source research lab (pick the proto to clone)",
            d => d.SourceId, (d, v) => d.SourceId = v,
            d => d.ResearchId, (d, v) => d.ResearchId = v,
            d => d.LockedOnInit, (d, v) => d.LockedOnInit = v,
            fillFromSource: (def, src) => {
                def.ElectricityConsumedKw    = (int)src.ElectricityConsumed.Value;
                def.ComputingConsumed        = (int)src.ComputingConsumed.Value;
                def.DurationForRecipeSeconds = src.DurationOfRecipe.SecondsFloored;
                def.SciencePerRecipe         = src.SciencePerRecipe.IntegerPart;
                def.UnityMonthlyCost         = src.UnityMonthlyCost.Value.IntegerPart;
            },
            getLayoutSourceStr: d => d.LayoutSourceStr,
            setLayoutSourceStr: (d, v) => d.LayoutSourceStr = v) { }
        protected override void buildOverrideFields() {
            AddNullableIntField("electricityConsumedKw (kW; blank = inherit)",
                d => d.ElectricityConsumedKw, (d, v) => d.ElectricityConsumedKw = v);
            AddNullableIntField("computingConsumed (blank = inherit)",
                d => d.ComputingConsumed, (d, v) => d.ComputingConsumed = v);
            AddNullableIntField("durationForRecipeSeconds (cycle time; blank = inherit)",
                d => d.DurationForRecipeSeconds, (d, v) => d.DurationForRecipeSeconds = v);
            AddNullableIntField("sciencePerRecipe (blank = inherit)",
                d => d.SciencePerRecipe, (d, v) => d.SciencePerRecipe = v);
            AddNullableIntField("unityMonthlyCost (Upoints; blank = inherit)",
                d => d.UnityMonthlyCost, (d, v) => d.UnityMonthlyCost = v);
        }
    }

    public sealed class NuclearReactorDefEditor : EntityCloneDefEditor<NuclearReactorDef, NuclearReactorProto> {
        // Holder re-rendered on every Value() swap so the FuelPairListEditor's
        // captured `value.FuelPairs` reference matches the currently bound def.
        // Held as an instance field so the fill-from-source button (which
        // populates FuelPairs from the source reactor's chemistry list) can
        // call Refresh() after copying.
        private Column m_fuelPairsHolder;
        private FuelPairListEditor m_fuelPairList;

        // Layout grid + add_ports list — same pattern as
        // BuildMachineDefEditor / CloneMachineDefEditor. Lets the modder
        // click empty cells around the reactor's footprint to drop extra
        // ports beyond the built-in fuel/water/steam/coolant slots.
        private Column m_layoutPreviewHolder;
        private Column m_addPortsHolder;
        private PortListEditor m_addPortsList;

        public NuclearReactorDefEditor(PackModel m, ProtosDb p) : base(m, p,
            "reactorId (new id for the reactor)",
            "source reactor (pick the proto to clone)",
            d => d.SourceId, (d, v) => d.SourceId = v,
            d => d.ResearchId, (d, v) => d.ResearchId = v,
            d => d.LockedOnInit, (d, v) => d.LockedOnInit = v,
            fillFromSource: (def, src) => {
                def.MaxPowerLevel          = src.MaxPowerLevel;
                def.FuelCapacity           = src.FuelCapacity.Value;
                def.MinFuelToOperate       = src.MinFuelToOperate.Value;
                def.ProcessDurationSeconds = src.ProcessDuration.SecondsFloored;
                def.ComputingConsumed      = (int)src.ComputingConsumed.Value;
                // Copy the source reactor's fuel chemistry list into the
                // def's fuel_pairs override so the editor lands with a
                // working defaults set the modder can tune.
                var pairs = new System.Collections.Generic.List<FuelPairRef>();
                foreach (var fd in src.FuelPairs) {
                    pairs.Add(new FuelPairRef(
                        fd.FuelInProto?.Id.Value,
                        fd.SpentFuelOutProto?.Id.Value,
                        fd.Duration.SecondsFloored));
                }
                def.FuelPairs = pairs;
            },
            getLayoutSourceStr: d => d.LayoutSourceStr,
            setLayoutSourceStr: (d, v) => d.LayoutSourceStr = v) { }
        protected override void buildOverrideFields() {
            AddNullableIntField("maxPowerLevel (blank = inherit)",
                d => d.MaxPowerLevel, (d, v) => d.MaxPowerLevel = v);
            AddNullableIntField("fuelCapacity (quantity; blank = inherit)",
                d => d.FuelCapacity, (d, v) => d.FuelCapacity = v);
            AddNullableIntField("minFuelToOperate (quantity; blank = inherit)",
                d => d.MinFuelToOperate, (d, v) => d.MinFuelToOperate = v);
            AddNullableIntField("processDurationSeconds (blank = inherit)",
                d => d.ProcessDurationSeconds, (d, v) => d.ProcessDurationSeconds = v);
            AddNullableIntField("computingConsumed (blank = inherit)",
                d => d.ComputingConsumed, (d, v) => d.ComputingConsumed = v);

            // Explicit port-shape overrides — independent of fuel product
            // type. Required when fuel-in / fuel-out products have
            // different shapes (e.g. solid rod in, fluid waste out) or
            // when the modder wants a port shape that doesn't match the
            // auto-detection from product type.
            //
            // Typed picker backed by ProtoPicker<IoPortShapeProto> so the
            // set of choices is restricted to live IoPortShape protos in
            // the prototypes DB — picking an unknown id is impossible,
            // which removes the only validation case the old free-text
            // field had to handle. The "no-shape" sentinel uses the
            // picker's clear option so the modder can revert back to
            // "infer from first fuel".
            Column inShapeHolder = new Column();
            inShapeHolder.AlignItemsStretch();
            AddField("fuelInPortShape (blank / clear = infer from first fuel)",
                inShapeHolder, onRefresh: () => {
                    inShapeHolder.Clear();
                    if (value == null) return;
                    inShapeHolder.Add(new ProtoPicker<IoPortShapeProto>(
                        m_protosDb,
                        getId: () => value?.FuelInPortShape,
                        setId: id => { if (value != null) value.FuelInPortShape = string.IsNullOrEmpty(id) ? null : id; },
                        title: new LocStrFormatted("Pick fuel-IN port shape"),
                        variableResolver: EditorHelpers.VariableResolverFor(value),
                        allowNone: true));
                });

            Column outShapeHolder = new Column();
            outShapeHolder.AlignItemsStretch();
            AddField("fuelOutPortShape (blank / clear = infer from first fuel)",
                outShapeHolder, onRefresh: () => {
                    outShapeHolder.Clear();
                    if (value == null) return;
                    outShapeHolder.Add(new ProtoPicker<IoPortShapeProto>(
                        m_protosDb,
                        getId: () => value?.FuelOutPortShape,
                        setId: id => { if (value != null) value.FuelOutPortShape = string.IsNullOrEmpty(id) ? null : id; },
                        title: new LocStrFormatted("Pick fuel-OUT port shape"),
                        variableResolver: EditorHelpers.VariableResolverFor(value),
                        allowNone: true));
                });

            // Fuel-pair list. Empty / null = "inherit source's fuel
            // chemistry list verbatim"; once the modder adds an entry
            // the emitter writes a fuel_pairs= argument and the C# ctor
            // substitutes the modder's list at registration time.
            m_fuelPairsHolder = new Column();
            m_fuelPairsHolder.AlignItemsStretch();
            AddField("fuel_pairs (REPLACES source's fuel chemistry list; blank list = inherit)",
                m_fuelPairsHolder, onRefresh: () => {
                    m_fuelPairsHolder.Clear();
                    m_fuelPairList = null;
                    if (value == null) return;
                    if (value.FuelPairs == null) value.FuelPairs = new System.Collections.Generic.List<FuelPairRef>();
                    m_fuelPairList = new FuelPairListEditor(
                        value.FuelPairs,
                        onChanged: () => { value.Dirty = true; },
                        m_packModel, m_protosDb);
                    m_fuelPairsHolder.Add(m_fuelPairList);
                });

            // ---- Fluid overrides (coolant / water / steam) -------------
            //
            // Every field below defaults null → inherit from the source
            // reactor. The runtime ctor only patches the corresponding
            // proto field when the override is set, so leaving everything
            // blank keeps the cloned reactor's fluid wiring identical to
            // the source. Product pickers use allowNone so the modder
            // can clear back to "inherit"; port-letter fields are plain
            // strings (single character expected, validated runtime-side).

            // Port letters (CoolantInPort / CoolantOutPort / WaterInPorts
            // / SteamOutPorts) are intentionally not surfaced — the
            // runtime infers them from the selected product against the
            // source reactor's layout. The fields stay on the model so
            // hand-edited .py files round-trip, but the editor doesn't
            // ask the modder for something they shouldn't need to know.

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

            // ---- Enrichment / breeding override ------------------------
            //
            // Gated by an explicit "Add enrichment override" / "Clear
            // enrichment override" button pair instead of a Toggle —
            // Mafi's Toggle.Value(bool) round-trips through OnValueChanged,
            // so binding the toggle to value.Enrichment via onRefresh
            // caused the handler to fire with v=false on every Value()
            // swap and silently nuke the modder's Enrichment. The button
            // pair makes the create/clear actions explicit and one-way,
            // and the holder rebuilds against the current def state on
            // every Value() swap.
            Column enrichmentHolder = new Column();
            enrichmentHolder.AlignItemsStretch();
            AddField("enrichment (breeding chemistry override; blank = inherit)",
                enrichmentHolder, onRefresh: () => rebuildEnrichment(enrichmentHolder));

            // Interactive layout grid — mirror of the machine editors.
            // Shows the source reactor's footprint + every existing
            // port (fuel/water/steam/coolant); clicking an empty cell
            // adjacent to the building edge adds a draft PortRef to
            // value.AddPorts; clicking a draft port removes it. The
            // resulting list ends up in the emitted `add_ports` arg.
            m_layoutPreviewHolder = new Column();
            m_layoutPreviewHolder.AlignItemsStretch();
            AddField(
                "layout (click an empty slot to add a port; click a draft port to remove)",
                m_layoutPreviewHolder, onRefresh: refreshLayoutPreview);

            // Textual port list mirroring the layout — modder can edit
            // each port's name / type / shape / position / direction
            // here too. Same PortListEditor that machines use.
            m_addPortsHolder = new Column();
            m_addPortsHolder.AlignItemsStretch();
            AddField("add_ports (extra ports beyond the source's built-in slots)",
                m_addPortsHolder, onRefresh: () => {
                    m_addPortsHolder.Clear();
                    m_addPortsList = null;
                    if (value == null) return;
                    if (value.AddPorts == null) value.AddPorts = new System.Collections.Generic.List<PortRef>();
                    m_addPortsList = new PortListEditor(value.AddPorts, refreshLayoutPreview);
                    m_addPortsHolder.Add(m_addPortsList);
                });

            // Note: the layout-string textarea + layout-string seeding
            // are now handled by EntityCloneDefEditor base — see the
            // m_getLayoutSourceStr delegate passed to the base ctor.
        }

        // Render the interactive grid above the textual port list.
        // Pending PortRefs whose position parses are overlaid on top of
        // the source layout. Click handlers seed / remove draft ports
        // exactly like BuildMachineDefEditor.
        private void refreshLayoutPreview() {
            m_layoutPreviewHolder.Clear();
            if (value == null) return;
            NuclearReactorProto src = RecipeFormParts.ResolveProtoSafe<NuclearReactorProto>(m_protosDb, value.SourceId);
            if (src == null) {
                m_layoutPreviewHolder.Add(new Label(new LocStrFormatted(
                    string.IsNullOrEmpty(value.SourceId)
                        ? "(pick a source reactor to see its layout)"
                        : "(reactor '" + value.SourceId + "' could not be resolved)"))
                    .Color(ColorRgba.LightGray));
                return;
            }
            if (value.AddPorts == null) value.AddPorts = new System.Collections.Generic.List<PortRef>();
            var pendings = RecipeFormParts.ResolvePendingPorts(value.AddPorts);
            m_layoutPreviewHolder.Add(RecipeFormParts.BuildInteractivePortLayoutGrid(
                src.Layout, pendings,
                onPlace: (bx, by, dir) => {
                    char name = freshPortName(src);
                    value.AddPorts.Add(new PortRef {
                        Name      = name.ToString(),
                        Type      = "input",
                        Shape     = "IoPortShape_FlatConveyor",
                        PositionX = bx,
                        PositionY = by,
                        PositionZ = 0,
                        Direction = dir,
                    });
                    value.Dirty = true;
                    m_addPortsList?.Refresh();
                    refreshLayoutPreview();
                },
                onRemove: pr => {
                    value.AddPorts.Remove(pr);
                    value.Dirty = true;
                    m_addPortsList?.Refresh();
                    refreshLayoutPreview();
                }));
        }

        // Render a ProductProto picker as a labelled field. Wrapped in a
        // re-render holder so a Value() swap rebinds against the new def's
        // current value (the inner picker's get/setId closures capture
        // `value` via the helper closure).
        private void addProductPicker(string label,
                Func<string> getId, Action<string> setId, string pickerTitle) {
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

        // Render an IoPortShapeProto picker as a labelled field. Same
        // closure-rebind pattern as addProductPicker.
        private void addPortShapePicker(string label,
                Func<string> getId, Action<string> setId, string pickerTitle) {
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

        // Render either the "Add enrichment override" button (when no
        // Enrichment is set) or the EnrichmentRefEditor + "Clear" button
        // (when one exists). Both actions rebuild the holder so the UI
        // immediately reflects the new state.
        private void rebuildEnrichment(Column holder) {
            holder.Clear();
            if (value == null) return;

            if (value.Enrichment == null) {
                holder.Add(new ButtonText(
                    new LocStrFormatted("+ Add enrichment override"),
                    () => {
                        if (value == null) return;
                        value.Enrichment = new EnrichmentRef();
                        value.Dirty = true;
                        rebuildEnrichment(holder);
                    }));
                return;
            }

            holder.Add(new EnrichmentRefEditor(
                value.Enrichment,
                onChanged: () => { if (value != null) value.Dirty = true; },
                m_protosDb));
            holder.Add(new ButtonText(
                new LocStrFormatted("✕ Clear enrichment override"),
                () => {
                    if (value == null) return;
                    value.Enrichment = null;
                    value.Dirty = true;
                    rebuildEnrichment(holder);
                }));
        }

        private char freshPortName(NuclearReactorProto src) {
            var used = new System.Collections.Generic.HashSet<char>();
            if (src != null) foreach (var p in src.Layout.Ports) used.Add(p.Name);
            if (value?.AddPorts != null) {
                foreach (var pr in value.AddPorts) {
                    if (!string.IsNullOrEmpty(pr.Name)) used.Add(pr.Name[0]);
                }
            }
            for (char c = 'a'; c <= 'z'; c++) if (!used.Contains(c)) return c;
            for (char c = 'A'; c <= 'Z'; c++) if (!used.Contains(c)) return c;
            for (char c = '0'; c <= '9'; c++) if (!used.Contains(c)) return c;
            return '?';
        }
    }
}
