using System;
using CustomAssets.Ui.Components;
using Mafi;
using Mafi.Core;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Unity.Ui;
using Mafi.Unity.Ui.Hud;
using Mafi.Unity.UiStatic.Toolbar;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using UnityEngine;

namespace CustomAssets.Ui;

/// Toolbar window holding <see cref="ModSettingsPanel"/> — the settings of every
/// loaded CustomAssets pack.
///
/// Deliberately a normal toolbar window rather than a popup inside the pack editor:
/// the editor's own button is gated on <c>SandboxManager.CanCheat</c>, so in an
/// ordinary game it does not exist, and settings that players are meant to change
/// have to be reachable from the game's own toolbar. This one registers
/// unconditionally.
public sealed class ModSettingsWindow : Window {

    /// Configure.svg is COI's own settings glyph and is already used elsewhere in this
    /// editor, so it is known to resolve. Avoids shipping a custom SVG (which would
    /// pull in the asset-bundle build step this mod deliberately skips).
    private const string IconAssetPath = "Assets/Unity/UserInterface/General/Configure.svg";

    /// Just after the recipe editor's own toolbar slot, so the two mod entries sit
    /// together when both are present.
    private const float ToolbarOrder = -109f;

    public static readonly LocStrFormatted WindowTitle = new LocStrFormatted("Pack Settings");

    private readonly ModSettingsPanel m_panel;

    public ModSettingsWindow() : base(WindowTitle) {
        m_panel = new ModSettingsPanel();

        // Inspector-sized and movable, not immersive fullscreen: this is a settings
        // list a player opens beside the game, not a workspace like the pack editor.
        // Matches TranslationsDialog, the other movable window in this mod.
        MakeMovable();
        WindowSize(720.px(), 600.px());

        // The content goes inside a Panel so it gets the standard window chrome —
        // background, border, bolts. Added straight to Body (no AddBodySingle) so
        // there is exactly one panel, not one wrapped in another.
        Panel content = new Panel();
        content.BodyAdd(c => c.AlignItemsStretch().Padding(4.px()).Gap(3.pt()), m_panel);
        content.FlexGrow(1f);

        Body.AlignItemsStretch()
            .PaddingTop(60.px())
            .PaddingLeftRight(8.px())
            .PaddingBottom(8.px())
            .Gap(3.pt());
        Body.Add(content);
    }

    /// Re-read every pack's config.json. The window instance is created once and
    /// reused, so a file edited between openings would otherwise show stale values.
    public void Reload() {
        m_panel.Reload();
    }

    [GlobalDependency(RegistrationMode.AsEverything, false, false)]
    public sealed class Controller : WindowController<ModSettingsWindow>,
                                     IToolbarItemController,
                                     IUnityInputController,
                                     IHotReloadUi {
        private readonly ToolbarHud m_toolbar;

        /// Always true — unlike the pack editor, this is a player-facing window and
        /// must be present in a normal (non-sandbox) game.
        public bool IsVisible => true;

        public bool DeactivateShortcutsIfNotVisible => false;

        public event Action<IToolbarItemController> VisibilityChanged;

        public Controller(ControllerContext controllerContext, ToolbarHud toolbar)
            : base(controllerContext, null) {
            m_toolbar = toolbar;
            m_toolbar.AddMainMenuButton(
                WindowTitle, this, IconAssetPath, ToolbarOrder,
                _ => new KeyBindings(
                    ShortcutMode.Game,
                    KeyBinding.FromKeys(KbCategory.Windows,
                        KeyCode.LeftControl, KeyCode.LeftShift, KeyCode.O),
                    KeyBinding.Empty(KbCategory.Windows)));
        }

        public void DisposeForHotReload() {
            // Nothing registered on the game loop: the panel reads config.json on
            // demand, so there is no per-tick subscription to unhook.
        }
    }
}
