using System;
using System.Collections.Generic;
using System.Text;
using ProgramableNetwork.Python;

namespace ProgramableNetwork;

// Helpers shared by the PLC-PY editor: source → rich-text colorizer for
// the live preview pane, plus a tooltip dictionary for the well-known
// identifiers the player will see (self, self.Input, fix, Fix32, etc.).
//
// The tokenizer is reused as-is — we run it on the live editor buffer
// every keystroke and re-emit the source with `<color>` tags wrapped
// around each token's character range.  That keeps the colorizer in
// sync with what the runtime parser would actually accept; if the
// tokenizer can't lex a snippet (e.g., mid-edit unclosed string), we
// fall back to the raw escaped source so the panel still renders.
public static class PlcPySyntax {

	// VS-Code "Dark+" -ish palette.  Picked for legibility on the
	// existing dark Mafi window background and rough WCAG-AA contrast.
	private const string COLOR_KEYWORD  = "#569cd6";  // blue   — control flow + literals
	private const string COLOR_STRING   = "#ce9178";  // orange — string literals
	private const string COLOR_NUMBER   = "#b5cea8";  // green  — numeric literals
	private const string COLOR_COMMENT  = "#6a9955";  // green  — comments
	private const string COLOR_OPERATOR = "#dcdcaa";  // tan    — operators / punctuation

	// Identifier docs displayed when the player hovers a known name in
	// the editor.  Keep entries short — these render in a tooltip strip.
	// `self.X` paths are matched after a dot lookup so `self.Input.A`
	// resolves to the `self.Input` doc plus the dotted-accessor reminder.
	public readonly struct Completion {
		public readonly string Name;
		public readonly string Doc;
		public Completion(string name, string doc) { Name = name; Doc = doc; }
	}

	private static readonly Completion[] EMPTY = new Completion[0];

	// Members offered by the IntelliSense floater for a given dotted parent.
	// Empty parent ("") → top-level names; "self" → wrapper sub-views; etc.
	// Returns EMPTY when the parent isn't recognized so the caller can use
	// the result directly without null-checks.
	public static IReadOnlyList<Completion> GetCompletions(string parent) {
		switch (parent ?? "") {
			case "":
				return new[] {
					new Completion("self",  "Module wrapper bound to this PLC instance."),
					new Completion("fix",   "fix(value) — convert int/float to Fix32."),
					new Completion("Fix32", "Fix32 type. Common: Fix32.Zero, Fix32.One, Fix32.Half."),
					new Completion("True",  "Boolean true."),
					new Completion("False", "Boolean false."),
					new Completion("None",  "Null / not-connected sentinel."),
					new Completion("if",    "Conditional branch (if expr:)."),
					new Completion("elif",  "Else-if branch."),
					new Completion("else",  "Else branch."),
					new Completion("and",   "Boolean and."),
					new Completion("or",    "Boolean or."),
					new Completion("not",   "Boolean not."),
					new Completion("return","Exit current call with a value."),
				};
			case "self":
				return new[] {
					new Completion("Input",        "Input pins."),
					new Completion("Output",       "Output pins."),
					new Completion("Field",        "Player-set fields."),
					new Completion("FieldOrInput", "Read-only coalesced view: input override OR field."),
					new Completion("Display",      "Module-row text/LED."),
					new Completion("Array",        "Persistent Fix32[] scratch buffer."),
					new Completion("NumberData",   "Persistent int dictionary."),
					new Completion("StringData",   "Persistent string dictionary."),
				};
			case "self.Input":
			case "self.Output":
			case "self.Field":
				return new[] {
					new Completion("A",        "Pin A (dotted access)."),
					new Completion("B",        "Pin B."),
					new Completion("C",        "Pin C."),
					new Completion("D",        "Pin D."),
					new Completion("get",      "get(name, default) — read as Fix32."),
					new Completion("set",      "set(name, value) — write Fix32."),
					new Completion("get_int",  "get_int(name, default) — read as int."),
					new Completion("set_int",  "set_int(name, value) — write int."),
					new Completion("get_bool", "get_bool(name, default) — read as bool."),
					new Completion("set_bool", "set_bool(name, value) — write bool."),
				};
			case "self.FieldOrInput":
				return new[] {
					new Completion("get",      "get(name, default) — read as Fix32."),
					new Completion("get_int",  "get_int(name, default) — read as int."),
					new Completion("get_bool", "get_bool(name, default) — read as bool."),
				};
			case "self.Display":
				return new[] {
					new Completion("get", "get(name, default) — read string."),
					new Completion("set", "set(name, value) — write string."),
				};
			case "self.Array":
				return new[] {
					new Completion("length",          "Current array length."),
					new Completion("get",             "get(index, default) — read slot."),
					new Completion("set",             "set(index, value) — write slot."),
					new Completion("resize",          "resize(size) or resize(size, fill_new)."),
					new Completion("clear",           "Clear all slots."),
					new Completion("shift_left_with", "shift_left_with(incoming) — atomic shift register step."),
				};
			case "self.NumberData":
			case "self.StringData":
				// Dynamic dict — any key works; surface a placeholder so the
				// floater doesn't appear empty when triggered on these.
				return new[] {
					new Completion("(any name)", "Persistent dictionary — any name works as a key."),
				};
			case "Fix32":
				return new[] {
					new Completion("Zero",     "Fix32 zero."),
					new Completion("One",      "Fix32 one."),
					new Completion("Half",     "Fix32 0.5."),
					new Completion("FromInt",  "Fix32.FromInt(i) — int → Fix32."),
				};
			default:
				return EMPTY;
		}
	}

