using System;
using CustomAssets.Data.Mod;
using Mafi;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
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
        private readonly Action m_onOpenTranslations;
        private readonly Action m_onOpenConfig;

        private readonly Label m_nameLabel;
        private readonly Label m_descLabel;
        // Holder column for the thumbnail Img so we can swap textures without
        // reconstructing the parent layout each time the pack changes.
        private readonly Column m_thumbHolder;

        /// <summary>The "switch ▾" button, exposed so the editor can anchor
        /// a FloatingColumn popup to it for pack selection.</summary>
        public readonly ButtonText SwitchButton;

        // The three pane buttons, kept so SetActivePane can light the active one.
        private ButtonText m_depsBtn;
        private ButtonText m_ttBtn;
        private ButtonText m_configBtn;

        /// Which pack-level pane the editor is currently showing in its main area.
        /// <see cref="PackPane.None"/> means a definition is selected instead.
        public enum PackPane {
            None,
            Deps,
            Translations,
            Config
        }

        /// Light the button whose pane is on screen, the way a selected tree row is
        /// lit — these buttons choose what the main pane shows, so they are a
        /// selection, not one-shot actions.
        public void SetActivePane(PackPane pane) {
            m_depsBtn?.ClassIff(Cls.selected, pane == PackPane.Deps);
            m_ttBtn?.ClassIff(Cls.selected, pane == PackPane.Translations);
            m_configBtn?.ClassIff(Cls.selected, pane == PackPane.Config);
        }

        public PackCardView(Action onSwitchPack, Action onOpenDeps, Action onOpenTranslations,
                Action onOpenConfig) {
            m_onSwitchPack       = onSwitchPack;
            m_onOpenDeps         = onOpenDeps;
            m_onOpenTranslations = onOpenTranslations;
            m_onOpenConfig       = onOpenConfig;

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

            // Top-right action buttons: deps (🔗) + translations (TT). Two
            // separate buttons stacked horizontally so each affordance keeps
            // its own tooltip. TT opens the translations dialog where the
            // modder can add a language and scan all pack strings into it;
            // each language is written to its own file so saving one
            // doesn't disturb the rest of the pack.
            m_depsBtn = new ButtonText(new LocStrFormatted("🔗"), m_onOpenDeps)
                            .Tooltip(new LocStrFormatted("Pack dependencies (manifest + load order)"));
            ButtonText depsBtn = m_depsBtn;
            m_ttBtn = new ButtonText(new LocStrFormatted("TT"), m_onOpenTranslations)
                            .Tooltip(new LocStrFormatted(
                                "Translations — add a language and generate entries from pack strings"));
            ButtonText ttBtn = m_ttBtn;

            // CFG edits THIS pack's config.json fields — the author's view: names, kinds,
            // constraints. The player-facing counterpart (values across every loaded
            // pack) is NOT here: this window is sandbox-gated, so it does not exist in an
            // ordinary game. That one is its own toolbar window, ModSettingsWindow.
            //
            // Text labels, not a gear glyph: the game's UI font has no ⚙ (U+2699), so it
            // rendered as a missing-glyph box. Only the few symbols already shipped
            // elsewhere in this UI (🔗 📋 ⚠ ▲ ▼ ✕ ⓘ) are known to be in the atlas —
            // anything else belongs in text, like the TT button beside these.
            m_configBtn = new ButtonText(new LocStrFormatted("CFG"), m_onOpenConfig)
                            .Tooltip(new LocStrFormatted(
                                "Config fields (config.json) — add, type and constrain this pack's settings"));
            Row actions = new Row { depsBtn, ttBtn, m_configBtn };
            actions.Gap(1.pt());

            Add(m_thumbHolder);
            Add(infoColumn);
            Add(actions);
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
