using System;
using System.Text;
using Mafi;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using ProgramableNetwork.Python;
using UnityEngine.UIElements;
using Button = Mafi.Unity.UiToolkit.Library.Button;

// Alias the ambiguous Label so we can write `new Label()` and mean Unity's
// primitive (which we manipulate via .style / .text directly).  Mafi's
// Label wrapper is still reachable via its full namespace when needed.
using Label = UnityEngine.UIElements.Label;

namespace ProgramableNetwork;

// PLC-PY code editor window.
//
// One Mafi panel hosts the editor surface; everything inside is laid out
// with raw Unity UIElements VisualElements so Mafi's per-panel chrome
// doesn't sprout extra bars between sections.  The layout, top to bottom:
//
//   ┌───────────────────────────┬─────────────────────┐
//   │ ▢ #│ multiline TextField  │ API reference       │ ← editorRow (flex-grow 1)
//   │   │                       │ self                │
//   │   │                       │   self.Input        │
//   │   │                       │   self.Output       │
//   │   │                       │   ...               │
//   ├───────────────────────────┴─────────────────────┤
//   │ red error strip (collapses when empty)          │
//   ├─────────────────────────────────────────────────┤
//   │ tokens / status                                 │
//   ├─────────────────────────────────────────────────┤
//   │ tooltip strip (active identifier doc)           │
//   ├─────────────────────────────────────────────────┤
//   │ [Save]   [Compile]                       [Back] │ ← footer
//   └─────────────────────────────────────────────────┘
//
// Children are added to `host.Body.RootElement` (not `host.RootElement`)
// because the Mafi Panel's outer container has the background / border /
// bolts as siblings of the Body — adding to RootElement directly stacks
// our content alongside those instead of inside, which leaves a big
// empty Body bar above and pushes the editor down.
[GlobalDependency(RegistrationMode.AsSelf)]
public class PlcPyCodeEditorWindow : Window {

	private readonly PlcPyCodeEditorWindowController m_controller;
	// Custom code editor — own caret, selection, undo, clipboard, syntax
	// rendering.  Replaces the dual-layer (transparent TextField + colored
	// overlay) approach so colored text and caret can never drift; both
	// are computed from the same buffer + same monospace font metrics.
	private readonly PlcPyTextEditor m_codeEditor;
	private readonly Label m_lineNumbers;
	private readonly Label m_errorLabel;
	private readonly Label m_statsLabel;
	private readonly Label m_tooltipLabel;
	private readonly VisualElement m_errorStrip;

	// IntelliSense floater state — collapsed by default; opened by '.' or
	// Ctrl+Space.  m_floaterEntries snapshots the active completion list
	// at open time; the visible Labels are rebuilt to match each open so
	// stale rows don't leak between trigger contexts.
	private readonly VisualElement m_floater;
	private readonly VisualElement m_floaterRowsContainer;
	private readonly Label m_floaterHintLabel;
	private readonly System.Collections.Generic.List<Label> m_floaterRows;
	private System.Collections.Generic.List<PlcPySyntax.Completion> m_floaterEntries;
	private int m_floaterSelected;
	private bool m_floaterOpen;
	// Cursor position at which the current completion list was opened —
	// used so Insert can replace the partial token and Refresh can re-
	// filter as the player keeps typing.
	private int m_floaterTriggerPos;
	private string m_floaterTriggerParent;
	// Where the floater is anchored in the body's local coords.  Captured
	// at open time so cursor movement during navigation doesn't reposition
	// the floater (matching most IDE behavior).
	private VisualElement m_floaterParent;