	public static readonly IReadOnlyDictionary<string, string> Docs = new Dictionary<string, string> {
		{ "self",          "Module wrapper bound to this PLC instance." },
		{ "self.Input",    "Input pins. .get(name, default) reads, or .NAME reads via dotted access." },
		{ "self.Output",   "Output pins. .set(name, value), .set_int, .set_bool, or .NAME = value via dotted access." },
		{ "self.Field",    "Player-set fields. Same shape as Input/Output (get/set + dotted)." },
		{ "self.FieldOrInput", "Read-only: returns the override input value if connected, else the field." },
		{ "self.Display",  "Module-row text/LED. .set(name, str) or .NAME = str via dotted access." },
		{ "self.Array",    "Persistent Fix32[] scratch buffer. .get(i, default), .set(i, v), .resize(n), .shift_left_with(v)." },
		{ "self.NumberData", "Persistent int dictionary. ['key'] / .key indexer + dotted access." },
		{ "self.StringData", "Persistent string dictionary. Same shape as NumberData." },
		{ "fix",           "fix(value) — convert int/float to Fix32." },
		{ "Fix32",         "Fix32 type. Common: Fix32.Zero, Fix32.One, Fix32.Half." },
		{ "True",          "Boolean true." },
		{ "False",         "Boolean false." },
		{ "None",          "Null / not-connected sentinel." },
	};

