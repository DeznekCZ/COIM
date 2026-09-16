using System;
using System.Collections.Generic;
using Mafi;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using ProgramableNetwork.Ladder;
using UnityEngine.UIElements;
using Align = UnityEngine.UIElements.Align;
using Button = Mafi.Unity.UiToolkit.Library.Button;
using Justify = UnityEngine.UIElements.Justify;

// Aliased the same way PlcPyCodeEditorWindow does: this file manipulates raw
// Unity primitives via .style / .text, so the short names mean the UIElements
// types and any Mafi equivalent is written out in full.  (The global house rule
// prefers aliasing the Mafi type instead — followed the sibling editor here so
// the two windows read the same.)
using Label = UnityEngine.UIElements.Label;
using TextField = UnityEngine.UIElements.TextField;
using Wrap = UnityEngine.UIElements.Wrap;

namespace ProgramableNetwork;

// PLC-LAD ladder diagram — the graphical half of the editor.
//
//   ┌────────────────────────────────────────┬─────────────┐
//   │ NETWORK 1  start / stop                │ Selection   │
//   │ ║──┤ ├──┬──┤/├────────[ ]─────( )──║   │ NO contact  │
//   │ ║  └─┤ ├─┘            ↑              ║ │ [NO][NC][P] │
//   ├──────────────────── empty channel ────┤ │ Inputs      │
//   │ red error strip (hidden when parsing) │ │ [I.A][I.B]  │
//   ├────────────────────────────────────────┤ Outputs     │
//   │ 1/2 networks · 4/5 channels · 2/2 br.  │ [Q.A][Q.B]  │
//   └────────────────────────────────────────┴─────────────┘
//
// Every channel the tier allows is drawn whether or not it holds a symbol, so
// the grid shows the player their budget rather than making them discover it.
// Clicking an empty channel starts a placement; clicking a symbol selects it.
//
// NOTHING IS EVER WRITTEN WITH A DEFAULT OPERAND.  A placement is only applied
// once the player picks an address from the operand list, which is built from
// the module's actual pins, its memory bank and its controller's bus pins —
// there is no free-text address field to guess at.
//
// Edits go through LadderEdit, which returns new program TEXT that goes back
// into the shared LadderEditSession, so a click and a keystroke in the
// statement-list window travel exactly the same path.
//
// Children go into `host.Body.RootElement`, not `host.RootElement` — the Mafi
// Panel keeps its background / border / bolts as siblings of the Body (see the
// same note on PlcPyCodeEditorWindow).
[GlobalDependency(RegistrationMode.AsSelf)]
public class LadderEditorWindow : Window {

	// Grid metrics. CELL_W is wide enough for an operand like "B.main.demand"
	// at 11px; WIRE_Y is the symbol row's centre line, which every wire, rail
	// and junction bar aligns to.
	private const int CELL_W = 104;
	private const int CELL_H = 64;
	private const int WIRE_Y = 34;
	private const int RAIL_W = 3;
	private const int WIRE_H = 2;

	private static readonly UnityEngine.Color COL_WIRE = new UnityEngine.Color32(0x59, 0x63, 0x6F, 0xFF);
	private static readonly UnityEngine.Color COL_HOT = new UnityEngine.Color32(0x46, 0xC9, 0x6F, 0xFF);
	private static readonly UnityEngine.Color COL_RAIL = new UnityEngine.Color32(0xB8, 0xC3, 0xD0, 0xFF);
	private static readonly UnityEngine.Color COL_TEXT = new UnityEngine.Color32(0xDB, 0xE2, 0xEA, 0xFF);
	private static readonly UnityEngine.Color COL_DIM = new UnityEngine.Color32(0x8B, 0x96, 0xA4, 0xFF);
	private static readonly UnityEngine.Color COL_BAD = new UnityEngine.Color32(0xE2, 0x57, 0x4C, 0xFF);
	private static readonly UnityEngine.Color COL_BAD_BG = new UnityEngine.Color32(0x3A, 0x1E, 0x1C, 0xFF);
	private static readonly UnityEngine.Color COL_BOX_BG = new UnityEngine.Color32(0x23, 0x2A, 0x33, 0xFF);
	private static readonly UnityEngine.Color COL_SEL_BG = new UnityEngine.Color32(0x3A, 0x33, 0x18, 0xFF);
	private static readonly UnityEngine.Color COL_PEND_BG = new UnityEngine.Color32(0x1C, 0x33, 0x24, 0xFF);
	private static readonly UnityEngine.Color COL_CLEAR = new UnityEngine.Color32(0, 0, 0, 0);
	private static readonly UnityEngine.Color COL_SLOT = new UnityEngine.Color32(0x35, 0x3D, 0x47, 0xFF);
	// The one channel in a row that will actually take the next element.
	private static readonly UnityEngine.Color COL_SLOT_NEXT = new UnityEngine.Color32(0x6E, 0x84, 0x9B, 0xFF);
	private static readonly UnityEngine.Color COL_HINT_BG = new UnityEngine.Color32(0x22, 0x28, 0x2F, 0xFF);

	// What the hint strip says when nothing is hovered.  The whole diagram is a
	// wiring diagram, so the resting text is the wiring rule everything else
	// follows from.
	private const string IDLE_HINT =
		"The green rail on the left is live. Each rung is a cable running right to a coil. "
		+ "A contact is a SWITCH in that cable — its address doesn't flow through it, it just "
		+ "opens and closes it. Hover anything for what it does.";

	private static readonly UnityEngine.Color COL_OP_IN = new UnityEngine.Color32(0x5A, 0xA9, 0xE6, 0xFF);
	private static readonly UnityEngine.Color COL_OP_OUT = new UnityEngine.Color32(0xE0, 0x8A, 0x4C, 0xFF);
	private static readonly UnityEngine.Color COL_OP_MEM = new UnityEngine.Color32(0xB6, 0x78, 0xD8, 0xFF);
	private static readonly UnityEngine.Color COL_OP_BUS = new UnityEngine.Color32(0x4F, 0xC3, 0xC7, 0xFF);

	// Where a pending placement will land.
	private enum PlaceMode {
		None = 0,
		// After an existing element, in series.
		Series = 1,
		// Beside an existing element, as a parallel leg.
		Branch = 2,
		// As the very first element of an empty network.
		First = 3,
		// Beside the WHOLE chain up to the anchor, as a parallel leg.  This is
		// the seal-in shape, and it is why a branch has to be an operator with a
		// scope rather than a row you drop symbols into.
		BranchSpan = 4,
	}

	// Adding an element asks three questions — what, where, which address — and
	// the answers used to live in three different parts of a long scrolling
	// panel, so putting one element in meant hunting for the next control (user:
	// "the 3 clicks are confusing, can you make it like a wizard").
	//
	// The wizard asks them one at a time, in one place, with Back at every step.
	// A step with only one possible answer is skipped rather than shown, so
	// adding a coil (which can only go at the end of the rung) is still two
	// clicks, not three.
	private enum WizardStep {
		Off = 0,
		Kind = 1,
		Where = 2,
		Address = 3,
	}

	// One answer to "where should it go", pre-resolved to the coordinates the
	// document takes so choosing it needs no further interpretation.
	private sealed class WhereOption {
		public string Label;
		public string Hint;
		public PlaceMode Mode;
		public int Network;
		public int From;
		public int To;
	}

	// One drawn symbol, kept so the power repaint and the selection highlight
	// can recolor without a full rebuild.  TraceIndex is the position in
	// LadderMachine.Trace, which is program order across ALL networks — not the
	// per-network Code index the edit operations use.
	private sealed class CellView {
		public VisualElement Root;
		public VisualElement Symbol;
		public VisualElement WireIn;
		public VisualElement WireOut;
		public int TraceIndex;
		public int NetworkIndex;
		public int CodeIndex;
		// Boxes have all four borders; contacts and coils only left/right.
		// Tracked here rather than read back off the style, because a
		// StyleFloat doesn't compare cleanly against a plain number.
		public bool IsBox;
	}

	// A stretch of wire that carries power but doesn't switch it: the lead into
	// a symbol, an empty channel, a junction bar between branch legs.
	//
	// These were the missing half of the power painting.  A trace entry says
	// whether an INSTRUCTION conducted, which colours the wire leaving it — but
	// the wire arriving at a symbol belongs to whatever feeds it, and nothing was
	// tracking that, so every element's input lead stayed dead no matter what the
	// rail was doing.
	private sealed class PowerView {
		public VisualElement Element;
		// Live when ANY of these instructions conducted — several sources is the
		// junction where parallel legs meet, and any one of them lighting it is
		// exactly what a parallel branch means.  Null means "fed straight off the
		// live rail", which is on whenever the module is running.
		public int[] Sources;
	}

	private readonly LadderEditorWindowController m_controller;
	private readonly LadderEditSession m_session;
	private readonly VisualElement m_canvas;
	private readonly Label m_errorLabel;
	private readonly VisualElement m_errorStrip;
	private readonly Label m_statsLabel;
	private readonly List<CellView> m_cellViews = new List<CellView>();
	private readonly List<PowerView> m_powerViews = new List<PowerView>();

	// ---- side panel ----
	private readonly VisualElement m_hintStrip;
	private readonly Label m_hintLabel;

	private readonly Label m_headlineLabel;
	private readonly VisualElement m_paletteRow;
	private readonly VisualElement m_kindRow;
	private readonly VisualElement m_paramRow;
	private readonly TextField m_presetField;
	private readonly VisualElement m_actionsRow;
	private readonly VisualElement m_operandPicker;
	private readonly TextField m_netTitle;

	// Selection, in the coordinates LadderEdit takes.  -1 means nothing picked.
	//
	// The selection is a RANGE, not a point: shift-clicking a second symbol
	// extends it, and a branch can then be thrown around the whole run at once.
	// m_selIndex is the anchor and m_selEnd the moving end, so they are not
	// necessarily in order — selectionFrom / selectionTo sort them.
	private int m_selNetwork = -1;
	private int m_selIndex = -1;
	private int m_selEnd = -1;

	// Pending placement.  While this is set the operand list acts as "place
	// here", and no program text has changed yet.
	private PlaceMode m_placeMode = PlaceMode.None;
	private int m_placeNetwork = -1;
	private int m_placeAnchor = -1;
	// BranchSpan only: the far end of the run the new leg will bypass.
	private int m_placeAnchorEnd = -1;
	private LadderOp m_placeKind = LadderOp.And;

	// Which question the add-element wizard is on. Off means it isn't running.
	private WizardStep m_step = WizardStep.Off;

	// Set when the wizard was entered from the coil position at the right rail.
	// Only a coil can go there, so step 1 offers coils and nothing else — asking
	// "contact or coil?" about a slot that can only hold one of them is the kind
	// of question that made the old panel hard to learn.
	private bool m_coilsOnly;

	// The machine whose trace this window armed.  Saving recompiles the module
	// into a NEW LadderMachine with tracing off, so without this the diagram went
	// permanently cold the moment the player pressed Save.
	private LadderMachine m_tracedMachine;

	// Set while the panel is pushing values into its own fields, so the change
	// callbacks don't read a programmatic update back as a player edit.
	private bool m_updatingPanel;

	private bool m_subscribed;
	private bool m_powerRefreshStarted;