	public PlcPyCodeEditorWindow(ControllerContext context, PlcPyCodeEditorWindowController controller)
		: base("PLC-PY: Code Editor".ToDoLoc(), addFullscreenButton: true) {
		m_controller = controller;

		WindowSize(900.px(), 90.Percent());
		MakeMovable();

		// Single host panel — children go into its Body, not the outer
		// RootElement (see file comment above).  Body uses flexGrow=1 so
		// the editor row can claim leftover vertical space.
		Panel host = AddPanel();
		VisualElement body = host.Body.RootElement;
		body.style.flexGrow = 1;
		body.style.paddingTop = 0;
		body.style.paddingBottom = 0;

		// ---- Editor row: editor (left) + API reference (right) ---------
		VisualElement editorRow = new VisualElement();
		editorRow.style.flexDirection = FlexDirection.Row;
		editorRow.style.flexGrow = 1;
		editorRow.style.flexShrink = 1;
		body.Add(editorRow);

		// ---- Editor side -----------------------------------------------
		// Wrapping container so the line-numbers gutter can sit flush left
		// of the TextField; both grow together within the editor side.
		VisualElement editorBox = new VisualElement();
		editorBox.style.flexDirection = FlexDirection.Row;
		editorBox.style.flexGrow = 1;
		editorBox.style.flexShrink = 1;
		editorBox.style.flexBasis = new StyleLength(new Length(70, LengthUnit.Percent));
		editorRow.Add(editorBox);

		// Line-numbers gutter — clipping container with the line-number
		// label inside as Position.Absolute, so we can translate it
		// vertically to follow the editor's ScrollView without it
		// pushing the layout around.  Width is fixed; height matches
		// the row.  Border-right separates it visually from the editor.
		VisualElement gutter = new VisualElement();
		gutter.style.width = 50;
		gutter.style.flexShrink = 0;
		gutter.style.overflow = Overflow.Hidden;
		gutter.style.borderRightWidth = 1;
		gutter.style.borderRightColor = new StyleColor(new UnityEngine.Color(0.25f, 0.25f, 0.25f, 1f));
		gutter.pickingMode = PickingMode.Ignore;
		editorBox.Add(gutter);

		// Monospace font matches the editor side so a single-digit number on
		// line 1 sits at the same baseline as a triple-digit one on line 999.
		// Padding-top/left match the editor's m_originX/Y (4 px) so line N
		// in the gutter sits at the same Y as line N in the editor.
		m_lineNumbers = new Label("1");
		m_lineNumbers.AddToClassList(Cls.fontMonospace);
		m_lineNumbers.style.position = Position.Absolute;
		m_lineNumbers.style.left = 0;
		m_lineNumbers.style.right = 0;
		m_lineNumbers.style.top = 0;
		m_lineNumbers.style.fontSize = 14;
		m_lineNumbers.style.paddingRight = 6;
		m_lineNumbers.style.paddingLeft = 6;
		m_lineNumbers.style.paddingTop = 4;
		m_lineNumbers.style.paddingBottom = 4;
		m_lineNumbers.style.unityTextAlign = UnityEngine.TextAnchor.UpperRight;
		m_lineNumbers.style.whiteSpace = WhiteSpace.Pre;
		m_lineNumbers.style.color = new StyleColor(new UnityEngine.Color(0.55f, 0.55f, 0.55f, 1f));
		m_lineNumbers.pickingMode = PickingMode.Ignore;
		gutter.Add(m_lineNumbers);

		// Editor — fully custom PlcPyTextEditor; owns its own buffer,
		// caret, selection, undo, mouse + keyboard handling, and
		// rendering.  Wrapped in a ScrollView with both axes enabled
		// because the editor now reports a real intrinsic size via its
		// own UpdateContentSize (minWidth = max-line × charWidth,
		// minHeight = lineCount × lineHeight).  The earlier "no scroll
		// wrapper" workaround was needed only because the editor used
		// to have no intrinsic size — flexGrow=1 in the auto-sized
		// ScrollView contentContainer collapsed it to 0×0.
		ScrollView editorScroll = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
		editorScroll.style.flexGrow = 1;
		editorScroll.style.flexShrink = 1;
		editorBox.Add(editorScroll);

		m_codeEditor = new PlcPyTextEditor();
		m_codeEditor.SyntaxHighlighter = PlcPySyntax.ToRichText;
		editorScroll.Add(m_codeEditor);

		// Sync the line-numbers gutter to the editor's vertical scroll —
		// translate the absolutely-positioned label up by scrollOffset.y
		// so the visible numbers stay aligned with the visible code as
		// the player scrolls.  Translate doesn't affect layout, only
		// painting, so the gutter container stays put while its content
		// shifts.  Horizontal scroll is intentionally NOT mirrored: line
		// numbers should stay anchored to the left even when the editor
		// scrolls right past long lines (matches every code editor).
		editorScroll.verticalScroller.valueChanged += y => {
			m_lineNumbers.style.translate = new StyleTranslate(
				new Translate(0, -y, 0));
		};

		// Floater nav hook — runs as the FIRST step inside the editor's
		// own OnKeyDown via PlcPyTextEditor.KeyDownInterceptor, so Tab /
		// Enter / Up / Down get a chance to drive the IntelliSense popup
		// before the editor would treat them as indent / newline / caret
		// movement.  A separate KeyDownEvent registration wouldn't work:
		// the editor registers its own handler in its constructor, before
		// we'd register ours, and "first registered wins" at TrickleDown.
		m_codeEditor.KeyDownInterceptor = onEditorKeyDown;

		m_codeEditor.OnValueChanged = text => {
			UpdateLineNumbers(text);
			RefreshIdentifierTooltip();
			OnEditorTextChanged(text);
		};

		// Tooltip refresh on caret movement — the editor doesn't expose a
		// caret-changed event explicitly, so piggy-back on KeyUp / MouseUp
		// (the two ways the caret can move without a value change).
		m_codeEditor.RegisterCallback<MouseUpEvent>(_ => RefreshIdentifierTooltip());
		m_codeEditor.RegisterCallback<KeyUpEvent>(_ => RefreshIdentifierTooltip());

		// ---- API reference panel (right side) --------------------------
		// Static list of well-known identifiers + brief docs, populated
		// from PlcPySyntax.Docs.  Read-only reference rather than a live
		// preview — the editor is the single source of truth, the right
		// side is "what's available".  Wrapped in a ScrollView so long
		// lists scroll instead of overflowing.
		ScrollView apiPanel = new ScrollView();
		apiPanel.style.flexBasis = new StyleLength(new Length(30, LengthUnit.Percent));
		apiPanel.style.flexShrink = 1;
		apiPanel.style.borderLeftWidth = 1;
		apiPanel.style.borderLeftColor = new StyleColor(new UnityEngine.Color(0.25f, 0.25f, 0.25f, 1f));
		apiPanel.style.backgroundColor = new StyleColor(new UnityEngine.Color(0.12f, 0.12f, 0.13f, 1f));
		editorRow.Add(apiPanel);

		Label apiHeader = new Label();
		apiHeader.text = "API Reference";
		apiHeader.enableRichText = true;
		apiHeader.style.color = new StyleColor(new UnityEngine.Color(0.85f, 0.85f, 0.55f, 1f));
		apiHeader.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
		apiHeader.style.paddingLeft = 8;
		apiHeader.style.paddingRight = 8;
		apiHeader.style.paddingTop = 6;
		apiHeader.style.paddingBottom = 4;
		apiPanel.Add(apiHeader);

		PopulateApiReference(apiPanel);

		// ---- Error strip -------------------------------------------------
		// Collapses to display:none when there's no error so the editor
		// reclaims the vertical space.  Background is a desaturated red so
		// it reads as "warning" without looking alarming when small.
		m_errorStrip = new VisualElement();
		m_errorStrip.style.backgroundColor = new StyleColor(new UnityEngine.Color(0.45f, 0.07f, 0.07f, 0.85f));
		m_errorStrip.style.paddingTop = 4;
		m_errorStrip.style.paddingBottom = 4;
		m_errorStrip.style.paddingLeft = 8;
		m_errorStrip.style.paddingRight = 8;
		m_errorStrip.style.display = DisplayStyle.None;
		body.Add(m_errorStrip);

		m_errorLabel = new Label();
		m_errorLabel.style.color = new StyleColor(UnityEngine.Color.white);
		m_errorLabel.style.whiteSpace = WhiteSpace.Normal;
		m_errorStrip.Add(m_errorLabel);

		// ---- Stats label -------------------------------------------------
		m_statsLabel = new Label();
		m_statsLabel.style.color = new StyleColor(new UnityEngine.Color(0.7f, 0.7f, 0.7f, 1f));
		m_statsLabel.style.paddingLeft = 8;
		m_statsLabel.style.paddingRight = 8;
		m_statsLabel.style.paddingTop = 2;
		m_statsLabel.style.paddingBottom = 2;
		body.Add(m_statsLabel);

		// ---- IntelliSense floater ---------------------------------------
		// Absolute-positioned popup that appears below the line containing
		// the caret (positioned in OpenFloater).  The body is its parent —
		// using `Position.Absolute` lets the floater hover over the editor
		// without taking flex space, so opening it doesn't push the rest
		// of the layout around.  Built as a column with a rows-container at
		// top + an in-floater hint label at bottom; the hint mirrors the
		// currently-highlighted entry's doc, replacing the bottom-of-window
		// tooltip while the floater is open.
		m_floater = new VisualElement();
		m_floater.style.position = Position.Absolute;
		m_floater.style.flexDirection = FlexDirection.Column;
		// Width pinned to ~50% of the editor each time the floater opens
		// (see OpenFloater).  No max-height — the hint label below grows
		// with its wrapped text, so the popup's overall size is driven by
		// content, not capped to a fixed rectangle that'd cut the doc off
		// for longer entries.
		m_floater.style.backgroundColor = new StyleColor(new UnityEngine.Color(0.16f, 0.16f, 0.18f, 0.97f));
		m_floater.style.borderTopWidth = 1;
		m_floater.style.borderBottomWidth = 1;
		m_floater.style.borderLeftWidth = 1;
		m_floater.style.borderRightWidth = 1;
		StyleColor floaterBorder = new StyleColor(new UnityEngine.Color(0.3f, 0.3f, 0.3f, 1f));
		m_floater.style.borderTopColor = floaterBorder;
		m_floater.style.borderBottomColor = floaterBorder;
		m_floater.style.borderLeftColor = floaterBorder;
		m_floater.style.borderRightColor = floaterBorder;
		m_floater.style.display = DisplayStyle.None;
		body.Add(m_floater);
		m_floaterParent = body;

		// Rows container — a vertical-scrolling ScrollView so completion
		// lists longer than the popup can scroll instead of being clipped.
		// Stored as VisualElement field but instantiated as ScrollView;
		// .Add() goes through ScrollView.contentContainer automatically,
		// so the rest of the code (RebuildFloaterRows / Clear) keeps
		// working unchanged.
		ScrollView rowsScroll = new ScrollView(ScrollViewMode.Vertical);
		rowsScroll.style.flexDirection = FlexDirection.Column;
		rowsScroll.style.paddingTop = 2;
		rowsScroll.style.paddingBottom = 2;
		rowsScroll.style.maxHeight = 160;
		rowsScroll.style.flexGrow = 0;
		rowsScroll.style.flexShrink = 1;
		m_floaterRowsContainer = rowsScroll;
		m_floater.Add(m_floaterRowsContainer);

		// Hint row pinned at the bottom of the floater — updated by
		// ApplyFloaterSelectionStyles to reflect the current selection.
		m_floaterHintLabel = new Label();
		m_floaterHintLabel.style.color = new StyleColor(new UnityEngine.Color(0.85f, 0.85f, 0.55f, 1f));
		m_floaterHintLabel.style.paddingLeft = 8;
		m_floaterHintLabel.style.paddingRight = 8;
		m_floaterHintLabel.style.paddingTop = 4;
		m_floaterHintLabel.style.paddingBottom = 4;
		m_floaterHintLabel.style.borderTopWidth = 1;
		m_floaterHintLabel.style.borderTopColor = floaterBorder;
		m_floaterHintLabel.style.whiteSpace = WhiteSpace.Normal;
		m_floater.Add(m_floaterHintLabel);

		m_floaterRows = new System.Collections.Generic.List<Label>();

		// Tooltip strip — single-line label populated when the cursor is
		// on a known identifier in the editor.  Sits below the stats
		// label so it doesn't shift layout when text appears/clears.
		m_tooltipLabel = new Label();
		m_tooltipLabel.style.color = new StyleColor(new UnityEngine.Color(0.85f, 0.85f, 0.55f, 1f));
		m_tooltipLabel.style.paddingLeft = 8;
		m_tooltipLabel.style.paddingRight = 8;
		m_tooltipLabel.style.paddingTop = 2;
		m_tooltipLabel.style.paddingBottom = 2;
		m_tooltipLabel.style.minHeight = 18;
		m_tooltipLabel.style.whiteSpace = WhiteSpace.NoWrap;
		body.Add(m_tooltipLabel);

		// ---- Footer ------------------------------------------------------
		// Mafi's `PanelFooterRow` — a proper Mafi component with the
		// edge-shadow divider and bolts decoration that matches the rest
		// of the game's panel footers.  Buttons added via BodyAdd get
		// the right inner-element padding / sizing without us having to
		// hand-roll inline styles (the previous custom VisualElement
		// footer was making the buttons look like cramped chips because
		// inline padding only grew the outer shadow).
		//
		// Save is the Primary (accent) variant; the others are General.
		// Back is pushed to the right via MarginLeft(Px.Auto) — Mafi's
		// canonical "fill remaining space" trick (see Save/LoadWindow).
		ButtonText saveButton = new ButtonText(Button.Primary, "Save".ToDoLoc())
			.OnClick(() => {
				if (CompileNow(m_codeEditor.Text)) {
					m_controller.Save(m_codeEditor.Text);
					m_controller.Back();
				}
			});
		ButtonText compileButton = new ButtonText(Button.General, "Compile".ToDoLoc())
			.OnClick(() => CompileNow(m_codeEditor.Text))
			.MarginLeft(8.px());
		ButtonText revertButton = new ButtonText(Button.General, "Revert".ToDoLoc())
			.OnClick(() => LoadFromModule(m_controller.CurrentModule))
			.MarginLeft(8.px());
		ButtonText backButton = new ButtonText(Button.General, "Back".ToDoLoc())
			.OnClick(() => m_controller.Back())
			.MarginLeft(Px.Auto);

		// `BodyAdd(Action<Row>, ...)` overload lets us style the inner Row
		// before the buttons land — bumping vertical padding + minHeight
		// here gives the footer real chrome instead of a strip that's
		// shorter than the button shadows it contains.  AlignItemsCenter
		// keeps the buttons vertically centered within the taller row so
		// they don't stick to the top edge.
		PanelFooterRow footer = new PanelFooterRow().BodyAdd(
			row => row.PaddingTopBottom(6.px())
			          .MinHeight(48.px())
			          .AlignItemsCenter(),
			saveButton, compileButton, revertButton, backButton);
		body.Add(footer.RootElement);

		// No periodic poll of the module's state.  The editor is independent
		// after opening — its visible code, error strip, and token count
		// only change in response to player actions (typing, Compile, Save,
		// Revert).  Polling clobbered local Compile results because the
		// module's persisted __compile_error stays stale until the next
		// tick after Save, which made fixed errors flash back on screen.
	}

