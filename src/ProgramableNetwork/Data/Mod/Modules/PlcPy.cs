using System;
using System.Collections.Generic;
using Mafi;
using Mafi.Core;
using Mafi.Core.Mods;
using Mafi.Base;
using Mafi.Collections;
using Mafi.Unity;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Localization;
using ProgramableNetwork.Data.Modules;
using ProgramableNetwork.Python;
using ProgramableNetwork.Ui;

namespace ProgramableNetwork;

// PLC-PY: Python-flavored programmable logic controller.
//
// The module ID is "PLC_PY" so future flavors (e.g. "PLC_LUA",
// "PLC_BLOCKS") can sit alongside without colliding.  Player writes a
// Python script in the `code` StringField; on every tick the action
// either reuses the cached parsed Block or — when the source hash
// changed — yields one tick of yellow LED and parses on the next.
// Compile / run errors land in StringData and the editor window picks
// them up live.  Computing cost scales with the parsed lexer-token
// count: 0.5 + 0.01 × count.  Maintenance is T3.
public class PlcPy : ModuleGroup, IModuleGroup {

	public override void RegisterData(ProtoRegistrator registrator) {
		// Empty class wrapper — lets self.X resolve C# members on
		// ModuleWrapper without NPE'ing on the @class.classContext fallback
		// in PropertyExpression.  The classContext stays empty; players write
		// the body of action() inline, no helper methods.
		Class plcClass = new Class("PLC_PY", new Type[0], new Dictionary<string, object>());
		PartialQuantity minimum = 0.5.Quantity();
		PartialQuantity perNode = 0.01.Quantity();

		// Mirror what `from Core.mafi import fix, Fix32` would inject in a .py
		// module — PLC scripts can't import (no class scope), so we pre-bind
		// the conversion helper and the Fix32 type globally so players can
		// write `fix(0)`, `Fix32.Zero`, etc. the same way module authors do.
		// Stored in lambdas because Constructor instances are stateless and
		// the context is fresh per tick.
		Func<IArgumentValue[], object> fixCtor = args => Expressions.__fix__(args[0].Value);

		// `int(value)` — value-preserving Fix32 → int (truncates toward zero
		// via Fix32.IntegerPart).  Inverse of `fix(...)` for the natural
		// reading of the number.  Mirrors the existing __int__ helper so all
		// numeric types Expressions already understands route through one
		// path: `int(fix(2.7))` → 2, `int(2.7)` → 2, `int(True)` → 1.
		Func<IArgumentValue[], object> intCtor = args => Expressions.__int__(args[0].Value);

		// `raw(value)` — Fix32 → its underlying fixed-point int (Fix32.RawValue).
		// Inverse of `hex(...)`.  Use it when a script needs to inspect or
		// store the exact bit pattern (NumberData, save round-trips) rather
		// than a value-preserving int.  Other numeric types pass through
		// their integer value so `raw(5)` doesn't blow up.
		Func<IArgumentValue[], object> rawCtor = args => Expressions.__raw__(args[0].Value);

		// `hex(value)` — int → Fix32.FromRaw, the inverse of `raw(...)`.
		// Lets a script reconstruct a Fix32 from a stored raw int (e.g. a
		// value previously squirreled away in NumberData via raw()).  Named
		// `hex` to evoke the "raw bit pattern" reading the player would see
		// when debugging a fixed-point dump.
		Func<IArgumentValue[], object> hexCtor = args => Fix32.FromRaw(Expressions.__int__(args[0].Value));

		// `range(...)` — produces a lazy RangeIterable so `for i in range(N):`
		// doesn't materialise an N-int list every tick.  Mirrors Python's
		// three forms (range(stop), range(start, stop), range(start, stop,
		// step)).  ForStatement has a fast path that recognises the type
		// and iterates with a plain int loop, avoiding both the IEnumerator
		// allocation and the per-iteration int→object boxing the IEnumerable
		// path would otherwise force.  Indexing (`range(10)[3]`) and
		// `len(range(10))` both resolve through standard fallbacks against
		// the type's int this[int] indexer + Length property.
		Func<IArgumentValue[], object> rangeCtor = args => {
			int start, stop, step;
			if (args.Length == 1) {
				start = ToIntForRange(args[0].Value);
				stop = start;
				start = 0;
				step = 1;
			} else if (args.Length == 2) {
				start = ToIntForRange(args[0].Value);
				stop = ToIntForRange(args[1].Value);
				step = 1;
			} else if (args.Length >= 3) {
				start = ToIntForRange(args[0].Value);
				stop = ToIntForRange(args[1].Value);
				step = ToIntForRange(args[2].Value);
			} else {
				throw new PythonRuntimeException("range() requires 1 to 3 arguments");
			}
			if (step == 0) {
				throw new PythonRuntimeException("range() step argument must not be zero");
			}
			return new RangeIterable(start, stop, step);
		};

		// `len(x)` — string length, collection count, dict size.  Falls
		// through to manually counting an IEnumerable for things like the
		// List<int> range() returns or any custom iterable a wrapper might
		// expose.  Player-facing error if the value isn't sized.
		Func<IArgumentValue[], object> lenCtor = args => {
			object v = args[0].Value;
			if (v is null) {
				throw new PythonRuntimeException("len(): argument is None");
			}
			if (v is string s) {
				return s.Length;
			}
			// Fast path for the lazy range — Length is O(1) and avoids
			// walking the iterator at all.  Falls before ICollection
			// because RangeIterable doesn't implement that interface.
			if (v is RangeIterable r) {
				return r.Length;
			}
			if (v is System.Collections.ICollection col) {
				return col.Count;
			}
			if (v is System.Collections.IEnumerable en) {
				int n = 0;
				foreach (object _ in en) {
					n++;
				}
				return n;
			}
			throw new PythonRuntimeException(
				"len(): not supported for " + v.GetType().Name);
		};

		registrator
			.ModuleBuilderStart("PLC_PY", "Custom: PLC (Python)", "PLC-PY")
			.SetDescription("Player-programmable logic controller — Python flavor. Write a Python script in the <b>code</b> field that reads <b>self.Input</b>, writes <b>self.Output</b>, and accesses <b>self.Field</b>/<b>self.Display</b>/<b>self.Array</b> like any other module. The script may use <b>init:</b> (runs once / on recovery) and <b>main:</b> (runs every tick) sections; top-level <b>def</b>s outside the sections are shared between them. Computing cost scales with the script size (0.5 + 0.01 per lexer token); requires T3 maintenance.")
			.AddCategory(Category.Control)
			.UnlockedBy(Ids.Research.Datacenter)
			.UseMaintenanceT3()
			.UseDynamicComputation(m => {
				int nodes = m.LexerNodeCount;
				if (nodes <= 0) {
					return minimum;
				}
				return minimum + (nodes * perNode);
			})
			.AddInput("A", "A")
			.AddInput("B", "B")
			.AddInput("C", "C")
			.AddInput("D", "D")
			.AddOutput("A", "A")
			.AddOutput("B", "B")
			.AddOutput("C", "C")
			.AddOutput("D", "D")
			// Player-extensible on both sides — extra pins default-named E, F, G, ...
			// continuing the alphabet from the static D, addable via the inspector
			// and accessible from the PLC script through self.Input["E"] /
			// self.Output["E"] (no script-side bindings change; pin lookup is by id).
			.AllowInputExtensions(8)
			.AllowOutputExtensions(8)
			.AddStringField("name", "Name", "Label shown on the module's display row.", defaultValue: "PLC-PY")
			// `code` stays a real StringField so the standard ModuleSetStringFieldCmd
			// pipeline routes saves through the same path everywhere else uses
			// (multiplayer / undo / tooltip stay consistent).  Single-line in the
			// inspector — the dedicated window is the proper editing surface.
			.AddStringField("code", "Code", "Python script body. Edit via the button below.", defaultValue: "")
			.AddCustomField("editor", "Edit code",
				(ControllerInspector inspector, UiComponent container, Module module, Action _, Reference _) => {
					container.Add(
						new ButtonText("Edit code...".ToDoLoc())
							.OnClick(() => inspector.PlcPyCodeEditorWindowController.OpenFor(module))
					);
				})
			// LED + name occupy the display row.  LED is width 1; the player-
			// editable name field gets the remaining 3 cells.  Color values are
			// chosen via the `#CRRGGBB` prefix that StatusText() parses; the
			// trailing `.` keeps the value non-empty so the LED stays "on"
			// (the empty fallback path renders red regardless of color).
			.AddDisplay("status", "Status (LED)", 1, led: true)
			.AddDisplay("label", "Module name", 3, defaultText: "PLC-PY")
			.Width(4)
			.Action(m => {
				// StringField writes through the Field indexer, which prefixes
				// keys with "field__" before storing in StringData.  Read through
				// the same indexer the inspector writes to.
				string source = m.Field["code", ""] ?? "";

				// Mirror the player-editable name into the label display every
				// tick (cheap; assignment is idempotent).
				m.Display["label"] = m.Field["name", "PLC-PY"] ?? "PLC-PY";

				// Tick-blink phase for the running LED — toggles 0/1 each
				// action invocation.  Stored in NumberData (an int dict) so it
				// persists across saves without burning a separate bit/flag.
				// Honor the player's accessibility preference: when "Disable
				// flashes" is on (Settings → Video → Accessibility), pin the
				// phase to 1 so the running LED stays solid dim green instead
				// of blinking — same gate level-crossings and weather flashes
				// use ((bool)GlobalPlayerPrefs.DisableFlashes).
				bool disableFlashes = (bool)GlobalPlayerPrefs.DisableFlashes;
				int ledPhase = m.NumberData.TryGetValue("__led_phase", out int p) ? p : 0;
				m.NumberData["__led_phase"] = disableFlashes ? 1 : ((ledPhase + 1) & 1);

				const string LED_RED       = "#CFF0000.";
				const string LED_YELLOW    = "#CFFCC00.";
				const string LED_GREEN_HI  = "#C00FF00.";
				const string LED_GREEN_LO  = "#C006400.";

				if (string.IsNullOrWhiteSpace(source)) {
					m.LexerNodeCount = 0;
					m.CompiledBlock = null;
					m.StringData["__compile_error"] = "No code";
					m.StringData.TryRemove("__run_error", out _);
					m.NumberData.TryRemove("__plc_compile_pending", out _);
					m.Display["status"] = LED_RED;
					m.SetError("No code");
					return ModuleStatus.Error;
				}

				int sourceHash = source.GetHashCode();
				bool sourceChanged = !(m.CompiledBlock is Block) || m.CompiledSourceHash != sourceHash;
				bool compilePending = m.NumberData.TryGetValue("__plc_compile_pending", out int pending) && pending != 0;

				// Step 1 — source diverged from cache: show yellow this tick,
				// flag the next tick to run the parser.  Splitting compile out
				// of the change-detect tick keeps each tick's work bounded
				// (the parser is the most expensive step) and gives the player
				// a visible "compiling" beat between edit and run.
				if (sourceChanged && !compilePending) {
					m.NumberData["__plc_compile_pending"] = 1;
					m.Display["status"] = LED_YELLOW;
					return ModuleStatus.Init;
				}

				// Step 2 — compile pending was set last tick: actually parse.
				// Stays yellow during this tick too (parse may fail; success
				// flips to green next tick when run-blink kicks in).
				//
				// The whole source is tokenised + parsed once; section
				// detection lives in the lexer (SectionStatement, emitted by
				// ParseBlock when it sees `init`/`main` + `:` at top level).
				// We then walk the parsed top-level Block and peel the init
				// and main bodies out, leaving everything else as the
				// preamble.  Doing the split inside the lexer (rather than a
				// text pre-pass) means the indent stack handles header
				// detection in the same code path as `def`/`if`/`for`, so a
				// player can't sneak past it with whitespace tricks the
				// pre-pass wouldn't notice.
				if (sourceChanged && compilePending) {
					m.NumberData.TryRemove("__plc_compile_pending", out _);
					try {
						Token[] tokens = Tokenizer.ParseString(source, "PLC_PY");
						Block parsedRoot = Lexer.Parse(tokens);

						Block initBlock = null;
						Block mainBlock = null;
						foreach (IStatement stmt in parsedRoot.statements)
						{
							if (stmt is SectionStatement section)
							{
								if (section.Name == "init") {
									initBlock = section.Body;
								} else if (section.Name == "main") {
									mainBlock = section.Body;
								}
							}
						}

						bool hasSections = initBlock != null || mainBlock != null;
						if (!hasSections)
						{
							// Legacy single-block script — no init/main markers.
							// The whole top-level block is the main body and
							// there's no preamble to register.
							m.CompiledPreambleBlock = null;
							m.CompiledInitBlock = null;
							m.CompiledBlock = parsedRoot;
						}
						else
						{
							// Section layout — the parsed root carries the
							// preamble (functions, classes, top-level vars)
							// alongside the SectionStatements.  RegisterPreamble
							// skips the SectionStatements when iterating so
							// the player's helper defs are bound while the
							// sections themselves only run via their dedicated
							// init / main dispatch below.
							m.CompiledPreambleBlock = parsedRoot;
							m.CompiledInitBlock = initBlock;
							m.CompiledBlock = mainBlock;
						}
						m.CompiledSourceHash = sourceHash;
						m.LexerNodeCount = tokens.Length;
						m.StringData.TryRemove("__compile_error", out _);

						// Source recompiled — the run state has to start fresh
						// because new init: code may seed different values.
						// Clearing PlcContext here forces both the preamble
						// register-pass and the init-dispatch below to start
						// against a clean slate.
						m.PlcContext = new Dict<string, object>();
					} catch (Exception parseError) {
						m.CompiledPreambleBlock = null;
						m.CompiledInitBlock = null;
						m.CompiledBlock = null;
						m.LexerNodeCount = 0;
						m.StringData["__compile_error"] = parseError.Message;
						m.Display["status"] = LED_RED;
						m.SetError("Compile: " + parseError.Message);
						return ModuleStatus.Error;
					}
				}

				Block mainBody = m.CompiledBlock as Block;
				Block initBody = m.CompiledInitBlock as Block;
				Block preambleBody = m.CompiledPreambleBlock as Block;
				if (mainBody == null && initBody == null && preambleBody == null) {
					// Defensive — should be unreachable since the source-change
					// branch above either populates a block or returns Error.
					m.Display["status"] = LED_RED;
					m.SetError("No compiled block");
					return ModuleStatus.Error;
				}

				// Make sure the persistent scratch dict exists so the player
				// can store a name in init: and read it in run:.  The Module
				// constructor / deserialiser leaves it null for non-PLC
				// modules; lazy-create here so non-PLC paths pay nothing.
				if (m.PlcContext == null) {
					m.PlcContext = new Dict<string, object>();
				}

				// Init dispatch: re-run init: when the source recompiled (the
				// PlcContext got cleared above) OR when last tick ended on a
				// non-Running status.  The Status check makes init:/main: a
				// poor-man's state machine: any time the main block returns
				// Paused/Error, init: gets to seed the recovery state on the
				// next tick before main: tries again.
				//
				// Preamble re-runs whenever its functions are missing from
				// PlcContext.  Two cases this covers:
				//   1. Fresh compile cleared PlcContext above — no methods yet.
				//   2. Save → load — Methods aren't serialisable, so they get
				//      dropped on save while primitive player vars persist.
				//      The function-missing check brings the helpers back
				//      without resetting init's accumulated state.
				bool needPreamble = preambleBody != null
					&& (m.PlcContext.Count == 0
						|| PreambleDefsMissing(preambleBody, m.PlcContext));
				bool needInit = initBody != null
					&& (m.PlcContext.Count == 0 || m.Status != ModuleStatus.Running);

				// Time the whole preamble+init+main sweep so the editor's
				// stats label can show the cost — Stopwatch.GetTimestamp is
				// a single QPC call, much cheaper than a heap-allocated
				// Stopwatch.
				long startTicks = System.Diagnostics.Stopwatch.GetTimestamp();

				try {
					if (needPreamble) {
						RegisterPreamble(preambleBody, m, plcClass, fixCtor, intCtor, rawCtor, hexCtor, rangeCtor, lenCtor);
					}
					if (needInit) {
						RunBlock(initBody, m, plcClass, fixCtor, intCtor, rawCtor, hexCtor, rangeCtor, lenCtor);
					}
					if (mainBody != null) {
						RunBlock(mainBody, m, plcClass, fixCtor, intCtor, rawCtor, hexCtor, rangeCtor, lenCtor);
					}
					m.StringData.TryRemove("__run_error", out _);
					m.Display["status"] = ledPhase == 0 ? LED_GREEN_HI : LED_GREEN_LO;
					RecordTiming(m, startTicks);
					return ModuleStatus.Running;
				} catch (ReturnException returnData) {
					RecordTiming(m, startTicks);
					switch (returnData.Value) {
					case string returnMessage:
						m.Display["status"] = LED_RED;
						m.SetError(returnMessage);
						// Keep status Running so the next tick re-runs run:
						// without re-running init: — string returns are a
						// "soft error" surface for the player and shouldn't
						// reset the script's state.
						return ModuleStatus.Running;
					case ModuleStatus status:
						switch (status) {
						case ModuleStatus.Running:
							m.Display["status"] = ledPhase == 0 ? LED_GREEN_HI : LED_GREEN_LO;
							break;
						case ModuleStatus.Paused:
							m.Display["status"] = LED_YELLOW;
							break;
						case ModuleStatus.Error:
							m.Display["status"] = LED_RED;
							m.SetError("Unexpected return: " + status);
							break;
						}
						return status;
					default:
						m.Display["status"] = ledPhase == 0 ? LED_GREEN_HI : LED_GREEN_LO;
						return ModuleStatus.Running;
					}
				} catch (Exception runError) {
					RecordTiming(m, startTicks);
					m.StringData["__run_error"] = runError.Message;
					m.Display["status"] = LED_RED;
					m.SetError("Run: " + runError.Message);
					return ModuleStatus.Error;
				}
			})
			.AddControllerDevice()
			.BuildAndAdd();
	}