	public LadderEditorWindow(ControllerContext context, LadderEditorWindowController controller, LadderEditSession session)
		: base("PLC-LAD: Ladder Diagram".ToDoLoc(), addFullscreenButton: true) {
		m_controller = controller;
		m_session = session;

		WindowSize(1060.px(), 600.px());
		MakeMovable();
		// Lets the player pin the diagram so it survives build mode and stays
		// put while they work in the statement list beside it.
		EnablePinning();

		// Fill() is what AddBodySingle does and AddPanel does not: the window's
		// Body already grows, but a bare Panel inside it sizes to its content, so
		// the diagram and the side panel occupied the top of the window and left
		// the rest empty — and the address list scrolled off the bottom of a
		// panel far shorter than the window it was in.
		Panel host = AddPanel().Fill();
		VisualElement body = host.Body.RootElement;
		body.style.flexGrow = 1;
		body.style.paddingTop = 0;
		body.style.paddingBottom = 0;

		// ---- Canvas + side panel -------------------------------------------
		VisualElement mainRow = new VisualElement();
		mainRow.style.flexDirection = FlexDirection.Row;
		mainRow.style.flexGrow = 1;
		mainRow.style.flexShrink = 1;
		body.Add(mainRow);

		VisualElement canvasColumn = new VisualElement();
		canvasColumn.style.flexDirection = FlexDirection.Column;
		canvasColumn.style.flexGrow = 1;
		canvasColumn.style.flexShrink = 1;
		mainRow.Add(canvasColumn);

		// Both axes scroll: a Full-tier program is 12 channels wide, which is
		// past the window even at fullscreen.
		ScrollView canvasScroll = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
		canvasScroll.style.flexGrow = 1;
		canvasScroll.style.flexShrink = 1;
		canvasScroll.style.minHeight = 200;
		canvasColumn.Add(canvasScroll);

		m_canvas = new VisualElement();
		m_canvas.style.flexDirection = FlexDirection.Column;
		m_canvas.style.paddingLeft = 10;
		m_canvas.style.paddingTop = 8;
		m_canvas.style.paddingBottom = 8;
		canvasScroll.Add(m_canvas);

		// ---- Error strip ---------------------------------------------------
		m_errorStrip = new VisualElement();
		m_errorStrip.style.backgroundColor = new StyleColor(COL_BAD_BG);
		m_errorStrip.style.paddingLeft = 10;
		m_errorStrip.style.paddingRight = 10;
		m_errorStrip.style.paddingTop = 5;
		m_errorStrip.style.paddingBottom = 5;
		m_errorStrip.style.display = DisplayStyle.None;
		m_errorStrip.style.flexShrink = 0;
		canvasColumn.Add(m_errorStrip);

		m_errorLabel = new Label("");
		m_errorLabel.style.color = new StyleColor(COL_BAD);
		m_errorLabel.style.whiteSpace = WhiteSpace.Normal;
		m_errorStrip.Add(m_errorLabel);

		// ---- Hint strip -----------------------------------------------------
		// Mafi's .Tooltip() cannot work in this window: a tooltip promise waits
		// for its component to be attached to a UiRoot through the UiComponent
		// PARENT CHAIN, and every element here is a raw VisualElement hanging off
		// Panel.Body.RootElement, so that chain is never formed and the tooltip
		// silently never opens.  This strip is the replacement — hovering
		// anything, symbol or button, writes here.  It sits outside the scrolling
		// side panel on purpose, so it can't scroll out of view just as you go
		// looking for it.
		m_hintStrip = new VisualElement();
		m_hintStrip.style.backgroundColor = new StyleColor(COL_HINT_BG);
		m_hintStrip.style.paddingLeft = 10;
		m_hintStrip.style.paddingRight = 10;
		m_hintStrip.style.paddingTop = 4;
		m_hintStrip.style.paddingBottom = 4;
		m_hintStrip.style.flexShrink = 0;
		m_hintStrip.style.minHeight = 34;
		canvasColumn.Add(m_hintStrip);

		m_hintLabel = new Label(IDLE_HINT);
		m_hintLabel.style.color = new StyleColor(COL_TEXT);
		m_hintLabel.style.fontSize = 11;
		m_hintLabel.style.whiteSpace = WhiteSpace.Normal;
		m_hintStrip.Add(m_hintLabel);

		m_statsLabel = new Label("");
		m_statsLabel.style.color = new StyleColor(COL_DIM);
		m_statsLabel.style.fontSize = 11;
		m_statsLabel.style.paddingLeft = 10;
		m_statsLabel.style.paddingTop = 3;
		m_statsLabel.style.paddingBottom = 3;
		m_statsLabel.style.flexShrink = 0;
		canvasColumn.Add(m_statsLabel);

		// ---- Side panel ------------------------------------------------------
		// Scrolls: the operand list is as long as the module has pins, and a
		// Full-tier module with buses and 32 memory bits runs past the window.
		ScrollView side = new ScrollView(ScrollViewMode.Vertical);
		side.style.width = 260;
		side.style.flexShrink = 0;
		side.style.paddingLeft = 10;
		side.style.paddingRight = 10;
		side.style.paddingTop = 8;
		side.style.borderLeftWidth = 1;
		side.style.borderLeftColor = new StyleColor(COL_WIRE);
		mainRow.Add(side);

		m_headlineLabel = new Label("");
		m_headlineLabel.style.color = new StyleColor(COL_DIM);
		m_headlineLabel.style.fontSize = 11;
		m_headlineLabel.style.whiteSpace = WhiteSpace.Normal;
		m_headlineLabel.style.marginBottom = 6;
		side.Add(m_headlineLabel);



		m_kindRow = new VisualElement();
		m_kindRow.style.flexDirection = FlexDirection.Row;
		m_kindRow.style.flexWrap = Wrap.Wrap;
		m_kindRow.style.marginBottom = 4;
		side.Add(m_kindRow);

		m_paramRow = new VisualElement();
		m_paramRow.style.flexDirection = FlexDirection.Row;
		m_paramRow.style.alignItems = Align.Center;
		m_paramRow.style.marginBottom = 4;
		m_paramRow.style.display = DisplayStyle.None;
		side.Add(m_paramRow);

		Label presetLabel = new Label("Preset ");
		presetLabel.style.color = new StyleColor(COL_DIM);
		presetLabel.style.fontSize = 11;
		m_paramRow.Add(presetLabel);

		m_presetField = new TextField();
		m_presetField.style.flexGrow = 1;
		m_presetField.RegisterValueChangedCallback(evt => {
			if (m_updatingPanel) {
				return;
			}
			applyPreset(evt.newValue);
		});
		m_paramRow.Add(m_presetField);

		m_actionsRow = new VisualElement();
		m_actionsRow.style.flexDirection = FlexDirection.Row;
		m_actionsRow.style.flexWrap = Wrap.Wrap;
		m_actionsRow.style.marginBottom = 8;
		side.Add(m_actionsRow);

		// The palette is PERSISTENT and position-free.  Every element type has a
		// place it can only go — a contact extends the chain, a coil belongs at
		// the right rail — so the editor works that out rather than making the
		// player click the right hole first.
		side.Add(sectionLabel("Add element"));
		m_paletteRow = new VisualElement();
		m_paletteRow.style.flexDirection = FlexDirection.Row;
		m_paletteRow.style.flexWrap = Wrap.Wrap;
		m_paletteRow.style.marginBottom = 6;
		side.Add(m_paletteRow);

		m_operandPicker = new VisualElement();
		m_operandPicker.style.flexDirection = FlexDirection.Column;
		side.Add(m_operandPicker);

		side.Add(sectionLabel("Network"));

		m_netTitle = new TextField();
		m_netTitle.style.marginBottom = 4;
		m_netTitle.RegisterValueChangedCallback(evt => {
			if (m_updatingPanel) {
				return;
			}
			applyNetworkTitle(evt.newValue);
		});
		side.Add(m_netTitle);

		VisualElement netActions = new VisualElement();
		netActions.style.flexDirection = FlexDirection.Row;
		netActions.style.flexWrap = Wrap.Wrap;
		side.Add(netActions);

		netActions.Add(smallButton("+ Network",
			"Appends an empty network. Click one of its channels to place the first element.",
			() => {
				cancelPlacement();
				clearSelection();
				if (m_session.Edit(doc => doc.AddNetwork(""))) {
					// The new one is where the player is now working, so the
					// panel's own controls follow it down.
					m_selNetwork = m_session.Document.NetworkCount - 1;
					refreshPanel();
				}
			}));
		// No "delete network" here on purpose — it lives on each network's own
		// header, where it doesn't depend on something being selected.

		// ---- Footer ---------------------------------------------------------
		// Save refuses a program that won't compile for this module's tier, so
		// a player can't silently save a module into a permanent error state.
		ButtonText saveButton = new ButtonText(Button.Primary, "Save".ToDoLoc())
			.OnClick(() => {
				if (m_session.CanCompile()) {
					m_controller.Save(m_session.Text);
				}
			});
		ButtonText revertButton = new ButtonText(Button.General, "Revert".ToDoLoc())
			.OnClick(() => {
				cancelPlacement();
				clearSelection();
				m_session.ReloadFromModule();
			})
			.MarginLeft(8.px());
		ButtonText codeButton = new ButtonText(Button.General, "Code".ToDoLoc())
			.Tooltip("Opens the statement list for the same program in its own window. Pin either window to keep both on screen.".ToDoLoc())
			.OnClick(() => m_controller.OpenCode())
			.MarginLeft(8.px());
		ButtonText backButton = new ButtonText(Button.General, "Back".ToDoLoc())
			.OnClick(() => m_controller.Back())
			.MarginLeft(Px.Auto);

		PanelFooterRow footer = new PanelFooterRow().BodyAdd(
			row => row.PaddingTopBottom(6.px())
			          .MinHeight(48.px())
			          .AlignItemsCenter(),
			saveButton, revertButton, codeButton, backButton);
		body.Add(footer.RootElement);
	}

	// Subscribes to the shared session, paints it, and asks the running machine
	// to record conduction so the canvas can show live power.
	public void Bind() {
		if (!m_subscribed) {
			m_subscribed = true;
			m_session.Changed += refresh;
		}

		ensureTrace();
		// Registered once for the window's lifetime — Bind runs on every
		// re-activation, and re-scheduling here would stack repaints.
		if (!m_powerRefreshStarted) {
			m_powerRefreshStarted = true;
			m_statsLabel.schedule
				.Execute(repaintPower)
				.Every(250);
		}

		refresh();
	}

	// Called by the controller on every close path — stops the module paying
	// for a trace nobody is looking at.  Deliberately NOT named OnClosed, so
	// it can't accidentally hide a member of the Mafi Window base.
	public void OnEditorClosed() {
		if (m_subscribed) {
			m_subscribed = false;
			m_session.Changed -= refresh;
		}
		// Whichever machine we armed, not whichever one is current — a save
		// during the session may have replaced it, and the one still paying for
		// the trace is the one we turned on.
		m_tracedMachine?.EnableTrace(false);
		m_tracedMachine = null;
	}

	// Points the trace at whatever the module is running NOW.
	//
	// Saving routes through a command that recompiles the module, so the machine
	// object the diagram was painting from is thrown away and replaced by one
	// that has never been asked to record anything.  Its Trace is null, which
	// repaintPower reads as "not running" and paints the whole ladder cold — the
	// diagram looked disconnected from the moment you saved it.  Re-arming costs
	// one bool array, and only when the machine actually changed.
	private void ensureTrace() {
		LadderMachine machine = m_session.Module?.CompiledBlock as LadderMachine;
		if (ReferenceEquals(machine, m_tracedMachine)) {
			return;
		}
		m_tracedMachine?.EnableTrace(false);
		m_tracedMachine = machine;
		// Costs one bool array and a store per instruction per tick, which is
		// why it stays off until a diagram is actually being looked at.
		machine?.EnableTrace(true);
	}

	// ---- placement ----
	//
	// A placement is ARMED by the wizard's second step and only written by the
	// third, so the program never changes until the last answer is given. That
	// is also why no element ever appears carrying an address nobody chose.

	private void cancelPlacement() {
		m_placeMode = PlaceMode.None;
		m_placeNetwork = -1;
		m_placeAnchor = -1;
		m_placeAnchorEnd = -1;
	}

	// ---- the add-element wizard ----

	private void startWizard() {
		LadderDocument document = m_session.Document;
		if (document == null || document.NetworkCount == 0) {
			return;
		}
		cancelPlacement();
		m_coilsOnly = false;
		// Which network is never left implicit — this used to be whatever
		// m_selNetwork happened to hold, and -1 quietly meant "the first one".
		if (m_selNetwork < 0 || m_selNetwork >= document.NetworkCount) {
			m_selNetwork = 0;
		}
		m_step = WizardStep.Kind;
		m_placeKind = LadderOp.And;
		refreshPanel();
	}

	// Entry from the output position at the right rail.  Nothing but a coil can
	// go there, so the wizard skips straight past the half of step 1 that would
	// only lead to "that can't go here" — and past step 2 entirely, since the end
	// of the rung is the only place a coil ever lands.
	private void startCoilWizard(int network) {
		LadderDocument document = m_session.Document;
		if (document == null || network < 0 || network >= document.NetworkCount) {
			return;
		}
		m_selNetwork = network;
		m_selIndex = -1;
		m_selEnd = -1;
		cancelPlacement();
		m_coilsOnly = true;
		m_step = WizardStep.Kind;
		m_placeKind = LadderOp.Assign;
		refreshPanel();
		paintSelection();
	}

	// Entry from a free channel in the diagram.  Marking the symbol the channel
	// follows means the wizard's "In series" already points at that spot, so
	// clicking the gap and clicking "Add element…" converge on the same flow
	// rather than being two different mechanisms.
	private void startWizardAt(int network, int anchor) {
		m_selNetwork = network;
		m_selIndex = anchor;
		m_selEnd = anchor;
		startWizard();
		paintSelection();
	}

