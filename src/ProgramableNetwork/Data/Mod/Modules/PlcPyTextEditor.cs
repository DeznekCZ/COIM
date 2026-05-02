using System;
using System.Collections.Generic;
using System.Text;
using Mafi.Unity.UiToolkit;
using UnityEngine.UIElements;

namespace ProgramableNetwork;

// Self-contained multi-line code editor element.
//
// Built from scratch (no MafiTextField underneath) because the dual-layer
// approach — transparent TextField + colored overlay — couldn't keep the
// painted glyphs in sync with the caret across all the cases that matter
// (focus toggles, font asset reloads, USS rule changes between Mafi
// versions).  Owning the rendering means colored text and caret can never
// drift apart: they're computed from the same buffer and the same font
// metrics.
//
// Layout — three siblings inside `this` (a Position.Relative root):
//   selection layer (back) — translucent rectangles, one per visible
//                             line in the selection range
//   text label             — a single rich-text Label spanning all lines
//                             (whiteSpace=Pre, monospace font)
//   caret (front)          — 1px-wide VisualElement positioned via inline
//                             style at GetPixelForIndex(CaretIndex)
//
// Coordinate math is monospace-only: charWidth × lineHeight come from the
// resolved fontSize (≈ 0.6 × fontSize for "M" in the Mafi monospace
// asset).  This is exact for true monospace fonts, which we get from the
// `Cls.fontMonospace` USS class.
//
// Public surface mirrors what MafiTextField exposed for the editor window:
//   Text (string)      — buffer; setter resets the caret if it'd land
//                        outside the new content
//   CaretIndex (int)   — caret position; setter clears the selection
//   AnchorIndex (int)  — selection anchor; equals CaretIndex when no
//                        selection is active
//   OnValueChanged     — fires on every buffer mutation (typing, paste,
//                        delete, undo, programmatic insert)
//   SyntaxHighlighter  — delegate the window injects so the editor stays
//                        agnostic to the PLC-PY-specific colorizer; if
//                        null, text renders without color tags
//   ReplaceRange / Insert / Focus — programmatic editing entry points
//                        used by the IntelliSense floater
public class PlcPyTextEditor : VisualElement {

	// Render layers, ordered back→front in Children.
	private readonly Label m_textLabel;
	private readonly VisualElement m_caret;
	private readonly VisualElement m_selectionLayer;
	// Hidden Label that always renders a single "M" with the same font
	// + size as the editor.  Its resolved layout gives us the EXACT char
	// width and line height Unity is using to render our text — much
	// more reliable than fontSize-ratio guesses or MeasureTextSize calls
	// that may return zero before the font asset is loaded.
	private readonly Label m_measureLabel;
	// Explicit fontSize so we don't inherit some Mafi-default that drifts
	// between window sizes / themes.  The measurement label uses the same
	// value, keeping visible text and metrics in lockstep.
	private const float FONT_SIZE = 14f;
	// 20 chars in the measurement string — long enough that the font's
	// one-time side-bearings amortise into sub-pixel noise when divided
	// out, short enough that the label's layout pass stays cheap.
	private const string MEASURE_SAMPLE_TEXT = "MMMMMMMMMMMMMMMMMMMM";
	private const int MEASURE_SAMPLE_COUNT = 20;

	private string m_text = "";
	private int m_caretIndex;
	private int m_anchorIndex;

	// Cache of newline positions for fast (line, col) ↔ index lookups.
	// Rebuilt whenever the buffer changes; List<int>[i] is the index of
	// the FIRST character of line i (line 0 starts at 0; line i+1 starts
	// at the index right after the i-th '\n').
	private readonly List<int> m_lineStarts = new List<int>(64) { 0 };

	// Monospace metrics — defaults are reasonable until MeasureMetrics
	// reads resolvedStyle.fontSize + measures "M" with the actual font.
	// All caret/selection placement reads m_originX/Y too, which track
	// the text label's real layout position so the cursor lines up with
	// the rendered glyphs even if the label inherits non-zero padding
	// from Mafi's USS rules.
	private float m_charWidth = 8f;
	private float m_lineHeight = 16f;
	private float m_originX = 4f;
	private float m_originY = 4f;

	// Mouse-drag state; CaptureMouse on press so drags off the editor
	// rect still extend the selection until release.
	private bool m_dragging;

