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
	[GenerateSerializer(false, null, 0)]
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
		protected void SerializeData(BlobWriter writer)
		{
			if (m_protoId == null) {
				throw new NullReferenceException($"Prototype was not set valid");
			}

			writer.WriteLong(Id);
			writer.WriteString(m_protoId);
			writer.WriteInt(/*Version*/ Controller.MODULE_LAYOUT_INFO);
			writer.WriteBool(IsPaused);
			writer.WriteInt((int)Status);
			Dict<string, int>.Serialize(NumberData, writer);
			Dict<string, Fix32>.Serialize(InputNumberData, writer);
			Dict<string, Fix32>.Serialize(OutputNumberData, writer);
			Dict<string, Fix32>.Serialize(FieldNumberData, writer);
			Dict<string, string>.Serialize(StringData, writer);
			Dict<string, ModuleConnector>.Serialize(InputModules, writer);
			writer.WriteInt(Row);
			writer.WriteInt(Column);
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
					this.Prototype = ModuleProto.Phantom;
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