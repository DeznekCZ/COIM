using System;
using Mafi.Core;
using Mafi.Core.Input;
using Mafi.Serialization;

namespace ProgramableNetwork
{
	/// <summary>
	/// Toggles whether a <see cref="ProgramableNetwork.Data.DisplayEntity.DisplayEntity"/>'s
	/// light/segment manager is currently active.  Routes the change through the input
	/// scheduler so multiplayer and replays stay deterministic.
	/// </summary>
	[ManuallyWrittenSerialization]
	public class DisplayEntitySetActiveCmd : InputCommand
	{
		public readonly EntityId EntityId;
		public readonly bool IsActive;

		public DisplayEntitySetActiveCmd(EntityId entityId, bool isActive)
		{
			EntityId = entityId;
			IsActive = isActive;
		}

		public static void Serialize(DisplayEntitySetActiveCmd value, BlobWriter writer)
		{
			if (writer.TryStartClassSerialization(value)) {
				writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
			}
		}

		protected override void SerializeData(BlobWriter writer)
		{
			base.SerializeData(writer);
			EntityId.Serialize(EntityId, writer);
			writer.WriteBool(IsActive);
		}

		public new static DisplayEntitySetActiveCmd Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out DisplayEntitySetActiveCmd obj,
				(Func<BlobReader, Type, DisplayEntitySetActiveCmd>)null,
				(Func<BlobReader, string, DisplayEntitySetActiveCmd>)null,
				false)) {
				reader.EnqueueDataDeserialization(obj, s_deserializeDataDelayedAction);
			}
			return obj;
		}

		protected override void DeserializeData(BlobReader reader)
		{
			base.DeserializeData(reader);
			reader.SetField(this, nameof(EntityId), EntityId.Deserialize(reader));
			reader.SetField(this, nameof(IsActive), reader.ReadBool());
		}

		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
			(obj, writer) => ((DisplayEntitySetActiveCmd)obj).SerializeData(writer);

		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
			(obj, reader) => ((DisplayEntitySetActiveCmd)obj).DeserializeData(reader);
	}
}
