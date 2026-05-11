using Mafi;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Localization;
using Mafi.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Research;
using ProgramableNetwork.Python;
using UnityEngine;

namespace ProgramableNetwork
{
	[ManuallyWrittenSerialization]
	public partial class Module : IEntity, IEntityWithCloneableConfig
	{
		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction = delegate (object obj, BlobWriter writer)
		{
			((Module)obj).SerializeData(writer);
		};
		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction = delegate (object obj, BlobReader reader)
		{
			((Module)obj).DeserializeData(reader);
		};
		public static void Serialize(Module value, BlobWriter writer)
		{
			if (writer.TryStartClassSerialization(value))
			{
				writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
			}
		}

		public static Module Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out Module obj))
			{
				reader.EnqueueDataDeserialization(obj, s_deserializeDataDelayedAction);
			}

			return obj;
		}

		private ModuleProto m_proto;
		private string m_protoId;
		[DoNotSave(0, null)]
		private string m_originalProtoId;
		private int loadedVersion;

		/// <summary>
		/// The proto id read from the save before any Deprecation swap.  Stays equal to
		/// <c>Prototype.Id.Value</c> for modules that weren't migrated; differs when the
		/// save's id was redirected through <see cref="Deprecation"/>.  Captured once during
		/// <see cref="DeserializeData"/> and never overwritten — the <c>Prototype</c> setter
		/// updates <c>m_protoId</c> but not this field.  Read by the controller's post-load
		/// pass to look up the migration entry whose <c>OutputIdMap</c> may need to be
		/// applied to consumer connectors.
		/// </summary>
		public string OriginalProtoId => m_originalProtoId ?? m_protoId;

		/// <summary>
		/// Save-format version this instance was deserialized at, or 0 for new instances.
		/// Used as the version gate for <see cref="Deprecation.Migration.UntilVersion"/>.
		/// </summary>
		public int LoadedVersion => loadedVersion;

		[DoNotSave]
		private bool m_unlocked = false;
		[DoNotSave]
		private ImmutableArray<ResearchNode> m_researchNodes;

		public Module(ModuleProto prototype, EntityContext context, Controller entity, long id)
		{
			this.Id = id;
			Prototype = prototype;
			Context = context;
			Controller = entity;
			Status = ModuleStatus.Init;
			NumberData = [];
			InputNumberData = [];
			OutputNumberData = [];
			FieldNumberData = [];
			StringData = [];
			ArrayData = [];
			InputModules = [];
		}

		public Controller Controller { get; set; }

		public ModuleStatus Status { get; private set; }
		public string Error { get; private set; } = "";
		public long Id { get; internal set; }

		// Position on the controller grid. Owned by the module since
		// Controller.MODULE_LAYOUT_INFO; controllers no longer store a layout.
		public int Row { get; set; }
		public int Column { get; set; }

		public ModuleProto Prototype {
			get => m_proto;
			set
			{
				m_protoId = value.Id.Value;
				m_proto = value;
			}
		}

		public ModuleConnector this[string id]
		{
			get => HasOutput(id) ? new(Id, id) : throw new ArgumentException($"Missing output '{id}' in module '{Prototype.Id}'");
			set => InputModules[id] = HasInput(id) ? value : throw new ArgumentException($"Missing input '{id}' in module '{Prototype.Id}'");
		}

		// Per-instance pin extension counts.  Static pins from the prototype always
		// come first; extensions [0..InputExtensionCount-1] of Prototype.InputExtensions
		// are appended on the right side via EffectiveInputs.  Same shape for outputs.
		// Persisted in MODULE_EXTENSIONS (v7+).  For non-extensible prototypes these
		// stay 0 and add nothing to width.
		public int InputExtensionCount { get; set; }
		public int OutputExtensionCount { get; set; }
		// Display extension count — grows the rightmost display by N cells when the
		// prototype opted in via AllowDisplayExtensions.  Value-displays (e.g. the
		// collapsed Display_Int) use this to choose precision/width without splitting
		// into separate fixed-arity prototypes.  Persisted in MODULE_DISPLAY_EXTENSIONS
		// (v8+); v7 saves predate it and load with a count of 0.
		public int DisplayExtensionCount { get; set; }

		/// <summary>
		/// Effective input list = prototype's static inputs, followed by the first
		/// <see cref="InputExtensionCount"/> extension entries, followed by any
		/// trailing inputs.  Returns the proto's static list verbatim when nothing
		/// else is active and there are no trailings, to avoid the per-call alloc.
		/// </summary>
		public IReadOnlyList<ModuleConnectorProto> EffectiveInputs
		{
			get
			{
				if (Prototype == null) {
					return (IReadOnlyList<ModuleConnectorProto>)System.Array.Empty<ModuleConnectorProto>();
				}
				int ext = System.Math.Min(InputExtensionCount, Prototype.MaxInputExtensions);
				int trail = Prototype.InputsTrailing?.Count ?? 0;
				if (ext <= 0 && trail == 0) {
					return Prototype.Inputs;
				}
				var list = new System.Collections.Generic.List<ModuleConnectorProto>(Prototype.Inputs.Count + ext + trail);
				list.AddRange(Prototype.Inputs);
				for (int i = 0; i < ext; i++) {
					list.Add(Prototype.InputExtensions[i]);
				}
				for (int i = 0; i < trail; i++) {
					list.Add(Prototype.InputsTrailing[i]);
				}
				return list;
			}
		}

		public IReadOnlyList<ModuleConnectorProto> EffectiveOutputs
		{
			get
			{
				int ext = System.Math.Min(OutputExtensionCount, Prototype?.MaxOutputExtensions ?? 0);
				if (ext <= 0 || Prototype == null) {
					return Prototype?.Outputs ?? (IReadOnlyList<ModuleConnectorProto>)System.Array.Empty<ModuleConnectorProto>();
				}
				var list = new System.Collections.Generic.List<ModuleConnectorProto>(Prototype.Outputs.Count + ext);
				list.AddRange(Prototype.Outputs);
				for (int i = 0; i < ext; i++) {
					list.Add(Prototype.OutputExtensions[i]);
				}
				return list;
			}
		}

		public bool HasInput(string id)
		{
			if (Prototype == null) {
				return false;
			}
			foreach (var p in Prototype.Inputs) {
				if (p.Id == id) {
					return true;
				}
			}
			int ext = System.Math.Min(InputExtensionCount, Prototype.MaxInputExtensions);
			for (int i = 0; i < ext; i++) {
				if (Prototype.InputExtensions[i].Id == id) {
					return true;
				}
			}
			if (Prototype.InputsTrailing != null) {
				foreach (var p in Prototype.InputsTrailing) {
					if (p.Id == id) {
						return true;
					}
				}
			}
			return false;
		}

		public bool HasOutput(string id)
		{
			if (Prototype == null) {
				return false;
			}
			foreach (var p in Prototype.Outputs) {
				if (p.Id == id) {
					return true;
				}
			}
			int ext = System.Math.Min(OutputExtensionCount, Prototype.MaxOutputExtensions);
			for (int i = 0; i < ext; i++) {
				if (Prototype.OutputExtensions[i].Id == id) {
					return true;
				}
			}
			return false;
		}

		/// <summary>Resolves an input pin by id, searching static, active extension, and trailing entries; null when missing.</summary>
		public ModuleConnectorProto GetInputProto(string id)
		{
			if (Prototype == null) {
				return null;
			}
			foreach (var p in Prototype.Inputs) {
				if (p.Id == id) {
					return p;
				}
			}
			int ext = System.Math.Min(InputExtensionCount, Prototype.MaxInputExtensions);
			for (int i = 0; i < ext; i++) {
				if (Prototype.InputExtensions[i].Id == id) {
					return Prototype.InputExtensions[i];
				}
			}
			if (Prototype.InputsTrailing != null) {
				foreach (var p in Prototype.InputsTrailing) {
					if (p.Id == id) {
						return p;
					}
				}
			}
			return null;
		}

		public ModuleConnectorProto GetOutputProto(string id)
		{
			if (Prototype == null) {
				return null;
			}
			foreach (var p in Prototype.Outputs) {
				if (p.Id == id) {
					return p;
				}
			}
			int ext = System.Math.Min(OutputExtensionCount, Prototype.MaxOutputExtensions);
			for (int i = 0; i < ext; i++) {
				if (Prototype.OutputExtensions[i].Id == id) {
					return Prototype.OutputExtensions[i];
				}
			}
			return null;
		}

		/// <summary>
		/// Absolute grid column where the given pin renders.  Mirrors ModuleView.AddInputs/
		/// AddOutputs: statics are right-aligned within the prototype's baseWidth (so their
		/// columns stay stable when extensions widen the module), then extensions are
		/// placed at columns baseWidth, baseWidth+1, ...  Returns -1 when the pin is not
		/// found among statics or active extensions.
		/// </summary>
		public int GetPinColumn(string pinId, bool isOutput)
		{
			if (Prototype == null) {
				return -1;
			}
			var statics = isOutput ? Prototype.Outputs : Prototype.Inputs;
			var exts    = isOutput ? Prototype.OutputExtensions : Prototype.InputExtensions;
			// Trailings only exist on the input side currently.  When pinning an
			// output, treat the trailing list as empty.
			var trailings = isOutput
				? (System.Collections.Generic.IReadOnlyList<ModuleConnectorProto>)System.Array.Empty<ModuleConnectorProto>()
				: (Prototype.InputsTrailing ?? (System.Collections.Generic.IReadOnlyList<ModuleConnectorProto>)System.Array.Empty<ModuleConnectorProto>());
			int extCount = isOutput
				? System.Math.Min(OutputExtensionCount, Prototype.MaxOutputExtensions)
				: System.Math.Min(InputExtensionCount, Prototype.MaxInputExtensions);
			int baseWidth = Layout.GetBaseWidth(this);
			// innerFiller right-aligns the static block within whatever baseWidth
			// claims after trailings reserve their own cells at the far right.
			int innerFiller = System.Math.Max(0, baseWidth - statics.Count - trailings.Count);

			for (int i = 0; i < statics.Count; i++) {
				if (statics[i].Id == pinId) {
					return Column + innerFiller + i;
				}
			}
			for (int i = 0; i < extCount; i++) {
				if (exts[i].Id == pinId) {
					return Column + innerFiller + statics.Count + i;
				}
			}
			// Trailing pin position depends only on THIS row's own extension count
			// (statics + extensions), not on the module's total width.  Display or
			// output extensions can widen the module past the active input cells,
			// but the trailing input must stay glued to the end of the active
			// input area — extra width is absorbed by outerFiller to its right.
			for (int i = 0; i < trailings.Count; i++) {
				if (trailings[i].Id == pinId) {
					return Column + innerFiller + statics.Count + extCount + i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Sets the extension count for the input or output side, clamped to
		/// [0, MaxXxxExtensions].  Returns the actual new count.  Drops any cable
		/// whose pin id falls outside the new range so the controller stays in a
		/// consistent state — caller does not need to reconcile InputModules.
		/// </summary>
		public int SetInputExtensionCount(int newCount)
		{
			int max = Prototype?.MaxInputExtensions ?? 0;
			newCount = System.Math.Max(0, System.Math.Min(newCount, max));
			InputExtensionCount = newCount;
			pruneInvalidInputCables();
			return newCount;
		}

		public int SetDisplayExtensionCount(int newCount)
		{
			int max = Prototype?.MaxDisplayExtensions ?? 0;
			newCount = System.Math.Max(0, System.Math.Min(newCount, max));
			DisplayExtensionCount = newCount;
			return newCount;
		}

		/// <summary>
		/// Sets <paramref name="side"/>'s extension count and, when the prototype opted
		/// in via <see cref="ModuleProto.LinkInputOutputExtensions"/>, mirrors the same
		/// count to the other I/O side too.  Use this from the command executor instead
		/// of <see cref="SetInputExtensionCount"/> / <see cref="SetOutputExtensionCount"/>
		/// directly so the linked sides can never drift apart from a single +/- click.
		/// Display side is always treated independently — display lock-step is a
		/// separate concept driven by <see cref="ModuleProto.ExtensionDisplaysLinkedSide"/>.
		/// Returns the actual new count (after each side's own clamp).
		/// </summary>
		public int SetExtensionCountLinked(ExtensionSide side, int newCount)
		{
			int applied;
			switch (side)
			{
				case ExtensionSide.Input:
					applied = SetInputExtensionCount(newCount);
					if (Prototype != null && Prototype.LinkInputOutputExtensions) {
						SetOutputExtensionCount(applied);
					}
					return applied;
				case ExtensionSide.Output:
					applied = SetOutputExtensionCount(newCount);
					if (Prototype != null && Prototype.LinkInputOutputExtensions) {
						SetInputExtensionCount(applied);
					}
					return applied;
				case ExtensionSide.Display:
				default:
					return SetDisplayExtensionCount(newCount);
			}
		}

		public int SetOutputExtensionCount(int newCount)
		{
			int max = Prototype?.MaxOutputExtensions ?? 0;
			newCount = System.Math.Max(0, System.Math.Min(newCount, max));
			OutputExtensionCount = newCount;
			// An output going away invalidates inbound cables on OTHER modules — the
			// controller-wide initContexts pass already strips dangling targets, but
			// at runtime we need to clean up here too.  Walk every module on this
			// controller's set and drop connections whose source is this module on a
			// pin that no longer exists.
			if (Controller != null)
			{
				foreach (Module m in Controller.Modules)
				{
					foreach (var kv in m.InputModules.ToList())
					{
						if (kv.Value.ModuleId == Id && !HasOutput(kv.Value.OutputId)) {
							m.InputModules.Remove(kv.Key);
						}
					}
				}
			}
			return newCount;
		}

		private void pruneInvalidInputCables()
		{
			foreach (var kv in InputModules.ToList())
			{
				if (!HasInput(kv.Key)) {
					InputModules.Remove(kv.Key);
				}
			}
		}

		public EntityContext Context { get; set; }
		public ModuleLayout Layout => new ModuleLayout(Prototype);

		public void Execute()
		{
			try
			{
				Status = Prototype.Action(this);
				if (Status != ModuleStatus.Error)
				{
					Error = "";
				}
			}
			catch (System.Exception e)
			{
				Error = e.Message;
				Status = ModuleStatus.Error;
				//if (IsDebugging)
					Debug.LogError(e);
			}
		}

		/// <summary>
		/// Resets all volatile data
		/// </summary>
		public void Reset()
		{
			Error = "";
			Warning = false;
			Info = false;
			Status = ModuleStatus.Init;
		}

		[DoNotSave(0, null)]
		public Dict<string, ModuleConnector> InputModules { get; private set; } // todo get by module id, cached

		// MODULE_COMPACT_DATA: bitmask describing which optional containers carry
		// content for this module instance.  An "absent" flag means the matching
		// dict/array was empty and nothing was written for it — read-back paths
		// substitute a fresh empty container.  Packed into a single byte so most
		// modules (which use only one or two of these) shrink their on-disk size
		// from a header-per-empty-dict down to one bit per slot.
		//
		// CodeMetadata (bit 7) was added in MODULE_PYTHON_CODE to persist the
		// PLC module's cached lexer-node count.  Non-PLC modules never set it
		// and pay nothing for the extension.
		[Flags]
		private enum DataFlags : byte
		{
			None             = 0,
			NumberData       = 1 << 0,
			InputNumberData  = 1 << 1,
			OutputNumberData = 1 << 2,
			FieldNumberData  = 1 << 3,
			StringData       = 1 << 4,
			InputModules     = 1 << 5,
			ArrayData        = 1 << 6,
			CodeMetadata     = 1 << 7,
		}

		// PLC-only state.  Persisted via DataFlags.CodeMetadata; controls the
		// dynamic computing cost (1 + 0.05 × count).  Zero for non-PLC modules
		// and PLC modules whose script hasn't been parsed yet — the next
		// successful Execute() will populate this and flip the flag bit.
		[DoNotSave(0, null)]
		private int m_lexerNodeCount;
		public int LexerNodeCount {
			get => m_lexerNodeCount;
			set => m_lexerNodeCount = value;
		}

		// Compiled cache for the PLC module's parsed Block.  Non-serialized —
		// re-parsed on first Execute() after load, or whenever the source's
		// hash diverges from m_compiledSourceHash.
		[DoNotSave(0, null)]
		public object CompiledBlock { get; set; }
		// Compiled cache for the PLC's init: section.  Same lifetime as
		// CompiledBlock — recreated whenever the source hash diverges, never
		// persisted (the Block AST itself isn't serializable; we re-tokenize
		// + re-parse on first execute after load).
		[DoNotSave(0, null)]
		public object CompiledInitBlock { get; set; }
		// Preamble — top-level code outside init: / main:.  Holds player-
		// authored helper `def` / `class` definitions that should be
		// callable from both sections.  Compiled once per source change;
		// runs once per compile to register its functions into PlcContext
		// so init / main pick them up via the persistent-vars overlay.
		[DoNotSave(0, null)]
		public object CompiledPreambleBlock { get; set; }
		[DoNotSave(0, null)]
		public int CompiledSourceHash { get; set; }

		// Per-tick "live" runtime context — set by PlcPy.RunBlock at the
		// start of each tick (preamble / init / main) to the dict the
		// statements are actually executing against.  Preamble-registered
		// Methods read this when invoked so their ChildContext parent is
		// the *current* tick's scope (with init's freshly-set vars and
		// main's running mutations) instead of the stale snapshot the
		// preamble captured when it first registered them.  Cleared in
		// the finally so a method called outside an active tick falls
		// back to the captured preamble scope.
		[DoNotSave(0, null)]
		public System.Collections.Generic.IDictionary<string, object> CurrentPlcRuntime { get; set; }

		// Player-defined variables that survive between init and run, and
		// across run ticks.  Populated by PlcPy's init dispatch, mutated by
		// each run tick, and serialised on save.  Stays null for non-PLC
		// modules and lazy-allocated by the PLC code path so non-PLC
		// instances pay nothing for the field.
		//
		// Only the subset of types PlcPy can serialise
		// is round-tripped to the save (primitives + Fix32 + null);
		// non-serialisable values (Constructor, Type, wrappers, generic
		// collections) are dropped on save and won't reappear after load.
		// The init: block re-runs after any non-Running result anyway, so
		// dropping non-serialisable scratch values is recoverable.
		[DoNotSave(0, null)]
		public Dict<string, object> PlcContext { get; set; }

		protected void SerializeData(BlobWriter writer)
		{
			if (m_protoId == null) {
				throw new NullReferenceException($"Prototype was not set valid");
			}

			writer.WriteLong(Id);
			writer.WriteString(m_protoId);
			writer.WriteInt(/*Version*/ Controller.MODULE_PLC_CONTEXT);
			writer.WriteBool(IsPaused);
			writer.WriteInt((int)Status);

			DataFlags flags = DataFlags.None;
			if (NumberData != null       && NumberData.Count       > 0) flags |= DataFlags.NumberData;
			if (InputNumberData != null  && InputNumberData.Count  > 0) flags |= DataFlags.InputNumberData;
			if (OutputNumberData != null && OutputNumberData.Count > 0) flags |= DataFlags.OutputNumberData;
			if (FieldNumberData != null  && FieldNumberData.Count  > 0) flags |= DataFlags.FieldNumberData;
			if (StringData != null       && StringData.Count       > 0) flags |= DataFlags.StringData;
			if (InputModules != null     && InputModules.Count     > 0) flags |= DataFlags.InputModules;
			if (ArrayData != null        && ArrayData.Length       > 0) flags |= DataFlags.ArrayData;
			// CodeMetadata: cached lexer-node count for PLC modules (drives the
			// dynamic computing cost without re-tokenizing on load).  Only set
			// if the value is meaningful — a 0 count means "no script yet" and
			// can be reconstructed on first execute, so don't burn a flag bit
			// for it.
			if (m_lexerNodeCount > 0) flags |= DataFlags.CodeMetadata;
			writer.WriteByte((byte)flags);

			if ((flags & DataFlags.NumberData)       != 0) Dict<string, int>.Serialize(NumberData, writer);
			if ((flags & DataFlags.InputNumberData)  != 0) Dict<string, Fix32>.Serialize(InputNumberData, writer);
			if ((flags & DataFlags.OutputNumberData) != 0) Dict<string, Fix32>.Serialize(OutputNumberData, writer);
			if ((flags & DataFlags.FieldNumberData)  != 0) Dict<string, Fix32>.Serialize(FieldNumberData, writer);
			if ((flags & DataFlags.StringData)       != 0) Dict<string, string>.Serialize(StringData, writer);
			if ((flags & DataFlags.InputModules)     != 0) Dict<string, ModuleConnector>.Serialize(InputModules, writer);
			writer.WriteInt(Row);
			writer.WriteInt(Column);
			if ((flags & DataFlags.ArrayData) != 0) {
				writer.WriteArray(ArrayData);
			}
			if ((flags & DataFlags.CodeMetadata) != 0) {
				writer.WriteInt(m_lexerNodeCount);
			}
			// v7+ MODULE_EXTENSIONS: input + output extension counts.  v8+
			// MODULE_DISPLAY_EXTENSIONS adds DisplayExtensionCount as a separate
			// int — kept gated by its own version so v7 in-progress saves
			// (written when display extensions weren't a feature yet) still load.
			writer.WriteInt(InputExtensionCount);
			writer.WriteInt(OutputExtensionCount);
			writer.WriteInt(DisplayExtensionCount);

			// v9+ MODULE_PLC_CONTEXT: PLC-PY persistent scratch dict.  Always
			// written (count int + entries) — empty/missing context produces
			// `0` for the count and zero entry bytes, so non-PLC modules pay
			// 4 bytes per save, which is negligible and avoids burning a flag
			// bit on it.  Whitelisted types only; non-serialisable values
			// (Constructors, Type, wrappers) are dropped on save.  See
			// PlcContextSerializer.IsSerialisable for the supported set.
			PlcContextSerializer.Serialize(PlcContext, writer);
		}

		protected void DeserializeData(BlobReader reader)
		{
			Id = reader.ReadLong();
			m_protoId = reader.ReadString();
			// Snapshot the save's original proto id once — the Prototype setter (called
			// later by initContexts when a Deprecation entry swaps protos) overwrites
			// m_protoId, so without this capture we couldn't tell migrated modules apart
			// from native ones in the controller's post-load remap pass.
			m_originalProtoId = m_protoId;
			loadedVersion = reader.ReadInt();
			IsPaused = reader.ReadBool();
			if (loadedVersion >= 2) {
				Status = (ModuleStatus)reader.ReadInt();
			} else {
				Status = ModuleStatus.Running;
			}

			if (loadedVersion >= Controller.MODULE_COMPACT_DATA)
			{
				// v5+: single byte tells us which containers are present.  Absent
				// flags get a fresh empty container — same shape the constructor sets.
				DataFlags flags = (DataFlags)reader.ReadByte();
				NumberData       = (flags & DataFlags.NumberData)       != 0 ? Dict<string, int>.Deserialize(reader)            : new Dict<string, int>();
				InputNumberData  = (flags & DataFlags.InputNumberData)  != 0 ? Dict<string, Fix32>.Deserialize(reader)          : new Dict<string, Fix32>();
				OutputNumberData = (flags & DataFlags.OutputNumberData) != 0 ? Dict<string, Fix32>.Deserialize(reader)          : new Dict<string, Fix32>();
				FieldNumberData  = (flags & DataFlags.FieldNumberData)  != 0 ? Dict<string, Fix32>.Deserialize(reader)          : new Dict<string, Fix32>();
				StringData       = (flags & DataFlags.StringData)       != 0 ? Dict<string, string>.Deserialize(reader)         : new Dict<string, string>();
				InputModules     = (flags & DataFlags.InputModules)     != 0 ? Dict<string, ModuleConnector>.Deserialize(reader): new Dict<string, ModuleConnector>();
				Row = reader.ReadInt();
				Column = reader.ReadInt();
				ArrayData = (flags & DataFlags.ArrayData) != 0
					? (reader.ReadArray<Fix32>() ?? System.Array.Empty<Fix32>())
					: System.Array.Empty<Fix32>();
				// v6+ (MODULE_PYTHON_CODE): CodeMetadata bit, when set, carries
				// the PLC's cached lexer-node count.  Older v5 saves never set
				// the bit and the read is skipped — m_lexerNodeCount stays 0
				// and the next Execute() will recompute it from the source.
				m_lexerNodeCount = (loadedVersion >= Controller.MODULE_PYTHON_CODE && (flags & DataFlags.CodeMetadata) != 0)
					? reader.ReadInt()
					: 0;
				if (loadedVersion >= Controller.MODULE_EXTENSIONS)
				{
					InputExtensionCount = reader.ReadInt();
					OutputExtensionCount = reader.ReadInt();
				}
				else
				{
					InputExtensionCount = 0;
					OutputExtensionCount = 0;
				}
				// DisplayExtensionCount was added one version later than the pin
				// counts.  v7 saves don't carry it — leave it at 0 and let the
				// player grow the display from the inspector if needed.
				if (loadedVersion >= Controller.MODULE_DISPLAY_EXTENSIONS)
				{
					DisplayExtensionCount = reader.ReadInt();
				}
				else
				{
					DisplayExtensionCount = 0;
				}

				// v9+ MODULE_PLC_CONTEXT: PLC-PY scratch dict (player vars
				// from init/run).  Pre-v9 saves don't carry it — start with
				// a fresh empty dict so the next Action can run init: from
				// a clean slate.  Same shape on read regardless of whether
				// the module is a PLC instance or not (non-PLC modules
				// simply leave the dict alone).
				if (loadedVersion >= Controller.MODULE_PLC_CONTEXT)
				{
					PlcContext = PlcContextSerializer.Deserialize(reader);
				}
				else
				{
					PlcContext = new Dict<string, object>();
				}
			}
			else
			{
				// Legacy path (pre-v5): all dicts written unconditionally, no
				// array yet.  ArrayData defaults to empty for any save older than
				// MODULE_COMPACT_DATA.
				NumberData = Dict<string, int>.Deserialize(reader);
				if (loadedVersion >= 3)
				{
					InputNumberData = Dict<string, Fix32>.Deserialize(reader);
					OutputNumberData = Dict<string, Fix32>.Deserialize(reader);
					FieldNumberData = Dict<string, Fix32>.Deserialize(reader);
				}
				else
				{
					InputNumberData = [];
					OutputNumberData = [];
					FieldNumberData = [];
				}
				StringData = Dict<string, string>.Deserialize(reader);
				InputModules = Dict<string, ModuleConnector>.Deserialize(reader);

				if (loadedVersion >= Controller.MODULE_LAYOUT_INFO)
				{
					Row = reader.ReadInt();
					Column = reader.ReadInt();
				}
				// else: position is back-filled by Controller from its legacy Rows table.

				ArrayData = System.Array.Empty<Fix32>();
				// Pre-v5 (and pre-v9) saves never wrote a PLC context — start
				// the dict empty so the next Action can populate it from init.
				PlcContext = new Dict<string, object>();
			}

			Log.Info($"[Programable Network] Instance (deserialization): {GetHashCode()}({Id}), version: {loadedVersion}");
		}

		[InitAfterLoad(InitPriority.High)]
		[OnlyForSaveCompatibility(null)]
		public void initContexts(int saveVersion)
		{
			Option<ModuleProto> Prototype = Context.ProtosDb.Get<ModuleProto>(new ModuleProto.ID(m_protoId));
			Log.Info($"[Programable Network] Instance (init): {GetHashCode()}({Id}), version: {loadedVersion}");

			// Always consult Deprecation — it can carry pin-rename entries for protos that
			// are still registered (Replacement == null), so the proto-found branch needs
			// the migration too.  Version-gated entries return null below the gate via
			// AppliesAt, in which case we fall through to the no-migration path.
			Deprecation.Migration? migration = Deprecation.GetMigration(new ModuleProto.ID(m_protoId));
			if (migration.HasValue && !migration.Value.AppliesAt(loadedVersion)) {
				migration = null;
			}

			if (Prototype.HasValue)
			{
				this.Prototype = Prototype.Value;
			}
			else if (migration.HasValue && migration.Value.Replacement.HasValue)
			{
				this.Prototype = Context.ProtosDb.Get<ModuleProto>(migration.Value.Replacement.Value)
					.ValueOrThrow("Invalid module proto: " + m_protoId);
			}
			else
			{
				// No proto and no Deprecation replacement — leave a visible tombstone with
				// a clear error string so the hover tooltip explains *which* prototype is
				// missing.  Phantom's per-tick Action also reports ModuleStatus.Error.
				this.Prototype = ModuleProto.Phantom;
				SetError($"Original module '{m_protoId}' no longer exists. Remove it or install the mod that provides it.");
				SetStatus(ModuleStatus.Error);
			}

			if (migration.HasValue)
			{
				// Apply ext-count migration when the deprecation entry asked for one —
				// e.g. Sum_4 → Sum sets InputExtensionCount=2 so the migrated module
				// has the same four input pins as the legacy save.  The clamping below
				// caps the value at the new proto's max.
				if (migration.Value.InputExtensionCount.HasValue) {
					InputExtensionCount = migration.Value.InputExtensionCount.Value;
				}
				if (migration.Value.OutputExtensionCount.HasValue) {
					OutputExtensionCount = migration.Value.OutputExtensionCount.Value;
				}
				if (migration.Value.DisplayExtensionCount.HasValue) {
					DisplayExtensionCount = migration.Value.DisplayExtensionCount.Value;
				}

				// Rename keys of InputModules using the migration's input id map.  This
				// preserves cables coming INTO this module when the new prototype uses
				// different pin ids than the saved one.  Outgoing cables (other modules
				// referencing this one's outputs) are remapped in the controller's
				// post-load pass via OutputIdMap.  We also rename InputNumberData (the
				// persisted per-input scratch values) so they stay attached to the
				// renamed pin instead of going dead under the old key.
				if (migration.Value.InputIdMap != null)
				{
					renameKeysInPlace(InputModules, migration.Value.InputIdMap, "input pin (cable)");
					renameKeysInPlace(InputNumberData, migration.Value.InputIdMap, "input pin (number)");
				}
				if (migration.Value.OutputIdMap != null)
				{
					// Outgoing cables are stored on consumers, not here — remapped in
					// Controller.initContexts.  This module's own OutputNumberData scratch
					// dict is keyed by output pin id, so rename it here.
					renameKeysInPlace(OutputNumberData, migration.Value.OutputIdMap, "output pin (number)");
				}
			}

			if (loadedVersion == 0)
			{
				foreach (IField item in this.Prototype.Fields)
				{
					if (item is not Ui.EntityField && NumberData.TryGetValue("field__" + item.Id, out var value))
					{
						NumberData["field__" + item.Id] = value.ToFix32().RawValue;
					}
				}
				foreach (ModuleConnectorProto item in this.Prototype.Inputs)
				{
					if (NumberData.TryGetValue("in__" + item.Id, out var value)) {
						NumberData["in__" + item.Id] = value.ToFix32().RawValue;
					}
				}
				foreach (ModuleConnectorProto item in this.Prototype.Outputs)
				{
					if (NumberData.TryGetValue("out__" + item.Id, out var value)) {
						NumberData["out__" + item.Id] = value.ToFix32().RawValue;
					}
				}
			}

			// Clamp extension counts to whatever the prototype now exposes — handles a
			// save written when the proto allowed N extensions but reloaded after the
			// proto's max was reduced.  Phantom (proto missing) gets 0.
			if (this.Prototype != null)
			{
				if (InputExtensionCount > this.Prototype.MaxInputExtensions) {
					InputExtensionCount = this.Prototype.MaxInputExtensions;
				}
				if (OutputExtensionCount > this.Prototype.MaxOutputExtensions) {
					OutputExtensionCount = this.Prototype.MaxOutputExtensions;
				}
				if (DisplayExtensionCount > this.Prototype.MaxDisplayExtensions) {
					DisplayExtensionCount = this.Prototype.MaxDisplayExtensions;
				}
			}

			if (loadedVersion < 3)
			{
				var keysToRemove = new System.Collections.Generic.List<string>();
				foreach (var kvp in NumberData)
				{
					if (kvp.Key.StartsWith("in__"))
					{
						InputNumberData[kvp.Key.Substring("in__".Length)] = Fix32.FromRaw(kvp.Value);
						keysToRemove.Add(kvp.Key);
					}
					else if (kvp.Key.StartsWith("out__"))
					{
						OutputNumberData[kvp.Key.Substring("out__".Length)] = Fix32.FromRaw(kvp.Value);
						keysToRemove.Add(kvp.Key);
					}
					else if (kvp.Key.StartsWith("field__"))
					{
						FieldNumberData[kvp.Key.Substring("field__".Length)] = Fix32.FromRaw(kvp.Value);
						keysToRemove.Add(kvp.Key);
					}
				}
				foreach (var k in keysToRemove)
				{
					NumberData.TryRemove(k, out _);
				}
			}
		}

		// Rename string keys of <paramref name="dict"/> in-place using <paramref name="map"/>.
		// Used during Deprecation pin-rename application — preserves the value but moves it
		// from the old pin id to the new one.  Last-write-wins if both ids ended up populated:
		// the explicit-rename intent overrides any stale pre-existing entry under newKey.
		private void renameKeysInPlace<TValue>(Dict<string, TValue> dict,
			IReadOnlyDictionary<string, string> map, string what)
		{
			if (dict == null || dict.Count == 0 || map == null) {
				return;
			}
			foreach (var kv in dict.ToList())
			{
				if (map.TryGetValue(kv.Key, out string newKey)
					&& !string.Equals(kv.Key, newKey, StringComparison.Ordinal))
				{
					dict.Remove(kv.Key);
					dict[newKey] = kv.Value;
					Log.Info($"[Programable Network] Module {Id}: {what} renamed '{kv.Key}' -> '{newKey}' via Deprecation");
				}
			}
		}

		public void AddToConfig(EntityConfigData data)
		{
			// clone proto id, position and cofigured fields
			// the position on grid is hold by computer
		}

		public void ApplyConfig(EntityConfigData data)
		{
			// applies prototype and accessible fields
		}

		public void UpdateIsEnabled()
		{
			//throw new NotImplementedException();
		}

		public void UpdateIsBroken()
		{
			//throw new NotImplementedException();
		}

		public void UpdateProperties()
		{
			//throw new NotImplementedException();
		}

		public void SetPaused(bool isPaused)
		{
			//throw new NotImplementedException();
		}

		public void AddObserver(IEntityObserver observer)
		{
			//throw new NotImplementedException();
		}

		public void RemoveObserver(IEntityObserver observer)
		{
			//throw new NotImplementedException();
		}

		public void SetStatus(ModuleStatus status)
		{
			Status = status;
		}

		public void SetError(string message)
		{
			Error = message;
		}

		public void CopyFrom(Module other)
		{
			// Copying over the entire state, including volatile runtime data.  Used by
			// the controller's CloneModule command to duplicate an existing module along
			// with its current values and connections.
			InputExtensionCount = other.InputExtensionCount;
			OutputExtensionCount = other.OutputExtensionCount;
			DisplayExtensionCount = other.DisplayExtensionCount;
			Error = other.Error;
			Status = other.Status;
			IsPaused = other.IsPaused;
			IsDebugging = other.IsDebugging;
			NumberData = new Dict<string, int>(other.NumberData);
			InputNumberData = new Dict<string, Fix32>(other.InputNumberData);
			OutputNumberData = new Dict<string, Fix32>(other.OutputNumberData);
			FieldNumberData = new Dict<string, Fix32>(other.FieldNumberData);
			StringData = new Dict<string, string>(other.StringData);
			ArrayData = (Fix32[])other.ArrayData.Clone();
			InputModules = new Dict<string, ModuleConnector>(other.InputModules);
		}

		[DoNotSave(0, null)]
		public Dict<string, int> NumberData { get; private set; }
		[DoNotSave(0, null)]
		public Dict<string, Fix32> InputNumberData { get; private set; }
		[DoNotSave(0, null)]
		public Dict<string, Fix32> OutputNumberData { get; private set; }
		[DoNotSave(0, null)]
		public Dict<string, Fix32> FieldNumberData { get; private set; }
		[DoNotSave(0, null)]
		public Dict<string, string> StringData { get; private set; }
		// Single Fix32[] scratch buffer per module — runtime arrays (ring buffers,
		// FIR windows, history slices).  Public getter for direct read in inner
		// loops; the setter is private so size changes go through ArrayAccess.Resize
		// which keeps the existing contents and zero-fills any new slots.
		[DoNotSave(0, null)]
		public Fix32[] ArrayData { get; private set; }

		[DoNotSave(0, null)]
		public OutputData Output => new OutputData(this);

		[DoNotSave(0, null)]
		public InputData Input => new InputData(this);

		[DoNotSave(0, null)]
		public FieldOrInputData FieldOrInput => new FieldOrInputData(this);

		[DoNotSave(0, null)]
		public FieldData Field => new FieldData(this);

		[DoNotSave(0, null)]
		public DisplayData Display => new DisplayData(this);

		[DoNotSave(0, null)]
		public ArrayAccess Array => new ArrayAccess(this);

		public LocStrFormatted DefaultTitle => throw new NotImplementedException();

		EntityId IEntity.Id => new EntityId((int)(Id&0xFFFFFFFF));

		EntityProto IEntity.Prototype => Prototype;

		public bool IsEnabled => true;

		public bool IsPaused { get; set; }

		public bool CanBePaused => false;

		public bool IsDestroyed => false;

		public bool IsDebugging { get; set; } = false;

		public bool Info {
			get => NumberData.TryGetValue("__info", out int value) && value > 0;
			set => NumberData["__info"] = value ? 1 : 0;
		}

		public bool Warning {
			get => NumberData.TryGetValue("__warning", out int value) && value > 0;
			set => NumberData["__warning"] = value ? 1 : 0;
		}

		public bool Unlocked {
			get {
				if (m_unlocked) {
					return true;
				}

				if (Prototype.ResearchDependency.IsNotValidOrEmpty) {
					m_unlocked = true;
					return true;
				}

				if (m_researchNodes.IsNotValidOrEmpty) {
					m_researchNodes = Prototype.ResearchDependency
						.Map(Controller.Resolver.Resolve<ResearchManager>().GetResearchNode);
				}

				foreach (ResearchNode researchNode in m_researchNodes) {
					if (researchNode.State != ResearchNodeState.Researched) {
						return false;
					}
				}

				m_unlocked = true;
				return true;
			}
		}
	}
}