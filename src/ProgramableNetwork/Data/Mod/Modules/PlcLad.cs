using System;
using System.Collections.Generic;
using Mafi;
using Mafi.Base;
using Mafi.Core;
using Mafi.Core.Mods;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.Research;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using ProgramableNetwork.Data.Modules;
using ProgramableNetwork.Ladder;
using ProgramableNetwork.Ui;

namespace ProgramableNetwork;

// PLC-LAD: Siemens-flavored ladder diagram, the second PLC flavor alongside
// PLC-PY.
//
// Three prototypes rather than one configurable module.  A tier is a research
// unlock, so an unresearched tier is simply absent from the module picker and
// its instructions do not exist for the player yet — the ladder reveals itself
// as the factory grows instead of arriving fully formed.  All three share a
// swap group, so upgrading a placed module keeps its cables and its program.
//
//   Basic     ProgramableNetwork   2 networks, 5 channels, own pins only
//   Extended  BasicComputing       + memory bits, edge detection
//   Full      Datacenter           + timers, counters, compares, bus pins
//
// The program is stored as statement-list text in the `code` field, exactly
// like PLC-PY's script.  Ladder and statement list are two views of the same
// thing (as they are in TIA Portal), so the text form costs no new
// serialization and rides the existing ModuleSetStringFieldCmd path for
// blueprints, copy-paste, undo and multiplayer.
public class PlcLad : ModuleGroup, IModuleGroup {

	// Prototype ids, shared between registration and TierOf so the two can't
	// drift apart.
	public const string ID_BASIC = "PLC_LAD";
	public const string ID_EXTENDED = "PLC_LAD_EXT";
	public const string ID_FULL = "PLC_LAD_FULL";

	// Module-local memory bank, all 32 bits packed into one NumberData entry.
	private const string MEMORY_KEY = "__lad_mem";
	// Timer / counter / edge state, one entry per slot plus a count so a
	// shrinking program doesn't leave orphans behind.
	private const string STATE_PREFIX = "__lad_s";
	private const string STATE_COUNT_KEY = "__lad_sn";
	private const string COMPILE_PENDING_KEY = "__lad_pending";
	private const string LED_PHASE_KEY = "__lad_led";
	private const string COMPILE_ERROR_KEY = "__compile_error";
	private const string RUN_ERROR_KEY = "__run_error";

	private const string LED_RED = "#CFF0000.";
	private const string LED_YELLOW = "#CFFCC00.";
	private const string LED_GREEN_HI = "#C00FF00.";
	private const string LED_GREEN_LO = "#C006400.";

	// A working start/stop latch that fits inside the Basic tier — it seals in
	// through its own output rather than a memory bit, which is the trick that
	// makes a 5-channel ladder useful before the player has researched
	// anything.
	private const string STARTER_PROGRAM =
		"NETWORK 1  // start / stop latch\n"
		+ "  A(\n"
		+ "  O    I.A\n"
		+ "  O    Q.A\n"
		+ "  )\n"
		+ "  AN   I.B\n"
		+ "  =    Q.A";

	public override void RegisterData(ProtoRegistrator registrator) {
		ModuleSwapGroupBuilder ladderModules =
			registrator.SwapGroupStart(SwapGroups.Ladder, Category.Control.Name);

		// Basic arrives with Electronics II — long before the computing chain
		// exists — and costs no computing at all.  That is what makes it worth
		// building: it is the only PLC in the mod a player can run before a
		// datacenter, and its narrowness (two networks, five channels, no
		// memory) is the price.
		registerTier(registrator, ladderModules, LadderTier.Basic,
			id: ID_BASIC,
			title: "Custom: Ladder (LAD)",
			shortName: "PLC-LAD",
			research: Ids.Research.PolySiliconProduction,
			maintenance: Ids.Products.MaintenanceT1,
			baseComputing: 0);

		registerTier(registrator, ladderModules, LadderTier.Extended,
			id: ID_EXTENDED,
			title: "Custom: Ladder (LAD, extended)",
			shortName: "PLC-LAD+",
			research: Ids.Research.BasicComputing,
			maintenance: Ids.Products.MaintenanceT2,
			baseComputing: 0.5);

		registerTier(registrator, ladderModules, LadderTier.Full,
			id: ID_FULL,
			title: "Custom: Ladder (LAD, full)",
			shortName: "PLC-LAD*",
			research: Ids.Research.Datacenter,
			maintenance: Ids.Products.MaintenanceT3,
			baseComputing: 1.0);
	}