	// Walks the tokens once, building a colored rich-text version of the
	// original source.  Whitespace + tokens the tokenizer skips (newline,
	// indent, dedent) get emitted verbatim from the gap between consecutive
	// token positions, so layout-sensitive code lines up the way it does
	// in the editor.
	public static string ToRichText(string source) {
		if (string.IsNullOrEmpty(source)) {
			return "";
		}

		Token[] tokens;
		try {
			tokens = Tokenizer.ParseString(source, "PLC_PY.highlight");
		} catch {
			// Mid-edit failures (unclosed string, bad indent) fall through
			// to a raw-but-escaped render; live preview keeps showing the
			// player's text instead of going blank.
			return Escape(source);
		}

		List<int> lineOffsets = ComputeLineOffsets(source);
		List<ColorSpan> spans = new List<ColorSpan>(tokens.Length);
		foreach (Token token in tokens) {
			string color = ColorFor(token.type);
			if (color == null) {
				continue;
			}
			int lineIdx = token.line - 1;
			if (lineIdx < 0 || lineIdx >= lineOffsets.Count) {
				continue;
			}
			int start = lineOffsets[lineIdx] + token.column;
			int length = token.length;
			if (length <= 0 || start < 0 || start + length > source.Length) {
				continue;
			}
			spans.Add(new ColorSpan { Start = start, Length = length, Color = color });
		}

		spans.Sort((a, b) => a.Start.CompareTo(b.Start));

		StringBuilder sb = new StringBuilder(source.Length + spans.Count * 24);
		int pos = 0;
		foreach (ColorSpan span in spans) {
			// Skip overlapping spans (defensive — shouldn't happen with a
			// well-behaved tokenizer, but the fallback keeps output sane).
			if (span.Start < pos) {
				continue;
			}
			if (span.Start > pos) {
				AppendEscaped(sb, source, pos, span.Start - pos);
			}
			sb.Append("<color=").Append(span.Color).Append(">");
			AppendEscaped(sb, source, span.Start, span.Length);
			sb.Append("</color>");
			pos = span.Start + span.Length;
		}
		if (pos < source.Length) {
			AppendEscaped(sb, source, pos, source.Length - pos);
		}
		return sb.ToString();
	}

	private struct ColorSpan {
		public int Start;
		public int Length;
		public string Color;
	}

	private static List<int> ComputeLineOffsets(string source) {
		List<int> offsets = new List<int> { 0 };
		for (int i = 0; i < source.Length; i++) {
			if (source[i] == '\n') {
				offsets.Add(i + 1);
			}
		}
		return offsets;
	}

	private static string ColorFor(PythonTokens type) {
		switch (type) {
			case PythonTokens.ifp:
			case PythonTokens.elif:
			case PythonTokens.elsep:
			case PythonTokens.returnp:
			case PythonTokens.classp:
			case PythonTokens.def:
			case PythonTokens.from:
			case PythonTokens.import:
			case PythonTokens.and:
			case PythonTokens.or:
			case PythonTokens.not:
			case PythonTokens.ink:
			case PythonTokens.isp:
			case PythonTokens.pass:
			case PythonTokens.none:
			case PythonTokens.btrue:
			case PythonTokens.bfalse:
				return COLOR_KEYWORD;
			case PythonTokens.str:
			case PythonTokens.mstr:
			case PythonTokens.fstrbegin:
			case PythonTokens.fstrmiddle:
				return COLOR_STRING;
			case PythonTokens.number:
				return COLOR_NUMBER;
			case PythonTokens.comment:
				return COLOR_COMMENT;
			case PythonTokens.eq:
			case PythonTokens.neq:
			case PythonTokens.lre:
			case PythonTokens.gre:
			case PythonTokens.lr:
			case PythonTokens.gr:
			case PythonTokens.set:
			case PythonTokens.plus:
			case PythonTokens.minus:
			case PythonTokens.mul:
			case PythonTokens.div:
			case PythonTokens.divint:
			case PythonTokens.mod:
			case PythonTokens.power:
			case PythonTokens.bitand:
			case PythonTokens.bitor:
			case PythonTokens.bitxor:
			case PythonTokens.shiftl:
			case PythonTokens.shiftr:
			case PythonTokens.invert:
				return COLOR_OPERATOR;
			default:
				return null;
		}
	}

	private static string Escape(string s) {
		// Rich-text uses `<...>` for tags; escape `<` so the player's literal
		// angle brackets don't accidentally close our color spans.  `&` is
		// also a Unity rich-text escape character, so guard it too.
		return s.Replace("&", "&amp;").Replace("<", "&lt;");
	}

	private static void AppendEscaped(StringBuilder sb, string s, int start, int length) {
		for (int i = 0; i < length; i++) {
			char c = s[start + i];
			if (c == '<') {
				sb.Append("&lt;");
			} else if (c == '&') {
				sb.Append("&amp;");
			} else {
				sb.Append(c);
			}
		}
	}
}
