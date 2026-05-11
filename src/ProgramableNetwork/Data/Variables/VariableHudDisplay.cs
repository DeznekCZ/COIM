using Mafi;
using Mafi.Base;
using Mafi.Core.Research;
using Mafi.Localization;
using Mafi.Unity.InputControl;
using Mafi.Unity.Ui;
using Mafi.Unity.Ui.Hud;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Contexts;

namespace ProgramableNetwork.Data.Variables;

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
	private readonly VariableManager m_variableManager;
	private readonly UiContext m_context;
	private readonly VariableWindowController m_windowController;
	private readonly ResearchManager m_researchManager;
	private UiComponent m_statusBar;

	public VariableHudDisplay(
		UiContext context,
		VariableManager variableManager,
		VariableWindowController windowController,
		ResearchManager researchManager)
	{
		m_context = context;
		m_variableManager = variableManager;
		m_windowController = windowController;
		m_researchManager = researchManager;

		context.UiRoot.Schedule
			.Execute(() => {
				m_statusBar = context.UiRoot.GetLayer(UiLayer.STATUS_BAR)
					.FirstOrDefault(c => c.RootElement.name == "StatusBarContainer");
			})
			.Every(1000)
			.Until(() => {
				if (m_statusBar != null) {
					initUi();
					return true;
				}
				return false;
			});
	}
	private void initUi() {
		ButtonText button = new ButtonText(NewTr.Inspector.NetworkVariablesTitle)
			.OnClick(() => m_windowController.ActivateSelf());
		button.LaterText(() => NewTr.Inspector.NetworkVariablesTitle, button, (b, v) => b.Value(v));

		Display countDisplay = new Display("0".AsLoc()).Width(40.px()).Fill();
		countDisplay.TextCenterMiddle();
		countDisplay.ObserveValue(() => m_variableManager.AllVariables.Count);

		PanelRow root = new PanelRow(noBolts: true)
			.PanelStyleHud()
			.BodyAdd(c => c
				.Padding(left: 4, right: 4, top: 4, bottom: 4)
				.Gap(4)
				.Width(Px.Auto), button, countDisplay);

		if (m_researchManager.TryGetResearchNode(Ids.Research.Datacenter, out ResearchNode node)) {
			UiComponent validator = new UiComponent()
				.AbsolutePosition(0, 0);
			m_context.UiRoot.AddComponent(validator);
			root.ObserveVisible(validator, () => node.State == ResearchNodeState.Researched);
		}

		try {
			UiComponent statusBar = m_context.UiRoot.GetLayer(UiLayer.STATUS_BAR);
			UiComponent statusBarContainer = statusBar.First(c => c.RootElement.name == "StatusBarContainer");
			UiComponent _0 = statusBarContainer.First(c => c.RootElement.name == "Row");
			UiComponent _1 = _0.First(c => c.RootElement.name == "StatusBar");
			UiComponent component = _1.Where(c => c.RootElement.name == "Column").Skip(2).First();
			component.Add(root);
			component.OverflowVisible();
			root.AbsolutePosition(bottom: -24, right: 20);
			root.Height(36.px());
			root.Width(200.px());
		} catch (Exception e) {
			// Fallback if the query fails for some reason — better to have a misplaced panel than no panel at all.
			Log.Error(e.Message + "\n" + e.StackTrace);
			m_context.UiRoot.AddComponent(root);
			const float TOP_PX   = 110f;
			const float RIGHT_PX = 10f;
			root.AbsolutePosition(top: TOP_PX.px(), right: RIGHT_PX.px());
		}
	}
}

public static class UiRootExtensions {

	extension(UiRoot root) {
		public UiLayerContainer GeneralLayer => root.GetLayer(UiLayer.GENERAL);
		public UiLayerContainer GetLayer(UiLayer layer) {
			return root.GetType()!
				.GetMethod("getOrCreateContainer", BindingFlags.Instance | BindingFlags.NonPublic)!
				.Invoke(root, [layer]) as UiLayerContainer;
		}
	}

}
