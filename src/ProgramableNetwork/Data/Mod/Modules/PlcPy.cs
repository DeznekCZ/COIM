using System;
using System.Collections.Generic;
using Mafi;
using Mafi.Core;
using Mafi.Core.Mods;
using Mafi.Base;
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

		// `range(...)` — produces a List<int> the for-statement can iterate.
		// Mirrors Python's three forms (range(stop), range(start, stop),
		// range(start, stop, step)).  Returns int (not Fix32) so loop
		// indices feed directly into Module.Array.get(i, ...) etc; if a
		// player needs Fix32 they can wrap with fix(i) inside the body.
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
			List<int> values = new List<int>();
			if (step > 0) {
				for (int i = start; i < stop; i += step) {
					values.Add(i);
				}
			} else {
				for (int i = start; i > stop; i += step) {
					values.Add(i);
				}
			}
			return values;
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
			.SetDescription("Player-programmable logic controller — Python flavor. Write a Python script in the <b>code</b> field that reads <b>self.Input</b>, writes <b>self.Output</b>, and accesses <b>self.Field</b>/<b>self.Display</b>/<b>self.Array</b> like any other module. Computing cost scales with the script size (0.5 + 0.01 per lexer token); requires T3 maintenance.")
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
				if (sourceChanged && compilePending) {
					m.NumberData.TryRemove("__plc_compile_pending", out _);
					try {
						Token[] tokens = Tokenizer.ParseString(source, "PLC_PY");
						Block parsedBlock = Lexer.Parse(tokens);
						m.CompiledBlock = parsedBlock;
						m.CompiledSourceHash = sourceHash;
						m.LexerNodeCount = tokens.Length;
						m.StringData.TryRemove("__compile_error", out _);
					} catch (Exception parseError) {
						m.CompiledBlock = null;
						m.LexerNodeCount = 0;
						m.StringData["__compile_error"] = parseError.Message;
						m.Display["status"] = LED_RED;
						m.SetError("Compile: " + parseError.Message);
						return ModuleStatus.Error;
					}
				}

				Block block = m.CompiledBlock as Block;
				if (block == null) {
					// Defensive — should be unreachable since the source-change
					// branch above either populates CompiledBlock or returns
					// Error.  Surface as red rather than silently no-op.
					m.Display["status"] = LED_RED;
					m.SetError("No compiled block");
					return ModuleStatus.Error;
				}

				try {
					Dictionary<string, object> context = new Dictionary<string, object> {
						["self"] = new ModuleWrapper(m, plcClass),
						["Fix32"] = typeof(Fix32),
						["fix"] = new Constructor(fixCtor, new[] { "value" }),
						// Loop helpers — variadic args, so the parameter-name list
						// is set to the maximum the player can pass; Constructor
						// hands extra slots through args[i] indexing in the lambda.
						["range"] = new Constructor(rangeCtor, new[] { "start", "stop", "step" }),
						["len"] = new Constructor(lenCtor, new[] { "value" }),
					};
					foreach (IStatement stmt in block.statements) {
						stmt.Execute(context);
					}
					m.StringData.TryRemove("__run_error", out _);
					m.Display["status"] = ledPhase == 0 ? LED_GREEN_HI : LED_GREEN_LO;
					return ModuleStatus.Running;
				} catch (Exception runError) {
					m.StringData["__run_error"] = runError.Message;
					m.Display["status"] = LED_RED;
					m.SetError("Run: " + runError.Message);
					return ModuleStatus.Error;
				}
			})
			.AddControllerDevice()
			.BuildAndAdd();
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