	// Fill the API panel with one row per entry in PlcPySyntax.Docs.  Each
	// row is two stacked labels: the identifier (highlighted blue) and its
	// short doc (gray, wrapping).  Colors mirror the editor's tooltip strip
	// for visual consistency.
	private void PopulateApiReference(VisualElement target) {
		foreach (var entry in PlcPySyntax.Docs) {
			VisualElement row = new VisualElement();
			row.style.paddingLeft = 8;
			row.style.paddingRight = 8;
			row.style.paddingTop = 4;
			row.style.paddingBottom = 4;
			row.style.borderBottomWidth = 1;
			row.style.borderBottomColor = new StyleColor(new UnityEngine.Color(0.18f, 0.18f, 0.18f, 1f));

			Label name = new Label();
			name.text = entry.Key;
			name.style.color = new StyleColor(new UnityEngine.Color(0.34f, 0.61f, 0.84f, 1f));
			name.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
			row.Add(name);

			Label desc = new Label();
			desc.text = entry.Value;
			desc.style.color = new StyleColor(new UnityEngine.Color(0.78f, 0.78f, 0.78f, 1f));
			desc.style.whiteSpace = WhiteSpace.Normal;
			desc.style.paddingLeft = 8;
			row.Add(desc);

			target.Add(row);
		}
	}

