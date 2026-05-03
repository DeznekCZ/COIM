using Mafi;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Unity.Ui;

namespace ProgramableNetwork;

// Owns the PLC-PY code-editor window's lifecycle and brokers Save / Back.
// Mirrors the VariableWindowController shape so it can be hung off the
// ControllerInspector via DI and called as
// `inspector.PlcPyCodeEditorWindowController.OpenFor(module)` from the
// custom-field button on the PLC-PY module.  Future PLC flavors (Lua,
// blocks, etc.) get their own controllers so each can carry flavor-
// specific tooling without colliding here.
//
// CurrentModule is captured at OpenFor time and read by the window during
// activate / save / live error refresh.  Re-opening for a different module
// while already active just rebinds and reloads the editor text — saves
// players from having to close-then-reopen if they jump between PLCs.
[GlobalDependency(RegistrationMode.AsSelf)]
public class PlcPyCodeEditorWindowController : WindowController<PlcPyCodeEditorWindow> {

	// Editor-tailored config: same as InspectorWindow but with camera +
	// keyboard shortcuts disabled while the editor is active.  Without
	// these flags, arrow keys / WASD pan the camera under the editor and
	// number-key hotkeys fire toolbar actions, even though the player is
	// trying to type.  The window-level InputUpdate override returns true
	// while the editor has focus to consume Mafi's poll-based dispatch,
	// but Mafi only blocks the camera/shortcut paths when the controller
	// CONFIG asks it to — those paths run separately from the per-tick
	// dispatch loop.
	private static readonly ControllerConfig EDITOR_CONFIG = new ControllerConfig {
		DeactivateOnNonUiClick = true,
		AllowInspectorCursor = true,
		Group = ControllerGroup.Inspector,
		BlockShortcuts = true,
		DisableCameraControl = true,
		BlockCameraControlIfInputWasProcessed = true,
		// Critical: the game-speed controller (Space toggles pause)
		// runs INSIDE Mafi's dispatch BEFORE per-controller InputUpdate
		// gets a chance to swallow keys.  PreventSpeedControl makes the
		// outer dispatcher skip GameSpeedController.InputUpdate while
		// this controller is active, so Space can reach the editor as a
		// regular character.
		PreventSpeedControl = true,
	};

	private readonly UiContext m_uiContext;
	private Module m_currentModule;

	public PlcPyCodeEditorWindowController(
		ControllerContext controllerContext,
		UiContext uiContext
	) : base(controllerContext, EDITOR_CONFIG) {
		m_uiContext = uiContext;
	}

	public Module CurrentModule => m_currentModule;

	public void OpenFor(Module module) {
		m_currentModule = module;
		// If the window already exists (player previously opened it on this
		// or another PLC-PY), repopulate its text in place.  Otherwise the
		// OnActivate hook below seeds it after CreateWindow runs.
		if (HasWindow && IsActive) {
			Window.LoadFromModule(module);
		}
		if (!IsActive) {
			ActivateSelf();
		}
	}

	protected override PlcPyCodeEditorWindow CreateWindow() {
		return new PlcPyCodeEditorWindow(Context, this);
	}

	protected override void OnActivate() {
		base.OnActivate();
		if (m_currentModule != null) {
			Window.LoadFromModule(m_currentModule);
		}
	}

	// Centralized "close → reopen inspector" so every close path (Back
	// button, X button, Escape, click-outside) lands the player back on the
	// PLC-PY's controller inspector.  We capture the module reference up
	// front because the deactivate side-effects below could cause it to be
	// reset before TryActivateFor reads it.
	protected override void OnDeactivate() {
		base.OnDeactivate();
		Module module = m_currentModule;
		if (module != null && module.Controller != null
			&& m_uiContext.InspectorsManager.TryActivateFor(module.Controller, out var inspector)) {
			m_uiContext.InputMgr.ActivateNewController(inspector);
		}
	}

	// Routes the player's edited text into the same command pipeline the
	// inline StringField uses, so multiplayer / replay / undo all stay
	// consistent.  No close on save — the player can keep tweaking and
	// re-saving without losing the editor.
	public void Save(string code) {
		if (m_currentModule == null) {
			return;
		}
		m_uiContext.InputScheduler.ScheduleInputCmd(new ModuleSetStringFieldCmd(
			m_currentModule.Controller.Id,
			m_currentModule.Id,
			"code",
			code ?? ""));
	}

	// Back button — delegated to OnDeactivate, which handles the inspector
	// restore for every close path.
	public void Back() {
		DeactivateSelf();
	}
}
