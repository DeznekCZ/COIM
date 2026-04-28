using System;
using Mafi;
using Mafi.Core;
using Mafi.Core.Input;
using Mafi.Serialization;

namespace ProgramableNetwork
{
	/// <summary>
	/// Sets a Fix32-typed field value on a module owned by a specific controller.
	/// Covers Fix32, int (via Fix32.FromInt), bool (0/1), and any value packed into Fix32 by raw bits
	/// (controller color, ProductProto.SlimId, prototype-string hash).
	/// </summary>
	[ManuallyWrittenSerialization]
	public class ModuleSetFix32FieldCmd : InputCommand
	{
		public readonly EntityId ControllerId;
		public readonly long ModuleId;
		public readonly string FieldId;
		public readonly Fix32 Value;

		public ModuleSetFix32FieldCmd(EntityId controllerId, long moduleId, string fieldId, Fix32 value)
		{
			ControllerId = controllerId;
			ModuleId = moduleId;
			FieldId = fieldId;
			Value = value;
		}

		public static void Serialize(ModuleSetFix32FieldCmd value, BlobWriter writer)
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
			Fix32.Serialize(Value, writer);
		}

		public new static ModuleSetFix32FieldCmd Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out ModuleSetFix32FieldCmd obj,
				(Func<BlobReader, Type, ModuleSetFix32FieldCmd>)null,
				(Func<BlobReader, string, ModuleSetFix32FieldCmd>)null,
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
			reader.SetField(this, nameof(Value), Fix32.Deserialize(reader));
		}

		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
			(obj, writer) => ((ModuleSetFix32FieldCmd)obj).SerializeData(writer);

		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
			(obj, reader) => ((ModuleSetFix32FieldCmd)obj).DeserializeData(reader);
	}
}