	// Read the editor's caret position, extract the identifier under it,
	// and surface its doc string (if we have one) in the tooltip strip.
	// Falls back through dotted parents — `self.Input.A` → `self.Input`
	// → `self` — so the player still gets help on a name that has no
	// direct entry but inherits semantics from a known prefix.
	private void RefreshIdentifierTooltip() {
		if (m_codeEditor == null) {
			SetTooltipText("");
			return;
		}
		string source = m_codeEditor.Text ?? "";
		int cursor = m_codeEditor.CaretIndex;
		if (cursor < 0 || cursor > source.Length) {
			SetTooltipText("");
			return;
		}
		// If the caret sits between an identifier char and a non-identifier
		// char, walk one position back so a click immediately after a name
		// still resolves it.
		int probe = cursor;
		if (probe >= source.Length || !IsIdentifierChar(source[probe])) {
			probe--;
		}
		if (probe < 0) {
			SetTooltipText("");
			return;
		}
		string ident = ExtractIdentifierAt(source, probe);
		if (string.IsNullOrEmpty(ident)) {
			SetTooltipText("");
			return;
		}
		if (PlcPySyntax.Docs.TryGetValue(ident, out string doc)) {
			SetTooltipText(ident + " — " + doc);
			return;
		}
		int dot = ident.LastIndexOf('.');
		while (dot > 0) {
			string parent = ident.Substring(0, dot);
			if (PlcPySyntax.Docs.TryGetValue(parent, out string parentDoc)) {
				SetTooltipText(parent + " — " + parentDoc);
				return;
			}
			dot = parent.LastIndexOf('.');
		}
		SetTooltipText("");
	}