	// Each entry is a buffer snapshot + caret position; pushed before any
	// mutation, popped on Ctrl+Z.  Not coalesced — each edit is its own
	// step.  Keeps memory bounded by capping at 200 entries (drops oldest).
	private readonly LinkedList<UndoSnapshot> m_undo = new LinkedList<UndoSnapshot>();
	private readonly LinkedList<UndoSnapshot> m_redo = new LinkedList<UndoSnapshot>();
	private const int MAX_UNDO = 200;

	private struct UndoSnapshot {
		public string Text;
		public int CaretIndex;
		public int AnchorIndex;
	}

	public string Text {
		get => m_text;
		set {
			string newText = value ?? "";
			if (m_text == newText) {
				return;
			}
			m_text = newText;
			RebuildLineStarts();
			ClampCaret();
			Render();
			OnValueChanged?.Invoke(m_text);
		}
	}

	public int CaretIndex {
		get => m_caretIndex;
		set {
			int clamped = Math.Max(0, Math.Min(m_text.Length, value));
			m_caretIndex = clamped;
			m_anchorIndex = clamped;
			RenderCaretAndSelection();
		}
	}

	public int AnchorIndex => m_anchorIndex;

	public bool HasSelection => m_caretIndex != m_anchorIndex;
	public int SelectionStart => Math.Min(m_caretIndex, m_anchorIndex);
	public int SelectionEnd => Math.Max(m_caretIndex, m_anchorIndex);

	public Action<string> OnValueChanged;

	// Window injects PlcPySyntax.ToRichText here so the editor itself
	// doesn't need to know about the PLC-PY token table.  Returns null
	// for empty / falsy → render the raw text.
	public Func<string, string> SyntaxHighlighter;

	// Pre-OnKeyDown hook so the window can claim Tab / Enter / arrows
	// for the IntelliSense floater BEFORE the editor handles them as
	// indent / newline / caret-move.  Return true to indicate "consumed"
	// — editor's own logic short-circuits and the bubble pass is stopped.
	// We use this instead of a separate KeyDownEvent registration because
	// even both being TrickleDown, the editor's constructor registers
	// first → its handler runs first → it would consume Tab before the
	// window's handler ever sees it.
	public Func<KeyDownEvent, bool> KeyDownInterceptor;

