using System;
using Mafi.Core;
using Mafi.Core.Input;
using Mafi.Serialization;

namespace ProgramableNetwork
{
	/// <summary>
	/// Binds the source antena of an FM redirected channel.  Replaces
	/// <c>channel.Antena = entity</c> in <see cref="ProgramableNetwork.Ui.AntenaPicker"/>.
	/// Null <see cref="SourceAntenaId"/> clears the binding.
	/// </summary>
	[ManuallyWrittenSerialization]
	public class AntenaChannelSetFmSourceCmd : InputCommand
	{
		public readonly EntityId AntenaId;
		public readonly int RedirectedSlot;
		public readonly EntityId? SourceAntenaId;

		public AntenaChannelSetFmSourceCmd(EntityId antenaId, int redirectedSlot, EntityId? sourceAntenaId)
		{
			AntenaId = antenaId;
			RedirectedSlot = redirectedSlot;
			SourceAntenaId = sourceAntenaId;
		}

		public static void Serialize(AntenaChannelSetFmSourceCmd value, BlobWriter writer)
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
			writer.WriteNullableStruct(SourceAntenaId);
		}

		public new static AntenaChannelSetFmSourceCmd Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out AntenaChannelSetFmSourceCmd obj,
				(Func<BlobReader, Type, AntenaChannelSetFmSourceCmd>)null,
				(Func<BlobReader, string, AntenaChannelSetFmSourceCmd>)null,
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
			reader.SetField(this, nameof(SourceAntenaId), reader.ReadNullableStruct<EntityId>());
		}

		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
			(obj, writer) => ((AntenaChannelSetFmSourceCmd)obj).SerializeData(writer);

		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
			(obj, reader) => ((AntenaChannelSetFmSourceCmd)obj).DeserializeData(reader);
	}
}
