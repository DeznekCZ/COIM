using System;
using Mafi.Core;
using Mafi.Core.Input;
using Mafi.Serialization;

namespace ProgramableNetwork
{
	/// <summary>
	/// Binds the source entity of an AM redirected channel.  The source can be either a
	/// <c>WorldMapMine</c> or the player's <c>BattleShip</c>; the executor disambiguates
	/// at apply time via the entity's runtime type, so the wire format stores only the
	/// entity ID — no proto identifier is needed.
	/// Null <see cref="SourceEntityId"/> clears the binding.
	/// </summary>
	[ManuallyWrittenSerialization]
	public class AntenaChannelSetAmSourceCmd : InputCommand
	{
		public readonly EntityId AntenaId;
		public readonly int RedirectedSlot;
		public readonly EntityId? SourceEntityId;

		public AntenaChannelSetAmSourceCmd(EntityId antenaId, int redirectedSlot, EntityId? sourceEntityId)
		{
			AntenaId = antenaId;
			RedirectedSlot = redirectedSlot;
			SourceEntityId = sourceEntityId;
		}

		public static void Serialize(AntenaChannelSetAmSourceCmd value, BlobWriter writer)
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
			writer.WriteNullableStruct(SourceEntityId);
		}

		public new static AntenaChannelSetAmSourceCmd Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out AntenaChannelSetAmSourceCmd obj,
				(Func<BlobReader, Type, AntenaChannelSetAmSourceCmd>)null,
				(Func<BlobReader, string, AntenaChannelSetAmSourceCmd>)null,
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
			reader.SetField(this, nameof(SourceEntityId), reader.ReadNullableStruct<EntityId>());
		}

		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
			(obj, writer) => ((AntenaChannelSetAmSourceCmd)obj).SerializeData(writer);

		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
			(obj, reader) => ((AntenaChannelSetAmSourceCmd)obj).DeserializeData(reader);
	}
}