	// Keys the runtime installs on the per-tick context dict before each
	// init/main dispatch.  Used to filter the dict on the way back out so
	// only player-defined variables survive into Module.PlcContext.  Any
	// new system binding added to RunBlock MUST also be added here, or the
	// next tick will read a stale system value as if the player had
	// shadowed it.
	private static readonly HashSet<string> SYSTEM_CONTEXT_KEYS = new HashSet<string>
	{
		"self", "Fix32", "fix", "int", "raw", "hex", "ModuleStatus", "range", "len",
	};

	// True when at least one preamble-level definition (function OR class)
	// is missing from PlcContext, or has been overwritten by a value of
	// the wrong runtime kind.  Used to decide whether to re-run the
	// preamble's register pass.  Fresh compile (PlcContext cleared) and
	// save→load (Method / Class instances are dropped by the serialiser)
	// both surface as "the bindings aren't there" without us needing to
	// track an explicit "preamble-was-run" flag.
	//
	// Functions and classes are checked at the same level — both come from
	// top-level `def` / `class` statements in the preamble and both lose
	// their runtime instances on save, so re-running the preamble has to
	// restore them together.
	private static bool PreambleDefsMissing(Block preamble, Dict<string, object> ctx)
	{
		if (ctx == null || ctx.Count == 0) {
			return preamble.functions.Count + preamble.classes.Count > 0;
		}
		foreach (string name in preamble.functions.Keys)
		{
			if (!ctx.TryGetValue(name, out object existingFn) || !(existingFn is Method)) {
				return true;
			}
		}
		foreach (string name in preamble.classes.Keys)
		{
			if (!ctx.TryGetValue(name, out object existingCls) || !(existingCls is Class)) {
				return true;
			}
		}
		return false;
	}

