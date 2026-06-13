using CustomAssets.Data.Mod;
using CustomAssets.Editor;
using CustomAssets.Editor.Model;
using CustomAssets.Ui.Components;
using Mafi.Localization;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Editors {

    /// <c>add_prefab_box</c> editor — path + optional texture expression.
    public sealed class PrefabBoxDefEditor : DefEditor<PrefabBoxDef> {

        private readonly AssetPathPicker m_texture;

        public PrefabBoxDefEditor(LoadedPack pack, PackModel model) {
            AddStringField("path",
                getter: d => d.Path,
                setter: (d, v) => d.Path = v);

            m_texture = AddField(
                "texture (optional path / Tex)",
                new AssetPathPicker(
                    pack,
                    getPath: () => AssetExpression.Parse(value?.TextureExpression),
                    setPath: v => { if (value != null) value.TextureExpression = AssetExpression.Render(v); },
                    allowMafiAssets: true,
                    kind: AssetsCatalog.AssetKind.Image,
                    title: new LocStrFormatted("Pick prefab texture"),
                    variableCandidates: () => EditorHelpers.AssetVariablesIn(
                        model, value, AssetsCatalog.AssetKind.Image)),
                onRefresh: () => m_texture.RefreshDisplay());
        }
    }
}
