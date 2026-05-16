using Mafi;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mafi.Unity.Ui;
using Mafi.Unity.UiToolkit.Component;
using UnityEngine;

namespace ProgramableNetwork.Data.Variables;

[GlobalDependency(RegistrationMode.AsSelf)]
public class VariableWindowController : WindowController<VariableWindow> {

	private readonly VariableManager m_variableManager;
	private readonly IUnityInputMgr m_unityInput;
	private readonly UiContext m_uiContext;

	public VariableWindowController(
		ControllerContext controllerContext,
		IUnityInputMgr unityInput,
		VariableManager variableManager,
		UiContext uiContext
	) : base(
		controllerContext,
		ControllerConfig.InspectorWindow
	) {
		m_variableManager = variableManager;
		m_unityInput = unityInput;
		m_uiContext = uiContext;

		unityInput.RegisterGlobalShortcut(
			m => KeyBindings.FromPrimaryKeys(KbCategory.General, ShortcutMode.Game, KeyCode.LeftControl, KeyCode.N),
			this);
	}

	protected override VariableWindow CreateWindow() {
		return new VariableWindow(Context, m_variableManager, m_uiContext, m_unityInput);
	}
}