	public PlcPyTextEditor() {
		focusable = true;
		// tabIndex 0 keeps the editor in the focus ring so the window's
		// title-row focus chain still works; we just steal Tab keypresses
		// in OnKeyDown to insert spaces instead of cycling focus.
		tabIndex = 0;
		pickingMode = PickingMode.Position;
		style.position = Position.Relative;
		style.overflow = Overflow.Hidden;
		AddToClassList(Cls.fontMonospace);
		style.backgroundColor = new StyleColor(new UnityEngine.Color(0.10f, 0.10f, 0.11f, 1f));

		// Selection layer (back).  pickingMode=Ignore so it never eats
		// clicks meant for the editor; rectangles inside it are rebuilt
		// per render and inherit the same Ignore.
		m_selectionLayer = new VisualElement();
		m_selectionLayer.style.position = Position.Absolute;
		m_selectionLayer.style.left = 0;
		m_selectionLayer.style.top = 0;
		m_selectionLayer.style.right = 0;
		m_selectionLayer.style.bottom = 0;
		m_selectionLayer.pickingMode = PickingMode.Ignore;
		Add(m_selectionLayer);

		// Text label — single Label with rich-text colored content.  Pre
		// preserves whitespace and newlines.  All padding/margin forced
		// to 0 so the first glyph paints exactly at the label's layout
		// origin — any inherited USS padding from `Cls.fontMonospace`
		// would otherwise push the text inside the label and our caret
		// math would point to the wrong pixel.
		m_textLabel = new Label();
		m_textLabel.AddToClassList(Cls.fontMonospace);
		m_textLabel.enableRichText = true;
		m_textLabel.pickingMode = PickingMode.Ignore;
		m_textLabel.style.whiteSpace = WhiteSpace.Pre;
		m_textLabel.style.position = Position.Absolute;
		m_textLabel.style.left = 4;
		m_textLabel.style.top = 4;
		m_textLabel.style.fontSize = FONT_SIZE;
		m_textLabel.style.paddingLeft = 0;
		m_textLabel.style.paddingTop = 0;
		m_textLabel.style.paddingRight = 0;
		m_textLabel.style.paddingBottom = 0;
		m_textLabel.style.marginLeft = 0;
		m_textLabel.style.marginTop = 0;
		m_textLabel.style.marginRight = 0;
		m_textLabel.style.marginBottom = 0;
		m_textLabel.style.color = new StyleColor(new UnityEngine.Color(0.83f, 0.83f, 0.83f, 1f));
		Add(m_textLabel);

		// Hidden measurement label — renders MEASURE_SAMPLE_COUNT "M"s
		// with the same font + size as the visible text.  Its resolved
		// layout / MEASURE_SAMPLE_COUNT gives the per-character ADVANCE
		// (the gap between successive glyphs), which is what we need to
		// position caret + selection on column boundaries.  Measuring a
		// single character would include left/right side-bearings on the
		// font's outermost glyph, which don't repeat per-char in a real
		// text run — that overshoot is what made the selection extend
		// past the visible text by ~1 char per line.  20 M's averages
		// the side-bearings down to sub-pixel.
		m_measureLabel = new Label(MEASURE_SAMPLE_TEXT);
		m_measureLabel.AddToClassList(Cls.fontMonospace);
		m_measureLabel.style.position = Position.Absolute;
		m_measureLabel.style.left = -9999;
		m_measureLabel.style.top = -9999;
		m_measureLabel.style.fontSize = FONT_SIZE;
		m_measureLabel.style.paddingLeft = 0;
		m_measureLabel.style.paddingTop = 0;
		m_measureLabel.style.paddingRight = 0;
		m_measureLabel.style.paddingBottom = 0;
		m_measureLabel.style.opacity = 0;
		m_measureLabel.pickingMode = PickingMode.Ignore;
		Add(m_measureLabel);
		m_measureLabel.RegisterCallback<GeometryChangedEvent>(_ => MeasureMetrics());

		// Caret — thin vertical bar.  Hidden until first focus; width 1
		// is enough on Mafi's UI scale (anything thicker reads as a
		// selection block).
		m_caret = new VisualElement();
		m_caret.style.position = Position.Absolute;
		m_caret.style.width = 1;
		m_caret.style.backgroundColor = new StyleColor(new UnityEngine.Color(0.95f, 0.95f, 0.95f, 1f));
		m_caret.pickingMode = PickingMode.Ignore;
		m_caret.style.display = DisplayStyle.None;
		Add(m_caret);

		// TrickleDown phase so we beat any FocusController / parent handler
		// that might want to claim Tab, arrows, or Enter — those would
		// cycle focus or trigger Mafi UI defaults before our editor's
		// own logic runs.  StopPropagation in OnKeyDown then prevents the
		// bubble pass from delivering the same event to those handlers.
		RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
		RegisterCallback<MouseDownEvent>(OnMouseDown);
		RegisterCallback<MouseMoveEvent>(OnMouseMove);
		RegisterCallback<MouseUpEvent>(OnMouseUp);
		RegisterCallback<FocusInEvent>(_ => { m_caret.style.display = DisplayStyle.Flex; });
		RegisterCallback<FocusOutEvent>(_ => { m_caret.style.display = DisplayStyle.None; });
		RegisterCallback<GeometryChangedEvent>(_ => MeasureMetrics());
		// Re-measure when the text label finishes its initial layout —
		// MeasureTextSize / label.layout return zeroes before the first
		// resolve, so MeasureMetrics() called on the editor's geometry
		// alone misses the case where the label resolves later.
		m_textLabel.RegisterCallback<GeometryChangedEvent>(_ => MeasureMetrics());
	}

	// True when this editor element holds keyboard focus.  Used by the
	// owning window's InputUpdate override to consume Mafi's poll-based
	// input dispatch while the player is typing — UIElements'
	// StopPropagation doesn't reach Mafi.Core's `Input.GetKeyDown` poll,
	// so we have to gate at the controller level too.
	public bool HasFocus() {
		return focusController != null && focusController.focusedElement == this;
	}

	// ---- Public programmatic editing -----------------------------------