	private void SetTooltipText(string text) {
		m_tooltipLabel.text = text ?? "";
	}

	// Walks left and right from `index` collecting the longest run of
	// identifier characters (plus dots, so dotted paths are returned in
	// one piece).  Used to convert a caret position into a name we can
	// look up in the docs table.
	private static string ExtractIdentifierAt(string source, int index) {
		if (index < 0 || index >= source.Length) {
			return null;
		}
		if (!IsIdentifierChar(source[index])) {
			return null;
		}
		int start = index;
		while (start > 0 && IsIdentifierChar(source[start - 1])) {
			start--;
		}
		int end = index;
		while (end < source.Length - 1 && IsIdentifierChar(source[end + 1])) {
			end++;
		}
		return source.Substring(start, end - start + 1);
	}

	private static bool IsIdentifierChar(char c) {
		return char.IsLetterOrDigit(c) || c == '_' || c == '.';
	}

	// Pushes the colored rich-text version of the current source into the
	// Recompute the gutter on every value change.  Counted via a single
	// pass; line text is built into a pre-sized StringBuilder.
	private void UpdateLineNumbers(string code) {
		int newlineCount = 0;
		if (!string.IsNullOrEmpty(code)) {
			for (int i = 0; i < code.Length; i++) {
				if (code[i] == '\n') {
					newlineCount++;
				}
			}
		}
		int lines = newlineCount + 1;
		StringBuilder sb = new StringBuilder(lines * 4);
		for (int i = 1; i <= lines; i++) {
			if (i > 1) {
				sb.Append('\n');
			}
			sb.Append(i);
		}
		m_lineNumbers.text = sb.ToString();
	}

	// Floater-aware key interceptor — assigned to
	// PlcPyTextEditor.KeyDownInterceptor and called as the FIRST step of
	// the editor's OnKeyDown.  Returning true means "consumed; stop the
	// editor's normal handling".  The editor itself handles Tab → 4
	// spaces, arrow keys, etc., but only when this interceptor passes.
	// Escape is intentionally NOT handled here — it's consumed in the
	// InputUpdate override below.  Handling it here too would close the
	// floater synchronously and then UnityInputManager's poll-based
	// Escape pass would still see the keypress and deactivate the editor
	// controller — closing the window on the same press the player meant
	// only to dismiss the floater.
	private bool onEditorKeyDown(KeyDownEvent evt) {
		// Ctrl+Space → open the floater with all top-level names (or
		// the dotted-parent set if the cursor is on `something.`).
		if (evt.ctrlKey && evt.keyCode == UnityEngine.KeyCode.Space) {
			OpenFloater();
			evt.StopPropagation();
			evt.PreventDefault();
			return true;
		}

		if (m_floaterOpen) {
			switch (evt.keyCode) {
				case UnityEngine.KeyCode.DownArrow:
					MoveFloaterSelection(+1);
					evt.StopPropagation();
					evt.PreventDefault();
					return true;
				case UnityEngine.KeyCode.UpArrow:
					MoveFloaterSelection(-1);
					evt.StopPropagation();
					evt.PreventDefault();
					return true;
				case UnityEngine.KeyCode.Return:
				case UnityEngine.KeyCode.KeypadEnter:
				case UnityEngine.KeyCode.Tab:
					InsertFloaterSelection();
					evt.StopPropagation();
					evt.PreventDefault();
					return true;
			}
			// Any other key falls through to the editor (typing /
			// backspace etc.), then OnEditorTextChanged refilters the
			// floater on the new value.
		}
		return false;
	}

	// Called from the OnValueChanged handler.  When the floater is open
	// we re-filter against the partial token at the current cursor; if
	// it's closed we look for a freshly-typed `.` at the cursor and
	// open the floater scoped to that dotted parent.
	private void OnEditorTextChanged(string text) {
		text = text ?? "";
		int cursor = m_codeEditor.CaretIndex;
		if (cursor < 0 || cursor > text.Length) {
			cursor = text.Length;
		}

		if (m_floaterOpen) {
			// Close if the player typed a non-identifier char (space, etc.)
			// or backspaced past the trigger position.
			if (cursor < m_floaterTriggerPos) {
				CloseFloater();
				return;
			}
			RebuildFloaterRows(GetFloaterFilter(text, cursor));
			return;
		}

		// Detect freshly-typed '.'  -> open floater scoped to the parent.
		if (cursor > 0 && cursor <= text.Length && text[cursor - 1] == '.') {
			OpenFloater();
		}
	}

