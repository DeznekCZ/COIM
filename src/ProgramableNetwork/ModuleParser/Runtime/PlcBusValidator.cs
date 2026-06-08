using System.Collections.Generic;

namespace ProgramableNetwork.Python
{
	// Compile-time check that a PLC-PY script only accesses bus pins in the
	// direction the pin's type allows.  Each ControllerBus pin carries a
	// BusPinType (Input / Controller / Plc / NetworkRead).  From a PLC
	// script's perspective:
	//   - Plc pins are write-only  (they exist so the PLC can emit a value
	//     that other modules / controllers / network bridges read).
	//   - Input / Controller / NetworkRead pins are read-only (cables and
	//     external sources feed them; the PLC only consumes).
	//
	// Scans the token stream produced by Tokenizer.ParseString for the four
	// access shapes Custom/template.py players actually use:
	//   self.Bus.<bus>.<pin>           — dotted read or write (direction
	//                                    inferred from the token after the
	//                                    pin: `=` / compound-assign = write,
	//                                    anything else = read)
	//   self.Bus.<bus>.set(...)        — explicit write (any of set / set_int
	//                                    / set_bool); first string-literal
	//                                    arg is the pin name
	//   self.Bus.<bus>.get(...)        — explicit read (any of get / get_int
	//                                    / get_bool)
	//   self.Bus.<bus>[<int>]          — by-index access; resolves the pin
	//                                    via PinNames[i] when the index is
	//                                    an integer literal
	//
	// Dynamic patterns (`b = self.Bus.main; b.x = 1`, `self.Bus[busVar][pinVar]`,
	// computed pin names) slip past this scan — the runtime falls through to
	// existing behaviour for those (silent zero on read of an unconfigured
	// pin, dropped write).  The check is best-effort: it catches the cases
	// where a player typed the names inline, which is the overwhelming
	// majority.
	internal static class PlcBusValidator
	{
		// Methods whose first arg is a pin name and whose direction is fixed
		// by their name.  Mirrors the BusPinSetter API on ModuleWrapper.
		private static readonly HashSet<string> READ_METHODS = new HashSet<string> {
			"get", "get_int", "get_bool",
		};
		private static readonly HashSet<string> WRITE_METHODS = new HashSet<string> {
			"set", "set_int", "set_bool",
		};

		// Tokens the scan transparently steps over when looking for "the next
		// real token" — same set Lexer ignores while building expressions.
		private static bool IsTrivia(PythonTokens t)
		{
			return t == PythonTokens.newline
				|| t == PythonTokens.comment
				|| t == PythonTokens.indent
				|| t == PythonTokens.dedent;
		}

		// All compound-assignment tokens — a `self.Bus.X.pin OP= rhs` line
		// both READS the pin (to compute the new value) and WRITES it back,
		// so the validator demands the pin allow both directions.  In the
		// current bus model no single pin can be both readable and writable
		// at once, so compound-assigning a bus pin always surfaces an error
		// — which is the right outcome: the player has to pick a direction.
		private static bool IsCompoundAssign(PythonTokens t)
		{
			return t == PythonTokens.setplus
				|| t == PythonTokens.setminus
				|| t == PythonTokens.setmul
				|| t == PythonTokens.setdiv
				|| t == PythonTokens.setshl
				|| t == PythonTokens.setshr;
		}

