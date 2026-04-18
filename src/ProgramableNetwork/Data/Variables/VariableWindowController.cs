using Mafi;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mafi.Unity.UiToolkit.Component;
using UnityEngine;

namespace ProgramableNetwork.Data.Variables;

[GlobalDependency(RegistrationMode.AsSelf)]
public class VariableWindowController : WindowController<VariableWindow> {

	private readonly VariableManager m_variableManager;

	public VariableWindowController(
		ControllerContext controllerContext,
		IUnityInputMgr unityInput,
		VariableManager variableManager
	) : base(
		controllerContext,
		ControllerConfig.InspectorWindow
	) {
		m_variableManager = variableManager;

		unityInput.RegisterGlobalShortcut(
			m => KeyBindings.FromPrimaryKeys(KbCategory.General, ShortcutMode.Game, KeyCode.LeftControl, KeyCode.N),
			this);
	}

	protected override VariableWindow CreateWindow() {
		return new VariableWindow(Context, m_variableManager);
	}
}
