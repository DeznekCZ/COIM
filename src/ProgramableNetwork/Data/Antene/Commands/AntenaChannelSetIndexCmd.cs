using System;
using Mafi.Core;
using Mafi.Core.Input;
using Mafi.Serialization;

namespace ProgramableNetwork
{
	/// <summary>
	/// Sets the tuning <c>Index</c> of a redirected channel on an antena's data band.
	/// Replaces direct <c>channel.Index = N</c> / <c>channel.Move(...)</c> calls in
	/// FM/AM channel-view nav buttons.  The cmd carries the absolute target index;
	/// relative moves are resolved by the UI before dispatch (Move(N) → currentIndex + N
	/// with wrap-around) so the executor doesn't need to read back state.
	/// </summary>
	[ManuallyWrittenSerialization]
	public class AntenaChannelSetIndexCmd : InputCommand
	{
		public readonly EntityId AntenaId;
		public readonly int RedirectedSlot;
		public readonly int NewIndex;

		public AntenaChannelSetIndexCmd(EntityId antenaId, int redirectedSlot, int newIndex)
		{
			AntenaId = antenaId;
			RedirectedSlot = redirectedSlot;
			NewIndex = newIndex;
		}

		public static void Serialize(AntenaChannelSetIndexCmd value, BlobWriter writer)
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
			writer.WriteInt(NewIndex);
		}

		public new static AntenaChannelSetIndexCmd Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out AntenaChannelSetIndexCmd obj,
				(Func<BlobReader, Type, AntenaChannelSetIndexCmd>)null,
				(Func<BlobReader, string, AntenaChannelSetIndexCmd>)null,
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
			reader.SetField(this, nameof(NewIndex), reader.ReadInt());
		}

		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
			(obj, writer) => ((AntenaChannelSetIndexCmd)obj).SerializeData(writer);

		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
			(obj, reader) => ((AntenaChannelSetIndexCmd)obj).DeserializeData(reader);
	}
}