	// Replaces the buffer slice [start, end) with `replacement` and
	// positions the caret right after the inserted text.  Used by the
	// IntelliSense floater to swap a partial token for the chosen
	// completion; also the implementation behind every other editing
	// command in this file.
	public void ReplaceRange(int start, int end, string replacement) {
		if (start < 0) start = 0;
		if (end > m_text.Length) end = m_text.Length;
		if (end < start) end = start;
		replacement = replacement ?? "";

		PushUndo();
		m_text = m_text.Substring(0, start) + replacement + m_text.Substring(end);
		RebuildLineStarts();
		m_caretIndex = start + replacement.Length;
		m_anchorIndex = m_caretIndex;
		Render();
		OnValueChanged?.Invoke(m_text);
	}

	// Inserts text at the current caret position.  When a selection is
	// active the selection is replaced (matches every text editor's
	// behavior; surprising otherwise — typing while text is selected
	// would otherwise just appear after the selection).
	public void Insert(string text) {
		if (HasSelection) {
			ReplaceRange(SelectionStart, SelectionEnd, text);
		} else {
			ReplaceRange(m_caretIndex, m_caretIndex, text);
		}
	}

	public void SelectAll() {
		m_anchorIndex = 0;
		m_caretIndex = m_text.Length;
		RenderCaretAndSelection();
	}

	// ---- Keyboard input -------------------------------------------------

	private void OnKeyDown(KeyDownEvent evt) {
		// Hand the event to the window's interceptor first — the floater
		// nav (Up/Down/Enter/Tab/Ctrl+Space) needs to win against the
		// editor's own bindings while the popup is open.  If the
		// interceptor reports it consumed the event, stop here.
		if (KeyDownInterceptor != null && KeyDownInterceptor(evt)) {
			return;
		}

		bool ctrl = evt.ctrlKey || evt.commandKey;
		bool shift = evt.shiftKey;

		// Navigation + editing commands first; character input falls
		// through at the end if no command matched and a printable char
		// is in evt.character.
		switch (evt.keyCode) {
			case UnityEngine.KeyCode.LeftArrow:
				MoveCaretTo(ctrl ? PrevWordIndex(m_caretIndex) : Math.Max(0, m_caretIndex - 1), shift);
				evt.StopPropagation(); evt.PreventDefault(); return;
			case UnityEngine.KeyCode.RightArrow:
				MoveCaretTo(ctrl ? NextWordIndex(m_caretIndex) : Math.Min(m_text.Length, m_caretIndex + 1), shift);
				evt.StopPropagation(); evt.PreventDefault(); return;
			case UnityEngine.KeyCode.UpArrow:
				MoveCaretTo(MoveByLine(m_caretIndex, -1), shift);
				evt.StopPropagation(); evt.PreventDefault(); return;
			case UnityEngine.KeyCode.DownArrow:
				MoveCaretTo(MoveByLine(m_caretIndex, +1), shift);
				evt.StopPropagation(); evt.PreventDefault(); return;
			case UnityEngine.KeyCode.PageUp:
				MoveCaretTo(MoveByLine(m_caretIndex, -10), shift);
				evt.StopPropagation(); evt.PreventDefault(); return;
			case UnityEngine.KeyCode.PageDown:
				MoveCaretTo(MoveByLine(m_caretIndex, +10), shift);
				evt.StopPropagation(); evt.PreventDefault(); return;
			case UnityEngine.KeyCode.Home:
				MoveCaretTo(ctrl ? 0 : LineStart(m_caretIndex), shift);
				evt.StopPropagation(); evt.PreventDefault(); return;
			case UnityEngine.KeyCode.End:
				MoveCaretTo(ctrl ? m_text.Length : LineEnd(m_caretIndex), shift);
				evt.StopPropagation(); evt.PreventDefault(); return;
			case UnityEngine.KeyCode.Backspace:
				if (HasSelection) {
					ReplaceRange(SelectionStart, SelectionEnd, "");
				} else if (ctrl) {
					int wordStart = PrevWordIndex(m_caretIndex);
					if (wordStart < m_caretIndex) {
						ReplaceRange(wordStart, m_caretIndex, "");
					}
				} else if (m_caretIndex > 0) {
					ReplaceRange(m_caretIndex - 1, m_caretIndex, "");
				}
				evt.StopPropagation(); evt.PreventDefault(); return;
			case UnityEngine.KeyCode.Delete:
				if (HasSelection) {
					ReplaceRange(SelectionStart, SelectionEnd, "");
				} else if (ctrl) {
					int wordEnd = NextWordIndex(m_caretIndex);
					if (wordEnd > m_caretIndex) {
						ReplaceRange(m_caretIndex, wordEnd, "");
					}
				} else if (m_caretIndex < m_text.Length) {
					ReplaceRange(m_caretIndex, m_caretIndex + 1, "");
				}
				evt.StopPropagation(); evt.PreventDefault(); return;
			case UnityEngine.KeyCode.Return:
			case UnityEngine.KeyCode.KeypadEnter:
				Insert("\n");
				evt.StopPropagation(); evt.PreventDefault(); return;
			case UnityEngine.KeyCode.Tab:
				// Four-space indent — the default Tab focus-cycle would be
				// useless mid-code.  Shift+Tab is intentionally NOT handled
				// here so the player can still tab out of the editor when
				// they really want to (matches existing behavior).
				if (!shift) {
					Insert("    ");
					evt.StopPropagation(); evt.PreventDefault(); return;
				}
				return;
		}

		if (ctrl) {
			switch (evt.keyCode) {
				case UnityEngine.KeyCode.A: SelectAll(); evt.StopPropagation(); evt.PreventDefault(); return;
				case UnityEngine.KeyCode.C: CopySelection(); evt.StopPropagation(); evt.PreventDefault(); return;
				case UnityEngine.KeyCode.X: CutSelection(); evt.StopPropagation(); evt.PreventDefault(); return;
				case UnityEngine.KeyCode.V: PasteClipboard(); evt.StopPropagation(); evt.PreventDefault(); return;
				case UnityEngine.KeyCode.Z: Undo(); evt.StopPropagation(); evt.PreventDefault(); return;
				case UnityEngine.KeyCode.Y: Redo(); evt.StopPropagation(); evt.PreventDefault(); return;
			}
			// Unhandled ctrl combos fall through silently — we don't want
			// to insert character input for things like Ctrl+S that the
			// window may bind separately.
			return;
		}

		// Character input — Unity packs the typed character (if any) on
		// the KeyDownEvent.  '\0' means "no character" (function key,
		// modifier, etc.); IsControl filters out CR/LF/Tab which are
		// handled above as keyCodes.
		char c = evt.character;
		if (c != '\0' && !char.IsControl(c)) {
			Insert(c.ToString());
			evt.StopPropagation(); evt.PreventDefault();
		}
	}

