using CustomAssets.Editor.Model;
using Mafi.Core.Buildings.Settlements;
using Mafi.Core.Prototypes;

namespace CustomAssets.Ui.Editors {

    /// <c>build_housing</c> editor — clones a SettlementHousingModuleProto
    /// from a source housing (e.g. <c>HousingT2</c>) and overrides the
    /// modder-facing knobs: capacity / unity-points capacity / research /
    /// lockedOnInit. Now extends the shared
    /// <see cref="EntityCloneDefEditor{TDef,TSourceProto}"/> so it gets the
    /// same typed source picker + "Fill fields from source" affordance
    /// the other entity-clone editors do; the unity-bonus needs profile
    /// + per-need consumption increases still come from source verbatim.
    public sealed class HousingDefEditor
            : EntityCloneDefEditor<HousingDef, SettlementHousingModuleProto> {

        public HousingDefEditor(PackModel packModel, ProtosDb protosDb) : base(
            packModel, protosDb,
            idLabel:     "housingId (new id for the housing)",
            sourceLabel: "source housing (pick the proto to clone)",
            getSourceId: d => d.SourceId, setSourceId: (d, v) => d.SourceId = v,
            getResearchId: d => d.ResearchId, setResearchId: (d, v) => d.ResearchId = v,
            getLockedOnInit: d => d.LockedOnInit, setLockedOnInit: (d, v) => d.LockedOnInit = v,
            fillFromSource: (def, src) => {
                def.Capacity        = src.Capacity;
                def.UpointsCapacity = src.UpointsCapacity.Value.IntegerPart;
            },
            getLayoutSourceStr: d => d.LayoutSourceStr,
            setLayoutSourceStr: (d, v) => d.LayoutSourceStr = v) { }

        protected override void buildOverrideFields() {
            AddNullableIntField("capacity (max pop count; blank = inherit from source)",
                d => d.Capacity, (d, v) => d.Capacity = v);
            AddNullableIntField("upointsCapacity (unity-points capacity bonus; blank = inherit)",
                d => d.UpointsCapacity, (d, v) => d.UpointsCapacity = v);
        }
    }
}
