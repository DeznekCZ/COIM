using System.Collections.Generic;
using CustomAssets.Editor.Model;
using CustomAssets.Ui.Components;
using Mafi;
using Mafi.Core.Buildings.Farms;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Editors {

    /// <c>edit_crop</c> editor — pick an existing crop and retune its rates.
    /// The edit_* sibling of <see cref="CropDefEditor"/>: same field vocabulary,
    /// but every field is an override that stays blank until the modder sets it,
    /// and blank means "keep whatever the crop already does".
    ///
    /// A read-out of the crop's live values sits above the fields, because an
    /// override form is unusable without seeing what is being overridden.
    public sealed class EditCropDefEditor : DefEditor<EditCropDef> {

        private readonly ProtosDb m_protosDb;
        private readonly Column m_currentHolder;
        private readonly Column m_productHolder;
        private readonly TextField m_requiresGreenhouse;
        private readonly TextField m_plantByDefault;

        public EditCropDefEditor(PackModel packModel, ProtosDb protosDb) {
            m_protosDb = protosDb;

            ProtoPicker<CropProto> cropPicker = new ProtoPicker<CropProto>(
                m_protosDb,
                getId: () => value?.CropId,
                setId: id => {
                    if (value != null)
                    {
                        value.CropId = id;
                    }
                    MarkEdited();
                    refreshCurrent();
                },
                emptyLabel: new LocStrFormatted("(pick a crop…)"),
                title: new LocStrFormatted("Pick crop to retune"),
                variableResolver: resolveVariable);
            AddField("crop (existing crop to retune)", cropPicker,
                onRefresh: () => {
                    cropPicker.RefreshDisplay();
                    refreshCurrent();
                });

            m_currentHolder = new Column();
            m_currentHolder.AlignItemsStretch().Gap(2.px());
            AddField("current rates in game (what this call overrides)", m_currentHolder,
                onRefresh: refreshCurrent);

            m_productHolder = new Column();
            m_productHolder.AlignItemsStretch().Gap(4.px());
            AddField("productProduced (leave unset to keep the crop's own harvest)",
                m_productHolder, onRefresh: rebuildProductPicker);

            AddNullableIntField(
                "multiplyYieldPercent (scales the harvest; 100 = unchanged, 150 = 1.5x)",
                d => d.MultiplyYieldPercent, (d, v) => d.MultiplyYieldPercent = v);

            AddNullableIntField("growthDurationDays (days from planting to harvest)",
                d => d.GrowthDurationDays, (d, v) => d.GrowthDurationDays = v);

            AddNullableIntField("consumedWaterPerDay",
                d => d.ConsumedWaterPerDay, (d, v) => d.ConsumedWaterPerDay = v);

            AddNullableIntField(
                "consumedFertilityPercentPerDay (negative REPLENISHES the soil, as green-manure crops do)",
                d => d.ConsumedFertilityPercentPerDay,
                (d, v) => d.ConsumedFertilityPercentPerDay = v);

            AddNullableIntField("minFertilityToStartGrowthPercent",
                d => d.MinFertilityToStartGrowthPercent,
                (d, v) => d.MinFertilityToStartGrowthPercent = v);

            AddNullableIntField("surviveWithNoWaterDays",
                d => d.SurviveWithNoWaterDays, (d, v) => d.SurviveWithNoWaterDays = v);

            m_requiresGreenhouse = AddField(
                "requiresGreenhouse (true / false / blank to leave the crop unchanged)",
                new TextField().OnValueChanged(v => {
                    if (value == null)
                    {
                        return;
                    }
                    value.RequiresGreenhouse = parseTriState(v);
                    MarkEdited();
                }),
                onRefresh: () => m_requiresGreenhouse.Text(formatTriState(value.RequiresGreenhouse)));

            m_plantByDefault = AddField(
                "plantByDefault (true / false / blank to leave the crop unchanged)",
                new TextField().OnValueChanged(v => {
                    if (value == null)
                    {
                        return;
                    }
                    value.PlantByDefault = parseTriState(v);
                    MarkEdited();
                }),
                onRefresh: () => m_plantByDefault.Text(formatTriState(value.PlantByDefault)));

            AddCommentField();
        }

        // ---- Target resolution ----------------------------------------------

        private CropProto resolveCrop() {
            if (m_protosDb == null || value == null || string.IsNullOrEmpty(value.CropId))
            {
                return null;
            }
            CropProto direct = lookup(value.CropId);
            if (direct != null)
            {
                return direct;
            }
            string viaTypedRef = TypedRefResolver.ResolveOrNull(value.CropId);
            if (!string.IsNullOrEmpty(viaTypedRef))
            {
                CropProto byPath = lookup(viaTypedRef);
                if (byPath != null)
                {
                    return byPath;
                }
            }
            string viaVariable = resolveVariable(value.CropId);
            return string.IsNullOrEmpty(viaVariable) ? null : lookup(viaVariable);
        }

        private CropProto lookup(string id) {
            Option<CropProto> found = m_protosDb.Get<CropProto>(new Proto.ID(id));
            return found.HasValue ? found.Value : null;
        }

        private string resolveVariable(string maybeVariableName) {
            if (string.IsNullOrEmpty(maybeVariableName) || value == null)
            {
                return null;
            }
            Dictionary<string, string> vars = value.SourceFileVariables;
            if (vars == null)
            {
                return null;
            }
            return vars.TryGetValue(maybeVariableName, out string id) ? id : null;
        }

        // ---- Sections --------------------------------------------------------

        private void refreshCurrent() {
            m_currentHolder.Clear();
            if (value == null)
            {
                return;
            }
            CropProto crop = resolveCrop();
            if (crop == null)
            {
                m_currentHolder.Add(new Label(new LocStrFormatted(
                    string.IsNullOrEmpty(value.CropId)
                        ? "(pick a crop to see its current rates)"
                        : "(crop '" + value.CropId + "' could not be resolved)"))
                    .Color(ColorRgba.LightGray));
                return;
            }

            string harvest = crop.ProductProduced.IsEmpty
                ? "nothing (cover crop)"
                : crop.ProductProduced.Quantity.Value + "x " + crop.ProductProduced.Product.Id.Value;
            m_currentHolder.Add(new Label(new LocStrFormatted(
                "harvest: " + harvest + "   ·   grows in " + crop.DaysToGrow + " days")).TinyFontSize());
            m_currentHolder.Add(new Label(new LocStrFormatted(
                "water/day: " + crop.ConsumedWaterPerDay
                + "   ·   fertility/day: " + crop.ConsumedFertilityPerDay
                + "   ·   min fertility: " + crop.MinFertilityToStartGrowth
                + "   ·   survives dry: "
                + (crop.DaysToSurviveWithNoWater.HasValue
                    ? crop.DaysToSurviveWithNoWater.Value + " days"
                    : "n/a")))
                .TinyFontSize().Color(ColorRgba.LightGray));
            m_currentHolder.Add(new Label(new LocStrFormatted(
                "greenhouse only: " + (crop.RequiresGreenhouse ? "yes" : "no")
                + "   ·   planted by default: " + (crop.PlantByDefault ? "yes" : "no")))
                .TinyFontSize().Color(ColorRgba.LightGray));
        }

        // Harvest override. Absent (null product id) means "keep the crop's own",
        // which is why this starts as a button rather than an empty picker — an
        // empty picker would read as "harvest nothing".
        private void rebuildProductPicker() {
            m_productHolder.Clear();
            if (value == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(value.ProductProducedId))
            {
                m_productHolder.Add(new ButtonText(
                    new LocStrFormatted("+ Override the harvested product"),
                    () => {
                        if (value == null)
                        {
                            return;
                        }
                        CropProto crop = resolveCrop();
                        // Seed from the crop's own harvest so the modder edits a
                        // number instead of re-picking a product they didn't mean
                        // to change.
                        if (crop != null && !crop.ProductProduced.IsEmpty)
                        {
                            value.ProductProducedId       = crop.ProductProduced.Product.Id.Value;
                            value.ProductProducedQuantity = crop.ProductProduced.Quantity.Value;
                        }
                        else
                        {
                            value.ProductProducedQuantity = 1;
                        }
                        MarkEdited();
                        rebuildProductPicker();
                    }));
                return;
            }

            Row row = new Row().Gap(4.pt()).AlignItemsCenter();
            row.Add(new Label(new LocStrFormatted("product")).TinyFontSize());
            row.Add(new ProtoPicker<Mafi.Core.Products.ProductProto>(
                m_protosDb,
                getId: () => value?.ProductProducedId,
                setId: id => {
                    if (value != null)
                    {
                        value.ProductProducedId = id;
                    }
                    MarkEdited();
                },
                title: new LocStrFormatted("Pick harvested product"),
                variableResolver: resolveVariable).FlexGrow(1f));
            row.Add(new Label(new LocStrFormatted("qty")).TinyFontSize());
            TextField qtyField = new TextField().AllIntegersOnly().Width(80.px());
            qtyField.Text(value.ProductProducedQuantity.HasValue
                ? value.ProductProducedQuantity.Value.ToString()
                : "");
            qtyField.OnValueChanged(v => {
                if (value == null)
                {
                    return;
                }
                if (int.TryParse(v, System.Globalization.NumberStyles.Integer,
                        System.Globalization.CultureInfo.InvariantCulture, out int parsed))
                {
                    value.ProductProducedQuantity = parsed;
                }
                MarkEdited();
            });
            row.Add(qtyField);
            row.Add(new ButtonText(new LocStrFormatted("✕"), () => {
                if (value == null)
                {
                    return;
                }
                value.ProductProducedId       = null;
                value.ProductProducedQuantity = null;
                MarkEdited();
                rebuildProductPicker();
            }).Tooltip(new LocStrFormatted("Drop the override — keep the crop's own harvest")));
            m_productHolder.Add(row);
        }

        // ---- Tri-state helpers -----------------------------------------------

        private static bool? parseTriState(string raw) {
            string normalized = (raw ?? "").Trim().ToLowerInvariant();
            if (normalized == "true")
            {
                return true;
            }
            if (normalized == "false")
            {
                return false;
            }
            return null;
        }

        private static string formatTriState(bool? current) {
            if (!current.HasValue)
            {
                return "";
            }
            return current.Value ? "true" : "false";
        }
    }
}