	private void OpenFloater() {
		string text = m_codeEditor.Text ?? "";
		int cursor = m_codeEditor.CaretIndex;
		if (cursor < 0 || cursor > text.Length) {
			cursor = text.Length;
		}

		// Determine the dotted parent and the partial filter.  If the char
		// before cursor is `.`, the parent is everything up to (but not
		// including) the dot, and the filter is empty.  Otherwise, the
		// player has typed part of a name — walk back through identifier
		// chars to find any leading `parent.` and the partial.
		string parent;
		string filter;
		int triggerPos;
		if (cursor > 0 && text[cursor - 1] == '.') {
			parent = WalkBackDottedPath(text, cursor - 1);
			filter = "";
			triggerPos = cursor;
		} else {
			int wordStart = cursor;
			while (wordStart > 0 && IsIdentifierChar(text[wordStart - 1]) && text[wordStart - 1] != '.') {
				wordStart--;
			}
			filter = text.Substring(wordStart, cursor - wordStart);
			if (wordStart > 0 && text[wordStart - 1] == '.') {
				parent = WalkBackDottedPath(text, wordStart - 1);
			} else {
				parent = "";
			}
			triggerPos = wordStart;
		}

		var entries = new System.Collections.Generic.List<PlcPySyntax.Completion>(PlcPySyntax.GetCompletions(parent));
		if (entries.Count == 0) {
			return;
		}

		m_floaterEntries = entries;
		m_floaterTriggerParent = parent;
		m_floaterTriggerPos = triggerPos;
		m_floaterOpen = true;
		// Pin width to 50% of the editor's resolved width so the popup
		// stays a constant size across selection changes — without an
		// explicit width, the floater would resize to fit each row's
		// hint text and "wobble" as the player arrows through entries.
		// Falls back to a fixed 320 px if the editor hasn't measured yet
		// (very first open before layout); the next open uses the real
		// width once the editor is laid out.
		float editorWidth = m_codeEditor.resolvedStyle.width;
		float floaterWidth = editorWidth > 0 ? editorWidth * 0.5f : 320f;
		m_floater.style.width = floaterWidth;
		m_floater.style.display = DisplayStyle.Flex;
		// Suppress the bottom-of-window caret tooltip while the floater is
		// open — the in-floater hint takes over so the player isn't reading
		// two competing strings.
		SetTooltipText("");
		RebuildFloaterRows(filter);
		// Position the floater under the line containing the cursor.  We
		// schedule a follow-up reposition on the next frame because the
		// flex geometry resolves after `display` flips from None to Flex,
		// and the worldBound we'd read right now is still stale.
		PositionFloaterAtCaret(text, cursor);
		m_floaterParent?.schedule.Execute(() => {
			if (m_floaterOpen) {
				PositionFloaterAtCaret(m_codeEditor.Text ?? "", m_floaterTriggerPos);
			}
		}).ExecuteLater(0);
	}

	// Position the floater under the caret line, in the floater's parent
	// (body) local space.  The custom editor exposes its own pixel math
	// (GetPixelForIndex + LineHeight), so we go editor-local → world →
	// parent-local without re-deriving font metrics.  Clamps inside the
	// parent rect; flips above the caret if it would overflow the bottom.
	private void PositionFloaterAtCaret(string text, int cursor) {
		if (m_codeEditor == null || m_floaterParent == null) {
			return;
		}
		if (cursor < 0 || cursor > (text?.Length ?? 0)) {
			cursor = text?.Length ?? 0;
		}

		UnityEngine.Vector2 caretEditorLocal = m_codeEditor.GetPixelForIndex(cursor);
		// Below the caret line: bump down by the line height + 2 px gap so
		// the popup doesn't sit on top of the line being edited.
		caretEditorLocal.y += m_codeEditor.LineHeight + 2;

		UnityEngine.Vector2 caretWorld = m_codeEditor.LocalToWorld(caretEditorLocal);
		UnityEngine.Vector2 local = m_floaterParent.WorldToLocal(caretWorld);

		// Clamp inside the parent's resolved size so the floater doesn't
		// disappear off the right or bottom edge if the caret is near the
		// window border.  Width/height pulled from resolvedStyle since the
		// floater hasn't been measured yet on the very first open.
		float parentW = m_floaterParent.resolvedStyle.width;
		float parentH = m_floaterParent.resolvedStyle.height;
		float floaterW = m_floater.resolvedStyle.width > 0 ? m_floater.resolvedStyle.width : 220;
		float floaterH = m_floater.resolvedStyle.height > 0 ? m_floater.resolvedStyle.height : 180;
		if (parentW > 0 && local.x + floaterW > parentW - 4) {
			local.x = parentW - floaterW - 4;
		}
		if (local.x < 4) {
			local.x = 4;
		}
		if (parentH > 0 && local.y + floaterH > parentH - 4) {
			// Doesn't fit below — flip above the line.
			local.y -= (floaterH + m_codeEditor.LineHeight + 4);
			if (local.y < 4) {
				local.y = 4;
			}
		}

		m_floater.style.left = local.x;
		m_floater.style.top = local.y;
	}

	private void CloseFloater() {
		m_floaterOpen = false;
		m_floater.style.display = DisplayStyle.None;
		m_floaterRows.Clear();
		m_floaterRowsContainer.Clear();
		m_floaterHintLabel.text = "";
		m_floaterEntries = null;
		// Restore the regular caret-driven tooltip at the bottom of the window.
		RefreshIdentifierTooltip();
	}

	// Walks back from `dotPos` (an index pointing AT a '.') across the
	// identifier-and-dot run that precedes it, returning the dotted
	// path text (e.g., for `if self.Input.|` returns "self.Input").
	private static string WalkBackDottedPath(string text, int dotPos) {
		int start = dotPos;
		while (start > 0) {
			char c = text[start - 1];
			if (char.IsLetterOrDigit(c) || c == '_' || c == '.') {
				start--;
			} else {
				break;
			}
		}
		return text.Substring(start, dotPos - start);
	}

