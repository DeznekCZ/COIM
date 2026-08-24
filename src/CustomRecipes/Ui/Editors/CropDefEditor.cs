using System.Collections.Generic;
using CustomAssets.Data.Mod;
using CustomAssets.Editor.Model;
using CustomAssets.Ui.Components;
using Mafi;
using Mafi.Core.Buildings.Farms;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.Ui;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Editors {

    /// <c>add_crop</c> / <c>clone_crop</c> editor — depending on whether
    /// <see cref="CropDef.SourceId"/> is set the emitter writes either
    /// function, and the "Fill from source" affordance below only shows
    /// when a source is picked. Every numeric field is nullable so
    /// clone_crop can omit fields and inherit from the source proto.
    ///
    /// Not a <see cref="EntityCloneDefEditor{TDef, TSourceProto}"/> —
    /// <see cref="CropProto"/> extends <see cref="Proto"/>, not
    /// <see cref="Mafi.Core.Entities.Static.Layout.LayoutEntityProto"/>,
    /// so it has no layout / port surface to inherit and no need for the
    /// layout-editor host. Straight <see cref="DefEditor{TDefBase}"/>
    /// with typed field builders.
    public sealed class CropDefEditor : NamedDefEditor<CropDef> {

        private readonly PackModel m_packModel;
        private readonly ProtosDb m_protosDb;
        private readonly LoadedPack m_pack;
        private readonly Column m_sourceHolder;
        private readonly Column m_fillHolder;
        private readonly Column m_productHolder;
        private readonly Column m_farmsHolder;
        private readonly AssetPathPicker m_iconPicker;
        private readonly AssetPathPicker m_prefabPicker;

        public CropDefEditor(LoadedPack pack, PackModel packModel, ProtosDb protosDb) {
            m_pack = pack;
            m_packModel = packModel;
            m_protosDb = protosDb;

            AddIdField("cropId (new id for the crop)");
            AddNameField();
            AddStringField("description (optional)",
                d => d.Description, (d, v) => d.Description = v, multiline: true);

            // Source picker — when set, the emitter writes clone_crop and
            // the runtime seeds omitted fields from the source proto. When
            // null, add_crop is emitted and every required field must be
            // supplied.
            m_sourceHolder = new Column();
            m_sourceHolder.AlignItemsStretch();
            AddField("source (pick to CLONE from — blank = create from scratch)",
                m_sourceHolder, onRefresh: () => {
                    m_sourceHolder.Clear();
                    if (value == null) return;
                    m_sourceHolder.Add(new ProtoPicker<CropProto>(
                        m_protosDb,
                        getId: () => value?.SourceId,
                        setId: id => {
                            if (value != null) value.SourceId = string.IsNullOrEmpty(id) ? null : id;
                            rebuildFillButton();
                        },
                        title: new LocStrFormatted("Pick source crop to clone"),
                        variableResolver: EditorHelpers.VariableResolverFor(value),
                        allowNone: true));
                });

            // "Fill from source" — only visible when a source is picked
            // AND resolvable in the ProtoDb. Seeds every field from the
            // source proto so clone_crop starts with sensible defaults.
            m_fillHolder = new Column();
            m_fillHolder.AlignItemsStretch();
            AddField("seed from source", m_fillHolder, onRefresh: rebuildFillButton);

            // Product produced — Product(id, qty) wrapper. The picker
            // itself edits both halves. Null when no product is produced
            // (cover crops match this).
            m_productHolder = new Column();
            m_productHolder.AlignItemsStretch();
            AddField("productProduced (harvested per grow cycle; blank = cover crop / no output)",
                m_productHolder, onRefresh: rebuildProductPicker);

            AddNullableIntField("consumedWaterPerDay (per crop, per day)",
                d => d.ConsumedWaterPerDay,
                (d, v) => d.ConsumedWaterPerDay = v);
            AddNullableIntField("consumedFertilityPercentPerDay (0 = doesn't drain; negative = restores)",
                d => d.ConsumedFertilityPercentPerDay,
                (d, v) => d.ConsumedFertilityPercentPerDay = v);
            AddNullableIntField("minFertilityToStartGrowthPercent (soil-fertility gate)",
                d => d.MinFertilityToStartGrowthPercent,
                (d, v) => d.MinFertilityToStartGrowthPercent = v);
            AddNullableIntField("growthDurationDays (in-game days to fully grow)",
                d => d.GrowthDurationDays,
                (d, v) => d.GrowthDurationDays = v);
            AddNullableIntField("surviveWithNoWaterDays (blank = crop never dies from thirst)",
                d => d.SurviveWithNoWaterDays,
                (d, v) => d.SurviveWithNoWaterDays = v);

            m_iconPicker = AddField("icon (asset path or pack texture)",
                new AssetPathPicker(
                    pack,
                    getPath: () => value?.IconPath,
                    setPath: v => { if (value != null) value.IconPath = string.IsNullOrEmpty(v) ? null : v; },
                    allowMafiAssets: true,
                    kind: CustomAssets.Editor.AssetsCatalog.AssetKind.Image,
                    title: new LocStrFormatted("Pick crop icon"),
                    variableCandidates: () => EditorHelpers.AssetVariablesIn(
                        packModel, value, CustomAssets.Editor.AssetsCatalog.AssetKind.Image)),
                onRefresh: () => m_iconPicker.RefreshDisplay());

            m_prefabPicker = AddField("prefab (asset path — required on create-from-scratch)",
                new AssetPathPicker(
                    pack,
                    getPath: () => value?.PrefabPath,
                    setPath: v => { if (value != null) value.PrefabPath = string.IsNullOrEmpty(v) ? null : v; },
                    allowMafiAssets: true,
                    kind: CustomAssets.Editor.AssetsCatalog.AssetKind.Prefab,
                    title: new LocStrFormatted("Pick crop prefab")),
                onRefresh: () => m_prefabPicker.RefreshDisplay());

            AddStringField("requiresGreenhouse (true / false / blank = inherit)",
                getter: d => d.RequiresGreenhouse.HasValue
                    ? (d.RequiresGreenhouse.Value ? "true" : "false") : "",
                setter: (d, v) => {
                    string s = (v ?? "").Trim().ToLowerInvariant();
                    if (s == "true")       d.RequiresGreenhouse = true;
                    else if (s == "false") d.RequiresGreenhouse = false;
                    else                   d.RequiresGreenhouse = null;
                });
            AddStringField("plantByDefault (true / false / blank = inherit)",
                getter: d => d.PlantByDefault.HasValue
                    ? (d.PlantByDefault.Value ? "true" : "false") : "",
                setter: (d, v) => {
                    string s = (v ?? "").Trim().ToLowerInvariant();
                    if (s == "true")       d.PlantByDefault = true;
                    else if (s == "false") d.PlantByDefault = false;
                    else                   d.PlantByDefault = null;
                });

            // Research picker — same shape as EntityCloneDefEditor uses.
            var researchPicker = AddField("research (optional; unlocks this crop)",
                new ResearchIdPicker(
                    m_packModel, m_protosDb, ownerDef: null,
                    getId: () => value == null ? null : value.ResearchId,
                    setId: id => { if (value != null) value.ResearchId = string.IsNullOrEmpty(id) ? null : id; },
                    title: new LocStrFormatted("Pick research")));
            OnRefresh(() => researchPicker.RefreshDisplay());

            // farms= restrict-to list. Each row = an EntityIdPicker that
            // filters to farm defs / FarmProtos. Currently NOT enforced
            // at runtime (see the runtime ctor note) — surfaced here so
            // modders can express intent and hand-editing the file
            // round-trips cleanly.
            m_farmsHolder = new Column();
            m_farmsHolder.AlignItemsStretch();
            AddField("farms (restrict to these farm ids — currently informational; blank = auto-link to every compatible farm)",
                m_farmsHolder, onRefresh: rebuildFarmsList);

            AddCommentField();
        }

        // ---- Sub-editors ----------------------------------------------------

        // Product-produced picker. Rebuilds each Value() swap so the inner
        // ProductRef reference tracks the current def. When cleared, the
        // ProductRef itself is nulled so the emitter treats the crop as a
        // cover crop with no output.
        private void rebuildProductPicker() {
            m_productHolder.Clear();
            if (value == null) return;
            if (value.ProductProduced == null) {
                m_productHolder.Add(new ButtonText(
                    new LocStrFormatted("+ Add produced product"),
                    () => {
                        if (value == null) return;
                        value.ProductProduced = new ProductRef();
                        value.Dirty = true;
                        rebuildProductPicker();
                    }));
                return;
            }
            Row row = new Row().Gap(4.pt()).AlignItemsCenter();
            row.Add(new Label(new LocStrFormatted("product")).TinyFontSize());
            row.Add(new ProtoPicker<Mafi.Core.Products.ProductProto>(
                m_protosDb,
                getId: () => value?.ProductProduced?.ProductId,
                setId: id => {
                    if (value?.ProductProduced != null) {
                        value.ProductProduced.ProductId = id;
                        value.Dirty = true;
                    }
                },
                title: new LocStrFormatted("Pick produced product")).FlexGrow(1f));
            row.Add(new Label(new LocStrFormatted("qty")).TinyFontSize());
            TextField qtyField = new TextField().AllIntegersOnly().Width(80.px());
            qtyField.Text(value.ProductProduced.Quantity.ToString());
            qtyField.OnValueChanged(v => {
                if (int.TryParse(v, System.Globalization.NumberStyles.Integer,
                        System.Globalization.CultureInfo.InvariantCulture, out int n)
                        && value.ProductProduced != null) {
                    value.ProductProduced.Quantity = n;
                    value.Dirty = true;
                }
            });
            row.Add(qtyField);
            row.Add(new ButtonText(new LocStrFormatted("✕"), () => {
                if (value != null) {
                    value.ProductProduced = null;
                    value.Dirty = true;
                    rebuildProductPicker();
                }
            }).Tooltip(new LocStrFormatted("Remove produced product (turn into cover crop)")));
            m_productHolder.Add(row);
        }

        // "Fill from source" seeds every override field from the picked
        // source proto so the modder edits deltas instead of typing every
        // value. Hidden when no resolvable source is picked.
        private void rebuildFillButton() {
            m_fillHolder.Clear();
            if (value == null || string.IsNullOrEmpty(value.SourceId)) return;
            CropProto src = RecipeFormParts.ResolveProtoSafe<CropProto>(m_protosDb, value.SourceId);
            if (src == null) return;

            m_fillHolder.Add(new ButtonText(
                new LocStrFormatted("⇩ Fill fields from source"),
                () => {
                    if (value == null || src == null) return;
                    value.Name = src.Strings.Name.TranslatedString ?? value.Name;
                    value.Description = src.Strings.DescShort.TranslatedString ?? value.Description;
                    if (src.ProductProduced.Product != null) {
                        value.ProductProduced = new ProductRef {
                            ProductId = src.ProductProduced.Product.Id.Value,
                            Quantity  = src.ProductProduced.Quantity.Value,
                        };
                    }
                    value.ConsumedWaterPerDay             = src.ConsumedWaterPerDay.Value.IntegerPart;
                    value.ConsumedFertilityPercentPerDay  = src.ConsumedFertilityPerDay.RawValue / 1000;
                    value.MinFertilityToStartGrowthPercent= src.MinFertilityToStartGrowth.RawValue / 1000;
                    value.GrowthDurationDays              = src.DaysToGrow;
                    value.SurviveWithNoWaterDays          = src.DaysToSurviveWithNoWater;
                    value.IconPath                        = src.Graphics.IconPath;
                    value.PrefabPath                      = src.Graphics.PrefabPath;
                    value.RequiresGreenhouse              = src.RequiresGreenhouse;
                    value.PlantByDefault                  = src.PlantByDefault;
                    value.Dirty = true;
                    // Force a full editor refresh so every picker rebinds
                    // to the freshly-populated def.
                    Value(value);
                })
                .Tooltip(new LocStrFormatted(
                    "Populate every field from the source crop's current values, then tune deltas.")));
        }

        // Rebuild the farms= restrict-to list. Each row is an EntityIdPicker
        // wired to a specific index in value.Farms; the "+ add farm" button
        // appends. Removed rows via ✕ button.
        private void rebuildFarmsList() {
            m_farmsHolder.Clear();
            if (value == null) return;
            if (value.Farms == null) value.Farms = new List<string>();
            for (int i = 0; i < value.Farms.Count; i++) {
                int captured = i;
                Row row = new Row().Gap(2.pt()).AlignItemsCenter();
                row.Add(new EntityIdPicker(
                    m_packModel, m_protosDb,
                    getId: () => captured < value.Farms.Count ? value.Farms[captured] : null,
                    setId: id => {
                        if (captured < value.Farms.Count) {
                            value.Farms[captured] = id ?? "";
                            value.Dirty = true;
                        }
                    },
                    getOwnerDef: () => value,
                    title: new LocStrFormatted("Pick farm to restrict crop to")).FlexGrow(1f));
                row.Add(new ButtonText(new LocStrFormatted("✕"), () => {
                    if (captured < value.Farms.Count) {
                        value.Farms.RemoveAt(captured);
                        value.Dirty = true;
                        rebuildFarmsList();
                    }
                }));
                m_farmsHolder.Add(row);
            }
            if (value.Farms.Count == 0) {
                m_farmsHolder.Add(new Label(new LocStrFormatted("  (auto-link to every compatible farm)"))
                    .Color(ColorRgba.LightGray));
            }
            m_farmsHolder.Add(new ButtonText(
                new LocStrFormatted("+ add farm"),
                () => {
                    if (value == null) return;
                    if (value.Farms == null) value.Farms = new List<string>();
                    value.Farms.Add("");
                    value.Dirty = true;
                    rebuildFarmsList();
                }));
        }
    }
}
