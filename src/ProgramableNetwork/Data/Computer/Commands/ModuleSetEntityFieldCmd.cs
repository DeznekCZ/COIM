using System;
using Mafi.Core;
using Mafi.Core.Input;
using Mafi.Serialization;

namespace ProgramableNetwork
{
	/// <summary>
	/// Sets an entity reference field on a module. The processor performs the dual write
	/// (FieldNumberData + StringData JSON envelope) atomically by calling Module.FieldData.Entity().
	/// A null TargetEntityId clears the field.
	/// </summary>
	[ManuallyWrittenSerialization]
	public class ModuleSetEntityFieldCmd : InputCommand
	{
		public readonly EntityId ControllerId;
		public readonly long ModuleId;
		public readonly string FieldId;
		public readonly EntityId? TargetEntityId;

		public ModuleSetEntityFieldCmd(EntityId controllerId, long moduleId, string fieldId, EntityId? targetEntityId)
		{
			ControllerId = controllerId;
			ModuleId = moduleId;
			FieldId = fieldId;
			TargetEntityId = targetEntityId;
		}

		public static void Serialize(ModuleSetEntityFieldCmd value, BlobWriter writer)
		{
			if (writer.TryStartClassSerialization(value)) {
				writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
			}
		}

		protected override void SerializeData(BlobWriter writer)
		{
			base.SerializeData(writer);
			EntityId.Serialize(ControllerId, writer);
			writer.WriteLong(ModuleId);
			writer.WriteString(FieldId);
			writer.WriteNullableStruct(TargetEntityId);
		}

		public new static ModuleSetEntityFieldCmd Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out ModuleSetEntityFieldCmd obj,
				(Func<BlobReader, Type, ModuleSetEntityFieldCmd>)null,
				(Func<BlobReader, string, ModuleSetEntityFieldCmd>)null,
				false)) {
				reader.EnqueueDataDeserialization(obj, s_deserializeDataDelayedAction);
			}
			return obj;
		}

		protected override void DeserializeData(BlobReader reader)
		{
			base.DeserializeData(reader);
			reader.SetField(this, nameof(ControllerId), EntityId.Deserialize(reader));
			reader.SetField(this, nameof(ModuleId), reader.ReadLong());
			reader.SetField(this, nameof(FieldId), reader.ReadString());
			reader.SetField(this, nameof(TargetEntityId), reader.ReadNullableStruct<EntityId>());
		}

		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
			(obj, writer) => ((ModuleSetEntityFieldCmd)obj).SerializeData(writer);

		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
			(obj, reader) => ((ModuleSetEntityFieldCmd)obj).DeserializeData(reader);
	}
}