	// What the player has typed since the trigger position — used to
	// filter the visible rows so the floater narrows live as they type.
	private string GetFloaterFilter(string text, int cursor) {
		if (cursor <= m_floaterTriggerPos) {
			return "";
		}
		string slice = text.Substring(m_floaterTriggerPos, cursor - m_floaterTriggerPos);
		// If the slice contains anything that breaks the identifier (like
		// a space or operator), bail — the OnEditorTextChanged caller
		// will close the floater on the next refresh.
		for (int i = 0; i < slice.Length; i++) {
			char c = slice[i];
			if (!IsIdentifierChar(c)) {
				return null;
			}
		}
		return slice;
	}

	// Rebuilds the floater's row labels from m_floaterEntries, applying
	// the case-insensitive prefix filter.  Selection resets to 0 on each
	// rebuild because the visible set may have shrunk past the previous
	// index.
	private void RebuildFloaterRows(string filter) {
		if (filter == null) {
			CloseFloater();
			return;
		}

		m_floaterRowsContainer.Clear();
		m_floaterRows.Clear();

		foreach (var entry in m_floaterEntries) {
			if (filter.Length > 0 && entry.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) != 0) {
				continue;
			}
			Label row = new Label();
			row.text = entry.Name;
			row.style.paddingLeft = 12;
			row.style.paddingRight = 12;
			// 6 px top + 6 px bottom gives the rows breathing room so
			// adjacent entries (and the highlighted selection rectangle)
			// read as separate lines instead of a smushed strip.
			row.style.paddingTop = 6;
			row.style.paddingBottom = 6;
			row.style.color = new StyleColor(new UnityEngine.Color(0.85f, 0.9f, 1f, 1f));
			row.RegisterCallback<MouseDownEvent>(_ => {
				m_floaterSelected = m_floaterRows.IndexOf(row);
				if (m_floaterSelected < 0) {
					m_floaterSelected = 0;
				}
				InsertFloaterSelection();
			});
			m_floaterRowsContainer.Add(row);
			m_floaterRows.Add(row);
		}

		if (m_floaterRows.Count == 0) {
			CloseFloater();
			return;
		}