	private void cancelWizard() {
		m_step = WizardStep.Off;
		m_coilsOnly = false;
		cancelPlacement();
		refreshPanel();
		paintSelection();
	}

	private void wizardBack() {
		if (m_step == WizardStep.Address) {
			// Going back from the address undoes the placement, not just the
			// step — nothing has been written yet, so there is nothing to undo
			// beyond forgetting where it was going.
			cancelPlacement();
			m_step = whereOptions().Count > 1 ? WizardStep.Where : WizardStep.Kind;
		} else {
			m_step = WizardStep.Kind;
		}
		refreshPanel();
		paintSelection();
	}

	// Step 1 answered.  If there is only one place this kind can go, the second
	// question has no content and is skipped.
	private void chooseKind(LadderOp kind) {
		m_placeKind = kind;
		List<WhereOption> options = whereOptions();
		if (options.Count == 0) {
			setHint("There is nowhere to put that yet — add a network first.");
			return;
		}
		if (options.Count == 1) {
			chooseWhere(options[0]);
			return;
		}
		m_step = WizardStep.Where;
		refreshPanel();
	}

	// Step 2 answered.  Kinds that address nothing — NOT, and the timer and
	// counter boxes, which are configured by a preset rather than a pin — have
	// no third question, so they land here and now.
	private void chooseWhere(WhereOption option) {
		m_placeMode = option.Mode;
		m_placeNetwork = option.Network;
		m_placeAnchor = option.From;
		m_placeAnchorEnd = option.To;

		if (!needsAddress(m_placeKind)) {
			commitPlacement(null);
			return;
		}
		m_step = WizardStep.Address;
		refreshPanel();
		paintSelection();
	}

	// Where an element of the current kind could legally go, given what is
	// marked.  Everything the wizard offers comes from here, so an option that
	// isn't in this list can't be chosen by any route.
	private List<WhereOption> whereOptions() {
		List<WhereOption> options = new List<WhereOption>();
		LadderDocument document = m_session.Document;
		if (document == null || document.NetworkCount == 0) {
			return options;
		}

		int network = m_selNetwork >= 0 && m_selNetwork < document.NetworkCount ? m_selNetwork : 0;
		List<LadderInstruction> code = document.Networks[network].Code;
		bool coil = LadderInstruction.IsCoil(m_placeKind);

		int anchor;
		if (coil) {
			// The end of the network: a coil appended after the existing coils
			// becomes another parallel output on the same rung.
			anchor = code.Count - 1;
		} else if (m_selIndex >= 0 && m_selIndex < code.Count
			&& !LadderInstruction.IsCoil(code[m_selIndex].Op)
			&& !LadderInstruction.IsStructural(code[m_selIndex].Op)) {
			anchor = m_selIndex;
		} else {
			anchor = -1;
			for (int i = code.Count - 1; i >= 0; i--) {
				if (!LadderInstruction.IsCoil(code[i].Op)) {
					anchor = i;
					break;
				}
			}
		}

		options.Add(new WhereOption {
			Label = coil ? "At the end of the rung" : "In series",
			Hint = coil
				? "Coils live at the right rail. Another one there is a second thing driven by "
					+ "the same cable."
				: "Adds it into the cable, after the marked symbol, so power has to get through "
					+ "both.",
			Mode = anchor >= 0 ? PlaceMode.Series : PlaceMode.First,
			Network = network,
			From = anchor,
			To = anchor,
		});

		// A bypass needs something to bypass, and a coil can't be on one.
		bool canBypass = !coil
			&& m_selNetwork == network
			&& m_selIndex >= 0
			&& m_selIndex < code.Count
			&& !LadderInstruction.IsCoil(code[m_selIndex].Op)
			&& !LadderInstruction.IsStructural(code[m_selIndex].Op);
		if (canBypass) {
			int from = selectionFrom();
			int to = selectionTo();
			int count = to - from + 1;
			options.Add(new WhereOption {
				Label = count > 1 ? $"Bypass around the {count} marked" : "Bypass around the marked one",
				Hint = "A second cable soldered around the marked stretch, so power gets past if "
					+ "EITHER route is closed.",
				Mode = PlaceMode.BranchSpan,
				Network = network,
				From = from,
				To = to,
			});

			// The seal-in: bypass the whole chain feeding the marked symbol, not
			// just the symbol. Only offered when that is actually a wider span.
			int start = document.GroupStart(network, m_selIndex);
			if (count == 1 && start < from) {
				options.Add(new WhereOption {
					Label = "Bypass around everything up to it",
					Hint = "Goes around the whole chain feeding the marked symbol. This is how a "
						+ "start/stop latch is wired.",
					Mode = PlaceMode.BranchSpan,
					Network = network,
					From = start,
					To = m_selIndex,
				});
			}

			// A branch is not limited to two legs, so if the mark is already
			// inside one, offer to add a THIRD (or fourth...) leg to it.  Wrapping
			// the group's whole contents is what makes the new leg flat; wrapping
			// just the marked leg would nest a second branch inside the first,
			// which draws the same but reads as a different circuit.
			int groupEnd = document.GroupEnd(network, m_selIndex);
			if (count == 1 && groupEnd >= start && groupEnd > from) {
				options.Add(new WhereOption {
					Label = "Another leg on this branch",
					Hint = "Adds one more parallel route to the branch the marked symbol is "
						+ "already part of — a branch can carry any number of legs, not just two.",
					Mode = PlaceMode.BranchSpan,
					Network = network,
					From = start,
					To = groupEnd,
				});
			}
		}
		return options;
	}

	// Boxes must parse with a preset above zero, so one is unavoidable. Ten
	// ticks is a visible, obviously-placeholder value and the field to change it
	// is focused the moment the box lands.
	private static int defaultPresetOf(LadderOp kind) {
		return LadderInstruction.IsBox(kind) ? 10 : 0;
	}

	private int selectionFrom() {
		int end = m_selEnd >= 0 ? m_selEnd : m_selIndex;
		return m_selIndex < end ? m_selIndex : end;
	}

	private int selectionTo() {
		int end = m_selEnd >= 0 ? m_selEnd : m_selIndex;
		return m_selIndex > end ? m_selIndex : end;
	}

	// The one place an element is created.  Called only from the operand list,
	// so the operand is always something the player pointed at.
	private void placeOrAssign(LadderOperand operand) {
		if (m_session.Document == null) {
			return;
		}

		if (m_placeMode == PlaceMode.None) {
			// No placement armed — the click retargets the current selection,
			// which stays selected so the player can keep trying addresses.
			if (m_selNetwork >= 0 && m_selIndex >= 0) {
				int network = m_selNetwork;
				int index = m_selIndex;
				m_session.Edit(doc => doc.SetOperand(network, index, operand));
			}
			return;
		}

		commitPlacement(operand);
	}

	// The one place an element is written into the document.  Everything that
	// adds a symbol funnels through here, so there is a single answer to "what
	// did that click actually do".
	private void commitPlacement(LadderOperand operand) {
		LadderInstruction element = new LadderInstruction(
			m_placeKind, operand, null, LadderCompare.Greater, defaultPresetOf(m_placeKind), null, -1, 0);
		PlaceMode mode = m_placeMode;
		int placeNetwork = m_placeNetwork;
		int anchor = m_placeAnchor;
		int anchorEnd = m_placeAnchorEnd;
		int landsAt = anchor >= 0 ? anchor + 1 : 0;

		bool changed = m_session.Edit(doc => {
			switch (mode) {
			case PlaceMode.First:
				return doc.InsertFirst(placeNetwork, element);
			case PlaceMode.Branch:
			case PlaceMode.BranchSpan:
				return doc.AddParallelBranch(placeNetwork, anchor, anchorEnd, element);
			default:
				return doc.InsertAfter(placeNetwork, anchor, element);
			}
		});

		if (!changed) {
			// The document refused it. Say so instead of appearing to do nothing.
			setHint("That can't go there — a coil needs an address this module can write, "
				+ "and a bypass can't be thrown across only part of an existing one.");
			return;
		}

		m_step = WizardStep.Off;
		m_coilsOnly = false;
		cancelPlacement();
		if (mode == PlaceMode.Series || mode == PlaceMode.First) {
			// Leave the new symbol marked: it is almost always what the next
			// action is about, and a box's preset field is then already showing.
			select(placeNetwork, landsAt);
		} else {
			clearSelection();
		}
	}

	private void applyPreset(string text) {
		if (m_selNetwork < 0 || m_selIndex < 0) {
			return;
		}
		if (!int.TryParse(text, out int preset) || preset <= 0) {
			return;
		}
		int network = m_selNetwork;
		int index = m_selIndex;
		m_session.Edit(doc => doc.SetPreset(network, index, preset));
	}

	private void applyNetworkTitle(string text) {
		if (m_selNetwork < 0) {
			return;
		}
		int network = m_selNetwork;
		m_session.Edit(doc => doc.SetNetworkTitle(network, text));
	}

	private void applyKind(LadderOp kind) {
		if (m_placeMode != PlaceMode.None) {
			// Choosing the shape of the element about to be placed.
			m_placeKind = kind;
			refreshPanel();
			return;
		}
		if (m_selNetwork < 0 || m_selIndex < 0) {
			return;
		}
		// The element stays selected through a retype: changing NO to NC is one
		// step of a fiddle, not the end of one.
		int network = m_selNetwork;
		int index = m_selIndex;
		m_session.Edit(doc => doc.SetKind(network, index, kind));
	}

	// Reorders the marked symbol along its rung.  Only ever moves ONE symbol —
	// with a run marked, "which end moved" has no good answer, so the panel only
	// offers this for a single selection.
	private void moveSelected(int direction) {
		if (m_selNetwork < 0 || m_selIndex < 0) {
			return;
		}
		int network = m_selNetwork;
		int index = m_selIndex;
		if (m_session.Edit(doc => doc.MoveElement(network, index, direction))) {
			// The selection travels with the symbol, so the player can keep
			// nudging it without re-clicking.
			select(network, index + direction);
		} else {
			setHint("It can't move any further that way — a symbol can't cross into or "
				+ "out of a branch, and logic can't move past the end of the rung.");
		}
	}

	private void deleteSelected() {
		if (m_selNetwork < 0 || m_selIndex < 0) {
			return;
		}
		int network = m_selNetwork;
		int index = m_selIndex;
		clearSelection();
		m_session.Edit(doc => doc.Delete(network, index));
	}

	private void select(int network, int index) {
		m_selNetwork = network;
		m_selIndex = index;
		m_selEnd = index;
		paintSelection();
		refreshPanel();
	}

	// Unmarks the symbols but REMEMBERS which network the player was working in.
	//
	// It used to clear the network too, and m_selNetwork == -1 made the wizard
	// fall back to network 0 — so after adding a network, clearing one, or
	// finishing a branch (all of which unmark), "Add element…" silently dropped
	// the next element into the FIRST network however far down the player was
	// actually working.
	private void clearSelection() {
		m_selIndex = -1;
		m_selEnd = -1;
	}

	// Forgets the network as well. Only for the case where there may not be one
	// any more — the program was replaced under us.
	private void clearSelectedNetwork() {
		m_selNetwork = -1;
		m_selIndex = -1;
		m_selEnd = -1;
	}

	// Shift-click extends the selection instead of replacing it — the same
	// modifier ModuleSlotButton and EntityConnectionsView already use.  Extending
	// across networks is meaningless (a branch can't span two of them), so a
	// shift-click in a different network just starts over there.
	private void onCellClicked(int network, int index, bool extend) {
		cancelPlacement();
		// Clicking a symbol ends the wizard.  It used to only move the selection,
		// so the panel went on showing the wizard's question while the symbol you
		// had just clicked sat highlighted behind it — and the address list, the
		// one way to retarget an existing coil, never appeared.  Nothing has been
		// written at this point, so abandoning is free.
		m_step = WizardStep.Off;
		m_coilsOnly = false;
		if (extend && network == m_selNetwork && m_selIndex >= 0) {
			m_selEnd = index;
		} else {
			m_selNetwork = network;
			m_selIndex = index;
			m_selEnd = index;
		}
		paintSelection();
		refreshPanel();
	}

	// What the cursor is over, written to the always-visible strip.  Everything
	// hoverable in this window goes through here — symbols, palette buttons,
	// action buttons, address buttons — because Mafi's tooltips can't attach to
	// raw VisualElements (see the strip's construction).
	private void setHint(string text) {
		m_hintLabel.text = text ?? "";
	}

