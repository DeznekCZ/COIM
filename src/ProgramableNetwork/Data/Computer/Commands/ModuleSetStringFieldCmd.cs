using System;
using Mafi.Core;
using Mafi.Core.Input;
using Mafi.Serialization;

namespace ProgramableNetwork
{
	/// <summary>
	/// Sets a string-typed field value on a module owned by a specific controller. Maps to the
	/// "field__&lt;id&gt;" StringData slot used for free-form text fields, long values stored as
	/// strings, prototype Id strings, and the JSON envelope for entity references.
	/// </summary>
	[ManuallyWrittenSerialization]
	public class ModuleSetStringFieldCmd : InputCommand
	{
		public readonly EntityId ControllerId;
		public readonly long ModuleId;
		public readonly string FieldId;
		public readonly string Value;

		public ModuleSetStringFieldCmd(EntityId controllerId, long moduleId, string fieldId, string value)
		{
			ControllerId = controllerId;
			ModuleId = moduleId;
			FieldId = fieldId;
			Value = value ?? "";
		}

		public static void Serialize(ModuleSetStringFieldCmd value, BlobWriter writer)
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
			writer.WriteString(Value);
		}

		public new static ModuleSetStringFieldCmd Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out ModuleSetStringFieldCmd obj,
				(Func<BlobReader, Type, ModuleSetStringFieldCmd>)null,
				(Func<BlobReader, string, ModuleSetStringFieldCmd>)null,
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
			reader.SetField(this, nameof(Value), reader.ReadString());
		}

		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
			(obj, writer) => ((ModuleSetStringFieldCmd)obj).SerializeData(writer);

		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
			(obj, reader) => ((ModuleSetStringFieldCmd)obj).DeserializeData(reader);
	}
}