		m_floaterSelected = 0;
		ApplyFloaterSelectionStyles();
	}

	private void MoveFloaterSelection(int delta) {
		if (m_floaterRows.Count == 0) {
			return;
		}
		m_floaterSelected = (m_floaterSelected + delta + m_floaterRows.Count) % m_floaterRows.Count;
		ApplyFloaterSelectionStyles();
		// Pull the selected row into the visible part of the scroll view
		// so arrow navigation past the viewport edge follows the cursor.
		// Cast is safe — m_floaterRowsContainer is always a ScrollView
		// (built that way in the constructor); the field type is the
		// VisualElement base purely so existing .Add / .Clear calls keep
		// working without route-through ceremony.
		if (m_floaterRowsContainer is ScrollView scroll
			&& m_floaterSelected >= 0
			&& m_floaterSelected < m_floaterRows.Count) {
			scroll.ScrollTo(m_floaterRows[m_floaterSelected]);
		}
	}

	// Tints the selected row + writes its doc into the tooltip strip so
	// the bottom hint reflects the floater's current focus.
	private void ApplyFloaterSelectionStyles() {
		for (int i = 0; i < m_floaterRows.Count; i++) {
			Label row = m_floaterRows[i];
			if (i == m_floaterSelected) {
				row.style.backgroundColor = new StyleColor(new UnityEngine.Color(0.21f, 0.36f, 0.55f, 1f));
			} else {
				row.style.backgroundColor = new StyleColor(new UnityEngine.Color(0, 0, 0, 0));
			}
		}

		if (m_floaterEntries != null && m_floaterSelected >= 0 && m_floaterSelected < m_floaterRows.Count) {
			// Find the selected entry by name (rebuilds may have filtered out
			// some entries, so list-index doesn't map directly to entry-index).
			string visibleName = m_floaterRows[m_floaterSelected].text;
			foreach (var entry in m_floaterEntries) {
				if (entry.Name == visibleName) {
					m_floaterHintLabel.text = entry.Name + " — " + entry.Doc;
					return;
				}
			}
		}
		m_floaterHintLabel.text = "";
	}

	// Replace [triggerPos..cursor] with the selected name and close.
	// For PLC-PY's "any name" placeholder on Number/StringData, leave
	// the cursor where it is rather than inserting the placeholder text.
	private void InsertFloaterSelection() {
		if (!m_floaterOpen || m_floaterRows.Count == 0) {
			CloseFloater();
			return;
		}
		string visibleName = m_floaterRows[m_floaterSelected].text;
		if (visibleName.Length == 0 || visibleName.StartsWith("(")) {
			CloseFloater();
			return;
		}

		string text = m_codeEditor.Text ?? "";
		int cursor = m_codeEditor.CaretIndex;
		if (cursor < 0 || cursor > text.Length) {
			cursor = text.Length;
		}
		if (m_floaterTriggerPos < 0 || m_floaterTriggerPos > text.Length || m_floaterTriggerPos > cursor) {
			CloseFloater();
			return;
		}

		// ReplaceRange handles both buffer mutation and caret repositioning
		// (caret lands right after the inserted text), so no manual cursor
		// fiddling needed afterwards.
		m_codeEditor.ReplaceRange(m_floaterTriggerPos, cursor, visibleName);
		CloseFloater();
	}

	// Synchronous validate-without-save.  Runs the same Tokenizer + Lexer
	// the per-tick action uses, but discards the parsed Block — the editor
	// just wants to know if the source is syntactically clean.  Result is
	// shown via the stats label (token count) and the error strip (if any).
	// Returns true if the source compiled cleanly so callers (Save) can
	// chain follow-up actions only when the script is valid.
	private bool CompileNow(string source) {
		if (string.IsNullOrWhiteSpace(source)) {
			ShowError("Compile: empty script");
			m_statsLabel.text = "Tokens: 0";
			return false;
		}
		try {
			Token[] tokens = Tokenizer.ParseString(source, "PLC_PY.preview");
			Lexer.Parse(tokens);
			HideError();
			m_statsLabel.text = "Tokens: " + tokens.Length + "  (compile OK)";
			return true;
		} catch (Exception parseError) {
			ShowError("Compile: " + parseError.Message);
			return false;
		}
	}


	private void ShowError(string text) {
		m_errorLabel.text = text;
		m_errorStrip.style.display = DisplayStyle.Flex;
	}

	private void HideError() {
		m_errorLabel.text = "";
		m_errorStrip.style.display = DisplayStyle.None;
	}

	// Snapshots the module's saved code + error state into the editor.
	// Called from OnActivate (initial open) and the Revert button (manual
	// pull).  After this returns the editor stays independent: the visible
	// code/error/stats only change in response to the player's actions
	// until the next Save or Revert.  The error strip mirrors what the
	// running module knows (its persisted compile or runtime error), so
	// the player still sees runtime failures after a Revert without us
	// polling every tick.
	public void LoadFromModule(Module module) {
		CloseFloater();
		string current = module?.Field["code", ""] ?? "";
		m_codeEditor.Text = current;
		UpdateLineNumbers(current);
		SetTooltipText("");

		if (module == null) {
			m_statsLabel.text = "(no module bound)";
			HideError();
			return;
		}

		m_statsLabel.text = "Tokens: " + module.LexerNodeCount;
		string compileError = module.StringData.TryGetValue("__compile_error", out string ce) ? ce : null;
		string runError = module.StringData.TryGetValue("__run_error", out string re) ? re : null;
		if (!string.IsNullOrEmpty(compileError)) {
			ShowError("Compile: " + compileError);
		} else if (!string.IsNullOrEmpty(runError)) {
			ShowError("Run: " + runError);
		} else {
			HideError();
		}
	}

	// Intercept Escape + swallow Mafi's poll-based input dispatch while
	// the editor is focused.
	//
	// UIElements StopPropagation on KeyDownEvent only stops the UIElements
	// event pipeline — Mafi.Core's UnityInputManager polls Unity's legacy
	// `Input.GetKeyDown` directly every tick and would otherwise fire its
	// own bindings (Tab focus cycling, hotkeys, camera moves) on top of
	// the player's typing.  Returning true here short-circuits that
	// dispatch so only the editor sees the keypress.
	//
	// Escape is special-cased twice: once to close an open floater
	// (counterpart to the UIElements path), and a second time so the
	// player can still close the editor via Escape even while the editor
	// has focus — without that carve-out the focus-gate below would block
	// it forever.
	public override bool InputUpdate() {
		if (m_floaterOpen && UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Escape)) {
			CloseFloater();
			return true;
		}
		if (m_codeEditor != null && m_codeEditor.HasFocus() && IsEditorOwnedKeyDown()) {
			return true;
		}
		return base.InputUpdate();
	}

	// Whitelist of keycodes the editor owns while focused — listed
	// explicitly (rather than `Input.anyKey`) so it's obvious which
	// game bindings we're stealing and which we leave alone.  Escape
	// is intentionally absent: we want it to reach Mafi so a player
	// without an open floater can close the editor with it.  Modifier
	// keys (Shift / Ctrl / Alt) are also absent because they're not
	// game bindings on their own — they only matter as part of a
	// combo, and the combo's other key (a letter, an arrow, etc.) is
	// what we consume.
	private static readonly UnityEngine.KeyCode[] EDITOR_OWNED_KEYS = new[] {
		// Whitespace + control keys the editor uses for editing.
		UnityEngine.KeyCode.Space,
		UnityEngine.KeyCode.Tab,
		UnityEngine.KeyCode.Return,
		UnityEngine.KeyCode.KeypadEnter,
		UnityEngine.KeyCode.Backspace,
		UnityEngine.KeyCode.Delete,
		// Caret nav.
		UnityEngine.KeyCode.LeftArrow,
		UnityEngine.KeyCode.RightArrow,
		UnityEngine.KeyCode.UpArrow,
		UnityEngine.KeyCode.DownArrow,
		UnityEngine.KeyCode.Home,
		UnityEngine.KeyCode.End,
		UnityEngine.KeyCode.PageUp,
		UnityEngine.KeyCode.PageDown,
	};

	// Returns true if any keycode the editor cares about is currently
	// pressed.  Cheap (linear scan over ~14 codes); runs once per tick
	// only while the editor is focused.  Letter / digit / symbol keys
	// don't need explicit entries because Mafi's toolbar shortcuts only
	// fire on Input.anyKeyDown (handled in the per-controller dispatch
	// loop) and BlockShortcuts in EDITOR_CONFIG already prevents that
	// from looping into our controller.
	private static bool IsEditorOwnedKeyDown() {
		for (int i = 0; i < EDITOR_OWNED_KEYS.Length; i++) {
			if (UnityEngine.Input.GetKey(EDITOR_OWNED_KEYS[i])) {
				return true;
			}
		}
		return false;
	}
}
