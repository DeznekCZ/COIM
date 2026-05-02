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

	private readonly UiContext m_uiContext;
	private Module m_currentModule;

	public PlcPyCodeEditorWindowController(
		ControllerContext controllerContext,
		UiContext uiContext
	) : base(controllerContext, ControllerConfig.InspectorWindow) {
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

	// Closes the editor and re-opens the controller's inspector so the player
	// lands back where they came from (matches the TrainDesignerWindow
	// OpenInspectorForEntityOnClose behavior at a higher level).  We capture
	// the module's controller before deactivate because m_currentModule may
	// be reset during the close path.
	public void Back() {
		Module module = m_currentModule;
		DeactivateSelf();
		if (module != null && module.Controller != null
			&& m_uiContext.InspectorsManager.TryActivateFor(module.Controller, out var inspector)) {
			m_uiContext.InputMgr.ActivateNewController(inspector);
		}
	}
}
