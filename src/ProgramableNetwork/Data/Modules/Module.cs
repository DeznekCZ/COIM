using Mafi;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Localization;
using Mafi.Serialization;
using System;
using System.Linq;
using System.Threading;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Research;
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
		private int loadedVersion;

		[DoNotSave]
		private bool m_unlocked = false;
		[DoNotSave]
		private ImmutableArray<ResearchNode> m_researchNodes;

		public Module(ModuleProto prototype, EntityContext context, Controller entity)
		{
			this.Id = DateTime.UtcNow.Ticks;
			Thread.Sleep(1);
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
		public long Id { get; private set; }

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
			get => Prototype.Outputs.Any(c => c.Id == id) ? new(Id, id) : throw new ArgumentException($"Missing output '{id}' in module '{Prototype.Id}'");
			set => InputModules[id] = Prototype.Inputs.Any(c => c.Id == id) ? value : throw new ArgumentException($"Missing input '{id}' in module '{Prototype.Id}'");
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
		[DoNotSave(0, null)]
		public int CompiledSourceHash { get; set; }

		protected void SerializeData(BlobWriter writer)
		{
			if (m_protoId == null) {
				throw new NullReferenceException($"Prototype was not set valid");
			}

			writer.WriteLong(Id);
			writer.WriteString(m_protoId);
			writer.WriteInt(/*Version*/ Controller.MODULE_PYTHON_CODE);
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
		}

		protected void DeserializeData(BlobReader reader)
		{
			Id = reader.ReadLong();
			m_protoId = reader.ReadString();
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
			}

			Log.Info($"[Programable Network] Instance (deserialization): {GetHashCode()}({Id}), version: {loadedVersion}");
		}

		[InitAfterLoad(InitPriority.High)]
		[OnlyForSaveCompatibility(null)]
		public void initContexts(int saveVersion)
		{
			Option<ModuleProto> Prototype = Context.ProtosDb.Get<ModuleProto>(new ModuleProto.ID(m_protoId));
			Log.Info($"[Programable Network] Instance (init): {GetHashCode()}({Id}), version: {loadedVersion}");

			if (Prototype.HasValue)
			{
				this.Prototype = Prototype.Value;
			}
			else
			{
				ModuleProto.ID? alternative = Deprecation.GetAlternative(new ModuleProto.ID(m_protoId));
				if (alternative != null) {
					this.Prototype = Context.ProtosDb.Get<ModuleProto>(alternative ?? new ModuleProto.ID()).ValueOrThrow("Invalid module proto: " + m_protoId);
				} else {
					// No proto and no Deprecation replacement — leave a visible tombstone with
					// a clear error string so the hover tooltip explains *which* prototype is
					// missing.  Phantom's per-tick Action also reports ModuleStatus.Error.
					this.Prototype = ModuleProto.Phantom;
					SetError($"Original module '{m_protoId}' no longer exists. Remove it or install the mod that provides it.");
					SetStatus(ModuleStatus.Error);
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
						.Map(Controller.ResearchManager.GetResearchNode);
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