using CustomAssets.Data.Mod;
using CustomAssets.Editor;
using CustomAssets.Editor.Model;
using CustomAssets.Ui.Components;
using Mafi;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Editors {

    /// <c>add_texture_material</c> editor — path + texture + reference
    /// material + optional shader name.
    public sealed class MaterialTextureDefEditor : DefEditor<MaterialTextureDef> {

        private readonly AssetPathPicker m_texture;
        private readonly AssetPathPicker m_reference;

        public MaterialTextureDefEditor(LoadedPack pack, PackModel model) {
            AddStringField("path",
                getter: d => d.Path,
                setter: (d, v) => d.Path = v);

            m_texture = AddField("texture (path / Tex)",
                new AssetPathPicker(
                    pack,
                    getPath: () => AssetExpression.Parse(value?.TextureExpression),
                    setPath: v => { if (value != null) value.TextureExpression = AssetExpression.Render(v); },
                    allowMafiAssets: true,
                    kind: AssetsCatalog.AssetKind.Image,
                    title: new LocStrFormatted("Pick texture"),
                    variableCandidates: () => EditorHelpers.AssetVariablesIn(
                        model, value, AssetsCatalog.AssetKind.Image)),
                onRefresh: () => m_texture.RefreshDisplay());

            m_reference = AddField("reference (existing material to clone)",
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

            AddOptionalStringField("shader (optional shader name)",
                getter: d => d.Shader,
                setter: (d, v) => d.Shader = v,
                monospace: true);
        }
    }
}
