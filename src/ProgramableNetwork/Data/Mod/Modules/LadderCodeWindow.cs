using Mafi;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using UnityEngine.UIElements;
using Button = Mafi.Unity.UiToolkit.Library.Button;
using Label = UnityEngine.UIElements.Label;
using TextField = UnityEngine.UIElements.TextField;

namespace ProgramableNetwork;

// PLC-LAD statement list — the text half of the editor, in its own window so it
// can sit beside the ladder rather than under it.
//
//   ┌──────────────────────────────────────────────────┐
//   │ NETWORK 1  // start / stop                       │
//   │   A(                                             │ ← the program text
//   │   O    I.A                                       │
//   │   ...                                            │
//   ├──────────────────────────────────────────────────┤
//   │ red error strip (hidden when the program parses) │
//   ├──────────────────────────────────────────────────┤
//   │ 1 network · 4 elements · needs Basic             │
//   └──────────────────────────────────────────────────┘
//   ┌──────────────────────────────────────────────────┐
//   │ [Save] [Revert] [Tidy] [Ladder]           [Back] │
//   └──────────────────────────────────────────────────┘
//
// Neither window owns the program — LadderEditSession does.  Typing here calls
// SetText, which reparses and raises Changed, and the ladder window redraws on
// the same event.  The two stay in step without either polling the other.
[GlobalDependency(RegistrationMode.AsSelf)]
public class LadderCodeWindow : Window {

	private static readonly UnityEngine.Color COL_DIM = new UnityEngine.Color32(0x8B, 0x96, 0xA4, 0xFF);
	private static readonly UnityEngine.Color COL_BAD = new UnityEngine.Color32(0xE2, 0x57, 0x4C, 0xFF);
	private static readonly UnityEngine.Color COL_BAD_BG = new UnityEngine.Color32(0x3A, 0x1E, 0x1C, 0xFF);

	private readonly LadderCodeWindowController m_controller;
	private readonly LadderEditSession m_session;
	private readonly TextField m_source;
	private readonly Label m_errorLabel;
	private readonly VisualElement m_errorStrip;
	private readonly Label m_statsLabel;

	private bool m_subscribed;

	public LadderCodeWindow(ControllerContext context, LadderCodeWindowController controller, LadderEditSession session)
		: base("PLC-LAD: Statement List".ToDoLoc(), addFullscreenButton: true) {
		m_controller = controller;
		m_session = session;

		WindowSize(560.px(), 520.px());
		MakeMovable();
		// Lets the player pin this beside the ladder so neither the world nor
		// build mode closes it mid-edit.
		EnablePinning();

		Panel host = AddPanel();
		VisualElement body = host.Body.RootElement;
		body.style.flexGrow = 1;
		body.style.paddingTop = 0;
		body.style.paddingBottom = 0;

		m_source = new TextField();
		m_source.multiline = true;
		m_source.style.flexGrow = 1;
		m_source.style.flexShrink = 1;
		m_source.style.minHeight = 240;
		m_source.style.whiteSpace = WhiteSpace.Normal;
		m_source.RegisterValueChangedCallback(evt => m_session.SetText(evt.newValue));
		body.Add(m_source);

		m_errorStrip = new VisualElement();
		m_errorStrip.style.backgroundColor = new StyleColor(COL_BAD_BG);
		m_errorStrip.style.paddingLeft = 10;
		m_errorStrip.style.paddingRight = 10;
		m_errorStrip.style.paddingTop = 5;
		m_errorStrip.style.paddingBottom = 5;
		m_errorStrip.style.display = DisplayStyle.None;
		m_errorStrip.style.flexShrink = 0;
		body.Add(m_errorStrip);

		m_errorLabel = new Label("");
		m_errorLabel.style.color = new StyleColor(COL_BAD);
		m_errorLabel.style.whiteSpace = WhiteSpace.Normal;
		m_errorStrip.Add(m_errorLabel);

		m_statsLabel = new Label("");
		m_statsLabel.style.color = new StyleColor(COL_DIM);
		m_statsLabel.style.fontSize = 11;
		m_statsLabel.style.paddingLeft = 10;
		m_statsLabel.style.paddingTop = 3;
		m_statsLabel.style.paddingBottom = 3;
		m_statsLabel.style.flexShrink = 0;
		body.Add(m_statsLabel);

		ButtonText saveButton = new ButtonText(Button.Primary, "Save".ToDoLoc())
			.OnClick(() => {
				if (m_session.CanCompile()) {
					m_controller.Save(m_session.Text);
				}
			});
		ButtonText revertButton = new ButtonText(Button.General, "Revert".ToDoLoc())
			.OnClick(() => m_session.ReloadFromModule())
			.MarginLeft(8.px());
		ButtonText tidyButton = new ButtonText(Button.General, "Tidy".ToDoLoc())
			.Tooltip("Rewrites the program in canonical form — consistent indentation and spacing. The logic is untouched.".ToDoLoc())
			.OnClick(() => m_session.Tidy())
			.MarginLeft(8.px());
		ButtonText ladderButton = new ButtonText(Button.General, "Ladder".ToDoLoc())
			.Tooltip("Opens the ladder diagram for the same program. Pin either window to keep both on screen.".ToDoLoc())
			.OnClick(() => m_controller.OpenLadder())
			.MarginLeft(8.px());
		ButtonText backButton = new ButtonText(Button.General, "Back".ToDoLoc())
			.OnClick(() => m_controller.Back())
			.MarginLeft(Px.Auto);

		PanelFooterRow footer = new PanelFooterRow().BodyAdd(
			row => row.PaddingTopBottom(6.px())
			          .MinHeight(48.px())
			          .AlignItemsCenter(),
			saveButton, revertButton, tidyButton, ladderButton, backButton);
		body.Add(footer.RootElement);
	}

	// Subscribes to the shared session and paints its current state.  Called
	// whenever the window is (re)activated.
	public void Bind() {
		if (!m_subscribed) {
			m_subscribed = true;
			m_session.Changed += refresh;
		}
		refresh();
	}

	public void OnEditorClosed() {
		if (m_subscribed) {
			m_subscribed = false;
			m_session.Changed -= refresh;
		}
	}

	// Repaints from the session.  The text is pushed with
	// SetValueWithoutNotify so echoing the player's own keystroke back into the
	// field can't re-enter SetText — and, more importantly, can't reset the
	// caret to the start of the line while they are typing.
	private void refresh() {
		if (m_source.value != m_session.Text) {
			m_source.SetValueWithoutNotify(m_session.Text);
		}

		if (!m_session.HasModule) {
			m_statsLabel.text = "(no module bound)";
			hideError();
			return;
		}

		if (m_session.Program != null) {
			ProgramableNetwork.Ladder.LadderCapabilityReport report =
				ProgramableNetwork.Ladder.LadderCapability.Analyze(m_session.Program);
			m_statsLabel.text =
				$"{m_session.Program.Networks.Count} networks · {m_session.Program.ElementCount} elements · "
				+ $"widest {report.WidestNetwork} channels · needs "
				+ $"{ProgramableNetwork.Ladder.LadderLimits.NameOf(report.RequiredTier)} · this module is "
				+ $"{ProgramableNetwork.Ladder.LadderLimits.NameOf(m_session.Tier)}";
		}

		if (m_session.Error != null) {
			showError(m_session.Error);
		} else {
			hideError();
		}
	}

	private void showError(string message) {
		m_errorLabel.text = message ?? "";
		m_errorStrip.style.display = DisplayStyle.Flex;
	}

	private void hideError() {
		m_errorLabel.text = "";
		m_errorStrip.style.display = DisplayStyle.None;
	}
}