	// ---- Mouse input ----------------------------------------------------

	private void OnMouseDown(MouseDownEvent evt) {
		if (evt.button != 0) {
			return;
		}
		Focus();
		int idx = GetIndexForLocalPoint(evt.localMousePosition);
		m_caretIndex = idx;
		// Shift-click extends the existing selection from the current
		// anchor; plain click resets the anchor to the new caret.
		if (!evt.shiftKey) {
			m_anchorIndex = idx;
		}
		m_dragging = true;
		this.CaptureMouse();
		RenderCaretAndSelection();
		evt.StopPropagation();
	}

	private void OnMouseMove(MouseMoveEvent evt) {
		if (!m_dragging) {
			return;
		}
		m_caretIndex = GetIndexForLocalPoint(evt.localMousePosition);
		RenderCaretAndSelection();
	}

	private void OnMouseUp(MouseUpEvent evt) {
		if (!m_dragging) {
			return;
		}
		m_dragging = false;
		this.ReleaseMouse();
	}

	// ---- Clipboard ------------------------------------------------------

	private void CopySelection() {
		if (!HasSelection) {
			return;
		}
		UnityEngine.GUIUtility.systemCopyBuffer = m_text.Substring(SelectionStart, SelectionEnd - SelectionStart);
	}

	private void CutSelection() {
		if (!HasSelection) {
			return;
		}
		UnityEngine.GUIUtility.systemCopyBuffer = m_text.Substring(SelectionStart, SelectionEnd - SelectionStart);
		ReplaceRange(SelectionStart, SelectionEnd, "");
	}

	private void PasteClipboard() {
		string clip = UnityEngine.GUIUtility.systemCopyBuffer;
		if (string.IsNullOrEmpty(clip)) {
			return;
		}
		// Normalise CRLF to \n so pastes from Windows-line-ending sources
		// don't sprinkle extra blank lines through the script.
		clip = clip.Replace("\r\n", "\n").Replace("\r", "\n");
		Insert(clip);
	}

	// ---- Undo / redo ----------------------------------------------------

