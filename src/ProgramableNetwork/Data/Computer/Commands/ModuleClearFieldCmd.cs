using System;
using Mafi.Core;
using Mafi.Core.Input;
using Mafi.Serialization;

namespace ProgramableNetwork
{
	/// <summary>
	/// Clears a module field — removes both the Fix32 (FieldNumberData) and the string-side
	/// (StringData "field__&lt;id&gt;") entries atomically. Used by reset / trash buttons in the UI;
	/// avoids the two-command chain Set(Fix32.Zero) + Set(string "") and is unambiguous about
	/// "remove" vs "store zero".
	/// </summary>
	[ManuallyWrittenSerialization]
	public class ModuleClearFieldCmd : InputCommand
	{
		public readonly EntityId ControllerId;
		public readonly long ModuleId;
		public readonly string FieldId;

		public ModuleClearFieldCmd(EntityId controllerId, long moduleId, string fieldId)
		{
			ControllerId = controllerId;
			ModuleId = moduleId;
			FieldId = fieldId;
		}

		public static void Serialize(ModuleClearFieldCmd value, BlobWriter writer)
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
		}

		public new static ModuleClearFieldCmd Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out ModuleClearFieldCmd obj,
				(Func<BlobReader, Type, ModuleClearFieldCmd>)null,
				(Func<BlobReader, string, ModuleClearFieldCmd>)null,
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
		}

		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
			(obj, writer) => ((ModuleClearFieldCmd)obj).SerializeData(writer);

		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
			(obj, reader) => ((ModuleClearFieldCmd)obj).DeserializeData(reader);
	}
}
