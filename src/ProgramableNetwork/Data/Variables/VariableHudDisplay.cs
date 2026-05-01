using Mafi;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.Ui;
using Mafi.Unity.Ui.Hud;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.InputControl;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Library.FloatingPanel;
using System.Linq;

namespace ProgramableNetwork.Data.Variables
{
    /// <summary>
    /// Top-of-screen HUD button showing how many network variables are currently
    /// in use and a hover-floater that lists every name + value.  Clicking opens
    /// the existing <see cref="VariableWindow"/>.
    ///
    /// Registered via <see cref="GlobalDependencyAttribute"/> so the DI container
    /// instantiates one at game-load and the constructor docks the panel into
    /// <see cref="UiContext.UiRoot"/> at <see cref="UiLayer.STATUS_BAR"/>.  Lives
    /// in the same layer as the base-game status bar but uses absolute positioning
    /// so it doesn't have to reach inside the (sealed) <c>StatusBarHud</c> — that
    /// avoids any Harmony-style runtime patching.
    /// </summary>
    [GlobalDependency(RegistrationMode.AsSelf, false, false)]
    public class VariableHudDisplay
    {
        private static readonly LocStr s_networkVariablesTitle = Loc.Str(
            "ProgramableNetwork_NetworkVariablesTitle",
            "Network variables", "");

        private readonly VariableManager m_variableManager;
        private readonly UiComponent m_root;

        public VariableHudDisplay(
            UiContext context,
            VariableManager variableManager,
            VariableWindowController windowController)
        {
            m_variableManager = variableManager;

            ButtonText button = new ButtonText(s_networkVariablesTitle)
                .OnClick(() => windowController.ActivateSelf());
            button.LaterText(() => s_networkVariablesTitle, button, (b, v) => b.Value(v));

            Display countDisplay = new Display("0".AsLoc()).Width(40.px());
            countDisplay.TextCenterMiddle();
            countDisplay.ObserveValue(() => m_variableManager.AllVariables.Count);

            // Position the panel so it sits directly below the computing bar
            // on the right edge of the screen.  StatusBarHud's right column
            // stacks its top panel (computing) at translate(-10, -6) followed
            // by the bottom panel (cargo / fleet) — the bottom of that whole
            // stack lands ~100 px from the screen top.  We dock right under
            // it with the same horizontal offset (~10 px from the right edge)
            // so the visual line continues cleanly.  All four numbers are
            // here in one block for easy fine-tuning.
            const float TOP_PX   = 110f;
            const float RIGHT_PX = 10f;
            m_root = new PanelRow(noBolts: true)
                .PanelStyleHud()
                .BodyAdd(c => c.Padding(left: 10, right: 10, top: 4, bottom: 4), button, countDisplay);
            m_root.AbsolutePosition(top: TOP_PX.px(), right: RIGHT_PX.px());

            context.UiRoot.AddComponent(m_root, UiLayer.STATUS_BAR);
        }
    }
}