	// Compiles the preamble's top-level definitions into PlcContext so the
	// init/main sections can call them.  `def foo(): ...` becomes a Method
	// keyed by name; `class Foo: ...` becomes a Class via the existing
	// ClasssStatement.Execute path; any other top-level statement (e.g. an
	// assignment) runs against the PlcContext-backed dict so the resulting
	// value persists into init/main.
	//
	// We DON'T iterate `body.statements` and call .Execute uniformly because
	// FunctionStatement.Execute runs the function's body — fine for class
	// methods (where the lambda calls it), wrong for a top-level def that's
	// supposed to *register* a callable.  Special-casing FunctionStatement
	// here keeps the rest of the runtime untouched.
	private static void RegisterPreamble(
		Block body,
		Module m,
		Class plcClass,
		Func<IArgumentValue[], object> fixCtor,
		Func<IArgumentValue[], object> intCtor,
		Func<IArgumentValue[], object> rawCtor,
		Func<IArgumentValue[], object> hexCtor,
		Func<IArgumentValue[], object> rangeCtor,
		Func<IArgumentValue[], object> lenCtor)
	{
		// Same shape as RunBlock — the preamble can read self / fix / etc.
		// and write to PlcContext.  Use a dict that mirrors PlcContext so
		// any top-level assignment lands directly in the persistent store.
		Dictionary<string, object> context = new Dictionary<string, object>
		{
			["self"] = new ModuleWrapper(m, plcClass),
			["Fix32"] = typeof(Fix32),
			["fix"] = new Constructor(fixCtor, ["value"]),
			["int"] = new Constructor(intCtor, ["value"]),
			["raw"] = new Constructor(rawCtor, ["value"]),
			["hex"] = new Constructor(hexCtor, ["value"]),
			["ModuleStatus"] = typeof(ModuleStatus),
			["range"] = new Constructor(rangeCtor, ["start", "stop", "step"]),
			["len"] = new Constructor(lenCtor, ["value"]),
		};

		// Same live-runtime publish RunBlock does — preamble can have
		// top-level statements (constants, class defs) that themselves
		// call helpers defined just above, and those helpers should
		// resolve names against the *current* preamble context, not the
		// captured snapshot inside their own closure.
		IDictionary<string, object> previousRuntime = m.CurrentPlcRuntime;
		m.CurrentPlcRuntime = context;
		try
		{
			foreach (IStatement stmt in body.statements)
			{
				// Skip SectionStatement — the lexer emits one of these per
				// `init:` / `main:` block.  They live in the preamble's
				// statement list because that's where the lexer parsed them,
				// but PlcPy.Action handles their bodies separately via the
				// dedicated init / main dispatch above.  Calling .Execute on
				// one would throw the defensive guard in SectionStatement.
				if (stmt is SectionStatement) {
					continue;
				}
				if (stmt is FunctionStatement fn)
				{
				// Bind the def as a Method that routes calls back through
				// f.Execute against a per-call ChildContext — same shape
				// ClasssStatement uses for class methods.  Capturing `fn`
				// in the lambda lets the function close over the preamble
				// scope (so a helper can read top-level constants the
				// player defined alongside it).
				FunctionStatement local = fn;
				List<string> declaredArgs = local.Arguments;
				// Pythonic shorthand: if the player declared `def helper(self, ...)`
				// at the preamble level, treat `self` like a class method's
				// implicit receiver — bind it from the LIVE runtime scope
				// (`m.CurrentPlcRuntime["self"]`) and shift the player's
				// positional args accordingly.  Lets a player carry the same
				// `def foo(self, x):` shape they'd use inside a class without
				// us forcing them to call `foo(self, 5)` at every site.  The
				// closure fallback (reading bare `self` inside the body) still
				// works for `def helper(x):`-style functions that omit it.
				bool hasSelfParam = declaredArgs.Count > 0 && declaredArgs[0] == "self";
				context[local.Name] = new Method((IArgumentValue[] args) =>
				{
					// Live runtime context — set by RunBlock at the start of
					// each preamble / init / main dispatch to the dict the
					// statements are executing against.  The method's
					// ChildContext chains to it (instead of the stale
					// preamble snapshot) so a helper called from main: can
					// see vars set in init:, mutations made earlier in main:,
					// and the per-tick `self` wrapper.  Falls back to the
					// preamble-time `context` when no tick is active (e.g.
					// the registration pass itself).
					IDictionary<string, object> liveParent = m.CurrentPlcRuntime ?? context;
					IDictionary<string, object> callContext = new ChildContext(liveParent);
					int paramOffset = 0;
					int argOffset = 0;
					if (hasSelfParam)
					{
						// Bind self from the live runtime — that's the per-tick
						// ModuleWrapper RunBlock built, so the helper sees the
						// same `self` instance init:/main: are working with.
						callContext["self"] = liveParent.TryGetValue("self", out object liveSelf) ? liveSelf : null;
						paramOffset = 1;
						// We DON'T increment argOffset — the player's call
						// site `helper(5)` provides positional args starting
						// at the FIRST non-self parameter, so args[0] maps
						// to declaredArgs[1].
					}
					for (int i = 0; i + paramOffset < declaredArgs.Count && i + argOffset < args.Length; i++)
					{
						callContext[declaredArgs[i + paramOffset]] = args[i + argOffset].Value;
					}
					local.Execute(callContext);
					// Return value lands in __return__ if the body used `return X`,
					// or null when the body fell off the end.  ReturnException is
					// caught inside Method.Invoke so we don't double-handle it.
					return callContext.TryGetValue("__return__", out object r) ? r : null;
				}, declaredArgs.ToArray());
			}
			else
			{
				stmt.Execute(context);
			}
			}
		}
		finally
		{
			m.CurrentPlcRuntime = previousRuntime;
		}

		// Sweep everything (defs included) into PlcContext so init/main pick
		// it up via the persistent-vars overlay.  System keys are filtered
		// out so the next tick's RunBlock can install fresh ones without
		// colliding with stale snapshots.
		Dict<string, object> next = new Dict<string, object>();
		foreach (KeyValuePair<string, object> kv in context)
		{
			if (kv.Key == null || SYSTEM_CONTEXT_KEYS.Contains(kv.Key)) {
				continue;
			}
			next[kv.Key] = kv.Value;
		}
		m.PlcContext = next;
	}

