using CustomAssets.Data.Mod;
using CustomAssets.Editor;
using CustomAssets.Editor.Model;
using CustomAssets.Ui.Components;
using Mafi.Localization;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Editors {

    /// <c>add_loose_product_material</c> editor — path + 4 texture-channel
    /// expressions + tiling. Texture channels use the image-kind picker;
    /// reference uses the material-kind picker.
    public sealed class MaterialLooseDefEditor : DefEditor<MaterialLooseDef> {

        private readonly AssetPathPicker m_albedo;
        private readonly AssetPathPicker m_normals;
        private readonly AssetPathPicker m_metallic;
        private readonly AssetPathPicker m_reference;

        public MaterialLooseDefEditor(LoadedPack pack, PackModel model) {
            AddStringField("path",
                getter: d => d.Path,
                setter: (d, v) => d.Path = v);

            m_albedo = AddField("albedo (path / Tex / list)",
                new AssetPathPicker(
                    pack,
                    getPath: () => AssetExpression.Parse(value?.AlbedoExpression),
                    setPath: v => { if (value != null) value.AlbedoExpression = AssetExpression.Render(v); },
                    allowMafiAssets: true,
                    kind: AssetsCatalog.AssetKind.Image,
                    title: new LocStrFormatted("Pick albedo texture"),
                    variableCandidates: () => EditorHelpers.AssetVariablesIn(
                        model, value, AssetsCatalog.AssetKind.Image)),
                onRefresh: () => m_albedo.RefreshDisplay());

            m_normals = AddField("normals (optional)",
                new AssetPathPicker(
                    pack,
                    getPath: () => AssetExpression.Parse(value?.NormalsExpression),
                    setPath: v => { if (value != null) value.NormalsExpression = AssetExpression.Render(v); },
                    allowMafiAssets: true,
                    kind: AssetsCatalog.AssetKind.Image,
                    title: new LocStrFormatted("Pick normals texture"),
                    variableCandidates: () => EditorHelpers.AssetVariablesIn(
                        model, value, AssetsCatalog.AssetKind.Image)),
                onRefresh: () => m_normals.RefreshDisplay());

            m_metallic = AddField("metallic (optional)",
                new AssetPathPicker(
                    pack,
                    getPath: () => AssetExpression.Parse(value?.MetallicExpression),
                    setPath: v => { if (value != null) value.MetallicExpression = AssetExpression.Render(v); },
                    allowMafiAssets: true,
                    kind: AssetsCatalog.AssetKind.Image,
                    title: new LocStrFormatted("Pick metallic texture"),
                    variableCandidates: () => EditorHelpers.AssetVariablesIn(
                        model, value, AssetsCatalog.AssetKind.Image)),
                onRefresh: () => m_metallic.RefreshDisplay());

            m_reference = AddField(
                "reference (existing material to clone; defaults to FilterMedia_mat)",
                new AssetPathPicker(
                    pack,
                    getPath: () => AssetExpression.Parse(value?.ReferenceExpression),
                    setPath: v => { if (value != null) value.ReferenceExpression = AssetExpression.Render(v); },
                    allowMafiAssets: true,
                    kind: AssetsCatalog.AssetKind.Material,
                    title: new LocStrFormatted("Pick reference material"),
                    variableCandidates: () => EditorHelpers.AssetVariablesIn(
                        model, value, AssetsCatalog.AssetKind.Material)),
                onRefresh: () => m_reference.RefreshDisplay());

            AddOptionalStringField("tiling (repeat factor; default 1)",
                getter: d => d.TilingExpression,
                setter: (d, v) => d.TilingExpression = v);
        }
    }
}