	private void registerTier(
		ProtoRegistrator registrator,
		ModuleSwapGroupBuilder swapGroup,
		LadderTier tier,
		string id,
		string title,
		string shortName,
		ResearchNodeProto.ID research,
		VirtualProductProto.ID maintenance,
		double baseComputing) {

		LadderLimits limits = LadderLimits.For(tier);

		var builder = registrator
			.ModuleBuilderStart(id, title, shortName)
			.SetDescription(describe(tier, limits, baseComputing))
			.AddCategory(Category.Control)
			.UnlockedBy(research)
			.UseMaintenance(maintenance, 1);

		// Basic charges maintenance but no computing — skipping the call
		// entirely (rather than passing zero) keeps it in the same shape as the
		// mod's other computing-free modules.
		if (baseComputing > 0) {
			PartialQuantity minimum = baseComputing.Quantity();
			PartialQuantity perElement = 0.02.Quantity();
			// Cost tracks the program the player actually wrote, so a sparse
			// ladder is cheap and a dense one pays for itself.  ElementCount is
			// cached in LexerNodeCount, which persists — the controller asks
			// for this outside the tick too.
			builder = builder.UseDynamicComputation(m => {
				int elements = m.LexerNodeCount;
				if (elements <= 0) {
					return minimum;
				}
				return minimum + (elements * perElement);
			});
		}

		builder
			.AddInput("A", "A")
			.AddInput("B", "B")
			.AddInput("C", "C")
			.AddInput("D", "D")
			.AddOutput("A", "A")
			.AddOutput("B", "B")
			.AddOutput("C", "C")
			.AddOutput("D", "D")
			// Extra pins continue the alphabet and are addressable straight
			// away — `I.E` resolves by pin id, so nothing in the program needs
			// to know whether a pin is static or an extension.
			.AllowInputExtensions(8)
			.AllowOutputExtensions(8)
			.AddStringField("name", "Name", "Label shown on the module's display row.", defaultValue: shortName)
			// No angle brackets anywhere in player-facing text — Unity TMP parses
			// rich-text tags here and does NOT decode HTML entities, so both a
			// raw '<n>' and an escaped '&lt;n&gt;' come out wrong (the latter as
			// the four literal characters).  Concrete examples instead of
			// placeholders.  Same reason PlcPySyntax wraps script text in
			// <noparse>.
			.AddStringField("code", "Program", "Ladder program in statement-list form. One instruction per line, with a 'NETWORK 1' header before each rung.", defaultValue: STARTER_PROGRAM)
			// The inline field stays the storage; the two windows are where the
			// work happens.  Both are offered here so the player can go
			// straight to whichever view they think in — and since the windows
			// live in different controller groups, opening one doesn't close
			// the other.
			.AddCustomField("editor", "Edit ladder",
				(ControllerInspector inspector, UiComponent container, Module module, Action _, Reference _) => {
					container.Add(
						new ButtonText("Ladder diagram...".ToDoLoc())
							.OnClick(() => inspector.LadderEditorWindowController.OpenFor(module))
					);
				})
			.AddCustomField("editor_code", "Edit program text",
				(ControllerInspector inspector, UiComponent container, Module module, Action _, Reference _) => {
					container.Add(
						new ButtonText("Statement list...".ToDoLoc())
							.OnClick(() => inspector.LadderCodeWindowController.OpenFor(module))
					);
				})
			.AddDisplay("status", "Status (LED)", 1, led: true)
			.AddDisplay("label", "Module name", 3, defaultText: shortName)
			.Width(4)
			.Action(m => execute(m, tier, shortName))
			.AddControllerDevice()
			.BuildAndAdd()
			// Every tier has the same pin layout, so an upgrade never strands
			// a cable; PreservesExtensions still guards the downgrade path.
			.EnlistSwapable(swapGroup, ModuleSwapGroup.PreservesExtensions);
	}

