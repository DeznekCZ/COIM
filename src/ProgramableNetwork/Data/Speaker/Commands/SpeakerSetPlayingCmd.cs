using System;
using Mafi.Core;
using Mafi.Core.Input;
using Mafi.Serialization;

namespace ProgramableNetwork
{
	/// <summary>
	/// Toggles whether a <see cref="ProgramableNetwork.Data.Speaker.Speaker"/> is currently
	/// playing its bound sound.  Replaces the direct <c>Entity.SetPlaying</c> call from the
	/// inspector toggle so multiplayer stays in lock-step.
	/// </summary>
	[ManuallyWrittenSerialization]
	public class SpeakerSetPlayingCmd : InputCommand
	{
		public readonly EntityId EntityId;
		public readonly bool IsPlaying;

		public SpeakerSetPlayingCmd(EntityId entityId, bool isPlaying)
		{
			EntityId = entityId;
			IsPlaying = isPlaying;
		}

		public static void Serialize(SpeakerSetPlayingCmd value, BlobWriter writer)
		{
			if (writer.TryStartClassSerialization(value)) {
				writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
			}
		}

		protected override void SerializeData(BlobWriter writer)
		{
			base.SerializeData(writer);
			EntityId.Serialize(EntityId, writer);
			writer.WriteBool(IsPlaying);
		}

		public new static SpeakerSetPlayingCmd Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out SpeakerSetPlayingCmd obj,
				(Func<BlobReader, Type, SpeakerSetPlayingCmd>)null,
				(Func<BlobReader, string, SpeakerSetPlayingCmd>)null,
				false)) {
				reader.EnqueueDataDeserialization(obj, s_deserializeDataDelayedAction);
			}
			return obj;
		}

		protected override void DeserializeData(BlobReader reader)
		{
			base.DeserializeData(reader);
			reader.SetField(this, nameof(EntityId), EntityId.Deserialize(reader));
			reader.SetField(this, nameof(IsPlaying), reader.ReadBool());
		}

		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
			(obj, writer) => ((SpeakerSetPlayingCmd)obj).SerializeData(writer);

		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
			(obj, reader) => ((SpeakerSetPlayingCmd)obj).DeserializeData(reader);
	}
}