	// Builds a fresh per-tick context (system bindings + persisted player
	// vars + the budget), runs every statement in `body`, then sweeps any
	// new/mutated player keys back into Module.PlcContext.  Shared between
	// the init and run dispatch paths so both halves see the same scope
	// and the same set of helpers (a function defined in init: stays
	// callable from run: because we pull saved Constructor refs through —
	// but the serialiser drops them on save, so player-authored functions
	// only survive within a session).
	private static void RunBlock(
		Block body,
		Module m,
		Class plcClass,
		Func<IArgumentValue[], object> fixCtor,
		Func<IArgumentValue[], object> intCtor,
		Func<IArgumentValue[], object> rawCtor,
		Func<IArgumentValue[], object> hexCtor,
		Func<IArgumentValue[], object> rangeCtor,
		Func<IArgumentValue[], object> lenCtor)
	{
		Dictionary<string, object> context = new Dictionary<string, object>
		{
			["self"] = new ModuleWrapper(m, plcClass),
			["Fix32"] = typeof(Fix32),
			["fix"] = new Constructor(fixCtor, ["value"]),
			["int"] = new Constructor(intCtor, ["value"]),
			["raw"] = new Constructor(rawCtor, ["value"]),
			["hex"] = new Constructor(hexCtor, ["value"]),
			["ModuleStatus"] = typeof(ModuleStatus),
			["range"] = new Constructor(rangeCtor, ["start", "stop", "step"]),
			["len"] = new Constructor(lenCtor, ["value"]),
		};
		// Pre-load any player vars from the previous tick (or from init's
		// pass when this is the first run after init).  Done after the
		// system bindings so a player var named e.g. "self" cannot shadow
		// the wrapper — the assignment in the dict initializer silently
		// overrides via the indexer below.
		foreach (KeyValuePair<string, object> kv in m.PlcContext)
		{
			context[kv.Key] = kv.Value;
		}

		// Publish the per-tick context as the live PLC runtime so methods
		// registered by the preamble see THIS tick's `self` + player vars,
		// not the stale snapshot they captured at registration time.
		// Cleared in finally so a method called outside an active tick
		// (which shouldn't happen, but defensive) falls back to its
		// preamble closure parent.
		IDictionary<string, object> previousRuntime = m.CurrentPlcRuntime;
		m.CurrentPlcRuntime = context;
		try
		{
			foreach (IStatement stmt in body.statements)
			{
				stmt.Execute(context);
			}
		}
		finally
		{
			m.CurrentPlcRuntime = previousRuntime;
		}

		// Sweep player vars back to PlcContext so they survive into the
		// next tick (and across saves, for whitelisted types).  Build a
		// fresh dict instead of mutating in place — a player var that
		// was deleted via `del x` won't reappear because we're rebuilding
		// from scratch.
		Dict<string, object> next = new Dict<string, object>();
		foreach (KeyValuePair<string, object> kv in context)
		{
			if (kv.Key == null || SYSTEM_CONTEXT_KEYS.Contains(kv.Key)) {
				continue;
			}
			next[kv.Key] = kv.Value;
		}
		m.PlcContext = next;
	}