		public static void Validate(Token[] tokens, Controller controller)
		{
			// Bus validation is controller-scoped; without a controller the
			// PLC is being compiled in isolation (e.g. the editor's offline
			// preview) and we have nothing to check against.  Lex-level
			// errors still surface.
			if (controller == null || tokens == null) {
				return;
			}

			for (int i = 0; i < tokens.Length; i++)
			{
				// Anchor: a bare `self` name token followed by `.Bus`.  Skip
				// past trivia so a comment between the dots doesn't hide an
				// otherwise valid pattern.
				if (tokens[i].type != PythonTokens.name || tokens[i].value != "self") {
					continue;
				}
				int dotBus = NextNonTrivia(tokens, i);
				if (dotBus < 0 || tokens[dotBus].type != PythonTokens.dot) {
					continue;
				}
				int busKw = NextNonTrivia(tokens, dotBus);
				if (busKw < 0 || tokens[busKw].type != PythonTokens.name || tokens[busKw].value != "Bus") {
					continue;
				}
				int dotBusName = NextNonTrivia(tokens, busKw);
				if (dotBusName < 0 || tokens[dotBusName].type != PythonTokens.dot) {
					// `self.Bus` used by itself (passed to a helper, etc.) —
					// nothing to validate without a concrete bus name.
					continue;
				}
				int busNameTok = NextNonTrivia(tokens, dotBusName);
				if (busNameTok < 0 || tokens[busNameTok].type != PythonTokens.name) {
					continue;
				}
				string busName = tokens[busNameTok].value;
				ControllerBus bus = controller.GetBus(busName);
				if (bus == null) {
					// Unknown bus — could be a typo or a bus the player will
					// add later.  Leave the diagnostic to runtime (returns
					// Fix32.Zero / drops the write) so a save mid-edit still
					// compiles.
					continue;
				}

				int afterBusName = NextNonTrivia(tokens, busNameTok);
				if (afterBusName < 0) {
					continue;
				}

				if (tokens[afterBusName].type == PythonTokens.dot) {
					int pinOrMethod = NextNonTrivia(tokens, afterBusName);
					if (pinOrMethod < 0 || tokens[pinOrMethod].type != PythonTokens.name) {
						continue;
					}
					string pinOrMethodName = tokens[pinOrMethod].value;
					int afterPin = NextNonTrivia(tokens, pinOrMethod);

					// Distinguish a method call (`set("pin", ...)`) from a
					// dotted pin name (`pinName = ...`).  `(` right after
					// the name = call; anything else = dotted access.
					if (afterPin >= 0 && tokens[afterPin].type == PythonTokens.lparen
						&& (READ_METHODS.Contains(pinOrMethodName) || WRITE_METHODS.Contains(pinOrMethodName)))
					{
						bool isWrite = WRITE_METHODS.Contains(pinOrMethodName);
						int pinArg = NextNonTrivia(tokens, afterPin);
						if (pinArg < 0 || tokens[pinArg].type != PythonTokens.str) {
							// First arg isn't a string literal — pin name is
							// computed.  Skip; runtime check is the fallback.
							continue;
						}
						string pinName = StripQuotes(tokens[pinArg].value);
						CheckPin(bus, busName, pinName, isWrite, tokens[pinArg]);
						continue;
					}

					// Dotted pin access — determine direction from what
					// follows the pin name.
					string pinName2 = pinOrMethodName;
					bool isWrite2 = false;
					bool isReadAndWrite = false;
					if (afterPin >= 0)
					{
						PythonTokens nextType = tokens[afterPin].type;
						if (nextType == PythonTokens.set) {
							isWrite2 = true;
						} else if (IsCompoundAssign(nextType)) {
							isWrite2 = true;
							isReadAndWrite = true;
						}
					}
					CheckPin(bus, busName, pinName2, isWrite2, tokens[pinOrMethod]);
					if (isReadAndWrite) {
						// The "+=" / "-=" / etc. case — also has to allow
						// reading the same pin.  Run the check again with
						// the opposite polarity so a single bad direction
						// surfaces a clear error message.
						CheckPin(bus, busName, pinName2, false, tokens[pinOrMethod]);
					}
				}
				else if (tokens[afterBusName].type == PythonTokens.llist)
				{
					// `self.Bus.<bus>[<idx>]` — resolve the pin index when
					// the index is an integer literal; skip otherwise.
					int idxTok = NextNonTrivia(tokens, afterBusName);
					if (idxTok < 0 || tokens[idxTok].type != PythonTokens.number) {
						continue;
					}
					if (!int.TryParse(tokens[idxTok].value, out int idx)) {
						continue;
					}
					if (idx < 0 || idx >= ControllerBus.PinCount) {
						continue;
					}
					string pinName3 = bus.PinNames[idx];
					if (string.IsNullOrEmpty(pinName3)) {
						continue;
					}
					int closeBracket = NextNonTrivia(tokens, idxTok);
					if (closeBracket < 0 || tokens[closeBracket].type != PythonTokens.rlist) {
						continue;
					}
					int afterClose = NextNonTrivia(tokens, closeBracket);
					bool isWrite3 = false;
					bool isReadAndWrite3 = false;
					if (afterClose >= 0)
					{
						PythonTokens nextType = tokens[afterClose].type;
						if (nextType == PythonTokens.set) {
							isWrite3 = true;
						} else if (IsCompoundAssign(nextType)) {
							isWrite3 = true;
							isReadAndWrite3 = true;
						}
					}
					CheckPin(bus, busName, pinName3, isWrite3, tokens[idxTok]);
					if (isReadAndWrite3) {
						CheckPin(bus, busName, pinName3, false, tokens[idxTok]);
					}
				}
			}
		}

		// Returns the index of the next non-trivia token after `i`, or -1
		// when the scan runs off the end.  Same skip set the lexer uses for
		// expression position, so comments / blank lines between dots don't
		// hide a valid bus chain.
		private static int NextNonTrivia(Token[] tokens, int i)
		{
			for (int j = i + 1; j < tokens.Length; j++)
			{
				if (!IsTrivia(tokens[j].type)) {
					return j;
				}
			}
			return -1;
		}

		// Quoted-string tokens carry the surrounding quotes in their value
		// (the tokenizer matches `"..."` / `'...'` as one chunk).  Peel a
		// single pair when present so we can compare against the bus pin
		// name as-stored.  Embedded escapes aren't unescaped — that's a
		// lexer concern at runtime; the validator only matches literal pin
		// names, and players don't usually put escapes in pin labels.
		private static string StripQuotes(string s)
		{
			if (s == null || s.Length < 2) {
				return s ?? "";
			}
			char first = s[0];
			char last = s[s.Length - 1];
			if ((first == '"' || first == '\'') && first == last) {
				return s.Substring(1, s.Length - 2);
			}
			return s;
		}

		// Compares the bus pin's configured type against what the script is
		// trying to do.  Unknown pin names are silent (matches the runtime
		// "drop write / read zero" default) so the validator doesn't refuse
		// a save while the player is still wiring up a bus.
		private static void CheckPin(ControllerBus bus, string busName, string pinName, bool isWrite, Token anchor)
		{
			int idx = bus.IndexOfPin(pinName);
			if (idx < 0) {
				return;
			}
			ControllerBus.BusPinType type = bus.PinTypes[idx];
			if (isWrite)
			{
				if (type != ControllerBus.BusPinType.Plc)
				{
					throw new PythonParseException(anchor,
						$"Bus pin '{busName}.{pinName}' is type {type} — only Plc-typed pins can be WRITTEN from a PLC script. Change the pin's type to Plc in the bus settings, or remove the write.");
				}
			}
			else
			{
				if (type == ControllerBus.BusPinType.Plc)
				{
					throw new PythonParseException(anchor,
						$"Bus pin '{busName}.{pinName}' is type Plc — Plc-typed pins are WRITE-only from a PLC script. Change the pin's type to Input / Controller / NetworkRead in the bus settings, or remove the read.");
				}
			}
		}
	}
}
