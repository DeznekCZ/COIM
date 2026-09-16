using Mafi;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Unity.Ui;

namespace ProgramableNetwork;

// Lifecycle for the ladder diagram window.
//
// Stays in ControllerGroup.Inspector while the statement-list window sits in
// ControllerGroup.Window.  That split is load-bearing: UnityInputManager
// deactivates a running controller whenever a new one activates in the SAME
// group, so sharing a group would make the two windows mutually exclusive and
// there would be no detached editing at all.
//
// DeactivateOnNonUiClick is off so the diagram doesn't vanish when the player
// clicks the world to look at the factory they're programming.  Escape and Back
// still close it, and EnablePinning on the window covers build mode.
[GlobalDependency(RegistrationMode.AsSelf)]
public class LadderEditorWindowController : WindowController<LadderEditorWindow> {

	private static readonly ControllerConfig EDITOR_CONFIG = new ControllerConfig {
		DeactivateOnNonUiClick = false,
		AllowInspectorCursor = true,
		Group = ControllerGroup.Inspector,
		BlockShortcuts = true,
		DisableCameraControl = true,
		BlockCameraControlIfInputWasProcessed = true,
		// The game-speed controller runs inside Mafi's dispatch before any
		// per-controller InputUpdate, so without this Space pauses the game.
		PreventSpeedControl = true,
	};

	private readonly UiContext m_uiContext;
	private readonly LadderEditSession m_session;

	// Set by LadderCodeWindowController's constructor rather than injected.
	// The two controllers need to open each other, and constructor injection
	// in both directions is a dependency cycle the container can't resolve —
	// so the code controller, which is built second, wires the back-reference.
	public LadderCodeWindowController CodeController { get; set; }

	public LadderEditorWindowController(
		ControllerContext controllerContext,
		UiContext uiContext,
		LadderEditSession session
	) : base(controllerContext, EDITOR_CONFIG) {
		m_uiContext = uiContext;
		m_session = session;
	}

	public Module CurrentModule => m_session.Module;

	public void OpenFor(Module module) {
		m_session.BindTo(module);
		if (HasWindow && IsActive) {
			Window.Bind();
		}
		if (!IsActive) {
			ActivateSelf();
		}
	}

	protected override LadderEditorWindow CreateWindow() {
		return new LadderEditorWindow(Context, this, m_session);
	}

	protected override void OnActivate() {
		base.OnActivate();
		Window.Bind();
	}

	// Returns the player to the controller inspector — but only if the
	// statement-list window isn't still open, since closing the diagram while
	// the code pane is up shouldn't yank them out of the editor entirely.
	protected override void OnDeactivate() {
		base.OnDeactivate();
		if (HasWindow) {
			Window.OnEditorClosed();
		}

		bool codeStillOpen = CodeController != null && CodeController.IsActive;
		Module module = m_session.Module;
		if (!codeStillOpen
			&& module != null
			&& module.Controller != null
			&& m_uiContext.InspectorsManager.TryActivateFor(module.Controller, out var inspector)) {
			m_uiContext.InputMgr.ActivateNewController(inspector);
		}
	}

	public void OpenCode() {
		CodeController?.OpenFor(m_session.Module);
	}

	// Routes the edited program through the same command pipeline the inline
	// StringField uses, so multiplayer / replay / undo stay consistent.
	public void Save(string program) {
		Module module = m_session.Module;
		if (module == null) {
			return;
		}
		m_uiContext.InputScheduler.ScheduleInputCmd(new ModuleSetStringFieldCmd(
			module.Controller.Id,
			module.Id,
			"code",
			program ?? ""));
	}

	public void Back() {
		DeactivateSelf();
	}
}
