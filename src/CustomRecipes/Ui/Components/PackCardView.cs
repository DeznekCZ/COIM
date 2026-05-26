using System;
using CustomAssets.Data.Mod;
using Mafi;
using Mafi.Localization;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using UnityEngine;

namespace CustomAssets.Ui.Components {

    /// <summary>
    /// Bottom-left pack card showing the active pack's thumbnail, mod ID, and
    /// description, plus a "switch pack" link and a deps-icon button. Matches
    /// the bottom-left card in the HTML layout mockup.
    ///
    /// Thumbnail loads from <c>&lt;pack&gt;/Thumbnail.png</c> via PackThumbnailCache;
    /// falls back to <c>Assets/Unity/UserInterface/General/ModLarge.svg</c>.
    /// The description text comes from the pack's manifest.json display_name /
    /// description_short — for now we show ModId and a placeholder, until the
    /// manifest loader lands as a follow-up.
    /// </summary>
    public sealed class PackCardView : Row {

        private readonly Action m_onSwitchPack;
        private readonly Action m_onOpenDeps;

        private readonly Label m_nameLabel;
        private readonly Label m_descLabel;
        // Holder column for the thumbnail Img so we can swap textures without
        // reconstructing the parent layout each time the pack changes.
        private readonly Column m_thumbHolder;

        /// <summary>The "switch ▾" button, exposed so the editor can anchor
        /// a FloatingColumn popup to it for pack selection.</summary>
        public readonly ButtonText SwitchButton;

        public PackCardView(Action onSwitchPack, Action onOpenDeps) {
            m_onSwitchPack = onSwitchPack;
            m_onOpenDeps   = onOpenDeps;

            this.PaddingLeftRight(3.pt()).PaddingTopBottom(2.pt());

            m_thumbHolder = new Column().Width(64.px()).Height(64.px());
            m_thumbHolder.MarginRight(3.pt());

            m_nameLabel = new Label(new LocStrFormatted("(no pack selected)"));
            m_descLabel = new Label(new LocStrFormatted("")).TinyFontSize();

            // ButtonText doesn't carry a TinyFontSize helper — Label has one because
            // it's primarily a text component. Plain ButtonText is fine here.
            SwitchButton = new ButtonText(new LocStrFormatted("switch ▾"), m_onSwitchPack);
            ButtonText switchBtn = SwitchButton;

            // Stack name on top, description below it, switch link at the
            // bottom — keeps the card readable when the desc is two lines.
            Column infoColumn = new Column {
                m_nameLabel,
                m_descLabel,
                switchBtn
            };
            infoColumn.FlexGrow(1f);

            // Top-right deps button.
            ButtonText depsBtn = new ButtonText(new LocStrFormatted("🔗"), m_onOpenDeps)
                            .Tooltip(new LocStrFormatted("Pack dependencies (manifest + load order)"));

            Add(m_thumbHolder);
            Add(infoColumn);
            Add(depsBtn);
        }

        /// <summary>Reflect a newly selected pack: load thumbnail from cache,
        /// display the manifest's display_name as the primary label, and the
        /// underlying ModId as the (smaller) description below it. The id
        /// is the string modders use when referencing the pack from Python
        /// (e.g. mod_dependencies); surfacing both keeps the visual
        /// identity and the technical identity equally visible.</summary>
        public void SetPack(LoadedPack pack) {
            m_thumbHolder.Clear();
            if (pack == null) {
                m_nameLabel.Value(new LocStrFormatted("(no pack selected)"));
                m_descLabel.Value(new LocStrFormatted(""));
                return;
            }
            m_nameLabel.Value(new LocStrFormatted(PackManifestCache.DisplayName(pack)));
            m_descLabel.Value(new LocStrFormatted(pack.ModId));

            Texture2D tex = PackThumbnailCache.TryGet(pack);
            if (tex != null) {
                // Real thumbnail loaded from disk.
                m_thumbHolder.Add(new Img(tex));
            } else {
                // Fallback to the generic mod icon used by COI's own mods panel.
                m_thumbHolder.Add(new Img(PackThumbnailCache.FallbackIconPath));
            }
        }
    }
}