	private void clearHint() {
		m_hintLabel.text = IDLE_HINT;
	}

	private void setHint(LadderInstruction instruction) {
		string address = instruction.Operand != null ? "  " + instruction.Operand : "";
		string extra = subtitleOf(instruction);
		setHint(LadderInstruction.MnemonicOf(instruction.Op) + address
			+ (extra.Length > 0 ? "  " + extra : "")
			+ " — " + describeKind(instruction.Op) + "  " + explainPosition(instruction.Op));
	}

	// Attaches the hover readout to any element.  One helper so no control can
	// be added without saying what it is.
	private void withHint(VisualElement element, string text) {
		element.RegisterCallback<MouseEnterEvent>(_ => setHint(text));
		element.RegisterCallback<MouseLeaveEvent>(_ => clearHint());
	}

	// ---- rendering ----

	// Repaints the diagram from the session.  A program that parses is always
	// drawn, even when the tier check rejected it — seeing what you wrote is
	// exactly what makes "swap to Extended" actionable.
	private void refresh() {
		if (!m_session.HasModule) {
			m_statsLabel.text = "(no module bound)";
			hideError();
			return;
		}

		// Draw the DRAFT, not the last successful parse. The draft is always
		// structurally sound, so the diagram can't be left showing a stale shape
		// while the statement list shows something else — which is what happened
		// whenever a hand-typed line failed to parse.
		LadderProgram program = m_session.Document?.ToProgram() ?? m_session.Program;
		if (program != null) {
			draw(program);

			LadderCapabilityReport report = LadderCapability.Analyze(program);
			LadderLimits limits = LadderLimits.For(m_session.Tier);
			m_statsLabel.text =
				$"{LadderCapability.EffectiveNetworkCount(program)}/{limits.MaxNetworks} networks · "
				+ $"{report.WidestNetwork}/{limits.MaxChannels} channels · "
				+ $"{report.DeepestNetwork}/{limits.MaxNesting} nesting · "
				+ $"{program.ElementCount} elements · needs {LadderLimits.NameOf(report.RequiredTier)}, "
				+ $"this module is {LadderLimits.NameOf(m_session.Tier)}";
		}

		refreshPanel();

		if (m_session.Error != null) {
			showError(m_session.Error);
		} else {
			hideError();
		}
	}

	private void draw(LadderProgram program) {
		m_canvas.Clear();
		m_cellViews.Clear();
		m_powerViews.Clear();

		// A selection or placement can outlive the program that produced it —
		// the statement list window may have deleted the network out from
		// under us.
		if (m_selNetwork >= program.Networks.Count) {
			clearSelectedNetwork();
		}
		if (m_placeNetwork >= program.Networks.Count) {
			cancelPlacement();
		}

		// Trace indices count every instruction of every network in order,
		// matching how LadderMachine.Scan walks the program.
		int traceBase = 0;

		for (int n = 0; n < program.Networks.Count; n++) {
			LadderNetwork network = program.Networks[n];

			m_canvas.Add(networkHeader(network, n));

			LadderNetworkLayout layout = LadderLayout.Build(network);
			foreach (LadderRungLayout rung in layout.Rungs) {
				m_canvas.Add(buildRung(rung, n, traceBase));
			}
			traceBase += network.Code.Count;
		}

		paintSelection();
		repaintPower();
	}

	// Each network carries its own Add / Clear / Delete.
	//
	// These used to live in the side panel and act on "the selected network",
	// which was a dead end: a network you have just added is empty, so there is
	// no symbol in it to click, so nothing is selected, so the buttons no-op.
	// Add one network too many for the tier and the editor was stuck showing an
	// error with no reachable way to undo it.  Anchoring the actions to the
	// network they act on removes the selection from the equation entirely.
	private VisualElement networkHeader(LadderNetwork network, int index) {
		VisualElement row = new VisualElement();
		row.style.flexDirection = FlexDirection.Row;
		row.style.alignItems = Align.Center;
		row.style.paddingTop = 8;
		row.style.paddingBottom = 2;

		Label header = new Label(network.Title.Length > 0
			? $"NETWORK {network.Number}   {network.Title}"
			: $"NETWORK {network.Number}");
		header.style.color = new StyleColor(COL_DIM);
		header.style.fontSize = 11;
		header.style.marginRight = 10;
		row.Add(header);

		row.Add(smallButton("+ Network",
			"Adds an empty network directly below this one.",
			() => editNetwork(index, (doc, at) => doc.InsertNetworkAfter(at, ""))));
		row.Add(smallButton("Clear",
			"Removes every element from this network but keeps the network and its title.",
			() => editNetwork(index, (doc, at) => doc.ClearNetwork(at))));
		// Networks run top to bottom once per tick, so their order is program
		// order — moving one changes when its rung is evaluated, not just where
		// it appears.
		row.Add(smallButton("Up",
			"Moves this whole network one place earlier. Networks run top to bottom, "
				+ "so this changes when its rung is evaluated.",
			() => editNetwork(index, (doc, at) => doc.MoveNetwork(at, -1))));
		row.Add(smallButton("Down",
			"Moves this whole network one place later.",
			() => editNetwork(index, (doc, at) => doc.MoveNetwork(at, 1))));
		row.Add(smallButton("Delete",
			"Removes this network. The ones below it renumber.",
			() => editNetwork(index, (doc, at) => doc.DeleteNetwork(at))));
		return row;
	}

	// Runs a network-level edit against the CURRENT program rather than the one
	// captured when the header was drawn — a stale capture would address a
	// network that has since moved or gone.
	private void editNetwork(int index, Func<LadderDocument, int, bool> edit) {
		cancelPlacement();
		clearSelection();
		// Acting on a network's own header is also how the player says which one
		// they are working in — otherwise adding a network and then using
		// "Add element…" put the element somewhere else entirely.
		m_selNetwork = index;
		m_session.Edit(doc => edit(doc, index));
	}

	private VisualElement buildRung(LadderRungLayout rung, int networkIndex, int traceBase) {
		// Every channel the module can place is drawn, occupied or not — the
		// grid is the player's budget, so it has to be visible.  A program that
		// already exceeds the tier (opened on too small a module) still draws
		// at its own width rather than being clipped.
		LadderLimits limits = LadderLimits.For(m_session.Tier);
		// The last column belongs to the coils and nothing else, so a rung whose
		// logic has not sprouted a coil yet still reserves it.  Otherwise the
		// grid shifts sideways the moment the first coil appears, and — worse —
		// a full row of contacts would sit in the position the coil has to take.
		int used = rung.Columns < 1 ? 1 : rung.Columns;
		int needed = rung.Coils.Count > 0 ? used : used + 1;
		int columns = needed > limits.MaxChannels ? needed : limits.MaxChannels;
		int coilColumn = columns - 1;

		// One free coil position under the ones already there.  A rung may drive
		// several outputs, and this is where you click to add the next — without
		// it the right rail had nothing clickable at all, so a coil could not be
		// put at the end of the line (user: "the coil is not placeable on end of
		// the line").
		int coilRow = rung.Coils.Count;
		int gridRows = rung.Rows > coilRow + 1 ? rung.Rows : coilRow + 1;

		int width = columns * CELL_W + 2 * RAIL_W + 20;
		int height = gridRows * CELL_H;

		VisualElement grid = new VisualElement();
		grid.style.position = Position.Relative;
		grid.style.height = height;
		grid.style.width = width;
		grid.style.flexShrink = 0;
		grid.style.marginBottom = 4;

		// Left rail is always live; the right rail is neutral.
		VisualElement hot = rail(0, height, COL_HOT);
		withHint(hot, "The live rail. Every rung starts here — this is where the power in the "
			+ "diagram comes from.");
		grid.Add(hot);
		VisualElement neutral = rail(width - RAIL_W - 10, height, COL_RAIL);
		withHint(neutral, "The return rail. A rung has done its job once power reaches this "
			+ "side through a coil.");
		grid.Add(neutral);

		// Where each drawn position gets its power from.  Everything below —
		// input leads, junction bars, empty channels — is coloured off this map,
		// because "is this stretch of wire live" is a question about what feeds
		// it, not about the symbol sitting on it.
		int[,] outTrace = new int[gridRows, columns + 1];
		for (int r = 0; r < gridRows; r++) {
			for (int c = 0; c <= columns; c++) {
				outTrace[r, c] = NO_SOURCE;
			}
		}
		foreach (LadderCell cell in rung.Cells) {
			if (cell.Row < rung.Rows && cell.Column <= columns) {
				outTrace[cell.Row, cell.Column] = traceBase + cell.Index;
			}
		}
		foreach (LadderCell coil in rung.Coils) {
			if (coil.Row < rung.Rows) {
				outTrace[coil.Row, coilColumn] = traceBase + coil.Index;
			}
		}

		// Junction bars for parallel groups.  The opening bar carries whatever
		// enters the group; the closing bar is live if ANY leg made it across,
		// which is what a parallel branch means.
		foreach (LadderBranchSpan span in rung.Branches) {
			int x = RAIL_W + span.Column * CELL_W;
			int top = span.FromRow * CELL_H + WIRE_Y;
			int barHeight = (span.ToRow - span.FromRow) * CELL_H;
			if (barHeight <= 0) {
				continue;
			}
			VisualElement bar = new VisualElement();
			bar.style.position = Position.Absolute;
			bar.style.left = x;
			bar.style.top = top;
			bar.style.width = RAIL_W;
			bar.style.height = barHeight;
			bar.style.backgroundColor = new StyleColor(COL_WIRE);
			withHint(bar, span.IsOpening
				? "The cable splits here: every leg below gets the same power."
				: "The legs rejoin here. Power carries on to the right if ANY of them made it "
					+ "across.");
			grid.Add(bar);

			m_powerViews.Add(new PowerView {
				Element = bar,
				Sources = span.IsOpening
					? sourcesOf(feedTrace(outTrace, span.FromRow, span.Column))
					: legSources(outTrace, span),
			});

		}

		// Stacked coils are one rung driving several outputs, so they hang off a
		// vertical drop at the coil column fed by the logic on row 0.  Without it
		// the second coil was drawn with nothing on its left at all and read as
		// disconnected from the rung (user: the missing line for the output
		// coils).
		if (rung.Coils.Count > 1) {
			int coilFeed = feedTrace(outTrace, 0, coilColumn);
			VisualElement drop = new VisualElement();
			drop.style.position = Position.Absolute;
			drop.style.left = RAIL_W + coilColumn * CELL_W;
			drop.style.top = WIRE_Y;
			drop.style.width = RAIL_W;
			drop.style.height = (rung.Coils.Count - 1) * CELL_H;
			drop.style.backgroundColor = new StyleColor(COL_WIRE);
			withHint(drop, "One rung, several outputs. Every coil hanging off this drop sees the "
				+ "same power, so they all switch together.");
			grid.Add(drop);
			m_powerViews.Add(new PowerView { Element = drop, Sources = sourcesOf(coilFeed) });
		}

		foreach (LadderCell cell in rung.Cells) {
			grid.Add(buildCell(cell, networkIndex, traceBase, isCoil: false, column: cell.Column,
				feed: feedTrace(outTrace, cell.Row, cell.Column)));
		}
		// Coils hug the right rail rather than sitting where the logic happened
		// to end — that's how a ladder is drawn, and it keeps the empty channels
		// between the two as usable slots.
		foreach (LadderCell coil in rung.Coils) {
			grid.Add(buildCell(coil, networkIndex, traceBase, isCoil: true, column: coilColumn,
				feed: feedTrace(outTrace, coil.Row, coilColumn)));
		}

		buildEmptySlots(grid, rung, networkIndex, coilColumn, outTrace);
		grid.Add(buildCoilSlot(networkIndex, coilRow, coilColumn,
			feed: feedTrace(outTrace, 0, coilColumn)));

		// Last, so they sit on top: everything here is absolutely positioned in
		// one container, and hit-testing goes to whatever was added latest.  The
		// handles straddle a column boundary and would otherwise be covered by
		// the cell or empty slot next to them.
		foreach (LadderBranchSpan span in rung.Branches) {
			buildBranchHandles(grid, rung, networkIndex, span);
		}
		return grid;
	}

