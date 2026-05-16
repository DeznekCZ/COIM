using System;
using Mafi.Core.Input;
using Mafi.Serialization;

namespace ProgramableNetwork.Data.Variables
{
	/// <summary>
	/// Removes a network variable from <see cref="VariableManager"/> — both the value
	/// and the recorded writer EntityId.  Issued from the Variables window's per-row
	/// trash button when the source controller has been deleted, so a stale variable
	/// can be cleaned up without producing a new write.  Multiplayer-safe: the
	/// removal applies on every peer through the command pipeline rather than via a
	/// direct local mutation.
	/// </summary>
	[ManuallyWrittenSerialization]
	public class VariableRemoveCmd : InputCommand
	{
		public readonly string Name;

		public VariableRemoveCmd(string name)
		{
			Name = name;
		}

		public static void Serialize(VariableRemoveCmd value, BlobWriter writer)
		{
			if (writer.TryStartClassSerialization(value)) {
				writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
			}
		}

		protected override void SerializeData(BlobWriter writer)
		{
			base.SerializeData(writer);
			writer.WriteString(Name);
		}

		public new static VariableRemoveCmd Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out VariableRemoveCmd obj,
				(Func<BlobReader, Type, VariableRemoveCmd>)null,
				(Func<BlobReader, string, VariableRemoveCmd>)null,
				false)) {
				reader.EnqueueDataDeserialization(obj, s_deserializeDataDelayedAction);
			}
			return obj;
		}

		protected override void DeserializeData(BlobReader reader)
		{
			base.DeserializeData(reader);
			reader.SetField(this, nameof(Name), reader.ReadString());
		}

		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
			(obj, writer) => ((VariableRemoveCmd)obj).SerializeData(writer);

		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
			(obj, reader) => ((VariableRemoveCmd)obj).DeserializeData(reader);
	}
}
