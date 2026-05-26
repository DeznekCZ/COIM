using System;
using Mafi;
using Mafi.Localization;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using UnityEngine;

namespace CustomAssets.Ui.Components {

    /// <summary>
    /// Warning bar shown at the top of the editor when the currently selected
    /// pack uses the legacy load-order convention (no Definitions/__init__.py
    /// and dependencies(...) calls inside individual .py files). Carries the
    /// "Migrate now" button, "Dismiss" button, and a clickable link to the
    /// Custom Assets Loader documentation on hub.coigame.com.
    ///
    /// The banner is created hidden; the editor calls SetVisible(true) when
    /// PackRegistry.IsLegacyStructure returns true for the selected pack and
    /// the user hasn't dismissed it this session.
    /// </summary>
    public sealed class LegacyBanner : Row {

        // Single source of truth for the docs URL — referenced by both the
        // clickable label and the Application.OpenURL handler.
        public const string DocsUrl = "https://hub.coigame.com/Mod/60/Custom-Assets-Loader";

        public LegacyBanner(Action onMigrate, Action onDismiss) {
            // Warning-yellow background, padding, and a left-aligned message
            // with action buttons on the right.
            this.PaddingLeftRight(3.pt()).PaddingTopBottom(2.pt());
            // Set the warning gradient on the Inner element directly — there's
            // no dedicated "warning Row" component in Mafi, and a one-off
            // VisualElement-level style is the lightest option.
            InnerElement.style.backgroundColor =
                new UnityEngine.UIElements.StyleColor(new Color(0.39f, 0.32f, 0.10f, 1f));

            Add(new Label(new LocStrFormatted("⚠")).Width(20.px()));
            Add(new Label(new LocStrFormatted(
                    "This pack uses the legacy load-order convention. The editor " +
                    "will auto-migrate on save: a Definitions/__init__.py is created " +
                    "and inline dependencies(...) calls are removed from source files."))
                .FlexGrow(1f));

            // Hub-link button. Application.OpenURL spawns the OS-default
            // browser, which is the same surface COI uses for its own out-of-
            // game links (mod manager, news, etc.).
            ButtonText hubLink = new ButtonText(new LocStrFormatted("Docs"),
                () => Application.OpenURL(DocsUrl));
            Add(hubLink.MarginLeftRight(2.pt()));

            Add(new ButtonText(new LocStrFormatted("Migrate now"), onMigrate)
                .MarginLeftRight(2.pt()));
            Add(new ButtonText(new LocStrFormatted("Dismiss"), onDismiss)
                .MarginLeftRight(2.pt()));

            // Hidden by default. The editor reveals it for legacy packs.
            // Visible(false) removes the banner from layout (display:none)
            // so it takes no vertical space when hidden — VisibleForRender
            // would still reserve its row.
            this.Visible(false);
        }
    }
}