	// The arrows that reshape a branch in place.
	//
	// Where a branch splits and rejoins, and which leg is drawn on the main line,
	// are properties of the BRANCH rather than of the symbols in it, so they need
	// controls of their own.  The wizard can only wrap a span decided up front,
	// and until these existed there was no way to change your mind afterwards
	// short of deleting the branch and rebuilding it.
	//
	// Each junction bar carries ◄ ►, which slide that end along the rung, and
	// each leg carries ▲ ▼, which change its distance from the main line.  A
	// handle is drawn only when the move is available, so an arrow on screen
	// always does something.
	private void buildBranchHandles(
		VisualElement grid, LadderRungLayout rung, int networkIndex, LadderBranchSpan span) {
		LadderDocument document = m_session.Document;
		if (document == null || span.AnchorIndex < 0) {
			return;
		}

		int x = RAIL_W + span.Column * CELL_W;
		int centreY = span.FromRow * CELL_H + WIRE_Y + (span.ToRow - span.FromRow) * CELL_H / 2;

		if (span.IsOpening) {
			if (document.CanMoveBranchStart(networkIndex, span.AnchorIndex, -1)) {
				grid.Add(branchHandle("◄", x - 20, centreY,
					"Moves the split one symbol to the LEFT: the symbol in front of the branch is "
						+ "pulled inside, onto the top leg, so every other leg bypasses it too.",
					doc => doc.MoveBranchStart(networkIndex, span.AnchorIndex, -1)));
			}
			if (document.CanMoveBranchStart(networkIndex, span.AnchorIndex, 1)) {
				grid.Add(branchHandle("►", x + RAIL_W + 6, centreY,
					"Moves the split one symbol to the RIGHT: the first symbol on the top leg goes "
						+ "back out in front of the branch, where all the legs feed through it.",
					doc => doc.MoveBranchStart(networkIndex, span.AnchorIndex, 1)));
			}
			buildLegHandles(grid, rung, networkIndex, span);
			return;
		}

		if (document.CanMoveBranchEnd(networkIndex, span.AnchorIndex, -1)) {
			grid.Add(branchHandle("◄", x - 20, centreY,
				"Moves the junction one symbol to the LEFT: the last symbol on the top leg comes "
					+ "out of the branch and goes back on the main line, after the legs rejoin.",
				doc => doc.MoveBranchEnd(networkIndex, span.AnchorIndex, -1)));
		}
		if (document.CanMoveBranchEnd(networkIndex, span.AnchorIndex, 1)) {
			grid.Add(branchHandle("►", x + RAIL_W + 6, centreY,
				"Moves the junction one symbol to the RIGHT: the next symbol on the main line is "
					+ "pulled inside the branch, onto the top leg, so the other legs bypass it too.",
				doc => doc.MoveBranchEnd(networkIndex, span.AnchorIndex, 1)));
		}
	}

	// ▲ ▼ on each leg, at its left end where the branch splits.
	//
	// Parallel legs mean the same thing in any order, so this is partly a drawing
	// choice — but only partly: the main line is the leg both junction bars grow
	// along, so promoting a leg to the top is how you pick which one the ◄ ►
	// handles will extend.
	private void buildLegHandles(
		VisualElement grid, LadderRungLayout rung, int networkIndex, LadderBranchSpan opening) {
		LadderDocument document = m_session.Document;
		int closingColumn = closingColumnOf(rung, opening);

		for (int row = opening.FromRow; row <= opening.ToRow; row++) {
			int anchor = legAnchorAt(rung, row, opening.Column, closingColumn);
			if (anchor < 0) {
				continue;
			}
			int y = row * CELL_H + WIRE_Y;
			if (document.CanMoveBranchLeg(networkIndex, opening.AnchorIndex, anchor, -1)) {
				grid.Add(branchHandle("▲", opening.Column * CELL_W + 2 * RAIL_W + 3, y,
					"Moves this leg one row closer to the main line. The branch does the same thing "
						+ "either way — but the top leg is the one the ◄ ► handles grow.",
					doc => doc.MoveBranchLeg(networkIndex, opening.AnchorIndex, anchor, -1)));
			}
			if (document.CanMoveBranchLeg(networkIndex, opening.AnchorIndex, anchor, 1)) {
				grid.Add(branchHandle("▼", opening.Column * CELL_W + 2 * RAIL_W + 17, y,
					"Moves this leg one row further from the main line, swapping it with the leg "
						+ "below.",
					doc => doc.MoveBranchLeg(networkIndex, opening.AnchorIndex, anchor, 1)));
			}
		}
	}

	// The column where the group opened by `opening` closes again.  Both bars of
	// a pair carry the same anchor, which is the only reliable way to match them
	// — two nested groups can span exactly the same rows.
	private static int closingColumnOf(LadderRungLayout rung, LadderBranchSpan opening) {
		foreach (LadderBranchSpan span in rung.Branches) {
			if (!span.IsOpening
				&& span.AnchorIndex == opening.AnchorIndex
				&& span.Column > opening.Column) {
				return span.Column;
			}
		}
		return int.MaxValue;
	}

	// A code index belonging to the leg drawn on `row`: its leftmost symbol
	// inside the group.  -1 when the leg starts with a nested group, which has
	// no cell of its own to point at.
	private static int legAnchorAt(LadderRungLayout rung, int row, int fromColumn, int toColumn) {
		int best = -1;
		int bestColumn = int.MaxValue;
		foreach (LadderCell cell in rung.Cells) {
			if (cell.Row == row
				&& cell.Column >= fromColumn
				&& cell.Column < toColumn
				&& cell.Column < bestColumn) {
				bestColumn = cell.Column;
				best = cell.Index;
			}
		}
		return best;
	}

	private VisualElement branchHandle(
		string glyph, int x, int centreY, string hint, Func<LadderDocument, bool> edit) {
		Label handle = new Label(glyph);
		handle.style.position = Position.Absolute;
		handle.style.left = x;
		handle.style.top = centreY - 9;
		handle.style.width = 14;
		handle.style.height = 18;
		handle.style.fontSize = 11;
		handle.style.color = new StyleColor(COL_SLOT_NEXT);
		handle.style.unityTextAlign = UnityEngine.TextAnchor.MiddleCenter;
		handle.RegisterCallback<MouseUpEvent>(evt => {
			evt.StopPropagation();
			cancelPlacement();
			m_session.Edit(edit);
		});
		withHint(handle, hint);
		return handle;
	}

	// The free coil position at the right rail.
	//
	// It is a slot of its own rather than the last of the row's empty channels
	// because a coil does not obey the no-holes rule the contacts do: it always
	// hugs the right rail, however short the logic feeding it is.  Treating it as
	// an ordinary channel meant it only became clickable once contacts had filled
	// every column up to it, and once a coil existed it stopped being offered
	// entirely — so there was no way to add a second output to a rung.
	private VisualElement buildCoilSlot(int networkIndex, int row, int column, int feed) {
		VisualElement root = new VisualElement();
		root.style.position = Position.Absolute;
		root.style.left = RAIL_W + column * CELL_W;
		root.style.top = row * CELL_H;
		root.style.width = CELL_W;
		root.style.height = CELL_H;
		root.style.flexDirection = FlexDirection.Column;
		root.style.alignItems = Align.Center;

		root.RegisterCallback<MouseUpEvent>(evt => {
			evt.StopPropagation();
			startCoilWizard(networkIndex);
		});
		withHint(root, "The output position. Click it to hang something off the end of this rung — "
			+ "a coil is what turns the power arriving here into an output, a memory bit or a bus "
			+ "value. A rung can drive several, stacked one under the other.");

		VisualElement spacer = new VisualElement();
		spacer.style.height = 16;
		root.Add(spacer);

		VisualElement symbolRow = new VisualElement();
		symbolRow.style.flexDirection = FlexDirection.Row;
		symbolRow.style.alignItems = Align.Center;
		symbolRow.style.height = 28;
		symbolRow.style.width = CELL_W;
		root.Add(symbolRow);

		// The lead in carries whatever the logic on row 0 ended with, so an empty
		// rung shows the rail running all the way to the output position.
		VisualElement leadIn = wire();
		symbolRow.Add(leadIn);
		m_powerViews.Add(new PowerView { Element = leadIn, Sources = sourcesOf(feed) });

		VisualElement placeholder = new VisualElement();
		placeholder.style.width = 26;
		placeholder.style.height = 26;
		placeholder.style.borderLeftWidth = 1;
		placeholder.style.borderRightWidth = 1;
		placeholder.style.borderLeftColor = new StyleColor(COL_SLOT_NEXT);
		placeholder.style.borderRightColor = new StyleColor(COL_SLOT_NEXT);
		placeholder.style.alignItems = Align.Center;
		placeholder.style.justifyContent = Justify.Center;
		Label plus = new Label("+");
		plus.style.fontSize = 13;
		plus.style.color = new StyleColor(COL_SLOT_NEXT);
		placeholder.Add(plus);
		symbolRow.Add(placeholder);

		VisualElement leadOut = wire();
		symbolRow.Add(leadOut);
		m_powerViews.Add(new PowerView { Element = leadOut, Sources = sourcesOf(feed) });
		return root;
	}

	// Marks a grid position as carrying nothing, so it can be told apart from
	// trace index 0 (which is a real instruction).
	private const int NO_SOURCE = -2;
	// Fed straight off the live rail.
	private const int RAIL_SOURCE = -1;

	// What feeds the wire arriving at (row, column): the nearest symbol to its
	// left on the same row, or the rail.
	//
	// A row whose leg starts partway across has nothing to its left — its power
	// comes from the junction, which carries whatever entered the group. Looking
	// the same column up on row 0 gets that, and terminates, because row 0
	// eventually falls through to the rail.
	private static int feedTrace(int[,] outTrace, int row, int column) {
		for (int c = column - 1; c >= 0; c--) {
			if (outTrace[row, c] != NO_SOURCE) {
				return outTrace[row, c];
			}
		}
		if (row != 0) {
			return feedTrace(outTrace, 0, column);
		}
		return RAIL_SOURCE;
	}

	// The last symbol on each leg arriving at a closing junction.
	private static int[] legSources(int[,] outTrace, LadderBranchSpan span) {
		List<int> sources = new List<int>();
		for (int r = span.FromRow; r <= span.ToRow; r++) {
			for (int c = span.Column - 1; c >= 0; c--) {
				if (outTrace[r, c] != NO_SOURCE) {
					sources.Add(outTrace[r, c]);
					break;
				}
			}
		}
		return sources.Count > 0 ? sources.ToArray() : null;
	}

	private static int[] sourcesOf(int trace) {
		return trace < 0 ? null : new[] { trace };
	}

	// Draws the unoccupied channels so the player can always see how much room
	// the module still gives them, and click straight into it.
	//
	// A row only extends as far as it is allowed to: the main line runs to the
	// coil, but a branch leg stops at the column where its group closes —
	// offering slots past that would imply you can extend a leg through the
	// junction, which you can't.
	private void buildEmptySlots(
		VisualElement grid, LadderRungLayout rung, int networkIndex, int coilColumn, int[,] outTrace) {
		int[] lastColumn = new int[rung.Rows];
		int[] anchorIndex = new int[rung.Rows];
		for (int r = 0; r < rung.Rows; r++) {
			lastColumn[r] = -1;
			anchorIndex[r] = -1;
		}

		foreach (LadderCell cell in rung.Cells) {
			if (cell.Row < rung.Rows && cell.Column > lastColumn[cell.Row]) {
				lastColumn[cell.Row] = cell.Column;
				anchorIndex[cell.Row] = cell.Index;
			}
		}

		// The last channel belongs to the coil, always — buildCoilSlot draws it
		// and it is clickable whether or not a coil is there yet.  Contacts stop
		// short of it, which is what keeps the diagram from reflowing sideways
		// the first time a rung gets an output.
		int mainLimit = coilColumn;

		// Only rows the LOGIC occupies can take a contact.  A rung that drives
		// three outputs is three rows tall, but rows 1 and 2 belong to the
		// stacked coils — they are not branches.
		int placeableRows = rung.LogicRows < rung.Rows ? rung.LogicRows : rung.Rows;
		for (int row = 0; row < placeableRows; row++) {
			// A row with no element of its own has nothing to insert after.  The
			// sole exception is a completely empty rung, where row 0 places the
			// network's first element.
			if (anchorIndex[row] < 0 && !(row == 0 && rung.Cells.Count == 0)) {
				continue;
			}
			// A rung is a series of contacts wired end to end — it cannot have a
			// hole in it, so a new element always lands in the FIRST free channel
			// of its row, never in whichever one was clicked.  Only that one is
			// live; the rest are drawn as remaining capacity so the player can
			// still see the module's budget, but they don't accept a click and
			// so can't promise a landing spot the ladder can't honour.
			int limit = rowLimit(rung, row, mainLimit);
			int next = lastColumn[row] + 1;
			for (int column = next; column < limit; column++) {
				grid.Add(buildEmptySlot(
					networkIndex, anchorIndex[row], row, column, isNext: column == next,
					feed: feedTrace(outTrace, row, column)));
			}
		}
	}