	private void PushUndo() {
		m_undo.AddLast(new UndoSnapshot {
			Text = m_text,
			CaretIndex = m_caretIndex,
			AnchorIndex = m_anchorIndex,
		});
		while (m_undo.Count > MAX_UNDO) {
			m_undo.RemoveFirst();
		}
		// Any new edit invalidates the redo stack — matches every editor's
		// behavior; redoing what was overwritten is not reachable.
		m_redo.Clear();
	}

	private void Undo() {
		if (m_undo.Count == 0) {
			return;
		}
		UndoSnapshot snap = m_undo.Last.Value;
		m_undo.RemoveLast();
		m_redo.AddLast(new UndoSnapshot {
			Text = m_text,
			CaretIndex = m_caretIndex,
			AnchorIndex = m_anchorIndex,
		});
		m_text = snap.Text;
		m_caretIndex = snap.CaretIndex;
		m_anchorIndex = snap.AnchorIndex;
		RebuildLineStarts();
		Render();
		OnValueChanged?.Invoke(m_text);
	}

	private void Redo() {
		if (m_redo.Count == 0) {
			return;
		}
		UndoSnapshot snap = m_redo.Last.Value;
		m_redo.RemoveLast();
		m_undo.AddLast(new UndoSnapshot {
			Text = m_text,
			CaretIndex = m_caretIndex,
			AnchorIndex = m_anchorIndex,
		});
		m_text = snap.Text;
		m_caretIndex = snap.CaretIndex;
		m_anchorIndex = snap.AnchorIndex;
		RebuildLineStarts();
		Render();
		OnValueChanged?.Invoke(m_text);
	}

	// ---- Caret movement helpers ----------------------------------------

	// Moves caret to `target`; if `extendSelection` is true, the anchor is
	// kept (so shift+arrow grows the selection); otherwise the anchor
	// follows the caret (no selection).
	private void MoveCaretTo(int target, bool extendSelection) {
		m_caretIndex = Math.Max(0, Math.Min(m_text.Length, target));
		if (!extendSelection) {
			m_anchorIndex = m_caretIndex;
		}
		RenderCaretAndSelection();
	}

	// Walks back over whitespace, then over an identifier-like run.
	// "Word" here matches editor convention: skip consecutive whitespace,
	// then either a run of word chars or a single non-word punctuation.
	private int PrevWordIndex(int from) {
		int i = from;
		// skip whitespace immediately to the left
		while (i > 0 && char.IsWhiteSpace(m_text[i - 1])) {
			i--;
		}
		if (i == 0) {
			return 0;
		}
		bool wordChar = IsWordChar(m_text[i - 1]);
		while (i > 0 && IsWordChar(m_text[i - 1]) == wordChar && !char.IsWhiteSpace(m_text[i - 1])) {
			i--;
		}
		return i;
	}

	private int NextWordIndex(int from) {
		int i = from;
		int n = m_text.Length;
		// skip a run of identical-class chars
		if (i < n) {
			bool wordChar = IsWordChar(m_text[i]);
			while (i < n && !char.IsWhiteSpace(m_text[i]) && IsWordChar(m_text[i]) == wordChar) {
				i++;
			}
		}
		// then skip trailing whitespace
		while (i < n && char.IsWhiteSpace(m_text[i]) && m_text[i] != '\n') {
			i++;
		}
		return i;
	}

	private static bool IsWordChar(char c) {
		return char.IsLetterOrDigit(c) || c == '_';
	}

	private int LineStart(int index) {
		(int line, _) = GetLineCol(index);
		return m_lineStarts[line];
	}

	private int LineEnd(int index) {
		(int line, _) = GetLineCol(index);
		if (line + 1 < m_lineStarts.Count) {
			return m_lineStarts[line + 1] - 1; // before the '\n'
		}
		return m_text.Length;
	}

	// Move by N lines, preserving the column when possible.  Lines past
	// EOF clamp to the last line; lines past start clamp to line 0.
	private int MoveByLine(int index, int delta) {
		(int line, int col) = GetLineCol(index);
		int targetLine = Math.Max(0, Math.Min(m_lineStarts.Count - 1, line + delta));
		int lineStart = m_lineStarts[targetLine];
		int lineLength = (targetLine + 1 < m_lineStarts.Count
			? m_lineStarts[targetLine + 1] - 1
			: m_text.Length) - lineStart;
		return lineStart + Math.Min(col, lineLength);
	}