	// Which tier a placed module is.  The prototype IS the tier here, so its id
	// is the authoritative source — nothing needs to be stored per instance,
	// and a swap changes the answer for free.
	public static LadderTier TierOf(Module module) {
		string id = module?.Prototype?.Id.Value;
		if (id == null) {
			return LadderTier.Basic;
		}
		if (id == ID_FULL.ModuleId()) {
			return LadderTier.Full;
		}
		if (id == ID_EXTENDED.ModuleId()) {
			return LadderTier.Extended;
		}
		return LadderTier.Basic;
	}

	private static string describe(LadderTier tier, LadderLimits limits, double baseComputing) {
		string features = "contacts (<b>A</b>, <b>AN</b>, <b>O</b>, <b>ON</b>), branches (<b>A(</b> … <b>)</b>), "
			+ "inversion (<b>NOT</b>) and coils (<b>=</b>, <b>=N</b>, <b>S</b>, <b>R</b>)";
		if (limits.AllowEdges) {
			features += ", memory bits <b>M0</b>–<b>M31</b>, edge detection (<b>FP</b>, <b>FN</b>) "
				+ "and a toggle coil (<b>T</b>)";
		}
		if (limits.AllowBoxes) {
			features += ", timers (<b>TON</b>, <b>TOF</b>, <b>TP</b>), a counter (<b>CTU</b>), "
				+ "compares (<b>CMP</b>) and bus pins (<b>B.bus.pin</b>)";
		}

		string upgrade = tier >= LadderTier.Full
			? ""
			: $" Swap it for the {LadderLimits.NameOf(tier + 1)} ladder module to go further.";

		string cost = baseComputing > 0
			? " Needs computing, scaled to the size of the program."
			: " Needs <b>no computing</b> — the only PLC that runs without a datacenter.";

		return $"Player-programmable logic controller — Siemens ladder flavor ({LadderLimits.NameOf(tier)}). "
			+ $"Write a ladder in the <b>Program</b> field: up to <b>{limits.MaxNetworks} networks</b> of "
			+ $"<b>{limits.MaxChannels} channels</b> each, using {features}. "
			+ "A branch may carry as many parallel legs as you have channels for; what is rationed is "
			+ $"nesting one branch inside another, <b>{limits.MaxNesting} deep</b> per network. "
			+ "Operands are the module's own pins, <b>I.A</b> to <b>I.D</b> and <b>Q.A</b> to <b>Q.D</b>, plus any extension pins you add. "
			+ "Inputs are latched at the start of each scan and outputs written at the end, so rung order behaves like a real PLC."
			+ cost
			+ upgrade;
	}