	// How far right a row may be filled.  Row 0 is the main line and runs to
	// `mainLimit`; any other row belongs to a branch and stops at the innermost
	// group that closes over it.
	private static int rowLimit(LadderRungLayout rung, int row, int mainLimit) {
		if (row == 0) {
			return mainLimit;
		}
		int limit = mainLimit;
		foreach (LadderBranchSpan span in rung.Branches) {
			if (!span.IsOpening
				&& row >= span.FromRow
				&& row <= span.ToRow
				&& span.Column < limit) {
				limit = span.Column;
			}
		}
		return limit;
	}

	// An empty channel: the wire running through it, plus an outline showing a
	// symbol could go here.
	//
	// `isNext` is the first free channel of the row — the one an element would
	// actually land in.  It gets the brighter outline, a "+" and the click
	// handler; the channels past it are drawn dim and inert, because they show
	// remaining capacity rather than a place you can put something.
	//
	// Clicking ARMS a placement — it does not place anything until an address is
	// chosen.
	private VisualElement buildEmptySlot(
		int networkIndex, int anchorIndex, int row, int column, bool isNext, int feed) {
		VisualElement root = new VisualElement();
		root.style.position = Position.Absolute;
		root.style.left = RAIL_W + column * CELL_W;
		root.style.top = row * CELL_H;
		root.style.width = CELL_W;
		root.style.height = CELL_H;
		root.style.flexDirection = FlexDirection.Column;
		root.style.alignItems = Align.Center;

		if (isNext) {
			// Clicking the free channel is just another way into the wizard: it
			// marks the symbol this position follows, so "In series" already
			// means here and the player only answers what and which address.
			root.RegisterCallback<MouseUpEvent>(evt => {
				evt.StopPropagation();
				startWizardAt(networkIndex, anchorIndex);
			});
			withHint(root, "Free length of cable. Click it to add something here — it walks you "
				+ "through what to add and which address it uses.");

			// Only the live slot lights up: it is the one the pending element
			// will actually occupy.
			if (m_placeMode != PlaceMode.None
				&& m_placeNetwork == networkIndex
				&& m_placeAnchor == anchorIndex) {
				root.style.backgroundColor = new StyleColor(COL_PEND_BG);
			}
		} else {
			withHint(root, "Cable this rung could still use. Nothing can be dropped in the "
				+ "middle of a run, so it fills from the left.");
		}

		VisualElement spacer = new VisualElement();
		spacer.style.height = 16;
		root.Add(spacer);

		VisualElement symbolRow = new VisualElement();
		symbolRow.style.flexDirection = FlexDirection.Row;
		symbolRow.style.alignItems = Align.Center;
		symbolRow.style.height = 28;
		symbolRow.style.width = CELL_W;
		root.Add(symbolRow);

		// An empty channel switches nothing, so both halves carry whatever fed
		// the row.  In an empty network that is the rail itself, which is why a
		// network with nothing in it still shows a live line all the way across.
		VisualElement leadIn = wire();
		symbolRow.Add(leadIn);
		m_powerViews.Add(new PowerView { Element = leadIn, Sources = sourcesOf(feed) });

		UnityEngine.Color outline = isNext ? COL_SLOT_NEXT : COL_SLOT;
		VisualElement placeholder = new VisualElement();
		placeholder.style.width = 26;
		placeholder.style.height = 26;
		placeholder.style.borderLeftWidth = 1;
		placeholder.style.borderRightWidth = 1;
		placeholder.style.borderTopWidth = 1;
		placeholder.style.borderBottomWidth = 1;
		placeholder.style.borderLeftColor = new StyleColor(outline);
		placeholder.style.borderRightColor = new StyleColor(outline);
		placeholder.style.borderTopColor = new StyleColor(outline);
		placeholder.style.borderBottomColor = new StyleColor(outline);
		placeholder.style.alignItems = Align.Center;
		placeholder.style.justifyContent = Justify.Center;
		if (isNext) {
			Label plus = new Label("+");
			plus.style.fontSize = 13;
			plus.style.color = new StyleColor(COL_SLOT_NEXT);
			placeholder.Add(plus);
		}
		symbolRow.Add(placeholder);

		VisualElement leadOut = wire();
		symbolRow.Add(leadOut);
		m_powerViews.Add(new PowerView { Element = leadOut, Sources = sourcesOf(feed) });
		return root;
	}

	private VisualElement rail(int x, int height, UnityEngine.Color color) {
		VisualElement bar = new VisualElement();
		bar.style.position = Position.Absolute;
		bar.style.left = x;
		bar.style.top = 0;
		bar.style.width = RAIL_W;
		bar.style.height = height;
		bar.style.backgroundColor = new StyleColor(color);
		return bar;
	}

	// A cell is: operand label on top, symbol row in the middle (wire — symbol
	// — wire), sub-label underneath.  Everything is plain VisualElements and
	// borders; the only glyphs used are ones any font has ("/", "S", "R", "P"),
	// so nothing depends on box-drawing characters being present.
	private VisualElement buildCell(
		LadderCell cell, int networkIndex, int traceBase, bool isCoil, int column, int feed) {
		LadderInstruction instruction = cell.Instruction;

		VisualElement root = new VisualElement();
		root.style.position = Position.Absolute;
		root.style.left = RAIL_W + column * CELL_W;
		root.style.top = cell.Row * CELL_H;
		root.style.width = CELL_W;
		root.style.height = CELL_H;
		root.style.flexDirection = FlexDirection.Column;
		root.style.alignItems = Align.Center;

		// MouseUpEvent, not ClickEvent: it's what the rest of the mod's UI uses
		// (ModuleSlotButton, BusSlotButton, EntityConnectionsView), so it is
		// known to work against this Unity version.
		int codeIndex = cell.Index;
		root.RegisterCallback<MouseUpEvent>(evt => {
			evt.StopPropagation();
			onCellClicked(networkIndex, codeIndex, evt.shiftKey);
		});
		// MouseEnter / MouseLeave are what Mafi's own UI uses for hover state
		// (BufferSlider, BarChart, FloatingColumn), so they're known to work here.
		root.RegisterCallback<MouseEnterEvent>(_ => setHint(instruction));
		root.RegisterCallback<MouseLeaveEvent>(_ => clearHint());

		Label caption = new Label(captionOf(instruction));
		caption.style.fontSize = 11;
		caption.style.height = 16;
		caption.style.color = new StyleColor(operandColor(instruction));
		root.Add(caption);

		VisualElement symbolRow = new VisualElement();
		symbolRow.style.flexDirection = FlexDirection.Row;
		symbolRow.style.alignItems = Align.Center;
		symbolRow.style.height = 28;
		symbolRow.style.width = CELL_W;
		root.Add(symbolRow);

		VisualElement wireIn = wire();
		symbolRow.Add(wireIn);
		// The lead IN belongs to whatever feeds this position, not to this
		// symbol — a contact that isn't conducting still has live wire up to it.
		m_powerViews.Add(new PowerView { Element = wireIn, Sources = sourcesOf(feed) });

		VisualElement symbol = isCoil ? coilSymbol(instruction) : contactSymbol(instruction);
		symbolRow.Add(symbol);

		VisualElement wireOut = wire();
		symbolRow.Add(wireOut);

		Label sub = new Label(subtitleOf(instruction));
		sub.style.fontSize = 10;
		sub.style.height = 14;
		sub.style.color = new StyleColor(COL_DIM);
		root.Add(sub);

		m_cellViews.Add(new CellView {
			Root = root,
			Symbol = symbol,
			WireIn = wireIn,
			WireOut = wireOut,
			TraceIndex = traceBase + cell.Index,
			NetworkIndex = networkIndex,
			CodeIndex = codeIndex,
			IsBox = drawsAsBox(instruction.Op),
		});
		return root;
	}

	private VisualElement wire() {
		VisualElement w = new VisualElement();
		w.style.flexGrow = 1;
		w.style.height = WIRE_H;
		w.style.backgroundColor = new StyleColor(COL_WIRE);
		return w;
	}

	// Both defer to LadderInstruction so a new opcode can't be classified two
	// different ways in two different layers.
	private static bool isBoxOp(LadderOp op) {
		return LadderInstruction.IsBox(op);
	}

	private static bool isCoilOp(LadderOp op) {
		return LadderInstruction.IsCoil(op);
	}

	// NOT isn't a box in the tiering sense — it costs nothing and needs no
	// preset — but it draws as one, because a contact outline would read as
	// "reads an address" and it doesn't read anything.
	private static bool drawsAsBox(LadderOp op) {
		return LadderInstruction.IsBox(op) || op == LadderOp.Not;
	}

	// Contacts are two vertical bars with a gap — which is literally what the
	// symbol is, so borders draw it exactly.  NC adds a slash, edges a letter.
	private VisualElement contactSymbol(LadderInstruction instruction) {
		if (drawsAsBox(instruction.Op)) {
			return boxSymbol(instruction);
		}

		VisualElement gap = new VisualElement();
		gap.style.width = 26;
		gap.style.height = 26;
		gap.style.borderLeftWidth = 2;
		gap.style.borderRightWidth = 2;
		gap.style.borderLeftColor = new StyleColor(COL_WIRE);
		gap.style.borderRightColor = new StyleColor(COL_WIRE);
		gap.style.alignItems = Align.Center;
		gap.style.justifyContent = Justify.Center;

		string glyph = "";
		if (instruction.Op == LadderOp.AndNot || instruction.Op == LadderOp.OrNot) {
			glyph = "/";
		} else if (instruction.Op == LadderOp.EdgeRising) {
			glyph = "P";
		} else if (instruction.Op == LadderOp.EdgeFalling) {
			glyph = "N";
		}
		if (glyph.Length > 0) {
			Label mark = new Label(glyph);
			mark.style.fontSize = 13;
			mark.style.color = new StyleColor(COL_TEXT);
			gap.Add(mark);
		}
		return gap;
	}

	// Coils reuse the same two-bar element with a heavy corner radius, which
	// reads as the parenthesis pair without needing a font glyph for it.
	private VisualElement coilSymbol(LadderInstruction instruction) {
		VisualElement gap = new VisualElement();
		gap.style.width = 30;
		gap.style.height = 26;
		gap.style.borderLeftWidth = 2;
		gap.style.borderRightWidth = 2;
		gap.style.borderLeftColor = new StyleColor(COL_WIRE);
		gap.style.borderRightColor = new StyleColor(COL_WIRE);
		gap.style.borderTopLeftRadius = 13;
		gap.style.borderBottomLeftRadius = 13;
		gap.style.borderTopRightRadius = 13;
		gap.style.borderBottomRightRadius = 13;
		gap.style.alignItems = Align.Center;
		gap.style.justifyContent = Justify.Center;

		string glyph = instruction.Op == LadderOp.Set ? "S"
			: instruction.Op == LadderOp.Reset ? "R"
			: instruction.Op == LadderOp.AssignNot ? "/"
			: instruction.Op == LadderOp.Toggle ? "T"
			: "";
		if (glyph.Length > 0) {
			Label mark = new Label(glyph);
			mark.style.fontSize = 13;
			mark.style.color = new StyleColor(COL_TEXT);
			gap.Add(mark);
		}
		return gap;
	}

	private VisualElement boxSymbol(LadderInstruction instruction) {
		VisualElement box = new VisualElement();
		box.style.width = 72;
		box.style.height = 28;
		box.style.borderLeftWidth = 2;
		box.style.borderRightWidth = 2;
		box.style.borderTopWidth = 2;
		box.style.borderBottomWidth = 2;
		box.style.borderLeftColor = new StyleColor(COL_WIRE);
		box.style.borderRightColor = new StyleColor(COL_WIRE);
		box.style.borderTopColor = new StyleColor(COL_WIRE);
		box.style.borderBottomColor = new StyleColor(COL_WIRE);
		box.style.backgroundColor = new StyleColor(COL_BOX_BG);
		box.style.alignItems = Align.Center;
		box.style.justifyContent = Justify.Center;

		Label name = new Label(LadderInstruction.MnemonicOf(instruction.Op));
		name.style.fontSize = 11;
		name.style.color = new StyleColor(COL_TEXT);
		box.Add(name);
		return box;
	}

	// ---- side panel ----

