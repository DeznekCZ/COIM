using CustomAssets.Data.Mod;
using CustomAssets.Editor.Model;
using CustomAssets.Ui.Components;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Editors {

    /// <c>add_texture</c> editor — path is always a pack-relative file (the
    /// call registers the file as an asset; Mafi typed-refs aren't valid for
    /// the path). <c>replace</c> is the optional vanilla asset path the new
    /// texture overrides.
    public sealed class TextureDefEditor : DefEditor<TextureDef> {

        private readonly AssetPathPicker m_path;

        public TextureDefEditor(LoadedPack pack) {
            m_path = AddField(
                "path (file inside this pack's Assets folder)",
                new AssetPathPicker(
                    pack,
                    getPath: () => value?.Path,
                    setPath: v => { if (value != null) value.Path = string.IsNullOrEmpty(v) ? null : v; },
                    allowMafiAssets: false,
                    title: new LocStrFormatted("Pick texture file")),
                onRefresh: () => m_path.RefreshDisplay());

            AddOptionalStringField(
                "replace (vanilla asset to override; blank to omit)",
                getter: d => d.ReplacePath,
                setter: (d, v) => d.ReplacePath = v,
                monospace: true);
        }
    }
}