	// One scan per tick.  Mirrors PlcPy's compile-cache shape: a source-hash
	// check, a yellow "compiling" beat, then the cached machine runs until the
	// player edits the text again.
	private static ModuleStatus execute(Module m, LadderTier tier, string shortName) {
		string source = m.Field["code", ""] ?? "";
		m.Display["label"] = m.Field["name", shortName] ?? shortName;

		// Honor the accessibility preference the rest of the mod respects —
		// pinned phase means a solid dim green instead of a blink.
		bool disableFlashes = (bool)GlobalPlayerPrefs.DisableFlashes;
		int ledPhase = m.NumberData.TryGetValue(LED_PHASE_KEY, out int phase) ? phase : 0;
		m.NumberData[LED_PHASE_KEY] = disableFlashes ? 1 : ((ledPhase + 1) & 1);

		if (string.IsNullOrWhiteSpace(source)) {
			m.LexerNodeCount = 0;
			m.CompiledBlock = null;
			m.StringData[COMPILE_ERROR_KEY] = "No program";
			m.StringData.TryRemove(RUN_ERROR_KEY, out _);
			m.NumberData.TryRemove(COMPILE_PENDING_KEY, out _);
			m.Display["status"] = LED_RED;
			m.SetError("No program");
			return ModuleStatus.Error;
		}

		int sourceHash = source.GetHashCode();
		bool sourceChanged = !(m.CompiledBlock is LadderMachine) || m.CompiledSourceHash != sourceHash;
		bool compilePending = m.NumberData.TryGetValue(COMPILE_PENDING_KEY, out int pending) && pending != 0;

		// Split change-detect from compile so each tick's work stays bounded
		// and the player gets a visible beat between editing and running.
		if (sourceChanged && !compilePending) {
			m.NumberData[COMPILE_PENDING_KEY] = 1;
			m.Display["status"] = LED_YELLOW;
			return ModuleStatus.Init;
		}

		if (sourceChanged && compilePending) {
			m.NumberData.TryRemove(COMPILE_PENDING_KEY, out _);
			try {
				LadderProgram program = LadderParser.Parse(source);
				// Tier gate first: the message names the module to swap to,
				// which is more useful than a bus-direction complaint about an
				// instruction the player cannot run anyway.
				LadderValidator.Enforce(program, tier);
				validateBusDirections(program, m.Controller);

				m.CompiledBlock = new LadderMachine(program, readState(m));
				m.CompiledSourceHash = sourceHash;
				m.LexerNodeCount = program.ElementCount;
				m.StringData.TryRemove(COMPILE_ERROR_KEY, out _);
			} catch (Exception compileError) {
				m.CompiledBlock = null;
				m.LexerNodeCount = 0;
				m.StringData[COMPILE_ERROR_KEY] = compileError.Message;
				m.Display["status"] = LED_RED;
				m.SetError("Compile: " + noparse(compileError.Message));
				return ModuleStatus.Error;
			}
		}

		if (!(m.CompiledBlock is LadderMachine machine)) {
			// Defensive — the branch above either caches a machine or returns.
			m.Display["status"] = LED_RED;
			m.SetError("No compiled program");
			return ModuleStatus.Error;
		}

		try {
			machine.Scan(new ModuleLadderIo(m));
			writeState(m, machine.State);
			m.StringData.TryRemove(RUN_ERROR_KEY, out _);
			m.Display["status"] = ledPhase == 0 ? LED_GREEN_HI : LED_GREEN_LO;
			return ModuleStatus.Running;
		} catch (Exception runError) {
			m.StringData[RUN_ERROR_KEY] = runError.Message;
			m.Display["status"] = LED_RED;
			m.SetError("Run: " + noparse(runError.Message));
			return ModuleStatus.Error;
		}
	}

	// Compile and run messages quote the player's own operands and the compare
	// symbols (&gt;, &lt;, &lt;=), which Unity TMP would try to read as rich-text tag
	// boundaries.  A noparse block disables tag parsing for the span — the same
	// trick PlcPySyntax uses when painting script text, and the reason we don't
	// reach for HTML entities (TMP never decodes those back).
	private static string noparse(string text) {
		return "<noparse>" + (text ?? "") + "</noparse>";
	}

	// Bus pins carry a direction, and a ladder has to respect it the same way
	// a PLC-PY script does (see PlcBusValidator) — otherwise the same bus
	// behaves differently depending on which flavor is talking to it.  Coils
	// write; everything else reads.
	private static void validateBusDirections(LadderProgram program, Controller controller) {
		if (controller == null) {
			return;
		}

		foreach (LadderNetwork network in program.Networks) {
			foreach (LadderInstruction instruction in network.Code) {
				// Asks LadderInstruction rather than listing the coil opcodes
				// here: a new coil type added to the enum has to be caught by
				// this check, and a local copy of the list would silently miss it.
				bool isCoil = LadderInstruction.IsCoil(instruction.Op);

				checkBusPin(controller, instruction.Operand, isCoil, instruction.Line);
				checkBusPin(controller, instruction.Right, false, instruction.Line);
				checkBusPin(controller, instruction.Reset, false, instruction.Line);
			}
		}
	}

	private static void checkBusPin(Controller controller, LadderOperand operand, bool isWrite, int line) {
		if (operand == null || operand.Kind != LadderOperandKind.Bus) {
			return;
		}

		ControllerBus bus = controller.GetBus(operand.Name);
		if (bus == null) {
			// Unknown bus — the player may be mid-wiring.  Runtime reads it as
			// zero and drops writes, same as PLC-PY.
			return;
		}
		int index = bus.IndexOfPin(operand.Member);
		if (index < 0) {
			return;
		}

		ControllerBus.BusPinType type = bus.PinTypes[index];
		if (isWrite && type != ControllerBus.BusPinType.Plc) {
			throw new LadderParseException(line,
				$"Coil drives bus pin '{operand}', but that pin is type {type} — only Plc-typed pins can be written from a PLC module. Change the pin's type in the bus settings.");
		}
		if (!isWrite && type == ControllerBus.BusPinType.Plc) {
			throw new LadderParseException(line,
				$"Contact reads bus pin '{operand}', but Plc-typed pins are write-only from a PLC module. Change the pin's type in the bus settings.");
		}
	}