	private Label sectionLabel(string text) {
		Label label = new Label(text);
		label.style.color = new StyleColor(COL_TEXT);
		label.style.fontSize = 11;
		label.style.marginTop = 8;
		label.style.marginBottom = 4;
		return label;
	}

	private VisualElement smallButton(string text, string tooltip, Action onClick) {
		ButtonText button = new ButtonText(Button.General, text.ToDoLoc())
			.OnClick(onClick)
			.MarginRight(4.px());
		VisualElement element = button.RootElement;
		element.style.marginBottom = 4;
		withHint(element, tooltip);
		return element;
	}

	private VisualElement operandButton(LadderOperand operand) {
		ButtonText button = new ButtonText(Button.General, operand.ToString().ToDoLoc())
			.OnClick(() => placeOrAssign(operand))
			.MarginRight(4.px());
		VisualElement element = button.RootElement;
		element.style.marginBottom = 4;
		withHint(element, operand + " — " + explainOperand(operand));
		return element;
	}

	// Rebuilds the panel for the current selection or pending placement.
	// m_updatingPanel keeps the field callbacks quiet while values are pushed
	// in, so restoring a value can't be mistaken for the player editing it.
	private void refreshPanel() {
		m_updatingPanel = true;
		try {
			m_kindRow.Clear();
			m_actionsRow.Clear();
			m_paletteRow.Clear();
			m_operandPicker.Clear();

			if (m_step != WizardStep.Off) {
				buildWizard();
			} else {
				buildSelectionPanel();
				m_paletteRow.Add(smallButton("Add element…",
					"Walks you through it one question at a time: what to add, where it goes, "
						+ "and which address it uses.",
					startWizard));
				// The address list stays available outside the wizard for one job
				// only: retargeting what is already marked.
				if (selectedInstruction() != null && selectionFrom() == selectionTo()) {
					buildOperandPicker();
				}
			}

			LadderDocument document = m_session.Document;
			int network = m_placeMode != PlaceMode.None ? m_placeNetwork : m_selNetwork;
			if (document != null && network >= 0 && network < document.NetworkCount) {
				m_netTitle.SetValueWithoutNotify(document.Networks[network].Title);
			} else {
				m_netTitle.SetValueWithoutNotify("");
			}
		} finally {
			m_updatingPanel = false;
		}
	}

	// One question on screen at a time, always in the same place, always with a
	// way back. Nothing is written until the last answer.
	private void buildWizard() {
		m_paramRow.style.display = DisplayStyle.None;

		switch (m_step) {
		case WizardStep.Kind:
			// The network is named at every step. Nothing else on screen says
			// where the element is about to land, and getting that wrong is
			// invisible until you look at the diagram again.
			m_headlineLabel.text = m_coilsOnly
				? $"Step 1 of 2 — what should {wizardNetworkName()} drive?"
				: $"Step 1 of 3 — what do you want to add to {wizardNetworkName()}?";
			buildWizardKinds();
			break;
		case WizardStep.Where:
			m_headlineLabel.text = $"Step 2 of 3 — where should the {describeShort(m_placeKind)} go?";
			foreach (WhereOption option in whereOptions()) {
				WhereOption captured = option;
				m_paletteRow.Add(smallButton(option.Label, option.Hint, () => chooseWhere(captured)));
			}
			break;
		default:
			m_headlineLabel.text = m_coilsOnly
				? $"Step 2 of 2 — which address should the {describeShort(m_placeKind)} drive?"
				: $"Step 3 of 3 — which address should the {describeShort(m_placeKind)} use?";
			buildOperandPicker();
			break;
		}

		if (m_step != WizardStep.Kind) {
			m_actionsRow.Add(smallButton("← Back", "Returns to the previous question.", wizardBack));
		}
		m_actionsRow.Add(smallButton("Cancel",
			"Abandons this. Nothing has been added yet.", cancelWizard));
	}

	// Step 1: every element type this tier can run, in one row — or, when the
	// wizard came from the output position, only the ones that can hang there.
	private void buildWizardKinds() {
		LadderLimits limits = LadderLimits.For(m_session.Tier);

		if (!m_coilsOnly) {
			m_paletteRow.Add(wizardKindButton("NO contact", LadderOp.And));
			m_paletteRow.Add(wizardKindButton("NC contact", LadderOp.AndNot));
			m_paletteRow.Add(wizardKindButton("NOT", LadderOp.Not));
		}

		m_paletteRow.Add(wizardKindButton("Coil", LadderOp.Assign));
		m_paletteRow.Add(wizardKindButton("Coil /", LadderOp.AssignNot));
		m_paletteRow.Add(wizardKindButton("Set", LadderOp.Set));
		m_paletteRow.Add(wizardKindButton("Reset", LadderOp.Reset));
		if (limits.AllowEdges) {
			m_paletteRow.Add(wizardKindButton("Toggle", LadderOp.Toggle));
		}

		if (m_coilsOnly) {
			return;
		}
		if (limits.AllowEdges) {
			m_paletteRow.Add(wizardKindButton("On edge", LadderOp.EdgeRising));
			m_paletteRow.Add(wizardKindButton("Off edge", LadderOp.EdgeFalling));
		}
		if (limits.AllowBoxes) {
			m_paletteRow.Add(wizardKindButton("TON", LadderOp.TimerOn));
			m_paletteRow.Add(wizardKindButton("TOF", LadderOp.TimerOff));
			m_paletteRow.Add(wizardKindButton("TP", LadderOp.TimerPulse));
			m_paletteRow.Add(wizardKindButton("CTU", LadderOp.CountUp));
		}
	}

	// What the wizard is about to write into, as the player sees it labelled on
	// the diagram.
	private string wizardNetworkName() {
		LadderDocument document = m_session.Document;
		int network = m_placeMode != PlaceMode.None ? m_placeNetwork : m_selNetwork;
		if (document == null || network < 0 || network >= document.NetworkCount) {
			return "this network";
		}
		LadderNetwork target = document.Networks[network];
		return target.Title.Length > 0
			? $"NETWORK {target.Number} ({target.Title})"
			: $"NETWORK {target.Number}";
	}

	private VisualElement wizardKindButton(string text, LadderOp kind) {
		return smallButton(text, describeKind(kind) + "\n\n" + explainKind(kind),
			() => chooseKind(kind));
	}

	// A couple of words for the headline — the full sentence is in the hint strip.
	private static string describeShort(LadderOp op) {
		if (LadderInstruction.IsCoil(op)) {
			return "coil";
		}
		if (LadderInstruction.IsBox(op)) {
			return LadderInstruction.MnemonicOf(op) + " box";
		}
		return op == LadderOp.Not ? "NOT" : "contact";
	}

	private void buildSelectionPanel() {
		LadderInstruction selected = selectedInstruction();

		if (selected == null) {
			m_headlineLabel.text =
				"Nothing marked. Click a symbol to edit it, or use \"Add element…\" below. "
				+ "Mark a symbol first if you want to add a bypass around it.";
			m_paramRow.style.display = DisplayStyle.None;
			return;
		}

		int from = selectionFrom();
		int to = selectionTo();
		int count = to - from + 1;

		// Selection state only — one line.  What the symbol DOES is in the hint
		// strip the moment the cursor is over it, and repeating it here just
		// pushed the buttons off the bottom of the panel.
		m_headlineLabel.text = count > 1
			? $"{count} symbols marked. A bypass will go around all of them."
			: "Marked: " + LadderInstruction.MnemonicOf(selected.Op)
				+ (selected.Operand != null ? "  " + selected.Operand : "");

		// Retyping only makes sense for a single element — with a run marked the
		// buttons would silently apply to just the anchor.
		if (count == 1) {
			if (isCoilOp(selected.Op)) {
				m_kindRow.Add(kindButton("=", LadderOp.Assign));
				m_kindRow.Add(kindButton("=N", LadderOp.AssignNot));
				m_kindRow.Add(kindButton("S", LadderOp.Set));
				m_kindRow.Add(kindButton("R", LadderOp.Reset));
				if (LadderLimits.For(m_session.Tier).AllowEdges) {
					m_kindRow.Add(kindButton("T", LadderOp.Toggle));
				}
			} else if (!isBoxOp(selected.Op) && selected.Op != LadderOp.Not) {
				m_kindRow.Add(kindButton("NO", LadderOp.And));
				m_kindRow.Add(kindButton("NC", LadderOp.AndNot));
				if (LadderLimits.For(m_session.Tier).AllowEdges) {
					m_kindRow.Add(kindButton("P", LadderOp.EdgeRising));
					m_kindRow.Add(kindButton("N", LadderOp.EdgeFalling));
				}
			}
		}

		// No branch buttons here on purpose. Branching is one of the answers to
		// "where does it go", so it belongs inside the wizard next to the other
		// answers — having it here as well is what made adding an element a hunt
		// across three parts of the panel.

		// Reordering. Only for a single symbol — with a run marked there is no
		// good answer to "which end moved".
		if (count == 1) {
			m_actionsRow.Add(smallButton("◀ Move",
				"Swaps this symbol with the one before it. Won't cross into or out of a branch.",
				() => moveSelected(-1)));
			m_actionsRow.Add(smallButton("Move ▶",
				"Swaps this symbol with the one after it.",
				() => moveSelected(1)));
		}

		m_actionsRow.Add(smallButton("Delete",
			"Removes this element. A branch left holding one leg collapses back to a series.",
			deleteSelected));

		bool hasPreset = count == 1 && (selected.Op == LadderOp.TimerOn
			|| selected.Op == LadderOp.TimerOff
			|| selected.Op == LadderOp.TimerPulse
			|| selected.Op == LadderOp.CountUp);
		m_paramRow.style.display = hasPreset ? DisplayStyle.Flex : DisplayStyle.None;
		if (hasPreset) {
			m_presetField.SetValueWithoutNotify(selected.Preset.ToString());
		}
	}

	// NOT shapes the rung rather than reading a pin, and a timer or counter is
	// configured by its preset — none of them have an address to pick, so the
	// wizard's third step doesn't apply to them.  (CMP does take two, which is
	// why it isn't offered: it still has to be written in the statement list.)
	private static bool needsAddress(LadderOp kind) {
		return kind != LadderOp.Not && !LadderInstruction.IsBox(kind);
	}

	// The address list, built from what this module actually has: its own pins
	// (extensions included), its memory bank, and its controller's bus pins.
	// Typing an address was the old way and it was guesswork — every entry here
	// is something the player can point at.
	private void buildOperandPicker() {
		m_operandPicker.Clear();

		Module module = m_session.Module;
		if (module == null) {
			return;
		}
		LadderLimits limits = LadderLimits.For(m_session.Tier);

		// Outside the wizard the list retargets what is marked, which is not the
		// same job and has to say so — a bare row of addresses under a wizard
		// that adds things reads as "add another one", and there was no other
		// sign that this is how an output's address gets changed.
		if (m_placeMode == PlaceMode.None) {
			LadderInstruction marked = selectedInstruction();
			if (marked != null) {
				m_operandPicker.Add(sectionLabel(
					$"Change what {LadderInstruction.MnemonicOf(marked.Op)} "
					+ $"{marked.Operand?.ToString() ?? ""} points at:"));
			}
		}

		// Only addresses the target can legally use are offered.  A coil DRIVES
		// its address, and an input pin is driven from outside the module by
		// whatever you wired to it — so a coil simply cannot have one, and the
		// list must not pretend otherwise.  Offering it and then failing the
		// compile is the editor setting a trap.
		LadderOp target = targetKind();
		bool writing = LadderInstruction.IsCoil(target);

		if (writing) {
			Label note = new Label(
				"A coil drives its address, so only things this module can write are listed. "
				+ "Input pins are fed from outside — the ladder can only read those.");
			note.style.color = new StyleColor(COL_DIM);
			note.style.fontSize = 11;
			note.style.whiteSpace = WhiteSpace.Normal;
			note.style.marginTop = 6;
			m_operandPicker.Add(note);
		} else {
			VisualElement inputs = operandGroup("Inputs");
			foreach (ModuleConnectorProto pin in module.EffectiveInputs) {
				inputs.Add(operandButton(LadderOperand.ForInput(pin.Id)));
			}
		}

		VisualElement outputs = operandGroup("Outputs");
		foreach (ModuleConnectorProto pin in module.EffectiveOutputs) {
			outputs.Add(operandButton(LadderOperand.ForOutput(pin.Id)));
		}

		if (limits.AllowMemory) {
			VisualElement memory = operandGroup("Memory bits");
			for (int i = 0; i < LadderOperand.MemoryBitCount; i++) {
				memory.Add(operandButton(LadderOperand.ForMemory(i)));
			}
		}

		if (limits.AllowBus && module.Controller != null) {
			VisualElement bus = null;
			foreach (ControllerBus definition in module.Controller.Buses) {
				if (string.IsNullOrEmpty(definition.Name)) {
					continue;
				}
				for (int i = 0; i < ControllerBus.PinCount; i++) {
					string pinName = definition.PinNames[i];
					if (string.IsNullOrEmpty(pinName)) {
						continue;
					}
					// Bus pins carry a direction too: only a Plc-typed pin may be
					// driven from here, the rest are read-only to a ladder.
					if (writing && definition.PinTypes[i] != ControllerBus.BusPinType.Plc) {
						continue;
					}
					if (bus == null) {
						bus = operandGroup("Bus pins");
					}
					bus.Add(operandButton(LadderOperand.ForBus(definition.Name, pinName)));
				}
			}
		}
	}