	// Stores the elapsed wall-clock cost of the last tick in NumberData so
	// the editor's stats label can render the script's actual µs cost.
	// Persisted via the existing DataFlags.NumberData bit — the int fits
	// in the dict without needing a typed field of its own, and the
	// editor reads it back out the same way it reads the player's own
	// NumberData entries.  Long math through the divide so the conversion
	// stays exact for typical script costs (well below int.MaxValue µs).
	private static void RecordTiming(Module m, long startTicks)
	{
		long elapsed = System.Diagnostics.Stopwatch.GetTimestamp() - startTicks;
		long us = elapsed * 1_000_000L / System.Diagnostics.Stopwatch.Frequency;
		m.NumberData["__last_us"] = us > int.MaxValue ? int.MaxValue : (int)us;
	}

	// Coerces a player-supplied range() argument to int.  Goes through
	// Expressions.__fix__ first so any numeric type Expressions already
	// understands (int, float, Fix32) routes through the same conversion
	// path; then rounds to the nearest int.  Anything that __fix__ can't
	// convert raises a Python-flavored runtime error in the caller.
	private static int ToIntForRange(object value) {
		if (value is int i) {
			return i;
		}
		if (value is null) {
			throw new PythonRuntimeException("range(): None is not a valid argument");
		}
		try {
			return Expressions.__fix__(value).ToIntRounded();
		} catch (Exception convertError) {
			throw new PythonRuntimeException(
				"range(): cannot convert " + value.GetType().Name + " to int",
				convertError);
		}
	}
}
