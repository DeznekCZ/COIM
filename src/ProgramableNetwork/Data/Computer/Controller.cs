using Mafi;
using Mafi.Base;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.ComputingPower;
using Mafi.Core.Factory.ElectricPower;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Maintenance;
using Mafi.Core.Notifications;
using Mafi.Core.Population;
using Mafi.Core.Ports.Io;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.Research;
using Mafi.Core.Trains;
using Mafi.Core.Vehicles;
using Mafi.Localization;
using Mafi.Serialization;
using ProgramableNetwork.Data.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ProgramableNetwork
{
	// TODO (next commit): sort all fields and properties before the constructor,
	// then all methods (public then private) after it.  The class has grown by
	// accretion — IsRangeFree / TryRemoveModule / TryMoveModule / TryPlaceModule
	// / TryShiftAddModule / ApplyPythonTemplate currently sit between the
	// description property and the constructor, which puts methods before
	// fields.  Moving the new helpers below the constructor (alongside
	// UpdateModules) will restore the field-property-ctor-method ordering and
	// make the class easier to skim.
	[ManuallyWrittenSerialization]
	public class Controller : LayoutEntityBase, IAreaSelectableEntity, IEntityWithCloneableConfig, IEntityWithSimUpdate,
		IUnityConsumingEntity, IComputingConsumingEntity, IElectricityConsumingEntity, IMaintainedEntity, IObjectWithCustomTitle
	{
		// Serialization version where the per-cell layout grid (Controller.Rows) was
		// dropped and each Module started carrying its own (Row, Column). Used by both
		// Controller and Module deserialization for version-gated reads.
		public const int MODULE_LAYOUT_INFO = 4;

		// Serialization version where Module's optional containers (the five Number/
		// String dicts, InputModules, and the new Fix32[] ArrayData scratch buffer)
		// are gated behind a single byte bitmask so empty containers cost nothing
		// on disk.  Older saves read each dict unconditionally; v5+ writes only the
		// populated ones.  ArrayData was introduced in this same version — pre-v5
		// modules load with an empty array.
		public const int MODULE_COMPACT_DATA = 5;

		// Controller serialization version where the per-instance CustomDescription
		// field was added.  Earlier saves load with no description set; a fresh
		// CustomDescription = None is the safe default.
		public const int CONTROLLER_DESCRIPTION = 5;

		// Serialization version where the PLC (player-authored Python) module
		// landed.  The DataFlags byte gained a CodeMetadata bit (1 << 7) so PLC
		// instances can persist their cached lexer-node count alongside the
		// existing dicts; pre-v6 modules read with that bit absent and a
		// node count of 0 (re-tokenized on first execute).  Forward-compatible
		// with non-PLC modules — they simply never set the bit.
		public const int MODULE_PYTHON_CODE = 6;

		// Serialization version where Module gained per-instance pin extension
		// counts (InputExtensionCount / OutputExtensionCount) — extra input and/or
		// output pins added by the player on the right side of an extensible
		// prototype.  Pre-v7 saves load with both counts at 0 (no extensions),
		// matching the original behavior.
		public const int MODULE_EXTENSIONS = 7;

		// Display extension count was added a step later at the same conceptual
		// "extensions" feature but a separate version bump is required because
		// in-progress dev saves at v7 were written with only the two pin counts
		// (Input + Output) — reading a third int there walks past the end of the
		// module's data and corrupts the byte stream.  v8+ writes all three; v7
		// loads just the two and leaves DisplayExtensionCount at 0 so the player
		// can grow the display from the inspector after load.
		public const int MODULE_DISPLAY_EXTENSIONS = 8;

		// PLC-PY persistent context — Dict<string, object> of player-defined
		// variables that survive the init: → run: handoff and round-trip
		// through saves.  Stored at the very end of the module's data block
		// as `WriteInt(count) + entries`, so v8 saves (which lack it) can be
		// loaded by reading the rest of the module's fields and skipping the
		// context read.  PlcContextSerializer's whitelist drops non-
		// serialisable values silently; init: re-runs after any non-Running
		// result anyway, so dropped scratch values are recoverable.
		public const int MODULE_PLC_CONTEXT = 9;

		// Module serialization version where input connections (InputModules) gained
		// an explicit ConnectorKind (Module vs Bus) and are written INLINE
		// (count + per-entry key/kind/moduleId/outputId) instead of via
		// Dict<string,ModuleConnector>.Serialize — the inline form lets the kind byte
		// be version-gated.  Pre-v10 saves are read with the old Dict format and load
		// as all-Module connections (the bus feature didn't exist before v10).
		public const int MODULE_BUS_CONNECTOR_KIND = 10;

		// Controller serialization where module ids switched from a
		// time-based source (DateTime.UtcNow.Ticks + Thread.Sleep(1)) to a
		// per-controller pool counter persisted on the controller itself.
		// Pre-v6 saves don't carry the counter; on load, initContexts seeds
		// m_nextModuleId from <c>max(existing module ids)</c> so subsequent
		// allocations stay unique within the controller.  Existing modules
		// keep their original time-based ids — the migration is additive.
		public const int CONTROLLER_MODULE_ID_POOL = 6;

		// Controller serialization version where the per-controller variable bus
		// was added — a list of ControllerBus strips (each = 4 named bidirectional
		// Fix32 pins) appended at the very END of the controller's data block, so
		// pre-v7 saves (which stop after the module-id pool long) still load with
		// an empty bus list.  Separate constant from the Module-side versions even
		// though it shares the numeric sequence; the controller stream is read in
		// DeserializeData against CONTROLLER_* gates only.
		public const int CONTROLLER_VARIABLE_BUS = 7;

		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction = delegate(object obj, BlobWriter writer)
		{
			((Controller) obj).SerializeData(writer);
		};
		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction = delegate (object obj, BlobReader reader)
		{
			((Controller) obj).DeserializeData(reader);
		};

		public Option<string> CustomTitle { get; set; }

		// Legacy field kept on disk so old saves can still round-trip — for v6+ saves
		// it gets read/written but is no longer consulted at allocation time.  IDs
		// now come from <see cref="ModuleIdManager"/> which holds a single monotonic
		// counter and is serialized with the save; access is via the static
		// <see cref="ModuleIdManager.Instance"/> resolver variable rather than
		// GlobalDependencyResolver.Get<>().  The previous per-controller pool was
		// the source of cable-colour and entity-field collisions: every fresh
		// controller restarted at 1, so two pasted modules from different
		// blueprints could end up with identical ids on a single controller and
		// share their colour-palette / field-data lookups.
		private long m_nextModuleId;

		/// <summary>
		/// Hands out a fresh module id from the serialized global pool so values
		/// never collide across controllers, blueprint pastes, or template
		/// applications.  The legacy per-controller pool name is preserved to keep
		/// callers (BlueprintControllerTemplateEntry, etc.) source-compatible.
		/// </summary>
		public long AllocateModuleId() => Resolver.Resolve<ModuleIdManager>().Allocate();

		// Player-writable free-form description.  Auto-populated when a template/
		// blueprint is applied via the picker (set to the template's description),
		// editable in the inspector after that.  The on-screen rendering ALWAYS
		// has the live module list appended via <see cref="GetFullDescription"/> —
		// only the user-supplied prefix is persisted here.
		public Option<string> CustomDescription { get; set; }

		// Player-defined variable buses living in the left gutter of the controller
		// view.  Each bus is a 4-pin strip whose pins are read/written from PLC-PY
		// as self.Bus.<bus_name>.<pin_name>.  Initialized empty for fresh controllers
		// and re-populated in DeserializeData (v7+ saves); pre-v7 saves load empty.
		public Lyst<ControllerBus> Buses { get; private set; } = new Lyst<ControllerBus>();

		// Finds a bus by its name, or null when none matches (or the name is blank).
		// Used by the PLC-PY Bus wrapper and the IntelliSense completion builder.
		public ControllerBus GetBus(string busName)
		{
			if (string.IsNullOrEmpty(busName) || Buses == null)
			{
				return null;
			}
			foreach (ControllerBus bus in Buses)
			{
				if (bus.Name == busName)
				{
					return bus;
				}
			}
			return null;
		}

		public ControllerBus GetBusById(long busId)
		{
			if (Buses == null)
			{
				return null;
			}
			foreach (ControllerBus bus in Buses)
			{
				if (bus.Id == busId)
				{
					return bus;
				}
			}
			return null;
		}

		// ---- Variable bus mutation -------------------------------------------------
		// Direct mutators used by the gutter UI for now; these get wrapped in
		// serialized InputCommands at the end of Phase 2 (per plan) so multiplayer
		// hosts/clients stay in lock-step.  Each touches only the Buses list, which
		// the signal plan doesn't depend on yet (no bus edges until 2b), so no
		// InvalidateTopology is needed for name edits; CreateBus invalidates anyway
		// to be safe once bus edges land.
		public ControllerBus CreateBus(string name, ControllerBus.BusSide side = ControllerBus.BusSide.Left)
		{
			ControllerBus bus = new ControllerBus(AllocateModuleId(), name ?? "");
			bus.Side = side;
			Buses.Add(bus);
			InvalidateTopology();
			return bus;
		}

		public void SetBusSide(long busId, ControllerBus.BusSide side)
		{
			ControllerBus bus = GetBusById(busId);
			if (bus != null)
			{
				bus.Side = side;
			}
		}

		public void RenameBus(long busId, string name)
		{
			ControllerBus bus = GetBusById(busId);
			if (bus != null)
			{
				bus.Name = name ?? "";
			}
		}

		public void SetBusPinName(long busId, int pinIndex, string name)
		{
			ControllerBus bus = GetBusById(busId);
			if (bus != null && pinIndex >= 0 && pinIndex < ControllerBus.PinCount)
			{
				bus.PinNames[pinIndex] = name ?? "";
			}
		}

		public void SetBusPinType(long busId, int pinIndex, ControllerBus.BusPinType type)
		{
			ControllerBus bus = GetBusById(busId);
			if (bus != null && pinIndex >= 0 && pinIndex < ControllerBus.PinCount)
			{
				bus.PinTypes[pinIndex] = type;
				// Changing type invalidates any source that no longer matches; clearing
				// it keeps stale wiring from leaking across a type change.  (Stage-2
				// connect logic repopulates the source for the new type.)
				bus.PinSources[pinIndex] = null;
				// A type change also resets the pin's downstream wiring: drop any module
				// inputs that read this pin, since their validity depends on the (now
				// changed) type — e.g. an Input-type pin must never drive a module input.
				disconnectModuleInputsFromBusPin(busId, pinIndex);
				InvalidateTopology();
			}
		}

		// Removes every module-input cable that reads the given bus pin.  Used when a pin
		// is retyped (its meaning changes) so no stale bus→module-input connection lingers.
		private void disconnectModuleInputsFromBusPin(long busId, int pinIndex)
		{
			foreach (Module m in Modules)
			{
				if (m?.InputModules == null)
				{
					continue;
				}
				foreach (var kv in m.InputModules.ToArray())
				{
					if (kv.Value.IsBus && kv.Value.ModuleId == busId
						&& kv.Value.TryGetPinIndex(out int pi) && pi == pinIndex)
					{
						m.InputModules.Remove(kv.Key);
						m.InputNumberData.TryRemove(kv.Key, out _);
					}
				}
			}
		}

		public bool RemoveBus(long busId)
		{
			ControllerBus bus = GetBusById(busId);
			if (bus == null)
			{
				return false;
			}
			Buses.RemoveFirst(b => b.Id == busId);
			InvalidateTopology();
			return true;
		}

		// Sets (or clears, when source == null) the input source feeding a bus pin.
		// Used by the gutter connect flow for Input pins (module output) and later by
		// Controller / NetworkRead pin config.
		public void SetBusPinSource(long busId, int pinIndex, BusPinSource source)
		{
			ControllerBus bus = GetBusById(busId);
			if (bus != null && pinIndex >= 0 && pinIndex < ControllerBus.PinCount)
			{
				bus.PinSources[pinIndex] = source;
				InvalidateTopology();
			}
		}

		/// <summary>
		/// Returns the user-supplied description (if any) followed by an auto-generated
		/// list of every module currently on the controller.  The module-list tail is
		/// always present so a player browsing the inspector can see what's inside even
		/// when no description was authored.  Computed on demand — module list reflects
		/// the live state.
		/// </summary>
		public string GetFullDescription()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			if (CustomDescription.HasValue && !string.IsNullOrEmpty(CustomDescription.Value))
			{
				sb.Append(CustomDescription.Value);
				sb.Append("\n\n");
			}
			sb.Append("Modules:");
			if (Modules == null || Modules.Count == 0)
			{
				sb.Append(" (none)");
			}
			else
			{
				foreach (Module m in Modules)
				{
					if (m?.Prototype == null) {
						continue;
					}
					sb.Append("\n  ");
					sb.Append(m.Prototype.Symbol);
					sb.Append("  ");
					sb.Append(m.Prototype.Strings.Name.TranslatedString);
				}
			}
			return sb.ToString();
		}

		/// <summary>
		/// True iff every cell in [col, col+width) on the given row is in-bounds and
		/// unoccupied by any module other than <paramref name="ignore"/>.  Shared
		/// between the inspector (pre-validation, audio feedback) and the command
		/// executor (authoritative re-check on the sim thread) so both decisions
		/// use the same rule.  Out-of-bounds rows/cols return false rather than
		/// throwing — callers test this before mutating.
		/// </summary>
		public bool IsRangeFree(int row, int col, int width, Module ignore)
		{
			if (Prototype == null) {
				return false;
			}
			if (row < 0 || row >= Prototype.Rows) {
				return false;
			}
			if (col < 0 || col + width > Prototype.Columns) {
				return false;
			}
			foreach (Module m in Modules)
			{
				if (m == null || m.Prototype == null) {
					continue;
				}
				if (ignore != null && m.Id == ignore.Id) {
					continue;
				}
				if (m.Row != row) {
					continue;
				}
				int mw = m.Layout.GetWidth(m);
				int mEnd = m.Column + mw;
				int end = col + width;
				if (m.Column < end && col < mEnd) {
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Removes the module with the given id and drops every cable that
		/// referenced it from any other module on this controller.  Returns false
		/// if no such module exists.  Used by ModuleRemoveCmd on the sim thread.
		/// </summary>
		public bool TryRemoveModule(long moduleId)
		{
			Module victim = null;
			foreach (Module m in Modules) {
				if (m.Id == moduleId) { victim = m; break; }
			}
			if (victim == null) {
				return false;
			}
			Modules.RemoveFirst(m => m.Id == moduleId);
			foreach (Module item in Modules)
			{
				foreach (KeyValuePair<string, ModuleConnector> input in item.InputModules.ToList())
				{
					if (input.Value.ModuleId == moduleId) {
						item.InputModules.Remove(input.Key);
					}
				}
			}
			InvalidateTopology();
			return true;
		}

		/// <summary>
		/// Moves the module to (targetRow, targetColumn), snapping past-end-of-line
		/// targets to the rightmost valid slot rather than refusing.  The clamp is
		/// the "move to slot, on end of line if it would collide with himself"
		/// behaviour requested by the inspector: a wide module nudged right at the
		/// edge stays at the edge instead of bouncing.  Returns false only if the
		/// module is unknown, the controller has no valid slot for this width, or
		/// the clamped destination is occupied by a different module.  A clamped
		/// target equal to the module's current position is a successful no-op.
		/// Used by ModuleMoveToCmd.
		/// </summary>
		public bool TryMoveModule(long moduleId, int targetRow, int targetColumn)
		{
			Module module = null;
			foreach (Module m in Modules) {
				if (m.Id == moduleId) { module = m; break; }
			}
			if (module == null || Prototype == null) {
				return false;
			}
			int width = module.Layout.GetWidth(module);
			if (width <= 0 || width > Prototype.Columns || Prototype.Rows <= 0) {
				return false;
			}
			int clampedRow = System.Math.Max(0, System.Math.Min(Prototype.Rows - 1, targetRow));
			int clampedCol = System.Math.Max(0, System.Math.Min(Prototype.Columns - width, targetColumn));
			if (clampedRow == module.Row && clampedCol == module.Column) {
				// Already at the (clamped) target — treat as success rather than
				// forcing the caller to special-case the no-op.
				return true;
			}
			if (!IsRangeFree(clampedRow, clampedCol, width, ignore: module)) {
				return false;
			}
			module.Row = clampedRow;
			module.Column = clampedCol;
			return true;
		}

		/// <summary>
		/// Creates a fresh module of the given prototype at (targetRow, targetColumn)
		/// and returns the new id (or 0 on failure).  Calls <c>ExecuteInit</c> on
		/// the new module before returning so the prototype's default state (fields,
		/// extension counts, etc.) is set on the sim thread for every MP peer — the
		/// inspector used to do this client-side after a successful place, which
		/// would diverge between host and clients.  The allocated id is freed via
		/// <c>ModuleIdManager.Free</c> when placement validation fails so the id
		/// pool doesn't leak.  Used by ModulePlaceCmd.
		/// </summary>
		public long TryPlaceModule(ModuleProto proto, int targetRow, int targetColumn)
		{
			if (proto == null) {
				return 0;
			}
			ModuleIdManager idManager = Resolver.Resolve<ModuleIdManager>();
			long newId = idManager.Allocate();
			Module module = new Module(proto, Context, this, newId);
			int width = module.Layout.GetWidth(module);
			if (!IsRangeFree(targetRow, targetColumn, width, ignore: null))
			{
				idManager.Free(newId);
				return 0;
			}
			module.Row = targetRow;
			module.Column = targetColumn;
			module.Prototype.ExecuteInit(module);
			Modules.Add(module);
			InvalidateTopology();
			return newId;
		}

		/// <summary>
		/// Places a fresh module of <paramref name="proto"/> at (row, column) and
		/// then stamps the configurable state from <paramref name="snapshot"/> onto
		/// it — combines <see cref="TryPlaceModule"/> + <see cref="TryPasteModule"/>
		/// into a single atomic operation.  Used by ModulePlaceFromBlueprintCmd so
		/// the configure step (which carries arbitrary, possibly per-client data)
		/// runs on the sim thread for every MP peer.  Returns the new module's id
		/// (0 on failure).
		/// </summary>
		public long TryPlaceModuleFromSnapshot(ModuleProto proto, int targetRow, int targetColumn, Module snapshot)
		{
			long newId = TryPlaceModule(proto, targetRow, targetColumn);
			if (newId == 0) {
				return 0;
			}
			if (snapshot != null)
			{
				TryPasteModule(newId, snapshot);
			}
			return newId;
		}

		/// <summary>
		/// Copies the source module's configurable state onto an EXISTING destination
		/// module on this controller — same data copy that <see cref="TryShiftAddModule"/>
		/// performs, but without the placement step.  Used by ModulePasteCmd to back
		/// the inspector's "paste from last-created" button.  Cable connections
		/// (<c>InputModules</c>) are intentionally NOT copied — the source's
		/// endpoints don't generally point at neighbours of the destination.
		/// Returns false when the destination doesn't live on this controller.
		/// </summary>
		public bool TryPasteModule(long destModuleId, Module source)
		{
			Module dest = null;
			foreach (Module m in Modules) {
				if (m.Id == destModuleId) { dest = m; break; }
			}
			if (dest == null || source?.Prototype == null) {
				return false;
			}
			dest.SetStatus(ModuleStatus.Init);
			dest.NumberData.Clear();
			dest.FieldNumberData.Clear();
			dest.StringData.Clear();
			foreach (KeyValuePair<string, int> item in source.NumberData) {
				dest.NumberData[item.Key] = item.Value;
			}
			foreach (KeyValuePair<string, Fix32> item in source.FieldNumberData) {
				dest.FieldNumberData[item.Key] = item.Value;
			}
			foreach (KeyValuePair<string, string> item in source.StringData) {
				dest.StringData[item.Key] = item.Value;
			}
			// All three extension dimensions roundtrip via the Set* setters so the
			// linked-side mirroring and dropped-cable pruning happen on the sim
			// thread, deterministically across MP peers.
			dest.SetInputExtensionCount(source.InputExtensionCount);
			dest.SetOutputExtensionCount(source.OutputExtensionCount);
			dest.SetDisplayExtensionCount(source.DisplayExtensionCount);
			if (source.ArrayData != null && source.ArrayData.Length > 0)
			{
				Fix32[] copy = new Fix32[source.ArrayData.Length];
				System.Array.Copy(source.ArrayData, copy, copy.Length);
				typeof(Module).GetProperty(nameof(Module.ArrayData)).SetValue(dest, copy);
			}
			dest.Prototype.ExecuteInit(dest);
			// Extension count changes may have dropped cables — invalidate the
			// signal-copy plan so the next tick rebuilds without the stale edges.
			InvalidateTopology();
			return true;
		}

		/// <summary>
		/// Stamps a copy of <paramref name="source"/> at (targetRow, targetColumn) on
		/// this controller.  The destination's prototype is initialised, then the
		/// source's NumberData / FieldNumberData / StringData / extension counts /
		/// ArrayData are copied across so the new module reproduces the original's
		/// settings.  Returns the new id (or 0 on failure).  Used by ModuleShiftAddCmd.
		/// </summary>
		public long TryShiftAddModule(Module source, int targetRow, int targetColumn)
		{
			if (source?.Prototype == null) {
				return 0;
			}
			long newId = TryPlaceModule(source.Prototype, targetRow, targetColumn);
			if (newId == 0) {
				return 0;
			}
			Module placed = null;
			foreach (Module m in Modules) {
				if (m.Id == newId) { placed = m; break; }
			}
			if (placed == null) {
				return 0;
			}
			placed.Prototype.ExecuteInit(placed, log: false);
			foreach (KeyValuePair<string, int> item in source.NumberData) {
				placed.NumberData[item.Key] = item.Value;
			}
			foreach (KeyValuePair<string, Fix32> item in source.FieldNumberData) {
				placed.FieldNumberData[item.Key] = item.Value;
			}
			foreach (KeyValuePair<string, string> item in source.StringData) {
				placed.StringData[item.Key] = item.Value;
			}
			placed.SetInputExtensionCount(source.InputExtensionCount);
			placed.SetOutputExtensionCount(source.OutputExtensionCount);
			if (source.ArrayData != null && source.ArrayData.Length > 0)
			{
				Fix32[] copy = new Fix32[source.ArrayData.Length];
				System.Array.Copy(source.ArrayData, copy, copy.Length);
				typeof(Module).GetProperty(nameof(Module.ArrayData)).SetValue(placed, copy);
			}
			placed.Prototype.DisplayUpdate(placed);
			return newId;
		}

		/// <summary>
		/// Applies a Python-registered controller template (lookup by stable id) to
		/// this controller in place — clears existing modules, runs the template's
		/// placement lambda, executes per-module Init, then runs the deferred
		/// settings callback.  Returns false if no template with the given id is
		/// registered (e.g., mod removed since save).  Used by ControllerApplyTemplateCmd.
		/// </summary>
		public bool ApplyPythonTemplate(string templateId)
		{
			Python.ControllerTemplate? match = null;
			foreach (Python.ControllerTemplate t in Data.Mod.ControllerTemplates.CachedTemplates)
			{
				if (t.id == templateId) { match = t; break; }
			}
			if (!match.HasValue) {
				return false;
			}
			Python.ControllerTemplate template = match.Value;
			if (template.modules == null) {
				return false;
			}
			Modules.Clear();
			Python.ControllerTemplate.Settings settings = template.modules(this);
			foreach (Module module in Modules) {
				module.Prototype.ExecuteInit(module);
			}
			settings?.Invoke();
			InvalidateTopology();
			SetColor(template.color);
			if (!string.IsNullOrEmpty(template.description)) {
				CustomDescription = template.description.SomeOption();
			}
			return true;
		}

		public Controller(EntityId id, ControllerProto proto, TileTransform transform, EntityContext context,
			IEntityMaintenanceProvidersFactory maintenanceProvidersFactory,
			DependencyResolver resolver)
			: base(id, proto, transform, context)
		{
			// Mafi's [InitAfterLoad] hook only fires on deserialize, so brand-new
			// controllers (just placed by the player) don't go through initContexts
			// and would have Resolver = null.  Taking it as a ctor arg fixes that —
			// the entity factory passes the live DependencyResolver so module-add,
			// blueprint-save, and every other Resolver.Resolve<>() call site work
			// from the moment the controller exists in the world.
			Resolver = resolver;
			Prototype = proto.BasedOn ?? proto;
			ErrorMessage = "";
			Color = proto.DefaultColor;
			m_unityConsumer = Context.UnityConsumerFactory.CreateConsumer(this);
			m_electricConsumer = Context.ElectricityConsumerFactory.CreateConsumer(this);
			m_computingConsumer = Context.ComputingConsumerFactory.CreateConsumer(this);
			m_maintenanceConsumer = maintenanceProvidersFactory.CreateFor(this);
			m_notificationInfoManager = Context.NotificationsManager.CreateNotificatorFor(ControllerNotification.InfoNotification);
			m_notificationWarningManager = Context.NotificationsManager.CreateNotificatorFor(ControllerNotification.WarningNotification);
			m_notificationErrorManager = Context.NotificationsManager.CreateNotificatorFor(ControllerNotification.ErrorNotification);
			Modules = new Lyst<Module>();

			Action initSettings = proto.InitModules(this);
			foreach (Module module in Modules)
			{
				module.Prototype.ExecuteInit(module);
			}
			initSettings();

			Log.Info($"Created with {Prototype.Rows} rows, {Prototype.Columns} columns");
		}

		[DoNotSave(0, null)]
		private ControllerProto m_proto;
		[DoNotSave(0, null)]
		private Mafi.Core.Entities.Static.StaticEntityProto.ID m_protoId;

		[DoNotSave(0, null)]
		public new ControllerProto Prototype
		{
			get
			{
				return m_proto;
			}
			protected set
			{
				m_proto = value;
				m_protoId = m_proto.Id;
				base.Prototype = value;
			}
		}

		[DoNotSave(0, null)]
		public override bool CanBePaused => true;
		[DoNotSave(0, null)]
		public bool CanWorkOvertime => true;

		// Pre-compiled per-tick signal-copy plan.  Modules/connections only change
		// on user edits (and on load), so the dictionary indirections that used to
		// run every tick (rebuild Dictionary<long, Module>, ToArray on each
		// InputModules, two TryGetValue hops per cable) are hoisted into these
		// caches and rebuilt only when InvalidateTopology() is called.  See
		// UpdateModules() for the hot path that consumes them.
		[DoNotSave(0, null)]
		private bool m_topologyDirty;
		[DoNotSave(0, null)]
		private Dictionary<long, Module> m_moduleCache;
		[DoNotSave(0, null)]
		private CopyEdge[] m_copyPlan;
		[DoNotSave(0, null)]
		private InputClear[] m_inputClearPlan;

		// One per (input pin <- output pin) cable.  Direct dict references skip
		// the Modules-by-Id lookup; string keys are still needed because output
		// pin ids can be written dynamically by a module's Execute() — they are
		// not statically declared anywhere we could index by.
		private readonly struct CopyEdge
		{
			public readonly Dict<string, Fix32> SrcOutputs;
			public readonly string SrcKey;
			public readonly Dict<string, Fix32> DstInputs;
			public readonly string DstKey;
			public CopyEdge(Dict<string, Fix32> srcOutputs, string srcKey,
							Dict<string, Fix32> dstInputs, string dstKey)
			{
				SrcOutputs = srcOutputs;
				SrcKey = srcKey;
				DstInputs = dstInputs;
				DstKey = dstKey;
			}
		}

		// Per-module list of input pin ids that have NO incoming cable.  These
		// get TryRemove'd each tick so an unconnected pin reads as zero; connected
		// pins are unconditionally written by the copy plan so they don't need
		// clearing first.
		private readonly struct InputClear
		{
			public readonly Dict<string, Fix32> Dict;
			public readonly string[] Keys;
			public InputClear(Dict<string, Fix32> dict, string[] keys)
			{
				Dict = dict;
				Keys = keys;
			}
		}

		// Bus signal-plan caches (Input-type pin dataflow, stage 2a).  Bus pin values
		// live in ControllerBus.PinValues[] (Fix32[] per bus), so these edges carry a
		// bus reference + pin index instead of a Dict like CopyEdge.
		[DoNotSave(0, null)]
		private BusFeedEdge[] m_busFeedPlan;   // module output -> Input bus pin
		[DoNotSave(0, null)]
		private BusReadEdge[] m_busReadPlan;   // bus pin -> module input
		[DoNotSave(0, null)]
		private BusPinRef[] m_busClearPlan;    // Input-type pins cleared each tick

		private readonly struct BusFeedEdge
		{
			public readonly Dict<string, Fix32> SrcOutputs;
			public readonly string SrcKey;
			public readonly ControllerBus Bus;
			public readonly int PinIdx;
			public BusFeedEdge(Dict<string, Fix32> srcOutputs, string srcKey, ControllerBus bus, int pinIdx)
			{
				SrcOutputs = srcOutputs;
				SrcKey = srcKey;
				Bus = bus;
				PinIdx = pinIdx;
			}
		}

		private readonly struct BusReadEdge
		{
			public readonly ControllerBus Bus;
			public readonly int PinIdx;
			public readonly Dict<string, Fix32> DstInputs;
			public readonly string DstKey;
			public BusReadEdge(ControllerBus bus, int pinIdx, Dict<string, Fix32> dstInputs, string dstKey)
			{
				Bus = bus;
				PinIdx = pinIdx;
				DstInputs = dstInputs;
				DstKey = dstKey;
			}
		}

		private readonly struct BusPinRef
		{
			public readonly ControllerBus Bus;
			public readonly int PinIdx;
			public BusPinRef(ControllerBus bus, int pinIdx)
			{
				Bus = bus;
				PinIdx = pinIdx;
			}
		}

		/// <summary>
		/// Marks the cached signal-copy plan as stale.  Call after any change to
		/// the module list or to any module's InputModules connections.  The plan
		/// is rebuilt lazily on the next SimUpdate tick.
		/// </summary>
		public void InvalidateTopology()
		{
			m_topologyDirty = true;
		}

		private void BuildSignalPlan()
		{
			var cache = new Dictionary<long, Module>(Modules.Count);
			foreach (Module m in Modules) { cache[m.Id] = m; }
			m_moduleCache = cache;

			var copyEdges = new List<CopyEdge>();
			var busReadEdges = new List<BusReadEdge>();
			var clearPlan = new InputClear[Modules.Count];
			int moduleIdx = 0;

			foreach (Module module in Modules)
			{
				// Walk a snapshot of InputModules so we can prune dangling entries
				// (source module no longer on this controller) inline, matching
				// the self-healing behavior that UpdateModules used to do per tick.
				HashSet<string> connected = null;
				foreach (var item in module.InputModules.ToArray())
				{
					if (item.Value.IsBus)
					{
						// Source is a bus pin on this controller (ModuleId = bus id,
						// OutputId = pin index).
						if (GetBusById(item.Value.ModuleId) is ControllerBus srcBus
							&& item.Value.TryGetPinIndex(out int pinIdx)
							&& pinIdx >= 0 && pinIdx < ControllerBus.PinCount)
						{
							busReadEdges.Add(new BusReadEdge(srcBus, pinIdx, module.InputNumberData, item.Key));
							(connected ??= new HashSet<string>()).Add(item.Key);
						}
						else
						{
							module.InputModules.Remove(item.Key);
							module.InputNumberData.TryRemove(item.Key, out _);
						}
					}
					else if (cache.TryGetValue(item.Value.ModuleId, out Module srcMod))
					{
						copyEdges.Add(new CopyEdge(
							srcMod.OutputNumberData, item.Value.OutputId,
							module.InputNumberData, item.Key));
						(connected ??= new HashSet<string>()).Add(item.Key);
					}
					else
					{
						module.InputModules.Remove(item.Key);
						module.InputNumberData.TryRemove(item.Key, out _);
					}
				}

				// Inputs with no incoming cable — these get cleared each tick so
				// they read as zero.  Connected pins are unconditionally overwritten
				// by the copy plan, so we skip them here.
				var unconnected = new List<string>();
				foreach (var input in module.EffectiveInputs)
				{
					if (connected == null || !connected.Contains(input.Id)) {
						unconnected.Add(input.Id);
					}
				}
				clearPlan[moduleIdx++] = new InputClear(module.InputNumberData, unconnected.ToArray());
			}

			// Bus pins: per-tick clear list for every Input pin + the module-output
			// feeds for Input pins whose source is a local module output.  (Controller
			// and NetworkRead pin behaviors are added in stage 2b.)
			var busFeedEdges = new List<BusFeedEdge>();
			var busClears = new List<BusPinRef>();
			if (Buses != null)
			{
				foreach (ControllerBus bus in Buses)
				{
					for (int i = 0; i < ControllerBus.PinCount; i++)
					{
						if (bus.PinTypes[i] != ControllerBus.BusPinType.Output)
						{
							continue;
						}
						busClears.Add(new BusPinRef(bus, i));
						BusPinSource src = bus.PinSources[i];
						if (src != null && src.Kind == BusPinSource.SourceKind.LocalModule)
						{
							if (cache.TryGetValue(src.ModuleId, out Module srcMod))
							{
								busFeedEdges.Add(new BusFeedEdge(srcMod.OutputNumberData, src.OutputId, bus, i));
							}
							else
							{
								bus.PinSources[i] = null; // prune invalid source
							}
						}
					}
				}
			}

			m_copyPlan = copyEdges.ToArray();
			m_inputClearPlan = clearPlan;
			m_busReadPlan = busReadEdges.ToArray();
			m_busFeedPlan = busFeedEdges.ToArray();
			m_busClearPlan = busClears.ToArray();
			m_topologyDirty = false;
		}

		// Per-tick read for Controller-type bus pins: resolve the referenced controller
		// (this one or another) live and copy its module-output / bus-pin value into the
		// pin.  Latches to 0 when the reference can't be resolved or the target is out
		// of bus-link range.  Not part of the cached signal plan because the remote can
		// appear/disappear/move without changing THIS controller's topology.
		private void updateControllerPins()
		{
			if (Buses == null)
			{
				return;
			}
			IEntitiesManager entities = null;
			foreach (ControllerBus bus in Buses)
			{
				for (int i = 0; i < ControllerBus.PinCount; i++)
				{
					if (bus.PinTypes[i] != ControllerBus.BusPinType.Controller)
					{
						continue;
					}
					BusPinSource src = bus.PinSources[i];
					Fix32 value = Fix32.Zero;
					if (src != null && src.IsExternalController)
					{
						entities = entities ?? Resolver.Resolve<IEntitiesManager>();
						if (entities.TryGetEntity(src.ControllerId, out Controller remote)
							&& (remote == this || IsInBusLinkRange(remote)))
						{
							value = readExternalSource(src, remote);
						}
					}
					bus.PinValues[i] = value;
				}
			}
		}

		private static Fix32 readExternalSource(BusPinSource src, Controller remote)
		{
			if (src.Kind == BusPinSource.SourceKind.ExternalModule)
			{
				Module rm = remote.Modules?.Find(m => m.Id == src.ModuleId);
				if (rm != null)
				{
					// The picker only binds Controller-pins to a "Connection: Controller
					// (output)" endpoint, whose published values are its cabled INPUTS.
					// Read those; fall back to OutputNumberData for any legacy source that
					// referenced a real module output.
					if (rm.InputNumberData.TryGetValue(src.OutputId, out Fix32 vi))
					{
						return vi;
					}
					if (rm.OutputNumberData.TryGetValue(src.OutputId, out Fix32 vo))
					{
						return vo;
					}
				}
			}
			else if (src.Kind == BusPinSource.SourceKind.ExternalControllerBus)
			{
				// Resolve by id first; fall back to the remembered name when the id no
				// longer matches (the remote bus's id can change on copy/paste / reorder).
				ControllerBus rb = remote.GetBusById(src.BusId);
				if (rb == null && !string.IsNullOrEmpty(src.BusName) && remote.Buses != null)
				{
					rb = remote.Buses.Find(b => b.Name == src.BusName);
				}
				if (rb != null && src.PinIndex >= 0 && src.PinIndex < ControllerBus.PinCount)
				{
					return rb.PinValues[src.PinIndex];
				}
			}
			return Fix32.Zero;
		}

		public void AddToConfig(EntityConfigData data)
		{
			// TODO copy modules, name, entity connections, ...
			data.Set<StaticEntityProto.ID>("controller_proto", m_protoId, (str, blob) => blob.WriteString(str.Value));
			data.SetArray<Module>("controller_modules", Modules.ToImmutableArray(), Module.Serialize);
			// Module positions are carried on the modules themselves since MODULE_LAYOUT_INFO,
			// so no separate "controller_rows" entry is needed.
			data.SetInt("controller_speed", DelayBetweenTicks);
			data.SetInt("color", (int)Color.Rgba);
			// Persist the user-supplied description across blueprints/clones.  The
			// auto-appended module list is recomputed on display from live state, so
			// we only write the user's prefix here.
			if (CustomDescription.HasValue) {
				data.SetString("controller_description", CustomDescription.Value);
			}

			// Variable buses (names, pin names + latched values, per-pin sources).
			// External pin sources are re-validated by distance on ApplyConfig.  Use the
			// INLINE config serializer (not ControllerBus.Serialize) — the deferred
			// class-ref path doesn't round-trip pin names through the clone/blueprint
			// config; the inline form mirrors how entity-field / string-list data is stored.
			data.SetArray<ControllerBus>("controller_buses", Buses.ToImmutableArray(), ControllerBus.WriteConfig);
		}

		public void ApplyConfig(EntityConfigData data)
		{
			// TODO copy modules, name, entity connections ...
			// TODO validate connection and rotate connection when possible
			var newProto = data.Get("controller_proto", (blob) => new StaticEntityProto.ID(blob.ReadString()));
			if (newProto != m_protoId) {
				// TODO play sound
				return;
			}

			var newModules = data.GetArray("controller_modules", Module.Deserialize);
			if (newModules != null)
			{
				this.Modules.Clear();
				this.Modules.AddRange(newModules?.AsEnumerable());
				foreach (Module module in this.Modules)
				{
					module.Context = Context;
					module.Controller = this;
					module.initContexts(-1);
					foreach (IField field in module.Prototype.Fields)
					{
						field.Validate(module);
					}
				}
				InvalidateTopology();
			}
			// Legacy clones may still have "controller_rows"; back-fill module positions if so.
			ImmutableArray<Lyst<ModulePlacement>>? newLocation =
				data.GetArray("controller_rows", Lyst<ModulePlacement>.Deserialize);
			if (newLocation.HasValue)
			{
				MigrateLegacyRowsIntoModules(newLocation.Value.AsEnumerable());
			}
			var newSpeed = data.GetInt("controller_speed");
			if (newSpeed != null)
			{
				this.DelayBetweenTicks = newSpeed.Value;
			}

			Option<string> savedDescription = data.GetString("controller_description");
			if (savedDescription.HasValue)
			{
				CustomDescription = savedDescription;
			}

			int? color = data.GetInt("color");
			if (color != null)
			{
				this.Color = (uint)color.Value;
			}

			// Variable buses.  Copy them in, then drop any external pin source whose
			// target controller is gone or out of range relative to THIS (pasted)
			// controller's position.  Older blueprints have no entry — leave Buses as-is.
			ImmutableArray<ControllerBus>? newBuses = data.GetArray("controller_buses", ControllerBus.ReadConfig);
			if (newBuses.HasValue)
			{
				Buses = new Lyst<ControllerBus>();
				Buses.AddRange(newBuses.Value.AsEnumerable());
				validateBusExternalSources();
			}
		}

		// Maximum tile distance between two controllers for a bus pin's external
		// source (another controller's module or bus) to survive a clone/blueprint
		// paste.  Out-of-range or missing targets are dropped on ApplyConfig so a
		// pasted blueprint can't silently re-link to a far-away or vanished
		// controller.  Tunable.
		private static readonly Fix32 MAX_BUS_LINK_DISTANCE = Fix32.FromInt(40);

		// Drops external bus-pin sources that no longer resolve to an in-range
		// controller.  Local-module and network-variable sources are left untouched
		// (local module ids travel with the blueprint; network names are global).
		private void validateBusExternalSources()
		{
			if (Buses == null)
			{
				return;
			}
			IEntitiesManager entities = Resolver.Resolve<IEntitiesManager>();
			foreach (ControllerBus bus in Buses)
			{
				for (int i = 0; i < ControllerBus.PinCount; i++)
				{
					BusPinSource source = bus.PinSources[i];
					if (source != null && source.IsExternalController
						&& !isExternalControllerInRange(entities, source.ControllerId))
					{
						bus.PinSources[i] = null;
					}
				}
			}
		}

		private bool isExternalControllerInRange(IEntitiesManager entities, EntityId otherId)
		{
			if (!entities.TryGetEntity(otherId, out Controller other) || other == this)
			{
				return false;
			}
			return (Position3f - other.Position3f).Length <= MAX_BUS_LINK_DISTANCE;
		}

		// Public range check used by the bus-pin Controller-source picker to list only
		// controllers a pin may legally link to (excludes self).
		public bool IsInBusLinkRange(Controller other)
		{
			return other != null
				&& other != this
				&& (Position3f - other.Position3f).Length <= MAX_BUS_LINK_DISTANCE;
		}

		private void MigrateLegacyRowsIntoModules(IEnumerable<Lyst<ModulePlacement>> legacyRows)
		{
			var moduleById = new Dictionary<long, Module>();
			foreach (var m in Modules)
			{
				moduleById[m.Id] = m;
			}
			int rowIdx = 0;
			foreach (var row in legacyRows)
			{
				for (int col = 0; col < row.Count; col++)
				{
					var p = row[col];
					if (p.Placement && p.ModuleId != 0 && moduleById.TryGetValue(p.ModuleId, out var m))
					{
						m.Row = rowIdx;
						m.Column = col;
					}
				}
				rowIdx++;
			}
		}

		public static void Serialize(Controller value, BlobWriter writer)
		{
			if (writer.TryStartClassSerialization(value))
			{
				writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
			}
		}

		public static Controller Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out Controller value, (Func<BlobReader, Type, Controller>)null))
			{
				reader.EnqueueDataDeserialization(value, s_deserializeDataDelayedAction);
			}
			return value;
		}

		[InitAfterLoad(InitPriority.Normal)]
		[OnlyForSaveCompatibility(null)]
		private void initContexts(int saveVersion, DependencyResolver resolver)
		{
			Resolver = resolver;
			Log.Info($"Initialize context after load");

			Prototype = Context.ProtosDb.Get<ControllerProto>(m_protoId).ValueOrThrow("Invalid controller proto: " + m_protoId);
			m_electricConsumer = m_electricConsumer ?? Context.ElectricityConsumerFactory.CreateConsumer(this);
			m_computingConsumer = m_computingConsumer ?? Context.ComputingConsumerFactory.CreateConsumer(this);

			m_notificationInfoManager = WithId(ControllerNotification.InfoNotification, m_notificationInfoManager);
			m_notificationWarningManager = WithId(ControllerNotification.WarningNotification, m_notificationWarningManager);
			m_notificationErrorManager = WithId(ControllerNotification.ErrorNotification, m_notificationErrorManager);

			if (Modules == null)
			{
				Modules = new Lyst<Module>();
			}
			else
			{
				// Pre-v6 saves had no persisted module-id pool counter.  Seed the legacy
				// per-controller field from the highest existing module id (kept on
				// disk for round-trip compatibility).
				if (m_nextModuleId == 0)
				{
					long maxId = 0;
					foreach (var m in Modules)
					{
						if (m.Id > maxId) {
							maxId = m.Id;
						}
					}
					m_nextModuleId = maxId;
				}

				// Seed the live ModuleIdManager from every module loaded on this
				// controller.  Saves predating the manager have m_nextId = 0 in
				// its blob, so the very first Allocate would otherwise return 1
				// — colliding with every legacy module that already has id 1 from
				// the pre-manager per-controller pool.  Instance is lazy-initialized
				// so it's guaranteed non-null, and reaching the manager through it
				// avoids the GlobalDependencyResolver.Get<>() failure mode that
				// previously aborted the whole load and left modules with null
				// Prototype.
				ModuleIdManager idManager = resolver.Resolve<ModuleIdManager>();
				foreach (var m in Modules)
				{
					idManager.EnsureAtLeast(m.Id);
				}

				// Resolve prototypes for every module.  Phantom modules (proto removed and
				// no Deprecation replacement) are KEPT as visible tombstones — the player
				// sees a "!!" cell with an "Original prototype no longer exists" tooltip
				// and can decide whether to delete it manually.  We just skip field
				// validation for them since Phantom carries no fields.
				foreach (var m in Modules)
				{
					m.Controller = this;
					m.Context = Context;
					m.initContexts(saveVersion);

					if (m.Prototype == null) {
						Log.Warning($"Module {m.Id} with null prototype found in controller {Id}");
						continue;
					}
					if (m.Prototype == ModuleProto.Phantom) {
						Log.Warning($"Module {m.Id} resolved to Phantom in controller {Id}; original prototype no longer exists");
						continue;
					}

					foreach (IField field in m.Prototype.Fields)
					{
						field.Validate(m);
					}
				}

				// Reattach connections whose endpoints renamed pins in a Deprecation
				// entry, then drop the ones that are still unresolvable.  Without this,
				// cable rendering tries to look up pin protos on Phantom (no Inputs/
				// Outputs) or hits stale references when a mod author renamed/removed a
				// pin in a new version.  Death conditions:
				//   - source module no longer exists,
				//   - source module exists but its prototype has no such output id (after remap),
				//   - this module's prototype has no such input id,
				//   - either side is a Phantom (no pins by definition).
				var moduleById = new Dictionary<long, Module>();
				foreach (var m in Modules) { moduleById[m.Id] = m; }

				// Cache the OutputIdMap for each module that was migrated and carries one,
				// so we don't re-resolve the migration entry per consumer connection.
				var outputRemapBySource = new Dictionary<long, IReadOnlyDictionary<string, string>>();
				foreach (var m in Modules)
				{
					var mig = Deprecation.GetMigration(new ModuleProto.ID(m.OriginalProtoId));
					if (mig.HasValue && mig.Value.AppliesAt(m.LoadedVersion) && mig.Value.OutputIdMap != null)
					{
						outputRemapBySource[m.Id] = mig.Value.OutputIdMap;
					}
				}

				foreach (var m in Modules)
				{
					if (m.Prototype == null) {
						continue;
					}
					foreach (var kv in m.InputModules.ToList())
					{
						// Bus connections reference a BUS by id (ModuleId = bus id), not a
						// module, so they must be validated against the bus list + pin range
						// — not moduleById, which would otherwise strip every bus→input cable
						// on load (leaving the bus pins disconnected).
						if (kv.Value.IsBus)
						{
							bool busOk = GetBusById(kv.Value.ModuleId) != null
								&& kv.Value.TryGetPinIndex(out int busPin)
								&& busPin >= 0 && busPin < ControllerBus.PinCount
								&& m.HasInput(kv.Key);
							if (!busOk)
							{
								m.InputModules.Remove(kv.Key);
							}
							continue;
						}
						if (!moduleById.TryGetValue(kv.Value.ModuleId, out var src))
						{
							m.InputModules.Remove(kv.Key);
							continue;
						}
						if (src.Prototype == null || src.Prototype == ModuleProto.Phantom)
						{
							m.InputModules.Remove(kv.Key);
							continue;
						}
						if (m.Prototype == ModuleProto.Phantom)
						{
							m.InputModules.Remove(kv.Key);
							continue;
						}

						// Apply the source's output rename before validating, so cables
						// drawn from a renamed output pin reattach to the new id instead
						// of being dropped as orphans.  Only the OutputId on the connector
						// changes; ModuleId stays the same.
						var connector = kv.Value;
						if (outputRemapBySource.TryGetValue(src.Id, out var outMap)
							&& outMap.TryGetValue(connector.OutputId, out string newOutputId)
							&& !string.Equals(connector.OutputId, newOutputId, StringComparison.Ordinal))
						{
							connector = new ModuleConnector(connector.ModuleId, newOutputId);
							m.InputModules[kv.Key] = connector;
							Log.Info($"Module {m.Id}: output pin remapped on connection '{kv.Key}' — source {src.Id} '{kv.Value.OutputId}' -> '{newOutputId}' via Deprecation");
						}

						if (!src.HasOutput(connector.OutputId))
						{
							Log.Warning($"Module {m.Id}: dropping connection for input '{kv.Key}' — source module {src.Id} no longer has output '{connector.OutputId}'");
							m.InputModules.Remove(kv.Key);
							continue;
						}
						if (!m.HasInput(kv.Key))
						{
							Log.Warning($"Module {m.Id}: dropping connection — input '{kv.Key}' no longer exists on this module's prototype");
							m.InputModules.Remove(kv.Key);
						}
					}
				}
				InvalidateTopology();
			}

			if (m_legacyRows != null)
			{
				MigrateLegacyRowsIntoModules(m_legacyRows);
				m_legacyRows = null;
			}

			if (Color == ColorRgba.Empty)
			{
				Color = m_proto.DefaultColor;
			}

			m_productMaintenanceT1 = Context.ProtosDb.GetOrThrow<VirtualProductProto>(Ids.Products.MaintenanceT1);
			m_productMaintenanceT2 = Context.ProtosDb.GetOrThrow<VirtualProductProto>(Ids.Products.MaintenanceT2);
			m_productMaintenanceT3 = Context.ProtosDb.GetOrThrow<VirtualProductProto>(Ids.Products.MaintenanceT3);
		}

		private EntityNotificator WithId(EntityNotificationProto.ID newNotification, EntityNotificator notification)
		{
			if (!m_reninitNotification)
			{
				return Context.NotificationsManager.CreateNotificatorFor(newNotification);
			}

			PropertyInfo field = typeof(EntityNotificator).GetProperty("NotificationId");
			object v = Context.NotificationsManager.CreateNotificatorFor(newNotification);
			field.SetValue(v, notification.NotificationId);
			return (EntityNotificator)v;
		}

		protected override void SerializeData(BlobWriter writer)
		{
			base.SerializeData(writer);
			writer.WriteString(m_protoId.Value);
			writer.WriteInt(/*Version*/ CONTROLLER_VARIABLE_BUS);

			writer.WriteString(ErrorMessage ?? "");
			Option<string>.Serialize(CustomTitle, writer);
			ColorRgba.Serialize(Color, writer);

			writer.WriteInt(GeneralPriority);
			writer.WriteGeneric(m_maintenanceConsumer);
			writer.WriteGeneric(m_electricConsumer);
			writer.WriteGeneric(m_computingConsumer);

			writer.WriteUInt(m_notificationInfoManager.NotificationId.Value);
			writer.WriteUInt(m_notificationWarningManager.NotificationId.Value);
			writer.WriteUInt(m_notificationErrorManager.NotificationId.Value);

			writer.WriteInt(m_clockSpeed);
			writer.WriteInt(m_clock);

			Lyst<Module>.Serialize(Modules, writer);
			// Layout grid (Rows) was dropped at MODULE_LAYOUT_INFO; positions live on each Module now.

			// CONTROLLER_DESCRIPTION (v5+): player-writable description.  Empty Option
			// is the safe default for old saves loaded back through the v<5 branch.
			Option<string>.Serialize(CustomDescription, writer);

			// CONTROLLER_MODULE_ID_POOL (v6+): per-controller monotonic module-id
			// counter.  Old saves load with 0 here and the post-load init seeds it
			// from max(existing module ids) before any AllocateModuleId call.
			writer.WriteLong(m_nextModuleId);

			// CONTROLLER_VARIABLE_BUS (v7+): variable buses, appended LAST so a
			// pre-v7 reader (which stops after the long above) is byte-aligned.
			// Uses the same Lyst<T> class-serialization path as Modules above.
			Lyst<ControllerBus>.Serialize(Buses, writer);
		}

		protected override void DeserializeData(BlobReader reader)
		{
			base.DeserializeData(reader);
			m_protoId = new Mafi.Core.Entities.Static.StaticEntityProto.ID(reader.ReadString());
			int version = reader.ReadInt();

			CurrentInstruction = 0;

			ErrorMessage = reader.ReadString();
			CustomTitle = Option<string>.Deserialize(reader);
			if (version < 3)
			{
				Color = ColorRgba.Empty;
			}
			else
			{
				Color = ColorRgba.Deserialize(reader);
			}

			GeneralPriority = reader.ReadInt();
			m_maintenanceConsumer = reader.ReadGenericAs<IEntityMaintenanceProvider>();

			if (version >= 1)
			{
				m_electricConsumer = reader.ReadGenericAs<IElectricityConsumer>();
				m_computingConsumer = reader.ReadGenericAs<IComputingConsumer>();

				m_reninitNotification = true;

				object v = m_notificationInfoManager = new EntityNotificator();
				typeof(EntityNotificator).GetProperty("NotificationId").SetValue(v, new NotificationId(reader.ReadUInt()));
				m_notificationInfoManager = (EntityNotificator)v;

				v = m_notificationWarningManager = new EntityNotificator();
				typeof(EntityNotificator).GetProperty("NotificationId").SetValue(v, new NotificationId(reader.ReadUInt()));
				m_notificationWarningManager = (EntityNotificator)v;

				v = m_notificationErrorManager = new EntityNotificator();
				typeof(EntityNotificator).GetProperty("NotificationId").SetValue(v, new NotificationId(reader.ReadUInt()));
				m_notificationErrorManager = (EntityNotificator)v;
			}

			if (version >= 2)
			{
				m_clockSpeed = reader.ReadInt();
				m_clock   = reader.ReadInt();
			}
			else
			{
				m_clockSpeed = 0;
				m_clock   = 0;
			}

			Modules = Lyst<Module>.Deserialize(reader);
			if (version < MODULE_LAYOUT_INFO)
			{
				// Legacy save: positions live in the controller's grid. Stash and back-fill
				// into modules in initContexts (after prototypes are resolved).
				m_legacyRows = Lyst<Lyst<ModulePlacement>>.Deserialize(reader);
			}

			// CONTROLLER_DESCRIPTION (v5+): player-writable description string.
			// Pre-v5 saves had no field — leave CustomDescription = None.
			if (version >= CONTROLLER_DESCRIPTION)
			{
				CustomDescription = Option<string>.Deserialize(reader);
			}

			// CONTROLLER_MODULE_ID_POOL (v6+): persisted module-id pool counter.
			// Pre-v6 saves don't carry it; initContexts seeds it from max(existing
			// module ids) so the next AllocateModuleId can't collide with legacy
			// time-based ids that came along for the ride.
			if (version >= CONTROLLER_MODULE_ID_POOL)
			{
				m_nextModuleId = reader.ReadLong();
			}
			else
			{
				m_nextModuleId = 0;
			}

			// CONTROLLER_VARIABLE_BUS (v7+): per-controller variable buses.  Pre-v7
			// saves carry nothing here — load an empty list.  v7+ uses the same
			// Lyst<T> class-deserialization path as Modules above.
			if (version >= CONTROLLER_VARIABLE_BUS)
			{
				Buses = Lyst<ControllerBus>.Deserialize(reader);
			}
			else
			{
				Buses = new Lyst<ControllerBus>();
			}

			Log.Info($"Deserialized with {Modules.Count} modules" +
				(m_legacyRows != null ? $" + {m_legacyRows.Count} legacy rows (will migrate)" : ""));
			reader.RegisterInitAfterLoad(this, nameof(initContexts), InitPriority.Normal);
		}

		[DoNotSave(0, null)]
		public Upoints MonthlyUnityConsumed => 0.Upoints();

		[DoNotSave(0, null)]
		public Upoints MaxMonthlyUnityConsumed => 0.Upoints();

		public Proto.ID UpointsCategoryId => IdsCore.UpointsCategories.Boost;

		[DoNotSave(0, null)]
		public Option<UnityConsumer> UnityConsumer => m_unityConsumer;
		[DoNotSave(0, null)]
		private UnityConsumer m_unityConsumer;

		[DoNotSave(0, null)]
		public int CurrentInstruction { get; private set; }

		[DoNotSave(0, null)]
		public Electricity PowerRequired { get; private set; } = Electricity.Zero;

		[DoNotSave(0, null)]
		public Option<IElectricityConsumerReadonly> ElectricityConsumer => ((IElectricityConsumerReadonly)m_electricConsumer).SomeOption();
		[DoNotSave(0, null)]
		private IElectricityConsumer m_electricConsumer;

		[DoNotSave(0, null)]
		public Computing ComputingRequired { get; private set; } = Computing.Zero;
		[DoNotSave(0, null)]
		public Option<IComputingConsumerReadonly> ComputingConsumer => ((IComputingConsumerReadonly)m_computingConsumer).SomeOption();

		[DoNotSave(0, null)]
		private IComputingConsumer m_computingConsumer;
		[DoNotSave(0, null)]
		private EntityNotificator m_notificationInfoManager;
		[DoNotSave(0, null)]
		private EntityNotificator m_notificationErrorManager;
		[DoNotSave(0, null)]
		private EntityNotificator m_notificationWarningManager;

		public MaintenanceCosts MaintenanceCosts { get; private set; }

		[DoNotSave(0, null)]
		public IEntityMaintenanceProvider Maintenance => m_maintenanceConsumer;
		[DoNotSave(0, null)]
		private IEntityMaintenanceProvider m_maintenanceConsumer;
		[DoNotSave(0, null)]
		private bool m_reninitNotification;

		private int m_clockSpeed;
		private int m_clock;

		[DoNotSave(0, null)]
		public bool IsIdleForMaintenance => m_maintenanceConsumer.Status.IsBroken;

		[DoNotSave(0, null)]
		public string ErrorMessage { get; private set; }

		[DoNotSave(0, null)]
		public bool IsDebug { get; private set; }

		[DoNotSave(0, null)]
		public bool WaitForUser { get; private set; }

		[DoNotSave(0, null)]
		private VirtualProductProto m_productMaintenanceT1;
		[DoNotSave(0, null)]
		private VirtualProductProto m_productMaintenanceT2;
		[DoNotSave(0, null)]
		private VirtualProductProto m_productMaintenanceT3;

		// TODO handle computation speed quicker than ticks
		public void SimUpdate()
		{
			if (IsNotEnabled && IsNotPaused)
			{
				return;
			}

			if (Modules.Count == 0)
			{
				PowerRequired = Prototype.IddlePower;
				m_electricConsumer.OnPowerRequiredChanged();
				ComputingRequired = Computing.Zero;
				m_computingConsumer.OnComputingRequiredChanged();
				m_notificationErrorManager.Deactivate(this);
				m_notificationWarningManager.Deactivate(this);
				m_notificationInfoManager.Deactivate(this);
				return;
			}

			if (IsPaused) {
				return;
			}

			Electricity requiredRunningPower = GetRequiredRunningPower();
			PowerRequired = Prototype.IddlePower + requiredRunningPower;
			m_electricConsumer.OnPowerRequiredChanged();

			Computing requiredComputingPower = GetRequiredComputation();
			ComputingRequired = requiredComputingPower;
			m_computingConsumer.OnComputingRequiredChanged();

			float slotsMaintenance = 0.01f / Prototype.Columns;
			PartialQuantity quantity = new PartialQuantity(
					(Modules.Select(m => m.Layout.GetWidth(m)).Sum() * slotsMaintenance + 0.01f).ToFix32());

			VirtualProductProto lastMaintenanceProduct = Maintenance.Costs.Product;
			VirtualProductProto maintenanceProduct
				= ComputingRequired > 10.TFlops()
				? m_productMaintenanceT3
				: ComputingRequired.IsPositive
				? m_productMaintenanceT2
				: m_productMaintenanceT1;
			var newCosts = new MaintenanceCosts(maintenanceProduct, quantity);
			if (newCosts.MaintenancePerMonth != MaintenanceCosts.MaintenancePerMonth
				|| newCosts.Product != lastMaintenanceProduct)
			{
				MaintenanceCosts = newCosts;
				Maintenance.RefreshMaintenanceCost();
			}

			bool electricityConsumed = m_electricConsumer.TryConsume();

			bool computingConsumed = electricityConsumed && m_computingConsumer.TryConsume();

			if (m_clock >= m_clockSpeed)
			{
				m_clock = 0;
			}
			else
			{
				m_clock++;
				return;
			}

			if (electricityConsumed)
			{
				if (m_maintenanceConsumer.Status.CurrentBreakdownChance < new Random().Next(100).Percent())
				{
					UpdateModules(computingConsumed);
				}
			}
			else
			{
				State = Tr.EntityElectricityConsumptionTooltip__NotEnough;
			}
		}

		private Electricity GetRequiredRunningPower()
		{
			var total = Modules
				.ToArray()
				.Where(m => m.IsNotPaused())
				.Where(m => !(m.Prototype is null))
				.Select(m => m.Prototype.UsedPower.Value)
				.Sum();

			if (DelayBetweenTicks == 0) {
				return total.Kw();
			}

			return (total * 1 / (1 + DelayBetweenTicks)).Max(1).Kw();
		}

		private Computing GetRequiredComputation()
		{
			PartialQuantity sum = PartialQuantity.Zero;
			foreach (Module module in Modules)
			{
				if (module.IsPaused) {
					continue;
				}
				// DynamicComputing wins over the static UsedComputing when set
				// (PLC modules: cost = 1 + 0.05 × parsed-node-count).  Falling
				// back to UsedComputing keeps every other module unchanged.
				sum += module.Prototype.DynamicComputing != null
					? module.Prototype.DynamicComputing(module)
					: module.Prototype.UsedComputing;
			}

			if (sum == PartialQuantity.Zero) {
				return Computing.Zero;
			}
			sum = (sum * 1 / (1 + DelayBetweenTicks)).Max(PartialQuantity.One);
			return Computing.FromQuantity(sum.IntegerPart.Max(Quantity.One));
		}

		private void UpdateModules(bool computingConsumed)
		{
			// The "compiled tree" — module/connection topology only changes on
			// user edits, so the per-module-id lookup and the per-cable resolution
			// are hoisted into m_copyPlan / m_inputClearPlan and rebuilt lazily
			// via BuildSignalPlan() after InvalidateTopology().
			if (m_topologyDirty || m_copyPlan == null) {
				BuildSignalPlan();
			}

			// Clear unconnected inputs to zero (connected inputs are unconditionally
			// overwritten below, no point clearing them first).
			var clears = m_inputClearPlan;
			for (int i = 0; i < clears.Length; i++)
			{
				Dict<string, Fix32> dict = clears[i].Dict;
				string[] keys = clears[i].Keys;
				for (int k = 0; k < keys.Length; k++)
				{
					dict.TryRemove(keys[k], out _);
				}
			}

			// Run the pre-compiled copy plan.  Two dict ops per cable (1 read,
			// 1 write) — no module-by-id lookup, no per-tick ToArray on InputModules.
			CopyEdge[] plan = m_copyPlan;
			for (int i = 0; i < plan.Length; i++)
			{
				CopyEdge e = plan[i];
				e.DstInputs[e.DstKey] = e.SrcOutputs.TryGetValue(e.SrcKey, out Fix32 v)
					? v : Fix32.Zero;
			}

			// --- Variable bus dataflow (Input-type pins) -----------------------------
			// 1) Input bus pins reset each tick (matches module-input clearing).
			BusPinRef[] busClears = m_busClearPlan;
			if (busClears != null)
			{
				for (int i = 0; i < busClears.Length; i++)
				{
					busClears[i].Bus.PinValues[busClears[i].PinIdx] = Fix32.Zero;
				}
			}
			// 2) Feed Input bus pins from their module-output source (last tick's
			//    outputs, same as CopyEdge — so a module→bus→module hop is 1-tick).
			BusFeedEdge[] busFeed = m_busFeedPlan;
			if (busFeed != null)
			{
				for (int i = 0; i < busFeed.Length; i++)
				{
					BusFeedEdge e = busFeed[i];
					e.Bus.PinValues[e.PinIdx] = e.SrcOutputs.TryGetValue(e.SrcKey, out Fix32 bv)
						? bv : Fix32.Zero;
				}
			}
			// 2b) Controller-type pins read a specific pin on another (or this)
			//     controller — resolved LIVE each tick since the remote can change
			//     independently of this controller's topology.  Latches to 0 when the
			//     reference is missing or out of range.
			updateControllerPins();
			// 3) Copy bus pin values into the module inputs that read them.
			BusReadEdge[] busRead = m_busReadPlan;
			if (busRead != null)
			{
				for (int i = 0; i < busRead.Length; i++)
				{
					BusReadEdge e = busRead[i];
					e.DstInputs[e.DstKey] = e.Bus.PinValues[e.PinIdx];
				}
			}

			// Execute all modules
			bool anyError = false;
			bool anyWarning = false;
			bool anyInfo = false;
			bool missingComputation = false;
			foreach (Module module in Modules)
			{
				try
				{
					if (module.Unlocked == false) {
						// Only skip, UI will show automatically
						module.SetStatus(ModuleStatus.Skipped);
						continue;
					}
					// Match the GetRequiredComputation path: a module needs computing
					// if its dynamic-cost callback returns >0 OR (when no callback)
					// its static UsedComputing is >0.  Without this, PLC modules
					// (whose cost is always dynamic) would never skip on missing
					// computing because their UsedComputing stays at zero.
					PartialQuantity moduleCost = module.Prototype.DynamicComputing != null
						? module.Prototype.DynamicComputing(module)
						: module.Prototype.UsedComputing;
					if (moduleCost > PartialQuantity.Zero && !computingConsumed)
					{
						missingComputation = missingComputation || true;
						module.SetStatus(ModuleStatus.Skipped);
						continue;
					}

					if (module.Status == ModuleStatus.Skipped) {
						module.SetStatus(ModuleStatus.Init);
					}
					module.Execute();
				}
				catch (Exception e)
				{
					anyError = true;
					if (module.IsDebugging) {
						Log.Exception(e);
					}
					// ignore exception
				}
				anyInfo = anyInfo || module.Info;
				anyWarning = anyWarning || module.Warning;
				anyError = anyError || module.Status == ModuleStatus.Error;
			}
			if (anyError)
			{
				m_notificationErrorManager.Activate(this);
				m_notificationWarningManager.Deactivate(this);
				m_notificationInfoManager.Deactivate(this);
			}
			else if (anyWarning)
			{
				m_notificationErrorManager.Deactivate(this);
				m_notificationWarningManager.Activate(this);
				m_notificationInfoManager.Deactivate(this);
			}
			else if (anyInfo)
			{
				m_notificationErrorManager.Deactivate(this);
				m_notificationWarningManager.Deactivate(this);
				m_notificationInfoManager.Activate(this);
			}
			else
			{
				m_notificationErrorManager.Deactivate(this);
				m_notificationWarningManager.Deactivate(this);
				m_notificationInfoManager.Deactivate(this);
			}

			State = Tr.EntityStatus__Working;
		}

		public Quantity ReceiveAsMuchAsFromPort(ProductQuantity pq, IoPortToken sourcePort)
		{
			return Quantity.Zero; // TODO keep displayed content
		}

		[DoNotSave()]
		public Lyst<Module> Modules { get; private set; }

		// Holds the layout table read from a pre-MODULE_LAYOUT_INFO save until
		// initContexts can back-fill module positions; cleared right after.
		[DoNotSave(0, null)]
		private Lyst<Lyst<ModulePlacement>> m_legacyRows;

		// Computed grid view over Modules' (Row, Column). Recomputed on every read; do not
		// mutate the returned Lyst — it is a snapshot. Kept for line-painting / read-only consumers.
		[DoNotSave()]
		public Lyst<Lyst<ModulePlacement>> Rows
		{
			get
			{
				int rows = Prototype != null ? Prototype.Rows : 0;
				int cols = Prototype != null ? Prototype.Columns : 0;
				var grid = new Lyst<Lyst<ModulePlacement>>();
				for (int i = 0; i < rows; i++)
				{
					var row = new Lyst<ModulePlacement>();
					for (int j = 0; j < cols; j++)
					{
						row.Add(ModulePlacement.Empty);
					}
					grid.Add(row);
				}
				if (Modules == null) {
					return grid;
				}
				foreach (var m in Modules)
				{
					if (m == null || m.Prototype == null) {
						continue;
					}
					int width = m.Layout.GetWidth(m);
					if (m.Row < 0 || m.Row >= grid.Count) {
						continue;
					}
					var row = grid[m.Row];
					for (int x = 0; x < width; x++)
					{
						int c = m.Column + x;
						if (c < 0 || c >= row.Count) {
							continue;
						}
						row[c] = x == 0 ? ModulePlacement.Origin(m.Id) : ModulePlacement.Rest(m.Id);
					}
				}
				return grid;
			}
		}

		[DoNotSave()]
		public int GeneralPriority { get; set; }

		[DoNotSave()]
		public bool IsGeneralPriorityVisible => true;

		[DoNotSave()]
		public bool IsCargoAffectedByGeneralPriority => false;

		[DoNotSave()]
		public int DelayBetweenTicks { get => m_clockSpeed; set => m_clockSpeed = value; }

		[DoNotSave()]
		public int Clock { get => m_clock; set => m_clock = value; }
		public LocStrFormatted State { get; private set; }
		public ColorRgba Color { get; private set; }

		[DoNotSave(resolveAfterLoad: typeof(DependencyResolver))]
		public DependencyResolver Resolver { get; set; }
		public void SetColor(ColorRgba color)
		{
			Color = color;
		}
	}
}
