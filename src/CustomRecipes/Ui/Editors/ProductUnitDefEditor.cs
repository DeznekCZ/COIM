using CustomAssets.Data.Mod;
using CustomAssets.Editor;
using CustomAssets.Editor.Model;
using CustomAssets.Ui.Components;
using Mafi;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Editors {

    /// <c>build_product_unit</c> editor — common product header (via base)
    /// + four bool flags appended to the shared wrap-row + prefab asset
    /// picker + maxTransport int + packingMode dropdown.
    public sealed class ProductUnitDefEditor : ProductDefEditor<ProductUnitDef> {

        // Order matches Mafi.Core.Products.CountableProductStackingMode so
        // the dropdown reads in the same sequence the API enum declares.
        private static readonly string[] PackingModes = new[] {
            "Auto", "Stacked", "StackedAlternating",
            "Triangle", "TriangleHorizontal", "Row"
        };

        private readonly AssetPathPicker m_prefab;
        private readonly TextField m_maxTransport;
        private readonly Dropdown<string> m_packingMode;

        public ProductUnitDefEditor(LoadedPack pack, PackModel model, ProtosDb protosDb)
                : base(pack, model, protosDb) {

            AddBoolFlag("isStorable",                   d => d.IsStorable,                   (d, v) => d.IsStorable                   = v);
            AddBoolFlag("isWaste",                      d => d.IsWaste,                      (d, v) => d.IsWaste                      = v);
            AddBoolFlag("allowPackingNoise",            d => d.AllowPackingNoise,            (d, v) => d.AllowPackingNoise            = v);
            AddBoolFlag("rotateSecondPackedItem90Degs", d => d.RotateSecondPackedItem90Degs, (d, v) => d.RotateSecondPackedItem90Degs = v);

            // Prefab field filters strictly to .prefab assets — Mafi.Base
            // GameObject constants ending in ".prefab", pack-side .prefab
            // files, and add_prefab_box / add_unit_prefab variables in the
            // same file. Listing textures or materials here used to drown
            // the real prefab options in the AssetsDb dump (~800 entries
            // most of them icons); the Prefab kind narrows the list to
            // what actually fits the unit's prefab slot.
            m_prefab = AddField("prefab (path / Prefab / typed-ref)",
                new AssetPathPicker(
                    pack,
                    getPath: () => AssetExpression.Parse(value?.PrefabExpression),
                    setPath: v => {
                        if (value != null) value.PrefabExpression = AssetExpression.Render(v) ?? "None";
                    },
                    allowMafiAssets: true,
                    kind: AssetsCatalog.AssetKind.Prefab,
                    title: new LocStrFormatted("Pick unit prefab"),
                    variableCandidates: () => EditorHelpers.AssetVariablesIn(
                        model, value, AssetsCatalog.AssetKind.Prefab)),
                onRefresh: () => m_prefab.RefreshDisplay());

            m_maxTransport = AddField("maxTransport (default 3)",
                new TextField()
                    .PositiveIntegersOnly()
                    .OnValueChanged(v => {
                        if (value != null) value.MaxTransport = EditorHelpers.ParseNullableInt(v);
                    }),
                onRefresh: () => m_maxTransport.Text(value.MaxTransport.HasValue
                    ? value.MaxTransport.Value.ToString() : ""));

            // packingMode dropdown — six enum values. Treats "Auto" (the
            // API default) as omitted on write so the emitter doesn't bloat
            // the source with a redundant arg.
            Dropdown<string> dd = new Dropdown<string>(
                (option, index, isInDropdown) =>
                    new Label(new LocStrFormatted(option ?? "")));
            dd.SetOptions(PackingModes);
            dd.OnValueChanged((v, _) => {
                if (value == null) return;
                value.PackingMode = string.IsNullOrEmpty(v) || v == "Auto" ? null : v;
            });
            dd.MinWidth(200.px());
            m_packingMode = AddField("packingMode", dd,
                onRefresh: () => m_packingMode.SetValue(
                    string.IsNullOrEmpty(value.PackingMode) ? "Auto" : value.PackingMode));
        }
    }
}
