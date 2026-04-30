using System;
using Mafi;
using Mafi.Core;
using Mafi.Core.Input;
using Mafi.Serialization;

namespace ProgramableNetwork
{
	/// <summary>
	/// Sets the speaker's playback volume.  Carries a <see cref="Percent"/>; the
	/// inspector slider should debounce so a user dragging the thumb doesn't queue
	/// dozens of commands per frame.
	/// </summary>
	[ManuallyWrittenSerialization]
	public class SpeakerSetVolumeCmd : InputCommand
	{
		public readonly EntityId EntityId;
		public readonly Percent Volume;

		public SpeakerSetVolumeCmd(EntityId entityId, Percent volume)
		{
			EntityId = entityId;
			Volume = volume;
		}

		public static void Serialize(SpeakerSetVolumeCmd value, BlobWriter writer)
		{
			if (writer.TryStartClassSerialization(value)) {
				writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
			}
		}

		protected override void SerializeData(BlobWriter writer)
		{
			base.SerializeData(writer);
			EntityId.Serialize(EntityId, writer);
			Percent.Serialize(Volume, writer);
		}

		public new static SpeakerSetVolumeCmd Deserialize(BlobReader reader)
		{
			if (reader.TryStartClassDeserialization(out SpeakerSetVolumeCmd obj,
				(Func<BlobReader, Type, SpeakerSetVolumeCmd>)null,
				(Func<BlobReader, string, SpeakerSetVolumeCmd>)null,
				false)) {
				reader.EnqueueDataDeserialization(obj, s_deserializeDataDelayedAction);
			}
			return obj;
		}

		protected override void DeserializeData(BlobReader reader)
		{
			base.DeserializeData(reader);
			reader.SetField(this, nameof(EntityId), EntityId.Deserialize(reader));
			reader.SetField(this, nameof(Volume), Percent.Deserialize(reader));
		}

		private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction =
			(obj, writer) => ((SpeakerSetVolumeCmd)obj).SerializeData(writer);

		private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction =
			(obj, reader) => ((SpeakerSetVolumeCmd)obj).DeserializeData(reader);
	}
}
