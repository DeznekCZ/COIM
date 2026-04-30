using System;
using Mafi.Core;
using Mafi.Core.Input;
using Mafi.Serialization;

namespace ProgramableNetwork
{
	/// <summary>
	/// Removes the redirected channel at <see cref="RedirectedSlot"/> from the antena's
	/// data band.  Replaces the direct <c>RemoveChannel</c> call on the trash button.
	/// Slot index is the position in the data band's <c>m_redirected</c> list at the
	/// time the command applies (commands apply in dispatch order each tick, so the slot
	/// is stable for the duration of one click).
	/// </summary>
	[ManuallyWrittenSerialization]
	public class AntenaRemoveRedirectedChannelCmd : InputCommand
	{
		public readonly EntityId AntenaId;
		public readonly int RedirectedSlot;

		public AntenaRemoveRedirectedChannelCmd(EntityId antenaId, int redirectedSlot)
		{
			AntenaId = antenaId;
			RedirectedSlot = redirectedSlot;
		}

		public static void Serialize(AntenaRemoveRedirectedChannelCmd value, BlobWriter writer)
		{
			if (writer.TryStartClassSerialization(value)) {
				writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
			}
		}

		protected override void SerializeData(BlobWriter writer)
		{
			base.SerializeData(writer);
			EntityId.Serialize(AntenaId, writer);
			writer.WriteInt(RedirectedSlot);
		}

		public new static AntenaRemoveRedirectedChannelCmd Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out AntenaRemoveRedirectedChannelCmd obj,
				(Func<BlobReader, Type, AntenaRemoveRedirectedChannelCmd>)null,
				(Func<BlobReader, string, AntenaRemoveRedirectedChannelCmd>)null,
				false)) {
				reader.EnqueueDataDeserialization(obj, s_deserializeDataDelayedAction);
			}
			return obj;
		}

		protected override void DeserializeData(BlobReader reader)
		{
			base.DeserializeData(reader);
			reader.SetField(this, nameof(AntenaId), EntityId.Deserialize(reader));
			reader.SetField(this, nameof(RedirectedSlot), reader.ReadInt());
		}

		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
			(obj, writer) => ((AntenaRemoveRedirectedChannelCmd)obj).SerializeData(writer);

		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
			(obj, reader) => ((AntenaRemoveRedirectedChannelCmd)obj).DeserializeData(reader);
	}
}