	// What the address list is being built for: the element about to be placed,
	// or the one currently marked.
	private LadderOp targetKind() {
		if (m_placeMode != PlaceMode.None) {
			return m_placeKind;
		}
		LadderInstruction selected = selectedInstruction();
		return selected != null ? selected.Op : LadderOp.And;
	}

	private VisualElement operandGroup(string title) {
		m_operandPicker.Add(sectionLabel(title));
		VisualElement row = new VisualElement();
		row.style.flexDirection = FlexDirection.Row;
		row.style.flexWrap = Wrap.Wrap;
		m_operandPicker.Add(row);
		return row;
	}

	private VisualElement kindButton(string text, LadderOp kind) {
		return smallButton(text, describeKind(kind) + " " + explainKind(kind), () => applyKind(kind));
	}

	// The long-form description, told as wiring.
	//
	// A contact is NOT a wire carrying its address' value — that is the mistake
	// everyone makes first.  It is a switch sitting in the rung's cable, and its
	// address is the thing pressing the switch, like a signal on a transistor's
	// gate.  Every description here is written to keep that distinction visible.
	private static string explainKind(LadderOp op) {
		switch (op) {
		case LadderOp.And:
		case LadderOp.Or:
			return "A switch in the cable. Its address is not what flows through it — the "
				+ "address is the finger on the switch. Address on, switch closed, power "
				+ "carries on to the right. Two switches on one cable means both have to be "
				+ "closed.";
		case LadderOp.AndNot:
		case LadderOp.OrNot:
			return "The same switch, wired the other way round: it sits closed by default and "
				+ "its address OPENS it. This is how a stop button is wired in real life — the "
				+ "cable is cut when the signal arrives, not when it goes away.";
		case LadderOp.EdgeRising:
			return "A switch that blips. It remembers what its address read last tick, and "
				+ "closes for exactly one tick when the address goes off to on — then opens "
				+ "again even though the address is still on. Use it when you want one action "
				+ "per press, not an action for as long as it's held.";
		case LadderOp.EdgeFalling:
			return "The same blip, on release: one tick of contact each time the address goes "
				+ "on to off.";
		case LadderOp.Assign:
			return "The end of the cable — think of a lamp. Its address is on while power "
				+ "reaches it and off the moment it doesn't. It remembers nothing: cut the "
				+ "cable and the address goes straight back to zero.";
		case LadderOp.Set:
			return "A lamp with a latch. Power arriving switches the address on, and it stays "
				+ "on after the power is gone. Something has to switch it off again, which is "
				+ "what a Reset is for.";
		case LadderOp.Reset:
			return "Switches its address off when power arrives, and otherwise leaves it "
				+ "alone. The other half of a Set.";
		case LadderOp.TimerOn:
			return "A switch that is slow to close. Power has to be present for PT ticks "
				+ "without interruption before it lets anything through; break the cable at "
				+ "any point and it forgets and starts again.";
		case LadderOp.TimerOff:
			return "A switch that is slow to open. It closes at once when power arrives and "
				+ "stays closed for PT ticks after the power is gone. Use it to hold something "
				+ "running for a while after its trigger stops.";
		case LadderOp.CountUp:
			return "Counts how many separate times power has arrived and closes once that "
				+ "reaches PV. The count is saved with the module, so it survives a reload.";
		case LadderOp.TimerPulse:
			return "Power arriving fires a burst PT ticks long, and that burst finishes no "
				+ "matter what — even if the cable is cut on the very next tick. Use it to turn "
				+ "a flicker into a signal long enough to be useful.";
		case LadderOp.Not:
			return "A crossover in the cable: live becomes dead and dead becomes live, right "
				+ "here. It reads no address at all. Alone at the start of a rung it means "
				+ "\"always on\", because the cable starts dead and this flips it.";
		case LadderOp.AssignNot:
			return "A lamp wired backwards: on while power does NOT reach it, off while it "
				+ "does. Saves putting a crossover in front of an ordinary coil.";
		case LadderOp.Toggle:
			return "A push-button light switch. Each time power arrives, the address flips to "
				+ "the opposite of what it was — arrive once to turn on, again to turn off. "
				+ "Holding the cable live does nothing; only the arrival counts. It keeps its "
				+ "own value in the module's saved state, so it stays right even if something "
				+ "else writes the same address.";
		default:
			return "A switch pressed by a number rather than an on/off pin: it closes while "
				+ "the test between its two addresses holds.";
		}
	}

	// What an address IS, in the same wiring terms.
	private static string explainOperand(LadderOperand operand) {
		switch (operand.Kind) {
		case LadderOperandKind.Input:
			return "an input pin on this module — a signal coming in from the cable you wired "
				+ "to it. Contacts can read it; nothing in the ladder may drive it.";
		case LadderOperandKind.Output:
			return "an output pin on this module. A coil drives it, and it is written once at "
				+ "the end of each scan, so the whole ladder sees a steady value while it runs.";
		case LadderOperandKind.Memory:
			return "a memory bit inside this module — a scratch wire with no pin on the "
				+ "outside. Written through immediately, so a latch can close within one scan.";
		case LadderOperandKind.Bus:
			return "a pin on the controller's bus, shared with the other modules on it. "
				+ "Whether a coil may drive it depends on the pin's direction.";
		default:
			return "a fixed number.";
		}
	}

	// What the element does with the rung's power where it sits — series, branch
	// leg, or output.  This is the part players get wrong, so it is stated
	// explicitly rather than left to be read off the diagram.
	private static string explainPosition(LadderOp op) {
		if (isCoilOp(op)) {
			return "It is at the end of the cable. Everything to its left decides whether "
				+ "power gets here. Several stacked on one rung are just several things wired "
				+ "to the same cable end — not a branch.";
		}
		if (op == LadderOp.Or || op == LadderOp.OrNot) {
			return "It is on a second cable soldered around the one above, so power gets past "
				+ "if EITHER route is closed.";
		}
		return "It is in the middle of the cable: power has to get through it and through "
			+ "everything else on its row.";
	}

	private LadderInstruction selectedInstruction() {
		LadderDocument document = m_session.Document;
		if (document == null || m_selNetwork < 0 || m_selNetwork >= document.NetworkCount) {
			return null;
		}
		List<LadderInstruction> code = document.Networks[m_selNetwork].Code;
		if (m_selIndex < 0 || m_selIndex >= code.Count) {
			return null;
		}
		return code[m_selIndex];
	}

	private static string describeKind(LadderOp op) {
		switch (op) {
		case LadderOp.And:
		case LadderOp.Or:
			return "Switch, closed while its address is ON.";
		case LadderOp.AndNot:
		case LadderOp.OrNot:
			return "Switch wired the other way — closed while its address is OFF.";
		case LadderOp.EdgeRising:
			return "Switch that blips closed for one tick when its address turns on.";
		case LadderOp.EdgeFalling:
			return "Switch that blips closed for one tick when its address turns off.";
		case LadderOp.Assign:
			return "The thing the cable powers — its address copies whatever arrives.";
		case LadderOp.Set:
			return "Powers its address on and leaves it on.";
		case LadderOp.Reset:
			return "Switches its address back off.";
		case LadderOp.TimerOn:
			return "Holds the cable open until power has been steady for PT ticks.";
		case LadderOp.TimerOff:
			return "Keeps the cable live for PT ticks after power is lost.";
		case LadderOp.CountUp:
			return "Closes once the cable has arrived PV separate times.";
		case LadderOp.TimerPulse:
			return "One PT-tick burst of power per arrival, whatever happens after.";
		case LadderOp.Not:
			return "Swaps live for dead in the cable at this point. No address.";
		case LadderOp.AssignNot:
			return "Powers its address when the cable does NOT reach here.";
		case LadderOp.Toggle:
			return "Flips its address on/off each time the cable arrives.";
		default:
			return "Switch driven by a number test rather than an on/off pin.";
		}
	}

	private void paintSelection() {
		// A marked run highlights every symbol in it, and so does the run a
		// pending span-branch is about to bypass — otherwise "branch over 3
		// marked" is a claim the player can't check before committing to it.
		int from = selectionFrom();
		int to = selectionTo();
		int network = m_selNetwork;
		if (m_placeMode == PlaceMode.BranchSpan) {
			network = m_placeNetwork;
			from = m_placeAnchor;
			to = m_placeAnchorEnd;
		}

		foreach (CellView view in m_cellViews) {
			bool selected = view.NetworkIndex == network
				&& view.CodeIndex >= from
				&& view.CodeIndex <= to
				&& from >= 0;
			view.Root.style.backgroundColor = new StyleColor(selected ? COL_SEL_BG : COL_CLEAR);
		}
	}

	private static string captionOf(LadderInstruction instruction) {
		if (LadderInstruction.IsBox(instruction.Op) && instruction.Op != LadderOp.Compare) {
			return "";
		}
		return instruction.Operand?.ToString() ?? "";
	}

	private static string subtitleOf(LadderInstruction instruction) {
		switch (instruction.Op) {
		case LadderOp.TimerOn:
		case LadderOp.TimerOff:
		case LadderOp.TimerPulse:
			return "PT " + instruction.Preset;
		case LadderOp.CountUp:
			return "PV " + instruction.Preset;
		case LadderOp.Compare:
			return LadderInstruction.SymbolOf(instruction.Compare) + " " + instruction.Right;
		default:
			return "";
		}
	}

	private static UnityEngine.Color operandColor(LadderInstruction instruction) {
		LadderOperand operand = instruction.Operand;
		if (operand == null) {
			return COL_DIM;
		}
		switch (operand.Kind) {
		case LadderOperandKind.Input:
			return COL_OP_IN;
		case LadderOperandKind.Output:
			return COL_OP_OUT;
		case LadderOperandKind.Memory:
			return COL_OP_MEM;
		case LadderOperandKind.Bus:
			return COL_OP_BUS;
		default:
			return COL_DIM;
		}
	}

	// Recolors conducting elements green from the running machine's trace.
	// Paints everything cold when the working text has diverged from what the
	// module compiled — the indices would point at the wrong symbols.
	private void repaintPower() {
		// The repaint is scheduled for the window's lifetime, so this is also
		// where a machine swapped in by a save gets picked up — but only while
		// the window is actually bound, or a closed editor would keep the module
		// paying for a trace nobody is looking at.
		if (m_subscribed) {
			ensureTrace();
		}
		LadderMachine machine = m_session.Module?.CompiledBlock as LadderMachine;
		bool[] trace = m_session.MatchesModule ? machine?.Trace : null;

		foreach (CellView view in m_cellViews) {
			bool hot = live(trace, view.TraceIndex);

			UnityEngine.Color color = hot ? COL_HOT : COL_WIRE;
			StyleColor styled = new StyleColor(color);
			view.WireOut.style.backgroundColor = styled;
			view.Symbol.style.borderLeftColor = styled;
			view.Symbol.style.borderRightColor = styled;
			if (view.IsBox) {
				view.Symbol.style.borderTopColor = styled;
				view.Symbol.style.borderBottomColor = styled;
			}
		}

		// Plain wire: input leads, junction bars, empty channels.  A null source
		// list means the rail, which is live whenever the module is running — so
		// an empty network draws a live line straight across, which is exactly
		// what it is.
		foreach (PowerView view in m_powerViews) {
			bool hot = trace != null;
			if (hot && view.Sources != null) {
				hot = false;
				foreach (int source in view.Sources) {
					if (live(trace, source)) {
						hot = true;
						break;
					}
				}
			}
			view.Element.style.backgroundColor = new StyleColor(hot ? COL_HOT : COL_WIRE);
		}
	}

	private static bool live(bool[] trace, int index) {
		return trace != null && index >= 0 && index < trace.Length && trace[index];
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