	// Machine state is rebuilt from NumberData because CompiledBlock is a
	// non-persisted cache — after a load the program re-parses and the timers
	// pick up where they left off.
	private static int[] readState(Module m) {
		if (!m.NumberData.TryGetValue(STATE_COUNT_KEY, out int count) || count <= 0) {
			return null;
		}
		int[] state = new int[count];
		for (int i = 0; i < count; i++) {
			state[i] = m.NumberData.TryGetValue(STATE_PREFIX + i, out int value) ? value : 0;
		}
		return state;
	}

	private static void writeState(Module m, int[] state) {
		int previous = m.NumberData.TryGetValue(STATE_COUNT_KEY, out int stored) ? stored : 0;
		for (int i = 0; i < state.Length; i++) {
			m.NumberData[STATE_PREFIX + i] = state[i];
		}
		// A shrinking program would otherwise leave stale slots behind, which
		// would be read back as garbage if it later grew again.
		for (int i = state.Length; i < previous; i++) {
			m.NumberData.TryRemove(STATE_PREFIX + i, out _);
		}
		m.NumberData[STATE_COUNT_KEY] = state.Length;
	}

	// Binds ladder operands to the module, its controller's buses, and the
	// packed memory bank.
	//
	// The process image is the part worth being careful about: inputs are read
	// live (the sim has already settled them for this tick), memory bits are
	// written through immediately so a seal-in latch closes inside one scan,
	// and outputs buffer until EndScan so a later network reads the PREVIOUS
	// scan's value instead of seeing a half-updated world.
	private sealed class ModuleLadderIo : ILadderIo {
		private readonly Module m_module;
		private readonly Dictionary<string, Fix32> m_pendingOutputs = new Dictionary<string, Fix32>();
		private int m_memory;

		public ModuleLadderIo(Module module) {
			m_module = module;
		}

		public void BeginScan() {
			m_pendingOutputs.Clear();
			m_memory = m_module.NumberData.TryGetValue(MEMORY_KEY, out int bits) ? bits : 0;
		}

		public Fix32 Read(LadderOperand operand) {
			switch (operand.Kind) {
			case LadderOperandKind.Input:
				return m_module.Input[operand.Name];
			case LadderOperandKind.Output:
				// Deliberately NOT the pending buffer — reading a coil you
				// drove earlier in the same scan gives last scan's value, the
				// way a real process image behaves.
				return m_module.Output[operand.Name];
			case LadderOperandKind.Memory:
				return (m_memory & (1 << operand.Index)) != 0 ? Fix32.One : Fix32.Zero;
			case LadderOperandKind.Bus: {
				ControllerBus bus = m_module.Controller?.GetBus(operand.Name);
				return bus == null ? Fix32.Zero : bus.Get(operand.Member);
			}
			default:
				return operand.Literal;
			}
		}

		public void Write(LadderOperand operand, Fix32 value) {
			switch (operand.Kind) {
			case LadderOperandKind.Output:
				m_pendingOutputs[operand.Name] = value;
				break;
			case LadderOperandKind.Memory:
				if (value > Fix32.Zero) {
					m_memory |= 1 << operand.Index;
				} else {
					m_memory &= ~(1 << operand.Index);
				}
				m_module.NumberData[MEMORY_KEY] = m_memory;
				break;
			case LadderOperandKind.Bus: {
				ControllerBus bus = m_module.Controller?.GetBus(operand.Name);
				bus?.Set(operand.Member, value);
				break;
			}
			}
		}

		public void EndScan() {
			foreach (KeyValuePair<string, Fix32> pending in m_pendingOutputs) {
				m_module.Output[pending.Key] = pending.Value;
			}
			m_module.NumberData[MEMORY_KEY] = m_memory;
		}
	}
}