	// ---- Coordinate conversions ----------------------------------------

	private (int line, int col) GetLineCol(int index) {
		if (index <= 0) {
			return (0, 0);
		}
		// Binary search for the largest lineStart ≤ index — m_lineStarts
		// is sorted ascending so this is O(log lines).
		int lo = 0, hi = m_lineStarts.Count - 1;
		while (lo < hi) {
			int mid = (lo + hi + 1) / 2;
			if (m_lineStarts[mid] <= index) {
				lo = mid;
			} else {
				hi = mid - 1;
			}
		}
		return (lo, index - m_lineStarts[lo]);
	}

	// Public so the IntelliSense floater can anchor itself to the trigger
	// position rather than the live caret (lets the popup stay put while
	// the player types the partial token).
	public UnityEngine.Vector2 GetPixelForIndex(int index) {
		(int line, int col) = GetLineCol(index);
		// Origin tracks the text label's real layout position (set in
		// MeasureMetrics), so caret + selection sit on top of the glyphs
		// regardless of whatever USS padding Mafi adds to the label.
		float x = m_originX + col * m_charWidth;
		float y = m_originY + line * m_lineHeight;
		return new UnityEngine.Vector2(x, y);
	}

	private int GetIndexForLocalPoint(UnityEngine.Vector2 local) {
		// Reverse of GetPixelForIndex: subtract the inset, divide by char
		// metrics to get (line, col), then walk to the nearest valid index.
		float x = local.x - m_originX;
		float y = local.y - m_originY;
		int line = (int)Math.Floor(y / m_lineHeight);
		if (line < 0) line = 0;
		if (line >= m_lineStarts.Count) line = m_lineStarts.Count - 1;
		int lineStart = m_lineStarts[line];
		int lineEnd = (line + 1 < m_lineStarts.Count
			? m_lineStarts[line + 1] - 1
			: m_text.Length);
		int col = (int)Math.Round(x / m_charWidth);
		if (col < 0) col = 0;
		int idx = lineStart + col;
		if (idx > lineEnd) idx = lineEnd;
		return idx;
	}

	// Public helper for the IntelliSense floater so it can position
	// itself under the caret in the parent's local coords without having
	// to know about our internals.  Returns the caret pixel position in
	// the editor's local space; the caller is responsible for converting
	// to whatever frame it needs.
	public UnityEngine.Vector2 GetCaretLocalPosition() {
		return GetPixelForIndex(m_caretIndex);
	}

	public float LineHeight => m_lineHeight;
	public float CharWidth => m_charWidth;

	// ---- Rendering ------------------------------------------------------

	private void Render() {
		RenderText();
		RenderCaretAndSelection();
	}

	private void RenderText() {
		string toShow = m_text;
		if (SyntaxHighlighter != null) {
			try {
				toShow = SyntaxHighlighter(m_text);
			} catch {
				// Highlighter failures fall back to raw text so a mid-edit
				// tokenizer error doesn't blank the editor.
				toShow = EscapeRichText(m_text);
			}
		} else {
			toShow = EscapeRichText(m_text);
		}
		m_textLabel.text = toShow;
	}

	private void RenderCaretAndSelection() {
		// Caret position
		UnityEngine.Vector2 px = GetPixelForIndex(m_caretIndex);
		m_caret.style.left = px.x;
		m_caret.style.top = px.y;
		m_caret.style.height = m_lineHeight;

		// Selection rectangles — clear and rebuild every frame.  At most
		// one rectangle per line of the selection, so the cost scales
		// with line count, not character count.
		m_selectionLayer.Clear();
		if (!HasSelection) {
			return;
		}
		int start = SelectionStart;
		int end = SelectionEnd;
		(int startLine, int startCol) = GetLineCol(start);
		(int endLine, int endCol) = GetLineCol(end);

		StyleColor selectionColor = new StyleColor(new UnityEngine.Color(0.21f, 0.36f, 0.55f, 0.55f));
		for (int line = startLine; line <= endLine; line++) {
			int lineStart = m_lineStarts[line];
			int lineLen = (line + 1 < m_lineStarts.Count
				? m_lineStarts[line + 1] - 1
				: m_text.Length) - lineStart;
			int colA = (line == startLine) ? startCol : 0;
			int colB = (line == endLine) ? endCol : lineLen;
			// Show a small trailing block at end-of-line when a multiline
			// selection wraps so the player can see "the newline is
			// included" — Visual-Studio convention.
			float widthChars = colB - colA;
			if (line < endLine) {
				widthChars += 0.5f;
			}
			VisualElement rect = new VisualElement();
			rect.pickingMode = PickingMode.Ignore;
			rect.style.position = Position.Absolute;
			rect.style.left = m_originX + colA * m_charWidth;
			rect.style.top = m_originY + line * m_lineHeight;
			rect.style.width = Math.Max(2f, widthChars * m_charWidth);
			rect.style.height = m_lineHeight;
			rect.style.backgroundColor = selectionColor;
			m_selectionLayer.Add(rect);
		}
	}

