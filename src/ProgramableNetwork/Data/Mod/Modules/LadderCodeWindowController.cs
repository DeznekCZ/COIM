using Mafi;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Unity.Ui;

namespace ProgramableNetwork;

// Lifecycle for the statement-list window.
//
// Sits in ControllerGroup.Window while the ladder window sits in
// ControllerGroup.Inspector — deliberately.  UnityInputManager deactivates a
// running controller when a new one activates in the SAME group, so sharing a
// group would make the two windows mutually exclusive and there would be no
// "detached" editing at all.  Different groups, and neither closes the other.
//
// DeactivateOnNonUiClick is off for the same reason: an editor the player is
// working in shouldn't vanish because they clicked the world to look at the
// factory they're programming.  Escape and Back still close it.
[GlobalDependency(RegistrationMode.AsSelf)]
public class LadderCodeWindowController : WindowController<LadderCodeWindow> {

	private static readonly ControllerConfig EDITOR_CONFIG = new ControllerConfig {
		DeactivateOnNonUiClick = false,
		AllowInspectorCursor = true,
		Group = ControllerGroup.Window,
		BlockShortcuts = true,
		DisableCameraControl = true,
		BlockCameraControlIfInputWasProcessed = true,
		// The game-speed controller runs inside Mafi's dispatch before any
		// per-controller InputUpdate, so without this Space pauses the game
		// instead of typing a space into the program.
		PreventSpeedControl = true,
	};

	private readonly UiContext m_uiContext;
	private readonly LadderEditSession m_session;
	private readonly LadderEditorWindowController m_ladderController;

	public LadderCodeWindowController(
		ControllerContext controllerContext,
		UiContext uiContext,
		LadderEditSession session,
		LadderEditorWindowController ladderController
	) : base(controllerContext, EDITOR_CONFIG) {
		m_uiContext = uiContext;
		m_session = session;
		m_ladderController = ladderController;
		// Completes the pair. The ladder controller can't take this one in its
		// own constructor (that would be a cycle), so it gets the reference
		// here — this controller is always built second, because it depends on
		// that one.
		ladderController.CodeController = this;
	}

	public void OpenFor(Module module) {
		m_session.BindTo(module);
		if (HasWindow && IsActive) {
			Window.Bind();
		}
		if (!IsActive) {
			ActivateSelf();
		}
	}

	protected override LadderCodeWindow CreateWindow() {
		return new LadderCodeWindow(Context, this, m_session);
	}

	protected override void OnActivate() {
		base.OnActivate();
		Window.Bind();
	}

	// No inspector restore here, unlike the ladder window: closing the text
	// pane while the ladder is still open should leave the player on the
	// ladder, not yank them back to the controller.
	protected override void OnDeactivate() {
		base.OnDeactivate();
		if (HasWindow) {
			Window.OnEditorClosed();
		}
		if (!m_ladderController.IsActive
			&& m_session.Module != null
			&& m_session.Module.Controller != null
			&& m_uiContext.InspectorsManager.TryActivateFor(m_session.Module.Controller, out var inspector)) {
			m_uiContext.InputMgr.ActivateNewController(inspector);
		}
	}

	public void OpenLadder() {
		m_ladderController.OpenFor(m_session.Module);
	}

	public void Save(string program) {
		if (m_session.Module == null) {
			return;
		}
		m_uiContext.InputScheduler.ScheduleInputCmd(new ModuleSetStringFieldCmd(
			m_session.Module.Controller.Id,
			m_session.Module.Id,
			"code",
			program ?? ""));
	}

	public void Back() {
		DeactivateSelf();
	}
}
