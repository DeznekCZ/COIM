using System;
using System.Text;
using Mafi;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using ProgramableNetwork.Python;
using UnityEngine.UIElements;

// Alias the ambiguous types so we can write `Label`/`TextField` and mean
// the Unity primitives (which we manipulate via .style, .text directly),
// while keeping Mafi's components available under fully-qualified names
// where we actually want their wrappers.
using Label = UnityEngine.UIElements.Label;
using MafiTextField = Mafi.Unity.UiToolkit.Library.TextField;

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
	private readonly MafiTextField m_codeEditor;
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

		// Line-numbers gutter — narrow right-aligned label, separated from
		// the editor by a 1px border so the eye registers the divide
		// without the gutter blending into the textfield background.
		m_lineNumbers = new Label("1");
		m_lineNumbers.style.minWidth = 40;
		m_lineNumbers.style.paddingRight = 6;
		m_lineNumbers.style.paddingLeft = 6;
		m_lineNumbers.style.paddingTop = 4;
		m_lineNumbers.style.paddingBottom = 4;
		m_lineNumbers.style.unityTextAlign = UnityEngine.TextAnchor.UpperRight;
		m_lineNumbers.style.whiteSpace = WhiteSpace.Pre;
		m_lineNumbers.style.color = new StyleColor(new UnityEngine.Color(0.55f, 0.55f, 0.55f, 1f));
		m_lineNumbers.style.borderRightWidth = 1;
		m_lineNumbers.style.borderRightColor = new StyleColor(new UnityEngine.Color(0.25f, 0.25f, 0.25f, 1f));
		m_lineNumbers.pickingMode = PickingMode.Ignore;
		editorBox.Add(m_lineNumbers);

		// Editor field — Mafi TextField in multiline mode.  Default font is
		// kept (no displayFont class) so the player sees actual case in
		// their code rather than the LCD-style upper-case rendering.
		m_codeEditor = new MafiTextField();
		m_codeEditor.Multiline(true);
		m_codeEditor.RootElement.style.flexGrow = 1;
		m_codeEditor.RootElement.style.flexShrink = 1;
		editorBox.Add(m_codeEditor.RootElement);

		// Tab key → four spaces.  By default Tab focus-cycles out of the
		// TextField, which is useless inside a code area.  Intercept at the
		// trickle-down phase so we beat the focus controller.
		m_codeEditor.RootElement.RegisterCallback<KeyDownEvent>(onEditorKeyDown, TrickleDown.TrickleDown);

		m_codeEditor.OnValueChanged(text => {
			UpdateLineNumbers(text);
			RefreshIdentifierTooltip();
			OnEditorTextChanged(text);
		});

		// Tooltip triggers off the editor caret rather than the preview's
		// hit-test — Unity's character-index APIs aren't uniformly public,
		// and tying the tooltip to the cursor means the player gets help
		// for whatever they're currently typing or clicking on.
		m_codeEditor.RootElement.RegisterCallback<MouseUpEvent>(_ => RefreshIdentifierTooltip());
		m_codeEditor.RootElement.RegisterCallback<KeyUpEvent>(_ => RefreshIdentifierTooltip());

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
		m_floater.style.minWidth = 180;
		m_floater.style.maxWidth = 360;
		m_floater.style.maxHeight = 200;
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

		m_floaterRowsContainer = new VisualElement();
		m_floaterRowsContainer.style.flexDirection = FlexDirection.Column;
		m_floaterRowsContainer.style.paddingTop = 2;
		m_floaterRowsContainer.style.paddingBottom = 2;
		m_floaterRowsContainer.style.maxHeight = 160;
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
		VisualElement footer = new VisualElement();
		footer.style.flexDirection = FlexDirection.Row;
		footer.style.paddingTop = 6;
		footer.style.paddingBottom = 4;
		footer.style.paddingLeft = 4;
		footer.style.paddingRight = 4;
		body.Add(footer);

		ButtonText saveButton = new ButtonText("Save".ToDoLoc());
		saveButton.OnClick(() => m_controller.Save(m_codeEditor.GetText()));
		footer.Add(saveButton.RootElement);

		ButtonText compileButton = new ButtonText("Compile".ToDoLoc());
		compileButton.RootElement.style.marginLeft = 8;
		compileButton.OnClick(() => CompileNow(m_codeEditor.GetText()));
		footer.Add(compileButton.RootElement);

		VisualElement spacer = new VisualElement();
		spacer.style.flexGrow = 1;
		footer.Add(spacer);

		ButtonText backButton = new ButtonText("Back".ToDoLoc());
		backButton.OnClick(() => m_controller.Back());
		footer.Add(backButton.RootElement);

		// ---- Live status update -----------------------------------------
		this.DoOnSyncPeriodically(() => RefreshStatus(), Duration.OneTick);
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
		TextElement textElement = m_codeEditor.RootElement.Q<TextElement>();
		if (textElement is null) {
			SetTooltipText("");
			return;
		}
		string source = m_codeEditor.GetText() ?? "";
		int cursor = textElement.selection.cursorIndex;
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

	// Editor keyboard handler — covers IntelliSense triggers, floater
	// navigation, and the Tab → four-spaces shortcut.  Ctrl+Space and
	// floater-active arrow/Enter/Tab/Escape need to beat both the
	// TextField's own focus handling and any default key-binding, so we
	// register at TrickleDown phase and call StopPropagation/Prevent
	// Default eagerly when we consume the event.
	private void onEditorKeyDown(KeyDownEvent evt) {
		// Ctrl+Space → open the floater with all top-level names (or
		// the dotted-parent set if the cursor is on `something.`).
		if (evt.ctrlKey && evt.keyCode == UnityEngine.KeyCode.Space) {
			OpenFloater();
			evt.StopPropagation();
			evt.PreventDefault();
			return;
		}

		if (m_floaterOpen) {
			switch (evt.keyCode) {
				case UnityEngine.KeyCode.DownArrow:
					MoveFloaterSelection(+1);
					evt.StopPropagation();
					evt.PreventDefault();
					return;
				case UnityEngine.KeyCode.UpArrow:
					MoveFloaterSelection(-1);
					evt.StopPropagation();
					evt.PreventDefault();
					return;
				case UnityEngine.KeyCode.Return:
				case UnityEngine.KeyCode.KeypadEnter:
				case UnityEngine.KeyCode.Tab:
					InsertFloaterSelection();
					evt.StopPropagation();
					evt.PreventDefault();
					return;
				case UnityEngine.KeyCode.Escape:
					CloseFloater();
					evt.StopPropagation();
					evt.PreventDefault();
					return;
			}
			// Any other key falls through to the TextField, then
			// OnEditorTextChanged refilters the floater on the new value.
		}

		// Tab → four spaces.  Shift+Tab is left alone so the user can still
		// escape the field if they really need to (focus controller handles it).
		// Skipped while the floater is open because Tab there means "insert".
		if (evt.keyCode == UnityEngine.KeyCode.Tab && !evt.shiftKey && !m_floaterOpen) {
			TextElement textElement = m_codeEditor.RootElement.Q<TextElement>();
			if (textElement is null) {
				return;
			}
			string current = m_codeEditor.GetText() ?? "";
			int cursor = textElement.selection.cursorIndex;
			if (cursor < 0 || cursor > current.Length) {
				cursor = current.Length;
			}
			string updated = current.Substring(0, cursor) + "    " + current.Substring(cursor);
			m_codeEditor.SetValue(new LocStrFormatted(updated));
			textElement.selection.cursorIndex = cursor + 4;
			textElement.selection.selectIndex = cursor + 4;
			evt.StopPropagation();
			evt.PreventDefault();
		}
	}

	// Called from the OnValueChanged handler.  When the floater is open
	// we re-filter against the partial token at the current cursor; if
	// it's closed we look for a freshly-typed `.` at the cursor and
	// open the floater scoped to that dotted parent.
	private void OnEditorTextChanged(string text) {
		text = text ?? "";
		TextElement textElement = m_codeEditor.RootElement.Q<TextElement>();
		int cursor = textElement?.selection.cursorIndex ?? text.Length;
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
		string text = m_codeEditor.GetText() ?? "";
		TextElement textElement = m_codeEditor.RootElement.Q<TextElement>();
		int cursor = textElement?.selection.cursorIndex ?? text.Length;
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
				PositionFloaterAtCaret(m_codeEditor.GetText() ?? "", m_floaterTriggerPos);
			}
		}).ExecuteLater(0);
	}

	// Translate the caret's logical (line, column) into a body-local pixel
	// position and stamp it onto the floater's `left/top`.  Approximations
	// where Unity's text APIs don't give us precise metrics:
	//   - line height is taken from the editor's resolvedStyle font size
	//     (×1.4 — matches the Unity TextElement default lineHeight ratio);
	//   - column → x is `col × 0.55 × fontSize` (no real per-char width
	//     measurement; default UI font is proportional, so this is just a
	//     "roughly right of the typed letters" placement, not exact).
	// Players who want an at-the-pixel caret marker would need a real
	// `MeasureTextSize` integration, which is the bigger follow-up.
	private void PositionFloaterAtCaret(string text, int cursor) {
		TextElement textElement = m_codeEditor.RootElement.Q<TextElement>();
		if (textElement == null || m_floaterParent == null) {
			return;
		}
		if (cursor < 0 || cursor > text.Length) {
			cursor = text.Length;
		}

		// Line index + column within line.
		int line = 0;
		int lineStart = 0;
		for (int i = 0; i < cursor; i++) {
			if (text[i] == '\n') {
				line++;
				lineStart = i + 1;
			}
		}
		int col = cursor - lineStart;

		float fontSize = textElement.resolvedStyle.fontSize;
		if (fontSize <= 0) {
			fontSize = 12;
		}
		float lineHeight = fontSize * 1.4f;
		float charWidth = fontSize * 0.55f;

		// TextField wraps the TextElement in a ScrollView — pull the
		// scrollOffset so the floater follows the visible position when the
		// player has scrolled the editor.  Falls back to zero offset if the
		// ScrollView isn't found (some Unity TextField layouts skip it).
		ScrollView scroll = m_codeEditor.RootElement.Q<ScrollView>();
		UnityEngine.Vector2 scrollOffset = scroll?.scrollOffset ?? UnityEngine.Vector2.zero;

		UnityEngine.Rect textWorld = textElement.worldBound;
		float caretWorldX = textWorld.x + (col * charWidth) - scrollOffset.x + 4;
		// Place the floater UNDER the caret line (line+1 row down, plus a
		// 2px breathing gap).
		float caretWorldY = textWorld.y + ((line + 1) * lineHeight) - scrollOffset.y + 2;

		UnityEngine.Vector2 local = m_floaterParent.WorldToLocal(new UnityEngine.Vector2(caretWorldX, caretWorldY));

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
			local.y -= (floaterH + lineHeight + 4);
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
			row.style.paddingTop = 2;
			row.style.paddingBottom = 2;
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

		TextElement textElement = m_codeEditor.RootElement.Q<TextElement>();
		string text = m_codeEditor.GetText() ?? "";
		int cursor = textElement?.selection.cursorIndex ?? text.Length;
		if (cursor < 0 || cursor > text.Length) {
			cursor = text.Length;
		}
		if (m_floaterTriggerPos < 0 || m_floaterTriggerPos > text.Length || m_floaterTriggerPos > cursor) {
			CloseFloater();
			return;
		}

		string updated = text.Substring(0, m_floaterTriggerPos) + visibleName + text.Substring(cursor);
		m_codeEditor.SetValue(new LocStrFormatted(updated));
		int newCursor = m_floaterTriggerPos + visibleName.Length;
		if (textElement != null) {
			textElement.selection.cursorIndex = newCursor;
			textElement.selection.selectIndex = newCursor;
		}
		CloseFloater();
	}

	// Synchronous validate-without-save.  Runs the same Tokenizer + Lexer
	// the per-tick action uses, but discards the parsed Block — the editor
	// just wants to know if the source is syntactically clean.  Result is
	// shown via the stats label (token count) and the error strip (if any).
	private void CompileNow(string source) {
		if (string.IsNullOrWhiteSpace(source)) {
			ShowError("Compile: empty script");
			m_statsLabel.text = "Tokens: 0";
			return;
		}
		try {
			Token[] tokens = Tokenizer.ParseString(source, "PLC_PY.preview");
			Lexer.Parse(tokens);
			HideError();
			m_statsLabel.text = "Tokens: " + tokens.Length + "  (compile OK)";
		} catch (Exception parseError) {
			ShowError("Compile: " + parseError.Message);
		}
	}

	private void RefreshStatus() {
		Module module = m_controller.CurrentModule;
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

	private void ShowError(string text) {
		m_errorLabel.text = text;
		m_errorStrip.style.display = DisplayStyle.Flex;
	}

	private void HideError() {
		m_errorLabel.text = "";
		m_errorStrip.style.display = DisplayStyle.None;
	}

	// Called by the controller on activate (and on re-OpenFor while already
	// active) to seed the editor text from the module's current code field.
	public void LoadFromModule(Module module) {
		string current = module?.Field["code", ""] ?? "";
		m_codeEditor.SetValue(new LocStrFormatted(current));
		UpdateLineNumbers(current);
		SetTooltipText("");
	}
}