	// Reads the visible text label's actual layout origin + the hidden
	// measurement label's rendered size, so caret / selection placements
	// match what Unity actually paints.  Called on every
	// GeometryChangedEvent from the editor, the text label, and the
	// measurement label — text and measurement labels can resolve at
	// different ticks, so we re-run whenever any of them settles.
	//
	// Using a real rendered "M" instead of MeasureTextSize avoids a
	// race: MeasureTextSize returns 0 before the font asset is loaded,
	// but a Label that's been added to the visual tree will eventually
	// fire GeometryChangedEvent with a real size once the font resolves.
	private void MeasureMetrics() {
		UnityEngine.Rect measureLayout = m_measureLabel.layout;
		// Divide by sample count so per-line side-bearings don't show up
		// in the per-char advance — measuring a single char would over-
		// state width by ~1 char's worth across a long line, which is
		// exactly the "one character off" symptom.
		float newCharWidth = measureLayout.width / MEASURE_SAMPLE_COUNT;
		float newLineHeight = measureLayout.height;
		// Bail until the measurement label has actually rendered with
		// a positive size; defaults stay in place and the next
		// GeometryChangedEvent re-runs us.
		if (newCharWidth <= 0 || newLineHeight <= 0
			|| float.IsNaN(newCharWidth) || float.IsNaN(newLineHeight)) {
			return;
		}

		// The text label's layout rect tells us where the actual glyphs
		// begin in the editor's local coords (label.style.left=4 plus any
		// padding the Cls.fontMonospace USS rule adds — we forced our own
		// inline padding to 0 above to keep the inset minimal, but reading
		// layout still wins if anything else shifts the label).
		UnityEngine.Rect labelLayout = m_textLabel.layout;
		float newOriginX = float.IsNaN(labelLayout.x) ? 4f : labelLayout.x;
		float newOriginY = float.IsNaN(labelLayout.y) ? 4f : labelLayout.y;

		bool changed = Math.Abs(newCharWidth - m_charWidth) > 0.01f
			|| Math.Abs(newLineHeight - m_lineHeight) > 0.01f
			|| Math.Abs(newOriginX - m_originX) > 0.01f
			|| Math.Abs(newOriginY - m_originY) > 0.01f;
		if (!changed) {
			return;
		}
		m_charWidth = newCharWidth;
		m_lineHeight = newLineHeight;
		m_originX = newOriginX;
		m_originY = newOriginY;
		RenderCaretAndSelection();
	}

	// ---- Buffer maintenance --------------------------------------------

	private void RebuildLineStarts() {
		m_lineStarts.Clear();
		m_lineStarts.Add(0);
		for (int i = 0; i < m_text.Length; i++) {
			if (m_text[i] == '\n') {
				m_lineStarts.Add(i + 1);
			}
		}
	}

	private void ClampCaret() {
		if (m_caretIndex > m_text.Length) m_caretIndex = m_text.Length;
		if (m_anchorIndex > m_text.Length) m_anchorIndex = m_text.Length;
	}

	// Escapes `<` and `&` so a script containing literal angle brackets
	// (e.g. a string `"<not a tag>"`) doesn't get parsed as a rich-text
	// tag.  Mirrors the same logic in PlcPySyntax.AppendEscaped used by
	// the colorizer.
	private static string EscapeRichText(string s) {
		if (string.IsNullOrEmpty(s)) {
			return "";
		}
		StringBuilder sb = new StringBuilder(s.Length);
		for (int i = 0; i < s.Length; i++) {
			char c = s[i];
			if (c == '<') sb.Append("&lt;");
			else if (c == '&') sb.Append("&amp;");
			else sb.Append(c);
		}
		return sb.ToString();
	}
}
